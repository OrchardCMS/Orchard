using Orchard.UI.Resources;

namespace Orchard.jQuery {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery").SetUrl("jquery-1.7.1.min.js", "jquery-1.7.1.js").SetVersion("1.7.1");

            // Full jQuery UI bundle
            manifest.DefineScript("jQueryUI").SetUrl("jquery-ui.min.js", "jquery-ui.js").SetVersion("1.8.18").SetDependencies("jQuery");

            // UI Core
            manifest.DefineScript("jQueryUI_Core").SetUrl("jquery.ui.core.min.js", "jquery.ui.core.js").SetVersion("1.8.18").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Widget").SetUrl("jquery.ui.widget.min.js", "jquery.ui.widget.js").SetVersion("1.8.18").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Mouse").SetUrl("jquery.ui.mouse.min.js", "jquery.ui.mouse.js").SetVersion("1.8.18").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Position").SetUrl("jquery.ui.position.min.js", "jquery.ui.position.js").SetVersion("1.8.18").SetDependencies("jQuery");

            // Interactions
            manifest.DefineScript("jQueryUI_Draggable").SetUrl("jquery.ui.draggable.min.js", "jquery.ui.draggable.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Droppable").SetUrl("jquery.ui.droppable.min.js", "jquery.ui.droppable.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse", "jQueryUI_Draggable");
            manifest.DefineScript("jQueryUI_Resizable").SetUrl("jquery.ui.resizable.min.js", "jquery.ui.resizable.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Selectable").SetUrl("jquery.ui.selectable.min.js", "jquery.ui.selectable.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Sortable").SetUrl("jquery.ui.sortable.min.js", "jquery.ui.sortable.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");

            // Widgets
            manifest.DefineScript("jQueryUI_Accordion").SetUrl("jquery.ui.accordion.min.js", "jquery.ui.accordion.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Autocomplete").SetUrl("jquery.ui.autocomplete.min.js", "jquery.ui.autocomplete.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position");
            manifest.DefineScript("jQueryUI_Button").SetUrl("jquery.ui.button.min.js", "jquery.ui.button.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_Dialog").SetUrl("jquery.ui.dialog.min.js", "jquery.ui.dialog.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Position", "jQueryUI_Mouse", "jQueryUI_Draggable", "jQueryUI_Resizable");
            manifest.DefineScript("jQueryUI_Slider").SetUrl("jquery.ui.slider.min.js", "jquery.ui.slider.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget", "jQueryUI_Mouse");
            manifest.DefineScript("jQueryUI_Tabs").SetUrl("jquery.ui.tabs.min.js", "jquery.ui.tabs.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUI_DatePicker").SetUrl("jquery.ui.datepicker.min.js", "jquery.ui.datepicker.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("jQueryUI_TimePicker").SetUrl("jquery.ui.timepicker.min.js", "jquery.ui.timepicker.js").SetVersion("1.0.0").SetDependencies("jQueryUI_Slider", "jQueryUI_DatePicker");
            manifest.DefineScript("jQueryUI_Progressbar").SetUrl("jquery.ui.progressbar.min.js", "jquery.ui.progressbar.js").SetVersion("1.8.18").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");

            // Effects
            manifest.DefineScript("jQueryEffects_Core").SetUrl("jquery.effects.core.min.js", "jquery.effects.core.js").SetVersion("1.8.18").SetDependencies("jQuery");
            manifest.DefineScript("jQueryEffects_Blind").SetUrl("jquery.effects.blind.min.js", "jquery.effects.blind.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Bounce").SetUrl("jquery.effects.bounce.min.js", "jquery.effects.bounce.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Clip").SetUrl("jquery.effects.clip.min.js", "jquery.effects.clip.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Drop").SetUrl("jquery.effects.drop.min.js", "jquery.effects.drop.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Explode").SetUrl("jquery.effects.explode.min.js", "jquery.effects.explode.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Fade").SetUrl("jquery.effects.fade.min.js", "jquery.effects.fade.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Fold").SetUrl("jquery.effects.fold.min.js", "jquery.effects.fold.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Highlight").SetUrl("jquery.effects.highlight.min.js", "jquery.effects.highlight.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Pulsate").SetUrl("jquery.effects.pulsate.min.js", "jquery.effects.pulsate.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Scale").SetUrl("jquery.effects.scale.min.js", "jquery.effects.scale.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Shake").SetUrl("jquery.effects.shake.min.js", "jquery.effects.shake.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Slide").SetUrl("jquery.effects.slide.min.js", "jquery.effects.slide.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");
            manifest.DefineScript("jQueryEffects_Transfer").SetUrl("jquery.effects.transfer.min.js", "jquery.effects.transfer.js").SetVersion("1.8.18").SetDependencies("jQueryEffects_Core");

            manifest.DefineScript("jQueryUtils").SetUrl("jquery.utils.js").SetDependencies("jQuery");

            manifest.DefineStyle("jQueryUI_Orchard").SetUrl("jquery-ui-1.8.18.custom.css").SetVersion("1.8.18");
            manifest.DefineStyle("jQueryUI_DatePicker").SetUrl("ui.datepicker.css").SetDependencies("jQueryUI_Orchard").SetVersion("1.7.2");
            manifest.DefineStyle("jQueryUI_TimePicker").SetUrl("ui.timepicker.css").SetDependencies("jQueryUI_DatePicker").SetVersion("1.0.0");
        }
    }
}
