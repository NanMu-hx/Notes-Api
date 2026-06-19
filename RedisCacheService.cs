using StackExchange.Redis;
using System.Text.Json;

namespace first_net8._0
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T data, TimeSpan? expire = null);
        Task DeleteAsync(string key);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _redisDb;
        private readonly TimeSpan _defaultExpire = TimeSpan.FromMinutes(5);

        public RedisCacheService(IConnectionMultiplexer redisMultiplexer)
        {
            _redisDb = redisMultiplexer.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var val = await _redisDb.StringGetAsync(key);
            if (val.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(val!);
        }

        public async Task SetAsync<T>(string key, T data, TimeSpan? expire = null)
        {
            string json = JsonSerializer.Serialize(data);
            await _redisDb.StringSetAsync(key, json, expire ?? _defaultExpire);
        }

        public async Task DeleteAsync(string key)
        {
            await _redisDb.KeyDeleteAsync(key);
        }
    }
}