using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Autoroute.Models;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;

namespace Orchard.Blogs.Services {
    [UsedImplicitly]
    public class BlogService : IBlogService {
        private readonly IContentManager _contentManager;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly HashSet<int> _processedBlogParts = new HashSet<int>();
        public BlogService(
            IContentManager contentManager,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager) {
            _contentManager = contentManager;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public BlogPart Get(string path) {
            return _contentManager.Query<AutoroutePart, AutoroutePartRecord>().Where(r => r.DisplayAlias == path).ForPart<BlogPart>().Slice(0, 1).FirstOrDefault();
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            var blogPart = _contentManager.Get<BlogPart>(id, versionOptions);
            return blogPart == null ? null : blogPart.ContentItem;
        }

        public IEnumerable<BlogPart> Get() {
            return Get(VersionOptions.Published);
        }

        public IEnumerable<BlogPart> Get(VersionOptions versionOptions) {
            return _contentManager.Query<BlogPart>(versionOptions, "Blog")
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void Delete(ContentItem blog) {
            _contentManager.Remove(blog);
        }

        public void ProcessBlogPostsCount(int blogPartId) {
            if (!_processedBlogParts.Contains(blogPartId)) {
                _processedBlogParts.Add(blogPartId);
                _processingEngine.AddTask(_shellSettings, _shellDescriptorManager.GetShellDescriptor(), "IBlogPostsCountProcessor.Process", new Dictionary<string, object> { { "blogPartId", blogPartId } });
            }
        }
    }
}