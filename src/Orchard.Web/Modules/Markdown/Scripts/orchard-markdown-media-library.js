(function() {
    var converter = Markdown.getSanitizingConverter();
    var editors = $('.wmd-input');

    editors.each(function() {

        var idPostfix = $(this).attr('id').substr('wmd-input'.length);
        
        var editor = new Markdown.Editor(converter, idPostfix, {
            handler: function() { window.open("http://daringfireball.net/projects/markdown/syntax"); }
        });

        if (Boolean($(this).data("manage-media"))) {
            editor.hooks.set("insertImageDialog", function (callback) {
                // see if there's an image selected that they intend on editing
                var wmd = $('#wmd-input' + idPostfix);

                var editImage, content = wmd.selection ? wmd.selection.createRange().text : null;
                var adminIndex = location.href.toLowerCase().indexOf("/admin/");
                if (adminIndex === -1) return;
                var url = location.href.substr(0, adminIndex) + "/Admin/Orchard.MediaLibrary?dialog=true";
                $.colorbox({
                    href: url,
                    iframe: true,
                    reposition: true,
                    width: "90%",
                    height: "90%",
                    onLoad: function () {
                        // hide the scrollbars from the main window
                        $('html, body').css('overflow', 'hidden');
                    },
                    onClosed: function () {
                        $('html, body').css('overflow', '');

                        var selectedData = $.colorbox.selectedData;

                        if (selectedData == null) // Dialog cancelled, do nothing
                            return;

                        var newContent = '';
                        for (var i = 0; i < selectedData.length; i++) {
                            var renderMedia = location.href.substr(0, adminIndex) + "/Admin/Orchard.MediaLibrary/MediaItem/" + selectedData[i].id + "?displayType=Raw";
                            $.ajax({
                                async: false,
                                type: 'GET',
                                url: renderMedia,
                                success: function (data) {
                                    newContent += data;
                                }
                            });
                        }

                        var result = $.parseHTML(newContent);
                        var img = $(result).filter('img');
                        // if this is an image, use the callback which will format it in markdown
                        if (img.length > 0 && img.attr('src')) {
                            callback(img.attr('src'));
                        }

                            // otherwise, insert the raw HTML
                        else {
                            if (wmd.selection) {
                                wmd.selection.replace('.*', newContent);
                            } else {
                                wmd.text(newContent);
                            }
                            callback();
                        }
                    }
                });
                return true;
            });
        }

        editor.run();
    });
})();
