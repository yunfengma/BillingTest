//----------------------------------------------------------------------------------------------------------
// <copyright file="EnrollmentTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.EnrollmentTests
{
    using global::EnrollmentTests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.BankAccounts.V2;
    using Trupanion.Billing.Api.PaymentMethods.V2;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.Logging;

    [TestClass]
    public class BillingPaymentTests : BillingTestBase
    {
        private static ILog log = Logger.Log;

        private BillingDataVerifiers billingDataVerifiers { get; set; }
        private BillingRestClient billingRestClient { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            billingDataVerifiers = new BillingDataVerifiers();
            billingRestClient = new BillingRestClient();

            accountExpected.Currency = "USD";
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod, TestCategory("BVT"), TestCategory("Testing")]
        public async Task GetProducts()
        {
            log.Info($"\t\tGetProducts()");
            var pay = await billingRestClient.GetProducts();
            Assert.IsTrue(pay?.Count > 0, $"failed to get product");
            Assert.IsTrue(pay.Count == 4, $"product number is not as expected {pay.Count}");
            Assert.IsTrue(pay[0].Name.Equals("Pet Insurance"), $"pet insurance is not as expected {pay[0]}");
        }

        [TestMethod]
        public async Task GetPaymentMethodTypesTest()
        {
            var respond = await billingRestClient.GetPaymentTypes();
            Assert.IsTrue(respond?.Count > 0, $"failed to get payment method types");
            Assert.IsTrue(respond.Count == 5, $"payment method types number is not the expected {respond.Count}");
            Assert.IsTrue(respond[0].Name.Equals("CreditCard"), $"credit card payment is not the expected {respond[0]}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetBankAccount()
        {
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            BankAccount respond = await billingRestClient.GetBankAccouintsByOwnerId(ownerId);
            Assert.IsNotNull(respond, $"failed to get bank account");
            Assert.IsTrue(respond.NameOnAccount.Equals(EnrollTestSettings.Default.AccountUpdateTestAccountNameOnAccount), $"bank account name is not the expected {respond.NameOnAccount}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task SaveUsaBankAccount()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);
            BillingParameters billingParams = iep.BillingParams;
            SaveBankAccountResponse respond = await billingRestClient.PostSaveUsaBankAccount(ownerId, billingParams);
            Assert.IsNotNull(respond, $"failed to save bank account");
            BankAccount bAccount = await billingRestClient.GetBankAccouintsByOwnerId(ownerId);

            //var acount = qaLibRestClient.GetBillingAccountByOwnerId(ownerId);
        }

        [TestMethod]
        public async Task PaymentMethodsEtfs()
        {
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);
            BillingParameters billingParams = iep.BillingParams;
            PaymentMethod respond = await billingRestClient.PostPaymentMethodsETFS(ownerId, billingParams);
            Assert.IsNotNull(respond, $"failed to update payment methods");
            // TODO: verify payment methods updated
        }

        //[TestMethod]
        //public async Task GetBankAccountTypesTest()
        //{
        //    var respond = await billingRestClient.GetBankAccountTypes();
        //    Assert.IsTrue(respond?.Count > 0, $"failed to get bank account types");
        //    Assert.IsTrue(respond.Count == 2, $"bank account types number is not the expected {respond.Count}");
        //    Assert.IsTrue(respond[0].Name.Equals("Checking"), $"bank account type name of checking is not the expected {respond[0]}");
        //    Assert.IsTrue(respond[1].Name.Equals("Saving"), $"bank account type name of saving is not the expected {respond[1]}");
        //}

    }
}
