using FE.Core.Entities;
using FE.Core.Enums;

namespace FE.Core.Interfaces
{
    public interface IFraudAlertRepository
    {
        Task Add(FraudAlert fraudAlert, CancellationToken ct);

        Task<(IReadOnlyList<FraudAlert> Items, int TotalCount)> Search(
            List<Guid>? transactionIds,
            List<Severity>? severities,
            List<string>? ruleNames,
            int page,
            int pageSize,
            CancellationToken ct);
    }
}
