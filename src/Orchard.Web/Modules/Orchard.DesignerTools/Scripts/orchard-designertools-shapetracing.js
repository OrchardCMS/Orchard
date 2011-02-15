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
                            $(this).append('<div class="zone-name">' + classes[i].substr(classes[i].indexOf("-")+1) + '</div>');
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
    });
})(jQuery);
