var LayoutEditor;
(function ($, LayoutEditor) {

    var decode = function(value) {
        return !!value ? decodeURIComponent(value.replace(/\+/g, "%20")) : null;
    };

    var decodeGraph = function (graph) {

        if (!!graph.html) {
            graph.html = decode(graph.html);
        }

        if (!!graph.data) {
            var items = $.deserialize(graph.data);

            for (var i = 0; i < items.length; i++) {
                items[i] = decode(items[i]);
            }

            graph.data = $.param(items);
        }

        if (!!graph.children) {
            for (var i = 0; i < graph.children.length; i++) {
                decodeGraph(graph.children[i]);
            }
        }
    };

    LayoutEditor.decode = decode;
    LayoutEditor.decodeLayoutGraph = decodeGraph;

})(jQuery, LayoutEditor || (LayoutEditor = {}));