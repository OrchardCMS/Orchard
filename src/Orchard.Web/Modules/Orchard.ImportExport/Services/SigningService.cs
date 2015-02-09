using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public string SignContent(byte[] content, string timestamp, string secret) {
            var timestampedContent = Encoding.UTF8.GetBytes(timestamp).Concat(content).ToArray();
            return ComputeHash(secret, timestampedContent);
        }

        public string SignContent(Stream content, string timestamp, string secret) {
            var datedStream = new CompositeStream(Encoding.UTF8.GetBytes(timestamp), content);
            return ComputeHash(secret, datedStream);
        }

        public bool ValidateContent(string content, string timestamp, string secret, string signature) {
            var timestampedContent = timestamp + content;
            return IsDateValidated(timestamp) && IsAuthenticated(secret, timestampedContent, signature);
        }

        public bool ValidateContent(byte[] content, string timestamp, string secret, string signature) {
            var timestampedContent = Encoding.UTF8.GetBytes(timestamp).Concat(content).ToArray();
            return IsDateValidated(timestamp) && IsAuthenticated(secret, timestampedContent, signature);
        }

        public bool ValidateContent(Stream content, string timestamp, string secret, string signature) {
            var datedStream = new CompositeStream(Encoding.UTF8.GetBytes(timestamp), content);
            return IsDateValidated(timestamp) && IsAuthenticated(secret, datedStream, signature);
        }

        private string ComputeHash(string secret, string message) {
            return ComputeHash(secret, Encoding.UTF8.GetBytes(message));
        }

        private string ComputeHash(string secret, byte[] message) {
            var key = Encoding.UTF8.GetBytes(secret.ToUpper());
            string hashString;

            using (var hmac = HMAC.Create(_shellSettings.HashAlgorithm)) {
                hmac.Key = key;
                var hash = hmac.ComputeHash(message);
                hashString = Convert.ToBase64String(hash);
            }

            return hashString;
        }

        private string ComputeHash(string secret, Stream message) {
            var key = Encoding.UTF8.GetBytes(secret.ToUpper());
            string hashString;
            var previousPosition = message.Position;

            using (var hmac = HMAC.Create(_shellSettings.HashAlgorithm)) {
                hmac.Key = key;
                var hash = hmac.ComputeHash(message);
                hashString = Convert.ToBase64String(hash);
            }

            message.Seek(previousPosition, SeekOrigin.Begin);
            return hashString;
        }

        private static string BuildBaseString(string methodType, string timestamp, string uri) {
            var message = string.Join("\n", methodType.ToUpperInvariant(), timestamp, uri.ToLowerInvariant());
            return message;
        }

        private bool IsAuthenticated(string secret, string message, string signature) {
            if (!PerformSecretAndDateVerification(secret, signature)) return false;

            var verifiedHash = ComputeHash(secret, message);
            return signature != null && signature.Equals(verifiedHash);
        }

        private bool IsAuthenticated(string secret, byte[] message, string signature) {
            if (!PerformSecretAndDateVerification(secret, signature)) return false;

            var verifiedHash = ComputeHash(secret, message);
            return signature != null && signature.Equals(verifiedHash);
        }

        private bool IsAuthenticated(string secret, Stream message, string signature) {
            if (!PerformSecretAndDateVerification(secret, signature)) return false;

            var verifiedHash = ComputeHash(secret, message);
            return signature != null && signature.Equals(verifiedHash);
        }

        private bool PerformSecretAndDateVerification(string secret, string signature) {
            if (string.IsNullOrEmpty(secret)) {
                return false;
            }

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
            return true;
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

        private class CompositeStream : Stream {
            private readonly byte[] _header;
            private readonly Stream _stream;

            public CompositeStream(byte[] header, Stream stream) {
                _header = header;
                _stream = stream;
            }

            public override void Flush() {
                _stream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin) {
                switch (origin) {
                    case SeekOrigin.Begin:
                        return Goto(offset);
                    case SeekOrigin.Current:
                        return Goto(Position + offset);
                    case SeekOrigin.End:
                        return Goto(_header.Length + _stream.Length - 1 + offset);
                }
                Debug.Assert(false, "Unrecognized stream origin.");
                return 0;
            }

            private long Goto(long offset) {
                if (offset <= _header.Length) {
                    var newPos = _stream.Seek(0, SeekOrigin.Begin);
                    if (newPos > 0) {
                        return Position = newPos + _header.Length;
                    }
                    return Position = offset > 0 ? offset : 0;
                }
                else {
                    var newPos = _stream.Seek(offset - _header.Length, SeekOrigin.Begin);
                    return Position = newPos + _header.Length;
                }
            }

            public override void SetLength(long value) {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count) {
                var bytesRead = 0;
                if (Position < _header.Length) {
                    var bytesToReadFromHeader = Position + count < _header.Length
                        ? count
                        : _header.Length - (int)Position;
                    for (var i = 0; i < bytesToReadFromHeader; i++) {
                        buffer[offset + i] = _header[Position + i];
                    }
                    bytesRead += bytesToReadFromHeader;
                    Position += bytesToReadFromHeader;
                }
                if (bytesRead >= count) return bytesRead;
                var readFromStream = _stream.Read(buffer, offset + bytesRead, count - bytesRead);
                Position += readFromStream;
                return bytesRead + readFromStream;
            }

            public override void Write(byte[] buffer, int offset, int count) {
                _stream.Write(buffer, offset, count);
            }

            public override bool CanRead {
                get { return _stream.CanRead; }
            }

            public override bool CanSeek {
                get { return _stream.CanSeek; }
            }

            public override bool CanWrite {
                get { return _stream.CanWrite; }
            }

            public override long Length {
                get { return _stream.Length + _header.Length; }
            }

            public override long Position { get; set; }
        }
    }
}