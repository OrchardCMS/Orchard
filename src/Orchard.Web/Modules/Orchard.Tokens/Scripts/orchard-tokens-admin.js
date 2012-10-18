jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function(i) {
            if (document.selection) {
                //For browsers like Internet Explorer
                this.focus();
                sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            } else if (this.selectionStart || this.selectionStart == '0') {
                //For browsers like Firefox and Webkit based
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        });
    }
});

jQuery(function ($) {

    // provide autocomplete behavior to tokenized inputs
    // tokensUrl is initialized from the view
    $.get(tokensUrl, function (data) {
        $('.tokenized')
            .autocomplete({
                minLength: 0,
                source: data,
                select: function (event, ui) {
                    $(this).insertAtCaret(ui.item.value);
                    return false;
                }
            }).each(function () {
                $(this).data('autocomplete')._renderItem = function (ul, item) {
                    var result = item.value == '' ? $('<li class="accategory"></li>') : $("<li></li>");

                    var desc = item.desc.length > 50 ? item.desc.substring(0, 50) + "..." : item.desc;

                    return result
                        .data("item.autocomplete", item)
                        .append('<a ><span class="aclabel">' + item.label + ' </span><span class="acvalue">' + item.value + ' </span><span class="acdesc">' + desc + "</span></a>")
                        .appendTo(ul);
                };
            });
    });

    // add an icon to tokenized inputs
    $('.tokenized').wrap('<span class="token-wrapper"></span>');

    $('.token-wrapper').prepend('<div><span class="tokenized-popup">&nbsp;</span></div>');

    // show the full list of tokens when the icon is clicked
    $('.tokenized-popup').click(function () {
        var input = $(this).parent().next();
        // pass empty string as value to search for, displaying all results
        input.autocomplete("search", "");
        input.focus();
    });
});
