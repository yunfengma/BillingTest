//----------------------------------------------------------------------------------------------------------
// <copyright file="AccountTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.EnrollmentTests
{
    using BillingTestCommon.Models;
    using global::EnrollmentTests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.Billing.Api.Payments.V2;
    using Trupanion.Billing.Api.Refunds.V2;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.TruFoundation.Logging;

    [TestClass]
    public class AccountTests : BillingTestBase
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
        public async Task AccountBalancePayNow_bug161553()
        {
            log.Info($"\t\tAccountBalancePayNow_bug161553()");
            // enroll
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);
            // pay now
            CollectPaymentResponse pay = await billingRestClient.PostPayAccountPaymentCollections(ownerId);
            Assert.IsNotNull(pay?.PaymentId, $"failed to pay balance for owner {ownerId}");
            // verify balance
            await billingDataVerifiers.verifyAccountDueBalance(ownerId, 0.0m);
        }

        [TestMethod, TestCategory("BVT")]
        public async Task ChangeAccountMonthlyBillingDay()
        {
            int payDay = random.Next(1, 28);
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            // change to new day
            bool bUpdate = await billingRestClient.PostAccountUpdatesMonthlyBillingDay(ownerId, payDay);
            Assert.IsTrue(bUpdate, $"failed to change monthly billing day for owner {ownerId}");
            // verify new monthly billing day
            await billingDataVerifiers.verifyAccountMonthlyBillingDay(ownerId, payDay);
        }

        [TestMethod, TestCategory("BVT")]
        public async Task ChangeAccountPaymentMethod()
        {
            bool bUpdate = false;
            BillingPaymentMethodTypes pMethod = BillingPaymentMethodTypes.ACH;
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            // change to ACH
            pMethod = BillingPaymentMethodTypes.ACH;
            bUpdate = await billingRestClient.PostAccountUpdatesChangeDefaultPaymentMethod(ownerId, pMethod);
            Assert.IsTrue(bUpdate, $"failed to change default payment method to ACH for owner {ownerId}");
            await billingDataVerifiers.verifyAccountDefaultPaymentMethod(ownerId, pMethod);
            // change to credit card
            pMethod = BillingPaymentMethodTypes.CreditCard;
            bUpdate = await billingRestClient.PostAccountUpdatesChangeDefaultPaymentMethod(ownerId, pMethod);
            Assert.IsTrue(bUpdate, $"failed to change default payment method to credit card for owner {ownerId}");
            await billingDataVerifiers.verifyAccountDefaultPaymentMethod(ownerId, pMethod);
        }

        [TestMethod]
        public async Task ReschedulePaymenys()
        {
            DateTime dupDate = DateTime.UtcNow.AddDays(10);
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            // change due date
            bool update = await billingRestClient.PostRescheduledPayments(ownerId, dupDate);
            Assert.IsTrue(update, $"failed to change default payment method for owner {ownerId}");
            // verify payment due date == account.BalancePastDueDate is awayls null
            //await billingDataVerifiers.verifyAccountDueDate(ownerId, dupDate);
        }

        [TestMethod]
        public async Task USChangeAccountCharity()
        {
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            // change charity
            int iCharity = Charity.Charities[random.Next(3, 7)].Id;
            //iep.BillingParams.CharityId = iCharity;
            bool bUpdate = await billingRestClient.PostAccountUpdateCharity(ownerId, iCharity);
            Assert.IsTrue(bUpdate, $"failed to change charity for owner {ownerId}");
            // verify payment method
            await billingDataVerifiers.verifyAccountCharity(ownerId, iCharity);
        }

        [TestMethod]
        public async Task UpdateCreditCardExpirationDate()
        {
            DateTime dt = DateTime.UtcNow.AddYears(random.Next(1, 5));
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            // update expiration date 
            bool bUpdate = await billingRestClient.PostExpirationDate(ownerId, dt);
            Assert.IsTrue(bUpdate, $"failed to change charity for owner {ownerId}");
            // verify payment method
            await billingDataVerifiers.verifyAccountCreditCardExpirationDate(ownerId, dt);
        }

        [TestMethod]
        public async Task AccountCreditBalanceRefund_bug161553()
        {
            // enroll
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);
            // pay now
            var pay = await billingRestClient.PostPayAccountPaymentCollections(ownerId);
            Assert.IsNotNull(pay?.PaymentId, $"failed to pay balance for owner {ownerId}");
            // cancel
            bool bCanceled = testDataManager.CancelPolicy(ownerId, iep.Pets.First().PetName);                                                               // cancel pet
            Assert.IsTrue(bCanceled, $"failed to cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            // verify refund
            var refund = await billingRestClient.PostCreditBalanceRefunds(ownerId);
            Assert.IsTrue(refund, $"failed to refund credit balance for owner {ownerId}");
            // TODO - currently not always a good way to verify the amount, billing service may not processed yet due to date
            await billingDataVerifiers.verifyAccountCreditBalance(ownerId, 0.0m);
        }

        [TestMethod]
        public async Task GetAccountPaymentReschedulingRequests()
        {
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            var respond = await billingRestClient.GetAccountPaymentReschedulingRequests(ownerId);
            Assert.IsTrue(respond?.Count > 0, $"failed to get payment reschedule request for owner {ownerId}");
            var pr = respond.Where(i => i.Id == EnrollTestSettings.Default.AccountUpdateTestPaymentRequestId).FirstOrDefault();
            Assert.IsNotNull(pr, $"payment reschedule request is not expected {respond[0]}");
        }

        [TestMethod]
        public async Task InvoicesPreviewTest()
        {
            ownerId = EnrollTestSettings.Default.AccountUpdateTestOwnerId;
            InvoicePreview respond = await billingRestClient.PostInvoicesPreview(ownerId);
            Assert.IsNotNull(respond, $"failed to preview invoice");
            Assert.IsTrue(respond.Items.First().ChargeName.Equals("Pet Insurance Monthly Premium"), $"unexpected item - {respond}");
        }

        [TestMethod, TestCategory("BVT")]
        public async Task GetAccountByOwnerId()
        {
            string accountId = EnrollTestSettings.Default.AccountUpdateTestAccountId;
            Account account = await billingRestClient.GetAccountById(accountId);
            Assert.IsNotNull(account, $"failed to get billing account by account id - {accountId}");
        }

        [TestMethod]
        public async Task GetAccountRefund()
        {
            ownerId = EnrollTestSettings.Default.AccountRefundTestOwnerId;
            List<Refund> refund = await billingRestClient.GetAccountRefund(ownerId);
            Assert.IsTrue(refund?.Count > 0, $"failed to get refund for owner {ownerId}");
            Assert.IsTrue(refund[0].Amount == EnrollTestSettings.Default.AccountRefundTestAmount , $"refund amount <{refund[0].Amount}> is not as the expected {EnrollTestSettings.Default.AccountRefundTestAmount}");
        }

    }
}
