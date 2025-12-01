using Contracts.Domain.Interface;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates category name format (no leading or trailing spaces).
/// </summary>
public class CategoryNameFormatRule : IBusinessRule
{
    private readonly string _name;

    public CategoryNameFormatRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => !string.IsNullOrEmpty(_name) && _name.Trim() != _name;

    public string Message => "Category name cannot have leading or trailing spaces.";

    public string Code => "Category.NameInvalidFormat";
}
