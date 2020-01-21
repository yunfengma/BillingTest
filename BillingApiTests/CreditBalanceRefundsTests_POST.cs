//----------------------------------------------------------------------------------------------------------
// <copyright file="CreditBalanceRefundsTests_POST.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingApiTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Test.BillingApiTests;
    using Trupanion.TruFoundation;
    using Trupanion.TruFoundation.RestClient.Async;


    [TestClass]
    public class CreditBalanceRefundsTests_POST : BillingApiTestBase
    {
        private RestResult cbrResult { get; set; }
        private RefundCreditBalanceCommand command { get; set; }
        private OperationResponse cbrResponse { get; set; }


        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            request.Verb = HttpMethod.Post;
            request.RequestUri = $"v2/creditbalancerefunds";

            command = new RefundCreditBalanceCommand();
            command.AccountId = BillingApiTestSettings.Default.BillingServiceApiAccountExternalId.ToString();
        }


        [TestMethod, TestCategory("BVT")]
        public async Task CreditBalanceRefunds_success()
        {
            request.Content = JsonSerializer.Serialize(command);
            cbrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsTrue(cbrResult.Success, $"failed to restclient get from billing service");
            // TODO: verifying in db?
        }

        [TestMethod]
        public async Task CreditBalanceRefunds_nullCommand()
        {
            command = null;
            request.Content = JsonSerializer.Serialize(command);
            cbrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(cbrResult.Success, $"successed");
            Assert.IsTrue(cbrResult.Message.Contains(@"Value cannot be null."), $"unexpected message - {cbrResult.Message}");
        }

        [TestMethod]
        public async Task CreditBalanceRefunds_empryCommand()
        {
            command = new RefundCreditBalanceCommand();
            request.Content = JsonSerializer.Serialize(command);
            cbrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(cbrResult.Success, $"successed");
            Assert.IsTrue(cbrResult.Message.Contains(@"Account id must be supplied and non-empty."), $"unexpected message - {cbrResult.Message}");
        }

        [TestMethod]
        public async Task CreditBalanceRefunds_nullAccountId()
        {
            command.AccountId = null;
            request.Content = JsonSerializer.Serialize(command);
            cbrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(cbrResult.Success, $"successed for null account id");
            Assert.IsTrue(cbrResult.Message.Contains(@"Account id must be supplied and non-empty."), $"unexpected message - {cbrResult.Message}");
        }

        [TestMethod]
        public async Task CreditBalanceRefunds_emptyAccountId()
        {
            command.AccountId = string.Empty;
            request.Content = JsonSerializer.Serialize(command);
            cbrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(cbrResult.Success, $"successed for empty account id");
            Assert.IsTrue(cbrResult.Message.Contains(@"Account id must be supplied and non-empty."), $"unexpected message - {cbrResult.Message}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task CreditBalanceRefunds_nonExistAccountId()
        {
            command.AccountId = $"518{BillingApiTestSettings.Default.BillingServiceApiAccountExternalId.ToString()}";
            request.Content = JsonSerializer.Serialize(command);
            cbrResult = await asyncRestClientBilling.ExecuteAsync<string>(request);
            Assert.IsFalse(cbrResult.Success, $"successed for not exist account id");
            Assert.IsTrue(cbrResult.Message.Contains(@"Unable to find local account by externalId"), $"unexpected message - {cbrResult.Message}");
        }


    }
}