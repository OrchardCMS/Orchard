
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
            editor.insertContent("<a href=\"#{ContentManager.Get:" + returnData.id.toString() + ".DisplayUrl}\">" + textLink + "</a>");

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
        image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QAEABHAPK/xSSIAAAACXBIWXMAAC4jAAAuIwF4pT92AAAB3klEQVRIx9XVvUuVcRQH8I/e61WSIrMXG4xqaKihDCIbIsolgv6BhkaH1sbGoK2hvS3aKiepcGmICCQIaY+SpFChFE1T7235Xni4XB8dNOjA4fm9nfM9b8857DJ1tOz7cRG3sq6UyM7iJZ7j93YBL+MFFlBHo4QXMYHb6GljLKi27E9gBDU8xfImhtzEUQxHxzJeYSngm9JoHsxjoOTdm4InS5jEdfS1etK5A3ncg3N4mPz17jRAM9SncB9XdgOgCXIah8uSvF16hGeF/T7cS7i6dgJgrGU/gLsBsFshakv/P0B1m0Z0FLiOjdxVcjaHIwWZSrPVbAXQHcHBVMpamtx07o7hQIxYCVgVv/AVs2UAFRxP32n+obN4jykMhfdG6XxKtC/d9S3GywC6cCE95nsUL+ATDqY9LOR8NVzD/nTlG5iubhH7k/iDJ3hdyNlQZsFcFH/Bj9zVY9AoBssAGlFQiSdrWE+cO7MeTg4mkqND8eJ8CuFnGcB62vAZXMOlJK+SuK9k0LwL+EgA+5OvSUxV2yhdzYMHuIPHOJtqWon1vYXQfIi3fVn34Bs+4nOr1VcxXhiZzWqqFbi78C0aWIvy5ruKNkN9ETMRnsnYbCSe7bhekN1IBJp3Df+C/gLjrnlNxkqTcgAAAABJRU5ErkJggg==',
        tooltip: 'Content link',
        onclick: contentPickerAction
    });

    // Adds a menu item to the tools menu
    editor.addMenuItem('orchardlink ', {
        text: 'content link',
        //image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QAEABHAPK/xSSIAAAACXBIWXMAAC4jAAAuIwF4pT92AAAAvElEQVRIx+2VsRHCMAxFH3AwAR075CjSpGOfLEU6nAKmoIICJohHYANoVPjAji0nNLn8OxeSvny2vmTDDCUOQOfYb1mptpU9grDKDX12N20Nvq88OH+ZkLQGDFA7vho4SWwQdkADPIHS8ZfiOwonC1s5+R2oPPFKYka46hpegCtQ9PD3wA0452qwiXBWwlnklqhNKFGrKVFI5EdA5EYrsm8OQm1qPG0anaPRB216eI3wXNvYh2P/+eHM+MEHpr9Js1lOQvUAAAAASUVORK5CYII=',
        context: 'insert',
        onclick: contentPickerAction
    });

    return {
        getMetadata: function () {
            return {
                name: "Orchard content link plugin"
                //url: "http://exampleplugindocsurl.com"
            };
        }
    };
});
