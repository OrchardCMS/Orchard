using System;
using System.Globalization;
using Orchard.Localization.Services;
using Orchard.Logging;
using System.Web;
using System.Linq;
using System.Collections.Generic;

namespace Orchard.Localization {
    public class Text : IText {
        private readonly IEnumerable<string> _scopes;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ILocalizedStringManager _localizedStringManager;

        //public Text(string scope, IWorkContextAccessor workContextAccessor, ILocalizedStringManager localizedStringManager) {
        //    _scope = scope;
        //    _workContextAccessor = workContextAccessor;
        //    _localizedStringManager = localizedStringManager;
        //    Logger = NullLogger.Instance;
        //}

        public Text(IEnumerable<string> scopes, IWorkContextAccessor workContextAccessor, ILocalizedStringManager localizedStringManager) {
            _scopes = scopes;
            _workContextAccessor = workContextAccessor;
            _localizedStringManager = localizedStringManager;
            Logger = NullLogger.Instance;
        }


        public ILogger Logger { get; set; }

        public LocalizedString Get(string textHint, params object[] args) {
            Logger.Debug("{0} localizing '{1}'", _scopes.FirstOrDefault(), textHint);
            string scope = null;
            var workContext = _workContextAccessor.GetContext();

            if (workContext != null) {
                var currentCulture = workContext.CurrentCulture;
                string localizedFormat;
                Tuple<string,string> localizedFormatScope = _localizedStringManager.GetLocalizedString(_scopes, textHint, currentCulture);
                scope = localizedFormatScope.Item2;
                // localization arguments are HTML-encoded unless they implement IHtmlString

                return args.Length == 0
                ? new LocalizedString(localizedFormatScope.Item1, scope, textHint, args)
				: new LocalizedString(
                    String.Format(GetFormatProvider(currentCulture), localizedFormatScope.Item1, args.Select(Encode).ToArray()),
                    scope, 
                    textHint, 
                    args);
            }

            return new LocalizedString(textHint, scope, textHint, args);
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