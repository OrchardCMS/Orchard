using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPartArchiveHandler : ContentHandler {
        public BlogPartArchiveHandler(IRepository<BlogPartArchiveRecord> blogArchiveRepository, IBlogPostService blogPostService) {
            OnPublished<BlogPostPart>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, blogPostService, bp));
            OnUnpublished<BlogPostPart>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, blogPostService, bp));
            OnRemoved<BlogPostPart>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, blogPostService, bp));
        }

        private static void RecalculateBlogArchive(IRepository<BlogPartArchiveRecord> blogArchiveRepository, IBlogPostService blogPostService, BlogPostPart blogPostPart) {
            blogArchiveRepository.Flush();

            // remove all current blog archive records
            var blogArchiveRecords =
                from bar in blogArchiveRepository.Table
                where bar.BlogPart == blogPostPart.BlogPart.Record
                select bar;
            blogArchiveRecords.ToList().ForEach(blogArchiveRepository.Delete);

            // get all blog posts for the current blog
            var posts = blogPostService.Get(blogPostPart.BlogPart, VersionOptions.Published);

            // create a dictionary of all the year/month combinations and their count of posts that are published in this blog
            var inMemoryBlogArchives = new Dictionary<DateTime, int>();
            foreach (var post in posts) {
                if (!post.Has<CommonPart>())
                    continue;

                var commonPart = post.As<CommonPart>();
                var key = new DateTime(commonPart.CreatedUtc.Value.Year, commonPart.CreatedUtc.Value.Month, 1);

                if (inMemoryBlogArchives.ContainsKey(key))
                    inMemoryBlogArchives[key]++;
                else
                    inMemoryBlogArchives[key] = 1;
            }

            // create the new blog archive records based on the in memory values
            foreach (KeyValuePair<DateTime, int> item in inMemoryBlogArchives) {
                blogArchiveRepository.Create(new BlogPartArchiveRecord {BlogPart = blogPostPart.BlogPart.Record, Year = item.Key.Year, Month = item.Key.Month, PostCount = item.Value});
            }
        }
    }
}