var mediaPlugins = "";

if (mediaPickerEnabled) {
    mediaPlugins += " mediapicker";
}

if (mediaLibraryEnabled) {
    mediaPlugins += " medialibrary";
}

tinyMCE.init({
    selector: "textarea.tinymce",
    theme: "modern",
    schema: "html5",
    plugins: [
        "advlist autolink lists link image charmap print preview hr anchor pagebreak",
        "searchreplace wordcount visualblocks visualchars code fullscreen",
        "insertdatetime media nonbreaking table contextmenu directionality",
        "emoticons template paste textcolor colorpicker textpattern",
        "fullscreen autoresize" + mediaPlugins
    ],
    toolbar: "undo redo cut copy paste | bold italic | bullist numlist outdent indent formatselect | alignleft aligncenter alignright alignjustify ltr rtl | " + mediaPlugins + " link unlink charmap | code fullscreen",
    convert_urls: false,
    valid_elements: "*[*]",
    // Shouldn't be needed due to the valid_elements setting, but TinyMCE would strip script.src without it.
    extended_valid_elements: "script[type|defer|src|language]",
    //menubar: false,
    //statusbar: false,
    skin: "orchardlightgray",
    language: language,
    auto_focus: autofocus,
    directionality: directionality,
    setup: function (editor) {
        $(document).bind("localization.ui.directionalitychanged", function(event, directionality) {
            editor.getBody().dir = directionality;
        });
    }
});