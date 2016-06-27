(function ($) {

    var initLocalNav = function () {
        if ($(".localmenu-element-admin").length === 0)
            return;

        var tabViews = $(".tab-view").hide();

        $(".localmenu-element-admin").on("click", "a", function(e) {
            e.preventDefault();

            var link = $(this);
            var localMenu = link.closest(".localmenu-element-admin");
            var parent = link.closest("li");
            var targetTabView = $(link.attr("href"));
            
            localMenu.find("li").removeClass("selected");
            parent.addClass("selected");
            tabViews.hide();
            targetTabView.show();
        });

        $(".localmenu-element-admin li:first a").click();
    };

    $(function() {
        initLocalNav();
    });
})(jQuery);