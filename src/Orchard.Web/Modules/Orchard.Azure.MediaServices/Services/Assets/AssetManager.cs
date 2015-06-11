using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FluentNHibernate.Conventions;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Models.Records;
using Orchard.Azure.MediaServices.Services.TempFiles;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public class AssetManager : Component, IAssetManager {

        private readonly IAssetStorageProvider _assetStorageProvider;
        private readonly IRepository<AssetRecord> _assetRepository;
        private readonly IContentManager _contentManager;
        private readonly IClock _clock;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private readonly ITempFileManager _fileManager;
        private readonly IWamsClient _wamsClient;
        private readonly IAssetFactory _factory;

        public AssetManager(
            IAssetStorageProvider assetStoragePRovider,
            IRepository<AssetRecord> assetRepository,
            IContentManager contentManager,
            IClock clock,
            IMimeTypeProvider mimeTypeProvider,
            ITempFileManager fileManager,
            IWamsClient wamsClient,
            IAssetFactory factory) {

            _assetStorageProvider = assetStoragePRovider;
            _assetRepository = assetRepository;
            _contentManager = contentManager;
            _clock = clock;
            _mimeTypeProvider = mimeTypeProvider;
            _fileManager = fileManager;
            _wamsClient = wamsClient;
            _factory = factory;
        }

        public Asset GetAssetById(int id) {
            Logger.Debug("GetAssetById() invoked with id value {0}.", id);

            var record = _assetRepository.Get(id);
            return record != null ? Activate(record) : null;
        }

        public IEnumerable<Asset> LoadAssetsFor(CloudVideoPart part) {
            Logger.Debug("LoadAssetsFor() invoked for cloud video item with ID {0}.", part.Id);

            return LoadAssetRecordsFor(part).Select(Activate);
        }

        public IEnumerable<T> LoadAssetsFor<T>(CloudVideoPart part) where T:Asset {
            Logger.Debug("LoadAssetsFor<{0}>() invoked for cloud video item with ID {1}.", typeof(T).Name, part.Id);

            return LoadAssetRecordsFor(part).Select(Activate).Where(x => x is T).Cast<T>();
        }

        public IEnumerable<Asset> LoadPendingAssets() {
            Logger.Debug("LoadPendingAssets() invoked.");

            var pendingAssetsQuery =
                from a in _assetRepository.Table
                where a.UploadStatus == AssetUploadStatus.Pending
                select a;

            return pendingAssetsQuery.ToArray().Select(Activate);
        }

        public Asset CreateAssetFor<T>(CloudVideoPart part, Action<T> initialize = null) where T : Asset, new() {
            Logger.Debug("CreateAssetFor() invoked for cloud video item with ID {0}.", part.Id);

            var newAsset = (T)Activate(typeof(T).FullName);
            newAsset.CreatedUtc = _clock.UtcNow;
            newAsset.VideoPart = part;

            if (initialize != null)
                initialize(newAsset);

            _assetRepository.Create(newAsset.Record);
            Logger.Information("New asset was created with record ID {0} for cloud video item with ID {1}.", newAsset.Record.Id, part.Id);

            return newAsset;
        }

        public void DeleteAssetsFor(CloudVideoPart part) {
            Logger.Debug("DeleteAssetsFor() invoked for cloud video item with ID {0}.", part.Id);

            var assetsQuery =
                from asset in LoadAssetsFor(part)
                where asset.PublishState.Status != AssetPublishStatus.Removed
                select asset;

            DeleteAssets(assetsQuery);
        }

        public void DeleteAssets(IEnumerable<Asset> assets) {
            Logger.Debug("DeleteAssets() invoked.");

            var deleteTasks = new List<Task>();

            foreach (var asset in assets) {
                if (asset.UploadState.Status.IsAny(AssetUploadStatus.Pending, AssetUploadStatus.Uploading))
                    asset.UploadState.Status = AssetUploadStatus.Canceled;
                else {
                    deleteTasks.Add(DeleteAssetAsync(asset));
                }
            }

            Task.WaitAll(deleteTasks.ToArray());
        }

        public void DeleteAsset(Asset asset) {
            Logger.Debug("DeleteAsset() invoked for asset with record ID {0}.", asset.Record.Id);

            DeleteAssetAsync(asset).Wait();
        }

        public void PublishAssetsFor(CloudVideoPart part) {
            Logger.Debug("PublishAssetsFor() invoked for cloud video item with ID {0}.", part.Id);

            var unpublishedAssetsQuery =
                from asset in LoadAssetsFor(part)
                where asset.PublishState.Status == AssetPublishStatus.None && asset.UploadState.Status == AssetUploadStatus.Uploaded
                select asset;

            var publishTasks = new List<Task>();

            foreach (var asset in unpublishedAssetsQuery) {
                var wamsAsset = _wamsClient.GetAssetById(asset.WamsAssetId);
                if (wamsAsset == null)
                    throw new ApplicationException(String.Format("The asset record with ID {0} refers to a WAMS asset with ID '{1}' but no asset with that ID exists in the configured WAMS instance.", asset.Record.Id, asset.WamsAssetId));

                publishTasks.Add(_wamsClient.CreateLocatorsAsync(wamsAsset, WamsLocatorCategory.Public).ContinueWith((previousTask) => {
                    try {
                        var wamsLocators = previousTask.Result;
                        asset.WamsPublicLocatorId = wamsLocators.SasLocator.Id;
                        asset.WamsPublicLocatorUrl = wamsLocators.SasLocator.Url;
                        if (wamsLocators.OnDemandLocator != null && asset is DynamicVideoAsset) {
                            var videoAsset = (DynamicVideoAsset)asset;
                            videoAsset.WamsPublicOnDemandLocatorId = wamsLocators.OnDemandLocator.Id;
                            videoAsset.WamsPublicOnDemandLocatorUrl = wamsLocators.OnDemandLocator.Url;
                        }
                        asset.PublishState.PublishedUtc = _clock.UtcNow;
                        asset.PublishState.Status = AssetPublishStatus.Published;

                        Logger.Information("Assets with record ID {0} was published.", asset.Record.Id);
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "Error while publishing asset with record ID {0}.", asset.Record.Id);
                        throw;
                    }
                }));
            }

            Task.WaitAll(publishTasks.ToArray());

            Logger.Information("Assets were published for cloud video item with ID {0}.", part.Id);
        }

        public void UnpublishAssetsFor(CloudVideoPart part) {
            Logger.Debug("UnpublishAssetsFor() invoked for cloud video item with ID {0}.", part.Id);

            var publishedAssetsQuery =
                from asset in LoadAssetsFor(part)
                where asset.PublishState.Status == AssetPublishStatus.Published
                select asset;

            var unpublishTasks = new List<Task>();

            foreach (var asset in publishedAssetsQuery) {
                string wamsPublicSasLocatorId = asset.WamsPublicLocatorId;
                string wamsPublicOnDemandLocatorId = null;
                string wamsManifestFilename = null;
                if (asset is DynamicVideoAsset) {
                    var dynamicVideoAsset = (DynamicVideoAsset)asset;
                    wamsPublicOnDemandLocatorId = dynamicVideoAsset.WamsPublicOnDemandLocatorId;
                    wamsManifestFilename = dynamicVideoAsset.WamsManifestFilename;
                }

                var wamsAsset = _wamsClient.GetAssetById(asset.WamsAssetId);
                if (wamsAsset == null)
                    throw new ApplicationException(String.Format("The asset record with ID {0} refers to a WAMS asset with ID '{1}' but no asset with that ID exists in the configured WAMS instance.", asset.Record.Id, asset.WamsAssetId));
                
                var wamsLocators = new WamsLocators(new WamsLocatorInfo(wamsPublicSasLocatorId, null), new WamsLocatorInfo(wamsPublicOnDemandLocatorId, null), wamsManifestFilename);

                unpublishTasks.Add(_wamsClient.DeleteLocatorsAsync(wamsAsset, wamsLocators).ContinueWith((previousTask) => {
                    try {
                        asset.WamsPublicLocatorId = null;
                        asset.WamsPublicLocatorUrl = null;
                        if (asset is DynamicVideoAsset) {
                            var videoAsset = (DynamicVideoAsset)asset;
                            videoAsset.WamsPublicOnDemandLocatorId = null;
                            videoAsset.WamsPublicOnDemandLocatorUrl = null;
                        }
                        asset.PublishState.PublishedUtc = null;
                        asset.PublishState.Status = AssetPublishStatus.None;

                        Logger.Information("Assets with record ID {0} was unpublished.", asset.Record.Id);
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "Error while unpublishing asset with record ID {0}.", asset.Record.Id);
                        throw;
                    }
                }));
            }

            Task.WaitAll(unpublishTasks.ToArray());

            Logger.Information("Assets were unpublished for cloud video item with ID {0}.", part.Id);
        }

        public ThumbnailAsset GetThumbnailAssetFor(CloudVideoPart part) {
            Logger.Debug("GetThumbnailAssetFor() invoked for cloud video item with ID {0}.", part.Id);

            var thumbnailAssetQuery =
                from asset in part.Assets
                where asset is ThumbnailAsset
                let thumbnailAsset = (ThumbnailAsset)asset
                where thumbnailAsset.UploadState.Status == AssetUploadStatus.Uploaded && thumbnailAsset.PublishState.Status != AssetPublishStatus.Removed
                select thumbnailAsset;

            return thumbnailAssetQuery.FirstOrDefault();
        }

        public string SaveTemporaryFile(HttpPostedFileBase file) {
            Logger.Debug("SaveTemporaryFile() invoked for file with name '{0}'.", file.FileName);

            var extension = Path.GetExtension(file.FileName);
            var temporaryFilePath = _fileManager.CreateNewFileName(extension);
            _fileManager.SaveFile(temporaryFilePath, file.InputStream);

            return temporaryFilePath;
        }

        private async Task DeleteAssetAsync(Asset asset) {
            try {

                if (asset.UploadState.Status == AssetUploadStatus.Uploading) {
                    Logger.Information("Asset with record ID {0} is uploading; setting upload status to Canceled.", asset.Record.Id);

                    // Instruct AssetUploader to cease uploading, delete WAMS asset and clean up temporary storage.
                    asset.UploadState.Status = AssetUploadStatus.Canceled;
                }
                else {
                    var wamsAsset = !String.IsNullOrEmpty(asset.WamsAssetId) ? _wamsClient.GetAssetById(asset.WamsAssetId) : null;

                    if (wamsAsset != null)
                        await _wamsClient.DeleteAssetAsync(wamsAsset).ConfigureAwait(continueOnCapturedContext: false);

                    if (!String.IsNullOrEmpty(asset.LocalTempFileName)) {
                        _fileManager.DeleteFile(asset.LocalTempFileName);
                        asset.LocalTempFileName = null;
                        asset.LocalTempFileSize = null;
                    }
                }

                asset.PublishState.Status = AssetPublishStatus.Removed;
                asset.PublishState.RemovedUtc = _clock.UtcNow;

                Logger.Information("Asset with record ID {0} was deleted.", asset.Record.Id);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while deleting asset with record ID {0}.", asset.Record.Id);
                throw;
            }
        }

        private IEnumerable<AssetRecord> LoadAssetRecordsFor(CloudVideoPart part) {
            return _assetRepository.Fetch(x => x.VideoContentItemId == part.Id && x.PublishStatus != AssetPublishStatus.Removed);
        }

        private Asset Activate(string assetType) {
            return Activate(new AssetRecord { Type = assetType });
        }

        private Asset Activate(AssetRecord record) {
            var asset = _factory.Create(record.Type);
            return Activate(asset, record);
        }

        private Asset Activate(Asset asset, AssetRecord record) {
            asset.Record = record;
            _assetStorageProvider.BindStorage(asset);
            asset.MimeTypeProvider = _mimeTypeProvider;
            asset._videoPartField.Loader(() => _contentManager.Get<CloudVideoPart>(record.VideoContentItemId, VersionOptions.Latest));
            asset._videoPartField.Setter(x => {
                if (x == null) throw new ArgumentNullException("You must set a reference to a CloudVideoPart. Nulls are not alowed.");
                record.VideoContentItemId = x.Id;
                return x;
            });
            return asset;
        }
    }
}