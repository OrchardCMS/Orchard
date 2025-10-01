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

            // Additional utilities and plugins.
            manifest.DefineScript("jQueryUtils")
                .SetUrl("jquery.utils.min.js", "jquery.utils.js")
                .SetDependencies("jQuery");
            manifest.DefineScript("jQueryPlugin")
                .SetUrl("jquery.plugin.min.js", "jquery.plugin.js")
                .SetDependencies("jQuery");
            manifest.DefineScript("jQueryCookie") // jQuery Cookie.
                .SetUrl("jquery.cookie.min.js", "jquery.cookie.js")
                .SetVersion("1.4.1")
                .SetDependencies("jQuery");

            // jQuery Calendars.
            manifest.DefineScript("jQueryCalendars")
                .SetUrl("Calendars/jquery.calendars.all.min.js", "Calendars/jquery.calendars.all.js")
                .SetDependencies("jQueryPlugin")
                .SetVersion("2.0.1");

            manifest.DefineScript("jQueryCalendars_Picker")
                .SetUrl("Calendars/jquery.calendars.picker.full.min.js", "Calendars/jquery.calendars.picker.full.js")
                .SetDependencies("jQueryCalendars")
                .SetVersion("2.0.1");
            manifest.DefineStyle("jQueryCalendars_Picker")
                .SetUrl("Calendars/jquery.calendars.picker.full.min.css", "Calendars/jquery.calendars.picker.full.css")
                .SetDependencies("jQueryUI_Orchard")
                .SetVersion("2.0.1");

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

            // jQuery File Upload.
            manifest.DefineScript("jQueryFileUpload")
                .SetUrl("jquery.fileupload-full.min.js", "jquery.fileupload-full.js")
                .SetVersion("9.11.2")
                .SetDependencies("jQueryUI");

            // jQuery Color Box.
            manifest.DefineScript("jQueryColorBox")
                .SetUrl("jquery.colorbox.min.js", "jquery.colorbox.js")
                .SetVersion("1.6.3")
                .SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox")
                .SetUrl("jquery.colorbox.min.css", "jquery.colorbox.css")
                .SetVersion("1.6.3");
        }
    }
}
