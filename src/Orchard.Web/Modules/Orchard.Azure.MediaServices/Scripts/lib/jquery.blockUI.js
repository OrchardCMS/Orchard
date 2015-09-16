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

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqcXVlcnkuYmxvY2tVSS5qcyJdLCJzb3VyY2VzQ29udGVudCI6WyIvKiFcclxuICogalF1ZXJ5IGJsb2NrVUkgcGx1Z2luXHJcbiAqIFZlcnNpb24gMi42Ni4wLTIwMTMuMTAuMDlcclxuICogUmVxdWlyZXMgalF1ZXJ5IHYxLjcgb3IgbGF0ZXJcclxuICpcclxuICogRXhhbXBsZXMgYXQ6IGh0dHA6Ly9tYWxzdXAuY29tL2pxdWVyeS9ibG9jay9cclxuICogQ29weXJpZ2h0IChjKSAyMDA3LTIwMTMgTS4gQWxzdXBcclxuICogRHVhbCBsaWNlbnNlZCB1bmRlciB0aGUgTUlUIGFuZCBHUEwgbGljZW5zZXM6XHJcbiAqIGh0dHA6Ly93d3cub3BlbnNvdXJjZS5vcmcvbGljZW5zZXMvbWl0LWxpY2Vuc2UucGhwXHJcbiAqIGh0dHA6Ly93d3cuZ251Lm9yZy9saWNlbnNlcy9ncGwuaHRtbFxyXG4gKlxyXG4gKiBUaGFua3MgdG8gQW1pci1Ib3NzZWluIFNvYmhpIGZvciBzb21lIGV4Y2VsbGVudCBjb250cmlidXRpb25zIVxyXG4gKi9cclxuXHJcbjsoZnVuY3Rpb24oKSB7XHJcbi8qanNoaW50IGVxZXFlcTpmYWxzZSBjdXJseTpmYWxzZSBsYXRlZGVmOmZhbHNlICovXHJcblwidXNlIHN0cmljdFwiO1xyXG5cclxuXHRmdW5jdGlvbiBzZXR1cCgkKSB7XHJcblx0XHQkLmZuLl9mYWRlSW4gPSAkLmZuLmZhZGVJbjtcclxuXHJcblx0XHR2YXIgbm9PcCA9ICQubm9vcCB8fCBmdW5jdGlvbigpIHt9O1xyXG5cclxuXHRcdC8vIHRoaXMgYml0IGlzIHRvIGVuc3VyZSB3ZSBkb24ndCBjYWxsIHNldEV4cHJlc3Npb24gd2hlbiB3ZSBzaG91bGRuJ3QgKHdpdGggZXh0cmEgbXVzY2xlIHRvIGhhbmRsZVxyXG5cdFx0Ly8gY29uZnVzaW5nIHVzZXJBZ2VudCBzdHJpbmdzIG9uIFZpc3RhKVxyXG5cdFx0dmFyIG1zaWUgPSAvTVNJRS8udGVzdChuYXZpZ2F0b3IudXNlckFnZW50KTtcclxuXHRcdHZhciBpZTYgID0gL01TSUUgNi4wLy50ZXN0KG5hdmlnYXRvci51c2VyQWdlbnQpICYmICEgL01TSUUgOC4wLy50ZXN0KG5hdmlnYXRvci51c2VyQWdlbnQpO1xyXG5cdFx0dmFyIG1vZGUgPSBkb2N1bWVudC5kb2N1bWVudE1vZGUgfHwgMDtcclxuXHRcdHZhciBzZXRFeHByID0gJC5pc0Z1bmN0aW9uKCBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdkaXYnKS5zdHlsZS5zZXRFeHByZXNzaW9uICk7XHJcblxyXG5cdFx0Ly8gZ2xvYmFsICQgbWV0aG9kcyBmb3IgYmxvY2tpbmcvdW5ibG9ja2luZyB0aGUgZW50aXJlIHBhZ2VcclxuXHRcdCQuYmxvY2tVSSAgID0gZnVuY3Rpb24ob3B0cykgeyBpbnN0YWxsKHdpbmRvdywgb3B0cyk7IH07XHJcblx0XHQkLnVuYmxvY2tVSSA9IGZ1bmN0aW9uKG9wdHMpIHsgcmVtb3ZlKHdpbmRvdywgb3B0cyk7IH07XHJcblxyXG5cdFx0Ly8gY29udmVuaWVuY2UgbWV0aG9kIGZvciBxdWljayBncm93bC1saWtlIG5vdGlmaWNhdGlvbnMgIChodHRwOi8vd3d3Lmdvb2dsZS5jb20vc2VhcmNoP3E9Z3Jvd2wpXHJcblx0XHQkLmdyb3dsVUkgPSBmdW5jdGlvbih0aXRsZSwgbWVzc2FnZSwgdGltZW91dCwgb25DbG9zZSkge1xyXG5cdFx0XHR2YXIgJG0gPSAkKCc8ZGl2IGNsYXNzPVwiZ3Jvd2xVSVwiPjwvZGl2PicpO1xyXG5cdFx0XHRpZiAodGl0bGUpICRtLmFwcGVuZCgnPGgxPicrdGl0bGUrJzwvaDE+Jyk7XHJcblx0XHRcdGlmIChtZXNzYWdlKSAkbS5hcHBlbmQoJzxoMj4nK21lc3NhZ2UrJzwvaDI+Jyk7XHJcblx0XHRcdGlmICh0aW1lb3V0ID09PSB1bmRlZmluZWQpIHRpbWVvdXQgPSAzMDAwO1xyXG5cclxuXHRcdFx0Ly8gQWRkZWQgYnkga29uYXB1bjogU2V0IHRpbWVvdXQgdG8gMzAgc2Vjb25kcyBpZiB0aGlzIGdyb3dsIGlzIG1vdXNlZCBvdmVyLCBsaWtlIG5vcm1hbCB0b2FzdCBub3RpZmljYXRpb25zXHJcblx0XHRcdHZhciBjYWxsQmxvY2sgPSBmdW5jdGlvbihvcHRzKSB7XHJcblx0XHRcdFx0b3B0cyA9IG9wdHMgfHwge307XHJcblxyXG5cdFx0XHRcdCQuYmxvY2tVSSh7XHJcblx0XHRcdFx0XHRtZXNzYWdlOiAkbSxcclxuXHRcdFx0XHRcdGZhZGVJbiA6IHR5cGVvZiBvcHRzLmZhZGVJbiAgIT09ICd1bmRlZmluZWQnID8gb3B0cy5mYWRlSW4gIDogNzAwLFxyXG5cdFx0XHRcdFx0ZmFkZU91dDogdHlwZW9mIG9wdHMuZmFkZU91dCAhPT0gJ3VuZGVmaW5lZCcgPyBvcHRzLmZhZGVPdXQgOiAxMDAwLFxyXG5cdFx0XHRcdFx0dGltZW91dDogdHlwZW9mIG9wdHMudGltZW91dCAhPT0gJ3VuZGVmaW5lZCcgPyBvcHRzLnRpbWVvdXQgOiB0aW1lb3V0LFxyXG5cdFx0XHRcdFx0Y2VudGVyWTogZmFsc2UsXHJcblx0XHRcdFx0XHRzaG93T3ZlcmxheTogZmFsc2UsXHJcblx0XHRcdFx0XHRvblVuYmxvY2s6IG9uQ2xvc2UsXHJcblx0XHRcdFx0XHRjc3M6ICQuYmxvY2tVSS5kZWZhdWx0cy5ncm93bENTU1xyXG5cdFx0XHRcdH0pO1xyXG5cdFx0XHR9O1xyXG5cclxuXHRcdFx0Y2FsbEJsb2NrKCk7XHJcblx0XHRcdHZhciBub25tb3VzZWRPcGFjaXR5ID0gJG0uY3NzKCdvcGFjaXR5Jyk7XHJcblx0XHRcdCRtLm1vdXNlb3ZlcihmdW5jdGlvbigpIHtcclxuXHRcdFx0XHRjYWxsQmxvY2soe1xyXG5cdFx0XHRcdFx0ZmFkZUluOiAwLFxyXG5cdFx0XHRcdFx0dGltZW91dDogMzAwMDBcclxuXHRcdFx0XHR9KTtcclxuXHJcblx0XHRcdFx0dmFyIGRpc3BsYXlCbG9jayA9ICQoJy5ibG9ja01zZycpO1xyXG5cdFx0XHRcdGRpc3BsYXlCbG9jay5zdG9wKCk7IC8vIGNhbmNlbCBmYWRlb3V0IGlmIGl0IGhhcyBzdGFydGVkXHJcblx0XHRcdFx0ZGlzcGxheUJsb2NrLmZhZGVUbygzMDAsIDEpOyAvLyBtYWtlIGl0IGVhc2llciB0byByZWFkIHRoZSBtZXNzYWdlIGJ5IHJlbW92aW5nIHRyYW5zcGFyZW5jeVxyXG5cdFx0XHR9KS5tb3VzZW91dChmdW5jdGlvbigpIHtcclxuXHRcdFx0XHQkKCcuYmxvY2tNc2cnKS5mYWRlT3V0KDEwMDApO1xyXG5cdFx0XHR9KTtcclxuXHRcdFx0Ly8gRW5kIGtvbmFwdW4gYWRkaXRpb25zXHJcblx0XHR9O1xyXG5cclxuXHRcdC8vIHBsdWdpbiBtZXRob2QgZm9yIGJsb2NraW5nIGVsZW1lbnQgY29udGVudFxyXG5cdFx0JC5mbi5ibG9jayA9IGZ1bmN0aW9uKG9wdHMpIHtcclxuXHRcdFx0aWYgKCB0aGlzWzBdID09PSB3aW5kb3cgKSB7XHJcblx0XHRcdFx0JC5ibG9ja1VJKCBvcHRzICk7XHJcblx0XHRcdFx0cmV0dXJuIHRoaXM7XHJcblx0XHRcdH1cclxuXHRcdFx0dmFyIGZ1bGxPcHRzID0gJC5leHRlbmQoe30sICQuYmxvY2tVSS5kZWZhdWx0cywgb3B0cyB8fCB7fSk7XHJcblx0XHRcdHRoaXMuZWFjaChmdW5jdGlvbigpIHtcclxuXHRcdFx0XHR2YXIgJGVsID0gJCh0aGlzKTtcclxuXHRcdFx0XHRpZiAoZnVsbE9wdHMuaWdub3JlSWZCbG9ja2VkICYmICRlbC5kYXRhKCdibG9ja1VJLmlzQmxvY2tlZCcpKVxyXG5cdFx0XHRcdFx0cmV0dXJuO1xyXG5cdFx0XHRcdCRlbC51bmJsb2NrKHsgZmFkZU91dDogMCB9KTtcclxuXHRcdFx0fSk7XHJcblxyXG5cdFx0XHRyZXR1cm4gdGhpcy5lYWNoKGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRcdGlmICgkLmNzcyh0aGlzLCdwb3NpdGlvbicpID09ICdzdGF0aWMnKSB7XHJcblx0XHRcdFx0XHR0aGlzLnN0eWxlLnBvc2l0aW9uID0gJ3JlbGF0aXZlJztcclxuXHRcdFx0XHRcdCQodGhpcykuZGF0YSgnYmxvY2tVSS5zdGF0aWMnLCB0cnVlKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0dGhpcy5zdHlsZS56b29tID0gMTsgLy8gZm9yY2UgJ2hhc0xheW91dCcgaW4gaWVcclxuXHRcdFx0XHRpbnN0YWxsKHRoaXMsIG9wdHMpO1xyXG5cdFx0XHR9KTtcclxuXHRcdH07XHJcblxyXG5cdFx0Ly8gcGx1Z2luIG1ldGhvZCBmb3IgdW5ibG9ja2luZyBlbGVtZW50IGNvbnRlbnRcclxuXHRcdCQuZm4udW5ibG9jayA9IGZ1bmN0aW9uKG9wdHMpIHtcclxuXHRcdFx0aWYgKCB0aGlzWzBdID09PSB3aW5kb3cgKSB7XHJcblx0XHRcdFx0JC51bmJsb2NrVUkoIG9wdHMgKTtcclxuXHRcdFx0XHRyZXR1cm4gdGhpcztcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdGhpcy5lYWNoKGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRcdHJlbW92ZSh0aGlzLCBvcHRzKTtcclxuXHRcdFx0fSk7XHJcblx0XHR9O1xyXG5cclxuXHRcdCQuYmxvY2tVSS52ZXJzaW9uID0gMi42NjsgLy8gMm5kIGdlbmVyYXRpb24gYmxvY2tpbmcgYXQgbm8gZXh0cmEgY29zdCFcclxuXHJcblx0XHQvLyBvdmVycmlkZSB0aGVzZSBpbiB5b3VyIGNvZGUgdG8gY2hhbmdlIHRoZSBkZWZhdWx0IGJlaGF2aW9yIGFuZCBzdHlsZVxyXG5cdFx0JC5ibG9ja1VJLmRlZmF1bHRzID0ge1xyXG5cdFx0XHQvLyBtZXNzYWdlIGRpc3BsYXllZCB3aGVuIGJsb2NraW5nICh1c2UgbnVsbCBmb3Igbm8gbWVzc2FnZSlcclxuXHRcdFx0bWVzc2FnZTogICc8aDE+UGxlYXNlIHdhaXQuLi48L2gxPicsXHJcblxyXG5cdFx0XHR0aXRsZTogbnVsbCxcdFx0Ly8gdGl0bGUgc3RyaW5nOyBvbmx5IHVzZWQgd2hlbiB0aGVtZSA9PSB0cnVlXHJcblx0XHRcdGRyYWdnYWJsZTogdHJ1ZSxcdC8vIG9ubHkgdXNlZCB3aGVuIHRoZW1lID09IHRydWUgKHJlcXVpcmVzIGpxdWVyeS11aS5qcyB0byBiZSBsb2FkZWQpXHJcblxyXG5cdFx0XHR0aGVtZTogZmFsc2UsIC8vIHNldCB0byB0cnVlIHRvIHVzZSB3aXRoIGpRdWVyeSBVSSB0aGVtZXNcclxuXHJcblx0XHRcdC8vIHN0eWxlcyBmb3IgdGhlIG1lc3NhZ2Ugd2hlbiBibG9ja2luZzsgaWYgeW91IHdpc2ggdG8gZGlzYWJsZVxyXG5cdFx0XHQvLyB0aGVzZSBhbmQgdXNlIGFuIGV4dGVybmFsIHN0eWxlc2hlZXQgdGhlbiBkbyB0aGlzIGluIHlvdXIgY29kZTpcclxuXHRcdFx0Ly8gJC5ibG9ja1VJLmRlZmF1bHRzLmNzcyA9IHt9O1xyXG5cdFx0XHRjc3M6IHtcclxuXHRcdFx0XHRwYWRkaW5nOlx0MCxcclxuXHRcdFx0XHRtYXJnaW46XHRcdDAsXHJcblx0XHRcdFx0d2lkdGg6XHRcdCczMCUnLFxyXG5cdFx0XHRcdHRvcDpcdFx0JzQwJScsXHJcblx0XHRcdFx0bGVmdDpcdFx0JzM1JScsXHJcblx0XHRcdFx0dGV4dEFsaWduOlx0J2NlbnRlcicsXHJcblx0XHRcdFx0Y29sb3I6XHRcdCcjMDAwJyxcclxuXHRcdFx0XHRib3JkZXI6XHRcdCczcHggc29saWQgI2FhYScsXHJcblx0XHRcdFx0YmFja2dyb3VuZENvbG9yOicjZmZmJyxcclxuXHRcdFx0XHRjdXJzb3I6XHRcdCd3YWl0J1xyXG5cdFx0XHR9LFxyXG5cclxuXHRcdFx0Ly8gbWluaW1hbCBzdHlsZSBzZXQgdXNlZCB3aGVuIHRoZW1lcyBhcmUgdXNlZFxyXG5cdFx0XHR0aGVtZWRDU1M6IHtcclxuXHRcdFx0XHR3aWR0aDpcdCczMCUnLFxyXG5cdFx0XHRcdHRvcDpcdCc0MCUnLFxyXG5cdFx0XHRcdGxlZnQ6XHQnMzUlJ1xyXG5cdFx0XHR9LFxyXG5cclxuXHRcdFx0Ly8gc3R5bGVzIGZvciB0aGUgb3ZlcmxheVxyXG5cdFx0XHRvdmVybGF5Q1NTOiAge1xyXG5cdFx0XHRcdGJhY2tncm91bmRDb2xvcjpcdCcjMDAwJyxcclxuXHRcdFx0XHRvcGFjaXR5Olx0XHRcdDAuNixcclxuXHRcdFx0XHRjdXJzb3I6XHRcdFx0XHQnd2FpdCdcclxuXHRcdFx0fSxcclxuXHJcblx0XHRcdC8vIHN0eWxlIHRvIHJlcGxhY2Ugd2FpdCBjdXJzb3IgYmVmb3JlIHVuYmxvY2tpbmcgdG8gY29ycmVjdCBpc3N1ZVxyXG5cdFx0XHQvLyBvZiBsaW5nZXJpbmcgd2FpdCBjdXJzb3JcclxuXHRcdFx0Y3Vyc29yUmVzZXQ6ICdkZWZhdWx0JyxcclxuXHJcblx0XHRcdC8vIHN0eWxlcyBhcHBsaWVkIHdoZW4gdXNpbmcgJC5ncm93bFVJXHJcblx0XHRcdGdyb3dsQ1NTOiB7XHJcblx0XHRcdFx0d2lkdGg6XHRcdCczNTBweCcsXHJcblx0XHRcdFx0dG9wOlx0XHQnMTBweCcsXHJcblx0XHRcdFx0bGVmdDpcdFx0JycsXHJcblx0XHRcdFx0cmlnaHQ6XHRcdCcxMHB4JyxcclxuXHRcdFx0XHRib3JkZXI6XHRcdCdub25lJyxcclxuXHRcdFx0XHRwYWRkaW5nOlx0JzVweCcsXHJcblx0XHRcdFx0b3BhY2l0eTpcdDAuNixcclxuXHRcdFx0XHRjdXJzb3I6XHRcdCdkZWZhdWx0JyxcclxuXHRcdFx0XHRjb2xvcjpcdFx0JyNmZmYnLFxyXG5cdFx0XHRcdGJhY2tncm91bmRDb2xvcjogJyMwMDAnLFxyXG5cdFx0XHRcdCctd2Via2l0LWJvcmRlci1yYWRpdXMnOicxMHB4JyxcclxuXHRcdFx0XHQnLW1vei1ib3JkZXItcmFkaXVzJzpcdCcxMHB4JyxcclxuXHRcdFx0XHQnYm9yZGVyLXJhZGl1cyc6XHRcdCcxMHB4J1xyXG5cdFx0XHR9LFxyXG5cclxuXHRcdFx0Ly8gSUUgaXNzdWVzOiAnYWJvdXQ6YmxhbmsnIGZhaWxzIG9uIEhUVFBTIGFuZCBqYXZhc2NyaXB0OmZhbHNlIGlzIHMtbC1vLXdcclxuXHRcdFx0Ly8gKGhhdCB0aXAgdG8gSm9yZ2UgSC4gTi4gZGUgVmFzY29uY2Vsb3MpXHJcblx0XHRcdC8qanNoaW50IHNjcmlwdHVybDp0cnVlICovXHJcblx0XHRcdGlmcmFtZVNyYzogL15odHRwcy9pLnRlc3Qod2luZG93LmxvY2F0aW9uLmhyZWYgfHwgJycpID8gJ2phdmFzY3JpcHQ6ZmFsc2UnIDogJ2Fib3V0OmJsYW5rJyxcclxuXHJcblx0XHRcdC8vIGZvcmNlIHVzYWdlIG9mIGlmcmFtZSBpbiBub24tSUUgYnJvd3NlcnMgKGhhbmR5IGZvciBibG9ja2luZyBhcHBsZXRzKVxyXG5cdFx0XHRmb3JjZUlmcmFtZTogZmFsc2UsXHJcblxyXG5cdFx0XHQvLyB6LWluZGV4IGZvciB0aGUgYmxvY2tpbmcgb3ZlcmxheVxyXG5cdFx0XHRiYXNlWjogMTAwMCxcclxuXHJcblx0XHRcdC8vIHNldCB0aGVzZSB0byB0cnVlIHRvIGhhdmUgdGhlIG1lc3NhZ2UgYXV0b21hdGljYWxseSBjZW50ZXJlZFxyXG5cdFx0XHRjZW50ZXJYOiB0cnVlLCAvLyA8LS0gb25seSBlZmZlY3RzIGVsZW1lbnQgYmxvY2tpbmcgKHBhZ2UgYmxvY2sgY29udHJvbGxlZCB2aWEgY3NzIGFib3ZlKVxyXG5cdFx0XHRjZW50ZXJZOiB0cnVlLFxyXG5cclxuXHRcdFx0Ly8gYWxsb3cgYm9keSBlbGVtZW50IHRvIGJlIHN0ZXRjaGVkIGluIGllNjsgdGhpcyBtYWtlcyBibG9ja2luZyBsb29rIGJldHRlclxyXG5cdFx0XHQvLyBvbiBcInNob3J0XCIgcGFnZXMuICBkaXNhYmxlIGlmIHlvdSB3aXNoIHRvIHByZXZlbnQgY2hhbmdlcyB0byB0aGUgYm9keSBoZWlnaHRcclxuXHRcdFx0YWxsb3dCb2R5U3RyZXRjaDogdHJ1ZSxcclxuXHJcblx0XHRcdC8vIGVuYWJsZSBpZiB5b3Ugd2FudCBrZXkgYW5kIG1vdXNlIGV2ZW50cyB0byBiZSBkaXNhYmxlZCBmb3IgY29udGVudCB0aGF0IGlzIGJsb2NrZWRcclxuXHRcdFx0YmluZEV2ZW50czogdHJ1ZSxcclxuXHJcblx0XHRcdC8vIGJlIGRlZmF1bHQgYmxvY2tVSSB3aWxsIHN1cHJlc3MgdGFiIG5hdmlnYXRpb24gZnJvbSBsZWF2aW5nIGJsb2NraW5nIGNvbnRlbnRcclxuXHRcdFx0Ly8gKGlmIGJpbmRFdmVudHMgaXMgdHJ1ZSlcclxuXHRcdFx0Y29uc3RyYWluVGFiS2V5OiB0cnVlLFxyXG5cclxuXHRcdFx0Ly8gZmFkZUluIHRpbWUgaW4gbWlsbGlzOyBzZXQgdG8gMCB0byBkaXNhYmxlIGZhZGVJbiBvbiBibG9ja1xyXG5cdFx0XHRmYWRlSW46ICAyMDAsXHJcblxyXG5cdFx0XHQvLyBmYWRlT3V0IHRpbWUgaW4gbWlsbGlzOyBzZXQgdG8gMCB0byBkaXNhYmxlIGZhZGVPdXQgb24gdW5ibG9ja1xyXG5cdFx0XHRmYWRlT3V0OiAgNDAwLFxyXG5cclxuXHRcdFx0Ly8gdGltZSBpbiBtaWxsaXMgdG8gd2FpdCBiZWZvcmUgYXV0by11bmJsb2NraW5nOyBzZXQgdG8gMCB0byBkaXNhYmxlIGF1dG8tdW5ibG9ja1xyXG5cdFx0XHR0aW1lb3V0OiAwLFxyXG5cclxuXHRcdFx0Ly8gZGlzYWJsZSBpZiB5b3UgZG9uJ3Qgd2FudCB0byBzaG93IHRoZSBvdmVybGF5XHJcblx0XHRcdHNob3dPdmVybGF5OiB0cnVlLFxyXG5cclxuXHRcdFx0Ly8gaWYgdHJ1ZSwgZm9jdXMgd2lsbCBiZSBwbGFjZWQgaW4gdGhlIGZpcnN0IGF2YWlsYWJsZSBpbnB1dCBmaWVsZCB3aGVuXHJcblx0XHRcdC8vIHBhZ2UgYmxvY2tpbmdcclxuXHRcdFx0Zm9jdXNJbnB1dDogdHJ1ZSxcclxuXHJcbiAgICAgICAgICAgIC8vIGVsZW1lbnRzIHRoYXQgY2FuIHJlY2VpdmUgZm9jdXNcclxuICAgICAgICAgICAgZm9jdXNhYmxlRWxlbWVudHM6ICc6aW5wdXQ6ZW5hYmxlZDp2aXNpYmxlJyxcclxuXHJcblx0XHRcdC8vIHN1cHByZXNzZXMgdGhlIHVzZSBvZiBvdmVybGF5IHN0eWxlcyBvbiBGRi9MaW51eCAoZHVlIHRvIHBlcmZvcm1hbmNlIGlzc3VlcyB3aXRoIG9wYWNpdHkpXHJcblx0XHRcdC8vIG5vIGxvbmdlciBuZWVkZWQgaW4gMjAxMlxyXG5cdFx0XHQvLyBhcHBseVBsYXRmb3JtT3BhY2l0eVJ1bGVzOiB0cnVlLFxyXG5cclxuXHRcdFx0Ly8gY2FsbGJhY2sgbWV0aG9kIGludm9rZWQgd2hlbiBmYWRlSW4gaGFzIGNvbXBsZXRlZCBhbmQgYmxvY2tpbmcgbWVzc2FnZSBpcyB2aXNpYmxlXHJcblx0XHRcdG9uQmxvY2s6IG51bGwsXHJcblxyXG5cdFx0XHQvLyBjYWxsYmFjayBtZXRob2QgaW52b2tlZCB3aGVuIHVuYmxvY2tpbmcgaGFzIGNvbXBsZXRlZDsgdGhlIGNhbGxiYWNrIGlzXHJcblx0XHRcdC8vIHBhc3NlZCB0aGUgZWxlbWVudCB0aGF0IGhhcyBiZWVuIHVuYmxvY2tlZCAod2hpY2ggaXMgdGhlIHdpbmRvdyBvYmplY3QgZm9yIHBhZ2VcclxuXHRcdFx0Ly8gYmxvY2tzKSBhbmQgdGhlIG9wdGlvbnMgdGhhdCB3ZXJlIHBhc3NlZCB0byB0aGUgdW5ibG9jayBjYWxsOlxyXG5cdFx0XHQvL1x0b25VbmJsb2NrKGVsZW1lbnQsIG9wdGlvbnMpXHJcblx0XHRcdG9uVW5ibG9jazogbnVsbCxcclxuXHJcblx0XHRcdC8vIGNhbGxiYWNrIG1ldGhvZCBpbnZva2VkIHdoZW4gdGhlIG92ZXJsYXkgYXJlYSBpcyBjbGlja2VkLlxyXG5cdFx0XHQvLyBzZXR0aW5nIHRoaXMgd2lsbCB0dXJuIHRoZSBjdXJzb3IgdG8gYSBwb2ludGVyLCBvdGhlcndpc2UgY3Vyc29yIGRlZmluZWQgaW4gb3ZlcmxheUNzcyB3aWxsIGJlIHVzZWQuXHJcblx0XHRcdG9uT3ZlcmxheUNsaWNrOiBudWxsLFxyXG5cclxuXHRcdFx0Ly8gZG9uJ3QgYXNrOyBpZiB5b3UgcmVhbGx5IG11c3Qga25vdzogaHR0cDovL2dyb3Vwcy5nb29nbGUuY29tL3JlcXVpcmVkVXBsb2Fkcy9qcXVlcnktZW4vYnJvd3NlX3RocmVhZC90aHJlYWQvMzY2NDBhODczMDUwMzU5NS8yZjZhNzlhNzdhNzhlNDkzIzJmNmE3OWE3N2E3OGU0OTNcclxuXHRcdFx0cXVpcmtzbW9kZU9mZnNldEhhY2s6IDQsXHJcblxyXG5cdFx0XHQvLyBjbGFzcyBuYW1lIG9mIHRoZSBtZXNzYWdlIGJsb2NrXHJcblx0XHRcdGJsb2NrTXNnQ2xhc3M6ICdibG9ja01zZycsXHJcblxyXG5cdFx0XHQvLyBpZiBpdCBpcyBhbHJlYWR5IGJsb2NrZWQsIHRoZW4gaWdub3JlIGl0IChkb24ndCB1bmJsb2NrIGFuZCByZWJsb2NrKVxyXG5cdFx0XHRpZ25vcmVJZkJsb2NrZWQ6IGZhbHNlXHJcblx0XHR9O1xyXG5cclxuXHRcdC8vIHByaXZhdGUgZGF0YSBhbmQgZnVuY3Rpb25zIGZvbGxvdy4uLlxyXG5cclxuXHRcdHZhciBwYWdlQmxvY2sgPSBudWxsO1xyXG5cdFx0dmFyIHBhZ2VCbG9ja0VscyA9IFtdO1xyXG5cclxuXHRcdGZ1bmN0aW9uIGluc3RhbGwoZWwsIG9wdHMpIHtcclxuXHRcdFx0dmFyIGNzcywgdGhlbWVkQ1NTO1xyXG5cdFx0XHR2YXIgZnVsbCA9IChlbCA9PSB3aW5kb3cpO1xyXG5cdFx0XHR2YXIgbXNnID0gKG9wdHMgJiYgb3B0cy5tZXNzYWdlICE9PSB1bmRlZmluZWQgPyBvcHRzLm1lc3NhZ2UgOiB1bmRlZmluZWQpO1xyXG5cdFx0XHRvcHRzID0gJC5leHRlbmQoe30sICQuYmxvY2tVSS5kZWZhdWx0cywgb3B0cyB8fCB7fSk7XHJcblxyXG5cdFx0XHRpZiAob3B0cy5pZ25vcmVJZkJsb2NrZWQgJiYgJChlbCkuZGF0YSgnYmxvY2tVSS5pc0Jsb2NrZWQnKSlcclxuXHRcdFx0XHRyZXR1cm47XHJcblxyXG5cdFx0XHRvcHRzLm92ZXJsYXlDU1MgPSAkLmV4dGVuZCh7fSwgJC5ibG9ja1VJLmRlZmF1bHRzLm92ZXJsYXlDU1MsIG9wdHMub3ZlcmxheUNTUyB8fCB7fSk7XHJcblx0XHRcdGNzcyA9ICQuZXh0ZW5kKHt9LCAkLmJsb2NrVUkuZGVmYXVsdHMuY3NzLCBvcHRzLmNzcyB8fCB7fSk7XHJcblx0XHRcdGlmIChvcHRzLm9uT3ZlcmxheUNsaWNrKVxyXG5cdFx0XHRcdG9wdHMub3ZlcmxheUNTUy5jdXJzb3IgPSAncG9pbnRlcic7XHJcblxyXG5cdFx0XHR0aGVtZWRDU1MgPSAkLmV4dGVuZCh7fSwgJC5ibG9ja1VJLmRlZmF1bHRzLnRoZW1lZENTUywgb3B0cy50aGVtZWRDU1MgfHwge30pO1xyXG5cdFx0XHRtc2cgPSBtc2cgPT09IHVuZGVmaW5lZCA/IG9wdHMubWVzc2FnZSA6IG1zZztcclxuXHJcblx0XHRcdC8vIHJlbW92ZSB0aGUgY3VycmVudCBibG9jayAoaWYgdGhlcmUgaXMgb25lKVxyXG5cdFx0XHRpZiAoZnVsbCAmJiBwYWdlQmxvY2spXHJcblx0XHRcdFx0cmVtb3ZlKHdpbmRvdywge2ZhZGVPdXQ6MH0pO1xyXG5cclxuXHRcdFx0Ly8gaWYgYW4gZXhpc3RpbmcgZWxlbWVudCBpcyBiZWluZyB1c2VkIGFzIHRoZSBibG9ja2luZyBjb250ZW50IHRoZW4gd2UgY2FwdHVyZVxyXG5cdFx0XHQvLyBpdHMgY3VycmVudCBwbGFjZSBpbiB0aGUgRE9NIChhbmQgY3VycmVudCBkaXNwbGF5IHN0eWxlKSBzbyB3ZSBjYW4gcmVzdG9yZVxyXG5cdFx0XHQvLyBpdCB3aGVuIHdlIHVuYmxvY2tcclxuXHRcdFx0aWYgKG1zZyAmJiB0eXBlb2YgbXNnICE9ICdzdHJpbmcnICYmIChtc2cucGFyZW50Tm9kZSB8fCBtc2cuanF1ZXJ5KSkge1xyXG5cdFx0XHRcdHZhciBub2RlID0gbXNnLmpxdWVyeSA/IG1zZ1swXSA6IG1zZztcclxuXHRcdFx0XHR2YXIgZGF0YSA9IHt9O1xyXG5cdFx0XHRcdCQoZWwpLmRhdGEoJ2Jsb2NrVUkuaGlzdG9yeScsIGRhdGEpO1xyXG5cdFx0XHRcdGRhdGEuZWwgPSBub2RlO1xyXG5cdFx0XHRcdGRhdGEucGFyZW50ID0gbm9kZS5wYXJlbnROb2RlO1xyXG5cdFx0XHRcdGRhdGEuZGlzcGxheSA9IG5vZGUuc3R5bGUuZGlzcGxheTtcclxuXHRcdFx0XHRkYXRhLnBvc2l0aW9uID0gbm9kZS5zdHlsZS5wb3NpdGlvbjtcclxuXHRcdFx0XHRpZiAoZGF0YS5wYXJlbnQpXHJcblx0XHRcdFx0XHRkYXRhLnBhcmVudC5yZW1vdmVDaGlsZChub2RlKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0JChlbCkuZGF0YSgnYmxvY2tVSS5vblVuYmxvY2snLCBvcHRzLm9uVW5ibG9jayk7XHJcblx0XHRcdHZhciB6ID0gb3B0cy5iYXNlWjtcclxuXHJcblx0XHRcdC8vIGJsb2NrVUkgdXNlcyAzIGxheWVycyBmb3IgYmxvY2tpbmcsIGZvciBzaW1wbGljaXR5IHRoZXkgYXJlIGFsbCB1c2VkIG9uIGV2ZXJ5IHBsYXRmb3JtO1xyXG5cdFx0XHQvLyBsYXllcjEgaXMgdGhlIGlmcmFtZSBsYXllciB3aGljaCBpcyB1c2VkIHRvIHN1cHJlc3MgYmxlZWQgdGhyb3VnaCBvZiB1bmRlcmx5aW5nIGNvbnRlbnRcclxuXHRcdFx0Ly8gbGF5ZXIyIGlzIHRoZSBvdmVybGF5IGxheWVyIHdoaWNoIGhhcyBvcGFjaXR5IGFuZCBhIHdhaXQgY3Vyc29yIChieSBkZWZhdWx0KVxyXG5cdFx0XHQvLyBsYXllcjMgaXMgdGhlIG1lc3NhZ2UgY29udGVudCB0aGF0IGlzIGRpc3BsYXllZCB3aGlsZSBibG9ja2luZ1xyXG5cdFx0XHR2YXIgbHlyMSwgbHlyMiwgbHlyMywgcztcclxuXHRcdFx0aWYgKG1zaWUgfHwgb3B0cy5mb3JjZUlmcmFtZSlcclxuXHRcdFx0XHRseXIxID0gJCgnPGlmcmFtZSBjbGFzcz1cImJsb2NrVUlcIiBzdHlsZT1cInotaW5kZXg6JysgKHorKykgKyc7ZGlzcGxheTpub25lO2JvcmRlcjpub25lO21hcmdpbjowO3BhZGRpbmc6MDtwb3NpdGlvbjphYnNvbHV0ZTt3aWR0aDoxMDAlO2hlaWdodDoxMDAlO3RvcDowO2xlZnQ6MFwiIHNyYz1cIicrb3B0cy5pZnJhbWVTcmMrJ1wiPjwvaWZyYW1lPicpO1xyXG5cdFx0XHRlbHNlXHJcblx0XHRcdFx0bHlyMSA9ICQoJzxkaXYgY2xhc3M9XCJibG9ja1VJXCIgc3R5bGU9XCJkaXNwbGF5Om5vbmVcIj48L2Rpdj4nKTtcclxuXHJcblx0XHRcdGlmIChvcHRzLnRoZW1lKVxyXG5cdFx0XHRcdGx5cjIgPSAkKCc8ZGl2IGNsYXNzPVwiYmxvY2tVSSBibG9ja092ZXJsYXkgdWktd2lkZ2V0LW92ZXJsYXlcIiBzdHlsZT1cInotaW5kZXg6JysgKHorKykgKyc7ZGlzcGxheTpub25lXCI+PC9kaXY+Jyk7XHJcblx0XHRcdGVsc2VcclxuXHRcdFx0XHRseXIyID0gJCgnPGRpdiBjbGFzcz1cImJsb2NrVUkgYmxvY2tPdmVybGF5XCIgc3R5bGU9XCJ6LWluZGV4OicrICh6KyspICsnO2Rpc3BsYXk6bm9uZTtib3JkZXI6bm9uZTttYXJnaW46MDtwYWRkaW5nOjA7d2lkdGg6MTAwJTtoZWlnaHQ6MTAwJTt0b3A6MDtsZWZ0OjBcIj48L2Rpdj4nKTtcclxuXHJcblx0XHRcdGlmIChvcHRzLnRoZW1lICYmIGZ1bGwpIHtcclxuXHRcdFx0XHRzID0gJzxkaXYgY2xhc3M9XCJibG9ja1VJICcgKyBvcHRzLmJsb2NrTXNnQ2xhc3MgKyAnIGJsb2NrUGFnZSB1aS1kaWFsb2cgdWktd2lkZ2V0IHVpLWNvcm5lci1hbGxcIiBzdHlsZT1cInotaW5kZXg6JysoeisxMCkrJztkaXNwbGF5Om5vbmU7cG9zaXRpb246Zml4ZWRcIj4nO1xyXG5cdFx0XHRcdGlmICggb3B0cy50aXRsZSApIHtcclxuXHRcdFx0XHRcdHMgKz0gJzxkaXYgY2xhc3M9XCJ1aS13aWRnZXQtaGVhZGVyIHVpLWRpYWxvZy10aXRsZWJhciB1aS1jb3JuZXItYWxsIGJsb2NrVGl0bGVcIj4nKyhvcHRzLnRpdGxlIHx8ICcmbmJzcDsnKSsnPC9kaXY+JztcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cyArPSAnPGRpdiBjbGFzcz1cInVpLXdpZGdldC1jb250ZW50IHVpLWRpYWxvZy1jb250ZW50XCI+PC9kaXY+JztcclxuXHRcdFx0XHRzICs9ICc8L2Rpdj4nO1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2UgaWYgKG9wdHMudGhlbWUpIHtcclxuXHRcdFx0XHRzID0gJzxkaXYgY2xhc3M9XCJibG9ja1VJICcgKyBvcHRzLmJsb2NrTXNnQ2xhc3MgKyAnIGJsb2NrRWxlbWVudCB1aS1kaWFsb2cgdWktd2lkZ2V0IHVpLWNvcm5lci1hbGxcIiBzdHlsZT1cInotaW5kZXg6JysoeisxMCkrJztkaXNwbGF5Om5vbmU7cG9zaXRpb246YWJzb2x1dGVcIj4nO1xyXG5cdFx0XHRcdGlmICggb3B0cy50aXRsZSApIHtcclxuXHRcdFx0XHRcdHMgKz0gJzxkaXYgY2xhc3M9XCJ1aS13aWRnZXQtaGVhZGVyIHVpLWRpYWxvZy10aXRsZWJhciB1aS1jb3JuZXItYWxsIGJsb2NrVGl0bGVcIj4nKyhvcHRzLnRpdGxlIHx8ICcmbmJzcDsnKSsnPC9kaXY+JztcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cyArPSAnPGRpdiBjbGFzcz1cInVpLXdpZGdldC1jb250ZW50IHVpLWRpYWxvZy1jb250ZW50XCI+PC9kaXY+JztcclxuXHRcdFx0XHRzICs9ICc8L2Rpdj4nO1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2UgaWYgKGZ1bGwpIHtcclxuXHRcdFx0XHRzID0gJzxkaXYgY2xhc3M9XCJibG9ja1VJICcgKyBvcHRzLmJsb2NrTXNnQ2xhc3MgKyAnIGJsb2NrUGFnZVwiIHN0eWxlPVwiei1pbmRleDonKyh6KzEwKSsnO2Rpc3BsYXk6bm9uZTtwb3NpdGlvbjpmaXhlZFwiPjwvZGl2Pic7XHJcblx0XHRcdH1cclxuXHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0cyA9ICc8ZGl2IGNsYXNzPVwiYmxvY2tVSSAnICsgb3B0cy5ibG9ja01zZ0NsYXNzICsgJyBibG9ja0VsZW1lbnRcIiBzdHlsZT1cInotaW5kZXg6JysoeisxMCkrJztkaXNwbGF5Om5vbmU7cG9zaXRpb246YWJzb2x1dGVcIj48L2Rpdj4nO1xyXG5cdFx0XHR9XHJcblx0XHRcdGx5cjMgPSAkKHMpO1xyXG5cclxuXHRcdFx0Ly8gaWYgd2UgaGF2ZSBhIG1lc3NhZ2UsIHN0eWxlIGl0XHJcblx0XHRcdGlmIChtc2cpIHtcclxuXHRcdFx0XHRpZiAob3B0cy50aGVtZSkge1xyXG5cdFx0XHRcdFx0bHlyMy5jc3ModGhlbWVkQ1NTKTtcclxuXHRcdFx0XHRcdGx5cjMuYWRkQ2xhc3MoJ3VpLXdpZGdldC1jb250ZW50Jyk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGVsc2VcclxuXHRcdFx0XHRcdGx5cjMuY3NzKGNzcyk7XHJcblx0XHRcdH1cclxuXHJcblx0XHRcdC8vIHN0eWxlIHRoZSBvdmVybGF5XHJcblx0XHRcdGlmICghb3B0cy50aGVtZSAvKiYmICghb3B0cy5hcHBseVBsYXRmb3JtT3BhY2l0eVJ1bGVzKSovKVxyXG5cdFx0XHRcdGx5cjIuY3NzKG9wdHMub3ZlcmxheUNTUyk7XHJcblx0XHRcdGx5cjIuY3NzKCdwb3NpdGlvbicsIGZ1bGwgPyAnZml4ZWQnIDogJ2Fic29sdXRlJyk7XHJcblxyXG5cdFx0XHQvLyBtYWtlIGlmcmFtZSBsYXllciB0cmFuc3BhcmVudCBpbiBJRVxyXG5cdFx0XHRpZiAobXNpZSB8fCBvcHRzLmZvcmNlSWZyYW1lKVxyXG5cdFx0XHRcdGx5cjEuY3NzKCdvcGFjaXR5JywwLjApO1xyXG5cclxuXHRcdFx0Ly8kKFtseXIxWzBdLGx5cjJbMF0sbHlyM1swXV0pLmFwcGVuZFRvKGZ1bGwgPyAnYm9keScgOiBlbCk7XHJcblx0XHRcdHZhciBsYXllcnMgPSBbbHlyMSxseXIyLGx5cjNdLCAkcGFyID0gZnVsbCA/ICQoJ2JvZHknKSA6ICQoZWwpO1xyXG5cdFx0XHQkLmVhY2gobGF5ZXJzLCBmdW5jdGlvbigpIHtcclxuXHRcdFx0XHR0aGlzLmFwcGVuZFRvKCRwYXIpO1xyXG5cdFx0XHR9KTtcclxuXHJcblx0XHRcdGlmIChvcHRzLnRoZW1lICYmIG9wdHMuZHJhZ2dhYmxlICYmICQuZm4uZHJhZ2dhYmxlKSB7XHJcblx0XHRcdFx0bHlyMy5kcmFnZ2FibGUoe1xyXG5cdFx0XHRcdFx0aGFuZGxlOiAnLnVpLWRpYWxvZy10aXRsZWJhcicsXHJcblx0XHRcdFx0XHRjYW5jZWw6ICdsaSdcclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0Ly8gaWU3IG11c3QgdXNlIGFic29sdXRlIHBvc2l0aW9uaW5nIGluIHF1aXJrcyBtb2RlIGFuZCB0byBhY2NvdW50IGZvciBhY3RpdmV4IGlzc3VlcyAod2hlbiBzY3JvbGxpbmcpXHJcblx0XHRcdHZhciBleHByID0gc2V0RXhwciAmJiAoISQuc3VwcG9ydC5ib3hNb2RlbCB8fCAkKCdvYmplY3QsZW1iZWQnLCBmdWxsID8gbnVsbCA6IGVsKS5sZW5ndGggPiAwKTtcclxuXHRcdFx0aWYgKGllNiB8fCBleHByKSB7XHJcblx0XHRcdFx0Ly8gZ2l2ZSBib2R5IDEwMCUgaGVpZ2h0XHJcblx0XHRcdFx0aWYgKGZ1bGwgJiYgb3B0cy5hbGxvd0JvZHlTdHJldGNoICYmICQuc3VwcG9ydC5ib3hNb2RlbClcclxuXHRcdFx0XHRcdCQoJ2h0bWwsYm9keScpLmNzcygnaGVpZ2h0JywnMTAwJScpO1xyXG5cclxuXHRcdFx0XHQvLyBmaXggaWU2IGlzc3VlIHdoZW4gYmxvY2tlZCBlbGVtZW50IGhhcyBhIGJvcmRlciB3aWR0aFxyXG5cdFx0XHRcdGlmICgoaWU2IHx8ICEkLnN1cHBvcnQuYm94TW9kZWwpICYmICFmdWxsKSB7XHJcblx0XHRcdFx0XHR2YXIgdCA9IHN6KGVsLCdib3JkZXJUb3BXaWR0aCcpLCBsID0gc3ooZWwsJ2JvcmRlckxlZnRXaWR0aCcpO1xyXG5cdFx0XHRcdFx0dmFyIGZpeFQgPSB0ID8gJygwIC0gJyt0KycpJyA6IDA7XHJcblx0XHRcdFx0XHR2YXIgZml4TCA9IGwgPyAnKDAgLSAnK2wrJyknIDogMDtcclxuXHRcdFx0XHR9XHJcblxyXG5cdFx0XHRcdC8vIHNpbXVsYXRlIGZpeGVkIHBvc2l0aW9uXHJcblx0XHRcdFx0JC5lYWNoKGxheWVycywgZnVuY3Rpb24oaSxvKSB7XHJcblx0XHRcdFx0XHR2YXIgcyA9IG9bMF0uc3R5bGU7XHJcblx0XHRcdFx0XHRzLnBvc2l0aW9uID0gJ2Fic29sdXRlJztcclxuXHRcdFx0XHRcdGlmIChpIDwgMikge1xyXG5cdFx0XHRcdFx0XHRpZiAoZnVsbClcclxuXHRcdFx0XHRcdFx0XHRzLnNldEV4cHJlc3Npb24oJ2hlaWdodCcsJ01hdGgubWF4KGRvY3VtZW50LmJvZHkuc2Nyb2xsSGVpZ2h0LCBkb2N1bWVudC5ib2R5Lm9mZnNldEhlaWdodCkgLSAoalF1ZXJ5LnN1cHBvcnQuYm94TW9kZWw/MDonK29wdHMucXVpcmtzbW9kZU9mZnNldEhhY2srJykgKyBcInB4XCInKTtcclxuXHRcdFx0XHRcdFx0ZWxzZVxyXG5cdFx0XHRcdFx0XHRcdHMuc2V0RXhwcmVzc2lvbignaGVpZ2h0JywndGhpcy5wYXJlbnROb2RlLm9mZnNldEhlaWdodCArIFwicHhcIicpO1xyXG5cdFx0XHRcdFx0XHRpZiAoZnVsbClcclxuXHRcdFx0XHRcdFx0XHRzLnNldEV4cHJlc3Npb24oJ3dpZHRoJywnalF1ZXJ5LnN1cHBvcnQuYm94TW9kZWwgJiYgZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LmNsaWVudFdpZHRoIHx8IGRvY3VtZW50LmJvZHkuY2xpZW50V2lkdGggKyBcInB4XCInKTtcclxuXHRcdFx0XHRcdFx0ZWxzZVxyXG5cdFx0XHRcdFx0XHRcdHMuc2V0RXhwcmVzc2lvbignd2lkdGgnLCd0aGlzLnBhcmVudE5vZGUub2Zmc2V0V2lkdGggKyBcInB4XCInKTtcclxuXHRcdFx0XHRcdFx0aWYgKGZpeEwpIHMuc2V0RXhwcmVzc2lvbignbGVmdCcsIGZpeEwpO1xyXG5cdFx0XHRcdFx0XHRpZiAoZml4VCkgcy5zZXRFeHByZXNzaW9uKCd0b3AnLCBmaXhUKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGVsc2UgaWYgKG9wdHMuY2VudGVyWSkge1xyXG5cdFx0XHRcdFx0XHRpZiAoZnVsbCkgcy5zZXRFeHByZXNzaW9uKCd0b3AnLCcoZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LmNsaWVudEhlaWdodCB8fCBkb2N1bWVudC5ib2R5LmNsaWVudEhlaWdodCkgLyAyIC0gKHRoaXMub2Zmc2V0SGVpZ2h0IC8gMikgKyAoYmxhaCA9IGRvY3VtZW50LmRvY3VtZW50RWxlbWVudC5zY3JvbGxUb3AgPyBkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuc2Nyb2xsVG9wIDogZG9jdW1lbnQuYm9keS5zY3JvbGxUb3ApICsgXCJweFwiJyk7XHJcblx0XHRcdFx0XHRcdHMubWFyZ2luVG9wID0gMDtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGVsc2UgaWYgKCFvcHRzLmNlbnRlclkgJiYgZnVsbCkge1xyXG5cdFx0XHRcdFx0XHR2YXIgdG9wID0gKG9wdHMuY3NzICYmIG9wdHMuY3NzLnRvcCkgPyBwYXJzZUludChvcHRzLmNzcy50b3AsIDEwKSA6IDA7XHJcblx0XHRcdFx0XHRcdHZhciBleHByZXNzaW9uID0gJygoZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LnNjcm9sbFRvcCA/IGRvY3VtZW50LmRvY3VtZW50RWxlbWVudC5zY3JvbGxUb3AgOiBkb2N1bWVudC5ib2R5LnNjcm9sbFRvcCkgKyAnK3RvcCsnKSArIFwicHhcIic7XHJcblx0XHRcdFx0XHRcdHMuc2V0RXhwcmVzc2lvbigndG9wJyxleHByZXNzaW9uKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0Ly8gc2hvdyB0aGUgbWVzc2FnZVxyXG5cdFx0XHRpZiAobXNnKSB7XHJcblx0XHRcdFx0aWYgKG9wdHMudGhlbWUpXHJcblx0XHRcdFx0XHRseXIzLmZpbmQoJy51aS13aWRnZXQtY29udGVudCcpLmFwcGVuZChtc2cpO1xyXG5cdFx0XHRcdGVsc2VcclxuXHRcdFx0XHRcdGx5cjMuYXBwZW5kKG1zZyk7XHJcblx0XHRcdFx0aWYgKG1zZy5qcXVlcnkgfHwgbXNnLm5vZGVUeXBlKVxyXG5cdFx0XHRcdFx0JChtc2cpLnNob3coKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0aWYgKChtc2llIHx8IG9wdHMuZm9yY2VJZnJhbWUpICYmIG9wdHMuc2hvd092ZXJsYXkpXHJcblx0XHRcdFx0bHlyMS5zaG93KCk7IC8vIG9wYWNpdHkgaXMgemVyb1xyXG5cdFx0XHRpZiAob3B0cy5mYWRlSW4pIHtcclxuXHRcdFx0XHR2YXIgY2IgPSBvcHRzLm9uQmxvY2sgPyBvcHRzLm9uQmxvY2sgOiBub09wO1xyXG5cdFx0XHRcdHZhciBjYjEgPSAob3B0cy5zaG93T3ZlcmxheSAmJiAhbXNnKSA/IGNiIDogbm9PcDtcclxuXHRcdFx0XHR2YXIgY2IyID0gbXNnID8gY2IgOiBub09wO1xyXG5cdFx0XHRcdGlmIChvcHRzLnNob3dPdmVybGF5KVxyXG5cdFx0XHRcdFx0bHlyMi5fZmFkZUluKG9wdHMuZmFkZUluLCBjYjEpO1xyXG5cdFx0XHRcdGlmIChtc2cpXHJcblx0XHRcdFx0XHRseXIzLl9mYWRlSW4ob3B0cy5mYWRlSW4sIGNiMik7XHJcblx0XHRcdH1cclxuXHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0aWYgKG9wdHMuc2hvd092ZXJsYXkpXHJcblx0XHRcdFx0XHRseXIyLnNob3coKTtcclxuXHRcdFx0XHRpZiAobXNnKVxyXG5cdFx0XHRcdFx0bHlyMy5zaG93KCk7XHJcblx0XHRcdFx0aWYgKG9wdHMub25CbG9jaylcclxuXHRcdFx0XHRcdG9wdHMub25CbG9jaygpO1xyXG5cdFx0XHR9XHJcblxyXG5cdFx0XHQvLyBiaW5kIGtleSBhbmQgbW91c2UgZXZlbnRzXHJcblx0XHRcdGJpbmQoMSwgZWwsIG9wdHMpO1xyXG5cclxuXHRcdFx0aWYgKGZ1bGwpIHtcclxuXHRcdFx0XHRwYWdlQmxvY2sgPSBseXIzWzBdO1xyXG5cdFx0XHRcdHBhZ2VCbG9ja0VscyA9ICQob3B0cy5mb2N1c2FibGVFbGVtZW50cyxwYWdlQmxvY2spO1xyXG5cdFx0XHRcdGlmIChvcHRzLmZvY3VzSW5wdXQpXHJcblx0XHRcdFx0XHRzZXRUaW1lb3V0KGZvY3VzLCAyMCk7XHJcblx0XHRcdH1cclxuXHRcdFx0ZWxzZVxyXG5cdFx0XHRcdGNlbnRlcihseXIzWzBdLCBvcHRzLmNlbnRlclgsIG9wdHMuY2VudGVyWSk7XHJcblxyXG5cdFx0XHRpZiAob3B0cy50aW1lb3V0KSB7XHJcblx0XHRcdFx0Ly8gYXV0by11bmJsb2NrXHJcblx0XHRcdFx0dmFyIHRvID0gc2V0VGltZW91dChmdW5jdGlvbigpIHtcclxuXHRcdFx0XHRcdGlmIChmdWxsKVxyXG5cdFx0XHRcdFx0XHQkLnVuYmxvY2tVSShvcHRzKTtcclxuXHRcdFx0XHRcdGVsc2VcclxuXHRcdFx0XHRcdFx0JChlbCkudW5ibG9jayhvcHRzKTtcclxuXHRcdFx0XHR9LCBvcHRzLnRpbWVvdXQpO1xyXG5cdFx0XHRcdCQoZWwpLmRhdGEoJ2Jsb2NrVUkudGltZW91dCcsIHRvKTtcclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cclxuXHRcdC8vIHJlbW92ZSB0aGUgYmxvY2tcclxuXHRcdGZ1bmN0aW9uIHJlbW92ZShlbCwgb3B0cykge1xyXG5cdFx0XHR2YXIgY291bnQ7XHJcblx0XHRcdHZhciBmdWxsID0gKGVsID09IHdpbmRvdyk7XHJcblx0XHRcdHZhciAkZWwgPSAkKGVsKTtcclxuXHRcdFx0dmFyIGRhdGEgPSAkZWwuZGF0YSgnYmxvY2tVSS5oaXN0b3J5Jyk7XHJcblx0XHRcdHZhciB0byA9ICRlbC5kYXRhKCdibG9ja1VJLnRpbWVvdXQnKTtcclxuXHRcdFx0aWYgKHRvKSB7XHJcblx0XHRcdFx0Y2xlYXJUaW1lb3V0KHRvKTtcclxuXHRcdFx0XHQkZWwucmVtb3ZlRGF0YSgnYmxvY2tVSS50aW1lb3V0Jyk7XHJcblx0XHRcdH1cclxuXHRcdFx0b3B0cyA9ICQuZXh0ZW5kKHt9LCAkLmJsb2NrVUkuZGVmYXVsdHMsIG9wdHMgfHwge30pO1xyXG5cdFx0XHRiaW5kKDAsIGVsLCBvcHRzKTsgLy8gdW5iaW5kIGV2ZW50c1xyXG5cclxuXHRcdFx0aWYgKG9wdHMub25VbmJsb2NrID09PSBudWxsKSB7XHJcblx0XHRcdFx0b3B0cy5vblVuYmxvY2sgPSAkZWwuZGF0YSgnYmxvY2tVSS5vblVuYmxvY2snKTtcclxuXHRcdFx0XHQkZWwucmVtb3ZlRGF0YSgnYmxvY2tVSS5vblVuYmxvY2snKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0dmFyIGVscztcclxuXHRcdFx0aWYgKGZ1bGwpIC8vIGNyYXp5IHNlbGVjdG9yIHRvIGhhbmRsZSBvZGQgZmllbGQgZXJyb3JzIGluIGllNi83XHJcblx0XHRcdFx0ZWxzID0gJCgnYm9keScpLmNoaWxkcmVuKCkuZmlsdGVyKCcuYmxvY2tVSScpLmFkZCgnYm9keSA+IC5ibG9ja1VJJyk7XHJcblx0XHRcdGVsc2VcclxuXHRcdFx0XHRlbHMgPSAkZWwuZmluZCgnPi5ibG9ja1VJJyk7XHJcblxyXG5cdFx0XHQvLyBmaXggY3Vyc29yIGlzc3VlXHJcblx0XHRcdGlmICggb3B0cy5jdXJzb3JSZXNldCApIHtcclxuXHRcdFx0XHRpZiAoIGVscy5sZW5ndGggPiAxIClcclxuXHRcdFx0XHRcdGVsc1sxXS5zdHlsZS5jdXJzb3IgPSBvcHRzLmN1cnNvclJlc2V0O1xyXG5cdFx0XHRcdGlmICggZWxzLmxlbmd0aCA+IDIgKVxyXG5cdFx0XHRcdFx0ZWxzWzJdLnN0eWxlLmN1cnNvciA9IG9wdHMuY3Vyc29yUmVzZXQ7XHJcblx0XHRcdH1cclxuXHJcblx0XHRcdGlmIChmdWxsKVxyXG5cdFx0XHRcdHBhZ2VCbG9jayA9IHBhZ2VCbG9ja0VscyA9IG51bGw7XHJcblxyXG5cdFx0XHRpZiAob3B0cy5mYWRlT3V0KSB7XHJcblx0XHRcdFx0Y291bnQgPSBlbHMubGVuZ3RoO1xyXG5cdFx0XHRcdGVscy5zdG9wKCkuZmFkZU91dChvcHRzLmZhZGVPdXQsIGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRcdFx0aWYgKCAtLWNvdW50ID09PSAwKVxyXG5cdFx0XHRcdFx0XHRyZXNldChlbHMsZGF0YSxvcHRzLGVsKTtcclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlXHJcblx0XHRcdFx0cmVzZXQoZWxzLCBkYXRhLCBvcHRzLCBlbCk7XHJcblx0XHR9XHJcblxyXG5cdFx0Ly8gbW92ZSBibG9ja2luZyBlbGVtZW50IGJhY2sgaW50byB0aGUgRE9NIHdoZXJlIGl0IHN0YXJ0ZWRcclxuXHRcdGZ1bmN0aW9uIHJlc2V0KGVscyxkYXRhLG9wdHMsZWwpIHtcclxuXHRcdFx0dmFyICRlbCA9ICQoZWwpO1xyXG5cdFx0XHRpZiAoICRlbC5kYXRhKCdibG9ja1VJLmlzQmxvY2tlZCcpIClcclxuXHRcdFx0XHRyZXR1cm47XHJcblxyXG5cdFx0XHRlbHMuZWFjaChmdW5jdGlvbihpLG8pIHtcclxuXHRcdFx0XHQvLyByZW1vdmUgdmlhIERPTSBjYWxscyBzbyB3ZSBkb24ndCBsb3NlIGV2ZW50IGhhbmRsZXJzXHJcblx0XHRcdFx0aWYgKHRoaXMucGFyZW50Tm9kZSlcclxuXHRcdFx0XHRcdHRoaXMucGFyZW50Tm9kZS5yZW1vdmVDaGlsZCh0aGlzKTtcclxuXHRcdFx0fSk7XHJcblxyXG5cdFx0XHRpZiAoZGF0YSAmJiBkYXRhLmVsKSB7XHJcblx0XHRcdFx0ZGF0YS5lbC5zdHlsZS5kaXNwbGF5ID0gZGF0YS5kaXNwbGF5O1xyXG5cdFx0XHRcdGRhdGEuZWwuc3R5bGUucG9zaXRpb24gPSBkYXRhLnBvc2l0aW9uO1xyXG5cdFx0XHRcdGlmIChkYXRhLnBhcmVudClcclxuXHRcdFx0XHRcdGRhdGEucGFyZW50LmFwcGVuZENoaWxkKGRhdGEuZWwpO1xyXG5cdFx0XHRcdCRlbC5yZW1vdmVEYXRhKCdibG9ja1VJLmhpc3RvcnknKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0aWYgKCRlbC5kYXRhKCdibG9ja1VJLnN0YXRpYycpKSB7XHJcblx0XHRcdFx0JGVsLmNzcygncG9zaXRpb24nLCAnc3RhdGljJyk7IC8vICMyMlxyXG5cdFx0XHR9XHJcblxyXG5cdFx0XHRpZiAodHlwZW9mIG9wdHMub25VbmJsb2NrID09ICdmdW5jdGlvbicpXHJcblx0XHRcdFx0b3B0cy5vblVuYmxvY2soZWwsb3B0cyk7XHJcblxyXG5cdFx0XHQvLyBmaXggaXNzdWUgaW4gU2FmYXJpIDYgd2hlcmUgYmxvY2sgYXJ0aWZhY3RzIHJlbWFpbiB1bnRpbCByZWZsb3dcclxuXHRcdFx0dmFyIGJvZHkgPSAkKGRvY3VtZW50LmJvZHkpLCB3ID0gYm9keS53aWR0aCgpLCBjc3NXID0gYm9keVswXS5zdHlsZS53aWR0aDtcclxuXHRcdFx0Ym9keS53aWR0aCh3LTEpLndpZHRoKHcpO1xyXG5cdFx0XHRib2R5WzBdLnN0eWxlLndpZHRoID0gY3NzVztcclxuXHRcdH1cclxuXHJcblx0XHQvLyBiaW5kL3VuYmluZCB0aGUgaGFuZGxlclxyXG5cdFx0ZnVuY3Rpb24gYmluZChiLCBlbCwgb3B0cykge1xyXG5cdFx0XHR2YXIgZnVsbCA9IGVsID09IHdpbmRvdywgJGVsID0gJChlbCk7XHJcblxyXG5cdFx0XHQvLyBkb24ndCBib3RoZXIgdW5iaW5kaW5nIGlmIHRoZXJlIGlzIG5vdGhpbmcgdG8gdW5iaW5kXHJcblx0XHRcdGlmICghYiAmJiAoZnVsbCAmJiAhcGFnZUJsb2NrIHx8ICFmdWxsICYmICEkZWwuZGF0YSgnYmxvY2tVSS5pc0Jsb2NrZWQnKSkpXHJcblx0XHRcdFx0cmV0dXJuO1xyXG5cclxuXHRcdFx0JGVsLmRhdGEoJ2Jsb2NrVUkuaXNCbG9ja2VkJywgYik7XHJcblxyXG5cdFx0XHQvLyBkb24ndCBiaW5kIGV2ZW50cyB3aGVuIG92ZXJsYXkgaXMgbm90IGluIHVzZSBvciBpZiBiaW5kRXZlbnRzIGlzIGZhbHNlXHJcblx0XHRcdGlmICghZnVsbCB8fCAhb3B0cy5iaW5kRXZlbnRzIHx8IChiICYmICFvcHRzLnNob3dPdmVybGF5KSlcclxuXHRcdFx0XHRyZXR1cm47XHJcblxyXG5cdFx0XHQvLyBiaW5kIGFuY2hvcnMgYW5kIGlucHV0cyBmb3IgbW91c2UgYW5kIGtleSBldmVudHNcclxuXHRcdFx0dmFyIGV2ZW50cyA9ICdtb3VzZWRvd24gbW91c2V1cCBrZXlkb3duIGtleXByZXNzIGtleXVwIHRvdWNoc3RhcnQgdG91Y2hlbmQgdG91Y2htb3ZlJztcclxuXHRcdFx0aWYgKGIpXHJcblx0XHRcdFx0JChkb2N1bWVudCkuYmluZChldmVudHMsIG9wdHMsIGhhbmRsZXIpO1xyXG5cdFx0XHRlbHNlXHJcblx0XHRcdFx0JChkb2N1bWVudCkudW5iaW5kKGV2ZW50cywgaGFuZGxlcik7XHJcblxyXG5cdFx0Ly8gZm9ybWVyIGltcGwuLi5cclxuXHRcdC8vXHRcdHZhciAkZSA9ICQoJ2EsOmlucHV0Jyk7XHJcblx0XHQvL1x0XHRiID8gJGUuYmluZChldmVudHMsIG9wdHMsIGhhbmRsZXIpIDogJGUudW5iaW5kKGV2ZW50cywgaGFuZGxlcik7XHJcblx0XHR9XHJcblxyXG5cdFx0Ly8gZXZlbnQgaGFuZGxlciB0byBzdXBwcmVzcyBrZXlib2FyZC9tb3VzZSBldmVudHMgd2hlbiBibG9ja2luZ1xyXG5cdFx0ZnVuY3Rpb24gaGFuZGxlcihlKSB7XHJcblx0XHRcdC8vIGFsbG93IHRhYiBuYXZpZ2F0aW9uIChjb25kaXRpb25hbGx5KVxyXG5cdFx0XHRpZiAoZS50eXBlID09PSAna2V5ZG93bicgJiYgZS5rZXlDb2RlICYmIGUua2V5Q29kZSA9PSA5KSB7XHJcblx0XHRcdFx0aWYgKHBhZ2VCbG9jayAmJiBlLmRhdGEuY29uc3RyYWluVGFiS2V5KSB7XHJcblx0XHRcdFx0XHR2YXIgZWxzID0gcGFnZUJsb2NrRWxzO1xyXG5cdFx0XHRcdFx0dmFyIGZ3ZCA9ICFlLnNoaWZ0S2V5ICYmIGUudGFyZ2V0ID09PSBlbHNbZWxzLmxlbmd0aC0xXTtcclxuXHRcdFx0XHRcdHZhciBiYWNrID0gZS5zaGlmdEtleSAmJiBlLnRhcmdldCA9PT0gZWxzWzBdO1xyXG5cdFx0XHRcdFx0aWYgKGZ3ZCB8fCBiYWNrKSB7XHJcblx0XHRcdFx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24oKXtmb2N1cyhiYWNrKTt9LDEwKTtcclxuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHR2YXIgb3B0cyA9IGUuZGF0YTtcclxuXHRcdFx0dmFyIHRhcmdldCA9ICQoZS50YXJnZXQpO1xyXG5cdFx0XHRpZiAodGFyZ2V0Lmhhc0NsYXNzKCdibG9ja092ZXJsYXknKSAmJiBvcHRzLm9uT3ZlcmxheUNsaWNrKVxyXG5cdFx0XHRcdG9wdHMub25PdmVybGF5Q2xpY2soZSk7XHJcblxyXG5cdFx0XHQvLyBhbGxvdyBldmVudHMgd2l0aGluIHRoZSBtZXNzYWdlIGNvbnRlbnRcclxuXHRcdFx0aWYgKHRhcmdldC5wYXJlbnRzKCdkaXYuJyArIG9wdHMuYmxvY2tNc2dDbGFzcykubGVuZ3RoID4gMClcclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHJcblx0XHRcdC8vIGFsbG93IGV2ZW50cyBmb3IgY29udGVudCB0aGF0IGlzIG5vdCBiZWluZyBibG9ja2VkXHJcblx0XHRcdHJldHVybiB0YXJnZXQucGFyZW50cygpLmNoaWxkcmVuKCkuZmlsdGVyKCdkaXYuYmxvY2tVSScpLmxlbmd0aCA9PT0gMDtcclxuXHRcdH1cclxuXHJcblx0XHRmdW5jdGlvbiBmb2N1cyhiYWNrKSB7XHJcblx0XHRcdGlmICghcGFnZUJsb2NrRWxzKVxyXG5cdFx0XHRcdHJldHVybjtcclxuXHRcdFx0dmFyIGUgPSBwYWdlQmxvY2tFbHNbYmFjaz09PXRydWUgPyBwYWdlQmxvY2tFbHMubGVuZ3RoLTEgOiAwXTtcclxuXHRcdFx0aWYgKGUpXHJcblx0XHRcdFx0ZS5mb2N1cygpO1xyXG5cdFx0fVxyXG5cclxuXHRcdGZ1bmN0aW9uIGNlbnRlcihlbCwgeCwgeSkge1xyXG5cdFx0XHR2YXIgcCA9IGVsLnBhcmVudE5vZGUsIHMgPSBlbC5zdHlsZTtcclxuXHRcdFx0dmFyIGwgPSAoKHAub2Zmc2V0V2lkdGggLSBlbC5vZmZzZXRXaWR0aCkvMikgLSBzeihwLCdib3JkZXJMZWZ0V2lkdGgnKTtcclxuXHRcdFx0dmFyIHQgPSAoKHAub2Zmc2V0SGVpZ2h0IC0gZWwub2Zmc2V0SGVpZ2h0KS8yKSAtIHN6KHAsJ2JvcmRlclRvcFdpZHRoJyk7XHJcblx0XHRcdGlmICh4KSBzLmxlZnQgPSBsID4gMCA/IChsKydweCcpIDogJzAnO1xyXG5cdFx0XHRpZiAoeSkgcy50b3AgID0gdCA+IDAgPyAodCsncHgnKSA6ICcwJztcclxuXHRcdH1cclxuXHJcblx0XHRmdW5jdGlvbiBzeihlbCwgcCkge1xyXG5cdFx0XHRyZXR1cm4gcGFyc2VJbnQoJC5jc3MoZWwscCksMTApfHwwO1xyXG5cdFx0fVxyXG5cclxuXHR9XHJcblxyXG5cclxuXHQvKmdsb2JhbCBkZWZpbmU6dHJ1ZSAqL1xyXG5cdGlmICh0eXBlb2YgZGVmaW5lID09PSAnZnVuY3Rpb24nICYmIGRlZmluZS5hbWQgJiYgZGVmaW5lLmFtZC5qUXVlcnkpIHtcclxuXHRcdGRlZmluZShbJ2pxdWVyeSddLCBzZXR1cCk7XHJcblx0fSBlbHNlIHtcclxuXHRcdHNldHVwKGpRdWVyeSk7XHJcblx0fVxyXG5cclxufSkoKTtcclxuIl0sImZpbGUiOiJqcXVlcnkuYmxvY2tVSS5qcyIsInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9