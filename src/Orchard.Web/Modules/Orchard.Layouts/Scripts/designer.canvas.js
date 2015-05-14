(function ($) {
    var LayoutDesigner = function (element) {
        var self = this;
        this.element = element;
        this.canvas = element.find(".canvas");
        this.toolbar = new window.Orchard.Layouts.CanvasToolbar(element.find(".canvas-toolbar"));
        this.element.data("layout-designer", this);
        this.settings = {
            antiForgeryToken: self.element.data("anti-forgery-token"),
            editorDialogTitleFormat: self.element.data("editor-dialog-title-format"),
            editorDialogName: self.element.data("editor-dialog-name"),
            confirmDeletePrompt: self.element.data("confirm-delete-prompt"),
            displayType: self.element.data("display-type"),
            endpoints: {
                render: self.element.data("render-url"),
                edit: self.element.data("edit-url"),
                settings: self.element.data("settings-url"),
                browse: self.element.data("element-browser-url"),
                applyTemplate: self.element.data("apply-template-url")
            },
            domOperations: {
                append: function (container, element) { container.append(element); },
                replace: function (currentElement, newElement) { currentElement.replaceWith(newElement); }
            }
        };

        this.browse = function (targetContainer) {
            var href = self.settings.endpoints.browse;
            var w = window.parent || window;
            var dialog = new w.Orchard.Layouts.Dialog(".dialog-template." + self.settings.editorDialogName);

            dialog.show();
            dialog.load(href);

            dialog.element.on("command", function (e, data) {
                if (data.command == "add" || data.command == "save") {
                    var graph = {
                        elements: [
                            {
                                typeName: data.element.typeName,
                                data: data.element.data
                            }
                        ]
                    }
                    self.renderGraph(targetContainer, graph, self.settings.domOperations.append);
                    self.element.find("a.add.start").removeClass("start").hide();

                    dialog.close();
                }
            });
        };

        this.refreshElement = function (elementUI, elementData) {
            // Serialize the element UI into an object graph.
            var graph = {};
            var data = elementData;
            var stateFormValues = data ? $.deserialize(data) : null;
            var formData = $.extend({}, stateFormValues);
            window.Orchard.Layouts.Serializer.serialize(graph, elementUI);

            if (data) {
                // Update the element with the new Data.
                graph.elements[0].state = data;
            }

            // Special case for columns - we need to refresh the row, as changes to the column will have an effect to its silbings.
            if (elementUI.is(".x-column")) {
                // Serialize the parent UI into an object graph. 
                var containerUI = elementUI.parents(".x-container:first");
                var containerGraph = {};
                window.Orchard.Layouts.Serializer.serialize(containerGraph, containerUI);
                containerGraph.elements[0].elements.splice(graph.elements[0].index, 1, graph.elements[0]);

                // Render the graph.
                this.renderGraph(containerUI, containerGraph, self.settings.domOperations.replace, formData);
            } else {
                // Render the graph.
                this.renderGraph(elementUI, graph, self.settings.domOperations.replace, formData);
            }
        };

        this.serialize = window.Orchard.Layouts.Serializer.serialize;

        this.applyTemplate = function (templateId) {
            var graph = this.serialize({}, this.canvas);
            
            $.ajax({
                url: self.settings.endpoints.applyTemplate,
                data: {
                    templateId: templateId,
                    layoutstate: JSON.stringify(graph),
                    __RequestVerificationToken: self.settings.antiForgeryToken
                },
                dataType: "html",
                type: "post"
            }).then(function (html) {
                self.canvas.find(".x-root > .x-holder").html(html);

                // Trigger event.
                self.element.trigger("elementupdated", { elementUI: self.canvas });
            });
        };

        this.toolbar.element.on("templateselected", function (e, data) {
            var templateId = data.templateId;
            self.applyTemplate(templateId);
        });

        var onEditElement = function (e) {
            var sender = $(e.sender);
            var elementUI = sender.closest(".x-element");
            var elementData = elementUI.data("element");
            var data = elementData.data;
            var dialog = new window.Orchard.Layouts.Dialog(".dialog-template");

            dialog.show();
            dialog.load(self.settings.endpoints.edit, {
                typeName: elementData.typeName,
                elementData: data,
                __RequestVerificationToken: self.settings.antiForgeryToken
            }, "post");

            dialog.element.on("command", function (e, args) {
                if (args.command == "save") {
                    self.refreshElement(elementUI, args.element.data);
                    dialog.close();
                }
            });
        };


        var onRemoveElement = function (e) {
            var sender = $(e.sender);
            var elementUI = sender.closest(".x-element");

            if (confirm(self.settings.confirmDeletePrompt)) {
                removeElement(elementUI);
            }
        };

        var initElementActions = function () {
            self.canvas.on("click", "a.edit", function (e) {
                e.preventDefault();
                onEditElement({ sender: this });
            });
            self.canvas.on("click", "a.remove", function (e) {
                e.preventDefault();
                onRemoveElement({ sender: this });
            });
        };

        var init = function () {
            initElementActions();
        };

        init();
    };

    $(function () {
        //window.layoutDesigner = new LayoutDesigner($(".layout-designer"));;
    });
})(jQuery);