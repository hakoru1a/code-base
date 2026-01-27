using Contracts.Domain;
using TLBIOMASS.Domain.Agencies.Rules;

namespace TLBIOMASS.Domain.Agencies;

public class Agency : EntityAuditBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? BankAccount { get; private set; }
    public string? BankName { get; private set; }
    public string? IdentityCard { get; private set; }
    public string? IssuePlace { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Protected constructor for EF Core
    protected Agency() { }

    private Agency(
        string name,
        string? phone,
        string? email,
        string? address,
        string? bankAccount,
        string? bankName,
        string? identityCard,
        string? issuePlace,
        DateTime? issueDate,
        bool isActive)
    {
        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        BankAccount = bankAccount;
        BankName = bankName;
        IdentityCard = identityCard;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
        IsActive = isActive;
    }

    // Factory Method
    public static Agency Create(
        string name,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? bankAccount = null,
        string? bankName = null,
        string? identityCard = null,
        string? issuePlace = null,
        DateTime? issueDate = null,
        bool isActive = true)
    {
        CheckRule(new AgencyNameRequiredRule(name));

        return new Agency(
            name,
            phone,
            email,
            address,
            bankAccount,
            bankName,
            identityCard,
            issuePlace,
            issueDate,
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
        string? identityCard,
        string? issuePlace,
        DateTime? issueDate,
        bool isActive)
    {
        CheckRule(new AgencyNameRequiredRule(name));

        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        BankAccount = bankAccount;
        BankName = bankName;
        IdentityCard = identityCard;
        IssuePlace = issuePlace;
        IssueDate = issueDate;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
