(function($) {
    $(function() {
        $("<div id='debug-control'><ul><li id='debug-shape-templates'>shape templates</li><li id='debug-shape-zones'>Zones</li></ul><div id='debug-control-toggle'>&raquo;</div></div>")
            .appendTo("body");
        $("#debug-shape-templates").click(function () {
            var _this = $(this);
            $("html").toggleClass(_this.attr("id"));
            $(this).toggleClass("debug-active");
        });
        $("#debug-shape-zones").click(function () {
            var _this = $(this);
            $("html").toggleClass(_this.attr("id"));
            $(this).toggleClass("debug-active");
        });
        $("#debug-control-toggle").click(function () {
            var _this = $(this), open = "debug-open";
            if (_this.is("."+open)) {
                _this.prev().hide("fast", function() {_this.removeClass(open).html("&raquo;");});
            } else {
                _this.prev().show("fast", function() {_this.addClass(open).html("&laquo;");});
            }
        });
    });
})(jQuery);
