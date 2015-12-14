(function ($) {

    /* Helper functions
    **********************************************************************/
    var addTag = function ($plugin, label) {
        $plugin.tagit("add", label);
    };

    var removeTag = function ($plugin, label) {
        var tags = $plugin.tagit("tags");
        var index = findTagIndexByLabel(tags, label);

        if (index == -1)
            return;

        tags.splice(index, 1);
        $plugin.tagit("fill", tags);
    };

    var findTagIndexByLabel = function (tags, label) {
        for (var i = 0; i < tags.length; i++) {
            var tag = tags[i];

            if (tag.label == label) {
                return i;
            }
        }
        return -1;
    };

    var createTermCheckbox = function ($wrapper, tag) {
        var $ul = $("ul.terms", $wrapper);
        var singleChoice = $(".terms-editor", $wrapper).data("singlechoice");
        var namePrefix = $wrapper.data("name-prefix");
        var idPrefix = $wrapper.data("id-prefix");
        var nextIndex = $("li", $ul).length;
        var checkboxId = idPrefix + "_Terms_" + nextIndex + "__IsChecked";
        var checkboxName = namePrefix + ".Terms[" + nextIndex + "].IsChecked";
        var radioName = namePrefix + ".SingleTermId";
        var checkboxHtml = "<input type=\"checkbox\" value=\"true\" checked=\"checked\" data-term=\"" + tag + "\" data-term-identity=\"" + tag.toLowerCase() + "\" id=\"" + checkboxId + "\" name=\"" + checkboxName + "\" />";
        var radioHtml = "<input type=\"radio\" value=\"" + -nextIndex + "\" checked=\"checked\" data-term=\"" + tag + "\" data-term-identity=\"" + tag.toLowerCase() + "\" id=\"" + checkboxId + "\" name=\"" + radioName + "\" />";
        var inputHtml = singleChoice ? radioHtml : checkboxHtml;
        var $li = $("<li>" +
                    inputHtml +
                    "<input type=\"hidden\" value=\"" + -nextIndex + "\" id=\"" + idPrefix + "_Terms_" + nextIndex + "__Id" + "\" name=\"" + namePrefix + ".Terms[" + nextIndex + "].Id" + "\" />" +
                    "<input type=\"hidden\" value=\"" + tag + "\" id=\"" + idPrefix + "_Terms_" + nextIndex + "__Name" + "\" name=\"" + namePrefix + ".Terms[" + nextIndex + "].Name" + "\" />" +
                    "<label class=\"forcheckbox\" for=\"" + checkboxId + "\">" + tag + "</label>" +
                  "</li>").hide();

        if (singleChoice) {
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
        var $termCheckbox = $("input[data-term-identity='" + tag.label.toLowerCase() + "']", $terms).filter(function () {
            return $(this).siblings("input[value='" + tag.value + "']").length;
        });
        
        if ($termCheckbox.is(":disabled")) {
            removeTag($tagIt, tagLabelOrValue);
            return;
        }

        if ($termCheckbox.length == 0 && action == "added") {
            createTermCheckbox($wrapper, tag.label, this);
        }

        $termCheckbox.prop("checked", action == "added");

        if (singleChoice && action == "added") {
            $tagIt.tagit("fill", [tag.label]);
        }

        if (singleChoice) {
            if (action == "added") {
                var $option = $("select.term-picker", $wrapper).find("option[data-term-identity='" + tag.label.toLowerCase() + "']");
                $option.remove();
            } else {
                $("select.term-picker", $wrapper).append("<option data-term=\"" + tag.label + "\" data-term-identity=\"" + tag.label.toLowerCase() + "\">" + tag.label + "</option>");
            }
        }

        $(".no-terms", $wrapper).hide();
    };

    $("fieldset.taxonomy-wrapper .expando").on("change", "input[data-term]:enabled", function(e) {
        var $checkbox = $(this);
        var term = $checkbox.data("term");
        var $wrapper = $checkbox.parents("fieldset.taxonomy-wrapper:first");
        var $tagIt = $("ul.tagit", $wrapper);
        var isChecked = $checkbox.is(":checked");

        isChecked ? addTag($tagIt, term) : removeTag($tagIt, term);
    });

    $("select.term-picker").change(function (e) {
        var $select = $(this);
        var $firstOption = $("option:first", $select);

        if ($firstOption.is(":selected"))
            return;

        var $selecedOption = $("option:selected", $select);
        var $wrapper = $select.parents("fieldset.taxonomy-wrapper:first");
        var $tagIt = $("ul.tagit", $wrapper);
        var term = $selecedOption.text();
        var singleChoice = $(".terms-editor", $wrapper).data("singlechoice");

        addTag($tagIt, term);
        $select.val("");

        if (!singleChoice)
            $selecedOption.remove();
    });

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
        var allTerms = $(this).data("all-terms");
        var selectedTerms = $(this).data("selected-terms");

        var $tagit = $("> ul", this).tagit({
            tagSource: allTerms,
            initialTags: selectedTerms,
            triggerKeys: ['enter', 'tab'], // default is ['enter', 'space', 'comma', 'tab'] but we remove comma and space to allow them in the terms
            allowNewTags: $(this).data("allow-new-terms"),
            tagsChanged: onTagsChanged,
            caseSensitive: false,
            minLength: 0
        }).data("uiTagit");

        $tagit.input.autocomplete().data("uiAutocomplete")._renderItem = renderAutocompleteItem;

    });

})(jQuery);