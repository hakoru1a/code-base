namespace TLBIOMASS.Application.Features.Customers.DTOs;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? TaxCode { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
