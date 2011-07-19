namespace Orchard.Tokens {
    public interface ITokenProvider : IDependency {
        void BuildTokens(TokenBuilder builder);
    }
}
