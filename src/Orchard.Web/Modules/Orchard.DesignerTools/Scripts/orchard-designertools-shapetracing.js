 // declare global metadata host
if (!window.shapeTracingMetadataHost) {
    window.shapeTracingMetadataHost = {};
    window.shapeTracingMetadataHost.placement = {
        'n/a': 'n/a'
    };
}

jQuery(function ($) {

    // default shape window height when first opened
    var defaultHeight = 200;

    // preload main objects
    var shapeTracingContainer = $('#shape-tracing-container');
    var shapeTracingResizeHandle = $('#shape-tracing-resize-handle');
    var shapeTracingToolbar = $('#shape-tracing-toolbar');
    var shapeTracingToolbarSwitch = $('#shape-tracing-toolbar-switch');
    var shapeTracingWindow = $('#shape-tracing-window');
    var shapeTracingWindowTree = $('#shape-tracing-window-tree');
    var shapeTracingWindowContent = $('#shape-tracing-window-content');
    var shapeTracingGhost = $('#shape-tracing-container-ghost');
    var shapeTracingOverlay = $('#shape-tracing-overlay');
    var shapeTracingTabs = $('#shape-tracing-tabs');
    var shapeTracingTabsShape = $('#shape-tracing-tabs-shape');
    var shapeTracingTabsModel = $('#shape-tracing-tabs-model');
    var shapeTracingTabsPlacement = $('#shape-tracing-tabs-placement');
    var shapeTracingTabsTemplate = $('#shape-tracing-tabs-template');
    var shapeTracingTabsHtml = $('#shape-tracing-tabs-html');
    var shapeTracingBreadcrumb = $('#shape-tracing-breadcrumb');
    var shapeTracingMetaContent = $('#shape-tracing-meta-content');
    var shapeTracingEnabled = false;

    // store the size of the container when it is closed (default in css)
    var initialContainerSize = shapeTracingContainer.height();
    var previousSize = 0;

    // represents the arrow to add to any collpasible container
    var glyph = '<span class="expando-glyph-container closed"><span class="expando-glyph"></span>&#8203;</span>';

    // ensure the ghost has always the same size as the container
    // and the container is always positionned correctly
    var syncResize = function () {
        var _window = $(window);
        var containerHeight = shapeTracingContainer.outerHeight();
        var containerWidth = shapeTracingContainer.outerWidth();
        var toolbarHeight = shapeTracingToolbar.outerHeight();
        var resizeHandleHeight = shapeTracingResizeHandle.outerHeight();

        shapeTracingGhost.height(containerHeight);

        var windowHeight = _window.height();
        var scrollTop = _window.scrollTop();
        var containerWindowHeight = containerHeight - toolbarHeight - resizeHandleHeight;

        shapeTracingContainer.offset({ top: windowHeight - containerHeight + scrollTop, left: 0 });
        shapeTracingWindow.height(containerWindowHeight);
        shapeTracingWindowTree.height(containerWindowHeight);
        shapeTracingWindowContent.height(containerWindowHeight);
        shapeTracingContainer.width('100%');

        syncResizeMeta();
    };

    // forces the content meta zone's height to enable scrollbar
    var syncResizeMeta = function () {
        var containerHeight = shapeTracingContainer.outerHeight();
        var containerWidth = shapeTracingContainer.outerWidth();
        var toolbarHeight = shapeTracingToolbar.outerHeight();
        var resizeHandleHeight = shapeTracingResizeHandle.outerHeight();

        var tabsHeight = shapeTracingTabs.outerHeight();
        var breadcrumbHeight = shapeTracingBreadcrumb.outerHeight();
        if (tabsHeight) {
            padding = parseInt(shapeTracingMetaContent.css('padding-bottom') + shapeTracingMetaContent.css('padding-top'));
            shapeTracingMetaContent.height(containerHeight - toolbarHeight - resizeHandleHeight - tabsHeight - breadcrumbHeight - padding);
        }
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
            shapeTracingContainer.height(Math.max(previousSize, defaultHeight, shapeTracingContainer.height()));
            enableShapeTracing();
        }
        else {
            // save previous height
            previousSize = shapeTracingContainer.height();
            shapeTracingContainer.height(initialContainerSize);
            disableShapeTracing();
        }

        syncResize();
    });

    var disableShapeTracing = function () {
        shapeTracingEnabled = false;
        selectShape();
    }

    var enableShapeTracing = function () {
        shapeTracingEnabled = true;
    }

    // add a resizable handle to the container
    $('#shape-tracing-resize-handle').addClass('ui-resizable-handle ui-resizable-n');
    shapeTracingContainer.resizable({
        handles: { n: '#shape-tracing-resize-handle' },
        grid: 20, // mitigates the number of calls to syncResize()
        resize: function () { shapeTracingEnabled = false },
        stop: function () { shapeTracingEnabled = true }
    });

    var shapeNodes = {}; // represents the main index of shape nodes, indexed by id

    // projects the shape ids to each DOM element
    var startShapeTracingBeacons = $('.shape-tracing-wrapper[shape-id]');
    startShapeTracingBeacons.each(function () {
        var _this = $(this);

        var shapeNode = {
            id: _this.attr('shape-id'),
            type: _this.attr('shape-type'),
            hint: _this.attr('shape-hint'),
            parent: null,
            children: {}
        };

        // register the new shape node into the main shape nodes index
        shapeNodes[shapeNode.id] = shapeNode;

        // assign the shape-id attribute to all direct children, except wrappers themselves (it would erase their own shape-id)
        var found = false;
        _this
            .nextUntil('[end-of="' + shapeNode.id + '"]') // all elements between the script beacons
            .find(':not(.shape-tracing-wrapper)') // all children but not inner beacons
            .andSelf() // add the first level items
            .attr('shape-id', shapeNode.id) // add the shape-id attribute
            .each(function () {
                // assign a shapeNode instance to the DOM element
                this.shapeNode = shapeNode;
                found = true;
            });

        // if the shape is empty, add a hint
        if (!found) {
            shapeNode.hint = 'empty';
        }
        this.shapeNode = shapeNode;
    });

    // construct the shape tree based on all current nodes
    // for each start beacon, search for the first parent beacon, and create nodes if they don't exist
    startShapeTracingBeacons.each(function () {
        var _this = $(this);
        var shapeNode = this.shapeNode;
        var parent = _this.parents('[shape-id!=' + shapeNode.id + ']').get(0);

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

    // add first level shapes tree nodes
    var shapes = $('<ul></ul>');
    for (var shapeId in shapeNodes) {
        if (!shapeNodes[shapeId].parent) {
            shapes.append(createTreeNode(shapeNodes[shapeId]));
        }
    }

    shapeTracingWindowTree.append(shapes);

    // remove empty list items
    shapeTracingWindowTree.find('ul:empty').remove();

    // add the expand/collapse logic to the shapes tree
    shapeTracingWindowTree.find('li:has(ul)').prepend($(glyph));

    // collapse all sub uls
    shapeTracingWindowTree.find('ul ul').toggle(false);

    // expands a list of shapes in the tree
    var openExpando = function (expando) {
        if (expando.hasClass("closed")) {
            expando.siblings('ul').toggle(true);
            expando.removeClass("closed").addClass("open");
        }
    }

    // collapses a list of shapes in the tree
    var closeExpando = function (expando) {
        if (expando.hasClass("open")) {
            expando.siblings('ul').toggle(false);
            expando.removeClass("open").addClass("closed");
        }
    }

    shapeTracingWindow.add(shapeTracingResizeHandle).hover(function () {
        shapeTracingOverlay.hide();
    }, function () {
        shapeTracingOverlay.show();
    }
    );

    //create an overlay on shapes' descendants
    var overlayTarget = null;
    $('[shape-id]').add(shapeTracingOverlay).mousemove(
        function (event) {
            if (!shapeTracingEnabled) {
                return;
            }

            event.stopPropagation();

            if ($(this).get(0) == shapeTracingOverlay.get(0)) {
                shapeTracingOverlay.hide();
            }

            var element = document.elementFromPoint(event.pageX - $(window).scrollLeft(), event.pageY - $(window).scrollTop());
            shapeTracingOverlay.show();


            while (element && !element.shapeNode)
                element = element.parentNode;

            if (!element || (overlayTarget != null && overlayTarget.get(0) == element)) {
                return;
            }

            element = $(element);
            shapeTracingOverlay.offset(element.offset());
            shapeTracingOverlay.width(element.outerWidth()); // include border and padding 
            shapeTracingOverlay.height(element.outerHeight()); // include border and padding 

            overlayTarget = element;
        }
    );

    var currentShape;

    // selects a specific shape in the tree, highlight its elements, and display the current tab
    var selectShape = function (shapeId) {
        // remove current tab content
        shapeTracingMetaContent.children().remove();

        // remove selection ?
        if (!shapeId) {
            currentShape = null;
            shapeTracingOverlay.hide();
            $('.shape-tracing-selected').removeClass('shape-tracing-selected');
            shapeTracingWindowTree.find('.shape-tracing-selected').removeClass('shape-tracing-selected');
            return;
        }

        currentShape = shapeId;

        $('.shape-tracing-selected').removeClass('shape-tracing-selected');
        $('li[tree-shape-id="' + shapeId + '"] > div').add('[shape-id="' + shapeId + '"]').addClass('shape-tracing-selected');
        shapeTracingOverlay.hide();

        defaultTab();
    }

    // select shapes when clicked
    shapeTracingOverlay.click(function () {
        var shapeNode = overlayTarget.get(0).shapeNode;
        selectShape(shapeNode.id);

        var lastExpanded = null;
        // open the tree until the selected element
        $('li[tree-shape-id="' + shapeNode.id + '"]').parents('li').andSelf().find('> .expando-glyph-container').each(function () {
            openExpando($(this));
        }).each(function () {
            shapeTracingWindowTree.scrollTo(this, 0, { margin: true });
        });

        return false;
    });

    //create an overlay on shape tree nodes
    shapeTracingWindowTree.find('[tree-shape-id] > div')
    .hover(
        function () {
            var _this = $(this);
            $('.shape-tracing-overlay').removeClass('shape-tracing-overlay');
            _this.addClass('shape-tracing-overlay');
        },
        function () {
            $('.shape-tracing-overlay').removeClass('shape-tracing-overlay');
        })
    .click(function (event) {
        var shapeId = $(this).parent().get(0).shapeNode.id;
        selectShape(shapeId);

        var element = $('[shape-id="' + shapeId + '"]').get(0);
        // there might be no DOM element if the shape was empty, or is not displayed
        if (element) {
            $(window).scrollTo(element, 500, { margin: true });
        }

        event.stopPropagation();
    });

    // move all shape tracing meta blocks to the content window
    $("[shape-id-meta]").detach().prependTo(shapeTracingWindowContent);

    // remove empty list items
    shapeTracingWindowContent.find('ul:empty').remove();

    // add the expand/collapse logic to the shape model
    shapeTracingWindowContent.find('ul:has(ul)').prepend($(glyph));

    // collapse all sub uls
    shapeTracingWindowContent.find('ul ul').toggle(false);

    // Shape tab
    var displayTabShape = function () {
        // toggle the selected class
        shapeTracingTabs.children('.selected').removeClass('selected');
        shapeTracingTabsShape.addClass('selected');

        // remove old content
        shapeTracingMetaContent.children().remove();

        // render the template
        if (currentShape && shapeTracingMetadataHost[currentShape]) {
            $("#shape-tracing-tabs-shape-template").tmpl(shapeTracingMetadataHost[currentShape]).appendTo(shapeTracingMetaContent);
        }

        shapeTracingBreadcrumb.text('');

        // remove empty list items
        shapeTracingMetaContent.find('ul:empty').remove();

        // create collapsible containers
        shapeTracingMetaContent.find('li:has(ul)').prepend($(glyph));
        shapeTracingMetaContent.find('ul ul').toggle(false);
        shapeTracingMetaContent.find('.expando-glyph-container').click(expandCollapseExpando);

        $('#activeTemplate').click(function () {
            displayTabTemplate();
        });

        defaultTab = displayTabShape;
    };

    var defaultTab = displayTabShape;

    shapeTracingTabsShape.click(function () {
        displayTabShape();
    });

    // Model tab
    var displayTabModel = function () {
        // toggle the selected class
        shapeTracingTabs.children('.selected').removeClass('selected');
        shapeTracingTabsModel.addClass('selected');

        // remove old content
        shapeTracingMetaContent.children().remove();

        // render the template
        if (currentShape && shapeTracingMetadataHost[currentShape]) {
            $("#shape-tracing-tabs-model-template").tmpl(shapeTracingMetadataHost[currentShape].shape.model).appendTo(shapeTracingMetaContent);
        }

        shapeTracingBreadcrumb.text('');

        // remove empty list items
        shapeTracingMetaContent.find('ul:empty').remove();

        // create collapsible containers
        shapeTracingMetaContent.find('li:has(ul)').prepend($(glyph));
        shapeTracingMetaContent.find('ul ul').toggle(false);
        shapeTracingMetaContent.find('.expando-glyph-container').click(expandCollapseExpando);

        shapeTracingMetaContent.find('.model div.name')
        .hover(
            function () {
                var _this = $(this);
                $('.shape-tracing-overlay').removeClass('shape-tracing-overlay');
                _this.addClass('shape-tracing-overlay');
            },
            function () {
                $('.shape-tracing-overlay').removeClass('shape-tracing-overlay');
            })
        .click(function (event) {
            // model node is selected
            var _this = $(this);
            shapeTracingWindowContent.find('.shape-tracing-selected').removeClass('shape-tracing-selected');
            _this.addClass('shape-tracing-selected');

            // display breadcrumb
            var breadcrumb = null;
            _this.parentsUntil('.model').children('.name').each(function () {
                if (breadcrumb != null) {
                    breadcrumb = $(this).text() + '.' + breadcrumb;
                }
                else {
                    breadcrumb = $(this).text();
                }
            });

            // fix enumerable properties display
            breadcrumb = breadcrumb.replace('.[', '[');

            shapeTracingBreadcrumb.text('@' + breadcrumb);
            event.stopPropagation();
        });

        // open the root node (Model)
        shapeTracingMetaContent.find('.expando-glyph-container:first').click();

        defaultTab = displayTabModel;
    };

    shapeTracingTabsModel.click(function () {
        displayTabModel();
    });

    // Placement tab
    var displayTabPlacement = function () {
        // toggle the selected class
        shapeTracingTabs.children('.selected').removeClass('selected');
        shapeTracingTabsPlacement.addClass('selected');

        // remove old content
        shapeTracingMetaContent.children().remove();

        // render the template
        if (currentShape && shapeTracingMetadataHost[currentShape]) {
            var placementSource = shapeTracingMetadataHost[currentShape].shape.placement;
            shapeTracingBreadcrumb.text(placementSource);
            $("#shape-tracing-tabs-placement-template").tmpl(shapeTracingMetadataHost.placement[placementSource]).appendTo(shapeTracingMetaContent);
        }
        else {
            shapeTracingBreadcrumb.text('');
        }

        enableCodeMirror(shapeTracingMetaContent);
        defaultTab = displayTabPlacement;
    };

    shapeTracingTabsPlacement.click(function () {
        displayTabPlacement();
    });

    // Template tab
    var displayTabTemplate = function () {
        // toggle the selected class
        shapeTracingTabs.children('.selected').removeClass('selected');
        shapeTracingTabsTemplate.addClass('selected');

        // remove old content
        shapeTracingMetaContent.children().remove();

        // render the template
        if (currentShape && shapeTracingMetadataHost[currentShape]) {
            shapeTracingBreadcrumb.text(shapeTracingMetadataHost[currentShape].shape.template);
            $("#shape-tracing-tabs-template-template").tmpl(shapeTracingMetadataHost[currentShape].shape.templateContent).appendTo(shapeTracingMetaContent);
        }
        else {
            shapeTracingBreadcrumb.text('');
        }

        enableCodeMirror(shapeTracingMetaContent);
        defaultTab = displayTabTemplate;
    };

    shapeTracingTabsTemplate.click(function () {
        displayTabTemplate();
    });

    // HTML tab
    var displayTabHtml = function () {
        // toggle the selected class
        shapeTracingTabs.children('.selected').removeClass('selected');
        shapeTracingTabsHtml.addClass('selected');

        // remove old content
        shapeTracingMetaContent.children().remove();

        // render the template
        if (currentShape && shapeTracingMetadataHost[currentShape]) {
            $("#shape-tracing-tabs-html-template").tmpl(shapeTracingMetadataHost[currentShape].shape.html).appendTo(shapeTracingMetaContent);
        }

        shapeTracingBreadcrumb.text('');

        enableCodeMirror(shapeTracingMetaContent);
        defaultTab = displayTabHtml;
    };

    shapeTracingTabsHtml.click(function () {
        displayTabHtml();
    });

    // activates codemirror on specific textareas
    var enableCodeMirror = function (target) {
        // if there is a script, and colorization is not enabled yet, turn it on
        // code mirror seems to work only if the textarea is visible
        target.find('textarea:visible').each(function () {
            if ($(this).next('.CodeMirror').length == 0) {
                CodeMirror.fromTextArea(this, { mode: "razor", tabMode: "indent", readOnly: true, lineNumbers: true });
            }
        });
    }

    // hooks the click event on expandos
    var expandCollapseExpando = function () {
        var _this = $(this);
        if (_this.hasClass("closed")) {
            openExpando(_this);
        }
        else {
            closeExpando(_this);
        }

        return false;
    };

    // automatically expand or collapse shapes in the tree
    shapeTracingWindowTree.find('.expando-glyph-container').click(expandCollapseExpando);

    // recursively create a node for the shapes tree
    function createTreeNode(shapeNode) {
        var node = $('<li></li>');
        node.attr('tree-shape-id', shapeNode.id);
        node.get(0).shapeNode = shapeNode;

        var text = shapeNode.type;
        // add the hint to the tree node if available
        if (shapeNode.hint && shapeNode.hint != '') {
            text += ' [' + shapeNode.hint + ']';
        }

        node.append('<div>' + text + '</div>');
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

});
