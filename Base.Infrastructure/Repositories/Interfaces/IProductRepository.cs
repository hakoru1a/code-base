using Base.Domain.Entities;
using Contracts.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Interfaces
{
    public interface IProductRepository : IRepositoryBaseAsync<Product, long>
    {

    }
}
