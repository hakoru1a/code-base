# RBAC Guide - Hướng dẫn Role-Based Access Control

## 📖 Table of Contents
1. [Giới thiệu về RBAC](#-giới-thiệu-về-rbac)
2. [Quản lý Roles trong Keycloak](#-quản-lý-roles-trong-keycloak)
3. [Sử dụng Roles trong ứng dụng .NET](#-sử-dụng-roles-trong-ứng-dụng-net)
4. [Best Practices](#-best-practices)

---

## 🎯 Giới thiệu về RBAC

**Role-Based Access Control (RBAC)** là một mô hình phân quyền trong đó quyền truy cập được gán cho các "vai trò" (roles) thay vì cho từng người dùng riêng lẻ. Người dùng sau đó sẽ được gán các vai trò này, và qua đó kế thừa các quyền tương ứng.

- **Ví dụ**: Thay vì cấp quyền "xóa sản phẩm" cho 3 người dùng `Alice`, `Bob`, và `Carol`, chúng ta tạo một vai trò tên là `ProductManager`, gán quyền "xóa sản phẩm" cho vai trò đó, và sau đó gán vai trò `ProductManager` cho `Alice`, `Bob`, và `Carol`.

Mô hình này giúp đơn giản hóa việc quản lý quyền hạn, đặc biệt là trong các hệ thống có nhiều người dùng và quyền hạn phức tạp.

## 🔑 Quản lý Roles trong Keycloak

Keycloak là nơi trung tâm để chúng ta định nghĩa và quản lý tất cả các vai trò trong hệ thống.

### 1. Realm Roles vs. Client Roles

Keycloak cung cấp hai loại vai trò:
-   **Realm Roles**: Là các vai trò chung cho toàn bộ "realm" (không gian làm việc). Chúng có thể được sử dụng bởi bất kỳ client (ứng dụng) nào trong realm đó.
-   **Client Roles**: Là các vai trò chỉ dành riêng cho một client cụ thể.

**Khuyến nghị**: Ưu tiên sử dụng **Realm Roles** cho các vai trò liên quan đến nghiệp vụ chung (e.g., `admin`, `manager`, `user`) để đảm bảo tính nhất quán và dễ quản lý. Chỉ sử dụng Client Roles khi một vai trò thực sự chỉ có ý nghĩa trong phạm vi một ứng dụng duy nhất.

### 2. Cách tạo Role trong Keycloak

1.  Đăng nhập vào Keycloak Admin Console.
2.  Từ menu bên trái, chọn **Roles**.
3.  Click vào nút **Add Role**.
4.  Nhập `Role Name` (ví dụ: `OrderManager`) và một `Description` (mô tả) nếu cần.
5.  Click **Save**.

![Create Role in Keycloak](https://www.keycloak.org/docs/latest/server_admin/images/role-add.png)
*(Hình ảnh minh họa từ tài liệu Keycloak)*

### 3. Cách gán Role cho User

1.  Từ menu bên trái, chọn **Users**.
2.  Tìm và click vào user bạn muốn gán vai trò.
3.  Chuyển sang tab **Role Mappings**.
4.  Trong mục "Available Roles", tìm vai trò bạn muốn gán và click vào nó.
5.  Click **Add selected** để gán vai trò cho người dùng.

Vai trò được gán sẽ xuất hiện trong claim `realm_access.roles` của token JWT khi người dùng đó đăng nhập.

---

## ⚙️ Sử dụng Roles trong ứng dụng .NET

Hệ thống backend .NET của chúng ta được cấu hình để tự động đọc và hiểu các vai trò từ JWT token do Keycloak cung cấp.

Khi một request chứa JWT token hợp lệ được gửi đến, middleware xác thực của ASP.NET Core sẽ:
1.  Giải mã token.
2.  Tìm đến claim `realm_access`.
3.  Đọc mảng `roles` bên trong claim đó.
4.  Tạo ra một `ClaimsPrincipal` (đại diện cho người dùng) và điền các vai trò đã đọc được vào danh tính của người dùng.

Nhờ đó, các lập trình viên có thể dễ dàng kiểm tra vai trò của người dùng ở bất kỳ đâu trong ứng dụng bằng hai cách chính:

1.  **Sử dụng Attribute `[Authorize]`**: Đây là cách phổ biến và được khuyến khích nhất để bảo vệ các API endpoint.

    ```csharp
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // Chỉ những user có vai trò "Admin" mới được gọi API này
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        public IActionResult DeleteProduct(int id)
        {
            // ... logic xóa sản phẩm
        }

        // User cần có vai trò "Admin" HOẶC "ProductManager"
        [HttpPost]
        [Authorize(Roles = "Admin,ProductManager")] 
        public IActionResult CreateProduct(ProductDto product)
        {
            // ... logic tạo sản phẩm
        }
    }
    ```

2.  **Kiểm tra trong code (Programmatic Check)**: Sử dụng `User.IsInRole()` để kiểm tra vai trò một cách tường minh trong logic code.

    ```csharp
    public class OrderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void ApproveOrder(Order order)
        {
            var user = _httpContextAccessor.HttpContext.User;

            // Kiểm tra nếu user có vai trò "Manager" hoặc "Admin"
            if (user.IsInRole("Manager") || user.IsInRole("Admin"))
            {
                // ... logic duyệt đơn hàng
            }
            else
            {
                throw new UnauthorizedAccessException("User is not authorized to approve orders.");
            }
        }
    }
    ```

---

## 👍 Best Practices

-   **Đặt tên Role rõ ràng**: Sử dụng các tên vai trò gắn liền với nghiệp vụ (e.g., `FinanceAuditor`, `ContentEditor`) thay vì các tên chung chung như `role1`, `user_level_2`.
-   **Nguyên tắc Đặc quyền Tối thiểu (Principle of Least Privilege)**: Chỉ gán cho người dùng những vai trò và quyền hạn thực sự cần thiết để họ hoàn thành công việc. Tránh gán vai trò `Admin` một cách tùy tiện.
-   **Review định kỳ**: Thường xuyên xem xét lại danh sách các vai trò và quyền hạn của chúng để đảm bảo chúng vẫn còn phù hợp với nhuệ cầu của hệ thống và không có quyền thừa.
-   **Kết hợp với PBAC**: Đối với các logic phân quyền phức tạp hơn (ví dụ: "chỉ được sửa đơn hàng của chính mình"), hãy sử dụng PBAC thay vì cố gắng tạo ra quá nhiều vai trò nhỏ lẻ. RBAC mạnh ở việc phân loại người dùng, còn PBAC mạnh ở việc kiểm tra các quy tắc động.