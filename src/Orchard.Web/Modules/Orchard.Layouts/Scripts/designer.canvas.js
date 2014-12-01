(function ($) {
    var Canvas = function (element) {
        var self = this;
        this.element = element;
        this.canvas = element.find(".canvas");
        this.toolbar = new window.Orchard.Layouts.CanvasToolbar(element.find(".canvas-toolbar"));
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
                                state: data.element.state ? decodeURIComponent(data.element.state) : null
                            }
                        ]
                    }
                    self.renderGraph(targetContainer, graph, self.settings.domOperations.append);
                    self.element.find("a.add.start").removeClass("start").hide();

                    dialog.close();
                }
            });
        };

        this.refreshElement = function (elementUI, elementState) {
            // Serialize the element UI into an object graph.
            var graph = {};
            var state = elementState ? decodeURIComponent(elementState) : null;
            var stateFormValues = state ? $.deserialize(state) : null;
            var formData = $.extend({}, stateFormValues);
            window.Orchard.Layouts.Serializer.serialize(graph, elementUI);

            if (state) {
                // Update the element with the new state.
                graph.elements[0].state = state;
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

        this.renderGraph = function (container, graph, domOperation, extraData) {
            var deferred = new $.Deferred();
            getGraphHtml(graph, extraData).done(function (html) {
                var elementUI = $(html);
                var operation = domOperation || self.settings.domOperations.append;

                operation(container, elementUI);

                // Reinitialize drag & drop.
                initDragDrop();

                // Trigger event.
                self.element.trigger("elementupdated", { elementUI: elementUI });

                deferred.resolve({ elementUI: elementUI });
            });
            return deferred.promise();
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

                // Reinitialize drag & drop.
                initDragDrop();

                // Trigger event.
                self.element.trigger("elementupdated", { elementUI: self.canvas });
            });
        };

        this.toolbar.element.on("templateselected", function (e, data) {
            var templateId = data.templateId;
            self.applyTemplate(templateId);
        });

        this.toolbar.element.on("viewlayoutstate", function (e) {
            var graph = self.serialize({}, self.canvas);
            var layoutState = JSON.stringify(graph, null, 3);
            var w = window.parent || window;
            var dialog = new w.Orchard.Layouts.Dialog(".dialog-template");

            dialog.show();
            dialog.setHtml($(
                "<textarea class=\"text large\" rows=\"35\">" + layoutState + "</textarea>" + 
                "<div class=\"dialog-settings\">" +
                "    <div class=\"title\">Layout State</div>" +
                "    <div class=\"buttons\">" +
                "        <a href=\"#\" class=\"button update\" data-command=\"update\">Update</a>" +
                "        <a href=\"#\" class=\"button cancel\">Cancel</a>" +
                "    </div>" +
                "</div>"
            ));

            dialog.element.on("command", function (e, data) {
                if (data.command == "update") {
                    var updatedLayoutState = dialog.view.find("textarea").val();
                    var target = self.canvas.find(".x-root > .x-holder");
                    var updatedGraph = JSON.parse(updatedLayoutState);
                    self.renderGraph(target, updatedGraph, self.settings.domOperations.replace);
                    dialog.close();
                }
            });
        });

        var onEditElement = function (e) {
            var sender = $(e.sender);
            var elementUI = sender.closest(".x-element");
            var elementData = elementUI.data("element");
            var elementState = elementData.state;
            var w = window.parent || window;
            var dialog = new w.Orchard.Layouts.Dialog(".dialog-template");

            dialog.show();
            dialog.load(self.settings.endpoints.edit, {
                typeName: elementData.typeName,
                elementState: elementState,
                __RequestVerificationToken: self.settings.antiForgeryToken
            }, "post");

            dialog.element.on("command", function (e, data) {
                if (data.command == "save") {
                    var state = data.element.state;
                    self.refreshElement(elementUI, state);
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

        var initDragDrop = function () {
            self.element.find(".drop-target").sortable({
                revert: false,
                zIndex: 100,
                handle: "header, .drag-handle, .element-overlay",
                placeholder: "sortable-placeholder",
                helper: "clone",
                connectWith: ".drop-target"
            });
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

        var initContainers = function () {
            self.element.on("mouseenter", ".x-container", function (e) {
                $(this).find("a.add:not(.root)").fadeIn("fast");
                $(this).find(".dynamic-toolbar:first").fadeIn("fast");
            });
            self.element.on("mouseleave", ".x-container", function (e) {
                $(this).find("a.add:not(.root):last").fadeOut();
                $(this).find(".dynamic-toolbar:first").fadeOut();
            });
            self.element.on("mouseenter", ".x-root", function (e) {
                $(this).find("> a.add.root").fadeIn("fast");
            });
            self.element.on("mouseleave", ".x-root", function (e) {
                $(this).find("> a.add.root").fadeOut();
            });
            self.element.on("click", ".x-container a.add", function (e) {
                e.preventDefault();
                self.browse($(this).closest(".x-container").find(".x-holder:first"));
            });
        };

        var getGraphHtml = function (graph, extraData) {
            var url = self.settings.endpoints.render;
            return $.ajax(url, {
                data: $.extend({
                    displayType: self.settings.displayType,
                    graph: JSON.stringify(graph),
                    __RequestVerificationToken: self.settings.antiForgeryToken
                }, extraData),
                dataType: "html",
                type: "post"
            });
        };

        var removeElement = function (elementUI) {
            var trash = self.element.find(".trash");
            var container = null;

            if (elementUI.is(".x-column")) {
                container = elementUI.closest(".x-row");
            }

            elementUI.detach();
            trash.append(elementUI);

            if (container != null) {
                self.refreshElement(container);
            }

            self.element.trigger("elementremoved", { elementUI: elementUI });
        };

        var init = function () {
            initDragDrop();
            initElementActions();
            initContainers();
        };

        init();
    };

    $(function () {
        var layoudEditor = $(".layout-editor");
        var canvas = new Canvas(layoudEditor);

        layoudEditor.data("layout-editor", canvas);
        window.layoutEditor = canvas;
    });
})(jQuery);