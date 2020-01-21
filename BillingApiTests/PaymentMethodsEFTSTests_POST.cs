//----------------------------------------------------------------------------------------------------------
// <copyright file="PaymentMethodsEFTSTests_POST.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.PaymentMethods.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class PaymentMethodsEFTSTests_POST : BillingApiTestBase
    {
        private RestResult etfsResult { get; set; }
        private SaveEftPaymentMethodCommand command { get; set; }
        private PaymentMethod paymentMethod { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/paymentmethods/efts";

            command = new SaveEftPaymentMethodCommand();
            command.PaymentMethodId = BillingApiTestSettings.Default.BillingPaymentMethodsEFFSPOSTPartyId;
            command.PartyId = BillingApiTestSettings.Default.BillingApiPaymentMethodsEFTSPartyId;
            command.Currency = "USD";
            command.BankRoutingNumber = "125000024";
            command.BankAccountNumber = "518518";
            command.BankAccountType = "Checking";
            command.BankName = "bank account usa tests";
            command.AccountName = "518 518";
            command.IsBillingDefault = true;
            command.Data = null;
            //command.DynamicMessage = string.Empty;
            command.KeyList = new List<Trupanion.TruFoundation.EnterpriseCatalog.DynamicEntityKey>();
            //command.MessageInfo = string.Empty;
            command.OriginatingUserId = Guid.NewGuid();
        }


        [TestMethod, TestCategory("BVT")]
        public async Task PaymentMethodsETFS_success()
        {
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(etfsResult.Success, $"failed to restclient get from billing service");
            paymentMethod = JsonSerializer.Deserialize<PaymentMethod>(((RestResult<string>)etfsResult).Value);
            Assert.IsTrue(paymentMethod.AccountId.ToLower().Equals(BillingApiTestSettings.Default.BillingApiPaymentMethodsEFTSAccountId.ToLower()), $"unexpected payment method - {paymentMethod}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_nullCommand()
        {
            request.Content = JsonSerializer.Serialize(null);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when command is null");
            Assert.IsTrue(etfsResult.Message.Contains("Value cannot be null"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_emptyCommand()
        {
            command = new SaveEftPaymentMethodCommand();
            command.PaymentMethodId = Guid.Empty;
            //command.PartyId = BillingApiTestSettings.Default.BillingApiPaymentMethodsEFTSPartyId;
            command.Currency = string.Empty;
            command.BankRoutingNumber = string.Empty;
            command.BankAccountNumber = string.Empty;
            command.BankAccountType = string.Empty;
            command.BankName = string.Empty;
            command.AccountName = string.Empty;
            //command.IsBillingDefault = true;
            //command.Data = null;
            //command.DynamicMessage = string.Empty;
            command.KeyList = new List<Trupanion.TruFoundation.EnterpriseCatalog.DynamicEntityKey>();
            //command.MessageInfo = string.Empty;
            command.OriginatingUserId = Guid.Empty;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed of command is empty");
            Assert.IsTrue(etfsResult.Message.Contains("Supplied string must be non-null and non-whitespace"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_emptyPartyId()
        {
            command.PartyId = Guid.Empty;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when parityid is empty");
            Assert.IsTrue(etfsResult.Message.Contains("PartyId is required."), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_notExistPartyId()
        {
            command.PartyId = Guid.NewGuid();
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when parityid is not exist");
            Assert.IsTrue(etfsResult.Message.Contains("Unable to retrieve 'LocalAccount' entity by specified query"), $"unexpectedmessage - {etfsResult.Message}");
        }

        //[TestMethod]
        // not appliable for no other validations
        public async Task PaymentMethodsETFS_othersPartyId()
        {
            command.PartyId = Guid.Parse("8AA9B7DA-8C55-4D09-9E22-212C2F5E1C71");
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when parityid is not exist");
            Assert.IsTrue(etfsResult.Message.Contains("Unable to retrieve 'LocalAccount' entity by specified query"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_nullCurency()
        {
            command.Currency = null;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when currency is null");
            Assert.IsTrue(etfsResult.Message.Contains("Supplied string must be non-null and non-whitespace"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_emptyCurency()
        {
            command.Currency = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when currency is empty");
            Assert.IsTrue(etfsResult.Message.Contains("Supplied string must be non-null and non-whitespace"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_notSupportedCurency()
        {
            command.Currency = "YUAN";
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when currency doesn't supported");
            Assert.IsTrue(etfsResult.Message.Contains("Currency with symbol 'Trupanion.TruFoundation.Domain.StringNonWhiteSpace' not supported"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_nullBankRoutingNumber()
        {
            command.BankRoutingNumber = null;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when routing number is null");
            Assert.IsTrue(etfsResult.Message.Contains("Routing number is required."), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task PaymentMethodsETFS_emptyBankRoutingNumber()
        {
            command.BankRoutingNumber = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when routing number is empty");
            Assert.IsTrue(etfsResult.Message.Contains("Routing number is required."), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_longerBankRoutingNumber()
        {
            command.BankRoutingNumber += "518";
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when routing number is longer");
            Assert.IsTrue(etfsResult.Message.Contains("Routing number must be 9 digits long"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_shorterBankRoutingNumber()
        {
            command.BankRoutingNumber = command.BankRoutingNumber.Substring(1);
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when routing number is shorter");
            Assert.IsTrue(etfsResult.Message.Contains("Routing number must be 9 digits long"), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_badBankRoutingNumber()
        {
            command.BankRoutingNumber = "518 main st";
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when routing number is shorter");
            Assert.IsTrue(etfsResult.Message.Contains("Routing number must be 9 digits long"), $"unexpectedmessage - {etfsResult.Message}");
        }

        // invalid bank account type
        [TestMethod]
        public async Task PaymentMethodsETFS_nullBankType()
        {
            command.BankAccountType = null;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when bank type is null");
            Assert.IsTrue(etfsResult.Message.Contains("Bank account type '{0}' is invalid."), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod]
        public async Task PaymentMethodsETFS_emptyBankType()
        {
            command.BankAccountType = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when bank type is empty");
            Assert.IsTrue(etfsResult.Message.Contains("Bank account type '{0}' is invalid."), $"unexpectedmessage - {etfsResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task PaymentMethodsETFS_invalidBankType()
        {
            // not supported
            command.BankAccountType = "bitcoin";
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when bank type doesn't supported");
            Assert.IsTrue(etfsResult.Message.Contains("Bank account type '{0}' is invalid."), $"unexpectedmessage - {etfsResult.Message}");
        }


        //
        // skipped parameters which alway success
        //

        // invalid paymentmethodid, other account
        //[TestMethod]
        //public async Task PaymentMethodsETFS_invalidPaymentMethodId()
        //{
        //    // null
        //    command.PaymentMethodId = null;
        //    request.Content = JsonSerializer.Serialize(command);
        //    etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
        //    Assert.IsFalse(etfsResult.Success, $"successed to restclient get from null command");
        //    Assert.IsTrue(etfsResult.Message.Contains("Value cannot be null"), $"unexpectedmessage - {etfsResult.Message}");
        //    // empty
        //    command.PaymentMethodId = Guid.Empty;
        //    request.Content = JsonSerializer.Serialize(command);
        //    etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
        //    Assert.IsFalse(etfsResult.Success, $"successed to restclient get from null command");
        //    Assert.IsTrue(etfsResult.Message.Contains("Value cannot be null"), $"unexpectedmessage - {etfsResult.Message}");
        //    // not supported
        //    command.PaymentMethodId = Guid.NewGuid();
        //    request.Content = JsonSerializer.Serialize(command);
        //    etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
        //    Assert.IsFalse(etfsResult.Success, $"successed to restclient get from null command");
        //    Assert.IsTrue(etfsResult.Message.Contains("Value cannot be null"), $"unexpectedmessage - {etfsResult.Message}");
        //}

        //[TestMethod]
        public async Task PaymentMethodsETFS_invalidData()
        {
            // null
            command.Data = null;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when OriginatingUserId is empty");
            Assert.IsTrue(etfsResult.Message.Contains("none"), $"unexpectedmessage - {etfsResult.Message}");
            // empty
            // bad
        }

        //[TestMethod]
        public async Task PaymentMethodsETFS_invalidKeyList()
        {
            // null
            command.KeyList = null;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when OriginatingUserId is empty");
            Assert.IsTrue(etfsResult.Message.Contains("none"), $"unexpectedmessage - {etfsResult.Message}");
            // empty
            // bad
        }

        //[TestMethod]
        public async Task PaymentMethodsETFS_emptyOriginatingUserId()
        {
            command.OriginatingUserId = Guid.Empty;
            request.Content = JsonSerializer.Serialize(command);
            etfsResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(etfsResult.Success, $"successed when OriginatingUserId is empty");
            Assert.IsTrue(etfsResult.Message.Contains("none"), $"unexpectedmessage - {etfsResult.Message}");
            // empty
            // bad
            // other account
        }

    }
}