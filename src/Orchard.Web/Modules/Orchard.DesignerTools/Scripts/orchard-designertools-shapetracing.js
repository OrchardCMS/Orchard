(function ($) {
    $(function () {
        // append the shape tracing window container at the end of the document
        $('<div id="shape-tracing-container"> ' +
                '<div id="shape-tracing-resize-handle" ></div>' +
                '<div id="shape-tracing-toolbar">' +
                    '<div id="shape-tracing-toolbar-switch"></div>' +
                '</div>' +
                '<div id="shape-tracing-window"></div>' +
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
            shapeTracingGhost.height(containerHeight);

            var windowHeight = _window.height();
            var scrollTop = _window.scrollTop();

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

        var shapeNodes = {}; // represents the main index of shape nodes, indexed by id

        // projects the shape ids to each DOM element
        var startShapeTracingBeacons = $('.shape-tracing-wrapper[shape-id]');
        startShapeTracingBeacons.each(function () {
            var _this = $(this);

            var shapeNode = {
                id: _this.attr('shape-id'),
                type: _this.attr('shape-type'),
                parent: null,
                children: {}
            };

            // register the new shape node into the main shape nodes index
            shapeNodes[shapeNode.id] = shapeNode;

            // assign the shape-id attribute to all elements, except wrappers themselves (it would erase their own shape-id)
            _this
                .nextUntil('[end-of="' + shapeNode.id + '"]') // all elements between the script beacons
                .find(':not(.shape-tracing-wrapper)') // all children but not inner beacons
                .andSelf() // add the first level items
                .attr('shape-id', shapeNode.id) // add the shape-id attribute
                .each(function () {
                    // assign a shapeNode instance to the DOM element
                    this.shapeNode = shapeNode;
                });

            this.shapeNode = shapeNode;
        });

        // construct the shape tree based on all current nodes
        // for each start beacon, search for the first parent beacon, and create nodes if they don't exist
        startShapeTracingBeacons.each(function () {
            var _this = $(this);
            var shapeNode = this.shapeNode;
            var parent = _this.parent('[shape-id!=' + shapeNode.id + ']').get(0);

            shapeNodes[shapeNode.id] = shapeNode;

            if (parent.shapeNode) {
                var parentShapeNode = parent.shapeNode;
                shapeNodes[parentShapeNode.id] = parentShapeNode;
                shapeNode.parent = parentShapeNode;
                parentShapeNode.children[shapeNode.id] = shapeNode;
            }
        });

        // removes all beacons as we don't need them anymore
        $('.shape-tracing-wrapper').remove();

        var shapes = $('<ul></ul>');
        for (var shapeId in shapeNodes) {
            if (!shapeNodes[shapeId].parent) {
                shapes.append(createTreeNode(shapeNodes[shapeId]));
            }
        }
        shapeTracingWindow.append(shapes);

        //create an overlay on shapes' descendants
        $('[shape-id]').hover(
            function () {
                $('.shape-tracing-overlay').removeClass('shape-tracing-overlay');
                $(this).addClass('shape-tracing-overlay');
            },
            function () {
                $(this).removeClass('shape-tracing-overlay');
            }
        );
    });

    function createTreeNode(shapeNode) {
        var node = $('<li></li>');
        node.text(shapeNode.type);
        var list = $('<ul></ul>');
        node.append(list);
        if (shapeNode.children) {
            for (var shapeId in shapeNode.children) {
                var child = createTreeNode(shapeNode.children[shapeId]);
                list.append(child);
            }
        }
        return node;
    }

})(jQuery);
