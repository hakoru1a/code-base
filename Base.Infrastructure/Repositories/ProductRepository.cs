using Base.Domain.Entities;
using Base.Domain.Interfaces;
using Base.Infrastructure.Persistence;
using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBaseAsync<Product, long, BaseContext>, IProductRepository
    {
        public ProductRepository(BaseContext dbContext, IUnitOfWork<BaseContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

    }
}
