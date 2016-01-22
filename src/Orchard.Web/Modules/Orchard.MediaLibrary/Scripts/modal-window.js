var w = document.defaultView || document.parentWindow;
var frames = w.parent.document.getElementsByTagName('iframe');
for (var i = frames.length; i-- > 0;) {
    var frame = frames[i];
    if (frame.className.indexOf('cboxIframe') > -1) {
        frame.contentWindow.document.documentElement.className += ' media-library-modal-window';
    }
}