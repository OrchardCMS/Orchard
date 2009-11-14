using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Localization {
    public interface IText : IDependency {
        LocalizedString Get(string textHint, params object[] args);
    }


    public class Text : IText {
        public LocalizedString Get(string textHint, params object[] args) {
            var localizedFormat = textHint;
            var localizedText = string.Format(localizedFormat, args);
            return new LocalizedString(localizedText);
        }
    }
}
