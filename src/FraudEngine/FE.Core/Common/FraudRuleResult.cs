using FE.Core.Enums;

namespace FE.Core.Common
{
    public record FraudRuleResult(
        bool IsTriggered,
        string RuleName,
        Severity Severity,
        string Description
    );
}
