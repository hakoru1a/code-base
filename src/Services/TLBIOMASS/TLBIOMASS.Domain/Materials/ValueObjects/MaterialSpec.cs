namespace TLBIOMASS.Domain.Materials.ValueObjects;

/// <summary>Thông tin quy cách vật tư: tên, đơn vị, mô tả, tỷ lệ trừ tạp chất.</summary>
public record MaterialSpec(
    string Name,
    string Unit,
    string? Description = null,
    decimal? ProposedImpurityDeduction = 0);
