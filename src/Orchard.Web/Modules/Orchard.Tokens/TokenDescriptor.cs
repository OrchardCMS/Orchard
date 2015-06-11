using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Tokens {
    public class TokenDescriptor {
        public string Target { get; set; }
        public string Token { get; set; }
        public string ChainTarget { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
    }

    public class TokenTypeDescriptor {
        public string Target { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public IEnumerable<TokenDescriptor> Tokens { get; set; }
    }
}