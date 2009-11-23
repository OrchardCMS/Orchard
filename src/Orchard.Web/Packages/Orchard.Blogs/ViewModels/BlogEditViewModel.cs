using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Models;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogEditViewModel : AdminViewModel {
        public Blog Blog { get; set; }
        public IEnumerable<ModelTemplate> Editors { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Blog.Id; }
        }

        [Required]
        public string Name {
            get { return Blog.As<Blog>().Record.Name; }
            set { Blog.As<Blog>().Record.Name = value; }
        }

        [Required]
        public string Slug {
            get { return Blog.As<Blog>().Record.Slug; }
            set { Blog.As<Blog>().Record.Slug = value; }
        }

        public bool Enabled {
            get { return Blog.As<Blog>().Record.Enabled; }
            set { Blog.As<Blog>().Record.Enabled = value; }
        }
    }
}