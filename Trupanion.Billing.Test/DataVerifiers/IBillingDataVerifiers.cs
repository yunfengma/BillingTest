//----------------------------------------------------------------------------------------------------------
// <copyright file="IBillingDataVerifiers.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.DataVerifiers
{
    using BillingTestCommon.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Api.Invoices.V2;
    using Trupanion.LegacyPlatform.Constants;
    using Trupanion.Test.QALib.DatabaseAccess.Models.SprocParameters;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation.Services;
    using PaymentMethod = Api.PaymentMethods.V2.PaymentMethod;

    public interface IBillingDataVerifiers : IBusinessService
    {
        Task VerifyBillingAccount(int ownerIdn, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true);
        Task VerifyBillingAccountForConvertedTrialPolicy(int ownerId, IEnrollmentParameters iiep, bool bExist = true);
        Task VerifyBillingAccountForConvertedTrialPolicy(int ownerId, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true);
        Task<IReadOnlyList<Account>> VerifyBillingAccountExist(string policyHolderId, bool bExist = true);
        Task VerifyCanceledPetBillingInfo(int ownerId, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true);
        Task VerifyPendingCanceledPetBillingInfo(int ownerId, string petName, EnrollmentParameters iep, QaLibQuoteResponse quote, Account accountExpected, bool bExist = true);
        Task<InvoiceWithItems> VerifyAccountPremium(string accountId, EnrollmentParameters iep, QaLibQuoteResponse quote, bool bMatch = true);
        Task<InvoiceWithItems> VerifyCanceledAccountPremium(string accountId, EnrollmentParameters iep, QaLibQuoteResponse quote, bool bCanceld = true);
        //Task<InvoiceWithItems> GetAccountInvoicesNext(string policyHolderId);
        Task<List<PaymentMethod>> VerifyAccountPaymentMethod(string accountId, EnrollmentParameters iep);
        Task<decimal> GetExpectedDueBalance(Account account, int stateId, QaLibQuoteResponse quote);
        Task<bool> verifyOwnerPetEnrollmentStatus(int ownerId, string petName, EnrollmentStatus status = EnrollmentStatus.Enrolled);
        Task verifyAccountDueBalance(int ownerId, decimal balance);
        Task verifyAccountMonthlyBillingDay(int ownerId, int day);
        Task<PaymentMethod> verifyAccountDefaultPaymentMethod(int ownerId, BillingPaymentMethodTypes method);
        Task verifyAccountDueDate(int ownerId, DateTime dueDate);
        Task verifyAccountCharity(int ownerId, int charityId);
        Task verifyAccountCreditCardExpirationDate(int ownerId, DateTime expireDate);
        Task verifyAccountCreditBalance(int ownerId, decimal balance);
    }
}
