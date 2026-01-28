using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Receivers.Specifications;

public class ReceiverIsActiveSpecification : ISpecification<Receiver>
{
    private readonly bool _isActive;

    public ReceiverIsActiveSpecification(bool isActive) => _isActive = isActive;

    public bool IsSatisfiedBy(Receiver receiver) => receiver.IsActive == _isActive;

    public Expression<Func<Receiver, bool>> ToExpression()
    {
        return c => c.IsActive == _isActive;
    }
}
