using Base.Domain.Entities;
using Constracts.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Domain.Interfaces
{
    public interface IProductRepository : IRepositoryBaseAsync<Product, long>
    {

    }
}
