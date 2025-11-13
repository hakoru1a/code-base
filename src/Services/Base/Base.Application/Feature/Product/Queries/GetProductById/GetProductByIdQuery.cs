using Shared.DTOs.Product;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Feature.Product.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductDto?>
    {
        public long Id { get; set; }
    }
}
