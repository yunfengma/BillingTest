//----------------------------------------------------------------------------------------------------------
// <copyright file="RestRequestSpecification.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingTestCommon.Methods
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Trupanion.TruFoundation.RestClient.Async;

    public class RestRequestSpecification : IRestRequestSpecification
    {
        public HttpMethod Verb { get; set; }
        public string RequestUri { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public object Content { get; set; }
        public string ContentType { get; set; }
    }
}
