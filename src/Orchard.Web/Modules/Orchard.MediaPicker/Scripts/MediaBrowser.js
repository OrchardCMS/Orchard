(function ($) {
    var currentAspect = {},
        selectedItem,
        suppressResize;

    $.extend({
        mediaPicker: {
            init: function (data) {
                // called by the opener to initiate dialog with existing image data.
                // todo: data.img may contain existing image data, populate the fields
                var img = data.img;
                if (img) {
                    for (var name in img) {
                        $("#img-" + name).val(img[name]);
                    }
                    suppressResize = true;
                    $("#img-src").trigger("change");
                }
            },
            uploadMedia: uploadMedia,
            scalePreview: scalePreview
        }
    });

    $("#img-cancel, #lib-cancel").live("click", function () { window.close(); });
    // when url changes, set the preview and loader src
    $("#img-src").live("change", function () {
        selectImage(getIdPrefix(this), this.value);
    });
    $(".media-item").live("click", function () {
        if (selectedItem) {
            selectedItem.removeClass("selected");
        }
        selectedItem = $(this);
        selectedItem.addClass("selected");
        selectImage("#lib-", selectedItem.attr("data-imgsrc"));
    });
    // maintain aspect ratio when width or height is changed
    $("#img-width, #lib-width").live("change", fixAspectHeight);
    $("#img-height, #lib-height").live("change", fixAspectWidth);

    $("#img-insert, #lib-insert").live("click", function () {
        if ($(this).hasClass("disabled")) return;
        publishInsertEvent(this);
    });

    $(function () {
        $("#tabs").tabs({ selected: parseInt(location.hash.replace("#", "")) || 0 });

        // populate width and height when image loads
        // note: load event does not bubble so cannot be used with .live
        $("#img-loader, #lib-loader").bind("load", syncImage);

        // edit mode has slightly different wording
        // elements advertise this with data-edittext attributes,
        // the edit text is the element's new val() unless -content is specified.
        if (query("editmode") === "true") {
            $("[data-edittext]").each(function () {
                var self = $(this),
                    isContent = self.attr("data-edittext-content") === "true",
                    editText = self.attr("data-edittext");
                if (isContent) {
                    self.text(editText);
                }
                else {
                    self.attr("value", editText);
                }
            });
        }

        var data = window.mediaPickerData;
        if (data) {
            window.mediaPickerData = null;
            $.mediaPicker.init(data);
        }
    });

    function selectImage(prefix, src) {
        $(prefix + "preview").width("").height("").attr("src", src);
        $(prefix + "loader").attr("src", src);
        $(prefix + "src").val(src);

        var disabled = src ? "" : "disabled";
        $(prefix + "insert").attr("disabled", disabled).toggleClass("disabled", !!disabled);
    }

    function getIdPrefix(e) {
        return "#" + e.id.substr(0, 4);
    }
    function publishInsertEvent(button) {
        var prefix = getIdPrefix(button),
            editorId = query("editorId"),
            source = query("source"),
            img = {
                src: $(prefix + "src").val(),
                alt: $(prefix + "alt").val(),
                "class": $(prefix + "class").val(),
                style: $(prefix + "style").val(),
                align: $(prefix + "align").val(),
                width: $(prefix + "width").val(),
                height: $(prefix + "height").val()
            };
        img.html = getImageHtml(img);
        window.opener.OpenAjax.hub.publish("orchard.admin.pickimage-picked." + source, {
            editorId: editorId,
            img: img
        });
        window.close();
    }

    function parseUnits(value) {
        if (/\s*[0-9]+\s*(px)?\s*/i.test(value)) {
            return parseInt(value);
        }
        return NaN;
    }

    function fixAspectWidth() {
        var prefix = getIdPrefix(this);
        if (!$(prefix + "lock:checked").val()) return;
        var height = parseUnits(this.value);
        if (!isNaN(height)) {
            $(prefix + "width").val(Math.round(height * currentAspect[prefix]));
        }
    }

    function fixAspectHeight() {
        var prefix = getIdPrefix(this);
        if (!$(prefix + "lock:checked").val()) return;
        var width = parseUnits(this.value);
        if (!isNaN(width)) {
            $(prefix + "height").val(Math.round(width / currentAspect[prefix]));
        }
    }

    function scalePreview(img) {
        // ensures the loaded image preview fits within the preview area
        // by scaling it down if not.
        var self = $(img),
            width = self.width(),
            height = self.height(),
            aspect = width / height,
            maxWidth = self.parent().width(),
            maxHeight = self.parent().height();
        if (width > maxWidth) {
            width = maxWidth;
            height = Math.round(width / aspect);
        }
        if (height > maxHeight) {
            height = maxHeight;
            width = Math.round(width * aspect);
        }
        self.width(width).height(height);
    }

    function syncImage() {
        // when the image loader loads, we use it to calculate the current image
        // aspect ratio, and update the width and height fields.
        var prefix = getIdPrefix(this),
            self = $(this),
            width = self.width(),
            height = self.height();
        currentAspect[prefix] = width / height;
        // because we just loaded an edited image, leave the width/height 
        // at their configured values, not the natural size.
        if (!suppressResize) {
            $(prefix + "width").val(width);
            $(prefix + "height").val(height);
        }
        suppressResize = false;
    }

    function getAttr(name, value) {
        // get an attribute value, escaping any necessary characters to html entities.
        // not an exhastive list, but should cover all the necessary characters for this UI (e.g. you can't really put in newlines).
        if (!value && name !== "alt") return "";
        return ' ' + name + '="' + value.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;") + '"';
    }

    function getImageHtml(data) {
        return html = '<img src="' + encodeURI(data.src) + '"' + getAttr("alt", data.alt || "")
            + getAttr("class", data["class"])
            + getAttr("style", data.style)
            + getAttr("align", data.align)
            + getAttr("width", data.width)
            + getAttr("height", data.height)
            + "/>";
    }

    function uploadMedia(form) {
        var name = "addmedia__" + (new Date()).getTime();
        $("<iframe/>", {
            name: name,
            src: "about:blank",
            css: { display: "none" },
            load: iframeLoadHandler
        }).appendTo(document.body);
        form.target = name;
    }

    // get a querystring value
    function query(name) {
        name = name.toLowerCase();
        var search = location.search;
        var parts = search.replace("?", "").split("&");
        for (var i = 0, l = parts.length; i < l; i++) {
            var part = parts[i];
            var eqIndex = part.indexOf("=");
            if (eqIndex !== -1 && part.substr(0, eqIndex).toLowerCase() === name) {
                return part.substr(eqIndex + 1);
            }
        }
        return null;
    }

    function iframeLoadHandler() {
        try {
            var self = $(this),
                frame = window.frames[this.name];
            if (!frame.document || frame.document.URL == "about:blank") {
                return true;
            }
            var result = frame.result;
            if (result && result.url) {
                // successfully uploaded image, response by setting the url
                // to the new image. The change event will respond just as if
                // the user typed the url.
                $("#img-src").val(result.url).trigger("change");
            }
            else if (result && result.error) {
                alert(result.error);
            }
            else {
                var somethingPotentiallyHorrible = "";
                try {
                    somethingPotentiallyHorrible = $("body", frame.document).html();
                }
                catch (ex) { // some browsers flip out trying to access anything in the iframe when there's an error.
                }
                if (somethingPotentiallyHorrible) {
                    alert(somethingPotentiallyHorrible);
                }
            }

            //cleanup
            window.setTimeout(function () {
                self.remove();
            }, 123);

        }
        catch (ex) {
            alert(ex.message);
        }
    }

})(jQuery);


