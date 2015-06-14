(function ($)
{
    $(function ()
    {
        $(".engine-picker").on("change", function (e)
        {
            var engine = $(this).val();

            $(".engine-settings-editor").hide();
            $("[data-engine='" + engine + "']").show();
        }).trigger("change");
    });
})(jQuery);