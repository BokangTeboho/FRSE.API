using System.ComponentModel;

namespace FE.Core.Enums
{
    /// <summary>
    /// Risk severity level assigned to fraud alerts and watchlist entries.
    /// </summary>
    [Description("Risk severity level assigned to fraud alerts and watchlist entries.")]
    public enum Severity
    {
        /// <summary>Minimal risk, informational only.</summary>
        Low,

        /// <summary>Moderate risk, may require review.</summary>
        Medium,

        /// <summary>Significant risk, requires prompt attention.</summary>
        High,

        /// <summary>Highest risk, requires immediate action.</summary>
        Critical
    }
}
