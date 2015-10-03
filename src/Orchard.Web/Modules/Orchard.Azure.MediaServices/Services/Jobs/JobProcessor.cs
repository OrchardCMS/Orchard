using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Models.Jobs;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Tasks;
using Orchard.Tasks.Locking.Services;

namespace Orchard.Azure.MediaServices.Services.Jobs {
    public class JobProcessor : Component, IBackgroundTask {

        private readonly IWamsClient _wamsClient;
        private readonly IAssetManager _assetManager;
        private readonly IJobManager _jobManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IDistributedLockService _distributedLockService;

        public JobProcessor(
            IWamsClient wamsClient,
            IAssetManager assetManager,
            IJobManager jobManager,
            IOrchardServices orchardServices,
            IDistributedLockService distributedLockService) {

            _wamsClient = wamsClient;
            _assetManager = assetManager;
            _jobManager = jobManager;
            _orchardServices = orchardServices;
            _distributedLockService = distributedLockService;
        }

        public void Sweep() {
            Logger.Debug("Beginning sweep.");

            try {
                if (!_orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>().IsValid()) {
                    Logger.Debug("Settings are invalid; going back to sleep.");
                    return;
                }

                // Only allow this task to run on one farm node at a time.
                IDistributedLock @lock;
                if (_distributedLockService.TryAcquireLock(GetType().FullName, TimeSpan.FromHours(1), out @lock)) {
                    using (@lock) {
                        var jobs = _jobManager.GetActiveJobs().ToDictionary(job => job.WamsJobId);

                        if (!jobs.Any()) {
                            Logger.Debug("No open jobs were found; going back to sleep.");
                            return;
                        }

                        Logger.Information("Beginning processing of {0} open jobs.", jobs.Count());

                        var wamsJobs = _wamsClient.GetJobsById(jobs.Keys);

                        foreach (var wamsJob in wamsJobs) {
                            Logger.Information("Processing job '{0}'...", wamsJob.Name);

                            var job = jobs[wamsJob.Id];
                            var tasks = job.Tasks.ToDictionary(task => task.WamsTaskId);
                            var wamsTasks = wamsJob.Tasks.ToArray();

                            foreach (var wamsTask in wamsTasks) {
                                var task = tasks[wamsTask.Id];
                                task.Status = MapWamsJobState(wamsTask.State);
                                task.PercentComplete = (int)wamsTask.Progress;
                            }

                            var previousStatus = job.Status;
                            var wamsJobErrors = HarvestWamsJobErrors(wamsJob).ToArray();

                            job.CreatedUtc = wamsJob.Created;
                            job.StartedUtc = wamsJob.StartTime;
                            job.FinishedUtc = wamsJob.EndTime;
                            job.Status = MapWamsJobState(wamsJob.State);
                            job.ErrorMessage = GetAggregateErrorMessage(wamsJobErrors);

                            LogWamsJobErrors(wamsJobErrors);

                            if (job.Status != previousStatus) {
                                if (job.Status == JobStatus.Finished) {
                                    Logger.Information("Job '{0}' was finished in WAMS; creating locators.", wamsJob.Name);

                                    var lastTask = job.Tasks.Last();
                                    var lastWamsTask = wamsTasks.Single(task => task.Id == lastTask.WamsTaskId);
                                    var outputAsset = lastWamsTask.OutputAssets.First();
                                    var outputAssetName = !String.IsNullOrWhiteSpace(job.OutputAssetName) ? job.OutputAssetName : lastWamsTask.Name;
                                    var outputAssetDescription = job.OutputAssetDescription.TrimSafe();
                                    var encoderMetadataXml = _wamsClient.GetEncoderMetadataXml(outputAsset).Result;
                                    var cloudVideoPart = job.CloudVideoPart;
                                    var wamsLocators = _wamsClient.CreateLocatorsAsync(outputAsset, WamsLocatorCategory.Private).Result;

                                    // HACK: Temporary workaround to disable dynamic packaging for VC1-based assets. In future versions
                                    // this will be implemented more robustly by testing all the dynamic URLs to see which ones work
                                    // and only store and use the working ones.
                                    var forceNonDynamicAsset = lastWamsTask.Configuration.StartsWith("VC1");

                                    if (wamsLocators.OnDemandLocator != null && !forceNonDynamicAsset) {
                                        _assetManager.CreateAssetFor<DynamicVideoAsset>(cloudVideoPart, asset => {
                                            asset.IncludeInPlayer = true;
                                            asset.Name = outputAssetName;
                                            asset.Description = outputAssetDescription;
                                            asset.EncodingPreset = lastTask.HarvestAssetName;
                                            asset.WamsPrivateLocatorId = wamsLocators.SasLocator.Id;
                                            asset.WamsPrivateLocatorUrl = wamsLocators.SasLocator.Url;
                                            asset.WamsPrivateOnDemandLocatorId = wamsLocators.OnDemandLocator.Id;
                                            asset.WamsPrivateOnDemandLocatorUrl = wamsLocators.OnDemandLocator.Url;
                                            asset.WamsManifestFilename = wamsLocators.OnDemandManifestFilename;
                                            asset.WamsAssetId = outputAsset.Id;
                                            asset.WamsEncoderMetadataXml = encoderMetadataXml;
                                            asset.UploadState.Status = AssetUploadStatus.Uploaded;
                                            asset.PublishState.Status = AssetPublishStatus.None;
                                        });
                                    }
                                    else {
                                        _assetManager.CreateAssetFor<VideoAsset>(cloudVideoPart, asset => {
                                            asset.IncludeInPlayer = true;
                                            asset.Name = outputAssetName;
                                            asset.Description = outputAssetDescription;
                                            asset.EncodingPreset = lastTask.HarvestAssetName;
                                            asset.WamsPrivateLocatorId = wamsLocators.SasLocator.Id;
                                            asset.WamsPrivateLocatorUrl = wamsLocators.SasLocator.Url;
                                            asset.WamsAssetId = outputAsset.Id;
                                            asset.WamsEncoderMetadataXml = encoderMetadataXml;
                                            asset.UploadState.Status = AssetUploadStatus.Uploaded;
                                            asset.PublishState.Status = AssetPublishStatus.None;
                                        });
                                    }

                                    try {
                                        if (cloudVideoPart.IsPublished())
                                            _assetManager.PublishAssetsFor(cloudVideoPart);
                                    }
                                    catch (Exception ex) {
                                        Logger.Warning(ex, "Processing of job '{0}' was completed but an error occurred while publishing the cloud video item with ID {1} after processing.", wamsJob.Name, cloudVideoPart.Id);
                                    }
                                }
                            }

                            Logger.Information("Processing of job '{0}' was successfully completed.", wamsJob.Name);
                        }
                    }
                }
                else
                    Logger.Debug("Distributed lock could not be acquired; going back to sleep.");
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error during sweep.");
            }
            finally {
                Logger.Debug("Ending sweep.");
            }
        }

        private static string GetAggregateErrorMessage(IEnumerable<Tuple<IJob, ITask, ErrorDetail>> tuples) {
            var sb = new StringBuilder();
            foreach (var tuple in tuples) {
                var errorDetail = tuple.Item3;
                sb.AppendLine(errorDetail.Message);
            }
            return sb.ToString();
        }

        private void LogWamsJobErrors(IEnumerable<Tuple<IJob, ITask, ErrorDetail>> tuples) {
            foreach (var tuple in tuples) {
                var wamsJob = tuple.Item1;
                var wamsTask = tuple.Item2;
                var errorDetail = tuple.Item3;
                Logger.Information("An error occurred in WAMS while processing job: {0}, Task: '{1}'. Error code: '{2}'. Error message: {3}", wamsJob.Name, wamsTask.Id, errorDetail.Code, errorDetail.Message);
            }
        }

        private IEnumerable<Tuple<IJob, ITask, ErrorDetail>> HarvestWamsJobErrors(IJob wamsJob) {
            return from wamsTask in wamsJob.Tasks
                   from errorDetail in wamsTask.ErrorDetails
                   select new Tuple<IJob, ITask, ErrorDetail>(wamsJob, wamsTask, errorDetail);
        }

        private JobStatus MapWamsJobState(JobState state) {
            switch (state) {
                case JobState.Canceled:
                    return JobStatus.Canceled;
                case JobState.Canceling:
                    return JobStatus.Canceling;
                case JobState.Error:
                    return JobStatus.Faulted;
                case JobState.Finished:
                    return JobStatus.Finished;
                case JobState.Processing:
                    return JobStatus.Processing;
                case JobState.Queued:
                    return JobStatus.Queued;
                case JobState.Scheduled:
                    return JobStatus.Scheduled;
                default:
                    return JobStatus.Pending;
            }
        }
    }
}
