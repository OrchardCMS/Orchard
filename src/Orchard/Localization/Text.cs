using Orchard.Logging;

namespace Orchard.Localization {
    public class Text : IText {
        private readonly string _scope;

        public Text(string scope) {
            _scope = scope;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public LocalizedString Get(string textHint, params object[] args) {
            Logger.Debug("{0} localizing '{1}'", _scope, textHint);

            //todo: actually localize the textHint
            var localizedFormat = textHint;

            return args.Length < 1
                ? new LocalizedString(localizedFormat)
                : new LocalizedString(string.Format(localizedFormat, args));
        }
    }
}