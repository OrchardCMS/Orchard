using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class jQuery : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            // jQuery.
            manifest.DefineScript("jQuery").SetUrl("jquery.min.js", "jquery.js").SetVersion("2.1.4").SetCdn("//ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.4.min.js", "//ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.4.js", true);

            // jQuery UI (full package).
            manifest.DefineScript("jQueryUI").SetUrl("jquery-ui.min.js", "jquery-ui.js").SetVersion("1.11.4").SetDependencies("jQuery").SetCdn("//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.4/jquery-ui.min.js", "//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.4/jquery-ui.js", true);
            manifest.DefineStyle("jQueryUI").SetUrl("jquery-ui.min.css", "jquery-ui.css").SetVersion("1.11.4");
            manifest.DefineStyle("jQueryUI_Orchard").SetDependencies("jQueryUI"); // Right now no customization in the styles, but the resource might be used later.

            // DEPRECATED for 1.10: Resources for jQuery UI individual components. This now only
            // defer to the full jQueryUI resources. In some cases where modules depend on these,
            // they will now get the full package instead of just the expected individual resource,
            // which could be considered breaking, but was deemed acceptable in the weekly
            // meeting 2016-02-09.
            manifest.DefineScript("jQueryUI_Core").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Widget").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Mouse").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Position").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Draggable").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Droppable").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Resizable").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Selectable").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Sortable").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Accordion").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Autocomplete").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Button").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Dialog").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Slider").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Tabs").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_DatePicker").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Progressbar").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_SelectMenu").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Spinner").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Tooltip").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Menu").SetDependencies("jQueryUI");
            manifest.DefineStyle("jQueryUI_Structure").SetDependencies("jQueryUI");
            manifest.DefineStyle("jQueryUI_Theme").SetDependencies("jQueryUI");

            // Additional utilities and plugins.
            manifest.DefineScript("jQueryUtils").SetUrl("jquery.utils.min.js", "jquery.utils.js").SetDependencies("jQuery");
            manifest.DefineScript("jQueryPlugin").SetUrl("jquery.plugin.min.js", "jquery.plugin.js").SetDependencies("jQuery");

            // jQuery Calendars.
            manifest.DefineScript("jQueryCalendars").SetUrl("Calendars/jquery.calendars.all.min.js", "Calendars/jquery.calendars.all.js").SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineScript("jQueryCalendars_Picker").SetUrl("Calendars/jquery.calendars.picker.full.min.js", "Calendars/jquery.calendars.picker.full.js").SetDependencies("jQueryCalendars").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryCalendars_Picker").SetUrl("Calendars/jquery.calendars.picker.full.min.css", "Calendars/jquery.calendars.picker.full.css").SetDependencies("jQueryUI_Orchard").SetVersion("2.0.1");

            // jQuery Time Entry.
            manifest.DefineScript("jQueryTimeEntry").SetUrl("TimeEntry/jquery.timeentry.min.js", "TimeEntry/jquery.timeentry.js").SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryTimeEntry").SetUrl("TimeEntry/jquery.timeentry.min.css","TimeEntry/jquery.timeentry.css").SetVersion("2.0.1");

            // jQuery Date/Time Editor Enhancements.
            manifest.DefineStyle("jQueryDateTimeEditor").SetUrl("jquery-datetime-editor.min.css","jquery-datetime-editor.css").SetDependencies("DateTimeEditor");

            // jQuery File Upload.
            manifest.DefineScript("jQueryFileUpload").SetUrl("jquery.fileupload-full.min.js", "jquery.fileupload-full.js").SetVersion("9.11.2").SetDependencies("jQueryUI_Widget");

            // jQuery Color Box.
            manifest.DefineScript("jQueryColorBox").SetUrl("jquery.colorbox.min.js", "jquery.colorbox.js").SetVersion("1.6.3").SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox").SetUrl("jquery.colorbox.min.css", "jquery.colorbox.css").SetVersion("1.6.3");

            // jQuery Cookie.
            manifest.DefineScript("jQueryCookie").SetUrl("jquery.cookie.min.js", "jquery.cookie.js").SetVersion("1.4.1").SetDependencies("jQuery");
        }
    }
}
