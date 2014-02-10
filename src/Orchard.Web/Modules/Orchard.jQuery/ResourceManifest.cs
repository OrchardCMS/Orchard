using Orchard.UI.Resources;

namespace Orchard.jQuery {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery").SetUrl("jquery-1.9.1.min.js", "jquery-1.9.1.js").SetVersion("1.9.1")
                .SetCdn("//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js", "//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.js", true);

            manifest.DefineScript("jQueryMigrate").SetUrl("jquery-migrate-1.1.1.min.js", "jquery-migrate-1.1.1.js").SetVersion("1.1.1")
                .SetCdn("//ajax.aspnetcdn.com/ajax/jquery.migrate/jquery-migrate-1.1.1.min.js", "//ajax.aspnetcdn.com/ajax/jquery.migrate/jquery-migrate-1.1.1.js", true);

            // Full jQuery UI bundle
            manifest.DefineScript("jQueryUI").SetUrl("jquery-ui.min.js", "jquery-ui.js").SetVersion("1.9.2").SetDependencies("jQuery")
                .SetCdn("//ajax.aspnetcdn.com/ajax/jquery.ui/1.9.2/jquery-ui.min.js", "//ajax.aspnetcdn.com/ajax/jquery.ui/1.9.2/jquery-ui.js", true);

            // UI Core
            manifest.DefineScript("jQueryUI_Core").SetUrl("jquery.ui.core.min.js", "jquery.ui.core.js").SetVersion("1.9.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Widget").SetUrl("jquery.ui.widget.min.js", "jquery.ui.widget.js").SetVersion("1.9.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Mouse").SetUrl("jquery.ui.mouse.min.js", "jquery.ui.mouse.js").SetVersion("1.9.2").SetDependencies("jQuery", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Position").SetUrl("jquery.ui.position.min.js", "jquery.ui.position.js").SetVersion("1.9.2").SetDependencies("jQuery");

            // Interactions
            manifest.DefineScript("jQueryUI_Draggable").SetUrl("jquery.ui.draggable.min.js", "jquery.ui.draggable.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Droppable").SetUrl("jquery.ui.droppable.min.js", "jquery.ui.droppable.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse", "jQueryUI_Draggable");
            manifest.DefineScript("jQueryUI_Resizable").SetUrl("jquery.ui.resizable.min.js", "jquery.ui.resizable.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Selectable").SetUrl("jquery.ui.selectable.min.js", "jquery.ui.selectable.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Sortable").SetUrl("jquery.ui.sortable.min.js", "jquery.ui.sortable.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");

            // Widgets
            manifest.DefineScript("jQueryUI_Accordion").SetUrl("jquery.ui.accordion.min.js", "jquery.ui.accordion.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Autocomplete").SetUrl("jquery.ui.autocomplete.min.js", "jquery.ui.autocomplete.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position", "jQueryUI_Menu");
            manifest.DefineScript("jQueryUI_Button").SetUrl("jquery.ui.button.min.js", "jquery.ui.button.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Dialog").SetUrl("jquery.ui.dialog.min.js", "jquery.ui.dialog.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position", "jQueryUI_Mouse", "jQueryUI_Draggable", "jQueryUI_Resizable", "jQueryUI_Button");
            manifest.DefineScript("jQueryUI_Slider").SetUrl("jquery.ui.slider.min.js", "jquery.ui.slider.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Tabs").SetUrl("jquery.ui.tabs.min.js", "jquery.ui.tabs.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_DatePicker").SetUrl("jquery.ui.datepicker.min.js", "jquery.ui.datepicker.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("jQueryUI_SliderAccess").SetUrl("jquery-ui-sliderAccess.js").SetVersion("0.2").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("jQueryUI_TimePicker").SetUrl("jquery-ui-timepicker-addon.js").SetVersion("1.0.5").SetDependencies("jQueryUI_Core", "jQueryUI_Slider", "jQueryUI_SliderAccess");
            manifest.DefineScript("jQueryUI_Progressbar").SetUrl("jquery.ui.progressbar.min.js", "jquery.ui.progressbar.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Spinner").SetUrl("jquery.ui.spinner.min.js", "jquery.ui.spinner.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Button");
            manifest.DefineScript("jQueryUI_Tooltip").SetUrl("jquery.ui.tooltip.min.js", "jquery.ui.tooltip.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position");

            // Effects
            manifest.DefineScript("jQueryEffects_Core").SetUrl("jquery.ui.effect.min.js", "jquery.ui.effect.js").SetVersion("1.9.2").SetDependencies("jQuery");
            manifest.DefineScript("jQueryEffects_Blind").SetUrl("jquery.ui.effect-blind.min.js", "jquery.ui.effect-blind.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Bounce").SetUrl("jquery.ui.effect-bounce.min.js", "jquery.ui.effect-bounce.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Clip").SetUrl("jquery.ui.effect-clip.min.js", "jquery.ui.effect-clip.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Drop").SetUrl("jquery.ui.effect-drop.min.js", "jquery.ui.effect-drop.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Explode").SetUrl("jquery.ui.effect-explode.min.js", "jquery.ui.effect-explode.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Fade").SetUrl("jquery.ui.effect-fade.min.js", "jquery.ui.effect-fade.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Fold").SetUrl("jquery.ui.effect-fold.min.js", "jquery.ui.effect-fold.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Highlight").SetUrl("jquery.ui.effect-highlight.min.js", "jquery.ui.effect-highlight.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Pulsate").SetUrl("jquery.ui.effect-pulsate.min.js", "jquery.ui.effect-pulsate.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Scale").SetUrl("jquery.ui.effect-scale.min.js", "jquery.ui.effect-scale.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Shake").SetUrl("jquery.ui.effect-shake.min.js", "jquery.ui.effect-shake.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Slide").SetUrl("jquery.ui.effect-slide.min.js", "jquery.ui.effect-slide.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Transfer").SetUrl("jquery.ui.effect-transfer.min.js", "jquery.ui.effect-transfer.js").SetVersion("1.9.2").SetDependencies("jQueryEffects_Core");

            // Menu
            manifest.DefineScript("jQueryUI_Menu").SetUrl("jquery.ui.menu.min.js", "jquery.ui.menu.js").SetVersion("1.9.2").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position");

            manifest.DefineScript("jQueryUtils").SetUrl("jquery.utils.js").SetDependencies("jQuery");

            manifest.DefineStyle("jQueryUI_Orchard").SetUrl("jquery-ui-1.9.2.custom.css").SetVersion("1.9.2");
            manifest.DefineStyle("jQueryUI_DatePicker").SetUrl("ui.datepicker.css").SetDependencies("jQueryUI_Orchard").SetVersion("1.7.2");
            manifest.DefineStyle("jQueryUI_TimePicker").SetUrl("jquery-ui-timepicker-addon.css").SetDependencies("jQueryUI_Orchard").SetVersion("1.0.5");

            // jQuery Calendars
            manifest.DefineScript("jQueryCalendars_All").SetUrl("calendars/jquery.calendars.all.min.js", "calendars/jquery.calendars.all.js").SetDependencies("jQuery").SetVersion("1.2.1");
            manifest.DefineScript("jQueryCalendars_Picker_Ext").SetUrl("calendars/jquery.calendars.picker.ext.min.js", "calendars/jquery.calendars.picker.ext.js").SetDependencies("jQueryCalendars_Picker").SetVersion("1.2.1");
            manifest.DefineStyle("jQueryCalendars_Picker").SetUrl("jquery.calendars.picker.css").SetVersion("1.2.1");
            manifest.DefineStyle("jQueryUI_Calendars_Picker").SetUrl("ui.calendars.picker.css").SetDependencies("jQueryUI_Orchard").SetVersion("1.2.1");

            // jQuery Time Entry
            manifest.DefineScript("jQueryTimeEntry").SetUrl("timeentry/jquery.timeentry.min.js", "timeentry/jquery.timeentry.js").SetDependencies("jQuery").SetVersion("1.5.2");
            manifest.DefineStyle("jQueryTimeEntry").SetUrl("jquery.timeentry.css").SetVersion("1.5.2");

            // jQuery Date/Time Editor Enhancements
            manifest.DefineScript("jQueryDateTimeEditor").SetUrl("jquery-datetime-editor.js").SetDependencies("jQuery");
            manifest.DefineStyle("jQueryDateTimeEditor").SetUrl("jquery-datetime-editor.css").SetDependencies("DateTimeEditor");

            // jQuery File Upload
            manifest.DefineScript("jQueryIFrameTransport").SetUrl("jquery.iframe-transport.min.js", "jquery.iframe-transport.js").SetVersion("1.6.1").SetDependencies("jQuery");
            manifest.DefineScript("jQueryFileUpload").SetUrl("jquery.fileupload.min.js", "jquery.fileupload.js").SetVersion("1.6.1").SetDependencies("jQueryIFrameTransport").SetDependencies("jQueryUI_Widget");

            // jquer Color Box
            manifest.DefineScript("jQueryColorBox").SetUrl("jquery.colorbox.min.js", "jquery.colorbox.js").SetVersion("1.4.10").SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox").SetUrl("colorbox.css").SetVersion("1.4.10");
        }
    }
}
