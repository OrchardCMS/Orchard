(function ($) {
    var searchFields = $(".search-fields");
    var selectedIndex = searchFields.data("selected-index");
    $('li[data-index]').hide();
    $("li[data-index='" + selectedIndex + "']").show();
    $('#selectIndex').change(function () {
        $('li[data-index]').hide();
        $("li[data-index='" + $(this).val() + "']").show();
    });

})(jQuery);