(function ($) {
    var Dialog = function (templateElementSelector) {
        var self = this;
        this.template = $(templateElementSelector);
        this.root = null;
        this.element = null;
        this.frame = null;
        this.view = null;
        this.isVisible = false;
        this._title = this.template.find(".title").html();

        this.title = function (value) {
            var titleElement = this.root.find(".title");
            this._title = value;
            titleElement.text(this._title);
        };

        this.show = function () {
            if (this.isVisible)
                return;

            var dialogTemplate = this.template.find(".dialog-wrapper");
            this.root = dialogTemplate.clone().appendTo("body");
            this.overlay = this.root.find(".overlay");
            this.element = this.root.find(".dialog");
            this.view = this.root.find(".dialog-view");
            this.frame = new window.Orchard.Frame(this.root.find("iframe"));
            var titleElement = this.root.find(".title");
            this.root.show();
            this.isVisible = true;

            titleElement.text(this._title);
            centerPosition();
            initButtons();
            makeDraggable();
            window.currentDialog = this;

            $(document).on("keyup", onKeyUp);

            this.frame.element.on("load", function(e) {
                updateDialog(self.frame.getDocument());
            });
        }

        this.close = function () {
            this.isVisible = false;

            if (this.root) {
                $(window).off("resize", resizeIFrame);
                this.root.remove();
            }
            
            this.root = null;
            this.element = null;
            this.frame = null;

            $(document).off("keyup", onKeyUp);
        };

        this.load = function (url, data, method) {
            method = method || "get";

            this.frame.load(url, data, method);
            this.frame.element.show();
            this.view.hide();

            resizeIFrame();
            centerPosition();
            this.frame.getDocument().on("keyup", onKeyUp);

            $(window).on("resize", function () {
                resizeIFrame();
                centerPosition();
            });
        };

        this.setHtml = function(html) {
            this.frame.element.hide();
            this.view.show();
            this.view.html(html);

            resizeView();
            centerPosition();
            updateDialog(this.view);

            $(window).on("resize", function () {
                resizeView();
                centerPosition();
            });
        }

        this.trigger = function (event, args) {
            if (this.element == null)
                throw Error("Cannot trigger events on an uninitialized dialog. You must invoke Show first.");

            this.element.trigger(event, args);
        };

        var resizeIFrame = function () {
            if (self.frame == null)
                return;

            self.frame.element.height($(window).height() * .75);
            self.frame.element.width($(window).width() * .75);
        };

        var resizeView = function () {
            if (self.view == null)
                return;

            self.view.height($(window).height() * .75);
            self.view.width($(window).width() * .75);
        };

        var onKeyUp = function(e) {
            var esc = 27;
            if (e.keyCode == esc) {
                self.close();
            }
        };

        var centerPosition = function () {
            if (self.element == null)
                return;

            self.element.position({
                my: "center",
                at: "center center",
                of: self.overlay
            });
        };

        var initButtons = function () {
            self.element.on("click", "a.close, a.cancel", function (e) {
                e.preventDefault();
                self.close();
            });

            self.element.on("click", "a.save", function (e) {
                e.preventDefault();
                var frameDoc = self.frame.getDocument();
                var form = frameDoc.find("form:first");
                form.submit();
            });
        };

        var makeDraggable = function () {
            self.element.draggable({
                handle: ".drag-handle",
                containment: "parent"
            });
        };

        var updateDialog = function(scope) {
            //var document = self.frame.getDocument();
            var dialogSettings = scope.find(".dialog-settings");
            var title = dialogSettings.find(".title");
            var buttons = dialogSettings.find(".buttons");

            if (title.length > 0) self.title(title.html());
            if (buttons.length > 0) {
                var footer = self.root.find(".dialog footer");
                footer.empty();
                footer.append(buttons);

                buttons.on("click", "a[data-command]", function (e) {
                    e.preventDefault();
                    var button = $(this);
                    var command = button.data("command");

                    if (!command || command.length == 0)
                        return;

                    self.trigger("command", {
                        command: command
                    });
                });
            }
        };
    };

    // Export types.
    window.Orchard = window.Orchard || {};
    window.Orchard.Layouts = window.Orchard.Layouts || {};
    window.Orchard.Layouts.Dialog = Dialog;
})(jQuery);