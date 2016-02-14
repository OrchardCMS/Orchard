(function ($) {
    var LayoutDesignerHost = function (element, layoutEditor) {
        var self = this;
        this.element = element;
        this.element.data("layout-designer-host", this);
        this.editor = layoutEditor;
        this.settings = {
            antiForgeryToken: self.element.data("anti-forgery-token"),
            editorDialogTitleFormat: self.element.data("editor-dialog-title-format"),
            editorDialogName: self.element.data("editor-dialog-name"),
            confirmDeletePrompt: self.element.data("confirm-delete-prompt"),
            displayType: self.element.data("display-type"),
            endpoints: {
                edit: self.element.data("edit-url"),
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

        var serializeRecycleBin = function () {
            var recycleBinData = self.editor.recycleBin.toObject();
            return JSON.stringify(recycleBinData, null, "\t");
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

        var monitorForm = function () {
            var form = $(".zone-content form:first");

            form.on("submit", function (e) {
                form.attr("isSubmitting", true);
                serializeLayout();
            });
        };

        var serializeLayout = function () {
            var layoutDataField = self.element.find(".layout-data-field");
            var recycleBinDataField = self.element.find(".recycle-bin-data-field");
            var layoutDataDataJson = serializeCanvas();
            var recycleBinDataJson = serializeRecycleBin();

            layoutDataField.val(layoutDataDataJson);
            recycleBinDataField.val(recycleBinDataJson);
        };

        this.element.on("change", ".template-picker select", function (e) {
            var selectList = $(this);
            var templateId = parseInt(selectList.val());
            applyTemplate(templateId);
        });

        $(window).on("beforeunload", function () {

            var form = $(".zone-content form:first");
            var isFormSubmitting = form.attr("isSubmitting");
            if (!isFormSubmitting && self.editor.isDirty())
                return "You have unsaved changes.";

            return undefined;
        });

        monitorForm();
    };

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Layouts = window.Orchard.Layouts || {};
    window.Orchard.Layouts.LayoutDesignerHost = LayoutDesignerHost;

})(jQuery);