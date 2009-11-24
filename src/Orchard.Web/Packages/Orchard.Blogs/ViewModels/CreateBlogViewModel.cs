using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogViewModel : AdminViewModel {
        [Required]
        public string Name { get; set; }

        //TODO: (erikpo) Need a data type for slug
        [Required]
        public string Slug { get; set; }

        public string Description { get; set; }

        //[Required]
        //public bool Enabled { get; set; }
    }
}