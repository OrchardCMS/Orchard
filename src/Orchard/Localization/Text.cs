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
        private readonly ILocalizedStringManager _localizedStringManager;

        public Text(string scope, ICultureManager cultureManager, ILocalizedStringManager localizedStringManager) {
            _scope = scope;
            _cultureManager = cultureManager;
            _localizedStringManager = localizedStringManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public LocalizedString Get(string textHint, params object[] args) {
            Logger.Debug("{0} localizing '{1}'", _scope, textHint);

            string currentCulture = HttpContext.Current == null ? _cultureManager.GetSiteCulture() : _cultureManager.GetCurrentCulture(new HttpContextWrapper(HttpContext.Current));
            var localizedFormat = _localizedStringManager.GetLocalizedString(_scope, textHint, currentCulture);

            return args.Length == 0 
                ? new LocalizedString(localizedFormat, _scope, textHint, args)
                : new LocalizedString(string.Format(GetFormatProvider(currentCulture), localizedFormat, args.Select(Encode).ToArray()), _scope, textHint, args);
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