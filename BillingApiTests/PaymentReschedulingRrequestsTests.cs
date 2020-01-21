//----------------------------------------------------------------------------------------------------------
// <copyright file="PaymentReschedulingRrequestsTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Payments.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class PaymentReschedulingRrequestsTests : BillingApiTestBase
    {
        private RestResult prrResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetPaymentReschedulingRrequests_success()
        {
            request.RequestUri = $"v2/paymentreschedulingrequests?criteria.accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}";
            prrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(prrResult.Success, $"failed to restclient get from billing service");
            List<PaymentReschedulingRequest> pprs = JsonSerializer.Deserialize<List<PaymentReschedulingRequest>>(((RestResult<string>)prrResult).Value);
            PaymentReschedulingRequest ppr = pprs.Where(p => p.Id == BillingApiTestSettings.Default.BillingServiceApiAccountPaymentReschedulingRrequestsId).FirstOrDefault();
            Assert.IsNotNull(ppr, $"payment rescheduling requests is not as expected - {pprs}");
        }

        [TestMethod]
        public async Task GetPaymentReschedulingRrequests_nullAccountId()
        {
            request.RequestUri = $"v2/paymentreschedulingrequests?criteria.accountId=null";
            prrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(prrResult.Success, $"successed get payments with empty account id");
            Assert.IsTrue(prrResult.Message.Contains(@"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected error message - {prrResult.Message}");
        }

        [TestMethod]
        public async Task GetPaymentReschedulingRrequests_emptyAccountId()
        {
            request.RequestUri = $"v2/paymentreschedulingrequests?criteria.accountId";
            prrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(prrResult.Success, $"successed get payments with empty account id");
            Assert.IsTrue(prrResult.Message.Contains(@"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected error message - {prrResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetPaymentReschedulingRrequests_invalidAccountId()
        {
            request.RequestUri = $"v2/paymentreschedulingrequests?criteria.accountId=invalidaccountid";
            prrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(prrResult.Success, $"successed unexpectedly");
            Assert.IsTrue(prrResult.Message.Contains(@"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected error message - {prrResult.Message}");
        }

        [TestMethod]
        public async Task GetPaymentReschedulingRrequests_missingAccountId()
        {
            request.RequestUri = $"v2/paymentreschedulingrequests?";
            prrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(prrResult.Success, $"successed unexpectedly");
            Assert.IsTrue(prrResult.Message.Contains("Object reference not set to an instance of an object."), $"unexpected message - {prrResult.Message}");
        }

        [TestMethod]
        public async Task GetPaymentReschedulingRrequests_extraParameter()
        {
            request.RequestUri = $"v2/paymentreschedulingrequests?criteria.accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/date=15//18";
            prrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(prrResult.Success, $"successed unexpectedly");
            Assert.IsTrue(prrResult.Message.Contains("Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected message - {prrResult.Message}");
        }


    }
}
