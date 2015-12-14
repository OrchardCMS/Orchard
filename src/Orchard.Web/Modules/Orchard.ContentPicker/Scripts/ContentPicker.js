jQuery(function ($) {

    $("#layout-content").on("orchard-admin-contentpicker-open", "form", function (ev, data) {
        data = data || {};
        // the popup will be doing full page reloads, so will not be able to retain
        // a pointer to the callback. We will generate a temporary callback
        // with a known/unique name and pass that in on the querystring so it
        // is remembers across reloads. Once executed, it calls the real callback
        // and removes itself.
        var callbackName = "_contentpicker_" + new Date().getTime();
        data.callbackName = callbackName;
        $[callbackName] = function (returnData) {
            delete $[callbackName];
            data.callback(returnData);
        };
        $[callbackName].data = data;

        var baseUrl = data.baseUrl;

        // remove trailing slash if any
        if (baseUrl.slice(-1) == '/')
            baseUrl = baseUrl.substr(0, baseUrl.length - 1);

        var url = baseUrl + "/Admin/Orchard.ContentPicker?";
        
        if (data.types) {
            url += "types=" + encodeURIComponent(data.types) + "&";
        }

        url += "callback=" + callbackName + "&" + (new Date() - 0);

        if (data.part) {
            url += "&part=" + encodeURIComponent(data.part);
        }

        if (data.field) {
            url += "&field=" + encodeURIComponent(data.field);
        }

        var w = window.open(url, "_blank", data.windowFeatures || "width=685,height=700,status=no,toolbar=no,location=no,menubar=no,resizable=no,scrollbars=yes");
    });
    
    // Render content items table and initialize controls.
    (function() {
        $("fieldset.content-picker-field").each(function() {
            var self = this;
            var required = $(self).data("required");
            var multiple = $(self).data("multiple");
            var addButton = $(self).find(".button.add");
            var removeText = $(self).data("remove-text");
            var notPublishedText = $(self).data("not-published-text");
            var template = '<tr><td>&nbsp;</td><td><span data-id="{contentItemId}" data-fieldid="@idsFieldId" class="content-picker-item">{edit-link}{status-text}</span></td><td><span data-id="{contentItemId}" class="content-picker-remove button grey">' + removeText + '</span></td></tr>';
            var selectedItemsFieldname = $(self).data("selected-items-fieldname");
            var baseUrl = $(self).data("base-url");
            var partName = $(self).data("part-name");
            var fieldName = $(self).data("field-name");
            var types = $(self).data("types");
            
            var refreshIds = function() {
                var id = $("[name='" + selectedItemsFieldname + "']");
                var fieldId = $(self).find("span[data-fieldid]");
                
                id.val("");
                fieldId.each(function() {
                    id.val(id.val() + "," + $(this).attr("data-id"));
                });

                var itemsCount = fieldId.length;
            
                if(!multiple && itemsCount > 0) {
                    addButton.hide();
                }
                else {
                    addButton.show();
                }
            };

            refreshIds();
        
            addButton.click(function() {
                addButton.trigger("orchard-admin-contentpicker-open", {
                    callback: function(data) {
                        if (Array.isArray && Array.isArray(data)) {
                            data.forEach(function (d) {
                                var tmpl = template.replace(/\{contentItemId\}/g, d.id)
                                    .replace(/\{edit-link\}/g, d.editLink)
                                    .replace(/\{status-text}/g, d.published ? "" : " - " + notPublishedText);
                                var content = $(tmpl);
                                $(self).find('table.content-picker tbody').append(content);
                            });
                            refreshIds();
                            $(self).find('.content-picker-message').show();
                        } else {
                            var tmpl = template.replace(/\{contentItemId\}/g, data.id)
                                .replace(/\{edit-link\}/g, data.editLink)
                                .replace(/\{status-text}/g, data.published ? "" : " - " + notPublishedText);
                            var content = $(tmpl);
                            $(self).find('table.content-picker tbody').append(content);

                            refreshIds();
                            $(self).find('.content-picker-message').show();
                        }
                    },
                    baseUrl: baseUrl,
                    part: partName,
                    field: fieldName,
                    types: types
                });
            });

            $(self).on("click",'table.content-picker .content-picker-remove',function() {
                $(this).closest('tr').remove();
                refreshIds();
                $(self).find('.content-picker-message').show();
            });
            
            $(self).find("table.content-picker tbody").sortable({
                handle: 'td:first',
                stop: function(event, ui) {
                    refreshIds();
                    $(self).find('.content-picker-message').show();
                }
            }).disableSelection();
        });
    })();
});