namespace Shared.DTOs.BankAccount;

public class BankAccountSyncDto
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
