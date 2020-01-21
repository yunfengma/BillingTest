//----------------------------------------------------------------------------------------------------------
// <copyright file="AccountUpdatesTests_POST.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class AccountUpdatesTests_POST : BillingApiTestBase
    {
        private RestResult auResult { get; set; }
        private UpdateAccountCommand command { get; set; }
        private OperationResponse auResponse { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/accountupdates";

            command = new UpdateAccountCommand();
            command.AccountId = BillingApiTestSettings.Default.BillingAccountUpdateTestAccountId.ToString();
            command.DefaultPaymentMethodId = BillingApiTestSettings.Default.BillingAccountUpdateTestDefaultPaymentMethodId;
            command.BillCycleDay = BillingApiTestSettings.Default.BillingAccountUpdateTestBillCycleDay;
            command.CharityId = BillingApiTestSettings.Default.BillingAccountUpdateTestCharityId;
            command.NonReferencedCreditMethodTypeId = BillingApiTestSettings.Default.BillingAccountUpdateTestNonReferencedCreditMethodTypeId;
        }


        [TestMethod, TestCategory("BVT")]
        public async Task AccountUpdates_success()
        {
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(auResult.Success, $"failed to restclient get from billing service");
        }

        [TestMethod]
        public async Task AccountUpdates_nullCommand()
        {
            command = null;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed");
            Assert.IsTrue(auResult.Message.Contains(@"Parameter cannot be null."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_empryCommand()
        {
            command.AccountId = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed");
            Assert.IsTrue(auResult.Message.Contains(@"Account id must be supplied and non-empty."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_nullAccountId()
        {
            command.AccountId = null;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for null account id");
            Assert.IsTrue(auResult.Message.Contains(@"Account id must be supplied and non-empty."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_emptyAccountId()
        {
            command.AccountId = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for empty account id");
            Assert.IsTrue(auResult.Message.Contains(@"Account id must be supplied and non-empty."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task AccountUpdates_invalidAccountId()
        {
            command.AccountId = $"518{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId.ToString()}";
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for not exist account id");
            Assert.IsTrue(auResult.Message.Contains(@"Unable to find local account by externalId"), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_nullDefaultPaymentMethod()
        {
            command.DefaultPaymentMethodId = null;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for null default payment method");
            Assert.IsTrue(auResult.Message.Contains(@"Object reference not set to an instance of an object."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_emptyDefaultPaymentMethod()
        {
            command.DefaultPaymentMethodId = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for empty default payment method");
            Assert.IsTrue(auResult.Message.Contains(@"Unable to retrieve entity 'PaymentMethod' by id"), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_nonExistDefaultPaymentMethod()
        {
            command.DefaultPaymentMethodId = $"518{BillingApiTestSettings.Default.BillingServiceApiAccountDefaultPaymentMethodId}";
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for invalid account id");
            Assert.IsTrue(auResult.Message.Contains(@"Unable to retrieve entity 'PaymentMethod' by id"), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_notExistBillCycleDay()
        {
            command.BillCycleDay = int.MaxValue;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for nont exist");
            Assert.IsTrue(auResult.Message.Contains(@"Bill cycle day must be between 1 and 31."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_nullCharityId()
        {
            command.CharityId = null;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            //Assert.IsFalse(auResult.Success, $"successed for null");
            //Assert.IsTrue(auResult.Message.Contains(@"Invalid value for field DefaultPaymentMethodId"), $"unexpected message - {auResult.Message}");
            Assert.IsTrue(auResult.Success, $"may not fauled");
            Assert.IsNull(((RestResult<string>)auResult).Value);
        }

        [TestMethod]
        public async Task AccountUpdates_emptyCharityId()
        {
            command.CharityId = Guid.Empty;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for empty");
            Assert.IsTrue(auResult.Message.Contains(@"Unable to retrieve 'Charity' entity by specified query."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_nonExistCharityId()
        {
            command.CharityId = Guid.NewGuid();
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for nont exist");
            Assert.IsTrue(auResult.Message.Contains(@"Unable to retrieve 'Charity' entity by specified query."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_nullNonReferencedCreditMethodTypeId()
        {
            command.NonReferencedCreditMethodTypeId = Guid.Empty;
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for empty");
            Assert.IsTrue(auResult.Message.Contains(@"Non-referenced credit method type must be supplied."), $"unexpected message - {auResult.Message}");
        }

        [TestMethod]
        public async Task AccountUpdates_notExistNonReferencedCreditMethodTypeId()
        {
            command.NonReferencedCreditMethodTypeId = Guid.NewGuid();
            request.Content = JsonSerializer.Serialize(command);
            auResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(auResult.Success, $"successed for nont exist");
            Assert.IsTrue(auResult.Message.Contains(@"Unable to retrieve 'PaymentMethodType' entity by specified query."), $"unexpected message - {auResult.Message}");
        }


    }
}