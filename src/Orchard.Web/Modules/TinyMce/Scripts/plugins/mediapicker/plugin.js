(function () {

    ////////////////////////////////////////////////////////////////////////
    // NOTE: IF YOU EDIT THIS FILE
    // You must also update editor_plugin.js with a minified version.
    ////////////////////////////////////////////////////////////////////////
    tinymce.create('tinymce.plugins.Orchard.MediaPicker', {
        /**
        * Initializes the plugin, this will be executed after the plugin has been created.
        * This call is done before the editor instance has finished it's initialization so use the onInit event
        * of the editor instance to intercept that event.
        *
        * @param {tinymce.Editor} ed Editor instance that the plugin is initialized in.
        * @param {string} url Absolute URL to where the plugin is located.
        */
        init: function (ed, url) {
            // Register the command so that it can be invoked by using tinyMCE.activeEditor.execCommand('mceMediaPicker');
            ed.addCommand('mceMediaPicker', function () {
                ed.focus();
                // see if there's an image selected that they intend on editing
                var editImage,
                    content = ed.selection.getContent();
                if (content) {
                    // replace <img> with <editimg>, so we can easily use jquery to get the 'src' without it
                    // being resolved by the browser (e.g. prevent '/foo.png' becoming 'http://localhost:12345/orchardlocal/foo.png').
                    content = content.replace(/\<IMG/gi, "<editimg");
                    var firstImg = $(content).filter("editimg");
                    if (firstImg.length) {
                        editImage = {
                            src: firstImg.attr("src"),
                            "class": firstImg.attr("class"),
                            style: firstImg.css("cssText"),
                            alt: firstImg.attr("alt"),
                            width: firstImg.attr("width"),
                            height: firstImg.attr("height"),
                            align: firstImg.attr("align")
                        };
                    }
                }
                jQuery("#" + ed.id).trigger("orchard-admin-pickimage-open", {
                    img: editImage,
                    uploadMediaPath: jQuery("#" + ed.id).data("mediapicker-uploadpath"), //ed.getParam("mediapicker_uploadpath"),
                    callback: function (data) {
                        ed.focus();
                        ed.selection.setContent(data.img.html);
                    }
                });
            });

            // Register media button
            ed.addButton('mediapicker', {
                icon: "image",
                title: jQuery("#" + ed.id).data("mediapicker-title"), //ed.getParam("mediapicker_title"),
                cmd: 'mceMediaPicker'
            });
        },

        /**
        * Creates control instances based in the incomming name. This method is normally not
        * needed since the addButton method of the tinymce.Editor class is a more easy way of adding buttons
        * but you sometimes need to create more complex controls like listboxes, split buttons etc then this
        * method can be used to create those.
        *
        * @param {String} n Name of the control to create.
        * @param {tinymce.ControlManager} cm Control manager to use inorder to create new control.
        * @return {tinymce.ui.Control} New control instance or null if no control was created.
        */
        createControl: function (n, cm) {
            return null;
        },

        /**
        * Returns information about the plugin as a name/value array.
        * The current keys are longname, author, authorurl, infourl and version.
        *
        * @return {Object} Name/value array containing information about the plugin.
        */
        getInfo: function () {
            return {
                longname: 'Orchard MediaPicker Plugin',
                author: 'Dave Reed',
                authorurl: 'http://orchardproject.net',
                infourl: 'http://orchardproject.net',
                version: '1.1'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('mediapicker', tinymce.plugins.Orchard.MediaPicker);
})();