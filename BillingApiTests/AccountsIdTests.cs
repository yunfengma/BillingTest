//----------------------------------------------------------------------------------------------------------
// <copyright file="AccountsIdTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using BillingTestCommon.Methods;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class AccountsIdTests : BillingApiTestBase
    {
        private RestResult accountResult { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountsById_success()
        {
            request.RequestUri = $"v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to restclient get account by id");
            Account account = JsonSerializer.Deserialize<Account>(((RestResult<string>)accountResult).Value);
            Assert.IsNotNull(account, $"failed to get account by id {BillingApiTestSettings.Default.BillingServiceApiAccountTruDatOwnerUniqueId}");
            Assert.IsTrue(account.PartyId.ToString().ToLower().Equals(BillingApiTestSettings.Default.BillingServiceApiAccountPartyId.ToLower()), $"parutyid is not as expected");
        }

        [TestMethod]
        public async Task GetAccountsById_nullRequest()
        {
            try
            {
                await asyncRestClientBilling.ExecuteAsync<string>(null);
                Assert.Fail($"successed restclient get from billing services");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Value cannot be null."), $"unexpected message - {ex.Message}");
            }
        }

        [TestMethod]
        public async Task GetAccountsById_emptyRequest()
        {
            try
            {
                await asyncRestClientBilling.ExecuteAsync<string>(new RestRequestSpecification());
                Assert.Fail($"successed restclient get from billing services");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Object reference not set to an instance of an object."), $"unexpected message - {ex.Message}");
            }
        }

        [TestMethod]
        public async Task GetAccountsById_nonExistAccountId()
        {
            request.RequestUri = $"v2/accounts/{int.MaxValue}";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account number of maximum integer");
            Assert.IsTrue(accountResult.Message.Contains($"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsById_zeroAccountId()
        {
            request.RequestUri = $"v2/accounts/0";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account number of 0");
            Assert.IsTrue(accountResult.Message.Contains($"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsById_negativeAccountId()
        {
            request.RequestUri = $"v2/accounts/-1";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account number of -1");
            Assert.IsTrue(accountResult.Message.Contains($"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsById_digitsAccountId()
        {
            request.RequestUri = $"v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountId}";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account with garbase string");
            Assert.IsTrue(accountResult.Message.Contains($"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountsById_invalidAccountId()
        {
            request.RequestUri = $"v2/accounts/*&34324^(*asdf&";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account with garbase string");
            Assert.IsTrue(accountResult.Message.Contains($"Unable to retrieve 'LocalAccount' entity by specified query."), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task GetAccountsById_extraParameter()
        {
            request.RequestUri = $"v2/accounts/{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId}/accounttype=518";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"failed to restclient get from billing services");
            Assert.IsTrue(accountResult.Message.Contains("An HTTP error of code 404 occurred while executing the request"), $"unexpected message - {accountResult.Message}");
        }


    }
}
