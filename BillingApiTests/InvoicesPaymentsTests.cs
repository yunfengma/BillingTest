//----------------------------------------------------------------------------------------------------------
// <copyright file="InvoicesPaymentsTests.cs" company="Trupanion">
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
    public class InvoicesPaymentsTests : BillingApiTestBase
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
        public async Task GetInvoicesWithPayments_success()
        {
            request.RequestUri = $"v2/invoices/{BillingApiTestSettings.Default.BillingServiceApiAccountInvoiceId}/payments";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"failed to restclient get from billing service");
            List<InvoicePayment> payments = JsonSerializer.Deserialize<List<InvoicePayment>>(((RestResult<string>)invoicesResult).Value);
            InvoicePayment pay = payments.Where(p => p.Id.Equals(BillingApiTestSettings.Default.BillingServiceApiAccountInvoicePaymentId)).FirstOrDefault();
            Assert.IsNotNull(pay, $"payment is not as expected - {payments}");
        }

        [TestMethod]
        public async Task GetInvoicesWithPayments_nullInvoiceId()
        {
            request.RequestUri = $"v2/invoices/null/payments";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"invoice payment is not as expected - {invoicesResult}");
        }

        [TestMethod]
        public async Task GetInvoicesWithPayments_emptyInvoiceId()
        {
            request.RequestUri = $"v2/invoices//payments";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains(@"Unable to retrieve entity 'Invoice' by id 'payments'"), $"message is not as expected - {invoicesResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetInvoicesWithPayments_notExistInvoiceId()
        {
            request.RequestUri = $"v2/invoices/518{BillingApiTestSettings.Default.BillingServiceApiAccountInvoiceId}/payments";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"invoice payment is not as expected - {invoicesResult}");
        }

        [TestMethod]
        public async Task GetInvoicesWithPayments_extraParameter()
        {
            request.RequestUri = $"v2/invoices/{BillingApiTestSettings.Default.BillingServiceApiAccountInvoiceId}/payments/{BillingApiTestSettings.Default.BillingServiceApiAccountInvoicePaymentId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains(@"An HTTP error of code 404 occurred while executing the request to uri"), $"message is not as expected - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task GetInvoicesWithPayments_invalidFormat()
        {
            request.RequestUri = $"v2/payments/invoices/{BillingApiTestSettings.Default.BillingServiceApiAccountInvoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains(@"An HTTP error of code 404 occurred while executing the request to uri"), $"message is not as expected - {invoicesResult.Message}");
        }

    }
}