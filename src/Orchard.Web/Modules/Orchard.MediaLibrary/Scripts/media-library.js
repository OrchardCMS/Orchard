var enhanceViewModel = function(viewModel) {
    // extension point for other modules to alter the view model
};

var baseViewModel = function() {

};

$(function () {
    (function (settings) {

        var listWidth = $('#media-library-main-list').width();
        var listHeight = $('#media-library-main-list').height();
        var itemSize = $('.thumbnail').first().width();

        var itemsPerRow = Math.floor(listWidth / itemSize);
        var itemsPerColumn = Math.ceil(listHeight / itemSize);

        var pageCount = itemsPerRow * itemsPerColumn;

        function mediaPartViewModel(data) {
            var self = this;

            // values
            self.data = data;

            //id,
            //contentType,
            //contentTypeClass,
            //title,
            //alternateText,
            //caption,
            //resource,
            //mimeType,
            //mimeTypeClass,
            //thumbnail,
            //editLink,

            self.hasFocus = ko.observable();
            self.selected = ko.observable();
            self.status = ko.observable('');
            self.summary = ko.observable('');
            self.cssClass = ko.computed(function() {
                var css = '';
                if (self.selected()) {
                    css += ' selected';
                }

                if (self.hasFocus()) {
                    css += ' has-focus';
                }

                return css;
            });

            // operations
            self.setData = function(value) {
                self.data = value;
            };
        }

        function mediaIndexViewModel() {
            var self = this;

            // placeholder function called to retrieve content when scrolling
            self.loadMediaItemsUrl = function (folderPath, skip, count, order, mediaType) {
            };

            // values
            self.selection = ko.observableArray([]);
            self.focus = ko.observable();
            self.results = ko.observableArray();
            self.displayed = ko.observable();
            self.pendingRequest = ko.observable(false);
            self.mediaItemsCount = 0;
            self.orderMedia = ko.observableArray(['created']);
            self.mediaType = ko.observableArray([]);

            self.getMediaItems = function (count, append) {
                var folderPath = self.displayed() || '';
                
                if (self.pendingRequest()) {
                    return;
                }

                if (append) {
                    if (self.results().length > 0 && self.results().length >= self.mediaItemsCount) {
                        return;
                    }
                } else {
                    self.results([]);
                }

                self.pendingRequest(true);

                var url = self.loadMediaItemsUrl(folderPath, self.results().length, count, self.orderMedia(), self.mediaType());
                console.log(url);
                
                $.ajax({
                    type: "GET",
                    url: url,
                    cache: false
                }).done(function(data) {
                    var mediaItems = data.mediaItems;
                    self.mediaItemsCount = data.mediaItemsCount;
                    for (var i = 0; i < mediaItems.length; i++) {
                        var item = new mediaPartViewModel(mediaItems[i]);
                        self.results.push(item);

                        // pre-select result which are already part of the selection
                        var selection = self.selection();
                        for (var j = 0; j < selection.length; j++) {
                            if (selection[j].data.id == item.data.id) {
                                viewModel.toggleSelect(item, true);
                            }
                        }
                    }
                }).fail(function(data) {
                    console.error(data);
                }).always(function() {
                    self.pendingRequest(false);
                });
            };

            self.clearSelection = function() {
                this.focus(null);
                // unselect previous elements
                self.selection().forEach(function(item) {
                    item.selected(false);
                });

                self.selection([]);
            };

            self.focus.subscribe(function(oldValue) {
                if (oldValue) {
                    oldValue.hasFocus(false);
                }
            }, this, "beforeChange");

            self.focus.subscribe(function(newValue) {
                if (newValue) {
                    newValue.hasFocus(true);

                    // load summary admin if not alreay done
                    if (newValue.summary() == '') {
                        var id = newValue.data.id;
                        var url = settings.mediaItemActionUrl + '/' + id;

                        $.ajax({
                            type: "GET",
                            url: url,
                            cache: false
                        }).done(function(data) {
                            newValue.summary(data);
                        });
                    }
                }
            });

            self.displayFolder = function(folderPath) {

                self.results([]);
                self.displayed(folderPath);

                self.loadMediaItemsUrl = function (f, skip, count, order, mediaType) {
                    return settings.mediaItemsActionUrl + '?folderPath=' + encodeURIComponent(f) + '&skip=' + skip + '&count=' + count + '&order=' + order + '&mediaType=' + mediaType;
                };
                
                self.getMediaItems(pageCount);
            };

            self.selectFolder = function(folderPath) {
                History.pushState({ action: 'displayFolder', folderPath: folderPath }, '', '?folderPath=' + folderPath);
                self.displayFolder(folderPath);
            };

            self.selectRecent = function() {
                History.pushState({ action: 'selectRecent' }, '', '?recent');

                self.loadMediaItemsUrl = function (folderPath, skip, count, order, mediaType) {
                    return settings.recentMediaItemsActionUrl + '?skip=' + skip + '&count=' + count + '&order=' + order + '&mediaType=' + mediaType;
                };
                
                self.results([]);
                self.displayed(null);
                self.getMediaItems(pageCount);
            };

            self.toggleSelect = function(searchResult, force) {
                var index = $.inArray(searchResult, self.selection());
                if (index == -1 || force) {
                    self.selection.remove(function(item) { return item.data.id == searchResult.data.id; });
                    self.selection.push(searchResult);
                    searchResult.selected(true);
                } else {
                    self.selection.remove(searchResult);
                    searchResult.selected(false);
                }

                this.focus(searchResult);
            };

            self.select = function(searchResult) {
                var index = $.inArray(searchResult, self.selection());
                if (index == -1) {
                    self.clearSelection();
                    self.selection([searchResult]);
                    searchResult.selected(true);
                }

                this.focus(searchResult);
            };

            self.scrolled = function(data, event) {
                var elem = event.target;
                if (elem.scrollTop > (elem.scrollHeight - elem.offsetHeight - 300)) {
                    self.getMediaItems(pageCount, true);
                }
            };

            self.importMedia = function() {
                var url = settings.importActionUrl + '?folderPath=' + encodeURIComponent(self.displayed());
                window.location = url;
            };

            var selectFolderOrRecent = function () {
                if (self.displayed()) {
                    self.selectFolder(self.displayed());
                } else {
                    self.selectRecent();
                }
            };
            self.orderMedia.subscribe(selectFolderOrRecent);
            self.mediaType.subscribe(selectFolderOrRecent);
        }

        var viewModel = new mediaIndexViewModel();

        enhanceViewModel(viewModel);
        
        ko.applyBindings(viewModel);

        if (settings.hasFolderPath) {
            viewModel.displayFolder(settings.folderPath);
            History.pushState({
                action: 'displayFolder',
                folderPath: settings.folderPath
            }, '', '?folderPath=' + settings.folderPath);
        } else {
            viewModel.selectRecent();
            History.pushState({ action: 'selectRecent' }, '', '?recent');
        }

        window.onpopstate = function(event) {
            if (event && event.state && event.state.action == 'displayFolder') {
                viewModel.displayFolder(event.state.folder);
            }

            if (event && event.state && event.state.action == 'selectRecent') {
                viewModel.selectRecent();
            }
        };

        $("#media-library-main-list").on("mousedown", "li", function(e) {
            // only for left click
            if (e.which != 1) {
                return;
            }

            var searchResult = ko.dataFor(this);
            if (e.ctrlKey) {
                viewModel.toggleSelect(searchResult);
            } else {
                viewModel.select(searchResult);
            }
        }).on("contextmenu", "li", function() {
            var searchResult = ko.dataFor(this);
            viewModel.toggleSelect(searchResult);
            return false;
        });

        $("#media-library-main-selection-select > .button-select").on('click', function() {
            if (parent.$.colorbox) {
                var selectedData = [];
                for (var i = 0; i < viewModel.selection().length; i++) {
                    var selection = viewModel.selection()[i];
                    selectedData.push(selection.data);
                }
                parent.$.colorbox.selectedData = selectedData;
                parent.$.colorbox.close();
            }
            ;
        });

        $("#media-library-main-selection-select > .button-cancel").on('click', function() {
            if (parent.$.colorbox) {
                parent.$.colorbox.selectedData = null;
                parent.$.colorbox.close();
            }
            ;
        });

        $(".media-library-folder-title").droppable({
            accept: function() {
                var targetId = $(this).data('term-id');
                return targetId != viewModel.displayed();
            },
            over: function(event, ui) {
                $(ui.helper).addClass('over');
                $(this).addClass('dropping');
            },
            out: function(event, ui) {
                $(ui.helper).removeClass('over');
                $(this).removeClass('dropping');
            },
            tolerance: "pointer",
            drop: function() {
                $(this).removeClass('dropping');
                var folderPath = $(this).data('media-path');

                if (folderPath == viewModel.displayed()) {
                    return;
                }

                var ids = [];
                viewModel.selection().forEach(function(item) { ids.push(item.data.id); });
                var url = settings.moveActionUrl;

                console.log(folderPath);

                $.ajax({
                    type: "POST",
                    url: url,
                    dataType: "json",
                    traditional: true,
                    data: {
                        folderPath: folderPath,
                        mediaItemIds: ids,
                        __RequestVerificationToken: settings.antiForgeryToken
                    },
                }).done(function(result) {
                    if (result) {
                        if (viewModel.displayed()) {
                            viewModel.results.remove(function(item) {
                                return ids.indexOf(item.data.id) != -1;
                            });
                        }

                        viewModel.clearSelection();
                    } else {
                        alert(errorMessage);
                        console.log('failed to move media items: ' + result.toString());
                    }
                }).fail(function (result) {
                    alert(errorMessage);
                    console.log('failed to move media items: ' + result.toString());
                });
            }
        });

        $("#media-library-main-list").on("mouseover", ".media-thumbnail", function() {
            if (!$(this).hasClass("ui-draggable")) {
                $(this).draggable({
                    revert: 'invalid',
                    //containment: 'li',
                    helper: function(event) {
                        var clone = $(event.currentTarget).clone().removeAttr('id');
                        clone.removeClass('selected');
                        clone.addClass('dragged-selection');

                        if (viewModel.selection().length > 1) {
                            clone.append($('<div class="draggable-selection"><p>' + viewModel.selection().length + '</p></div>'));
                        }

                        return clone;

                    },
                    cursor: 'move',
                    distance: 10,
                    appendTo: 'body',
                    create: function() {
                        // position the handler a little left to the center to let the number of selected items to appear
                        $(this).draggable("option", "cursorAt", { left: $(this).width() / 2 - 20, top: $(this).height() / 2 });
                    }
                });
            }
        });

        $('#delete-selection-button').click(function() {
            if (!confirm(settings.deleteConfirmationMessage)) {
                return false;
            }

            var ids = [];
            viewModel.selection().forEach(function(item) { ids.push(item.data.id); });
            var url = settings.deleteActionUrl;

            $.ajax({
                type: "POST",
                url: url,
                dataType: "json",
                traditional: true,
                data: {
                    mediaItemIds: ids,
                    __RequestVerificationToken: settings.antiForgeryToken
                }
            }).done(function(result) {
                if (result) {
                    viewModel.results.remove(function(item) {
                        return ids.indexOf(item.data.id) != -1;
                    });

                    viewModel.clearSelection();
                } else {
                    console.log('failed to delete media items');
                }
                return false;
            });
            return false;
        });
    })(window.mediaLibrarySettings);
})
