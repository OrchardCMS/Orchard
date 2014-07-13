namespace Orchard.Azure.MediaServices.Helpers {
    public static class BitExtensions {
        public static int ToKiloBits(this int bits) {
            return bits/1000;
        }
    }
}