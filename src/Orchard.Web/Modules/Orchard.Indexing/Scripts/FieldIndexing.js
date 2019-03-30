$(".indexing-controlling-checkbox").change(function () {
    $("[data-indexing-controlled-by*='" + $(this).attr("name") + "']").toggle($(this).prop("checked"));
}).trigger("change");