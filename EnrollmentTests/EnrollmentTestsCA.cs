//----------------------------------------------------------------------------------------------------------
// <copyright file="EnrollmentTestsCA.cs" company="Trupanion">
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
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;


    [TestClass]
    public class EnrollmentTestsCA : BillingTestBase
    {
        private BillingDataVerifiers billingDataVerifiers { get; set; }
        private BillingRestClient billingRestClient { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            billingDataVerifiers = new BillingDataVerifiers();
            billingRestClient = new BillingRestClient();

            accountExpected.Currency = "CAD";
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod, TestCategory("BVT")]
        public async Task CAEnrollmentOnePetNoRiderBank()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1);                  // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod]
        public async Task CAEnrollmentOnePetWithBankSavingAccount()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));  // get test data
            iep.BillingParams.BankAccountAccountType = BankAccountType.Saving;
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);                          // verify billing account
        }

        [TestMethod]
        public async Task CAEnrollmentOnePetWithTwoRiderBank()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: 2);  // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                       // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod]
        public async Task CAEnrollmentTwoPetsBank()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 2, riderNumber: random.Next(1,2));       // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                                  // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                           // enroll with service standard enroll
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);                              // verify billing account
        }

        [TestMethod]
        public async Task CAAddAPetInEnrolledPolicySkipPayment()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));      // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                                  // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                           // enroll with service standard enroll
            PetParameters petParams = testDataManager.GetPetParameter(iep.PostalCode);                                          // another pet
            int petId = testDataManager.AddPetSkipPayment(ownerId, petParams);
            Assert.IsTrue(petId > 0, $"failed to add pet for owner - {ownerId}. {petParams}");
            iep.Pets.Add(petParams);                                                                                            // add
            quote = await qaLibRestClient.CreateQuote(iep);
            // TODO - no new invoice in billing
            //System.Threading.Thread.Sleep(3000);                                                                              // waiting for back end processes
            //await BillingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);                            // verify billing account
        }

        [TestMethod]
        public async Task CAAddAPetInEnrolledPolicyCollectPayment()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));      // get test data
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                                  // get quote
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                           // enroll with service standard enroll
            PetParameters petParams = testDataManager.GetPetParameter(iep.PostalCode);                                          // another pet
            int petId = testDataManager.AddPetSkipPayment(ownerId, petParams);
            Assert.IsTrue(petId > 0, $"failed to add pet for owner - {ownerId}. {petParams}");
            iep.Pets.Add(petParams);                                                                                            // add
            // TODO - no new invoice in billing
            //System.Threading.Thread.Sleep(3000);                                                                              // waiting for back end processes
            //await BillingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);                            // verify billing account
        }

        [TestMethod, TestCategory("BVT")]
        public async Task CACancelAFullEnrolledPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));                                  // get test data
            iep.EffectiveDate = DateTime.Now.AddDays(-7);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                                                       // enroll with service standard enroll
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes
            List<InvoiceItem> invoices = await billingDataVerifiers.GetAccountInvoiceItemsByOwnerId(ownerId);                                               // save invoice items

            bool bCanceled = testDataManager.CancelPolicy(ownerId, iep.Pets.First().PetName);                                                               // cancel pet
            Assert.IsTrue(bCanceled, $"failed to cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.Cancelled);           // verify pet has been canceled

            await billingDataVerifiers.VerifyCanceledPetBillingInfo(ownerId, iep, invoices, accountExpected);                                                  // verify billing account
        }

        [TestMethod]
        public async Task CAPendingCancelAEnrolledPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));                                  // get test data
            iep.EffectiveDate = DateTime.Now.AddDays(-7);
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                                                       // enroll with service standard enroll
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes

            bool bCanceled = testDataManager.PendingCancelPolicy(ownerId, iep.Pets.First().PetName);                                                        // pending cancel pet
            Assert.IsTrue(bCanceled, $"failed to peding cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.PendingCancellation); // verify pet has been canceled

            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                                                              // get quote
            await billingDataVerifiers.VerifyPendingCanceledPetBillingInfo(ownerId, iep.Pets.First().PetName, iep, quote, accountExpected);                 // verify billing account
        }

    }
}
