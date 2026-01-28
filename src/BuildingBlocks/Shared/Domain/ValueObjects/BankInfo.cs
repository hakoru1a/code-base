namespace Shared.Domain.ValueObjects;

/// <summary>Thông tin ngân hàng: số tài khoản, tên ngân hàng (dùng chung cho Agency, Landowner, Receiver).</summary>
public record BankInfo(
    string? BankAccount = null,
    string? BankName = null);
