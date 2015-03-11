var LayoutEditor;
(function (LayoutEditor) {

    var decode = function(value) {
        return !!value ? decodeURIComponent(value.replace(/\+/g, "%20")) : null;
    };

    var decodeGraph = function (graph) {
        var propertiesToDecode = ["data", "html"];

        for (var i = 0; i < propertiesToDecode.length; i++) {
            var prop = propertiesToDecode[i];
            var propVal = graph[prop];

            if(!!propVal)
                graph[prop] = decode(propVal);
        }

        if (!!graph.children) {
            for (var i = 0; i < graph.children.length; i++) {
                decodeGraph(graph.children[i]);
            }
        }
    };

    LayoutEditor.DecodeLayoutGraph = decodeGraph;

})(LayoutEditor || (LayoutEditor = {}));