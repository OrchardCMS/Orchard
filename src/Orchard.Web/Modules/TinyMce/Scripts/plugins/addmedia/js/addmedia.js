tinyMCEPopup.requireLangPack("addmedia");

// async media file uploads brought to you with a little insight from reading: http://www.bennadel.com/blog/1244-ColdFusion-jQuery-And-AJAX-File-Upload-Demo.htm
var AddMediaDialog = {
    init: function () {
        var form = document.forms[0];
        form.action = tinyMCE.activeEditor.getParam('addmedia_action');
        form.MediaPath.value = tinyMCE.activeEditor.getParam('addmedia_path');
        form.__RequestVerificationToken.value = tinyMCE.activeEditor.getParam('request_verification_token');
    },

    addMedia: function (form) {
        var iframeName = "addmedia__" + (new Date()).getTime();

        var iframe;
        try {
            iframe = document.createElement("<iframe name=\"" + iframeName + "\" />"); // <- for some vowel browser
        } catch (ex) {
            iframe = document.createElement("iframe");
            iframe.name = iframeName;
        }
        iframe.src = "about:blank";
        tinymce.DOM.setStyles(iframe, { display: "none" });

        var iframeLoadHandler = tinymce.dom.Event.add(iframe, 'load', function (ev) {
            try {
                var frameWindow = window.frames[iframeName];

                if (frameWindow.document.URL == "about:blank") {
                    return true;
                }

                var result = frameWindow.result, close = 0;

                if (result && result.url) {
                    if (window.parent && window.parent.AddMediaDialog) {
                        window.parent.AddMediaDialog.insertMedia(result.url);
                    } else {
                        AddMediaDialog.insertMedia(result.url);
                    }

                    close = 1;
                } else if (result && result.error) {
                    alert(tinyMCEPopup.getLang("addmedia_dlg.msg_error") + "\n\r\n\r" + result.error);
                } else {
                    var somethingPotentiallyHorrible = "";
                    try {
                        somethingPotentiallyHorrible = window.frames[iframeName].document.getElementsByTagName("body")[0].innerHTML;
                    } catch (ex) { // some browsers flip out trying to access anything in the iframe when there's an error. best we can do is guess at this point
                        somethingPotentiallyHorrible = tinyMCEPopup.getLang("addmedia_dlg.msg_error_unknown");
                    }
                    if (somethingPotentiallyHorrible) {
                        alert(tinyMCEPopup.getLang("addmedia_dlg.msg_error_unexpected") + "\n\r\n\r" + somethingPotentiallyHorrible);
                    }
                }

                //cleanup
                setTimeout(function () {
                    tinymce.dom.Event.remove(iframe, 'load', iframeLoadHandler);
                    tinymce.DOM.remove(iframe);
                    iframe = null;

                    if (close) {
                        if (window.parent && window.parent.tinyMCEPopup) {
                            window.parent.tinyMCEPopup.close();
                        } else {
                            tinyMCEPopup.close();
                        }
                    }
                },
                123);
            } catch (ex) {
                alert(ex.message);
            }
        });

        tinymce.DOM.add(document.body, iframe);
        form.target = iframe.name;
    },

    insertMedia: function (url) {
        if (!url) return;

        var markup = /\.(jpe?g|png|gif)$/i.test(url)
            ? "<img src=\"" + url + "\" />"
            : "<a href=\"" + url + "\">" + url + "</a>";

        tinyMCE.activeEditor.execCommand("mceInsertContent", false, markup);
    }
};

tinyMCEPopup.onInit.add(AddMediaDialog.init, AddMediaDialog);