(function ($) {
    var AutorouteCultureBrowser = function (culture) {
        var self = this;
        this.culture = culture;

        this.initialize = function () {
            self.culture.find(".autoroute-cultures").on("click", "a.culture", function (e) {
                var categoryLink = $(this);
                var href = categoryLink.attr("href");

                self.culture.find(".autoroute-cultures li").removeClass("selected");
                categoryLink.closest("li").addClass("selected");
                self.culture.find(".items").hide();
                self.culture.find(href).show();
                e.preventDefault();
            });

            self.culture.find(".autoroute-cultures a").first().click();
        }
    };

    $(".use-culture-pattern[type=checkbox]").click(function () {
        if ($(this).attr("checked") == "checked") {
            $(".autoroute-cultures li:not(:first)").hide();
            $(".autoroute-cultures li").removeClass("selected");
            $(".autoroute-cultures li:first").addClass("selected");
            $("#content .items").hide();
            $("#content .items.default").show();
            $(this).removeAttr('checked');
        } else {
            $(".autoroute-cultures li:not(:first)").show();
            $("#content .items.default").show();
            $(this).attr('checked', 'checked');
        }
    });

    $(function () {
        var browser = new AutorouteCultureBrowser($("#main"));
        browser.initialize();

    });
})(jQuery);