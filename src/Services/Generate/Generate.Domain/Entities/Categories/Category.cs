using Contracts.Domain;
using Generate.Domain.Entities.Products;
using Generate.Domain.Entities.Categories.Rules;
using Generate.Domain.Entities.Categories.Specifications;
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
        CategoryValidationRules.CategoryName.ValidateCategoryName(name);
        Name = name;
    }

    public static Category Create(string name)
    {
        return new Category(name);
    }

    public void UpdateName(string name)
    {
        CategoryValidationRules.CategoryName.ValidateCategoryName(name);
        Name = name;
    }

    public void AddProduct(Product product)
    {
        CategoryBusinessRules.ProductManagement.AddProduct(_products, product);
    }

    public void RemoveProduct(Product product)
    {
        CategoryBusinessRules.ProductManagement.RemoveProduct(_products, product);
    }


    // Business rules and queries - sử dụng Business Rules và Specifications
    public bool CanBeDeleted()
    {
        return CategoryBusinessRules.Lifecycle.CanBeDeleted(_products);
    }

    public int GetProductCount()
    {
        return CategoryBusinessRules.Analytics.CalculateProductCount(_products);
    }

    public bool HasProducts()
    {
        return CategoryBusinessRules.Lifecycle.HasProducts(_products);
    }

    // Sử dụng Specifications cho business queries phức tạp
    public bool SatisfiesSpecification(CategorySpecifications.ICategorySpecification specification)
    {
        return specification.IsSatisfiedBy(this);
    }
}
