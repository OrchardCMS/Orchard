(function ($) {
    var LayoutDesignerHost = function (element) {
        var self = this;
        this.element = element;
        this.element.data("layout-designer-host", this);
        this.editor = window.layoutEditor;
        this.isFormSubmitting = false;
        this.settings = {
            antiForgeryToken: self.element.data("anti-forgery-token"),
            editorDialogTitleFormat: self.element.data("editor-dialog-title-format"),
            editorDialogName: self.element.data("editor-dialog-name"),
            confirmDeletePrompt: self.element.data("confirm-delete-prompt"),
            displayType: self.element.data("display-type"),
            endpoints: {
                render: self.element.data("render-url"),
                edit: self.element.data("edit-url"),
                add: self.element.data("add-url"),
                addDirect: self.element.data("add-direct-url"),
                settings: self.element.data("settings-url"),
                browse: self.element.data("element-browser-url"),
                applyTemplate: self.element.data("apply-template-url")
            },
            domOperations: {
                append: function (container, element) { container.append(element); },
                replace: function (currentElement, newElement) { currentElement.replaceWith(newElement); }
            }
        };

        this.editElement = function (element) {
            var deferred = new $.Deferred();

            if (!element.isTemplated) {
                var elementType = element.contentType;
                var elementData = element.data;
                var elementEditorData = $.param(element.getEditorObject());
                var dialog = new window.Orchard.Layouts.Dialog(".dialog-template." + self.settings.editorDialogName);

                dialog.show();
                dialog.load(self.settings.endpoints.edit, {
                    typeName: elementType,
                    elementData: elementData,
                    elementEditorData: elementEditorData,
                    __RequestVerificationToken: self.settings.antiForgeryToken
                }, "post");

                dialog.element.on("command", function (e, args) {
                    
                    switch(args.command) {
                        case "update":
                            deferred.resolve(args);
                            dialog.close();
                            break;
                        case "cancel":
                        case "close":
                            args.cancel = true;
                            deferred.resolve(args);
                            break;
                    }
                });
            }

            return deferred.promise();
        };

        var serializeCanvas = function () {
            var layoutData = self.editor.canvas.toObject();
            return JSON.stringify(layoutData, null, "\t");
        };

        var applyTemplate = function (templateId) {
            var layoutData = serializeCanvas();

            $.ajax({
                url: self.settings.endpoints.applyTemplate,
                data: {
                    templateId: templateId,
                    layoutData: layoutData,
                    __RequestVerificationToken: self.settings.antiForgeryToken
                },
                dataType: "json",
                type: "post"
            }).then(function (response) {
                self.element.trigger("replacecanvas", { canvas: response });
            });
        };

        var monitorForm = function() {
            var layoutDesigner = self.element;
            var form = layoutDesigner.closest("form");
            
            form.on("submit", function (e) {
                self.isFormSubmitting = true;
                serializeLayout();
            });
        };

        var serializeLayout = function () {
            var layoutDataField = self.element.find(".layout-data-field");
            var layoutDataDataJson = serializeCanvas();

            layoutDataField.val(layoutDataDataJson);
        };

        this.element.on("change", ".template-picker select", function (e) {
            var selectList = $(this);
            var templateId = parseInt(selectList.val());
            applyTemplate(templateId);
        });

        $(window).on("beforeunload", function () {
            if (!self.isFormSubmitting && self.editor.isDirty())
                return "You have unsaved changes.";

            return undefined;
        });

        monitorForm();
    };

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Layouts = window.Orchard.Layouts || {};
    window.Orchard.Layouts.LayoutEditorHost = window.Orchard.Layouts.LayoutEditorHost || {};

    $(function () {
        var host = new LayoutDesignerHost($(".layout-designer"));
        $(".layout-designer").each(function (e) {
            var designer = $(this);
            var dialog = designer.find(".layout-editor-help-dialog");
            designer.find(".layout-editor-help-link").click(function (e) {
                dialog.dialog({
                    modal: true,
                    width: 840
                });
                e.preventDefault();
            });
        });
    });
})(jQuery);