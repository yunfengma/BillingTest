//-----------------------------------------------------------------------
// <copyright file="BillingRestClient.cs" company="Trupanion">
// Copyright(c) 2019 by Trupanion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Trupanion.Billing.Test
{
    using BillingTestCommon;
    using BillingTestCommon.Methods;
    using BillingTestCommon.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Trupanion.Billing.Adapters.Rest.V2;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Api.BankAccounts.V2;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.Billing.Api.PaymentMethods.V2;
    using Trupanion.Billing.Api.Payments.V2;
    using Trupanion.Billing.Api.Products.V2;
    using Trupanion.Billing.Api.Refunds.V2;
    using Trupanion.Billing.Application;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Enrollment;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.IoC;
    using Trupanion.TruFoundation.Logging;
    using Trupanion.TruFoundation.RestClient.Async;

    public class BillingRestClient : BillingTestBase
    {
        public static ILog log = Logger.Log;

        public static IAsyncRestClient asyncRestClientBilling;
        public const string billingServiceBaseUrl = "http://host1.test-billing.services.trupanion.com";
        //public const string billingServiceBaseUrl = "http://localhost:11004";                           // local
        public static Dictionary<string, string> Headers = null;
        public const string authorizationTokenV2 = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE1Njg2NTkxMDcsImV4cCI6MTU2OTI2MzkwNywiaXNzIjoidGVzdC1hcHAwMS50cnVpZC50cnVwYW5pb24uY29tIiwiYXVkIjoidGVzdC5zZXJ2aWNlcy50cnVwYW5pb24uY29tIiwianRpIjoiNTNiNmZhNmNkZmNkNDc1NmI5Y2JlNmQ2NWM5N2U3N2QiLCJUcnVJZCI6eyJJZCI6IjA4MDkyYWFhMmNlNjQ2ZjA4ZGJiMzc3OGVkNDM0NGQ4IiwiVXNlciI6Inl1bmZlbmcubWEiLCJOYW1lIjoiWXVuZmVuZyBNYSIsIlNjb3BlIjowfX0=.DN4F10I85s5vKfFKnykDNwEW/1Lc6Est5vupyvzSnMY=";

        public IPaymentApi paymentApiService = null;
        public IPaymentCollectionService paymentCollectionService = null;

        public HttpRequestMessage Request { get; set; }
        public AccountFilterCriteria criteria { get; set; }
        public Account BillingAccount { get; set; }


        public BillingRestClient()
        {
            InitTestClass();
            Init();
        }

        public void Init()
        {
            try
            {
                var asyncRestClientFactory = ServiceFactory.Instance.Create<IAsyncRestClientFactory>();
                asyncRestClientBilling = asyncRestClientFactory.CreateClient(RestClientDefinitionBuilder.Build()
                            .ForServiceUri(billingServiceBaseUrl)
                            .Create());

                Headers = new Dictionary<string, string>();
                Headers.Add("Authorization", authorizationTokenV2);

                paymentApiService = IocContainer.Resolve<IPaymentApi>();
                paymentCollectionService = IocContainer.Resolve<IPaymentCollectionService>();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }

        public async Task<Account> GetAccountById(string billingAccountId)
        {
            Account ret = null;
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"v2/accounts/{billingAccountId}";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (string.IsNullOrEmpty(returnPost?.Message))
                {
                    ret = JsonSerializer.Deserialize<Account>(returnPost.Value);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<IReadOnlyList<Account>> GetBillingAccountByPolicyHolderId(string policyHolderId)
        {
            IReadOnlyList<Account> ret = null;
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"v2/accounts?policyholderId={policyHolderId}";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (string.IsNullOrEmpty(returnPost?.Message))
                {
                    ret = JsonSerializer.Deserialize<IReadOnlyList<Account>>(returnPost.Value);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<InvoiceWithItems> GetAccountInvoicesNext(string accountId)
        {
            InvoiceWithItems ret = new InvoiceWithItems();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"/v2/accounts/{accountId}/invoices/next";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<InvoiceWithItems>(returnPost.Value);
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<List<InvoiceWithItems>> GetAccountInvoices(string accountId)
        {
            List<InvoiceWithItems> ret = new List<InvoiceWithItems>();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"v2/invoices?criteria.accountId={accountId}&criteria.refundId=0";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<List<InvoiceWithItems>>(returnPost.Value);
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<List<InvoiceItem>> GetInvoicesItems(string invoiceId)
        {
            List<InvoiceItem> ret = new List<InvoiceItem>();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"v2/invoices/{invoiceId}/items";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<List<InvoiceItem>>(returnPost.Value);
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<List<InvoiceItem>> GetAccountInvoiceItems(string accountId)
        {
            List<InvoiceItem> ret = new List<InvoiceItem>();
            try
            {
                List<InvoiceWithItems> invoices = await GetAccountInvoices(accountId);
                foreach (InvoiceWithItems invoice in invoices)
                {
                    List<InvoiceItem> invoicItems = await GetInvoicesItems(invoice.Id);
                    foreach (InvoiceItem item in invoicItems)
                    {
                        ret.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<List<PaymentMethod>> GetAccountPaymentMethodByAccountId(string accountId)
        {
            List<PaymentMethod> ret = new List<PaymentMethod>();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"v2/paymentmethods?accountId={accountId}";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<List<PaymentMethod>>(returnPost.Value);
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<List<PaymentMethod>> PostPaymentMethodsAccountEtfs(string accountId)
        {
            List<PaymentMethod> ret = new List<PaymentMethod>();
            try
            {
                SaveEftPaymentMethodCommand command = new SaveEftPaymentMethodCommand();

                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.Content = JsonSerializer.Serialize(command);
                req.RequestUri = $"v2/paymentmethods?accountId={accountId}";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<List<PaymentMethod>>(returnPost.Value);
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<List<Invoice>> GetInvoices(string accountId)
        {
            List<Invoice> ret = new List<Invoice>();
            InvoiceFilterCriteria criteria = new InvoiceFilterCriteria();

            try
            {
                criteria.AccountId = accountId;
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"/v2/invoices?criteria.accountId={accountId}";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                if (returnPost.Success)
                {
                    ret = JsonSerializer.Deserialize<List<Invoice>>(returnPost.Value.ToString());
                }
                else
                {
                    ret = null;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }

        public async Task<CollectPaymentResponse> PostPayAccountPaymentCollections(int ownerId)
        {
            CollectPaymentResponse ret = null;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());

                CollectPaymentCommand command = new CollectPaymentCommand();
                command.AccountId = accounts.First().Id;

                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/paymentcollections";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<CollectPaymentResponse>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<Payment> GetAccountPayment(int ownerId)
        {
            Payment ret = new Payment();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/payments?accountId={accounts.First().Id}";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                ret = JsonSerializer.Deserialize<Payment>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> PostAccountUpdatesMonthlyBillingDay(int ownerId, int day)
        {
            bool ret = false;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                UpdateAccountCommand command = GetUpdateAccountCommandFromAccount(accounts.First());
                command.BillCycleDay = day;
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/AccountUpdates";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = returnPost.Success;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> PostAccountUpdatesChangeDefaultPaymentMethod(int ownerId, BillingPaymentMethodTypes method)
        {
            bool ret = false;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                List<PaymentMethod> methods = await GetAccountPaymentMethods(ownerId);
                PaymentMethod newMethod = methods.Where(e => e.Type.Equals(method.ToString())).First();
                UpdateAccountCommand command = GetUpdateAccountCommandFromAccount(accounts.First());
                command.DefaultPaymentMethodId = newMethod.Id;
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/AccountUpdates";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = returnPost.Success;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> PostAccountUpdateCharity(int ownerId, int charityId)
        {
            bool ret = false;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                UpdateAccountCommand command = GetUpdateAccountCommandFromAccount(accounts.First());
                CharityIds charity = Charity.Charities.Where(i => i.Id == charityId).First();
                if (charity != null)
                {
                    command.CharityId = charity.UniqueId;
                    RestRequestSpecification req = new RestRequestSpecification();
                    req.Verb = HttpMethod.Post;
                    req.Headers = Headers;
                    req.ContentType = "application/json";
                    req.RequestUri = $"v2/AccountUpdates";
                    req.Content = JsonSerializer.Serialize(command);
                    var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                    ret = returnPost.Success;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public UpdateAccountCommand GetUpdateAccountCommandFromAccount(Account account)
        {
            UpdateAccountCommand ret = new UpdateAccountCommand();
            ret.AccountId = account.Id;
            ret.BillCycleDay = account.BillCycleDay;
            ret.CharityId = account.CharityId;
            ret.DefaultPaymentMethodId = account.DefaultPaymentMethodId;
            ret.NonReferencedCreditMethodTypeId = account.NonReferencedCreditMethodTypeId;
            return ret;
        }

        public async Task<bool> PostReschedulePayments(int ownerId, DateTime date)
        {
            bool ret = false;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                ReschedulePaymentCommand command = new ReschedulePaymentCommand();
                command.AccountId = accounts.First().Id;
                command.RequestedDate = date;
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/reschedulepayments";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = returnPost.Success;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<List<PaymentMethod>> GetAccountPaymentMethods(int ownerId)
        {
            List<PaymentMethod> ret = new List<PaymentMethod>();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                //RestRequestSpecification req = new RestRequestSpecification();
                //req.Verb = HttpMethod.Get;
                //req.Headers = Headers;
                //req.ContentType = "application/json";
                //req.RequestUri = $"v2/paymentmethods?accountId={accounts.First().Id}";
                //var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                //ret = JsonSerializer.Deserialize<List<PaymentMethod>>(returnPost.Value);
                ret = await GetAccountPaymentMethodByAccountId(accounts.First().Id.ToString());
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> PostRescheduledPayments(int ownerId, DateTime dueDate)
        {
            bool ret = false;
            ReschedulePaymentCommand command = new ReschedulePaymentCommand();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                command.AccountId = accounts.First().Id;
                command.RequestedDate = dueDate;
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"/v2/reschedulepayments";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = returnPost.Success;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> PostExpirationDate(int ownerId, DateTime date)
        {
            bool ret = false;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                // get payment method
                List<PaymentMethod> methods = await GetAccountPaymentMethods(ownerId);
                PaymentMethod method = methods.First();
                if (method.Type == BillingPaymentMethodTypes.CreditCard.ToString())
                {
                    // update date
                    UpdateCreditCardExpirationCommand command = new UpdateCreditCardExpirationCommand();
                    command.PaymentMethodId = method.Id;
                    command.Year = date.Year;
                    command.Month = date.Month;

                    RestRequestSpecification req = new RestRequestSpecification();
                    req.Verb = HttpMethod.Post;
                    req.Headers = Headers;
                    req.ContentType = "application/json";
                    req.RequestUri = $"v2/creditcards/expirationupdates";
                    req.Content = JsonSerializer.Serialize(command);
                    var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                    ret = returnPost.Success;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<bool> PostCreditBalanceRefunds(int ownerId)
        {
            bool ret = false;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                RefundCreditBalanceCommand command = new RefundCreditBalanceCommand();
                command.AccountId = accounts.First().Id;

                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/creditbalancerefunds";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = returnPost.Success;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<List<Refund>> GetAccountRefund(int ownerId)
        {
            List<Refund> ret = new List<Refund>();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/refunds?accountId={accounts.First().Id}";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<List<Refund>>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = new List<Refund>();
            }
            return ret;
        }

        public async Task<List<Product>> GetProducts()
        {
            List<Product> ret = new List<Product>();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/products";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<List<Product>>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<List<PaymentReschedulingRequest>> GetAccountPaymentReschedulingRequests(int ownerId)
        {
            List<PaymentReschedulingRequest> ret = new List<PaymentReschedulingRequest>();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/paymentreschedulingrequests?criteria.accountId={accounts.First().Id}";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<List<PaymentReschedulingRequest>>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<List<PaymentMethodType>> GetPaymentTypes()
        {
            List<PaymentMethodType> ret = new List<PaymentMethodType>();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/paymentmethodtypes";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<List<PaymentMethodType>>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<BankAccount> GetBankAccouintsByOwnerId(int ownerId)
        {
            BankAccount ret = null;
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/bankaccounts?criteria.accountId={accounts.First().Id}";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<BankAccount>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<SaveBankAccountResponse> PostSaveUsaBankAccount(int ownerId, BillingParameters billingParam)
        {
            SaveBankAccountResponse ret = null;
            SaveUsaBankAccountCommand command = new SaveUsaBankAccountCommand();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());

                command = GetSaveUsaBankAccountCommandFromBillingParameter(accounts.First(), billingParam);
                command.AccountId = BillingTestCommonSettings.Default.SaveUsaBankAccountBillingAccountId;
                command.NameOnAccount = $"bat{billingParam.BankAccountNameOnAccount}";

                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/bankaccounts/usa";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<SaveBankAccountResponse>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<SaveBankAccountResponse> PostSaveCanadaBankAccount(int ownerId, BillingParameters billingParam)
        {
            SaveBankAccountResponse ret = null;
            SaveUsaBankAccountCommand command = new SaveUsaBankAccountCommand();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());

                command = GetSaveUsaBankAccountCommandFromBillingParameter(accounts.First(), billingParam);
                command.AccountId = BillingTestCommonSettings.Default.SaveUsaBankAccountBillingAccountId;
                command.NameOnAccount = $"bat{billingParam.BankAccountNameOnAccount}";

                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/bankaccounts/canada";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<SaveBankAccountResponse>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public async Task<PaymentMethod> PostPaymentMethodsETFS(int ownerId, BillingParameters billingParam)
        {
            PaymentMethod ret = null;
            SaveEftPaymentMethodCommand command = new SaveEftPaymentMethodCommand();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());

                command = GetSaveEftPaymentMethodCommandFromBillingParameter(accounts.First(), billingParam);
                command.AccountName = $"bat{billingParam.BankAccountNameOnAccount}";

                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/paymentmethods/efts";
                req.Content = JsonSerializer.Serialize(command);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<PaymentMethod>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public SaveUsaBankAccountCommand GetSaveUsaBankAccountCommandFromBillingParameter(Account account, BillingParameters billingParam)
        {
            SaveUsaBankAccountCommand ret = new SaveUsaBankAccountCommand();
            ret.AccountId = null;
            ret.AccountNumber = $"518{billingParam.BankAccountTransitNumber}";
            ret.AccountTypeId = billingParam.BankAccountAccountType == LegacyPlatform.Constants.BankAccountType.Checking ? WellKnownBankAccountTypes.Checking : WellKnownBankAccountTypes.Saving;
            ret.BankAccountId = Guid.NewGuid();
            ret.BankName = billingParam.BankAccountBankName;
            ret.Data = null;
            //ret.DynamicMessage = string.Empty;
            ret.IsNonReferencedCreditMethod = false;
            ret.IsPaymentMethod = false;
            ret.KeyList = new List<TruFoundation.EnterpriseCatalog.DynamicEntityKey>();
            ret.MessageInfo = null;
            ret.NameOnAccount = billingParam.BankAccountNameOnAccount;
            ret.OriginatingUserId = Guid.Empty;
            ret.RoutingNumber = billingParam.BankAccountTransitNumber;
            ret.Time = DateTime.UtcNow;
            ret.UserId = Guid.Empty;
            return ret;
        }

        public SaveEftPaymentMethodCommand GetSaveEftPaymentMethodCommandFromBillingParameter(Account account, BillingParameters billingParam)
        {
            SaveEftPaymentMethodCommand ret = new SaveEftPaymentMethodCommand();
            ret.AccountName = $"518{billingParam.BankAccountNameOnAccount}";
            ret.BankAccountNumber = "632756";
            ret.BankAccountType = billingParam.BankAccountAccountType == LegacyPlatform.Constants.BankAccountType.Checking ? "Checking" : "Saving";
            ret.BankName = billingParam.BankAccountBankName;
            ret.BankRoutingNumber = billingParam.BankAccountTransitNumber;
            ret.Currency = account.Currency;
            //ret.Data = null;
            //ret.DynamicMessage = null;
            ret.IsBillingDefault = false;
            ret.KeyList = new List<TruFoundation.EnterpriseCatalog.DynamicEntityKey>();
            //ret.MessageInfo = null;
            ret.OriginatingUserId = Guid.NewGuid();
            ret.PartyId = account.PartyId;
            ret.PaymentMethodId = Guid.Parse(account.DefaultPaymentMethodId);
            //ret.SensitiveStringProperties = string.Empty;
            ret.Time = DateTime.UtcNow.ToLocalTime();
            ret.UserId = Guid.NewGuid();
            return ret;
        }

        public async Task<InvoicePreview> PostInvoicesPreview(int ownerId, string IsoAlpha3Code = "USA")
        {
            InvoicePreview ret = new InvoicePreview();
            try
            {
                // get a invoice
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                IReadOnlyList<Account> accounts = await GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                List<InvoiceWithItems> invoices = await GetAccountInvoices(accounts.First().Id.ToString());
                // preview invoice
                InvoicePreviewCriteria crtieria = BuildInvoicePreviewCriteriaFromInvoice(ownerCollection, invoices.First(), IsoAlpha3Code);
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Post;
                req.Headers = Headers;
                req.ContentType = "application/json";
                req.RequestUri = $"v2/invoices/previews";
                req.Content = JsonSerializer.Serialize(crtieria);
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);

                ret = JsonSerializer.Deserialize<InvoicePreview>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
            return ret;
        }

        public InvoicePreviewCriteria BuildInvoicePreviewCriteriaFromInvoice(OwnerCollection ownerCollection, InvoiceWithItems invoice, string IsoAlpha3Code)
        {
            InvoicePreviewCriteria ret = new InvoicePreviewCriteria();
            ret.PostalCode = ownerCollection.AddressInformation.Zipcode;
            ret.IsoAlpha2SateOrProvinceCode = "Minnesota"; // ownerCollection.AddressInformation.StateId.ToString();
            ret.IsoAlpha3CountryCode = IsoAlpha3Code;
            InvoicePreviewItemCriteria item = new InvoicePreviewItemCriteria();
            item.Amount = invoice.Amount;
            item.DiscountAmount = 0;
            item.ProductId = "2c91a0f8557bc1f401557f2ceb5e1d31";         // "Pet Insurance";
            ret.Items.Add(item);
            return ret;
        }

        public async Task<List<BankAccountType>> GetBankAccountTypes()
        {
            List<BankAccountType> ret = new List<BankAccountType>();
            try
            {
                RestRequestSpecification req = new RestRequestSpecification();
                req.Verb = HttpMethod.Get;
                req.RequestUri = $"v2/bankaccounttypes";
                req.Headers = Headers;
                req.ContentType = "application/json";
                var returnPost = await asyncRestClientBilling.ExecuteAsync<string>(req);
                ret = JsonSerializer.Deserialize<List<BankAccountType>>(returnPost.Value);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                ret = null;
            }
            return ret;
        }


    }
}
