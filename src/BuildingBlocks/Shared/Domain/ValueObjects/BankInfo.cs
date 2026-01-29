namespace Shared.Domain.ValueObjects;

/// <summary>Thông tin ngân hàng: số tài khoản, tên ngân hàng (dùng chung cho Agency, Landowner, Receiver).</summary>
public record BankInfo
{
    public string? BankAccount { get; init; }
    public string? BankName { get; init; }

    public BankInfo() { }

    public BankInfo(string? bankAccount, string? bankName)
    {
        BankAccount = bankAccount;
        BankName = bankName;
    }
}
