namespace TLBIOMASS.Application.Common.DTOs;


public abstract class BaseFilterDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}
