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
                //console.log(filter_a + ' > ' + filter_b);
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
            if (array == null)
                return void 0;
            if (n == null)
                return array[array.length - 1];
            return array.slice(Math.max(array.length - n, 0));
        };
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
                            var last = $(sortDom('priority', links)).last();
                            if (last.length > 0) {
                                var li = $('<li>');
                                li.append(last.clone())
                                moreactions.prepend(li);
                                last.hide();
                                mainActionsWidth -= getElementMinWidth(last);
                            }
                        } else {
                            var first = moreactions.find('li:first > a').first();
                            if (first.length > 0) {
                                var actionId = first.attr('data-id');
                                var mainAction = mainActions
                                    .find('li a[data-id="' + actionId + '"]');
                                mainAction.show()
                                first.parent('li').remove();
                                mainActionsWidth += getElementMinWidth(mainAction);
                            }
                        }
                        //var mainActionsWidth = dropdownToggleWidth;
                        //var links = $el.find('li:first-child > a:visible');
                        //$.each(links, function (i, a) {
                        //    mainActionsWidth += getElementMinWidth($(a));
                        //});
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

        // Bulk edit bootstrap dropdown button actions

        // Verify if object exist
        jQuery.fn.exists = function () { return this.length > 0; }

        $("#dataConfirmOK").on("click", function () {
            var _this = $(this);
            var hrefParts = _this.attr("href").split("?");

            //for single action
            if ($('#publishActions').val() == 'None') {
                var magicToken = $("input[name=__RequestVerificationToken]").first();
                if (!magicToken) { return; } // no sense in continuing if form POSTS will fail


                var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
                form.append(magicToken.clone());
                if (hrefParts.length > 1) {
                    var queryParts = hrefParts[1].split("&");
                    for (var i = 0; i < queryParts.length; i++) {
                        var queryPartKVP = queryParts[i].split("=");
                        //trusting hrefs in the page here
                        form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
                    }
                }
                form.css({ "position": "absolute", "left": "-9999em" });
                $("body").append(form);

                var unsafeUrlPrompt = _this.data("unsafe-url");

                if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                    if (!confirm(unsafeUrlPrompt)) {
                        return false;
                    }
                }

                if (_this.filter("[itemprop~='RemoveUrl']").length == 1) {
                    // use a custom message if its set in data-message
                    var dataMessage = _this.data('message');
                    if (dataMessage === undefined) {
                        dataMessage = confirmRemoveMessage;
                    }

                    if (!confirm(dataMessage)) {
                        return false;
                    }
                }

                form.submit();
                return false;
            }
            else {
                //for bulk actions
                submitForm($('#publishActions').val());
            }
        });

        //Bootstrap modal confirm
        $('[data-confirm]').click(function (ev) {
            var href = $(this).attr('href');
            $('#publishActions').val('None');

            $('#dataConfirmModal').find('.modal-body').text($(this).attr('data-confirm'));
            $('#dataConfirmOK').attr('href', href);
            $('#dataConfirmModal').modal({ show: true });

            return false;
        });

        $('[data-action]').click(function (ev) {
            var action = $(this).attr("data-action");
            $('#publishActions').val(action);

            var href = $(this).attr('href');
            $('#dataConfirmModal').find('.modal-body').text($(this).attr('data-confirm'));
            $('#dataConfirmOK').attr('href', href);
            $('#dataConfirmModal').modal({ show: true });

            return false;
        });

        function submitForm(action) {
            //verify the use of one name or the other
            //TODO standardize name in all views
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