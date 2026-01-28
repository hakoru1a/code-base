namespace Shared.Domain.ValueObjects;

/// <summary>Thông tin CMND/CCCD: số, nơi cấp, ngày cấp, ngày sinh (dùng chung cho Agency, Landowner, Receiver).</summary>
public record IdentityInfo
{
    public string? IdentityNumber { get; init; }
    public string? IssuePlace { get; init; }
    public DateTime? IssueDate { get; init; }
    public DateTime? DateOfBirth { get; init; }

    public IdentityInfo() { }

    public IdentityInfo(string? identityNumber, string? issuePlace, DateTime? issueDate, DateTime? dateOfBirth)
    {
        IdentityNumber = identityNumber;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
        DateOfBirth = dateOfBirth;
    }
}
