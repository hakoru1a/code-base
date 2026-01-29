using Shared.DTOs;

namespace Shared.DTOs.BankAccount;

public class BankAccountResponseDto : BaseResponseDto<int>
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerType { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public bool IsDefault { get; set; }
}
