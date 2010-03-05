tinyMCEPopup.requireLangPack("addmedia");

// async media file uploads brought to you with a little insight from reading: http://www.bennadel.com/blog/1244-ColdFusion-jQuery-And-AJAX-File-Upload-Demo.htm
var AddMediaDialog = {
    init: function() {
        var form = document.forms[0];
        form.action = tinyMCE.activeEditor.getParam('addmedia_action');
        form.MediaPath.value = tinyMCE.activeEditor.getParam('addmedia_path');
        form.__RequestVerificationToken.value = tinyMCE.activeEditor.getParam('request_verification_token');
    },

    addMedia: function(form) {
        var callback = AddMediaDialog.insertMediaAndClose;
        var iframeName = "addmedia__" + (new Date()).getTime()
        var iframe = document.createElement("iframe");
        iframe.name = iframeName;
        iframe.src = "about:blank";

        tinymce.DOM.setStyles(iframe, { display: "none" });
        tinymce.dom.Event.add(iframe, 'load', function(ev) {
            var result = window.frames[iframeName].result;
            if (result && result.url) {
                return callback(result.url);
            } else if (false && result && result.error) {
                alert("Wait, there was an error:\n\r\n\r" + result.error);
            } else {
                var somethingPotentiallyHorrible = window.frames[iframeName].document.getElementsByTagName("body")[0].innerHTML;
                if (somethingPotentiallyHorrible) {
                    alert("Something unexpected happened:\n\r\n\r" + somethingPotentiallyHorrible);
                }
            }

            //cleanup
            setTimeout(function() { callback = null; tinymce.DOM.remove(iframe); }, 123);
        });

        form.target = iframeName;
        tinymce.DOM.add(document.body, iframe);
    },

    insertMediaAndClose: function(url) {
        if (!url) return;

        //todo: (heskew) needs more awesome
        var markup = /\.(jpe?g|png|gif)$/i.test(url)
            ? "<img src=\"" + url + "\" />"
            : "<a href=\"" + url + "\">" + url + "</a>";
        
        tinyMCE.activeEditor.execCommand("mceInsertContent", false, markup);
        tinyMCEPopup.close();
    }
};

tinyMCEPopup.onInit.add(AddMediaDialog.init, AddMediaDialog);