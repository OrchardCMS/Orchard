using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;

namespace Orchard.Blogs.Extensions {
    public static class BlogCreateViewModelExtensions {
        public static CreateBlogParams ToCreateBlogParams(this CreateBlogViewModel viewModel) {
            return new CreateBlogParams() {Name = viewModel.Name, Slug = viewModel.Slug, Enabled = viewModel.Enabled};
        }
    }
}