using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace SessionStoreSample.Session.DistributedCache
{
    public class DistributedCacheSessionStore : ITicketStore
    {
        private readonly IDistributedCache _cache;
        private readonly SessionSetting _setting;

        public DistributedCacheSessionStore(IDistributedCache cache, SessionSetting setting)
        {
            _cache = cache;
            _setting = setting;
        } 

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var serializedTicket = TicketSerializer.Default.Serialize(ticket);
            await _cache.SetAsync(key, serializedTicket, new DistributedCacheEntryOptions() { SlidingExpiration = _setting.ExpireTime });
        }

        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            var serializedTicket = await _cache.GetAsync(key);
            return serializedTicket != null ? TicketSerializer.Default.Deserialize(serializedTicket) : null;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = Guid.NewGuid().ToString();
            await RenewAsync(key, ticket);
            return key;
        }
    }
}
