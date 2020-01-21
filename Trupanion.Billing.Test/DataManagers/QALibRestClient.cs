//----------------------------------------------------------------------------------------------------------
// <copyright file="QALibRestClient.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.DataVerifiers
{
    using BillingTestCommon;
    using BillingTestCommon.Methods;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Test.QALib.DatabaseAccess.Models.Poco.TruDat;
    using Trupanion.Test.QALib.DatabaseAccess.Models.Projections.Geography;
    using Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters;
    using Trupanion.Test.QALib.Resolvers;
    using Trupanion.Test.QALib.WebServices.Client;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Poco.Billing;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Poco.TestData.PriceTesting;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.Test.QALib.WebServices.Contracts.Resolvers;
    using Trupanion.TruFoundation.IoC;
    using Trupanion.TruFoundation.Repository;
    using Trupanion.TruFoundation.RestClient.Async;

    public class QALibRestClient : QALibServiceBase
    {
        public IAsyncRestClient asyncRestClientQALib;
        public string qalibBaseUrl = BillingTest.Default.TestEnvironment.ToLower().Equals("test") ? 
            "http://test-qa.services.trupanion.com" : "http://localhost:21301";
        private IEngineVersionResolver stateEngineResolver = null;

        public QALibRestClient()
        {
            var asyncRestClientFactoryQALib = ServiceFactory.Instance.Create<IAsyncRestClientFactory>();
            asyncRestClientQALib = asyncRestClientFactoryQALib.CreateClient(RestClientDefinitionBuilder.Build()
                        .ForServiceUri($"{qalibBaseUrl}")
                        .Create());
            stateEngineResolver = ServiceFactory.Instance.Create<IEngineVersionResolver>();
        }

        public async Task<decimal> GetPremium(EnrollmentParameters iep)
        {
            decimal ret = 0;
            PremiumSprocParameters psp = new PremiumSprocParameters();
            List<PetParameters> pets = (List<PetParameters>)iep.Pets;

            try
            {
                psp.AgeId = (int)pets[0].AgeId;
                psp.BreedId = pets[0].BreedId;
                psp.ClinicId = pets[0].HospitalId == null ? 0 : (int)pets[0].HospitalId;
                psp.CountryId = 0;
                psp.Deductible = pets[0].Deductible;
                psp.Gender = pets[0].Gender == LegacyPlatform.Constants.Gender.Male ? 'M' : 'F';
                psp.IsRenewal = false;
                psp.PetFoodId = 0;
                psp.PolicyVersionId = int.Parse(await GetPolicyVersionForState(iep.StateId));
                psp.PromoCode = pets[0].PromoCode;
                if (pets[0].Riders?.Count > 0)
                {
                    foreach (SelectedPolicyOption r in pets[0].Riders)
                    {
                        psp.SelectedPolicyOptions += $"{r.Name} ";
                        ret += r.Cost;
                    }
                }
                psp.SpayedNeuteredStatus = pets[0].SpayedNeutered ? 1 : 0;
                psp.SpecieId = (int)pets[0].Species;
                psp.UserId = 0;
                psp.VersionId = int.Parse(await new QALibRestClient().GetEnginVersionForState(iep.StateId));
                psp.WorkingPets = (pets[0].IsWorkingPet ? 1 : 0).ToString();
                psp.Zipcode = iep.PostalCode;

                var dec = await GetTruDatPremium(psp);
                ret += System.Math.Round(decimal.Parse(dec), 2);
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
                ret = 0;
            }
            return ret;
        }

        public async Task<string> GetTruDatPremium(PremiumSprocParameters psp)
        {
            string ret = string.Empty;
            try
            {
                var returnPost = await asyncRestClientQALib.PostAsync<string, PremiumSprocParameters>($"/TruDatCalculate/TruDatCalculates/TruDatPremium", psp).ConfigureAwait(false);
                ret = returnPost?.Value;
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<string> GetStateCodeById(int stateId)
        {
            string ret = string.Empty;

            try
            {
                var returnPost = await asyncRestClientQALib.GetAsync<string>($"/QaLibUtilities/Resolvers/StateCodeById/{stateId}");
                ret = returnPost.Value.ToString().Replace("\"", "");
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<string> GetEnginVersionForState(int stateId)
        {
            string ret = string.Empty;
            EngineVersionLookupByStateCodeParameter stateParam = new EngineVersionLookupByStateCodeParameter();

            try
            {
                stateParam.StateCode = await GetStateCodeById(stateId);
                stateParam.EffectiveDate = DateTime.Now;
                var returnPost = await asyncRestClientQALib.PostAsync<string, EngineVersionLookupByStateCodeParameter>($"/QaLibUtilities/Resolvers/GetEngineVersionForState", stateParam)/*.ConfigureAwait(false)*/;
                ret = returnPost.Value.ToString();
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<string> GetPolicyVersionForState(int stateId)
        {
            string ret = string.Empty;
            EngineVersionLookupByStateIdParameter stateParam = new EngineVersionLookupByStateIdParameter();

            try
            {
                stateParam.StateID = stateId;
                stateParam.EffectiveDate = DateTime.Now;
                var returnPost = await asyncRestClientQALib.PostAsync<string, EngineVersionLookupByStateIdParameter>($"QaLibUtilities/Resolvers/GetPolicyVersionForState", stateParam);
                ret = returnPost.Value.ToString();
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<QaLibQuoteResponse> CreateQuote(EnrollmentParameters iep)
        {
            QaLibQuoteResponse ret = new QaLibQuoteResponse();
            try
            {
                var returnPost = await asyncRestClientQALib.PostAsync<string, EnrollmentParameters>($"/Enrollment/Quotes/CreateQuote", iep).ConfigureAwait(false);
                ret = JsonSerializer.Deserialize<QaLibQuoteResponse>(returnPost.Value);
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<int> TrialEnrollOwner(EnrollmentParameters iep)
        {
            int ret = 0;
            try
            {
                var returnPost = await asyncRestClientQALib.PostAsync<string, EnrollmentParameters>($"/Enrollment/EnrollmentParameters/TrialEnrollOwner", iep).ConfigureAwait(false);
                ret = JsonSerializer.Deserialize<int>(returnPost.Value);
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<decimal> GetSetupFeeByStateId(int stateId)
        {
            decimal ret = 0;
            try
            {
                var returnPost = await asyncRestClientQALib.GetAsync<string>($"/QaLibUtilities/Resolvers/GeoRetrieveByStateId/{stateId}").ConfigureAwait(false);
                GeographyStateProjection state = JsonSerializer.Deserialize<GeographyStateProjection>(returnPost.Value);
                ret = state.SetupFee;
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<List<ZipcodeWithCountryId>> GeAllZipsByStateId(int stateId)
        {
            List<ZipcodeWithCountryId> ret = new List<ZipcodeWithCountryId>();
            try
            {
                var returnPost = await asyncRestClientQALib.GetAsync<string>($"QaLibUtilities/Resolvers/GetAllZipsForStateId/{stateId}").ConfigureAwait(false);
                ret = JsonSerializer.Deserialize<List<ZipcodeWithCountryId>>(returnPost.Value);
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }


        public async Task<string> GetAustraliaRandomZipFromStateId(int stateId)
        {
            string ret = string.Empty;
            try
            {
                var zips = await GetAustraliaRandomZipFromStateId(stateId);
                //ret = zips == null ? string.Empty : (List<ZipcodeWithCountryId>)zips[new Random().Next(0, zips.Count - 1)].Zipcode;
            }
            catch(Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }


        public async Task<int> EnrollSingleOwner(EnrollmentParameters iep)
        {
            int ret = 0;
            RestRequestSpecification request = null;

            try
            {
                request = new RestRequestSpecification();
                request.Verb = HttpMethod.Post;
                request.RequestUri = $"Enrollment/EnrollmentParameters/FullEnrollOwner";
                request.ContentType = "application/json";
                request.Content = JsonSerializer.Serialize(iep);

                var returnPost = await asyncRestClientQALib.ExecuteAsync<RestResult<EnrollmentParameters>>(request);
                ret = int.Parse(returnPost.Value.ToString());
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<string> AddATrialPet(int ownerId, PetParameters petParams)
        {
            string ret = string.Empty;
            RestRequestSpecification request = null;

            try
            {
                request = new RestRequestSpecification();
                request.Verb = HttpMethod.Post;
                request.RequestUri = $"Enrollment/PetParameters/AddATrialPet/{ownerId}";
                request.ContentType = "application/json";
                request.Content = JsonSerializer.Serialize(petParams);

                var returnPost = await asyncRestClientQALib.ExecuteAsync<string>(request);
                ret = returnPost.Value.ToString();
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> AddAPetSkipPayment(int ownerId, PetParameters petParams)
        {
            bool ret = false;
            RestRequestSpecification request = null;

            try
            {
                request = new RestRequestSpecification();
                request.Verb = HttpMethod.Post;
                request.RequestUri = $"Enrollment/PetParameters/AddAPetSkipPayment/{ownerId}";
                request.ContentType = "application/json";
                request.Content = JsonSerializer.Serialize(petParams);

                var returnPost = await asyncRestClientQALib.ExecuteAsync<string>(request);
                ret = bool.Parse(returnPost.Value.ToString());
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> ConvertTrialPolicy(int ownerId, IEnrollmentParameters iiep)
        {
            bool ret = false;
            RestRequestSpecification request = null;

            try
            {
                request = new RestRequestSpecification();
                request.Verb = HttpMethod.Post;
                request.RequestUri = $"Enrollment/ConvertTrials/ConvertPolicy/{ownerId}";
                request.ContentType = "application/json";
                request.Content = JsonSerializer.Serialize(iiep);

                var returnPost = await asyncRestClientQALib.ExecuteAsync<string>(request);
                ret = bool.Parse(returnPost.Value.ToString());
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> ConvertTrialPet(int ownerId, string petName, EnrollmentParameters iep)
        {
            bool ret = false;
            RestRequestSpecification request = null;

            try
            {
                request = new RestRequestSpecification();
                request.Verb = HttpMethod.Post;
                request.RequestUri = $"Enrollment/ConvertTrials/ConverPet/{ownerId}/{petName}";
                request.ContentType = "application/json";
                request.Content = JsonSerializer.Serialize(iep);

                var returnPost = await asyncRestClientQALib.ExecuteAsync<string>(request);
                ret = bool.Parse(returnPost.Value.ToString());
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<string> CancelATrialPet(int ownerId, PetParameters petParams)
        {
            string ret = string.Empty;
            RestRequestSpecification request = null;

            try
            {
                request = new RestRequestSpecification();
                request.Verb = HttpMethod.Post;
                request.RequestUri = $"Enrollment/PetParameters/AddATrialPet/{ownerId}";
                request.ContentType = "application/json";
                request.Content = JsonSerializer.Serialize(petParams);

                var returnPost = await asyncRestClientQALib.ExecuteAsync<string>(request);
                ret = returnPost.Value.ToString();
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            return ret;
        }

        public async Task<Owner> GetTruDatOwnerByOwnerId(int ownerId)
        {
            Owner ret = null;
            try
            {
                var filter = QueryBuilder<Owner>.CreateDynamicQueryDefinition().SimpleFilter(p => p.Id == ownerId).CreateDefinition();
                ret = this.Repository.RetrieveMany<Owner, int>(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Info(ex);
            }
            return ret;
        }

        public async Task<QALib_BillingAccount> GetBillingAccountByOwnerId(int ownerId)
        {
            QALib_BillingAccount ret = null;
            try
            {
                var filter = QueryBuilder<Owner>.CreateDynamicQueryDefinition().SimpleFilter(p => p.Id == ownerId).CreateDefinition();
                var owner = this.Repository.RetrieveMany<Owner, int>(filter);
                string uniqueid = owner.First().UniqueId.ToString().ToLower();
                var afilter = QueryBuilder<QALib_BillingAccount>.CreateDynamicQueryDefinition().SimpleFilter(p => p.ExternalId.ToLower().Equals(uniqueid)).CreateDefinition();
                var accs = this.Repository.RetrieveMany<QALib_BillingAccount, int>(afilter);
                ret = accs.First();
            }
            catch(Exception ex)
            {
                BillingTestCommon.log.Info(ex);
            }
            return ret;
        }

    }
}
