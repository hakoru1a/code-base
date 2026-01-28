using Shared.DTOs;

namespace Shared.DTOs.Agency;

public class AgencyResponseDto : BaseResponseDto<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityCard { get; set; }
    public string? IssuePlace { get; set; }
    public DateTime? IssueDate { get; set; }
    public bool IsActive { get; set; }
}
