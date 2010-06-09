using System;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard.Localization.Services;
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
            var localizedFormat = _resourceManager.GetLocalizedString(_scope, textHint, currentCulture);

            return args.Length == 0 
                ? new LocalizedString(localizedFormat) 
                : string.Format(GetFormatProvider(currentCulture), localizedFormat, args.Select(Encode).ToArray());
        }

        private static IFormatProvider GetFormatProvider(string currentCulture) {
            try {
                return CultureInfo.GetCultureInfoByIetfLanguageTag(currentCulture);
            }
            catch {
                return null;
            }
        }

        static object Encode(object arg) {
            if (arg is IFormattable || arg is IHtmlString) {
                return arg;
            }
            return HttpUtility.HtmlEncode(arg);
        }
    }
}