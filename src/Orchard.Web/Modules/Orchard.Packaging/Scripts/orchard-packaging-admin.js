function checkCustom() {
    setDisabled(document.getElementById("custom_features"), false);
}

function checkAll() {
    setDisabled(document.getElementById("custom_features"), true);
}

function setDisabled(el, state) {
    try {
        el.disabled = state;
    }
    catch (E) { }

    if (el.childNodes && el.childNodes.length > 0) {
        for (var x = 0; x < el.childNodes.length; x++) {
            setDisabled(el.childNodes[x]);
        }
    }
}

(function ($) {
    $(checkAll)
})(jQuery);