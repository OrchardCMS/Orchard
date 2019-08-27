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
                context.Granted = _mediaLibraryService.CheckMediaFolderPermission(context.Permission, mediaPart.FolderPath);
            }
        }
    }
}