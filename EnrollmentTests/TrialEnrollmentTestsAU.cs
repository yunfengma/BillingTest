//----------------------------------------------------------------------------------------------------------
// <copyright file="TrialEnrollmentTestsAU.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.EnrollmentTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;


    [TestClass]
    public class TrialEnrollmentTestsAU : BillingTestBase
    {
        private BillingDataVerifiers billingDataVerifiers { get; set; }
        private BillingRestClient billingRestClient { get; set; }

        [TestInitialize]
        public void Init()
        {
            InitTestClass();

            billingDataVerifiers = new BillingDataVerifiers();
            billingRestClient = new BillingRestClient();

            accountExpected.Currency = "AUD";
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public async Task AUTrialEnrollmentOnePet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "AU", numPets: 1);                  // test data for trial
            iep.EnrollmentTypeVal = EnrollmentType.IssueCertificate;                                        // making it a trial
            iep.BillingParams = null;
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                          // trial enroll
            System.Threading.Thread.Sleep(3000);                                                            // waiting for back end processes
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);       // verify billing account not exist
        }

        [TestMethod, TestCategory("BVT")]
        public async Task AUAddATrialPetToTrial()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "AU", numPets: 1);                  // test data for trial
            iep.EnrollmentTypeVal = EnrollmentType.IssueCertificate;                                        // making it a trial
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                          // trial enroll
            PetParameters petParams = testDataManager.GetTrialPetParameter(iep.PostalCode, iep.StateId);    // get new pet
            int petId = testDataManager.AddATrialPet(ownerId, petParams);                                   // adding to trial policy
            System.Threading.Thread.Sleep(3000);                                                            // waiting for back end processes
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);       // verify billing account not exist
        }

        [TestMethod]
        public async Task AUCancelTrialPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "AU", numPets: 1, riderNumber: random.Next(0, 2));                          // get test data
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                                                                  // trial enroll
            System.Threading.Thread.Sleep(10000);

            bool bCanceled = testDataManager.CancelPolicy(ownerId, iep.Pets.First().PetName);                                                       // cancel pet
            Assert.IsTrue(bCanceled, $"failed to cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(10000);                                                                                                    // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.Cancelled);   // verify pet has been canceled
            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);                                               // verify billing account not exist
        }

        [TestMethod]
        public async Task AUPendingCancelTrialPet()
        {
            iep = testDataManager.GenerateOwnerPetTestData(countryCode: "AU", numPets: 1, riderNumber: random.Next(0, 2));                                  // get test data
            ownerId = testDataManager.DoTrialEnrollmentReturnOwnerCollection(iep);                                                                          // trial enroll
            System.Threading.Thread.Sleep(3000);

            bool bCanceled = testDataManager.PendingCancelPolicy(ownerId, iep.Pets.First().PetName);                                                        // pending cancel pet
            Assert.IsTrue(bCanceled, $"failed to pending cancel pet - {iep.Pets.First().PetName} from owner's policy (ownerid = {ownerId})");
            System.Threading.Thread.Sleep(5000);                                                                                                            // waiting for back end processes

            bCanceled = await billingDataVerifiers.verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName, EnrollmentStatus.PendingCancellation); // verify pet has been canceled

            await billingDataVerifiers.VerifyBillingAccount(ownerId, iep, null, null, bExist: false);                                                       // verify billing account not exist
        }

    }
}
