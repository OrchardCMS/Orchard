(function() {
    var resizableSelector = ".wmd-input,.wmd-preview",
        resizeInnerElements = function (el, size) {
            if (size > 120) {
                el.height(size - 20);
            }
        };
    resizeInnerElements($(resizableSelector), 400);

    $(".has-grip").TextAreaResizer(function (size, resizing) {
        resizing.find(resizableSelector).each(function() { resizeInnerElements($(this), size - 18); });
    }, {
        resizeWrapper: true,
        useParentWidth: window.isRTL
    });
})();
