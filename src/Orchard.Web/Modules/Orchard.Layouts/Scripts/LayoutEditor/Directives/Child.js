angular
    .module("LayoutEditor")
    .directive("orcLayoutChild", function ($compile) {
        return {
            restrict: "E",
            scope: { element: "=" },
            link: function (scope, element) {
                var template = "<orc-layout-" + scope.element.type.toLowerCase() + " element='element' />";
                var html = $compile(template)(scope);
                $(element).replaceWith(html);
            }
        };
    });