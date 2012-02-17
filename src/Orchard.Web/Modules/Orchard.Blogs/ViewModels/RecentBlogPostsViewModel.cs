using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class RecentBlogPostsViewModel {
        [Required]
        public int Count { get; set; }
        
        [Required]
        public int BlogId { get; set; }

        public IEnumerable<BlogPart> Blogs { get; set; }

    }
}