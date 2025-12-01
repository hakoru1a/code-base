using Contracts.Domain.Interface;

namespace Contracts.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// Contains information about the specific rule that was broken.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    public string Code { get; }
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
        Code = brokenRule.Code;
    }

    public BusinessRuleValidationException(IBusinessRule brokenRule, Exception innerException)
        : base(brokenRule.Message, innerException)
    {
        BrokenRule = brokenRule;
        Code = brokenRule.Code;
    }
}
