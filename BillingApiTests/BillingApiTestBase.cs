//----------------------------------------------------------------------------------------------------------
// <copyright file="BillingApiTestBase.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.BillingApiTests
{
    using BillingTestCommon.Methods;
    using global::BillingApiTests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using System.Text;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.TruFoundation;
    using Trupanion.TruFoundation.IoC;
    using Trupanion.TruFoundation.Logging;
    using Trupanion.TruFoundation.RestClient.Async;
    using Trupanion.TruFoundation.TestTools;

    public class BillingApiTestBase : BaseIntegrationTests
    {
        public static ILog log = Logger.Log;

        public IAsyncRestClient asyncRestClientBilling;
        public string billingServiceBaseUrl = "http://host1.test-billing.services.trupanion.com";
        //public const string billingServiceBaseUrl = "http://localhost:11004";                           // local
        public Dictionary<string, string> Headers = null;
        public RestRequestSpecification request { get; set; }


        public void InitTestClass()
        {
            try
            {
                ServiceFactory.InitializeServiceFactory(new ContainerConfiguration(ApplicationProfileType.TestFramework));

                billingServiceBaseUrl = BillingApiTestSettings.Default.BillingServiceBaseUrlTestEnvironment;

                var asyncRestClientFactory = ServiceFactory.Instance.Create<IAsyncRestClientFactory>();
                asyncRestClientBilling = asyncRestClientFactory.CreateClient(RestClientDefinitionBuilder.Build()
                            .ForServiceUri(billingServiceBaseUrl)
                            .Create());

                Headers = new Dictionary<string, string>();
                Headers.Add("Authorization", BillingApiTestSettings.Default.BillingServiceAuthorizationTokenTestEnvironment);

                request = new RestRequestSpecification();
                request.Headers = Headers;
                request.ContentType = "application/json";
            }
            catch(Exception ex)
            {
                log.Fatal(ex);
            }
        }


        public static bool CheckServiceIsRunning(string machine, string name)
        {
            bool bRet = false;
            ServiceController sc = new ServiceController();
            StringBuilder sbRet = new StringBuilder();

            try
            {
                sc.MachineName = machine;
                sc.ServiceName = name;
                bRet = sc.Status.Equals(ServiceControllerStatus.Running);
            }
            catch (System.Exception ex)
            {
                sbRet.AppendLine($"\t\tCheckServiceIsRunning() exception for service {name} on machine of {machine} - {ex}");
                bRet = false;
            }
            return bRet;
        }

        public static InvoicePreviewCriteria BuildInvoicePreviewCriteriaFromInvoice(string IsoAlpha3Code)
        {
            InvoicePreviewCriteria ret = new InvoicePreviewCriteria();
            ret.IsoAlpha3CountryCode = IsoAlpha3Code;
            InvoicePreviewItemCriteria item = new InvoicePreviewItemCriteria();
            item.Amount = 518.18m;
            item.ProductId = BillingApiTestSettings.Default.BillingServiceProductId;   // "Pet Insurance";
            switch(IsoAlpha3Code.ToLower())
            {
                case "usa":
                    ret.PostalCode = "56068";
                    ret.IsoAlpha2SateOrProvinceCode = "Minnesota";
                    break;
                case "can":
                    ret.PostalCode = "V0G 1M0";
                    ret.IsoAlpha2SateOrProvinceCode = "British Columbia";
                    break;
                case "aus":
                    ret.PostalCode = "6065";
                    ret.IsoAlpha2SateOrProvinceCode = "Western Australia";
                    break;
                default:
                    Assert.Fail($"IsoAlpha3Code is not supported: {IsoAlpha3Code}");
                    break;
            }
            ret.Items.Add(item);
            return ret;
        }

    }
}
