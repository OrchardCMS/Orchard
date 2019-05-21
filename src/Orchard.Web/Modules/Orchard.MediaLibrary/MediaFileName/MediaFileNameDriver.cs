using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.MediaLibrary.MediaFileName
{
    public class MediaFileNameDriver : ContentPartDriver<MediaPart> {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly INotifier _notifier;

        public MediaFileNameDriver(IAuthorizationService authorizationService, IAuthenticationService authenticationService, IMediaLibraryService mediaLibraryService, INotifier notifier) {
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            _mediaLibraryService = mediaLibraryService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "MediaFileName"; }
        }

        protected override DriverResult Editor(MediaPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MediaPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Media_Edit_FileName",
                () => {
                    var currentUser = _authenticationService.GetAuthenticatedUser();
                    if (!_authorizationService.TryCheckAccess(Permissions.EditMediaContent, currentUser, part)) {
                        return null;
                    }

                    var settings = part.TypeDefinition.Settings.GetModel<MediaFileNameEditorSettings>();
                    if (!settings.ShowFileNameEditor) {
                        return null;
                    }

                    MediaFileNameEditorViewModel model = shapeHelper.Parts_Media_Edit_FileName(typeof(MediaFileNameEditorViewModel));

                    if (part.FileName != null) {
                        model.FileName = part.FileName;
                    }

                    if (updater != null) {
                        var priorFileName = model.FileName;
                        if (updater.TryUpdateModel(model, Prefix, null, null)) {
                            if (model.FileName != null && !model.FileName.Equals(priorFileName, StringComparison.OrdinalIgnoreCase)) {
                                try {
                                    _mediaLibraryService.RenameFile(part.FolderPath, priorFileName, model.FileName);
                                    part.FileName = model.FileName;
                                    
                                    _notifier.Add(NotifyType.Success, T("File '{0}' was renamed to '{1}'", priorFileName, model.FileName));
                                }
                                catch (Exception) {
                                    updater.AddModelError("MediaFileNameEditorSettings.FileName", T("Unable to rename file"));
                                }
                            }
                        }
                    }

                    return model;
                });
        }
    }
}