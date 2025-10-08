using Base.Domain.Interfaces;
using Infrastucture.Common.Repository;
using StackExchange.Redis;

namespace Base.Infrastructure.Repositories
{
    public class ProductRedisRepository : RedisRepository, IProductRedisRepository
    {
        public ProductRedisRepository(IConnectionMultiplexer redis) : base(redis)
        {
        }

    }
}
