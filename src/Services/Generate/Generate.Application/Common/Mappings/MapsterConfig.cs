using Mapster;
using Shared.DTOs.Category;
using Shared.DTOs.Order;
using Shared.DTOs.Product;
using Generate.Domain.Categories;
using Generate.Domain.Orders;
using Generate.Domain.Products;

namespace Generate.Application.Common.Mappings;

/// <summary>
/// Optimized Mapster configuration for Generate service
/// Follows DDD principles with clean mapping logic
/// </summary>
public static class MapsterConfig
{
    /// <summary>
    /// Configure all mappings for the Generate service
    /// </summary>
    public static void ConfigureMappings()
    {
        // Configure global settings for better performance
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        ConfigureCategoryMappings();
        ConfigureProductMappings();
        ConfigureOrderMappings();
        ConfigureProductDetailMappings();
    }

    /// <summary>
    /// Configure Category entity mappings - Optimized
    /// </summary>
    private static void ConfigureCategoryMappings()
    {
        // Category -> CategoryResponseDto
        TypeAdapterConfig<Category, CategoryResponseDto>
            .NewConfig()
            .Map(dest => dest.ProductCount, src => src.GetProductCount())
            .Map(dest => dest.CanBeDeleted, src => src.CanBeDeleted());

        // CategoryCreateDto -> Category (factory method)
        TypeAdapterConfig<CategoryCreateDto, Category>
            .NewConfig()
            .ConstructUsing(src => Category.Create(src.Name));
    }

    /// <summary>
    /// Configure Product entity mappings - Optimized
    /// </summary>
    private static void ConfigureProductMappings()
    {
        // Product -> ProductDto (full DTO with business logic)
        TypeAdapterConfig<Product, ProductDto>
            .NewConfig()
            .Map(dest => dest.CategoryId, src => src.Category != null ? src.Category.Id : (long?)null)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.IsInCategory, src => src.IsInCategory())
            .Map(dest => dest.OrderItemsCount, src => src.GetOrderItemsCount())
            .Map(dest => dest.TotalOrderedQuantity, src => src.GetTotalOrderedQuantity())
            .Map(dest => dest.CanBeDeleted, src => src.CanBeDeleted());

        // Product -> ProductResponseDto (simplified version)
        TypeAdapterConfig<Product, ProductResponseDto>
            .NewConfig()
            .Map(dest => dest.CategoryId, src => src.Category != null ? src.Category.Id : (long?)null)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.ProductDetailSummary, src => src.ProductDetail != null ? src.ProductDetail.GetSummary(100) : null)
            .Map(dest => dest.IsInCategory, src => src.IsInCategory())
            .Map(dest => dest.CanBeDeleted, src => src.CanBeDeleted());

        // ProductCreateDto -> Product (factory method)
        TypeAdapterConfig<ProductCreateDto, Product>
            .NewConfig()
            .ConstructUsing(src => Product.Create(
                src.Name,
                null, // Category will be set in CommandHandler with proper entity
                src.ProductDetail != null ? ProductDetail.Create(src.ProductDetail.Description) : null
            ));
    }

    /// <summary>
    /// Configure ProductDetail entity mappings - Optimized
    /// </summary>
    private static void ConfigureProductDetailMappings()
    {
        // ProductDetail -> ProductDetailDto
        TypeAdapterConfig<ProductDetail, ProductDetailDto>
            .NewConfig()
            .Map(dest => dest.DescriptionSummary, src => src.GetSummary(100))
            .Map(dest => dest.HasDescription, src => src.HasDescription());

        // ProductDetailDto -> ProductDetail (factory method)
        TypeAdapterConfig<ProductDetailDto, ProductDetail>
            .NewConfig()
            .ConstructUsing(src => ProductDetail.Create(src.Description));
    }

    /// <summary>
    /// Configure Order entity mappings - Optimized
    /// </summary>
    private static void ConfigureOrderMappings()
    {
        // Order -> OrderResponseDto
        TypeAdapterConfig<Order, OrderResponseDto>
            .NewConfig()
            .Map(dest => dest.HasOrderItems, src => src.HasOrderItems())
            .Map(dest => dest.TotalItemsCount, src => src.GetTotalItemsCount())
            .Map(dest => dest.UniqueProductsCount, src => src.GetUniqueProductsCount())
            .Map(dest => dest.IsLargeOrder, src => src.IsLargeOrder(50))
            .Map(dest => dest.TotalOrderValue, src => src.GetTotalOrderValue())
            .Map(dest => dest.CanBeDeleted, src => src.CanBeDeleted());

        // OrderCreateDto -> Order (factory method)
        TypeAdapterConfig<OrderCreateDto, Order>
            .NewConfig()
            .ConstructUsing(src => Order.Create(src.CustomerName));

        // OrderItem -> OrderItemResponseDto (handle composite key via navigation properties)
        TypeAdapterConfig<OrderItem, OrderItemResponseDto>
            .NewConfig()
            .Map(dest => dest.OrderId, src => src.Order != null ? src.Order.Id : 0)
            .Map(dest => dest.ProductId, src => src.Product != null ? src.Product.Id : 0)
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.IsLargeOrder, src => src.IsLargeOrder(100))
            .Map(dest => dest.TotalValue, src => src.GetTotalValue());
    }
}