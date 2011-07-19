using System.Collections.Generic;

namespace Orchard.Tokens {
    public class DefaultTokenManager : ITokenManager {
        private readonly IEnumerable<ITokenProvider> _tokenProviders;

        public DefaultTokenManager(IEnumerable<ITokenProvider> tokenProviders) {
            _tokenProviders = tokenProviders;
        }

        public TokenTable GetTokenTable() {
            var builder = new TokenBuilder();
            foreach (var provider in _tokenProviders) {
                provider.BuildTokens(builder);
            }
            return builder.BuildTokens();
        }
    }
}
