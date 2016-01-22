jQuery(function($) {
    $.imageEditor.registerPlugin(function () {
        console.log('initiliazing crop');
        var host = $.imageEditor;
        var jcropApi;
        var coords;
        var changing = false;
        
        var cropWidth = $('#crop-width');
        var cropHeight = $('#crop-height');
        
        $('#crop-maintain').change(function () {
            var ratio = (coords.x2 - coords.x) / (coords.y2 - coords.y);
            jcropApi.setOptions($(this).is(':checked') ? { aspectRatio: ratio } : { aspectRatio: 0 });
            jcropApi.focus();
        });
        
        $('#crop').on("click", function () {
            console.log('crop clicked');

            $(host.image).Jcrop({
                onSelect: function (c) { coords = c; },
                onChange: function (c) {
                    // ignore modifications when typing a value
                    if (changing) {
                        console.log('chortcircuit');
                        return;
                    }
                    
                    coords = c;
                    var w = Math.floor(coords.w);
                    var h = Math.floor(coords.h);

                    if (cropWidth.val() != w) {
                        cropWidth.val(w);
                    }

                    if (cropHeight.val() != h) {
                        cropHeight.val(h);
                    }
                }
            });

            jcropApi = $(host.image).data('Jcrop');
            jcropApi.setSelect([host.image.width * 0.25, host.image.height * 0.25, host.image.width * 0.75, host.image.height * 0.75]);

            $('#crop-options').show();
            host.showOptions();
        });
        
        // apply cropping selection
        $('#crop-apply').on('click', function () {

            if (coords) {
                console.log(coords);
                jcropApi.destroy();

                var image = new Image();
                image.onload = function() {
                    var canvas = document.createElement('canvas');
                    $(host.image).css('width', canvas.width = coords.w);
                    $(host.image).css('height', canvas.height = coords.h);
                    var context = canvas.getContext('2d');
                    context.drawImage(image, coords.x, coords.y, coords.w, coords.h, 0, 0, coords.w, coords.h);
                    host.image.src = canvas.toDataURL("image/png");
                };

                image.src = host.image.src;
            }

            $('#crop-options').hide();
            host.hideOptions();
        });

        // cancel cropping
        $('#crop-cancel').on('click', function () {
            jcropApi.destroy();
            $('#crop-options').hide();
            host.hideOptions();
        });
        
        cropWidth.on("keyup paste", function () {
            changing = true;
            jcropApi.setOptions({ aspectRatio: 0 });
            $('#crop-maintain').prop('checked', false);
            jcropApi.setSelect([coords.x, coords.y, coords.x + +$(this).val(), coords.y2]);
            changing = false;
        });

        cropHeight.on("keyup paste", function () {
            changing = true;
            jcropApi.setOptions({ aspectRatio: 0 });
            $('#crop-maintain').prop('checked', false);
            jcropApi.setSelect([coords.x, coords.y, coords.x2, coords.y + +$(this).val()]);
            changing = false;
        });
    });
});
