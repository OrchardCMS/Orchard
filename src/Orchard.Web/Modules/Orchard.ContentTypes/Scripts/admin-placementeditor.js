(function ($) {
    var assignPositions = function () {
        var position = 0;
        $('.type').each(function () {
            var input = $(this);
            var tab = input.closest(".zone-container").data("tab");
            //input = input.next();
            var postab = tab != "" ? position + "#" + tab : position + "";
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

    function initTab() {
        $(".tabdrag").sortable({
            placeholder: "placement-placeholder",
            connectWith: ".tabdrag",
            stop: function (event, ui) {
                assignPositions();
            }
        });
    }

    $('#sortableTabs').sortable({
        placeholder: "tab-placeholder",
        start: function (event, ui) {
            var self = $(ui.item);
            startPos = self.prevAll().size();
        },
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

        $("#sortableTabs").append('<div data-tab="' + tab + '" class="zone-container tab-container"><h2><a class="delete">Delete</a>'
            + tab + '</h2><ul class="tabdrag"></ul></div>'
        );
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
        var list = parent.children(".tabdrag").html();
        // get first tab
        var newList = $("#placement .tabdrag").first();
        if (newList.length) {
            parent.remove();
            newList.append(list);
        }

        assignPositions();
    });

    // toggle editor shapes
    $("#placement").on("click", ".toggle", function (e) {
        var $this = $(this);
        var shape = $(this).next().next();
        shape.toggle();
        if ($this.text() == 'Show Editor Shape') {
            $this.text('Hide Editor Shape');
        } else {
            $this.text('Show Editor Shape');
        }
    })

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