using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class jQueryUI : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            // jQuery UI Core.
            manifest.DefineScript("jQueryUI")
                .SetUrl("jQueryUI/jquery-ui.min.js", "jQueryUI/jquery-ui.js")
                .SetVersion("1.14.1")
                .SetDependencies("jQuery")
                .SetCdn(
                    "//code.jquery.com/ui/1.14.1/jquery-ui.min.js",
                    "//code.jquery.com/ui/1.14.1/jquery-ui.js");

            manifest.DefineStyle("jQueryUI")
                .SetUrl("jQueryUI/jquery-ui.min.css", "jQueryUI/jquery-ui.css")
                .SetVersion("1.14.1");

            // jQuery UI Structure.
            manifest.DefineStyle("jQueryUI.Structure")
                .SetUrl("jQueryUI/jquery-ui.structure.min.css", "jQueryUI/jquery-ui.structure.css")
                .SetVersion("1.14.1");

            // jQuery UI Theme.
            manifest.DefineStyle("jQueryUI.Theme")
                .SetUrl("jQueryUI/jquery-ui.theme.min.css", "jQueryUI/jquery-ui.theme.css")
                .SetVersion("1.14.1");

            // jQuery UI Full (Core + Structure + Theme).
            manifest.DefineStyle("jQueryUI_Full")
                .SetDependencies("jQueryUI", "jQueryUI.Structure", "jQueryUI.Theme");

            // Right now no customization in the styles, but the resource might be used later.
            manifest.DefineStyle("jQueryUI_Orchard").SetDependencies("jQueryUI_Full");

            // jQuery Time Entry.
            manifest.DefineScript("jQueryTimeEntry")
                .SetUrl("TimeEntry/jquery.timeentry.min.js", "TimeEntry/jquery.timeentry.js")
                .SetDependencies("jQueryPlugin")
                .SetVersion("2.0.1");
            manifest.DefineStyle("jQueryTimeEntry")
                .SetUrl("TimeEntry/jquery.timeentry.min.css", "TimeEntry/jquery.timeentry.css")
                .SetVersion("2.0.1");

            // jQuery Date/Time Editor Enhancements.
            manifest.DefineStyle("jQueryDateTimeEditor")
                .SetUrl("jquery-datetime-editor.min.css", "jquery-datetime-editor.css")
                .SetDependencies("DateTimeEditor");

            // jQuery Color Box.
            manifest.DefineScript("jQueryColorBox")
                .SetUrl("jQuery.Colorbox/jquery.colorbox.min.js", "jQuery.Colorbox/jquery.colorbox.js")
                .SetVersion("1.6.3")
                .SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox")
                .SetUrl("jQuery.Colorbox/jquery.colorbox.min.css", "jQuery.Colorbox/jquery.colorbox.css")
                .SetVersion("1.6.3");
        }
    }
}
