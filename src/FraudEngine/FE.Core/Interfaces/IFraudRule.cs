using FE.Core.Common;
using FE.Core.Entities;
using FE.Core.Enums;

namespace FE.Core.Interfaces
{
    public interface IFraudRule
    {
        string Name { get; }
        RuleApplicability Applicability { get; }
        Task<FraudRuleResult> Evaluate(Transaction transaction);
    }
}
