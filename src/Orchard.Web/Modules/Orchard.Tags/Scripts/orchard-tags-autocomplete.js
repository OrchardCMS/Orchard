(function ($) {
    $.extend({
        tagsAutocomplete: {
            minSnippetLength: 3,

            autocomplete: function (textboxId, fetchUrl, minSnippetLength) {
                if (minSnippetLength != null) this.minSnippetLength = minSnippetLength;

                function split(val) {
                    return val.split(/,\s*/);
                }
                function extractLast(snippet) {
                    return split(snippet).pop();
                }

                var that = this;
                var textBox = $('#' + textboxId);

                textBox.bind('keydown', function (event) {
                    // don't navigate away from the field on tab when selecting an item
                    if (event.keyCode === $.ui.keyCode.TAB && $(this).data('autocomplete').menu.active) {
                        event.preventDefault();
                    }
                }).autocomplete({
                    source: function (request, response) {
                        $.getJSON(fetchUrl, {
                            snippet: extractLast(request.term)
                        }, response);
                    },
                    appendTo: textBox.parent(),
                    search: function () {
                        // custom minLength
                        var snippet = extractLast(this.value);
                        if (snippet.length < that.minSnippetLength) {
                            return false;
                        }
                    },
                    focus: function () {
                        // prevent value inserted on focus
                        return false;
                    },
                    select: function (event, ui) {
                        var snippets = split(this.value);
                        // remove the current input
                        snippets.pop();
                        // add the selected item
                        snippets.push(ui.item.value);
                        // add placeholder to get the comma-and-space at the end
                        snippets.push('');
                        this.value = snippets.join(', ');
                        return false;
                    }
                });
            }
        }
    });
})(jQuery);