using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;

namespace FE.Core.Interfaces
{
    public interface IFraudRule
    {
        string Name { get; }
        IReadOnlySet<PaymentChannel> ApplicableChannels { get; }
        FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot);
    }
}
