using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Orchard.Localization.Services;

namespace Orchard.Tests.Localization {

    [TestFixture]
    public class LocalizationStreamParserTests {

        [Test]
        public void ShouldTrimLeadingQuotes() {
            var parser = new LocalizationStreamParser();

            var text = new StringBuilder();
            text.AppendLine("#: ~/Themes/MyTheme/Views/MyView.cshtml");
            text.AppendLine("msgctxt \"~/Themes/MyTheme/Views/MyView.cshtml\"");
            text.AppendLine("msgid \"\\\"{0}\\\" Foo\"");
            text.AppendLine("msgstr \"\\\"{0}\\\" Foo\"");

            var translations = new Dictionary<string, string>();
            parser.ParseLocalizationStream(text.ToString(), translations, false);

            Assert.AreEqual("\"{0}\" Foo", translations["~/themes/mytheme/views/myview.cshtml|\"{0}\" foo"]);
        }

        [Test]
        public void ShouldTrimTrailingQuotes() {
            var parser = new LocalizationStreamParser();

            var text = new StringBuilder();
            text.AppendLine("#: ~/Themes/MyTheme/Views/MyView.cshtml");
            text.AppendLine("msgctxt \"~/Themes/MyTheme/Views/MyView.cshtml\"");
            text.AppendLine("msgid \"Foo \\\"{0}\\\"\"");
            text.AppendLine("msgstr \"Foo \\\"{0}\\\"\"");

            var translations = new Dictionary<string, string>();
            parser.ParseLocalizationStream(text.ToString(), translations, false);

            Assert.AreEqual("Foo \"{0}\"", translations["~/themes/mytheme/views/myview.cshtml|foo \"{0}\""]);
        }
    }
}