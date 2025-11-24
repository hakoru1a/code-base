# RBAC Quick Start - Hướng dẫn nhanh

Hướng dẫn này chỉ cho bạn cách nhanh nhất để bảo vệ một API endpoint bằng cách sử dụng vai trò (Roles).

## 🎯 Mục tiêu

Bảo vệ một API để chỉ những người dùng có vai trò (role) cụ thể mới có thể truy cập.

**Tình huống**: Chúng ta muốn tạo một endpoint để xóa sản phẩm, và chỉ những user có vai trò `Admin` mới được phép thực hiện hành động này.

---

## 🚀 Các bước thực hiện

### Bước 1: Đảm bảo Role đã tồn tại trong Keycloak

Trước tiên, hãy chắc chắn rằng vai trò `Admin` đã được tạo trong Keycloak và đã được gán cho người dùng bạn sẽ dùng để test.

> 📚 Xem chi tiết cách tạo và gán vai trò trong [RBAC Guide](./guide.md).

### Bước 2: Bảo vệ API Endpoint với `[Authorize]`

Đây là cách đơn giản và phổ biến nhất. Chỉ cần thêm attribute `[Authorize(Roles = "...")]` ngay trên action của controller.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    // ... các actions khác

    /// <summary>
    /// Xóa một sản phẩm theo ID.
    /// Yêu cầu user phải có vai trò "Admin".
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // <--- BẢO VỆ ENDPOINT
    public IActionResult DeleteProduct(int id)
    {
        // Nếu user không có vai trò "Admin", request sẽ bị từ chối với lỗi 403 Forbidden
        // và sẽ không bao giờ chạy đến đoạn code bên dưới.

        _productService.Delete(id);

        return NoContent(); // Trả về 204 No Content khi thành công
    }

    /// <summary>
    /// Chuyển sản phẩm sang trạng thái "featured".
    /// Yêu cầu user phải có vai trò "Admin" HOẶC "ProductManager".
    /// </summary>
    [HttpPost("{id}/feature")]
    [Authorize(Roles = "Admin,ProductManager")] // <--- NHIỀU ROLE (logic OR)
    public IActionResult FeatureProduct(int id)
    {
        _productService.SetFeatured(id);
        return Ok();
    }
}
```

**Ghi chú:**
-   **Một Role**: `[Authorize(Roles = "Admin")]`
-   **Nhiều Roles (Logic OR)**: `[Authorize(Roles = "Admin,ProductManager")]` - User chỉ cần có *một trong các* vai trò được liệt kê.

### Bước 3: (Tùy chọn) Kiểm tra Role bên trong Code

Đôi khi bạn cần các logic phức tạp hơn và phải kiểm tra vai trò của người dùng ngay trong một service hoặc một phương thức.

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class ReportService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReportService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public FinancialReport GenerateFinancialReport()
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext.User;

        // Kiểm tra xem user có phải là "Admin" hoặc thuộc phòng "Finance" không
        if (user.IsInRole("Admin") || user.IsInRole("FinanceTeam"))
        {
            // Logic tạo report tài chính phức tạp chỉ dành cho người có quyền
            return _internalReportGenerator.CreateFinanceReport();
        }
        else
        {
            // Đối với những user khác, trả về một báo cáo đã được đơn giản hóa
            return _internalReportGenerator.CreatePublicSummaryReport();
        }
    }
}
```
**Lưu ý**: Để sử dụng `IHttpContextAccessor`, bạn cần đăng ký nó trong `Startup.cs` hoặc `Program.cs`:
```csharp
services.AddHttpContextAccessor();
```

---

## ✅ Cách kiểm tra (Testing)

1.  **Lấy JWT Token**: Dùng tài khoản user đã được gán vai trò (`Admin` trong ví dụ trên) để đăng nhập vào hệ thống và lấy token JWT.

2.  **Gọi API với Postman hoặc curl**:
    *   Tạo một request `DELETE` đến endpoint, ví dụ: `https://your-api.com/api/products/123`.
    *   Trong tab **Headers**, thêm một header mới:
        *   **Key**: `Authorization`
        *   **Value**: `Bearer <your_jwt_token_here>` (thay thế bằng token bạn đã lấy).

3.  **Kiểm tra kết quả**:
    *   **Nếu thành công**: Bạn sẽ nhận được response `204 No Content`.
    *   **Nếu thất bại (user không có role `Admin`)**: Bạn sẽ nhận được response `403 Forbidden`.
    *   **Nếu thất bại (không gửi token hoặc token không hợp lệ)**: Bạn sẽ nhận được response `401 Unauthorized`.