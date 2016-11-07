using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Localization;
using Orchard.PublishLater.Models;
using Orchard.Security;
using Orchard.Tasks.Scheduling;

namespace Orchard.PublishLater.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IContentManager _contentManager;
        private readonly IPublishingTaskManager _publishingTaskManager;
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;

        public XmlRpcHandler(IContentManager contentManager,
            IPublishingTaskManager publishingTaskManager,
            IMembershipService membershipService,
            IAuthorizationService authorizationService) {
            _contentManager = contentManager;
            _publishingTaskManager = publishingTaskManager;
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void SetCapabilities(XElement options) {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
            options.SetElementValue(XName.Get("supportsCustomDate", manifestUri), "Yes");
        }

        public void Process(XmlRpcContext context) {
            switch (context.Request.MethodName) {
                case "metaWeblog.newPost":
                    MetaWeblogSetCustomPublishedDate(
                        GetId(context.Response),
                        Convert.ToString(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct)context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
                case "metaWeblog.editPost":
                    MetaWeblogSetCustomPublishedDate(
                        GetId(context.Response),
                        Convert.ToString(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        (XRpcStruct)context.Request.Params[3].Value,
                        Convert.ToBoolean(context.Request.Params[4].Value),
                        context._drivers);
                    break;
                case "metaWeblog.getPost":
                    MetaWeblogGetCustomPublishedDate(
                        GetPost(context.Response),
                        Convert.ToInt32(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        context._drivers);
                    break;
                case "metaWeblog.getRecentPosts":
                    MetaWeblogGetCustomPublishedDate(
                        GetPost(context.Response),
                        Convert.ToInt32(context.Request.Params[0].Value),
                        Convert.ToString(context.Request.Params[1].Value),
                        Convert.ToString(context.Request.Params[2].Value),
                        context._drivers);
                    break;
            }
        }

        private void MetaWeblogGetCustomPublishedDate(XRpcStruct postStruct, int itemId, string userName, string password, ICollection<IXmlRpcDriver> drivers) {
            if (itemId < 1)
                return;

            var driver = new XmlRpcDriver(item => {
                var post = item as XRpcStruct;
                if (post == null)
                    return;

                var postId = post.Optional<int>("postid");
                var contentItem = _contentManager.Get(postId, VersionOptions.Latest);
                if (contentItem == null)
                    return;

                var publishedUtc = contentItem.As<PublishLaterPart>().ScheduledPublishUtc.Value;
                if (publishedUtc == null || publishedUtc <= DateTime.UtcNow)
                    return;

                post.Set("dateCreated", publishedUtc);
                post.Set("date_created_gmt", publishedUtc);
            });

            if (postStruct != null)
                driver.Process(postStruct);
            else
                drivers.Add(driver);
        }

        private void MetaWeblogSetCustomPublishedDate(int contentItemId, string appKey, string userName, string password, XRpcStruct content, bool publish, ICollection<IXmlRpcDriver> drivers) {
            var user = ValidateUser(userName, password);
            if (user == null)
                return;

            var publishedUtc = content.Optional<DateTime?>("dateCreated");
            if (publishedUtc == null || publishedUtc <= DateTime.UtcNow) // only post-dating/scheduling of content with the PublishLaterPart
                return;

            var driver = new XmlRpcDriver(item => {
                if (!(item is int))
                    return;

                var id = (int)item;
                var contentItem = _contentManager.Get(id, VersionOptions.Latest);
                if (contentItem == null || !contentItem.Is<PublishLaterPart>())
                    return;

                _authorizationService.CheckAccess(Permissions.PublishContent, user, null);

                contentItem.As<PublishLaterPart>().ScheduledPublishUtc.Value = publishedUtc;
                _publishingTaskManager.Publish(contentItem, (DateTime)publishedUtc);
            });

            if (contentItemId > 0)
                driver.Process(contentItemId);
            else
                drivers.Add(driver);
        }

        private static XRpcStruct GetPost(XRpcMethodResponse response) {
            return response != null && response.Params.Count == 1 && response.Params[0].Value is XRpcStruct
                       ? response.Params[0].Value as XRpcStruct
                       : null;
        }

        private static int GetId(XRpcMethodResponse response) {
            return response != null && response.Params.Count == 1 && response.Params[0].Value is int
                       ? Convert.ToInt32(response.Params[0].Value)
                       : 0;
        }

        private IUser ValidateUser(string userName, string password) {
            IUser user = _membershipService.ValidateUser(userName, password);
            if (user == null) {
                throw new OrchardCoreException(T("The username or e-mail or password provided is incorrect."));
            }

            return user;
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