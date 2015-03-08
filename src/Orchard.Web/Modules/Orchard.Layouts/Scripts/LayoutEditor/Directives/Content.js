angular
    .module("LayoutEditor")
    .directive("orcLayoutContent", ["$sce", "scopeConfigurator", "environment",
        function ($sce, scopeConfigurator, environment) {
            return {
                restrict: "E",
                scope: { element: "=" },
                controller: function ($scope, $element) {
                    scopeConfigurator.configureForElement($scope, $element);
                    $scope.edit = function () {
                        $scope.$root.editElement($scope.element).then(function (args) {
                            $scope.$apply(function () {
                                if (args.cancel)
                                    return;

                                $scope.element.data = decodeURIComponent(args.element.data);
                                $scope.element.setHtml(decodeURIComponent(args.element.html.replace(/\+/g, "%20")));
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

                    $scope.element.setHtml(decodeURIComponent($scope.element.html.replace(/\+/g, "%20")));
                },
                templateUrl: environment.templateUrl("Content"),
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