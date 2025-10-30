using Base.Application.Common.Models.Product;
using MediatR;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Feature.Product.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
    {
        public PagedRequestParameter Parameters { get; set; } = new PagedRequestParameter();
        
        /// <summary>
        /// Maximum price filter (for PBAC - basic users can only see products under this price)
        /// </summary>
        public decimal? MaxPrice { get; set; }
    }
}
