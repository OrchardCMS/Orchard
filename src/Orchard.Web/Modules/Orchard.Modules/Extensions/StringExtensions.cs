using System;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Modules.Extensions {
    public static class StringExtensions {
        public static string AsFeatureId(this string text, Func<string, LocalizedString> localize) {
            return string.IsNullOrEmpty(text)
                       ? ""
                       : string.Format(localize("{0} feature").ToString(), text).HtmlClassify();
        }
    }
}