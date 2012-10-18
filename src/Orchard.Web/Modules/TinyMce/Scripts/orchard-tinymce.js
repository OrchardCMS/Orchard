tinyMCE.init({
    theme: "advanced",
    mode: "specific_textareas",
    editor_selector: "tinymce",
    plugins: "fullscreen,autoresize,searchreplace,mediapicker,inlinepopups",
    theme_advanced_toolbar_location: "top",
    theme_advanced_toolbar_align: "left",
    theme_advanced_buttons1: "search,replace,|,cut,copy,paste,|,undo,redo,|,mediapicker,|,link,unlink,charmap,emoticon,codeblock,|,bold,italic,|,numlist,bullist,formatselect,|,code,fullscreen",
    theme_advanced_buttons2: "",
    theme_advanced_buttons3: "",
    convert_urls: false,
    valid_elements: "*[*]",
    // shouldn't be needed due to the valid_elements setting, but TinyMCE would strip script.src without it.
    extended_valid_elements: "script[type|defer|src|language]"
});
