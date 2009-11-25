using System;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public class CreateBlogPostParams {
        public Blog Blog { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }
        public DateTime? Published { get; set; }
    }
}