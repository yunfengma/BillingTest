//----------------------------------------------------------------------------------------------------------
// <copyright file="AccountsTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class AccountsTests : BillingApiTestBase
    {
        private RestResult accountResult { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountsByPolicyHolderId_success()
        {
            request.RequestUri = $"v2/accounts?criteria.policyholderId={BillingApiTestSettings.Default.BillingServiceApiAccountTruDatOwnerUniqueId}";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"successed unexpectedly");
            List<Account> accounts = JsonSerializer.Deserialize<List<Account>>(((RestResult<string>)accountResult).Value);
            Assert.IsTrue(accounts?.Count > 0, $"failed to get account by id {BillingApiTestSettings.Default.BillingServiceApiAccountTruDatOwnerUniqueId}");
            Assert.IsTrue(accounts.First().PartyId.ToString().ToLower().Equals(BillingApiTestSettings.Default.BillingServiceApiAccountPartyId.ToLower()), $"parutyid is not as expected");
        }

        [TestMethod]
        public async Task GetAccountsByPolicyHolderId_nullPolicyHolderId()
        {
            request.RequestUri = $"v2/accounts?criteria.policyholderId=null";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to get result from billing service");
            Assert.IsTrue(((RestResult<string>)accountResult).Value.Equals($"[]"), $"unexpected value for empty id - {((RestResult<string>)accountResult).Value}");
        }

        [TestMethod, TestCategory("Testing")]
        public async Task GetAccountsByPolicyHolderId_emptyPolicyHolderId()
        {
            request.RequestUri = $"v2/accounts?criteria.policyholderId=";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to get result from billing service");
            Assert.IsTrue(((RestResult<string>)accountResult).Value.Equals($"[]"), $"unexpected value for empty id - {((RestResult<string>)accountResult).Value}");
        }

        [TestMethod]
        public async Task GetAccountsByPolicyHolderId_nonExistingPolicyHolderId()
        {
            request.RequestUri = $"v2/accounts?criteria.policyholderId={Guid.NewGuid().ToString()}";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to get result from billing service");
            Assert.IsTrue(((RestResult<string>)accountResult).Value.Equals($"[]"), $"unexpected value for not existing id - {((RestResult<string>)accountResult).Value}");
        }

        [TestMethod]
        public async Task GetAccountsByPolicyHolderId_invalidFormatPolicyHolderId()
        {
            request.RequestUri = $"v2/accounts?criteria.policyholderId={BillingApiTestSettings.Default.BillingServiceApiAccountTruDatOwnerUniqueId.Substring(8)}";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to get result from billing service");
            Assert.IsTrue(((RestResult<string>)accountResult).Value.Equals($"[]"), $"unexpected value for invalid format id - {((RestResult<string>)accountResult).Value}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountsByPolicyHolderId_invalidPolicyHolderId()
        {
            request.RequestUri = $"v2/accounts?criteria.policyholderId=asdf(*45&(*";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(accountResult.Success, $"failed to get result from billing service");
            Assert.IsTrue(((RestResult<string>)accountResult).Value.Equals($"[]"), $"unexpected value for garbage - {((RestResult<string>)accountResult).Value}");
        }

        [TestMethod]
        public async Task GetAccountsByPolicyHolderId_missingCriteria()
        {
            request.RequestUri = $"v2/accounts?";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed unexpectedly");
            Assert.IsTrue(accountResult.Message.Contains("Parameter cannot be null."), $"unexpected message - {accountResult.Message}");
        }


    }
}
