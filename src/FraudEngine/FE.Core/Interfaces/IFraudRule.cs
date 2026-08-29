using FE.Core.Common;
using FE.Core.Entities;

namespace FE.Core.Interfaces
{
    public interface IFraudRule
    {
        string Name { get; }
        FraudRuleResult Evaluate(Transaction transaction, ScanSnapshot snapshot);
    }
}
