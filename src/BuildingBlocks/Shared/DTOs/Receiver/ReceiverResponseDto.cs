using Shared.DTOs;
using Shared.DTOs.BankAccount;
using Contracts.Domain.Enums;

namespace Shared.DTOs.Receiver;

public class ReceiverResponseDto : BaseResponseDto<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? IssuedPlace { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public EntityStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public List<BankAccountResponseDto>? BankAccounts { get; set; }
}
