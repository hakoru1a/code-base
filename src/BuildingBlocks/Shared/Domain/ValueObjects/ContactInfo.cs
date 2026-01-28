namespace Shared.Domain.ValueObjects;

/// <summary>Thông tin liên hệ: phone, email, địa chỉ, ghi chú (dùng chung cho Agency, Customer, Landowner, Receiver).</summary>
public record ContactInfo
{
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Note { get; init; }

    public ContactInfo() { }

    public ContactInfo(string? phone, string? email, string? address, string? note)
    {
        Phone = phone;
        Email = email;
        Address = address;
        Note = note;
    }
}
