using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Jobs;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.Azure.MediaServices.ViewModels.Media;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Notify;

namespace Orchard.Azure.MediaServices.Drivers {
    public class CloudVideoPartDriver : ContentPartDriver<CloudVideoPart> {

        private readonly IOrchardServices _services;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAssetManager _assetManager;
        private readonly IJobManager _jobManager;
        private readonly IWamsClient _wamsClient;

        public CloudVideoPartDriver(
            IOrchardServices services, 
            IHttpContextAccessor httpContextAccessor, 
            IAssetManager assetManager, 
            IJobManager jobManager,
            IWamsClient wamsClient) {

            _services = services;
            _httpContextAccessor = httpContextAccessor;
            _assetManager = assetManager;
            _jobManager = jobManager;
            _wamsClient = wamsClient;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(CloudVideoPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_CloudVideo_Metadata", () => shapeHelper.Parts_CloudVideo_Metadata(ActiveJobCount: _jobManager.GetActiveJobs().Count(job => job.Record.CloudVideoPartId == part.Id))),
                ContentShape("Parts_CloudVideo_SummaryAdmin", () => shapeHelper.Parts_CloudVideo_SummaryAdmin()),
                ContentShape("Parts_CloudVideo_Summary", () => shapeHelper.Parts_CloudVideo_Summary()),
                ContentShape("Parts_CloudVideo_Raw", () => shapeHelper.Parts_CloudVideo_Raw()),
                ContentShape("Parts_CloudVideo", () => shapeHelper.Parts_CloudVideo()));
        }

        protected override DriverResult Editor(CloudVideoPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CloudVideoPart part, IUpdateModel updater, dynamic shapeHelper) {
            var results = new List<DriverResult>();
            results.Add(ContentShape("Parts_CloudVideo_Edit", () => {
                var settings = _services.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();
                var httpContext = _httpContextAccessor.Current();
                
                var occupiedSubtitleLanguagesQuery =
                    from asset in part.Assets
                    where asset is SubtitleAsset
                    select ((SubtitleAsset)asset).Language;
                var availableSubtitleLanguagesQuery =
                    from language in settings.SubtitleLanguages
                    where !occupiedSubtitleLanguagesQuery.Contains(language)
                    select language;

                var viewModel = new CloudVideoPartViewModel(availableSubtitleLanguagesQuery.ToArray()) {
                    Id = part.Id,
                    Part = part,
                    AllowedVideoFilenameExtensions = settings.AllowedVideoFilenameExtensions,
                    TemporaryVideoFile = new TemporaryFileViewModel {
                        OriginalFileName = part.MezzanineAsset != null ? part.MezzanineAsset.OriginalFileName : "",
                        FileSize = 0,
                        TemporaryFileName = ""
                    },
                    AddedSubtitleLanguage = settings.SubtitleLanguages.FirstOrDefault(),
                    WamsVideo = new WamsAssetViewModel(),
                    WamsThumbnail = new WamsAssetViewModel(),
                    WamsSubtitle = new WamsAssetViewModel()
                };

                if (updater != null) {
                    
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null) && AVideoWasUploaded(part, updater, viewModel)) {

                        ProcessCreatedWamsAssets(part, viewModel);
                        ProcessUploadedFiles(part, viewModel);
                        
                        var unpublish = httpContext.Request.Form["submit.Save"] == "submit.Unpublish";
                        if (unpublish) {
                            _services.ContentManager.Unpublish(part.ContentItem);
                            _services.Notifier.Success(T("Your {0} has been unpublished.", part.ContentItem.TypeDefinition.DisplayName));
                        }

                        if (part.IsPublished())
                            _assetManager.PublishAssetsFor(part);
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/CloudVideo", Model: viewModel, Prefix: Prefix);
            }));

            if (part.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable) {
                if (part.IsPublished()) {
                    results.Add(ContentShape("CloudVideo_Edit_UnpublishButton", actions => actions));
                }
            }

            return Combined(results.ToArray());
        }

        private bool AVideoWasUploaded(CloudVideoPart part, IUpdateModel updater, CloudVideoPartViewModel viewModel) {
            var isValid = viewModel.WamsVideo.WamsAssetId != null || viewModel.TemporaryVideoFile.FileSize > 0 || part.MezzanineAsset != null;

            if (!isValid)
                updater.AddModelError(Prefix + ".WamsVideo.WamsAssetId", T("You need to upload a video."));

            return isValid;
        }

        private void ProcessCreatedWamsAssets(CloudVideoPart part, CloudVideoPartViewModel viewModel) {
            if (viewModel.WamsVideo.AssetId == null && !String.IsNullOrWhiteSpace(viewModel.WamsVideo.WamsAssetId)) {
                var asset = _assetManager.CreateAssetFor<MezzanineAsset>(part, a => {
                    a.Name = "Mezzanine";
                    a.IncludeInPlayer = false;
                    a.OriginalFileName = viewModel.WamsVideo.FileName;
                    a.WamsAssetId = viewModel.WamsVideo.WamsAssetId;
                    a.UploadState.Status = AssetUploadStatus.Uploaded;
                    CreatePrivateLocatorFor(a);
                });
                viewModel.WamsVideo.AssetId = asset.Record.Id;
            }

            if (viewModel.WamsThumbnail.AssetId == null && !String.IsNullOrWhiteSpace(viewModel.WamsThumbnail.WamsAssetId)) {
                var asset = _assetManager.CreateAssetFor<ThumbnailAsset>(part, a => {
                    a.Name = viewModel.WamsThumbnail.FileName;
                    a.IncludeInPlayer = true;
                    a.OriginalFileName = viewModel.WamsThumbnail.FileName;
                    a.WamsAssetId = viewModel.WamsThumbnail.WamsAssetId;
                    a.UploadState.Status = AssetUploadStatus.Uploaded;
                    CreatePrivateLocatorFor(a);
                });
                viewModel.WamsThumbnail.AssetId = asset.Record.Id;
            }

            if (viewModel.WamsSubtitle.AssetId == null && !String.IsNullOrWhiteSpace(viewModel.WamsSubtitle.WamsAssetId)) {
                var asset = _assetManager.CreateAssetFor<SubtitleAsset>(part, a => {
                    a.Name = viewModel.AddedSubtitleLanguage;
                    a.IncludeInPlayer = true;
                    a.OriginalFileName = viewModel.WamsSubtitle.FileName;
                    a.Language = viewModel.AddedSubtitleLanguage;
                    a.WamsAssetId = viewModel.WamsSubtitle.WamsAssetId;
                    a.UploadState.Status = AssetUploadStatus.Uploaded;
                    CreatePrivateLocatorFor(a);
                });
                viewModel.WamsThumbnail.AssetId = asset.Record.Id;
            }
        }

        private void ProcessUploadedFiles(CloudVideoPart part, CloudVideoPartViewModel viewModel) {
            var httpContext = _httpContextAccessor.Current();
            var files = httpContext.Request.Files;
            var postedThumbnailFile = files["ThumbnailFile.Proxied"];
            var postedSubtitleFile = files["SubtitleFile.Proxied"];

            if (viewModel.TemporaryVideoFile.FileSize > 0) {
                _assetManager.CreateAssetFor<MezzanineAsset>(part, a => {
                    a.Name = "Mezzanine";
                    a.IncludeInPlayer = false;
                    a.OriginalFileName = Path.GetFileName(viewModel.TemporaryVideoFile.OriginalFileName);
                    a.LocalTempFileName = viewModel.TemporaryVideoFile.TemporaryFileName;
                    a.LocalTempFileSize = viewModel.TemporaryVideoFile.FileSize;
                });
            }
            if (postedThumbnailFile != null && postedThumbnailFile.ContentLength > 0) {
                var thumbnailTempFilePath = _assetManager.SaveTemporaryFile(postedThumbnailFile);
                _assetManager.CreateAssetFor<ThumbnailAsset>(part, a => {
                    a.Name = Path.GetFileName(postedThumbnailFile.FileName);
                    a.IncludeInPlayer = true;
                    a.OriginalFileName = Path.GetFileName(postedThumbnailFile.FileName);
                    a.LocalTempFileName = thumbnailTempFilePath;
                    a.LocalTempFileSize = postedThumbnailFile.ContentLength;
                });
            }
            if (postedSubtitleFile != null && postedSubtitleFile.ContentLength > 0) {
                var subtitleTempFilePath = _assetManager.SaveTemporaryFile(postedSubtitleFile);
                _assetManager.CreateAssetFor<SubtitleAsset>(part, a => {
                    a.Name = Path.GetFileName(postedSubtitleFile.FileName);
                    a.IncludeInPlayer = true;
                    a.OriginalFileName = Path.GetFileName(postedSubtitleFile.FileName);
                    a.LocalTempFileName = subtitleTempFilePath;
                    a.LocalTempFileSize = postedSubtitleFile.ContentLength;
                    a.Language = viewModel.AddedSubtitleLanguage;
                });
            }
        }

        private void DeleteExistingThumbnails(CloudVideoPart part) {
            var thumbnailAssets = part.Assets.Where(x => x is ThumbnailAsset);

            foreach (var asset in thumbnailAssets) {
                _assetManager.DeleteAsset(asset);
            }
        }

        public void CreatePrivateLocatorFor(Asset asset) {
            var wamsAsset = _wamsClient.GetAssetById(asset.WamsAssetId);
            var wamsLocators =  _wamsClient.CreateLocatorsAsync(wamsAsset, WamsLocatorCategory.Private).Result;

            asset.WamsPrivateLocatorId = wamsLocators.SasLocator.Id;
            asset.WamsPrivateLocatorUrl = wamsLocators.SasLocator.Url;
        }
    }
}