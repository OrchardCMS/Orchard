using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Yaml.Serialization;
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

        class Content {
            public string Name { get; set; }
            public string DataProvider { get; set; }
            public string DataConnectionString { get; set; }
            public string DataPrefix { get; set; }
            public string RequestUrlHost { get; set; }
            public string RequestUrlPrefix { get; set; }
            public string State { get; set; }
        }

        static ShellSettings ParseSettings(string text) {
            var ser = new YamlSerializer();
            var content = ser.Deserialize(text, typeof(Content)).Cast<Content>().Single();
            return new ShellSettings {
                Name = content.Name,
                DataProvider = content.DataProvider,
                DataConnectionString = content.DataConnectionString,
                DataTablePrefix = content.DataPrefix,
                RequestUrlHost = content.RequestUrlHost,
                RequestUrlPrefix = content.RequestUrlPrefix,
                State = new TenantState(content.State)
            };
        }

        static string ComposeSettings(ShellSettings settings) {
            if ( settings == null )
                return "";

            var ser = new YamlSerializer();
            return ser.Serialize(new Content {
                Name = settings.Name,
                DataProvider = settings.DataProvider,
                DataConnectionString = settings.DataConnectionString,
                DataPrefix = settings.DataTablePrefix,
                RequestUrlHost = settings.RequestUrlHost,
                RequestUrlPrefix = settings.RequestUrlPrefix,
                State = settings.State != null ? settings.State.ToString() : String.Empty
            });
        }
    }
}
