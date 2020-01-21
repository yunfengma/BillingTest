//-----------------------------------------------------------------------
// <copyright file="BillingTestBase.cs" company="Trupanion">
// Copyright(c) 2019 by Trupanion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Trupanion.Billing.Test
{
    using BillingTestCommon;
    using DataManagers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using TruFoundation.TestTools;
    using Trupanion.Billing.Api.Accounts.V2;
    using Trupanion.Billing.Test.DataVerifiers;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Enrollment;
    using Trupanion.Test.QALib.WebServices.Contracts.DatabaseAccess.Models.Projections.Quote;
    using Trupanion.Test.QALib.WebServices.Contracts.Enrollment;
    using Trupanion.TruFoundation;
    using Trupanion.TruFoundation.IoC;
    using Trupanion.TruFoundation.Serialization;

    public class BillingTestBase : BaseIntegrationTests
    {
        public ITestDataManager testDataManager = null;

        public IJsonSerialization serializer = null;

        public QALibRestClient qaLibRestClient { get; set; }
        public OwnerCollection ownerCollection { get; set; }
        public EnrollmentParameters iep { get; set; }
        public QaLibQuoteResponse quote { get; set; }
        public Account accountExpected { get; set; }
        public int ownerId { get; set; }
        public decimal premium { get; set; }
        public Random random { get; set; }


        //protected ITestDataManager TestDataManager
        //{
        //    get
        //    {
        //        return testDataManager;
        //    }
        //}

        public void InitTestClass()
        {
            try
            {
                ServiceFactory.InitializeServiceFactory(new ContainerConfiguration(ApplicationProfileType.TestFramework));
                testDataManager = IocContainer.Resolve<ITestDataManager>();

                serializer = ServiceFactory.Instance.Create<IJsonSerialization>();

                qaLibRestClient = new QALibRestClient();
                ownerCollection = new OwnerCollection();
                accountExpected = new Account();
                accountExpected.AutoPay = true;

                random = new Random();
            }
            catch (Exception ex)
            {
                BillingTestCommon.log.Fatal(ex);
            }
            Assert.IsNotNull(testDataManager);
        }

    }
}
