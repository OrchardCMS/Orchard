
var saveLocal = function (localId) {
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

var loadActivities = function (localId) {
    var workflow = loadWorkflow(localId);
    if (workflow == null) {
        return;
    }
    
    // activities        
    for (var i = 0; i < workflow.activities.length; i++) {
        var activity = workflow.activities[i];

        // if an activity has been modified, update it
        if (updatedActivityState != null && updatedActivityClientId == activity.clientId) {
            activity.state = JSON.parse(updatedActivityState);
        }

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

var loadWorkflow = function(localId) {
    var workflow = sessionStorage.getItem(localId);

    if (!workflow) {
        return;
    }

    // deserialize
    workflow = JSON.parse(workflow);
    

    return workflow;
};

var getActivity = function(localId, clientId) {

    var workflow = loadWorkflow(localId);
    if (workflow == null) {
        return;
    }

    var activity = null;
    for (var i = 0; i < workflow.activities.length; i++) {
        var a = workflow.activities[i];
        if (a.clientId == clientId) {
            activity = a;
        }
    }

    return activity;
};

var loadForm = function(localId, clientId) {

    // bind state to form

    var activity = getActivity(localId, clientId);
    bindForm($('form'), activity.state);
};

var bindForm = function(form, data) {

    $.each(data, function (name, val) {
        var $el = $('[name="' + name + '"]'),
            type = $el.attr('type');

        switch (type) {
            case 'checkbox':
                $el.attr('checked', 'checked');
                break;
            case 'radio':
                $el.filter('[value="' + val + '"]').attr('checked', 'checked');
                break;
            default:
                $el.val(val);
        }
    });
};

/*
var serializeForm = function(form) {
    var data = {};

    $(form).find('select, input, textarea').each(function(index, el) {
        var $el = $(el);
        var name = $el.attr('name');
        var type = $el.attr('type');

        switch (type) {
            case 'checkbox':
                data[name] = $el.attr('checked') ? 'true' : false;
                break;
            case 'radio':
                var value = $el.filter('checked').attr('value');
                if (value) {
                    data[name] = value;
                }
                break;
            default:
                data[name] = $el.val();
        }
    });

    return data;
};

var saveState = function(localId, clientId, data) {
    var activity = getActivity(localId, clientId);
    activity.state = data;
    
    sessionStorage.setItem(localId, JSON.stringify(wokflow));
};
*/