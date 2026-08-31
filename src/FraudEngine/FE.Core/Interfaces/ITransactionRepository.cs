using FE.Core.Entities;

namespace FE.Core.Interfaces
{
    public interface ITransactionRepository
    {
        Task Add(Transaction transaction, CancellationToken ct);
        Task<IReadOnlyList<Transaction>> GetRecentByAccountNumber(string accountNumber, TimeSpan timeWindow, CancellationToken ct);
        Task<Transaction?> GetByReferenceAndAccount(string referenceId, string accountNumber, CancellationToken ct);
    }
}
