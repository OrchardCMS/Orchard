using System;
using System.Linq;

namespace Orchard {
    public static class OrchardConvert {
        public static string ToHexString(this byte[] bytes) {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] FromHexString(this string hex) {
            return Enumerable.Range(0, hex.Length).
                Where(x => 0 == x % 2).
                Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                ToArray();
        }
    }
}
