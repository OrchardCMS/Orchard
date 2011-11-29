using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;

namespace Orchard.Media.Services {
    [UsedImplicitly]
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

            try {
                // delete the file if it already exists, e.g. and updated image in a blog post
                // it's safe to delete the file as each content item gets a specific folder
                _mediaService.DeleteFile(Path.GetDirectoryName(name), Path.GetFileName(name));
            }
            catch {
                // current way to delete a file if it exists
            }

            string publicUrl = _mediaService.UploadMediaFile(Path.GetDirectoryName(name), Path.GetFileName(name), bits, false);
            return new XRpcStruct().Set("url", url.MakeAbsolute(publicUrl));
        }
    }
}