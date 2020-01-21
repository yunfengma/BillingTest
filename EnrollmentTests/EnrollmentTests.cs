//----------------------------------------------------------------------------------------------------------
// <copyright file="EnrollmentTests.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.EnrollmentTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.Billing.Api.Subscriptions.V1;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.Logging;

    [TestClass]
    public class EnrollmentTests : BillingTestBase
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
        public async Task USEnrollmentOnePetNoRiderBank()
        {
            log.Info($"\t\tUSEnrollmentOnePetNoRiderBank()");
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);                                     // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod]
        public async Task USEnrollmentOnePetWithBankSavingAccount()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));     // get test data
            iep.BillingParams.BankAccountAccountType = BankAccountType.Saving;
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod]
        public async Task USEnrollmentOnePetWithTwoRiderBank()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: 2);                     // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod]
        public async Task USEnrollmentTwoPetsBank()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 2, riderNumber: random.Next(0, 2));     // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod]
        public async Task USAddAPetInEnrolledPolicySkipPayment()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));     // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            PetParameters petParams = testDataManager.GetPetParameter(iep.PostalCode);                      // another pet
            int petId = testDataManager.AddPetSkipPayment(ownerId, petParams);
            Assert.IsTrue(petId > 0, $"failed to add pet for owner - {ownerId}. {petParams}");
            iep.Pets.Add(petParams);                                                                        // add
            quote = await qaLibRestClient.CreateQuote(iep);
            // TODO - no new invoice in billing
            //System.Threading.Thread.Sleep(3000);                                                          // waiting for back end processes
            //await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);        // verify billing account
        }

        [TestMethod]
        public async Task USAddAPetInEnrolledPolicyCollectPayment()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));     // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            PetParameters petParams = testDataManager.GetPetParameter(iep.PostalCode);                      // another pet
            int petId = testDataManager.AddPetCollectPayment(ownerId, petParams, iep);
            Assert.IsTrue(petId > 0, $"failed to add pet for owner - {ownerId}. {petParams}");
            iep.Pets.Add(petParams);                                                                        // add
            quote = await qaLibRestClient.CreateQuote(iep);
            // TODO - no new invoice in billing
            //System.Threading.Thread.Sleep(3000);                                                          // waiting for back end processes
            //await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);        // verify billing account
        }

        [TestMethod, TestCategory("BVT")]
        public async Task USCancelAFullEnrolledPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));                                                     // get test data
            iep.EffectiveDate = DateTime.Now.AddDays(-7);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                                                       // enroll with service standard enroll
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes
            List<InvoiceItem> invoices = await billingDataVerifiers.GetAccountInvoiceItemsByOwnerId(ownerId);                                               // save invoice items

            bool bCanceled = testDataManager.CancelPolicy(ownerId, iep.Pets.First().PetName);                                                               // cancel pet
            Assert.IsTrue(bCanceled, $"failed to cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.Cancelled);           // verify pet has been canceled

            await billingDataVerifiers.VerifyCanceledPetBillingInfo(ownerId, iep, invoices, accountExpected);                                               // verify billing account
        }

        [TestMethod]
        public async Task USPendingCancelAEnrolledPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));                                                     // get test data
            iep.EffectiveDate = DateTime.Now.AddDays(-7);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                                                       // enroll with service standard enroll
            System.Threading.Thread.Sleep(3000);                                                                                                            // waiting for back end processes

            bool bCanceled = testDataManager.PendingCancelPolicy(ownerId, iep.Pets.First().PetName);                                                        // pending cancel pet
            Assert.IsTrue(bCanceled, $"failed to peding cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.PendingCancellation); // verify pet has been canceled

            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                                                              // get quote
            await billingDataVerifiers.VerifyPendingCanceledPetBillingInfo(ownerId, iep.Pets.First().PetName, iep, quote, accountExpected);                 // verify billing account
        }


        //[TestMethod]
        public async Task USFullEnrollmentOnePetWithBankAccountPlusCharity()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));     // get test data
            iep.BillingParams.CharityId = 1;
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        #region tryings
        //[TestMethod]
        [TestCategory("Testing"), TestCategory("BVT")]
        public async Task FullEnrollmentUSCreditCard()
        {
            // enroll
            EnrollmentParameters iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1);         // get test data
            EnrollmentParameters iepc = testDataManager.SetupBillingCreditCard(iep);    // setup credit card

            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);         // get quote

            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);
            // verify billing account
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);
        }


        //[TestMethod]
        public async Task GettingPremium()
        {
            EnrollmentParameters iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);         // get test data
            decimal premium = await qaLibRestClient.GetPremium(iep);
            EnrollmentParameters iep1 = iep;
            foreach(PetParameters p in iep1.Pets)
            {
                p.Riders = null;
            }
            decimal premium1 = await qaLibRestClient.GetPremium(iep1);
            Assert.IsTrue(premium > 0, "");
        }

        //[TestMethod]
        public async Task GetEnginVersionForState()
        {
            EnrollmentParameters iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);
            var versionId = await qaLibRestClient.GetEnginVersionForState(iep.StateId);
            var pVersionId = await qaLibRestClient.GetPolicyVersionForState(iep.StateId);
        }

        //[TestMethod]
        public async Task CreateQuote()
        {
            EnrollmentParameters iep = testDataManager.GenerateOwnerPetTestData(numPets: 1);         // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);
            Assert.IsNotNull(quote, "");
        }

        //[TestMethod]
        public async Task GetBillingSubscriptions()
        {
            SubscriptionFilterCriteria critieria = new SubscriptionFilterCriteria();
            IReadOnlyList<Account> accounts = await billingRestClient.GetBillingAccountByPolicyHolderId("DB7DB197-8E81-4825-A977-9071D5BE6FBE");
            foreach(Account acc in accounts)
            {
                critieria.AccountId = acc.Id;
                break;
            }
            critieria.PetId = 773236;
            ZuoraAccess zuoraAccess = new ZuoraAccess();
            var sub = await zuoraAccess.GetSubscription(critieria);
            //Assert.IsNotNull(quote, "");
        }
        #endregion


        [TestMethod, TestCategory("BVT")]
        public async Task GetInbvoices()
        {
            iep = testDataManager.GenerateOwnerPetTestData(numPets: 1, riderNumber: random.Next(0, 2));                                                     // get test data
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                                                       // enroll with service standard enroll
            //var invoices = billingDataVerifiers.GetOwnerInvoices(ownerIdd);
            //var invoices = await billingDataVerifiers.GetAccountByOwnerId(ownerId);
            List <InvoiceItem> invoices = await billingDataVerifiers.GetAccountInvoiceItemsByOwnerId(ownerId);
        }

    }
}
