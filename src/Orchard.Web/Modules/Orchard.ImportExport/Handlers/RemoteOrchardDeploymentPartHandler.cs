using System;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard.ImportExport.Handlers {
    [UsedImplicitly]
    [OrchardFeature("Orchard.Deployment")]
    public class RemoteOrchardDeploymentPartHandler : ContentHandler {
        private readonly IEncryptionService _encryptionService;

        public RemoteOrchardDeploymentPartHandler(IRepository<RemoteOrchardDeploymentPartRecord> repository,
            IEncryptionService encryptionService) {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            Filters.Add(StorageFilter.For(repository));

            _encryptionService = encryptionService;
            OnActivated<RemoteOrchardDeploymentPart>(LazyLoadHandlers);
        }

        public new ILogger Logger { get; set; }
        public Localizer T { get; set; }

        private void LazyLoadHandlers(ActivatedContentContext context, RemoteOrchardDeploymentPart part) {
            part.PrivateApiKeyField.Getter(() => {
                try {
                    return String.IsNullOrWhiteSpace(part.Record.PrivateApiKey) ?
                        String.Empty : Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(part.Record.PrivateApiKey)));
                }
                catch (CryptographicException) {
                    LogDecryptionError();
                }
                catch (ArgumentException) {
                    LogDecryptionError();
                }
                catch (FormatException) {
                    LogDecryptionError();
                }
                return null;
            });

            part.PrivateApiKeyField.Setter(value => part.Record.PrivateApiKey = String.IsNullOrWhiteSpace(value) ?
                String.Empty : Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(value))));
        }

        private void LogDecryptionError() {
            Logger.Error(T("The remote orchard user password could not be decrypted. It might be corrupted, try to reset it.").Text);
        }
    }
}