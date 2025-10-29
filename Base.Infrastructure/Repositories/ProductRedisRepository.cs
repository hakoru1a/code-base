using Base.Infrastructure.Interfaces;
using Infrastructure.Common.Repository;
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
