tinymce.PluginManager.add('orchardcontentlinks', function (editor, url) {
    var contentPickerAction = function () {
        var data = {};
        var callbackName = "_contentpicker_" + new Date().getTime();
        data.callbackName = callbackName;
        $[callbackName] = function (returnData) {
            delete $[callbackName];
            //the code to wrap text with link
            var textLink = editor.selection.getContent();
            if (!textLink || textLink == "") {
                textLink = returnData.displayText;
            }
            editor.insertContent("<a href=\"#{ContentItem.Id:" + returnData.id.toString() + ".DisplayUrl}\">" + textLink + "</a>");

        };
        $[callbackName].data = data;

        // Open content picker window
        var baseUrl = baseOrchardPath;

        // remove trailing slash if any
        if (baseUrl.slice(-1) == '/')
            baseUrl = baseUrl.substr(0, baseUrl.length - 1);
        var url = baseUrl + "/Admin/Orchard.ContentPicker?";
        url += "callback=" + callbackName + "&" + (new Date() - 0);
        if ($("#" + editor.id).data("content-types")) {
            url += "&types=" + $("#" + editor.id).data("content-types");
        }
        var w = window.open(url, "_blank", data.windowFeatures || "width=685,height=700,status=no,toolbar=no,location=no,menubar=no,resizable=no,scrollbars=yes");
    }

    // Add a button that opens a window
    editor.addButton('orchardlink', {
        image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAABb0lEQVRIibWW223CQBBFDxEFUAIdJNHsf6CCkApiKgBXAFSAUgFJBdCB4X9HogSX4A6Sjx0rK8cGY5P7sw9Z93h3HvYAk4jbAk/cpgLYqPpz0wODCJDZ9NTSfBVBpk2QKuCk6tdt3EXcN5BGoFrIQ8u3bdIZeAZyIBNxf664LwBVnwPTJkhvgEGKJsiwp/dCxE2i9RFYAlsD9gIcgRHwUtkv4kVngKqf1u2LuHUMvUsMLunfAVevSMQtCUf+UvWHWwEXTyDidoSMGAF7EZfcDWDmCTC3gM6BnYgb9wZYoSSEXlP2l3LsDCiAQzTPgXd+KzMDzqr+2Amg6t+AmXXViUHGBs0MWJv7rQB25yszWpn5xsYjoR0XVYNrGkbmCSGQr4SsyYG8qWLbqjxBAqSq/hP4MNDYQL1UFloBPEbzPeEENxdWEyAl5PiIEOCcjndeVfxNngELM08NdOtfBlgnLWM3qHvC8n7bwbzUWdWnAD+rk38HDdeC5gAAAABJRU5ErkJggg==',
        tooltip: 'Content link',
        onclick: contentPickerAction
    });

    // Adds a menu item to the tools menu
    editor.addMenuItem('orchardlink ', {
        text: 'content link',
        image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAABb0lEQVRIibWW223CQBBFDxEFUAIdJNHsf6CCkApiKgBXAFSAUgFJBdCB4X9HogSX4A6Sjx0rK8cGY5P7sw9Z93h3HvYAk4jbAk/cpgLYqPpz0wODCJDZ9NTSfBVBpk2QKuCk6tdt3EXcN5BGoFrIQ8u3bdIZeAZyIBNxf664LwBVnwPTJkhvgEGKJsiwp/dCxE2i9RFYAlsD9gIcgRHwUtkv4kVngKqf1u2LuHUMvUsMLunfAVevSMQtCUf+UvWHWwEXTyDidoSMGAF7EZfcDWDmCTC3gM6BnYgb9wZYoSSEXlP2l3LsDCiAQzTPgXd+KzMDzqr+2Amg6t+AmXXViUHGBs0MWJv7rQB25yszWpn5xsYjoR0XVYNrGkbmCSGQr4SsyYG8qWLbqjxBAqSq/hP4MNDYQL1UFloBPEbzPeEENxdWEyAl5PiIEOCcjndeVfxNngELM08NdOtfBlgnLWM3qHvC8n7bwbzUWdWnAD+rk38HDdeC5gAAAABJRU5ErkJggg==',
        context: 'insert',
        onclick: contentPickerAction
    });

    return {
        getMetadata: function () {
            return {
                name: "Orchard content link plugin"
            };
        }
    };
});
