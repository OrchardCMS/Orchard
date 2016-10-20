namespace Orchard.OpenId {
    public class Constants {
        public const string AuthenticationErrorUrl = "/Authentication/Error";
        public const string LogonCallbackUrl = "/Users/Account/Logoncallback";
        public const string OpenIdOwinMiddlewarePriority = "10";

        public const string AzureActiveDirectoryObjectIdentifierKey = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        // Certificate Subject Key Identifier
        public const string VeriSignClass3SecureServerCA_G2 = "A5EF0B11CEC04103A34A659048B21CE0572D7D47";
        public const string VeriSignClass3SecureServerCA_G3 = "0D445C165344C1827E1D20AB25F40163D8BE79A5";
        public const string VeriSignClass3PublicPrimaryCA_G5 = "7FD365A7C2DDECBBF03009F34339FA02AF333133";
        public const string SymantecClass3SecureServerCA_G4 = "39A55D933676616E73A761DFA16A7E59CDE66FAD";
        public const string DigiCertSHA2HighAssuranceServerCA = "5168FF90AF0207753CCCD9656462A212B859723B";
        public const string DigiCertHighAssuranceEVRootCA = "B13EC36903F8BF4701D498261A0802EF63642BC3";
    }
}