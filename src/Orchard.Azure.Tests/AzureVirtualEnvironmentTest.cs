using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;
using Orchard.Azure.Services.Environment.Configuration;

namespace Orchard.Azure.Tests
{
    public abstract class AzureVirtualEnvironmentTest
    {
        protected IPlatformConfigurationAccessor PlatformConfigurationAccessor = new DefaultPlatformConfigurationAccessor();
        protected abstract string StorageConnectionStringName { get; }
        protected CloudStorageAccount DevAccount { get; private set; }

        protected abstract void OnInit();

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            var connectionString = PlatformConfigurationAccessor.GetSetting(StorageConnectionStringName, "default", "");

            CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount devAccount);
            DevAccount = devAccount;

            OnInit();
        }

        protected void DeleteAllBlobs(string containerName, CloudStorageAccount account)
        {
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            foreach (var blob in container.ListBlobs())
            {
                if (blob is CloudBlob blobLeaf)
                {
                    blobLeaf.DeleteIfExists();
                }

                if (blob is CloudBlobDirectory directory)
                {
                    DeleteAllBlobs(directory);
                }
            }
        }

        private static void DeleteAllBlobs(CloudBlobDirectory cloudBlobDirectory)
        {
            foreach (var blob in cloudBlobDirectory.ListBlobs())
            {
                if (blob is CloudBlob blobLeaf)
                {
                    blobLeaf.DeleteIfExists();
                }

                if (blob is CloudBlobDirectory directory)
                {
                    DeleteAllBlobs(directory);
                }
            }
        }
    }
}
