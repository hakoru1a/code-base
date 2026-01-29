using Contracts.Domain;

namespace TLBIOMASS.Domain.BankAccounts;

public class BankAccount : EntityAuditBase<int>
{
    public string BankName { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public string OwnerType { get; private set; } = string.Empty; // Using string for flexibility (Agency, Landowner, Receiver, etc.)
    public int OwnerId { get; private set; }
    public bool IsDefault { get; private set; }

    protected BankAccount() { }

    private BankAccount(string bankName, string accountNumber, string ownerType, int ownerId, bool isDefault)
    {
        BankName = bankName;
        AccountNumber = accountNumber;
        OwnerType = ownerType;
        OwnerId = ownerId;
        IsDefault = isDefault;
    }

    public static BankAccount Create(string bankName, string accountNumber, string ownerType, int ownerId, bool isDefault = false)
    {
        return new BankAccount(bankName, accountNumber, ownerType, ownerId, isDefault);
    }

    public void Update(string bankName, string accountNumber, bool isDefault)
    {
        BankName = bankName;
        AccountNumber = accountNumber;
        IsDefault = isDefault;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}
