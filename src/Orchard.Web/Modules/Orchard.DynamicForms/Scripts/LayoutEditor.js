angular
    .module("LayoutEditor")
    .directive("orcLayoutFieldset", ["$compile", "scopeConfigurator", "environment",
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
                                $scope.$apply(function() {
                                    $scope.element.data = decodeURIComponent(args.element.data);
                                    $scope.element.applyElementEditorModel(args.elementEditorModel);
                                });
                            });
                        };
                    }
                ],
                templateUrl: environment.templateUrl("Fieldset"),
                replace: true
            };
        }
    ]);
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

                                $scope.$apply(function() {
                                    $scope.element.data = decodeURIComponent(args.element.data);
                                    $scope.element.applyElementEditorModel(args.elementEditorModel);
                                });
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

    LayoutEditor.Fieldset = function (data, htmlId, htmlClass, htmlStyle, isTemplated, legend, contentType, contentTypeLabel, contentTypeClass, hasEditor, children) {
        LayoutEditor.Element.call(this, "Fieldset", data, htmlId, htmlClass, htmlStyle, isTemplated);
        LayoutEditor.Container.call(this, ["Grid", "Content"], children);

        var self = this;
        this.isContainable = true;
        this.dropTargetClass = "layout-common-holder";
        this.contentType = contentType;
        this.contentTypeLabel = contentTypeLabel;
        this.contentTypeClass = contentTypeClass;
        this.legend = legend || "";
        this.hasEditor = hasEditor;

        this.toObject = function () {
            var result = this.elementToObject();
            result.legend = this.legend;
            result.children = this.childrenToObject();

            return result;
        };

        var getEditorObject = this.getEditorObject;
        this.getEditorObject = function() {
            var dto = getEditorObject();
            return $.extend(dto, {
                Legend: this.legend
            });
        }

        this.setChildren = function (children) {
            this.children = children;
            _(this.children).each(function (child) {
                child.parent = self;
            });
        };

        this.applyElementEditorModel = function(model) {
            this.legend = model.legend;
        };

        this.setChildren(children);
    };

    LayoutEditor.Fieldset.from = function (value) {
        return new LayoutEditor.Fieldset(
            value.data,
            value.htmlId,
            value.htmlClass,
            value.htmlStyle,
            value.isTemplated,
            value.legend,
            value.contentType,
            value.contentTypeLabel,
            value.contentTypeClass,
            value.hasEditor,
            LayoutEditor.childrenFrom(value.children));
    };

    LayoutEditor.registerFactory("Fieldset", function(value) {
        return LayoutEditor.Fieldset.from(value);
    });

})(jQuery, LayoutEditor || (LayoutEditor = {}));

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
                child.setParent(self);
            });
        };

        this.onDescendantAdded = function(element) {
            var getEditorObject = element.getEditorObject;
            element.getEditorObject = function () {
                var dto = getEditorObject();
                return $.extend(dto, {
                    FormBindingContentType: self.formBindingContentType
                });
            };
        };

        this.applyElementEditorModel = function(model) {
            this.name = model.name;
            this.formBindingContentType = model.formBindingContentType;
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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIkRpcmVjdGl2ZXMvRmllbGRzZXQuanMiLCJEaXJlY3RpdmVzL0Zvcm0uanMiLCJNb2RlbHMvRmllbGRzZXQuanMiLCJNb2RlbHMvRm9ybS5qcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDNUJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzdCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2xFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsImZpbGUiOiJMYXlvdXRFZGl0b3IuanMiLCJzb3VyY2VzQ29udGVudCI6WyJhbmd1bGFyXHJcbiAgICAubW9kdWxlKFwiTGF5b3V0RWRpdG9yXCIpXHJcbiAgICAuZGlyZWN0aXZlKFwib3JjTGF5b3V0RmllbGRzZXRcIiwgW1wiJGNvbXBpbGVcIiwgXCJzY29wZUNvbmZpZ3VyYXRvclwiLCBcImVudmlyb25tZW50XCIsXHJcbiAgICAgICAgZnVuY3Rpb24gKCRjb21waWxlLCBzY29wZUNvbmZpZ3VyYXRvciwgZW52aXJvbm1lbnQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIHJlc3RyaWN0OiBcIkVcIixcclxuICAgICAgICAgICAgICAgIHNjb3BlOiB7IGVsZW1lbnQ6IFwiPVwiIH0sXHJcbiAgICAgICAgICAgICAgICBjb250cm9sbGVyOiBbXCIkc2NvcGVcIiwgXCIkZWxlbWVudFwiLFxyXG4gICAgICAgICAgICAgICAgICAgIGZ1bmN0aW9uICgkc2NvcGUsICRlbGVtZW50KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlQ29uZmlndXJhdG9yLmNvbmZpZ3VyZUZvckVsZW1lbnQoJHNjb3BlLCAkZWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHNjb3BlQ29uZmlndXJhdG9yLmNvbmZpZ3VyZUZvckNvbnRhaW5lcigkc2NvcGUsICRlbGVtZW50KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLnNvcnRhYmxlT3B0aW9uc1tcImF4aXNcIl0gPSBcInlcIjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVkaXQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJHJvb3QuZWRpdEVsZW1lbnQoJHNjb3BlLmVsZW1lbnQpLnRoZW4oZnVuY3Rpb24gKGFyZ3MpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoYXJncy5jYW5jZWwpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuJGFwcGx5KGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5kYXRhID0gZGVjb2RlVVJJQ29tcG9uZW50KGFyZ3MuZWxlbWVudC5kYXRhKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLmVsZW1lbnQuYXBwbHlFbGVtZW50RWRpdG9yTW9kZWwoYXJncy5lbGVtZW50RWRpdG9yTW9kZWwpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgXSxcclxuICAgICAgICAgICAgICAgIHRlbXBsYXRlVXJsOiBlbnZpcm9ubWVudC50ZW1wbGF0ZVVybChcIkZpZWxkc2V0XCIpLFxyXG4gICAgICAgICAgICAgICAgcmVwbGFjZTogdHJ1ZVxyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH1cclxuICAgIF0pOyIsImFuZ3VsYXJcclxuICAgIC5tb2R1bGUoXCJMYXlvdXRFZGl0b3JcIilcclxuICAgIC5kaXJlY3RpdmUoXCJvcmNMYXlvdXRGb3JtXCIsIFtcIiRjb21waWxlXCIsIFwic2NvcGVDb25maWd1cmF0b3JcIiwgXCJlbnZpcm9ubWVudFwiLFxyXG4gICAgICAgIGZ1bmN0aW9uICgkY29tcGlsZSwgc2NvcGVDb25maWd1cmF0b3IsIGVudmlyb25tZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICByZXN0cmljdDogXCJFXCIsXHJcbiAgICAgICAgICAgICAgICBzY29wZTogeyBlbGVtZW50OiBcIj1cIiB9LFxyXG4gICAgICAgICAgICAgICAgY29udHJvbGxlcjogW1wiJHNjb3BlXCIsIFwiJGVsZW1lbnRcIixcclxuICAgICAgICAgICAgICAgICAgICBmdW5jdGlvbiAoJHNjb3BlLCAkZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBzY29wZUNvbmZpZ3VyYXRvci5jb25maWd1cmVGb3JFbGVtZW50KCRzY29wZSwgJGVsZW1lbnQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBzY29wZUNvbmZpZ3VyYXRvci5jb25maWd1cmVGb3JDb250YWluZXIoJHNjb3BlLCAkZWxlbWVudCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5zb3J0YWJsZU9wdGlvbnNbXCJheGlzXCJdID0gXCJ5XCI7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lZGl0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJHNjb3BlLiRyb290LmVkaXRFbGVtZW50KCRzY29wZS5lbGVtZW50KS50aGVuKGZ1bmN0aW9uIChhcmdzKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGFyZ3MuY2FuY2VsKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS4kYXBwbHkoZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICRzY29wZS5lbGVtZW50LmRhdGEgPSBkZWNvZGVVUklDb21wb25lbnQoYXJncy5lbGVtZW50LmRhdGEpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAkc2NvcGUuZWxlbWVudC5hcHBseUVsZW1lbnRFZGl0b3JNb2RlbChhcmdzLmVsZW1lbnRFZGl0b3JNb2RlbCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBdLFxyXG4gICAgICAgICAgICAgICAgdGVtcGxhdGVVcmw6IGVudmlyb25tZW50LnRlbXBsYXRlVXJsKFwiRm9ybVwiKSxcclxuICAgICAgICAgICAgICAgIHJlcGxhY2U6IHRydWVcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9XHJcbiAgICBdKTsiLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKCQsIExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5GaWVsZHNldCA9IGZ1bmN0aW9uIChkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCwgbGVnZW5kLCBjb250ZW50VHlwZSwgY29udGVudFR5cGVMYWJlbCwgY29udGVudFR5cGVDbGFzcywgaGFzRWRpdG9yLCBjaGlsZHJlbikge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJGaWVsZHNldFwiLCBkYXRhLCBodG1sSWQsIGh0bWxDbGFzcywgaHRtbFN0eWxlLCBpc1RlbXBsYXRlZCk7XHJcbiAgICAgICAgTGF5b3V0RWRpdG9yLkNvbnRhaW5lci5jYWxsKHRoaXMsIFtcIkdyaWRcIiwgXCJDb250ZW50XCJdLCBjaGlsZHJlbik7XHJcblxyXG4gICAgICAgIHZhciBzZWxmID0gdGhpcztcclxuICAgICAgICB0aGlzLmlzQ29udGFpbmFibGUgPSB0cnVlO1xyXG4gICAgICAgIHRoaXMuZHJvcFRhcmdldENsYXNzID0gXCJsYXlvdXQtY29tbW9uLWhvbGRlclwiO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGUgPSBjb250ZW50VHlwZTtcclxuICAgICAgICB0aGlzLmNvbnRlbnRUeXBlTGFiZWwgPSBjb250ZW50VHlwZUxhYmVsO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVDbGFzcyA9IGNvbnRlbnRUeXBlQ2xhc3M7XHJcbiAgICAgICAgdGhpcy5sZWdlbmQgPSBsZWdlbmQgfHwgXCJcIjtcclxuICAgICAgICB0aGlzLmhhc0VkaXRvciA9IGhhc0VkaXRvcjtcclxuXHJcbiAgICAgICAgdGhpcy50b09iamVjdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IHRoaXMuZWxlbWVudFRvT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJlc3VsdC5sZWdlbmQgPSB0aGlzLmxlZ2VuZDtcclxuICAgICAgICAgICAgcmVzdWx0LmNoaWxkcmVuID0gdGhpcy5jaGlsZHJlblRvT2JqZWN0KCk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHZhciBnZXRFZGl0b3JPYmplY3QgPSB0aGlzLmdldEVkaXRvck9iamVjdDtcclxuICAgICAgICB0aGlzLmdldEVkaXRvck9iamVjdCA9IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgZHRvID0gZ2V0RWRpdG9yT2JqZWN0KCk7XHJcbiAgICAgICAgICAgIHJldHVybiAkLmV4dGVuZChkdG8sIHtcclxuICAgICAgICAgICAgICAgIExlZ2VuZDogdGhpcy5sZWdlbmRcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnNldENoaWxkcmVuID0gZnVuY3Rpb24gKGNoaWxkcmVuKSB7XHJcbiAgICAgICAgICAgIHRoaXMuY2hpbGRyZW4gPSBjaGlsZHJlbjtcclxuICAgICAgICAgICAgXyh0aGlzLmNoaWxkcmVuKS5lYWNoKGZ1bmN0aW9uIChjaGlsZCkge1xyXG4gICAgICAgICAgICAgICAgY2hpbGQucGFyZW50ID0gc2VsZjtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy5hcHBseUVsZW1lbnRFZGl0b3JNb2RlbCA9IGZ1bmN0aW9uKG1vZGVsKSB7XHJcbiAgICAgICAgICAgIHRoaXMubGVnZW5kID0gbW9kZWwubGVnZW5kO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4oY2hpbGRyZW4pO1xyXG4gICAgfTtcclxuXHJcbiAgICBMYXlvdXRFZGl0b3IuRmllbGRzZXQuZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHJldHVybiBuZXcgTGF5b3V0RWRpdG9yLkZpZWxkc2V0KFxyXG4gICAgICAgICAgICB2YWx1ZS5kYXRhLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sSWQsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbFN0eWxlLFxyXG4gICAgICAgICAgICB2YWx1ZS5pc1RlbXBsYXRlZCxcclxuICAgICAgICAgICAgdmFsdWUubGVnZW5kLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVMYWJlbCxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaGFzRWRpdG9yLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5yZWdpc3RlckZhY3RvcnkoXCJGaWVsZHNldFwiLCBmdW5jdGlvbih2YWx1ZSkge1xyXG4gICAgICAgIHJldHVybiBMYXlvdXRFZGl0b3IuRmllbGRzZXQuZnJvbSh2YWx1ZSk7XHJcbiAgICB9KTtcclxuXHJcbn0pKGpRdWVyeSwgTGF5b3V0RWRpdG9yIHx8IChMYXlvdXRFZGl0b3IgPSB7fSkpO1xyXG4iLCJ2YXIgTGF5b3V0RWRpdG9yO1xyXG4oZnVuY3Rpb24gKCQsIExheW91dEVkaXRvcikge1xyXG5cclxuICAgIExheW91dEVkaXRvci5Gb3JtID0gZnVuY3Rpb24gKGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkLCBuYW1lLCBmb3JtQmluZGluZ0NvbnRlbnRUeXBlLCBjb250ZW50VHlwZSwgY29udGVudFR5cGVMYWJlbCwgY29udGVudFR5cGVDbGFzcywgaGFzRWRpdG9yLCBjaGlsZHJlbikge1xyXG4gICAgICAgIExheW91dEVkaXRvci5FbGVtZW50LmNhbGwodGhpcywgXCJGb3JtXCIsIGRhdGEsIGh0bWxJZCwgaHRtbENsYXNzLCBodG1sU3R5bGUsIGlzVGVtcGxhdGVkKTtcclxuICAgICAgICBMYXlvdXRFZGl0b3IuQ29udGFpbmVyLmNhbGwodGhpcywgW1wiR3JpZFwiLCBcIkNvbnRlbnRcIl0sIGNoaWxkcmVuKTtcclxuXHJcbiAgICAgICAgdmFyIHNlbGYgPSB0aGlzO1xyXG4gICAgICAgIHRoaXMuaXNDb250YWluYWJsZSA9IHRydWU7XHJcbiAgICAgICAgdGhpcy5kcm9wVGFyZ2V0Q2xhc3MgPSBcImxheW91dC1jb21tb24taG9sZGVyXCI7XHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZSA9IGNvbnRlbnRUeXBlO1xyXG4gICAgICAgIHRoaXMuY29udGVudFR5cGVMYWJlbCA9IGNvbnRlbnRUeXBlTGFiZWw7XHJcbiAgICAgICAgdGhpcy5jb250ZW50VHlwZUNsYXNzID0gY29udGVudFR5cGVDbGFzcztcclxuICAgICAgICB0aGlzLm5hbWUgPSBuYW1lIHx8IFwiVW50aXRsZWRcIjtcclxuICAgICAgICB0aGlzLmZvcm1CaW5kaW5nQ29udGVudFR5cGUgPSBmb3JtQmluZGluZ0NvbnRlbnRUeXBlO1xyXG4gICAgICAgIHRoaXMuaGFzRWRpdG9yID0gaGFzRWRpdG9yO1xyXG5cclxuICAgICAgICB0aGlzLnRvT2JqZWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gdGhpcy5lbGVtZW50VG9PYmplY3QoKTtcclxuICAgICAgICAgICAgcmVzdWx0Lm5hbWUgPSB0aGlzLm5hbWU7XHJcbiAgICAgICAgICAgIHJlc3VsdC5mb3JtQmluZGluZ0NvbnRlbnRUeXBlID0gdGhpcy5mb3JtQmluZGluZ0NvbnRlbnRUeXBlO1xyXG4gICAgICAgICAgICByZXN1bHQuY2hpbGRyZW4gPSB0aGlzLmNoaWxkcmVuVG9PYmplY3QoKTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdmFyIGdldEVkaXRvck9iamVjdCA9IHRoaXMuZ2V0RWRpdG9yT2JqZWN0O1xyXG4gICAgICAgIHRoaXMuZ2V0RWRpdG9yT2JqZWN0ID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHZhciBkdG8gPSBnZXRFZGl0b3JPYmplY3QoKTtcclxuICAgICAgICAgICAgcmV0dXJuICQuZXh0ZW5kKGR0bywge1xyXG4gICAgICAgICAgICAgICAgRm9ybU5hbWU6IHRoaXMubmFtZSxcclxuICAgICAgICAgICAgICAgIEZvcm1CaW5kaW5nQ29udGVudFR5cGU6IHRoaXMuZm9ybUJpbmRpbmdDb250ZW50VHlwZVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuc2V0Q2hpbGRyZW4gPSBmdW5jdGlvbiAoY2hpbGRyZW4pIHtcclxuICAgICAgICAgICAgdGhpcy5jaGlsZHJlbiA9IGNoaWxkcmVuO1xyXG4gICAgICAgICAgICBfKHRoaXMuY2hpbGRyZW4pLmVhY2goZnVuY3Rpb24gKGNoaWxkKSB7XHJcbiAgICAgICAgICAgICAgICBjaGlsZC5zZXRQYXJlbnQoc2VsZik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHRoaXMub25EZXNjZW5kYW50QWRkZWQgPSBmdW5jdGlvbihlbGVtZW50KSB7XHJcbiAgICAgICAgICAgIHZhciBnZXRFZGl0b3JPYmplY3QgPSBlbGVtZW50LmdldEVkaXRvck9iamVjdDtcclxuICAgICAgICAgICAgZWxlbWVudC5nZXRFZGl0b3JPYmplY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgZHRvID0gZ2V0RWRpdG9yT2JqZWN0KCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gJC5leHRlbmQoZHRvLCB7XHJcbiAgICAgICAgICAgICAgICAgICAgRm9ybUJpbmRpbmdDb250ZW50VHlwZTogc2VsZi5mb3JtQmluZGluZ0NvbnRlbnRUeXBlXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLmFwcGx5RWxlbWVudEVkaXRvck1vZGVsID0gZnVuY3Rpb24obW9kZWwpIHtcclxuICAgICAgICAgICAgdGhpcy5uYW1lID0gbW9kZWwubmFtZTtcclxuICAgICAgICAgICAgdGhpcy5mb3JtQmluZGluZ0NvbnRlbnRUeXBlID0gbW9kZWwuZm9ybUJpbmRpbmdDb250ZW50VHlwZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLnNldENoaWxkcmVuKGNoaWxkcmVuKTtcclxuICAgIH07XHJcblxyXG4gICAgTGF5b3V0RWRpdG9yLkZvcm0uZnJvbSA9IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgIHJldHVybiBuZXcgTGF5b3V0RWRpdG9yLkZvcm0oXHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGEsXHJcbiAgICAgICAgICAgIHZhbHVlLmh0bWxJZCxcclxuICAgICAgICAgICAgdmFsdWUuaHRtbENsYXNzLFxyXG4gICAgICAgICAgICB2YWx1ZS5odG1sU3R5bGUsXHJcbiAgICAgICAgICAgIHZhbHVlLmlzVGVtcGxhdGVkLFxyXG4gICAgICAgICAgICB2YWx1ZS5uYW1lLFxyXG4gICAgICAgICAgICB2YWx1ZS5mb3JtQmluZGluZ0NvbnRlbnRUeXBlLFxyXG4gICAgICAgICAgICB2YWx1ZS5jb250ZW50VHlwZSxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVMYWJlbCxcclxuICAgICAgICAgICAgdmFsdWUuY29udGVudFR5cGVDbGFzcyxcclxuICAgICAgICAgICAgdmFsdWUuaGFzRWRpdG9yLFxyXG4gICAgICAgICAgICBMYXlvdXRFZGl0b3IuY2hpbGRyZW5Gcm9tKHZhbHVlLmNoaWxkcmVuKSk7XHJcbiAgICB9O1xyXG5cclxuICAgIExheW91dEVkaXRvci5yZWdpc3RlckZhY3RvcnkoXCJGb3JtXCIsIGZ1bmN0aW9uKHZhbHVlKSB7IHJldHVybiBMYXlvdXRFZGl0b3IuRm9ybS5mcm9tKHZhbHVlKTsgfSk7XHJcblxyXG59KShqUXVlcnksIExheW91dEVkaXRvciB8fCAoTGF5b3V0RWRpdG9yID0ge30pKTsiXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=