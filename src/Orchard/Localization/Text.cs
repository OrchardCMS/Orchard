using System.Web;
using Orchard.Logging;

namespace Orchard.Localization {
    public class Text : IText {
        private readonly string _scope;
        private readonly ICultureManager _cultureManager;
        private readonly IResourceManager _resourceManager;

        public Text(string scope, ICultureManager cultureManager, IResourceManager resourceManager) {
            _scope = scope;
            _cultureManager = cultureManager;
            _resourceManager = resourceManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public LocalizedString Get(string textHint, params object[] args) {
            Logger.Debug("{0} localizing '{1}'", _scope, textHint);

            string currentCulture = _cultureManager.GetCurrentCulture(HttpContext.Current);
            var localizedFormat = _resourceManager.GetLocalizedString(textHint, currentCulture);

            return args.Length < 1
                ? new LocalizedString(localizedFormat)
                : new LocalizedString(string.Format(localizedFormat, args));
        }
    }
}