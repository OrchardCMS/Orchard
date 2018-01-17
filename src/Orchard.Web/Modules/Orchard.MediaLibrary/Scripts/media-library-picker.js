(function($) {

    $(".media-library-picker-field").each(function() {
        var element = $(this);
        var multiple = element.data("multiple");
        var removeText = element.data("remove-text");
        var removePrompt = element.data("remove-prompt");
        var removeAllPrompt = element.data("remove-all-prompt");
        var editText = element.data("edit-text");
        var dirtyText = element.data("dirty-text");
        var pipe = element.data("pipe");
        var returnUrl = element.data("return-url");
        var addUrl = element.data("add-url");
        var promptOnNavigate = element.data("prompt-on-navigate");
        var showSaveWarning = element.data("show-save-warning");
        var addButton = element.find(".button.add");
        var saveButton = element.find('.button.save');
        var removeAllButton = element.find(".button.remove");
        var template = 
            '<li><div data-id="{contentItemId}" class="media-library-picker-item"><div class="thumbnail">{thumbnail}<div class="overlay"><h3>{title}</h3></div></div></div><a href="#" data-id="{contentItemId}" class="media-library-picker-remove">' + removeText + '</a>' + pipe + '<a href="{editLink}?ReturnUrl=' + returnUrl + '">' + editText + '</a></li>';
        
        var refreshIds = function() {
            var id = element.find('.selected-ids');
            var ids = [];

            element.find(".media-library-picker-item").each(function () {
                ids.push($(this).attr("data-id"));
            });

            id.val(ids.join());

            var itemsCount = ids.length;
            
            if(!multiple && itemsCount > 0) {
                addButton.hide();
                saveButton.show();
            }
            else {
                addButton.show();
                saveButton.hide();
            }

            if(itemsCount > 1) {
                removeAllButton.show();
            }
            else {
                removeAllButton.hide();
            }
        };

        var showSaveMsg = function () {
            if (!showSaveWarning)
                return;

            element.find('.media-library-picker-message').show();
            window.mediaLibraryDirty = true;
        };

        window.mediaLibraryDirty = false;

        if (promptOnNavigate) {    
            if (!window.mediaLibraryNavigateAway) {
                $(window).on("beforeunload", window.mediaLibraryNavigateAway = function() {
                    if (window.mediaLibraryDirty) {
                        return dirtyText;
                    }
                });
                element.closest("form").on("submit", function() {
                    window.mediaLibraryDirty = false;
                });
            }
        }

        refreshIds();
        
        addButton.click(function () {
            var url = addUrl;
            $.colorbox({
                href: url,
                iframe: true,
                reposition: true,
                width: "100%",
                height: "100%",
                onLoad: function() { // hide the scrollbars from the main window
                    $('html, body').css('overflow', 'hidden');
                    $('#cboxClose').remove();
                    element.trigger("opened");
                },
                onClosed: function() {
                    $('html, body').css('overflow', '');
                    var selectedData = $.colorbox.selectedData;
                    if (selectedData == null) { // Dialog cancelled, do nothing
                        element.trigger("closed");
                        return;
                    }

                    var selectionLength = multiple ? selectedData.length : Math.min(selectedData.length, 1);
                    
                    for (var i = 0; i < selectionLength ; i++) {
                        var tmpl = template
                            .replace(/\{contentItemId\}/g, selectedData[i].id)
                            .replace(/\{thumbnail\}/g, selectedData[i].thumbnail)
                            .replace(/\{title\}/g, selectedData[i].title)
                            .replace(/\{editLink\}/g, selectedData[i].editLink);
                        var content = $(tmpl);
                        element.find('.media-library-picker.items ul.media-items').append(content);
                    }
                    
                    refreshIds();

                    if (selectedData.length) {
                        showSaveMsg();
                    }

                    element.trigger("closed");
                }
            });
            
        });

        removeAllButton.click(function (e) {
            e.preventDefault();
            if (!confirm(removeAllPrompt)) return false;
            element.find('.media-library-picker.items ul').children('li').remove();
            refreshIds();
            showSaveMsg();
            return false;
        });

        element.on("click",'.media-library-picker-remove', function(e) {
            e.preventDefault();
            if (!confirm(removePrompt)) return false;
            $(this).closest('.media-library-picker.items > ul > li').remove();
            refreshIds();
            showSaveMsg();
            return false;
        });
            
        element.find(".media-library-picker.items ul").sortable({
            handle: '.thumbnail',
            stop: function() {
                refreshIds();
                showSaveMsg();
            }
        }).disableSelection();
    });

})(jQuery);