using System.ComponentModel;

namespace FE.Core.Enums
{
    /// <summary>
    /// Channel through which a payment transaction was initiated.
    /// </summary>
    [Description("Channel through which a payment transaction was initiated.")]
    public enum PaymentChannel
    {
        /// <summary>In-person card swipe, tap, or chip insert.</summary>
        CardPresent,

        /// <summary>E-commerce or card-not-present transaction.</summary>
        Online,

        /// <summary>Bank-to-bank or wire transfer.</summary>
        Transfer
    }
}
