using System.Configuration;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace Orchard.Azure.Tests {
    public abstract class AzureVirtualEnvironmentTest {
        private Process _dsService;

        protected abstract void OnInit();

        [TestFixtureSetUp]
        public void FixtureSetup() {
            var count = Process.GetProcessesByName("DSService").Length;
            if ( count == 0 ) {
                var start = new ProcessStartInfo {
                    Arguments = "/devstore:start",
                    FileName = Path.Combine(ConfigurationManager.AppSettings["AzureSDK"], @"emulator\csrun.exe")
                };

                _dsService = new Process { StartInfo = start };
                _dsService.Start();
                _dsService.WaitForExit();
            }

            OnInit();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {

            if ( _dsService != null )
                _dsService.Close();
        }

        protected void DeleteAllBlobs(string containerName, CloudStorageAccount account)
        {
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            foreach ( var blob in container.ListBlobs() ) {
                if ( blob is CloudBlob ) {
                    ( (CloudBlob)blob ).DeleteIfExists();
                }

                if ( blob is CloudBlobDirectory ) {
                    DeleteAllBlobs((CloudBlobDirectory)blob);
                }
            }
        }

        private static void DeleteAllBlobs(CloudBlobDirectory cloudBlobDirectory) {
            foreach ( var blob in cloudBlobDirectory.ListBlobs() ) {
                if ( blob is CloudBlob ) {
                    ( (CloudBlob)blob ).DeleteIfExists();
                }

                if ( blob is CloudBlobDirectory ) {
                    DeleteAllBlobs((CloudBlobDirectory)blob);
                }
            }
        }
    }
}
