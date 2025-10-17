using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts.Common.Interface
{
    public interface IRedisRepository
    {
        // String Operations
        Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetStringAsync(string key);
        Task<bool> DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);

        // Object Operations
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);

        // Hash Operations
        Task<bool> HashSetAsync(string key, string field, string value);
        Task<string?> HashGetAsync(string key, string field);
        Task<Dictionary<string, string>> HashGetAllAsync(string key);
        Task<bool> HashDeleteAsync(string key, string field);
        Task<bool> HashExistsAsync(string key, string field);

        // List Operations
        Task<long> ListLeftPushAsync(string key, string value);
        Task<long> ListRightPushAsync(string key, string value);
        Task<string?> ListLeftPopAsync(string key);
        Task<string?> ListRightPopAsync(string key);
        Task<long> ListLengthAsync(string key);
        Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1);

        // Set Operations
        Task<bool> SetAddAsync(string key, string value);
        Task<bool> SetRemoveAsync(string key, string value);
        Task<bool> SetContainsAsync(string key, string value);
        Task<List<string>> SetMembersAsync(string key);
        Task<long> SetLengthAsync(string key);

        // Key Operations
        Task<bool> ExpireAsync(string key, TimeSpan expiry);
        Task<TimeSpan?> GetTimeToLiveAsync(string key);
        Task<bool> PersistAsync(string key);

        // Batch Operations
        Task<long> DeleteMultipleAsync(IEnumerable<string> keys);
        Task<List<string>> GetKeysAsync(string pattern);
    }
}

