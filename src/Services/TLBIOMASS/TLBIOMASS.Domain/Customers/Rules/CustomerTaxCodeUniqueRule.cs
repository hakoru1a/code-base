using Microsoft.EntityFrameworkCore;
using Contracts.Domain.Interface;
using TLBIOMASS.Domain.Customers.Interfaces;

namespace TLBIOMASS.Domain.Customers.Rules;

public class CustomerTaxCodeUniqueRule : IBusinessRule
{
    private readonly ICustomerRepository _repository;
    private readonly string? _taxCode;
    private readonly int? _excludeId;

    public CustomerTaxCodeUniqueRule(ICustomerRepository repository, string? taxCode, int? excludeId = null)
    {
        _repository = repository;
        _taxCode = taxCode;
        _excludeId = excludeId;
    }

    public bool IsBroken()
    {
        if (string.IsNullOrEmpty(_taxCode)) return false;
        
        return _repository.FindByCondition(c => c.TaxCode == _taxCode && (_excludeId == null || c.Id != _excludeId))
            .AnyAsync().GetAwaiter().GetResult();
    }

    public string Message => "Tax code already exists in the system";
    public string Code => "Customer.TaxCodeUnique";
}
