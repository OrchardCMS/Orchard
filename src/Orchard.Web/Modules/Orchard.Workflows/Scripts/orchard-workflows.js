    var connectorPaintStyle = {
        lineWidth: 3,
        strokeStyle: "grey",
        joinstyle: "round",
        //outlineColor: "white",
        //outlineWidth: 7
    };

    var connectorHoverStyle = {
        lineWidth: 3,
        strokeStyle: "#2e2aF8"
    };

    var sourceEndpointOptions = {
        endpoint: "Dot",
        paintStyle: { fillStyle: "#225588", radius: 7 },
        isSource: true,
        isTarget: false,
        connector: ["Flowchart"], // gap needs to be the same as makeTarget.paintStyle.radius
        connectorStyle: connectorPaintStyle,
        hoverPaintStyle: connectorHoverStyle,
        connectorHoverStyle: connectorHoverStyle,
        overlays: [["Label", { location: [0.5, 1.5], cssClass: "sourceEndpointLabel" }]]
    };

    jsPlumb.bind("ready", function () {

        jsPlumb.importDefaults({
            Anchor : "Continuous",
            // default drag options
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            // default to blue at one end and green at the other
            EndpointStyles: [{ fillStyle: '#225588' }],
            // blue endpoints 7 px; Blank endpoints.
            Endpoints: [["Dot", { radius: 7 }], ["Blank"]],
            // the overlays to decorate each connection with.  note that the label overlay uses a function to generate the label text; in this
            // case it returns the 'labelText' member that we set on each connection in the 'init' method below.
            ConnectionOverlays: [
                ["Arrow", { width: 12, length: 12, location: 1 }],
                // ["Label", { location: 0.1, id: "label", cssClass: "aLabel" }]
            ],
            ConnectorZIndex: 5
        });

        // updates the state of any edited activity
        updateActivities(localId);

        // deserialize the previously locally saved workflow
        loadActivities(localId);
        
        // a new connection is created
        jsPlumb.bind("jsPlumbConnection", function (connectionInfo) {
            // ...update your data model here.  The contents of the 'connectionInfo' are described below.
        });

        // a connection is detached
        jsPlumb.bind("jsPlumbConnectionDetached", function (connectionInfo) {
            // ...update your data model here.  The contents of the 'connectionInfo' are described below.
        });

    });


    // instanciates a new workflow widget in the editor
    var createActivity = function (activityName) {
        renderActivity(null, activityName, {}, 10, 10);
    };

    
    // create a new activity node on the editor
    $('.activity-toolbox-item').on('click', function () {
        var self = $(this);
        var activityName = self.data('activity-name');
        createActivity(activityName);
    });

    var renderActivity = function (clientId, name, state, top, left) {

        $.ajax({
            type: 'POST',
            url: renderActivityUrl,
            data: { name: name, state: state, __RequestVerificationToken: requestAntiForgeryToken },
            async: false,
            success: function(data) {
                var dom = $(data);

                if (dom == null) {
                    return null;
                }

                dom.addClass('activity');

                if (clientId) {
                    dom.attr('id', clientId);
                }

                var editor = $('#activity-editor');
                editor.append(dom);

                jsPlumb.draggable(dom, { containment: "parent", scroll: true });

                jsPlumb.makeTarget(dom, {
                    dropOptions: { hoverClass: "dragHover" },
                    anchor: "Continuous",
                    endpoint: "Blank",
                    paintStyle: { fillStyle: "#558822", radius: 3 },
                });

                var elt = dom.get(0);
                elt.viewModel = {
                    name: name,
                    state: state,
                    clientId: dom.attr("id"),
                };

                elt.endpoints = {};

                var outcomes = activities[name].outcomes;
                for (i = 0; i < outcomes.length; i++) {
                    var ep = jsPlumb.addEndpoint(dom, {
                            anchor: "Continuous",
                            connectorOverlays: [["Label", { label: outcomes[i], cssClass: "connection-label" }]],
                        },
                        sourceEndpointOptions);

                    elt.endpoints[outcomes[i]] = ep;
                    ep.outcome = outcomes[i];
                    //ep.setLabel(outcomes[i]);
                }

                if (activities[name].hasForm) {
                    dom.dblclick(function() {
                        saveLocal(localId);
                        window.location.href = editActivityUrl + "/" + $("#id").val() + "?name=" + name + "&clientId=" + elt.viewModel.clientId + "&localId=" + localId;
                    });
                }

                dom.css('top', top + 'px');
                dom.css('left', left + 'px');
                jsPlumb.repaint(elt.viewModel.clientId);
            }
        });

    };


    

