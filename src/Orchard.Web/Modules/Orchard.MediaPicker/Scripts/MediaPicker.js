(function () {
    var eventPrefix = "orchard.admin.pickimage-open.",
        pickerAction = "/MediaPicker",
        defaultFeatures = "width=800,height=600,status=no,toolbar=no,location=no,menubar=no,resizable=no";

    OpenAjax.hub.subscribe(eventPrefix + "*", function (name, data) {
        var adminIndex = location.href.toLowerCase().indexOf("/admin/");
        if (adminIndex === -1) return;
        var url = location.href.substr(0, adminIndex)
            + pickerAction + "?source=" + name.substr(eventPrefix.length)
            + "&upload=" + (data.uploadMediaAction || "")
            + "&uploadpath=" + (data.uploadMediaPath || "")
            + "&editmode=" + (!!(data.img && data.img.src))
            + "&editorId=" + data.editorId + "&" + (new Date() - 0);
        var w = window.open(url, "_blank", data.windowFeatures || defaultFeatures);
        if (w.jQuery && w.jQuery.mediaPicker) {
            w.jQuery.mediaPicker.init(data);
        }
        else {
            w.mediaPickerData = data;
        }
    });
})();

//// Or, with jQuery
//(function ($) {
//    $.orchardHub = $.orchardHub || {}; // this part would be built into the admin.js script or something

//    $($.orchardHub).bind("orchard-admin-pickimage", function(ev, data) {
//        var adminIndex = location.href.toLowerCase().indexOf("/admin/");
//        if (adminIndex === -1) return;
//        var url = location.href.substr(0, adminIndex)
//            + "/Orchard.MediaPicker/MediaPicker/Index?source=" + data.source
//            + "&upload=" + data.uploadMediaAction
//            + "&editorId=" + data.editorId + "&" + (new Date() - 0);
//        var w = window.open(url, "Orchard.MediaPicker", data.windowFeatures || "width=600,height=300,status=no,toolbar=no,location=no,menubar=no");
//        // in case it was already open, bring to the fore
//        w.focus();
//    });
//})(jQuery);
