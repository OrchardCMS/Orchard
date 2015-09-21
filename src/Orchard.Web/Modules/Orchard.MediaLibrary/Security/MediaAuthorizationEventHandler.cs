using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Security;

namespace Orchard.MediaLibrary.Security {
    public class MediaAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IAuthorizer _authorizer;
        private readonly IMediaLibraryService _mediaLibraryService;

        public MediaAuthorizationEventHandler(
            IAuthorizer authorizer,
            IMediaLibraryService mediaLibraryService) {
            _authorizer = authorizer;
            _mediaLibraryService = mediaLibraryService;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            var mediaPart = context.Content.As<MediaPart>();
            if (mediaPart != null) {
                if(_authorizer.Authorize(Permissions.ManageMediaContent)) {
                    context.Granted = true;
                    return;
                }

                if(_authorizer.Authorize(Permissions.ManageOwnMedia)) {
                    context.Granted = _mediaLibraryService.CanManageMediaFolder(mediaPart.FolderPath);
                }
            }
        }
    }
}