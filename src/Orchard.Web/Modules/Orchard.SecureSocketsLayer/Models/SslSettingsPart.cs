using System;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.SecureSocketsLayer.Models {
    public class SslSettings {
        public bool Enabled { get; set; }
        public string Urls { get; set; }
        public bool SecureEverything { get; set; }
        public bool CustomEnabled { get; set; }
        public string SecureHostName { get; set; }
        public string InsecureHostName { get; set; }
    }

    public class SslSettingsPart : ContentPart {
        public const string CacheKey = "SslSettingsPart";

        public string Urls {
            get { return this.As<InfosetPart>().Get<SslSettingsPart>("Urls"); }
            set { this.As<InfosetPart>().Set<SslSettingsPart>("Urls", value); }
        }

        public bool SecureEverything {
            get {
                var attributeValue = this.As<InfosetPart>().Get<SslSettingsPart>("SecureEverything");
                return !String.IsNullOrWhiteSpace(attributeValue) && Convert.ToBoolean(attributeValue);
            }
            set { this.As<InfosetPart>().Set<SslSettingsPart>("SecureEverything", value.ToString()); }
        }

        public bool Enabled {
            get {
                var attributeValue = this.As<InfosetPart>().Get<SslSettingsPart>("Enabled");
                return !String.IsNullOrWhiteSpace(attributeValue) && Convert.ToBoolean(attributeValue);
            }
            set { this.As<InfosetPart>().Set<SslSettingsPart>("Enabled", value.ToString()); }
        }

        public bool CustomEnabled {
            get {
                var attributeValue = this.As<InfosetPart>().Get<SslSettingsPart>("CustomEnabled");
                return !String.IsNullOrWhiteSpace(attributeValue) && Convert.ToBoolean(attributeValue);
            }
            set { this.As<InfosetPart>().Set<SslSettingsPart>("CustomEnabled", value.ToString()); }
        }

        [Required]
        public string SecureHostName {
            get { return this.As<InfosetPart>().Get<SslSettingsPart>("SecureHostName"); }
            set { this.As<InfosetPart>().Set<SslSettingsPart>("SecureHostName", value); }
        }

        [Required]
        public string InsecureHostName {
            get { return this.As<InfosetPart>().Get<SslSettingsPart>("InsecureHostName"); }
            set { this.As<InfosetPart>().Set<SslSettingsPart>("InsecureHostName", value); }
        }

        public bool SendStrictTransportSecurityHeaders {
            get { return this.Retrieve(x => x.SendStrictTransportSecurityHeaders); }
            set { this.Store(x => x.SendStrictTransportSecurityHeaders, value); }
        }

        public bool StrictTransportSecurityIncludeSubdomains {
            get { return this.Retrieve(x => x.StrictTransportSecurityIncludeSubdomains); }
            set { this.Store(x => x.StrictTransportSecurityIncludeSubdomains, value); }
        }

        [Required]
        public int StrictTransportSecurityMaxAge {
            get { return this.Retrieve(x => x.StrictTransportSecurityMaxAge, 31536000); }
            set { this.Store(x => x.StrictTransportSecurityMaxAge, value); }
        }

        public bool StrictTransportSecurityPreload {
            get { return this.Retrieve(x => x.StrictTransportSecurityPreload); }
            set { this.Store(x => x.StrictTransportSecurityPreload, value); }
        }
    }
}