using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogPostRecord> _blogPostRepository;

        public BlogPostService(IContentManager contentManager, IRepository<BlogPostRecord> blogPostRepository) {
            _contentManager = contentManager;
            _blogPostRepository = blogPostRepository;
        }

        public BlogPost Get(Blog blog, string slug) {
            return
                _contentManager.Query<BlogPost, BlogPostRecord>().Join<RoutableRecord>().Where(rr => rr.Slug == slug).
                    Join<CommonRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord).List().
                    SingleOrDefault();
        }

        public IEnumerable<BlogPost> Get(Blog blog) {
            return GetBlogQuery(blog).List();
        }

        public IEnumerable<BlogPost> Get(Blog blog, ArchiveData archiveData) {
            var query = GetBlogQuery(blog);

            if (archiveData.Day > 0)
                query = query.Where(cr => cr.CreatedUtc >= new DateTime(archiveData.Year, archiveData.Month, archiveData.Day) && cr.CreatedUtc < new DateTime(archiveData.Year, archiveData.Month, archiveData.Day + 1));
            else if (archiveData.Month > 0)
                query = query.Where(cr => cr.CreatedUtc >= new DateTime(archiveData.Year, archiveData.Month, 1) && cr.CreatedUtc < new DateTime(archiveData.Year, archiveData.Month + 1, 1));
            else
                query = query.Where(cr => cr.CreatedUtc >= new DateTime(archiveData.Year, 1, 1) && cr.CreatedUtc < new DateTime(archiveData.Year + 1, 1, 1));

            return query.List();
        }

        public void Delete(BlogPost blogPost) {
            _blogPostRepository.Delete(blogPost.Record);
        }

        private IContentQuery<BlogPost, CommonRecord> GetBlogQuery(Blog blog) {
            return
                _contentManager.Query<BlogPost, BlogPostRecord>().Join<CommonRecord>().Where(
                    cr => cr.Container == blog.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}