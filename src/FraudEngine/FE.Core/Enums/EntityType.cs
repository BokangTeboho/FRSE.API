using System.ComponentModel;

namespace FE.Core.Enums
{
    /// <summary>
    /// Type of entity on the fraud watchlist.
    /// </summary>
    [Description("Type of entity on the fraud watchlist.")]
    public enum EntityType
    {
        /// <summary>A merchant or seller.</summary>
        Merchant,

        /// <summary>A payment beneficiary or recipient.</summary>
        Beneficiary
    }
}
