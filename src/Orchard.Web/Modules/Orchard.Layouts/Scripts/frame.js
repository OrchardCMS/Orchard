(function ($) {
    var Frame = function (element) {
        var self = this;
        this.element = element;
        this.frame = element.get(0);
        this.element.data("frame", this);

        this.frame.triggerEvent = function(eventName, data) {
            self.element.trigger(eventName, data);
        };

        this.getBody = function () {
            return this.frame.contentWindow.document.body;
        };

        this.getDocument = function () {
            return $(this.frame.contentDocument || this.frame.contentWindow.document);
        };

        this.getWindow = function () {
            return this.frame.contentWindow;
        };

        this.find = function(selector) {
            return $(this.getBody()).find(selector);
        };

        this.load = function (url, data, method) {
            switch (method) {
                case "post":
                    post(url, data);
                    break;
                default:
                    this.getWindow().location.href = url + (data ? $.param(data) : "");
                    break;
            }

            this.autoHeight();
        };

        this.autoHeight = function () {
            var newHeight = this.getBody().scrollHeight;

            if (this.frame.height == newHeight)
                return;

            this.frame.height = "50px"; // forces the frame height to decrease if the new height is smaller than the previous height.
            this.frame.height = (newHeight) + "px";
        };

        var post = function (url, data) {
            var form = $("<form method=\"post\" action=\"" + url + "\"></form>");

            if (data) {
                for (var key in data) {
                    if (data.hasOwnProperty(key)) {
                        var input = $("<input type=\"hidden\" name=\"" + key + "\"/>");
                        input.val(data[key]);
                        form.append(input);
                    }
                }
            }

            var doc = self.getDocument();

            doc.find("body").html(form);
            form.submit();
            form.remove();
        };

        return this;
    };

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Frame = Frame;
})(jQuery);