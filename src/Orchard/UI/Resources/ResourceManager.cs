using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.UI.Resources {
    [UsedImplicitly]
    public class ResourceManager : IResourceManager {
        private const string ConditionFormat = "\r\n<!--[{0}]>{{0}}<![endif]-->";
        private const string MetaFormat = "\r\n<meta name=\"{0}\" content=\"{1}\" />";
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
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void RegisterMeta(string name, string content) {
            if (!string.IsNullOrEmpty(name) && !_metas.ContainsKey(name))
                _metas.Add(name, content);
        }

        public FileRegistrationContext RegisterStyle(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(T("Style fileName was not given.").ToString());

            var context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, "link", fileName);
            context.SetAttribute("type", "text/css");
            context.SetAttribute("rel", "stylesheet");

            if (!_styles.Contains(context))
                _styles.Add(context);

            return context;
        }

        public void RegisterLink(LinkEntry entry, HtmlHelper html) {
            _links.Add(entry);
        }

        public FileRegistrationContext RegisterHeadScript(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(T("Head script fileName was not given.").ToString());

            var context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, "script", fileName);
            context.SetAttribute("type", "text/javascript");

            if (!_headScripts.Contains(context))
                _headScripts.Add(context);

            return context;
        }

        public FileRegistrationContext RegisterFootScript(string fileName, HtmlHelper html) { // type=\"text/javascript\" src=\"{0}\"
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(T("Foot script fileName was not given.").ToString());

            var context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, "script", fileName);
            context.SetAttribute("type", "text/javascript");

            if (!_footScripts.Contains(context))
                _footScripts.Add(context);

            return context;
        }

        public MvcHtmlString GetMetas() {
            return
                MvcHtmlString.Create(string.Join("",
                                                 _metas.Select(m => string.Format(MetaFormat, m.Key, m.Value)).Reverse().ToArray()));
        }

        public MvcHtmlString GetStyles() {
            return GetFiles(_styles, "/styles/");
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

                if (!string.IsNullOrEmpty(link.Title)) {
                    sb
                        .Append(@" title=""")
                        .Append(html.AttributeEncode(link.Title))
                        .Append(@"""");
                }

                if (!string.IsNullOrEmpty(link.Href)) {
                    sb
                        .Append(@" href=""")
                        .Append(html.AttributeEncode(link.Href))
                        .Append(@"""");
                }

                sb.Append(@" />");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        public MvcHtmlString GetHeadScripts() {
            return GetFiles(_headScripts, "/scripts/");
        }

        public MvcHtmlString GetFootScripts() {
            return GetFiles(_footScripts, "/scripts/");
        }

        private static MvcHtmlString GetFiles(IEnumerable<FileRegistrationContext> fileRegistrationContexts, string containerRelativePath) {
            return
                MvcHtmlString.Create(string.Join("",
                                                 fileRegistrationContexts.Select(
                                                     c =>
                                                     string.Format(
                                                         !string.IsNullOrEmpty(c.Condition)
                                                             ? string.Format(ConditionFormat, c.Condition)
                                                             : "{0}",
                                                         GetTag(c, c.GetFilePath(containerRelativePath)))).
                                                     ToArray()));
        }

        private static string GetTag(FileRegistrationContext fileRegistrationContext, string filePath) {
            fileRegistrationContext.SetAttribute(fileRegistrationContext.FilePathAttributeName, filePath);
            return fileRegistrationContext.GetTag();
        }
    }
}