using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FE.Infrastructure.Services
{
    public class WatchlistCacheOptions
    {
        public TimeSpan SlidingExpiration { get; init; }
        public TimeSpan NegativeCacheDuration { get; init; }
        public int SizeLimit { get; init; }
    }

    public class WatchlistService : IWatchlistService
    {
        private readonly FraudEngineDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly WatchlistCacheOptions _options;

        private const string MerchantPrefix = "watchlist:merchant:";
        private const string BeneficiaryPrefix = "watchlist:beneficiary:";

        public WatchlistService(
            FraudEngineDbContext db,
            IMemoryCache cache,
            IOptions<WatchlistCacheOptions> options)
        {
            _db = db;
            _cache = cache;
            _options = options.Value;
        }

        public async Task<WatchlistEntry?> CheckMerchant(string merchantId, CancellationToken ct)
        {
            return await Check(MerchantPrefix + merchantId, EntityType.Merchant, merchantId, ct);
        }

        public async Task<WatchlistEntry?> CheckBeneficiary(string beneficiaryAccountNumber, CancellationToken ct)
        {
            return await Check(BeneficiaryPrefix + beneficiaryAccountNumber, EntityType.Beneficiary, beneficiaryAccountNumber, ct);
        }

        public async Task Add(WatchlistEntry entry, CancellationToken ct)
        {
            _db.WatchlistEntries.Add(entry);

            var prefix = entry.EntityType == EntityType.Merchant
                ? MerchantPrefix
                : BeneficiaryPrefix;

            _cache.Set(prefix + entry.EntityIdentifier, entry, new MemoryCacheEntryOptions
            {
                SlidingExpiration = _options.SlidingExpiration,
                Size = 1
            });
        }

        private async Task<WatchlistEntry?> Check(
            string cacheKey, EntityType entityType, string identifier, CancellationToken ct)
        {
            if (_cache.TryGetValue(cacheKey, out WatchlistEntry? cached))
                return cached;

            var entry = await _db.WatchlistEntries
                .FirstOrDefaultAsync(w =>
                    w.EntityType == entityType
                    && w.EntityIdentifier == identifier
                    && w.IsActive, ct);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                Size = 1,
                SlidingExpiration = entry is not null
                    ? _options.SlidingExpiration
                    : _options.NegativeCacheDuration
            };

            _cache.Set(cacheKey, entry, cacheOptions);

            return entry;
        }
    }
}
