using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogArchiveHandler : ContentHandler {
        public BlogArchiveHandler(IRepository<BlogArchiveRecord> blogArchiveRepository, IRepository<CommonRecord> commonRepository) {
            OnPublished<BlogPost>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, commonRepository, bp));
            OnRemoved<BlogPost>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, commonRepository, bp));
        }

        private static void RecalculateBlogArchive(IRepository<BlogArchiveRecord> blogArchiveRepository, IRepository<CommonRecord> commonRepository, BlogPost blogPost) {
            blogArchiveRepository.Flush();

            //INFO: (erikpo) Remove all current blog archive records
            var blogArchiveRecords =
                from bar in blogArchiveRepository.Table
                where bar.Blog == blogPost.Blog.Record
                select bar;
            blogArchiveRecords.ToList().ForEach(blogArchiveRepository.Delete);

            //INFO: (erikpo) Get all blog posts for the current blog
            var postsQuery =
                from bpr in commonRepository.Table
                where bpr.ContentItemRecord.ContentType.Name == BlogPostDriver.ContentType.Name && bpr.Container.Id == blogPost.Blog.Record.Id
                orderby bpr.PublishedUtc
                select bpr;

            //INFO: (erikpo) Create a dictionary of all the year/month combinations and their count of posts that are published in this blog
            var inMemoryBlogArchives = new Dictionary<DateTime, int>(postsQuery.Count());
            foreach (var post in postsQuery) {
                if (!post.PublishedUtc.HasValue)
                    continue;

                var key = new DateTime(post.PublishedUtc.Value.Year, post.PublishedUtc.Value.Month, 1);

                if (inMemoryBlogArchives.ContainsKey(key))
                    inMemoryBlogArchives[key]++;
                else
                    inMemoryBlogArchives[key] = 1;
            }

            //INFO: (erikpo) Create the new blog archive records based on the in memory values
            foreach (KeyValuePair<DateTime, int> item in inMemoryBlogArchives) {
                blogArchiveRepository.Create(new BlogArchiveRecord {Blog = blogPost.Blog.Record, Year = item.Key.Year, Month = item.Key.Month, PostCount = item.Value});
            }
        }
    }
}