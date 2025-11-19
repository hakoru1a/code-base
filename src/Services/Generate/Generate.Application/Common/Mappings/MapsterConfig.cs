using Mapster;
using Shared.DTOs.Category;
using Shared.DTOs.Order;
using Shared.DTOs.Product;
using Generate.Domain.Entities;

namespace Generate.Application.Common.Mappings;

/// <summary>
/// Mapster configuration for Generate service mappings
/// High-performance alternative to AutoMapper
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
        ConfigureOrderMappings();
        ConfigureProductMappings();
    }

    /// <summary>
    /// Configure Category entity mappings
    /// </summary>
    private static void ConfigureCategoryMappings()
    {
        // Category -> CategoryResponseDto (explicit mapping for clarity)
        TypeAdapterConfig<Category, CategoryResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // CategoryCreateDto -> Category
        TypeAdapterConfig<CategoryCreateDto, Category>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Ignore(dest => dest.Id); // Id is auto-generated

        // CategoryUpdateDto -> Category
        TypeAdapterConfig<CategoryUpdateDto, Category>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);
    }

    /// <summary>
    /// Configure Order entity mappings
    /// </summary>
    private static void ConfigureOrderMappings()
    {
        // Order -> OrderResponseDto
        TypeAdapterConfig<Order, OrderResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CustomerName, src => src.CustomerName)
            .Map(dest => dest.OrderItems, src => src.OrderItems);

        // OrderCreateDto -> Order
        TypeAdapterConfig<OrderCreateDto, Order>
            .NewConfig()
            .Map(dest => dest.CustomerName, src => src.CustomerName)
            .Map(dest => dest.OrderItems, src => src.OrderItems)
            .Ignore(dest => dest.Id);

        // OrderUpdateDto -> Order
        TypeAdapterConfig<OrderUpdateDto, Order>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CustomerName, src => src.CustomerName)
            .Map(dest => dest.OrderItems, src => src.OrderItems);

        // OrderItem mappings
        TypeAdapterConfig<OrderItem, OrderItemResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.ProductName, src => src.Product.Name);

        TypeAdapterConfig<OrderItemCreateDto, OrderItem>
            .NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Product);

        TypeAdapterConfig<OrderItemUpdateDto, OrderItem>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Ignore(dest => dest.Product);
    }

    /// <summary>
    /// Configure Product entity mappings
    /// </summary>
    private static void ConfigureProductMappings()
    {
        // Product -> ProductResponseDto
        TypeAdapterConfig<Product, ProductResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null);

        // ProductCreateDto -> Product
        TypeAdapterConfig<ProductCreateDto, Product>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Category);

        // ProductUpdateDto -> Product
        TypeAdapterConfig<ProductUpdateDto, Product>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Ignore(dest => dest.Category);
    }
}