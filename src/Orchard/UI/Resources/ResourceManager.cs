using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Mvc.Html;

namespace Orchard.UI.Resources {
    [UsedImplicitly]
    public class ResourceManager : IResourceManager {
        private const string MetaFormat = "\r\n<meta name=\"{0}\" content=\"{1}\" />";
        private const string StyleFormat = "\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
        private const string ScriptFormat = "\r\n<script type=\"text/javascript\" src=\"{0}\"></script>";
        private readonly Dictionary<string, string> _metas;
        private readonly List<FileRegistrationContext> _styles;
        private readonly List<LinkEntry> _links;
        private readonly List<FileRegistrationContext> _headScripts;
        private readonly List<FileRegistrationContext> _footScripts;

        public ResourceManager() {
            _metas = new Dictionary<string, string>(20) {{"generator", "Orchard"}};
            _styles = new List<FileRegistrationContext>(10);
            _links = new List<LinkEntry>();
            _headScripts = new List<FileRegistrationContext>(10);
            _footScripts = new List<FileRegistrationContext>(5);
        }

        public void RegisterMeta(string name, string content) {
            if (!string.IsNullOrEmpty(name) && !_metas.ContainsKey(name))
                _metas.Add(name, content);
        }

        public void RegisterStyle(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                return;

            var context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, fileName);

            if (!_styles.Contains(context))
                _styles.Add(context);
        }

        public void RegisterLink(LinkEntry entry, HtmlHelper html) {
            _links.Add(entry);
        }

        public void RegisterHeadScript(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                return;

            var context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, fileName);

            if (!_headScripts.Contains(context))
                _headScripts.Add(context);
        }

        public void RegisterFootScript(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                return;

            var context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, fileName);

            if (!_footScripts.Contains(context))
                _footScripts.Add(context);
        }

        public MvcHtmlString GetMetas() {
            return
                MvcHtmlString.Create(string.Join("\r\n",
                                                 _metas.Select(m => string.Format(MetaFormat, m.Key, m.Value)).Reverse().ToArray()));
        }

        public MvcHtmlString GetStyles() {
            return GetFiles(_styles, StyleFormat, "/styles/");
        }

        public MvcHtmlString GetLinks(HtmlHelper html) {
            var sb = new StringBuilder();
            foreach (var link in _links) {
                sb.Append("\r\n");
                sb.Append(@"<link");

                if (!string.IsNullOrEmpty(link.Rel)) {
                    sb
                        .Append(@" rel=""")
                        .Append(html.AttributeEncode(link.Rel))
                        .Append(@"""");
                }

                if (!string.IsNullOrEmpty(link.Type)) {
                    sb
                        .Append(@" type=""")
                        .Append(html.AttributeEncode(link.Type))
                        .Append(@"""");
                }

                if (!string.IsNullOrEmpty(link.Href)) {
                    sb
                        .Append(@" href=""")
                        .Append(@"http://localhost:30320")
                        .Append(html.AttributeEncode(link.Href))
                        .Append(@"""");
                }

                sb.Append(@" />");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public MvcHtmlString GetHeadScripts() {
            return GetFiles(_headScripts, ScriptFormat, "/scripts/");
        }

        public MvcHtmlString GetFootScripts() {
            return GetFiles(_footScripts, ScriptFormat, "/scripts/");
        }

        private static MvcHtmlString GetFiles(IEnumerable<FileRegistrationContext> fileRegistrationContexts, string fileFormat, string containerRelativePath) {
            return
                MvcHtmlString.Create(string.Join("\r\n",
                                                 fileRegistrationContexts.Select(c => string.Format(fileFormat, c.GetFilePath(containerRelativePath))).ToArray()));
        }
    }
}