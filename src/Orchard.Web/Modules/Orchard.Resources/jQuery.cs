using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class jQuery : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery").SetUrl("jquery.min.js", "jquery.js").SetVersion("2.1.4").SetCdn("//ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.4.min.js", "//ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.4.js", true);

            // Full jQuery UI package.
            manifest.DefineScript("jQueryUI").SetUrl("jquery-ui.min.js", "jquery-ui.js").SetVersion("1.11.4").SetDependencies("jQuery").SetCdn("//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.4/jquery-ui.min.js", "//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.4/jquery-ui.js", true);
            manifest.DefineStyle("jQueryUI_Orchard").SetUrl("jqueryui-orchard.min.css", "jqueryui-orchard.css").SetVersion("1.11.4");
            manifest.DefineScript("jQueryUI_Interaction").SetUrl("jquery-ui-interaction.min.js", "jquery-ui-interaction.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Widgets").SetUrl("jquery-ui-widgets.min.js", "jquery-ui-widgets.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Interaction");
            manifest.DefineScript("jQueryUI_Effects").SetUrl("jquery-ui-effects.min.js", "jquery-ui-effects.js").SetVersion("1.11.4").SetDependencies("jQuery");

            // Individual jQuery UI components.
            manifest.DefineScript("jQueryUI_Mouse").SetUrl("ui/mouse.min.js", "ui/mouse.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Position").SetUrl("ui/position.min.js", "ui/position.js").SetVersion("1.11.4").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Draggable").SetUrl("ui/draggable.min.js", "ui/draggable.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Droppable").SetUrl("ui/droppable.min.js", "ui/droppable.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Draggable");
            manifest.DefineScript("jQueryUI_Resizable").SetUrl("ui/resizable.min.js", "ui/resizable.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Selectable").SetUrl("ui/selectable.min.js", "ui/selectable.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Sortable").SetUrl("ui/sortable.min.js", "ui/sortable.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Accordion").SetUrl("ui/accordion.min.js", "ui/accordion.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Autocomplete").SetUrl("ui/autocomplete.min.js", "ui/autocomplete.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Position", "jQueryUI_Menu");
            manifest.DefineScript("jQueryUI_Button").SetUrl("ui/button.min.js", "ui/button.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Dialog").SetUrl("ui/dialog.min.js", "ui/dialog.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Mouse", "jQueryUI_Draggable", "jQueryUI_Resizable", "jQueryUI_Button");
            manifest.DefineScript("jQueryUI_Slider").SetUrl("ui/slider.min.js", "ui/slider.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Tabs").SetUrl("ui/tabs.min.js", "ui/tabs.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_DatePicker").SetUrl("ui/datepicker.min.js", "ui/datepicker.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Progressbar").SetUrl("ui/progressbar.min.js", "ui/progressbar.js").SetVersion("1.11.4").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_SelectMenu").SetUrl("ui/selectmenu.min.js", "ui/selectmenu.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Position", "jQueryUI_Menu");
            manifest.DefineScript("jQueryUI_Spinner").SetUrl("ui/spinner.min.js", "ui/spinner.js").SetVersion("1.11.4").SetDependencies("jQueryUI_Button");
            manifest.DefineScript("jQueryUI_Tooltip").SetUrl("ui/tooltip.min.js", "ui/tooltip.js").SetVersion("1.11.4").SetDependencies("jQueryUI", "jQueryUI_Position");
            manifest.DefineScript("jQueryUI_Menu").SetUrl("ui/menu.min.js", "ui/menu.js").SetVersion("1.11.4").SetDependencies("jQueryUI", "jQueryUI_Position");

            // Additional utilities and plugins.
            manifest.DefineScript("jQueryUtils").SetUrl("jquery.utils.min.js", "jquery.utils.js").SetDependencies("jQuery");
            manifest.DefineScript("jQueryPlugin").SetUrl("jquery.plugin.min.js", "jquery.plugin.js").SetDependencies("jQuery");

            // jQuery Calendars.
            manifest.DefineScript("jQueryCalendars").SetUrl("jquery.calendars.all.min.js", "jquery.calendars.all.js").SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineScript("jQueryCalendars_Picker").SetUrl("jquery.calendars.picker.full.min.js", "jquery.calendars.picker.full.js").SetDependencies("jQueryCalendars").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryCalendars_Picker").SetUrl("jquery.calendars.picker.full.min.css", "jquery.calendars.picker.full.css").SetDependencies("jQueryUI_Orchard").SetVersion("2.0.1");

            // jQuery Time Entry.
            manifest.DefineScript("jQueryTimeEntry").SetUrl("jquery.timeentry.min.js", "jquery.timeentry.js").SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryTimeEntry").SetUrl("jquery.timeentry.css").SetVersion("2.0.1");

            // jQuery Date/Time Editor Enhancements.
            manifest.DefineStyle("jQueryDateTimeEditor").SetUrl("jquery-datetime-editor.css").SetDependencies("DateTimeEditor");

            // jQuery File Upload.
            manifest.DefineScript("jQueryFileUpload").SetUrl("jquery.fileupload-full.min.js", "jquery.fileupload-full.js").SetVersion("9.11.2").SetDependencies("jQueryUI_Widget");

            // jQuery Color Box.
            manifest.DefineScript("jQueryColorBox").SetUrl("jquery.colorbox.min.js", "jquery.colorbox.js").SetVersion("1.6.3").SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox").SetUrl("jquery.colorbox.min.css", "jquery.colorbox.min.css").SetVersion("1.6.3");

            // jQuery Cookie.
            manifest.DefineScript("jQueryCookie").SetUrl("jquery.cookie.min.js", "jquery.cookie.js").SetVersion("1.4.1").SetDependencies("jQuery");
        }
    }
}
