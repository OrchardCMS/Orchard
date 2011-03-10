(function ($) {
    $(function () {
        // append the shape tracing window container at the end of the document
        $('<div id="shape-tracing-container"><div id="shape-tracing-toolbar">toolbar</div><div id="shape-tracing-window">window</div></div><div id="shape-tracing-container-ghost"/>').appendTo('body');

        // preload main objects
        var shapeTracingContainer = $('#shape-tracing-container');
        var shapeTracingToolbar = $('#shape-tracing-toolbar');
        var shapeTracingWindow = $('#shape-tracing-window');
        var shapeTracingGhost = $('#shape-tracing-container-ghost');

        // ensure the ghost has always the same size as the container
        shapeTracingContainer.resize(function () { shapeTracingGhost.height(shapeTracingContainer.height()); });
        shapeTracingContainer.trigger('resize');

    });
})(jQuery);
