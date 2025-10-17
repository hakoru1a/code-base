using Contracts.Common.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastucture.Common.Repository
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisRepository(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _database = _redis.GetDatabase();
        }

        #region String Operations

        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _database.StringSetAsync(key, value, expiry);
        }

        public async Task<string?> GetStringAsync(string key)
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        #endregion

        #region Object Operations

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            return await _database.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            catch
            {
                return default;
            }
        }

        #endregion

        #region Key Operations

        public async Task<bool> DeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return await _database.KeyExpireAsync(key, expiry);
        }

        public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
        {
            return await _database.KeyTimeToLiveAsync(key);
        }

        public async Task<bool> PersistAsync(string key)
        {
            return await _database.KeyPersistAsync(key);
        }

        #endregion

        #region Hash Operations

        public async Task<bool> HashSetAsync(string key, string field, string value)
        {
            return await _database.HashSetAsync(key, field, value);
        }

        public async Task<string?> HashGetAsync(string key, string field)
        {
            var value = await _database.HashGetAsync(key, field);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
        {
            var hashEntries = await _database.HashGetAllAsync(key);
            return hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => entry.Value.ToString()
            );
        }

        public async Task<bool> HashDeleteAsync(string key, string field)
        {
            return await _database.HashDeleteAsync(key, field);
        }

        public async Task<bool> HashExistsAsync(string key, string field)
        {
            return await _database.HashExistsAsync(key, field);
        }

        #endregion

        #region List Operations

        public async Task<long> ListLeftPushAsync(string key, string value)
        {
            return await _database.ListLeftPushAsync(key, value);
        }

        public async Task<long> ListRightPushAsync(string key, string value)
        {
            return await _database.ListRightPushAsync(key, value);
        }

        public async Task<string?> ListLeftPopAsync(string key)
        {
            var value = await _database.ListLeftPopAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<string?> ListRightPopAsync(string key)
        {
            var value = await _database.ListRightPopAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<long> ListLengthAsync(string key)
        {
            return await _database.ListLengthAsync(key);
        }

        public async Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1)
        {
            var values = await _database.ListRangeAsync(key, start, stop);
            return values.Select(v => v.ToString()).ToList();
        }

        #endregion

        #region Set Operations

        public async Task<bool> SetAddAsync(string key, string value)
        {
            return await _database.SetAddAsync(key, value);
        }

        public async Task<bool> SetRemoveAsync(string key, string value)
        {
            return await _database.SetRemoveAsync(key, value);
        }

        public async Task<bool> SetContainsAsync(string key, string value)
        {
            return await _database.SetContainsAsync(key, value);
        }

        public async Task<List<string>> SetMembersAsync(string key)
        {
            var members = await _database.SetMembersAsync(key);
            return members.Select(m => m.ToString()).ToList();
        }

        public async Task<long> SetLengthAsync(string key)
        {
            return await _database.SetLengthAsync(key);
        }

        #endregion

        #region Batch Operations

        public async Task<long> DeleteMultipleAsync(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            return await _database.KeyDeleteAsync(redisKeys);
        }

        public async Task<List<string>> GetKeysAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            return await Task.FromResult(keys.Select(k => k.ToString()).ToList());
        }

        #endregion
    }
}

