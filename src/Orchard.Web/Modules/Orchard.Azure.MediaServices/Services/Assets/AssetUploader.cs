using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Models.Records;
using Orchard.Azure.MediaServices.Services.TempFiles;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks;
using NHibernate;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public class AssetUploader : Component, IBackgroundTask {

        private static readonly object _sweepLock = new object();

        private readonly IOrchardServices _orchardServices;
        private readonly IClock _clock;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private readonly IAssetManager _assetManager;
        private readonly ITempFileManager _fileManager;
        private readonly IWamsClient _wamsClient;

        public AssetUploader(
            IOrchardServices orchardServices,
            IClock clock,
            ITransactionManager transactionManager,
            IContentManager contentManager,
            IAssetManager assetManager,
            ITempFileManager fileManager,
            IWamsClient wamsClient) {

            _orchardServices = orchardServices;
            _clock = clock;
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            _assetManager = assetManager;
            _fileManager = fileManager;
            _wamsClient = wamsClient;
        }

        void IBackgroundTask.Sweep() {
            if (Monitor.TryEnter(_sweepLock)) {
                try {
                    Logger.Debug("Beginning sweep.");

                    if (!_orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>().IsValid()) {
                        Logger.Debug("Settings are invalid; going back to sleep.");
                        return;
                    }
                    var pendingAssetsQuery =
                        from asset in _assetManager.LoadPendingAssets()
                        where _fileManager.FileExists(asset.LocalTempFileName)
                        select asset;

                    var pendingAssets = pendingAssetsQuery.ToArray();

                    if (!pendingAssets.Any()) {
                        Logger.Debug("No pending assets with temp files on local machine were found; going back to sleep.");
                        return;
                    }

                    Logger.Information("Beginning processing of {0} pending assets.", pendingAssets.Length);

                    var uploadTasks = new List<Task>();
                    var progressInfoQueue = new BlockingCollection<WamsUploadProgressInfo>();
                    var assetProgressMonikers = new Dictionary<Guid, Asset>();
                    var assetCancellationTokenSources = new Dictionary<Guid, CancellationTokenSource>();

                    foreach (var pendingAsset in pendingAssets) {
                        Logger.Information("Uploading asset of type {0} with name '{1}'...", pendingAsset.Record.Type, pendingAsset.Name);

                        var cloudVideoPart = _contentManager.Get<CloudVideoPart>(pendingAsset.Record.VideoContentItemId, VersionOptions.Latest);

                        pendingAsset.UploadState.Status = AssetUploadStatus.Uploading;
                        pendingAsset.UploadState.StartedUtc = _clock.UtcNow;
                        pendingAsset.UploadState.BytesComplete = 0;
                        pendingAsset.UploadState.CompletedUtc = null;
                        _transactionManager.RequireNew();

                        var assetProgressMoniker = Guid.NewGuid();
                        var assetCancellationTokenSource = new CancellationTokenSource();
                        assetProgressMonikers.Add(assetProgressMoniker, pendingAsset);
                        assetCancellationTokenSources.Add(assetProgressMoniker, assetCancellationTokenSource);

                        var localPendingAsset = pendingAsset;
                        var uploadTask =
                            _wamsClient.UploadAssetAsync(_fileManager.GetPhysicalFilePath(pendingAsset.LocalTempFileName), progressInfoQueue.Add, assetProgressMoniker, assetCancellationTokenSource.Token)
                            .ContinueWith((previousTask) => {
                                if (!previousTask.IsCanceled) {
                                    try {
                                        var wamsAsset = previousTask.Result; // Throws if previous task was faulted.
                                        var wamsLocators = _wamsClient.CreateLocatorsAsync(wamsAsset, WamsLocatorCategory.Private).Result;

                                        localPendingAsset.WamsPrivateLocatorId = wamsLocators.SasLocator.Id;
                                        localPendingAsset.WamsPrivateLocatorUrl = wamsLocators.SasLocator.Url;
                                        localPendingAsset.WamsAssetId = wamsAsset.Id;
                                        localPendingAsset.UploadState.Status = AssetUploadStatus.Uploaded;
                                        localPendingAsset.UploadState.BytesComplete = localPendingAsset.LocalTempFileSize;
                                        localPendingAsset.UploadState.CompletedUtc = _clock.UtcNow;
                                    }
                                    catch (Exception ex) {
                                        Logger.Error(ex, "Error while uploading asset of type {0} with name '{1}'. Resetting asset upload status to {2}.", localPendingAsset.Record.Type, localPendingAsset.Name, AssetUploadStatus.Pending);

                                        // Reset asset to pending status so it will be retried later.
                                        localPendingAsset.UploadState.CompletedUtc = null;
                                        localPendingAsset.UploadState.BytesComplete = 0;
                                        localPendingAsset.UploadState.StartedUtc = null;
                                        localPendingAsset.UploadState.Status = AssetUploadStatus.Pending;

                                        return;
                                    }
                                }

                                _fileManager.DeleteFile(localPendingAsset.LocalTempFileName);
                                localPendingAsset.LocalTempFileName = null;
                                localPendingAsset.LocalTempFileSize = null;
                                Logger.Information("Deleted temp file '{0}' for asset of type {1} with name '{2}'.", localPendingAsset.LocalTempFileName, localPendingAsset.Record.Type, localPendingAsset.Name);

                                if (previousTask.IsCanceled)
                                    Logger.Information("Upload of asset of type {0} with name '{1}' was canceled.", localPendingAsset.Record.Type, localPendingAsset.Name);
                                else {
                                    try {
                                        if (cloudVideoPart.PublishOnUpload) {
                                            var remainingPendingAssetsQuery =
                                                from asset in cloudVideoPart.Assets
                                                where asset.UploadState.Status.IsAny(AssetUploadStatus.Pending, AssetUploadStatus.Uploading)
                                                select asset;

                                            if (!remainingPendingAssetsQuery.Any()) {
                                                Logger.Information("No more remaining assets on cloud video item with ID {0}; publishing the item.", cloudVideoPart.Id);
                                                _contentManager.Publish(cloudVideoPart.ContentItem);
                                                cloudVideoPart.PublishOnUpload = false;
                                                Logger.Information("Published cloud video item with ID {0}.", cloudVideoPart.Id);
                                            }
                                        }
                                        else if (cloudVideoPart.ContentItem.IsPublished()) {
                                            // Publish the asset.
                                            _assetManager.PublishAssetsFor(cloudVideoPart);
                                        }
                                    }
                                    catch (Exception ex) {
                                        Logger.Warning(ex, "Upload of asset of type {0} with name '{1}' was completed but an error occurred while publishing the cloud video item with ID {2} after upload.", localPendingAsset.Record.Type, localPendingAsset.Name, cloudVideoPart.Id);
                                    }

                                    Logger.Information("Upload of asset of type {0} with name '{1}' was successfully completed.", localPendingAsset.Record.Type, localPendingAsset.Name);
                                }
                            }, assetCancellationTokenSource.Token);

                        uploadTask.ConfigureAwait(continueOnCapturedContext: false);
                        uploadTasks.Add(uploadTask);
                    }

                    Task.WhenAll(uploadTasks).ContinueWith((previousTask) => progressInfoQueue.CompleteAdding());

                    var lastUpdateUtc = _clock.UtcNow;
                    foreach (var progressInfo in progressInfoQueue.GetConsumingEnumerable()) {
                        var progressAsset = assetProgressMonikers[progressInfo.AssetMoniker];
                        var cancellationTokenSource = assetCancellationTokenSources[progressInfo.AssetMoniker];

                        // Check for cancellation (asset upload status was set to Canceled).
                        if (!cancellationTokenSource.IsCancellationRequested) {
                            var session = _transactionManager.GetSession();
                            session.Refresh(progressAsset.Record, LockMode.None);

                            if (progressAsset.UploadState.Status == AssetUploadStatus.Canceled) {
                                Logger.Information("Cancellation request was detected for asset of type {0} with name '{1}'; cancelling upload...", progressAsset.Record.Type, progressAsset.Name);
                                cancellationTokenSource.Cancel();
                            }
                        }

                        // Don't flood the database with progress updates; limit it to every 5 seconds.
                        if ((_clock.UtcNow - lastUpdateUtc).Seconds >= 5) {
                            progressAsset.UploadState.BytesComplete = progressInfo.Data.BytesTransferred;
                            _transactionManager.RequireNew();
                            lastUpdateUtc = _clock.UtcNow;
                        }

                        Logger.Debug("Uploading asset of type {0} with name '{1}'; {2}/{3} KB uploaded ({4}%) at {5} KB/sec.", progressAsset.Record.Type, progressAsset.Name, Convert.ToInt64(progressInfo.Data.BytesTransferred / 1024), Convert.ToInt64(progressInfo.Data.TotalBytesToTransfer / 1024), progressInfo.Data.ProgressPercentage, Convert.ToInt32(progressInfo.Data.TransferRateBytesPerSecond / 1024));
                    }
                }
                finally {
                    Logger.Debug("Ending sweep.");
                    Monitor.Exit(_sweepLock);
                }
            }
        }
    }
}