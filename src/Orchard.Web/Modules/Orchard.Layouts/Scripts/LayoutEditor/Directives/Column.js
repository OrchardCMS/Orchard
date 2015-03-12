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