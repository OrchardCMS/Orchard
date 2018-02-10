jQuery(function ($) {
    
    var imeToolbar = document.getElementById('image-editor-toolbar');
    var imeOptions = document.getElementById('image-editor-options');
    var imeOptionsContainer = document.getElementById('image-editor-options-container');
    var imeImage = document.getElementById("image-editor-image");
    var imeImageWrapper = document.getElementById("image-editor-image-wrapper");
    var imeManageButtons = document.getElementById("buttons-manage");
    
    var awaiting = [];
    
    $(imeImage).load(function () {
        $(imeImage).off('load');
        for (var i = 0; i < awaiting.length; i++) {
            awaiting[i]();
        }
    });
    
    $.imageEditor = {
            toolbar: imeToolbar,
            options: imeOptions,
            onApply: function() {
            },
            onCancel: function() {
            },
            image: imeImage,
            imageWrapper: imeImageWrapper,
            showOptions: function() {
                $(imeOptionsContainer).show();
                $(imeToolbar).hide();
                $(imeManageButtons).hide();
            },
            hideOptions: function () {
                $(imeOptionsContainer).hide();
                $(imeToolbar).show();
                $(imeManageButtons).show();
            },
            registerPlugin: function (callBack) {
                // wait for the image to be loaded before calling registrations
                // as plugins might need to extract the image metadata

                if (!imeImage.complete) {
                    awaiting.push(callBack);
                } else {
                    callBack();
                }
            }    
    };
    
    $('#button-save').click(function () {
        var image = new Image();
        image.onload = function () {
            var canvas = document.createElement('canvas');
            canvas.width = imeImage.width;
            canvas.height = imeImage.height;
            var context = canvas.getContext('2d');
            context.drawImage(image, 0, 0, imeImage.width, imeImage.height, 0, 0, imeImage.width, imeImage.height);

            var quality = 0.92;
            var imageSrc = canvas.toDataURL("image/jpeg", quality);
            
            $.post($('#upload-image-url').val(), {
                content: imageSrc,
                width: canvas.width,
                height: canvas.height,
                __RequestVerificationToken: $("input[name=__RequestVerificationToken]").val()
            })
            .done(function () {
                window.location = $('#button-cancel').attr('href');
            });
        };

        image.src = imeImage.src;
    });

    $('#button-cancel').click(function () {
    });
    
});
