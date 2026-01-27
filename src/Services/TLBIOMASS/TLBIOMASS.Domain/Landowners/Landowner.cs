using Contracts.Domain;
using TLBIOMASS.Domain.Landowners.Rules;

namespace TLBIOMASS.Domain.Landowners;

public class Landowner : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? BankAccount { get; private set; }
    public string? BankName { get; private set; }
    public string? IdentityCardNo { get; private set; }
    public string? IssuePlace { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Protected constructor for EF Core
    protected Landowner() { }

    private Landowner(
        string name,
        string? phone,
        string? email,
        string? address,
        string? bankAccount,
        string? bankName,
        string? identityCardNo,
        string? issuePlace,
        DateTime? issueDate,
        DateTime? dateOfBirth,
        bool isActive)
    {
        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        BankAccount = bankAccount;
        BankName = bankName;
        IdentityCardNo = identityCardNo;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
        DateOfBirth = dateOfBirth;
        IsActive = isActive;
    }

    // Factory Method
    public static Landowner Create(
        string name,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? bankAccount = null,
        string? bankName = null,
        string? identityCardNo = null,
        string? issuePlace = null,
        DateTime? issueDate = null,
        DateTime? dateOfBirth = null,
        bool isActive = true)
    {
        CheckRule(new LandownerNameRequiredRule(name));

        return new Landowner(
            name,
            phone,
            email,
            address,
            bankAccount,
            bankName,
            identityCardNo,
            issuePlace,
            issueDate,
            dateOfBirth,
            isActive);
    }

    // Update method
    public void Update(
        string name,
        string? phone,
        string? email,
        string? address,
        string? bankAccount,
        string? bankName,
        string? identityCardNo,
        string? issuePlace,
        DateTime? issueDate,
        DateTime? dateOfBirth,
        bool isActive)
    {
        CheckRule(new LandownerNameRequiredRule(name));

        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        BankAccount = bankAccount;
        BankName = bankName;
        IdentityCardNo = identityCardNo;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
        DateOfBirth = dateOfBirth;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
