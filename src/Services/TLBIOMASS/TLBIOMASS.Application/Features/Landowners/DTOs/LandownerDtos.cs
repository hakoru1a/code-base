namespace TLBIOMASS.Application.Features.Landowners.DTOs;

public class LandownerResponseDto
{
    public int Id { get; set; }
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
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
}

public class LandownerCreateDto
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
    public bool IsActive { get; set; } = true;
}

public class LandownerUpdateDto
{
    public int Id { get; set; }
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
}

public class LandownerFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
