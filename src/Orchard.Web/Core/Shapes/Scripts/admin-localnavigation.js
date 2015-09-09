(function($) {
    $(function () {
        var allViews = [];

        $("#local-navigation").on("click", "a", function(e) {
            e.preventDefault();

            for (i = 0; i < allViews.length; i++) {
                allViews[i].hide();
            }

            $("#local-navigation li.selected").removeClass("selected");

            var tab = $(this);
            var selector = tab.attr("href");
            var views = $(selector);

            tab.closest("li").addClass("selected");
            views.show();
        });

        $("#local-navigation a").each(function (e) {
            var tab = $(this);
            var selector = tab.attr("href");
            var views = $(selector);

            views.hide();
            allViews.push(views);
        });

        $("#local-navigation a:first").click();
    });
})(jQuery);