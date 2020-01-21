//-----------------------------------------------------------------------
// <copyright file="ZuoraAccess.cs" company="Trupanion">
// Copyright(c) 2019 by Trupanion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Trupanion.Billing.Test
{
    using BillingTestCommon.Methods;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TruFoundation.TestTools;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Api.Subscriptions.V1;
    using Trupanion.TruFoundation;
    using Trupanion.TruFoundation.IoC;
    using Trupanion.TruFoundation.RestClient.Async;
    using Trupanion.TruFoundation.Serialization;

    public class ZuoraAccess : BaseIntegrationTests
    {
        public static IAsyncRestClient asyncRestClientZuora;
        public const string zuoraServiceBaseUrl = "https://pt1.zuora.com/";

        public static Dictionary<string, string> Headers = null;
        public const string authorizationTokenV2 = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE1Njg2NTkxMDcsImV4cCI6MTU2OTI2MzkwNywiaXNzIjoidGVzdC1hcHAwMS50cnVpZC50cnVwYW5pb24uY29tIiwiYXVkIjoidGVzdC5zZXJ2aWNlcy50cnVwYW5pb24uY29tIiwianRpIjoiNTNiNmZhNmNkZmNkNDc1NmI5Y2JlNmQ2NWM5N2U3N2QiLCJUcnVJZCI6eyJJZCI6IjA4MDkyYWFhMmNlNjQ2ZjA4ZGJiMzc3OGVkNDM0NGQ4IiwiVXNlciI6Inl1bmZlbmcubWEiLCJOYW1lIjoiWXVuZmVuZyBNYSIsIlNjb3BlIjowfX0=.DN4F10I85s5vKfFKnykDNwEW/1Lc6Est5vupyvzSnMY=";
        public static IJsonSerialization serializer = null;

        public HttpRequestMessage Request { get; set; }
        public AccountFilterCriteria criteria { get; set; }
        public static RestRequestSpecification request;


        public ZuoraAccess()
        {
            InitTestClass();
        }

        //public static void InitTestClass(TestContext context)
        public void InitTestClass()
        {
            try
            {
                ServiceFactory.InitializeServiceFactory(new ContainerConfiguration(ApplicationProfileType.TestFramework));

                var asyncRestClientFactory = ServiceFactory.Instance.Create<IAsyncRestClientFactory>();
                asyncRestClientZuora = asyncRestClientFactory.CreateClient(RestClientDefinitionBuilder.Build()
                            .ForServiceUri(zuoraServiceBaseUrl)
                            .Create());
                request = new RestRequestSpecification();

                Headers = new Dictionary<string, string>();
                //Headers.Add("Authorization", authorizationTokenV2);

                serializer = ServiceFactory.Instance.Create<IJsonSerialization>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<IEnumerable<Subscription>> GetSubscription(SubscriptionFilterCriteria criteria)
        {
            IEnumerable<Subscription> ret = null;

            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"/apps/Subscription.do?method=view&id={criteria.AccountId}";
                //req.Headers = Headers;
                req.ContentType = "application/json";
                //var returnPost = await asyncRestClientZuora.ExecuteAsync<IEnumerable<Persistence.Subscription>>(req);
                var returnPost = await asyncRestClientZuora.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<IEnumerable<Subscription>>(returnPost.Value.ToString());
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ret = null;
            }
            return ret;
        }

    }
}
