using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostEditViewModel : AdminViewModel {
        public Blog Blog { get; set; }
        public BlogPost Post { get; set; }
        public ItemEditorViewModel ItemView { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Post.Id; }
        }

        [Required]
        public string Title {
            get { return Post.As<RoutableAspect>().Record.Title; }
            set { Post.As<RoutableAspect>().Record.Title = value; }
        }

        [Required]
        public string Body {
            get { return Post.As<BodyAspect>().Record.Text; }
            set { Post.As<BodyAspect>().Record.Text = value; }
        }

        [Required]
        public string Slug {
            get { return Post.As<RoutableAspect>().Record.Slug; }
            set { Post.As<RoutableAspect>().Record.Slug = value; }
        }

        public DateTime? Published {
            get { return Post.Record.Published; }
            set { Post.Record.Published = value; }
        }
    }
}