//----------------------------------------------------------------------------------------------------------
// <copyright file="ServiceAccessTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using BillingTestCommon.Methods;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class ServiceAccessTests : BillingApiTestBase
    {
        private RestResult accountResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public void BillingServiceIsRunning()
        {
            bool bv = CheckServiceIsRunning(BillingApiTestSettings.Default.BillingServiceManchineName, BillingApiTestSettings.Default.BillingServiceName);
            Assert.IsTrue(bv, $"billing service is not running - <{BillingApiTestSettings.Default.BillingServiceManchineName}>, {BillingApiTestSettings.Default.BillingServiceName}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task BillingService_successWithAuthenticationToken()
        {
            request.RequestUri = $"v2/products";
            var response = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(response.Success, $"failed to restclient get from billing services");
        }

        [TestMethod]
        public async Task BillingService_nullAuthorizationToken()
        {
            request = new RestRequestSpecification();
            request.Verb = HttpMethod.Get;
            Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", null);
            request.Headers = Headers;
            request.ContentType = "application/json";
            request.RequestUri = $"v2/products";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account with null token");
            Assert.IsTrue(accountResult.Message.Contains($"An error occurred (URL=http://"), $"unexpected message - {accountResult.Message}");
        }

        [TestMethod]
        public async Task BillingService_emptyAuthorizationToken()
        {
            request = new RestRequestSpecification();
            request.Verb = HttpMethod.Get;
            Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", string.Empty);
            request.Headers = Headers;
            request.ContentType = "application/json";
            request.RequestUri = $"v2/products";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account with invalid token");
            Assert.IsTrue(accountResult.Message.Contains($"An error occurred (URL=http://"), $"unexpected message - {accountResult.Message}");
        }

        //[TestMethod]
        public async Task BillingService_withoutAuthorizationToken()
        {
            request = new RestRequestSpecification();
            request.Verb = HttpMethod.Get;
            request.ContentType = "application/json";
            request.RequestUri = $"v2/products";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account with invalid token");
            Assert.IsTrue(accountResult.Message.Contains($"An error occurred (URL=http://"), $"unexpected message - {accountResult.Message}");
        }

        //[TestMethod]
        public async Task BillingService_invalidAuthorizationToken()
        {
            request = new RestRequestSpecification();
            request.Verb = HttpMethod.Get;
            Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", $"badtoken");
            request.Headers = Headers;
            request.ContentType = "application/json";
            request.RequestUri = $"v2/products";
            accountResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(accountResult.Success, $"successed get account with invalid token");
            Assert.IsTrue(accountResult.Message.Contains($"An error occurred (URL=http://"), $"unexpected message - {accountResult.Message}");
        }

    }
}
