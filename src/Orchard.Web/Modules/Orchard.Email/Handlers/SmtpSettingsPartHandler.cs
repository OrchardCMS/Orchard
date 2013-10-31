using System;
using System.Text;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard.Email.Handlers {
    [UsedImplicitly]
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
                    return String.IsNullOrWhiteSpace(part.Password) ? String.Empty : Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(part.Password)));
                }
                catch {
                    Logger.Error("The email password could not be decrypted. It might be corrupted, try to reset it.");
                    return null;
                }
            });

            part.PasswordField.Setter(value => part.Password = String.IsNullOrWhiteSpace(value) ? String.Empty : Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(value))));
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