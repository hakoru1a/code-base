using Shared.DTOs;
using Shared.DTOs.BankAccount;

namespace Shared.DTOs.Landowner;

public class LandownerResponseDto : BaseResponseDto<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityCardNo { get; set; }
    public string? IssuePlace { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public List<BankAccountResponseDto>? BankAccounts { get; set; }
}
