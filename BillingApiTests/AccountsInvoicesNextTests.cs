//----------------------------------------------------------------------------------------------------------
// <copyright file="AccountsInvoicesNextTests.cs" company="Trupanion">
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
    public class AccountsInvoicesNextTests : BillingApiTestBase
    {
        private RestResult accountResult { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountsNextInvoice_success()
        {
            request.RequestUri = $"/v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to restclient get account next invoice");
            InvoiceWithItems invoiceWithItems = JsonSerializer.Deserialize<InvoiceWithItems>(((RestResult<string>)accountResult).Value);
            Assert.IsNotNull(invoiceWithItems, $"no invoice for {BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}");
            //Assert.IsTrue(invoiceWithItems.Amount == BillingApiTestSettings.Default.BillingServiceApiAccountNextInvoiceAmount, $"invoice amout {invoiceWithItems.Amount.ToString()} is not as expected - {BillingApiTestSettings.Default.BillingServiceApiAccountNextInvoiceAmount.ToString()}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_nullExternalId()
        {
            request.RequestUri = $"/v2/accounts/null/invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get invoice for null account");
            Assert.IsTrue(accountResult.Message.Contains("Unable to retrieve entity 'ExternalAccount' by id 'null'"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_emptyExternalId()
        {
            request.RequestUri = $"/v2/accounts//invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get invoice for empty account");
            Assert.IsTrue(accountResult.Message.Contains("An HTTP error of code 404 occurred"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountsNextInvoice_nonExistingExternalId()
        {
            request.RequestUri = $"/v2/accounts/518{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get invoice for notexist account");
            Assert.IsTrue(accountResult.Message.Contains("Unable to retrieve entity 'ExternalAccount'"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_invalidExternalId()
        {
            // garbage string
            request.RequestUri = $"/v2/accounts/*&asdf^*35&^*/invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get invoice for garbage account");
            Assert.IsTrue(accountResult.Message.Contains("Unable to retrieve entity 'ExternalAccount'"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_missingAccountId()
        {
            request.RequestUri = $"/v2/accounts/invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed unexpectedly");
            Assert.IsTrue(accountResult.Message.Contains("An HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_extraParameterAtTheEnd()
        {
            request.RequestUri = $"/v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/invoices/next/item=518";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed unexpectedly for adding para");
            Assert.IsTrue(accountResult.Message.Contains("An HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_extraParameterAfterAccountId()
        {
            request.RequestUri = $"/v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/item=518/invoices/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed unexpectedly for adding after accountid");
            Assert.IsTrue(accountResult.Message.Contains("An HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsNextInvoice_extraParameterAfterInvoice()
        {
            request.RequestUri = $"/v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/invoices/item=518/next";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed unexpectedly after invoice");
            Assert.IsTrue(accountResult.Message.Contains("An HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {accountResult.Message}");
        }

    }
}
