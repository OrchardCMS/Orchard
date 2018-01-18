using System;
using System.Globalization;
using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    public class SecuritySettingsPart : ContentPart {

        public TimeSpan AuthCookieLifeSpan {
            get {
                var span = this.Retrieve<string>("AuthCookieLifeSpan");
                return string.IsNullOrEmpty(span)
                    ? TimeSpan.FromDays(30) // default value is 30 days
                    : TimeSpan.Parse(span, CultureInfo.InvariantCulture);
            }
            set { this.Store("AuthCookieLifeSpan", value.ToString()); }
        }
        
    }
}