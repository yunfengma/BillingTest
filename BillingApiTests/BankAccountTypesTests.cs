//----------------------------------------------------------------------------------------------------------
// <copyright file="BankAccountTypesTests.cs" company="Trupanion">
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
    using Trupanion.Billing.Api.BankAccounts.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class BankAccountTypesTests : BillingApiTestBase
    {
        private RestResult bankAccountTypeResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetBankAccountTypes_success()
        {
            request.RequestUri = $"v2/bankaccounttypes";
            bankAccountTypeResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(bankAccountTypeResult.Success, $"failed to restclient get account by id");
            List<BankAccountType> bankAccountTypes = JsonSerializer.Deserialize<List<BankAccountType>>(((RestResult<string>)bankAccountTypeResult).Value);
            Assert.IsTrue(bankAccountTypes?.Count > 0, $"failed to get account types");
            Assert.IsTrue(bankAccountTypes.First().Name.ToLower().Equals(BillingApiTestSettings.Default.BillingServiceTypesChecking.ToLower()), $"bank account types are not as expected - {bankAccountTypes}");
        }

        [TestMethod]
        public async Task GetBankAccountTypes_extraParameter()
        {
            request.RequestUri = $"v2/bankaccounttypes/type=518";
            bankAccountTypeResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(bankAccountTypeResult.Success, $"successed unexpectedly");
            Assert.IsTrue(bankAccountTypeResult.Message.Contains("An HTTP error of code 404 occurred while executing the request"), $"unexpected message - {bankAccountTypeResult.Message}");
        }

    }
}
