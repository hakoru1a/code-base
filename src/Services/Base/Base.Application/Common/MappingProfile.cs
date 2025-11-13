using AutoMapper;
using Shared.DTOs.Product;
using Base.Domain.Entities;
using Infrastructure.Mapping;

namespace Base.Application.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .IgnoreAllNonExisting();

        CreateMap<ProductDto, Product>()
            .IgnoreAllNonExisting();
    }
}


