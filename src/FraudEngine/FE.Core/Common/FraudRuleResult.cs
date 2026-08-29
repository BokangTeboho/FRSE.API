using FE.Core.Enums;

namespace FE.Core.Common
{
    public class FraudRuleResult
    {
        public bool IsTriggered { get; init; }
        public string? RuleName { get; init; }
        public Severity? Severity { get; init; }
        public string? Description { get; init; }

        public static FraudRuleResult Clean() => new()
        {
            IsTriggered = false
        };

        public static FraudRuleResult Triggered(string ruleName, Severity severity, string description) => new()
        {
            IsTriggered = true,
            RuleName = ruleName,
            Severity = severity,
            Description = description
        };
    }
}
