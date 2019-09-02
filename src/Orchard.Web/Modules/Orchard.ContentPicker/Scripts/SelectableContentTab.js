jQuery(function ($) {

    Initialize = function () {
        $('.content-picker-itemCheck').each(function () {
            var related = $(this).siblings('.content-picker-item').children('.related');

            if (window.sessionStorage.getItem(related.data("id")) != null) {
                $(this).prop('checked', true);
            }
        });

        $('.content-picker-itemCheck').change(function () {
            var related = $(this).siblings('.content-picker-item').children('.related');

            if (this.checked) {
                var data = {
                    id: related.data("id"),
                    displayText: related.data("display-text"),
                    editLink: related.data("edit-link"),
                    editUrl: related.data("edit-url"),
                    adminUrl: related.data("admin-url"),
                    displayLink: related.data("display-link"),
                    published: related.data("published")
                };

                window.sessionStorage.setItem(related.data("id"), JSON.stringify(data));
            } else {
                window.sessionStorage.removeItem(related.data("id"));
            }
        });

        $('.button.addSelected').on('click', function () {
            var itemsToAdd = new Array();
            for (var i = 0; i < sessionStorage.length; i++) {
                var data = window.sessionStorage.getItem(sessionStorage.key(i));
                itemsToAdd.push(JSON.parse(data));
            }
            window.sessionStorage.clear();
            window.opener.jQuery[query("callback")](itemsToAdd);
            window.close();
        });

        $('.content-picker-SelectAll').on('click', function () {
            $('.content-picker-itemCheck').prop('checked', $(this).prop("checked"));
            $('.content-picker-itemCheck').change();
        });
    };

    $(document).ready(function () {
        return Initialize();
    });
});