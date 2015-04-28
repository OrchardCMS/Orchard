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

    $(function () {
        var browser = new AutorouteCultureBrowser($("#main"));
        browser.initialize();
    });
})(jQuery);