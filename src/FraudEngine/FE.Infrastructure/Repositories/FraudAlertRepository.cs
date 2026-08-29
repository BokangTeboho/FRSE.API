using FE.Core.Entities;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;

namespace FE.Infrastructure.Repositories
{
    public class FraudAlertRepository(FraudEngineDbContext db) : IFraudAlertRepository
    {
        public async Task Add(FraudAlert alert, CancellationToken ct)
        {
            await db.FraudAlerts.AddAsync(alert, ct);
        }
    }
}
