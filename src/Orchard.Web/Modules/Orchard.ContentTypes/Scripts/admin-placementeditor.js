(function ($) {
    var assignPositions = function () {
        var position = 0;

        $('.type').each(function () {
            var input = $(this);
            var tab = input.closest(".tab-container").data("tab");
            var card = input.closest(".card-container").data("card");
            //input = input.next();
            var postab = tab !== "" ? position + "#" + tab : position + "";
            postab += (card) && card !== "" ? "%" + card : "";
            reAssignIdName(input, position);  // type

            input = input.next();
            reAssignIdName(input, position);  // differentiator

            input = input.next();
            reAssignIdName(input, position);  // zone

            input = input.next();
            reAssignIdName(input, position);  // position

            input.val(postab);
            position++;
        });
    };

    var reAssignIdName = function (input, pos) {
        var name = input.attr('name');
        input.attr('name', name.replace(new RegExp("\\[.*\\]", 'gi'), '[' + pos + ']'));

        var id = input.attr('id');
        input.attr('id', id.replace(new RegExp('_.*__', 'i'), '_' + pos + '__'));
    };

    var startPos;

    // Makes sortable Cards and Shapes
    function initTab() {
        $(".tabdrag").sortable({
            placeholder: "placement-placeholder",
            connectWith: ".tabdrag",
            stop: function (event, ui) {
                assignPositions();
                $('#save-message').show();
            }
        });
        $(".carddrag").sortable({
            placeholder: "placement-placeholder",
            connectWith: ".carddrag",
            stop: function (event, ui) {
                assignPositions();
                $('#save-message').show();
            }
        });
    }

    // Makes sortable tabs
    $('#sortableTabs').sortable({
        placeholder: "tab-placeholder",
        stop: function (event, ui) {
            assignPositions();
            $('#save-message').show();
        }
    });

    $("#newTab").click(function (e) {
        e.preventDefault();
        // get the new tab name, cancel if blank
        var tab = $("#tabName").val().replace(/\s/g, "");
        if (!tab.length) {
            return;
        }

        if (tab.toLowerCase() === "content") {
            $("#tabName").val("");
            return;
        }

        // in tabs already
        var tabs = getTabs(true);
        if ($.inArray(tab, tabs) >= 0) {
            $("#tabName").val("");
            return;
        }
        //Insert the tab with an empty card
        $("#sortableTabs").append('<div data-tab="' + tab + '" class="zone-container tab-container"><h2><a class="delete">Delete</a>'
            + tab + '</h2><ul class="tabdrag">'
            + '<li data-tab="' + tab + '" data-card="" class="zone-container card-container">'
            + '<ul class="carddrag"></ul></li>'
            + '</ul></div> '
        );
        // make it sortable
        initTab();
        $("#sortableTabs").sortable("refresh");
        // clear the textbox
        $("#tabName").val("");
    });

    $("#newCard").click(function (e) {
        e.preventDefault();
        // get the new tab name, cancel if blank
        var card = $("#tabName").val().replace(/\s/g, "");
        if (!card.length) {
            return;
        }
        // insert card in the Content Tab 
        $("#content-tab > ul").append('<li data-tab="" data-card="' + card + '" class="zone-container card-container"><a class="delete">Delete</a><div class="card-type"><h2>'
            + card + '</h2></div><ul class="carddrag"></ul></li>');
        // make it sortable
        initTab();
        $("#sortableTabs").sortable("refresh");
        // clear the textbox
        $("#tabName").val("");
    });
    // remove tabs
    // append items to content, create content if not there
    $("#placement").on("click", ".delete", function (e) {
        var me = $(this);
        var parent = me.parent(".zone-container");
        if (!parent.length) {
            parent = me.parents(".zone-container");
        }
        var list, newList;
        if (parent.hasClass("tab-container")) {
            list = parent.children(".tabdrag").html();
            newList = $("#placement .tabdrag").first();
        } else if (parent.hasClass("card-container")) {
            list = parent.children(".carddrag").html();
            newList = $("#placement .tabdrag").first();
        }
        // get first tab
        if (newList.length) {
            parent.remove();
            newList.append(list);
        }
        assignPositions();
        // make it sortable
        initTab();
    });

    // toggle editor shapes
    $("#placement").on("click", ".toggle", function (e) {
        var $this = $(this);
        var shape = $(this).next().next();
        shape.toggle();

        if ($this.text() === $this.data("text-show")) {
            $this.text($this.data("text-hide"));
        } else {
            $this.text($this.data("text-show"));
        }
    });

    // returns all the tabs
    function getTabs(header) {
        var tabs = [];
        $(".zone-container").each(function (index, e) {
            tabs.push($(e).data("tab"));
        });

        return tabs;
    }

    initTab();
    assignPositions();
    $('.shape-editor *').attr('disabled', 'disabled');
    $("#placement").disableSelection();
})(jQuery);