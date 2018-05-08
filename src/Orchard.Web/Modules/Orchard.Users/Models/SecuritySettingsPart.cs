using System;
using System.Globalization;
using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    public class SecuritySettingsPart : ContentPart {
        
        /// <summary>
        /// The way this setting works is that it controls the behaviour of 
        /// PasswordChangedDateUserDataProvider. If the setting is true, when the password
        /// changes all extant authentication tokens using that provider to generate UserData
        /// become invalid. If this setting is set to true, all authenticated user who have
        /// changed their password between the moment the setting is enabled and their last login
        /// (i.e. the last time they have been provided an authentication cookie) will be logged out,
        /// because the information in the cookie will fail to validate, but the LoggedOut event
        /// will not have fired for them.
        /// </summary>
        public bool ShouldInvalidateAuthOnPasswordChanged {
            get { return this.Retrieve(x => x.ShouldInvalidateAuthOnPasswordChanged); }
            set { this.Store(x => x.ShouldInvalidateAuthOnPasswordChanged, value); }
        }

    }
}