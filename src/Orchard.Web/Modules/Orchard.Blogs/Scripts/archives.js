/** archives **/
(function ($) {
    $(function() {
        $('.archives ul.years li.previous').each(function() {
            $(this).click(function(ev) {
                if (!ev || $(ev.target).not("a").size()) {
                    $(this).toggleClass("open");
                    $(this).find("h4>span").toggle();
                    $(this).children("ul").toggle();
                }
            });

            //$(this).hoverClassIfy();
        });
    });
})(jQuery);