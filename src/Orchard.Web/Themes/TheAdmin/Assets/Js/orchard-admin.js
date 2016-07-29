(function ($) {

    $(document).ready(function () {
        App.init();

        // Pagers.
        $("select.pager.selector").change(function () {
            window.location = $(this).attr("disabled", true).val();
        });

        // Auto-bulk actions.
        $(".bulk-actions-auto select").change(function () {
            $(this).closest("form").find(".apply-bulk-actions-auto:first").click();
        });

        // RemoveUrl and UnsafeUrl.
        $("body").on("click", "[itemprop~='RemoveUrl']", function () {
            // Don"t show the confirm dialog if the link is also UnsafeUrl, as it will already be handled in base.js.
            if ($(this).filter("[itemprop~='UnsafeUrl']").length === 1) {
                return false;
            }

            // Use a custom message if its set in data-message.
            var dataMessage = $(this).data("message");
            if (dataMessage === undefined) {
                dataMessage = $("[data-default-remove-confirmation-prompt]").data("default-remove-confirmation-prompt");
            }

            return confirm(dataMessage);
        });

        $(".check-all").change(function () {
            $("input[type=checkbox]:not(:disabled)").prop("checked", $(this).prop("checked"));
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
                //console.log(filter_a + " > " + filter_b);
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
        function last(array, n) {
            if (array === null)
                return void 0;
            if (n === null)
                return array[array.length - 1];
            return array.slice(Math.max(array.length - n, 0));
        };
        function init() {
            var links = $(".content-items-with-actions li > a");
            //sets the initial sort order
            $.each(links, function (i, el) {
                $(el).attr("data-id", "action-" + i);
            });
        }
        init();

        function renderContentItemActions() {
            var container = $(".content-items-with-actions li");
            var containerWidth = $(".content-items").outerWidth(true);
            $.each(container, function (i, c) {
                var dropdownToggleWidth = 37;// getElementMinWidth();
                var mainActions = $(c).find("ul.main-actions");
                var moreactions = $(c).find("ul.more-actions");
                $.each(mainActions, function (i, el) {
                    var $el = $(el);
                    var loop = 1;
                    do {
                        var mainActionsWidth = dropdownToggleWidth;
                        var links = $el.find("li:first-child > a:visible");
                        $.each(links, function (i, a) {
                            mainActionsWidth += getElementMinWidth($(a));
                        });
                        if (mainActionsWidth >= containerWidth) {
                            var last = $(sortDom("priority", links)).last();
                            if (last.length > 0) {
                                var li = $("<li>");
                                li.append(last.clone())
                                moreactions.prepend(li);
                                last.hide();
                                mainActionsWidth -= getElementMinWidth(last);
                            }
                        } else {
                            var first = moreactions.find("li:first > a").first();
                            if (first.length > 0) {
                                var actionId = first.attr("data-id");
                                var mainAction = mainActions
                                    .find("li a[data-id='" + actionId + "']");
                                mainAction.show()
                                first.parent("li").remove();
                                mainActionsWidth += getElementMinWidth(mainAction);
                            }
                        }
                        //var mainActionsWidth = dropdownToggleWidth;
                        //var links = $el.find("li:first-child > a:visible");
                        //$.each(links, function (i, a) {
                        //    mainActionsWidth += getElementMinWidth($(a));
                        //});
                        loop++;
                    } while (loop < 20 && mainActionsWidth > containerWidth)//(mainActionsWidth > containerWidth)
                });

                $.each(moreactions, function (i, el) {
                    var $el = $(el);
                    if ($el.find("li").length > 0) {
                        $(c).find(".more-actions-dropdown").show();
                    }
                    else {
                        $(c).find(".more-actions-dropdown").hide();
                    }
                });
            });
        }

        function getElementMinWidth(el) {
            var props = { position: "absolute", visibility: "hidden", display: "inline-block" };
            var itemWidth = 0;
            if (el !== undefined) {
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

        // Verify if object exist
        jQuery.fn.exists = function () { return this.length > 0; }

        // Bootstrap modal confirmation.
        $("[data-confirm]").unbind('click');
        $("[data-confirm]").click(function (e) {
            var href = $(this).attr("href");
            var dialog = $("#confirmationDialog").clone();

            $("body").append(dialog);
            dialog.find(".modal-body").text($(this).data("confirm"));
            dialog.find(".dialog-command-yes").attr("href", href);
            dialog.modal({ show: true });

            dialog.find(".dialog-command-yes").on("click", function (e) {
                
                var magicToken = $("input[name='__RequestVerificationToken']").first();
                if (!magicToken) { return; } // No sense in continuing if form POSTS will fail.

                var form = $("<form action=\"" + href + "\" method=\"POST\" />");
                form.append(magicToken.clone());
                form.css({ "position": "absolute", "left": "-9999em" });
                $("body").append(form);

                form.submit();
                e.preventDefault();
            });

            e.preventDefault();
        });

        // Bulk action confirmation.
        $("[data-bulk-action]").unbind('click');
        $("[data-bulk-action]").click(function (e) {
            var trigger = $(this);
            var action = trigger.data("bulk-action");
            $("#bulkActions").val(action);

            var dialog = $("#confirmationDialog").clone();

            $("body").append(dialog);
            dialog.find(".modal-body").text($(this).data("bulk-action-confirm"));
            dialog.modal({ show: true });

            dialog.find(".dialog-command-yes").on("click", function (e) {
                submitForm(action); //replace trigger.closest("form").submit(); unless we remove the controller FormValueRequired everywhere
                e.preventDefault();
            });

            e.preventDefault();
        });

        //We need to click the hidden submit button to POST the form.
        //It is needed for the controller action(s) to be hit
        function submitForm(action) {
            //verify the use of one name or the other
            //TODO standardize name in all views and controllers
            if ($('button[name="submit.BulkEdit"]').exists()) {
                var button = $('button[name="submit.BulkEdit"]').click();
            }
            else if ($('button[name="submit.BulkExecute"]').exists()) {
                var button = $('button[name="submit.BulkExecute"]').click();
            }
        }

        // End Bulk edit bootstrap dropdown button actions
    });
})(jQuery);