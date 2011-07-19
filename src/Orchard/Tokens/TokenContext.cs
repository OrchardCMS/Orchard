namespace Orchard.Tokens {
    public class TokenContext {
        public int Offset { get; set; }
        public int Length { get; set; }
        public TokenDescriptor Token { get; set; }
        public object Replacement { get; set; }
    }
}
