using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Extensions;
using Orchard.Mvc.Html;
using Orchard.Themes;

namespace Orchard.UI.Resources {
    public class ResourceManager : IResourceManager {
        private readonly IThemeService _themeService;
        private readonly IExtensionManager _extensionManager;
        private const string MetaFormat = "\r\n<meta name=\"{0}\" content=\"{1}\" />";
        private const string StyleFormat = "\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
        private const string ScriptFormat = "\r\n<script type=\"text/javascript\" src=\"{0}\"></script>";
        private readonly Dictionary<string, string> _metas;
        private readonly List<FileRegistrationContext> _styles;
        private readonly List<FileRegistrationContext> _headScripts;
        private readonly List<FileRegistrationContext> _footScripts;

        public ResourceManager(IThemeService themeService,
            IExtensionManager extensionManager) {
            _themeService = themeService;
            _extensionManager = extensionManager;
            //TODO: (erikpo) Not sure if generator should be initialized here or somewhere else
            _metas = new Dictionary<string, string>(20) {{"generator", "Orchard"}};
            _styles = new List<FileRegistrationContext>(10);
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

            FileRegistrationContext context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, fileName);

            if (!_styles.Contains(context))
                _styles.Add(context);
        }

        public void RegisterHeadScript(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                return;

            FileRegistrationContext context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, fileName);

            if (!_headScripts.Contains(context))
                _headScripts.Add(context);
        }

        public void RegisterFootScript(string fileName, HtmlHelper html) {
            if (string.IsNullOrEmpty(fileName))
                return;

            FileRegistrationContext context = new FileRegistrationContext(html.ViewContext, html.ViewDataContainer, fileName);

            if (!_footScripts.Contains(context))
                _footScripts.Add(context);
        }

        public MvcHtmlString GetMetas() {
            return
                MvcHtmlString.Create(string.Join("\r\n",
                                                 _metas.Select(m => string.Format(MetaFormat, m.Key, m.Value)).ToArray()));
        }

        public MvcHtmlString GetStyles() {
            return GetFiles(_styles, StyleFormat, "/styles/");
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

        //TODO: (erikpo) Old code that may be needed later
        //private string GetThemePath(string fileName, RequestContext requestContext) {
        //    var requestTheme = _themeService.GetRequestTheme(requestContext); // <- todo: (erikpo) will need context eventually

        //    if (requestTheme == null)
        //        return fileName;
            
        //    //todo: (erikpo) this might be the worst code ever so resolve for real
        //    return  (_extensionManager.GetThemeLocation(requestTheme) + fileName)
        //        .Replace("~", "")
        //        .Replace("\\", "/");
        //}
    }
}