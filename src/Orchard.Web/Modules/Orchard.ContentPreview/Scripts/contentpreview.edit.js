$(function () {
    $(document)
        .on('input change propertychange', 'form input', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('keyup', 'form input', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                $(document).trigger('contentpreview:render');
            }
        });

    $('.layout-data-field').change(function () {
        $(document).trigger('contentpreview:render');
    });

    if (typeof (tinyMCE) !== "undefined") {
        tinyMCE.activeEditor.on('change keyup', function (e) {
            this.targetElm.value = this.getContent();
            $(document).trigger('contentpreview:render');
        });
    }
});

var previewButton, contentItemType, previewId, previewContentItemId, previewContentItemVersionId, form, formData;

$(function () {
    function serializeLayoutHandler() {
        $.event.trigger({
            type: "serializelayout"
        });
    };

    $(document).on("layouteditor:edited", serializeLayoutHandler);

    previewButton = document.getElementById('previewButton');
    contentItemType = $(document.getElementById('contentItemType')).data('value');
    previewId = $(document.getElementById('previewId')).data('value');
    form = $(previewButton).closest('form');

    sendFormData = function () {
        formData = form.serializeArray(); // convert form to array

        formData.push({ name: "ContentItemType", value: contentItemType });
        formData.push({ name: "PreviewId", value: previewId });

        var foundIndex = formData.findIndex(data => data.name === "AutoroutePart.CurrentUrl");
        if (foundIndex > -1) {
            formData[foundIndex].value += "-preview";
        }

        // store the form data to pass it in the event handler
        localStorage.setItem('contentpreview:' + previewId, JSON.stringify($.param(formData)));
    }

    $(document).on('contentpreview:render', function () {
        sendFormData();
    });


    $(window).on('storage', function (ev) {
        if (ev.originalEvent.key != 'contentpreview:ready:' + previewId) return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no contentpreview:render is already sent
        sendFormData();
    });

    $(window).on('unload', function () {
        localStorage.removeItem('contentpreview:' + previewId);
        // this will raise an event in the preview window to notify that the live preview is no longer active.
        localStorage.setItem('contentpreview:not-connected:' + previewId, '');
        localStorage.removeItem('contentpreview:not-connected:' + previewId);
    });
});
