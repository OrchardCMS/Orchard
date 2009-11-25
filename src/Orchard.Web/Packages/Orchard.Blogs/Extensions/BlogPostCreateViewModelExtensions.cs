using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;

namespace Orchard.Blogs.Extensions {
    public static class BlogPostCreateViewModelExtensions {
        public static CreateBlogPostParams ToCreateBlogPostParams(this CreateBlogPostViewModel viewModel, Blog blog) {
            return new CreateBlogPostParams() {Blog = blog, Title = viewModel.Title, Body = viewModel.Body, Slug = viewModel.Slug, Published = viewModel.Published};
        }
    }
}