var LayoutEditor;
(function ($, LayoutEditor) {

    var decode = function (value) {
        return !!value ? decodeURIComponent(value.replace(/\+/g, "%20")) : null;
    };

    LayoutEditor.decode = decode;

})(jQuery, LayoutEditor || (LayoutEditor = {}));