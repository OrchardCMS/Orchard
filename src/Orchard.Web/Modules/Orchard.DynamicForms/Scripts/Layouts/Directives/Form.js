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