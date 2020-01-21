//----------------------------------------------------------------------------------------------------------
// <copyright file="TrialEnrollmentTestsCA.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.EnrollmentTests
{
    using BillingTestCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;


    [TestClass]
    public class TrialEnrollmentTestsCA : BillingTestBase
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

        [TestMethod]
        public async Task CATrialEnrollmentOnePet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1);                  // test data for trial
            iep.EnrollmentTypeVal = EnrollmentType.IssueCertificate;                                        // making it a trial
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                          // trial enroll
            System.Threading.Thread.Sleep(3000);                                                            // waiting for back end processes
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);       // verify billing account not exist
        }

        [TestMethod]
        public async Task CAAddATrialPetToTrial()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1);                  // test data for trial
            iep.EnrollmentTypeVal = EnrollmentType.IssueCertificate;                                        // making it a trial
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                          // trial enroll
            PetParameters petParams = testDataManager.GetTrialPetParameter(iep.PostalCode, iep.StateId);    // get new pet
            int petId = testDataManager.AddATrialPet(ownerId, petParams);                                   // adding to trial policy
            System.Threading.Thread.Sleep(3000);                                                            // waiting for back end processes
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);       // verify billing account not exist
        }

        [TestMethod]
        public async Task CACancelTrialPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));                          // get test data
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                                                                  // trial enroll
            System.Threading.Thread.Sleep(10000);

            bool bCanceled = testDataManager.CancelPolicy(ownerId, iep.Pets.First().PetName);                                                       // cancel pet
            Assert.IsTrue(bCanceled, $"failed to cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                    // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.Cancelled);   // verify pet has been canceled
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);                                               // verify billing account not exist
        }

        [TestMethod]
        public async Task CAPendingCancelTrialPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));                                  // get test data
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                                                                          // trial enroll
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes
            
            bool bCanceled = testDataManager.PendingCancelPolicy(ownerId, iep.Pets.First().PetName);                                                        // pending cancel pet
            Assert.IsTrue(bCanceled, $"failed to pending cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                            // waiting for back end processes
            
            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.PendingCancellation); // verify pet has been canceled

            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);                                                       // verify billing account not exist
        }

        [TestMethod, TestCategory("BVT")]
        public async Task CAAddATrialPetInFullEnrollment()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1, riderNumber: random.Next(0, 2));  // get test data
            ownerId = testDataManager.DoStandardEnrollmentReturnOwnerCollection(iep);                                       // enroll with service standard enroll
            PetParameters petParams = testDataManager.GetTrialPetParameter(iep.PostalCode, iep.StateId);                    // another pet
            int petId = testDataManager.AddATrialPet(ownerId, petParams);                                                   // adding to trial policy
            System.Threading.Thread.Sleep(3000);                                                                            // waiting for back end processes
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                              // get quote
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);                          // verify billing account
        }

        [TestMethod]
        public async Task CAAddAEnrollmentPetInTrialPolicy()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1);
            iep.EnrollmentTypeVal = EnrollmentType.IssueCertificate;
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                          // trial enroll
            var iep2 = testDataManager.GenerateOwnerPetTestData(countryCode: "CA", numPets: 1);
            iep.BillingParams = iep2.BillingParams;
            PetParameters petParams = testDataManager.GetPetParameter(iep.PostalCode);                      // another pet
            petParams.PromoCode = BillingTestCommonSettings.Default.PromoCodeUsaCanada;
            int petId = testDataManager.AddPetCollectPayment(ownerId, petParams, iep);
            System.Threading.Thread.Sleep(3000);                                                            // waiting for back end processes
            iep.Pets.Remove(iep.Pets.First());
            iep.Pets.Add(petParams);
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                              // get quote
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, quote, accountExpected);          // verify billing account
        }

        [TestMethod, TestCategory("BVT")]
        public async Task CATrialConvertOnePet()
        {
            bool bConvert = false;
            IEnrollmentParameters iiep = testDataManager.GenerateTrialOwnerWithPet(EnrollmentType.AddTrial, countryCode: "CA", numPets: 1);                                 // test data for trial
            ownerId = testDataManager.DoEnrollTrial(iiep);                                                                                                                  // trial enroll
            IEnrollmentParameters fullEnrollIep = testDataManager.GenerateTrialOwnerWithPet(EnrollmentType.FullEnrollmentCollectPayment, countryCode: "CA", numPets: 1);
            iiep.BillingParams = fullEnrollIep.BillingParams;
            iiep.EffectiveDate = DateTime.UtcNow.AddDays(-1);
            iiep.EnrollmentTypeVal = EnrollmentType.TrialUpgradeCollectPayment;
            bConvert = testDataManager.ConvertTrial(ownerId, iiep);                                                                                                        // convert
            System.Threading.Thread.Sleep(5000);                                                                                                                            // waiting for back end processes
            Assert.IsTrue(bConvert, $"failed to convert trial pet - ownerId <{ownerId}>; pet <{iiep.Pets.First().PetName}>");
            await billingDataVerifiers.VerifyBillingAccountForConvertedTrialPolicy(ownerId, iiep);                                                                          // verify billing account
        }

        [TestMethod]
        public async Task USTrialConvertTwoWeeksAgoPet()
        {
            bool bConvert = false;
            IEnrollmentParameters iiep = testDataManager.GenerateTrialOwnerWithPet(EnrollmentType.AddTrial, countryCode: "CA", numPets: 1);                                 // test data for trial
            iiep.EffectiveDate = DateTime.UtcNow.AddDays(-15);
            iiep.Pets.First().LastExamDate = DateTime.UtcNow.AddDays(-15);
            iiep.Pets.First().CreatedOn = DateTime.UtcNow.AddDays(-15);
            ownerId = testDataManager.DoEnrollTrial(iiep);                                                                                                                  // trial enroll
            IEnrollmentParameters fullEnrollIep = testDataManager.GenerateTrialOwnerWithPet(EnrollmentType.FullEnrollmentCollectPayment, countryCode: "CA", numPets: 1);
            iiep.BillingParams = fullEnrollIep.BillingParams;
            iiep.BillingParams.BankAccountTransitNumber = iiep.BillingParams.BankAccountTransitNumber.Substring(0, 5);
            iiep.BillingParams.BankAccountBankCode = iiep.BillingParams.BankAccountTransitNumber.Substring(0, 3);
            iiep.EffectiveDate = DateTime.UtcNow.AddDays(-14);
            iiep.EnrollmentTypeVal = EnrollmentType.TrialUpgradeCollectPayment;
            bConvert = testDataManager.ConvertTrial(ownerId, iiep);                                                                                                        // convert
            Assert.IsTrue(bConvert, $"failed to convert trial pet - {iiep.Pets.First().PetName}");
            System.Threading.Thread.Sleep(5000);                                                                                                                            // waiting for back end processed to complete
            iep = testDataManager.ConvertEnrollmentParametersFromIEnrollmentParameters(iiep);
            QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);                                                                                              // get quote
            await billingDataVerifiers.VerifyBillingAccountForConvertedTrialPolicy(ownerId, iep, quote, accountExpected);                                                   // verify billing account
        }

    }
}
