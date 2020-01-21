//----------------------------------------------------------------------------------------------------------
// <copyright file="InvoicesPreviewTests.cs" company="Trupanion">
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
    public class InvoicesPreviewTests : BillingApiTestBase
    {
        private RestResult invoicesResult { get; set; }
        private static InvoicePreviewCriteria previewCriteria { get; set; }


        [ClassInitialize]
        public static new void InitTestClass(TestContext context)
        {
            BillingApiTestBase.InitTestClass(context);

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/invoices/previews";

            previewCriteria = BuildInvoicePreviewCriteriaFromInvoice("USA");
        }


        [TestMethod]
        public async Task InvoicesPreview_success()
        {
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsNotNull(invoicesResult.Success, $"failed to restclient get from billing service");
            InvoicePreview preview = JsonSerializer.Deserialize<InvoicePreview>(((RestResult<string>)invoicesResult).Value);
            Assert.IsTrue(preview.Items.First().ChargeName.Equals("Pet Insurance Monthly Premium"), $"unexpected item - {preview}");
        }

        [TestMethod]
        public async Task InvoicesPreview_nullInvoicePreviewCriteria()
        {
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("Parameter cannot be null."), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task InvoicesPreview_emptyInvoicePreviewCriteria()
        {
            request.Content = string.Empty;
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly");
            Assert.IsTrue(invoicesResult.Message.Contains("Parameter cannot be null."), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task InvoicesPreview_invalidPostalCode()
        {
            // null
            previewCriteria.PostalCode = null;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            // ??? does postal code in use
            Assert.IsFalse(invoicesResult.Success, $"successed with null");
            Assert.IsTrue(invoicesResult.Message.Contains("'NULL' is not a valid postal code"), $"unexpected message - {invoicesResult.Message}");
            //Assert.IsTrue(invoicesResult.Success, $"failed to restclient get from billing service");
            //InvoicePreview preview = JsonSerializer.Deserialize<InvoicePreview>(((RestResult<string>)invoicesResult).Value);
            //Assert.IsTrue(preview.Items.First().ChargeName.Equals("Pet Insurance Monthly Premium"), $"unexpected item - {preview}");
            // empty
            previewCriteria.PostalCode = string.Empty;
            // invalid
            previewCriteria.PostalCode = "518";
            // doesn't matches are covered in another test case
        }

        [TestMethod]
        public async Task InvoicesPreview_invalidProviceCode()
        {
            // null
            previewCriteria.IsoAlpha2SateOrProvinceCode = null;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed with null");
            Assert.IsTrue(invoicesResult.Message.Contains("Taxation Requirement: State is required for Sold To Contact if the country is United States or Canada"), $"unexpected message - {invoicesResult.Message}");
            // empty
            previewCriteria.IsoAlpha2SateOrProvinceCode = string.Empty;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed with empty");
            Assert.IsTrue(invoicesResult.Message.Contains("Taxation Requirement: State is required for Sold To Contact if the country is United States or Canada"), $"unexpected message - {invoicesResult.Message}");
            // bad
            previewCriteria.IsoAlpha2SateOrProvinceCode = "518";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed with invalid");
            Assert.IsTrue(invoicesResult.Message.Contains("State/Province should be ISO standard state or province."), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task InvoicesPreview_invalidContryCode()
        {
            // null
            previewCriteria.IsoAlpha3CountryCode = null;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed with null");
            Assert.IsTrue(invoicesResult.Message.Contains("'NULL' is not a valid ISO alpha 3 country code"), $"unexpected message - {invoicesResult.Message}");
            // empty
            previewCriteria.IsoAlpha3CountryCode = string.Empty;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed with empty");
            Assert.IsTrue(invoicesResult.Message.Contains("'' is not a valid ISO alpha 3 country code."), $"unexpected message - {invoicesResult.Message}");
            // bad
            previewCriteria.IsoAlpha3CountryCode = "8";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed with invalid");
            Assert.IsTrue(invoicesResult.Message.Contains("'8' is not a valid ISO alpha 3 country code."), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task InvoicesPreview_invalidItems()
        {
            // null
            previewCriteria.Items = null;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            ////Assert.IsFalse(invoicesResult.Success, $"successed with null");
            ////Assert.IsTrue(invoicesResult.Message.Contains("'NULL' is not a valid ISO alpha 3 country code"), $"unexpected message - {invoicesResult.Message}");
            Assert.IsTrue(invoicesResult.Success, $"successed with null");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Contains("\"Items\":[]"), $"unexpected value - {((RestResult<string>)invoicesResult).Value}");
            // empty
            previewCriteria.Items = new List<InvoicePreviewItemCriteria>();
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            //Assert.IsFalse(invoicesResult.Success, $"successed with null");
            //Assert.IsTrue(invoicesResult.Message.Contains("'' is not a valid ISO alpha 3 country code."), $"unexpected message - {invoicesResult.Message}");
            Assert.IsTrue(invoicesResult.Success, $"successed with empty");
            Assert.IsTrue(((RestResult<string>)invoicesResult).Value.Contains("\"Items\":[]"), $"unexpected value - {((RestResult<string>)invoicesResult).Value}");
        }

        [TestMethod]
        public async Task InvoicesPreview_invalidItemsProducId()
        {
            // null
            previewCriteria.Items.First().ProductId = null;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly of null");
            Assert.IsTrue(invoicesResult.Message.Contains("Some items contain unsupported product ids"), $"unexpected message - {invoicesResult.Message}");
            // empty
            previewCriteria.Items.First().ProductId = string.Empty;
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly of empty");
            Assert.IsTrue(invoicesResult.Message.Contains("Some items contain unsupported product ids"), $"unexpected message - {invoicesResult.Message}");
            // bad
            previewCriteria.Items.First().ProductId = "51818";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed unexpectedly of invalid");
            Assert.IsTrue(invoicesResult.Message.Contains("Some items contain unsupported product ids"), $"unexpected message - {invoicesResult.Message}");
        }

        [TestMethod]
        public async Task InvoicesPreview_unMatchedCountryProvince()
        {
            // au vs us
            previewCriteria = BuildInvoicePreviewCriteriaFromInvoice("AUS");
            previewCriteria.IsoAlpha2SateOrProvinceCode = "Minnesota";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            // bug 159330
            Assert.IsFalse(invoicesResult.Success, $"successed for au vs us");
            Assert.IsTrue(invoicesResult.Message.Contains("State/Province should be ISO standard state or province."), $"unexpected message for au vs us - {invoicesResult.Message}");
            // us vs can
            previewCriteria.IsoAlpha2SateOrProvinceCode = "British Columbia";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed for us vs can");
            Assert.IsTrue(invoicesResult.Message.Contains("State/Province should be ISO standard state or province."), $"unexpected message for us vs can - {invoicesResult.Message}");
            // can vs au
            previewCriteria = BuildInvoicePreviewCriteriaFromInvoice("CAN");
            previewCriteria.IsoAlpha2SateOrProvinceCode = "Western Australia";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(invoicesResult.Success, $"successed for csn vs au");
            Assert.IsTrue(invoicesResult.Message.Contains("State/Province should be ISO standard state or province."), $"unexpected message for can vs au - {invoicesResult.Message}");
        }

        //[TestMethod]
        public async Task InvoicesPreview_unMatchedProvincePostal()
        {
            // us vs can
            previewCriteria.PostalCode = "V0G 1M0";
            request.Content = JsonSerializer.Serialize(previewCriteria);
            invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            // ??? postal code has not fails
            //Assert.IsFalse(invoicesResult.Success, $"successed for au vs us");
            Assert.IsTrue(invoicesResult.Message.Contains("State/Province should be ISO standard state or province."), $"unexpected message for au vs us - {invoicesResult.Message}");
            // can vs au
            // au vs us
        }

    }
}