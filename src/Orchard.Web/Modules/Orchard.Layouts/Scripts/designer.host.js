(function ($) {

    var LayoutEditorHost = function (element) {
        var self = this;
        this.element = element;
        this.frame = new window.Orchard.Frame(element.find(".layout-designer-host"));

        this.monitorForm = function() {
            var layoutDesigner = this.element;
            var form = layoutDesigner.closest("form");
            
            form.on("submit", function (e) {
                serializeLayout();
                serializeTrash();
            });
        };

        this.monitorLayout = function() {
            this.frame.element.on("load", function () {
                var canvas = self.frame.getWindow().layoutEditor;

                if (!canvas)
                    return; // This happens if the iframe didn't load the expected page (i.e. because of a runtime error).

                canvas.element.on("elementupdated", function (e, data) {
                    self.autoHeight();
                });

                canvas.element.on("elementremoved", function (e, data) {
                    self.autoHeight();
                });

                self.autoHeight();
            });
        };

        this.autoHeight = function() {
            self.frame.autoHeight();
        };

        this.recover = function() {
            var isModelStateValid = self.element.data("modelstate-valid");
            if (isModelStateValid)
                return;

            var form = self.element.closest("form");
            var stateFieldName = self.element.data("state-field-name");
            var stateField = form.find("input[name=\"" + stateFieldName + "\"]");
            var url = self.element.data("frame-url");

            self.frame.load(url, {
                state: stateField.val(),
                __RequestVerificationToken: self.element.data("anti-forgery-token")
            }, "post");
        }

        var serializeLayout = function () {
            var form = self.element.closest("form");
            var templateFieldName = self.element.data("template-picker-name");
            var templateField = form.find("input[name=\"" + templateFieldName + "\"]");
            var frameDocument = self.frame.getDocument();
            var templatePicker = frameDocument.find("select[name='template-picker']");
            var selectedTemplateId = templatePicker.val();

            templateField.val(selectedTemplateId);
            serialize("state-field-name", ".canvas");
        };

        var serializeTrash = function () {
            serialize("trash-field-name", ".trash");
        };

        var serialize = function (stateFieldDataName, scopeSelector) {
            var layoutDesigner = self.element;
            var stateFieldName = layoutDesigner.data(stateFieldDataName);
            var stateField = layoutDesigner.find("input[name=\"" + stateFieldName + "\"]");
            var frameDocument = self.frame.getDocument();
            var scope = frameDocument.find(scopeSelector);
            var graph = {};
            var serializer = window.Orchard.Layouts.Serializer;

            serializer.serialize(graph, scope);
            var state = JSON.stringify(graph);
            stateField.val(state);
        };
    };

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Layouts = window.Orchard.Layouts || {};
    window.Orchard.Layouts.LayoutEditorHost = window.Orchard.Layouts.LayoutEditorHost || {};

    $(function () {
        $(".layout-designer").each(function() {
            var host = new LayoutEditorHost($(this));
            host.recover();
            host.monitorForm();
            host.monitorLayout();
        });
    });
})(jQuery);