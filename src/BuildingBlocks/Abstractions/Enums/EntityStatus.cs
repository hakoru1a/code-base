namespace Abstractions.Enums;

/// <summary>
/// Trạng thái bản ghi. Dùng <see cref="EntityStatus?"/> khi giá trị có thể null.
/// </summary>
public enum EntityStatus
{
    /// <summary>Đã xóa / ẩn</summary>
    Delete = 0,

    /// <summary>Đang hoạt động</summary>
    Active = 1

    // Bổ sung trạng thái sau: thêm enum value tại đây, ví dụ:
    // Pending = 2,
    // Archived = 3,
}
