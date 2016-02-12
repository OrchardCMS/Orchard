using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTS.TalaveraBase.Gravatar {

    internal class Gremail {

        private readonly string _Email;

        public string Email { get { return _Email; } }

        public Gremail(string email) {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException("email");
            _Email = email;
        }

        public string Hash() {
            var email = Email.Trim().ToLower();
            var encoder = new System.Text.UTF8Encoding();
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var hashedBytes = md5.ComputeHash(encoder.GetBytes(email));
            var sb = new System.Text.StringBuilder(hashedBytes.Length * 2);

            for (var i = 0; i < hashedBytes.Length; i++)
                sb.Append(hashedBytes[i].ToString("X2"));

            return sb.ToString().ToLower();
        }
    }
}
