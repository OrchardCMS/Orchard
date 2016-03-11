using Orchard.UI.Resources;

namespace Orchard.jQuery {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery").SetUrl("jquery-1.11.1.min.js", "jquery-1.11.1.js").SetVersion("1.11.1").SetCdn("//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.11.1.min.js", "//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.11.1.js", true);
            manifest.DefineScript("jQueryMigrate").SetUrl("jquery-migrate-1.2.1.min.js", "jquery-migrate-1.2.1.js").SetVersion("1.2.1").SetCdn("//ajax.aspnetcdn.com/ajax/jquery.migrate/jquery-migrate-1.2.1.min.js", "//ajax.aspnetcdn.com/ajax/jquery.migrate/jquery-migrate-1.2.1.js", true);

            // Full jQuery UI package.
            manifest.DefineScript("jQueryUI").SetUrl("ui/jquery-ui.min.js", "ui/jquery-ui.js").SetVersion("1.11.2").SetDependencies("jQuery").SetCdn("//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.2/jquery-ui.min.js", "//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.2/jquery-ui.js", true);
            manifest.DefineStyle("jQueryUI").SetUrl("jquery-ui.min.css", "jquery-ui.css").SetVersion("1.11.2");
            manifest.DefineStyle("jQueryUI_Structure").SetUrl("jquery-ui.structure.min.css", "jquery-ui.structure.css").SetVersion("1.11.2");
            manifest.DefineStyle("jQueryUI_Theme").SetUrl("jquery-ui.theme.min.css", "jquery-ui.theme.css").SetVersion("1.11.2");
            manifest.DefineStyle("jQueryUI_Orchard").SetVersion("1.11.2").SetDependencies("jQueryUI", "jQueryUI_Structure", "jQueryUI_Theme");

            // Individual jQuery UI components.
            manifest.DefineScript("jQueryUI_Core").SetUrl("ui/core.min.js", "ui/core.js").SetVersion("1.11.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Widget").SetUrl("ui/widget.min.js", "ui/widget.js").SetVersion("1.11.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Mouse").SetUrl("ui/mouse.min.js", "ui/mouse.js").SetVersion("1.11.2").SetDependencies("jQuery", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Position").SetUrl("ui/position.min.js", "ui/position.js").SetVersion("1.11.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Draggable").SetUrl("ui/draggable.min.js", "ui/draggable.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Droppable").SetUrl("ui/droppable.min.js", "ui/droppable.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse", "jQueryUI_Draggable");
            manifest.DefineScript("jQueryUI_Resizable").SetUrl("ui/resizable.min.js", "ui/resizable.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Selectable").SetUrl("ui/selectable.min.js", "ui/selectable.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Sortable").SetUrl("ui/sortable.min.js", "ui/sortable.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Accordion").SetUrl("ui/accordion.min.js", "ui/accordion.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Autocomplete").SetUrl("ui/autocomplete.min.js", "ui/autocomplete.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position", "jQueryUI_Menu");
            manifest.DefineScript("jQueryUI_Button").SetUrl("ui/button.min.js", "ui/button.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Dialog").SetUrl("ui/dialog.min.js", "ui/dialog.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position", "jQueryUI_Mouse", "jQueryUI_Draggable", "jQueryUI_Resizable", "jQueryUI_Button");
            manifest.DefineScript("jQueryUI_Slider").SetUrl("ui/slider.min.js", "ui/slider.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Tabs").SetUrl("ui/tabs.min.js", "ui/tabs.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_DatePicker").SetUrl("ui/datepicker.min.js", "ui/datepicker.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("jQueryUI_Progressbar").SetUrl("ui/progressbar.min.js", "ui/progressbar.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_SelectMenu").SetUrl("ui/selectmenu.min.js", "ui/selectmenu.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position", "jQueryUI_Menu");
            manifest.DefineScript("jQueryUI_Spinner").SetUrl("ui/spinner.min.js", "ui/spinner.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Button");
            manifest.DefineScript("jQueryUI_Tooltip").SetUrl("ui/tooltip.min.js", "ui/tooltip.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position");
            manifest.DefineScript("jQueryUI_Menu").SetUrl("ui/menu.min.js", "ui/menu.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position");
            manifest.DefineScript("jQueryEffects_Core").SetUrl("ui/effect.min.js", "ui/effect.js").SetVersion("1.11.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryEffects_Blind").SetUrl("ui/effect-blind.min.js", "ui/effect-blind.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Bounce").SetUrl("ui/effect-bounce.min.js", "ui/effect-bounce.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Clip").SetUrl("ui/effect-clip.min.js", "ui/effect-clip.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Drop").SetUrl("ui/effect-drop.min.js", "ui/effect-drop.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Explode").SetUrl("ui/effect-explode.min.js", "ui/effect-explode.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Fade").SetUrl("ui/effect-fade.min.js", "ui/effect-fade.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Fold").SetUrl("ui/effect-fold.min.js", "ui/effect-fold.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Highlight").SetUrl("ui/effect-highlight.min.js", "ui/effect-highlight.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Puff").SetUrl("ui/effect-puff.min.js", "ui/effect-puff.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Pulsate").SetUrl("ui/effect-pulsate.min.js", "ui/effect-pulsate.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Scale").SetUrl("ui/effect-scale.min.js", "ui/effect-scale.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Shake").SetUrl("ui/effect-shake.min.js", "ui/effect-shake.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Size").SetUrl("ui/effect-size.min.js", "ui/effect-size.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Slide").SetUrl("ui/effect-slide.min.js", "ui/effect-slide.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Transfer").SetUrl("ui/effect-transfer.min.js", "ui/effect-transfer.js").SetVersion("1.11.2").SetDependencies("jQueryEffects_Core");

            // Additional utilities and plugins.
            manifest.DefineScript("jQueryUtils").SetUrl("jquery.utils.js").SetDependencies("jQuery");
            manifest.DefineScript("jQueryPlugin").SetUrl("jquery.plugin.min.js", "jquery.plugin.js").SetDependencies("jQuery");

            // jQuery Calendars.
            manifest.DefineScript("jQueryCalendars_All").SetUrl("calendars/jquery.calendars.all.min.js", "calendars/jquery.calendars.all.js").SetDependencies("jQueryPlugin").SetVersion("2.0.0");
            manifest.DefineScript("jQueryCalendars_Picker_Ext").SetUrl("calendars/jquery.calendars.picker.ext.min.js", "calendars/jquery.calendars.picker.ext.js").SetDependencies("jQueryCalendars_Picker").SetVersion("2.0.0");
            manifest.DefineStyle("jQueryCalendars_Picker").SetUrl("jquery.calendars.picker.css").SetVersion("2.0.0");
            manifest.DefineStyle("jQueryUI_Calendars_Picker").SetUrl("ui.calendars.picker.css").SetDependencies("jQueryUI_Orchard").SetVersion("2.0.0");

            // jQuery Time Entry.
            manifest.DefineScript("jQueryTimeEntry").SetUrl("timeentry/jquery.timeentry.min.js", "timeentry/jquery.timeentry.js").SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryTimeEntry").SetUrl("jquery.timeentry.css").SetVersion("2.0.1");

            // jQuery Date/Time Editor Enhancements.
            manifest.DefineStyle("jQueryDateTimeEditor").SetUrl("jquery-datetime-editor.css").SetDependencies("DateTimeEditor");

            // jQuery File Upload.
            manifest.DefineScript("jQueryIFrameTransport").SetUrl("jquery.iframe-transport.min.js", "jquery.iframe-transport.js").SetVersion("1.9.0").SetDependencies("jQuery");
            manifest.DefineScript("jQueryFileUpload").SetUrl("jquery.fileupload.min.js", "jquery.fileupload.js").SetVersion("5.41.0").SetDependencies("jQueryIFrameTransport").SetDependencies("jQueryUI_Widget");

            // jQuery Color Box.
            manifest.DefineScript("jQueryColorBox").SetUrl("colorbox/jquery.colorbox.min.js", "colorbox/jquery.colorbox.js").SetVersion("1.5.13").SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox").SetUrl("colorbox.css").SetVersion("1.5.13");

            // jQuery Cookie.
            manifest.DefineScript("jQueryCookie").SetUrl("jquery.cookie.min.js", "jquery.cookie.js").SetVersion("1.4.1").SetDependencies("jQuery");
        }
    }
}
