(function () {
    function show() {
        window.setTimeout(function () {
            document.getElementById("throbber").style.display = "block";
        }, 2000);
    }

    if (document.addEventListener) {
        document.addEventListener("submit", show, false);
    }
    else {
        document.attachEvent("onsubmit", show);
    }
})();
