using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;

namespace Orchard.MediaLibrary.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly RouteCollection _routeCollection;

        public XmlRpcHandler(
            IMembershipService membershipService,
            IAuthorizationService authorizationService,
            IMediaLibraryService mediaLibraryService,
            RouteCollection routeCollection,
            IContentManager contentManager) {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            _mediaLibraryService = mediaLibraryService;
            _routeCollection = routeCollection;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void SetCapabilities(XElement options) {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
            options.SetElementValue(XName.Get("supportsFileUpload", manifestUri), "Yes");
        }

        public void Process(XmlRpcContext context) {
            var urlHelper = new UrlHelper(context.ControllerContext.RequestContext, _routeCollection);

            if (context.Request.MethodName == "metaWeblog.newMediaObject") {
                var result = MetaWeblogNewMediaObject(
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value,
                    urlHelper);
                context.Response = new XRpcMethodResponse().Add(result);
            }
        }

        private XRpcStruct MetaWeblogNewMediaObject(
            string userName,
            string password,
            XRpcStruct file,
            UrlHelper url) {

            List<LocalizedString> validationErrors;
            var user = _membershipService.ValidateUser(userName, password, out validationErrors);
            if (!_authorizationService.TryCheckAccess(Permissions.ManageOwnMedia, user, null)
                && !_authorizationService.TryCheckAccess(Permissions.EditMediaContent, user, null)) {
                throw new OrchardCoreException(T("Access denied"));
            }

            var name = file.Optional<string>("name");
            var bits = file.Optional<byte[]>("bits");

            string directoryName = Path.GetDirectoryName(name);
            if (string.IsNullOrWhiteSpace(directoryName)) { // Some clients only pass in a name path that does not contain a directory component.
                directoryName = "media";
            }

            // If the user only has access to his own folder, rewrite the folder name
            if (!_authorizationService.TryCheckAccess(Permissions.EditMediaContent, user, null)) {
                directoryName = Path.Combine(_mediaLibraryService.GetRootedFolderPath(directoryName));
            }

            try {
                // delete the file if it already exists, e.g. an updated image in a blog post
                // it's safe to delete the file as each content item gets a specific folder
                _mediaLibraryService.DeleteFile(directoryName, Path.GetFileName(name));
            }
            catch {
                // current way to delete a file if it exists
            }

            string publicUrl = _mediaLibraryService.UploadMediaFile(directoryName, Path.GetFileName(name), bits);
            var mediaPart = _mediaLibraryService.ImportMedia(directoryName, Path.GetFileName(name));
            try {
                _contentManager.Create(mediaPart);
            }
            catch {
            }

            return new XRpcStruct() // Some clients require all optional attributes to be declared Wordpress responds in this way as well.
                .Set("file", publicUrl)
                .Set("url", url.MakeAbsolute(publicUrl))
                .Set("type", file.Optional<string>("type"));
        }
    }
}