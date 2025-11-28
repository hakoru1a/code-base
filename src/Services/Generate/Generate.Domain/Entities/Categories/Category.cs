using Contracts.Domain;
using Generate.Domain.Entities.Products;
using Contracts.Exceptions;

namespace Generate.Domain.Entities.Categories;

public class Category : EntityAuditBase<long>
{
    public string Name { get; private set; } = string.Empty;
    private readonly List<Product> _products = new();
    public virtual IReadOnlyList<Product> Products => _products.AsReadOnly();

    private Category() { }

    public Category(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public static Category Create(string name)
    {
        return new Category(name);
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public void AddProduct(Product product)
    {
        if (product == null)
            throw new BusinessException("Product cannot be null");

        if (_products.Any(p => p.Id == product.Id))
            throw new BusinessException("Product already exists in this category");

        _products.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        if (product == null)
            throw new BusinessException("Product cannot be null");

        _products.Remove(product);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Category name cannot be empty");

        if (name.Length > 100)
            throw new BusinessException("Category name cannot exceed 100 characters");

        if (name.Trim() != name)
            throw new BusinessException("Category name cannot have leading or trailing spaces");
    }

    public bool CanBeDeleted()
    {
        return !_products.Any();
    }

    public int GetProductCount()
    {
        return _products.Count;
    }
}
