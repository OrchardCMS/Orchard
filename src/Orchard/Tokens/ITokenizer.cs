using System.Collections.Generic;

namespace Orchard.Tokens {
    public interface ITokenizer : IDependency {
        IEnumerable<TokenContext> ParseTokens(string str, object contexts);
        IEnumerable<TokenContext> ParseTokens(string str, IDictionary<string, object> contexts);
        string Replace(IEnumerable<TokenContext> tokens, string str);
        string Replace(string str, object contexts);
        string Replace(string str, IDictionary<string, string> contexts);
    }

}
