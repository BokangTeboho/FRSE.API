using FE.Core.Entities;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace FE.Infrastructure.Repositories
{
    public class TransactionRepository(FraudEngineDbContext db) : ITransactionRepository
    {
        public async Task Add(Transaction transaction, CancellationToken ct)
        {
            await db.Transactions.AddAsync(transaction, ct);
        }

        public async Task<IReadOnlyList<Transaction>> GetRecentByAccountNumber(
            string accountNumber, TimeSpan window, CancellationToken ct)
        {
            var cutoff = DateTimeOffset.UtcNow - window;

            return await db.Transactions
                .Where(t => t.AccountNumber == accountNumber)
                .Where(t => t.CreatedAt >= cutoff)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public Task<Transaction?> GetByReferenceAndAccount(string referenceId, string accountNumber, CancellationToken ct)
        {
            return db.Transactions
                .Where(t => t.ReferenceId == referenceId)
                .Where(t => t.AccountNumber == accountNumber)
                .FirstOrDefaultAsync(ct);
        }
    }
}
