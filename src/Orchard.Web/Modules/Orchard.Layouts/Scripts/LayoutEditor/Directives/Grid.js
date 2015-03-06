angular
    .module("LayoutEditor")
    .directive("orcLayoutGrid", ["$compile", "scopeConfigurator", "environment",
        function ($compile, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: function ($scope, $element) {
                    scopeConfigurator.configureForElement($scope, $element);
                    scopeConfigurator.configureForContainer($scope, $element);
                    $scope.sortableOptions["axis"] = "y";
                },
                templateUrl: environment.templateUrl("Grid"),
                replace: true
            };
        }
    ]);