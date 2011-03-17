using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Orchard.Environment.Warmup {
    public static class WarmupUtility {
        private const string EncodingPattern = "[^a-z0-9]";

        public static string EncodeUrl(string url) {
            if(String.IsNullOrWhiteSpace(url)) {
                throw new ArgumentException("url can't be empty");
            }

            return Regex.Replace(url.ToLower(), EncodingPattern, m => "_" + Encoding.UTF8.GetBytes(m.Value).Select(b => b.ToString("X")).Aggregate((a, b) => a + b));
        }

    }
}
