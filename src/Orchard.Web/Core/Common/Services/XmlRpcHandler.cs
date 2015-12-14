using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.Core.Common.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IContentManager _contentManager;

        public XmlRpcHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void SetCapabilities(XElement options) {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
            options.SetElementValue(XName.Get("supportsCustomDate", manifestUri), "Yes");
        }

        public void Process(XmlRpcContext context) {
            switch (context.Request.MethodName) {
                case "metaWeblog.newPost":
                    MetaWeblogSetCustomCreatedDate(
                        Convert.ToInt32(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct) context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
                case "metaWeblog.editPost":
                    MetaWeblogSetCustomCreatedDate(
                        Convert.ToInt32(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct) context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
            }
        }

        private void MetaWeblogSetCustomCreatedDate(int contentItemId, string userName, string password, XRpcStruct content, bool publish, ICollection<IXmlRpcDriver> drivers) {
            if (!publish)
                return;

            var createdUtc = content.Optional<DateTime?>("dateCreated");
            if (createdUtc == null || createdUtc > DateTime.UtcNow)
                return;

            var driver = new XmlRpcDriver(item => {
                if (!(item is int))
                    return;

                var id = (int)item;
                var contentItem = _contentManager.Get(id, VersionOptions.Latest);
                if (contentItem == null || !contentItem.Is<CommonPart>()) // only pre-dating of content with the CommonPart (obviously)
                    return;

                contentItem.As<CommonPart>().CreatedUtc = createdUtc;
            });

            if (contentItemId > 0)
                driver.Process(contentItemId);
            else
                drivers.Add(driver);
        }

        private static int GetId(XRpcMethodResponse response) {
            return response != null && response.Params.Count == 1 && response.Params[0].Value is int
                       ? Convert.ToInt32(response.Params[0].Value)
                       : 0;
        }

        public class XmlRpcDriver : IXmlRpcDriver {
            private readonly Action<object> _process;

            public XmlRpcDriver(Action<object> process) {
                _process = process;
            }

            public void Process(object item) {
                _process(item);
            }
        }
    }
}