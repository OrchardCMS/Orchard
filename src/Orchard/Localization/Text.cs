using System;
using System.Globalization;
using Orchard.Localization.Services;
using Orchard.Logging;
using System.Web;
using System.Linq;

namespace Orchard.Localization {
    public class Text : IText {
        private readonly string _scope;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ILocalizedStringManager _localizedStringManager;

        public Text(string scope, IWorkContextAccessor workContextAccessor, ILocalizedStringManager localizedStringManager) {
            _scope = scope;
            _workContextAccessor = workContextAccessor;
            _localizedStringManager = localizedStringManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public LocalizedString Get(string textHint, params object[] args) {
            Logger.Debug("{0} localizing '{1}'", _scope, textHint);

            var workContext = _workContextAccessor.GetContext();
            
            if (workContext != null) {
                var currentCulture = workContext.CurrentCulture;
                var localizedFormat = _localizedStringManager.GetLocalizedString(_scope, textHint, currentCulture);

                // localization arguments are HTML-encoded unless they implement IHtmlString

                return args.Length == 0
                ? new LocalizedString(localizedFormat, _scope, textHint, args)
				: new LocalizedString(
                    String.Format(GetFormatProvider(currentCulture), localizedFormat, args.Select(Encode).ToArray()), 
                    _scope, 
                    textHint, 
                    args);
            }

            return new LocalizedString(textHint, _scope, textHint, args);
        }

        private static IFormatProvider GetFormatProvider(string currentCulture) {
            try {
                return CultureInfo.GetCultureInfo(currentCulture);
            }
            catch {
                return null;
            }
        }

        static object Encode(object arg)
        {
            if (arg is IFormattable || arg is IHtmlString) {
                return arg;
            }

            return HttpUtility.HtmlEncode(arg);
        }
    }
}