using Contracts.Common.Interface;
using Generate.Domain.Entities;
using Generate.Infrastructure.Interfaces;
using Generate.Infrastructure.Persistences;
using Infrastructure.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generate.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBaseAsync<Product, long, GenerateContext>, IProductRepository
    {
        public ProductRepository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }
    }
}
