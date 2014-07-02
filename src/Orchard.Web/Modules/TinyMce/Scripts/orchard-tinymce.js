var mediaPlugins = ",|";

if (mediaPickerEnabled) {
    mediaPlugins += ",mediapicker";
}

if (mediaLibraryEnabled) {
    mediaPlugins += ",medialibrary";
}

tinyMCE.init({
    selector: "textarea.tinymce",
    theme: "modern",
    schema: "html5",
    plugins: "fullscreen,autoresize,searchreplace,link,charmap,code" + mediaPlugins.substr(2),
    toolbar: "searchreplace,|,cut,copy,paste,|,undo,redo" + mediaPlugins + ",|,link,unlink,charmap,|,bold,italic,|,numlist,bullist,formatselect,|,code,fullscreen,",
    convert_urls: false,
    valid_elements: "*[*]",
    // shouldn't be needed due to the valid_elements setting, but TinyMCE would strip script.src without it.
    extended_valid_elements: "script[type|defer|src|language]",
    menubar: false,
    statusbar: false,
    skin: 'orchardlightgray'
});