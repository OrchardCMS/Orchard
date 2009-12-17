using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Extensions;
using Orchard.Themes;

namespace Orchard.UI.Resources {
    public class ResourceManager : IResourceManager {
        private readonly IThemeService _themeService;
        private readonly IExtensionManager _extensionManager;
        private const string MetaFormat = "\r\n<meta name=\"{0}\" content=\"{1}\" />";
        private const string StyleFormat = "\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
        private const string ScriptFormat = "\r\n<script type=\"text/javascript\" src=\"{0}\"></script>";
        private readonly Dictionary<string, string> _metas;
        private readonly List<string> _styles;
        private readonly List<string> _headScripts;
        private readonly List<string> _footScripts;

        public ResourceManager(IThemeService themeService,
            IExtensionManager extensionManager) {
            _themeService = themeService;
            _extensionManager = extensionManager;
            //TODO: (erikpo) Not sure if generator should be initialized here or somewhere else
            _metas = new Dictionary<string, string>(20) {{"generator", "Orchard"}};
            _styles = new List<string>(10);
            _headScripts = new List<string>(10);
            _footScripts = new List<string>(5);
        }

        public void RegisterMeta(string name, string content) {
            if (!string.IsNullOrEmpty(name) && !_metas.ContainsKey(name))
                _metas.Add(name, content);
        }

        public void RegisterStyle(string fileName) {
            if (!string.IsNullOrEmpty(fileName) && !_styles.Contains(fileName))
                _styles.Add(fileName);
        }

        public void RegisterHeadScript(string fileName) {
            if (!string.IsNullOrEmpty(fileName) && !_headScripts.Contains(fileName) && !_footScripts.Contains(fileName))
                _headScripts.Add(fileName);
        }

        public void RegisterFootScript(string fileName) {
            if (!string.IsNullOrEmpty(fileName) && !_headScripts.Contains(fileName) && !_footScripts.Contains(fileName))
                _footScripts.Add(fileName);
        }

        public MvcHtmlString GetMetas() {
            return
                MvcHtmlString.Create(string.Join("\r\n",
                                                 _metas.Select(m => string.Format(MetaFormat, m.Key, m.Value)).ToArray()));
        }

        public MvcHtmlString GetStyles() {
            return GetFiles(_styles, StyleFormat, s => GetThemePath("/styles/" + s));
        }

        public MvcHtmlString GetHeadScripts() {
            return GetFiles(_headScripts, ScriptFormat, s => GetThemePath("/scripts/" + s));
        }

        public MvcHtmlString GetFootScripts() {
            return GetFiles(_footScripts, ScriptFormat, s => GetThemePath("/scripts/" + s));
        }

        private static MvcHtmlString GetFiles(IEnumerable<string> files, string fileFormat, Func<string, string> getPath) {
            return
                MvcHtmlString.Create(string.Join("\r\n",
                                                 files.Select(s => string.Format(fileFormat, getPath(s))).ToArray()));
        }

        private string GetThemePath(string fileName) {
            var requestTheme = _themeService.GetRequestTheme(null); // <- todo: (erikpo) will need context eventually

            if (requestTheme == null)
                return fileName;
            
            //todo: (erikpo) this might be the worst code ever so resolve for real
            return  (_extensionManager.GetThemeLocation(requestTheme) + fileName)
                .Replace("~", "")
                .Replace("\\", "/");
        }
    }
}