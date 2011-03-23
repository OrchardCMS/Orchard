using System;
using System.Text;

namespace Orchard.Environment.Warmup {
    public static class WarmupUtility {
        public static string EncodeUrl(string url) {
            if(String.IsNullOrWhiteSpace(url)) {
                throw new ArgumentException("url can't be empty");
            }

            var sb = new StringBuilder();
            foreach (var c in url.ToLowerInvariant()) {
                // only accept alphanumeric chars
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) {
                    sb.Append(c);
                }
                // otherwise encode them in UTF8
                else {
                    sb.Append("_");
                    foreach(var b in Encoding.UTF8.GetBytes(new [] {c})) {
                        sb.Append(b.ToString("X"));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
