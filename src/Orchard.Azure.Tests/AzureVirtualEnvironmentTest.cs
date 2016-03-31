using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;

namespace Orchard.Azure.Tests {
    public abstract class AzureVirtualEnvironmentTest {
        private Process _storageEmulator;

        protected abstract void OnInit();

        
        [TestFixtureSetUp]
        public void FixtureSetup() {
            if (!Process.GetProcessesByName("AzureStorageEmulator").Any()) {
                var azureSDKPath = ConfigurationManager.AppSettings["AzureSDK"];
                var storageEmulatorRelativePath = "Storage Emulator\\AzureStorageEmulator.exe";

                if (String.IsNullOrEmpty(azureSDKPath)) {
                    throw new ConfigurationErrorsException("Could not find the AppSetting \"AzureSDK\" that indicates the path to the Azure SDK on the local file system.");
                }

                var storageEmulatorAbsolutePath = Path.Combine(azureSDKPath, storageEmulatorRelativePath);

                if (!File.Exists(storageEmulatorAbsolutePath)) {
                    throw new ConfigurationErrorsException("Could not find the executable to start the Azure Storage Emulator.");
                }

                var storageEmulatorStartInfo = new ProcessStartInfo {
                    Arguments = "start",
                    FileName = storageEmulatorAbsolutePath
                };

                _storageEmulator = new Process { StartInfo = storageEmulatorStartInfo };
                _storageEmulator.Start();
                _storageEmulator.WaitForExit();
            }

            OnInit();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {
            if (_storageEmulator != null)
                _storageEmulator.Close();
        }

        protected void DeleteAllBlobs(string containerName, CloudStorageAccount account) {
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            foreach (var blob in container.ListBlobs()) {
                if (blob is CloudBlob) {
                    ((CloudBlob)blob).DeleteIfExists();
                }

                if (blob is CloudBlobDirectory) {
                    DeleteAllBlobs((CloudBlobDirectory)blob);
                }
            }
        }

        private static void DeleteAllBlobs(CloudBlobDirectory cloudBlobDirectory) {
            foreach (var blob in cloudBlobDirectory.ListBlobs()) {
                if (blob is CloudBlob) {
                    ((CloudBlob)blob).DeleteIfExists();
                }

                if (blob is CloudBlobDirectory) {
                    DeleteAllBlobs((CloudBlobDirectory)blob);
                }
            }
        }
    }
}
