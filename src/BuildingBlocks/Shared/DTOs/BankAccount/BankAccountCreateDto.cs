using Shared.Domain.Enums;

namespace Shared.DTOs.BankAccount;

public class BankAccountCreateDto
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public OwnerType OwnerType { get; set; }
    public int OwnerId { get; set; }
    public bool IsDefault { get; set; }
}
