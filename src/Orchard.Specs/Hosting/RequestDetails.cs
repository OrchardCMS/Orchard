using System;
using System.Collections.Generic;

namespace Orchard.Specs.Hosting {
    [Serializable]
    public class RequestDetails {
        public RequestDetails() {
            RequestHeaders = new Dictionary<string, string>();
            ResponseHeaders = new Dictionary<string, string>();
        }

        public string HostName { get; set; }
        public string UrlPath { get; set; }
        public string Page { get; set; }
        public string Query { get; set; }
        public byte[] PostData { get; set; }

        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ResponseText { get; set; }

        public IDictionary<string, string> RequestHeaders { get; set; }
        public IDictionary<string, string> ResponseHeaders { get; set; }
    }
}
