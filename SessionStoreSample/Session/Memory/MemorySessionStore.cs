using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace SessionStoreSample.Session.Memory
{
    public class MemorySessionStore : ITicketStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemorySessionStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }


        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = Guid.NewGuid().ToString();
            var serializedTicket = TicketSerializer.Default.Serialize(ticket);

            _memoryCache.Set(key, serializedTicket);

            return Task.FromResult(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var serializedTicket = TicketSerializer.Default.Serialize(ticket);

            _memoryCache.Set(key, serializedTicket);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            if (_memoryCache.TryGetValue<byte[]>(key, out var serializedTicket))
            {
                return Task.FromResult(serializedTicket == null ? null : TicketSerializer.Default.Deserialize(serializedTicket));
            }

            return Task.FromResult<AuthenticationTicket?>(null);
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }

}
