using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.ImportExport.Services {
    [OrchardFeature("Orchard.Deployment")]
    public class SigningService : ISigningService {
        private readonly ShellSettings _shellSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;

        public SigningService(
            ShellSettings shellSettings,
            ICacheManager cacheManager,
            IClock clock
            ) {
            _shellSettings = shellSettings;
            _cacheManager = cacheManager;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public string AuthenticationHeaderName {
            get { return "Authentication"; }
        }

        public string TimestampHeaderName {
            get { return "Timestamp"; }
        }

        public string ContentHashHeaderName {
            get { return "ContentHash"; }
        }

        public string TimestampFormat {
            get { return "yyyy-MM-dd HH:mm:ss.fff"; }
        }

        public string SignRequest(string methodType, string timestamp, string uri, string secret) {
            var message = BuildBaseString(methodType, timestamp, uri);
            return ComputeHash(secret, message);
        }

        public bool ValidateRequest(string methodType, string timestamp, string uri, string secret, string signature) {
            var message = BuildBaseString(methodType, timestamp, uri);
            return IsDateValidated(timestamp) && IsAuthenticated(secret, message, signature);
        }

        public string SignContent(string content, string timestamp, string secret) {
            var timestampedContent = timestamp + content;
            return ComputeHash(secret, timestampedContent);
        }

        public bool ValidateContent(string content, string timestamp, string secret, string signature) {
            var timestampedContent = timestamp + content;
            return IsDateValidated(timestamp) && IsAuthenticated(secret, timestampedContent, signature);
        }

        private string ComputeHash(string secret, string message) {
            var key = Encoding.UTF8.GetBytes(secret.ToUpper());
            string hashString;

            using (var hmac = HMAC.Create(_shellSettings.HashAlgorithm)) {
                hmac.Key = key;
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                hashString = Convert.ToBase64String(hash);
            }

            return hashString;
        }

        private static string BuildBaseString(string methodType, string timestamp, string uri) {
            var message = string.Join("\n", methodType.ToUpperInvariant(), timestamp, uri.ToLowerInvariant());
            return message;
        }

        private bool IsAuthenticated(string secret, string message, string signature) {
            if (string.IsNullOrEmpty(secret))
                return false;

            var key = "SigningSignature:" + signature;
            var token = _clock.UtcNow;
            var cachedToken = _cacheManager.Get(key, ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(10)));
                return token;
            });

            if (token != cachedToken) {
                //This signature has been cached earlier. Signatures can only be used once.
                return false;
            }

            var verifiedHash = ComputeHash(secret, message);
            return signature != null && signature.Equals(verifiedHash);
        }

        private bool IsDateValidated(string timestampString) {
            DateTime timestamp;

            var isDateTime = DateTime.TryParse(timestampString, null,
                DateTimeStyles.AssumeUniversal, out timestamp);

            if (!isDateTime) return false;

            var now = _clock.UtcNow;
            timestamp = timestamp.ToUniversalTime();

            if (timestamp < now.AddMinutes(-5)) return false;

            return timestamp <= now.AddMinutes(5);
        }
    }
}