using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Blogs.Extensions;
using Orchard.Mvc.Html;
using Orchard.Core.Title.Models;

namespace Orchard.Blogs.Services {
    [UsedImplicitly]
    [OrchardFeature("Orchard.Blogs.RemotePublishing")]
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
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void SetCapabilities(XElement options) {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";
            options.SetElementValue(XName.Get("supportsSlug", manifestUri), "Yes");
        }

        public void Process(XmlRpcContext context) {
            var urlHelper = new UrlHelper(context.ControllerContext.RequestContext, _routeCollection);

            if (context.Request.MethodName == "blogger.getUsersBlogs") {
                var result = MetaWeblogGetUserBlogs(urlHelper,
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value));

                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.getRecentPosts") {
                var result = MetaWeblogGetRecentPosts(urlHelper,
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    Convert.ToInt32(context.Request.Params[3].Value),
                    context._drivers);

                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.newPost") {
                var result = MetaWeblogNewPost(
                    Convert.ToString(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value,
                    Convert.ToBoolean(context.Request.Params[4].Value),
                    context._drivers);

                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.getPost") {
                var result = MetaWeblogGetPost(
                    urlHelper,
                    Convert.ToInt32(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    context._drivers);
                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "metaWeblog.editPost") {
                var result = MetaWeblogEditPost(
                    Convert.ToInt32(context.Request.Params[0].Value),
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    (XRpcStruct)context.Request.Params[3].Value,
                    Convert.ToBoolean(context.Request.Params[4].Value),
                    context._drivers);
                context.Response = new XRpcMethodResponse().Add(result);
            }

            if (context.Request.MethodName == "blogger.deletePost") {
                var result = MetaWeblogDeletePost(
                    Convert.ToString(context.Request.Params[1].Value),
                    Convert.ToString(context.Request.Params[2].Value),
                    Convert.ToString(context.Request.Params[3].Value),
                    context._drivers);
                context.Response = new XRpcMethodResponse().Add(result);
            }
        }

        private XRpcArray MetaWeblogGetUserBlogs(UrlHelper urlHelper,
            string userName,
            string password) {

            IUser user = ValidateUser(userName, password);

            XRpcArray array = new XRpcArray();
            foreach (BlogPart blog in _blogService.Get()) {
                // User needs to at least have permission to edit its own blog posts to access the service
                if (_authorizationService.TryCheckAccess(Permissions.EditBlogPost, user, blog)) {

                    BlogPart blogPart = blog;
                    array.Add(new XRpcStruct()
                                  .Set("url", urlHelper.AbsoluteAction(() => urlHelper.Blog(blogPart)))
                                  .Set("blogid", blog.Id)
                                  .Set("blogName", _contentManager.GetItemMetadata(blog).DisplayText));
                }
            }

            return array;
        }

        private XRpcArray MetaWeblogGetRecentPosts(
            UrlHelper urlHelper,
            string blogId,
            string userName,
            string password,
            int numberOfPosts,
            IEnumerable<IXmlRpcDriver> drivers) {

            IUser user = ValidateUser(userName, password);

            // User needs to at least have permission to edit its own blog posts to access the service
            _authorizationService.CheckAccess(Permissions.EditBlogPost, user, null);

            BlogPart blog = _contentManager.Get<BlogPart>(Convert.ToInt32(blogId));
            if (blog == null) {
                throw new ArgumentException();
            }

            var array = new XRpcArray();
            foreach (var blogPost in _blogPostService.Get(blog, 0, numberOfPosts, VersionOptions.Latest)) {
                var postStruct = CreateBlogStruct(blogPost, urlHelper);

                foreach (var driver in drivers)
                    driver.Process(postStruct);

                array.Add(postStruct);
            }
            return array;
        }

        private int MetaWeblogNewPost(
            string blogId,
            string userName,
            string password,
            XRpcStruct content,
            bool publish,
            IEnumerable<IXmlRpcDriver> drivers) {

            IUser user = ValidateUser(userName, password);

            // User needs permission to edit or publish its own blog posts
            _authorizationService.CheckAccess(publish ? Permissions.PublishBlogPost : Permissions.EditBlogPost, user, null);

            BlogPart blog = _contentManager.Get<BlogPart>(Convert.ToInt32(blogId));
            if (blog == null)
                throw new ArgumentException();

            var title = content.Optional<string>("title");
            var description = content.Optional<string>("description");
            var slug = content.Optional<string>("wp_slug");

            var blogPost = _contentManager.New<BlogPostPart>("BlogPost");

            // BodyPart
            if (blogPost.Is<BodyPart>()) {
                blogPost.As<BodyPart>().Text = description;
            }

            //CommonPart
            if (blogPost.Is<ICommonPart>()) {
                blogPost.As<ICommonPart>().Owner = user;
                blogPost.As<ICommonPart>().Container = blog;
            }

            //TitlePart
            if (blogPost.Is<TitlePart>()) {
                blogPost.As<TitlePart>().Title = HttpUtility.HtmlDecode(title);
            }
            
            //AutoroutePart
            dynamic dBlogPost = blogPost;
            if (dBlogPost.AutoroutePart!=null){
                dBlogPost.AutoroutePart.DisplayAlias = slug;
            }

            _contentManager.Create(blogPost, VersionOptions.Draft);

            // try to get the UTC timezone by default
            var publishedUtc = content.Optional<DateTime?>("date_created_gmt");
            if (publishedUtc == null) {
                // take the local one
                publishedUtc = content.Optional<DateTime?>("dateCreated");
            }
            else {
                // ensure it's read as a UTC time
                publishedUtc = new DateTime(publishedUtc.Value.Ticks, DateTimeKind.Utc);
            }

            if (publish && (publishedUtc == null || publishedUtc <= DateTime.UtcNow))
                _blogPostService.Publish(blogPost);

            if (publishedUtc != null) {
                blogPost.As<CommonPart>().CreatedUtc = publishedUtc;
            }

            foreach (var driver in drivers)
                driver.Process(blogPost.Id);

            return blogPost.Id;
        }

        private XRpcStruct MetaWeblogGetPost(
            UrlHelper urlHelper,
            int postId,
            string userName,
            string password,
            IEnumerable<IXmlRpcDriver> drivers) {

            IUser user = ValidateUser(userName, password);
            var blogPost = _blogPostService.Get(postId, VersionOptions.Latest);
            if (blogPost == null)
                throw new ArgumentException();

            _authorizationService.CheckAccess(Permissions.EditBlogPost, user, blogPost);

            var postStruct = CreateBlogStruct(blogPost, urlHelper);

            foreach (var driver in drivers)
                driver.Process(postStruct);

            return postStruct;
        }

        private bool MetaWeblogEditPost(
            int postId,
            string userName,
            string password,
            XRpcStruct content,
            bool publish,
            IEnumerable<IXmlRpcDriver> drivers) {

            IUser user = ValidateUser(userName, password);
            var blogPost = _blogPostService.Get(postId, VersionOptions.DraftRequired);
            if (blogPost == null) {
                throw new OrchardCoreException(T("The specified Blog Post doesn't exist anymore. Please create a new Blog Post."));
            }

            _authorizationService.CheckAccess(publish ? Permissions.PublishBlogPost : Permissions.EditBlogPost, user, blogPost);

            var title = content.Optional<string>("title");
            var description = content.Optional<string>("description");
            var slug = content.Optional<string>("wp_slug");

            // BodyPart
            if (blogPost.Is<BodyPart>()) {
                blogPost.As<BodyPart>().Text = description;
            }

            //TitlePart
            if (blogPost.Is<TitlePart>()) {
                blogPost.As<TitlePart>().Title = HttpUtility.HtmlDecode(title);
            }
            //AutoroutePart
            dynamic dBlogPost = blogPost;
            if (dBlogPost.AutoroutePart != null) {
                dBlogPost.AutoroutePart.DisplayAlias = slug;
            }

            // try to get the UTC timezone by default
            var publishedUtc = content.Optional<DateTime?>("date_created_gmt");
            if (publishedUtc == null) {
                // take the local one
                publishedUtc = content.Optional<DateTime?>("dateCreated");
            }
            else {
                // ensure it's read as a UTC time
                publishedUtc = new DateTime(publishedUtc.Value.Ticks, DateTimeKind.Utc);
            }

            if (publish && (publishedUtc == null || publishedUtc <= DateTime.UtcNow))
                _blogPostService.Publish(blogPost);

            if (publishedUtc != null) {
                blogPost.As<CommonPart>().CreatedUtc = publishedUtc;
            }

            foreach (var driver in drivers)
                driver.Process(blogPost.Id);

            return true;
        }

        private bool MetaWeblogDeletePost(
            string postId,
            string userName,
            string password,
            IEnumerable<IXmlRpcDriver> drivers) {

            IUser user = ValidateUser(userName, password);
            var blogPost = _blogPostService.Get(Convert.ToInt32(postId), VersionOptions.Latest);
            if (blogPost == null)
                throw new ArgumentException();

            _authorizationService.CheckAccess(Permissions.DeleteBlogPost, user, blogPost);

            foreach (var driver in drivers)
                driver.Process(blogPost.Id);

            _blogPostService.Delete(blogPost);
            return true;
        }

        private IUser ValidateUser(string userName, string password) {
            IUser user = _membershipService.ValidateUser(userName, password);
            if (user == null) {
                throw new OrchardCoreException(T("The username or e-mail or password provided is incorrect."));
            }

            return user;
        }

        private static XRpcStruct CreateBlogStruct(
            BlogPostPart blogPostPart,
            UrlHelper urlHelper) {

            var url = urlHelper.AbsoluteAction(() => urlHelper.ItemDisplayUrl(blogPostPart));

            if (blogPostPart.HasDraft()) {
                url = urlHelper.AbsoluteAction("Preview", "Item", new { area = "Contents", id = blogPostPart.ContentItem.Id });
            }

            var blogStruct = new XRpcStruct()
                .Set("postid", blogPostPart.Id)
                .Set("title", HttpUtility.HtmlEncode(blogPostPart.Title))
                
                .Set("description", blogPostPart.Text)
                .Set("link", url)
                .Set("permaLink", url);
            
            blogStruct.Set("wp_slug", blogPostPart.As<IAliasAspect>().Path);
            

            if (blogPostPart.PublishedUtc != null) {
                blogStruct.Set("dateCreated", blogPostPart.PublishedUtc);
                blogStruct.Set("date_created_gmt", blogPostPart.PublishedUtc);
            }

            return blogStruct;
        }
    }
}
