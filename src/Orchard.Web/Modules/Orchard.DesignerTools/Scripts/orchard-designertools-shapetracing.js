(function ($) {
    $(function () {
        $("<div id='debug-control'><ul><li id='debug-shapes'>Shapes</li><li id='debug-zones'>Zones</li></ul><div id='debug-control-toggle'>&raquo;</div></div>")
            .appendTo("body");
        $("#debug-shapes").click(function () {
            var _this = $(this);
            $("html").toggleClass(_this.attr("id"));
            $(this).toggleClass("debug-active");
        });
        $("#debug-zones").click(function () {
            var _this = $(this);
            $("html").toggleClass(_this.attr("id"));
            $(this).toggleClass("debug-active");

            if ($(this).hasClass("debug-active")) {

                // renders the zone name in each zone
                $(".zone").each(function () {
                    var classes = $(this).attr("class").split(' ');

                    for (i = 0; i < classes.length; i++) {
                        if (classes[i].indexOf("zone-") === 0) {
                            $(this).append('<div class="zone-name">' + classes[i].substr(classes[i].indexOf("-") + 1) + '</div>');
                        }
                    }

                });
            }
            else {
                $(".zone-name").remove();
            }

        });
        $("#debug-control-toggle").click(function () {
            var _this = $(this), open = "debug-open";
            if (_this.is("." + open)) {
                _this.prev().hide("fast", function () { _this.removeClass(open).html("&raquo;"); });
            } else {
                _this.prev().show("fast", function () { _this.addClass(open).html("&laquo;"); });
            }
        });

        $("div.shape-tracing.wrapper").click(function (e) {
            var _this = $(this);
            var classes = $(this).attr("class").split(' ');
            e.stopPropagation();
            for (i = 0; i < classes.length; i++) {
                if (classes[i].indexOf("shapeId-") === 0) {
                    var shapeId = classes[i].substr(classes[i].indexOf("-") + 1);
                    $("div.shape-tracing.wrapper").toggleClass('selected', false);
                    _this.toggleClass('selected', true);
                    $("div.shape-tracing.meta").toggle(false);
                    $("div.shape-tracing.meta.shapeId-" + shapeId).toggle(true);
                }
            }
        });

        /* tabs */
        function bindTab(selector) {
            $('li' + selector).click(function () {
                var _this = $(this);

                // toggle the selected class on the tab li
                _this.parent().children('li').toggleClass('selected', false);
                _this.toggleClass('selected', true);

                // hide all tabs and display the selected one
                var wrapper = _this.parent().parent().first();
                wrapper.children('.content').children().toggle(false);
                wrapper.children('.content').children('div' + selector).toggle(true);

                if (wrapper.children('.content').children('div' + selector).children('.CodeMirror').length == 0) {
                    var textArea = wrapper.children('.content').children('div' + selector).children('textarea').get(0);
                    CodeMirror.fromTextArea(textArea, { mode: "razor", tabMode: "indent", height: "100%" });
                }

            });
        }

        var glyph = $("<span class=\"expando-glyph-container closed\"><span class=\"expando-glyph\"></span>&#8203;</span>");
        $('div.model div.type').prev().prepend(glyph);

        $('div.model ul ul').toggle(false);

        $('span.expando-glyph-container').click(function () {
            var __this = $(this);

            if (__this.hasClass("closed") || __this.hasClass("closing")) {
                __this.parent().parent().parent().children('ul').slideDown(300, function () { __this.removeClass("opening").removeClass("closed").addClass("open"); });
                __this.addClass("opening");
            }
            else {
                __this.parent().parent().parent().children('ul').slideUp(300, function () { __this.removeClass("closing").removeClass("open").addClass("closed"); });
                __this.addClass("closing");
            }

            return false;
        });

        bindTab('.shape');
        bindTab('.model');
        bindTab('.placement');
        bindTab('.templates');
        bindTab('.source');
        bindTab('.html');


    });
})(jQuery);
