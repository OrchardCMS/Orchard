(function ($) {
    var ElementBrowser = function (element) {
        var self = this;
        this.element = element;

        this.initialize = function () {
            self.element.find(".element-categories").on("click", "a.category", function (e) {
                var categoryLink = $(this);
                var href = categoryLink.attr("href");

                self.element.find(".element-categories li").removeClass("selected");
                categoryLink.closest("li").addClass("selected");
                self.element.find(".elements").hide();
                self.element.find(href).show();
                e.preventDefault();
            });

            self.element.find(".element-categories a").first().click();
        }
    };

    $(function() {
        var browser = new ElementBrowser($("#main"));
        browser.initialize();
    });
})(jQuery);