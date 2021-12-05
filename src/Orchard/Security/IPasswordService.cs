namespace Orchard.Security {
    public interface IPasswordService : IDependency {
        bool Equals(PasswordContext context, string plaintextPassword);
        //string ComputeHashBase64(string hashAlgorithmName, byte[] saltBytes, string password);
    }
}