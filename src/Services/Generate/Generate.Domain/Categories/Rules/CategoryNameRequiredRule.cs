using Contracts.Domain.Interface;

namespace Generate.Domain.Categories.Rules;

/// <summary>
/// Business rule that validates category name is required (not null, empty, or whitespace).
/// </summary>
public class CategoryNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public CategoryNameRequiredRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Category name cannot be empty.";

    public string Code => "Category.NameRequired";
}
