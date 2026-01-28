namespace Shared.Domain.ValueObjects;

/// <summary>Thông tin CMND/CCCD: số, nơi cấp, ngày cấp, ngày sinh (dùng chung cho Agency, Landowner, Receiver).</summary>
public record IdentityInfo(
    string? IdentityNumber = null,
    string? IssuePlace = null,
    DateTime? IssueDate = null,
    DateTime? DateOfBirth = null);
