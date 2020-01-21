//----------------------------------------------------------------------------------------------------------
// <copyright file="PaymentMethodTypesTests.cs" company="Trupanion">
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
    using Trupanion.Billing.Api.PaymentMethods.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class PaymentMethodTypesTests : BillingApiTestBase
    {
        private RestResult pmtResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task GetPaymentMethodTypes_success()
        {
            request.RequestUri = $"v2/paymentmethodtypes";
            pmtResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(pmtResult.Success, $"failed to restclient get from billing service");
            List<PaymentMethodType> paymentMethodTypes = JsonSerializer.Deserialize<List<PaymentMethodType>>(((RestResult<string>)pmtResult).Value);
            PaymentMethodType pm = paymentMethodTypes.Where(n => n.Name.ToLower().Equals("creditcard")).FirstOrDefault();
            Assert.IsTrue(pm != null, $"bank account types are not as expected - {paymentMethodTypes}");
        }

        [TestMethod]
        public async Task GetPaymentMethodTypes_extraParameter()
        {
            request.RequestUri = $"v2/paymentmethodtypes/id=518";
            pmtResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pmtResult.Success, $"successed unexpectedly");
            Assert.IsTrue(pmtResult.Message.Contains("An HTTP error of code 404 occurred while executing the request"), $"unexpected message - {pmtResult.Message}");
        }

    }
}
