using System;
using System.IO;
using System.Web;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Security;

namespace Orchard.Media.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly HttpContextBase _httpContext;

        public XmlRpcHandler(
            IMembershipService membershipService, 
            IAuthorizationService authorizationService,
            HttpContextBase httpContext) {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            _httpContext = httpContext;
        }

        public void Process(XmlRpcContext context) {
            var uriBuilder = new UriBuilder(context.HttpContext.Request.Url) {
                Path = context.HttpContext.Request.ApplicationPath,
                Query = string.Empty
            };


            if (context.Request.MethodName == "metaWeblog.newMediaObject") {
                var result = MetaWeblogNewMediaObject(
                    uriBuilder,
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value);
                context.Response = new XRpcMethodResponse().Add(result);
            }
        }

        private XRpcStruct MetaWeblogNewMediaObject(
            UriBuilder uriBuilder,
            string blogId,
            string userName,
            string password,
            XRpcStruct file) {

            var user = _membershipService.ValidateUser(userName, password);
            if (!_authorizationService.CheckAccess(user, Permissions.UploadMediaFiles)) {
                //TEMP: return appropriate access-denied response for user
                throw new ApplicationException("Access denied");
            }


            var name = file.Optional<string>("name");
            var bits = file.Optional<byte[]>("bits");

            var target = HttpContext.Current.Server.MapPath("~/Media/" + name);
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            using (var stream = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                stream.Write(bits, 0, bits.Length);
            }

            uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + "/Media/" + name.TrimStart('/');
            return new XRpcStruct().Set("url", uriBuilder.Uri.AbsoluteUri);
        }

    }
}
