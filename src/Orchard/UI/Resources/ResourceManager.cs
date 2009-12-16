using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.UI.Resources {
    public class ResourceManager : IResourceManager {
        private const string StyleFormat = "\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />";
        private const string ScriptFormat = "\r\n<script type=\"text/javascript\" src=\"{0}\"></script>";
        private readonly List<string> _styles;
        private readonly List<string> _headScripts;
        private readonly List<string> _footScripts;

        public ResourceManager() {
            _styles = new List<string>();
            _headScripts = new List<string>();
            _footScripts = new List<string>();
        }

        public void RegisterStyle(string fileName) {
            //TODO: (erikpo) Figure out best storage here
            if (!string.IsNullOrEmpty(fileName) && !_styles.Contains(fileName))
                _styles.Add(fileName);
        }

        public void RegisterHeadScript(string fileName) {
            //TODO: (erikpo) Figure out best storage here
            if (!string.IsNullOrEmpty(fileName) && !_headScripts.Contains(fileName) && !_footScripts.Contains(fileName))
                _headScripts.Add(fileName);
        }

        public void RegisterFootScript(string fileName) {
            //TODO: (erikpo) Figure out best storage here
            if (!string.IsNullOrEmpty(fileName) && !_headScripts.Contains(fileName) && !_footScripts.Contains(fileName))
                _footScripts.Add(fileName);
        }

        public MvcHtmlString GetStyles() {
            return GetFiles(_styles, StyleFormat);
        }

        public MvcHtmlString GetHeadScripts() {
            return GetFiles(_headScripts, ScriptFormat);
        }

        public MvcHtmlString GetFootScripts() {
            return GetFiles(_footScripts, ScriptFormat);
        }

        private static MvcHtmlString GetFiles(IEnumerable<string> files, string fileFormat) {
            return
                MvcHtmlString.Create(string.Join("\r\n",
                                                 files.Select(s => string.Format(fileFormat, s)).ToArray()));
        }
    }
}