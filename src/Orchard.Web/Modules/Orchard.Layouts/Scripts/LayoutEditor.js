angular.module("LayoutEditor", ["ngSanitize", "ngResource", "ui.sortable"]);
var LayoutEditor;
(function(LayoutEditor) {

    var Clipboard = function () {
        var self = this;
        this._clipboardData = {};
        this._isDisabled = false;
        this._wasInvoked = false;

        this.setData = function(contentType, data) {
            self._clipboardData[contentType] = data;
            self._wasInvoked = true;
        };
        this.getData = function (contentType) {
            return self._clipboardData[contentType];
            self._wasInvoked = true;
        };
        this.disable = function() {
            self._isDisabled = true;
            self._wasInvoked = false;
            self._clipboardData = {};
        };
        this.isDisabled = function () {
            return self._isDisabled;
        }
        this.wasInvoked = function () {
            return self._wasInvoked;
        }
    }

    LayoutEditor.Clipboard = new Clipboard();

    angular
        .module("LayoutEditor")
        .factory("clipboard", [
            function() {
                return {
                    setData: LayoutEditor.Clipboard.setData,
                    getData: LayoutEditor.Clipboard.getData,
                    disable: LayoutEditor.Clipboard.disable,
                    isDisabled: LayoutEditor.Clipboard.isDisabled,
                    wasInvoked: LayoutEditor.Clipboard.wasInvoked
                };
            }
        ]);
})(LayoutEditor || (LayoutEditor = {}));
angular
    .module("LayoutEditor")
    .factory("scopeConfigurator", ["$timeout", "clipboard",
        function ($timeout, clipboard) {
            return {

                configureForElement: function ($scope, $element) {
                
                    $element.find(".layout-panel").click(function (e) {
                        e.stopPropagation();
                    });

                    $element.parent().keydown(function (e) {
                        var handled = false;
                        var resetFocus = false;
                        var element = $scope.element;
                    
                        if (element.editor.isDragging || element.editor.inlineEditingIsActive)
                            return;

                        // If native clipboard support exists, the pseudo-clipboard will have been disabled.
                        if (!clipboard.isDisabled()) {
                            var focusedElement = element.editor.focusedElement;
                            if (!!focusedElement) {
                                // Pseudo clipboard handling for browsers not allowing real clipboard operations.
                                if (e.ctrlKey) {
                                    switch (e.which) {
                                    case 67: // C
                                        focusedElement.copy(clipboard);
                                        break;
                                    case 88: // X
                                        focusedElement.cut(clipboard);
                                        break;
                                    case 86: // V
                                        focusedElement.paste(clipboard);
                                        break;
                                    }
                                }
                            }
                        }

                        if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 46) { // Del
                            $scope.delete(element);
                            handled = true;
                        } else if (!e.ctrlKey && !e.shiftKey && !e.altKey && (e.which == 32 || e.which == 27)) { // Space or Esc
                            $element.find(".layout-panel-action-properties").first().click();
                            handled = true;
                        }

                        if (element.type == "Content") { // This is a content element.
                            if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 13) { // Enter
                                $element.find(".layout-panel-action-edit").first().click();
                                handled = true;
                            }
                        }

                        if (!!element.children) { // This is a container.
                            if (!e.ctrlKey && !e.shiftKey && e.altKey && e.which == 40) { // Alt+Down
                                if (element.children.length > 0)
                                    element.children[0].setIsFocused();
                                handled = true;
                            }

                            if (element.type == "Column") { // This is a column.
                                var connectAdjacent = !e.ctrlKey;
                                if (e.which == 37) { // Left
                                    if (e.altKey)
                                        element.expandLeft(connectAdjacent);
                                    if (e.shiftKey)
                                        element.contractRight(connectAdjacent);
                                    handled = true;
                                } else if (e.which == 39) { // Right
                                    if (e.altKey)
                                        element.contractLeft(connectAdjacent);
                                    if (e.shiftKey)
                                        element.expandRight(connectAdjacent);
                                    handled = true;
                                }
                            }
                        }

                        if (!!element.parent) { // This is a child.
                            if (e.altKey && e.which == 38) { // Alt+Up
                                element.parent.setIsFocused();
                                handled = true;
                            }

                            if (element.parent.type == "Row") { // Parent is a horizontal container.
                                if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 37) { // Left
                                    element.parent.moveFocusPrevChild(element);
                                    handled = true;
                                }
                                else if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 39) { // Right
                                    element.parent.moveFocusNextChild(element);
                                    handled = true;
                                }
                                else if (e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 37) { // Ctrl+Left
                                    element.moveUp();
                                    resetFocus = true;
                                    handled = true;
                                }
                                else if (e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 39) { // Ctrl+Right
                                    element.moveDown();
                                    handled = true;
                                }
                            }
                            else { // Parent is a vertical container.
                                if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 38) { // Up
                                    element.parent.moveFocusPrevChild(element);
                                    handled = true;
                                }
                                else if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 40) { // Down
                                    element.parent.moveFocusNextChild(element);
                                    handled = true;
                                }
                                else if (e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 38) { // Ctrl+Up
                                    element.moveUp();
                                    resetFocus = true;
                                    handled = true;
                                }
                                else if (e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 40) { // Ctrl+Down
                                    element.moveDown();
                                    handled = true;
                                }
                            }
                        }

                        if (handled) {
                            e.preventDefault();
                        }

                        e.stopPropagation();

                        $scope.$apply(); // Event is not triggered by Angular directive but raw event handler on element.

                        // HACK: Workaround because of how Angular treats the DOM when elements are shifted around - input focus is sometimes lost.
                        if (resetFocus) {
                            window.setTimeout(function () {
                                $scope.$apply(function () {
                                    element.editor.focusedElement.setIsFocused();
                                });
                            }, 100);
                        }
                    });

                    $scope.element.setIsFocusedEventHandlers.push(function () {
                        $element.parent().focus();
                    });

                    $scope.delete = function (element) {
                        element.delete();
                    }
                },

                configureForContainer: function ($scope, $element) {
                    var element = $scope.element;

                    //$scope.isReceiving = false; // True when container is receiving an external element via drag/drop.
                    $scope.getShowChildrenPlaceholder = function () {
                        return $scope.element.children.length === 0 && !$scope.element.getIsDropTarget();
                    };

                    $scope.sortableOptions = {
                        cursor: "move",
                        delay: 150,
                        disabled: element.getIsSealed(),
                        distance: 5,
                        //handle: element.children.length < 2 ? ".imaginary-class" : false, // For some reason doesn't get re-evaluated after adding more children.
                        start: function (e, ui) {
                            $scope.$apply(function () {
                                element.setIsDropTarget(true);
                                element.editor.isDragging = true;
                            });
                            // Make the drop target placeholder as high as the item being dragged.
                            ui.placeholder.height(ui.item.height() - 4);
                            ui.placeholder.css("min-height", 0);
                        },
                        stop: function (e, ui) {
                            $scope.$apply(function () {
                                element.editor.isDragging = false;
                                element.setIsDropTarget(false);
                            });
                        },
                        over: function (e, ui) {
                            if (!!ui.sender && !!ui.sender[0].isToolbox) {
                                if (!!ui.sender[0].dropTargetTimeout) {
                                    $timeout.cancel(ui.sender[0].dropTargetTimeout);
                                    ui.sender[0].dropTargetTimeout = null;
                                }
                                $timeout(function () {
                                    if (element.type == "Row") {
                                        // If there was a previous drop target and it was a row, roll back any pending column adds to it.
                                        var previousDropTarget = element.editor.dropTargetElement;
                                        if (!!previousDropTarget && previousDropTarget.type == "Row")
                                            previousDropTarget.rollbackAddColumn();
                                    }
                                    element.setIsDropTarget(false);
                                });
                                ui.sender[0].dropTargetTimeout = $timeout(function () {
                                    if (element.type == "Row") {
                                        var receivedColumn = ui.item.sortable.model;
                                        var receivedColumnWidth = Math.floor(12 / (element.children.length + 1));
                                        receivedColumn.width = receivedColumnWidth;
                                        receivedColumn.offset = 0;
                                        element.beginAddColumn(receivedColumnWidth);
                                        // Make the drop target placeholder the correct width and as high as the highest existing column in the row.
                                        var maxHeight = _.max(_($element.find("> .layout-children > .layout-column:not(.ui-sortable-placeholder)")).map(function (e) {
                                            return $(e).height();
                                        }));
                                        for (i = 1; i <= 12; i++)
                                            ui.placeholder.removeClass("col-xs-" + i);
                                        ui.placeholder.addClass("col-xs-" + receivedColumn.width);
                                        if (maxHeight > 0) {
                                            ui.placeholder.height(maxHeight);
                                            ui.placeholder.css("min-height", 0);
                                        }
                                        else {
                                            ui.placeholder.height(0);
                                            ui.placeholder.css("min-height", "");
                                        }
                                    }
                                    element.setIsDropTarget(true);
                                }, 150);
                            }
                        },
                        receive: function (e, ui) {
                            if (!!ui.sender && !!ui.sender[0].isToolbox) {
                                $scope.$apply(function () {
                                    var receivedElement = ui.item.sortable.model;
                                    if (!!receivedElement) {
                                        if (element.type == "Row")
                                            element.commitAddColumn();
                                        // Should ideally call LayoutEditor.Container.addChild() instead, but since this handler
                                        // is run *before* the ui-sortable directive's handler, if we try to add the child to the
                                        // array that handler will get an exception when trying to do the same.
                                        // Because of this, we need to invoke "setParent" so that specific container types can perform element speficic initialization.
                                        receivedElement.setEditor(element.editor);
                                        receivedElement.setParent(element);

                                        if (!!receivedElement.hasEditor) {
                                            $scope.$root.editElement(receivedElement).then(function (args) {
                                                if (!args.cancel) {
                                                    receivedElement.data = args.element.data;
                                                    receivedElement.applyElementEditorModel(args.elementEditorModel);

                                                    if (!!receivedElement.setHtml)
                                                        receivedElement.setHtml(args.element.html);
                                                }
                                                $timeout(function () {
                                                    if (!!args.cancel)
                                                        receivedElement.delete();
                                                    else
                                                        receivedElement.setIsFocused();
                                                    //$scope.isReceiving = false;
                                                    element.setIsDropTarget(false);

                                                });
                                                return;
                                            });
                                        }
                                    }
                                    $timeout(function () {
                                        //$scope.isReceiving = false;
                                        element.setIsDropTarget(false);
                                        if (!!receivedElement)
                                            receivedElement.setIsFocused();
                                    });
                                });
                            }
                        }
                    };

                    $scope.click = function (child, e) {
                        if (!child.editor.isDragging)
                            child.setIsFocused();
                        e.stopPropagation();
                    };

                    $scope.getClasses = function (child) {
                        var result = ["layout-element"];

                        if (!!child.children) {
                            result.push("layout-container");
                            if (child.getIsSealed())
                                result.push("layout-container-sealed");
                        }

                        result.push("layout-" + child.type.toLowerCase());

                        if (!!child.dropTargetClass)
                            result.push(child.dropTargetClass);

                        // TODO: Move these to either the Column directive or the Column model class.
                        if (child.type == "Row") {
                            result.push("row");
                            if (!child.canAddColumn())
                                result.push("layout-row-full");
                        }
                        if (child.type == "Column") {
                            result.push("col-xs-" + child.width);
                            result.push("col-xs-offset-" + child.offset);
                        }
                        if (child.type == "Content")
                            result.push("layout-content-" + child.contentTypeClass);

                        if (child.getIsActive())
                            result.push("layout-element-active");
                        if (child.getIsFocused())
                            result.push("layout-element-focused");
                        if (child.getIsSelected())
                            result.push("layout-element-selected");
                        if (child.getIsDropTarget())
                            result.push("layout-element-droptarget");
                        if (child.isTemplated)
                            result.push("layout-element-templated");

                        return result;
                    };
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutEditor", ["environment",
        function (environment) {
            return {
                restrict: "E",
                scope: {},
                controller: ["$scope", "$element", "$attrs", "$compile", "clipboard",
                    function ($scope, $element, $attrs, $compile, clipboard) {
                        if (!!$attrs.model)
                            $scope.element = eval($attrs.model);
                        else
                            throw new Error("The 'model' attribute must evaluate to a LayoutEditor.Editor object.");

                        $scope.click = function (canvas, e) {
                            if (!canvas.editor.isDragging)
                                canvas.setIsFocused();
                            e.stopPropagation();
                        };

                        $scope.getClasses = function (canvas) {
                            var result = ["layout-element", "layout-container", "layout-canvas"];

                            if (canvas.getIsActive())
                                result.push("layout-element-active");
                            if (canvas.getIsFocused())
                                result.push("layout-element-focused");
                            if (canvas.getIsSelected())
                                result.push("layout-element-selected");
                            if (canvas.getIsDropTarget())
                                result.push("layout-element-droptarget");
                            if (canvas.isTemplated)
                                result.push("layout-element-templated");

                            return result;
                        };

                        // An unfortunate side-effect of the next hack on line 54 is that the created elements aren't added to the DOM yet, so we can't use it to get to the parent ".layout-desiger" element.
                        // Work around: access that element directly (which efectively turns multiple layout editors on a single page impossible). 
                        // //var layoutDesignerHost = $element.closest(".layout-designer").data("layout-designer-host");
                        var layoutDesignerHost = $(".layout-designer").data("layout-designer-host");

                        $scope.$root.layoutDesignerHost = layoutDesignerHost;

                        layoutDesignerHost.element.on("replacecanvas", function (e, args) {
                            var editor = $scope.element;
                            var canvasData = {
                                data: args.canvas.data,
                                htmlId: args.canvas.htmlId,
                                htmlClass: args.canvas.htmlClass,
                                htmlStyle: args.canvas.htmlStyle,
                                isTemplated: args.canvas.isTemplated,
                                children: args.canvas.children
                            };

                            // HACK: Instead of simply updating the $scope.element with a new instance, we need to replace the entire orc-layout-editor markup
                            // in order for angular to rebind starting with the Canvas element. Otherwise, for some reason, it will rebind starting with the first child of Canvas.
                            // You can see this happening when setting a breakpoint in ScopeConfigurator where containers are initialized with drag & drop: on page load, the first element
                            // is a Canvas (good), but after having selected another template, the first element is (typically) a Grid (bad).
                            // Simply recompiling the orc-layout-editor directive will cause the entire thing to be generated, which works just fine as well (even though not is nice as simply leveraging model binding).
                            layoutDesignerHost.editor = window.layoutEditor = new LayoutEditor.Editor(editor.config, canvasData);
                            var template = "<orc-layout-editor" + " model='window.layoutEditor' />";
                            var html = $compile(template)($scope);
                            $(".layout-editor-holder").html(html);
                        });

                        $scope.$root.editElement = function (element) {
                            var host = $scope.$root.layoutDesignerHost;
                            return host.editElement(element);
                        };

                        $scope.$root.addElement = function (contentType) {
                            var host = $scope.$root.layoutDesignerHost;
                            return host.addElement(contentType);
                        };

                        $scope.toggleInlineEditing = function () {
                            if (!$scope.element.inlineEditingIsActive) {
                                $scope.element.inlineEditingIsActive = true;
                                $element.find(".layout-toolbar-container").show();
                                var selector = "#layout-editor-" + $scope.$id + " .layout-html .layout-content-markup[data-templated=false]";
                                var firstContentEditorId = $(selector).first().attr("id");
                                tinymce.init({
                                    selector: selector,
                                    theme: "modern",
                                    schema: "html5",
                                    plugins: [
                                        "advlist autolink lists link image charmap print preview hr anchor pagebreak",
                                        "searchreplace wordcount visualblocks visualchars code fullscreen",
                                        "insertdatetime media nonbreaking table contextmenu directionality",
                                        "emoticons template paste textcolor colorpicker textpattern",
                                        "fullscreen autoresize"
                                    ],
                                    toolbar: "undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | link unlink charmap | code fullscreen close",
                                    convert_urls: false,
                                    valid_elements: "*[*]",
                                    // Shouldn't be needed due to the valid_elements setting, but TinyMCE would strip script.src without it.
                                    extended_valid_elements: "script[type|defer|src|language]",
                                    statusbar: false,
                                    skin: "orchardlightgray",
                                    inline: true,
                                    fixed_toolbar_container: "#layout-editor-" + $scope.$id + " .layout-toolbar-container",
                                    init_instance_callback: function (editor) {
                                        if (editor.id == firstContentEditorId)
                                            tinymce.execCommand("mceFocus", false, editor.id);
                                    }
                                });
                            }
                            else {
                                tinymce.remove("#layout-editor-" + $scope.$id + " .layout-content-markup");
                                $element.find(".layout-toolbar-container").hide();
                                $scope.element.inlineEditingIsActive = false;
                            }
                        };

                        $(document).on("cut copy paste", function (e) {
                            // If the pseudo clipboard was already invoked (which happens on the first clipboard
                            // operation after page load even if native clipboard support exists) then sit this
                            // one operation out, but make sure whatever is on the pseudo clipboard gets migrated
                            // to the native clipboard for subsequent operations.
                            if (clipboard.wasInvoked()) {
                                e.originalEvent.clipboardData.setData("text/plain", clipboard.getData("text/plain"));
                                e.originalEvent.clipboardData.setData("text/json", clipboard.getData("text/json"));
                                e.preventDefault();
                            }
                            else {
                                var focusedElement = $scope.element.focusedElement;
                                if (!!focusedElement) {
                                    $scope.$apply(function () {
                                        switch (e.type) {
                                            case "copy":
                                                focusedElement.copy(e.originalEvent.clipboardData);
                                                break;
                                            case "cut":
                                                focusedElement.cut(e.originalEvent.clipboardData);
                                                break;
                                            case "paste":
                                                focusedElement.paste(e.originalEvent.clipboardData);
                                                break;
                                        }
                                    });

                                    // HACK: Workaround because of how Angular treats the DOM when elements are shifted around - input focus is sometimes lost.
                                    window.setTimeout(function () {
                                        $scope.$apply(function () {
                                            if (!!$scope.element.focusedElement)
                                                $scope.element.focusedElement.setIsFocused();
                                        });
                                    }, 100);

                                    e.preventDefault();
                                }
                            }

                            // Native clipboard support obviously exists, so disable the peudo clipboard from now on.
                            clipboard.disable();
                        });
                    }
                ],
                templateUrl: environment.templateUrl("Editor"),
                replace: true,
                link: function (scope, element) {
                    // No clicks should propagate from the TinyMCE toolbars.
                    element.find(".layout-toolbar-container").click(function (e) {
                        e.stopPropagation();
                    });
                    // Intercept mousedown on editor while in inline editing mode to 
                    // prevent current editor from losing focus.
                    element.mousedown(function (e) {
                        if (scope.element.inlineEditingIsActive) {
                            e.preventDefault();
                            e.stopPropagation();
                        }
                    })
                    // Unfocus and unselect everything on click outside of canvas.
                    $(window).click(function (e) {
                        // Except when in inline editing mode.
                        if (!scope.element.inlineEditingIsActive) {
                            scope.$apply(function () {
                                scope.element.activeElement = null;
                                scope.element.focusedElement = null;
                            });
                        }
                    });
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutCanvas", ["scopeConfigurator", "environment",
        function (scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element", "$attrs",
                    function ($scope, $element, $attrs) {
                        scopeConfigurator.configureForElement($scope, $element);
                        scopeConfigurator.configureForContainer($scope, $element);
                        $scope.sortableOptions["axis"] = "y";
                    }
                ],
                templateUrl: environment.templateUrl("Canvas"),
                replace: true
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutChild", ["$compile",
        function ($compile) {
            return {
                restrict: "E",
                scope: { element: "=" },
                link: function (scope, element) {
                    var template = "<orc-layout-" + scope.element.type.toLowerCase() + " element='element' />";
                    var html = $compile(template)(scope);
                    $(element).replaceWith(html);
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutColumn", ["$compile", "scopeConfigurator", "environment",
        function ($compile, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        scopeConfigurator.configureForElement($scope, $element);
                        scopeConfigurator.configureForContainer($scope, $element);
                        $scope.sortableOptions["axis"] = "y";
                    }
                ],
                templateUrl: environment.templateUrl("Column"),
                replace: true,
                link: function (scope, element, attrs) {
                    element.find(".layout-column-resize-bar").draggable({
                        axis: "x",
                        helper: "clone",
                        revert: true,
                        start: function (e, ui) {
                            scope.$apply(function () {
                                scope.element.editor.isResizing = true;
                            });
                        },
                        drag: function (e, ui) {
                            var columnElement = element.parent();
                            var columnSize = columnElement.width() / scope.element.width;
                            var connectAdjacent = !e.ctrlKey;
                            if ($(e.target).hasClass("layout-column-resize-bar-left")) {
                                var delta = ui.offset.left - columnElement.offset().left;
                                if (delta < -columnSize && scope.element.canExpandLeft(connectAdjacent)) {
                                    scope.$apply(function () {
                                        scope.element.expandLeft(connectAdjacent);
                                    });
                                }
                                else if (delta > columnSize && scope.element.canContractLeft(connectAdjacent)) {
                                    scope.$apply(function () {
                                        scope.element.contractLeft(connectAdjacent);
                                    });
                                }
                            }
                            else if ($(e.target).hasClass("layout-column-resize-bar-right")) {
                                var delta = ui.offset.left - columnElement.width() - columnElement.offset().left;
                                if (delta > columnSize && scope.element.canExpandRight(connectAdjacent)) {
                                    scope.$apply(function () {
                                        scope.element.expandRight(connectAdjacent);
                                    });
                                }
                                else if (delta < -columnSize && scope.element.canContractRight(connectAdjacent)) {
                                    scope.$apply(function () {
                                        scope.element.contractRight(connectAdjacent);
                                    });
                                }
                            }

                        },
                        stop: function (e, ui) {
                            scope.$apply(function () {
                              scope.element.editor.isResizing = false;
                            });
                        }
                    });
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutContent", ["$sce", "scopeConfigurator", "environment",
        function ($sce, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        scopeConfigurator.configureForElement($scope, $element);
                        $scope.edit = function () {
                            $scope.$root.editElement($scope.element).then(function (args) {
                                $scope.$apply(function () {
                                    if (args.cancel)
                                        return;

                                    $scope.element.data = args.element.data;
                                    $scope.element.setHtml(args.element.html);
                                });
                            });
                        };

                        // Overwrite the setHtml function so that we can use the $sce service to trust the html (and not have the html binding strip certain tags).
                        $scope.element.setHtml = function (html) {
                            $scope.element.html = html;
                            $scope.element.htmlUnsafe = $sce.trustAsHtml(html);
                        };

                        $scope.element.setHtml($scope.element.html);
                    }
                ],
                templateUrl: environment.templateUrl("Content"),
                replace: true
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutHtml", ["$sce", "scopeConfigurator", "environment",
        function ($sce, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        scopeConfigurator.configureForElement($scope, $element);
                        $scope.edit = function () {
                            $scope.$root.editElement($scope.element).then(function (args) {
                                $scope.$apply(function () {
                                    if (args.cancel)
                                        return;

                                    $scope.element.data = args.element.data;
                                    $scope.element.setHtml(args.element.html);
                                });
                            });
                        };
                        $scope.updateContent = function (e) {
                            $scope.element.setHtml(e.target.innerHTML);
                        };

                        // Overwrite the setHtml function so that we can use the $sce service to trust the html (and not have the html binding strip certain tags).
                        $scope.element.setHtml = function (html) {
                            $scope.element.html = html;
                            $scope.element.htmlUnsafe = $sce.trustAsHtml(html);
                        };

                        $scope.element.setHtml($scope.element.html);
                    }
                ],
                templateUrl: environment.templateUrl("Html"),
                replace: true,
                link: function (scope, element) {
                    // Mouse down events must not be intercepted by drag and drop while inline editing is active,
                    // otherwise clicks in inline editors will have no effect.
                    element.find(".layout-content-markup").mousedown(function (e) {
                        if (scope.element.editor.inlineEditingIsActive) {
                            e.stopPropagation();
                        }
                    });
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutGrid", ["$compile", "scopeConfigurator", "environment",
        function ($compile, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        scopeConfigurator.configureForElement($scope, $element);
                        scopeConfigurator.configureForContainer($scope, $element);
                        $scope.sortableOptions["axis"] = "y";
                    }
                ],
                templateUrl: environment.templateUrl("Grid"),
                replace: true
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutRow", ["$compile", "scopeConfigurator", "environment",
        function ($compile, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        scopeConfigurator.configureForElement($scope, $element);
                        scopeConfigurator.configureForContainer($scope, $element);
                        $scope.sortableOptions["axis"] = "x";
                        $scope.sortableOptions["ui-floating"] = true;
                    }
                ],
                templateUrl: environment.templateUrl("Row"),
                replace: true
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutPopup", [
        function () {
            return {
                restrict: "A",
                link: function (scope, element, attrs) {
                    var popup = $(element);
                    var trigger = popup.closest(".layout-popup-trigger");
                    var parentElement = popup.closest(".layout-element");
                    trigger.click(function () {
                        popup.toggle();
                        if (popup.is(":visible")) {
                            popup.position({
                                my: attrs.orcLayoutPopupMy || "left top",
                                at: attrs.orcLayoutPopupAt || "left bottom+4px",
                                of: trigger
                            });
                            popup.find("input").first().focus();
                        }
                    });
                    popup.click(function (e) {
                        e.stopPropagation();
                    });
                    parentElement.click(function (e) {
                        popup.hide();
                    });
                    popup.keydown(function (e) {
                        if (!e.ctrlKey && !e.shiftKey && !e.altKey && e.which == 27) // Esc
                            popup.hide();
                        e.stopPropagation();
                    });
                    popup.on("cut copy paste", function (e) {
                        // Allow clipboard operations in popup without invoking clipboard event handlers on parent element.
                        e.stopPropagation();
                    });
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutToolbox", ["$compile", "environment",
        function ($compile, environment) {
            return {
                restrict: "E",
                controller: ["$scope", "$element",
                    function ($scope, $element) {

                        $scope.resetElements = function () {

                            $scope.gridElements = [
                                LayoutEditor.Grid.from({
                                    toolboxIcon: "\uf00a",
                                    toolboxLabel: "Grid",
                                    toolboxDescription: "Empty grid.",
                                    children: []
                                })
                            ];

                            $scope.rowElements = [
                                LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (1 column)",
                                    toolboxDescription: "Row with 1 column.",
                                    children: LayoutEditor.Column.times(1)
                                }),
                                LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (2 columns)",
                                    toolboxDescription: "Row with 2 columns.",
                                    children: LayoutEditor.Column.times(2)
                                }),
                                LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (3 columns)",
                                    toolboxDescription: "Row with 3 columns.",
                                    children: LayoutEditor.Column.times(3)
                                }),
                                LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (4 columns)",
                                    toolboxDescription: "Row with 4 columns.",
                                    children: LayoutEditor.Column.times(4)
                                }),
                                LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (6 columns)",
                                    toolboxDescription: "Row with 6 columns.",
                                    children: LayoutEditor.Column.times(6)
                                }),
                                LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (12 columns)",
                                    toolboxDescription: "Row with 12 columns.",
                                    children: LayoutEditor.Column.times(12)
                                }), LayoutEditor.Row.from({
                                    toolboxIcon: "\uf0c9",
                                    toolboxLabel: "Row (empty)",
                                    toolboxDescription: "Empty row.",
                                    children: []
                                })
                            ];

                            $scope.columnElements = [
                                LayoutEditor.Column.from({
                                    toolboxIcon: "\uf0db",
                                    toolboxLabel: "Column",
                                    toolboxDescription: "Empty column.",
                                    width: 1,
                                    offset: 0,
                                    children: []
                                })
                            ];

                            $scope.contentElementCategories = _($scope.element.config.categories).map(function (category) {
                                return {
                                    name: category.name,
                                    elements: _(category.contentTypes).map(function (contentType) {
                                        var type = contentType.type;
                                        var factory = LayoutEditor.factories[type] || LayoutEditor.factories["Content"];
                                        var item = {
                                            isTemplated: false,
                                            contentType: contentType.id,
                                            contentTypeLabel: contentType.label,
                                            contentTypeClass: contentType.typeClass,
                                            data: null,
                                            hasEditor: contentType.hasEditor,
                                            html: contentType.html
                                        };
                                        var element = factory(item);
                                        element.toolboxIcon = contentType.icon || "\uf1c9";
                                        element.toolboxLabel = contentType.label;
                                        element.toolboxDescription = contentType.description;
                                        return element;
                                    })
                                };
                            });

                        };

                        $scope.resetElements();

                        $scope.getSortableOptions = function (type) {
                            var editorId = $element.closest(".layout-editor").attr("id");
                            var parentClasses;
                            var placeholderClasses;
                            var floating = false;

                            switch (type) {
                                case "Grid":
                                    parentClasses = [".layout-canvas", ".layout-column", ".layout-common-holder"];
                                    placeholderClasses = "layout-element layout-container layout-grid ui-sortable-placeholder";
                                    break;
                                case "Row":
                                    parentClasses = [".layout-grid"];
                                    placeholderClasses = "layout-element layout-container layout-row row ui-sortable-placeholder";
                                    break;
                                case "Column":
                                    parentClasses = [".layout-row:not(.layout-row-full)"];
                                    placeholderClasses = "layout-element layout-container layout-column ui-sortable-placeholder";
                                    floating = true; // To ensure a smooth horizontal-list reordering. https://github.com/angular-ui/ui-sortable#floating
                                    break;
                                case "Content":
                                    parentClasses = [".layout-canvas", ".layout-column", ".layout-common-holder"];
                                    placeholderClasses = "layout-element layout-content ui-sortable-placeholder";
                                    break;
                            }

                            return {
                                cursor: "move",
                                connectWith: _(parentClasses).map(function (e) { return "#" + editorId + " " + e + ":not(.layout-container-sealed) > .layout-element-wrapper > .layout-children"; }).join(", "),
                                placeholder: placeholderClasses,
                                "ui-floating": floating,
                                create: function (e, ui) {
                                    e.target.isToolbox = true; // Will indicate to connected sortables that dropped items were sent from toolbox.
                                },
                                start: function (e, ui) {
                                    $scope.$apply(function () {
                                        $scope.element.isDragging = true;
                                    });
                                },
                                stop: function (e, ui) {
                                    $scope.$apply(function () {
                                        $scope.element.isDragging = false;
                                        $scope.resetElements();
                                    });
                                },
                                over: function (e, ui) {
                                    $scope.$apply(function () {
                                        $scope.element.canvas.setIsDropTarget(false);
                                    });
                                },
                            }
                        };

                        var layoutIsCollapsedCookieName = "layoutToolboxCategory_Layout_IsCollapsed";
                        $scope.layoutIsCollapsed = $.cookie(layoutIsCollapsedCookieName) === "true";

                        $scope.toggleLayoutIsCollapsed = function (e) {
                            $scope.layoutIsCollapsed = !$scope.layoutIsCollapsed;
                            $.cookie(layoutIsCollapsedCookieName, $scope.layoutIsCollapsed, { expires: 365 }); // Remember collapsed state for a year.
                            e.preventDefault();
                            e.stopPropagation();
                        };
                    }
                ],
                templateUrl: environment.templateUrl("Toolbox"),
                replace: true,
                link: function (scope, element) {
                    var toolbox = element.find(".layout-toolbox");
                    $(window).on("resize scroll", function (e) {
                        var canvas = element.parent().find(".layout-canvas");
                        // If the canvas is taller than the toolbox, make the toolbox sticky-positioned within the editor
                        // to help the user avoid excessive vertical scrolling.
                        var canvasIsTaller = !!canvas && canvas.height() > toolbox.height();
                        var windowPos = $(window).scrollTop();
                        if (canvasIsTaller && windowPos > element.offset().top + element.height() - toolbox.height()) {
                            toolbox.addClass("sticky-bottom");
                            toolbox.removeClass("sticky-top");
                        }
                        else if (canvasIsTaller && windowPos > element.offset().top) {
                            toolbox.addClass("sticky-top");
                            toolbox.removeClass("sticky-bottom");
                        }
                        else {
                            toolbox.removeClass("sticky-top");
                            toolbox.removeClass("sticky-bottom");
                        }
                    });
                }
            };
        }
    ]);
angular
    .module("LayoutEditor")
    .directive("orcLayoutToolboxGroup", ["$compile", "environment",
        function ($compile, environment) {
            return {
                restrict: "E",
                scope: { category: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        var isCollapsedCookieName = "layoutToolboxCategory_" + $scope.category.name + "_IsCollapsed";
                        $scope.isCollapsed = $.cookie(isCollapsedCookieName) === "true";
                        $scope.toggleIsCollapsed = function (e) {
                            $scope.isCollapsed = !$scope.isCollapsed;
                            $.cookie(isCollapsedCookieName, $scope.isCollapsed, { expires: 365 }); // Remember collapsed state for a year.
                            e.preventDefault();
                            e.stopPropagation();
                        };
                    }
                ],
                templateUrl: environment.templateUrl("ToolboxGroup"),
                replace: true
            };
        }
    ]);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIk1vZHVsZS5qcyIsIkNsaXBib2FyZC5qcyIsIlNjb3BlQ29uZmlndXJhdG9yLmpzIiwiRWRpdG9yLmpzIiwiQ2FudmFzLmpzIiwiQ2hpbGQuanMiLCJDb2x1bW4uanMiLCJDb250ZW50LmpzIiwiSHRtbC5qcyIsIkdyaWQuanMiLCJSb3cuanMiLCJQb3B1cC5qcyIsIlRvb2xib3guanMiLCJUb29sYm94R3JvdXAuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzdDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2pVQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDM0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDbEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2RBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNuRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDbkNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQy9DQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2xCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDbkJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDdkNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNqTUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6IkxheW91dEVkaXRvci5qcyIsInNvdXJjZXNDb250ZW50IjpbImFuZ3VsYXIubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIsIFtcIm5nU2FuaXRpemVcIiwgXCJuZ1Jlc291cmNlXCIsIFwidWkuc29ydGFibGVcIl0pOyIsInZhciBMYXlvdXRFZGl0b3I7XHJcbihmdW5jdGlvbihMYXlvdXRFZGl0b3IpIHtcclxuXHJcbiAgICB2YXIgQ2xpcGJvYXJkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgIHZhciBzZWxmID0gdGhpcztcclxuICAgICAgICB0aGlzLl9jbGlwYm9hcmREYXRhID0ge307XHJcbiAgICAgICAgdGhpcy5faXNEaXNhYmxlZCA9IGZhbHNlO1xyXG4gICAgICAgIHRoaXMuX3dhc0ludm9rZWQgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgdGhpcy5zZXREYXRhID0gZnVuY3Rpb24oY29udGVudFR5cGUsIGRhdGEpIHtcclxuICAgICAgICAgICAgc2VsZi5fY2xpcGJvYXJkRGF0YVtjb250ZW50VHlwZV0gPSBkYXRhO1xyXG4gICAgICAgICAgICBzZWxmLl93YXNJbnZva2VkID0gdHJ1ZTtcclxuICAgICAgICB9O1xyXG4gICAgICAgIHRoaXMuZ2V0RGF0YSA9IGZ1bmN0aW9uIChjb250ZW50VHlwZSkge1xyXG4gICAgICAgICAgICByZXR1cm4gc2VsZi5fY2xpcGJvYXJkRGF0YVtjb250ZW50VHlwZV07XHJcbiAgICAgICAgICAgIHNlbGYuX3dhc0ludm9rZWQgPSB0cnVlO1xyXG4gICAgICAgIH07XHJcbiAgICAgICAgdGhpcy5kaXNhYmxlID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHNlbGYuX2lzRGlzYWJsZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICBzZWxmLl93YXNJbnZva2VkID0gZmFsc2U7XHJcbiAgICAgICAgICAgIHNlbGYuX2NsaXBib2FyZERhdGEgPSB7fTtcclxuICAgICAgICB9O1xyXG4gICAgICAgIHRoaXMuaXNEaXNhYmxlZCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHNlbGYuX2lzRGlzYWJsZWQ7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIHRoaXMud2FzSW52b2tlZCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHNlbGYuX3dhc0ludm9rZWQ7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIExheW91dEVkaXRvci5DbGlwYm9hcmQgPSBuZXcgQ2xpcGJvYXJkKCk7XHJcblxyXG4gICAgYW5ndWxhclxyXG4gICAgICAgIC5tb2R1bGUoXCJMYXlvdXRFZGl0b3JcIilcclxuICAgICAgICAuZmFjdG9yeShcImNsaXBib2FyZFwiLCBbXHJcbiAgICAgICAgICAgIGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgICAgICBzZXREYXRhOiBMYXlvdXRFZGl0b3IuQ2xpcGJvYXJkLnNldERhdGEsXHJcbiAgICAgICAgICAgICAgICAgICAgZ2V0RGF0YTogTGF5b3V0RWRpdG9yLkNsaXBib2FyZC5nZXREYXRhLFxyXG4gICAgICAgICAgICAgICAgICAgIGRpc2FibGU6IExheW91dEVkaXRvci5DbGlwYm9hcmQuZGlzYWJsZSxcclxuICAgICAgICAgICAgICAgICAgICBpc0Rpc2FibGVkOiBMYXlvdXRFZGl0b3IuQ2xpcGJvYXJkLmlzRGlzYWJsZWQsXHJcbiAgICAgICAgICAgICAgICAgICAgd2FzSW52b2tlZDogTGF5b3V0RWRpdG9yLkNsaXBib2FyZC53YXNJbnZva2VkXHJcbiAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgXSk7XHJcbn0pKExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiLCJhbmd1bGFyXHJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXHJcbiAgICAuZmFjdG9yeShcInNjb3BlQ29uZmlndXJhdG9yXCIsIFtcIiR0aW1lb3V0XCIsIFwiY2xpcGJvYXJkXCIsXHJcbiAgICAgICAgZnVuY3Rpb24gKCR0aW1lb3V0LCBjbGlwYm9hcmQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuXHJcbiAgICAgICAgICAgICAgICBjb25maWd1cmVGb3JFbGVtZW50OiBmdW5jdGlvbiAoJHNjb3BlLCAkZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgXHJcbiAgICAgICAgICAgICAgICAgICAgJGVsZW1lbnQuZmluZChcIi5sYXlvdXQtcGFuZWxcIikuY2xpY2soZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZS5zdG9wUHJvcGFnYXRpb24oKTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgJGVsZW1lbnQucGFyZW50KCkua2V5ZG93bihmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB2YXIgaGFuZGxlZCA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB2YXIgcmVzZXRGb2N1cyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB2YXIgZWxlbWVudCA9ICRzY29wZS5lbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoZWxlbWVudC5lZGl0b3IuaXNEcmFnZ2luZyB8fCBlbGVtZW50LmVkaXRvci5pbmxpbmVFZGl0aW5nSXNBY3RpdmUpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBJZiBuYXRpdmUgY2xpcGJvYXJkIHN1cHBvcnQgZXhpc3RzLCB0aGUgcHNldWRvLWNsaXBib2FyZCB3aWxsIGhhdmUgYmVlbiBkaXNhYmxlZC5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCFjbGlwYm9hcmQuaXNEaXNhYmxlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgZm9jdXNlZEVsZW1lbnQgPSBlbGVtZW50LmVkaXRvci5mb2N1c2VkRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghIWZvY3VzZWRFbGVtZW50KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gUHNldWRvIGNsaXBib2FyZCBoYW5kbGluZyBmb3IgYnJvd3NlcnMgbm90IGFsbG93aW5nIHJlYWwgY2xpcGJvYXJkIG9wZXJhdGlvbnMuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGUuY3RybEtleSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzd2l0Y2ggKGUud2hpY2gpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSA2NzogLy8gQ1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9jdXNlZEVsZW1lbnQuY29weShjbGlwYm9hcmQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNhc2UgODg6IC8vIFhcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvY3VzZWRFbGVtZW50LmN1dChjbGlwYm9hcmQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNhc2UgODY6IC8vIFZcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvY3VzZWRFbGVtZW50LnBhc3RlKGNsaXBib2FyZCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCFlLmN0cmxLZXkgJiYgIWUuc2hpZnRLZXkgJiYgIWUuYWx0S2V5ICYmIGUud2hpY2ggPT0gNDYpIHsgLy8gRGVsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZGVsZXRlKGVsZW1lbnQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaGFuZGxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSBpZiAoIWUuY3RybEtleSAmJiAhZS5zaGlmdEtleSAmJiAhZS5hbHRLZXkgJiYgKGUud2hpY2ggPT0gMzIgfHwgZS53aGljaCA9PSAyNykpIHsgLy8gU3BhY2Ugb3IgRXNjXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkZWxlbWVudC5maW5kKFwiLmxheW91dC1wYW5lbC1hY3Rpb24tcHJvcGVydGllc1wiKS5maXJzdCgpLmNsaWNrKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYW5kbGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGVsZW1lbnQudHlwZSA9PSBcIkNvbnRlbnRcIikgeyAvLyBUaGlzIGlzIGEgY29udGVudCBlbGVtZW50LlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCFlLmN0cmxLZXkgJiYgIWUuc2hpZnRLZXkgJiYgIWUuYWx0S2V5ICYmIGUud2hpY2ggPT0gMTMpIHsgLy8gRW50ZXJcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkZWxlbWVudC5maW5kKFwiLmxheW91dC1wYW5lbC1hY3Rpb24tZWRpdFwiKS5maXJzdCgpLmNsaWNrKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaGFuZGxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICghIWVsZW1lbnQuY2hpbGRyZW4pIHsgLy8gVGhpcyBpcyBhIGNvbnRhaW5lci5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghZS5jdHJsS2V5ICYmICFlLnNoaWZ0S2V5ICYmIGUuYWx0S2V5ICYmIGUud2hpY2ggPT0gNDApIHsgLy8gQWx0K0Rvd25cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZWxlbWVudC5jaGlsZHJlbi5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LmNoaWxkcmVuWzBdLnNldElzRm9jdXNlZCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGhhbmRsZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlbGVtZW50LnR5cGUgPT0gXCJDb2x1bW5cIikgeyAvLyBUaGlzIGlzIGEgY29sdW1uLlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBjb25uZWN0QWRqYWNlbnQgPSAhZS5jdHJsS2V5O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlLndoaWNoID09IDM3KSB7IC8vIExlZnRcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGUuYWx0S2V5KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5leHBhbmRMZWZ0KGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlLnNoaWZ0S2V5KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5jb250cmFjdFJpZ2h0KGNvbm5lY3RBZGphY2VudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGhhbmRsZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSBpZiAoZS53aGljaCA9PSAzOSkgeyAvLyBSaWdodFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZS5hbHRLZXkpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LmNvbnRyYWN0TGVmdChjb25uZWN0QWRqYWNlbnQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZS5zaGlmdEtleSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnQuZXhwYW5kUmlnaHQoY29ubmVjdEFkamFjZW50KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaGFuZGxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoISFlbGVtZW50LnBhcmVudCkgeyAvLyBUaGlzIGlzIGEgY2hpbGQuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZS5hbHRLZXkgJiYgZS53aGljaCA9PSAzOCkgeyAvLyBBbHQrVXBcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnBhcmVudC5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYW5kbGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoZWxlbWVudC5wYXJlbnQudHlwZSA9PSBcIlJvd1wiKSB7IC8vIFBhcmVudCBpcyBhIGhvcml6b250YWwgY29udGFpbmVyLlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghZS5jdHJsS2V5ICYmICFlLnNoaWZ0S2V5ICYmICFlLmFsdEtleSAmJiBlLndoaWNoID09IDM3KSB7IC8vIExlZnRcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5wYXJlbnQubW92ZUZvY3VzUHJldkNoaWxkKGVsZW1lbnQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYW5kbGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSBpZiAoIWUuY3RybEtleSAmJiAhZS5zaGlmdEtleSAmJiAhZS5hbHRLZXkgJiYgZS53aGljaCA9PSAzOSkgeyAvLyBSaWdodFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnBhcmVudC5tb3ZlRm9jdXNOZXh0Q2hpbGQoZWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGhhbmRsZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbHNlIGlmIChlLmN0cmxLZXkgJiYgIWUuc2hpZnRLZXkgJiYgIWUuYWx0S2V5ICYmIGUud2hpY2ggPT0gMzcpIHsgLy8gQ3RybCtMZWZ0XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnQubW92ZVVwKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc2V0Rm9jdXMgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYW5kbGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSBpZiAoZS5jdHJsS2V5ICYmICFlLnNoaWZ0S2V5ICYmICFlLmFsdEtleSAmJiBlLndoaWNoID09IDM5KSB7IC8vIEN0cmwrUmlnaHRcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5tb3ZlRG93bigpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYW5kbGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbHNlIHsgLy8gUGFyZW50IGlzIGEgdmVydGljYWwgY29udGFpbmVyLlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghZS5jdHJsS2V5ICYmICFlLnNoaWZ0S2V5ICYmICFlLmFsdEtleSAmJiBlLndoaWNoID09IDM4KSB7IC8vIFVwXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnQucGFyZW50Lm1vdmVGb2N1c1ByZXZDaGlsZChlbGVtZW50KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaGFuZGxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsc2UgaWYgKCFlLmN0cmxLZXkgJiYgIWUuc2hpZnRLZXkgJiYgIWUuYWx0S2V5ICYmIGUud2hpY2ggPT0gNDApIHsgLy8gRG93blxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnBhcmVudC5tb3ZlRm9jdXNOZXh0Q2hpbGQoZWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGhhbmRsZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbHNlIGlmIChlLmN0cmxLZXkgJiYgIWUuc2hpZnRLZXkgJiYgIWUuYWx0S2V5ICYmIGUud2hpY2ggPT0gMzgpIHsgLy8gQ3RybCtVcFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50Lm1vdmVVcCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXNldEZvY3VzID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaGFuZGxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsc2UgaWYgKGUuY3RybEtleSAmJiAhZS5zaGlmdEtleSAmJiAhZS5hbHRLZXkgJiYgZS53aGljaCA9PSA0MCkgeyAvLyBDdHJsK0Rvd25cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5tb3ZlRG93bigpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYW5kbGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChoYW5kbGVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJGFwcGx5KCk7IC8vIEV2ZW50IGlzIG5vdCB0cmlnZ2VyZWQgYnkgQW5ndWxhciBkaXJlY3RpdmUgYnV0IHJhdyBldmVudCBoYW5kbGVyIG9uIGVsZW1lbnQuXHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBIQUNLOiBXb3JrYXJvdW5kIGJlY2F1c2Ugb2YgaG93IEFuZ3VsYXIgdHJlYXRzIHRoZSBET00gd2hlbiBlbGVtZW50cyBhcmUgc2hpZnRlZCBhcm91bmQgLSBpbnB1dCBmb2N1cyBpcyBzb21ldGltZXMgbG9zdC5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHJlc2V0Rm9jdXMpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdpbmRvdy5zZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJGFwcGx5KGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5lZGl0b3IuZm9jdXNlZEVsZW1lbnQuc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9LCAxMDApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICRzY29wZS5lbGVtZW50LnNldElzRm9jdXNlZEV2ZW50SGFuZGxlcnMucHVzaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICRlbGVtZW50LnBhcmVudCgpLmZvY3VzKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICRzY29wZS5kZWxldGUgPSBmdW5jdGlvbiAoZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LmRlbGV0ZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0sXHJcblxyXG4gICAgICAgICAgICAgICAgY29uZmlndXJlRm9yQ29udGFpbmVyOiBmdW5jdGlvbiAoJHNjb3BlLCAkZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBlbGVtZW50ID0gJHNjb3BlLmVsZW1lbnQ7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIC8vJHNjb3BlLmlzUmVjZWl2aW5nID0gZmFsc2U7IC8vIFRydWUgd2hlbiBjb250YWluZXIgaXMgcmVjZWl2aW5nIGFuIGV4dGVybmFsIGVsZW1lbnQgdmlhIGRyYWcvZHJvcC5cclxuICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZ2V0U2hvd0NoaWxkcmVuUGxhY2Vob2xkZXIgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAkc2NvcGUuZWxlbWVudC5jaGlsZHJlbi5sZW5ndGggPT09IDAgJiYgISRzY29wZS5lbGVtZW50LmdldElzRHJvcFRhcmdldCgpO1xyXG4gICAgICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICRzY29wZS5zb3J0YWJsZU9wdGlvbnMgPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGN1cnNvcjogXCJtb3ZlXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRlbGF5OiAxNTAsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRpc2FibGVkOiBlbGVtZW50LmdldElzU2VhbGVkKCksXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRpc3RhbmNlOiA1LFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAvL2hhbmRsZTogZWxlbWVudC5jaGlsZHJlbi5sZW5ndGggPCAyID8gXCIuaW1hZ2luYXJ5LWNsYXNzXCIgOiBmYWxzZSwgLy8gRm9yIHNvbWUgcmVhc29uIGRvZXNuJ3QgZ2V0IHJlLWV2YWx1YXRlZCBhZnRlciBhZGRpbmcgbW9yZSBjaGlsZHJlbi5cclxuICAgICAgICAgICAgICAgICAgICAgICAgc3RhcnQ6IGZ1bmN0aW9uIChlLCB1aSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5zZXRJc0Ryb3BUYXJnZXQodHJ1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5lZGl0b3IuaXNEcmFnZ2luZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIE1ha2UgdGhlIGRyb3AgdGFyZ2V0IHBsYWNlaG9sZGVyIGFzIGhpZ2ggYXMgdGhlIGl0ZW0gYmVpbmcgZHJhZ2dlZC5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHVpLnBsYWNlaG9sZGVyLmhlaWdodCh1aS5pdGVtLmhlaWdodCgpIC0gNCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB1aS5wbGFjZWhvbGRlci5jc3MoXCJtaW4taGVpZ2h0XCIsIDApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBzdG9wOiBmdW5jdGlvbiAoZSwgdWkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kYXBwbHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnQuZWRpdG9yLmlzRHJhZ2dpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnNldElzRHJvcFRhcmdldChmYWxzZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgb3ZlcjogZnVuY3Rpb24gKGUsIHVpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISF1aS5zZW5kZXIgJiYgISF1aS5zZW5kZXJbMF0uaXNUb29sYm94KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCEhdWkuc2VuZGVyWzBdLmRyb3BUYXJnZXRUaW1lb3V0KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICR0aW1lb3V0LmNhbmNlbCh1aS5zZW5kZXJbMF0uZHJvcFRhcmdldFRpbWVvdXQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB1aS5zZW5kZXJbMF0uZHJvcFRhcmdldFRpbWVvdXQgPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkdGltZW91dChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlbGVtZW50LnR5cGUgPT0gXCJSb3dcIikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gSWYgdGhlcmUgd2FzIGEgcHJldmlvdXMgZHJvcCB0YXJnZXQgYW5kIGl0IHdhcyBhIHJvdywgcm9sbCBiYWNrIGFueSBwZW5kaW5nIGNvbHVtbiBhZGRzIHRvIGl0LlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHByZXZpb3VzRHJvcFRhcmdldCA9IGVsZW1lbnQuZWRpdG9yLmRyb3BUYXJnZXRFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCEhcHJldmlvdXNEcm9wVGFyZ2V0ICYmIHByZXZpb3VzRHJvcFRhcmdldC50eXBlID09IFwiUm93XCIpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJldmlvdXNEcm9wVGFyZ2V0LnJvbGxiYWNrQWRkQ29sdW1uKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5zZXRJc0Ryb3BUYXJnZXQoZmFsc2UpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHVpLnNlbmRlclswXS5kcm9wVGFyZ2V0VGltZW91dCA9ICR0aW1lb3V0KGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGVsZW1lbnQudHlwZSA9PSBcIlJvd1wiKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgcmVjZWl2ZWRDb2x1bW4gPSB1aS5pdGVtLnNvcnRhYmxlLm1vZGVsO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHJlY2VpdmVkQ29sdW1uV2lkdGggPSBNYXRoLmZsb29yKDEyIC8gKGVsZW1lbnQuY2hpbGRyZW4ubGVuZ3RoICsgMSkpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZWRDb2x1bW4ud2lkdGggPSByZWNlaXZlZENvbHVtbldpZHRoO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZWRDb2x1bW4ub2Zmc2V0ID0gMDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnQuYmVnaW5BZGRDb2x1bW4ocmVjZWl2ZWRDb2x1bW5XaWR0aCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBNYWtlIHRoZSBkcm9wIHRhcmdldCBwbGFjZWhvbGRlciB0aGUgY29ycmVjdCB3aWR0aCBhbmQgYXMgaGlnaCBhcyB0aGUgaGlnaGVzdCBleGlzdGluZyBjb2x1bW4gaW4gdGhlIHJvdy5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBtYXhIZWlnaHQgPSBfLm1heChfKCRlbGVtZW50LmZpbmQoXCI+IC5sYXlvdXQtY2hpbGRyZW4gPiAubGF5b3V0LWNvbHVtbjpub3QoLnVpLXNvcnRhYmxlLXBsYWNlaG9sZGVyKVwiKSkubWFwKGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuICQoZSkuaGVpZ2h0KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb3IgKGkgPSAxOyBpIDw9IDEyOyBpKyspXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdWkucGxhY2Vob2xkZXIucmVtb3ZlQ2xhc3MoXCJjb2wteHMtXCIgKyBpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHVpLnBsYWNlaG9sZGVyLmFkZENsYXNzKFwiY29sLXhzLVwiICsgcmVjZWl2ZWRDb2x1bW4ud2lkdGgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKG1heEhlaWdodCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB1aS5wbGFjZWhvbGRlci5oZWlnaHQobWF4SGVpZ2h0KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB1aS5wbGFjZWhvbGRlci5jc3MoXCJtaW4taGVpZ2h0XCIsIDApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdWkucGxhY2Vob2xkZXIuaGVpZ2h0KDApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHVpLnBsYWNlaG9sZGVyLmNzcyhcIm1pbi1oZWlnaHRcIiwgXCJcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5zZXRJc0Ryb3BUYXJnZXQodHJ1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSwgMTUwKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZTogZnVuY3Rpb24gKGUsIHVpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISF1aS5zZW5kZXIgJiYgISF1aS5zZW5kZXJbMF0uaXNUb29sYm94KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciByZWNlaXZlZEVsZW1lbnQgPSB1aS5pdGVtLnNvcnRhYmxlLm1vZGVsO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISFyZWNlaXZlZEVsZW1lbnQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlbGVtZW50LnR5cGUgPT0gXCJSb3dcIilcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LmNvbW1pdEFkZENvbHVtbigpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gU2hvdWxkIGlkZWFsbHkgY2FsbCBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmFkZENoaWxkKCkgaW5zdGVhZCwgYnV0IHNpbmNlIHRoaXMgaGFuZGxlclxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gaXMgcnVuICpiZWZvcmUqIHRoZSB1aS1zb3J0YWJsZSBkaXJlY3RpdmUncyBoYW5kbGVyLCBpZiB3ZSB0cnkgdG8gYWRkIHRoZSBjaGlsZCB0byB0aGVcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIGFycmF5IHRoYXQgaGFuZGxlciB3aWxsIGdldCBhbiBleGNlcHRpb24gd2hlbiB0cnlpbmcgdG8gZG8gdGhlIHNhbWUuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBCZWNhdXNlIG9mIHRoaXMsIHdlIG5lZWQgdG8gaW52b2tlIFwic2V0UGFyZW50XCIgc28gdGhhdCBzcGVjaWZpYyBjb250YWluZXIgdHlwZXMgY2FuIHBlcmZvcm0gZWxlbWVudCBzcGVmaWNpYyBpbml0aWFsaXphdGlvbi5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlY2VpdmVkRWxlbWVudC5zZXRFZGl0b3IoZWxlbWVudC5lZGl0b3IpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZWRFbGVtZW50LnNldFBhcmVudChlbGVtZW50KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISFyZWNlaXZlZEVsZW1lbnQuaGFzRWRpdG9yKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLiRyb290LmVkaXRFbGVtZW50KHJlY2VpdmVkRWxlbWVudCkudGhlbihmdW5jdGlvbiAoYXJncykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoIWFyZ3MuY2FuY2VsKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZWNlaXZlZEVsZW1lbnQuZGF0YSA9IGFyZ3MuZWxlbWVudC5kYXRhO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZWRFbGVtZW50LmFwcGx5RWxlbWVudEVkaXRvck1vZGVsKGFyZ3MuZWxlbWVudEVkaXRvck1vZGVsKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISFyZWNlaXZlZEVsZW1lbnQuc2V0SHRtbClcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZWNlaXZlZEVsZW1lbnQuc2V0SHRtbChhcmdzLmVsZW1lbnQuaHRtbCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHRpbWVvdXQoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCEhYXJncy5jYW5jZWwpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZWRFbGVtZW50LmRlbGV0ZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlY2VpdmVkRWxlbWVudC5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vJHNjb3BlLmlzUmVjZWl2aW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnNldElzRHJvcFRhcmdldChmYWxzZSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICR0aW1lb3V0KGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vJHNjb3BlLmlzUmVjZWl2aW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnNldElzRHJvcFRhcmdldChmYWxzZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISFyZWNlaXZlZEVsZW1lbnQpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVjZWl2ZWRFbGVtZW50LnNldElzRm9jdXNlZCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICRzY29wZS5jbGljayA9IGZ1bmN0aW9uIChjaGlsZCwgZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoIWNoaWxkLmVkaXRvci5pc0RyYWdnaW5nKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGQuc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmdldENsYXNzZXMgPSBmdW5jdGlvbiAoY2hpbGQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHJlc3VsdCA9IFtcImxheW91dC1lbGVtZW50XCJdO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCEhY2hpbGQuY2hpbGRyZW4pIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKFwibGF5b3V0LWNvbnRhaW5lclwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChjaGlsZC5nZXRJc1NlYWxlZCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKFwibGF5b3V0LWNvbnRhaW5lci1zZWFsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKFwibGF5b3V0LVwiICsgY2hpbGQudHlwZS50b0xvd2VyQ2FzZSgpKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICghIWNoaWxkLmRyb3BUYXJnZXRDbGFzcylcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKGNoaWxkLmRyb3BUYXJnZXRDbGFzcyk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBUT0RPOiBNb3ZlIHRoZXNlIHRvIGVpdGhlciB0aGUgQ29sdW1uIGRpcmVjdGl2ZSBvciB0aGUgQ29sdW1uIG1vZGVsIGNsYXNzLlxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY2hpbGQudHlwZSA9PSBcIlJvd1wiKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXN1bHQucHVzaChcInJvd1wiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghY2hpbGQuY2FuQWRkQ29sdW1uKCkpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtcm93LWZ1bGxcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNoaWxkLnR5cGUgPT0gXCJDb2x1bW5cIikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJjb2wteHMtXCIgKyBjaGlsZC53aWR0aCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXN1bHQucHVzaChcImNvbC14cy1vZmZzZXQtXCIgKyBjaGlsZC5vZmZzZXQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChjaGlsZC50eXBlID09IFwiQ29udGVudFwiKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtY29udGVudC1cIiArIGNoaWxkLmNvbnRlbnRUeXBlQ2xhc3MpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzQWN0aXZlKCkpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXN1bHQucHVzaChcImxheW91dC1lbGVtZW50LWFjdGl2ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzRm9jdXNlZCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtZWxlbWVudC1mb2N1c2VkXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY2hpbGQuZ2V0SXNTZWxlY3RlZCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtZWxlbWVudC1zZWxlY3RlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNoaWxkLmdldElzRHJvcFRhcmdldCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtZWxlbWVudC1kcm9wdGFyZ2V0XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY2hpbGQuaXNUZW1wbGF0ZWQpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXN1bHQucHVzaChcImxheW91dC1lbGVtZW50LXRlbXBsYXRlZFwiKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9XHJcbiAgICBdKTsiLCJhbmd1bGFyXHJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXHJcbiAgICAuZGlyZWN0aXZlKFwib3JjTGF5b3V0RWRpdG9yXCIsIFtcImVudmlyb25tZW50XCIsXHJcbiAgICAgICAgZnVuY3Rpb24gKGVudmlyb25tZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICByZXN0cmljdDogXCJFXCIsXHJcbiAgICAgICAgICAgICAgICBzY29wZToge30sXHJcbiAgICAgICAgICAgICAgICBjb250cm9sbGVyOiBbXCIkc2NvcGVcIiwgXCIkZWxlbWVudFwiLCBcIiRhdHRyc1wiLCBcIiRjb21waWxlXCIsIFwiY2xpcGJvYXJkXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKCRzY29wZSwgJGVsZW1lbnQsICRhdHRycywgJGNvbXBpbGUsIGNsaXBib2FyZCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoISEkYXR0cnMubW9kZWwpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudCA9IGV2YWwoJGF0dHJzLm1vZGVsKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiVGhlICdtb2RlbCcgYXR0cmlidXRlIG11c3QgZXZhbHVhdGUgdG8gYSBMYXlvdXRFZGl0b3IuRWRpdG9yIG9iamVjdC5cIik7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuY2xpY2sgPSBmdW5jdGlvbiAoY2FudmFzLCBlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoIWNhbnZhcy5lZGl0b3IuaXNEcmFnZ2luZylcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjYW52YXMuc2V0SXNGb2N1c2VkKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnN0b3BQcm9wYWdhdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmdldENsYXNzZXMgPSBmdW5jdGlvbiAoY2FudmFzKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgcmVzdWx0ID0gW1wibGF5b3V0LWVsZW1lbnRcIiwgXCJsYXlvdXQtY29udGFpbmVyXCIsIFwibGF5b3V0LWNhbnZhc1wiXTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoY2FudmFzLmdldElzQWN0aXZlKCkpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtZWxlbWVudC1hY3RpdmVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoY2FudmFzLmdldElzRm9jdXNlZCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKFwibGF5b3V0LWVsZW1lbnQtZm9jdXNlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChjYW52YXMuZ2V0SXNTZWxlY3RlZCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKFwibGF5b3V0LWVsZW1lbnQtc2VsZWN0ZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoY2FudmFzLmdldElzRHJvcFRhcmdldCgpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJlc3VsdC5wdXNoKFwibGF5b3V0LWVsZW1lbnQtZHJvcHRhcmdldFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChjYW52YXMuaXNUZW1wbGF0ZWQpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVzdWx0LnB1c2goXCJsYXlvdXQtZWxlbWVudC10ZW1wbGF0ZWRcIik7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC8vIEFuIHVuZm9ydHVuYXRlIHNpZGUtZWZmZWN0IG9mIHRoZSBuZXh0IGhhY2sgb24gbGluZSA1NCBpcyB0aGF0IHRoZSBjcmVhdGVkIGVsZW1lbnRzIGFyZW4ndCBhZGRlZCB0byB0aGUgRE9NIHlldCwgc28gd2UgY2FuJ3QgdXNlIGl0IHRvIGdldCB0byB0aGUgcGFyZW50IFwiLmxheW91dC1kZXNpZ2VyXCIgZWxlbWVudC5cclxuICAgICAgICAgICAgICAgICAgICAgICAgLy8gV29yayBhcm91bmQ6IGFjY2VzcyB0aGF0IGVsZW1lbnQgZGlyZWN0bHkgKHdoaWNoIGVmZWN0aXZlbHkgdHVybnMgbXVsdGlwbGUgbGF5b3V0IGVkaXRvcnMgb24gYSBzaW5nbGUgcGFnZSBpbXBvc3NpYmxlKS4gXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC8vIC8vdmFyIGxheW91dERlc2lnbmVySG9zdCA9ICRlbGVtZW50LmNsb3Nlc3QoXCIubGF5b3V0LWRlc2lnbmVyXCIpLmRhdGEoXCJsYXlvdXQtZGVzaWduZXItaG9zdFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGxheW91dERlc2lnbmVySG9zdCA9ICQoXCIubGF5b3V0LWRlc2lnbmVyXCIpLmRhdGEoXCJsYXlvdXQtZGVzaWduZXItaG9zdFwiKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kcm9vdC5sYXlvdXREZXNpZ25lckhvc3QgPSBsYXlvdXREZXNpZ25lckhvc3Q7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBsYXlvdXREZXNpZ25lckhvc3QuZWxlbWVudC5vbihcInJlcGxhY2VjYW52YXNcIiwgZnVuY3Rpb24gKGUsIGFyZ3MpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBlZGl0b3IgPSAkc2NvcGUuZWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBjYW52YXNEYXRhID0ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRhdGE6IGFyZ3MuY2FudmFzLmRhdGEsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaHRtbElkOiBhcmdzLmNhbnZhcy5odG1sSWQsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaHRtbENsYXNzOiBhcmdzLmNhbnZhcy5odG1sQ2xhc3MsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaHRtbFN0eWxlOiBhcmdzLmNhbnZhcy5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaXNUZW1wbGF0ZWQ6IGFyZ3MuY2FudmFzLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNoaWxkcmVuOiBhcmdzLmNhbnZhcy5jaGlsZHJlblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBIQUNLOiBJbnN0ZWFkIG9mIHNpbXBseSB1cGRhdGluZyB0aGUgJHNjb3BlLmVsZW1lbnQgd2l0aCBhIG5ldyBpbnN0YW5jZSwgd2UgbmVlZCB0byByZXBsYWNlIHRoZSBlbnRpcmUgb3JjLWxheW91dC1lZGl0b3IgbWFya3VwXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBpbiBvcmRlciBmb3IgYW5ndWxhciB0byByZWJpbmQgc3RhcnRpbmcgd2l0aCB0aGUgQ2FudmFzIGVsZW1lbnQuIE90aGVyd2lzZSwgZm9yIHNvbWUgcmVhc29uLCBpdCB3aWxsIHJlYmluZCBzdGFydGluZyB3aXRoIHRoZSBmaXJzdCBjaGlsZCBvZiBDYW52YXMuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBZb3UgY2FuIHNlZSB0aGlzIGhhcHBlbmluZyB3aGVuIHNldHRpbmcgYSBicmVha3BvaW50IGluIFNjb3BlQ29uZmlndXJhdG9yIHdoZXJlIGNvbnRhaW5lcnMgYXJlIGluaXRpYWxpemVkIHdpdGggZHJhZyAmIGRyb3A6IG9uIHBhZ2UgbG9hZCwgdGhlIGZpcnN0IGVsZW1lbnRcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIGlzIGEgQ2FudmFzIChnb29kKSwgYnV0IGFmdGVyIGhhdmluZyBzZWxlY3RlZCBhbm90aGVyIHRlbXBsYXRlLCB0aGUgZmlyc3QgZWxlbWVudCBpcyAodHlwaWNhbGx5KSBhIEdyaWQgKGJhZCkuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBTaW1wbHkgcmVjb21waWxpbmcgdGhlIG9yYy1sYXlvdXQtZWRpdG9yIGRpcmVjdGl2ZSB3aWxsIGNhdXNlIHRoZSBlbnRpcmUgdGhpbmcgdG8gYmUgZ2VuZXJhdGVkLCB3aGljaCB3b3JrcyBqdXN0IGZpbmUgYXMgd2VsbCAoZXZlbiB0aG91Z2ggbm90IGlzIG5pY2UgYXMgc2ltcGx5IGxldmVyYWdpbmcgbW9kZWwgYmluZGluZykuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsYXlvdXREZXNpZ25lckhvc3QuZWRpdG9yID0gd2luZG93LmxheW91dEVkaXRvciA9IG5ldyBMYXlvdXRFZGl0b3IuRWRpdG9yKGVkaXRvci5jb25maWcsIGNhbnZhc0RhdGEpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHRlbXBsYXRlID0gXCI8b3JjLWxheW91dC1lZGl0b3JcIiArIFwiIG1vZGVsPSd3aW5kb3cubGF5b3V0RWRpdG9yJyAvPlwiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGh0bWwgPSAkY29tcGlsZSh0ZW1wbGF0ZSkoJHNjb3BlKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICQoXCIubGF5b3V0LWVkaXRvci1ob2xkZXJcIikuaHRtbChodG1sKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJHJvb3QuZWRpdEVsZW1lbnQgPSBmdW5jdGlvbiAoZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGhvc3QgPSAkc2NvcGUuJHJvb3QubGF5b3V0RGVzaWduZXJIb3N0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGhvc3QuZWRpdEVsZW1lbnQoZWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJHJvb3QuYWRkRWxlbWVudCA9IGZ1bmN0aW9uIChjb250ZW50VHlwZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGhvc3QgPSAkc2NvcGUuJHJvb3QubGF5b3V0RGVzaWduZXJIb3N0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGhvc3QuYWRkRWxlbWVudChjb250ZW50VHlwZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUudG9nZ2xlSW5saW5lRWRpdGluZyA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghJHNjb3BlLmVsZW1lbnQuaW5saW5lRWRpdGluZ0lzQWN0aXZlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuaW5saW5lRWRpdGluZ0lzQWN0aXZlID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkZWxlbWVudC5maW5kKFwiLmxheW91dC10b29sYmFyLWNvbnRhaW5lclwiKS5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHNlbGVjdG9yID0gXCIjbGF5b3V0LWVkaXRvci1cIiArICRzY29wZS4kaWQgKyBcIiAubGF5b3V0LWh0bWwgLmxheW91dC1jb250ZW50LW1hcmt1cFtkYXRhLXRlbXBsYXRlZD1mYWxzZV1cIjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgZmlyc3RDb250ZW50RWRpdG9ySWQgPSAkKHNlbGVjdG9yKS5maXJzdCgpLmF0dHIoXCJpZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aW55bWNlLmluaXQoe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Rvcjogc2VsZWN0b3IsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoZW1lOiBcIm1vZGVyblwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY2hlbWE6IFwiaHRtbDVcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGx1Z2luczogW1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgXCJhZHZsaXN0IGF1dG9saW5rIGxpc3RzIGxpbmsgaW1hZ2UgY2hhcm1hcCBwcmludCBwcmV2aWV3IGhyIGFuY2hvciBwYWdlYnJlYWtcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIFwic2VhcmNocmVwbGFjZSB3b3JkY291bnQgdmlzdWFsYmxvY2tzIHZpc3VhbGNoYXJzIGNvZGUgZnVsbHNjcmVlblwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgXCJpbnNlcnRkYXRldGltZSBtZWRpYSBub25icmVha2luZyB0YWJsZSBjb250ZXh0bWVudSBkaXJlY3Rpb25hbGl0eVwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgXCJlbW90aWNvbnMgdGVtcGxhdGUgcGFzdGUgdGV4dGNvbG9yIGNvbG9ycGlja2VyIHRleHRwYXR0ZXJuXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBcImZ1bGxzY3JlZW4gYXV0b3Jlc2l6ZVwiXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIF0sXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xiYXI6IFwidW5kbyByZWRvIGN1dCBjb3B5IHBhc3RlIHwgYm9sZCBpdGFsaWMgfCBidWxsaXN0IG51bWxpc3Qgb3V0ZGVudCBpbmRlbnQgZm9ybWF0c2VsZWN0IHwgYWxpZ25sZWZ0IGFsaWduY2VudGVyIGFsaWducmlnaHQgYWxpZ25qdXN0aWZ5IGx0ciBydGwgfCBsaW5rIHVubGluayBjaGFybWFwIHwgY29kZSBmdWxsc2NyZWVuIGNsb3NlXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnZlcnRfdXJsczogZmFsc2UsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhbGlkX2VsZW1lbnRzOiBcIipbKl1cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gU2hvdWxkbid0IGJlIG5lZWRlZCBkdWUgdG8gdGhlIHZhbGlkX2VsZW1lbnRzIHNldHRpbmcsIGJ1dCBUaW55TUNFIHdvdWxkIHN0cmlwIHNjcmlwdC5zcmMgd2l0aG91dCBpdC5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZXh0ZW5kZWRfdmFsaWRfZWxlbWVudHM6IFwic2NyaXB0W3R5cGV8ZGVmZXJ8c3JjfGxhbmd1YWdlXVwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzdGF0dXNiYXI6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBza2luOiBcIm9yY2hhcmRsaWdodGdyYXlcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaW5saW5lOiB0cnVlLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmaXhlZF90b29sYmFyX2NvbnRhaW5lcjogXCIjbGF5b3V0LWVkaXRvci1cIiArICRzY29wZS4kaWQgKyBcIiAubGF5b3V0LXRvb2xiYXItY29udGFpbmVyXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGluaXRfaW5zdGFuY2VfY2FsbGJhY2s6IGZ1bmN0aW9uIChlZGl0b3IpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChlZGl0b3IuaWQgPT0gZmlyc3RDb250ZW50RWRpdG9ySWQpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGlueW1jZS5leGVjQ29tbWFuZChcIm1jZUZvY3VzXCIsIGZhbHNlLCBlZGl0b3IuaWQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aW55bWNlLnJlbW92ZShcIiNsYXlvdXQtZWRpdG9yLVwiICsgJHNjb3BlLiRpZCArIFwiIC5sYXlvdXQtY29udGVudC1tYXJrdXBcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJGVsZW1lbnQuZmluZChcIi5sYXlvdXQtdG9vbGJhci1jb250YWluZXJcIikuaGlkZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lbGVtZW50LmlubGluZUVkaXRpbmdJc0FjdGl2ZSA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgJChkb2N1bWVudCkub24oXCJjdXQgY29weSBwYXN0ZVwiLCBmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gSWYgdGhlIHBzZXVkbyBjbGlwYm9hcmQgd2FzIGFscmVhZHkgaW52b2tlZCAod2hpY2ggaGFwcGVucyBvbiB0aGUgZmlyc3QgY2xpcGJvYXJkXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBvcGVyYXRpb24gYWZ0ZXIgcGFnZSBsb2FkIGV2ZW4gaWYgbmF0aXZlIGNsaXBib2FyZCBzdXBwb3J0IGV4aXN0cykgdGhlbiBzaXQgdGhpc1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gb25lIG9wZXJhdGlvbiBvdXQsIGJ1dCBtYWtlIHN1cmUgd2hhdGV2ZXIgaXMgb24gdGhlIHBzZXVkbyBjbGlwYm9hcmQgZ2V0cyBtaWdyYXRlZFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gdG8gdGhlIG5hdGl2ZSBjbGlwYm9hcmQgZm9yIHN1YnNlcXVlbnQgb3BlcmF0aW9ucy5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChjbGlwYm9hcmQud2FzSW52b2tlZCgpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZS5vcmlnaW5hbEV2ZW50LmNsaXBib2FyZERhdGEuc2V0RGF0YShcInRleHQvcGxhaW5cIiwgY2xpcGJvYXJkLmdldERhdGEoXCJ0ZXh0L3BsYWluXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLm9yaWdpbmFsRXZlbnQuY2xpcGJvYXJkRGF0YS5zZXREYXRhKFwidGV4dC9qc29uXCIsIGNsaXBib2FyZC5nZXREYXRhKFwidGV4dC9qc29uXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgZm9jdXNlZEVsZW1lbnQgPSAkc2NvcGUuZWxlbWVudC5mb2N1c2VkRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoISFmb2N1c2VkRWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJGFwcGx5KGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHN3aXRjaCAoZS50eXBlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBcImNvcHlcIjpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9jdXNlZEVsZW1lbnQuY29weShlLm9yaWdpbmFsRXZlbnQuY2xpcGJvYXJkRGF0YSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNhc2UgXCJjdXRcIjpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9jdXNlZEVsZW1lbnQuY3V0KGUub3JpZ2luYWxFdmVudC5jbGlwYm9hcmREYXRhKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBcInBhc3RlXCI6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvY3VzZWRFbGVtZW50LnBhc3RlKGUub3JpZ2luYWxFdmVudC5jbGlwYm9hcmREYXRhKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gSEFDSzogV29ya2Fyb3VuZCBiZWNhdXNlIG9mIGhvdyBBbmd1bGFyIHRyZWF0cyB0aGUgRE9NIHdoZW4gZWxlbWVudHMgYXJlIHNoaWZ0ZWQgYXJvdW5kIC0gaW5wdXQgZm9jdXMgaXMgc29tZXRpbWVzIGxvc3QuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdpbmRvdy5zZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kYXBwbHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghISRzY29wZS5lbGVtZW50LmZvY3VzZWRFbGVtZW50KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5mb2N1c2VkRWxlbWVudC5zZXRJc0ZvY3VzZWQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9LCAxMDApO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBOYXRpdmUgY2xpcGJvYXJkIHN1cHBvcnQgb2J2aW91c2x5IGV4aXN0cywgc28gZGlzYWJsZSB0aGUgcGV1ZG8gY2xpcGJvYXJkIGZyb20gbm93IG9uLlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY2xpcGJvYXJkLmRpc2FibGUoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgXSxcclxuICAgICAgICAgICAgICAgIHRlbXBsYXRlVXJsOiBlbnZpcm9ubWVudC50ZW1wbGF0ZVVybChcIkVkaXRvclwiKSxcclxuICAgICAgICAgICAgICAgIHJlcGxhY2U6IHRydWUsXHJcbiAgICAgICAgICAgICAgICBsaW5rOiBmdW5jdGlvbiAoc2NvcGUsIGVsZW1lbnQpIHtcclxuICAgICAgICAgICAgICAgICAgICAvLyBObyBjbGlja3Mgc2hvdWxkIHByb3BhZ2F0ZSBmcm9tIHRoZSBUaW55TUNFIHRvb2xiYXJzLlxyXG4gICAgICAgICAgICAgICAgICAgIGVsZW1lbnQuZmluZChcIi5sYXlvdXQtdG9vbGJhci1jb250YWluZXJcIikuY2xpY2soZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZS5zdG9wUHJvcGFnYXRpb24oKTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAvLyBJbnRlcmNlcHQgbW91c2Vkb3duIG9uIGVkaXRvciB3aGlsZSBpbiBpbmxpbmUgZWRpdGluZyBtb2RlIHRvIFxyXG4gICAgICAgICAgICAgICAgICAgIC8vIHByZXZlbnQgY3VycmVudCBlZGl0b3IgZnJvbSBsb3NpbmcgZm9jdXMuXHJcbiAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5tb3VzZWRvd24oZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHNjb3BlLmVsZW1lbnQuaW5saW5lRWRpdGluZ0lzQWN0aXZlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnN0b3BQcm9wYWdhdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgICAgICAvLyBVbmZvY3VzIGFuZCB1bnNlbGVjdCBldmVyeXRoaW5nIG9uIGNsaWNrIG91dHNpZGUgb2YgY2FudmFzLlxyXG4gICAgICAgICAgICAgICAgICAgICQod2luZG93KS5jbGljayhmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBFeGNlcHQgd2hlbiBpbiBpbmxpbmUgZWRpdGluZyBtb2RlLlxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoIXNjb3BlLmVsZW1lbnQuaW5saW5lRWRpdGluZ0lzQWN0aXZlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS4kYXBwbHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLmVsZW1lbnQuYWN0aXZlRWxlbWVudCA9IG51bGw7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZWxlbWVudC5mb2N1c2VkRWxlbWVudCA9IG51bGw7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH1cclxuICAgIF0pOyIsImFuZ3VsYXJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXG4gICAgLmRpcmVjdGl2ZShcIm9yY0xheW91dENhbnZhc1wiLCBbXCJzY29wZUNvbmZpZ3VyYXRvclwiLCBcImVudmlyb25tZW50XCIsXG4gICAgICAgIGZ1bmN0aW9uIChzY29wZUNvbmZpZ3VyYXRvciwgZW52aXJvbm1lbnQpIHtcbiAgICAgICAgICAgIHJldHVybiB7XG4gICAgICAgICAgICAgICAgcmVzdHJpY3Q6IFwiRVwiLFxuICAgICAgICAgICAgICAgIHNjb3BlOiB7IGVsZW1lbnQ6IFwiPVwiIH0sXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogW1wiJHNjb3BlXCIsIFwiJGVsZW1lbnRcIiwgXCIkYXR0cnNcIixcbiAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKCRzY29wZSwgJGVsZW1lbnQsICRhdHRycykge1xuICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGVDb25maWd1cmF0b3IuY29uZmlndXJlRm9yRWxlbWVudCgkc2NvcGUsICRlbGVtZW50KTtcbiAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlQ29uZmlndXJhdG9yLmNvbmZpZ3VyZUZvckNvbnRhaW5lcigkc2NvcGUsICRlbGVtZW50KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5zb3J0YWJsZU9wdGlvbnNbXCJheGlzXCJdID0gXCJ5XCI7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICBdLFxuICAgICAgICAgICAgICAgIHRlbXBsYXRlVXJsOiBlbnZpcm9ubWVudC50ZW1wbGF0ZVVybChcIkNhbnZhc1wiKSxcbiAgICAgICAgICAgICAgICByZXBsYWNlOiB0cnVlXG4gICAgICAgICAgICB9O1xuICAgICAgICB9XG4gICAgXSk7IiwiYW5ndWxhclxuICAgIC5tb2R1bGUoXCJMYXlvdXRFZGl0b3JcIilcbiAgICAuZGlyZWN0aXZlKFwib3JjTGF5b3V0Q2hpbGRcIiwgW1wiJGNvbXBpbGVcIixcbiAgICAgICAgZnVuY3Rpb24gKCRjb21waWxlKSB7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICAgIHJlc3RyaWN0OiBcIkVcIixcbiAgICAgICAgICAgICAgICBzY29wZTogeyBlbGVtZW50OiBcIj1cIiB9LFxuICAgICAgICAgICAgICAgIGxpbms6IGZ1bmN0aW9uIChzY29wZSwgZWxlbWVudCkge1xuICAgICAgICAgICAgICAgICAgICB2YXIgdGVtcGxhdGUgPSBcIjxvcmMtbGF5b3V0LVwiICsgc2NvcGUuZWxlbWVudC50eXBlLnRvTG93ZXJDYXNlKCkgKyBcIiBlbGVtZW50PSdlbGVtZW50JyAvPlwiO1xuICAgICAgICAgICAgICAgICAgICB2YXIgaHRtbCA9ICRjb21waWxlKHRlbXBsYXRlKShzY29wZSk7XG4gICAgICAgICAgICAgICAgICAgICQoZWxlbWVudCkucmVwbGFjZVdpdGgoaHRtbCk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfTtcbiAgICAgICAgfVxuICAgIF0pOyIsImFuZ3VsYXJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXG4gICAgLmRpcmVjdGl2ZShcIm9yY0xheW91dENvbHVtblwiLCBbXCIkY29tcGlsZVwiLCBcInNjb3BlQ29uZmlndXJhdG9yXCIsIFwiZW52aXJvbm1lbnRcIixcbiAgICAgICAgZnVuY3Rpb24gKCRjb21waWxlLCBzY29wZUNvbmZpZ3VyYXRvciwgZW52aXJvbm1lbnQpIHtcbiAgICAgICAgICAgIHJldHVybiB7XG4gICAgICAgICAgICAgICAgcmVzdHJpY3Q6IFwiRVwiLFxuICAgICAgICAgICAgICAgIHNjb3BlOiB7IGVsZW1lbnQ6IFwiPVwiIH0sXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogW1wiJHNjb3BlXCIsIFwiJGVsZW1lbnRcIixcbiAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKCRzY29wZSwgJGVsZW1lbnQpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlQ29uZmlndXJhdG9yLmNvbmZpZ3VyZUZvckVsZW1lbnQoJHNjb3BlLCAkZWxlbWVudCk7XG4gICAgICAgICAgICAgICAgICAgICAgICBzY29wZUNvbmZpZ3VyYXRvci5jb25maWd1cmVGb3JDb250YWluZXIoJHNjb3BlLCAkZWxlbWVudCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuc29ydGFibGVPcHRpb25zW1wiYXhpc1wiXSA9IFwieVwiO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgXSxcbiAgICAgICAgICAgICAgICB0ZW1wbGF0ZVVybDogZW52aXJvbm1lbnQudGVtcGxhdGVVcmwoXCJDb2x1bW5cIiksXG4gICAgICAgICAgICAgICAgcmVwbGFjZTogdHJ1ZSxcbiAgICAgICAgICAgICAgICBsaW5rOiBmdW5jdGlvbiAoc2NvcGUsIGVsZW1lbnQsIGF0dHJzKSB7XG4gICAgICAgICAgICAgICAgICAgIGVsZW1lbnQuZmluZChcIi5sYXlvdXQtY29sdW1uLXJlc2l6ZS1iYXJcIikuZHJhZ2dhYmxlKHtcbiAgICAgICAgICAgICAgICAgICAgICAgIGF4aXM6IFwieFwiLFxuICAgICAgICAgICAgICAgICAgICAgICAgaGVscGVyOiBcImNsb25lXCIsXG4gICAgICAgICAgICAgICAgICAgICAgICByZXZlcnQ6IHRydWUsXG4gICAgICAgICAgICAgICAgICAgICAgICBzdGFydDogZnVuY3Rpb24gKGUsIHVpKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuJGFwcGx5KGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZWxlbWVudC5lZGl0b3IuaXNSZXNpemluZyA9IHRydWU7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgICAgICAgICB9LFxuICAgICAgICAgICAgICAgICAgICAgICAgZHJhZzogZnVuY3Rpb24gKGUsIHVpKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGNvbHVtbkVsZW1lbnQgPSBlbGVtZW50LnBhcmVudCgpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBjb2x1bW5TaXplID0gY29sdW1uRWxlbWVudC53aWR0aCgpIC8gc2NvcGUuZWxlbWVudC53aWR0aDtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgY29ubmVjdEFkamFjZW50ID0gIWUuY3RybEtleTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoJChlLnRhcmdldCkuaGFzQ2xhc3MoXCJsYXlvdXQtY29sdW1uLXJlc2l6ZS1iYXItbGVmdFwiKSkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgZGVsdGEgPSB1aS5vZmZzZXQubGVmdCAtIGNvbHVtbkVsZW1lbnQub2Zmc2V0KCkubGVmdDtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGRlbHRhIDwgLWNvbHVtblNpemUgJiYgc2NvcGUuZWxlbWVudC5jYW5FeHBhbmRMZWZ0KGNvbm5lY3RBZGphY2VudCkpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZWxlbWVudC5leHBhbmRMZWZ0KGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbHNlIGlmIChkZWx0YSA+IGNvbHVtblNpemUgJiYgc2NvcGUuZWxlbWVudC5jYW5Db250cmFjdExlZnQoY29ubmVjdEFkamFjZW50KSkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuJGFwcGx5KGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS5lbGVtZW50LmNvbnRyYWN0TGVmdChjb25uZWN0QWRqYWNlbnQpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSBpZiAoJChlLnRhcmdldCkuaGFzQ2xhc3MoXCJsYXlvdXQtY29sdW1uLXJlc2l6ZS1iYXItcmlnaHRcIikpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGRlbHRhID0gdWkub2Zmc2V0LmxlZnQgLSBjb2x1bW5FbGVtZW50LndpZHRoKCkgLSBjb2x1bW5FbGVtZW50Lm9mZnNldCgpLmxlZnQ7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChkZWx0YSA+IGNvbHVtblNpemUgJiYgc2NvcGUuZWxlbWVudC5jYW5FeHBhbmRSaWdodChjb25uZWN0QWRqYWNlbnQpKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS4kYXBwbHkoZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLmVsZW1lbnQuZXhwYW5kUmlnaHQoY29ubmVjdEFkamFjZW50KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsc2UgaWYgKGRlbHRhIDwgLWNvbHVtblNpemUgJiYgc2NvcGUuZWxlbWVudC5jYW5Db250cmFjdFJpZ2h0KGNvbm5lY3RBZGphY2VudCkpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGUuZWxlbWVudC5jb250cmFjdFJpZ2h0KGNvbm5lY3RBZGphY2VudCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgICAgICAgICAgICAgfSxcbiAgICAgICAgICAgICAgICAgICAgICAgIHN0b3A6IGZ1bmN0aW9uIChlLCB1aSkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzY29wZS5lbGVtZW50LmVkaXRvci5pc1Jlc2l6aW5nID0gZmFsc2U7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH07XG4gICAgICAgIH1cbiAgICBdKTsiLCJhbmd1bGFyXG4gICAgLm1vZHVsZShcIkxheW91dEVkaXRvclwiKVxuICAgIC5kaXJlY3RpdmUoXCJvcmNMYXlvdXRDb250ZW50XCIsIFtcIiRzY2VcIiwgXCJzY29wZUNvbmZpZ3VyYXRvclwiLCBcImVudmlyb25tZW50XCIsXG4gICAgICAgIGZ1bmN0aW9uICgkc2NlLCBzY29wZUNvbmZpZ3VyYXRvciwgZW52aXJvbm1lbnQpIHtcbiAgICAgICAgICAgIHJldHVybiB7XG4gICAgICAgICAgICAgICAgcmVzdHJpY3Q6IFwiRVwiLFxuICAgICAgICAgICAgICAgIHNjb3BlOiB7IGVsZW1lbnQ6IFwiPVwiIH0sXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogW1wiJHNjb3BlXCIsIFwiJGVsZW1lbnRcIixcbiAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKCRzY29wZSwgJGVsZW1lbnQpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlQ29uZmlndXJhdG9yLmNvbmZpZ3VyZUZvckVsZW1lbnQoJHNjb3BlLCAkZWxlbWVudCk7XG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWRpdCA9IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJHJvb3QuZWRpdEVsZW1lbnQoJHNjb3BlLmVsZW1lbnQpLnRoZW4oZnVuY3Rpb24gKGFyZ3MpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoYXJncy5jYW5jZWwpXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuO1xuXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5kYXRhID0gYXJncy5lbGVtZW50LmRhdGE7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5zZXRIdG1sKGFyZ3MuZWxlbWVudC5odG1sKTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xuXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBPdmVyd3JpdGUgdGhlIHNldEh0bWwgZnVuY3Rpb24gc28gdGhhdCB3ZSBjYW4gdXNlIHRoZSAkc2NlIHNlcnZpY2UgdG8gdHJ1c3QgdGhlIGh0bWwgKGFuZCBub3QgaGF2ZSB0aGUgaHRtbCBiaW5kaW5nIHN0cmlwIGNlcnRhaW4gdGFncykuXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5zZXRIdG1sID0gZnVuY3Rpb24gKGh0bWwpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5odG1sID0gaHRtbDtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5odG1sVW5zYWZlID0gJHNjZS50cnVzdEFzSHRtbChodG1sKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIH07XG5cbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lbGVtZW50LnNldEh0bWwoJHNjb3BlLmVsZW1lbnQuaHRtbCk7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICBdLFxuICAgICAgICAgICAgICAgIHRlbXBsYXRlVXJsOiBlbnZpcm9ubWVudC50ZW1wbGF0ZVVybChcIkNvbnRlbnRcIiksXG4gICAgICAgICAgICAgICAgcmVwbGFjZTogdHJ1ZVxuICAgICAgICAgICAgfTtcbiAgICAgICAgfVxuICAgIF0pOyIsImFuZ3VsYXJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXG4gICAgLmRpcmVjdGl2ZShcIm9yY0xheW91dEh0bWxcIiwgW1wiJHNjZVwiLCBcInNjb3BlQ29uZmlndXJhdG9yXCIsIFwiZW52aXJvbm1lbnRcIixcbiAgICAgICAgZnVuY3Rpb24gKCRzY2UsIHNjb3BlQ29uZmlndXJhdG9yLCBlbnZpcm9ubWVudCkge1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgICByZXN0cmljdDogXCJFXCIsXG4gICAgICAgICAgICAgICAgc2NvcGU6IHsgZWxlbWVudDogXCI9XCIgfSxcbiAgICAgICAgICAgICAgICBjb250cm9sbGVyOiBbXCIkc2NvcGVcIiwgXCIkZWxlbWVudFwiLFxuICAgICAgICAgICAgICAgICAgICBmdW5jdGlvbiAoJHNjb3BlLCAkZWxlbWVudCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGVDb25maWd1cmF0b3IuY29uZmlndXJlRm9yRWxlbWVudCgkc2NvcGUsICRlbGVtZW50KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lZGl0ID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kcm9vdC5lZGl0RWxlbWVudCgkc2NvcGUuZWxlbWVudCkudGhlbihmdW5jdGlvbiAoYXJncykge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJGFwcGx5KGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChhcmdzLmNhbmNlbClcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm47XG5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lbGVtZW50LmRhdGEgPSBhcmdzLmVsZW1lbnQuZGF0YTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lbGVtZW50LnNldEh0bWwoYXJncy5lbGVtZW50Lmh0bWwpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgICAgICAgICAgICAgIH07XG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUudXBkYXRlQ29udGVudCA9IGZ1bmN0aW9uIChlKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuc2V0SHRtbChlLnRhcmdldC5pbm5lckhUTUwpO1xuICAgICAgICAgICAgICAgICAgICAgICAgfTtcblxuICAgICAgICAgICAgICAgICAgICAgICAgLy8gT3ZlcndyaXRlIHRoZSBzZXRIdG1sIGZ1bmN0aW9uIHNvIHRoYXQgd2UgY2FuIHVzZSB0aGUgJHNjZSBzZXJ2aWNlIHRvIHRydXN0IHRoZSBodG1sIChhbmQgbm90IGhhdmUgdGhlIGh0bWwgYmluZGluZyBzdHJpcCBjZXJ0YWluIHRhZ3MpLlxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuc2V0SHRtbCA9IGZ1bmN0aW9uIChodG1sKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuaHRtbCA9IGh0bWw7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuaHRtbFVuc2FmZSA9ICRzY2UudHJ1c3RBc0h0bWwoaHRtbCk7XG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xuXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5zZXRIdG1sKCRzY29wZS5lbGVtZW50Lmh0bWwpO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgXSxcbiAgICAgICAgICAgICAgICB0ZW1wbGF0ZVVybDogZW52aXJvbm1lbnQudGVtcGxhdGVVcmwoXCJIdG1sXCIpLFxuICAgICAgICAgICAgICAgIHJlcGxhY2U6IHRydWUsXG4gICAgICAgICAgICAgICAgbGluazogZnVuY3Rpb24gKHNjb3BlLCBlbGVtZW50KSB7XG4gICAgICAgICAgICAgICAgICAgIC8vIE1vdXNlIGRvd24gZXZlbnRzIG11c3Qgbm90IGJlIGludGVyY2VwdGVkIGJ5IGRyYWcgYW5kIGRyb3Agd2hpbGUgaW5saW5lIGVkaXRpbmcgaXMgYWN0aXZlLFxuICAgICAgICAgICAgICAgICAgICAvLyBvdGhlcndpc2UgY2xpY2tzIGluIGlubGluZSBlZGl0b3JzIHdpbGwgaGF2ZSBubyBlZmZlY3QuXG4gICAgICAgICAgICAgICAgICAgIGVsZW1lbnQuZmluZChcIi5sYXlvdXQtY29udGVudC1tYXJrdXBcIikubW91c2Vkb3duKGZ1bmN0aW9uIChlKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoc2NvcGUuZWxlbWVudC5lZGl0b3IuaW5saW5lRWRpdGluZ0lzQWN0aXZlKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZS5zdG9wUHJvcGFnYXRpb24oKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfTtcbiAgICAgICAgfVxuICAgIF0pOyIsImFuZ3VsYXJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXG4gICAgLmRpcmVjdGl2ZShcIm9yY0xheW91dEdyaWRcIiwgW1wiJGNvbXBpbGVcIiwgXCJzY29wZUNvbmZpZ3VyYXRvclwiLCBcImVudmlyb25tZW50XCIsXG4gICAgICAgIGZ1bmN0aW9uICgkY29tcGlsZSwgc2NvcGVDb25maWd1cmF0b3IsIGVudmlyb25tZW50KSB7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICAgIHJlc3RyaWN0OiBcIkVcIixcbiAgICAgICAgICAgICAgICBzY29wZTogeyBlbGVtZW50OiBcIj1cIiB9LFxuICAgICAgICAgICAgICAgIGNvbnRyb2xsZXI6IFtcIiRzY29wZVwiLCBcIiRlbGVtZW50XCIsXG4gICAgICAgICAgICAgICAgICAgIGZ1bmN0aW9uICgkc2NvcGUsICRlbGVtZW50KSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBzY29wZUNvbmZpZ3VyYXRvci5jb25maWd1cmVGb3JFbGVtZW50KCRzY29wZSwgJGVsZW1lbnQpO1xuICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGVDb25maWd1cmF0b3IuY29uZmlndXJlRm9yQ29udGFpbmVyKCRzY29wZSwgJGVsZW1lbnQpO1xuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLnNvcnRhYmxlT3B0aW9uc1tcImF4aXNcIl0gPSBcInlcIjtcbiAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIF0sXG4gICAgICAgICAgICAgICAgdGVtcGxhdGVVcmw6IGVudmlyb25tZW50LnRlbXBsYXRlVXJsKFwiR3JpZFwiKSxcbiAgICAgICAgICAgICAgICByZXBsYWNlOiB0cnVlXG4gICAgICAgICAgICB9O1xuICAgICAgICB9XG4gICAgXSk7IiwiYW5ndWxhclxuICAgIC5tb2R1bGUoXCJMYXlvdXRFZGl0b3JcIilcbiAgICAuZGlyZWN0aXZlKFwib3JjTGF5b3V0Um93XCIsIFtcIiRjb21waWxlXCIsIFwic2NvcGVDb25maWd1cmF0b3JcIiwgXCJlbnZpcm9ubWVudFwiLFxuICAgICAgICBmdW5jdGlvbiAoJGNvbXBpbGUsIHNjb3BlQ29uZmlndXJhdG9yLCBlbnZpcm9ubWVudCkge1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgICByZXN0cmljdDogXCJFXCIsXG4gICAgICAgICAgICAgICAgc2NvcGU6IHsgZWxlbWVudDogXCI9XCIgfSxcbiAgICAgICAgICAgICAgICBjb250cm9sbGVyOiBbXCIkc2NvcGVcIiwgXCIkZWxlbWVudFwiLFxuICAgICAgICAgICAgICAgICAgICBmdW5jdGlvbiAoJHNjb3BlLCAkZWxlbWVudCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgc2NvcGVDb25maWd1cmF0b3IuY29uZmlndXJlRm9yRWxlbWVudCgkc2NvcGUsICRlbGVtZW50KTtcbiAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlQ29uZmlndXJhdG9yLmNvbmZpZ3VyZUZvckNvbnRhaW5lcigkc2NvcGUsICRlbGVtZW50KTtcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5zb3J0YWJsZU9wdGlvbnNbXCJheGlzXCJdID0gXCJ4XCI7XG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuc29ydGFibGVPcHRpb25zW1widWktZmxvYXRpbmdcIl0gPSB0cnVlO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgXSxcbiAgICAgICAgICAgICAgICB0ZW1wbGF0ZVVybDogZW52aXJvbm1lbnQudGVtcGxhdGVVcmwoXCJSb3dcIiksXG4gICAgICAgICAgICAgICAgcmVwbGFjZTogdHJ1ZVxuICAgICAgICAgICAgfTtcbiAgICAgICAgfVxuICAgIF0pOyIsImFuZ3VsYXJcclxuICAgIC5tb2R1bGUoXCJMYXlvdXRFZGl0b3JcIilcclxuICAgIC5kaXJlY3RpdmUoXCJvcmNMYXlvdXRQb3B1cFwiLCBbXHJcbiAgICAgICAgZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgcmVzdHJpY3Q6IFwiQVwiLFxyXG4gICAgICAgICAgICAgICAgbGluazogZnVuY3Rpb24gKHNjb3BlLCBlbGVtZW50LCBhdHRycykge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwb3B1cCA9ICQoZWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHRyaWdnZXIgPSBwb3B1cC5jbG9zZXN0KFwiLmxheW91dC1wb3B1cC10cmlnZ2VyXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwYXJlbnRFbGVtZW50ID0gcG9wdXAuY2xvc2VzdChcIi5sYXlvdXQtZWxlbWVudFwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0cmlnZ2VyLmNsaWNrKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcG9wdXAudG9nZ2xlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChwb3B1cC5pcyhcIjp2aXNpYmxlXCIpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBwb3B1cC5wb3NpdGlvbih7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbXk6IGF0dHJzLm9yY0xheW91dFBvcHVwTXkgfHwgXCJsZWZ0IHRvcFwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGF0OiBhdHRycy5vcmNMYXlvdXRQb3B1cEF0IHx8IFwibGVmdCBib3R0b20rNHB4XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgb2Y6IHRyaWdnZXJcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9wdXAuZmluZChcImlucHV0XCIpLmZpcnN0KCkuZm9jdXMoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIHBvcHVwLmNsaWNrKGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgcGFyZW50RWxlbWVudC5jbGljayhmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBwb3B1cC5oaWRlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgcG9wdXAua2V5ZG93bihmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoIWUuY3RybEtleSAmJiAhZS5zaGlmdEtleSAmJiAhZS5hbHRLZXkgJiYgZS53aGljaCA9PSAyNykgLy8gRXNjXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBwb3B1cC5oaWRlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgcG9wdXAub24oXCJjdXQgY29weSBwYXN0ZVwiLCBmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBBbGxvdyBjbGlwYm9hcmQgb3BlcmF0aW9ucyBpbiBwb3B1cCB3aXRob3V0IGludm9raW5nIGNsaXBib2FyZCBldmVudCBoYW5kbGVycyBvbiBwYXJlbnQgZWxlbWVudC5cclxuICAgICAgICAgICAgICAgICAgICAgICAgZS5zdG9wUHJvcGFnYXRpb24oKTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9XHJcbiAgICBdKTsiLCJhbmd1bGFyXHJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXHJcbiAgICAuZGlyZWN0aXZlKFwib3JjTGF5b3V0VG9vbGJveFwiLCBbXCIkY29tcGlsZVwiLCBcImVudmlyb25tZW50XCIsXHJcbiAgICAgICAgZnVuY3Rpb24gKCRjb21waWxlLCBlbnZpcm9ubWVudCkge1xyXG4gICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgcmVzdHJpY3Q6IFwiRVwiLFxyXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogW1wiJHNjb3BlXCIsIFwiJGVsZW1lbnRcIixcclxuICAgICAgICAgICAgICAgICAgICBmdW5jdGlvbiAoJHNjb3BlLCAkZWxlbWVudCkge1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLnJlc2V0RWxlbWVudHMgPSBmdW5jdGlvbiAoKSB7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmdyaWRFbGVtZW50cyA9IFtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuR3JpZC5mcm9tKHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveEljb246IFwiXFx1ZjAwYVwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94TGFiZWw6IFwiR3JpZFwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94RGVzY3JpcHRpb246IFwiRW1wdHkgZ3JpZC5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIF07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLnJvd0VsZW1lbnRzID0gW1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIExheW91dEVkaXRvci5Sb3cuZnJvbSh7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hJY29uOiBcIlxcdWYwYzlcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveExhYmVsOiBcIlJvdyAoMSBjb2x1bW4pXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hEZXNjcmlwdGlvbjogXCJSb3cgd2l0aCAxIGNvbHVtbi5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IExheW91dEVkaXRvci5Db2x1bW4udGltZXMoMSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94SWNvbjogXCJcXHVmMGM5XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hMYWJlbDogXCJSb3cgKDIgY29sdW1ucylcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveERlc2NyaXB0aW9uOiBcIlJvdyB3aXRoIDIgY29sdW1ucy5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IExheW91dEVkaXRvci5Db2x1bW4udGltZXMoMilcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94SWNvbjogXCJcXHVmMGM5XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hMYWJlbDogXCJSb3cgKDMgY29sdW1ucylcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveERlc2NyaXB0aW9uOiBcIlJvdyB3aXRoIDMgY29sdW1ucy5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IExheW91dEVkaXRvci5Db2x1bW4udGltZXMoMylcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94SWNvbjogXCJcXHVmMGM5XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hMYWJlbDogXCJSb3cgKDQgY29sdW1ucylcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveERlc2NyaXB0aW9uOiBcIlJvdyB3aXRoIDQgY29sdW1ucy5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IExheW91dEVkaXRvci5Db2x1bW4udGltZXMoNClcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94SWNvbjogXCJcXHVmMGM5XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hMYWJlbDogXCJSb3cgKDYgY29sdW1ucylcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveERlc2NyaXB0aW9uOiBcIlJvdyB3aXRoIDYgY29sdW1ucy5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IExheW91dEVkaXRvci5Db2x1bW4udGltZXMoNilcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBMYXlvdXRFZGl0b3IuUm93LmZyb20oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94SWNvbjogXCJcXHVmMGM5XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hMYWJlbDogXCJSb3cgKDEyIGNvbHVtbnMpXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hEZXNjcmlwdGlvbjogXCJSb3cgd2l0aCAxMiBjb2x1bW5zLlwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjaGlsZHJlbjogTGF5b3V0RWRpdG9yLkNvbHVtbi50aW1lcygxMilcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSwgTGF5b3V0RWRpdG9yLlJvdy5mcm9tKHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveEljb246IFwiXFx1ZjBjOVwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94TGFiZWw6IFwiUm93IChlbXB0eSlcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveERlc2NyaXB0aW9uOiBcIkVtcHR5IHJvdy5cIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2hpbGRyZW46IFtdXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIF07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmNvbHVtbkVsZW1lbnRzID0gW1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIExheW91dEVkaXRvci5Db2x1bW4uZnJvbSh7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3hJY29uOiBcIlxcdWYwZGJcIixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveExhYmVsOiBcIkNvbHVtblwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94RGVzY3JpcHRpb246IFwiRW1wdHkgY29sdW1uLlwiLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aWR0aDogMSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgb2Zmc2V0OiAwLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjaGlsZHJlbjogW11cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgXTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuY29udGVudEVsZW1lbnRDYXRlZ29yaWVzID0gXygkc2NvcGUuZWxlbWVudC5jb25maWcuY2F0ZWdvcmllcykubWFwKGZ1bmN0aW9uIChjYXRlZ29yeSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5hbWU6IGNhdGVnb3J5Lm5hbWUsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnRzOiBfKGNhdGVnb3J5LmNvbnRlbnRUeXBlcykubWFwKGZ1bmN0aW9uIChjb250ZW50VHlwZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHR5cGUgPSBjb250ZW50VHlwZS50eXBlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGZhY3RvcnkgPSBMYXlvdXRFZGl0b3IuZmFjdG9yaWVzW3R5cGVdIHx8IExheW91dEVkaXRvci5mYWN0b3JpZXNbXCJDb250ZW50XCJdO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGl0ZW0gPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaXNUZW1wbGF0ZWQ6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnRlbnRUeXBlOiBjb250ZW50VHlwZS5pZCxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250ZW50VHlwZUxhYmVsOiBjb250ZW50VHlwZS5sYWJlbCxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250ZW50VHlwZUNsYXNzOiBjb250ZW50VHlwZS50eXBlQ2xhc3MsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGF0YTogbnVsbCxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBoYXNFZGl0b3I6IGNvbnRlbnRUeXBlLmhhc0VkaXRvcixcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBodG1sOiBjb250ZW50VHlwZS5odG1sXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGVsZW1lbnQgPSBmYWN0b3J5KGl0ZW0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC50b29sYm94SWNvbiA9IGNvbnRlbnRUeXBlLmljb24gfHwgXCJcXHVmMWM5XCI7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnRvb2xib3hMYWJlbCA9IGNvbnRlbnRUeXBlLmxhYmVsO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudC50b29sYm94RGVzY3JpcHRpb24gPSBjb250ZW50VHlwZS5kZXNjcmlwdGlvbjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBlbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUucmVzZXRFbGVtZW50cygpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmdldFNvcnRhYmxlT3B0aW9ucyA9IGZ1bmN0aW9uICh0eXBlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgZWRpdG9ySWQgPSAkZWxlbWVudC5jbG9zZXN0KFwiLmxheW91dC1lZGl0b3JcIikuYXR0cihcImlkXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHBhcmVudENsYXNzZXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgcGxhY2Vob2xkZXJDbGFzc2VzO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGZsb2F0aW5nID0gZmFsc2U7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc3dpdGNoICh0eXBlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBcIkdyaWRcIjpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFyZW50Q2xhc3NlcyA9IFtcIi5sYXlvdXQtY2FudmFzXCIsIFwiLmxheW91dC1jb2x1bW5cIiwgXCIubGF5b3V0LWNvbW1vbi1ob2xkZXJcIl07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyQ2xhc3NlcyA9IFwibGF5b3V0LWVsZW1lbnQgbGF5b3V0LWNvbnRhaW5lciBsYXlvdXQtZ3JpZCB1aS1zb3J0YWJsZS1wbGFjZWhvbGRlclwiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjYXNlIFwiUm93XCI6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhcmVudENsYXNzZXMgPSBbXCIubGF5b3V0LWdyaWRcIl07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyQ2xhc3NlcyA9IFwibGF5b3V0LWVsZW1lbnQgbGF5b3V0LWNvbnRhaW5lciBsYXlvdXQtcm93IHJvdyB1aS1zb3J0YWJsZS1wbGFjZWhvbGRlclwiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjYXNlIFwiQ29sdW1uXCI6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhcmVudENsYXNzZXMgPSBbXCIubGF5b3V0LXJvdzpub3QoLmxheW91dC1yb3ctZnVsbClcIl07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyQ2xhc3NlcyA9IFwibGF5b3V0LWVsZW1lbnQgbGF5b3V0LWNvbnRhaW5lciBsYXlvdXQtY29sdW1uIHVpLXNvcnRhYmxlLXBsYWNlaG9sZGVyXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZsb2F0aW5nID0gdHJ1ZTsgLy8gVG8gZW5zdXJlIGEgc21vb3RoIGhvcml6b250YWwtbGlzdCByZW9yZGVyaW5nLiBodHRwczovL2dpdGh1Yi5jb20vYW5ndWxhci11aS91aS1zb3J0YWJsZSNmbG9hdGluZ1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjYXNlIFwiQ29udGVudFwiOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYXJlbnRDbGFzc2VzID0gW1wiLmxheW91dC1jYW52YXNcIiwgXCIubGF5b3V0LWNvbHVtblwiLCBcIi5sYXlvdXQtY29tbW9uLWhvbGRlclwiXTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGxhY2Vob2xkZXJDbGFzc2VzID0gXCJsYXlvdXQtZWxlbWVudCBsYXlvdXQtY29udGVudCB1aS1zb3J0YWJsZS1wbGFjZWhvbGRlclwiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGN1cnNvcjogXCJtb3ZlXCIsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY29ubmVjdFdpdGg6IF8ocGFyZW50Q2xhc3NlcykubWFwKGZ1bmN0aW9uIChlKSB7IHJldHVybiBcIiNcIiArIGVkaXRvcklkICsgXCIgXCIgKyBlICsgXCI6bm90KC5sYXlvdXQtY29udGFpbmVyLXNlYWxlZCkgPiAubGF5b3V0LWVsZW1lbnQtd3JhcHBlciA+IC5sYXlvdXQtY2hpbGRyZW5cIjsgfSkuam9pbihcIiwgXCIpLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyOiBwbGFjZWhvbGRlckNsYXNzZXMsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgXCJ1aS1mbG9hdGluZ1wiOiBmbG9hdGluZyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjcmVhdGU6IGZ1bmN0aW9uIChlLCB1aSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnRhcmdldC5pc1Rvb2xib3ggPSB0cnVlOyAvLyBXaWxsIGluZGljYXRlIHRvIGNvbm5lY3RlZCBzb3J0YWJsZXMgdGhhdCBkcm9wcGVkIGl0ZW1zIHdlcmUgc2VudCBmcm9tIHRvb2xib3guXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzdGFydDogZnVuY3Rpb24gKGUsIHVpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kYXBwbHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuaXNEcmFnZ2luZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc3RvcDogZnVuY3Rpb24gKGUsIHVpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kYXBwbHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuaXNEcmFnZ2luZyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLnJlc2V0RWxlbWVudHMoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBvdmVyOiBmdW5jdGlvbiAoZSwgdWkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLiRhcHBseShmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5jYW52YXMuc2V0SXNEcm9wVGFyZ2V0KGZhbHNlKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBsYXlvdXRJc0NvbGxhcHNlZENvb2tpZU5hbWUgPSBcImxheW91dFRvb2xib3hDYXRlZ29yeV9MYXlvdXRfSXNDb2xsYXBzZWRcIjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmxheW91dElzQ29sbGFwc2VkID0gJC5jb29raWUobGF5b3V0SXNDb2xsYXBzZWRDb29raWVOYW1lKSA9PT0gXCJ0cnVlXCI7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUudG9nZ2xlTGF5b3V0SXNDb2xsYXBzZWQgPSBmdW5jdGlvbiAoZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmxheW91dElzQ29sbGFwc2VkID0gISRzY29wZS5sYXlvdXRJc0NvbGxhcHNlZDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICQuY29va2llKGxheW91dElzQ29sbGFwc2VkQ29va2llTmFtZSwgJHNjb3BlLmxheW91dElzQ29sbGFwc2VkLCB7IGV4cGlyZXM6IDM2NSB9KTsgLy8gUmVtZW1iZXIgY29sbGFwc2VkIHN0YXRlIGZvciBhIHllYXIuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlLnN0b3BQcm9wYWdhdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIF0sXHJcbiAgICAgICAgICAgICAgICB0ZW1wbGF0ZVVybDogZW52aXJvbm1lbnQudGVtcGxhdGVVcmwoXCJUb29sYm94XCIpLFxyXG4gICAgICAgICAgICAgICAgcmVwbGFjZTogdHJ1ZSxcclxuICAgICAgICAgICAgICAgIGxpbms6IGZ1bmN0aW9uIChzY29wZSwgZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciB0b29sYm94ID0gZWxlbWVudC5maW5kKFwiLmxheW91dC10b29sYm94XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICQod2luZG93KS5vbihcInJlc2l6ZSBzY3JvbGxcIiwgZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGNhbnZhcyA9IGVsZW1lbnQucGFyZW50KCkuZmluZChcIi5sYXlvdXQtY2FudmFzXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBJZiB0aGUgY2FudmFzIGlzIHRhbGxlciB0aGFuIHRoZSB0b29sYm94LCBtYWtlIHRoZSB0b29sYm94IHN0aWNreS1wb3NpdGlvbmVkIHdpdGhpbiB0aGUgZWRpdG9yXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC8vIHRvIGhlbHAgdGhlIHVzZXIgYXZvaWQgZXhjZXNzaXZlIHZlcnRpY2FsIHNjcm9sbGluZy5cclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGNhbnZhc0lzVGFsbGVyID0gISFjYW52YXMgJiYgY2FudmFzLmhlaWdodCgpID4gdG9vbGJveC5oZWlnaHQoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHdpbmRvd1BvcyA9ICQod2luZG93KS5zY3JvbGxUb3AoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGNhbnZhc0lzVGFsbGVyICYmIHdpbmRvd1BvcyA+IGVsZW1lbnQub2Zmc2V0KCkudG9wICsgZWxlbWVudC5oZWlnaHQoKSAtIHRvb2xib3guaGVpZ2h0KCkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3guYWRkQ2xhc3MoXCJzdGlja3ktYm90dG9tXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveC5yZW1vdmVDbGFzcyhcInN0aWNreS10b3BcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSBpZiAoY2FudmFzSXNUYWxsZXIgJiYgd2luZG93UG9zID4gZWxlbWVudC5vZmZzZXQoKS50b3ApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3guYWRkQ2xhc3MoXCJzdGlja3ktdG9wXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdG9vbGJveC5yZW1vdmVDbGFzcyhcInN0aWNreS1ib3R0b21cIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB0b29sYm94LnJlbW92ZUNsYXNzKFwic3RpY2t5LXRvcFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvb2xib3gucmVtb3ZlQ2xhc3MoXCJzdGlja3ktYm90dG9tXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfVxyXG4gICAgXSk7IiwiYW5ndWxhclxuICAgIC5tb2R1bGUoXCJMYXlvdXRFZGl0b3JcIilcbiAgICAuZGlyZWN0aXZlKFwib3JjTGF5b3V0VG9vbGJveEdyb3VwXCIsIFtcIiRjb21waWxlXCIsIFwiZW52aXJvbm1lbnRcIixcbiAgICAgICAgZnVuY3Rpb24gKCRjb21waWxlLCBlbnZpcm9ubWVudCkge1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgICByZXN0cmljdDogXCJFXCIsXG4gICAgICAgICAgICAgICAgc2NvcGU6IHsgY2F0ZWdvcnk6IFwiPVwiIH0sXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogW1wiJHNjb3BlXCIsIFwiJGVsZW1lbnRcIixcbiAgICAgICAgICAgICAgICAgICAgZnVuY3Rpb24gKCRzY29wZSwgJGVsZW1lbnQpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBpc0NvbGxhcHNlZENvb2tpZU5hbWUgPSBcImxheW91dFRvb2xib3hDYXRlZ29yeV9cIiArICRzY29wZS5jYXRlZ29yeS5uYW1lICsgXCJfSXNDb2xsYXBzZWRcIjtcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5pc0NvbGxhcHNlZCA9ICQuY29va2llKGlzQ29sbGFwc2VkQ29va2llTmFtZSkgPT09IFwidHJ1ZVwiO1xuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLnRvZ2dsZUlzQ29sbGFwc2VkID0gZnVuY3Rpb24gKGUpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuaXNDb2xsYXBzZWQgPSAhJHNjb3BlLmlzQ29sbGFwc2VkO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICQuY29va2llKGlzQ29sbGFwc2VkQ29va2llTmFtZSwgJHNjb3BlLmlzQ29sbGFwc2VkLCB7IGV4cGlyZXM6IDM2NSB9KTsgLy8gUmVtZW1iZXIgY29sbGFwc2VkIHN0YXRlIGZvciBhIHllYXIuXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XG4gICAgICAgICAgICAgICAgICAgICAgICB9O1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgXSxcbiAgICAgICAgICAgICAgICB0ZW1wbGF0ZVVybDogZW52aXJvbm1lbnQudGVtcGxhdGVVcmwoXCJUb29sYm94R3JvdXBcIiksXG4gICAgICAgICAgICAgICAgcmVwbGFjZTogdHJ1ZVxuICAgICAgICAgICAgfTtcbiAgICAgICAgfVxuICAgIF0pOyJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==