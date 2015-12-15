/* 
	jQuery TextAreaResizer plugin
	Created on 17th January 2008 by Ryan O'Dell 
	Version 1.0.4
	
	Converted from Drupal -> textarea.js
	Found source: http://plugins.jquery.com/misc/textarea.js
	$Id: textarea.js,v 1.11.2.1 2007/04/18 02:41:19 drumm Exp $

	1.0.1 Updates to missing global 'var', added extra global variables, fixed multiple instances, improved iFrame support
	1.0.2 Updates according to textarea.focus
	1.0.3 Further updates including removing the textarea.focus and moving private variables to top
	1.0.4 Re-instated the blur/focus events, according to information supplied by dec

    1.0.5 Nick Mayne - Fixing issue with RTL when using useParentWidth. Should be looking at MarginLeft, not ignoring it.
	
*/
(function ($) {
    /* private variable "oHover" used to determine if you're still hovering over the same element */
    var overlay, // very hacky used so it works with iframes
        defaults = {
            'useParentWidth': false,
            'resizeWrapper': false,
            'minHeight': 32,
            'offsetTop': 0
        };

    /* TextAreaResizer plugin */
    $.fn.TextAreaResizer = function (cb, opts) {
        return this.each(function () {
            if (cb && typeof cb === 'function') {
                opts = opts || {};
                opts.callback = cb;
            } else if (cb && typeof cb === 'object' && !opts) {
                opts = cb;
            }

            textAreaResizer(this, $.extend({}, defaults, opts || {}));
        });
    };

    function textAreaResizer(target, opts) {
        var grippie,
            iLastMousePos = 0,
            options = opts,
            resizable,
            wrapper = $(target).addClass('processed');

        grippie = $('<div class="grippie"></div>').bind('mousedown', startDrag);
        resizable = wrapper.children(':visible');

        wrapper.append(grippie);

        if (!options.resizeWrapper) {
            if (!options.resizeSelector) {
                resizable = resizable.first();
            } else {
                resizable = resizable.filter(options.resizeSelector);
            }
        }

        if (!options.useParentWidth) {
            grippie[0].style.marginRight = (grippie[0].offsetWidth - resizable[0].offsetWidth) + 'px';
        } else {
            grippie[0].style.marginLeft = (grippie[0].offsetWidth - resizable[0].offsetWidth) + 'px';
        }

        if (options.initCallback && options.callback) {
            options.callback(wrapper.height() - grippie.outerHeight(true));
        }

        function startDrag(e) {
            iLastMousePos = mousePosition(e).y;
            // hack so it works with iframes
            overlay = $("<div id='overlay' style='position: absolute; zindex: 99; background-color: white; opacity:0.01; filter: alpha(opacity = 1); left:0; top:0;'>&nbsp;</div>");

            resizable.css('opacity', 0.25);
            wrapper.append(overlay[0]);
            overlay.width(wrapper.width());
            overlay.height(wrapper.height());

            $(document).mousemove(performDrag).mouseup(endDrag);

            return false;
        }

        function performDrag(e) {
            var iThisMousePos = mousePosition(e).y,
                iMousePos = iThisMousePos - wrapper.offset().top,
                resizing;

            if (iLastMousePos >= iThisMousePos) {
                iMousePos -= 5;
            }

            if (iMousePos <= options.minHeight) {
                return false;
            }

            iLastMousePos = iThisMousePos;
            iMousePos = iMousePos;
            resizing = options.resizeWrapper ? wrapper : resizable;

            resizing.height(iMousePos - options.offsetTop);
            overlay.height(wrapper.height());

            if (options.callback) {
                options.callback(iMousePos - (options.resizeWrapper ? grippie.outerHeight(true) : 0), resizing);
            }

            return false;
        }

        function endDrag() {
            $(document).unbind('mousemove', performDrag).unbind('mouseup', endDrag);
            resizable.css('opacity', 1);
            wrapper.focus();
            overlay.remove();

            iLastMousePos = 0;
        }
    }

    function mousePosition(e) {
        return { x: e.clientX + document.documentElement.scrollLeft, y: e.clientY + (document.documentElement.scrollTop || document.body.scrollTop) };
    };
})(jQuery);
