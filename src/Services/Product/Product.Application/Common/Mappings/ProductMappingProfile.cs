using AutoMapper;
using Shared.DTOs.Product;
using Product.Domain.Entities;

namespace Product.Application.Common.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Domain.Entities.Product, ProductDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
                
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
                
            CreateMap<ProductVariantAttribute, ProductVariantAttributeDto>();
            
            CreateMap<AttributeDef, AttributeDefDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type));

            // Request mappings
            CreateMap<CreateProductRequest, Domain.Entities.Product>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProductStatus.Draft))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore());

            CreateMap<CreateProductVariantRequest, ProductVariant>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProductVariantStatus.Active))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore());

            CreateMap<CreateProductVariantAttributeRequest, ProductVariantAttribute>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariantId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariant, opt => opt.Ignore())
                .ForMember(dest => dest.AttributeDef, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore());

            CreateMap<UpdateProductRequest, Domain.Entities.Product>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore());
        }
    }
}