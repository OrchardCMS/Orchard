using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Security;
using Orchard.Tags.Helpers;
using Orchard.Tags.Models;

namespace Orchard.Tags.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly ITagService _tagService;
        private readonly IOrchardServices _orchardServices;

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
                case "metaWeblog.getCategories": // hack... because live writer still asks for it...
                    if (context.Response == null)
                        context.Response = new XRpcMethodResponse().Add(new XRpcArray());
                    break;
                case "wp.getTags":
                    var tags = MetaWeblogGetTags(
                        Convert.ToString(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value));
                    context.Response = new XRpcMethodResponse().Add(tags);
                    break;
                case "metaWeblog.getPost":
                    MetaWeblogAttachTagsToPost(
                        GetPost(context.Response),
                        Convert.ToInt32(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        context._drivers);
                    break;
                case "metaWeblog.newPost":
                    MetaWeblogUpdateTags(
                        GetId(context.Response), // for a new Post, the id is in the Response, as the request contains the blogId
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct)context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
                case "metaWeblog.editPost":
                    MetaWeblogUpdateTags(
                        Convert.ToInt32(context.Request.Params[0].Value),
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

        private void MetaWeblogAttachTagsToPost(XRpcStruct postStruct, int postId, string userName, string password, ICollection<IXmlRpcDriver> drivers) {
            if (postId < 1)
                return;

            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessAdminPanel, user, null);

            var driver = new XmlRpcDriver(item => {
                var post = item as XRpcStruct;
                if (post == null)
                    return;

                var contentItem = _contentManager.Get(postId, VersionOptions.Latest);
                if (contentItem == null)
                    return;

                var tags = contentItem.As<TagsPart>().CurrentTags;
                post.Set("mt_keywords", string.Join(", ", tags));
            });

            if (postStruct != null)
                driver.Process(postStruct);
            else
                drivers.Add(driver);
        }

        private static XRpcStruct GetPost(XRpcMethodResponse response) {
            return response != null && response.Params.Count == 1 && response.Params[0].Value is XRpcStruct
                       ? response.Params[0].Value as XRpcStruct
                       : null;
        }

        private XRpcArray MetaWeblogGetTags(string appKey, string userName, string password) {
            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessAdminPanel, user, null);

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

        private void MetaWeblogUpdateTags(int contentItemId, string userName, string password, XRpcStruct content, bool publish, ICollection<IXmlRpcDriver> drivers) {
            var user = _membershipService.ValidateUser(userName, password);

            var rawTags = content.Optional<string>("mt_keywords");
            if (string.IsNullOrWhiteSpace(rawTags))
                return;

            var tags = TagHelpers.ParseCommaSeparatedTagNames(rawTags);
            var driver = new XmlRpcDriver(item => {
                if (!(item is int))
                    return;

                var id = (int)item;
                var contentItem = _contentManager.Get(id, VersionOptions.Latest);
                if (contentItem == null)
                    return;

                _orchardServices.WorkContext.CurrentUser = user;
                _tagService.UpdateTagsForContentItem(contentItem, tags);
            });

            if (contentItemId > 0)
                driver.Process(contentItemId);
            else
                drivers.Add(driver);
        }

        public class XmlRpcDriver : IXmlRpcDriver {
            private readonly Action<object> _process;

            public XmlRpcDriver(Action<object > process) {
                _process = process;
            }

            public void Process(object item) {
                _process(item);
            }
        }
    }
}