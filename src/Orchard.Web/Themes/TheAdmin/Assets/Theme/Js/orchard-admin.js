(function ($) {

    $(document).ready(function () {
        App.init();

        // Pagers.
        $('select.pager.selector').change(function () {
            window.location = $(this).attr("disabled", true).val();
        });

        // Auto-bulk actions.
        $(".bulk-actions-auto select").change(function () {
            $(this).closest("form").find(".apply-bulk-actions-auto:first").click();
        });

        // RemoveUrl and UnsafeUrl.
        $("body").on("click", "[itemprop~='RemoveUrl']", function () {
            // Don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled in base.js.
            if ($(this).filter("[itemprop~='UnsafeUrl']").length === 1) {
                return false;
            }

            // Use a custom message if its set in data-message.
            var dataMessage = $(this).data('message');
            if (dataMessage === undefined) {
                dataMessage = $("[data-default-remove-confirmation-prompt]").data("default-remove-confirmation-prompt");
            }

            return confirm(dataMessage);
        });

        $(".check-all").change(function () {
            $("input[type=checkbox]:not(:disabled)").prop('checked', $(this).prop("checked"));
        });

        // Handle keypress events in bulk action fieldsets that are part of a single form.
        // This will make sure the expected action executes when pressing "enter" on a text field.
        $("form .bulk-actions").on("keypress", "input[type='text']", function (e) {
            if (e.which !== 13)
                return;

            var sender = $(this);
            var fieldset = sender.closest("fieldset.bulk-actions");
            var submitButton = fieldset.find("button[type='submit']");

            if (submitButton.length === 0)
                return;

            e.preventDefault();
            submitButton.click();
        });

        //more-actions
        function sortBy(prop) {
            return function (a, b) {
                var filter_a = parseInt($(a).attr(prop));
                var filter_b = parseInt($(b).attr(prop));
                if (isNaN(filter_a)) {
                    filter_a = -1;
                    return 1;
                }
                if (isNaN(filter_b)) {
                    filter_b = -1;
                    return -1;
                }
                console.log(filter_a + ' > ' + filter_b);
                return filter_a > filter_b
                ? -1
                : (filter_a > filter_b ? 1 : 0);
            }
        }
        function sortDom(filter, elements) {
            //Transform our nodeList into array and apply sort function
            return [].map.call(elements, function (elm) {
                return elm;
            }).sort(sortBy(filter))
        }

        function init() {
            var links = $('.content-items-with-actions li > a');
            //sets the initial sort order
            $.each(links, function (i, el) {
                $(el).attr('data-id', 'action-' + i);
            });
        }
        init();

        function renderContentItemActions() {
            var container = $('.content-items-with-actions li');
            var containerWidth = $('.content-items').outerWidth(true);
            $.each(container, function (i, c) {
                var dropdownToggleWidth = 37;// getElementMinWidth();
                var mainActions = $(c).find('ul.main-actions');
                var moreactions = $(c).find('ul.more-actions');
                $.each(mainActions, function (i, el) {
                    var $el = $(el);
                    var loop = 1;
                    do {
                        var mainActionsWidth = dropdownToggleWidth;
                        var links = $el.find('li:first-child > a:visible');
                        $.each(links, function (i, a) {
                            mainActionsWidth += getElementMinWidth($(a));
                        });
                        if (mainActionsWidth >= containerWidth) {
                            var last = $(sortDom('priority',links)).last();
                            var li = $('<li>');
                            li.append(last.clone())
                            moreactions.prepend(li);
                            last.hide();
                        } else {
                            var first = moreactions.find('li:first > a');
                            var actionId = first.attr('data-id');
                            mainActions
                                .find('li:first-child')
                                .find("a[data-id='" + actionId + "']")
                                .show();//.append(first.clone());
                            first.parent('li').remove();
                        }
                        var mainActionsWidth = dropdownToggleWidth;
                        var links = $el.find('li:first-child > a:visible');
                        $.each(links, function (i, a) {
                            mainActionsWidth += getElementMinWidth($(a));
                        });
                        loop++;
                    } while (loop < 20 && mainActionsWidth > containerWidth)//(mainActionsWidth > containerWidth)
                });

                $.each(moreactions, function (i, el) {
                    var $el = $(el);
                    if ($el.find('li').length > 0) {
                        $(c).find('.more-actions-dropdown').show();
                    }
                    else {
                        $(c).find('.more-actions-dropdown').hide();
                    }
                });
            });
        }

        function getElementMinWidth(el) {
            var props = { position: "absolute", visibility: "hidden", display: "inline-block" };
            var itemWidth = 0;
            if (el != undefined) {
                $.swap(el[0], props, function () {
                    itemWidth = el.outerWidth();
                });
            }
            return itemWidth;
        }
        var id;
        var startWidth = window.innerWidth; //get the original screen width

        renderContentItemActions();
        $(window).resize(function () {
            clearTimeout(id);
            id = setTimeout(doneResizing, 200);
        });
        function doneResizing() {
            renderContentItemActions();
        }

    });
})(jQuery);
