var DynamicForms;
(function (DynamicForms) {
    var TaxonomyElement;
    (function (TaxonomyElement) {
        $(function () {
            $(".taxonomy-element").each(function () {
                var taxonomySelectList = $(this);
                var taxonomySelectListId = this.id;
                var url = taxonomySelectList.data("taxonomy-element-ajax-url");
                var inputType = taxonomySelectList.data("taxonomy-element-input-type");
                var parentTaxonomyElementOptions = $("[name=" + taxonomySelectList.data("taxonomy-element-parent-name") + "]");
                var parentInputType = taxonomySelectList.data("taxonomy-element-parent-input-type");
                parentTaxonomyElementOptions.on('change', updateChildrenTerms);

                function updateChildrenTerms() {
                    taxonomySelectList.children().remove().end();
                    var parentTermIds = [];
                    if (parentInputType == "CheckList" || parentInputType == "RadioList") {
                        $.each(parentTaxonomyElementOptions, function () {
                            if (this.checked)
                                parentTermIds.push($(this).val());
                        });
                    }
                    else
                        parentTermIds = parentTaxonomyElementOptions.val();
                    if (typeof parentTermIds === 'string') {
                        if (parentTermIds === 0)
                            return;
                        parentTermIds = [parentTermIds];
                    }
                    var ajaxUrl = url + "&parentTermIds=" + parentTermIds.toString();
                    var parent = taxonomySelectList.parent();
                    if (ajaxUrl) {
                        var update = function (url, target) {
                            $.get(url, function (json) {
                                jQuery.each(json, function () {
                                    if (inputType == "CheckList" || inputType == "RadioList") {
                                        var type = (inputType == "CheckList") ? "checkbox" : "radio";
                                        taxonomySelectList.append($("<li><label><input name='" + taxonomySelectListId + "' type='" + type + "' value='" + this.value + "'>" + this.text + "</label></li>"));
                                    }
                                    else {
                                        taxonomySelectList.append($("<option></option>")
                                        .attr("value", this.value)
                                        .text(this.text));
                                    }
                                });
                                if (json.length===0)
                                    taxonomySelectList.append($("<option></option>"));
                            }).fail(function () {
                            });
                        };
                        update(ajaxUrl, parent);
                    }
                }
            });
        });
    })(TaxonomyElement = DynamicForms.TaxonomyElement || (DynamicForms.TaxonomyElement = {}));
})(DynamicForms || (DynamicForms = {}));
