namespace TLBIOMASS.Application.Features.Materials.DTOs;

public class MaterialResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ProposedImpurityDeduction { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MaterialCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ProposedImpurityDeduction { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MaterialUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ProposedImpurityDeduction { get; set; }
    public bool IsActive { get; set; }
}

public class MaterialFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
