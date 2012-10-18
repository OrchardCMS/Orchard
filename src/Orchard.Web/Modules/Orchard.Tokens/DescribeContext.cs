using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Tokens {
    public abstract class DescribeContext {
        public abstract IEnumerable<TokenTypeDescriptor> Describe(params string[] targets);
        public abstract DescribeFor For(string target);
        public abstract DescribeFor For(string target, LocalizedString name, LocalizedString description);
    }
}