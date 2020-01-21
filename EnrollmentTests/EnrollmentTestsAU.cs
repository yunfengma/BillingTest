//----------------------------------------------------------------------------------------------------------
// <copyright file="EnrollmentTestsAU.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.EnrollmentTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Trupanion.Billing.Test.DataVerifiers;


    [TestClass]
    public class EnrollmentTestsAU : BillingTestBase
    {
        private BillingDataVerifiers billingDataVerifiers { get; set; }
        private BillingRestClient billingRestClient { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            billingDataVerifiers = new BillingDataVerifiers();
            billingRestClient = new BillingRestClient();

            accountExpected.Currency = "AUD";
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

    }
}
