namespace TLBIOMASS.Application.Features.Receivers.DTOs;

public class ReceiverResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? IssuedPlace { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReceiverCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? IssuedPlace { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class ReceiverUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? IssuedPlace { get; set; }
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class ReceiverFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
