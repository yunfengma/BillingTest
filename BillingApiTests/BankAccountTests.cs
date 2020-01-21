//----------------------------------------------------------------------------------------------------------
// <copyright file="BankAccountTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.BankAccounts.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class BankAccountTests : BillingApiTestBase
    {
        private RestResult bankAccountResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetBankAccountsById_success()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(bankAccountResult.Success, $"failed to restclient get account by id");
            BankAccount bankAccount = JsonSerializer.Deserialize<BankAccount>(((RestResult<string>)bankAccountResult).Value);
            Assert.IsTrue(bankAccount != null, $"failed to get account by id {BillingApiTestSettings.Default.BillingServiceApiAccountTruDatOwnerUniqueId}");
            Assert.IsTrue(bankAccount.NameOnAccount.ToString().ToLower().Equals(BillingApiTestSettings.Default.BillingServiceApiBankAccountNameOnAccount.ToLower()), $"bank account is not as expected - {bankAccount}");
        }

        [TestMethod]
        public async Task GetBankAccountsById_nullAccountId()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId=null";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed get bank account");
            Assert.IsTrue(bankAccountResult.Message.Contains($"n HTTP error of code 404 occurred while executing the request to uri"), $"unexpected message - {bankAccountResult.Message}");
        }

        [TestMethod]
        public async Task GetBankAccountsById_enptyAccountId()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId=";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed get bank account");
            Assert.IsTrue(bankAccountResult.Message.Contains($"Sequence contains more than one element"), $"unexpected message - {bankAccountResult.Message}");
        }

        [TestMethod]
        public async Task GetBankAccountsById_zeroAccountId()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId={0}";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed get account number of 0");
            Assert.IsTrue(bankAccountResult.Message.Contains($"An HTTP error of code 404 occurred"), $"unexpected message - {bankAccountResult.Message}");
        }

        [TestMethod]
        public async Task GetBankAccountsById_negativeAccountId()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId={-1}";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed get account number of -1");
            Assert.IsTrue(bankAccountResult.Message.Contains($"An HTTP error of code 404 occurred"), $"unexpected message - {bankAccountResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetBankAccountsById_invalidAccountId()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId=*&^(*&^(*&";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed get account with garbase string");
            Assert.IsTrue(bankAccountResult.Message.Contains($"An HTTP error of code 404 occurred"), $"unexpected message - {bankAccountResult.Message}");
        }

        [TestMethod]
        public async Task GetBankAccountsById_missingAccountId()
        {
            request.RequestUri = $"v2/bankaccounts?";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed unexpectedly");
            Assert.IsTrue(bankAccountResult.Message.Contains("Object reference not set to an instance of an object."), $"unexpected message - {bankAccountResult.Message}");
        }

        [TestMethod]
        public async Task GetBankAccountsById_extraParameter()
        {
            request.RequestUri = $"v2/bankaccounts?criteria.accountId={BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/bankaccount=518";
            bankAccountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountResult.Success, $"successed unexpectedly");
            Assert.IsTrue(bankAccountResult.Message.Contains("An HTTP error of code 404 occurred while executing the request"), $"unexpected message - {bankAccountResult.Message}");
        }

    }
}
