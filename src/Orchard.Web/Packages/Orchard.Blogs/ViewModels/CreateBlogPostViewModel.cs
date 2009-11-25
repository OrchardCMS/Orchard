using System;
using System.ComponentModel.DataAnnotations;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogPostViewModel : AdminViewModel {
        public Blog Blog { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        //TODO: (erikpo) Need a data type for slug
        [Required]
        public string Slug { get; set; }

        public DateTime? Published { get; set; }
    }
}