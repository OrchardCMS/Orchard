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
                 var parentTaxonomyElement = $("#" + taxonomySelectList.data("taxonomy-element-parent-name"));
                var parentInputType = taxonomySelectList.data("taxonomy-element-parent-input-type");
                parentTaxonomyElement.on('change', updateChildrenTerms);
                function updateChildrenTerms() {
                    var currentInput = taxonomySelectList.find("input");
                    $.each(currentInput, function () {
                        if (this.checked) {
                            this.checked = false;
                        }
                    });
                    taxonomySelectList.trigger("change");
                    
                    taxonomySelectList.children().remove().end();
                    var parentTermIds = [];
                    var parentTaxonomyElementOptions = $("[name=" + taxonomySelectList.data("taxonomy-element-parent-name") + "]");
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
                            parentTermIds = [];
                        parentTermIds = [parentTermIds];
                    }
                    var param = "";
                    if (parentTermIds != null)
                        param = parentTermIds.toString();
                    var ajaxUrl = url + "&parentTermIds=" + param;
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
