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

// If the editable area is taller than the window, make the menu and the toolbox sticky-positioned within the editor
// to help the user avoid excessive vertical scrolling.
// There is a built-in fixed_toolbar_container option in the TinyMCE, but we can't use it, because it is only
// available if the selector is a DIV with inline mode.

$(window).load(function () {

    var $container = $("<div/>", { id: "stickyContainer", class: "container-layout" });
    $(".mce-stack-layout:first").prepend($container);
    $container.append($(".mce-menubar"));
    $container.append($(".mce-toolbar-grp"));

    var containerPosition = $container.get(0).getBoundingClientRect();
    var $placeholder = $("<div/>");
    var isAdded = false;

    $(window).scroll(function (event) {
        var $statusbarPosition = $(".mce-statusbar").offset();
        if ($(window).scrollTop() >= containerPosition.top && !isAdded) {
            $container.addClass("sticky-top");
            $placeholder.insertBefore($container);
            $container.width($placeholder.width());
            $placeholder.height($container.height());
            $(window).on("resize", function () {
                $container.width($placeholder.width());
                $placeholder.height($container.height());
            });
            $container.addClass("sticky-container-layout");
            isAdded = true;
        } else if ($(window).scrollTop() < containerPosition.top && isAdded) {
            $container.removeClass("sticky-top");
            $placeholder.remove();
            $container.removeClass("sticky-container-layout");
            $(window).on("resize", function () {
                $container.width("100%");
            });
            isAdded = false;
        }
        if ($(window).scrollTop() >= ($statusbarPosition.top - $container.height())) {
            $container.hide();
        } else if ($(window).scrollTop() < ($statusbarPosition.top - $container.height()) && isAdded) {
            $container.show();
        }
    });
});
