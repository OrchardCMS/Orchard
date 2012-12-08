
var saveLocal = function () {
    var wokflow = {
        activities: [],
        connections: []
    };

    var allActivities = $('.activity');
    for (var i = 0; i < allActivities.length; i++) {
        var activity = allActivities[i];

        wokflow.activities.push({
            name: activity.viewModel.name,
            clientId: activity.viewModel.clientId,
            state: activity.viewModel.state,
            left: $(activity).position().left,
            top: $(activity).position().top
        });
    }

    var allConnections = jsPlumb.getConnections();
    for (var i = 0; i < allConnections.length; i++) {
        var connection = allConnections[i];

        wokflow.connections.push({
            sourceId: connection.sourceId,
            targetId: connection.targetId,
            sourceEndpoint: connection.endpoints[0].outcome,
            //targetEndpoint: connection.targetEndpoint
        });
    }
    // serialize the object
    sessionStorage.setItem(localId, JSON.stringify(wokflow));
};

var loadActivities = function () {
    var workflow = sessionStorage.getItem(localId);

    if (!workflow) {
        return;
    }

    // deserialize
    workflow = JSON.parse(workflow);

    // activities        
    for (var i = 0; i < workflow.activities.length; i++) {
        var activity = workflow.activities[i];
        renderActivity(activity.clientId, activity.name, activity.state, activity.top, activity.left);
    }

    // connections
    for (var i = 0; i < workflow.connections.length; i++) {
        var connection = workflow.connections[i];

        var source = document.getElementById(connection.sourceId);
        var ep = source.endpoints[connection.sourceEndpoint];

        jsPlumb.connect({
            source: ep,
            target: connection.targetId,
            newConnection: true

        });
    }
};