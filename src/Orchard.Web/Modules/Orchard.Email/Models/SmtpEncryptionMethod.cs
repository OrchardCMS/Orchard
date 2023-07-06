namespace Orchard.Email.Models {
    /// <summary>
    /// Represents an enumeration for mail encryption methods.
    /// </summary>
    public enum SmtpEncryptionMethod {
        None = 0,
        SslTls = 1,
        StartTls = 2
    }
}