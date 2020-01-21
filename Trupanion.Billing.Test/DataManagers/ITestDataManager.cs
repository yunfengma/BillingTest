//----------------------------------------------------------------------------------------------------------
// <copyright file="ITestDataManager.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Enrollment;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.Services;

    public interface ITestDataManager : IBusinessService
    {
        EnrollmentParameters GenerateOwnerPetData(int numPets = 1);
        EnrollmentParameters GenerateOwnerPetTestData(string countryCode = "US", int numPets = 1, int riderNumber = 0);
        EnrollmentParameters GenerateUSOwnerPetData(int numPets = 1, int riderNumber = 0);
        IEnrollmentParameters CreateOwnerWithPet(EnrollmentType enrollType, int numPets = 1);
        IEnrollmentParameters GenerateTrialOwnerWithPet(EnrollmentType enrollType, string countryCode = "US", int numPets = 1, int riderNumber = 0);
        EnrollmentParameters GenerateCAOwnerPetData(int numPets = 1, int riderNumber = 0);
        EnrollmentParameters GenerateAUOwnerPetData(int numPets = 1, int riderNumber = 0);
        EnrollmentParameters GenerateOwnerPetWithRiderData(int numPets = 1);
        EnrollmentParameters GenerateOwnerPetWithTwoRiderData(int numPets = 1);
        EnrollmentParameters SetupBillingCreditCard(EnrollmentParameters input);
        int DoStandardEnrollmentReturnOwnerCollection(EnrollmentParameters iep);
        int DoTrialEnrollmentReturnOwnerCollection(EnrollmentParameters iep);
        int DoEnrollTrial(IEnrollmentParameters iiep);
        OwnerCollection GetEnrolledOwnerCollection(int ownerId);
        PetParameters GetPetParameter(string zip);
        int AddPetCollectPayment(int ownerId, PetParameters pet, EnrollmentParameters fiep);
        int AddPetSkipPayment(int ownerId, PetParameters pet);
        int AddATrialPet(int ownerId, PetParameters pet);
        PetParameters GetTrialPetParameter(string zip, int stateId);
        bool ConvertTrial(int ownerId, IEnrollmentParameters iep);
        FullEnrollmentParameters ConverteFullEnrollmentParametersFromEnrollmentParameters(EnrollmentParameters iep);
        EnrollmentParameters ConvertEnrollmentParametersFromIEnrollmentParameters(IEnrollmentParameters iep);
        bool CancelPolicy(int ownerId, string petName);
        bool PendingCancelPolicy(int ownerId, string petName);
        Task<QaLibQuoteResponse> CreateQuote(EnrollmentParameters ep);
    }
}
