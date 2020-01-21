//----------------------------------------------------------------------------------------------------------
// <copyright file="InvoicesItemsTests.cs" company="Trupanion">
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
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class InvoicesItemsTests : BillingApiTestBase
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
        public async Task GetInvoicesItems_success()
        {
            request.RequestUri = $"v2/invoices/{invoiceId}/items";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"failed to restclient get from billing service");
            List<InvoiceItem> invoices = JsonSerializer.Deserialize<List<InvoiceItem>>(((RestResult<string>)invoicesResult).Value);
            InvoiceItem invoice = invoices.Where(i => i.ChargeAmount == BillingApiTestSettings.Default.BillingServiceApiAccountInvoiceItemChargeAmount).FirstOrDefault();
            Assert.IsTrue(invoice != null, $"invoice is not as expected - {invoices}");
        }

        [TestMethod]
        public async Task GetInvoicesItems_nullInvoiceId()
        {
            request.RequestUri = $"v2/invoices/null/items";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)invoicesResult).Value}");
        }

        [TestMethod]
        public async Task GetInvoicesItems_emptyInvoiceId()
        {
            request.RequestUri = $"v2/invoices//items";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains(@"Unable to retrieve entity 'Invoice' by id 'items'"), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetInvoicesItems_invalidInvoiceId()
        {
            request.RequestUri = $"v2/invoices/518{invoiceId}/items";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)invoicesResult).Value}");
        }

        [TestMethod]
        public async Task GetInvoicesItems_invalidFormat()
        {
            request.RequestUri = $"v2/invoices/items/{invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains(@"An HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task GetInvoicesItems_extraParameter()
        {
            request.RequestUri = $"v2/invoices/518{invoiceId}/items/id=518";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains(@"An HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {invoicesResult.Message}");
        }

    }
}