using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests
{
    public class FontAwesome : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest.DefineStyle("FontAwesome").SetVersion("7.1.0").SetDependencies("FontAwesome.All");

            manifest.DefineStyle("FontAwesome.V4Compatibility")
                .SetVersion("7.1.0")
                .SetDependencies("FontAwesome.V4FontFace", "FontAwesome.V4Shims");

            manifest.DefineStyle("FontAwesome.All")
                .SetUrl("FontAwesome/all.min.css", "FontAwesome/all.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/all.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/all.css");

            manifest.DefineStyle("FontAwesome.Brands")
                .SetUrl("FontAwesome/brands.min.css", "FontAwesome/brands.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/brands.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/brands.css");

            manifest.DefineStyle("FontAwesome.Core")
                .SetUrl("FontAwesome/fontawesome.min.css", "FontAwesome/fontawesome.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/fontawesome.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/fontawesome.css");

            manifest.DefineStyle("FontAwesome.Regular")
                .SetUrl("FontAwesome/regular.min.css", "FontAwesome/regular.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/regular.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/regular.css");

            manifest.DefineStyle("FontAwesome.Solid")
                .SetUrl("FontAwesome/solid.min.css", "FontAwesome/solid.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/solid.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/solid.css");

            manifest.DefineStyle("FontAwesome.V4FontFace")
                .SetUrl("FontAwesome/v4-font-face.min.css", "FontAwesome/v4-font-face.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/v4-font-face.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/v4-font-face.css");

            manifest.DefineStyle("FontAwesome.V4Shims")
                .SetUrl("FontAwesome/v4-shims.min.css", "FontAwesome/v4-shims.css")
                .SetVersion("7.1.0")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/v4-shims.min.css",
                    "//cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@7.1.0/css/v4-shims.css");
        }
    }
}
