var LayoutEditor;
(function(LayoutEditor) {

    var Clipboard = function () {
        var self = this;
        this.clipboardData = {};
        this.setData = function(contentType, data, realClipBoard) {
            self.clipboardData[contentType] = data;
        };
        this.getData = function (contentType, realClipBoard) {
            return self.clipboardData[contentType];
        };

        this.disable = function() {
            this.disabled = true;
        };
    }

    LayoutEditor.Clipboard = new Clipboard();

    angular
        .module("LayoutEditor")
        .factory("clipboard", [
            function() {
                return {
                    setData: LayoutEditor.Clipboard.setData,
                    getData: LayoutEditor.Clipboard.getData,
                    disable: LayoutEditor.Clipboard.disable
                };
            }
        ]);
})(LayoutEditor || (LayoutEditor = {}));