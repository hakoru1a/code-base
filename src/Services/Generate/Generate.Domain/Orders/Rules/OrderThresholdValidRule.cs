using Contracts.Domain.Interface;

namespace Generate.Domain.Orders.Rules;

/// <summary>
/// Business rule that validates threshold value must be greater than zero.
/// </summary>
public class OrderThresholdValidRule : IBusinessRule
{
    private readonly int _threshold;

    public OrderThresholdValidRule(int threshold)
    {
        _threshold = threshold;
    }

    public bool IsBroken() => _threshold <= 0;

    public string Message => $"Threshold must be greater than zero, got: {_threshold}.";

    public string Code => "Order.InvalidThreshold";
}
