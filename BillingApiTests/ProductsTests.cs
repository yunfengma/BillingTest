//----------------------------------------------------------------------------------------------------------
// <copyright file="ProductsTests.cs" company="Trupanion">
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
    using Trupanion.Billing.Api.Products.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class ProductsTests : BillingApiTestBase
    {
        private RestResult productsResult { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Get;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task Getproducts_success()
        {
            request.RequestUri = $"v2/products";
            productsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(productsResult.Success, $"failed to restclient get from billing services");
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(((RestResult<string>)productsResult).Value);
            Assert.IsTrue(products?.Count == BillingApiTestSettings.Default.BillingServiceProductsNumber, $"unexpected products - {productsResult}");
            Product prod = products.Where(p => p.Name.ToLower().Equals(BillingApiTestSettings.Default.BillingServiceProductsName.ToLower())).FirstOrDefault();
            Assert.IsTrue(prod != null, $"unexpected products - {productsResult}");
        }

        [TestMethod]
        public async Task Getproducts_extraParameter()
        {
            request.RequestUri = $"v2/products/accountid=12345";
            productsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(productsResult.Success, $"successed unexpectedly");
            Assert.IsTrue(productsResult.Message.Contains("An HTTP error of code 404 occurred while executing the request"), $"unexpected message - {productsResult.Message}");
        }

    }
}
