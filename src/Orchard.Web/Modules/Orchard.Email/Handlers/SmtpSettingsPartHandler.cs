using System;
using System.Text;
using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using System.Configuration;

namespace Orchard.Email.Handlers {
    public class SmtpSettingsPartHandler : ContentHandler {
        private readonly IEncryptionService _encryptionService;

        public SmtpSettingsPartHandler(IEncryptionService encryptionService) {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            _encryptionService = encryptionService;
            Filters.Add(new ActivatingFilter<SmtpSettingsPart>("Site"));

            OnLoaded<SmtpSettingsPart>(LazyLoadHandlers);

            OnInitializing<SmtpSettingsPart>((context, part) => {
                part.Port = 25;
                part.RequireCredentials = false;
                part.EnableSsl = false;
            });
        }

        public new ILogger Logger { get; set; }

        void LazyLoadHandlers(LoadContentContext context, SmtpSettingsPart part) {
            part.PasswordField.Getter(() => {
                try {
                    var encryptedPassword = part.Retrieve(x => x.Password);
                    return String.IsNullOrWhiteSpace(encryptedPassword) ? String.Empty : Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(encryptedPassword)));
                }
                catch {
                    Logger.Error("The email password could not be decrypted. It might be corrupted, try to reset it.");
                    return null;
                }
            });

            part.PasswordField.Setter(value => {
                var encryptedPassword = String.IsNullOrWhiteSpace(value) ? String.Empty : Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(value)));
                part.Store(x => x.Password, encryptedPassword);
            });

            part.AddressPlaceholderField.Loader(() => (string)((dynamic)ConfigurationManager.GetSection("system.net/mailSettings/smtp")).From);
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Email")));
        }
    }
}