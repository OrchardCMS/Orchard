using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Blogs.Extensions;

namespace Orchard.Blogs.Services {
    [UsedImplicitly]
    public class XmlRpcHandler : IXmlRpcHandler {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;
        private readonly RouteCollection _routeCollection;

        public XmlRpcHandler(IBlogService blogService, IBlogPostService blogPostService, IContentManager contentManager,
            IAuthorizationService authorizationService, IMembershipService membershipService,
            RouteCollection routeCollection) {
            _blogService = blogService;
            _blogPostService = blogPostService;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _membershipService = membershipService;
            _routeCollection = routeCollection;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Process(XmlRpcContext context) {
            var urlHelper = new UrlHelper(context.ControllerContext.RequestContext, _routeCollection);

            if (context.Request.MethodName == "blogger.getUsersBlogs") {
                var result = MetaWeblogGetUserBlogs(urlHelper,
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value));

                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.getRecentPosts") {
                var result = MetaWeblogGetRecentPosts(urlHelper,
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    Convert.ToInt32(context.Request.Params[3].Value));

                context.Response = new XRpcMethodResponse().Add(result);
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
                    urlHelper,
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

            if (context.Request.MethodName == "blogger.deletePost") {
                var result = MetaWeblogDeletePost(
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    Convert.ToString(context.Request.Params[3].Value),
                    Convert.ToBoolean(context.Request.Params[4].Value));
                context.Response = new XRpcMethodResponse().Add(result);
            }
        }

        private XRpcArray MetaWeblogGetUserBlogs(UrlHelper urlHelper,
            string appkey,
            string userName,
            string password) {

            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessFrontEnd, user, null);

            var array = new XRpcArray();
            foreach (var blog in _blogService.Get()) {
                array.Add(new XRpcStruct()
                              .Set("url", urlHelper.AbsoluteAction(() => urlHelper.Blog(blog.Slug)))
                              .Set("blogid", blog.Id)
                              .Set("blogName", blog.Name));
            }
            return array;
        }

        private XRpcArray MetaWeblogGetRecentPosts(
            UrlHelper urlHelper,
            string blogId,
            string userName,
            string password,
            int numberOfPosts) {

            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessFrontEnd, user, null);

            var blog = _contentManager.Get<Blog>(Convert.ToInt32(blogId));
            if (blog == null)
                throw new ArgumentException();

            var array = new XRpcArray();
            foreach (var blogPost in _blogPostService.Get(blog).Take(numberOfPosts)) {
                array.Add(CreateBlogStruct(blogPost, urlHelper));
            }
            return array;
        }

        private int MetaWeblogNewPost(
            string blogId,
            string userName,
            string password,
            XRpcStruct content,
            bool publish) {

            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(Permissions.EditBlogPost, user, null);

            var blog = _contentManager.Get<Blog>(Convert.ToInt32(blogId));
            if (blog == null)
                throw new ArgumentException();

            var title = content.Optional<string>("title");
            var description = content.Optional<string>("description");
            var slug = content.Optional<string>("wp_slug");

            var blogPost = _contentManager.New<BlogPost>(BlogPostDriver.ContentType.Name);
            blogPost.Blog = blog;
            blogPost.Title = title;
            blogPost.Slug = slug;
            blogPost.Text = description;
            blogPost.Creator = user;

            _contentManager.Create(blogPost.ContentItem, VersionOptions.Draft);

            if (publish)
                _blogPostService.Publish(blogPost);

            return blogPost.Id;
        }

        private XRpcStruct MetaWeblogGetPost(
            UrlHelper urlHelper,
            int postId,
            string userName,
            string password) {

            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessFrontEnd, user, null);

            var blogPost = _blogPostService.Get(postId);
            if (blogPost == null)
                throw new ArgumentException();

            return CreateBlogStruct(blogPost, urlHelper);
        }

        private bool MetaWeblogEditPost(
            int postId,
            string userName,
            string password,
            XRpcStruct content,
            bool publish) {

            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessFrontEnd, user, null);

            var blogPost = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (blogPost == null)
                throw new ArgumentException();


            var title = content.Optional<string>("title");
            var description = content.Optional<string>("description");
            var slug = content.Optional<string>("wp_slug");

            blogPost.Title = title;
            blogPost.Slug = slug;
            blogPost.Text = description;

            if (publish) {
                _blogPostService.Publish(blogPost);
            }

            return true;
        }

        private bool MetaWeblogDeletePost(
                string appkey,
                string postId,
                string userName,
                string password,
                bool publish) {
            var user = _membershipService.ValidateUser(userName, password);
            _authorizationService.CheckAccess(StandardPermissions.AccessFrontEnd, user, null);

            var blogPost = _blogPostService.Get(Convert.ToInt32(postId), VersionOptions.Latest);
            if (blogPost == null)
                throw new ArgumentException();

            _blogPostService.Delete(blogPost);
            return true;
        }

        private static XRpcStruct CreateBlogStruct(BlogPost blogPost, UrlHelper urlHelper) {
            var url = urlHelper.AbsoluteAction(() => urlHelper.BlogPost(blogPost));
            return new XRpcStruct()
                .Set("postid", blogPost.Id)
                .Set("dateCreated", blogPost.CreatedUtc)
                .Set("title", blogPost.Title)
                .Set("wp_slug", blogPost.Slug)
                .Set("description", blogPost.Text)
                .Set("link", url)
                .Set("permaLink", url);
        }
    }
}
