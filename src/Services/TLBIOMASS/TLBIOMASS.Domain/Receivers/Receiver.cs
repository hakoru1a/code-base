using Contracts.Domain;
using Contracts.Domain.Interface;
using TLBIOMASS.Domain.Receivers.Rules;

using TLBIOMASS.Domain.Receivers.Events;

namespace TLBIOMASS.Domain.Receivers;

public class Receiver : EntityBase<int>
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? BankAccount { get; private set; }
    public string? BankName { get; private set; }
    public string? IdentityNumber { get; private set; }
    public DateTime? IssuedDate { get; private set; }
    public string? IssuedPlace { get; private set; }
    public string? Address { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Note { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Protected constructor for EF Core
    protected Receiver() { }

    private Receiver(
        string name,
        string? phone,
        string? bankAccount,
        string? bankName,
        string? identityNumber,
        DateTime? issuedDate,
        string? issuedPlace,
        string? address,
        bool isDefault,
        bool isActive,
        string? note,
        DateTime? dateOfBirth)
    {
        Name = name;
        Phone = phone;
        BankAccount = bankAccount;
        BankName = bankName;
        IdentityNumber = identityNumber;
        IssuedDate = issuedDate;
        IssuedPlace = issuedPlace;
        Address = address;
        IsDefault = isDefault;
        Note = note;
        DateOfBirth = dateOfBirth;
        CreatedAt = DateTime.Now;
        IsActive = isActive;
    }

    // Factory Method
    public static Receiver Create(
        string name,
        string? phone = null,
        string? bankAccount = null,
        string? bankName = null,
        string? identityNumber = null,
        DateTime? issuedDate = null,
        string? issuedPlace = null,
        string? address = null,
        bool isDefault = false,
        bool isActive = true,
        string? note = null,
        DateTime? dateOfBirth = null)
    {
        CheckRule(new ReceiverNameRequiredRule(name));

        var receiver = new Receiver(
            name,
            phone,
            bankAccount,
            bankName,
            identityNumber,
            issuedDate,
            issuedPlace,
            address,
            isDefault,
            isActive,
            note,
            dateOfBirth);

        // receiver.AddDomainEvent(new ReceiverCreatedEvent { ReceiverId = receiver.Id, Name = receiver.Name });

        return receiver;
    }

    // Update method
    public void Update(
        string name,
        string? phone,
        string? bankAccount,
        string? bankName,
        string? identityNumber,
        DateTime? issuedDate,
        string? issuedPlace,
        string? address,
        bool isDefault,
        bool isActive,
        string? note,
        DateTime? dateOfBirth)
    {
        CheckRule(new ReceiverNameRequiredRule(name));

        Name = name;
        Phone = phone;
        BankAccount = bankAccount;
        BankName = bankName;
        IdentityNumber = identityNumber;
        IssuedDate = issuedDate;
        IssuedPlace = issuedPlace;
        Address = address;
        IsDefault = isDefault;
        IsActive = isActive;
        Note = note;
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTime.Now;

        // AddDomainEvent(new ReceiverUpdatedEvent { ReceiverId = Id, Name = Name });
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDefault(bool isDefault) => IsDefault = isDefault;
}
