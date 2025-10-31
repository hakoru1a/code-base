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
    internal class OrderRepository : RepositoryBaseAsync<Order, long, GenerateContext>, IOrderRepository
    {
        public OrderRepository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }
    }
}
