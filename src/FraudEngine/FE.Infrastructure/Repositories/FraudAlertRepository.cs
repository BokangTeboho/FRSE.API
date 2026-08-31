using FE.Core.Entities;
using FE.Core.Enums;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FE.Infrastructure.Repositories
{
    public class FraudAlertRepository(FraudEngineDbContext db) : IFraudAlertRepository
    {
        public async Task<IReadOnlyList<FraudAlert>> GetByTransactionId(Guid id, CancellationToken ct)
        {
            return await db.FraudAlerts
                .Where(a => a.TransactionId == id)
                .ToListAsync(ct);
        }

        public async Task Add(FraudAlert alert, CancellationToken ct)
        {
            await db.FraudAlerts.AddAsync(alert, ct);
        }

        public async Task<(IReadOnlyList<FraudAlert> Items, int TotalCount)> Search(
            List<Guid>? transactionIds,
            List<Severity>? severities,
            List<string>? ruleNames,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var query = db.FraudAlerts.AsQueryable();

            if (transactionIds is { Count: > 0 })
                query = query.Where(a => transactionIds.Contains(a.TransactionId));

            if (severities is { Count: > 0 })
                query = query.Where(a => severities.Contains(a.Severity));

            if (ruleNames is { Count: > 0 })
                query = query.Where(a => ruleNames.Contains(a.RuleName));

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
