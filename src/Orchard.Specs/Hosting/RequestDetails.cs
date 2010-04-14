using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Specs.Hosting
{
    [Serializable]
    public class RequestDetails
    {
        public string Page { get; set; }
        public string Query { get; set; }

        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ResponseText { get; set; }
    }
}
