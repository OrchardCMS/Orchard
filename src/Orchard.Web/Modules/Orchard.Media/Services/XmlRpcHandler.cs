using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;

namespace Orchard.Media.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaService _mediaService;
        private readonly RouteCollection _routeCollection;

        public XmlRpcHandler(
            IMembershipService membershipService,
            IAuthorizationService authorizationService,
            IMediaService mediaService,
            RouteCollection routeCollection) {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            _mediaService = mediaService;
            _routeCollection = routeCollection;

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

            var user = _membershipService.ValidateUser(userName, password);
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMedia, user, null)) {
                throw new OrchardCoreException(T("Access denied"));
            }

            var name = file.Optional<string>("name");
            var bits = file.Optional<byte[]>("bits");

            string directoryName = Path.GetDirectoryName(name);
            if (string.IsNullOrWhiteSpace(directoryName)) { // Some clients only pass in a name path that does not contain a directory component.
                directoryName = "media";
            }

            try {
                // delete the file if it already exists, e.g. an updated image in a blog post
                // it's safe to delete the file as each content item gets a specific folder
                _mediaService.DeleteFile(directoryName, Path.GetFileName(name));
            }
            catch {
                // current way to delete a file if it exists
            }

            string publicUrl = _mediaService.UploadMediaFile(directoryName, Path.GetFileName(name), bits, true);
            return new XRpcStruct() // Some clients require all optional attributes to be declared Wordpress responds in this way as well.
                .Set("file", publicUrl)
                .Set("url", url.MakeAbsolute(publicUrl))
                .Set("type", file.Optional<string>("type"));
        }
    }
}