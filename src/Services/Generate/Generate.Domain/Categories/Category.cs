using Contracts.Domain;
using Generate.Domain.Products;
using Generate.Domain.Categories.Rules;
using Contracts.Exceptions;
using Contracts.Domain.Interface;
using Contracts.Domain.Rules;

namespace Generate.Domain.Categories;

public class Category : EntityAuditBase<long>
{
    public string Name { get; private set; } = string.Empty;
    private readonly List<Product> _products = new();
    public virtual IReadOnlyList<Product> Products => _products.AsReadOnly();

    private Category() { }

    public Category(string name)
    {
        Name = name;
    }

    public static Category Create(string name)
    {
        return new Category(name);
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void AddProduct(Product product)
    {
        CheckRule(new CategoryProductRequiredRule(product)
            .And(new CategoryProductNotExistsRule(_products, product))
            .And(new CategoryMaxProductsLimitRule(_products)));

        _products.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        CheckRule(new CategoryProductRequiredRule(product)
            .And(new CategoryProductExistsRule(_products, product)));

        var productToRemove = _products.FirstOrDefault(p => p.Id == product.Id);
        if (productToRemove != null)
        {
            _products.Remove(productToRemove);
        }
    }


    // Business rules and queries - sử dụng Business Rules và Specifications
    public bool CanBeDeleted()
    {
        var rule = new CategoryCanBeDeletedRule(_products);
        return !rule.IsBroken();
    }

    public int GetProductCount()
    {
        return _products.Count;
    }

    public bool HasProducts()
    {
        return _products.Any();
    }
}

