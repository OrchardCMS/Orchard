/*!
 * jQuery blockUI plugin
 * Version 2.66.0-2013.10.09
 * Requires jQuery v1.7 or later
 *
 * Examples at: http://malsup.com/jquery/block/
 * Copyright (c) 2007-2013 M. Alsup
 * Dual licensed under the MIT and GPL licenses:
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.gnu.org/licenses/gpl.html
 *
 * Thanks to Amir-Hossein Sobhi for some excellent contributions!
 */

;(function() {
/*jshint eqeqeq:false curly:false latedef:false */
"use strict";

	function setup($) {
		$.fn._fadeIn = $.fn.fadeIn;

		var noOp = $.noop || function() {};

		// this bit is to ensure we don't call setExpression when we shouldn't (with extra muscle to handle
		// confusing userAgent strings on Vista)
		var msie = /MSIE/.test(navigator.userAgent);
		var ie6  = /MSIE 6.0/.test(navigator.userAgent) && ! /MSIE 8.0/.test(navigator.userAgent);
		var mode = document.documentMode || 0;
		var setExpr = $.isFunction( document.createElement('div').style.setExpression );

		// global $ methods for blocking/unblocking the entire page
		$.blockUI   = function(opts) { install(window, opts); };
		$.unblockUI = function(opts) { remove(window, opts); };

		// convenience method for quick growl-like notifications  (http://www.google.com/search?q=growl)
		$.growlUI = function(title, message, timeout, onClose) {
			var $m = $('<div class="growlUI"></div>');
			if (title) $m.append('<h1>'+title+'</h1>');
			if (message) $m.append('<h2>'+message+'</h2>');
			if (timeout === undefined) timeout = 3000;

			// Added by konapun: Set timeout to 30 seconds if this growl is moused over, like normal toast notifications
			var callBlock = function(opts) {
				opts = opts || {};

				$.blockUI({
					message: $m,
					fadeIn : typeof opts.fadeIn  !== 'undefined' ? opts.fadeIn  : 700,
					fadeOut: typeof opts.fadeOut !== 'undefined' ? opts.fadeOut : 1000,
					timeout: typeof opts.timeout !== 'undefined' ? opts.timeout : timeout,
					centerY: false,
					showOverlay: false,
					onUnblock: onClose,
					css: $.blockUI.defaults.growlCSS
				});
			};

			callBlock();
			var nonmousedOpacity = $m.css('opacity');
			$m.mouseover(function() {
				callBlock({
					fadeIn: 0,
					timeout: 30000
				});

				var displayBlock = $('.blockMsg');
				displayBlock.stop(); // cancel fadeout if it has started
				displayBlock.fadeTo(300, 1); // make it easier to read the message by removing transparency
			}).mouseout(function() {
				$('.blockMsg').fadeOut(1000);
			});
			// End konapun additions
		};

		// plugin method for blocking element content
		$.fn.block = function(opts) {
			if ( this[0] === window ) {
				$.blockUI( opts );
				return this;
			}
			var fullOpts = $.extend({}, $.blockUI.defaults, opts || {});
			this.each(function() {
				var $el = $(this);
				if (fullOpts.ignoreIfBlocked && $el.data('blockUI.isBlocked'))
					return;
				$el.unblock({ fadeOut: 0 });
			});

			return this.each(function() {
				if ($.css(this,'position') == 'static') {
					this.style.position = 'relative';
					$(this).data('blockUI.static', true);
				}
				this.style.zoom = 1; // force 'hasLayout' in ie
				install(this, opts);
			});
		};

		// plugin method for unblocking element content
		$.fn.unblock = function(opts) {
			if ( this[0] === window ) {
				$.unblockUI( opts );
				return this;
			}
			return this.each(function() {
				remove(this, opts);
			});
		};

		$.blockUI.version = 2.66; // 2nd generation blocking at no extra cost!

		// override these in your code to change the default behavior and style
		$.blockUI.defaults = {
			// message displayed when blocking (use null for no message)
			message:  '<h1>Please wait...</h1>',

			title: null,		// title string; only used when theme == true
			draggable: true,	// only used when theme == true (requires jquery-ui.js to be loaded)

			theme: false, // set to true to use with jQuery UI themes

			// styles for the message when blocking; if you wish to disable
			// these and use an external stylesheet then do this in your code:
			// $.blockUI.defaults.css = {};
			css: {
				padding:	0,
				margin:		0,
				width:		'30%',
				top:		'40%',
				left:		'35%',
				textAlign:	'center',
				color:		'#000',
				border:		'3px solid #aaa',
				backgroundColor:'#fff',
				cursor:		'wait'
			},

			// minimal style set used when themes are used
			themedCSS: {
				width:	'30%',
				top:	'40%',
				left:	'35%'
			},

			// styles for the overlay
			overlayCSS:  {
				backgroundColor:	'#000',
				opacity:			0.6,
				cursor:				'wait'
			},

			// style to replace wait cursor before unblocking to correct issue
			// of lingering wait cursor
			cursorReset: 'default',

			// styles applied when using $.growlUI
			growlCSS: {
				width:		'350px',
				top:		'10px',
				left:		'',
				right:		'10px',
				border:		'none',
				padding:	'5px',
				opacity:	0.6,
				cursor:		'default',
				color:		'#fff',
				backgroundColor: '#000',
				'-webkit-border-radius':'10px',
				'-moz-border-radius':	'10px',
				'border-radius':		'10px'
			},

			// IE issues: 'about:blank' fails on HTTPS and javascript:false is s-l-o-w
			// (hat tip to Jorge H. N. de Vasconcelos)
			/*jshint scripturl:true */
			iframeSrc: /^https/i.test(window.location.href || '') ? 'javascript:false' : 'about:blank',

			// force usage of iframe in non-IE browsers (handy for blocking applets)
			forceIframe: false,

			// z-index for the blocking overlay
			baseZ: 1000,

			// set these to true to have the message automatically centered
			centerX: true, // <-- only effects element blocking (page block controlled via css above)
			centerY: true,

			// allow body element to be stetched in ie6; this makes blocking look better
			// on "short" pages.  disable if you wish to prevent changes to the body height
			allowBodyStretch: true,

			// enable if you want key and mouse events to be disabled for content that is blocked
			bindEvents: true,

			// be default blockUI will supress tab navigation from leaving blocking content
			// (if bindEvents is true)
			constrainTabKey: true,

			// fadeIn time in millis; set to 0 to disable fadeIn on block
			fadeIn:  200,

			// fadeOut time in millis; set to 0 to disable fadeOut on unblock
			fadeOut:  400,

			// time in millis to wait before auto-unblocking; set to 0 to disable auto-unblock
			timeout: 0,

			// disable if you don't want to show the overlay
			showOverlay: true,

			// if true, focus will be placed in the first available input field when
			// page blocking
			focusInput: true,

            // elements that can receive focus
            focusableElements: ':input:enabled:visible',

			// suppresses the use of overlay styles on FF/Linux (due to performance issues with opacity)
			// no longer needed in 2012
			// applyPlatformOpacityRules: true,

			// callback method invoked when fadeIn has completed and blocking message is visible
			onBlock: null,

			// callback method invoked when unblocking has completed; the callback is
			// passed the element that has been unblocked (which is the window object for page
			// blocks) and the options that were passed to the unblock call:
			//	onUnblock(element, options)
			onUnblock: null,

			// callback method invoked when the overlay area is clicked.
			// setting this will turn the cursor to a pointer, otherwise cursor defined in overlayCss will be used.
			onOverlayClick: null,

			// don't ask; if you really must know: http://groups.google.com/requiredUploads/jquery-en/browse_thread/thread/36640a8730503595/2f6a79a77a78e493#2f6a79a77a78e493
			quirksmodeOffsetHack: 4,

			// class name of the message block
			blockMsgClass: 'blockMsg',

			// if it is already blocked, then ignore it (don't unblock and reblock)
			ignoreIfBlocked: false
		};

		// private data and functions follow...

		var pageBlock = null;
		var pageBlockEls = [];

		function install(el, opts) {
			var css, themedCSS;
			var full = (el == window);
			var msg = (opts && opts.message !== undefined ? opts.message : undefined);
			opts = $.extend({}, $.blockUI.defaults, opts || {});

			if (opts.ignoreIfBlocked && $(el).data('blockUI.isBlocked'))
				return;

			opts.overlayCSS = $.extend({}, $.blockUI.defaults.overlayCSS, opts.overlayCSS || {});
			css = $.extend({}, $.blockUI.defaults.css, opts.css || {});
			if (opts.onOverlayClick)
				opts.overlayCSS.cursor = 'pointer';

			themedCSS = $.extend({}, $.blockUI.defaults.themedCSS, opts.themedCSS || {});
			msg = msg === undefined ? opts.message : msg;

			// remove the current block (if there is one)
			if (full && pageBlock)
				remove(window, {fadeOut:0});

			// if an existing element is being used as the blocking content then we capture
			// its current place in the DOM (and current display style) so we can restore
			// it when we unblock
			if (msg && typeof msg != 'string' && (msg.parentNode || msg.jquery)) {
				var node = msg.jquery ? msg[0] : msg;
				var data = {};
				$(el).data('blockUI.history', data);
				data.el = node;
				data.parent = node.parentNode;
				data.display = node.style.display;
				data.position = node.style.position;
				if (data.parent)
					data.parent.removeChild(node);
			}

			$(el).data('blockUI.onUnblock', opts.onUnblock);
			var z = opts.baseZ;

			// blockUI uses 3 layers for blocking, for simplicity they are all used on every platform;
			// layer1 is the iframe layer which is used to supress bleed through of underlying content
			// layer2 is the overlay layer which has opacity and a wait cursor (by default)
			// layer3 is the message content that is displayed while blocking
			var lyr1, lyr2, lyr3, s;
			if (msie || opts.forceIframe)
				lyr1 = $('<iframe class="blockUI" style="z-index:'+ (z++) +';display:none;border:none;margin:0;padding:0;position:absolute;width:100%;height:100%;top:0;left:0" src="'+opts.iframeSrc+'"></iframe>');
			else
				lyr1 = $('<div class="blockUI" style="display:none"></div>');

			if (opts.theme)
				lyr2 = $('<div class="blockUI blockOverlay ui-widget-overlay" style="z-index:'+ (z++) +';display:none"></div>');
			else
				lyr2 = $('<div class="blockUI blockOverlay" style="z-index:'+ (z++) +';display:none;border:none;margin:0;padding:0;width:100%;height:100%;top:0;left:0"></div>');

			if (opts.theme && full) {
				s = '<div class="blockUI ' + opts.blockMsgClass + ' blockPage ui-dialog ui-widget ui-corner-all" style="z-index:'+(z+10)+';display:none;position:fixed">';
				if ( opts.title ) {
					s += '<div class="ui-widget-header ui-dialog-titlebar ui-corner-all blockTitle">'+(opts.title || '&nbsp;')+'</div>';
				}
				s += '<div class="ui-widget-content ui-dialog-content"></div>';
				s += '</div>';
			}
			else if (opts.theme) {
				s = '<div class="blockUI ' + opts.blockMsgClass + ' blockElement ui-dialog ui-widget ui-corner-all" style="z-index:'+(z+10)+';display:none;position:absolute">';
				if ( opts.title ) {
					s += '<div class="ui-widget-header ui-dialog-titlebar ui-corner-all blockTitle">'+(opts.title || '&nbsp;')+'</div>';
				}
				s += '<div class="ui-widget-content ui-dialog-content"></div>';
				s += '</div>';
			}
			else if (full) {
				s = '<div class="blockUI ' + opts.blockMsgClass + ' blockPage" style="z-index:'+(z+10)+';display:none;position:fixed"></div>';
			}
			else {
				s = '<div class="blockUI ' + opts.blockMsgClass + ' blockElement" style="z-index:'+(z+10)+';display:none;position:absolute"></div>';
			}
			lyr3 = $(s);

			// if we have a message, style it
			if (msg) {
				if (opts.theme) {
					lyr3.css(themedCSS);
					lyr3.addClass('ui-widget-content');
				}
				else
					lyr3.css(css);
			}

			// style the overlay
			if (!opts.theme /*&& (!opts.applyPlatformOpacityRules)*/)
				lyr2.css(opts.overlayCSS);
			lyr2.css('position', full ? 'fixed' : 'absolute');

			// make iframe layer transparent in IE
			if (msie || opts.forceIframe)
				lyr1.css('opacity',0.0);

			//$([lyr1[0],lyr2[0],lyr3[0]]).appendTo(full ? 'body' : el);
			var layers = [lyr1,lyr2,lyr3], $par = full ? $('body') : $(el);
			$.each(layers, function() {
				this.appendTo($par);
			});

			if (opts.theme && opts.draggable && $.fn.draggable) {
				lyr3.draggable({
					handle: '.ui-dialog-titlebar',
					cancel: 'li'
				});
			}

			// ie7 must use absolute positioning in quirks mode and to account for activex issues (when scrolling)
			var expr = setExpr && (!$.support.boxModel || $('object,embed', full ? null : el).length > 0);
			if (ie6 || expr) {
				// give body 100% height
				if (full && opts.allowBodyStretch && $.support.boxModel)
					$('html,body').css('height','100%');

				// fix ie6 issue when blocked element has a border width
				if ((ie6 || !$.support.boxModel) && !full) {
					var t = sz(el,'borderTopWidth'), l = sz(el,'borderLeftWidth');
					var fixT = t ? '(0 - '+t+')' : 0;
					var fixL = l ? '(0 - '+l+')' : 0;
				}

				// simulate fixed position
				$.each(layers, function(i,o) {
					var s = o[0].style;
					s.position = 'absolute';
					if (i < 2) {
						if (full)
							s.setExpression('height','Math.max(document.body.scrollHeight, document.body.offsetHeight) - (jQuery.support.boxModel?0:'+opts.quirksmodeOffsetHack+') + "px"');
						else
							s.setExpression('height','this.parentNode.offsetHeight + "px"');
						if (full)
							s.setExpression('width','jQuery.support.boxModel && document.documentElement.clientWidth || document.body.clientWidth + "px"');
						else
							s.setExpression('width','this.parentNode.offsetWidth + "px"');
						if (fixL) s.setExpression('left', fixL);
						if (fixT) s.setExpression('top', fixT);
					}
					else if (opts.centerY) {
						if (full) s.setExpression('top','(document.documentElement.clientHeight || document.body.clientHeight) / 2 - (this.offsetHeight / 2) + (blah = document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop) + "px"');
						s.marginTop = 0;
					}
					else if (!opts.centerY && full) {
						var top = (opts.css && opts.css.top) ? parseInt(opts.css.top, 10) : 0;
						var expression = '((document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop) + '+top+') + "px"';
						s.setExpression('top',expression);
					}
				});
			}

			// show the message
			if (msg) {
				if (opts.theme)
					lyr3.find('.ui-widget-content').append(msg);
				else
					lyr3.append(msg);
				if (msg.jquery || msg.nodeType)
					$(msg).show();
			}

			if ((msie || opts.forceIframe) && opts.showOverlay)
				lyr1.show(); // opacity is zero
			if (opts.fadeIn) {
				var cb = opts.onBlock ? opts.onBlock : noOp;
				var cb1 = (opts.showOverlay && !msg) ? cb : noOp;
				var cb2 = msg ? cb : noOp;
				if (opts.showOverlay)
					lyr2._fadeIn(opts.fadeIn, cb1);
				if (msg)
					lyr3._fadeIn(opts.fadeIn, cb2);
			}
			else {
				if (opts.showOverlay)
					lyr2.show();
				if (msg)
					lyr3.show();
				if (opts.onBlock)
					opts.onBlock();
			}

			// bind key and mouse events
			bind(1, el, opts);

			if (full) {
				pageBlock = lyr3[0];
				pageBlockEls = $(opts.focusableElements,pageBlock);
				if (opts.focusInput)
					setTimeout(focus, 20);
			}
			else
				center(lyr3[0], opts.centerX, opts.centerY);

			if (opts.timeout) {
				// auto-unblock
				var to = setTimeout(function() {
					if (full)
						$.unblockUI(opts);
					else
						$(el).unblock(opts);
				}, opts.timeout);
				$(el).data('blockUI.timeout', to);
			}
		}

		// remove the block
		function remove(el, opts) {
			var count;
			var full = (el == window);
			var $el = $(el);
			var data = $el.data('blockUI.history');
			var to = $el.data('blockUI.timeout');
			if (to) {
				clearTimeout(to);
				$el.removeData('blockUI.timeout');
			}
			opts = $.extend({}, $.blockUI.defaults, opts || {});
			bind(0, el, opts); // unbind events

			if (opts.onUnblock === null) {
				opts.onUnblock = $el.data('blockUI.onUnblock');
				$el.removeData('blockUI.onUnblock');
			}

			var els;
			if (full) // crazy selector to handle odd field errors in ie6/7
				els = $('body').children().filter('.blockUI').add('body > .blockUI');
			else
				els = $el.find('>.blockUI');

			// fix cursor issue
			if ( opts.cursorReset ) {
				if ( els.length > 1 )
					els[1].style.cursor = opts.cursorReset;
				if ( els.length > 2 )
					els[2].style.cursor = opts.cursorReset;
			}

			if (full)
				pageBlock = pageBlockEls = null;

			if (opts.fadeOut) {
				count = els.length;
				els.stop().fadeOut(opts.fadeOut, function() {
					if ( --count === 0)
						reset(els,data,opts,el);
				});
			}
			else
				reset(els, data, opts, el);
		}

		// move blocking element back into the DOM where it started
		function reset(els,data,opts,el) {
			var $el = $(el);
			if ( $el.data('blockUI.isBlocked') )
				return;

			els.each(function(i,o) {
				// remove via DOM calls so we don't lose event handlers
				if (this.parentNode)
					this.parentNode.removeChild(this);
			});

			if (data && data.el) {
				data.el.style.display = data.display;
				data.el.style.position = data.position;
				if (data.parent)
					data.parent.appendChild(data.el);
				$el.removeData('blockUI.history');
			}

			if ($el.data('blockUI.static')) {
				$el.css('position', 'static'); // #22
			}

			if (typeof opts.onUnblock == 'function')
				opts.onUnblock(el,opts);

			// fix issue in Safari 6 where block artifacts remain until reflow
			var body = $(document.body), w = body.width(), cssW = body[0].style.width;
			body.width(w-1).width(w);
			body[0].style.width = cssW;
		}

		// bind/unbind the handler
		function bind(b, el, opts) {
			var full = el == window, $el = $(el);

			// don't bother unbinding if there is nothing to unbind
			if (!b && (full && !pageBlock || !full && !$el.data('blockUI.isBlocked')))
				return;

			$el.data('blockUI.isBlocked', b);

			// don't bind events when overlay is not in use or if bindEvents is false
			if (!full || !opts.bindEvents || (b && !opts.showOverlay))
				return;

			// bind anchors and inputs for mouse and key events
			var events = 'mousedown mouseup keydown keypress keyup touchstart touchend touchmove';
			if (b)
				$(document).bind(events, opts, handler);
			else
				$(document).unbind(events, handler);

		// former impl...
		//		var $e = $('a,:input');
		//		b ? $e.bind(events, opts, handler) : $e.unbind(events, handler);
		}

		// event handler to suppress keyboard/mouse events when blocking
		function handler(e) {
			// allow tab navigation (conditionally)
			if (e.type === 'keydown' && e.keyCode && e.keyCode == 9) {
				if (pageBlock && e.data.constrainTabKey) {
					var els = pageBlockEls;
					var fwd = !e.shiftKey && e.target === els[els.length-1];
					var back = e.shiftKey && e.target === els[0];
					if (fwd || back) {
						setTimeout(function(){focus(back);},10);
						return false;
					}
				}
			}
			var opts = e.data;
			var target = $(e.target);
			if (target.hasClass('blockOverlay') && opts.onOverlayClick)
				opts.onOverlayClick(e);

			// allow events within the message content
			if (target.parents('div.' + opts.blockMsgClass).length > 0)
				return true;

			// allow events for content that is not being blocked
			return target.parents().children().filter('div.blockUI').length === 0;
		}

		function focus(back) {
			if (!pageBlockEls)
				return;
			var e = pageBlockEls[back===true ? pageBlockEls.length-1 : 0];
			if (e)
				e.focus();
		}

		function center(el, x, y) {
			var p = el.parentNode, s = el.style;
			var l = ((p.offsetWidth - el.offsetWidth)/2) - sz(p,'borderLeftWidth');
			var t = ((p.offsetHeight - el.offsetHeight)/2) - sz(p,'borderTopWidth');
			if (x) s.left = l > 0 ? (l+'px') : '0';
			if (y) s.top  = t > 0 ? (t+'px') : '0';
		}

		function sz(el, p) {
			return parseInt($.css(el,p),10)||0;
		}

	}


	/*global define:true */
	if (typeof define === 'function' && define.amd && define.amd.jQuery) {
		define(['jquery'], setup);
	} else {
		setup(jQuery);
	}

})();

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuYmxvY2tVSS5qcyJdLCJzb3VyY2VzQ29udGVudCI6WyIvKiFcbiAqIGpRdWVyeSBibG9ja1VJIHBsdWdpblxuICogVmVyc2lvbiAyLjY2LjAtMjAxMy4xMC4wOVxuICogUmVxdWlyZXMgalF1ZXJ5IHYxLjcgb3IgbGF0ZXJcbiAqXG4gKiBFeGFtcGxlcyBhdDogaHR0cDovL21hbHN1cC5jb20vanF1ZXJ5L2Jsb2NrL1xuICogQ29weXJpZ2h0IChjKSAyMDA3LTIwMTMgTS4gQWxzdXBcbiAqIER1YWwgbGljZW5zZWQgdW5kZXIgdGhlIE1JVCBhbmQgR1BMIGxpY2Vuc2VzOlxuICogaHR0cDovL3d3dy5vcGVuc291cmNlLm9yZy9saWNlbnNlcy9taXQtbGljZW5zZS5waHBcbiAqIGh0dHA6Ly93d3cuZ251Lm9yZy9saWNlbnNlcy9ncGwuaHRtbFxuICpcbiAqIFRoYW5rcyB0byBBbWlyLUhvc3NlaW4gU29iaGkgZm9yIHNvbWUgZXhjZWxsZW50IGNvbnRyaWJ1dGlvbnMhXG4gKi9cblxuOyhmdW5jdGlvbigpIHtcbi8qanNoaW50IGVxZXFlcTpmYWxzZSBjdXJseTpmYWxzZSBsYXRlZGVmOmZhbHNlICovXG5cInVzZSBzdHJpY3RcIjtcblxuXHRmdW5jdGlvbiBzZXR1cCgkKSB7XG5cdFx0JC5mbi5fZmFkZUluID0gJC5mbi5mYWRlSW47XG5cblx0XHR2YXIgbm9PcCA9ICQubm9vcCB8fCBmdW5jdGlvbigpIHt9O1xuXG5cdFx0Ly8gdGhpcyBiaXQgaXMgdG8gZW5zdXJlIHdlIGRvbid0IGNhbGwgc2V0RXhwcmVzc2lvbiB3aGVuIHdlIHNob3VsZG4ndCAod2l0aCBleHRyYSBtdXNjbGUgdG8gaGFuZGxlXG5cdFx0Ly8gY29uZnVzaW5nIHVzZXJBZ2VudCBzdHJpbmdzIG9uIFZpc3RhKVxuXHRcdHZhciBtc2llID0gL01TSUUvLnRlc3QobmF2aWdhdG9yLnVzZXJBZ2VudCk7XG5cdFx0dmFyIGllNiAgPSAvTVNJRSA2LjAvLnRlc3QobmF2aWdhdG9yLnVzZXJBZ2VudCkgJiYgISAvTVNJRSA4LjAvLnRlc3QobmF2aWdhdG9yLnVzZXJBZ2VudCk7XG5cdFx0dmFyIG1vZGUgPSBkb2N1bWVudC5kb2N1bWVudE1vZGUgfHwgMDtcblx0XHR2YXIgc2V0RXhwciA9ICQuaXNGdW5jdGlvbiggZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnZGl2Jykuc3R5bGUuc2V0RXhwcmVzc2lvbiApO1xuXG5cdFx0Ly8gZ2xvYmFsICQgbWV0aG9kcyBmb3IgYmxvY2tpbmcvdW5ibG9ja2luZyB0aGUgZW50aXJlIHBhZ2Vcblx0XHQkLmJsb2NrVUkgICA9IGZ1bmN0aW9uKG9wdHMpIHsgaW5zdGFsbCh3aW5kb3csIG9wdHMpOyB9O1xuXHRcdCQudW5ibG9ja1VJID0gZnVuY3Rpb24ob3B0cykgeyByZW1vdmUod2luZG93LCBvcHRzKTsgfTtcblxuXHRcdC8vIGNvbnZlbmllbmNlIG1ldGhvZCBmb3IgcXVpY2sgZ3Jvd2wtbGlrZSBub3RpZmljYXRpb25zICAoaHR0cDovL3d3dy5nb29nbGUuY29tL3NlYXJjaD9xPWdyb3dsKVxuXHRcdCQuZ3Jvd2xVSSA9IGZ1bmN0aW9uKHRpdGxlLCBtZXNzYWdlLCB0aW1lb3V0LCBvbkNsb3NlKSB7XG5cdFx0XHR2YXIgJG0gPSAkKCc8ZGl2IGNsYXNzPVwiZ3Jvd2xVSVwiPjwvZGl2PicpO1xuXHRcdFx0aWYgKHRpdGxlKSAkbS5hcHBlbmQoJzxoMT4nK3RpdGxlKyc8L2gxPicpO1xuXHRcdFx0aWYgKG1lc3NhZ2UpICRtLmFwcGVuZCgnPGgyPicrbWVzc2FnZSsnPC9oMj4nKTtcblx0XHRcdGlmICh0aW1lb3V0ID09PSB1bmRlZmluZWQpIHRpbWVvdXQgPSAzMDAwO1xuXG5cdFx0XHQvLyBBZGRlZCBieSBrb25hcHVuOiBTZXQgdGltZW91dCB0byAzMCBzZWNvbmRzIGlmIHRoaXMgZ3Jvd2wgaXMgbW91c2VkIG92ZXIsIGxpa2Ugbm9ybWFsIHRvYXN0IG5vdGlmaWNhdGlvbnNcblx0XHRcdHZhciBjYWxsQmxvY2sgPSBmdW5jdGlvbihvcHRzKSB7XG5cdFx0XHRcdG9wdHMgPSBvcHRzIHx8IHt9O1xuXG5cdFx0XHRcdCQuYmxvY2tVSSh7XG5cdFx0XHRcdFx0bWVzc2FnZTogJG0sXG5cdFx0XHRcdFx0ZmFkZUluIDogdHlwZW9mIG9wdHMuZmFkZUluICAhPT0gJ3VuZGVmaW5lZCcgPyBvcHRzLmZhZGVJbiAgOiA3MDAsXG5cdFx0XHRcdFx0ZmFkZU91dDogdHlwZW9mIG9wdHMuZmFkZU91dCAhPT0gJ3VuZGVmaW5lZCcgPyBvcHRzLmZhZGVPdXQgOiAxMDAwLFxuXHRcdFx0XHRcdHRpbWVvdXQ6IHR5cGVvZiBvcHRzLnRpbWVvdXQgIT09ICd1bmRlZmluZWQnID8gb3B0cy50aW1lb3V0IDogdGltZW91dCxcblx0XHRcdFx0XHRjZW50ZXJZOiBmYWxzZSxcblx0XHRcdFx0XHRzaG93T3ZlcmxheTogZmFsc2UsXG5cdFx0XHRcdFx0b25VbmJsb2NrOiBvbkNsb3NlLFxuXHRcdFx0XHRcdGNzczogJC5ibG9ja1VJLmRlZmF1bHRzLmdyb3dsQ1NTXG5cdFx0XHRcdH0pO1xuXHRcdFx0fTtcblxuXHRcdFx0Y2FsbEJsb2NrKCk7XG5cdFx0XHR2YXIgbm9ubW91c2VkT3BhY2l0eSA9ICRtLmNzcygnb3BhY2l0eScpO1xuXHRcdFx0JG0ubW91c2VvdmVyKGZ1bmN0aW9uKCkge1xuXHRcdFx0XHRjYWxsQmxvY2soe1xuXHRcdFx0XHRcdGZhZGVJbjogMCxcblx0XHRcdFx0XHR0aW1lb3V0OiAzMDAwMFxuXHRcdFx0XHR9KTtcblxuXHRcdFx0XHR2YXIgZGlzcGxheUJsb2NrID0gJCgnLmJsb2NrTXNnJyk7XG5cdFx0XHRcdGRpc3BsYXlCbG9jay5zdG9wKCk7IC8vIGNhbmNlbCBmYWRlb3V0IGlmIGl0IGhhcyBzdGFydGVkXG5cdFx0XHRcdGRpc3BsYXlCbG9jay5mYWRlVG8oMzAwLCAxKTsgLy8gbWFrZSBpdCBlYXNpZXIgdG8gcmVhZCB0aGUgbWVzc2FnZSBieSByZW1vdmluZyB0cmFuc3BhcmVuY3lcblx0XHRcdH0pLm1vdXNlb3V0KGZ1bmN0aW9uKCkge1xuXHRcdFx0XHQkKCcuYmxvY2tNc2cnKS5mYWRlT3V0KDEwMDApO1xuXHRcdFx0fSk7XG5cdFx0XHQvLyBFbmQga29uYXB1biBhZGRpdGlvbnNcblx0XHR9O1xuXG5cdFx0Ly8gcGx1Z2luIG1ldGhvZCBmb3IgYmxvY2tpbmcgZWxlbWVudCBjb250ZW50XG5cdFx0JC5mbi5ibG9jayA9IGZ1bmN0aW9uKG9wdHMpIHtcblx0XHRcdGlmICggdGhpc1swXSA9PT0gd2luZG93ICkge1xuXHRcdFx0XHQkLmJsb2NrVUkoIG9wdHMgKTtcblx0XHRcdFx0cmV0dXJuIHRoaXM7XG5cdFx0XHR9XG5cdFx0XHR2YXIgZnVsbE9wdHMgPSAkLmV4dGVuZCh7fSwgJC5ibG9ja1VJLmRlZmF1bHRzLCBvcHRzIHx8IHt9KTtcblx0XHRcdHRoaXMuZWFjaChmdW5jdGlvbigpIHtcblx0XHRcdFx0dmFyICRlbCA9ICQodGhpcyk7XG5cdFx0XHRcdGlmIChmdWxsT3B0cy5pZ25vcmVJZkJsb2NrZWQgJiYgJGVsLmRhdGEoJ2Jsb2NrVUkuaXNCbG9ja2VkJykpXG5cdFx0XHRcdFx0cmV0dXJuO1xuXHRcdFx0XHQkZWwudW5ibG9jayh7IGZhZGVPdXQ6IDAgfSk7XG5cdFx0XHR9KTtcblxuXHRcdFx0cmV0dXJuIHRoaXMuZWFjaChmdW5jdGlvbigpIHtcblx0XHRcdFx0aWYgKCQuY3NzKHRoaXMsJ3Bvc2l0aW9uJykgPT0gJ3N0YXRpYycpIHtcblx0XHRcdFx0XHR0aGlzLnN0eWxlLnBvc2l0aW9uID0gJ3JlbGF0aXZlJztcblx0XHRcdFx0XHQkKHRoaXMpLmRhdGEoJ2Jsb2NrVUkuc3RhdGljJywgdHJ1ZSk7XG5cdFx0XHRcdH1cblx0XHRcdFx0dGhpcy5zdHlsZS56b29tID0gMTsgLy8gZm9yY2UgJ2hhc0xheW91dCcgaW4gaWVcblx0XHRcdFx0aW5zdGFsbCh0aGlzLCBvcHRzKTtcblx0XHRcdH0pO1xuXHRcdH07XG5cblx0XHQvLyBwbHVnaW4gbWV0aG9kIGZvciB1bmJsb2NraW5nIGVsZW1lbnQgY29udGVudFxuXHRcdCQuZm4udW5ibG9jayA9IGZ1bmN0aW9uKG9wdHMpIHtcblx0XHRcdGlmICggdGhpc1swXSA9PT0gd2luZG93ICkge1xuXHRcdFx0XHQkLnVuYmxvY2tVSSggb3B0cyApO1xuXHRcdFx0XHRyZXR1cm4gdGhpcztcblx0XHRcdH1cblx0XHRcdHJldHVybiB0aGlzLmVhY2goZnVuY3Rpb24oKSB7XG5cdFx0XHRcdHJlbW92ZSh0aGlzLCBvcHRzKTtcblx0XHRcdH0pO1xuXHRcdH07XG5cblx0XHQkLmJsb2NrVUkudmVyc2lvbiA9IDIuNjY7IC8vIDJuZCBnZW5lcmF0aW9uIGJsb2NraW5nIGF0IG5vIGV4dHJhIGNvc3QhXG5cblx0XHQvLyBvdmVycmlkZSB0aGVzZSBpbiB5b3VyIGNvZGUgdG8gY2hhbmdlIHRoZSBkZWZhdWx0IGJlaGF2aW9yIGFuZCBzdHlsZVxuXHRcdCQuYmxvY2tVSS5kZWZhdWx0cyA9IHtcblx0XHRcdC8vIG1lc3NhZ2UgZGlzcGxheWVkIHdoZW4gYmxvY2tpbmcgKHVzZSBudWxsIGZvciBubyBtZXNzYWdlKVxuXHRcdFx0bWVzc2FnZTogICc8aDE+UGxlYXNlIHdhaXQuLi48L2gxPicsXG5cblx0XHRcdHRpdGxlOiBudWxsLFx0XHQvLyB0aXRsZSBzdHJpbmc7IG9ubHkgdXNlZCB3aGVuIHRoZW1lID09IHRydWVcblx0XHRcdGRyYWdnYWJsZTogdHJ1ZSxcdC8vIG9ubHkgdXNlZCB3aGVuIHRoZW1lID09IHRydWUgKHJlcXVpcmVzIGpxdWVyeS11aS5qcyB0byBiZSBsb2FkZWQpXG5cblx0XHRcdHRoZW1lOiBmYWxzZSwgLy8gc2V0IHRvIHRydWUgdG8gdXNlIHdpdGggalF1ZXJ5IFVJIHRoZW1lc1xuXG5cdFx0XHQvLyBzdHlsZXMgZm9yIHRoZSBtZXNzYWdlIHdoZW4gYmxvY2tpbmc7IGlmIHlvdSB3aXNoIHRvIGRpc2FibGVcblx0XHRcdC8vIHRoZXNlIGFuZCB1c2UgYW4gZXh0ZXJuYWwgc3R5bGVzaGVldCB0aGVuIGRvIHRoaXMgaW4geW91ciBjb2RlOlxuXHRcdFx0Ly8gJC5ibG9ja1VJLmRlZmF1bHRzLmNzcyA9IHt9O1xuXHRcdFx0Y3NzOiB7XG5cdFx0XHRcdHBhZGRpbmc6XHQwLFxuXHRcdFx0XHRtYXJnaW46XHRcdDAsXG5cdFx0XHRcdHdpZHRoOlx0XHQnMzAlJyxcblx0XHRcdFx0dG9wOlx0XHQnNDAlJyxcblx0XHRcdFx0bGVmdDpcdFx0JzM1JScsXG5cdFx0XHRcdHRleHRBbGlnbjpcdCdjZW50ZXInLFxuXHRcdFx0XHRjb2xvcjpcdFx0JyMwMDAnLFxuXHRcdFx0XHRib3JkZXI6XHRcdCczcHggc29saWQgI2FhYScsXG5cdFx0XHRcdGJhY2tncm91bmRDb2xvcjonI2ZmZicsXG5cdFx0XHRcdGN1cnNvcjpcdFx0J3dhaXQnXG5cdFx0XHR9LFxuXG5cdFx0XHQvLyBtaW5pbWFsIHN0eWxlIHNldCB1c2VkIHdoZW4gdGhlbWVzIGFyZSB1c2VkXG5cdFx0XHR0aGVtZWRDU1M6IHtcblx0XHRcdFx0d2lkdGg6XHQnMzAlJyxcblx0XHRcdFx0dG9wOlx0JzQwJScsXG5cdFx0XHRcdGxlZnQ6XHQnMzUlJ1xuXHRcdFx0fSxcblxuXHRcdFx0Ly8gc3R5bGVzIGZvciB0aGUgb3ZlcmxheVxuXHRcdFx0b3ZlcmxheUNTUzogIHtcblx0XHRcdFx0YmFja2dyb3VuZENvbG9yOlx0JyMwMDAnLFxuXHRcdFx0XHRvcGFjaXR5Olx0XHRcdDAuNixcblx0XHRcdFx0Y3Vyc29yOlx0XHRcdFx0J3dhaXQnXG5cdFx0XHR9LFxuXG5cdFx0XHQvLyBzdHlsZSB0byByZXBsYWNlIHdhaXQgY3Vyc29yIGJlZm9yZSB1bmJsb2NraW5nIHRvIGNvcnJlY3QgaXNzdWVcblx0XHRcdC8vIG9mIGxpbmdlcmluZyB3YWl0IGN1cnNvclxuXHRcdFx0Y3Vyc29yUmVzZXQ6ICdkZWZhdWx0JyxcblxuXHRcdFx0Ly8gc3R5bGVzIGFwcGxpZWQgd2hlbiB1c2luZyAkLmdyb3dsVUlcblx0XHRcdGdyb3dsQ1NTOiB7XG5cdFx0XHRcdHdpZHRoOlx0XHQnMzUwcHgnLFxuXHRcdFx0XHR0b3A6XHRcdCcxMHB4Jyxcblx0XHRcdFx0bGVmdDpcdFx0JycsXG5cdFx0XHRcdHJpZ2h0Olx0XHQnMTBweCcsXG5cdFx0XHRcdGJvcmRlcjpcdFx0J25vbmUnLFxuXHRcdFx0XHRwYWRkaW5nOlx0JzVweCcsXG5cdFx0XHRcdG9wYWNpdHk6XHQwLjYsXG5cdFx0XHRcdGN1cnNvcjpcdFx0J2RlZmF1bHQnLFxuXHRcdFx0XHRjb2xvcjpcdFx0JyNmZmYnLFxuXHRcdFx0XHRiYWNrZ3JvdW5kQ29sb3I6ICcjMDAwJyxcblx0XHRcdFx0Jy13ZWJraXQtYm9yZGVyLXJhZGl1cyc6JzEwcHgnLFxuXHRcdFx0XHQnLW1vei1ib3JkZXItcmFkaXVzJzpcdCcxMHB4Jyxcblx0XHRcdFx0J2JvcmRlci1yYWRpdXMnOlx0XHQnMTBweCdcblx0XHRcdH0sXG5cblx0XHRcdC8vIElFIGlzc3VlczogJ2Fib3V0OmJsYW5rJyBmYWlscyBvbiBIVFRQUyBhbmQgamF2YXNjcmlwdDpmYWxzZSBpcyBzLWwtby13XG5cdFx0XHQvLyAoaGF0IHRpcCB0byBKb3JnZSBILiBOLiBkZSBWYXNjb25jZWxvcylcblx0XHRcdC8qanNoaW50IHNjcmlwdHVybDp0cnVlICovXG5cdFx0XHRpZnJhbWVTcmM6IC9eaHR0cHMvaS50ZXN0KHdpbmRvdy5sb2NhdGlvbi5ocmVmIHx8ICcnKSA/ICdqYXZhc2NyaXB0OmZhbHNlJyA6ICdhYm91dDpibGFuaycsXG5cblx0XHRcdC8vIGZvcmNlIHVzYWdlIG9mIGlmcmFtZSBpbiBub24tSUUgYnJvd3NlcnMgKGhhbmR5IGZvciBibG9ja2luZyBhcHBsZXRzKVxuXHRcdFx0Zm9yY2VJZnJhbWU6IGZhbHNlLFxuXG5cdFx0XHQvLyB6LWluZGV4IGZvciB0aGUgYmxvY2tpbmcgb3ZlcmxheVxuXHRcdFx0YmFzZVo6IDEwMDAsXG5cblx0XHRcdC8vIHNldCB0aGVzZSB0byB0cnVlIHRvIGhhdmUgdGhlIG1lc3NhZ2UgYXV0b21hdGljYWxseSBjZW50ZXJlZFxuXHRcdFx0Y2VudGVyWDogdHJ1ZSwgLy8gPC0tIG9ubHkgZWZmZWN0cyBlbGVtZW50IGJsb2NraW5nIChwYWdlIGJsb2NrIGNvbnRyb2xsZWQgdmlhIGNzcyBhYm92ZSlcblx0XHRcdGNlbnRlclk6IHRydWUsXG5cblx0XHRcdC8vIGFsbG93IGJvZHkgZWxlbWVudCB0byBiZSBzdGV0Y2hlZCBpbiBpZTY7IHRoaXMgbWFrZXMgYmxvY2tpbmcgbG9vayBiZXR0ZXJcblx0XHRcdC8vIG9uIFwic2hvcnRcIiBwYWdlcy4gIGRpc2FibGUgaWYgeW91IHdpc2ggdG8gcHJldmVudCBjaGFuZ2VzIHRvIHRoZSBib2R5IGhlaWdodFxuXHRcdFx0YWxsb3dCb2R5U3RyZXRjaDogdHJ1ZSxcblxuXHRcdFx0Ly8gZW5hYmxlIGlmIHlvdSB3YW50IGtleSBhbmQgbW91c2UgZXZlbnRzIHRvIGJlIGRpc2FibGVkIGZvciBjb250ZW50IHRoYXQgaXMgYmxvY2tlZFxuXHRcdFx0YmluZEV2ZW50czogdHJ1ZSxcblxuXHRcdFx0Ly8gYmUgZGVmYXVsdCBibG9ja1VJIHdpbGwgc3VwcmVzcyB0YWIgbmF2aWdhdGlvbiBmcm9tIGxlYXZpbmcgYmxvY2tpbmcgY29udGVudFxuXHRcdFx0Ly8gKGlmIGJpbmRFdmVudHMgaXMgdHJ1ZSlcblx0XHRcdGNvbnN0cmFpblRhYktleTogdHJ1ZSxcblxuXHRcdFx0Ly8gZmFkZUluIHRpbWUgaW4gbWlsbGlzOyBzZXQgdG8gMCB0byBkaXNhYmxlIGZhZGVJbiBvbiBibG9ja1xuXHRcdFx0ZmFkZUluOiAgMjAwLFxuXG5cdFx0XHQvLyBmYWRlT3V0IHRpbWUgaW4gbWlsbGlzOyBzZXQgdG8gMCB0byBkaXNhYmxlIGZhZGVPdXQgb24gdW5ibG9ja1xuXHRcdFx0ZmFkZU91dDogIDQwMCxcblxuXHRcdFx0Ly8gdGltZSBpbiBtaWxsaXMgdG8gd2FpdCBiZWZvcmUgYXV0by11bmJsb2NraW5nOyBzZXQgdG8gMCB0byBkaXNhYmxlIGF1dG8tdW5ibG9ja1xuXHRcdFx0dGltZW91dDogMCxcblxuXHRcdFx0Ly8gZGlzYWJsZSBpZiB5b3UgZG9uJ3Qgd2FudCB0byBzaG93IHRoZSBvdmVybGF5XG5cdFx0XHRzaG93T3ZlcmxheTogdHJ1ZSxcblxuXHRcdFx0Ly8gaWYgdHJ1ZSwgZm9jdXMgd2lsbCBiZSBwbGFjZWQgaW4gdGhlIGZpcnN0IGF2YWlsYWJsZSBpbnB1dCBmaWVsZCB3aGVuXG5cdFx0XHQvLyBwYWdlIGJsb2NraW5nXG5cdFx0XHRmb2N1c0lucHV0OiB0cnVlLFxuXG4gICAgICAgICAgICAvLyBlbGVtZW50cyB0aGF0IGNhbiByZWNlaXZlIGZvY3VzXG4gICAgICAgICAgICBmb2N1c2FibGVFbGVtZW50czogJzppbnB1dDplbmFibGVkOnZpc2libGUnLFxuXG5cdFx0XHQvLyBzdXBwcmVzc2VzIHRoZSB1c2Ugb2Ygb3ZlcmxheSBzdHlsZXMgb24gRkYvTGludXggKGR1ZSB0byBwZXJmb3JtYW5jZSBpc3N1ZXMgd2l0aCBvcGFjaXR5KVxuXHRcdFx0Ly8gbm8gbG9uZ2VyIG5lZWRlZCBpbiAyMDEyXG5cdFx0XHQvLyBhcHBseVBsYXRmb3JtT3BhY2l0eVJ1bGVzOiB0cnVlLFxuXG5cdFx0XHQvLyBjYWxsYmFjayBtZXRob2QgaW52b2tlZCB3aGVuIGZhZGVJbiBoYXMgY29tcGxldGVkIGFuZCBibG9ja2luZyBtZXNzYWdlIGlzIHZpc2libGVcblx0XHRcdG9uQmxvY2s6IG51bGwsXG5cblx0XHRcdC8vIGNhbGxiYWNrIG1ldGhvZCBpbnZva2VkIHdoZW4gdW5ibG9ja2luZyBoYXMgY29tcGxldGVkOyB0aGUgY2FsbGJhY2sgaXNcblx0XHRcdC8vIHBhc3NlZCB0aGUgZWxlbWVudCB0aGF0IGhhcyBiZWVuIHVuYmxvY2tlZCAod2hpY2ggaXMgdGhlIHdpbmRvdyBvYmplY3QgZm9yIHBhZ2Vcblx0XHRcdC8vIGJsb2NrcykgYW5kIHRoZSBvcHRpb25zIHRoYXQgd2VyZSBwYXNzZWQgdG8gdGhlIHVuYmxvY2sgY2FsbDpcblx0XHRcdC8vXHRvblVuYmxvY2soZWxlbWVudCwgb3B0aW9ucylcblx0XHRcdG9uVW5ibG9jazogbnVsbCxcblxuXHRcdFx0Ly8gY2FsbGJhY2sgbWV0aG9kIGludm9rZWQgd2hlbiB0aGUgb3ZlcmxheSBhcmVhIGlzIGNsaWNrZWQuXG5cdFx0XHQvLyBzZXR0aW5nIHRoaXMgd2lsbCB0dXJuIHRoZSBjdXJzb3IgdG8gYSBwb2ludGVyLCBvdGhlcndpc2UgY3Vyc29yIGRlZmluZWQgaW4gb3ZlcmxheUNzcyB3aWxsIGJlIHVzZWQuXG5cdFx0XHRvbk92ZXJsYXlDbGljazogbnVsbCxcblxuXHRcdFx0Ly8gZG9uJ3QgYXNrOyBpZiB5b3UgcmVhbGx5IG11c3Qga25vdzogaHR0cDovL2dyb3Vwcy5nb29nbGUuY29tL3JlcXVpcmVkVXBsb2Fkcy9qcXVlcnktZW4vYnJvd3NlX3RocmVhZC90aHJlYWQvMzY2NDBhODczMDUwMzU5NS8yZjZhNzlhNzdhNzhlNDkzIzJmNmE3OWE3N2E3OGU0OTNcblx0XHRcdHF1aXJrc21vZGVPZmZzZXRIYWNrOiA0LFxuXG5cdFx0XHQvLyBjbGFzcyBuYW1lIG9mIHRoZSBtZXNzYWdlIGJsb2NrXG5cdFx0XHRibG9ja01zZ0NsYXNzOiAnYmxvY2tNc2cnLFxuXG5cdFx0XHQvLyBpZiBpdCBpcyBhbHJlYWR5IGJsb2NrZWQsIHRoZW4gaWdub3JlIGl0IChkb24ndCB1bmJsb2NrIGFuZCByZWJsb2NrKVxuXHRcdFx0aWdub3JlSWZCbG9ja2VkOiBmYWxzZVxuXHRcdH07XG5cblx0XHQvLyBwcml2YXRlIGRhdGEgYW5kIGZ1bmN0aW9ucyBmb2xsb3cuLi5cblxuXHRcdHZhciBwYWdlQmxvY2sgPSBudWxsO1xuXHRcdHZhciBwYWdlQmxvY2tFbHMgPSBbXTtcblxuXHRcdGZ1bmN0aW9uIGluc3RhbGwoZWwsIG9wdHMpIHtcblx0XHRcdHZhciBjc3MsIHRoZW1lZENTUztcblx0XHRcdHZhciBmdWxsID0gKGVsID09IHdpbmRvdyk7XG5cdFx0XHR2YXIgbXNnID0gKG9wdHMgJiYgb3B0cy5tZXNzYWdlICE9PSB1bmRlZmluZWQgPyBvcHRzLm1lc3NhZ2UgOiB1bmRlZmluZWQpO1xuXHRcdFx0b3B0cyA9ICQuZXh0ZW5kKHt9LCAkLmJsb2NrVUkuZGVmYXVsdHMsIG9wdHMgfHwge30pO1xuXG5cdFx0XHRpZiAob3B0cy5pZ25vcmVJZkJsb2NrZWQgJiYgJChlbCkuZGF0YSgnYmxvY2tVSS5pc0Jsb2NrZWQnKSlcblx0XHRcdFx0cmV0dXJuO1xuXG5cdFx0XHRvcHRzLm92ZXJsYXlDU1MgPSAkLmV4dGVuZCh7fSwgJC5ibG9ja1VJLmRlZmF1bHRzLm92ZXJsYXlDU1MsIG9wdHMub3ZlcmxheUNTUyB8fCB7fSk7XG5cdFx0XHRjc3MgPSAkLmV4dGVuZCh7fSwgJC5ibG9ja1VJLmRlZmF1bHRzLmNzcywgb3B0cy5jc3MgfHwge30pO1xuXHRcdFx0aWYgKG9wdHMub25PdmVybGF5Q2xpY2spXG5cdFx0XHRcdG9wdHMub3ZlcmxheUNTUy5jdXJzb3IgPSAncG9pbnRlcic7XG5cblx0XHRcdHRoZW1lZENTUyA9ICQuZXh0ZW5kKHt9LCAkLmJsb2NrVUkuZGVmYXVsdHMudGhlbWVkQ1NTLCBvcHRzLnRoZW1lZENTUyB8fCB7fSk7XG5cdFx0XHRtc2cgPSBtc2cgPT09IHVuZGVmaW5lZCA/IG9wdHMubWVzc2FnZSA6IG1zZztcblxuXHRcdFx0Ly8gcmVtb3ZlIHRoZSBjdXJyZW50IGJsb2NrIChpZiB0aGVyZSBpcyBvbmUpXG5cdFx0XHRpZiAoZnVsbCAmJiBwYWdlQmxvY2spXG5cdFx0XHRcdHJlbW92ZSh3aW5kb3csIHtmYWRlT3V0OjB9KTtcblxuXHRcdFx0Ly8gaWYgYW4gZXhpc3RpbmcgZWxlbWVudCBpcyBiZWluZyB1c2VkIGFzIHRoZSBibG9ja2luZyBjb250ZW50IHRoZW4gd2UgY2FwdHVyZVxuXHRcdFx0Ly8gaXRzIGN1cnJlbnQgcGxhY2UgaW4gdGhlIERPTSAoYW5kIGN1cnJlbnQgZGlzcGxheSBzdHlsZSkgc28gd2UgY2FuIHJlc3RvcmVcblx0XHRcdC8vIGl0IHdoZW4gd2UgdW5ibG9ja1xuXHRcdFx0aWYgKG1zZyAmJiB0eXBlb2YgbXNnICE9ICdzdHJpbmcnICYmIChtc2cucGFyZW50Tm9kZSB8fCBtc2cuanF1ZXJ5KSkge1xuXHRcdFx0XHR2YXIgbm9kZSA9IG1zZy5qcXVlcnkgPyBtc2dbMF0gOiBtc2c7XG5cdFx0XHRcdHZhciBkYXRhID0ge307XG5cdFx0XHRcdCQoZWwpLmRhdGEoJ2Jsb2NrVUkuaGlzdG9yeScsIGRhdGEpO1xuXHRcdFx0XHRkYXRhLmVsID0gbm9kZTtcblx0XHRcdFx0ZGF0YS5wYXJlbnQgPSBub2RlLnBhcmVudE5vZGU7XG5cdFx0XHRcdGRhdGEuZGlzcGxheSA9IG5vZGUuc3R5bGUuZGlzcGxheTtcblx0XHRcdFx0ZGF0YS5wb3NpdGlvbiA9IG5vZGUuc3R5bGUucG9zaXRpb247XG5cdFx0XHRcdGlmIChkYXRhLnBhcmVudClcblx0XHRcdFx0XHRkYXRhLnBhcmVudC5yZW1vdmVDaGlsZChub2RlKTtcblx0XHRcdH1cblxuXHRcdFx0JChlbCkuZGF0YSgnYmxvY2tVSS5vblVuYmxvY2snLCBvcHRzLm9uVW5ibG9jayk7XG5cdFx0XHR2YXIgeiA9IG9wdHMuYmFzZVo7XG5cblx0XHRcdC8vIGJsb2NrVUkgdXNlcyAzIGxheWVycyBmb3IgYmxvY2tpbmcsIGZvciBzaW1wbGljaXR5IHRoZXkgYXJlIGFsbCB1c2VkIG9uIGV2ZXJ5IHBsYXRmb3JtO1xuXHRcdFx0Ly8gbGF5ZXIxIGlzIHRoZSBpZnJhbWUgbGF5ZXIgd2hpY2ggaXMgdXNlZCB0byBzdXByZXNzIGJsZWVkIHRocm91Z2ggb2YgdW5kZXJseWluZyBjb250ZW50XG5cdFx0XHQvLyBsYXllcjIgaXMgdGhlIG92ZXJsYXkgbGF5ZXIgd2hpY2ggaGFzIG9wYWNpdHkgYW5kIGEgd2FpdCBjdXJzb3IgKGJ5IGRlZmF1bHQpXG5cdFx0XHQvLyBsYXllcjMgaXMgdGhlIG1lc3NhZ2UgY29udGVudCB0aGF0IGlzIGRpc3BsYXllZCB3aGlsZSBibG9ja2luZ1xuXHRcdFx0dmFyIGx5cjEsIGx5cjIsIGx5cjMsIHM7XG5cdFx0XHRpZiAobXNpZSB8fCBvcHRzLmZvcmNlSWZyYW1lKVxuXHRcdFx0XHRseXIxID0gJCgnPGlmcmFtZSBjbGFzcz1cImJsb2NrVUlcIiBzdHlsZT1cInotaW5kZXg6JysgKHorKykgKyc7ZGlzcGxheTpub25lO2JvcmRlcjpub25lO21hcmdpbjowO3BhZGRpbmc6MDtwb3NpdGlvbjphYnNvbHV0ZTt3aWR0aDoxMDAlO2hlaWdodDoxMDAlO3RvcDowO2xlZnQ6MFwiIHNyYz1cIicrb3B0cy5pZnJhbWVTcmMrJ1wiPjwvaWZyYW1lPicpO1xuXHRcdFx0ZWxzZVxuXHRcdFx0XHRseXIxID0gJCgnPGRpdiBjbGFzcz1cImJsb2NrVUlcIiBzdHlsZT1cImRpc3BsYXk6bm9uZVwiPjwvZGl2PicpO1xuXG5cdFx0XHRpZiAob3B0cy50aGVtZSlcblx0XHRcdFx0bHlyMiA9ICQoJzxkaXYgY2xhc3M9XCJibG9ja1VJIGJsb2NrT3ZlcmxheSB1aS13aWRnZXQtb3ZlcmxheVwiIHN0eWxlPVwiei1pbmRleDonKyAoeisrKSArJztkaXNwbGF5Om5vbmVcIj48L2Rpdj4nKTtcblx0XHRcdGVsc2Vcblx0XHRcdFx0bHlyMiA9ICQoJzxkaXYgY2xhc3M9XCJibG9ja1VJIGJsb2NrT3ZlcmxheVwiIHN0eWxlPVwiei1pbmRleDonKyAoeisrKSArJztkaXNwbGF5Om5vbmU7Ym9yZGVyOm5vbmU7bWFyZ2luOjA7cGFkZGluZzowO3dpZHRoOjEwMCU7aGVpZ2h0OjEwMCU7dG9wOjA7bGVmdDowXCI+PC9kaXY+Jyk7XG5cblx0XHRcdGlmIChvcHRzLnRoZW1lICYmIGZ1bGwpIHtcblx0XHRcdFx0cyA9ICc8ZGl2IGNsYXNzPVwiYmxvY2tVSSAnICsgb3B0cy5ibG9ja01zZ0NsYXNzICsgJyBibG9ja1BhZ2UgdWktZGlhbG9nIHVpLXdpZGdldCB1aS1jb3JuZXItYWxsXCIgc3R5bGU9XCJ6LWluZGV4OicrKHorMTApKyc7ZGlzcGxheTpub25lO3Bvc2l0aW9uOmZpeGVkXCI+Jztcblx0XHRcdFx0aWYgKCBvcHRzLnRpdGxlICkge1xuXHRcdFx0XHRcdHMgKz0gJzxkaXYgY2xhc3M9XCJ1aS13aWRnZXQtaGVhZGVyIHVpLWRpYWxvZy10aXRsZWJhciB1aS1jb3JuZXItYWxsIGJsb2NrVGl0bGVcIj4nKyhvcHRzLnRpdGxlIHx8ICcmbmJzcDsnKSsnPC9kaXY+Jztcblx0XHRcdFx0fVxuXHRcdFx0XHRzICs9ICc8ZGl2IGNsYXNzPVwidWktd2lkZ2V0LWNvbnRlbnQgdWktZGlhbG9nLWNvbnRlbnRcIj48L2Rpdj4nO1xuXHRcdFx0XHRzICs9ICc8L2Rpdj4nO1xuXHRcdFx0fVxuXHRcdFx0ZWxzZSBpZiAob3B0cy50aGVtZSkge1xuXHRcdFx0XHRzID0gJzxkaXYgY2xhc3M9XCJibG9ja1VJICcgKyBvcHRzLmJsb2NrTXNnQ2xhc3MgKyAnIGJsb2NrRWxlbWVudCB1aS1kaWFsb2cgdWktd2lkZ2V0IHVpLWNvcm5lci1hbGxcIiBzdHlsZT1cInotaW5kZXg6JysoeisxMCkrJztkaXNwbGF5Om5vbmU7cG9zaXRpb246YWJzb2x1dGVcIj4nO1xuXHRcdFx0XHRpZiAoIG9wdHMudGl0bGUgKSB7XG5cdFx0XHRcdFx0cyArPSAnPGRpdiBjbGFzcz1cInVpLXdpZGdldC1oZWFkZXIgdWktZGlhbG9nLXRpdGxlYmFyIHVpLWNvcm5lci1hbGwgYmxvY2tUaXRsZVwiPicrKG9wdHMudGl0bGUgfHwgJyZuYnNwOycpKyc8L2Rpdj4nO1xuXHRcdFx0XHR9XG5cdFx0XHRcdHMgKz0gJzxkaXYgY2xhc3M9XCJ1aS13aWRnZXQtY29udGVudCB1aS1kaWFsb2ctY29udGVudFwiPjwvZGl2Pic7XG5cdFx0XHRcdHMgKz0gJzwvZGl2Pic7XG5cdFx0XHR9XG5cdFx0XHRlbHNlIGlmIChmdWxsKSB7XG5cdFx0XHRcdHMgPSAnPGRpdiBjbGFzcz1cImJsb2NrVUkgJyArIG9wdHMuYmxvY2tNc2dDbGFzcyArICcgYmxvY2tQYWdlXCIgc3R5bGU9XCJ6LWluZGV4OicrKHorMTApKyc7ZGlzcGxheTpub25lO3Bvc2l0aW9uOmZpeGVkXCI+PC9kaXY+Jztcblx0XHRcdH1cblx0XHRcdGVsc2Uge1xuXHRcdFx0XHRzID0gJzxkaXYgY2xhc3M9XCJibG9ja1VJICcgKyBvcHRzLmJsb2NrTXNnQ2xhc3MgKyAnIGJsb2NrRWxlbWVudFwiIHN0eWxlPVwiei1pbmRleDonKyh6KzEwKSsnO2Rpc3BsYXk6bm9uZTtwb3NpdGlvbjphYnNvbHV0ZVwiPjwvZGl2Pic7XG5cdFx0XHR9XG5cdFx0XHRseXIzID0gJChzKTtcblxuXHRcdFx0Ly8gaWYgd2UgaGF2ZSBhIG1lc3NhZ2UsIHN0eWxlIGl0XG5cdFx0XHRpZiAobXNnKSB7XG5cdFx0XHRcdGlmIChvcHRzLnRoZW1lKSB7XG5cdFx0XHRcdFx0bHlyMy5jc3ModGhlbWVkQ1NTKTtcblx0XHRcdFx0XHRseXIzLmFkZENsYXNzKCd1aS13aWRnZXQtY29udGVudCcpO1xuXHRcdFx0XHR9XG5cdFx0XHRcdGVsc2Vcblx0XHRcdFx0XHRseXIzLmNzcyhjc3MpO1xuXHRcdFx0fVxuXG5cdFx0XHQvLyBzdHlsZSB0aGUgb3ZlcmxheVxuXHRcdFx0aWYgKCFvcHRzLnRoZW1lIC8qJiYgKCFvcHRzLmFwcGx5UGxhdGZvcm1PcGFjaXR5UnVsZXMpKi8pXG5cdFx0XHRcdGx5cjIuY3NzKG9wdHMub3ZlcmxheUNTUyk7XG5cdFx0XHRseXIyLmNzcygncG9zaXRpb24nLCBmdWxsID8gJ2ZpeGVkJyA6ICdhYnNvbHV0ZScpO1xuXG5cdFx0XHQvLyBtYWtlIGlmcmFtZSBsYXllciB0cmFuc3BhcmVudCBpbiBJRVxuXHRcdFx0aWYgKG1zaWUgfHwgb3B0cy5mb3JjZUlmcmFtZSlcblx0XHRcdFx0bHlyMS5jc3MoJ29wYWNpdHknLDAuMCk7XG5cblx0XHRcdC8vJChbbHlyMVswXSxseXIyWzBdLGx5cjNbMF1dKS5hcHBlbmRUbyhmdWxsID8gJ2JvZHknIDogZWwpO1xuXHRcdFx0dmFyIGxheWVycyA9IFtseXIxLGx5cjIsbHlyM10sICRwYXIgPSBmdWxsID8gJCgnYm9keScpIDogJChlbCk7XG5cdFx0XHQkLmVhY2gobGF5ZXJzLCBmdW5jdGlvbigpIHtcblx0XHRcdFx0dGhpcy5hcHBlbmRUbygkcGFyKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRpZiAob3B0cy50aGVtZSAmJiBvcHRzLmRyYWdnYWJsZSAmJiAkLmZuLmRyYWdnYWJsZSkge1xuXHRcdFx0XHRseXIzLmRyYWdnYWJsZSh7XG5cdFx0XHRcdFx0aGFuZGxlOiAnLnVpLWRpYWxvZy10aXRsZWJhcicsXG5cdFx0XHRcdFx0Y2FuY2VsOiAnbGknXG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXG5cdFx0XHQvLyBpZTcgbXVzdCB1c2UgYWJzb2x1dGUgcG9zaXRpb25pbmcgaW4gcXVpcmtzIG1vZGUgYW5kIHRvIGFjY291bnQgZm9yIGFjdGl2ZXggaXNzdWVzICh3aGVuIHNjcm9sbGluZylcblx0XHRcdHZhciBleHByID0gc2V0RXhwciAmJiAoISQuc3VwcG9ydC5ib3hNb2RlbCB8fCAkKCdvYmplY3QsZW1iZWQnLCBmdWxsID8gbnVsbCA6IGVsKS5sZW5ndGggPiAwKTtcblx0XHRcdGlmIChpZTYgfHwgZXhwcikge1xuXHRcdFx0XHQvLyBnaXZlIGJvZHkgMTAwJSBoZWlnaHRcblx0XHRcdFx0aWYgKGZ1bGwgJiYgb3B0cy5hbGxvd0JvZHlTdHJldGNoICYmICQuc3VwcG9ydC5ib3hNb2RlbClcblx0XHRcdFx0XHQkKCdodG1sLGJvZHknKS5jc3MoJ2hlaWdodCcsJzEwMCUnKTtcblxuXHRcdFx0XHQvLyBmaXggaWU2IGlzc3VlIHdoZW4gYmxvY2tlZCBlbGVtZW50IGhhcyBhIGJvcmRlciB3aWR0aFxuXHRcdFx0XHRpZiAoKGllNiB8fCAhJC5zdXBwb3J0LmJveE1vZGVsKSAmJiAhZnVsbCkge1xuXHRcdFx0XHRcdHZhciB0ID0gc3ooZWwsJ2JvcmRlclRvcFdpZHRoJyksIGwgPSBzeihlbCwnYm9yZGVyTGVmdFdpZHRoJyk7XG5cdFx0XHRcdFx0dmFyIGZpeFQgPSB0ID8gJygwIC0gJyt0KycpJyA6IDA7XG5cdFx0XHRcdFx0dmFyIGZpeEwgPSBsID8gJygwIC0gJytsKycpJyA6IDA7XG5cdFx0XHRcdH1cblxuXHRcdFx0XHQvLyBzaW11bGF0ZSBmaXhlZCBwb3NpdGlvblxuXHRcdFx0XHQkLmVhY2gobGF5ZXJzLCBmdW5jdGlvbihpLG8pIHtcblx0XHRcdFx0XHR2YXIgcyA9IG9bMF0uc3R5bGU7XG5cdFx0XHRcdFx0cy5wb3NpdGlvbiA9ICdhYnNvbHV0ZSc7XG5cdFx0XHRcdFx0aWYgKGkgPCAyKSB7XG5cdFx0XHRcdFx0XHRpZiAoZnVsbClcblx0XHRcdFx0XHRcdFx0cy5zZXRFeHByZXNzaW9uKCdoZWlnaHQnLCdNYXRoLm1heChkb2N1bWVudC5ib2R5LnNjcm9sbEhlaWdodCwgZG9jdW1lbnQuYm9keS5vZmZzZXRIZWlnaHQpIC0gKGpRdWVyeS5zdXBwb3J0LmJveE1vZGVsPzA6JytvcHRzLnF1aXJrc21vZGVPZmZzZXRIYWNrKycpICsgXCJweFwiJyk7XG5cdFx0XHRcdFx0XHRlbHNlXG5cdFx0XHRcdFx0XHRcdHMuc2V0RXhwcmVzc2lvbignaGVpZ2h0JywndGhpcy5wYXJlbnROb2RlLm9mZnNldEhlaWdodCArIFwicHhcIicpO1xuXHRcdFx0XHRcdFx0aWYgKGZ1bGwpXG5cdFx0XHRcdFx0XHRcdHMuc2V0RXhwcmVzc2lvbignd2lkdGgnLCdqUXVlcnkuc3VwcG9ydC5ib3hNb2RlbCAmJiBkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuY2xpZW50V2lkdGggfHwgZG9jdW1lbnQuYm9keS5jbGllbnRXaWR0aCArIFwicHhcIicpO1xuXHRcdFx0XHRcdFx0ZWxzZVxuXHRcdFx0XHRcdFx0XHRzLnNldEV4cHJlc3Npb24oJ3dpZHRoJywndGhpcy5wYXJlbnROb2RlLm9mZnNldFdpZHRoICsgXCJweFwiJyk7XG5cdFx0XHRcdFx0XHRpZiAoZml4TCkgcy5zZXRFeHByZXNzaW9uKCdsZWZ0JywgZml4TCk7XG5cdFx0XHRcdFx0XHRpZiAoZml4VCkgcy5zZXRFeHByZXNzaW9uKCd0b3AnLCBmaXhUKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0ZWxzZSBpZiAob3B0cy5jZW50ZXJZKSB7XG5cdFx0XHRcdFx0XHRpZiAoZnVsbCkgcy5zZXRFeHByZXNzaW9uKCd0b3AnLCcoZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LmNsaWVudEhlaWdodCB8fCBkb2N1bWVudC5ib2R5LmNsaWVudEhlaWdodCkgLyAyIC0gKHRoaXMub2Zmc2V0SGVpZ2h0IC8gMikgKyAoYmxhaCA9IGRvY3VtZW50LmRvY3VtZW50RWxlbWVudC5zY3JvbGxUb3AgPyBkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuc2Nyb2xsVG9wIDogZG9jdW1lbnQuYm9keS5zY3JvbGxUb3ApICsgXCJweFwiJyk7XG5cdFx0XHRcdFx0XHRzLm1hcmdpblRvcCA9IDA7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdGVsc2UgaWYgKCFvcHRzLmNlbnRlclkgJiYgZnVsbCkge1xuXHRcdFx0XHRcdFx0dmFyIHRvcCA9IChvcHRzLmNzcyAmJiBvcHRzLmNzcy50b3ApID8gcGFyc2VJbnQob3B0cy5jc3MudG9wLCAxMCkgOiAwO1xuXHRcdFx0XHRcdFx0dmFyIGV4cHJlc3Npb24gPSAnKChkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuc2Nyb2xsVG9wID8gZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LnNjcm9sbFRvcCA6IGRvY3VtZW50LmJvZHkuc2Nyb2xsVG9wKSArICcrdG9wKycpICsgXCJweFwiJztcblx0XHRcdFx0XHRcdHMuc2V0RXhwcmVzc2lvbigndG9wJyxleHByZXNzaW9uKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXG5cdFx0XHQvLyBzaG93IHRoZSBtZXNzYWdlXG5cdFx0XHRpZiAobXNnKSB7XG5cdFx0XHRcdGlmIChvcHRzLnRoZW1lKVxuXHRcdFx0XHRcdGx5cjMuZmluZCgnLnVpLXdpZGdldC1jb250ZW50JykuYXBwZW5kKG1zZyk7XG5cdFx0XHRcdGVsc2Vcblx0XHRcdFx0XHRseXIzLmFwcGVuZChtc2cpO1xuXHRcdFx0XHRpZiAobXNnLmpxdWVyeSB8fCBtc2cubm9kZVR5cGUpXG5cdFx0XHRcdFx0JChtc2cpLnNob3coKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKChtc2llIHx8IG9wdHMuZm9yY2VJZnJhbWUpICYmIG9wdHMuc2hvd092ZXJsYXkpXG5cdFx0XHRcdGx5cjEuc2hvdygpOyAvLyBvcGFjaXR5IGlzIHplcm9cblx0XHRcdGlmIChvcHRzLmZhZGVJbikge1xuXHRcdFx0XHR2YXIgY2IgPSBvcHRzLm9uQmxvY2sgPyBvcHRzLm9uQmxvY2sgOiBub09wO1xuXHRcdFx0XHR2YXIgY2IxID0gKG9wdHMuc2hvd092ZXJsYXkgJiYgIW1zZykgPyBjYiA6IG5vT3A7XG5cdFx0XHRcdHZhciBjYjIgPSBtc2cgPyBjYiA6IG5vT3A7XG5cdFx0XHRcdGlmIChvcHRzLnNob3dPdmVybGF5KVxuXHRcdFx0XHRcdGx5cjIuX2ZhZGVJbihvcHRzLmZhZGVJbiwgY2IxKTtcblx0XHRcdFx0aWYgKG1zZylcblx0XHRcdFx0XHRseXIzLl9mYWRlSW4ob3B0cy5mYWRlSW4sIGNiMik7XG5cdFx0XHR9XG5cdFx0XHRlbHNlIHtcblx0XHRcdFx0aWYgKG9wdHMuc2hvd092ZXJsYXkpXG5cdFx0XHRcdFx0bHlyMi5zaG93KCk7XG5cdFx0XHRcdGlmIChtc2cpXG5cdFx0XHRcdFx0bHlyMy5zaG93KCk7XG5cdFx0XHRcdGlmIChvcHRzLm9uQmxvY2spXG5cdFx0XHRcdFx0b3B0cy5vbkJsb2NrKCk7XG5cdFx0XHR9XG5cblx0XHRcdC8vIGJpbmQga2V5IGFuZCBtb3VzZSBldmVudHNcblx0XHRcdGJpbmQoMSwgZWwsIG9wdHMpO1xuXG5cdFx0XHRpZiAoZnVsbCkge1xuXHRcdFx0XHRwYWdlQmxvY2sgPSBseXIzWzBdO1xuXHRcdFx0XHRwYWdlQmxvY2tFbHMgPSAkKG9wdHMuZm9jdXNhYmxlRWxlbWVudHMscGFnZUJsb2NrKTtcblx0XHRcdFx0aWYgKG9wdHMuZm9jdXNJbnB1dClcblx0XHRcdFx0XHRzZXRUaW1lb3V0KGZvY3VzLCAyMCk7XG5cdFx0XHR9XG5cdFx0XHRlbHNlXG5cdFx0XHRcdGNlbnRlcihseXIzWzBdLCBvcHRzLmNlbnRlclgsIG9wdHMuY2VudGVyWSk7XG5cblx0XHRcdGlmIChvcHRzLnRpbWVvdXQpIHtcblx0XHRcdFx0Ly8gYXV0by11bmJsb2NrXG5cdFx0XHRcdHZhciB0byA9IHNldFRpbWVvdXQoZnVuY3Rpb24oKSB7XG5cdFx0XHRcdFx0aWYgKGZ1bGwpXG5cdFx0XHRcdFx0XHQkLnVuYmxvY2tVSShvcHRzKTtcblx0XHRcdFx0XHRlbHNlXG5cdFx0XHRcdFx0XHQkKGVsKS51bmJsb2NrKG9wdHMpO1xuXHRcdFx0XHR9LCBvcHRzLnRpbWVvdXQpO1xuXHRcdFx0XHQkKGVsKS5kYXRhKCdibG9ja1VJLnRpbWVvdXQnLCB0byk7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0Ly8gcmVtb3ZlIHRoZSBibG9ja1xuXHRcdGZ1bmN0aW9uIHJlbW92ZShlbCwgb3B0cykge1xuXHRcdFx0dmFyIGNvdW50O1xuXHRcdFx0dmFyIGZ1bGwgPSAoZWwgPT0gd2luZG93KTtcblx0XHRcdHZhciAkZWwgPSAkKGVsKTtcblx0XHRcdHZhciBkYXRhID0gJGVsLmRhdGEoJ2Jsb2NrVUkuaGlzdG9yeScpO1xuXHRcdFx0dmFyIHRvID0gJGVsLmRhdGEoJ2Jsb2NrVUkudGltZW91dCcpO1xuXHRcdFx0aWYgKHRvKSB7XG5cdFx0XHRcdGNsZWFyVGltZW91dCh0byk7XG5cdFx0XHRcdCRlbC5yZW1vdmVEYXRhKCdibG9ja1VJLnRpbWVvdXQnKTtcblx0XHRcdH1cblx0XHRcdG9wdHMgPSAkLmV4dGVuZCh7fSwgJC5ibG9ja1VJLmRlZmF1bHRzLCBvcHRzIHx8IHt9KTtcblx0XHRcdGJpbmQoMCwgZWwsIG9wdHMpOyAvLyB1bmJpbmQgZXZlbnRzXG5cblx0XHRcdGlmIChvcHRzLm9uVW5ibG9jayA9PT0gbnVsbCkge1xuXHRcdFx0XHRvcHRzLm9uVW5ibG9jayA9ICRlbC5kYXRhKCdibG9ja1VJLm9uVW5ibG9jaycpO1xuXHRcdFx0XHQkZWwucmVtb3ZlRGF0YSgnYmxvY2tVSS5vblVuYmxvY2snKTtcblx0XHRcdH1cblxuXHRcdFx0dmFyIGVscztcblx0XHRcdGlmIChmdWxsKSAvLyBjcmF6eSBzZWxlY3RvciB0byBoYW5kbGUgb2RkIGZpZWxkIGVycm9ycyBpbiBpZTYvN1xuXHRcdFx0XHRlbHMgPSAkKCdib2R5JykuY2hpbGRyZW4oKS5maWx0ZXIoJy5ibG9ja1VJJykuYWRkKCdib2R5ID4gLmJsb2NrVUknKTtcblx0XHRcdGVsc2Vcblx0XHRcdFx0ZWxzID0gJGVsLmZpbmQoJz4uYmxvY2tVSScpO1xuXG5cdFx0XHQvLyBmaXggY3Vyc29yIGlzc3VlXG5cdFx0XHRpZiAoIG9wdHMuY3Vyc29yUmVzZXQgKSB7XG5cdFx0XHRcdGlmICggZWxzLmxlbmd0aCA+IDEgKVxuXHRcdFx0XHRcdGVsc1sxXS5zdHlsZS5jdXJzb3IgPSBvcHRzLmN1cnNvclJlc2V0O1xuXHRcdFx0XHRpZiAoIGVscy5sZW5ndGggPiAyIClcblx0XHRcdFx0XHRlbHNbMl0uc3R5bGUuY3Vyc29yID0gb3B0cy5jdXJzb3JSZXNldDtcblx0XHRcdH1cblxuXHRcdFx0aWYgKGZ1bGwpXG5cdFx0XHRcdHBhZ2VCbG9jayA9IHBhZ2VCbG9ja0VscyA9IG51bGw7XG5cblx0XHRcdGlmIChvcHRzLmZhZGVPdXQpIHtcblx0XHRcdFx0Y291bnQgPSBlbHMubGVuZ3RoO1xuXHRcdFx0XHRlbHMuc3RvcCgpLmZhZGVPdXQob3B0cy5mYWRlT3V0LCBmdW5jdGlvbigpIHtcblx0XHRcdFx0XHRpZiAoIC0tY291bnQgPT09IDApXG5cdFx0XHRcdFx0XHRyZXNldChlbHMsZGF0YSxvcHRzLGVsKTtcblx0XHRcdFx0fSk7XG5cdFx0XHR9XG5cdFx0XHRlbHNlXG5cdFx0XHRcdHJlc2V0KGVscywgZGF0YSwgb3B0cywgZWwpO1xuXHRcdH1cblxuXHRcdC8vIG1vdmUgYmxvY2tpbmcgZWxlbWVudCBiYWNrIGludG8gdGhlIERPTSB3aGVyZSBpdCBzdGFydGVkXG5cdFx0ZnVuY3Rpb24gcmVzZXQoZWxzLGRhdGEsb3B0cyxlbCkge1xuXHRcdFx0dmFyICRlbCA9ICQoZWwpO1xuXHRcdFx0aWYgKCAkZWwuZGF0YSgnYmxvY2tVSS5pc0Jsb2NrZWQnKSApXG5cdFx0XHRcdHJldHVybjtcblxuXHRcdFx0ZWxzLmVhY2goZnVuY3Rpb24oaSxvKSB7XG5cdFx0XHRcdC8vIHJlbW92ZSB2aWEgRE9NIGNhbGxzIHNvIHdlIGRvbid0IGxvc2UgZXZlbnQgaGFuZGxlcnNcblx0XHRcdFx0aWYgKHRoaXMucGFyZW50Tm9kZSlcblx0XHRcdFx0XHR0aGlzLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQodGhpcyk7XG5cdFx0XHR9KTtcblxuXHRcdFx0aWYgKGRhdGEgJiYgZGF0YS5lbCkge1xuXHRcdFx0XHRkYXRhLmVsLnN0eWxlLmRpc3BsYXkgPSBkYXRhLmRpc3BsYXk7XG5cdFx0XHRcdGRhdGEuZWwuc3R5bGUucG9zaXRpb24gPSBkYXRhLnBvc2l0aW9uO1xuXHRcdFx0XHRpZiAoZGF0YS5wYXJlbnQpXG5cdFx0XHRcdFx0ZGF0YS5wYXJlbnQuYXBwZW5kQ2hpbGQoZGF0YS5lbCk7XG5cdFx0XHRcdCRlbC5yZW1vdmVEYXRhKCdibG9ja1VJLmhpc3RvcnknKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKCRlbC5kYXRhKCdibG9ja1VJLnN0YXRpYycpKSB7XG5cdFx0XHRcdCRlbC5jc3MoJ3Bvc2l0aW9uJywgJ3N0YXRpYycpOyAvLyAjMjJcblx0XHRcdH1cblxuXHRcdFx0aWYgKHR5cGVvZiBvcHRzLm9uVW5ibG9jayA9PSAnZnVuY3Rpb24nKVxuXHRcdFx0XHRvcHRzLm9uVW5ibG9jayhlbCxvcHRzKTtcblxuXHRcdFx0Ly8gZml4IGlzc3VlIGluIFNhZmFyaSA2IHdoZXJlIGJsb2NrIGFydGlmYWN0cyByZW1haW4gdW50aWwgcmVmbG93XG5cdFx0XHR2YXIgYm9keSA9ICQoZG9jdW1lbnQuYm9keSksIHcgPSBib2R5LndpZHRoKCksIGNzc1cgPSBib2R5WzBdLnN0eWxlLndpZHRoO1xuXHRcdFx0Ym9keS53aWR0aCh3LTEpLndpZHRoKHcpO1xuXHRcdFx0Ym9keVswXS5zdHlsZS53aWR0aCA9IGNzc1c7XG5cdFx0fVxuXG5cdFx0Ly8gYmluZC91bmJpbmQgdGhlIGhhbmRsZXJcblx0XHRmdW5jdGlvbiBiaW5kKGIsIGVsLCBvcHRzKSB7XG5cdFx0XHR2YXIgZnVsbCA9IGVsID09IHdpbmRvdywgJGVsID0gJChlbCk7XG5cblx0XHRcdC8vIGRvbid0IGJvdGhlciB1bmJpbmRpbmcgaWYgdGhlcmUgaXMgbm90aGluZyB0byB1bmJpbmRcblx0XHRcdGlmICghYiAmJiAoZnVsbCAmJiAhcGFnZUJsb2NrIHx8ICFmdWxsICYmICEkZWwuZGF0YSgnYmxvY2tVSS5pc0Jsb2NrZWQnKSkpXG5cdFx0XHRcdHJldHVybjtcblxuXHRcdFx0JGVsLmRhdGEoJ2Jsb2NrVUkuaXNCbG9ja2VkJywgYik7XG5cblx0XHRcdC8vIGRvbid0IGJpbmQgZXZlbnRzIHdoZW4gb3ZlcmxheSBpcyBub3QgaW4gdXNlIG9yIGlmIGJpbmRFdmVudHMgaXMgZmFsc2Vcblx0XHRcdGlmICghZnVsbCB8fCAhb3B0cy5iaW5kRXZlbnRzIHx8IChiICYmICFvcHRzLnNob3dPdmVybGF5KSlcblx0XHRcdFx0cmV0dXJuO1xuXG5cdFx0XHQvLyBiaW5kIGFuY2hvcnMgYW5kIGlucHV0cyBmb3IgbW91c2UgYW5kIGtleSBldmVudHNcblx0XHRcdHZhciBldmVudHMgPSAnbW91c2Vkb3duIG1vdXNldXAga2V5ZG93biBrZXlwcmVzcyBrZXl1cCB0b3VjaHN0YXJ0IHRvdWNoZW5kIHRvdWNobW92ZSc7XG5cdFx0XHRpZiAoYilcblx0XHRcdFx0JChkb2N1bWVudCkuYmluZChldmVudHMsIG9wdHMsIGhhbmRsZXIpO1xuXHRcdFx0ZWxzZVxuXHRcdFx0XHQkKGRvY3VtZW50KS51bmJpbmQoZXZlbnRzLCBoYW5kbGVyKTtcblxuXHRcdC8vIGZvcm1lciBpbXBsLi4uXG5cdFx0Ly9cdFx0dmFyICRlID0gJCgnYSw6aW5wdXQnKTtcblx0XHQvL1x0XHRiID8gJGUuYmluZChldmVudHMsIG9wdHMsIGhhbmRsZXIpIDogJGUudW5iaW5kKGV2ZW50cywgaGFuZGxlcik7XG5cdFx0fVxuXG5cdFx0Ly8gZXZlbnQgaGFuZGxlciB0byBzdXBwcmVzcyBrZXlib2FyZC9tb3VzZSBldmVudHMgd2hlbiBibG9ja2luZ1xuXHRcdGZ1bmN0aW9uIGhhbmRsZXIoZSkge1xuXHRcdFx0Ly8gYWxsb3cgdGFiIG5hdmlnYXRpb24gKGNvbmRpdGlvbmFsbHkpXG5cdFx0XHRpZiAoZS50eXBlID09PSAna2V5ZG93bicgJiYgZS5rZXlDb2RlICYmIGUua2V5Q29kZSA9PSA5KSB7XG5cdFx0XHRcdGlmIChwYWdlQmxvY2sgJiYgZS5kYXRhLmNvbnN0cmFpblRhYktleSkge1xuXHRcdFx0XHRcdHZhciBlbHMgPSBwYWdlQmxvY2tFbHM7XG5cdFx0XHRcdFx0dmFyIGZ3ZCA9ICFlLnNoaWZ0S2V5ICYmIGUudGFyZ2V0ID09PSBlbHNbZWxzLmxlbmd0aC0xXTtcblx0XHRcdFx0XHR2YXIgYmFjayA9IGUuc2hpZnRLZXkgJiYgZS50YXJnZXQgPT09IGVsc1swXTtcblx0XHRcdFx0XHRpZiAoZndkIHx8IGJhY2spIHtcblx0XHRcdFx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24oKXtmb2N1cyhiYWNrKTt9LDEwKTtcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdHZhciBvcHRzID0gZS5kYXRhO1xuXHRcdFx0dmFyIHRhcmdldCA9ICQoZS50YXJnZXQpO1xuXHRcdFx0aWYgKHRhcmdldC5oYXNDbGFzcygnYmxvY2tPdmVybGF5JykgJiYgb3B0cy5vbk92ZXJsYXlDbGljaylcblx0XHRcdFx0b3B0cy5vbk92ZXJsYXlDbGljayhlKTtcblxuXHRcdFx0Ly8gYWxsb3cgZXZlbnRzIHdpdGhpbiB0aGUgbWVzc2FnZSBjb250ZW50XG5cdFx0XHRpZiAodGFyZ2V0LnBhcmVudHMoJ2Rpdi4nICsgb3B0cy5ibG9ja01zZ0NsYXNzKS5sZW5ndGggPiAwKVxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcblxuXHRcdFx0Ly8gYWxsb3cgZXZlbnRzIGZvciBjb250ZW50IHRoYXQgaXMgbm90IGJlaW5nIGJsb2NrZWRcblx0XHRcdHJldHVybiB0YXJnZXQucGFyZW50cygpLmNoaWxkcmVuKCkuZmlsdGVyKCdkaXYuYmxvY2tVSScpLmxlbmd0aCA9PT0gMDtcblx0XHR9XG5cblx0XHRmdW5jdGlvbiBmb2N1cyhiYWNrKSB7XG5cdFx0XHRpZiAoIXBhZ2VCbG9ja0Vscylcblx0XHRcdFx0cmV0dXJuO1xuXHRcdFx0dmFyIGUgPSBwYWdlQmxvY2tFbHNbYmFjaz09PXRydWUgPyBwYWdlQmxvY2tFbHMubGVuZ3RoLTEgOiAwXTtcblx0XHRcdGlmIChlKVxuXHRcdFx0XHRlLmZvY3VzKCk7XG5cdFx0fVxuXG5cdFx0ZnVuY3Rpb24gY2VudGVyKGVsLCB4LCB5KSB7XG5cdFx0XHR2YXIgcCA9IGVsLnBhcmVudE5vZGUsIHMgPSBlbC5zdHlsZTtcblx0XHRcdHZhciBsID0gKChwLm9mZnNldFdpZHRoIC0gZWwub2Zmc2V0V2lkdGgpLzIpIC0gc3oocCwnYm9yZGVyTGVmdFdpZHRoJyk7XG5cdFx0XHR2YXIgdCA9ICgocC5vZmZzZXRIZWlnaHQgLSBlbC5vZmZzZXRIZWlnaHQpLzIpIC0gc3oocCwnYm9yZGVyVG9wV2lkdGgnKTtcblx0XHRcdGlmICh4KSBzLmxlZnQgPSBsID4gMCA/IChsKydweCcpIDogJzAnO1xuXHRcdFx0aWYgKHkpIHMudG9wICA9IHQgPiAwID8gKHQrJ3B4JykgOiAnMCc7XG5cdFx0fVxuXG5cdFx0ZnVuY3Rpb24gc3ooZWwsIHApIHtcblx0XHRcdHJldHVybiBwYXJzZUludCgkLmNzcyhlbCxwKSwxMCl8fDA7XG5cdFx0fVxuXG5cdH1cblxuXG5cdC8qZ2xvYmFsIGRlZmluZTp0cnVlICovXG5cdGlmICh0eXBlb2YgZGVmaW5lID09PSAnZnVuY3Rpb24nICYmIGRlZmluZS5hbWQgJiYgZGVmaW5lLmFtZC5qUXVlcnkpIHtcblx0XHRkZWZpbmUoWydqcXVlcnknXSwgc2V0dXApO1xuXHR9IGVsc2Uge1xuXHRcdHNldHVwKGpRdWVyeSk7XG5cdH1cblxufSkoKTtcbiJdLCJmaWxlIjoianF1ZXJ5LmJsb2NrVUkuanMiLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==