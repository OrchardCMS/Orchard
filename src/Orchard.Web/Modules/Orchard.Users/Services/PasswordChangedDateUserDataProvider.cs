using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Settings;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class PasswordChangedDateUserDataProvider : BaseUserDataProvider {


        private readonly ISiteService _siteService;

        public PasswordChangedDateUserDataProvider(
            ISiteService siteService) : base(true) {
            // By calling base(true) we set DefaultValid to true. This means that cookies whose
            // UserData dictionary does not contain the entry from this provider will be valid.

            _siteService = siteService;
        }

        protected override bool DefaultValid {
            get {
                return !_siteService
                    .GetSiteSettings()
                    .As<SecuritySettingsPart>()
                    .ShouldInvalidateAuthOnPasswordChanged;
            }
        }

        public override bool IsValid(IUser user, IDictionary<string, string> userData) {
            
            return DefaultValid || base.IsValid(user, userData);
        }

        protected override string Value(IUser user) {
            var part = user.As<UserPart>();
            if (part == null) {
                return string.Empty;
            }

            var date = GetDate(part);
            return date.ToString();
        }

        private DateTime GetDate(UserPart part) {
            // CreatedUTC should never require a default value to fallback to.
            var created = DateOrDefault(part.CreatedUtc);
            // LastPasswordChangeUtc may require a value to fallback to for users that have not changed their
            // password since migration UpdateFrom4()
            var changed = DateOrDefault(part.LastPasswordChangeUtc);

            // Return the most recent of the two dates
            return created > changed
                ? created
                : changed;
        }

        private DateTime DateOrDefault(DateTime? date) {
            return date.HasValue
                ? date.Value
                : new DateTime(1990, 1, 1);// Just a default value.
        }
    }
}