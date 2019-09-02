(function ($) {

    /* Helper functions
    **********************************************************************/
	var createTermCheckbox = function ($wrapper, tag, checked) {
        var $ul = $("ul.terms", $wrapper);
        var singleChoice = $(".terms-editor", $wrapper).data("singlechoice");
        var namePrefix = $wrapper.data("name-prefix");
        var idPrefix = $wrapper.data("id-prefix");
        var nextIndex = $("li", $ul).length;
        var id = isNaN(tag.value) ? -nextIndex : tag.value;
        var checkboxId = idPrefix + "_Terms_" + nextIndex + "__IsChecked";
        var checkboxName = namePrefix + ".Terms[" + nextIndex + "].IsChecked";
        var radioName = namePrefix + ".SingleTermId";
        var checkboxHtml = "<input type=\"checkbox\" value=\"" + (checked ? "true\" checked=\"checked\"" : "false") + " data-term=\"" + tag.label + "\" data-term-identity=\"" + tag.label.toLowerCase() + "\" id=\"" + checkboxId + "\" name=\"" + checkboxName + "\" />";
        var radioHtml = "<input type=\"radio\" value=\"" + id + (checked ? "\" checked=\"checked\"" : "\"") + " data-term=\"" + tag.label + "\" data-term-identity=\"" + tag.label.toLowerCase() + "\" id=\"" + checkboxId + "\" name=\"" + radioName + "\" />";
        var inputHtml = singleChoice ? radioHtml : checkboxHtml;
        var $li = $("<li>" +
                    inputHtml +
                    "<input type=\"hidden\" value=\"" + id + "\" id=\"" + idPrefix + "_Terms_" + nextIndex + "__Id" + "\" name=\"" + namePrefix + ".Terms[" + nextIndex + "].Id" + "\" />" +
                    "<input type=\"hidden\" value=\"" + tag.label + "\" id=\"" + idPrefix + "_Terms_" + nextIndex + "__Name" + "\" name=\"" + namePrefix + ".Terms[" + nextIndex + "].Name" + "\" />" +
                    "<label class=\"forcheckbox\" for=\"" + checkboxId + "\">" + tag.label + "</label>" +
                  "</li>").hide();

        if (checked && singleChoice) {
        	$("input[type='radio']", $ul).removeAttr("checked");
            $("input[type='radio'][name$='IsChecked']", $ul).val("false");
        }

        $ul.append($li);
        $li.fadeIn();
    };

    /* Event handlers
    **********************************************************************/
    var onTagsChanged = function (tagLabelOrValue, action, tag) {

        if (tagLabelOrValue == null)
            return;

        var $input = this.appendTo;
        var $wrapper = $input.parents("fieldset:first");
        var $tagIt = $("ul.tagit", $wrapper);
        var singleChoice = $(".terms-editor", $wrapper).data("singlechoice");
        var $terms = $("ul.terms", $wrapper);
        var initialTags = $(".terms-editor", $wrapper).data("selected-terms");

        if (singleChoice && action == "added") {
            $tagIt.tagit("fill", tag);
        }

        $terms.empty();

        var tags = $tagIt.tagit("tags");
        $(tags).each(function (index, tag) {
        	createTermCheckbox($wrapper, tag, true);
        });

    	// Add any tags that are no longer selected but were initially on page load.
    	// These are required to be posted back so they can be removed.
        var removedTags = $.grep(initialTags, function (initialTag) { return $.grep(tags, function (tag) { return tag.value === initialTag.value }).length === 0 });
        $(removedTags).each(function (index, tag) {
        	createTermCheckbox($wrapper, tag, false);
        });

        $(".no-terms", $wrapper).hide();
    };

    var renderAutocompleteItem = function (ul, item) {

        var label = item.label;

        for (var i = 0; i < item.levels; i++) {
            label = "<span class=\"gap\">&nbsp;</span>" + label;
        }

        var li = item.disabled ? "<li class=\"disabled\"></li>" : "<li></li>";

        return $(li)
            .data("item.autocomplete", item)
            .append($("<a></a>").html(label))
            .appendTo(ul);
    };

    /* Initialization
    **********************************************************************/
    $(".terms-editor").each(function () {
        var selectedTerms = $(this).data("selected-terms");

        var autocompleteUrl = $(this).data("autocomplete-url");

        var $tagit = $("> ul", this).tagit({
            tagSource: function (request, response) {
                var termsEditor = $(this.element).parents(".terms-editor");
                $.getJSON(autocompleteUrl, { taxonomyId: termsEditor.data("taxonomy-id"), leavesOnly: termsEditor.data("leaves-only"), query: request.term }, function (data, status, xhr) {
                    response(data);
                });
            },
            initialTags: selectedTerms,
            triggerKeys: ['enter', 'tab'], // default is ['enter', 'space', 'comma', 'tab'] but we remove comma and space to allow them in the terms
            allowNewTags: $(this).data("allow-new-terms"),
            tagsChanged: onTagsChanged,
            caseSensitive: false,
            minLength: 0,
            sortable: true
        }).data("ui-tagit");

        $tagit.input.autocomplete().data("ui-autocomplete")._renderItem = renderAutocompleteItem;
        $tagit.input.autocomplete().on("autocompletefocus", function (event, ui) {
            event.preventDefault();
        });

    });

})(jQuery);