using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Orchard.CmsPages.ViewModels;
using Orchard.Logging;
using Orchard.XmlRpc;
using Orchard.XmlRpc.Models;

namespace Orchard.CmsPages.Services {
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IPageManager _pageManager;

        public XmlRpcHandler(IPageManager pageManager) {
            _pageManager = pageManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Process(XmlRpcContext context) {
            var uriBuilder = new UriBuilder(context.HttpContext.Request.Url);
            uriBuilder.Path = context.HttpContext.Request.ApplicationPath;
            uriBuilder.Query = string.Empty;


            if (context.Request.MethodName == "blogger.getUsersBlogs") {
                context.Response = new XRpcMethodResponse()
                    .Add(new XRpcArray()
                             .Add(new XRpcStruct()
                                      .Set("url", uriBuilder.Uri.AbsoluteUri)
                                      .Set("blogid", "Orchard.CmsPages")
                                      .Set("blogName", "Orchard Pages")));
            }

            if (context.Request.MethodName == "metaWeblog.newPost") {
                var result = MetaWeblogNewPost(
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value,
                    Convert.ToBoolean(context.Request.Params[4].Value));

                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.getPost") {
                var result = MetaWeblogGetPost(
                    Convert.ToInt32(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value));
                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.editPost") {
                var result = MetaWeblogEditPost(
                    Convert.ToInt32(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value,
                    Convert.ToBoolean(context.Request.Params[4].Value));
                context.Response = new XRpcMethodResponse().Add(result);
            }
        }

        private int MetaWeblogNewPost(
            string blogId,
            string user,
            string password,
            XRpcStruct content,
            bool publish) {

            var title = content.Optional<string>("title");
            var description = content.Optional<string>("description");

            var pageRevision = _pageManager.CreatePage(new PageCreateViewModel {
                Title = title,
                TemplateName = "TwoColumns"
            });
            pageRevision.Contents.First().Content = description;

            if (publish) {
                if (string.IsNullOrEmpty(pageRevision.Slug))
                    pageRevision.Slug = "slug" + pageRevision.Page.Id;

                _pageManager.Publish(pageRevision, new PublishOptions());
            }

            return pageRevision.Page.Id;
        }

        private XRpcStruct MetaWeblogGetPost(
            int postId,
            string user,
            string password) {

            var pageRevision = _pageManager.GetLastRevision(postId);

            var url = "http://localhost/orchard/" + pageRevision.Slug;

            return new XRpcStruct()
                .Set("userid", 37)
                .Set("postid", pageRevision.Page.Id)
                .Set("description", pageRevision.Contents.First().Content)
                .Set("title", pageRevision.Title)
                .Set("link", url)
                .Set("permaLink", url);
        }

        private bool MetaWeblogEditPost(
            int postId,
            string user,
            string password,
            XRpcStruct content,
            bool publish) {

            var pageRevision = _pageManager.AcquireDraft(postId);

            var title = content.Optional<string>("title");
            var description = content.Optional<string>("description");

            pageRevision.Title = title;
            pageRevision.Contents.First().Content = description;

            if (publish) {
                if (string.IsNullOrEmpty(pageRevision.Slug))
                    pageRevision.Slug = "slug" + postId;

                _pageManager.Publish(pageRevision, new PublishOptions());
            }

            return true;

        }

    }
}
