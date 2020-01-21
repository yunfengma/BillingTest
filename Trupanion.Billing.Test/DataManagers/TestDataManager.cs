//----------------------------------------------------------------------------------------------------------
// <copyright file="TestDataManager.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.DataManagers
{
    using BillingTestCommon;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.AccountModification;
    using Trupanion.Test.QALib.DatabaseAccess.Models.Projections.TruDat;
    using Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters;
    using Trupanion.Test.QALib.Enrollment;
    using Trupanion.Test.QALib.OwnerGen;
    using Trupanion.Test.QALib.WebServices.Client.Enrollment;
    using Trupanion.Test.QALib.WebServices.Client.OwnerPetGen;
    using Trupanion.Test.QALib.WebServices.Client.Resolvers.OwnerResolver;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Enrollment;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.IoC;
    using Trupanion.TruFoundation.TestTools;
    using BillingParameters = Trupanion.Test.QALib.WebServices.Contracts.Enrollment.BillingParameters;
    using BillingTestCommon.Models;
    using Trupanion.TruFoundation.Logging;

    public class TestDataManager : BaseIntegrationTests, ITestDataManager
    {
        public static ILog log = Logger.Log;

        public string TestEnvironment = BillingTest.Default.TestEnvironment;
        private const int MaxIterations = 10;

        private IOwnerPetGenService ownerPetService = null;
        private IEnrollmentService enrollmentService = null;
        private IQuoteService quoteService = null;
        private IOwnerResolverService ownerResolver = null;
        private IOwnerPetFactory ownerPetFactory = null;
        private IAddPet addPetFactory = null;
        private IEnrollOwner enrollOwner = null;
        private IPolicyModification policyModification = null;

        private EnrollmentParameters iep { get; set; }
        private SelectedPolicyOption riderA { get; set; }
        private SelectedPolicyOption riderB { get; set; }
        private OwnerCollection ownerCollection { get; set; }
        private IEnrollmentParameters iiep { get; set; }

        private Random random { get; set; }


        public TestDataManager()
        {
            this.ownerPetService = ServiceFactory.Instance.Create<IOwnerPetGenService>();
            this.enrollmentService = ServiceFactory.Instance.Create<IEnrollmentService>();
            this.ownerResolver = ServiceFactory.Instance.Create<IOwnerResolverService>();
            this.addPetFactory = ServiceFactory.Instance.Create<IAddPet>();
            enrollOwner = IocContainer.Resolve<IEnrollOwner>();
            quoteService = IocContainer.Resolve<IQuoteService>();
            ownerPetFactory = IocContainer.Resolve<IOwnerPetFactory>();
            this.policyModification = ServiceFactory.Instance.Create<IPolicyModification>();

            riderA = new SelectedPolicyOption
            {
                Id = 0,
                CoveragePackageId = 17,
                CreatedBy = Guid.Empty,
                CreatedOn = DateTime.UtcNow,
                CreatedOnBehalfOf = null,
                ModifiedBy = Guid.Empty,
                ModifiedOnBehalfOf = null,
                Cost = 11.46M,
                Name = "Recovery and Complementary Care Rider"
            };
            riderB = new SelectedPolicyOption
            {
                Id = 0,
                CoveragePackageId = 19,
                CreatedBy = Guid.Empty,
                CreatedOn = DateTime.UtcNow,
                CreatedOnBehalfOf = null,
                ModifiedBy = Guid.Empty,
                ModifiedOnBehalfOf = null,
                Cost = 4.95M,
                Name = "Rider B"           // "Pet Owner Assistance Package"
            };

            random = new Random();

            Charity.Init();
        }

        public EnrollmentParameters GenerateOwnerPetData(int numPets = 1)
        {
            iep = this.ownerPetService.GetNewOwnerAndPet(numPets);
            return iep;
        }

        public IEnrollmentParameters CreateOwnerWithPet(EnrollmentType enrollType, int numPets = 1)
        {
            return ownerPetFactory.CreateRandomOwnerWithPet(enrollType, numPets);
        }

        public EnrollmentParameters GenerateOwnerPetTestData(string countryCode = "US", int numPets = 1, int riderNumber = 0)
        {
            switch (countryCode.ToLower())
            {
                case "us":
                    iep = GenerateUSOwnerPetData(numPets, riderNumber);
                    break;
                case "ca":
                    iep = GenerateCAOwnerPetData(numPets, riderNumber);
                    iep.BillingParams.BankAccountTransitNumber = iep.BillingParams.BankAccountTransitNumber.Substring(0, 5);
                    iep.BillingParams.BankAccountBankCode = iep.BillingParams.BankAccountTransitNumber.Substring(0, 3);
                    break;
                case "au":
                    iep = GenerateAUOwnerPetData(numPets, riderNumber);
                    break;
                default:
                    throw new Exception($"country is not supported yet - {countryCode}");
            }
            // set billing test enroll last name
            iep.LastName = $"bat{iep.LastName}";
            iep.EffectiveDate = DateTime.UtcNow;
            // riders?
            foreach (PetParameters pet in iep.Pets)
            {
                if (riderNumber > 0)
                {
                    pet.Riders = new System.Collections.Generic.List<SelectedPolicyOption>();
                    pet.Riders.Add(riderA);
                    if (riderNumber > 1)
                    {
                        pet.Riders.Add(riderB);
                    }
                }
                else
                {
                    pet.Riders = null;
                }
                pet.AddPolicyOption(new PolicyOption());
            }
            return iep;
        }

        public IEnrollmentParameters GenerateTrialOwnerWithPet(EnrollmentType enrollType, string countryCode = "US", int numPets = 1, int riderNumber = 0)
        {
            IEnrollmentParameters iiep = null;
            switch (countryCode.ToLower())
            {
                case "us":
                    iiep = GenerateUSTrialOwnerWithPet(enrollType, numPets);
                    break;
                case "ca":
                    iiep = GenerateCATrialOwnerWithPet(enrollType, numPets);
                    if (enrollType == EnrollmentType.FullEnrollmentCollectPayment)
                    {
                        //iiep.BillingParams.BankAccountAccountNumber = BillingTest.Default.CanadaBankAccountNumber;            // back end need this encrypt format
                        iiep.BillingParams.BankAccountTransitNumber = BillingTestCommonSettings.Default.BankAccountNumberCA.Substring(0, 5);
                        iiep.BillingParams.BankAccountBankCode = BillingTestCommonSettings.Default.BankAccountNumberCA.Substring(0, 3);
                    }
                    break;
                case "au":
                    iiep = GenerateAUTrialOwnerWithPet(enrollType, numPets);
                    break;
                default:
                    throw new Exception($"country is not supported yet - {countryCode}");
            }
            // set billing test enroll last name
            iiep.LastName = $"bat{iiep.LastName}";
            iiep.EffectiveDate = DateTime.UtcNow.AddDays(-1);
            // riders?
            foreach (PetParameters pet in iiep.Pets)
            {
                if (riderNumber > 0)
                {
                    pet.Riders = new List<SelectedPolicyOption>();
                    pet.Riders.Add(riderA);
                    if (riderNumber > 1)
                    {
                        pet.Riders.Add(riderB);
                    }
                }
                else
                {
                    pet.Riders = new List<SelectedPolicyOption>();
                }
            }
            return iiep;
        }

        public EnrollmentParameters GenerateUSOwnerPetData(int numPets = 1, int riderNumber = 0)
        {
            bool gotit = false;
            do
            {
                gotit = false;
                iep = this.GenerateOwnerPetData(numPets);
                gotit = iep.StateId <= 59;
            } while (!gotit);
            return iep;
        }

        public EnrollmentParameters GenerateCAOwnerPetData(int numPets = 1, int riderNumber = 0)
        {
            bool gotit = false;
            do
            {
                gotit = false;
                iep = this.GenerateOwnerPetData(numPets);
                gotit = iep.StateId >= 60 && iep.StateId <= 72;
            } while (!gotit);
            return iep;
        }

        public EnrollmentParameters GenerateAUOwnerPetData(int numPets = 1, int riderNumber = 0)
        {
            bool gotit = false;
            do
            {
                gotit = false;
                iep = this.GenerateOwnerPetData(numPets);
                gotit = iep.StateId >= 73;
            } while (!gotit);
            return iep;
        }

        public EnrollmentParameters GenerateOwnerPetWithRiderData(int numPets = 1)
        {
            bool hasRider = false;
            do
            {
                hasRider = false;
                iep = this.ownerPetService.GetNewOwnerAndPet(numPets);
                foreach (PetParameters pet in iep.Pets)
                {
                    hasRider |= pet.HasRiderA || pet.HasRiderB;
                }
            } while (!hasRider);
            iep.LastName = $"bat{iep.LastName}";
            iep.EffectiveDate = DateTime.Now;
            return iep;
        }

        public EnrollmentParameters GenerateOwnerPetWithTwoRiderData(int numPets = 1)
        {
            bool hasRider = false;
            do
            {
                hasRider = false;
                iep = this.ownerPetService.GetNewOwnerAndPet(numPets);
                foreach (PetParameters pet in iep.Pets)
                {
                    hasRider |= pet.HasRiderA && pet.HasRiderB;
                }
            } while (!hasRider);
            iep.LastName = $"bat{iep.LastName}";
            iep.EffectiveDate = DateTime.Now;
            return iep;
        }

        public IEnrollmentParameters GenerateUSTrialOwnerWithPet(EnrollmentType enrollType, int numPets = 1, int riderNumber = 0)
        {
            bool gotit = false;
            do
            {
                gotit = false;
                iiep = this.CreateOwnerWithPet(enrollType, numPets);
                gotit = iiep.StateId <= 59;
            } while (!gotit);
            return iiep;
        }

        public IEnrollmentParameters GenerateCATrialOwnerWithPet(EnrollmentType enrollType, int numPets = 1, int riderNumber = 0)
        {
            bool gotit = false;
            do
            {
                gotit = false;
                iiep = this.CreateOwnerWithPet(enrollType, numPets);
                gotit = iiep.StateId >= 60 && iiep.StateId <= 72;
            } while (!gotit);
            return iiep;
        }

        public IEnrollmentParameters GenerateAUTrialOwnerWithPet(EnrollmentType enrollType, int numPets = 1, int riderNumber = 0)
        {
            bool gotit = false;
            do
            {
                gotit = false;
                iiep = this.CreateOwnerWithPet(enrollType, numPets);
                gotit = iiep.StateId >= 73;
            } while (!gotit);
            return iiep;
        }


        public int DoStandardEnrollmentReturnOwnerCollection(EnrollmentParameters iep)
        {
            int ownerId = 0;
            ownerId = this.enrollmentService.EnrollSingleOwner(iep);
            if (ownerId == 0)
            {
                throw new ApplicationException("Created Owner Id is 0");
            }
            Trace.WriteLine($"Email address: {iep.EMailAddress}");
            return ownerId;
        }

        public int DoTrialEnrollmentReturnOwnerCollection(EnrollmentParameters iep)
        {
            int ownerId = 0;
            iep.BillingParams = new BillingParameters();
            ownerId = this.enrollmentService.TrialEnrollSingleOwner(iep);
            if (ownerId == 0)
            {
                throw new ApplicationException("Created Owner Id is 0");
            }
            Trace.WriteLine($"Email address: {iep.EMailAddress}");
            return ownerId;
        }

        public int DoEnrollTrial(IEnrollmentParameters iiep)
        {
            int ownerId = 0;
            ownerId = this.enrollOwner.EnrollTestOwner(iiep);
            if (ownerId == 0)
            {
                throw new ApplicationException("Created Owner Id is 0");
            }
            Trace.WriteLine($"Email address: {iiep.EMailAddress}");
            return ownerId;
        }

        public async Task<int> DoStandardEnrollmentReturnOwnerCollectionRestClient(EnrollmentParameters iep)
        {
            int ownerId = 0;
            QALibRestClient qalib = new QALibRestClient();
            ownerId = await qalib.EnrollSingleOwner(iep);
            if (ownerId == 0)
            {
                throw new ApplicationException("Created Owner Id is 0");
            }

            Trace.WriteLine($"Email address: {iep.EMailAddress}");
            return ownerId;
        }

        public async Task<QaLibQuoteResponse> CreateQuote(EnrollmentParameters ep)
        {
            QaLibQuoteResponse qr = await quoteService.CreateQuoteAsync(ep);
            return qr;
        }

        public string BillingTestEmailAddress()
        {
            return $"bat{DateTime.Now.ToString("MMddyyyyhhss")}@trupanion.com";
        }

        public string MangleEmailAddress(string emailAddress)
        {
            string mangledAddress = emailAddress.Replace("@", "_");
            mangledAddress = mangledAddress.Replace(".com", ".com@trupanion.com");
            Trace.WriteLine($"Mangled Email address: {mangledAddress}");
            return mangledAddress;
        }

        public OwnerCollection GetEnrolledOwnerCollection(int ownerId)
        {
            OwnerCollection ownerCollection = this.ownerResolver.GetOwnerCollection(ownerId);
            Trace.WriteLine("Owner Id: " + ownerId);
            Trace.WriteLine("Policy number: " + ownerCollection.OwnerInformation.PolicyNumber);
            return ownerCollection;
        }

        public FullEnrollmentParameters ConverteFullEnrollmentParametersFromEnrollmentParameters(EnrollmentParameters iep)
        {
            FullEnrollmentParameters fiep = new FullEnrollmentParameters();
            fiep.Address = iep.Address;
            fiep.Address2 = iep.Address2;
            fiep.BillingParams = new Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters.BillingParameters();
            fiep.BillingParams.BankAccountAccountNumber = iep.BillingParams.BankAccountAccountNumber;
            fiep.BillingParams.BankAccountAccountNumberLast4 = iep.BillingParams.BankAccountAccountNumberLast4;
            fiep.BillingParams.BankAccountAccountType = iep.BillingParams.BankAccountAccountType;
            fiep.BillingParams.BankAccountBankCode = iep.BillingParams.BankAccountBankCode;
            fiep.BillingParams.BankAccountBankName = iep.BillingParams.BankAccountBankName;
            fiep.BillingParams.BankAccountNameOnAccount = iep.BillingParams.BankAccountNameOnAccount;
            fiep.BillingParams.BankAccountTransitNumber = iep.BillingParams.BankAccountTransitNumber;
            fiep.BillingParams.BillingDayOfMonth = iep.BillingParams.BillingDayOfMonth;
            fiep.BillingParams.CharityId = iep.BillingParams.CharityId;
            fiep.BillingParams.ExternalAccountId = string.Empty;
            fiep.BillingParams.PaymentMethod = iep.BillingParams.PaymentMethod;
            fiep.CampaignInstanceId = iep.CampaignInstanceId;
            fiep.City = iep.City;
            fiep.EffectiveDate = DateTime.UtcNow;
            //fiep.EMailAddress = iep.EMailAddress;
            fiep.EMailSuffix = iep.EMailSuffix;
            fiep.EnrollmentTypeVal = iep.EnrollmentTypeVal;
            fiep.FirstName = iep.FirstName;
            fiep.LastName = iep.LastName;
            fiep.LeadId = iep.LeadId;
            fiep.Password = iep.Password;
            fiep.PersonInfo = new Trupanion.Test.QALib.DatabaseAccess.Models.Poco.TestData.OwnerPocos.PersonInfo();
            fiep.Pets = new List<PetParameters>();
            foreach (PetParameters pet in iep.Pets)
            {
                fiep.Pets.Add(pet);
            }
            //fiep.PetCount = iep.PetCount;
            fiep.Platform = iep.Platform;
            fiep.PostalCode = iep.PostalCode;
            fiep.PrimaryPhone = iep.PrimaryPhone;
            fiep.ReferenceNumber = iep.ReferenceNumber;
            fiep.SecondaryPhone = iep.SecondaryPhone;
            fiep.StateId = iep.StateId;
            fiep.WorkPhone = iep.WorkPhone;

            return fiep;
        }

        public EnrollmentParameters ConvertEnrollmentParametersFromIEnrollmentParameters(IEnrollmentParameters iep)
        {
            EnrollmentParameters fiep = new EnrollmentParameters();
            fiep.Address = iep.Address;
            fiep.Address2 = iep.Address2;
            fiep.BillingParams = new BillingParameters();
            fiep.BillingParams.BankAccountAccountNumber = iep.BillingParams.BankAccountAccountNumber;
            fiep.BillingParams.BankAccountAccountNumberLast4 = iep.BillingParams.BankAccountAccountNumberLast4;
            fiep.BillingParams.BankAccountAccountType = iep.BillingParams.BankAccountAccountType;
            fiep.BillingParams.BankAccountBankCode = iep.BillingParams.BankAccountBankCode;
            fiep.BillingParams.BankAccountBankName = iep.BillingParams.BankAccountBankName;
            fiep.BillingParams.BankAccountNameOnAccount = iep.BillingParams.BankAccountNameOnAccount;
            fiep.BillingParams.BankAccountTransitNumber = iep.BillingParams.BankAccountTransitNumber;
            fiep.BillingParams.BillingDayOfMonth = iep.BillingParams.BillingDayOfMonth;
            fiep.BillingParams.CharityId = iep.BillingParams.CharityId;
            fiep.BillingParams.PaymentMethod = iep.BillingParams.PaymentMethod;
            fiep.CampaignInstanceId = iep.CampaignInstanceId;
            fiep.City = iep.City;
            fiep.EffectiveDate = DateTime.UtcNow;
            //fiep.EMailAddress = iep.EMailAddress;
            fiep.EMailSuffix = iep.EMailSuffix;
            fiep.EnrollmentTypeVal = iep.EnrollmentTypeVal;
            fiep.FirstName = iep.FirstName;
            fiep.LastName = iep.LastName;
            fiep.LeadId = iep.LeadId;
            fiep.Password = iep.Password;
            fiep.Pets = new List<PetParameters>();
            foreach (PetParameters pet in iep.Pets)
            {
                fiep.Pets.Add(pet);
            }
            //fiep.PetCount = iep.PetCount;
            fiep.Platform = iep.Platform;
            fiep.PostalCode = iep.PostalCode;
            fiep.PrimaryPhone = iep.PrimaryPhone;
            fiep.ReferenceNumber = iep.ReferenceNumber;
            fiep.SecondaryPhone = iep.SecondaryPhone;
            fiep.StateId = iep.StateId;
            fiep.WorkPhone = iep.WorkPhone;

            return fiep;
        }


        public EnrollmentParameters SetupBillingCreditCard(EnrollmentParameters input)
        {
            EnrollmentParameters iep = input;
            iep.BillingParams.BankAccountAccountNumber = "4111111111111111";
            iep.BillingParams.BankAccountAccountNumberLast4 = "1234";
            //iep.BillingParams.BankAccountAccountType = LegacyPlatform.Constants.BankAccountType.Checking;
            //iep.BillingParams.BankAccountBankCode = string.Empty;
            //iep.BillingParams.BankAccountBankName = "Visa";
            //iep.BillingParams.BankAccountNameOnAccount = "Visa Tester";
            //iep.BillingParams.BankAccountTransitNumber = string.Empty;
            //iep.BillingParams.BillingDayOfMonth = 1;
            //iep.BillingParams.CharityId = 0;
            iep.BillingParams.PaymentMethod = LegacyPlatform.Constants.PaymentMethod.CreditCard;
            return iep;
        }

        public PetParameters GetPetParameter(string zip)
        {
            PetParameters petParams = ownerPetFactory.GeneratePet(zip);
            return petParams;
        }

        public PetParameters GetTrialPetParameter(string zip, int stateId)
        {
            PetParameters petParams = ownerPetFactory.GeneratePet(zip);
            petParams.PromoCode = stateId >= 73 ? BillingTestCommonSettings.Default.PromoCodeAustralia : BillingTestCommonSettings.Default.PromoCodeUsaCanada;
            petParams.LastExamDate = DateTime.UtcNow;
            return petParams;
        }

        public int AddATrialPet(int ownerId, PetParameters pet)
        {
            int ret = 0;
            try
            {
                ret = this.addPetFactory.AddATrialPet(ownerId, pet);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public int AddPetCollectPayment(int ownerId, PetParameters pet, EnrollmentParameters iep)
        {
            int ret = 0;
            IEnrollmentParameters fiep = ConverteFullEnrollmentParametersFromEnrollmentParameters(iep);
            try
            {
                pet.PromoCode = string.Empty;
                ret = this.addPetFactory.AddPetCollectPayment(ownerId, pet, fiep);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public int AddPetSkipPayment(int ownerId, PetParameters pet)
        {
            int ret = 0;
            IEnrollmentParameters fiep = ConverteFullEnrollmentParametersFromEnrollmentParameters(iep);
            try
            {
                pet.PromoCode = string.Empty;
                ret = this.addPetFactory.AddAPetSkipPayment(ownerId, pet);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public bool ConvertTrial(int ownerId, IEnrollmentParameters iep)
        {
            bool ret = false;
            try
            {
                ret = this.enrollOwner.ConvertTrial(ownerId, iep);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public bool CancelPolicy(int ownerId, string petName)
        {
            bool matchPet = false, ret = false;
            int policyId = 0;
            try
            {
                ownerCollection = GetEnrolledOwnerCollection(ownerId);
                foreach (PetAndPolicy pet in ownerCollection.pets)
                {
                    matchPet = pet.Pet.Name.ToLower().Equals(petName.ToLower());
                    if (matchPet)
                    {
                        var response = policyModification.CancelPetPolicy(pet.PetPolicy.Id, DateTime.Today);
                        ret = response?.Count > 0;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public bool PendingCancelPolicy(int ownerId, string petName)
        {
            bool matchPet = false, ret = false;
            int policyId = 0;
            try
            {
                ownerCollection = GetEnrolledOwnerCollection(ownerId);
                foreach (PetAndPolicy pet in ownerCollection.pets)
                {
                    matchPet = pet.Pet.Name.ToLower().Equals(petName.ToLower());
                    if (matchPet)
                    {
                        var response = policyModification.PendingCancelPetPolicy(pet.PetPolicy.Id, DateTime.Today);
                        ret = response?.Count > 0;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }
         
    }
}
