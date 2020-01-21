//----------------------------------------------------------------------------------------------------------
// <copyright file="PaymentMethodsTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.PaymentMethods.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class PaymentMethodsTests : BillingApiTestBase
    {
        private RestResult pmResult { get; set; }
        private string accountExternalId { get; set; }
        private List<PaymentMethod> paymentMethods { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;

            accountExternalId = BillingApiTestSettings.Default.BillingServiceApiAccountExternalId;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetInvoicesById_success()
        {
            request.RequestUri = $"v2/paymentmethods?accountId={accountExternalId}";
            pmResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pmResult.Success, $"failed to restclient get from billing service");
            paymentMethods = JsonSerializer.Deserialize<List<PaymentMethod>>(((RestResult<string>)pmResult).Value);
            Assert.IsTrue(paymentMethods?.Count == BillingApiTestSettings.Default.BillingServiceApiAccounPaymentMethodsCount, $"payment method count is not as expected - {pmResult}");
            Assert.IsTrue(paymentMethods[0].CreditCardType.Equals(BillingApiTestSettings.Default.BillingServiceApiAccounPaymentMethodsCreditCardType), $"payment method credit type is not as expected - {paymentMethods[0]}");
            Assert.IsTrue(paymentMethods[1].AchAccountType.Equals(BillingApiTestSettings.Default.BillingServiceApiAccounPaymentMethodsBankAccountType), $"payment method bank type is not as expected - {paymentMethods[1]}");
        }

        [TestMethod]
        public async Task GetInvoicesById_nullAccountId()
        {
            request.RequestUri = $"v2/paymentmethods?accountId=null";
            pmResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pmResult.Success, $"failed to restclient get from billing service");
            Assert.IsTrue(((RestResult<string>)pmResult).Value.Equals("[]"), $"payment method is not as expected - {pmResult}");
        }

        [TestMethod]
        public async Task GetInvoicesById_emptyAccountId()
        {
            request.RequestUri = $"v2/paymentmethods?accountId=";
            pmResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pmResult.Success, $"failed to restclient get from billing service");
            Assert.IsTrue(pmResult.Message.Contains(@"An error occurred (URL="), $"unexpected message - {pmResult.Message}");
        }

        [TestMethod]
        public async Task GetInvoicesById_invalidAccountId()
        {
            request.RequestUri = $"v2/paymentmethods?accountId=518518518518518";
            pmResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pmResult.Success, $"failed to restclient get from billing service");
            Assert.IsTrue(((RestResult<string>)pmResult).Value.Equals("[]"), $"payment method is not as expected - {pmResult}");
        }

        [TestMethod]
        public async Task GetInvoicesById_missingAccountId()
        {
            request.RequestUri = $"v2/paymentmethods?";
            pmResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pmResult.Success, $"successed with missing account id parameter");
            Assert.IsTrue(pmResult.Message.Contains("No HTTP resource was found that matches the request URI"), $"unexpected message - {pmResult.Message}");
        }

        [TestMethod]
        public async Task GetInvoicesById_extralParameter()
        {
            request.RequestUri = $"v2/paymentmethods?accountId={accountExternalId}/banktype=Saving";
            pmResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pmResult.Success, $"failed to restclient get from billing service");
            Assert.IsTrue(((RestResult<string>)pmResult).Value.Equals("[]"), $"payment method is not as expected - {pmResult}");
        }

    }
}