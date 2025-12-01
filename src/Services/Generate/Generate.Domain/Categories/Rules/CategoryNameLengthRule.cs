using Contracts.Domain.Interface;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates category name length does not exceed maximum allowed characters.
/// </summary>
public class CategoryNameLengthRule : IBusinessRule
{
    private readonly string _name;
    private readonly int _maxLength;

    public CategoryNameLengthRule(string name, int maxLength = 100)
    {
        _name = name;
        _maxLength = maxLength;
    }

    public bool IsBroken() => !string.IsNullOrEmpty(_name) && _name.Length > _maxLength;

    public string Message => $"Category name cannot exceed {_maxLength} characters.";

    public string Code => "Category.NameTooLong";
}
