jQuery(function ($) {

    $("form").bind("orchard-admin-contentpicker-open", function (ev, data) {
        data = data || {};
        // the popup will be doing full page reloads, so will not be able to retain
        // a pointer to the callback. We will generate a temporary callback
        // with a known/unique name and pass that in on the querystring so it
        // is remembers across reloads. Once executed, it calls the real callback
        // and removes itself.
        var callbackName = "_contentpicker_" + new Date().getTime();
        data.callbackName = callbackName;
        $[callbackName] = function (returnData) {
            delete $[callbackName];
            data.callback(returnData);
        };
        $[callbackName].data = data;

        var baseUrl = data.baseUrl;

        // remove trailing slash if any
        if (baseUrl.substr(-1) == '/')
            baseUrl = baseUrl.substr(0, baseUrl.length - 1);
        
        var url = baseUrl
            + "/Admin/Orchard.ContentPicker?"
            + "callback=" + callbackName
            + "&" + (new Date() - 0);
        var w = window.open(url, "_blank", data.windowFeatures || "width=685,height=700,status=no,toolbar=no,location=no,menubar=no,resizable=no,scrollbars=yes");
    });
});