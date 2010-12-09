using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Orchard.Environment.Configuration;
using Orchard.Localization;

namespace Orchard.Azure.Environment.Configuration {

    public class AzureShellSettingsManager : IShellSettingsManager {
        public const string ContainerName = "sites"; // container names must be lower cased
        private readonly IShellSettingsManagerEventHandler _events;

        private readonly CloudStorageAccount _storageAccount;
        public CloudBlobClient BlobClient { get; private set; }
        public CloudBlobContainer Container { get; private set; }

        Localizer T { get; [UsedImplicitly]
        set; }

        public AzureShellSettingsManager(IShellSettingsManagerEventHandler events)
            : this(CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString")), events)
        {
        }

        public AzureShellSettingsManager(CloudStorageAccount storageAccount, IShellSettingsManagerEventHandler events)
        {
            // Setup the connection to custom storage accountm, e.g. Development Storage
            _storageAccount = storageAccount;
            _events = events;

            using ( new HttpContextWeaver() ) {
                BlobClient = _storageAccount.CreateCloudBlobClient();

                // Get and create the container if it does not exist
                // The container is named with DNS naming restrictions (i.e. all lower case)
                Container = new CloudBlobContainer(ContainerName, BlobClient);
                Container.CreateIfNotExist();

                // Tenant settings are protected by default
                Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });
            }

        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            return LoadSettings().ToArray();
        }

        void IShellSettingsManager.SaveSettings(ShellSettings settings) {
            if ( settings == null )
                throw new ArgumentException(T("There are no settings to save.").ToString());
            
            if ( string.IsNullOrEmpty(settings.Name) )
                throw new ArgumentException(T("Settings \"Name\" is not set.").ToString());

            using ( new HttpContextWeaver() ) {
                var filePath = String.Concat(settings.Name, "/", "Settings.txt");
                var blob = Container.GetBlockBlobReference(filePath);
                blob.UploadText(ComposeSettings(settings));
            }

            _events.Saved(settings);
        }

        IEnumerable<ShellSettings> LoadSettings() {

            using ( new HttpContextWeaver() ) {
                var settingsBlobs =
                    BlobClient.ListBlobsWithPrefix(Container.Name + "/").OfType<CloudBlobDirectory>()
                        .SelectMany(directory => directory.ListBlobs()).OfType<CloudBlockBlob>()
                        .Where(
                            blob =>
                            string.Equals(Path.GetFileName(blob.Uri.ToString()),
                                          "Settings.txt",
                                          StringComparison.OrdinalIgnoreCase));

                return settingsBlobs.Select(settingsBlob => ParseSettings(settingsBlob.DownloadText())).ToList();
            }
        }

        static ShellSettings ParseSettings(string text)
        {
            var shellSettings = new ShellSettings();
            if (String.IsNullOrEmpty(text))
                return shellSettings;

            string[] settings = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var setting in settings)
            {
                string[] settingFields = setting.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                int fieldsLength = settingFields.Length;
                if (fieldsLength != 2)
                    continue;
                for (int i = 0; i < fieldsLength; i++)
                {
                    settingFields[i] = settingFields[i].Trim();
                }
                if (settingFields[1] != "null")
                {
                    switch (settingFields[0])
                    {
                        case "Name":
                            shellSettings.Name = settingFields[1];
                            break;
                        case "DataProvider":
                            shellSettings.DataProvider = settingFields[1];
                            break;
                        case "State":
                            shellSettings.State = new TenantState(settingFields[1]);
                            break;
                        case "DataConnectionString":
                            shellSettings.DataConnectionString = settingFields[1];
                            break;
                        case "DataPrefix":
                            shellSettings.DataTablePrefix = settingFields[1];
                            break;
                        case "RequestUrlHost":
                            shellSettings.RequestUrlHost = settingFields[1];
                            break;
                        case "RequestUrlPrefix":
                            shellSettings.RequestUrlPrefix = settingFields[1];
                            break;
                        case "EncryptionAlgorithm":
                            shellSettings.EncryptionAlgorithm = settingFields[1];
                            break;
                        case "EncryptionKey":
                            shellSettings.EncryptionKey = settingFields[1];
                            break;
                        case "EncryptionIV":
                            shellSettings.EncryptionIV = settingFields[1];
                            break;
                    }
                }
            }
            return shellSettings;
        }

        static string ComposeSettings(ShellSettings settings)
        {
            if (settings == null)
                return "";

            return string.Format("Name: {0}\r\nDataProvider: {1}\r\nDataConnectionString: {2}\r\nDataPrefix: {3}\r\nRequestUrlHost: {4}\r\nRequestUrlPrefix: {5}\r\nState: {6}\r\nEncryptionAlgorithm: {7}\r\nEncryptionKey: {8}\r\nEncryptionIV: {9}\r\n",
                     settings.Name,
                     settings.DataProvider,
                     settings.DataConnectionString ?? "null",
                     settings.DataTablePrefix ?? "null",
                     settings.RequestUrlHost ?? "null",
                     settings.RequestUrlPrefix ?? "null",
                     settings.State != null ? settings.State.ToString() : String.Empty,
                     settings.EncryptionAlgorithm ?? "null",
                     settings.EncryptionKey ?? "null",
                     settings.EncryptionIV ?? "null"
                     );
        }
    }
}
