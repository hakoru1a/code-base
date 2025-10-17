using Base.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Feature.Product.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
