/* Modernizr 2.8.3 (Custom Build) | MIT & BSD
 * Build: http://modernizr.com/download/#-csstransforms-csstransforms3d-csstransitions-teststyles-testprop-testallprops-prefixes-domprefixes
 */
;window.Modernizr=function(a,b,c){function y(a){i.cssText=a}function z(a,b){return y(l.join(a+";")+(b||""))}function A(a,b){return typeof a===b}function B(a,b){return!!~(""+a).indexOf(b)}function C(a,b){for(var d in a){var e=a[d];if(!B(e,"-")&&i[e]!==c)return b=="pfx"?e:!0}return!1}function D(a,b,d){for(var e in a){var f=b[a[e]];if(f!==c)return d===!1?a[e]:A(f,"function")?f.bind(d||b):f}return!1}function E(a,b,c){var d=a.charAt(0).toUpperCase()+a.slice(1),e=(a+" "+n.join(d+" ")+d).split(" ");return A(b,"string")||A(b,"undefined")?C(e,b):(e=(a+" "+o.join(d+" ")+d).split(" "),D(e,b,c))}var d="2.8.3",e={},f=b.documentElement,g="modernizr",h=b.createElement(g),i=h.style,j,k={}.toString,l=" -webkit- -moz- -o- -ms- ".split(" "),m="Webkit Moz O ms",n=m.split(" "),o=m.toLowerCase().split(" "),p={},q={},r={},s=[],t=s.slice,u,v=function(a,c,d,e){var h,i,j,k,l=b.createElement("div"),m=b.body,n=m||b.createElement("body");if(parseInt(d,10))while(d--)j=b.createElement("div"),j.id=e?e[d]:g+(d+1),l.appendChild(j);return h=["&#173;",'<style id="s',g,'">',a,"</style>"].join(""),l.id=g,(m?l:n).innerHTML+=h,n.appendChild(l),m||(n.style.background="",n.style.overflow="hidden",k=f.style.overflow,f.style.overflow="hidden",f.appendChild(n)),i=c(l,a),m?l.parentNode.removeChild(l):(n.parentNode.removeChild(n),f.style.overflow=k),!!i},w={}.hasOwnProperty,x;!A(w,"undefined")&&!A(w.call,"undefined")?x=function(a,b){return w.call(a,b)}:x=function(a,b){return b in a&&A(a.constructor.prototype[b],"undefined")},Function.prototype.bind||(Function.prototype.bind=function(b){var c=this;if(typeof c!="function")throw new TypeError;var d=t.call(arguments,1),e=function(){if(this instanceof e){var a=function(){};a.prototype=c.prototype;var f=new a,g=c.apply(f,d.concat(t.call(arguments)));return Object(g)===g?g:f}return c.apply(b,d.concat(t.call(arguments)))};return e}),p.csstransforms=function(){return!!E("transform")},p.csstransforms3d=function(){var a=!!E("perspective");return a&&"webkitPerspective"in f.style&&v("@media (transform-3d),(-webkit-transform-3d){#modernizr{left:9px;position:absolute;height:3px;}}",function(b,c){a=b.offsetLeft===9&&b.offsetHeight===3}),a},p.csstransitions=function(){return E("transition")};for(var F in p)x(p,F)&&(u=F.toLowerCase(),e[u]=p[F](),s.push((e[u]?"":"no-")+u));return e.addTest=function(a,b){if(typeof a=="object")for(var d in a)x(a,d)&&e.addTest(d,a[d]);else{a=a.toLowerCase();if(e[a]!==c)return e;b=typeof b=="function"?b():b,typeof enableClasses!="undefined"&&enableClasses&&(f.className+=" "+(b?"":"no-")+a),e[a]=b}return e},y(""),h=j=null,e._version=d,e._prefixes=l,e._domPrefixes=o,e._cssomPrefixes=n,e.testProp=function(a){return C([a])},e.testAllProps=E,e.testStyles=v,e}(this,this.document);
/*! jCarousel - v0.3.1 - 2014-04-26
* http://sorgalla.com/jcarousel
* Copyright (c) 2014 Jan Sorgalla; Licensed MIT */
(function($) {
    'use strict';

    var jCarousel = $.jCarousel = {};

    jCarousel.version = '0.3.1';

    var rRelativeTarget = /^([+\-]=)?(.+)$/;

    jCarousel.parseTarget = function(target) {
        var relative = false,
            parts    = typeof target !== 'object' ?
                           rRelativeTarget.exec(target) :
                           null;

        if (parts) {
            target = parseInt(parts[2], 10) || 0;

            if (parts[1]) {
                relative = true;
                if (parts[1] === '-=') {
                    target *= -1;
                }
            }
        } else if (typeof target !== 'object') {
            target = parseInt(target, 10) || 0;
        }

        return {
            target: target,
            relative: relative
        };
    };

    jCarousel.detectCarousel = function(element) {
        var carousel;

        while (element.length > 0) {
            carousel = element.filter('[data-jcarousel]');

            if (carousel.length > 0) {
                return carousel;
            }

            carousel = element.find('[data-jcarousel]');

            if (carousel.length > 0) {
                return carousel;
            }

            element = element.parent();
        }

        return null;
    };

    jCarousel.base = function(pluginName) {
        return {
            version:  jCarousel.version,
            _options:  {},
            _element:  null,
            _carousel: null,
            _init:     $.noop,
            _create:   $.noop,
            _destroy:  $.noop,
            _reload:   $.noop,
            create: function() {
                this._element
                    .attr('data-' + pluginName.toLowerCase(), true)
                    .data(pluginName, this);

                if (false === this._trigger('create')) {
                    return this;
                }

                this._create();

                this._trigger('createend');

                return this;
            },
            destroy: function() {
                if (false === this._trigger('destroy')) {
                    return this;
                }

                this._destroy();

                this._trigger('destroyend');

                this._element
                    .removeData(pluginName)
                    .removeAttr('data-' + pluginName.toLowerCase());

                return this;
            },
            reload: function(options) {
                if (false === this._trigger('reload')) {
                    return this;
                }

                if (options) {
                    this.options(options);
                }

                this._reload();

                this._trigger('reloadend');

                return this;
            },
            element: function() {
                return this._element;
            },
            options: function(key, value) {
                if (arguments.length === 0) {
                    return $.extend({}, this._options);
                }

                if (typeof key === 'string') {
                    if (typeof value === 'undefined') {
                        return typeof this._options[key] === 'undefined' ?
                                null :
                                this._options[key];
                    }

                    this._options[key] = value;
                } else {
                    this._options = $.extend({}, this._options, key);
                }

                return this;
            },
            carousel: function() {
                if (!this._carousel) {
                    this._carousel = jCarousel.detectCarousel(this.options('carousel') || this._element);

                    if (!this._carousel) {
                        $.error('Could not detect carousel for plugin "' + pluginName + '"');
                    }
                }

                return this._carousel;
            },
            _trigger: function(type, element, data) {
                var event,
                    defaultPrevented = false;

                data = [this].concat(data || []);

                (element || this._element).each(function() {
                    event = $.Event((pluginName + ':' + type).toLowerCase());

                    $(this).trigger(event, data);

                    if (event.isDefaultPrevented()) {
                        defaultPrevented = true;
                    }
                });

                return !defaultPrevented;
            }
        };
    };

    jCarousel.plugin = function(pluginName, pluginPrototype) {
        var Plugin = $[pluginName] = function(element, options) {
            this._element = $(element);
            this.options(options);

            this._init();
            this.create();
        };

        Plugin.fn = Plugin.prototype = $.extend(
            {},
            jCarousel.base(pluginName),
            pluginPrototype
        );

        $.fn[pluginName] = function(options) {
            var args        = Array.prototype.slice.call(arguments, 1),
                returnValue = this;

            if (typeof options === 'string') {
                this.each(function() {
                    var instance = $(this).data(pluginName);

                    if (!instance) {
                        return $.error(
                            'Cannot call methods on ' + pluginName + ' prior to initialization; ' +
                            'attempted to call method "' + options + '"'
                        );
                    }

                    if (!$.isFunction(instance[options]) || options.charAt(0) === '_') {
                        return $.error(
                            'No such method "' + options + '" for ' + pluginName + ' instance'
                        );
                    }

                    var methodValue = instance[options].apply(instance, args);

                    if (methodValue !== instance && typeof methodValue !== 'undefined') {
                        returnValue = methodValue;
                        return false;
                    }
                });
            } else {
                this.each(function() {
                    var instance = $(this).data(pluginName);

                    if (instance instanceof Plugin) {
                        instance.reload(options);
                    } else {
                        new Plugin(this, options);
                    }
                });
            }

            return returnValue;
        };

        return Plugin;
    };
}(jQuery));

(function($, window) {
    'use strict';

    var toFloat = function(val) {
        return parseFloat(val) || 0;
    };

    $.jCarousel.plugin('jcarousel', {
        animating:   false,
        tail:        0,
        inTail:      false,
        resizeTimer: null,
        lt:          null,
        vertical:    false,
        rtl:         false,
        circular:    false,
        underflow:   false,
        relative:    false,

        _options: {
            list: function() {
                return this.element().children().eq(0);
            },
            items: function() {
                return this.list().children();
            },
            animation:   400,
            transitions: false,
            wrap:        null,
            vertical:    null,
            rtl:         null,
            center:      false
        },

        // Protected, don't access directly
        _list:         null,
        _items:        null,
        _target:       null,
        _first:        null,
        _last:         null,
        _visible:      null,
        _fullyvisible: null,
        _init: function() {
            var self = this;

            this.onWindowResize = function() {
                if (self.resizeTimer) {
                    clearTimeout(self.resizeTimer);
                }

                self.resizeTimer = setTimeout(function() {
                    self.reload();
                }, 100);
            };

            return this;
        },
        _create: function() {
            this._reload();

            $(window).on('resize.jcarousel', this.onWindowResize);
        },
        _destroy: function() {
            $(window).off('resize.jcarousel', this.onWindowResize);
        },
        _reload: function() {
            this.vertical = this.options('vertical');

            if (this.vertical == null) {
                this.vertical = this.list().height() > this.list().width();
            }

            this.rtl = this.options('rtl');

            if (this.rtl == null) {
                this.rtl = (function(element) {
                    if (('' + element.attr('dir')).toLowerCase() === 'rtl') {
                        return true;
                    }

                    var found = false;

                    element.parents('[dir]').each(function() {
                        if ((/rtl/i).test($(this).attr('dir'))) {
                            found = true;
                            return false;
                        }
                    });

                    return found;
                }(this._element));
            }

            this.lt = this.vertical ? 'top' : 'left';

            // Ensure before closest() call
            this.relative = this.list().css('position') === 'relative';

            // Force list and items reload
            this._list  = null;
            this._items = null;

            var item = this._target && this.index(this._target) >= 0 ?
                           this._target :
                           this.closest();

            // _prepare() needs this here
            this.circular  = this.options('wrap') === 'circular';
            this.underflow = false;

            var props = {'left': 0, 'top': 0};

            if (item.length > 0) {
                this._prepare(item);
                this.list().find('[data-jcarousel-clone]').remove();

                // Force items reload
                this._items = null;

                this.underflow = this._fullyvisible.length >= this.items().length;
                this.circular  = this.circular && !this.underflow;

                props[this.lt] = this._position(item) + 'px';
            }

            this.move(props);

            return this;
        },
        list: function() {
            if (this._list === null) {
                var option = this.options('list');
                this._list = $.isFunction(option) ? option.call(this) : this._element.find(option);
            }

            return this._list;
        },
        items: function() {
            if (this._items === null) {
                var option = this.options('items');
                this._items = ($.isFunction(option) ? option.call(this) : this.list().find(option)).not('[data-jcarousel-clone]');
            }

            return this._items;
        },
        index: function(item) {
            return this.items().index(item);
        },
        closest: function() {
            var self    = this,
                pos     = this.list().position()[this.lt],
                closest = $(), // Ensure we're returning a jQuery instance
                stop    = false,
                lrb     = this.vertical ? 'bottom' : (this.rtl && !this.relative ? 'left' : 'right'),
                width;

            if (this.rtl && this.relative && !this.vertical) {
                pos += this.list().width() - this.clipping();
            }

            this.items().each(function() {
                closest = $(this);

                if (stop) {
                    return false;
                }

                var dim = self.dimension(closest);

                pos += dim;

                if (pos >= 0) {
                    width = dim - toFloat(closest.css('margin-' + lrb));

                    if ((Math.abs(pos) - dim + (width / 2)) <= 0) {
                        stop = true;
                    } else {
                        return false;
                    }
                }
            });


            return closest;
        },
        target: function() {
            return this._target;
        },
        first: function() {
            return this._first;
        },
        last: function() {
            return this._last;
        },
        visible: function() {
            return this._visible;
        },
        fullyvisible: function() {
            return this._fullyvisible;
        },
        hasNext: function() {
            if (false === this._trigger('hasnext')) {
                return true;
            }

            var wrap = this.options('wrap'),
                end = this.items().length - 1;

            return end >= 0 && !this.underflow &&
                ((wrap && wrap !== 'first') ||
                    (this.index(this._last) < end) ||
                    (this.tail && !this.inTail)) ? true : false;
        },
        hasPrev: function() {
            if (false === this._trigger('hasprev')) {
                return true;
            }

            var wrap = this.options('wrap');

            return this.items().length > 0 && !this.underflow &&
                ((wrap && wrap !== 'last') ||
                    (this.index(this._first) > 0) ||
                    (this.tail && this.inTail)) ? true : false;
        },
        clipping: function() {
            return this._element['inner' + (this.vertical ? 'Height' : 'Width')]();
        },
        dimension: function(element) {
            return element['outer' + (this.vertical ? 'Height' : 'Width')](true);
        },
        scroll: function(target, animate, callback) {
            if (this.animating) {
                return this;
            }

            if (false === this._trigger('scroll', null, [target, animate])) {
                return this;
            }

            if ($.isFunction(animate)) {
                callback = animate;
                animate  = true;
            }

            var parsed = $.jCarousel.parseTarget(target);

            if (parsed.relative) {
                var end    = this.items().length - 1,
                    scroll = Math.abs(parsed.target),
                    wrap   = this.options('wrap'),
                    current,
                    first,
                    index,
                    start,
                    curr,
                    isVisible,
                    props,
                    i;

                if (parsed.target > 0) {
                    var last = this.index(this._last);

                    if (last >= end && this.tail) {
                        if (!this.inTail) {
                            this._scrollTail(animate, callback);
                        } else {
                            if (wrap === 'both' || wrap === 'last') {
                                this._scroll(0, animate, callback);
                            } else {
                                if ($.isFunction(callback)) {
                                    callback.call(this, false);
                                }
                            }
                        }
                    } else {
                        current = this.index(this._target);

                        if ((this.underflow && current === end && (wrap === 'circular' || wrap === 'both' || wrap === 'last')) ||
                            (!this.underflow && last === end && (wrap === 'both' || wrap === 'last'))) {
                            this._scroll(0, animate, callback);
                        } else {
                            index = current + scroll;

                            if (this.circular && index > end) {
                                i = end;
                                curr = this.items().get(-1);

                                while (i++ < index) {
                                    curr = this.items().eq(0);
                                    isVisible = this._visible.index(curr) >= 0;

                                    if (isVisible) {
                                        curr.after(curr.clone(true).attr('data-jcarousel-clone', true));
                                    }

                                    this.list().append(curr);

                                    if (!isVisible) {
                                        props = {};
                                        props[this.lt] = this.dimension(curr);
                                        this.moveBy(props);
                                    }

                                    // Force items reload
                                    this._items = null;
                                }

                                this._scroll(curr, animate, callback);
                            } else {
                                this._scroll(Math.min(index, end), animate, callback);
                            }
                        }
                    }
                } else {
                    if (this.inTail) {
                        this._scroll(Math.max((this.index(this._first) - scroll) + 1, 0), animate, callback);
                    } else {
                        first  = this.index(this._first);
                        current = this.index(this._target);
                        start  = this.underflow ? current : first;
                        index  = start - scroll;

                        if (start <= 0 && ((this.underflow && wrap === 'circular') || wrap === 'both' || wrap === 'first')) {
                            this._scroll(end, animate, callback);
                        } else {
                            if (this.circular && index < 0) {
                                i    = index;
                                curr = this.items().get(0);

                                while (i++ < 0) {
                                    curr = this.items().eq(-1);
                                    isVisible = this._visible.index(curr) >= 0;

                                    if (isVisible) {
                                        curr.after(curr.clone(true).attr('data-jcarousel-clone', true));
                                    }

                                    this.list().prepend(curr);

                                    // Force items reload
                                    this._items = null;

                                    var dim = this.dimension(curr);

                                    props = {};
                                    props[this.lt] = -dim;
                                    this.moveBy(props);

                                }

                                this._scroll(curr, animate, callback);
                            } else {
                                this._scroll(Math.max(index, 0), animate, callback);
                            }
                        }
                    }
                }
            } else {
                this._scroll(parsed.target, animate, callback);
            }

            this._trigger('scrollend');

            return this;
        },
        moveBy: function(properties, opts) {
            var position = this.list().position(),
                multiplier = 1,
                correction = 0;

            if (this.rtl && !this.vertical) {
                multiplier = -1;

                if (this.relative) {
                    correction = this.list().width() - this.clipping();
                }
            }

            if (properties.left) {
                properties.left = (position.left + correction + toFloat(properties.left) * multiplier) + 'px';
            }

            if (properties.top) {
                properties.top = (position.top + correction + toFloat(properties.top) * multiplier) + 'px';
            }

            return this.move(properties, opts);
        },
        move: function(properties, opts) {
            opts = opts || {};

            var option       = this.options('transitions'),
                transitions  = !!option,
                transforms   = !!option.transforms,
                transforms3d = !!option.transforms3d,
                duration     = opts.duration || 0,
                list         = this.list();

            if (!transitions && duration > 0) {
                list.animate(properties, opts);
                return;
            }

            var complete = opts.complete || $.noop,
                css = {};

            if (transitions) {
                var backup = list.css(['transitionDuration', 'transitionTimingFunction', 'transitionProperty']),
                    oldComplete = complete;

                complete = function() {
                    $(this).css(backup);
                    oldComplete.call(this);
                };
                css = {
                    transitionDuration: (duration > 0 ? duration / 1000 : 0) + 's',
                    transitionTimingFunction: option.easing || opts.easing,
                    transitionProperty: duration > 0 ? (function() {
                        if (transforms || transforms3d) {
                            // We have to use 'all' because jQuery doesn't prefix
                            // css values, like transition-property: transform;
                            return 'all';
                        }

                        return properties.left ? 'left' : 'top';
                    })() : 'none',
                    transform: 'none'
                };
            }

            if (transforms3d) {
                css.transform = 'translate3d(' + (properties.left || 0) + ',' + (properties.top || 0) + ',0)';
            } else if (transforms) {
                css.transform = 'translate(' + (properties.left || 0) + ',' + (properties.top || 0) + ')';
            } else {
                $.extend(css, properties);
            }

            if (transitions && duration > 0) {
                list.one('transitionend webkitTransitionEnd oTransitionEnd otransitionend MSTransitionEnd', complete);
            }

            list.css(css);

            if (duration <= 0) {
                list.each(function() {
                    complete.call(this);
                });
            }
        },
        _scroll: function(item, animate, callback) {
            if (this.animating) {
                if ($.isFunction(callback)) {
                    callback.call(this, false);
                }

                return this;
            }

            if (typeof item !== 'object') {
                item = this.items().eq(item);
            } else if (typeof item.jquery === 'undefined') {
                item = $(item);
            }

            if (item.length === 0) {
                if ($.isFunction(callback)) {
                    callback.call(this, false);
                }

                return this;
            }

            this.inTail = false;

            this._prepare(item);

            var pos     = this._position(item),
                currPos = this.list().position()[this.lt];

            if (pos === currPos) {
                if ($.isFunction(callback)) {
                    callback.call(this, false);
                }

                return this;
            }

            var properties = {};
            properties[this.lt] = pos + 'px';

            this._animate(properties, animate, callback);

            return this;
        },
        _scrollTail: function(animate, callback) {
            if (this.animating || !this.tail) {
                if ($.isFunction(callback)) {
                    callback.call(this, false);
                }

                return this;
            }

            var pos = this.list().position()[this.lt];

            if (this.rtl && this.relative && !this.vertical) {
                pos += this.list().width() - this.clipping();
            }

            if (this.rtl && !this.vertical) {
                pos += this.tail;
            } else {
                pos -= this.tail;
            }

            this.inTail = true;

            var properties = {};
            properties[this.lt] = pos + 'px';

            this._update({
                target:       this._target.next(),
                fullyvisible: this._fullyvisible.slice(1).add(this._visible.last())
            });

            this._animate(properties, animate, callback);

            return this;
        },
        _animate: function(properties, animate, callback) {
            callback = callback || $.noop;

            if (false === this._trigger('animate')) {
                callback.call(this, false);
                return this;
            }

            this.animating = true;

            var animation = this.options('animation'),
                complete  = $.proxy(function() {
                    this.animating = false;

                    var c = this.list().find('[data-jcarousel-clone]');

                    if (c.length > 0) {
                        c.remove();
                        this._reload();
                    }

                    this._trigger('animateend');

                    callback.call(this, true);
                }, this);

            var opts = typeof animation === 'object' ?
                           $.extend({}, animation) :
                           {duration: animation},
                oldComplete = opts.complete || $.noop;

            if (animate === false) {
                opts.duration = 0;
            } else if (typeof $.fx.speeds[opts.duration] !== 'undefined') {
                opts.duration = $.fx.speeds[opts.duration];
            }

            opts.complete = function() {
                complete();
                oldComplete.call(this);
            };

            this.move(properties, opts);

            return this;
        },
        _prepare: function(item) {
            var index  = this.index(item),
                idx    = index,
                wh     = this.dimension(item),
                clip   = this.clipping(),
                lrb    = this.vertical ? 'bottom' : (this.rtl ? 'left'  : 'right'),
                center = this.options('center'),
                update = {
                    target:       item,
                    first:        item,
                    last:         item,
                    visible:      item,
                    fullyvisible: wh <= clip ? item : $()
                },
                curr,
                isVisible,
                margin,
                dim;

            if (center) {
                wh /= 2;
                clip /= 2;
            }

            if (wh < clip) {
                while (true) {
                    curr = this.items().eq(++idx);

                    if (curr.length === 0) {
                        if (!this.circular) {
                            break;
                        }

                        curr = this.items().eq(0);

                        if (item.get(0) === curr.get(0)) {
                            break;
                        }

                        isVisible = this._visible.index(curr) >= 0;

                        if (isVisible) {
                            curr.after(curr.clone(true).attr('data-jcarousel-clone', true));
                        }

                        this.list().append(curr);

                        if (!isVisible) {
                            var props = {};
                            props[this.lt] = this.dimension(curr);
                            this.moveBy(props);
                        }

                        // Force items reload
                        this._items = null;
                    }

                    dim = this.dimension(curr);

                    if (dim === 0) {
                        break;
                    }

                    wh += dim;

                    update.last    = curr;
                    update.visible = update.visible.add(curr);

                    // Remove right/bottom margin from total width
                    margin = toFloat(curr.css('margin-' + lrb));

                    if ((wh - margin) <= clip) {
                        update.fullyvisible = update.fullyvisible.add(curr);
                    }

                    if (wh >= clip) {
                        break;
                    }
                }
            }

            if (!this.circular && !center && wh < clip) {
                idx = index;

                while (true) {
                    if (--idx < 0) {
                        break;
                    }

                    curr = this.items().eq(idx);

                    if (curr.length === 0) {
                        break;
                    }

                    dim = this.dimension(curr);

                    if (dim === 0) {
                        break;
                    }

                    wh += dim;

                    update.first   = curr;
                    update.visible = update.visible.add(curr);

                    // Remove right/bottom margin from total width
                    margin = toFloat(curr.css('margin-' + lrb));

                    if ((wh - margin) <= clip) {
                        update.fullyvisible = update.fullyvisible.add(curr);
                    }

                    if (wh >= clip) {
                        break;
                    }
                }
            }

            this._update(update);

            this.tail = 0;

            if (!center &&
                this.options('wrap') !== 'circular' &&
                this.options('wrap') !== 'custom' &&
                this.index(update.last) === (this.items().length - 1)) {

                // Remove right/bottom margin from total width
                wh -= toFloat(update.last.css('margin-' + lrb));

                if (wh > clip) {
                    this.tail = wh - clip;
                }
            }

            return this;
        },
        _position: function(item) {
            var first  = this._first,
                pos    = first.position()[this.lt],
                center = this.options('center'),
                centerOffset = center ? (this.clipping() / 2) - (this.dimension(first) / 2) : 0;

            if (this.rtl && !this.vertical) {
                if (this.relative) {
                    pos -= this.list().width() - this.dimension(first);
                } else {
                    pos -= this.clipping() - this.dimension(first);
                }

                pos += centerOffset;
            } else {
                pos -= centerOffset;
            }

            if (!center &&
                (this.index(item) > this.index(first) || this.inTail) &&
                this.tail) {
                pos = this.rtl && !this.vertical ? pos - this.tail : pos + this.tail;
                this.inTail = true;
            } else {
                this.inTail = false;
            }

            return -pos;
        },
        _update: function(update) {
            var self = this,
                current = {
                    target:       this._target || $(),
                    first:        this._first || $(),
                    last:         this._last || $(),
                    visible:      this._visible || $(),
                    fullyvisible: this._fullyvisible || $()
                },
                back = this.index(update.first || current.first) < this.index(current.first),
                key,
                doUpdate = function(key) {
                    var elIn  = [],
                        elOut = [];

                    update[key].each(function() {
                        if (current[key].index(this) < 0) {
                            elIn.push(this);
                        }
                    });

                    current[key].each(function() {
                        if (update[key].index(this) < 0) {
                            elOut.push(this);
                        }
                    });

                    if (back) {
                        elIn = elIn.reverse();
                    } else {
                        elOut = elOut.reverse();
                    }

                    self._trigger(key + 'in', $(elIn));
                    self._trigger(key + 'out', $(elOut));

                    self['_' + key] = update[key];
                };

            for (key in update) {
                doUpdate(key);
            }

            return this;
        }
    });
}(jQuery, window));

(function($) {
    'use strict';

    $.jcarousel.fn.scrollIntoView = function(target, animate, callback) {
        var parsed = $.jCarousel.parseTarget(target),
            first  = this.index(this._fullyvisible.first()),
            last   = this.index(this._fullyvisible.last()),
            index;

        if (parsed.relative) {
            index = parsed.target < 0 ? Math.max(0, first + parsed.target) : last + parsed.target;
        } else {
            index = typeof parsed.target !== 'object' ? parsed.target : this.index(parsed.target);
        }

        if (index < first) {
            return this.scroll(index, animate, callback);
        }

        if (index >= first && index <= last) {
            if ($.isFunction(callback)) {
                callback.call(this, false);
            }

            return this;
        }

        var items = this.items(),
            clip = this.clipping(),
            lrb  = this.vertical ? 'bottom' : (this.rtl ? 'left'  : 'right'),
            wh   = 0,
            curr;

        while (true) {
            curr = items.eq(index);

            if (curr.length === 0) {
                break;
            }

            wh += this.dimension(curr);

            if (wh >= clip) {
                var margin = parseFloat(curr.css('margin-' + lrb)) || 0;
                if ((wh - margin) !== clip) {
                    index++;
                }
                break;
            }

            if (index <= 0) {
                break;
            }

            index--;
        }

        return this.scroll(index, animate, callback);
    };
}(jQuery));

(function($) {
    'use strict';

    $.jCarousel.plugin('jcarouselControl', {
        _options: {
            target: '+=1',
            event:  'click',
            method: 'scroll'
        },
        _active: null,
        _init: function() {
            this.onDestroy = $.proxy(function() {
                this._destroy();
                this.carousel()
                    .one('jcarousel:createend', $.proxy(this._create, this));
            }, this);
            this.onReload = $.proxy(this._reload, this);
            this.onEvent = $.proxy(function(e) {
                e.preventDefault();

                var method = this.options('method');

                if ($.isFunction(method)) {
                    method.call(this);
                } else {
                    this.carousel()
                        .jcarousel(this.options('method'), this.options('target'));
                }
            }, this);
        },
        _create: function() {
            this.carousel()
                .one('jcarousel:destroy', this.onDestroy)
                .on('jcarousel:reloadend jcarousel:scrollend', this.onReload);

            this._element
                .on(this.options('event') + '.jcarouselcontrol', this.onEvent);

            this._reload();
        },
        _destroy: function() {
            this._element
                .off('.jcarouselcontrol', this.onEvent);

            this.carousel()
                .off('jcarousel:destroy', this.onDestroy)
                .off('jcarousel:reloadend jcarousel:scrollend', this.onReload);
        },
        _reload: function() {
            var parsed   = $.jCarousel.parseTarget(this.options('target')),
                carousel = this.carousel(),
                active;

            if (parsed.relative) {
                active = carousel
                    .jcarousel(parsed.target > 0 ? 'hasNext' : 'hasPrev');
            } else {
                var target = typeof parsed.target !== 'object' ?
                                carousel.jcarousel('items').eq(parsed.target) :
                                parsed.target;

                active = carousel.jcarousel('target').index(target) >= 0;
            }

            if (this._active !== active) {
                this._trigger(active ? 'active' : 'inactive');
                this._active = active;
            }

            return this;
        }
    });
}(jQuery));

(function($) {
    'use strict';

    $.jCarousel.plugin('jcarouselPagination', {
        _options: {
            perPage: null,
            item: function(page) {
                return '<a href="#' + page + '">' + page + '</a>';
            },
            event:  'click',
            method: 'scroll'
        },
        _carouselItems: null,
        _pages: {},
        _items: {},
        _currentPage: null,
        _init: function() {
            this.onDestroy = $.proxy(function() {
                this._destroy();
                this.carousel()
                    .one('jcarousel:createend', $.proxy(this._create, this));
            }, this);
            this.onReload = $.proxy(this._reload, this);
            this.onScroll = $.proxy(this._update, this);
        },
        _create: function() {
            this.carousel()
                .one('jcarousel:destroy', this.onDestroy)
                .on('jcarousel:reloadend', this.onReload)
                .on('jcarousel:scrollend', this.onScroll);

            this._reload();
        },
        _destroy: function() {
            this._clear();

            this.carousel()
                .off('jcarousel:destroy', this.onDestroy)
                .off('jcarousel:reloadend', this.onReload)
                .off('jcarousel:scrollend', this.onScroll);

            this._carouselItems = null;
        },
        _reload: function() {
            var perPage = this.options('perPage');

            this._pages = {};
            this._items = {};

            // Calculate pages
            if ($.isFunction(perPage)) {
                perPage = perPage.call(this);
            }

            if (perPage == null) {
                this._pages = this._calculatePages();
            } else {
                var pp    = parseInt(perPage, 10) || 0,
                    items = this._getCarouselItems(),
                    page  = 1,
                    i     = 0,
                    curr;

                while (true) {
                    curr = items.eq(i++);

                    if (curr.length === 0) {
                        break;
                    }

                    if (!this._pages[page]) {
                        this._pages[page] = curr;
                    } else {
                        this._pages[page] = this._pages[page].add(curr);
                    }

                    if (i % pp === 0) {
                        page++;
                    }
                }
            }

            this._clear();

            var self     = this,
                carousel = this.carousel().data('jcarousel'),
                element  = this._element,
                item     = this.options('item'),
                numCarouselItems = this._getCarouselItems().length;

            $.each(this._pages, function(page, carouselItems) {
                var currItem = self._items[page] = $(item.call(self, page, carouselItems));

                currItem.on(self.options('event') + '.jcarouselpagination', $.proxy(function() {
                    var target = carouselItems.eq(0);

                    // If circular wrapping enabled, ensure correct scrolling direction
                    if (carousel.circular) {
                        var currentIndex = carousel.index(carousel.target()),
                            newIndex     = carousel.index(target);

                        if (parseFloat(page) > parseFloat(self._currentPage)) {
                            if (newIndex < currentIndex) {
                                target = '+=' + (numCarouselItems - currentIndex + newIndex);
                            }
                        } else {
                            if (newIndex > currentIndex) {
                                target = '-=' + (currentIndex + (numCarouselItems - newIndex));
                            }
                        }
                    }

                    carousel[this.options('method')](target);
                }, self));

                element.append(currItem);
            });

            this._update();
        },
        _update: function() {
            var target = this.carousel().jcarousel('target'),
                currentPage;

            $.each(this._pages, function(page, carouselItems) {
                carouselItems.each(function() {
                    if (target.is(this)) {
                        currentPage = page;
                        return false;
                    }
                });

                if (currentPage) {
                    return false;
                }
            });

            if (this._currentPage !== currentPage) {
                this._trigger('inactive', this._items[this._currentPage]);
                this._trigger('active', this._items[currentPage]);
            }

            this._currentPage = currentPage;
        },
        items: function() {
            return this._items;
        },
        reloadCarouselItems: function() {
            this._carouselItems = null;
            return this;
        },
        _clear: function() {
            this._element.empty();
            this._currentPage = null;
        },
        _calculatePages: function() {
            var carousel = this.carousel().data('jcarousel'),
                items    = this._getCarouselItems(),
                clip     = carousel.clipping(),
                wh       = 0,
                idx      = 0,
                page     = 1,
                pages    = {},
                curr;

            while (true) {
                curr = items.eq(idx++);

                if (curr.length === 0) {
                    break;
                }

                if (!pages[page]) {
                    pages[page] = curr;
                } else {
                    pages[page] = pages[page].add(curr);
                }

                wh += carousel.dimension(curr);

                if (wh >= clip) {
                    page++;
                    wh = 0;
                }
            }

            return pages;
        },
        _getCarouselItems: function() {
            if (!this._carouselItems) {
                this._carouselItems = this.carousel().jcarousel('items');
            }

            return this._carouselItems;
        }
    });
}(jQuery));

(function($) {
    'use strict';

    $.jCarousel.plugin('jcarouselAutoscroll', {
        _options: {
            target:    '+=1',
            interval:  3000,
            autostart: true
        },
        _timer: null,
        _init: function () {
            this.onDestroy = $.proxy(function() {
                this._destroy();
                this.carousel()
                    .one('jcarousel:createend', $.proxy(this._create, this));
            }, this);

            this.onAnimateEnd = $.proxy(this.start, this);
        },
        _create: function() {
            this.carousel()
                .one('jcarousel:destroy', this.onDestroy);

            if (this.options('autostart')) {
                this.start();
            }
        },
        _destroy: function() {
            this.stop();
            this.carousel()
                .off('jcarousel:destroy', this.onDestroy);
        },
        start: function() {
            this.stop();

            this.carousel()
                .one('jcarousel:animateend', this.onAnimateEnd);

            this._timer = setTimeout($.proxy(function() {
                this.carousel().jcarousel('scroll', this.options('target'));
            }, this), this.options('interval'));

            return this;
        },
        stop: function() {
            if (this._timer) {
                this._timer = clearTimeout(this._timer);
            }

            this.carousel()
                .off('jcarousel:animateend', this.onAnimateEnd);

            return this;
        }
    });
}(jQuery));

(function ($)
{
    $(".jcarousel-engine").each(function ()
    {
        var engine = $(this);
        var autoStart = engine.data("autostart");
        var interval = engine.data("interval") || 3000;
        var transitions = engine.data("transitions");
        var easing = engine.data("easing");
        var wrap = engine.data("wrap");
        var vertical = engine.data("vertical");
        var center = engine.data("center");

        engine.find(".jcarousel")
            .on("jcarousel:create jcarousel:reload", function ()
            {
                var element = $(this);
                var width = element.innerWidth();
                element.jcarousel("items").css("width", width + "px");
            })
            .jcarousel({
                wrap: wrap,
                vertical: vertical,
                center: center,
                transitions: transitions ? {
                    transforms: Modernizr.csstransforms,
                    transforms3d: Modernizr.csstransforms3d,
                    easing: easing
                } : false
            })

        .jcarouselAutoscroll({
            interval: interval,
            target: "+=1",
            autostart: autoStart
        });

        engine.find(".jcarousel-prev").jcarouselControl({
            target: "-=1"
        });

        engine.find(".jcarousel-next").jcarouselControl({
            target: "+=1"
        });

        engine.find(".jcarousel-pagination")
            .on("jcarouselpagination:active", "a", function ()
            {
                $(this).addClass("active");
            })
            .on("jcarouselpagination:inactive", "a", function ()
            {
                $(this).removeClass("active");
            })
            .on("click", function (e)
            {
                e.preventDefault();
            })
            .jcarouselPagination({
                item: function (page)
                {
                    return "<a href=\"#" + page + "\">" + page + "</a>";
                }
            });
    });
})(jQuery);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm1vZGVybml6ci50cmFuc2l0aW9ucy5qcyIsImpxdWVyeS5qY2Fyb3VzZWwuanMiLCJlbmdpbmUtamNhcm91c2VsLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ240Q0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6IkVuZ2luZS5KQ2Fyb3VzZWwuanMiLCJzb3VyY2VzQ29udGVudCI6WyIvKiBNb2Rlcm5penIgMi44LjMgKEN1c3RvbSBCdWlsZCkgfCBNSVQgJiBCU0RcclxuICogQnVpbGQ6IGh0dHA6Ly9tb2Rlcm5penIuY29tL2Rvd25sb2FkLyMtY3NzdHJhbnNmb3Jtcy1jc3N0cmFuc2Zvcm1zM2QtY3NzdHJhbnNpdGlvbnMtdGVzdHN0eWxlcy10ZXN0cHJvcC10ZXN0YWxscHJvcHMtcHJlZml4ZXMtZG9tcHJlZml4ZXNcclxuICovXHJcbjt3aW5kb3cuTW9kZXJuaXpyPWZ1bmN0aW9uKGEsYixjKXtmdW5jdGlvbiB5KGEpe2kuY3NzVGV4dD1hfWZ1bmN0aW9uIHooYSxiKXtyZXR1cm4geShsLmpvaW4oYStcIjtcIikrKGJ8fFwiXCIpKX1mdW5jdGlvbiBBKGEsYil7cmV0dXJuIHR5cGVvZiBhPT09Yn1mdW5jdGlvbiBCKGEsYil7cmV0dXJuISF+KFwiXCIrYSkuaW5kZXhPZihiKX1mdW5jdGlvbiBDKGEsYil7Zm9yKHZhciBkIGluIGEpe3ZhciBlPWFbZF07aWYoIUIoZSxcIi1cIikmJmlbZV0hPT1jKXJldHVybiBiPT1cInBmeFwiP2U6ITB9cmV0dXJuITF9ZnVuY3Rpb24gRChhLGIsZCl7Zm9yKHZhciBlIGluIGEpe3ZhciBmPWJbYVtlXV07aWYoZiE9PWMpcmV0dXJuIGQ9PT0hMT9hW2VdOkEoZixcImZ1bmN0aW9uXCIpP2YuYmluZChkfHxiKTpmfXJldHVybiExfWZ1bmN0aW9uIEUoYSxiLGMpe3ZhciBkPWEuY2hhckF0KDApLnRvVXBwZXJDYXNlKCkrYS5zbGljZSgxKSxlPShhK1wiIFwiK24uam9pbihkK1wiIFwiKStkKS5zcGxpdChcIiBcIik7cmV0dXJuIEEoYixcInN0cmluZ1wiKXx8QShiLFwidW5kZWZpbmVkXCIpP0MoZSxiKTooZT0oYStcIiBcIitvLmpvaW4oZCtcIiBcIikrZCkuc3BsaXQoXCIgXCIpLEQoZSxiLGMpKX12YXIgZD1cIjIuOC4zXCIsZT17fSxmPWIuZG9jdW1lbnRFbGVtZW50LGc9XCJtb2Rlcm5penJcIixoPWIuY3JlYXRlRWxlbWVudChnKSxpPWguc3R5bGUsaixrPXt9LnRvU3RyaW5nLGw9XCIgLXdlYmtpdC0gLW1vei0gLW8tIC1tcy0gXCIuc3BsaXQoXCIgXCIpLG09XCJXZWJraXQgTW96IE8gbXNcIixuPW0uc3BsaXQoXCIgXCIpLG89bS50b0xvd2VyQ2FzZSgpLnNwbGl0KFwiIFwiKSxwPXt9LHE9e30scj17fSxzPVtdLHQ9cy5zbGljZSx1LHY9ZnVuY3Rpb24oYSxjLGQsZSl7dmFyIGgsaSxqLGssbD1iLmNyZWF0ZUVsZW1lbnQoXCJkaXZcIiksbT1iLmJvZHksbj1tfHxiLmNyZWF0ZUVsZW1lbnQoXCJib2R5XCIpO2lmKHBhcnNlSW50KGQsMTApKXdoaWxlKGQtLSlqPWIuY3JlYXRlRWxlbWVudChcImRpdlwiKSxqLmlkPWU/ZVtkXTpnKyhkKzEpLGwuYXBwZW5kQ2hpbGQoaik7cmV0dXJuIGg9W1wiJiMxNzM7XCIsJzxzdHlsZSBpZD1cInMnLGcsJ1wiPicsYSxcIjwvc3R5bGU+XCJdLmpvaW4oXCJcIiksbC5pZD1nLChtP2w6bikuaW5uZXJIVE1MKz1oLG4uYXBwZW5kQ2hpbGQobCksbXx8KG4uc3R5bGUuYmFja2dyb3VuZD1cIlwiLG4uc3R5bGUub3ZlcmZsb3c9XCJoaWRkZW5cIixrPWYuc3R5bGUub3ZlcmZsb3csZi5zdHlsZS5vdmVyZmxvdz1cImhpZGRlblwiLGYuYXBwZW5kQ2hpbGQobikpLGk9YyhsLGEpLG0/bC5wYXJlbnROb2RlLnJlbW92ZUNoaWxkKGwpOihuLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQobiksZi5zdHlsZS5vdmVyZmxvdz1rKSwhIWl9LHc9e30uaGFzT3duUHJvcGVydHkseDshQSh3LFwidW5kZWZpbmVkXCIpJiYhQSh3LmNhbGwsXCJ1bmRlZmluZWRcIik/eD1mdW5jdGlvbihhLGIpe3JldHVybiB3LmNhbGwoYSxiKX06eD1mdW5jdGlvbihhLGIpe3JldHVybiBiIGluIGEmJkEoYS5jb25zdHJ1Y3Rvci5wcm90b3R5cGVbYl0sXCJ1bmRlZmluZWRcIil9LEZ1bmN0aW9uLnByb3RvdHlwZS5iaW5kfHwoRnVuY3Rpb24ucHJvdG90eXBlLmJpbmQ9ZnVuY3Rpb24oYil7dmFyIGM9dGhpcztpZih0eXBlb2YgYyE9XCJmdW5jdGlvblwiKXRocm93IG5ldyBUeXBlRXJyb3I7dmFyIGQ9dC5jYWxsKGFyZ3VtZW50cywxKSxlPWZ1bmN0aW9uKCl7aWYodGhpcyBpbnN0YW5jZW9mIGUpe3ZhciBhPWZ1bmN0aW9uKCl7fTthLnByb3RvdHlwZT1jLnByb3RvdHlwZTt2YXIgZj1uZXcgYSxnPWMuYXBwbHkoZixkLmNvbmNhdCh0LmNhbGwoYXJndW1lbnRzKSkpO3JldHVybiBPYmplY3QoZyk9PT1nP2c6Zn1yZXR1cm4gYy5hcHBseShiLGQuY29uY2F0KHQuY2FsbChhcmd1bWVudHMpKSl9O3JldHVybiBlfSkscC5jc3N0cmFuc2Zvcm1zPWZ1bmN0aW9uKCl7cmV0dXJuISFFKFwidHJhbnNmb3JtXCIpfSxwLmNzc3RyYW5zZm9ybXMzZD1mdW5jdGlvbigpe3ZhciBhPSEhRShcInBlcnNwZWN0aXZlXCIpO3JldHVybiBhJiZcIndlYmtpdFBlcnNwZWN0aXZlXCJpbiBmLnN0eWxlJiZ2KFwiQG1lZGlhICh0cmFuc2Zvcm0tM2QpLCgtd2Via2l0LXRyYW5zZm9ybS0zZCl7I21vZGVybml6cntsZWZ0OjlweDtwb3NpdGlvbjphYnNvbHV0ZTtoZWlnaHQ6M3B4O319XCIsZnVuY3Rpb24oYixjKXthPWIub2Zmc2V0TGVmdD09PTkmJmIub2Zmc2V0SGVpZ2h0PT09M30pLGF9LHAuY3NzdHJhbnNpdGlvbnM9ZnVuY3Rpb24oKXtyZXR1cm4gRShcInRyYW5zaXRpb25cIil9O2Zvcih2YXIgRiBpbiBwKXgocCxGKSYmKHU9Ri50b0xvd2VyQ2FzZSgpLGVbdV09cFtGXSgpLHMucHVzaCgoZVt1XT9cIlwiOlwibm8tXCIpK3UpKTtyZXR1cm4gZS5hZGRUZXN0PWZ1bmN0aW9uKGEsYil7aWYodHlwZW9mIGE9PVwib2JqZWN0XCIpZm9yKHZhciBkIGluIGEpeChhLGQpJiZlLmFkZFRlc3QoZCxhW2RdKTtlbHNle2E9YS50b0xvd2VyQ2FzZSgpO2lmKGVbYV0hPT1jKXJldHVybiBlO2I9dHlwZW9mIGI9PVwiZnVuY3Rpb25cIj9iKCk6Yix0eXBlb2YgZW5hYmxlQ2xhc3NlcyE9XCJ1bmRlZmluZWRcIiYmZW5hYmxlQ2xhc3NlcyYmKGYuY2xhc3NOYW1lKz1cIiBcIisoYj9cIlwiOlwibm8tXCIpK2EpLGVbYV09Yn1yZXR1cm4gZX0seShcIlwiKSxoPWo9bnVsbCxlLl92ZXJzaW9uPWQsZS5fcHJlZml4ZXM9bCxlLl9kb21QcmVmaXhlcz1vLGUuX2Nzc29tUHJlZml4ZXM9bixlLnRlc3RQcm9wPWZ1bmN0aW9uKGEpe3JldHVybiBDKFthXSl9LGUudGVzdEFsbFByb3BzPUUsZS50ZXN0U3R5bGVzPXYsZX0odGhpcyx0aGlzLmRvY3VtZW50KTsiLCIvKiEgakNhcm91c2VsIC0gdjAuMy4xIC0gMjAxNC0wNC0yNlxyXG4qIGh0dHA6Ly9zb3JnYWxsYS5jb20vamNhcm91c2VsXHJcbiogQ29weXJpZ2h0IChjKSAyMDE0IEphbiBTb3JnYWxsYTsgTGljZW5zZWQgTUlUICovXHJcbihmdW5jdGlvbigkKSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcblxyXG4gICAgdmFyIGpDYXJvdXNlbCA9ICQuakNhcm91c2VsID0ge307XHJcblxyXG4gICAgakNhcm91c2VsLnZlcnNpb24gPSAnMC4zLjEnO1xyXG5cclxuICAgIHZhciByUmVsYXRpdmVUYXJnZXQgPSAvXihbK1xcLV09KT8oLispJC87XHJcblxyXG4gICAgakNhcm91c2VsLnBhcnNlVGFyZ2V0ID0gZnVuY3Rpb24odGFyZ2V0KSB7XHJcbiAgICAgICAgdmFyIHJlbGF0aXZlID0gZmFsc2UsXHJcbiAgICAgICAgICAgIHBhcnRzICAgID0gdHlwZW9mIHRhcmdldCAhPT0gJ29iamVjdCcgP1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICByUmVsYXRpdmVUYXJnZXQuZXhlYyh0YXJnZXQpIDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgbnVsbDtcclxuXHJcbiAgICAgICAgaWYgKHBhcnRzKSB7XHJcbiAgICAgICAgICAgIHRhcmdldCA9IHBhcnNlSW50KHBhcnRzWzJdLCAxMCkgfHwgMDtcclxuXHJcbiAgICAgICAgICAgIGlmIChwYXJ0c1sxXSkge1xyXG4gICAgICAgICAgICAgICAgcmVsYXRpdmUgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgaWYgKHBhcnRzWzFdID09PSAnLT0nKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGFyZ2V0ICo9IC0xO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSBlbHNlIGlmICh0eXBlb2YgdGFyZ2V0ICE9PSAnb2JqZWN0Jykge1xyXG4gICAgICAgICAgICB0YXJnZXQgPSBwYXJzZUludCh0YXJnZXQsIDEwKSB8fCAwO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgdGFyZ2V0OiB0YXJnZXQsXHJcbiAgICAgICAgICAgIHJlbGF0aXZlOiByZWxhdGl2ZVxyXG4gICAgICAgIH07XHJcbiAgICB9O1xyXG5cclxuICAgIGpDYXJvdXNlbC5kZXRlY3RDYXJvdXNlbCA9IGZ1bmN0aW9uKGVsZW1lbnQpIHtcclxuICAgICAgICB2YXIgY2Fyb3VzZWw7XHJcblxyXG4gICAgICAgIHdoaWxlIChlbGVtZW50Lmxlbmd0aCA+IDApIHtcclxuICAgICAgICAgICAgY2Fyb3VzZWwgPSBlbGVtZW50LmZpbHRlcignW2RhdGEtamNhcm91c2VsXScpO1xyXG5cclxuICAgICAgICAgICAgaWYgKGNhcm91c2VsLmxlbmd0aCA+IDApIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBjYXJvdXNlbDtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgY2Fyb3VzZWwgPSBlbGVtZW50LmZpbmQoJ1tkYXRhLWpjYXJvdXNlbF0nKTtcclxuXHJcbiAgICAgICAgICAgIGlmIChjYXJvdXNlbC5sZW5ndGggPiAwKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gY2Fyb3VzZWw7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGVsZW1lbnQgPSBlbGVtZW50LnBhcmVudCgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcmV0dXJuIG51bGw7XHJcbiAgICB9O1xyXG5cclxuICAgIGpDYXJvdXNlbC5iYXNlID0gZnVuY3Rpb24ocGx1Z2luTmFtZSkge1xyXG4gICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgIHZlcnNpb246ICBqQ2Fyb3VzZWwudmVyc2lvbixcclxuICAgICAgICAgICAgX29wdGlvbnM6ICB7fSxcclxuICAgICAgICAgICAgX2VsZW1lbnQ6ICBudWxsLFxyXG4gICAgICAgICAgICBfY2Fyb3VzZWw6IG51bGwsXHJcbiAgICAgICAgICAgIF9pbml0OiAgICAgJC5ub29wLFxyXG4gICAgICAgICAgICBfY3JlYXRlOiAgICQubm9vcCxcclxuICAgICAgICAgICAgX2Rlc3Ryb3k6ICAkLm5vb3AsXHJcbiAgICAgICAgICAgIF9yZWxvYWQ6ICAgJC5ub29wLFxyXG4gICAgICAgICAgICBjcmVhdGU6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fZWxlbWVudFxyXG4gICAgICAgICAgICAgICAgICAgIC5hdHRyKCdkYXRhLScgKyBwbHVnaW5OYW1lLnRvTG93ZXJDYXNlKCksIHRydWUpXHJcbiAgICAgICAgICAgICAgICAgICAgLmRhdGEocGx1Z2luTmFtZSwgdGhpcyk7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKGZhbHNlID09PSB0aGlzLl90cmlnZ2VyKCdjcmVhdGUnKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHRoaXMuX2NyZWF0ZSgpO1xyXG5cclxuICAgICAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2NyZWF0ZWVuZCcpO1xyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBkZXN0cm95OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIGlmIChmYWxzZSA9PT0gdGhpcy5fdHJpZ2dlcignZGVzdHJveScpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy5fZGVzdHJveSgpO1xyXG5cclxuICAgICAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2Rlc3Ryb3llbmQnKTtcclxuXHJcbiAgICAgICAgICAgICAgICB0aGlzLl9lbGVtZW50XHJcbiAgICAgICAgICAgICAgICAgICAgLnJlbW92ZURhdGEocGx1Z2luTmFtZSlcclxuICAgICAgICAgICAgICAgICAgICAucmVtb3ZlQXR0cignZGF0YS0nICsgcGx1Z2luTmFtZS50b0xvd2VyQ2FzZSgpKTtcclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgcmVsb2FkOiBmdW5jdGlvbihvcHRpb25zKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoZmFsc2UgPT09IHRoaXMuX3RyaWdnZXIoJ3JlbG9hZCcpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKG9wdGlvbnMpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLm9wdGlvbnMob3B0aW9ucyk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy5fcmVsb2FkKCk7XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcigncmVsb2FkZW5kJyk7XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGVsZW1lbnQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2VsZW1lbnQ7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIG9wdGlvbnM6IGZ1bmN0aW9uKGtleSwgdmFsdWUpIHtcclxuICAgICAgICAgICAgICAgIGlmIChhcmd1bWVudHMubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuICQuZXh0ZW5kKHt9LCB0aGlzLl9vcHRpb25zKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICBpZiAodHlwZW9mIGtleSA9PT0gJ3N0cmluZycpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodHlwZW9mIHZhbHVlID09PSAndW5kZWZpbmVkJykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gdHlwZW9mIHRoaXMuX29wdGlvbnNba2V5XSA9PT0gJ3VuZGVmaW5lZCcgP1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG51bGwgOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX29wdGlvbnNba2V5XTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuX29wdGlvbnNba2V5XSA9IHZhbHVlO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLl9vcHRpb25zID0gJC5leHRlbmQoe30sIHRoaXMuX29wdGlvbnMsIGtleSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGNhcm91c2VsOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIGlmICghdGhpcy5fY2Fyb3VzZWwpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLl9jYXJvdXNlbCA9IGpDYXJvdXNlbC5kZXRlY3RDYXJvdXNlbCh0aGlzLm9wdGlvbnMoJ2Nhcm91c2VsJykgfHwgdGhpcy5fZWxlbWVudCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmICghdGhpcy5fY2Fyb3VzZWwpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgJC5lcnJvcignQ291bGQgbm90IGRldGVjdCBjYXJvdXNlbCBmb3IgcGx1Z2luIFwiJyArIHBsdWdpbk5hbWUgKyAnXCInKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2Nhcm91c2VsO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBfdHJpZ2dlcjogZnVuY3Rpb24odHlwZSwgZWxlbWVudCwgZGF0YSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGV2ZW50LFxyXG4gICAgICAgICAgICAgICAgICAgIGRlZmF1bHRQcmV2ZW50ZWQgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgICAgICAgICBkYXRhID0gW3RoaXNdLmNvbmNhdChkYXRhIHx8IFtdKTtcclxuXHJcbiAgICAgICAgICAgICAgICAoZWxlbWVudCB8fCB0aGlzLl9lbGVtZW50KS5lYWNoKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50ID0gJC5FdmVudCgocGx1Z2luTmFtZSArICc6JyArIHR5cGUpLnRvTG93ZXJDYXNlKCkpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAkKHRoaXMpLnRyaWdnZXIoZXZlbnQsIGRhdGEpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoZXZlbnQuaXNEZWZhdWx0UHJldmVudGVkKCkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdFByZXZlbnRlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuICFkZWZhdWx0UHJldmVudGVkO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG4gICAgakNhcm91c2VsLnBsdWdpbiA9IGZ1bmN0aW9uKHBsdWdpbk5hbWUsIHBsdWdpblByb3RvdHlwZSkge1xyXG4gICAgICAgIHZhciBQbHVnaW4gPSAkW3BsdWdpbk5hbWVdID0gZnVuY3Rpb24oZWxlbWVudCwgb3B0aW9ucykge1xyXG4gICAgICAgICAgICB0aGlzLl9lbGVtZW50ID0gJChlbGVtZW50KTtcclxuICAgICAgICAgICAgdGhpcy5vcHRpb25zKG9wdGlvbnMpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5faW5pdCgpO1xyXG4gICAgICAgICAgICB0aGlzLmNyZWF0ZSgpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIFBsdWdpbi5mbiA9IFBsdWdpbi5wcm90b3R5cGUgPSAkLmV4dGVuZChcclxuICAgICAgICAgICAge30sXHJcbiAgICAgICAgICAgIGpDYXJvdXNlbC5iYXNlKHBsdWdpbk5hbWUpLFxyXG4gICAgICAgICAgICBwbHVnaW5Qcm90b3R5cGVcclxuICAgICAgICApO1xyXG5cclxuICAgICAgICAkLmZuW3BsdWdpbk5hbWVdID0gZnVuY3Rpb24ob3B0aW9ucykge1xyXG4gICAgICAgICAgICB2YXIgYXJncyAgICAgICAgPSBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChhcmd1bWVudHMsIDEpLFxyXG4gICAgICAgICAgICAgICAgcmV0dXJuVmFsdWUgPSB0aGlzO1xyXG5cclxuICAgICAgICAgICAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnc3RyaW5nJykge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5lYWNoKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBpbnN0YW5jZSA9ICQodGhpcykuZGF0YShwbHVnaW5OYW1lKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCFpbnN0YW5jZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gJC5lcnJvcihcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICdDYW5ub3QgY2FsbCBtZXRob2RzIG9uICcgKyBwbHVnaW5OYW1lICsgJyBwcmlvciB0byBpbml0aWFsaXphdGlvbjsgJyArXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAnYXR0ZW1wdGVkIHRvIGNhbGwgbWV0aG9kIFwiJyArIG9wdGlvbnMgKyAnXCInXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoISQuaXNGdW5jdGlvbihpbnN0YW5jZVtvcHRpb25zXSkgfHwgb3B0aW9ucy5jaGFyQXQoMCkgPT09ICdfJykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gJC5lcnJvcihcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICdObyBzdWNoIG1ldGhvZCBcIicgKyBvcHRpb25zICsgJ1wiIGZvciAnICsgcGx1Z2luTmFtZSArICcgaW5zdGFuY2UnXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICB2YXIgbWV0aG9kVmFsdWUgPSBpbnN0YW5jZVtvcHRpb25zXS5hcHBseShpbnN0YW5jZSwgYXJncyk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChtZXRob2RWYWx1ZSAhPT0gaW5zdGFuY2UgJiYgdHlwZW9mIG1ldGhvZFZhbHVlICE9PSAndW5kZWZpbmVkJykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm5WYWx1ZSA9IG1ldGhvZFZhbHVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmVhY2goZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGluc3RhbmNlID0gJCh0aGlzKS5kYXRhKHBsdWdpbk5hbWUpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoaW5zdGFuY2UgaW5zdGFuY2VvZiBQbHVnaW4pIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaW5zdGFuY2UucmVsb2FkKG9wdGlvbnMpO1xyXG4gICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG5ldyBQbHVnaW4odGhpcywgb3B0aW9ucyk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiByZXR1cm5WYWx1ZTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICByZXR1cm4gUGx1Z2luO1xyXG4gICAgfTtcclxufShqUXVlcnkpKTtcclxuXHJcbihmdW5jdGlvbigkLCB3aW5kb3cpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICB2YXIgdG9GbG9hdCA9IGZ1bmN0aW9uKHZhbCkge1xyXG4gICAgICAgIHJldHVybiBwYXJzZUZsb2F0KHZhbCkgfHwgMDtcclxuICAgIH07XHJcblxyXG4gICAgJC5qQ2Fyb3VzZWwucGx1Z2luKCdqY2Fyb3VzZWwnLCB7XHJcbiAgICAgICAgYW5pbWF0aW5nOiAgIGZhbHNlLFxyXG4gICAgICAgIHRhaWw6ICAgICAgICAwLFxyXG4gICAgICAgIGluVGFpbDogICAgICBmYWxzZSxcclxuICAgICAgICByZXNpemVUaW1lcjogbnVsbCxcclxuICAgICAgICBsdDogICAgICAgICAgbnVsbCxcclxuICAgICAgICB2ZXJ0aWNhbDogICAgZmFsc2UsXHJcbiAgICAgICAgcnRsOiAgICAgICAgIGZhbHNlLFxyXG4gICAgICAgIGNpcmN1bGFyOiAgICBmYWxzZSxcclxuICAgICAgICB1bmRlcmZsb3c6ICAgZmFsc2UsXHJcbiAgICAgICAgcmVsYXRpdmU6ICAgIGZhbHNlLFxyXG5cclxuICAgICAgICBfb3B0aW9uczoge1xyXG4gICAgICAgICAgICBsaXN0OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLmVsZW1lbnQoKS5jaGlsZHJlbigpLmVxKDApO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBpdGVtczogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5saXN0KCkuY2hpbGRyZW4oKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgYW5pbWF0aW9uOiAgIDQwMCxcclxuICAgICAgICAgICAgdHJhbnNpdGlvbnM6IGZhbHNlLFxyXG4gICAgICAgICAgICB3cmFwOiAgICAgICAgbnVsbCxcclxuICAgICAgICAgICAgdmVydGljYWw6ICAgIG51bGwsXHJcbiAgICAgICAgICAgIHJ0bDogICAgICAgICBudWxsLFxyXG4gICAgICAgICAgICBjZW50ZXI6ICAgICAgZmFsc2VcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICAvLyBQcm90ZWN0ZWQsIGRvbid0IGFjY2VzcyBkaXJlY3RseVxyXG4gICAgICAgIF9saXN0OiAgICAgICAgIG51bGwsXHJcbiAgICAgICAgX2l0ZW1zOiAgICAgICAgbnVsbCxcclxuICAgICAgICBfdGFyZ2V0OiAgICAgICBudWxsLFxyXG4gICAgICAgIF9maXJzdDogICAgICAgIG51bGwsXHJcbiAgICAgICAgX2xhc3Q6ICAgICAgICAgbnVsbCxcclxuICAgICAgICBfdmlzaWJsZTogICAgICBudWxsLFxyXG4gICAgICAgIF9mdWxseXZpc2libGU6IG51bGwsXHJcbiAgICAgICAgX2luaXQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgc2VsZiA9IHRoaXM7XHJcblxyXG4gICAgICAgICAgICB0aGlzLm9uV2luZG93UmVzaXplID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoc2VsZi5yZXNpemVUaW1lcikge1xyXG4gICAgICAgICAgICAgICAgICAgIGNsZWFyVGltZW91dChzZWxmLnJlc2l6ZVRpbWVyKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICBzZWxmLnJlc2l6ZVRpbWVyID0gc2V0VGltZW91dChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICBzZWxmLnJlbG9hZCgpO1xyXG4gICAgICAgICAgICAgICAgfSwgMTAwKTtcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2NyZWF0ZTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuX3JlbG9hZCgpO1xyXG5cclxuICAgICAgICAgICAgJCh3aW5kb3cpLm9uKCdyZXNpemUuamNhcm91c2VsJywgdGhpcy5vbldpbmRvd1Jlc2l6ZSk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfZGVzdHJveTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICQod2luZG93KS5vZmYoJ3Jlc2l6ZS5qY2Fyb3VzZWwnLCB0aGlzLm9uV2luZG93UmVzaXplKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9yZWxvYWQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLnZlcnRpY2FsID0gdGhpcy5vcHRpb25zKCd2ZXJ0aWNhbCcpO1xyXG5cclxuICAgICAgICAgICAgaWYgKHRoaXMudmVydGljYWwgPT0gbnVsbCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy52ZXJ0aWNhbCA9IHRoaXMubGlzdCgpLmhlaWdodCgpID4gdGhpcy5saXN0KCkud2lkdGgoKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdGhpcy5ydGwgPSB0aGlzLm9wdGlvbnMoJ3J0bCcpO1xyXG5cclxuICAgICAgICAgICAgaWYgKHRoaXMucnRsID09IG51bGwpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMucnRsID0gKGZ1bmN0aW9uKGVsZW1lbnQpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoKCcnICsgZWxlbWVudC5hdHRyKCdkaXInKSkudG9Mb3dlckNhc2UoKSA9PT0gJ3J0bCcpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICB2YXIgZm91bmQgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgZWxlbWVudC5wYXJlbnRzKCdbZGlyXScpLmVhY2goZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICgoL3J0bC9pKS50ZXN0KCQodGhpcykuYXR0cignZGlyJykpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb3VuZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZvdW5kO1xyXG4gICAgICAgICAgICAgICAgfSh0aGlzLl9lbGVtZW50KSk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMubHQgPSB0aGlzLnZlcnRpY2FsID8gJ3RvcCcgOiAnbGVmdCc7XHJcblxyXG4gICAgICAgICAgICAvLyBFbnN1cmUgYmVmb3JlIGNsb3Nlc3QoKSBjYWxsXHJcbiAgICAgICAgICAgIHRoaXMucmVsYXRpdmUgPSB0aGlzLmxpc3QoKS5jc3MoJ3Bvc2l0aW9uJykgPT09ICdyZWxhdGl2ZSc7XHJcblxyXG4gICAgICAgICAgICAvLyBGb3JjZSBsaXN0IGFuZCBpdGVtcyByZWxvYWRcclxuICAgICAgICAgICAgdGhpcy5fbGlzdCAgPSBudWxsO1xyXG4gICAgICAgICAgICB0aGlzLl9pdGVtcyA9IG51bGw7XHJcblxyXG4gICAgICAgICAgICB2YXIgaXRlbSA9IHRoaXMuX3RhcmdldCAmJiB0aGlzLmluZGV4KHRoaXMuX3RhcmdldCkgPj0gMCA/XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3RhcmdldCA6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY2xvc2VzdCgpO1xyXG5cclxuICAgICAgICAgICAgLy8gX3ByZXBhcmUoKSBuZWVkcyB0aGlzIGhlcmVcclxuICAgICAgICAgICAgdGhpcy5jaXJjdWxhciAgPSB0aGlzLm9wdGlvbnMoJ3dyYXAnKSA9PT0gJ2NpcmN1bGFyJztcclxuICAgICAgICAgICAgdGhpcy51bmRlcmZsb3cgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgICAgIHZhciBwcm9wcyA9IHsnbGVmdCc6IDAsICd0b3AnOiAwfTtcclxuXHJcbiAgICAgICAgICAgIGlmIChpdGVtLmxlbmd0aCA+IDApIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3ByZXBhcmUoaXRlbSk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmxpc3QoKS5maW5kKCdbZGF0YS1qY2Fyb3VzZWwtY2xvbmVdJykucmVtb3ZlKCk7XHJcblxyXG4gICAgICAgICAgICAgICAgLy8gRm9yY2UgaXRlbXMgcmVsb2FkXHJcbiAgICAgICAgICAgICAgICB0aGlzLl9pdGVtcyA9IG51bGw7XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy51bmRlcmZsb3cgPSB0aGlzLl9mdWxseXZpc2libGUubGVuZ3RoID49IHRoaXMuaXRlbXMoKS5sZW5ndGg7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmNpcmN1bGFyICA9IHRoaXMuY2lyY3VsYXIgJiYgIXRoaXMudW5kZXJmbG93O1xyXG5cclxuICAgICAgICAgICAgICAgIHByb3BzW3RoaXMubHRdID0gdGhpcy5fcG9zaXRpb24oaXRlbSkgKyAncHgnO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLm1vdmUocHJvcHMpO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBsaXN0OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuX2xpc3QgPT09IG51bGwpIHtcclxuICAgICAgICAgICAgICAgIHZhciBvcHRpb24gPSB0aGlzLm9wdGlvbnMoJ2xpc3QnKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2xpc3QgPSAkLmlzRnVuY3Rpb24ob3B0aW9uKSA/IG9wdGlvbi5jYWxsKHRoaXMpIDogdGhpcy5fZWxlbWVudC5maW5kKG9wdGlvbik7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9saXN0O1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgaXRlbXM6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5faXRlbXMgPT09IG51bGwpIHtcclxuICAgICAgICAgICAgICAgIHZhciBvcHRpb24gPSB0aGlzLm9wdGlvbnMoJ2l0ZW1zJyk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9pdGVtcyA9ICgkLmlzRnVuY3Rpb24ob3B0aW9uKSA/IG9wdGlvbi5jYWxsKHRoaXMpIDogdGhpcy5saXN0KCkuZmluZChvcHRpb24pKS5ub3QoJ1tkYXRhLWpjYXJvdXNlbC1jbG9uZV0nKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2l0ZW1zO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgaW5kZXg6IGZ1bmN0aW9uKGl0ZW0pIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuaXRlbXMoKS5pbmRleChpdGVtKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGNsb3Nlc3Q6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgc2VsZiAgICA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICBwb3MgICAgID0gdGhpcy5saXN0KCkucG9zaXRpb24oKVt0aGlzLmx0XSxcclxuICAgICAgICAgICAgICAgIGNsb3Nlc3QgPSAkKCksIC8vIEVuc3VyZSB3ZSdyZSByZXR1cm5pbmcgYSBqUXVlcnkgaW5zdGFuY2VcclxuICAgICAgICAgICAgICAgIHN0b3AgICAgPSBmYWxzZSxcclxuICAgICAgICAgICAgICAgIGxyYiAgICAgPSB0aGlzLnZlcnRpY2FsID8gJ2JvdHRvbScgOiAodGhpcy5ydGwgJiYgIXRoaXMucmVsYXRpdmUgPyAnbGVmdCcgOiAncmlnaHQnKSxcclxuICAgICAgICAgICAgICAgIHdpZHRoO1xyXG5cclxuICAgICAgICAgICAgaWYgKHRoaXMucnRsICYmIHRoaXMucmVsYXRpdmUgJiYgIXRoaXMudmVydGljYWwpIHtcclxuICAgICAgICAgICAgICAgIHBvcyArPSB0aGlzLmxpc3QoKS53aWR0aCgpIC0gdGhpcy5jbGlwcGluZygpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLml0ZW1zKCkuZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIGNsb3Nlc3QgPSAkKHRoaXMpO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmIChzdG9wKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHZhciBkaW0gPSBzZWxmLmRpbWVuc2lvbihjbG9zZXN0KTtcclxuXHJcbiAgICAgICAgICAgICAgICBwb3MgKz0gZGltO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmIChwb3MgPj0gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHdpZHRoID0gZGltIC0gdG9GbG9hdChjbG9zZXN0LmNzcygnbWFyZ2luLScgKyBscmIpKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKChNYXRoLmFicyhwb3MpIC0gZGltICsgKHdpZHRoIC8gMikpIDw9IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgc3RvcCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcblxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIGNsb3Nlc3Q7XHJcbiAgICAgICAgfSxcclxuICAgICAgICB0YXJnZXQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fdGFyZ2V0O1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgZmlyc3Q6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fZmlyc3Q7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBsYXN0OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2xhc3Q7XHJcbiAgICAgICAgfSxcclxuICAgICAgICB2aXNpYmxlOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX3Zpc2libGU7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBmdWxseXZpc2libGU6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fZnVsbHl2aXNpYmxlO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgaGFzTmV4dDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGlmIChmYWxzZSA9PT0gdGhpcy5fdHJpZ2dlcignaGFzbmV4dCcpKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdHJ1ZTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdmFyIHdyYXAgPSB0aGlzLm9wdGlvbnMoJ3dyYXAnKSxcclxuICAgICAgICAgICAgICAgIGVuZCA9IHRoaXMuaXRlbXMoKS5sZW5ndGggLSAxO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIGVuZCA+PSAwICYmICF0aGlzLnVuZGVyZmxvdyAmJlxyXG4gICAgICAgICAgICAgICAgKCh3cmFwICYmIHdyYXAgIT09ICdmaXJzdCcpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgKHRoaXMuaW5kZXgodGhpcy5fbGFzdCkgPCBlbmQpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgKHRoaXMudGFpbCAmJiAhdGhpcy5pblRhaWwpKSA/IHRydWUgOiBmYWxzZTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGhhc1ByZXY6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBpZiAoZmFsc2UgPT09IHRoaXMuX3RyaWdnZXIoJ2hhc3ByZXYnKSkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHZhciB3cmFwID0gdGhpcy5vcHRpb25zKCd3cmFwJyk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5pdGVtcygpLmxlbmd0aCA+IDAgJiYgIXRoaXMudW5kZXJmbG93ICYmXHJcbiAgICAgICAgICAgICAgICAoKHdyYXAgJiYgd3JhcCAhPT0gJ2xhc3QnKSB8fFxyXG4gICAgICAgICAgICAgICAgICAgICh0aGlzLmluZGV4KHRoaXMuX2ZpcnN0KSA+IDApIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgKHRoaXMudGFpbCAmJiB0aGlzLmluVGFpbCkpID8gdHJ1ZSA6IGZhbHNlO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgY2xpcHBpbmc6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fZWxlbWVudFsnaW5uZXInICsgKHRoaXMudmVydGljYWwgPyAnSGVpZ2h0JyA6ICdXaWR0aCcpXSgpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgZGltZW5zaW9uOiBmdW5jdGlvbihlbGVtZW50KSB7XHJcbiAgICAgICAgICAgIHJldHVybiBlbGVtZW50WydvdXRlcicgKyAodGhpcy52ZXJ0aWNhbCA/ICdIZWlnaHQnIDogJ1dpZHRoJyldKHRydWUpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgc2Nyb2xsOiBmdW5jdGlvbih0YXJnZXQsIGFuaW1hdGUsIGNhbGxiYWNrKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmFuaW1hdGluZykge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmIChmYWxzZSA9PT0gdGhpcy5fdHJpZ2dlcignc2Nyb2xsJywgbnVsbCwgW3RhcmdldCwgYW5pbWF0ZV0pKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKCQuaXNGdW5jdGlvbihhbmltYXRlKSkge1xyXG4gICAgICAgICAgICAgICAgY2FsbGJhY2sgPSBhbmltYXRlO1xyXG4gICAgICAgICAgICAgICAgYW5pbWF0ZSAgPSB0cnVlO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB2YXIgcGFyc2VkID0gJC5qQ2Fyb3VzZWwucGFyc2VUYXJnZXQodGFyZ2V0KTtcclxuXHJcbiAgICAgICAgICAgIGlmIChwYXJzZWQucmVsYXRpdmUpIHtcclxuICAgICAgICAgICAgICAgIHZhciBlbmQgICAgPSB0aGlzLml0ZW1zKCkubGVuZ3RoIC0gMSxcclxuICAgICAgICAgICAgICAgICAgICBzY3JvbGwgPSBNYXRoLmFicyhwYXJzZWQudGFyZ2V0KSxcclxuICAgICAgICAgICAgICAgICAgICB3cmFwICAgPSB0aGlzLm9wdGlvbnMoJ3dyYXAnKSxcclxuICAgICAgICAgICAgICAgICAgICBjdXJyZW50LFxyXG4gICAgICAgICAgICAgICAgICAgIGZpcnN0LFxyXG4gICAgICAgICAgICAgICAgICAgIGluZGV4LFxyXG4gICAgICAgICAgICAgICAgICAgIHN0YXJ0LFxyXG4gICAgICAgICAgICAgICAgICAgIGN1cnIsXHJcbiAgICAgICAgICAgICAgICAgICAgaXNWaXNpYmxlLFxyXG4gICAgICAgICAgICAgICAgICAgIHByb3BzLFxyXG4gICAgICAgICAgICAgICAgICAgIGk7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKHBhcnNlZC50YXJnZXQgPiAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGxhc3QgPSB0aGlzLmluZGV4KHRoaXMuX2xhc3QpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAobGFzdCA+PSBlbmQgJiYgdGhpcy50YWlsKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICghdGhpcy5pblRhaWwpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbFRhaWwoYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHdyYXAgPT09ICdib3RoJyB8fCB3cmFwID09PSAnbGFzdCcpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9zY3JvbGwoMCwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKGNhbGxiYWNrKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjYWxsYmFjay5jYWxsKHRoaXMsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjdXJyZW50ID0gdGhpcy5pbmRleCh0aGlzLl90YXJnZXQpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCh0aGlzLnVuZGVyZmxvdyAmJiBjdXJyZW50ID09PSBlbmQgJiYgKHdyYXAgPT09ICdjaXJjdWxhcicgfHwgd3JhcCA9PT0gJ2JvdGgnIHx8IHdyYXAgPT09ICdsYXN0JykpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAoIXRoaXMudW5kZXJmbG93ICYmIGxhc3QgPT09IGVuZCAmJiAod3JhcCA9PT0gJ2JvdGgnIHx8IHdyYXAgPT09ICdsYXN0JykpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9zY3JvbGwoMCwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaW5kZXggPSBjdXJyZW50ICsgc2Nyb2xsO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmNpcmN1bGFyICYmIGluZGV4ID4gZW5kKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaSA9IGVuZDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJyID0gdGhpcy5pdGVtcygpLmdldCgtMSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdoaWxlIChpKysgPCBpbmRleCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJyID0gdGhpcy5pdGVtcygpLmVxKDApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpc1Zpc2libGUgPSB0aGlzLl92aXNpYmxlLmluZGV4KGN1cnIpID49IDA7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoaXNWaXNpYmxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJyLmFmdGVyKGN1cnIuY2xvbmUodHJ1ZSkuYXR0cignZGF0YS1qY2Fyb3VzZWwtY2xvbmUnLCB0cnVlKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMubGlzdCgpLmFwcGVuZChjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICghaXNWaXNpYmxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwcm9wcyA9IHt9O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJvcHNbdGhpcy5sdF0gPSB0aGlzLmRpbWVuc2lvbihjdXJyKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMubW92ZUJ5KHByb3BzKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gRm9yY2UgaXRlbXMgcmVsb2FkXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX2l0ZW1zID0gbnVsbDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbChjdXJyLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbChNYXRoLm1pbihpbmRleCwgZW5kKSwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5pblRhaWwpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKE1hdGgubWF4KCh0aGlzLmluZGV4KHRoaXMuX2ZpcnN0KSAtIHNjcm9sbCkgKyAxLCAwKSwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZpcnN0ICA9IHRoaXMuaW5kZXgodGhpcy5fZmlyc3QpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjdXJyZW50ID0gdGhpcy5pbmRleCh0aGlzLl90YXJnZXQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBzdGFydCAgPSB0aGlzLnVuZGVyZmxvdyA/IGN1cnJlbnQgOiBmaXJzdDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaW5kZXggID0gc3RhcnQgLSBzY3JvbGw7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoc3RhcnQgPD0gMCAmJiAoKHRoaXMudW5kZXJmbG93ICYmIHdyYXAgPT09ICdjaXJjdWxhcicpIHx8IHdyYXAgPT09ICdib3RoJyB8fCB3cmFwID09PSAnZmlyc3QnKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKGVuZCwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuY2lyY3VsYXIgJiYgaW5kZXggPCAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaSAgICA9IGluZGV4O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZ2V0KDApO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aGlsZSAoaSsrIDwgMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJyID0gdGhpcy5pdGVtcygpLmVxKC0xKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaXNWaXNpYmxlID0gdGhpcy5fdmlzaWJsZS5pbmRleChjdXJyKSA+PSAwO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGlzVmlzaWJsZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY3Vyci5hZnRlcihjdXJyLmNsb25lKHRydWUpLmF0dHIoJ2RhdGEtamNhcm91c2VsLWNsb25lJywgdHJ1ZSkpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmxpc3QoKS5wcmVwZW5kKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gRm9yY2UgaXRlbXMgcmVsb2FkXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX2l0ZW1zID0gbnVsbDtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciBkaW0gPSB0aGlzLmRpbWVuc2lvbihjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHByb3BzID0ge307XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHByb3BzW3RoaXMubHRdID0gLWRpbTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5tb3ZlQnkocHJvcHMpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbChjdXJyLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbChNYXRoLm1heChpbmRleCwgMCksIGFuaW1hdGUsIGNhbGxiYWNrKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbChwYXJzZWQudGFyZ2V0LCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ3Njcm9sbGVuZCcpO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBtb3ZlQnk6IGZ1bmN0aW9uKHByb3BlcnRpZXMsIG9wdHMpIHtcclxuICAgICAgICAgICAgdmFyIHBvc2l0aW9uID0gdGhpcy5saXN0KCkucG9zaXRpb24oKSxcclxuICAgICAgICAgICAgICAgIG11bHRpcGxpZXIgPSAxLFxyXG4gICAgICAgICAgICAgICAgY29ycmVjdGlvbiA9IDA7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5ydGwgJiYgIXRoaXMudmVydGljYWwpIHtcclxuICAgICAgICAgICAgICAgIG11bHRpcGxpZXIgPSAtMTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAodGhpcy5yZWxhdGl2ZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNvcnJlY3Rpb24gPSB0aGlzLmxpc3QoKS53aWR0aCgpIC0gdGhpcy5jbGlwcGluZygpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAocHJvcGVydGllcy5sZWZ0KSB7XHJcbiAgICAgICAgICAgICAgICBwcm9wZXJ0aWVzLmxlZnQgPSAocG9zaXRpb24ubGVmdCArIGNvcnJlY3Rpb24gKyB0b0Zsb2F0KHByb3BlcnRpZXMubGVmdCkgKiBtdWx0aXBsaWVyKSArICdweCc7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmIChwcm9wZXJ0aWVzLnRvcCkge1xyXG4gICAgICAgICAgICAgICAgcHJvcGVydGllcy50b3AgPSAocG9zaXRpb24udG9wICsgY29ycmVjdGlvbiArIHRvRmxvYXQocHJvcGVydGllcy50b3ApICogbXVsdGlwbGllcikgKyAncHgnO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5tb3ZlKHByb3BlcnRpZXMsIG9wdHMpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgbW92ZTogZnVuY3Rpb24ocHJvcGVydGllcywgb3B0cykge1xyXG4gICAgICAgICAgICBvcHRzID0gb3B0cyB8fCB7fTtcclxuXHJcbiAgICAgICAgICAgIHZhciBvcHRpb24gICAgICAgPSB0aGlzLm9wdGlvbnMoJ3RyYW5zaXRpb25zJyksXHJcbiAgICAgICAgICAgICAgICB0cmFuc2l0aW9ucyAgPSAhIW9wdGlvbixcclxuICAgICAgICAgICAgICAgIHRyYW5zZm9ybXMgICA9ICEhb3B0aW9uLnRyYW5zZm9ybXMsXHJcbiAgICAgICAgICAgICAgICB0cmFuc2Zvcm1zM2QgPSAhIW9wdGlvbi50cmFuc2Zvcm1zM2QsXHJcbiAgICAgICAgICAgICAgICBkdXJhdGlvbiAgICAgPSBvcHRzLmR1cmF0aW9uIHx8IDAsXHJcbiAgICAgICAgICAgICAgICBsaXN0ICAgICAgICAgPSB0aGlzLmxpc3QoKTtcclxuXHJcbiAgICAgICAgICAgIGlmICghdHJhbnNpdGlvbnMgJiYgZHVyYXRpb24gPiAwKSB7XHJcbiAgICAgICAgICAgICAgICBsaXN0LmFuaW1hdGUocHJvcGVydGllcywgb3B0cyk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHZhciBjb21wbGV0ZSA9IG9wdHMuY29tcGxldGUgfHwgJC5ub29wLFxyXG4gICAgICAgICAgICAgICAgY3NzID0ge307XHJcblxyXG4gICAgICAgICAgICBpZiAodHJhbnNpdGlvbnMpIHtcclxuICAgICAgICAgICAgICAgIHZhciBiYWNrdXAgPSBsaXN0LmNzcyhbJ3RyYW5zaXRpb25EdXJhdGlvbicsICd0cmFuc2l0aW9uVGltaW5nRnVuY3Rpb24nLCAndHJhbnNpdGlvblByb3BlcnR5J10pLFxyXG4gICAgICAgICAgICAgICAgICAgIG9sZENvbXBsZXRlID0gY29tcGxldGU7XHJcblxyXG4gICAgICAgICAgICAgICAgY29tcGxldGUgPSBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICAkKHRoaXMpLmNzcyhiYWNrdXApO1xyXG4gICAgICAgICAgICAgICAgICAgIG9sZENvbXBsZXRlLmNhbGwodGhpcyk7XHJcbiAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICAgICAgY3NzID0ge1xyXG4gICAgICAgICAgICAgICAgICAgIHRyYW5zaXRpb25EdXJhdGlvbjogKGR1cmF0aW9uID4gMCA/IGR1cmF0aW9uIC8gMTAwMCA6IDApICsgJ3MnLFxyXG4gICAgICAgICAgICAgICAgICAgIHRyYW5zaXRpb25UaW1pbmdGdW5jdGlvbjogb3B0aW9uLmVhc2luZyB8fCBvcHRzLmVhc2luZyxcclxuICAgICAgICAgICAgICAgICAgICB0cmFuc2l0aW9uUHJvcGVydHk6IGR1cmF0aW9uID4gMCA/IChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHRyYW5zZm9ybXMgfHwgdHJhbnNmb3JtczNkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBXZSBoYXZlIHRvIHVzZSAnYWxsJyBiZWNhdXNlIGpRdWVyeSBkb2Vzbid0IHByZWZpeFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gY3NzIHZhbHVlcywgbGlrZSB0cmFuc2l0aW9uLXByb3BlcnR5OiB0cmFuc2Zvcm07XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gJ2FsbCc7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBwcm9wZXJ0aWVzLmxlZnQgPyAnbGVmdCcgOiAndG9wJztcclxuICAgICAgICAgICAgICAgICAgICB9KSgpIDogJ25vbmUnLFxyXG4gICAgICAgICAgICAgICAgICAgIHRyYW5zZm9ybTogJ25vbmUnXHJcbiAgICAgICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAodHJhbnNmb3JtczNkKSB7XHJcbiAgICAgICAgICAgICAgICBjc3MudHJhbnNmb3JtID0gJ3RyYW5zbGF0ZTNkKCcgKyAocHJvcGVydGllcy5sZWZ0IHx8IDApICsgJywnICsgKHByb3BlcnRpZXMudG9wIHx8IDApICsgJywwKSc7XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAodHJhbnNmb3Jtcykge1xyXG4gICAgICAgICAgICAgICAgY3NzLnRyYW5zZm9ybSA9ICd0cmFuc2xhdGUoJyArIChwcm9wZXJ0aWVzLmxlZnQgfHwgMCkgKyAnLCcgKyAocHJvcGVydGllcy50b3AgfHwgMCkgKyAnKSc7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAkLmV4dGVuZChjc3MsIHByb3BlcnRpZXMpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAodHJhbnNpdGlvbnMgJiYgZHVyYXRpb24gPiAwKSB7XHJcbiAgICAgICAgICAgICAgICBsaXN0Lm9uZSgndHJhbnNpdGlvbmVuZCB3ZWJraXRUcmFuc2l0aW9uRW5kIG9UcmFuc2l0aW9uRW5kIG90cmFuc2l0aW9uZW5kIE1TVHJhbnNpdGlvbkVuZCcsIGNvbXBsZXRlKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgbGlzdC5jc3MoY3NzKTtcclxuXHJcbiAgICAgICAgICAgIGlmIChkdXJhdGlvbiA8PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBsaXN0LmVhY2goZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY29tcGxldGUuY2FsbCh0aGlzKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfc2Nyb2xsOiBmdW5jdGlvbihpdGVtLCBhbmltYXRlLCBjYWxsYmFjaykge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5hbmltYXRpbmcpIHtcclxuICAgICAgICAgICAgICAgIGlmICgkLmlzRnVuY3Rpb24oY2FsbGJhY2spKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2FsbGJhY2suY2FsbCh0aGlzLCBmYWxzZSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICh0eXBlb2YgaXRlbSAhPT0gJ29iamVjdCcpIHtcclxuICAgICAgICAgICAgICAgIGl0ZW0gPSB0aGlzLml0ZW1zKCkuZXEoaXRlbSk7XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAodHlwZW9mIGl0ZW0uanF1ZXJ5ID09PSAndW5kZWZpbmVkJykge1xyXG4gICAgICAgICAgICAgICAgaXRlbSA9ICQoaXRlbSk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmIChpdGVtLmxlbmd0aCA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKCQuaXNGdW5jdGlvbihjYWxsYmFjaykpIHtcclxuICAgICAgICAgICAgICAgICAgICBjYWxsYmFjay5jYWxsKHRoaXMsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdGhpcy5pblRhaWwgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3ByZXBhcmUoaXRlbSk7XHJcblxyXG4gICAgICAgICAgICB2YXIgcG9zICAgICA9IHRoaXMuX3Bvc2l0aW9uKGl0ZW0pLFxyXG4gICAgICAgICAgICAgICAgY3VyclBvcyA9IHRoaXMubGlzdCgpLnBvc2l0aW9uKClbdGhpcy5sdF07XHJcblxyXG4gICAgICAgICAgICBpZiAocG9zID09PSBjdXJyUG9zKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKGNhbGxiYWNrKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNhbGxiYWNrLmNhbGwodGhpcywgZmFsc2UpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB2YXIgcHJvcGVydGllcyA9IHt9O1xyXG4gICAgICAgICAgICBwcm9wZXJ0aWVzW3RoaXMubHRdID0gcG9zICsgJ3B4JztcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX2FuaW1hdGUocHJvcGVydGllcywgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfc2Nyb2xsVGFpbDogZnVuY3Rpb24oYW5pbWF0ZSwgY2FsbGJhY2spIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuYW5pbWF0aW5nIHx8ICF0aGlzLnRhaWwpIHtcclxuICAgICAgICAgICAgICAgIGlmICgkLmlzRnVuY3Rpb24oY2FsbGJhY2spKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2FsbGJhY2suY2FsbCh0aGlzLCBmYWxzZSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHZhciBwb3MgPSB0aGlzLmxpc3QoKS5wb3NpdGlvbigpW3RoaXMubHRdO1xyXG5cclxuICAgICAgICAgICAgaWYgKHRoaXMucnRsICYmIHRoaXMucmVsYXRpdmUgJiYgIXRoaXMudmVydGljYWwpIHtcclxuICAgICAgICAgICAgICAgIHBvcyArPSB0aGlzLmxpc3QoKS53aWR0aCgpIC0gdGhpcy5jbGlwcGluZygpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5ydGwgJiYgIXRoaXMudmVydGljYWwpIHtcclxuICAgICAgICAgICAgICAgIHBvcyArPSB0aGlzLnRhaWw7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICBwb3MgLT0gdGhpcy50YWlsO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLmluVGFpbCA9IHRydWU7XHJcblxyXG4gICAgICAgICAgICB2YXIgcHJvcGVydGllcyA9IHt9O1xyXG4gICAgICAgICAgICBwcm9wZXJ0aWVzW3RoaXMubHRdID0gcG9zICsgJ3B4JztcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3VwZGF0ZSh7XHJcbiAgICAgICAgICAgICAgICB0YXJnZXQ6ICAgICAgIHRoaXMuX3RhcmdldC5uZXh0KCksXHJcbiAgICAgICAgICAgICAgICBmdWxseXZpc2libGU6IHRoaXMuX2Z1bGx5dmlzaWJsZS5zbGljZSgxKS5hZGQodGhpcy5fdmlzaWJsZS5sYXN0KCkpXHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fYW5pbWF0ZShwcm9wZXJ0aWVzLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9hbmltYXRlOiBmdW5jdGlvbihwcm9wZXJ0aWVzLCBhbmltYXRlLCBjYWxsYmFjaykge1xyXG4gICAgICAgICAgICBjYWxsYmFjayA9IGNhbGxiYWNrIHx8ICQubm9vcDtcclxuXHJcbiAgICAgICAgICAgIGlmIChmYWxzZSA9PT0gdGhpcy5fdHJpZ2dlcignYW5pbWF0ZScpKSB7XHJcbiAgICAgICAgICAgICAgICBjYWxsYmFjay5jYWxsKHRoaXMsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLmFuaW1hdGluZyA9IHRydWU7XHJcblxyXG4gICAgICAgICAgICB2YXIgYW5pbWF0aW9uID0gdGhpcy5vcHRpb25zKCdhbmltYXRpb24nKSxcclxuICAgICAgICAgICAgICAgIGNvbXBsZXRlICA9ICQucHJveHkoZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5hbmltYXRpbmcgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGMgPSB0aGlzLmxpc3QoKS5maW5kKCdbZGF0YS1qY2Fyb3VzZWwtY2xvbmVdJyk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjLmxlbmd0aCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYy5yZW1vdmUoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fcmVsb2FkKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdhbmltYXRlZW5kJyk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGNhbGxiYWNrLmNhbGwodGhpcywgdHJ1ZSk7XHJcbiAgICAgICAgICAgICAgICB9LCB0aGlzKTtcclxuXHJcbiAgICAgICAgICAgIHZhciBvcHRzID0gdHlwZW9mIGFuaW1hdGlvbiA9PT0gJ29iamVjdCcgP1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAkLmV4dGVuZCh7fSwgYW5pbWF0aW9uKSA6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgIHtkdXJhdGlvbjogYW5pbWF0aW9ufSxcclxuICAgICAgICAgICAgICAgIG9sZENvbXBsZXRlID0gb3B0cy5jb21wbGV0ZSB8fCAkLm5vb3A7XHJcblxyXG4gICAgICAgICAgICBpZiAoYW5pbWF0ZSA9PT0gZmFsc2UpIHtcclxuICAgICAgICAgICAgICAgIG9wdHMuZHVyYXRpb24gPSAwO1xyXG4gICAgICAgICAgICB9IGVsc2UgaWYgKHR5cGVvZiAkLmZ4LnNwZWVkc1tvcHRzLmR1cmF0aW9uXSAhPT0gJ3VuZGVmaW5lZCcpIHtcclxuICAgICAgICAgICAgICAgIG9wdHMuZHVyYXRpb24gPSAkLmZ4LnNwZWVkc1tvcHRzLmR1cmF0aW9uXTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgb3B0cy5jb21wbGV0ZSA9IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgY29tcGxldGUoKTtcclxuICAgICAgICAgICAgICAgIG9sZENvbXBsZXRlLmNhbGwodGhpcyk7XHJcbiAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICB0aGlzLm1vdmUocHJvcGVydGllcywgb3B0cyk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9wcmVwYXJlOiBmdW5jdGlvbihpdGVtKSB7XHJcbiAgICAgICAgICAgIHZhciBpbmRleCAgPSB0aGlzLmluZGV4KGl0ZW0pLFxyXG4gICAgICAgICAgICAgICAgaWR4ICAgID0gaW5kZXgsXHJcbiAgICAgICAgICAgICAgICB3aCAgICAgPSB0aGlzLmRpbWVuc2lvbihpdGVtKSxcclxuICAgICAgICAgICAgICAgIGNsaXAgICA9IHRoaXMuY2xpcHBpbmcoKSxcclxuICAgICAgICAgICAgICAgIGxyYiAgICA9IHRoaXMudmVydGljYWwgPyAnYm90dG9tJyA6ICh0aGlzLnJ0bCA/ICdsZWZ0JyAgOiAncmlnaHQnKSxcclxuICAgICAgICAgICAgICAgIGNlbnRlciA9IHRoaXMub3B0aW9ucygnY2VudGVyJyksXHJcbiAgICAgICAgICAgICAgICB1cGRhdGUgPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGFyZ2V0OiAgICAgICBpdGVtLFxyXG4gICAgICAgICAgICAgICAgICAgIGZpcnN0OiAgICAgICAgaXRlbSxcclxuICAgICAgICAgICAgICAgICAgICBsYXN0OiAgICAgICAgIGl0ZW0sXHJcbiAgICAgICAgICAgICAgICAgICAgdmlzaWJsZTogICAgICBpdGVtLFxyXG4gICAgICAgICAgICAgICAgICAgIGZ1bGx5dmlzaWJsZTogd2ggPD0gY2xpcCA/IGl0ZW0gOiAkKClcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICBjdXJyLFxyXG4gICAgICAgICAgICAgICAgaXNWaXNpYmxlLFxyXG4gICAgICAgICAgICAgICAgbWFyZ2luLFxyXG4gICAgICAgICAgICAgICAgZGltO1xyXG5cclxuICAgICAgICAgICAgaWYgKGNlbnRlcikge1xyXG4gICAgICAgICAgICAgICAgd2ggLz0gMjtcclxuICAgICAgICAgICAgICAgIGNsaXAgLz0gMjtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKHdoIDwgY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgd2hpbGUgKHRydWUpIHtcclxuICAgICAgICAgICAgICAgICAgICBjdXJyID0gdGhpcy5pdGVtcygpLmVxKCsraWR4KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGN1cnIubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICghdGhpcy5jaXJjdWxhcikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZXEoMCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoaXRlbS5nZXQoMCkgPT09IGN1cnIuZ2V0KDApKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaXNWaXNpYmxlID0gdGhpcy5fdmlzaWJsZS5pbmRleChjdXJyKSA+PSAwO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGlzVmlzaWJsZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY3Vyci5hZnRlcihjdXJyLmNsb25lKHRydWUpLmF0dHIoJ2RhdGEtamNhcm91c2VsLWNsb25lJywgdHJ1ZSkpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmxpc3QoKS5hcHBlbmQoY3Vycik7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoIWlzVmlzaWJsZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIHByb3BzID0ge307XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBwcm9wc1t0aGlzLmx0XSA9IHRoaXMuZGltZW5zaW9uKGN1cnIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5tb3ZlQnkocHJvcHMpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAvLyBGb3JjZSBpdGVtcyByZWxvYWRcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5faXRlbXMgPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgZGltID0gdGhpcy5kaW1lbnNpb24oY3Vycik7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChkaW0gPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICB3aCArPSBkaW07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHVwZGF0ZS5sYXN0ICAgID0gY3VycjtcclxuICAgICAgICAgICAgICAgICAgICB1cGRhdGUudmlzaWJsZSA9IHVwZGF0ZS52aXNpYmxlLmFkZChjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgLy8gUmVtb3ZlIHJpZ2h0L2JvdHRvbSBtYXJnaW4gZnJvbSB0b3RhbCB3aWR0aFxyXG4gICAgICAgICAgICAgICAgICAgIG1hcmdpbiA9IHRvRmxvYXQoY3Vyci5jc3MoJ21hcmdpbi0nICsgbHJiKSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmICgod2ggLSBtYXJnaW4pIDw9IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdXBkYXRlLmZ1bGx5dmlzaWJsZSA9IHVwZGF0ZS5mdWxseXZpc2libGUuYWRkKGN1cnIpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHdoID49IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAoIXRoaXMuY2lyY3VsYXIgJiYgIWNlbnRlciAmJiB3aCA8IGNsaXApIHtcclxuICAgICAgICAgICAgICAgIGlkeCA9IGluZGV4O1xyXG5cclxuICAgICAgICAgICAgICAgIHdoaWxlICh0cnVlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKC0taWR4IDwgMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZXEoaWR4KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGN1cnIubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgZGltID0gdGhpcy5kaW1lbnNpb24oY3Vycik7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChkaW0gPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICB3aCArPSBkaW07XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHVwZGF0ZS5maXJzdCAgID0gY3VycjtcclxuICAgICAgICAgICAgICAgICAgICB1cGRhdGUudmlzaWJsZSA9IHVwZGF0ZS52aXNpYmxlLmFkZChjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgLy8gUmVtb3ZlIHJpZ2h0L2JvdHRvbSBtYXJnaW4gZnJvbSB0b3RhbCB3aWR0aFxyXG4gICAgICAgICAgICAgICAgICAgIG1hcmdpbiA9IHRvRmxvYXQoY3Vyci5jc3MoJ21hcmdpbi0nICsgbHJiKSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmICgod2ggLSBtYXJnaW4pIDw9IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdXBkYXRlLmZ1bGx5dmlzaWJsZSA9IHVwZGF0ZS5mdWxseXZpc2libGUuYWRkKGN1cnIpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHdoID49IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLl91cGRhdGUodXBkYXRlKTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMudGFpbCA9IDA7XHJcblxyXG4gICAgICAgICAgICBpZiAoIWNlbnRlciAmJlxyXG4gICAgICAgICAgICAgICAgdGhpcy5vcHRpb25zKCd3cmFwJykgIT09ICdjaXJjdWxhcicgJiZcclxuICAgICAgICAgICAgICAgIHRoaXMub3B0aW9ucygnd3JhcCcpICE9PSAnY3VzdG9tJyAmJlxyXG4gICAgICAgICAgICAgICAgdGhpcy5pbmRleCh1cGRhdGUubGFzdCkgPT09ICh0aGlzLml0ZW1zKCkubGVuZ3RoIC0gMSkpIHtcclxuXHJcbiAgICAgICAgICAgICAgICAvLyBSZW1vdmUgcmlnaHQvYm90dG9tIG1hcmdpbiBmcm9tIHRvdGFsIHdpZHRoXHJcbiAgICAgICAgICAgICAgICB3aCAtPSB0b0Zsb2F0KHVwZGF0ZS5sYXN0LmNzcygnbWFyZ2luLScgKyBscmIpKTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAod2ggPiBjbGlwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy50YWlsID0gd2ggLSBjbGlwO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9wb3NpdGlvbjogZnVuY3Rpb24oaXRlbSkge1xyXG4gICAgICAgICAgICB2YXIgZmlyc3QgID0gdGhpcy5fZmlyc3QsXHJcbiAgICAgICAgICAgICAgICBwb3MgICAgPSBmaXJzdC5wb3NpdGlvbigpW3RoaXMubHRdLFxyXG4gICAgICAgICAgICAgICAgY2VudGVyID0gdGhpcy5vcHRpb25zKCdjZW50ZXInKSxcclxuICAgICAgICAgICAgICAgIGNlbnRlck9mZnNldCA9IGNlbnRlciA/ICh0aGlzLmNsaXBwaW5nKCkgLyAyKSAtICh0aGlzLmRpbWVuc2lvbihmaXJzdCkgLyAyKSA6IDA7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5ydGwgJiYgIXRoaXMudmVydGljYWwpIHtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLnJlbGF0aXZlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcG9zIC09IHRoaXMubGlzdCgpLndpZHRoKCkgLSB0aGlzLmRpbWVuc2lvbihmaXJzdCk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHBvcyAtPSB0aGlzLmNsaXBwaW5nKCkgLSB0aGlzLmRpbWVuc2lvbihmaXJzdCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcG9zICs9IGNlbnRlck9mZnNldDtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHBvcyAtPSBjZW50ZXJPZmZzZXQ7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICghY2VudGVyICYmXHJcbiAgICAgICAgICAgICAgICAodGhpcy5pbmRleChpdGVtKSA+IHRoaXMuaW5kZXgoZmlyc3QpIHx8IHRoaXMuaW5UYWlsKSAmJlxyXG4gICAgICAgICAgICAgICAgdGhpcy50YWlsKSB7XHJcbiAgICAgICAgICAgICAgICBwb3MgPSB0aGlzLnJ0bCAmJiAhdGhpcy52ZXJ0aWNhbCA/IHBvcyAtIHRoaXMudGFpbCA6IHBvcyArIHRoaXMudGFpbDtcclxuICAgICAgICAgICAgICAgIHRoaXMuaW5UYWlsID0gdHJ1ZTtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuaW5UYWlsID0gZmFsc2U7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiAtcG9zO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX3VwZGF0ZTogZnVuY3Rpb24odXBkYXRlKSB7XHJcbiAgICAgICAgICAgIHZhciBzZWxmID0gdGhpcyxcclxuICAgICAgICAgICAgICAgIGN1cnJlbnQgPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGFyZ2V0OiAgICAgICB0aGlzLl90YXJnZXQgfHwgJCgpLFxyXG4gICAgICAgICAgICAgICAgICAgIGZpcnN0OiAgICAgICAgdGhpcy5fZmlyc3QgfHwgJCgpLFxyXG4gICAgICAgICAgICAgICAgICAgIGxhc3Q6ICAgICAgICAgdGhpcy5fbGFzdCB8fCAkKCksXHJcbiAgICAgICAgICAgICAgICAgICAgdmlzaWJsZTogICAgICB0aGlzLl92aXNpYmxlIHx8ICQoKSxcclxuICAgICAgICAgICAgICAgICAgICBmdWxseXZpc2libGU6IHRoaXMuX2Z1bGx5dmlzaWJsZSB8fCAkKClcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICBiYWNrID0gdGhpcy5pbmRleCh1cGRhdGUuZmlyc3QgfHwgY3VycmVudC5maXJzdCkgPCB0aGlzLmluZGV4KGN1cnJlbnQuZmlyc3QpLFxyXG4gICAgICAgICAgICAgICAga2V5LFxyXG4gICAgICAgICAgICAgICAgZG9VcGRhdGUgPSBmdW5jdGlvbihrZXkpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgZWxJbiAgPSBbXSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxPdXQgPSBbXTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdXBkYXRlW2tleV0uZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKGN1cnJlbnRba2V5XS5pbmRleCh0aGlzKSA8IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVsSW4ucHVzaCh0aGlzKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBjdXJyZW50W2tleV0uZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHVwZGF0ZVtrZXldLmluZGV4KHRoaXMpIDwgMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxPdXQucHVzaCh0aGlzKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoYmFjaykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBlbEluID0gZWxJbi5yZXZlcnNlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxPdXQgPSBlbE91dC5yZXZlcnNlKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBzZWxmLl90cmlnZ2VyKGtleSArICdpbicsICQoZWxJbikpO1xyXG4gICAgICAgICAgICAgICAgICAgIHNlbGYuX3RyaWdnZXIoa2V5ICsgJ291dCcsICQoZWxPdXQpKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZlsnXycgKyBrZXldID0gdXBkYXRlW2tleV07XHJcbiAgICAgICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgZm9yIChrZXkgaW4gdXBkYXRlKSB7XHJcbiAgICAgICAgICAgICAgICBkb1VwZGF0ZShrZXkpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9XHJcbiAgICB9KTtcclxufShqUXVlcnksIHdpbmRvdykpO1xyXG5cclxuKGZ1bmN0aW9uKCQpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICAkLmpjYXJvdXNlbC5mbi5zY3JvbGxJbnRvVmlldyA9IGZ1bmN0aW9uKHRhcmdldCwgYW5pbWF0ZSwgY2FsbGJhY2spIHtcclxuICAgICAgICB2YXIgcGFyc2VkID0gJC5qQ2Fyb3VzZWwucGFyc2VUYXJnZXQodGFyZ2V0KSxcclxuICAgICAgICAgICAgZmlyc3QgID0gdGhpcy5pbmRleCh0aGlzLl9mdWxseXZpc2libGUuZmlyc3QoKSksXHJcbiAgICAgICAgICAgIGxhc3QgICA9IHRoaXMuaW5kZXgodGhpcy5fZnVsbHl2aXNpYmxlLmxhc3QoKSksXHJcbiAgICAgICAgICAgIGluZGV4O1xyXG5cclxuICAgICAgICBpZiAocGFyc2VkLnJlbGF0aXZlKSB7XHJcbiAgICAgICAgICAgIGluZGV4ID0gcGFyc2VkLnRhcmdldCA8IDAgPyBNYXRoLm1heCgwLCBmaXJzdCArIHBhcnNlZC50YXJnZXQpIDogbGFzdCArIHBhcnNlZC50YXJnZXQ7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgaW5kZXggPSB0eXBlb2YgcGFyc2VkLnRhcmdldCAhPT0gJ29iamVjdCcgPyBwYXJzZWQudGFyZ2V0IDogdGhpcy5pbmRleChwYXJzZWQudGFyZ2V0KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmIChpbmRleCA8IGZpcnN0KSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLnNjcm9sbChpbmRleCwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKGluZGV4ID49IGZpcnN0ICYmIGluZGV4IDw9IGxhc3QpIHtcclxuICAgICAgICAgICAgaWYgKCQuaXNGdW5jdGlvbihjYWxsYmFjaykpIHtcclxuICAgICAgICAgICAgICAgIGNhbGxiYWNrLmNhbGwodGhpcywgZmFsc2UpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBpdGVtcyA9IHRoaXMuaXRlbXMoKSxcclxuICAgICAgICAgICAgY2xpcCA9IHRoaXMuY2xpcHBpbmcoKSxcclxuICAgICAgICAgICAgbHJiICA9IHRoaXMudmVydGljYWwgPyAnYm90dG9tJyA6ICh0aGlzLnJ0bCA/ICdsZWZ0JyAgOiAncmlnaHQnKSxcclxuICAgICAgICAgICAgd2ggICA9IDAsXHJcbiAgICAgICAgICAgIGN1cnI7XHJcblxyXG4gICAgICAgIHdoaWxlICh0cnVlKSB7XHJcbiAgICAgICAgICAgIGN1cnIgPSBpdGVtcy5lcShpbmRleCk7XHJcblxyXG4gICAgICAgICAgICBpZiAoY3Vyci5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB3aCArPSB0aGlzLmRpbWVuc2lvbihjdXJyKTtcclxuXHJcbiAgICAgICAgICAgIGlmICh3aCA+PSBjbGlwKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgbWFyZ2luID0gcGFyc2VGbG9hdChjdXJyLmNzcygnbWFyZ2luLScgKyBscmIpKSB8fCAwO1xyXG4gICAgICAgICAgICAgICAgaWYgKCh3aCAtIG1hcmdpbikgIT09IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICBpbmRleCsrO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmIChpbmRleCA8PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaW5kZXgtLTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiB0aGlzLnNjcm9sbChpbmRleCwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgfTtcclxufShqUXVlcnkpKTtcclxuXHJcbihmdW5jdGlvbigkKSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcblxyXG4gICAgJC5qQ2Fyb3VzZWwucGx1Z2luKCdqY2Fyb3VzZWxDb250cm9sJywge1xyXG4gICAgICAgIF9vcHRpb25zOiB7XHJcbiAgICAgICAgICAgIHRhcmdldDogJys9MScsXHJcbiAgICAgICAgICAgIGV2ZW50OiAgJ2NsaWNrJyxcclxuICAgICAgICAgICAgbWV0aG9kOiAnc2Nyb2xsJ1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2FjdGl2ZTogbnVsbCxcclxuICAgICAgICBfaW5pdDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMub25EZXN0cm95ID0gJC5wcm94eShmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2Rlc3Ryb3koKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgICAgIC5vbmUoJ2pjYXJvdXNlbDpjcmVhdGVlbmQnLCAkLnByb3h5KHRoaXMuX2NyZWF0ZSwgdGhpcykpO1xyXG4gICAgICAgICAgICB9LCB0aGlzKTtcclxuICAgICAgICAgICAgdGhpcy5vblJlbG9hZCA9ICQucHJveHkodGhpcy5fcmVsb2FkLCB0aGlzKTtcclxuICAgICAgICAgICAgdGhpcy5vbkV2ZW50ID0gJC5wcm94eShmdW5jdGlvbihlKSB7XHJcbiAgICAgICAgICAgICAgICBlLnByZXZlbnREZWZhdWx0KCk7XHJcblxyXG4gICAgICAgICAgICAgICAgdmFyIG1ldGhvZCA9IHRoaXMub3B0aW9ucygnbWV0aG9kJyk7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKCQuaXNGdW5jdGlvbihtZXRob2QpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgbWV0aG9kLmNhbGwodGhpcyk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAuamNhcm91c2VsKHRoaXMub3B0aW9ucygnbWV0aG9kJyksIHRoaXMub3B0aW9ucygndGFyZ2V0JykpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9LCB0aGlzKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9jcmVhdGU6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgIC5vbmUoJ2pjYXJvdXNlbDpkZXN0cm95JywgdGhpcy5vbkRlc3Ryb3kpXHJcbiAgICAgICAgICAgICAgICAub24oJ2pjYXJvdXNlbDpyZWxvYWRlbmQgamNhcm91c2VsOnNjcm9sbGVuZCcsIHRoaXMub25SZWxvYWQpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fZWxlbWVudFxyXG4gICAgICAgICAgICAgICAgLm9uKHRoaXMub3B0aW9ucygnZXZlbnQnKSArICcuamNhcm91c2VsY29udHJvbCcsIHRoaXMub25FdmVudCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9yZWxvYWQoKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9kZXN0cm95OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5fZWxlbWVudFxyXG4gICAgICAgICAgICAgICAgLm9mZignLmpjYXJvdXNlbGNvbnRyb2wnLCB0aGlzLm9uRXZlbnQpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAub2ZmKCdqY2Fyb3VzZWw6ZGVzdHJveScsIHRoaXMub25EZXN0cm95KVxyXG4gICAgICAgICAgICAgICAgLm9mZignamNhcm91c2VsOnJlbG9hZGVuZCBqY2Fyb3VzZWw6c2Nyb2xsZW5kJywgdGhpcy5vblJlbG9hZCk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfcmVsb2FkOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdmFyIHBhcnNlZCAgID0gJC5qQ2Fyb3VzZWwucGFyc2VUYXJnZXQodGhpcy5vcHRpb25zKCd0YXJnZXQnKSksXHJcbiAgICAgICAgICAgICAgICBjYXJvdXNlbCA9IHRoaXMuY2Fyb3VzZWwoKSxcclxuICAgICAgICAgICAgICAgIGFjdGl2ZTtcclxuXHJcbiAgICAgICAgICAgIGlmIChwYXJzZWQucmVsYXRpdmUpIHtcclxuICAgICAgICAgICAgICAgIGFjdGl2ZSA9IGNhcm91c2VsXHJcbiAgICAgICAgICAgICAgICAgICAgLmpjYXJvdXNlbChwYXJzZWQudGFyZ2V0ID4gMCA/ICdoYXNOZXh0JyA6ICdoYXNQcmV2Jyk7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgdGFyZ2V0ID0gdHlwZW9mIHBhcnNlZC50YXJnZXQgIT09ICdvYmplY3QnID9cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjYXJvdXNlbC5qY2Fyb3VzZWwoJ2l0ZW1zJykuZXEocGFyc2VkLnRhcmdldCkgOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhcnNlZC50YXJnZXQ7XHJcblxyXG4gICAgICAgICAgICAgICAgYWN0aXZlID0gY2Fyb3VzZWwuamNhcm91c2VsKCd0YXJnZXQnKS5pbmRleCh0YXJnZXQpID49IDA7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICh0aGlzLl9hY3RpdmUgIT09IGFjdGl2ZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcihhY3RpdmUgPyAnYWN0aXZlJyA6ICdpbmFjdGl2ZScpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fYWN0aXZlID0gYWN0aXZlO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9XHJcbiAgICB9KTtcclxufShqUXVlcnkpKTtcclxuXHJcbihmdW5jdGlvbigkKSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcblxyXG4gICAgJC5qQ2Fyb3VzZWwucGx1Z2luKCdqY2Fyb3VzZWxQYWdpbmF0aW9uJywge1xyXG4gICAgICAgIF9vcHRpb25zOiB7XHJcbiAgICAgICAgICAgIHBlclBhZ2U6IG51bGwsXHJcbiAgICAgICAgICAgIGl0ZW06IGZ1bmN0aW9uKHBhZ2UpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiAnPGEgaHJlZj1cIiMnICsgcGFnZSArICdcIj4nICsgcGFnZSArICc8L2E+JztcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgZXZlbnQ6ICAnY2xpY2snLFxyXG4gICAgICAgICAgICBtZXRob2Q6ICdzY3JvbGwnXHJcbiAgICAgICAgfSxcclxuICAgICAgICBfY2Fyb3VzZWxJdGVtczogbnVsbCxcclxuICAgICAgICBfcGFnZXM6IHt9LFxyXG4gICAgICAgIF9pdGVtczoge30sXHJcbiAgICAgICAgX2N1cnJlbnRQYWdlOiBudWxsLFxyXG4gICAgICAgIF9pbml0OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5vbkRlc3Ryb3kgPSAkLnByb3h5KGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fZGVzdHJveSgpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmNyZWF0ZWVuZCcsICQucHJveHkodGhpcy5fY3JlYXRlLCB0aGlzKSk7XHJcbiAgICAgICAgICAgIH0sIHRoaXMpO1xyXG4gICAgICAgICAgICB0aGlzLm9uUmVsb2FkID0gJC5wcm94eSh0aGlzLl9yZWxvYWQsIHRoaXMpO1xyXG4gICAgICAgICAgICB0aGlzLm9uU2Nyb2xsID0gJC5wcm94eSh0aGlzLl91cGRhdGUsIHRoaXMpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2NyZWF0ZTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmRlc3Ryb3knLCB0aGlzLm9uRGVzdHJveSlcclxuICAgICAgICAgICAgICAgIC5vbignamNhcm91c2VsOnJlbG9hZGVuZCcsIHRoaXMub25SZWxvYWQpXHJcbiAgICAgICAgICAgICAgICAub24oJ2pjYXJvdXNlbDpzY3JvbGxlbmQnLCB0aGlzLm9uU2Nyb2xsKTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3JlbG9hZCgpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2Rlc3Ryb3k6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLl9jbGVhcigpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAub2ZmKCdqY2Fyb3VzZWw6ZGVzdHJveScsIHRoaXMub25EZXN0cm95KVxyXG4gICAgICAgICAgICAgICAgLm9mZignamNhcm91c2VsOnJlbG9hZGVuZCcsIHRoaXMub25SZWxvYWQpXHJcbiAgICAgICAgICAgICAgICAub2ZmKCdqY2Fyb3VzZWw6c2Nyb2xsZW5kJywgdGhpcy5vblNjcm9sbCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9jYXJvdXNlbEl0ZW1zID0gbnVsbDtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9yZWxvYWQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgcGVyUGFnZSA9IHRoaXMub3B0aW9ucygncGVyUGFnZScpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fcGFnZXMgPSB7fTtcclxuICAgICAgICAgICAgdGhpcy5faXRlbXMgPSB7fTtcclxuXHJcbiAgICAgICAgICAgIC8vIENhbGN1bGF0ZSBwYWdlc1xyXG4gICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKHBlclBhZ2UpKSB7XHJcbiAgICAgICAgICAgICAgICBwZXJQYWdlID0gcGVyUGFnZS5jYWxsKHRoaXMpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAocGVyUGFnZSA9PSBudWxsKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9wYWdlcyA9IHRoaXMuX2NhbGN1bGF0ZVBhZ2VzKCk7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgcHAgICAgPSBwYXJzZUludChwZXJQYWdlLCAxMCkgfHwgMCxcclxuICAgICAgICAgICAgICAgICAgICBpdGVtcyA9IHRoaXMuX2dldENhcm91c2VsSXRlbXMoKSxcclxuICAgICAgICAgICAgICAgICAgICBwYWdlICA9IDEsXHJcbiAgICAgICAgICAgICAgICAgICAgaSAgICAgPSAwLFxyXG4gICAgICAgICAgICAgICAgICAgIGN1cnI7XHJcblxyXG4gICAgICAgICAgICAgICAgd2hpbGUgKHRydWUpIHtcclxuICAgICAgICAgICAgICAgICAgICBjdXJyID0gaXRlbXMuZXEoaSsrKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGN1cnIubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCF0aGlzLl9wYWdlc1twYWdlXSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9wYWdlc1twYWdlXSA9IGN1cnI7XHJcbiAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fcGFnZXNbcGFnZV0gPSB0aGlzLl9wYWdlc1twYWdlXS5hZGQoY3Vycik7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoaSAlIHBwID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHBhZ2UrKztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuX2NsZWFyKCk7XHJcblxyXG4gICAgICAgICAgICB2YXIgc2VsZiAgICAgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgY2Fyb3VzZWwgPSB0aGlzLmNhcm91c2VsKCkuZGF0YSgnamNhcm91c2VsJyksXHJcbiAgICAgICAgICAgICAgICBlbGVtZW50ICA9IHRoaXMuX2VsZW1lbnQsXHJcbiAgICAgICAgICAgICAgICBpdGVtICAgICA9IHRoaXMub3B0aW9ucygnaXRlbScpLFxyXG4gICAgICAgICAgICAgICAgbnVtQ2Fyb3VzZWxJdGVtcyA9IHRoaXMuX2dldENhcm91c2VsSXRlbXMoKS5sZW5ndGg7XHJcblxyXG4gICAgICAgICAgICAkLmVhY2godGhpcy5fcGFnZXMsIGZ1bmN0aW9uKHBhZ2UsIGNhcm91c2VsSXRlbXMpIHtcclxuICAgICAgICAgICAgICAgIHZhciBjdXJySXRlbSA9IHNlbGYuX2l0ZW1zW3BhZ2VdID0gJChpdGVtLmNhbGwoc2VsZiwgcGFnZSwgY2Fyb3VzZWxJdGVtcykpO1xyXG5cclxuICAgICAgICAgICAgICAgIGN1cnJJdGVtLm9uKHNlbGYub3B0aW9ucygnZXZlbnQnKSArICcuamNhcm91c2VscGFnaW5hdGlvbicsICQucHJveHkoZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHRhcmdldCA9IGNhcm91c2VsSXRlbXMuZXEoMCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIC8vIElmIGNpcmN1bGFyIHdyYXBwaW5nIGVuYWJsZWQsIGVuc3VyZSBjb3JyZWN0IHNjcm9sbGluZyBkaXJlY3Rpb25cclxuICAgICAgICAgICAgICAgICAgICBpZiAoY2Fyb3VzZWwuY2lyY3VsYXIpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGN1cnJlbnRJbmRleCA9IGNhcm91c2VsLmluZGV4KGNhcm91c2VsLnRhcmdldCgpKSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIG5ld0luZGV4ICAgICA9IGNhcm91c2VsLmluZGV4KHRhcmdldCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAocGFyc2VGbG9hdChwYWdlKSA+IHBhcnNlRmxvYXQoc2VsZi5fY3VycmVudFBhZ2UpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAobmV3SW5kZXggPCBjdXJyZW50SW5kZXgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0YXJnZXQgPSAnKz0nICsgKG51bUNhcm91c2VsSXRlbXMgLSBjdXJyZW50SW5kZXggKyBuZXdJbmRleCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAobmV3SW5kZXggPiBjdXJyZW50SW5kZXgpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0YXJnZXQgPSAnLT0nICsgKGN1cnJlbnRJbmRleCArIChudW1DYXJvdXNlbEl0ZW1zIC0gbmV3SW5kZXgpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgY2Fyb3VzZWxbdGhpcy5vcHRpb25zKCdtZXRob2QnKV0odGFyZ2V0KTtcclxuICAgICAgICAgICAgICAgIH0sIHNlbGYpKTtcclxuXHJcbiAgICAgICAgICAgICAgICBlbGVtZW50LmFwcGVuZChjdXJySXRlbSk7XHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fdXBkYXRlKCk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfdXBkYXRlOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdmFyIHRhcmdldCA9IHRoaXMuY2Fyb3VzZWwoKS5qY2Fyb3VzZWwoJ3RhcmdldCcpLFxyXG4gICAgICAgICAgICAgICAgY3VycmVudFBhZ2U7XHJcblxyXG4gICAgICAgICAgICAkLmVhY2godGhpcy5fcGFnZXMsIGZ1bmN0aW9uKHBhZ2UsIGNhcm91c2VsSXRlbXMpIHtcclxuICAgICAgICAgICAgICAgIGNhcm91c2VsSXRlbXMuZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGFyZ2V0LmlzKHRoaXMpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGN1cnJlbnRQYWdlID0gcGFnZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmIChjdXJyZW50UGFnZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5fY3VycmVudFBhZ2UgIT09IGN1cnJlbnRQYWdlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdpbmFjdGl2ZScsIHRoaXMuX2l0ZW1zW3RoaXMuX2N1cnJlbnRQYWdlXSk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdhY3RpdmUnLCB0aGlzLl9pdGVtc1tjdXJyZW50UGFnZV0pO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9jdXJyZW50UGFnZSA9IGN1cnJlbnRQYWdlO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgaXRlbXM6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5faXRlbXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICByZWxvYWRDYXJvdXNlbEl0ZW1zOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5fY2Fyb3VzZWxJdGVtcyA9IG51bGw7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2NsZWFyOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5fZWxlbWVudC5lbXB0eSgpO1xyXG4gICAgICAgICAgICB0aGlzLl9jdXJyZW50UGFnZSA9IG51bGw7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfY2FsY3VsYXRlUGFnZXM6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgY2Fyb3VzZWwgPSB0aGlzLmNhcm91c2VsKCkuZGF0YSgnamNhcm91c2VsJyksXHJcbiAgICAgICAgICAgICAgICBpdGVtcyAgICA9IHRoaXMuX2dldENhcm91c2VsSXRlbXMoKSxcclxuICAgICAgICAgICAgICAgIGNsaXAgICAgID0gY2Fyb3VzZWwuY2xpcHBpbmcoKSxcclxuICAgICAgICAgICAgICAgIHdoICAgICAgID0gMCxcclxuICAgICAgICAgICAgICAgIGlkeCAgICAgID0gMCxcclxuICAgICAgICAgICAgICAgIHBhZ2UgICAgID0gMSxcclxuICAgICAgICAgICAgICAgIHBhZ2VzICAgID0ge30sXHJcbiAgICAgICAgICAgICAgICBjdXJyO1xyXG5cclxuICAgICAgICAgICAgd2hpbGUgKHRydWUpIHtcclxuICAgICAgICAgICAgICAgIGN1cnIgPSBpdGVtcy5lcShpZHgrKyk7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKGN1cnIubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKCFwYWdlc1twYWdlXSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHBhZ2VzW3BhZ2VdID0gY3VycjtcclxuICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcGFnZXNbcGFnZV0gPSBwYWdlc1twYWdlXS5hZGQoY3Vycik7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgd2ggKz0gY2Fyb3VzZWwuZGltZW5zaW9uKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmICh3aCA+PSBjbGlwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcGFnZSsrO1xyXG4gICAgICAgICAgICAgICAgICAgIHdoID0gMDtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHBhZ2VzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2dldENhcm91c2VsSXRlbXM6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBpZiAoIXRoaXMuX2Nhcm91c2VsSXRlbXMpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2Nhcm91c2VsSXRlbXMgPSB0aGlzLmNhcm91c2VsKCkuamNhcm91c2VsKCdpdGVtcycpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fY2Fyb3VzZWxJdGVtcztcclxuICAgICAgICB9XHJcbiAgICB9KTtcclxufShqUXVlcnkpKTtcclxuXHJcbihmdW5jdGlvbigkKSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcblxyXG4gICAgJC5qQ2Fyb3VzZWwucGx1Z2luKCdqY2Fyb3VzZWxBdXRvc2Nyb2xsJywge1xyXG4gICAgICAgIF9vcHRpb25zOiB7XHJcbiAgICAgICAgICAgIHRhcmdldDogICAgJys9MScsXHJcbiAgICAgICAgICAgIGludGVydmFsOiAgMzAwMCxcclxuICAgICAgICAgICAgYXV0b3N0YXJ0OiB0cnVlXHJcbiAgICAgICAgfSxcclxuICAgICAgICBfdGltZXI6IG51bGwsXHJcbiAgICAgICAgX2luaXQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdGhpcy5vbkRlc3Ryb3kgPSAkLnByb3h5KGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fZGVzdHJveSgpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmNyZWF0ZWVuZCcsICQucHJveHkodGhpcy5fY3JlYXRlLCB0aGlzKSk7XHJcbiAgICAgICAgICAgIH0sIHRoaXMpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5vbkFuaW1hdGVFbmQgPSAkLnByb3h5KHRoaXMuc3RhcnQsIHRoaXMpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2NyZWF0ZTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmRlc3Ryb3knLCB0aGlzLm9uRGVzdHJveSk7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5vcHRpb25zKCdhdXRvc3RhcnQnKSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zdGFydCgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfZGVzdHJveTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuc3RvcCgpO1xyXG4gICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgIC5vZmYoJ2pjYXJvdXNlbDpkZXN0cm95JywgdGhpcy5vbkRlc3Ryb3kpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgc3RhcnQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLnN0b3AoKTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmFuaW1hdGVlbmQnLCB0aGlzLm9uQW5pbWF0ZUVuZCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl90aW1lciA9IHNldFRpbWVvdXQoJC5wcm94eShmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKS5qY2Fyb3VzZWwoJ3Njcm9sbCcsIHRoaXMub3B0aW9ucygndGFyZ2V0JykpO1xyXG4gICAgICAgICAgICB9LCB0aGlzKSwgdGhpcy5vcHRpb25zKCdpbnRlcnZhbCcpKTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgc3RvcDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLl90aW1lcikge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fdGltZXIgPSBjbGVhclRpbWVvdXQodGhpcy5fdGltZXIpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgIC5vZmYoJ2pjYXJvdXNlbDphbmltYXRlZW5kJywgdGhpcy5vbkFuaW1hdGVFbmQpO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfVxyXG4gICAgfSk7XHJcbn0oalF1ZXJ5KSk7XHJcbiIsIihmdW5jdGlvbiAoJClcclxue1xyXG4gICAgJChcIi5qY2Fyb3VzZWwtZW5naW5lXCIpLmVhY2goZnVuY3Rpb24gKClcclxuICAgIHtcclxuICAgICAgICB2YXIgZW5naW5lID0gJCh0aGlzKTtcclxuICAgICAgICB2YXIgYXV0b1N0YXJ0ID0gZW5naW5lLmRhdGEoXCJhdXRvc3RhcnRcIik7XHJcbiAgICAgICAgdmFyIGludGVydmFsID0gZW5naW5lLmRhdGEoXCJpbnRlcnZhbFwiKSB8fCAzMDAwO1xyXG4gICAgICAgIHZhciB0cmFuc2l0aW9ucyA9IGVuZ2luZS5kYXRhKFwidHJhbnNpdGlvbnNcIik7XHJcbiAgICAgICAgdmFyIGVhc2luZyA9IGVuZ2luZS5kYXRhKFwiZWFzaW5nXCIpO1xyXG4gICAgICAgIHZhciB3cmFwID0gZW5naW5lLmRhdGEoXCJ3cmFwXCIpO1xyXG4gICAgICAgIHZhciB2ZXJ0aWNhbCA9IGVuZ2luZS5kYXRhKFwidmVydGljYWxcIik7XHJcbiAgICAgICAgdmFyIGNlbnRlciA9IGVuZ2luZS5kYXRhKFwiY2VudGVyXCIpO1xyXG5cclxuICAgICAgICBlbmdpbmUuZmluZChcIi5qY2Fyb3VzZWxcIilcclxuICAgICAgICAgICAgLm9uKFwiamNhcm91c2VsOmNyZWF0ZSBqY2Fyb3VzZWw6cmVsb2FkXCIsIGZ1bmN0aW9uICgpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIHZhciBlbGVtZW50ID0gJCh0aGlzKTtcclxuICAgICAgICAgICAgICAgIHZhciB3aWR0aCA9IGVsZW1lbnQuaW5uZXJXaWR0aCgpO1xyXG4gICAgICAgICAgICAgICAgZWxlbWVudC5qY2Fyb3VzZWwoXCJpdGVtc1wiKS5jc3MoXCJ3aWR0aFwiLCB3aWR0aCArIFwicHhcIik7XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgIC5qY2Fyb3VzZWwoe1xyXG4gICAgICAgICAgICAgICAgd3JhcDogd3JhcCxcclxuICAgICAgICAgICAgICAgIHZlcnRpY2FsOiB2ZXJ0aWNhbCxcclxuICAgICAgICAgICAgICAgIGNlbnRlcjogY2VudGVyLFxyXG4gICAgICAgICAgICAgICAgdHJhbnNpdGlvbnM6IHRyYW5zaXRpb25zID8ge1xyXG4gICAgICAgICAgICAgICAgICAgIHRyYW5zZm9ybXM6IE1vZGVybml6ci5jc3N0cmFuc2Zvcm1zLFxyXG4gICAgICAgICAgICAgICAgICAgIHRyYW5zZm9ybXMzZDogTW9kZXJuaXpyLmNzc3RyYW5zZm9ybXMzZCxcclxuICAgICAgICAgICAgICAgICAgICBlYXNpbmc6IGVhc2luZ1xyXG4gICAgICAgICAgICAgICAgfSA6IGZhbHNlXHJcbiAgICAgICAgICAgIH0pXHJcblxyXG4gICAgICAgIC5qY2Fyb3VzZWxBdXRvc2Nyb2xsKHtcclxuICAgICAgICAgICAgaW50ZXJ2YWw6IGludGVydmFsLFxyXG4gICAgICAgICAgICB0YXJnZXQ6IFwiKz0xXCIsXHJcbiAgICAgICAgICAgIGF1dG9zdGFydDogYXV0b1N0YXJ0XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIGVuZ2luZS5maW5kKFwiLmpjYXJvdXNlbC1wcmV2XCIpLmpjYXJvdXNlbENvbnRyb2woe1xyXG4gICAgICAgICAgICB0YXJnZXQ6IFwiLT0xXCJcclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgZW5naW5lLmZpbmQoXCIuamNhcm91c2VsLW5leHRcIikuamNhcm91c2VsQ29udHJvbCh7XHJcbiAgICAgICAgICAgIHRhcmdldDogXCIrPTFcIlxyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBlbmdpbmUuZmluZChcIi5qY2Fyb3VzZWwtcGFnaW5hdGlvblwiKVxyXG4gICAgICAgICAgICAub24oXCJqY2Fyb3VzZWxwYWdpbmF0aW9uOmFjdGl2ZVwiLCBcImFcIiwgZnVuY3Rpb24gKClcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgJCh0aGlzKS5hZGRDbGFzcyhcImFjdGl2ZVwiKTtcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgLm9uKFwiamNhcm91c2VscGFnaW5hdGlvbjppbmFjdGl2ZVwiLCBcImFcIiwgZnVuY3Rpb24gKClcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgJCh0aGlzKS5yZW1vdmVDbGFzcyhcImFjdGl2ZVwiKTtcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgLm9uKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIGUucHJldmVudERlZmF1bHQoKTtcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgLmpjYXJvdXNlbFBhZ2luYXRpb24oe1xyXG4gICAgICAgICAgICAgICAgaXRlbTogZnVuY3Rpb24gKHBhZ2UpXHJcbiAgICAgICAgICAgICAgICB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIFwiPGEgaHJlZj1cXFwiI1wiICsgcGFnZSArIFwiXFxcIj5cIiArIHBhZ2UgKyBcIjwvYT5cIjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICB9KTtcclxufSkoalF1ZXJ5KTsiXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=