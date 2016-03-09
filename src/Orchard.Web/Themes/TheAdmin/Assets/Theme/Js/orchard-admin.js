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

        function renderContentItemActions() {
            var container = $('.content-items li');
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
                        var links = $el.find('li:first-child > a');
                        $.each(links, function (i, a) {
                            mainActionsWidth += getElementMinWidth($(a));
                        });
                        if (mainActionsWidth >= containerWidth) {
                            var last = links.last();
                            var li = $('<li>');
                            li.append(last.clone())
                            moreactions.prepend(li);
                            last.remove();
                        } else {
                            var first = moreactions.find('li:first > a');
                            mainActions.find('li:first-child').append(first.clone());
                            first.parent('li').remove();
                        }
                        var mainActionsWidth = dropdownToggleWidth;
                        var links = $el.find('li:first-child > a');
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


        (function ($, sr) {

            // debouncing function from John Hann
            // http://unscriptable.com/index.php/2009/03/20/debouncing-javascript-methods/
            var debounce = function (func, threshold, execAsap) {
                var timeout;

                return function debounced() {
                    var obj = this, args = arguments;
                    function delayed() {
                        if (!execAsap)
                            func.apply(obj, args);
                        timeout = null;
                    };

                    if (timeout)
                        clearTimeout(timeout);
                    else if (execAsap)
                        func.apply(obj, args);

                    timeout = setTimeout(delayed, threshold || 100);
                };
            }
            // smartresize 
            jQuery.fn[sr] = function (fn) { return fn ? this.bind('resize', debounce(fn)) : this.trigger(sr); };

        })(jQuery, 'smartresize');

        renderContentItemActions();

        $(window).smartresize(function () {
            renderContentItemActions();
        },200);

    });
})(jQuery);
