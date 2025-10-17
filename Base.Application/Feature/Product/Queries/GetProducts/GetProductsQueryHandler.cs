using AutoMapper;
using Base.Application.Common.Models;
using Base.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Feature.Product.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.FindAll().ToListAsync(cancellationToken);
            return _mapper.Map<List<ProductDto>>(products);
        }
    }
}
