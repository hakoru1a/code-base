using Contracts.Domain.Interface;

namespace Contracts.Domain.Rules;

/// <summary>
/// Composite business rules để kết hợp nhiều rules
/// </summary>
public static class CompositeRule
{
    /// <summary>
    /// AND rule - cả hai rules phải không bị broken (tất cả rules phải pass)
    /// </summary>
    public class AndRule : IBusinessRule
    {
        private readonly IBusinessRule _left;
        private readonly IBusinessRule _right;

        public AndRule(IBusinessRule left, IBusinessRule right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public bool IsBroken() => _left.IsBroken() || _right.IsBroken();

        public string Message
        {
            get
            {
                if (_left.IsBroken())
                    return _left.Message;
                if (_right.IsBroken())
                    return _right.Message;
                return string.Empty;
            }
        }

        public string Code
        {
            get
            {
                if (_left.IsBroken())
                    return _left.Code;
                if (_right.IsBroken())
                    return _right.Code;
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// OR rule - một trong hai rules không bị broken là đủ (ít nhất một rule pass)
    /// </summary>
    public class OrRule : IBusinessRule
    {
        private readonly IBusinessRule _left;
        private readonly IBusinessRule _right;

        public OrRule(IBusinessRule left, IBusinessRule right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public bool IsBroken() => _left.IsBroken() && _right.IsBroken();

        public string Message
        {
            get
            {
                if (IsBroken())
                    return $"{_left.Message} or {_right.Message}";
                return string.Empty;
            }
        }

        public string Code
        {
            get
            {
                if (IsBroken())
                    return $"{_left.Code}|{_right.Code}";
                return string.Empty;
            }
        }
    }
}

