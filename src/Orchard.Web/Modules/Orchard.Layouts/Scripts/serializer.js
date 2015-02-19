(function ($) {

    var serializer = {
        serialize: function (graph, scope) {
            scope = scope.wrap("<div></div>").parent();
            serializeInternal(graph, scope);
            return graph;
        },

        deserialize: function() {
            var layoutEditor = $(".layout-editor");
            var form = layoutEditor.closest("form");
            var stateFieldName = layoutEditor.data("Data-field-name");
            var stateField = form.find("input[name=\"" + stateFieldName + "\"]");
            return JSON.parse(stateField.val());
        }
    };

    var serializeInternal = function (graph, scope) {
        var children = scope.children();
        var index = 0;

        for (var i = 0; i < children.length; i++) {
            var child = $(children.get(i));
            var isElement = child.hasClass("x-element");
            var subGraph = graph;

            if (isElement) {
                var elementData = child.data("element");
                var elements = graph.elements = graph.elements || [];
                var element = {
                    typeName: elementData.typeName,
                    state: elementData.state,
                    settings: elementData.settings,
                    index: elementData.index || index,
                    isTemplated: elementData.isTemplated || false
                };
                elements.push(element);
                subGraph = element;
                index++;
            }

            serializeInternal(subGraph, child);
        }
    }

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Layouts = window.Orchard.Layouts || {};
    window.Orchard.Layouts.Serializer = serializer;
})(jQuery);