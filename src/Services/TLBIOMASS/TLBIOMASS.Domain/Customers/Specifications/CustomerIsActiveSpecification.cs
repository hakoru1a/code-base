using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Customers.Specifications;

public class CustomerIsActiveSpecification : ISpecification<Customer>
{
    private readonly bool _isActive;

    public CustomerIsActiveSpecification(bool isActive = true)
    {
        _isActive = isActive;
    }

    public bool IsSatisfiedBy(Customer customer)
    {
        return ToExpression().Compile()(customer);
    }

    public Expression<Func<Customer, bool>> ToExpression()
    {
        return customer => customer.IsActive == _isActive;
    }
}
