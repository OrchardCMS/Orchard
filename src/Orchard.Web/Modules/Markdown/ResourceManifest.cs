using Orchard.UI.Resources;

namespace Markdown {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Markdown_Converter").SetUrl("Markdown.Converter.js");
            manifest.DefineScript("Markdown_Sanitizer").SetUrl("Markdown.Sanitizer.js").SetDependencies("Markdown_Converter");
            manifest.DefineScript("Markdown_Editor").SetUrl("Markdown.Editor.js").SetDependencies("Markdown_Sanitizer");
            manifest.DefineScript("Resizer").SetUrl("jquery.textarearesizer.min.js").SetDependencies("jQuery");

            manifest.DefineScript("OrchardMarkdown").SetUrl("orchard-markdown.js").SetDependencies("Resizer", "Markdown_Editor");
            manifest.DefineStyle("OrchardMarkdown").SetUrl("admin-markdown.css");
        }
    }
}
