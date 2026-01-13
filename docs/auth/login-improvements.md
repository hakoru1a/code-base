# Cải thiện Login Flow - Xử lý Re-login

## Vấn đề trước khi cải thiện

Khi user đã đăng nhập nhưng click nút "Login" lại:

1. **Session cũ không được xóa ngay** → lãng phí bộ nhớ Redis
2. **Tokens cũ vẫn còn hiệu lực** → rủi ro bảo mật
3. **Không có thông báo** → user không biết session đã đổi

## Giải pháp đã implement

### 1. Cải thiện `AuthController.Login()`

**Thêm tham số `force`:**
```csharp
[HttpGet("login")]
public async Task<IActionResult> Login([FromQuery] string? returnUrl = null, [FromQuery] bool force = false)
```

**Logic mới:**
- Check session hiện tại trước khi redirect đến Keycloak
- Nếu đã có session hợp lệ và `force=false` → redirect về `returnUrl`
- Nếu `force=true` hoặc không có session → tiếp tục OAuth flow

**Cách sử dụng:**
```javascript
// Login bình thường (check session trước)
window.location.href = '/auth/login?returnUrl=/dashboard';

// Force login (bỏ qua check session)
window.location.href = '/auth/login?returnUrl=/dashboard&force=true';
```

### 2. Cải thiện `SignInCallback()`

**Cleanup session cũ:**
```csharp
// Clean up old session if exists
string? oldSessionId = null;
if (Request.Cookies.TryGetValue(CookieConstants.SessionIdCookieName, out oldSessionId) &&
    !string.IsNullOrEmpty(oldSessionId))
{
    var oldSession = await _sessionManager.GetSessionAsync(oldSessionId);
    if (oldSession != null)
    {
        // Revoke old refresh token for security
        await _oauthClient.RevokeTokenAsync(oldSession.RefreshToken);
        
        // Remove old session from Redis
        await _sessionManager.RemoveSessionAsync(oldSessionId);
    }
}
```

### 3. Thêm `CleanupOldSessionAsync()` method

**Interface:**
```csharp
/// <summary>
/// Clean up old session when creating new one (for re-login scenarios)
/// Revokes old tokens and removes old session
/// </summary>
Task CleanupOldSessionAsync(string oldSessionId);
```

**Implementation:**
- Lấy session cũ từ Redis
- Revoke old refresh token (nếu có IOAuthClient)
- Xóa session cũ khỏi Redis
- Log cleanup actions

## Flow mới khi user click Login

### Trường hợp 1: User chưa login hoặc session expired
```
1. GET /auth/login
2. Không có session hợp lệ → tiếp tục OAuth flow
3. Redirect đến Keycloak
4. ... (flow bình thường)
```

### Trường hợp 2: User đã login, click Login (force=false)
```
1. GET /auth/login
2. Check session hiện tại → có session hợp lệ
3. Redirect về returnUrl (không cần login lại)
```

### Trường hợp 3: User đã login, click Login (force=true)
```
1. GET /auth/login?force=true
2. Bỏ qua check session → tiếp tục OAuth flow
3. Redirect đến Keycloak
4. Callback về → cleanup session cũ
5. Tạo session mới → set cookie mới
```

## Lợi ích

### 1. Tiết kiệm bộ nhớ Redis
- Session cũ được xóa ngay khi tạo session mới
- Không còn orphaned sessions

### 2. Tăng bảo mật
- Revoke old refresh tokens
- Tokens cũ không còn hiệu lực

### 3. Trải nghiệm người dùng tốt hơn
- Không cần login lại nếu đã có session hợp lệ
- Option `force=true` cho trường hợp cần login lại

### 4. Logging rõ ràng
```
[INFO] User already has valid session abc123, redirecting to /dashboard
[INFO] Revoked old refresh token for session: abc123
[INFO] Removed old session: abc123
[INFO] User logged in successfully, new session: xyz789, old session cleaned: abc123
```

## Backward Compatibility

- API không thay đổi (chỉ thêm optional parameter `force`)
- Existing clients vẫn hoạt động bình thường
- Chỉ cải thiện behavior, không break changes

## Testing

### Test Cases

1. **Normal login (no existing session)**
   - Verify OAuth flow works normally
   - Verify session created correctly

2. **Re-login with existing session (force=false)**
   - Verify redirect to returnUrl without OAuth
   - Verify existing session unchanged

3. **Force re-login (force=true)**
   - Verify OAuth flow executed
   - Verify old session cleaned up
   - Verify old tokens revoked

4. **Re-login with invalid existing session**
   - Verify OAuth flow executed
   - Verify no errors during cleanup

### Manual Testing

```bash
# Test 1: Normal login
curl -v http://localhost:5238/auth/login?returnUrl=/dashboard

# Test 2: Re-login with session (should redirect)
curl -v http://localhost:5238/auth/login?returnUrl=/dashboard \
  -H "Cookie: session_id=existing_session"

# Test 3: Force re-login
curl -v http://localhost:5238/auth/login?returnUrl=/dashboard&force=true \
  -H "Cookie: session_id=existing_session"
```

## Redis Data Before/After

### Before (có vấn đề):
```
ApiGateway_session:old_session → { tokens cũ } (orphaned)
ApiGateway_session:new_session → { tokens mới } (active)
```

### After (đã sửa):
```
ApiGateway_session:new_session → { tokens mới } (active)
// Session cũ đã được xóa
```