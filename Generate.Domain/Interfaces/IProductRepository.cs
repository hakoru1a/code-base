using Contracts.Common.Interface;
using Generate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generate.Domain.Interfaces
{
    public interface IProductRepository : IRepositoryBaseAsync<Product, long>
    {
    }
}
