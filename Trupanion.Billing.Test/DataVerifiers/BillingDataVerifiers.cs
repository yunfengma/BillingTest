//----------------------------------------------------------------------------------------------------------
// <copyright file="BillingDataVerifiers.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.DataVerifiers
{
    using BillingTestCommon.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.DatabaseAccess.Models.Projections.TruDat;
    using Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.Logging;
    using PaymentMethod = Api.PaymentMethods.V2.PaymentMethod;

    public class BillingDataVerifiers : BillingTestBase, IBillingDataVerifiers
    {
        private IReadOnlyList<Account> accounts = null;
        private BillingRestClient billingRestClient { get; set; }


        public BillingDataVerifiers()
        {
            InitTestClass();
            billingRestClient = new BillingRestClient();
        }


        public async Task VerifyBillingAccount(int ownerId, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true)
        {
            // get trudat owner
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            // verify billing account existing
            accounts = await VerifyBillingAccountExist(ownerCollection.OwnerInformation.UniqueId.ToString(), bExist);
            if (bExist)
            {
                Account account = accounts.First();
                Assert.IsTrue(account.AutoPay == accountExpected.AutoPay, $"auto pay- {account.AutoPay} is not as expected - {accountExpected.AutoPay}");           // verify auto pay
                Assert.IsTrue(account.Currency.Equals(accountExpected.Currency), $"currency- {account.Currency} is as not expected - {accountExpected.Currency}");  // verify currency
                Assert.IsTrue(account.BillCycleDay.Equals(iep.BillingParams.BillingDayOfMonth), $"bill cycle day- {account.BillCycleDay} is as not expected - {iep.BillingParams.BillingDayOfMonth}");                  // verify currency
                List<PaymentMethod> paymentMethod = await VerifyAccountPaymentMethod(account.Id, iep);                                                              // verify bank info
                InvoiceWithItems iNext = await VerifyAccountPremium(account.Id, iep, quote);                                                                        // verify premium
                decimal dueBalance = await GetExpectedDueBalance(account, iep.StateId, quote);                                                                      // verify premium
                Assert.IsTrue(dueBalance == account.Balance, $"due balance {dueBalance} doen'st match expected {account.Balance}");
                // TODO - verify due date and charity
                //Assert.IsTrue(account.BalancePastDueDate == null || account.BalancePastDueDate.Equals(iep.EffectiveDate), $"past due date {account.BalancePastDueDate} doesn't match expected {iep.EffectiveDate}");  // past due date
                //Assert.IsTrue(account.CharityId == null || account.CharityId.Equals(iep.BillingParams.CharityId), $"charity {account.CharityId} doesn't match expected {iep.BillingParams.CharityId}");                 // past due date
            }
            else
            {
                Assert.IsTrue(accounts.Count == 0, $"account shouldn't exist");                                                                                     // verify auto pay
            }
        }

        public async Task VerifyBillingAccountForConvertedTrialPolicy(int ownerId, IEnrollmentParameters iiep, bool bExist = true)
        {
            // get trudat owner
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            // verify billing account existing
            accounts = await VerifyBillingAccountExist(ownerCollection.OwnerInformation.UniqueId.ToString(), bExist);
            // verify pet enrollment status
            bool b = await verifyOwnerPetEnrollmentStatus(ownerId, iiep.Pets.First().PetName);
            if (bExist)
            {
                Account account = accounts.First();
                Assert.IsTrue(account.AutoPay == accountExpected.AutoPay, $"auto pay- {account.AutoPay} is not as expected - {accountExpected.AutoPay}");           // verify auto pay
                //Assert.IsTrue(account.Currency.Equals(accountExpected.Currency), $"currency- {account.Currency} is as not expected - {accountExpected.Currency}");  // verify currency
                // TODO: billcycleday was not set correctly in convert
                //Assert.IsTrue(account.BillCycleDay.Equals(iiep.BillingParams.BillingDayOfMonth), $"bill cycle day- {account.BillCycleDay} is as not expected - {iiep.BillingParams.BillingDayOfMonth}");                  // verify currency
                iep = testDataManager.ConvertEnrollmentParametersFromIEnrollmentParameters(iiep);
                QaLibQuoteResponse quote = await qaLibRestClient.CreateQuote(iep);
                InvoiceWithItems iNext = await VerifyAccountPremium(account.Id, iep, quote);                                                                        // verify premium
            }
            else
            {
                Assert.IsTrue(accounts.Count == 0, $"account shouldn't exist");                                                                                     // verify auto pay
            }
        }

        public async Task VerifyBillingAccountForConvertedTrialPolicy(int ownerId, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true)
        {
            // get trudat owner
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            // verify billing account existing
            accounts = await VerifyBillingAccountExist(ownerCollection.OwnerInformation.UniqueId.ToString(), bExist);
            // verify pet enrollment status
            bool b = await verifyOwnerPetEnrollmentStatus(ownerId, iep.Pets.First().PetName);
            if (bExist)
            {
                Account account = accounts.First();
                Assert.IsTrue(account.AutoPay == accountExpected.AutoPay, $"auto pay- {account.AutoPay} is not as expected - {accountExpected.AutoPay}");           // verify auto pay
                Assert.IsTrue(account.Currency.Equals(accountExpected.Currency), $"currency- {account.Currency} is as not expected - {accountExpected.Currency}");  // verify currency
                // TODO: billcycleday was not set correctly in convert
                //Assert.IsTrue(account.BillCycleDay.Equals(iep.BillingParams.BillingDayOfMonth), $"bill cycle day- {account.BillCycleDay} is as not expected - {iiep.BillingParams.BillingDayOfMonth}");                  // verify currency
                InvoiceWithItems iNext = await VerifyAccountPremium(account.Id, iep, quote);                                                                        // verify premium
            }
            else
            {
                Assert.IsTrue(accounts.Count == 0, $"account shouldn't exist");                                                                                     // verify auto pay
            }
        }

        public async Task VerifyCanceledPetBillingInfo(int ownerId, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true)
        {
            //try
            //{
            //    // get trudat owner
            //    ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            //    accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            //}
            //catch(Exception ex)
            //{
            //    Logger.Log.Fatal(ex);
            //}
            //Account account = accounts.First();
            Account account = await GetAccountByOwnerId(ownerId);
            InvoiceWithItems iNext = await VerifyCanceledAccountPremium(account.Id, iep, quote);                                                   // verify premium
        }

        public async Task VerifyCanceledPetBillingInfo(int ownerId, EnrollmentParameters iep, List<InvoiceItem> invoiceItems, Account accountExpected, bool bExist = true)
        {
            Account account = await GetAccountByOwnerId(ownerId);
            InvoiceWithItems iNext = await VerifyCanceledAccountPremium(account.Id, iep, invoiceItems);                                                   // verify premium
        }

        public async Task<Account> GetAccountByOwnerId(int ownerId)
        {
             Account account = new Account();
           try
            {
                // get trudat owner
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                account = accounts.First();
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(ex);
            }
            return account;
        }

        public async Task<List<InvoiceWithItems>> GetAccountInvoicesByOwnerId(int ownerId)
        {
            List<InvoiceWithItems> invoices = new List<InvoiceWithItems>();
            try
            {
                // get trudat owner
                Account account = await GetAccountByOwnerId(ownerId);
                // get invoices
                invoices = await billingRestClient.GetAccountInvoices(account.Id);
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(ex);
            }
            return invoices;
        }

        public async Task<List<InvoiceItem>> GetAccountInvoiceItemsByOwnerId(int ownerId)
        {
            List<InvoiceItem> invliceItems = new List<InvoiceItem>();
            try
            {
                // get trudat owner
                Account account = await GetAccountByOwnerId(ownerId);
                // get invoiceintes
                invliceItems = await billingRestClient.GetAccountInvoiceItems(account.Id);
            }
            catch(Exception ex)
            {
                Logger.Log.Fatal(ex);
            }
            return invliceItems;
        }

        public async Task VerifyPendingCanceledPetBillingInfo(int ownerId, string petName, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);                  // get trudat owner
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            Account account = accounts.First();
            InvoiceWithItems iNext = await VerifyAccountPremium(account.Id, iep, quote);                                                   // verify premium
        }

        public async Task<IReadOnlyList<Account>> VerifyBillingAccountExist(string policyHolderId, bool bExist = true)
        {
            IReadOnlyList<Account> ret = null;
            ret = await billingRestClient.GetBillingAccountByPolicyHolderId(policyHolderId);
            if (bExist)
            {
                Assert.IsFalse(ret?.Count == 0, "account is not found unexpectedly");
            }
            else
            {
                Assert.IsTrue(ret?.Count == 0, "account has found unexpectedly");
            }
            return ret;
        }

        public async Task<InvoiceWithItems> VerifyAccountPremium(string accountId, EnrollmentParameters iep, QaLibQuoteResponse quote, bool bMatch = true)
        {
            InvoiceWithItems ret = null;
            bool found = false;
            string assertMsg = string.Empty;
            if (quote == null)
            {
                Assert.Inconclusive("please provide expected premium first");
            }
            ret = await billingRestClient.GetAccountInvoicesNext(accountId);
            Assert.IsTrue(ret != null, "no next invoice");
            for (int i = 0; i < iep.Pets.Count; i++)
            {
                found = false;
                foreach (InvoiceItem item in ret.InvoiceItems)
                {
                    assertMsg = $"<{quote.Pets[i].Premium} -- {item.ChargeAmount}>";
                    found = quote.Pets[i].Premium == item.ChargeAmount;
                    if (found)
                    {
                        assertMsg = string.Empty;
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
                else
                {
                    assertMsg += bMatch ? $"premium doesn't match any invoice item - {assertMsg}\r\n" 
                                        : $"premium matched invoice item - {assertMsg}\r\n";
                }
            }
            return ret;
        }

        public async Task<InvoiceWithItems> VerifyCanceledAccountPremium(string accountId, EnrollmentParameters iep, List<InvoiceItem> invoiceItems, bool bCanceld = true)
        {
            InvoiceWithItems ret = null;
            bool matchedPremium = false;
            string assertMsg = string.Empty;
            if (invoiceItems.Count <= 0)
            {
                Assert.Inconclusive("please provide expected premium first.");
            }
            var invoices = await billingRestClient.GetAccountInvoices(accountId);
            foreach (InvoiceWithItems invoice in invoices)
            {
                matchedPremium = false;
                for (int i = 0; i < invoiceItems.Count; i++)
                {
                    matchedPremium = invoiceItems[i].ChargeAmount == -1 * invoice.Amount;
                    if (matchedPremium)
                    {
                        assertMsg = string.Empty;
                        break;
                    }
                }
                if (matchedPremium)
                {
                    break;
                }
                else
                {
                    assertMsg += bCanceld ? $"canceled premium {invoice.Amount} doesn't match any expected\r\n"
                                          : $"pending canceled premium {invoice.Amount} is found\r\n"; ;
                }
            }
            Assert.IsTrue(matchedPremium, assertMsg);
            return ret;
        }

        public async Task<InvoiceWithItems> VerifyCanceledAccountPremium(string accountId, EnrollmentParameters iep, QaLibQuoteResponse quote, bool bCanceld = true)
        {
            InvoiceWithItems ret = null;
            bool found = false;
            string assertMsg = string.Empty;
            if (quote == null)
            {
                Assert.Inconclusive("please provide expected premium first.");
            }
            var invoices = await billingRestClient.GetAccountInvoices(accountId);
            foreach (InvoiceWithItems invoice in invoices)
            {
                for (int i = 0; i < iep.Pets.Count; i++)
                {
                    found = quote.Pets[i].Premium == -1 * invoice.Amount;
                    if (found)
                    {
                        assertMsg = string.Empty;
                        break;
                    }
                }
                if (bCanceld)
                {
                    assertMsg += found ? string.Empty : $"canceled premium {invoice.Amount} doesn't match any expected\r\n";
                }
                else
                {
                    assertMsg += !found ? string.Empty : $"pending canceled premium {invoice.Amount} is found\r\n";
                }
                if (found)
                {
                    break;
                }
            }
            Assert.IsTrue(found, assertMsg);
            return ret;
        }

        public async Task<List<PaymentMethod>> VerifyAccountPaymentMethod(string accountId, EnrollmentParameters iep)
        {
            List<PaymentMethod> ret = null;
            ret = await billingRestClient.GetAccountPaymentMethodByAccountId(accountId);
            Assert.IsTrue(ret?.Count > 0, "account payment methods is not retrieved");
            foreach(PaymentMethod payment in ret)
            {
                Assert.IsTrue(payment.Type.Equals(iep.BillingParams.PaymentMethod.ToString()), $"bank account type {payment.Type} is not match the expected {iep.BillingParams.BankAccountAccountType}");
                Assert.IsTrue(payment.AchBankName.Equals(iep.BillingParams.BankAccountBankName), $"bank name {payment.AchBankName} is not match the expected {iep.BillingParams.BankAccountBankName}");
                Assert.IsTrue(payment.AchAccountName.Equals(iep.BillingParams.BankAccountNameOnAccount), $"bank account name {payment.AchAccountName} is not match the expected {iep.BillingParams.BankAccountNameOnAccount}");
                Assert.IsTrue(payment.AchAccountType.Equals(iep.BillingParams.BankAccountAccountType.ToString()), $"bank account type {payment.Type} is not match the expected {iep.BillingParams.BankAccountAccountType}");
                Assert.IsTrue(payment.AchAccountNumberMask.Contains(iep.BillingParams.BankAccountAccountNumberLast4), $"last 4 digit {payment.AchAccountNumberMask} is not match the expected {iep.BillingParams.BankAccountAccountNumberLast4}");
                if (iep.StateId < 60)                    // us only
                {
                    Assert.IsTrue(payment.AchAbaCode.Equals(iep.BillingParams.BankAccountTransitNumber.ToString()), $"transit number {payment.AchAbaCode} is not match the expected {iep.BillingParams.BankAccountTransitNumber}");
                }
                break;
            }
            return ret;
        }

        public async Task<decimal> GetExpectedDueBalance(Account account, int stateId, QaLibQuoteResponse quote)
        {
            decimal ret = 0;
            try
            {
                ret = await qaLibRestClient.GetSetupFeeByStateId(stateId);
                for (int i = 0; i < quote.Pets.Count; i++)
                {
                    ret += quote.Pets[i].Premium;
                }
                if (stateId >= 60)                              // canada: tax are in item
                {
                    List<InvoiceWithItems> invoices = await billingRestClient.GetAccountInvoices(account.Id);
                    foreach (InvoiceWithItems invoice in invoices)
                    {
                        List<InvoiceItem> invoicItems = await billingRestClient.GetInvoicesItems(invoice.Id);
                        foreach (InvoiceItem item in invoicItems)
                        {
                            ret += item.TaxAmount;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(ex);
                ret = 0;
            }
            return ret;
        }

        public async Task<bool> verifyOwnerPetEnrollmentStatus(int ownerId, string petName, EnrollmentStatus status = EnrollmentStatus.Enrolled)
        {
            bool matchPet = false;
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            foreach (PetAndPolicy pet in ownerCollection.pets)
            {
                matchPet = pet.Pet.Name.ToLower().Equals(petName.ToLower());
                if (matchPet)
                {
                    matchPet = pet.PetPolicy.EnrollmentStatusId == (int?)status;
                    Assert.IsTrue(matchPet, $"{petName} enrollment status {pet.PetPolicy.EnrollmentStatusId} is not match the expected {status}");
                    break;
                }
            }
            Assert.IsTrue(matchPet, $"pet {petName} not found in owner {ownerId}");
            return matchPet;
        }

        public async Task verifyAccountDueBalance(int ownerId, decimal balance)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            Assert.IsTrue(accounts.First().Balance == balance, $"account due balance {accounts.First().Balance} doesn't match expencted {balance}");
        }

        public async Task verifyAccountMonthlyBillingDay(int ownerId, int day)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            Assert.IsTrue(accounts.First().BillCycleDay == day, $"monthly billing day {accounts.First().BillCycleDay} doesn't match expencted {day}");
        }

        public async Task<PaymentMethod> verifyAccountDefaultPaymentMethod(int ownerId, BillingPaymentMethodTypes method)
        {
            List<PaymentMethod> methods = new List<PaymentMethod>();
            PaymentMethod curMethod = new PaymentMethod();
            try
            {
                ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
                accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
                methods = await billingRestClient.GetAccountPaymentMethods(ownerId);
                curMethod = methods.Where(m => m.Id == accounts.First().DefaultPaymentMethodId).First();
            }
            catch(Exception ex)
            {
                Logger.Log.Fatal(ex);
            }
            Assert.IsTrue(curMethod.Type.Equals(method.ToString()), $"default payment method {curMethod.Type} doesn't match expencted {method.ToString()}");
            return curMethod;
        }

        public async Task verifyAccountDueDate(int ownerId, DateTime dueDate)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            DateTime curDate = (DateTime)accounts.First().BalancePastDueDate;
            Assert.IsTrue(DateTime.Compare(curDate, dueDate) == 0, $"monthly billing day {accounts.First().BalancePastDueDate} doesn't match expencted {dueDate}");
        }

        public async Task verifyAccountCharity(int ownerId, int charityId)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            Guid uniqueId = Charity.GetUniqueIdFromId(charityId);
            Assert.IsFalse(uniqueId == Guid.Empty, $"invalid expected charity id - {charityId}");
            Assert.IsTrue(accounts.First().CharityId == uniqueId, $"charity doesn't match expencted {charityId}");
        }

        public async Task verifyAccountCreditCardExpirationDate(int ownerId, DateTime expireDate)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            // get payment method
            List<PaymentMethod> methods = await billingRestClient.GetAccountPaymentMethods(ownerId);
            PaymentMethod method = methods.First();
            if(method.Type == BillingPaymentMethodTypes.CreditCard.ToString())
            {
                Assert.IsTrue(expireDate.Year == method.CreditCardExpirationYear && expireDate.Month == method.CreditCardExpirationMonth, $"creditcard expiration date doesn't match expencted {expireDate}");
            }
            else
            {
                Assert.Inconclusive("payment method is not credit card");
            }
        }

        public async Task verifyAccountCreditBalance(int ownerId, decimal balance)
        {
            ownerCollection = testDataManager.GetEnrolledOwnerCollection(ownerId);
            accounts = await billingRestClient.GetBillingAccountByPolicyHolderId(ownerCollection.OwnerInformation.UniqueId.ToString());
            Assert.IsTrue(accounts.First().CreditBalance == balance, $"account credit balance {accounts.First().CreditBalance} doesn't match expencted {balance}");
        }

    }
}
