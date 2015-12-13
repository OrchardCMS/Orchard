(function ($) {
    var currentAspect = {},
        selectedItem,
        suppressResize;

    $.extend({
        mediaPicker: {
            uploadMedia: uploadMedia,
            scalePreview: scalePreview
        }
    });

    $(document).on("click", "#img-cancel, #lib-cancel", function () { window.close(); });
    // when url changes, set the preview and loader src
    $(document).on("change", "#img-src", function () {
        selectImage(getIdPrefix(this), this.value);
    });
    $(document).on("click", ".media-item", function () {
        if (selectedItem) {
            selectedItem.removeClass("selected");
        }
        selectedItem = $(this);
        selectedItem.addClass("selected");
        selectImage("#lib-", selectedItem.attr("data-imgsrc"));
    });
    // maintain aspect ratio when width or height is changed
    $(document).on("change", "#img-width, #lib-width", fixAspectHeight);
    $(document).on("change", "#img-height, #lib-height", fixAspectWidth);

    $(document).on("click", "#img-insert, #lib-insert", function () {
        if ($(this).hasClass("disabled")) return;
        publishInsertEvent(this);
    });

    $(document).on("click", ".media-filename", function (ev) {
        // when clicking on a filename in the gallery view,
        // we interrupt the normal operation and write a <img>
        // tag into a new window to ensure the image displays in
        // a new window instead of being 'downloaded' like in Chrome
        ev.preventDefault();
        var self = $(this),
            src = attributeEncode(self.attr("href")),
            w = window.open("", self.attr("target"));
        w.document.write("<!DOCTYPE html><html><head><title>" + src + "</title></head><body><img src=\"" + src + "\" alt=\"\" /></body></html>");
    });

    $(document).on("click", "#createFolder", function () {
        if ($(this).hasClass("disabled")) return;
        $.post("MediaPicker/CreateFolder", { path: query("mediaPath") || "", folderName: $("#folderName").val(), __RequestVerificationToken: $("#__requesttoken").val() },
            function (response) {
                if (response.Success) {
                    location.reload(true);
                } else if (response.Success === false) {
                    alert(response.Message);
                } else if (response.indexOf($.mediaPicker.logonUrl) !== -1) {
                    // A redirection due to expired authorization
                    alert($.mediaPicker.accessDeniedMsg);
                } else alert($.mediaPicker.cannotPerformMsg);
            });
    });

    $(document).on("propertychange keyup input paste", "#folderName", function () {
        var empty = ($("#folderName").val() == "");
        $("#createFolder").attr("disabled", empty).toggleClass("disabled", empty);
    });

    $(function () {
        $("#tabs").tabs({
            selected: parseInt(query("tab", location.hash)) || 0,
            select: function (event, ui) {
                // sync the active tab with the hash value
                location.hash = "tab=" + ui.index;
            }
        });

        // populate width and height when image loads
        // note: load event does not bubble so cannot be used with .live
        $("#img-loader, #lib-loader").bind("load", syncImage);

        $("#lib-uploadform").bind("uploadComplete", function (ev, url) {
            // from the libary view, uploading should cause a reload
            var href = location.href,
                hashindex = location.href.indexOf("#");
            if (hashindex !== -1) {
                href = href.substr(0, hashindex);
            }
            location.href = href + "&rl=" + (new Date() - 0) + "#tab=1&select=" + url.substr(url.lastIndexOf("/") + 1);
        });

        var preselect = query("select", location.hash);
        if (preselect) {
            $("img[data-filename='" + preselect + "']").closest(".media-item").trigger("click");
        }

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

        try {
            var data = window.opener.jQuery[query("callback")].data,
            img = data ? data.img : null;
        }
        catch (ex) {
            alert($.mediaPicker.cannotPerformMsg);
            window.close();
        }

        if (img) {
            for (var name in img) {
                $("#img-" + name).val(img[name]);
            }
            suppressResize = true;
            $("#img-src").trigger("change");
        }
    });

    function selectImage(prefix, src) {
        $(prefix + "preview")
            .css({
                display: "none",
                width: "",
                height: ""
            })
            .attr("src", src);
        $(prefix + "loader").attr("src", src);
        $(prefix + "src").val(src);
        $(prefix + "insert").attr("disabled", !src).toggleClass("disabled", !src);
    }

    function getIdPrefix(e) {
        return "#" + e.id.substr(0, 4);
    }
    function publishInsertEvent(button) {
        var prefix = getIdPrefix(button),
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
        try {
            window.opener.jQuery[query("callback")]({ img: img });
        }
        catch (ex) {
            alert($.mediaPicker.cannotPerformMsg);
        }
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

        self.css({
            width: width,
            height: height,
            display: "inline"
        });
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

    function attributeEncode(value) {
        return !value ? "" : value.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\//g, "&#47;");
    }

    function getAttr(name, value) {
        // get an attribute value, escaping any necessary characters to html entities.
        // not an exhastive list, but should cover all the necessary characters for this UI (e.g. you can't really put in newlines).
        if (!value && name !== "alt") return "";
        return ' ' + name + '="' + attributeEncode(value) + '"';
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
        var name = "addmedia__" + (new Date()).getTime(),
            prefix = getIdPrefix(form);
        $("<iframe name='" + name + "' src='about:blank' style='display:none'/>")
            .attr("id", prefix.substr(1) + "iframe")
            .bind("load", iframeLoadHandler)
            .appendTo(form);
        form.target = name;
        $(prefix + "indicator").show();
    }

    // get a querystring value
    function query(name, querystring) {
        name = name.toLowerCase();
        var search = querystring || location.search;
        var parts = search.replace("?", "").replace("#", "").split("&");
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
                form = self.closest("form"),
                frame = this.contentWindow || window.frames[this.name];
            if (!frame.document || frame.document.URL == "about:blank") {
                return true;
            }
            var result = frame.result;
            if (result && result.url) {
                selectImage(getIdPrefix(this), result.url);
                form.trigger("uploadComplete", [result.url]);
            }
            else if (result && result.error) {
                alert(result.error);
            }
            else if (frame.location.pathname.match("AccessDenied")) {
                alert($.mediaPicker.accessDeniedMsg);
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
            $(getIdPrefix(form.get(0)) + "indicator").hide();
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


