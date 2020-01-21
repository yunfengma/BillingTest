//----------------------------------------------------------------------------------------------------------
// <copyright file="InvoicesIdTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class InvoicesIdTests : BillingApiTestBase
    {
        private RestResult invoicesResult { get; set; }
        private string accountExternalId { get; set; }
        private string invoiceId { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;

            accountExternalId = BillingApiTestSettings.Default.BillingServiceApiAccountExternalId;
            invoiceId = BillingApiTestSettings.Default.BillingServiceApiAccountInvoiceId;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetInvoicesById_success()
        {
            request.RequestUri = $"v2/invoices/{invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"failed to restclient get from billing service");
            Invoice invoice = JsonSerializer.Deserialize<Invoice>(((RestResult<string>)invoicesResult).Value);
            Assert.IsTrue(invoice.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountInoiceAmount, $"invoice is not as expected - {invoice}");
        }

        [TestMethod]
        public async Task GetInvoicesById_nullInvoiceId()
        {
            request.RequestUri = $"v2/invoices/null";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("Unable to retrieve entity 'Invoice' by id 'null'"), $"invoice is not as expected - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task GetInvoicesById_emptyInvoiceId()
        {
            request.RequestUri = $"v2/invoices";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("Value cannot be null"), $"invoice is not as expected - {invoicesResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetInvoicesById_invalidInvoiceId()
        {
            request.RequestUri = $"v2/invoices/518";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("Unable to retrieve entity 'Invoice' by id"), $"invoice is not as expected - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task GetInvoicesById_extraParameter()
        {
            request.RequestUri = $"v2/invoices/{invoiceId}/itemid=518";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("An HTTP error of code 404 occurred while executing the request to uri"), $"invoice is not as expected - {invoicesResult.Message}");
        }

    }
}