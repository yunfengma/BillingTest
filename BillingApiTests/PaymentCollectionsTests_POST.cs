//----------------------------------------------------------------------------------------------------------
// <copyright file="PaymentCollectionsTests_POST.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Payments.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class PaymentCollectionsTests_POST : BillingApiTestBase
    {
        private RestResult pcResult { get; set; }
        private CollectPaymentCommand command { get; set; }
        private CollectPaymentResponse pcResponse { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/paymentcollections";

            command = new CollectPaymentCommand();
            command.AccountId = BillingApiTestSettings.Default.BillingServiceApiPaymentCollectionsAccountId;
            pcResponse = new CollectPaymentResponse();
        }


        //[TestMethod]
        // covered in EnrollmentTests\AccountTests\AccountalancePayNow
        public async Task PaymentCollections_success_TODO()
        {
            // TODO: need a new payment created
            //request.Content = JsonSerializer.Serialize(command);
            //pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            //Assert.IsTrue(pcResult.Success, $"failed to restclient get from billing service");
            //pcResponse = JsonSerializer.Deserialize<CollectPaymentResponse>(((RestResult<string>)pcResult).Value);
            //Assert.IsTrue(pcResponse.PaymentId.Equals("Pet Insurance Monthly Premium"), $"unexpected item - {((RestResult<string>)pcResult).Value}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task PaymentCollections_reCollectPayment()
        {
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"succe3sed re-collected payment");
            Assert.IsTrue(pcResult.Message.Contains(@"An error occurred"), $"unexpected message - {pcResult.Message}");
        }

        [TestMethod]
        public async Task PaymentCollections_nullCommand()
        {
            command = null;
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"successed");
            Assert.IsTrue(pcResult.Message.Contains(@"Object reference not set to an instance of an object."), $"unexpected message - {pcResult.Message}");
        }

        [TestMethod]
        public async Task PaymentCollections_nullAccountId()
        {
            command.AccountId = null;
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"successed");
            Assert.IsTrue(pcResult.Message.Contains(@"Parameter cannot be null."), $"unexpected message - {pcResult.Message}");
        }

        [TestMethod]
        public async Task PaymentCollections_emptyAccountId()
        {
            command.AccountId = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"successed");
            Assert.IsTrue(pcResult.Message.Contains(@"Parameter cannot be null."), $"unexpected message - {pcResult.Message}");
        }

        [TestMethod]
        public async Task PaymentCollections_notExistAccountId()
        {
            command.AccountId = int.MaxValue.ToString();
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"successed with not exist");
            Assert.IsTrue(pcResult.Message.Contains(@"Payment collection failed."), $"unexpected message - {pcResult.Message}");
        }

        [TestMethod]
        public async Task PaymentCollections_negativeAccountId()
        {
            command.AccountId = $"-{command.AccountId}";
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"successed with negative number");
            Assert.IsTrue(pcResult.Message.Contains(@"Payment collection failed."), $"unexpected message - {pcResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task PaymentCollections_invalidAccountId()
        {
            command.AccountId = "abcd**&*&^";
            request.Content = JsonSerializer.Serialize(command);
            pcResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(pcResult.Success, $"successed with garbage string");
            Assert.IsTrue(pcResult.Message.Contains(@"Payment collection failed."), $"unexpected message - {pcResult.Message}");
        }


    }
}