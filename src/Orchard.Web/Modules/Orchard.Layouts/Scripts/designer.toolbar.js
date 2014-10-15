(function ($) {
    var Toolbar = function (element) {
        var self = this;
        this.element = element;
        
        var initTemplateSelector = function() {
            self.element.on("change", "select[name='template-picker']", function() {
                self.element.trigger("templateselected", {
                    templateId: parseInt($(this).val())
                });
            });
        }

        var initLayoutStateViewer = function () {
            self.element.on("click", ".edit-layout-state a", function (e) {
                e.preventDefault();
                self.element.trigger("viewlayoutstate");
            });
        }

        var init = function () {
            initTemplateSelector();
            initLayoutStateViewer();
        }

        init();
    };

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Layouts = window.Orchard.Layouts || {};
    window.Orchard.Layouts.CanvasToolbar = Toolbar;
})(jQuery);