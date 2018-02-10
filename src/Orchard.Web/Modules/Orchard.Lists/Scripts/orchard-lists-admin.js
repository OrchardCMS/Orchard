(function ($) {

    var ajax = function (url, data, error, success, complete, method) {
        var ajaxError = $("#ajaxError");

        ajaxError.hide();
        NProgress.start();
        NProgress.set(0.4);
        $.ajax({
            type: method,
            url: url,
            data: data,
            cache: false
        }).done(function(response) {
            NProgress.set(0.9);
            if (success) {
                success(response);
            }
        }).fail(function() {
            if (error) {
                error();
            }
            ajaxError.show();
        }).always(function() {
            if (complete) {
                complete();
            }
            NProgress.done();
        });
    };
    
    var post = function (url, data, error, success, complete) {
        data = $.extend(data, {
            __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
        });
        ajax(url, data, error, success, complete, "POST");
    };
    
    var get = function (url, data, error, success, complete) {
        ajax(url, data, error, success, complete, "GET");
    };
    
    var insertItem = function(itemId) {
        var url = $("#listManagement").data("insert-url");
        var data = { itemId: itemId };
        post(url, data, null, function (html) {
            $("#main").replaceWith($("#main", $.parseHTML(html)));
            $(window).trigger("reinitialize");
        });
    };
    
    var refresh = function () {
        var url = $("#listManagement").data("refresh-url");
        get(url, null, null, function (html) {
            $("#main").replaceWith($("#main", $.parseHTML(html)));
            $(window).trigger("reinitialize");
        });
    };

    var updatePositions = function (itemId, oldIndex, newIndex, cancel) {
        if (newIndex == oldIndex) {
            return;
        }
        var url = $("#listManagement").data("update-url");
        var data = {
            itemId: itemId,
            oldIndex: oldIndex,
            newIndex: newIndex
        };
        post(url, data, cancel);
    };

    var bindActions = function () {
        $("#layout-content").on("change", "#listOperations", function () {
            var value = $(this).val();
            var sortBy = $("#SortBy");
            var sortByDirection = $("#SortByDirection");
            if (value === "Sort") {
                sortBy.css("display", "inline");
                sortByDirection.css("display", "inline");
            } else {
                sortBy.css("display", "none");
                sortByDirection.css("display", "none");
            }
        });
    };

    var initializeContentPicker = function() {
        $("#layout-content").on("click", "#chooseItems", function (e) {
            e.preventDefault();
            $("form:first").trigger("orchard-admin-contentpicker-open", {
                types: $("#listManagement").data("itemtypes"),
                callback: function(data) {
                    if (Array.isArray && Array.isArray(data)) {
                        data.forEach(function (item) {
                            var id = parseInt(item.id);
                            insertItem(id);
                        });
                    } else {
                        var id = parseInt(data.id);
                        insertItem(id);
                    }
                },
                baseUrl: $("#listManagement").data("baseurl")
            });
        });
    };

    var initializeContentCreator = function() {
        $("#layout-content").on("click", ".create-content", function (e) {
            var url = $(this).attr("href");
            $.colorbox({
                href: url,
                iframe: true,
                reposition: true,
                width: "100%",
                height: "100%",
                initialWidth: "100%",
                initialHeight: "100%",
                onLoad: function () {
                    $('html, body').css('overflow', 'hidden');
                    $('#cboxClose').remove();
                },
                onClosed: function () {
                    $('html, body').css('overflow', 'auto');
                    refresh();
                }
            });
            e.preventDefault();
        });
    };

    var initializeSortable = function () {
        var dragdrop = $("#listManagement").data("dragdrop");

        if (!dragdrop) {
            return;
        }

        var startIndex = -1;
        var cancel = function() {
            $("table.content-list tbody").sortable("cancel");
        };

        $("table.content-list tbody").sortable({
            handle: "td:first",
            start: function(e, ui) { startIndex = ui.item.index(); },
            stop: function (e, ui) {
                var newIndex = ui.item.index();
                var itemId = ui.item.data("content-id");
                updatePositions(itemId, startIndex, newIndex, cancel);
            }
        }).disableSelection();
    };

    var initializeCheckAll = function () {
        $("#layout-content").on("change", "table.content-list thead .toggle-all", function () {
            var tbody = $(this).parents("table:first").find("tbody");
            tbody.find("input[type='checkbox']").prop("checked", $(this).is(":checked"));
        });
    };

    var initializeListViewButtons = function() {
        $("#layout-content").on("click", ".switch-button", function() {
            $(".switch-button").removeClass("active");
            $(this).addClass("active");
            $("#listViewName").prop("checked", true).val($(this).data("listviewname"));
            $(this).parents("form:first").submit();
        });
    };

    NProgress.configure({ showSpinner: false });
    bindActions();
    initializeSortable();
    initializeCheckAll();
    initializeContentPicker();
    initializeContentCreator();
    initializeListViewButtons();

    $(window).on("reinitialize", function() {
        initializeSortable();
    });

})(jQuery);
