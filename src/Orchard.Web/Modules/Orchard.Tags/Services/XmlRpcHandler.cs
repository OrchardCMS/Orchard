using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Security;
using Orchard.Tags.Helpers;

namespace Orchard.Tags.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly ITagService _tagService;
        private readonly IOrchardServices _orchardServices;
        private IEnumerable<string> _tags;

        public XmlRpcHandler(
            IMembershipService membershipService,
            IAuthorizationService authorizationService,
            IContentManager contentManager,
            ITagService tagService,
            IOrchardServices orchardServices) {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _tagService = tagService;
            _orchardServices = orchardServices;
        }

        public void SetCapabilities(XElement options) {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
            options.SetElementValue(XName.Get("supportsKeywords", manifestUri), "Yes");
            options.SetElementValue(XName.Get("supportsGetTags", manifestUri), "Yes");
            options.SetElementValue(XName.Get("keywordsAsTags", manifestUri), "Yes");
        }

        public void Process(XmlRpcContext context) {
            switch (context.Request.MethodName) {
                case "wp.getTags":
                    var tags = MetaWeblogGetTags(
                        Convert.ToString(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value));
                    context.Response = new XRpcMethodResponse().Add(tags);
                    break;
                case "metaWeblog.newPost":
                    MetaWeblogUpdateTags(
                        GetId(context.Response),
                        Convert.ToString(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct)context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
                case "metaWeblog.editPost":
                    MetaWeblogUpdateTags(
                        GetId(context.Response),
                        Convert.ToString(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct)context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
            }
        }

        private static int GetId(XRpcMethodResponse response) {
            return response != null && response.Params.Count == 1 && response.Params[0].Value is int
                       ? Convert.ToInt32(response.Params[0].Value)
                       : 0;
        }

        private XRpcArray MetaWeblogGetTags(string appKey, string userName, string password) {
            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessFrontEnd, user, null);

            var array = new XRpcArray();
            foreach (var tag in _tagService.GetTags()) {
                var thisTag = tag;
                array.Add(new XRpcStruct()
                              .Set("tag_id", thisTag.TagName)
                              .Set("name", thisTag.TagName));
                // nyi - not yet implemented
                              //.Set("count", "")
                              //.Set("slug", "")
                              //.Set("html_url", "")
                              //.Set("rss_url", ""));
            }

            return array;
        }

        private void MetaWeblogUpdateTags(int contentItemId, string appKey, string userName, string password, XRpcStruct content, bool publish, ICollection<IXmlRpcDriver> drivers) {
            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(Permissions.ApplyTag, user, null);

            var rawTags = content.Optional<string>("mt_keywords");
            if (string.IsNullOrWhiteSpace(rawTags))
                return;

            var driver = new XmlRpcDriver(id => {
                var contentItem = _contentManager.Get(id);
                if (contentItem == null)
                    return;

                _orchardServices.WorkContext.CurrentUser = user;
                _tagService.UpdateTagsForContentItem(id, _tags);
            });

            _tags = TagHelpers.ParseCommaSeparatedTagNames(rawTags);
            if (contentItemId > 0)
                driver.Process(contentItemId);
            else
                drivers.Add(driver);
        }

        public class XmlRpcDriver : IXmlRpcDriver {
            private readonly Action<int> _process;

            public XmlRpcDriver(Action<int> process) {
                _process = process;
            }

            public void Process(int id) {
                _process(id);
            }
        }
    }
}