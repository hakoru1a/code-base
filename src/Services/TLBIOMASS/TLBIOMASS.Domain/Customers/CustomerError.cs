using Contracts.Exceptions;

namespace TLBIOMASS.Domain.Customers;

public static class CustomerError
{
    public static BusinessException NameCannotBeEmpty()
        => new("Customer name cannot be empty");

    public static BusinessException NameTooLong(int maxLength = 200)
        => new($"Customer name cannot exceed {maxLength} characters");

    public static BusinessException EmailInvalidFormat()
        => new("Invalid email format");

    public static BusinessException TaxCodeAlreadyExists(string taxCode)
        => new($"Tax code '{taxCode}' already exists");
}
