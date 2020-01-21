//----------------------------------------------------------------------------------------------------------
// <copyright file="TestCategoryTypes.cs" company="Trupanion">
//    Copyright(c) 2020 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingTestCommon.Models
{
    using System.ComponentModel;

    public enum TestCategoryTypes : int
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("All")]
        All,
        [Description("BVT")]
        BVT,
        [Description("Enroll")]
        Enroll,
        [Description("Testing")]
        Testing
    }
}
