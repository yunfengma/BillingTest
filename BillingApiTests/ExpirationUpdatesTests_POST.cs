//----------------------------------------------------------------------------------------------------------
// <copyright file="ExpirationUpdatesTests_POST.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.PaymentMethods.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class ExpirationUpdatesTests_POST : BillingApiTestBase
    {
        private RestResult invoicesResult { get; set; }
        private UpdateCreditCardExpirationCommand command { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/creditcards/expirationupdates";

            command = new UpdateCreditCardExpirationCommand();
            command.PaymentMethodId = "";
            command.Month = 5;
            command.Year = 5828;
        }

        // TODO: wating for credit card enrollment happened

        ////[TestMethod]
        public async Task ExpirationUpdates_success_TODO()
        {
            //request.Content = JsonSerializer.Serialize(command);
            //invoicesResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            //Assert.IsNotNull(invoicesResult.Success, $"failed to restclient get from billing service");
            //InvoicePreview preview = JsonSerializer.Deserialize<InvoicePreview>(((RestResult<string>)invoicesResult).Value);
            //Assert.IsTrue(preview.Items.First().ChargeName.Equals("Pet Insurance Monthly Premium"), $"unexpected item - {preview}");
        }


    }
}