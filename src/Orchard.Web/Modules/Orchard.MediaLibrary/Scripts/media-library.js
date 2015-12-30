var enhanceViewModel = function(viewModel) {
    // extension point for other modules to alter the view model
};

var baseViewModel = function() {

};

$(function () {
    (function (settings) {

        function attachFolderTitleDropEvent (elements) {
            elements.droppable({
                accept: function () {
                    var targetId = $(this).data('term-id');
                    return targetId != viewModel.displayed();
                },
                over: function (event, ui) {
                    $(ui.helper).addClass('over');
                    $(this).addClass('dropping');
                },
                out: function (event, ui) {
                    $(ui.helper).removeClass('over');
                    $(this).removeClass('dropping');
                },
                tolerance: "pointer",
                drop: function () {
                    $(this).removeClass('dropping');
                    var folderPath = $(this).data('media-path');

                    if (folderPath == viewModel.displayed()) {
                        return;
                    }

                    var ids = [];
                    viewModel.selection().forEach(function (item) { ids.push(item.data.id); });
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
                    }).done(function (result) {
                        if (result) {
                            if (viewModel.displayed()) {
                                viewModel.results.remove(function (item) {
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
        };

        var listWidth = $('#media-library-main-list').width();
        var listHeight = $('#media-library-main-list').height();
        var itemWidth = $('#media-library-main-list li').first().width();
        var itemHeight = $('#media-library-main-list li').first().height();
        var defaultDimension = $(window).width() < 1420 ? 120 : 200;
        if (itemHeight == 0 || itemHeight == null) itemHeight = defaultDimension;
        if (itemWidth == 0 || itemWidth == null) itemWidth = defaultDimension;
        var draftText = $("#media-library").data("draft-text");

        var itemsPerRow = Math.floor(listWidth / itemWidth);
        var itemsPerColumn = Math.ceil(listHeight / itemHeight);

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

            self.publicationStatus = ko.computed(function() {
                return self.data.published ? "" : draftText;
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
            self.mediaItemsCount = 0;
            self.orderMedia = ko.observableArray(['created']);
            self.mediaType = ko.observableArray([]);

            self.mediaFolders = ko.observableArray([]);
            self.mediaFoldersRequestCount = ko.observable(0);
            self.mediaFoldersPendingRequest = ko.computed({
                read: function () {
                    return (self.mediaFoldersRequestCount() > 0);
                },
                write: function (value) {
                    if (value === true) {
                        self.mediaFoldersRequestCount(self.mediaFoldersRequestCount() + 1);
                    } else if (value === false) {
                        self.mediaFoldersRequestCount(self.mediaFoldersRequestCount() - 1);
                    }
                }
            });

            self.mediaPendingRequest = ko.observable(false);
            self.pendingRequest = ko.computed({
                read: function () {
                    return (self.mediaFoldersPendingRequest() || self.mediaPendingRequest());
                },
                write: function (value) {
                    self.mediaPendingRequest(value);
                }
            });

            self.getMediaItems = function (count, append) {
                var folderPath = self.displayed() || '';
                
                if (self.mediaPendingRequest()) {
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
                    var mediaItemsFolderPath = data.folderPath;

                    if (mediaItemsFolderPath !== self.displayed()) {
                        return;
                    }

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

            self.afterRenderMediaFolderTemplate = function(elements, model) {
                var childTitles = $(elements).find(".media-library-folder-title");
                attachFolderTitleDropEvent(childTitles);
            };

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

            self.selectFolder = function (folderPath) {
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

        function mediaFolderViewModel(data) {
            var self = this;

            self.mediaIndexViewModel = viewModel;

            self.folderPath = ko.observable(data.folderPath);

            self.name = ko.observable(data.name);

            self.childFolders = ko.observableArray([]);

            self.childFoldersFetchStatus = 0;  //0 = unfetched, 1 = fetching, 2 = fetched

            self.isExpanded = ko.observable(false);

            self.isSelected = ko.computed(function() {
                return (self.mediaIndexViewModel.displayed() == self.folderPath());
            });

            self.fetchChildren = function (deepestChildPath) {
                self.childFoldersFetchStatus = 1;
                
                var getChildFolderListUrl = function (f) {
                    return settings.childFolderListingActionUrl + '?folderPath=' + encodeURIComponent(f);
                };
                var url = getChildFolderListUrl(self.folderPath());

                self.mediaIndexViewModel.mediaFoldersPendingRequest(true);

                $.ajax({
                    type: "GET",
                    url: url,
                    cache: false
                }).done(function (data) {
                    var newChildFolders = data.childFolders;

                    var nextFetch = self.folderPath();

                    if (deepestChildPath !== undefined && deepestChildPath !== null && (deepestChildPath.indexOf(self.folderPath()) === 0)) {
                        var deepestChildPathBreadCrumbs = deepestChildPath.split('\\');
                        var currentBreadCrumbs = self.folderPath().split('\\');

                        var diff = deepestChildPathBreadCrumbs.length - currentBreadCrumbs.length;
                        if (diff > 0) {
                            nextFetch = self.folderPath() + '\\' + deepestChildPathBreadCrumbs[deepestChildPathBreadCrumbs.length - diff];
                        }
                    }

                    for (var y = 0; y < newChildFolders.length; y++) {
                        var newChildFolder = new mediaFolderViewModel(newChildFolders[y]);
                        if (newChildFolder.folderPath() === nextFetch) {
                            newChildFolder.fetchChildren(deepestChildPath);
                            newChildFolder.isExpanded(true);
                        }
                        self.childFolders.push(newChildFolder);
                    }

                    self.childFoldersFetchStatus = 2;
                }).fail(function (data) {
                    console.error(data);
                    self.childFoldersFetchStatus = 0;
                }).always(function () {
                    self.mediaIndexViewModel.mediaFoldersPendingRequest(false);
                });
            };

            self.folderClicked = function () {
                if (self.mediaIndexViewModel.mediaFoldersPendingRequest()) {
                    return;
                }

                self.mediaIndexViewModel.selectFolder(self.folderPath());

                if (self.childFoldersFetchStatus === 0) {
                    self.fetchChildren();
                }

                self.isExpanded(!self.isExpanded());
            };

            self.afterRenderMediaFolderTemplate = function (elements, model) {
                var childTitles = $(elements).find(".media-library-folder-title");
                attachFolderTitleDropEvent(childTitles);
            };
        }

        $.map(settings.childFolders, function (childFolder, index) {
            viewModel.mediaFolders.push(new mediaFolderViewModel(childFolder));
        });

        enhanceViewModel(viewModel);
        
        ko.applyBindings(viewModel);

        if (settings.hasFolderPath) {
            viewModel.displayFolder(settings.folderPath);

            //fetch displayed folder structure
            (function (displayedFolder) {
                var folders = viewModel.mediaFolders();
                for (var x = 0; x < folders.length; x++) {
                    var folder = folders[x];
                    if (displayedFolder.indexOf(folder.folderPath()) === 0) {
                        folder.fetchChildren(displayedFolder);
                        folder.isExpanded(true);
                        break;
                    }
                }
            })(settings.folderPath);

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

        var pickAndClose = function () {
        	if (parent.$.colorbox) {
        		var selectedData = [];
        		for (var i = 0; i < viewModel.selection().length; i++) {
        			var selection = viewModel.selection()[i];
        			selectedData.push(selection.data);
        		}
        		parent.$.colorbox.selectedData = selectedData;
        		parent.$.colorbox.close();
        	};
        }

        $("#media-library-main-selection-select > .button-select").on('click', function () {
        	pickAndClose();
        });

        $("#media-library-main-list").on('dblclick', function () {
        	pickAndClose();
        });

        $("#media-library-main-selection-select > .button-cancel").on('click', function() {
            if (parent.$.colorbox) {
                parent.$.colorbox.selectedData = null;
                parent.$.colorbox.close();
            }
            ;
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

        $('#clone-selection-button').click(function () {
            if (!confirm(settings.cloneConfirmationMessage)) {
                return false;
            }

            var ids = [];
            viewModel.selection().forEach(function (item) { ids.push(item.data.id); });

            if (ids.length != 1) {
                return false;
            }

            var url = settings.cloneActionUrl;

            $.ajax({
                type: "POST",
                url: url,
                dataType: "json",
                traditional: true,
                data: {
                    mediaItemId: ids[0],
                    __RequestVerificationToken: settings.antiForgeryToken
                }
            }).done(function (result) {
                if (result) {
                    viewModel.getMediaItems(viewModel.pageCount);
                } else {
                    console.log('failed to clone media items');
                }
                return false;
            });
            return false;
        });
    })(window.mediaLibrarySettings);
})
