# 🔧 Troubleshooting - Cải tiến Bảo mật

## 📋 Các vấn đề thường gặp

### **1. Session Validation Issues**

#### **Lỗi: "Session validation failed. Please login again."**

**Nguyên nhân:**
- Client fingerprint mismatch
- Session bị invalidate do security breach
- Network/proxy configuration thay đổi

**Cách khắc phục:**
```bash
# 1. Check logs để xem lý do cụ thể
docker logs api-gateway | grep "fingerprint mismatch"

# 2. Kiểm tra user agent và IP
curl -H "User-Agent: YourApp/1.0" http://localhost:5238/auth/user

# 3. Clear session và login lại
# Frontend: Clear cookies và redirect to login
```

**Debug:**
```csharp
// Temporary disable fingerprint validation for testing
// In ClientFingerprintService.ValidateFingerprint()
public bool ValidateFingerprint(string storedFingerprint, HttpContext context)
{
    // TODO: Remove this line after debugging
    return true; // Disable validation temporarily
    
    // ... rest of method
}
```

---

### **2. JWT Token Validation Issues**

#### **Lỗi: "Token validation failed: Invalid audience"**

**Nguyên nhân:**
- Token được issue cho client khác
- Audience configuration không đúng
- Cross-client token usage

**Cách khắc phục:**
```bash
# 1. Check token claims
echo "YOUR_JWT_TOKEN" | base64 -d | jq .

# 2. Verify audience in token
# Expected: "aud": ["gateway", "account"]

# 3. Check Keycloak client configuration
# Valid Audiences should include "gateway"
```

**Configuration Fix:**
```json
// In Keycloak client settings
{
  "clientId": "gateway",
  "standardFlowEnabled": true,
  "directAccessGrantsEnabled": true,
  "attributes": {
    "access.token.lifespan": "300"
  }
}
```

#### **Lỗi: "Token is too old"**

**Nguyên nhân:**
- Token được issue quá lâu trước (>24h)
- System clock không sync
- Replay attack

**Cách khắc phục:**
```bash
# 1. Check system time
date
timedatectl status

# 2. Sync time if needed
sudo ntpdate -s time.nist.gov

# 3. Check token issued time
# JWT payload: "iat" claim should be recent
```

---

### **3. Performance Issues**

#### **Lỗi: High memory usage from JWT cache**

**Nguyên nhân:**
- Quá nhiều unique tokens được cache
- Cache không expire đúng cách
- Memory leak

**Monitoring:**
```csharp
// Add to your monitoring
public class JwtCacheMetrics
{
    public void LogCacheStats(IMemoryCache cache)
    {
        // Monitor cache size, hit rate, evictions
        _logger.LogInformation("JWT Cache Stats: Size={Size}, HitRate={HitRate}%", 
            GetCacheSize(cache), GetHitRate());
    }
}
```

**Cách khắc phục:**
```csharp
// Adjust cache settings in JwtClaimsCache
entry.Priority = CacheItemPriority.Normal; // Instead of High
entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // Reduce from 10
```

#### **Lỗi: Redis connection timeout**

**Nguyên nhân:**
- Redis overload
- Network issues
- Connection pool exhaustion

**Cách khắc phục:**
```bash
# 1. Check Redis health
redis-cli ping

# 2. Monitor Redis metrics
redis-cli info memory
redis-cli info clients

# 3. Check connection pool
# In appsettings.json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "PoolSize": 50,
    "ConnectTimeout": 5000,
    "SyncTimeout": 1000
  }
}
```

---

### **4. Development/Testing Issues**

#### **Disable security features for testing**

**Temporary workarounds:**
```csharp
// 1. Disable fingerprint validation
// In Program.cs
#if DEBUG
builder.Services.AddScoped<IClientFingerprintService, MockClientFingerprintService>();
#endif

public class MockClientFingerprintService : IClientFingerprintService
{
    public string GenerateFingerprint(HttpContext context) => "test-fingerprint";
    public bool ValidateFingerprint(string stored, HttpContext context) => true;
}

// 2. Disable token age validation
// In KeycloakAuthenticationExtensions.ValidateTokenSecurityClaims()
#if DEBUG
return true; // Skip validation in debug mode
#endif

// 3. Extend session timeout for testing
// In UserSession.GetSessionTimeout()
#if DEBUG
return TimeSpan.FromHours(24); // Long timeout for testing
#endif
```

---

### **5. Migration Issues**

#### **Existing users getting logged out**

**Nguyên nhân:**
- Existing sessions không có fingerprint
- Backward compatibility không hoạt động

**Cách khắc phục:**
```csharp
// In SessionManager.ValidateSessionContextAsync()
public async Task<bool> ValidateSessionContextAsync(string sessionId, HttpContext httpContext)
{
    var session = await GetSessionWithoutUpdateAsync(sessionId);
    if (session == null) return false;

    // Backward compatibility: Skip validation if no fingerprint
    if (string.IsNullOrEmpty(session.ClientFingerprint))
    {
        _logger.LogInformation("Skipping fingerprint validation for legacy session: {SessionId}", sessionId);
        return true; // Allow legacy sessions
    }

    // ... rest of validation
}
```

---

### **6. Monitoring & Alerts**

#### **Setup monitoring queries**

**Elasticsearch/Kibana:**
```json
// High fingerprint validation failures
{
  "query": {
    "bool": {
      "must": [
        {"match": {"message": "fingerprint mismatch"}},
        {"range": {"@timestamp": {"gte": "now-1h"}}}
      ]
    }
  }
}

// JWT cache performance
{
  "aggs": {
    "cache_hit_rate": {
      "terms": {"field": "cache_result"},
      "aggs": {
        "avg_response_time": {"avg": {"field": "response_time_ms"}}
      }
    }
  }
}
```

**Application Insights:**
```csharp
// Custom metrics
public void TrackSecurityEvent(string eventType, Dictionary<string, string> properties)
{
    _telemetryClient.TrackEvent($"Security.{eventType}", properties);
}

// Usage
TrackSecurityEvent("FingerprintMismatch", new Dictionary<string, string>
{
    ["SessionId"] = sessionId,
    ["UserId"] = userId,
    ["IpAddress"] = ipAddress
});
```

---

### **7. Emergency Procedures**

#### **Rollback security features**

**Nếu có vấn đề nghiêm trọng:**

```bash
# 1. Disable fingerprint validation
# Set environment variable
export DISABLE_FINGERPRINT_VALIDATION=true

# 2. Disable enhanced JWT validation  
export DISABLE_ENHANCED_JWT_VALIDATION=true

# 3. Clear JWT cache
# Restart application hoặc
redis-cli FLUSHDB
```

**Code changes for emergency rollback:**
```csharp
// In Program.cs
var disableFingerprintValidation = Environment.GetEnvironmentVariable("DISABLE_FINGERPRINT_VALIDATION") == "true";

if (disableFingerprintValidation)
{
    builder.Services.AddScoped<IClientFingerprintService, NoOpClientFingerprintService>();
}

public class NoOpClientFingerprintService : IClientFingerprintService
{
    public string GenerateFingerprint(HttpContext context) => string.Empty;
    public bool ValidateFingerprint(string stored, HttpContext context) => true;
}
```

---

### **8. Common Log Messages**

#### **Normal Operations:**
```
[Information] Session created for user alice (ID: 123), SessionId: abc..., IP: 192.168.1.100
[Information] JWT Cache Stats: HitRate=92%, Size=1.2MB
[Debug] Caching JWT claims for signature: AbCdEfGh
```

#### **Security Events:**
```
[Warning] Client fingerprint mismatch for session abc123, User: alice
[Warning] Token validation failed: Invalid audience. Expected: gateway, Found: mobile-app
[Warning] Token is too old. IssuedAt: 2026-01-11T10:00:00Z, MaxAge: 24:00:00
```

#### **Performance Issues:**
```
[Warning] JWT cache miss rate high: 25% (target: <10%)
[Error] Redis connection timeout after 5000ms
[Warning] Session validation took 150ms (target: <50ms)
```

---

### **9. Health Checks**

#### **Verify system health:**
```bash
# 1. Check authentication health
curl http://localhost:5238/auth/health

# 2. Check JWT validation
curl -H "Authorization: Bearer VALID_TOKEN" http://localhost:5238/api/generate/health

# 3. Check Redis connectivity
curl http://localhost:5238/health | jq '.checks.redis'

# 4. Check cache performance
curl http://localhost:5238/_dev/cache-stats
```

#### **Performance benchmarks:**
```bash
# Load test authentication flow
ab -n 1000 -c 10 -C "session_id=valid_session" http://localhost:5238/auth/user

# Expected results:
# - Response time: <100ms (95th percentile)
# - Success rate: >99%
# - Cache hit rate: >90%
```

---

**Lưu ý:** Luôn test thoroughly trước khi deploy các thay đổi security lên production. Có sẵn rollback plan và monitoring alerts.