using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Jobs;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Azure.MediaServices.Handlers {
    public class CloudVideoPartHandler : ContentHandler {
        private readonly IAssetManager _assetManager;
        private readonly IJobManager _jobManager;
        private readonly INotifier _notifier;

        public CloudVideoPartHandler(
            IAssetManager assetManager,
            IJobManager jobManager,
            INotifier notifier) {

            _assetManager = assetManager;
            _jobManager = jobManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            OnActivated<CloudVideoPart>(SetupFields);
            OnPublishing<CloudVideoPart>(DeferOrPublishAssets);
            OnUnpublished<CloudVideoPart>(CancelAndUnpublishAssets);
            OnRemoved<CloudVideoPart>(RemoveAssets);
        }

        public Localizer T { get; set; }

        private void SetupFields(ActivatedContentContext context, CloudVideoPart part) {
            part._assetManager = _assetManager;
            part._jobManager = _jobManager;
        } 
        
        private void DeferOrPublishAssets(PublishContentContext context, CloudVideoPart part) {
            if (part.MezzanineAsset != null && part.MezzanineAsset.UploadState.Status != AssetUploadStatus.Uploaded) {
                part.PublishOnUpload = true;
                _notifier.Warning(T("The cloud video item was saved, but will not be published until the primary video asset has finished uploading to Microsoft Azure Media Services."));
                context.Cancel = true;
            }
            else
                _assetManager.PublishAssetsFor(part);
        }

        private void CancelAndUnpublishAssets(PublishContentContext context, CloudVideoPart part) {
            part.PublishOnUpload = false;
            _assetManager.UnpublishAssetsFor(part);
        }

        private void RemoveAssets(RemoveContentContext context, CloudVideoPart part) {
            _assetManager.DeleteAssetsFor(part);
            _jobManager.CloseJobsFor(part);
        }
    }
}