//----------------------------------------------------------------------------------------------------------
// <copyright file="BillingPaymentMethodTypes.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.Models
{
    using System.ComponentModel;

    public enum BillingPaymentMethodTypes
    {
        [Description("Credit Card")]
        CreditCard = 0,
        [Description("ACH")]
        ACH,
        [Description("Other")]
        Other,
        [Description("Bank Transfer")]
        BankTransfer,
        [Description("Check")]
        Check
    }
}
