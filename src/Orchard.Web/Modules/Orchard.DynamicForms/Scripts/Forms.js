angular
    .module("LayoutEditor")
    .directive("orcLayoutForm", ["$compile", "scopeConfigurator", "environment",
        function ($compile, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: ["$scope", "$element",
                    function ($scope, $element) {
                        scopeConfigurator.configureForElement($scope, $element);
                        scopeConfigurator.configureForContainer($scope, $element);
                        $scope.sortableOptions["axis"] = "y";
                        $scope.edit = function () {
                            $scope.$root.editElement($scope.element).then(function (args) {
                                if (args.cancel)
                                    return;
                                $scope.element.data = decodeURIComponent(args.element.data);
                                $scope.element.name = args.elementEditorModel.name;
                                $scope.element.formBindingContentType = args.elementEditorModel.formBindingContentType;
                                $scope.$apply();
                            });
                        };
                    }
                ],
                templateUrl: environment.templateUrl("Form"),
                replace: true
            };
        }
    ]);
var LayoutEditor;
(function ($, LayoutEditor) {

    LayoutEditor.Form = function (data, htmlId, htmlClass, htmlStyle, isTemplated, name, formBindingContentType, contentType, contentTypeLabel, contentTypeClass, hasEditor, children) {
        LayoutEditor.Element.call(this, "Form", data, htmlId, htmlClass, htmlStyle, isTemplated);
        LayoutEditor.Container.call(this, ["Grid", "Content"], children);

        var self = this;
        this.isContainable = true;
        this.dropTargetClass = "layout-common-holder";
        this.contentType = contentType;
        this.contentTypeLabel = contentTypeLabel;
        this.contentTypeClass = contentTypeClass;
        this.name = name || "Untitled";
        this.formBindingContentType = formBindingContentType;
        this.hasEditor = hasEditor;

        this.toObject = function () {
            var result = this.elementToObject();
            result.name = this.name;
            result.formBindingContentType = this.formBindingContentType;
            result.children = this.childrenToObject();

            return result;
        };

        var getEditorObject = this.getEditorObject;
        this.getEditorObject = function() {
            var dto = getEditorObject();
            return $.extend(dto, {
                FormName: this.name,
                FormBindingContentType: this.formBindingContentType
            });
        }

        this.setChildren = function (children) {
            this.children = children;
            _(this.children).each(function (child) {
                child.parent = self;
                self.linkChild(child);
            });
        };

        this.linkChild = function(element) {
            var getEditorObject = element.getEditorObject;
            element.getEditorObject = function () {
                var dto = getEditorObject();
                return $.extend(dto, {
                    FormBindingContentType: self.formBindingContentType
                });
            };
        };

        this.setChildren(children);
    };

    LayoutEditor.Form.from = function (value) {
        return new LayoutEditor.Form(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.name,
            value.formBindingContentType,
            value.contentType,
            value.contentTypeLabel,
            value.contentTypeClass,
            value.hasEditor,
            LayoutEditor.childrenFrom(value.children));
    };

    LayoutEditor.registerFactory("Form", function(value) { return LayoutEditor.Form.from(value); });

})(jQuery, LayoutEditor || (LayoutEditor = {}));
