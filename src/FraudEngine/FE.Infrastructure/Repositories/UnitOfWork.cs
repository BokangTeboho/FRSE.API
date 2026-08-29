using FE.Core.Interfaces;
using FE.Infrastructure.Context;

namespace FE.Infrastructure.Repositories
{
    public class UnitOfWork(FraudEngineDbContext db) : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken ct)
        {
            return db.SaveChangesAsync(ct);
        }
    }
}