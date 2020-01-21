//----------------------------------------------------------------------------------------------------------
// <copyright file="RescheduledPaymentsTests_POST.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Payments.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class RescheduledPaymentsTests_POST : BillingApiTestBase
    {
        private RestResult rpResult { get; set; }
        private ReschedulePaymentCommand command { get; set; }
        private OperationResponse rpResponse { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/reschedulepayments";

            command = new ReschedulePaymentCommand();
            command.AccountId = BillingApiTestSettings.Default.BillingServiceApiAccountExternalId.ToString();
            command.RequestedDate = DateTime.Now.AddDays(8);
        }


        [TestMethod, TestCategory("BVT")]
        public async Task ReschedulePayments_success()
        {
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(rpResult.Success, $"failed to restclient get from billing service");
            Assert.IsNull(((RestResult<string>)rpResult).Value, $"unexpected value - {rpResult}");
        }

        [TestMethod]
        public async Task ReschedulePayments_nullCommand()
        {
            command = null;
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed");
            Assert.IsTrue(rpResult.Message.Contains(@"Object reference not set to an instance of an object."), $"unexpected message - {rpResult.Message}");
        }

        [TestMethod]
        public async Task ReschedulePayments_empryCommand()
        {
            command = new ReschedulePaymentCommand();
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed");
            Assert.IsTrue(rpResult.Message.Contains(@"Unable to find local account by externalId"), $"unexpected message - {rpResult.Message}");
        }

        [TestMethod]
        public async Task ReschedulePayments_nullAccountId()
        {
            command.AccountId = null;
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed for null account id");
            Assert.IsTrue(rpResult.Message.Contains(@"Unable to find local account by externalId"), $"unexpected message - {rpResult.Message}");
        }

        [TestMethod]
        public async Task ReschedulePayments_emptyAccountId()
        {
            command.AccountId = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed for empty account id");
            Assert.IsTrue(rpResult.Message.Contains(@"Unable to find local account by externalId"), $"unexpected message - {rpResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task ReschedulePayments_nonExistAccountId()
        {
            command.AccountId = $"518{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId.ToString()}";
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed for not exist account id");
            Assert.IsTrue(rpResult.Message.Contains(@"Unable to find local account by externalId"), $"unexpected message - {rpResult.Message}");
        }

        [TestMethod]
        public async Task ReschedulePayments_minimumDateRequestedDate()
        {
            command.RequestedDate = DateTime.MinValue;
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed for minimum date");
            Assert.IsTrue(rpResult.Message.Contains(@"Requested date must be greater than today"), $"unexpected message - {rpResult.Message}");
        }

        [TestMethod]
        public async Task ReschedulePayments_maximumDateRequestedDate()
        {
            command.RequestedDate = DateTime.MaxValue;
            request.Content = JsonSerializer.Serialize(command);
            rpResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(rpResult.Success, $"successed for maximum date");
            Assert.IsTrue(rpResult.Message.Contains($"Payment can only be rescheduled up to"), $"unexpected message - {rpResult.Message}");
            // TODO: specific for the future month of a month in short date formatn mm/dd/yyyy
        }

    }
}