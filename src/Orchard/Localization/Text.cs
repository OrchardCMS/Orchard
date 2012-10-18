using System;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard.Localization.Services;
using Orchard.Logging;

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
            var currentCulture = workContext.CurrentCulture;
            var localizedFormat = _localizedStringManager.GetLocalizedString(_scope, textHint, currentCulture);

            return args.Length == 0 
                ? new LocalizedString(localizedFormat, _scope, textHint, args)
                : new LocalizedString(string.Format(GetFormatProvider(currentCulture), localizedFormat, args), _scope, textHint, args);
        }

        private static IFormatProvider GetFormatProvider(string currentCulture) {
            try {
                return CultureInfo.GetCultureInfoByIetfLanguageTag(currentCulture);
            }
            catch {
                return null;
            }
        }
    }
}