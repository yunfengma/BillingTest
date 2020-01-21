//----------------------------------------------------------------------------------------------------------
// <copyright file="InvoicesTests.cs" company="Trupanion">
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
    public class InvoicesTests : BillingApiTestBase
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
        public async Task GetInvoices_success()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId={accountExternalId}&criteria.refundId={invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"failed to restclient get from billing service");
            List<InvoiceWithItems> invoices = JsonSerializer.Deserialize<List<InvoiceWithItems>>(((RestResult<string>)invoicesResult).Value);
            InvoiceWithItems invoice = invoices.Where(i => i.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountInoiceAmount).FirstOrDefault();
            Assert.IsNotNull(invoice, $"invoice is not as expected - {invoices}");
        }

        [TestMethod]
        public async Task GetInvoices_nullAccountId()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId=null&criteria.refundId={invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"unexpected value - {((RestResult<string>)invoicesResult).Value}");
        }

        [TestMethod]
        public async Task GetInvoices_emptyAccountId()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId&criteria.refundId={invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            List<InvoiceWithItems> invoices = JsonSerializer.Deserialize<List<InvoiceWithItems>>(((RestResult<string>)invoicesResult).Value);
            Assert.IsTrue(invoices?.Count == 0, $"got invoices - {invoices}");
        }

        [TestMethod]
        public async Task GetInvoices_missingAccountId()
        {
            request.RequestUri = $"v2/invoices?criteria.refundId={invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);                            // got all invoices
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"invoice is not as expected - {invoicesResult}");
        }

        [TestMethod]
        public async Task GetInvoices_nullInvoiceId()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId={accountExternalId}&criteria.refundId=null";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);                            // ??? got all invoices
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            List<InvoiceWithItems> invoices = JsonSerializer.Deserialize<List<InvoiceWithItems>>(((RestResult<string>)invoicesResult).Value);
            InvoiceWithItems invoice = invoices.Where(i => i.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountInoiceAmount).FirstOrDefault();
            Assert.IsNotNull(invoice, $"invoice is not as expected - {invoices}");
        }

        [TestMethod]
        public async Task GetInvoices_emptyInvoiceId()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId={accountExternalId}&criteria.refundId";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);                            // got all invoices
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            List<InvoiceWithItems> invoices = JsonSerializer.Deserialize<List<InvoiceWithItems>>(((RestResult<string>)invoicesResult).Value);
            InvoiceWithItems invoice = invoices.Where(i => i.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountInoiceAmount).FirstOrDefault();
            Assert.IsNotNull(invoice, $"invoice is not as expected - {invoices}");
        }

        [TestMethod]
        public async Task GetInvoices_missingInvoiceId()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId={accountExternalId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);                            // got all invoices
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            List<InvoiceWithItems> invoices = JsonSerializer.Deserialize<List<InvoiceWithItems>>(((RestResult<string>)invoicesResult).Value);
            InvoiceWithItems invoice = invoices.Where(i => i.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountInoiceAmount).FirstOrDefault();
            Assert.IsNotNull(invoice, $"invoice is not as expected - {invoices}");
        }

        [TestMethod]
        public async Task GetInvoices_missingBothAccountAndInvoice()
        {
            request.RequestUri = $"v2/invoices?criteria.accountId={string.Empty}&criteria.refundId={string.Empty}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("'Refund Id' should not be empty."), $"invoice is not as expected - {invoicesResult}");
        }

        [TestMethod]
        public async Task GetInvoices_nullCriteria()
        {
            request.RequestUri = $"v2/invoices?{null}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("Value cannot be null."), $"invoice is not as expected - {invoicesResult}");
        }

        [TestMethod]
        public async Task GetInvoices_extraParameter()
        {
            request.RequestUri = $"v2/invoices?criteria.name=518/criteria.accountId={accountExternalId}&criteria.refundId={invoiceId}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Equals("[]"), $"invoice is not as expected - {invoicesResult}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetInvoices_notMatchingAccountAndInvoice()                 // invoice id belong to another account
        {
            request.RequestUri = $"v2/invoices?criteria.accountId={accountExternalId}&criteria.refundId={BillingApiTestSettings.Default.BillingServiceApiAccountInoiceIdUnMatched}";
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(invoicesResult.Success, $"failed to restclient get from billing service");
            List<InvoiceWithItems> invoices = JsonSerializer.Deserialize<List<InvoiceWithItems>>(((RestResult<string>)invoicesResult).Value);
            // ??? got all invoices of the account
            //InvoiceWithItems invoice = invoices.Where(i => i.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountInoiceAmount).FirstOrDefault();
            //Assert.IsNotNull(invoice, $"invoice is not as expected - {invoices}");
        }


    }
}