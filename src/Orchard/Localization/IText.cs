using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Logging;

namespace Orchard.Localization {
    public interface IText  {
        LocalizedString Get(string textHint, params object[] args);
    }


    public class Text : IText {
        private readonly string _scope;

        public Text(string scope) {
            _scope = scope;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public LocalizedString Get(string textHint, params object[] args) {
            Logger.Debug("{0} localizing '{1}'", _scope, textHint);

            var localizedFormat = textHint;
            var localizedText = string.Format(localizedFormat, args);
            return new LocalizedString(localizedText);
        }
    }
}
