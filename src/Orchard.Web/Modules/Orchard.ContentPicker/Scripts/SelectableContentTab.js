jQuery(function ($) {

    Initialize = function () {
 
        $('.button.addSelected').on('click', function () {
            var selectedItems = $('.content-picker-itemCheck:checked');
            var itemsToAdd = new Array();
            $.each(selectedItems, function (index, item) {
                var related = $(item).siblings('.content-picker-item').children('.related');
                var data = {
                    id: related.data("id"),
                    displayText: related.data("display-text"),
                    editLink: related.data("edit-link"),
                    editUrl: related.data("edit-url"),
                    adminUrl: related.data("admin-url"),
                    displayLink: related.data("display-link"),
                    published: related.data("published")
                };
                return itemsToAdd.push(data);
            });
            window.opener.jQuery[query("callback")](itemsToAdd);
            window.close();
        });
        $('.content-picker-SelectAll').on('click', function () {
            $('.content-picker-itemCheck').prop('checked', $(this).prop("checked"));
        });
    };

    $(document).ready(function () {
        return Initialize();
    });
});