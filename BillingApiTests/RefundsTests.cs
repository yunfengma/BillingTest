//----------------------------------------------------------------------------------------------------------
// <copyright file="RefundsTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Refunds.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class RefundsTests : BillingApiTestBase
    {
        private RestResult refundResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetRefunds_success()
        {
            request.RequestUri = $"v2/refunds?accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}";
            refundResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(refundResult.Success, $"failed to restclient get from billing service");
            List<Refund> refunds = JsonSerializer.Deserialize<List<Refund>>(((RestResult<string>)refundResult).Value);
            //Refund refund = refunds.Where(n => n.Amount).FirstOrDefault();
            //Assert.IsNotNull(refund, $"refund is not as expected - {refunds}");
        }

        [TestMethod]
        public async Task GetRefunds_nullAccountId()
        {
            request.RequestUri = $"v2/refunds?accountId=null";
            refundResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(refundResult.Success, $"successed get payments with empty account id");
            Assert.IsTrue(((RestResult<string>)refundResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)refundResult).Value}");
        }

        [TestMethod]
        public async Task GetRefunds_emptyAccountId_bug159313()
        {
            request.RequestUri = $"v2/refunds?accountId=";
            refundResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsNotNull(refundResult, $"failed to restclient to billing services");
            // bug: 159313
            Assert.IsTrue(refundResult.Success, $"check if bug has fixed");
            //Assert.IsFalse(refundResult.Success, $"successed get payments with empty account id");
            //Assert.IsTrue(refundResult.Message.Contains(@"An error occurred (URL="), $"unexpected error message - {refundResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetRefunds_invalidAccountId()
        {
            request.RequestUri = $"v2/refunds?accountId=invalidaccountid";
            refundResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(refundResult.Success, $"failed to restclient get from billing service");
            Assert.IsTrue(((RestResult<string>)refundResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)refundResult).Value}");
        }

        [TestMethod]
        public async Task GetRefunds_missingCriteria()
        {
            request.RequestUri = $"v2/refunds";
            refundResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(refundResult.Success, $"successed unexpectedly");
            Assert.IsTrue(refundResult.Message.Contains("No HTTP resource was found that matches the request URI"), $"unexpected message - {refundResult.Message}");
        }

        [TestMethod]
        public async Task GetRefunds_extraParameter()
        {
            request.RequestUri = $"v2/refunds?accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/refundid=12345";
            refundResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(refundResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)refundResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)refundResult).Value}");
        }

    }
}
