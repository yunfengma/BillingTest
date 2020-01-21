//----------------------------------------------------------------------------------------------------------
// <copyright file="PaymentsTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Payments.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class PaymentsTests : BillingApiTestBase
    {
        private RestResult pResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetPayments_success()
        {
            request.RequestUri = $"v2/payments?accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}";
            pResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pResult.Success, $"failed to restclient get from billing service");
            List<Payment> payments = JsonSerializer.Deserialize<List<Payment>>(((RestResult<string>)pResult).Value);
            Assert.IsTrue(payments?.Count >= 0, $"payments are not as expected - {payments}");
        }

        [TestMethod]
        public async Task GetPayments_nullAccountId()
        {
            request.RequestUri = $"v2/payments?accountId=null";
            pResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pResult.Success, $"successed get payments with empty account id");
            Assert.IsTrue(((RestResult<string>)pResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)pResult).Value}");
        }

        [TestMethod]
        public async Task GetPayments_emptyAccountId()
        {
            request.RequestUri = $"v2/payments?accountId=";
            pResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pResult.Success, $"successed get payments with empty account id");
            Assert.IsTrue(pResult.Message.Contains(@"An error occurred (URL="), $"unexpected error message - {pResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetPayments_invalidAccountId()
        {
            request.RequestUri = $"v2/payments?accountId=invalidaccountid";
            pResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pResult.Success, $"failed to restclient get from billing service");
            Assert.IsTrue(((RestResult<string>)pResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)pResult).Value}");
        }

        [TestMethod]
        public async Task GetPayments_missingCriteria()
        {
            request.RequestUri = $"v2/payments?";
            pResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pResult.Success, $"successed unexpectedly");
            Assert.IsTrue(pResult.Message.Contains("No HTTP resource was found that matches the request URI"), $"unexpected message - {pResult.Message}");
        }

        [TestMethod]
        public async Task GetPayments_extraParameter()
        {
            request.RequestUri = $"v2/payments?accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/paymentid=12345";
            pResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pResult.Success, $"failed to restclient get from billing services");
            Assert.IsTrue(((RestResult<string>)pResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)pResult).Value}");
        }

    }
}
