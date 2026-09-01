using System.ComponentModel;

namespace FE.Core.Enums
{
    /// <summary>
    /// Settlement timing for a payment transaction.
    /// </summary>
    [Description("Settlement timing for a payment transaction.")]
    public enum PaymentTiming
    {
        /// <summary>Funds are settled in real time.</summary>
        Immediate,

        /// <summary>Funds are settled in a standard batch cycle.</summary>
        Standard
    }
}
