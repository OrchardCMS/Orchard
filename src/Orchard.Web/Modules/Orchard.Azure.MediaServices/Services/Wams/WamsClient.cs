using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Services.TempFiles;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;

namespace Orchard.Azure.MediaServices.Services.Wams {
    public class WamsClient : Component, IWamsClient {

        private readonly IOrchardServices _orchardServices;
        private readonly ITempFileManager _tempFileManager;
        private readonly Lazy<CloudMediaSettingsPart> _settings;
        private readonly Lazy<CloudMediaContext> _context;
        private CloudMediaSettingsPart Settings { get { return _settings.Value; } }
        private CloudMediaContext Context { get { return _context.Value; } }

        public WamsClient(IOrchardServices orchardServices, ITempFileManager tempFileManager) {
            _orchardServices = orchardServices;
            _tempFileManager = tempFileManager;
            _settings = new Lazy<CloudMediaSettingsPart>(() => _orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>());
            _context = new Lazy<CloudMediaContext>(() => new CloudMediaContext(_settings.Value.WamsAccountName, _settings.Value.WamsAccountKey));
        }

        public IMediaProcessor GetLatestMediaProcessorByName(MediaProcessorName mediaProcessorName) {
            Logger.Debug("GetLatestMediaProcessorByName() invoked for media processor '{0}'.", mediaProcessorName);

            var processorQuery =
                from p in Context.MediaProcessors
                where p.Name == mediaProcessorName.Name
                select p;

            var latestProcessor =
                (from p in processorQuery.ToArray()
                 orderby new Version(p.Version)
                 select p).LastOrDefault();

            if (latestProcessor == null)
                throw new ArgumentException(String.Format("Unknown media processor '{0}'.", mediaProcessorName));

            return latestProcessor;
        }

        public IAsset GetAssetById(string assetId) {
            Logger.Debug("GetAssetById() invoked with assetId value '{0}'.", assetId);
            return Context.Assets.Where(x => x.Id == assetId).ToArray().FirstOrDefault(); // ToArray() first because FirstOrDefault() is unsupported by provider.
        }

        public IEnumerable<IAsset> GetAssetsById(IEnumerable<string> assetIds) {
            Logger.Debug("GetAssetsById() invoked.");

            // The following throws a NotSupportedException.
            //return ctx.Jobs.Where(job => jobIds.Contains(job.Id));

            var assetsQuery =
                from assetId in assetIds
                select GetAssetById(assetId);

            return assetsQuery.ToArray();
        }

        public async Task<IAsset> UploadAssetAsync(string localFilePath, UpdateProgressAction updateProgress, Guid assetProgressMoniker, CancellationToken cancellationToken) {
            Logger.Debug("UploadAssetAsync() invoked with localFilePath value '{0}'.", localFilePath);

            IAsset asset = null;
            IAccessPolicy uploadAccessPolicy = null;

            try {
                var assetName = Guid.NewGuid().ToString();
                asset = await Context.Assets.CreateAsync(assetName, AssetCreationOptions.None, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                var assetFile = await asset.AssetFiles.CreateAsync(Path.GetFileName(localFilePath), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                assetFile.IsPrimary = true;

                uploadAccessPolicy = await Context.AccessPolicies.CreateAsync("Upload Policy", TimeSpan.FromDays(1), AccessPermissions.Write | AccessPermissions.List).ConfigureAwait(continueOnCapturedContext: false);
                var uploadLocator = await Context.Locators.CreateLocatorAsync(LocatorType.Sas, asset, uploadAccessPolicy).ConfigureAwait(continueOnCapturedContext: false);
                var uploadClient = new BlobTransferClient();

                uploadClient.TransferProgressChanged += (sender, e) => updateProgress(new WamsUploadProgressInfo(assetProgressMoniker, e));
                await assetFile.UploadAsync(localFilePath, uploadClient, uploadLocator, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                await uploadLocator.DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);
                await uploadAccessPolicy.DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);

                Logger.Information("New asset with ID '{0}' was uploaded from temp file with name '{1}'.", asset.Id, localFilePath);

                return asset;
            }
            catch (Exception ex) {
                if (ex is OperationCanceledException)
                    Logger.Information("Upload of asset with ID '{0}' from temp file with name '{1}' was canceled.", asset.Id, localFilePath);
                else
                    Logger.Error(ex, "Error while uploading asset from temp file with name '{0}'. Cleaning up asset and any locators and access policy created for upload.", localFilePath);

                try {
                    if (asset != null)
                        asset.Delete(); // Deletes any locators also.
                    if (uploadAccessPolicy != null)
                        uploadAccessPolicy.Delete();
                }
                catch (Exception iex) {
                    Logger.Warning(iex, "Error while cleaning up asset and any locators and access policy created for upload.");
                }

                throw;
            }
        }

        public async Task DeleteAssetAsync(IAsset asset) {
            Logger.Debug("DeleteAssetAsync() invoked for asset '{0}'.", asset.Name);

            try {
                var locators = asset.Locators.ToArray();
                var accessPolicies = Context.AccessPolicies.ToArray();

                var accessPolicyIdsToDelete =
                    from locator in locators
                    where locator.AccessPolicyId != null
                    select locator.AccessPolicyId;

                await asset.DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);

                var accessPolicyDeleteTasksQuery =
                    from accessPolicy in accessPolicies
                    where accessPolicyIdsToDelete.Contains(accessPolicy.Id)
                    select accessPolicy.DeleteAsync();

                await Task.WhenAll(accessPolicyDeleteTasksQuery).ConfigureAwait(continueOnCapturedContext: false);

                Logger.Information("Asset '{0}' was deleted.", asset.Name);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while deleting asset '{0}'.", asset.Name);
                throw;
            }
        }

        public async Task<WamsLocators> CreateLocatorsAsync(IAsset asset, WamsLocatorCategory category) {
            Logger.Debug("CreateLocatorsAsync() invoked for asset '{0}' with category '{1}'.", asset.Name, category);

            Task<WamsLocatorInfo> sasLocatorInfoTask = null;
            Task<WamsLocatorInfo> onDemandLocatorInfoTask = null;
            var primaryAssetFile = asset.AssetFiles.Where(assetFile => assetFile.IsPrimary).Single();
            var isDynamicAsset = Settings.EnableDynamicPackaging && primaryAssetFile.Name.EndsWith(".ism", StringComparison.OrdinalIgnoreCase);

            try {
                sasLocatorInfoTask = CreateLocatorAsync(asset, String.Format("{0} {1} On-Demand Policy", asset.Id, category), LocatorType.Sas, Settings.AccessPolicyDuration);
                
                if (isDynamicAsset)
                    onDemandLocatorInfoTask = CreateLocatorAsync(asset, String.Format("{0} {1} On-Demand Policy", asset.Id, category), LocatorType.OnDemandOrigin, Settings.AccessPolicyDuration);

                Logger.Information("Locators with category '{0}' were created for asset '{1}'.", category, asset.Name);

                return new WamsLocators(
                    await sasLocatorInfoTask.ConfigureAwait(continueOnCapturedContext: false),
                    isDynamicAsset ? await onDemandLocatorInfoTask.ConfigureAwait(continueOnCapturedContext: false) : null,
                    isDynamicAsset ? primaryAssetFile.Name : null);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while creating locators for asset '{0}'. Cleaning up any orphaned locators.", asset.Name);

                try {
                    var wamsLocators = new WamsLocators(
                        sasLocatorInfoTask != null ? sasLocatorInfoTask.Result : null,
                        onDemandLocatorInfoTask != null ? onDemandLocatorInfoTask.Result : null,
                        isDynamicAsset ? primaryAssetFile.Name : null);

                    DeleteLocatorsAsync(asset, wamsLocators).Wait();
                }
                catch (Exception iex) {
                    Logger.Warning(iex, "Error while cleaning up orphaned locators.");
                }

                throw;
            }
        }

        public async Task DeleteLocatorsAsync(IAsset asset, WamsLocators locators) {
            Logger.Debug("DeleteLocatorsAsync() invoked for asset '{0}'.", asset.Name);

            var locatorsToDelete = new List<ILocator>();

            try {
                if (locators.SasLocator != null && !String.IsNullOrEmpty(locators.SasLocator.Id))
                    locatorsToDelete.AddRange(
                        from locator in Context.Locators
                        where locator.Id == locators.SasLocator.Id
                        select locator);

                if (locators.OnDemandLocator != null && !String.IsNullOrEmpty(locators.OnDemandLocator.Id))
                    locatorsToDelete.AddRange(
                        from locator in Context.Locators
                        where locator.Id == locators.OnDemandLocator.Id
                        select locator);

                var deleteTaskQuery =
                    from locator in locatorsToDelete
                    let accessPolicy = locator.AccessPolicy
                    select locator.DeleteAsync().ContinueWith((previousTask) => accessPolicy.DeleteAsync());

                if (deleteTaskQuery.Any())
                    await Task.WhenAll(deleteTaskQuery).ConfigureAwait(continueOnCapturedContext: false);

                Logger.Information("Locators were deleted for asset '{0}'.", asset.Name);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while deleting locators for asset '{0}'.", asset.Name);
                throw;
            }
        }

        public IJob GetJobById(string jobId) {
            Logger.Debug("GetJobById() invoked for with jobId value '{0}'.", jobId);

            return Context.Jobs.Where(job => job.Id == jobId).ToArray().SingleOrDefault(); // ToArray() first because FirstOrDefault() is unsupported by provider.
        }

        public IEnumerable<IJob> GetJobsById(IEnumerable<string> jobIds) {
            Logger.Debug("GetAssetsById() invoked.");

            // The following throws a NotSupportedException.
            //return ctx.Jobs.Where(job => jobIds.Contains(job.Id));

            var jobsQuery =
                from jobId in jobIds
                select GetJobById(jobId);

            return jobsQuery.Where(x => x != null).ToArray();
        }

        public IJob CreateNewJob(string name) {
            Logger.Debug("CreateNewJob() invoked for with name value '{0}'.", name);

            var job = Context.Jobs.Create(name);
            Logger.Information("New job was created with name '{0}'.", job.Name);

            return job;
        }

        public async Task<WamsAssetInfo> CreateAssetAsync(string filename) {
            var assetName = Guid.NewGuid().ToString();
            var asset = await Context.Assets.CreateAsync(assetName, AssetCreationOptions.None, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
            var assetFile = await asset.AssetFiles.CreateAsync(filename, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
            assetFile.IsPrimary = true;
            assetFile.Update();

            var uploadAccessPolicy = await Context.AccessPolicies.CreateAsync("Upload Policy", TimeSpan.FromDays(1), AccessPermissions.Write | AccessPermissions.List).ConfigureAwait(continueOnCapturedContext: false);
            var uploadLocator = await Context.Locators.CreateLocatorAsync(LocatorType.Sas, asset, uploadAccessPolicy).ConfigureAwait(continueOnCapturedContext: false);
            var blobUri = new UriBuilder(uploadLocator.Path);
            blobUri.Path += "/" + filename;

            return new WamsAssetInfo() { SasLocator = blobUri.Uri.AbsoluteUri, AssetId = asset.Id };
        }

        public async Task<string> GetEncoderMetadataXml(IAsset asset) {
            var metadataFile = asset.AssetFiles.ToArray().SingleOrDefault(a => a.Name.EndsWith("_manifest.xml"));
            if (metadataFile == null)
                return null;

            var metadataFilename = _tempFileManager.CreateNewFileName("xml");
            
            try {
                var metadataFilePath = _tempFileManager.GetPhysicalFilePath(metadataFilename);
                metadataFile.Download(metadataFilePath);

                string metadataXml;

                using (var stream = _tempFileManager.LoadFile(metadataFilename)) {
                    using (var reader = new StreamReader(stream)) {
                        metadataXml = await reader.ReadToEndAsync();
                    }
                }

                return metadataXml;
            }
            finally {
                try {
                    _tempFileManager.DeleteFile(metadataFilename);
                }
                catch (Exception ex) {
                    // No use doing anything here except logging.
                    Logger.Warning(ex, "Error while deleting temporary file '{0}'.", metadataFilename);
                }
            }
        }

        public async Task<IEnumerable<string>> EnsureCorsIsEnabledAsync(params string[] origins) {
            var storageAccount = new CloudStorageAccount(new StorageCredentials(Context.DefaultStorageAccount.Name, Settings.StorageAccountKey), false);
            var client = storageAccount.CreateCloudBlobClient();
            var serviceProperties = await client.GetServicePropertiesAsync().ConfigureAwait(continueOnCapturedContext: false);
            var requiredHeaders = new[] { "accept", "x-ms-blob-content-type", "x-ms-blob-type", "x-ms-date", "x-ms-version", "content-disposition", "content-length", "content-range", "content-type" };
            var requiredMethods = CorsHttpMethods.Put | CorsHttpMethods.Options;

            if (serviceProperties.Cors == null)
                serviceProperties.Cors = new CorsProperties();
            
            var rule = FindBestMatchingRule(serviceProperties, requiredHeaders, requiredMethods, origins);

            if (rule == null) {
                rule = new CorsRule {
                    AllowedHeaders = requiredHeaders,
                    AllowedMethods = requiredMethods,
                    AllowedOrigins = new List<string>(),
                    ExposedHeaders = new List<string> { "*" },
                    MaxAgeInSeconds = 1800 // 30 minutes
                };

                serviceProperties.Cors.CorsRules.Add(rule);
                Logger.Information("A new CORS rule has been added to the configured WAMS instance.");
            }

            var addedOrigins = new List<string>();
            var settingsChanged = false;

            foreach (var origin in origins.Where(origin => !rule.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))) {
                rule.AllowedOrigins.Add(origin);
                addedOrigins.Add(origin);
                settingsChanged = true;
                Logger.Information("The following CORS origins were added to the configured WAMS instance: {0}", origin);
            }

            if (settingsChanged)
                await client.SetServicePropertiesAsync(serviceProperties).ConfigureAwait(continueOnCapturedContext: false);

            return addedOrigins.AsEnumerable();
        }

        private static CorsRule FindBestMatchingRule(ServiceProperties serviceProperties, IEnumerable<string> requiredHeaders, CorsHttpMethods requiredMethods, IEnumerable<string> origins) {
            var query =
                from rule in serviceProperties.Cors.CorsRules
                let hasRequiredHeadersAndMethods = (rule.AllowedHeaders.Contains("*") || requiredHeaders.All(rule.AllowedHeaders.Contains)) && (rule.AllowedMethods & requiredMethods) == requiredMethods
                let numberMatchingOrigins = origins.Count(x => rule.AllowedOrigins.Contains(x))
                where hasRequiredHeadersAndMethods
                orderby hasRequiredHeadersAndMethods descending, numberMatchingOrigins descending
                select rule;

            return query.FirstOrDefault();
        }

        private async Task<WamsLocatorInfo> CreateLocatorAsync(IAsset asset, string accessPolicyName, LocatorType type, TimeSpan duration) {
            IAccessPolicy accessPolicy = null;
            ILocator locator = null;

            try {
                accessPolicy = await Context.AccessPolicies.CreateAsync(accessPolicyName, duration, AccessPermissions.Read | AccessPermissions.List).ConfigureAwait(continueOnCapturedContext: false);
                locator = await Context.Locators.CreateLocatorAsync(type, asset, accessPolicy).ConfigureAwait(continueOnCapturedContext: false);

                Logger.Information("New {0} locator with duration {1} was created for asset '{2}'", type, duration, asset.Name);

                return new WamsLocatorInfo(locator.Id, locator.Path);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while creating locator for asset '{0}'. Cleaning up any created locator and access policy.", asset.Name);

                try {
                    if (locator != null)
                        locator.Delete();
                    if (accessPolicy != null)
                        accessPolicy.Delete();
                }
                catch (Exception iex) {
                    Logger.Warning(iex, "Error while cleaning up created locator and access policy.");
                }

                throw;
            }
        }
    }
}