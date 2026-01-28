namespace Shared.Domain.ValueObjects;

/// <summary>Thông tin liên hệ: phone, email, địa chỉ, ghi chú (dùng chung cho Agency, Customer, Landowner, Receiver).</summary>
public record ContactInfo(
    string? Phone = null,
    string? Email = null,
    string? Address = null,
    string? Note = null);
