using System;
using System.IO;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Security;

namespace Orchard.Media.Services {
    [UsedImplicitly]
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaService _mediaService;

        public XmlRpcHandler(
            IMembershipService membershipService,
            IAuthorizationService authorizationService,
            IMediaService mediaService) {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            _mediaService = mediaService;
        }

        public void SetCapabilities(XElement options) {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
            options.SetElementValue(XName.Get("supportsFileUpload", manifestUri), "Yes");
        }

        public void Process(XmlRpcContext context) {
            if (context.Request.MethodName == "metaWeblog.newMediaObject") {
                var result = MetaWeblogNewMediaObject(
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value);
                context.Response = new XRpcMethodResponse().Add(result);
            }
        }

        private XRpcStruct MetaWeblogNewMediaObject(
            string userName,
            string password,
            XRpcStruct file) {

            var user = _membershipService.ValidateUser(userName, password);
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMedia, user, null)) {
                //TEMP: return appropriate access-denied response for user
                throw new ApplicationException("Access denied");
            }

            var name = file.Optional<string>("name");
            var bits = file.Optional<byte[]>("bits");

            string publicUrl = _mediaService.UploadMediaFile(Path.GetDirectoryName(name), Path.GetFileName(name), bits, true);
            return new XRpcStruct().Set("url", publicUrl);
        }
    }
}