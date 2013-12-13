(function ($) {
    var initializeEditor = function () {
        var textArea = $(".code-editor")[0];
        var editor = CodeMirror.fromTextArea(textArea, {
            lineNumbers: true,
            mode: "application/x-ejs",
            indentUnit: 4,
            indentWithTabs: true,
            enterMode: "keep",
            tabMode: "shift",
            theme: "default",
            autoCloseTags: true
        });
    };

    $(function () {
        initializeEditor();
    });
})(jQuery);