namespace Orchard.Tokens {
    public interface ITokenManager : IDependency {
        TokenTable GetTokenTable();
    }
}