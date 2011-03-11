(function ($) {
    $(function () {
        // append the shape tracing window container at the end of the document
        $('<div id="shape-tracing-container"> ' +
                '<div id="shape-tracing-resize-handle" ></div>' +
                '<div id="shape-tracing-toolbar">' +
                    '<div id="shape-tracing-toolbar-switch"></div>' +
                '</div>' +
                '<div id="shape-tracing-window">window</div>' +
            '</div>' +
            '<div id="shape-tracing-container-ghost"></div>'
        ).appendTo('body');

        // preload main objects
        var shapeTracingContainer = $('#shape-tracing-container');
        var shapeTracingToolbar = $('#shape-tracing-toolbar');
        var shapeTracingToolbarSwitch = $('#shape-tracing-toolbar-switch');
        var shapeTracingWindow = $('#shape-tracing-window');
        var shapeTracingGhost = $('#shape-tracing-container-ghost');

        // store the size of the container when it is closed (default in css)
        var initialContainerSize = shapeTracingContainer.height();
        var previousSize = 0;

        // ensure the ghost has always the same size as the container
        // and the container is always positionned correctly
        var syncResize = function () {
            var _window = $(window);
            var containerHeight = shapeTracingContainer.height();
            var windowHeight = _window.height();
            var scrollTop = _window.scrollTop();

            shapeTracingGhost.height(containerHeight);
            shapeTracingContainer.offset({ top: windowHeight - containerHeight + scrollTop, left: 0 });
            shapeTracingContainer.width('100%');
        };

        // ensure the size/position is correct whenver the container or the browser is resized
        shapeTracingContainer.resize(syncResize);
        $(window).resize(syncResize);
        $(window).resize();

        // removes the position flickering by hiding it first, then showing when ready
        shapeTracingContainer.show();

        // expand/collapse behavior
        // ensure the container has always a valid size when expanded
        shapeTracingToolbarSwitch.click(function () {
            var _this = $(this);
            _this.toggleClass('expanded');
            if (_this.hasClass('expanded')) {
                shapeTracingContainer.height(Math.max(previousSize, 100));
            }
            else {
                // save previous height
                previousSize = shapeTracingContainer.height();
                shapeTracingContainer.height(initialContainerSize);
            }

            syncResize();
        });

        // add a resizable handle to the container
        $('#shape-tracing-resize-handle').addClass('ui-resizable-handle ui-resizable-n');
        shapeTracingContainer.resizable({ handles: { n: '#shape-tracing-resize-handle'} });

        // projects the shape ids to each DOM element
        var shapeTracingWrappers = $('.shape-tracing-wrapper');
        shapeTracingWrappers.each(function () {
            var _this = $(this);
            var shapeId = _this.attr('shape-id');
            // assign the shape-id attribute to all children, except wrappers (it would erase their own shape-id)
            _this.find(':not(div.shape-tracing-wrapper)').attr('shape-id', shapeId);
        });

        // removes all wrappers, by unwrapping the first element of each of them
        shapeTracingWrappers.each(function () {
            $(this).contents().first().unwrap();
        });

        // create an overlay on shapes' descendants
        $('[shape-id]').hover(
            function () {
                $('*').removeClass('shape-tracing-overlay');
                $(this).addClass('shape-tracing-overlay');
            },
            function () {
                $(this).removeClass('shape-tracing-overlay');
            }
        );

    });
})(jQuery);
