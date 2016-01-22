using System;
using System.Collections.Generic;

namespace Orchard {
    public class OrchardParameters : MarshalByRefObject {
        public bool Verbose { get; set; }
        public string VirtualPath { get; set; }
        public string WorkingDirectory { get; set; }
        public string Tenant { get; set; }
        public IList<string> Arguments { get; set; }
        public IList<string> ResponseFiles { get; set; }
        public IDictionary<string, string> Switches { get; set; }

        public override object InitializeLifetimeService() {
            // never expire the cross-AppDomain lease on this object
            return null;
        }
    }
}