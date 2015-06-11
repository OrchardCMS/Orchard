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

        engine.find(".jcarousel").jcarousel({
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

        engine.find(".jcarousel-pagination").jcarouselPagination({
            item: function (page)
            {
                return "<a href=\"#" + page + "\">" + page + "</a>";
            }
        });
    });
})(jQuery);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm1vZGVybml6ci50cmFuc2l0aW9ucy5qcyIsImpxdWVyeS5qY2Fyb3VzZWwuanMiLCJlbmdpbmUtamNhcm91c2VsLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ240Q0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EiLCJmaWxlIjoiRW5naW5lLkpDYXJvdXNlbC5qcyIsInNvdXJjZXNDb250ZW50IjpbIi8qIE1vZGVybml6ciAyLjguMyAoQ3VzdG9tIEJ1aWxkKSB8IE1JVCAmIEJTRFxyXG4gKiBCdWlsZDogaHR0cDovL21vZGVybml6ci5jb20vZG93bmxvYWQvIy1jc3N0cmFuc2Zvcm1zLWNzc3RyYW5zZm9ybXMzZC1jc3N0cmFuc2l0aW9ucy10ZXN0c3R5bGVzLXRlc3Rwcm9wLXRlc3RhbGxwcm9wcy1wcmVmaXhlcy1kb21wcmVmaXhlc1xyXG4gKi9cclxuO3dpbmRvdy5Nb2Rlcm5penI9ZnVuY3Rpb24oYSxiLGMpe2Z1bmN0aW9uIHkoYSl7aS5jc3NUZXh0PWF9ZnVuY3Rpb24geihhLGIpe3JldHVybiB5KGwuam9pbihhK1wiO1wiKSsoYnx8XCJcIikpfWZ1bmN0aW9uIEEoYSxiKXtyZXR1cm4gdHlwZW9mIGE9PT1ifWZ1bmN0aW9uIEIoYSxiKXtyZXR1cm4hIX4oXCJcIithKS5pbmRleE9mKGIpfWZ1bmN0aW9uIEMoYSxiKXtmb3IodmFyIGQgaW4gYSl7dmFyIGU9YVtkXTtpZighQihlLFwiLVwiKSYmaVtlXSE9PWMpcmV0dXJuIGI9PVwicGZ4XCI/ZTohMH1yZXR1cm4hMX1mdW5jdGlvbiBEKGEsYixkKXtmb3IodmFyIGUgaW4gYSl7dmFyIGY9YlthW2VdXTtpZihmIT09YylyZXR1cm4gZD09PSExP2FbZV06QShmLFwiZnVuY3Rpb25cIik/Zi5iaW5kKGR8fGIpOmZ9cmV0dXJuITF9ZnVuY3Rpb24gRShhLGIsYyl7dmFyIGQ9YS5jaGFyQXQoMCkudG9VcHBlckNhc2UoKSthLnNsaWNlKDEpLGU9KGErXCIgXCIrbi5qb2luKGQrXCIgXCIpK2QpLnNwbGl0KFwiIFwiKTtyZXR1cm4gQShiLFwic3RyaW5nXCIpfHxBKGIsXCJ1bmRlZmluZWRcIik/QyhlLGIpOihlPShhK1wiIFwiK28uam9pbihkK1wiIFwiKStkKS5zcGxpdChcIiBcIiksRChlLGIsYykpfXZhciBkPVwiMi44LjNcIixlPXt9LGY9Yi5kb2N1bWVudEVsZW1lbnQsZz1cIm1vZGVybml6clwiLGg9Yi5jcmVhdGVFbGVtZW50KGcpLGk9aC5zdHlsZSxqLGs9e30udG9TdHJpbmcsbD1cIiAtd2Via2l0LSAtbW96LSAtby0gLW1zLSBcIi5zcGxpdChcIiBcIiksbT1cIldlYmtpdCBNb3ogTyBtc1wiLG49bS5zcGxpdChcIiBcIiksbz1tLnRvTG93ZXJDYXNlKCkuc3BsaXQoXCIgXCIpLHA9e30scT17fSxyPXt9LHM9W10sdD1zLnNsaWNlLHUsdj1mdW5jdGlvbihhLGMsZCxlKXt2YXIgaCxpLGosayxsPWIuY3JlYXRlRWxlbWVudChcImRpdlwiKSxtPWIuYm9keSxuPW18fGIuY3JlYXRlRWxlbWVudChcImJvZHlcIik7aWYocGFyc2VJbnQoZCwxMCkpd2hpbGUoZC0tKWo9Yi5jcmVhdGVFbGVtZW50KFwiZGl2XCIpLGouaWQ9ZT9lW2RdOmcrKGQrMSksbC5hcHBlbmRDaGlsZChqKTtyZXR1cm4gaD1bXCImIzE3MztcIiwnPHN0eWxlIGlkPVwicycsZywnXCI+JyxhLFwiPC9zdHlsZT5cIl0uam9pbihcIlwiKSxsLmlkPWcsKG0/bDpuKS5pbm5lckhUTUwrPWgsbi5hcHBlbmRDaGlsZChsKSxtfHwobi5zdHlsZS5iYWNrZ3JvdW5kPVwiXCIsbi5zdHlsZS5vdmVyZmxvdz1cImhpZGRlblwiLGs9Zi5zdHlsZS5vdmVyZmxvdyxmLnN0eWxlLm92ZXJmbG93PVwiaGlkZGVuXCIsZi5hcHBlbmRDaGlsZChuKSksaT1jKGwsYSksbT9sLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQobCk6KG4ucGFyZW50Tm9kZS5yZW1vdmVDaGlsZChuKSxmLnN0eWxlLm92ZXJmbG93PWspLCEhaX0sdz17fS5oYXNPd25Qcm9wZXJ0eSx4OyFBKHcsXCJ1bmRlZmluZWRcIikmJiFBKHcuY2FsbCxcInVuZGVmaW5lZFwiKT94PWZ1bmN0aW9uKGEsYil7cmV0dXJuIHcuY2FsbChhLGIpfTp4PWZ1bmN0aW9uKGEsYil7cmV0dXJuIGIgaW4gYSYmQShhLmNvbnN0cnVjdG9yLnByb3RvdHlwZVtiXSxcInVuZGVmaW5lZFwiKX0sRnVuY3Rpb24ucHJvdG90eXBlLmJpbmR8fChGdW5jdGlvbi5wcm90b3R5cGUuYmluZD1mdW5jdGlvbihiKXt2YXIgYz10aGlzO2lmKHR5cGVvZiBjIT1cImZ1bmN0aW9uXCIpdGhyb3cgbmV3IFR5cGVFcnJvcjt2YXIgZD10LmNhbGwoYXJndW1lbnRzLDEpLGU9ZnVuY3Rpb24oKXtpZih0aGlzIGluc3RhbmNlb2YgZSl7dmFyIGE9ZnVuY3Rpb24oKXt9O2EucHJvdG90eXBlPWMucHJvdG90eXBlO3ZhciBmPW5ldyBhLGc9Yy5hcHBseShmLGQuY29uY2F0KHQuY2FsbChhcmd1bWVudHMpKSk7cmV0dXJuIE9iamVjdChnKT09PWc/ZzpmfXJldHVybiBjLmFwcGx5KGIsZC5jb25jYXQodC5jYWxsKGFyZ3VtZW50cykpKX07cmV0dXJuIGV9KSxwLmNzc3RyYW5zZm9ybXM9ZnVuY3Rpb24oKXtyZXR1cm4hIUUoXCJ0cmFuc2Zvcm1cIil9LHAuY3NzdHJhbnNmb3JtczNkPWZ1bmN0aW9uKCl7dmFyIGE9ISFFKFwicGVyc3BlY3RpdmVcIik7cmV0dXJuIGEmJlwid2Via2l0UGVyc3BlY3RpdmVcImluIGYuc3R5bGUmJnYoXCJAbWVkaWEgKHRyYW5zZm9ybS0zZCksKC13ZWJraXQtdHJhbnNmb3JtLTNkKXsjbW9kZXJuaXpye2xlZnQ6OXB4O3Bvc2l0aW9uOmFic29sdXRlO2hlaWdodDozcHg7fX1cIixmdW5jdGlvbihiLGMpe2E9Yi5vZmZzZXRMZWZ0PT09OSYmYi5vZmZzZXRIZWlnaHQ9PT0zfSksYX0scC5jc3N0cmFuc2l0aW9ucz1mdW5jdGlvbigpe3JldHVybiBFKFwidHJhbnNpdGlvblwiKX07Zm9yKHZhciBGIGluIHApeChwLEYpJiYodT1GLnRvTG93ZXJDYXNlKCksZVt1XT1wW0ZdKCkscy5wdXNoKChlW3VdP1wiXCI6XCJuby1cIikrdSkpO3JldHVybiBlLmFkZFRlc3Q9ZnVuY3Rpb24oYSxiKXtpZih0eXBlb2YgYT09XCJvYmplY3RcIilmb3IodmFyIGQgaW4gYSl4KGEsZCkmJmUuYWRkVGVzdChkLGFbZF0pO2Vsc2V7YT1hLnRvTG93ZXJDYXNlKCk7aWYoZVthXSE9PWMpcmV0dXJuIGU7Yj10eXBlb2YgYj09XCJmdW5jdGlvblwiP2IoKTpiLHR5cGVvZiBlbmFibGVDbGFzc2VzIT1cInVuZGVmaW5lZFwiJiZlbmFibGVDbGFzc2VzJiYoZi5jbGFzc05hbWUrPVwiIFwiKyhiP1wiXCI6XCJuby1cIikrYSksZVthXT1ifXJldHVybiBlfSx5KFwiXCIpLGg9aj1udWxsLGUuX3ZlcnNpb249ZCxlLl9wcmVmaXhlcz1sLGUuX2RvbVByZWZpeGVzPW8sZS5fY3Nzb21QcmVmaXhlcz1uLGUudGVzdFByb3A9ZnVuY3Rpb24oYSl7cmV0dXJuIEMoW2FdKX0sZS50ZXN0QWxsUHJvcHM9RSxlLnRlc3RTdHlsZXM9dixlfSh0aGlzLHRoaXMuZG9jdW1lbnQpOyIsIi8qISBqQ2Fyb3VzZWwgLSB2MC4zLjEgLSAyMDE0LTA0LTI2XHJcbiogaHR0cDovL3NvcmdhbGxhLmNvbS9qY2Fyb3VzZWxcclxuKiBDb3B5cmlnaHQgKGMpIDIwMTQgSmFuIFNvcmdhbGxhOyBMaWNlbnNlZCBNSVQgKi9cclxuKGZ1bmN0aW9uKCQpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICB2YXIgakNhcm91c2VsID0gJC5qQ2Fyb3VzZWwgPSB7fTtcclxuXHJcbiAgICBqQ2Fyb3VzZWwudmVyc2lvbiA9ICcwLjMuMSc7XHJcblxyXG4gICAgdmFyIHJSZWxhdGl2ZVRhcmdldCA9IC9eKFsrXFwtXT0pPyguKykkLztcclxuXHJcbiAgICBqQ2Fyb3VzZWwucGFyc2VUYXJnZXQgPSBmdW5jdGlvbih0YXJnZXQpIHtcclxuICAgICAgICB2YXIgcmVsYXRpdmUgPSBmYWxzZSxcclxuICAgICAgICAgICAgcGFydHMgICAgPSB0eXBlb2YgdGFyZ2V0ICE9PSAnb2JqZWN0JyA/XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgIHJSZWxhdGl2ZVRhcmdldC5leGVjKHRhcmdldCkgOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICBudWxsO1xyXG5cclxuICAgICAgICBpZiAocGFydHMpIHtcclxuICAgICAgICAgICAgdGFyZ2V0ID0gcGFyc2VJbnQocGFydHNbMl0sIDEwKSB8fCAwO1xyXG5cclxuICAgICAgICAgICAgaWYgKHBhcnRzWzFdKSB7XHJcbiAgICAgICAgICAgICAgICByZWxhdGl2ZSA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICBpZiAocGFydHNbMV0gPT09ICctPScpIHtcclxuICAgICAgICAgICAgICAgICAgICB0YXJnZXQgKj0gLTE7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9IGVsc2UgaWYgKHR5cGVvZiB0YXJnZXQgIT09ICdvYmplY3QnKSB7XHJcbiAgICAgICAgICAgIHRhcmdldCA9IHBhcnNlSW50KHRhcmdldCwgMTApIHx8IDA7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICB0YXJnZXQ6IHRhcmdldCxcclxuICAgICAgICAgICAgcmVsYXRpdmU6IHJlbGF0aXZlXHJcbiAgICAgICAgfTtcclxuICAgIH07XHJcblxyXG4gICAgakNhcm91c2VsLmRldGVjdENhcm91c2VsID0gZnVuY3Rpb24oZWxlbWVudCkge1xyXG4gICAgICAgIHZhciBjYXJvdXNlbDtcclxuXHJcbiAgICAgICAgd2hpbGUgKGVsZW1lbnQubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICBjYXJvdXNlbCA9IGVsZW1lbnQuZmlsdGVyKCdbZGF0YS1qY2Fyb3VzZWxdJyk7XHJcblxyXG4gICAgICAgICAgICBpZiAoY2Fyb3VzZWwubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGNhcm91c2VsO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBjYXJvdXNlbCA9IGVsZW1lbnQuZmluZCgnW2RhdGEtamNhcm91c2VsXScpO1xyXG5cclxuICAgICAgICAgICAgaWYgKGNhcm91c2VsLmxlbmd0aCA+IDApIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBjYXJvdXNlbDtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgZWxlbWVudCA9IGVsZW1lbnQucGFyZW50KCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICByZXR1cm4gbnVsbDtcclxuICAgIH07XHJcblxyXG4gICAgakNhcm91c2VsLmJhc2UgPSBmdW5jdGlvbihwbHVnaW5OYW1lKSB7XHJcbiAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgdmVyc2lvbjogIGpDYXJvdXNlbC52ZXJzaW9uLFxyXG4gICAgICAgICAgICBfb3B0aW9uczogIHt9LFxyXG4gICAgICAgICAgICBfZWxlbWVudDogIG51bGwsXHJcbiAgICAgICAgICAgIF9jYXJvdXNlbDogbnVsbCxcclxuICAgICAgICAgICAgX2luaXQ6ICAgICAkLm5vb3AsXHJcbiAgICAgICAgICAgIF9jcmVhdGU6ICAgJC5ub29wLFxyXG4gICAgICAgICAgICBfZGVzdHJveTogICQubm9vcCxcclxuICAgICAgICAgICAgX3JlbG9hZDogICAkLm5vb3AsXHJcbiAgICAgICAgICAgIGNyZWF0ZTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9lbGVtZW50XHJcbiAgICAgICAgICAgICAgICAgICAgLmF0dHIoJ2RhdGEtJyArIHBsdWdpbk5hbWUudG9Mb3dlckNhc2UoKSwgdHJ1ZSlcclxuICAgICAgICAgICAgICAgICAgICAuZGF0YShwbHVnaW5OYW1lLCB0aGlzKTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoZmFsc2UgPT09IHRoaXMuX3RyaWdnZXIoJ2NyZWF0ZScpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy5fY3JlYXRlKCk7XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcignY3JlYXRlZW5kJyk7XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGRlc3Ryb3k6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGZhbHNlID09PSB0aGlzLl90cmlnZ2VyKCdkZXN0cm95JykpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICB0aGlzLl9kZXN0cm95KCk7XHJcblxyXG4gICAgICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcignZGVzdHJveWVuZCcpO1xyXG5cclxuICAgICAgICAgICAgICAgIHRoaXMuX2VsZW1lbnRcclxuICAgICAgICAgICAgICAgICAgICAucmVtb3ZlRGF0YShwbHVnaW5OYW1lKVxyXG4gICAgICAgICAgICAgICAgICAgIC5yZW1vdmVBdHRyKCdkYXRhLScgKyBwbHVnaW5OYW1lLnRvTG93ZXJDYXNlKCkpO1xyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICByZWxvYWQ6IGZ1bmN0aW9uKG9wdGlvbnMpIHtcclxuICAgICAgICAgICAgICAgIGlmIChmYWxzZSA9PT0gdGhpcy5fdHJpZ2dlcigncmVsb2FkJykpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICBpZiAob3B0aW9ucykge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMub3B0aW9ucyhvcHRpb25zKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICB0aGlzLl9yZWxvYWQoKTtcclxuXHJcbiAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKCdyZWxvYWRlbmQnKTtcclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgZWxlbWVudDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fZWxlbWVudDtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgb3B0aW9uczogZnVuY3Rpb24oa2V5LCB2YWx1ZSkge1xyXG4gICAgICAgICAgICAgICAgaWYgKGFyZ3VtZW50cy5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gJC5leHRlbmQoe30sIHRoaXMuX29wdGlvbnMpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIGlmICh0eXBlb2Yga2V5ID09PSAnc3RyaW5nJykge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0eXBlb2YgdmFsdWUgPT09ICd1bmRlZmluZWQnKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiB0eXBlb2YgdGhpcy5fb3B0aW9uc1trZXldID09PSAndW5kZWZpbmVkJyA/XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbnVsbCA6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fb3B0aW9uc1trZXldO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fb3B0aW9uc1trZXldID0gdmFsdWU7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuX29wdGlvbnMgPSAkLmV4dGVuZCh7fSwgdGhpcy5fb3B0aW9ucywga2V5KTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgY2Fyb3VzZWw6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKCF0aGlzLl9jYXJvdXNlbCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuX2Nhcm91c2VsID0gakNhcm91c2VsLmRldGVjdENhcm91c2VsKHRoaXMub3B0aW9ucygnY2Fyb3VzZWwnKSB8fCB0aGlzLl9lbGVtZW50KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCF0aGlzLl9jYXJvdXNlbCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAkLmVycm9yKCdDb3VsZCBub3QgZGV0ZWN0IGNhcm91c2VsIGZvciBwbHVnaW4gXCInICsgcGx1Z2luTmFtZSArICdcIicpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5fY2Fyb3VzZWw7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIF90cmlnZ2VyOiBmdW5jdGlvbih0eXBlLCBlbGVtZW50LCBkYXRhKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgZXZlbnQsXHJcbiAgICAgICAgICAgICAgICAgICAgZGVmYXVsdFByZXZlbnRlZCA9IGZhbHNlO1xyXG5cclxuICAgICAgICAgICAgICAgIGRhdGEgPSBbdGhpc10uY29uY2F0KGRhdGEgfHwgW10pO1xyXG5cclxuICAgICAgICAgICAgICAgIChlbGVtZW50IHx8IHRoaXMuX2VsZW1lbnQpLmVhY2goZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnQgPSAkLkV2ZW50KChwbHVnaW5OYW1lICsgJzonICsgdHlwZSkudG9Mb3dlckNhc2UoKSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICQodGhpcykudHJpZ2dlcihldmVudCwgZGF0YSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChldmVudC5pc0RlZmF1bHRQcmV2ZW50ZWQoKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZWZhdWx0UHJldmVudGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gIWRlZmF1bHRQcmV2ZW50ZWQ7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG4gICAgfTtcclxuXHJcbiAgICBqQ2Fyb3VzZWwucGx1Z2luID0gZnVuY3Rpb24ocGx1Z2luTmFtZSwgcGx1Z2luUHJvdG90eXBlKSB7XHJcbiAgICAgICAgdmFyIFBsdWdpbiA9ICRbcGx1Z2luTmFtZV0gPSBmdW5jdGlvbihlbGVtZW50LCBvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIHRoaXMuX2VsZW1lbnQgPSAkKGVsZW1lbnQpO1xyXG4gICAgICAgICAgICB0aGlzLm9wdGlvbnMob3B0aW9ucyk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9pbml0KCk7XHJcbiAgICAgICAgICAgIHRoaXMuY3JlYXRlKCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgUGx1Z2luLmZuID0gUGx1Z2luLnByb3RvdHlwZSA9ICQuZXh0ZW5kKFxyXG4gICAgICAgICAgICB7fSxcclxuICAgICAgICAgICAgakNhcm91c2VsLmJhc2UocGx1Z2luTmFtZSksXHJcbiAgICAgICAgICAgIHBsdWdpblByb3RvdHlwZVxyXG4gICAgICAgICk7XHJcblxyXG4gICAgICAgICQuZm5bcGx1Z2luTmFtZV0gPSBmdW5jdGlvbihvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIHZhciBhcmdzICAgICAgICA9IEFycmF5LnByb3RvdHlwZS5zbGljZS5jYWxsKGFyZ3VtZW50cywgMSksXHJcbiAgICAgICAgICAgICAgICByZXR1cm5WYWx1ZSA9IHRoaXM7XHJcblxyXG4gICAgICAgICAgICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdzdHJpbmcnKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmVhY2goZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGluc3RhbmNlID0gJCh0aGlzKS5kYXRhKHBsdWdpbk5hbWUpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoIWluc3RhbmNlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAkLmVycm9yKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJ0Nhbm5vdCBjYWxsIG1ldGhvZHMgb24gJyArIHBsdWdpbk5hbWUgKyAnIHByaW9yIHRvIGluaXRpYWxpemF0aW9uOyAnICtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICdhdHRlbXB0ZWQgdG8gY2FsbCBtZXRob2QgXCInICsgb3B0aW9ucyArICdcIidcclxuICAgICAgICAgICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmICghJC5pc0Z1bmN0aW9uKGluc3RhbmNlW29wdGlvbnNdKSB8fCBvcHRpb25zLmNoYXJBdCgwKSA9PT0gJ18nKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAkLmVycm9yKFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgJ05vIHN1Y2ggbWV0aG9kIFwiJyArIG9wdGlvbnMgKyAnXCIgZm9yICcgKyBwbHVnaW5OYW1lICsgJyBpbnN0YW5jZSdcclxuICAgICAgICAgICAgICAgICAgICAgICAgKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHZhciBtZXRob2RWYWx1ZSA9IGluc3RhbmNlW29wdGlvbnNdLmFwcGx5KGluc3RhbmNlLCBhcmdzKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKG1ldGhvZFZhbHVlICE9PSBpbnN0YW5jZSAmJiB0eXBlb2YgbWV0aG9kVmFsdWUgIT09ICd1bmRlZmluZWQnKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVyblZhbHVlID0gbWV0aG9kVmFsdWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgaW5zdGFuY2UgPSAkKHRoaXMpLmRhdGEocGx1Z2luTmFtZSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChpbnN0YW5jZSBpbnN0YW5jZW9mIFBsdWdpbikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpbnN0YW5jZS5yZWxvYWQob3B0aW9ucyk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbmV3IFBsdWdpbih0aGlzLCBvcHRpb25zKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHJldHVyblZhbHVlO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIHJldHVybiBQbHVnaW47XHJcbiAgICB9O1xyXG59KGpRdWVyeSkpO1xyXG5cclxuKGZ1bmN0aW9uKCQsIHdpbmRvdykge1xyXG4gICAgJ3VzZSBzdHJpY3QnO1xyXG5cclxuICAgIHZhciB0b0Zsb2F0ID0gZnVuY3Rpb24odmFsKSB7XHJcbiAgICAgICAgcmV0dXJuIHBhcnNlRmxvYXQodmFsKSB8fCAwO1xyXG4gICAgfTtcclxuXHJcbiAgICAkLmpDYXJvdXNlbC5wbHVnaW4oJ2pjYXJvdXNlbCcsIHtcclxuICAgICAgICBhbmltYXRpbmc6ICAgZmFsc2UsXHJcbiAgICAgICAgdGFpbDogICAgICAgIDAsXHJcbiAgICAgICAgaW5UYWlsOiAgICAgIGZhbHNlLFxyXG4gICAgICAgIHJlc2l6ZVRpbWVyOiBudWxsLFxyXG4gICAgICAgIGx0OiAgICAgICAgICBudWxsLFxyXG4gICAgICAgIHZlcnRpY2FsOiAgICBmYWxzZSxcclxuICAgICAgICBydGw6ICAgICAgICAgZmFsc2UsXHJcbiAgICAgICAgY2lyY3VsYXI6ICAgIGZhbHNlLFxyXG4gICAgICAgIHVuZGVyZmxvdzogICBmYWxzZSxcclxuICAgICAgICByZWxhdGl2ZTogICAgZmFsc2UsXHJcblxyXG4gICAgICAgIF9vcHRpb25zOiB7XHJcbiAgICAgICAgICAgIGxpc3Q6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMuZWxlbWVudCgpLmNoaWxkcmVuKCkuZXEoMCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGl0ZW1zOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLmxpc3QoKS5jaGlsZHJlbigpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBhbmltYXRpb246ICAgNDAwLFxyXG4gICAgICAgICAgICB0cmFuc2l0aW9uczogZmFsc2UsXHJcbiAgICAgICAgICAgIHdyYXA6ICAgICAgICBudWxsLFxyXG4gICAgICAgICAgICB2ZXJ0aWNhbDogICAgbnVsbCxcclxuICAgICAgICAgICAgcnRsOiAgICAgICAgIG51bGwsXHJcbiAgICAgICAgICAgIGNlbnRlcjogICAgICBmYWxzZVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIC8vIFByb3RlY3RlZCwgZG9uJ3QgYWNjZXNzIGRpcmVjdGx5XHJcbiAgICAgICAgX2xpc3Q6ICAgICAgICAgbnVsbCxcclxuICAgICAgICBfaXRlbXM6ICAgICAgICBudWxsLFxyXG4gICAgICAgIF90YXJnZXQ6ICAgICAgIG51bGwsXHJcbiAgICAgICAgX2ZpcnN0OiAgICAgICAgbnVsbCxcclxuICAgICAgICBfbGFzdDogICAgICAgICBudWxsLFxyXG4gICAgICAgIF92aXNpYmxlOiAgICAgIG51bGwsXHJcbiAgICAgICAgX2Z1bGx5dmlzaWJsZTogbnVsbCxcclxuICAgICAgICBfaW5pdDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHZhciBzZWxmID0gdGhpcztcclxuXHJcbiAgICAgICAgICAgIHRoaXMub25XaW5kb3dSZXNpemUgPSBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgIGlmIChzZWxmLnJlc2l6ZVRpbWVyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2xlYXJUaW1lb3V0KHNlbGYucmVzaXplVGltZXIpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHNlbGYucmVzaXplVGltZXIgPSBzZXRUaW1lb3V0KGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHNlbGYucmVsb2FkKCk7XHJcbiAgICAgICAgICAgICAgICB9LCAxMDApO1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfY3JlYXRlOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5fcmVsb2FkKCk7XHJcblxyXG4gICAgICAgICAgICAkKHdpbmRvdykub24oJ3Jlc2l6ZS5qY2Fyb3VzZWwnLCB0aGlzLm9uV2luZG93UmVzaXplKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9kZXN0cm95OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgJCh3aW5kb3cpLm9mZigncmVzaXplLmpjYXJvdXNlbCcsIHRoaXMub25XaW5kb3dSZXNpemUpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX3JlbG9hZDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMudmVydGljYWwgPSB0aGlzLm9wdGlvbnMoJ3ZlcnRpY2FsJyk7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy52ZXJ0aWNhbCA9PSBudWxsKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnZlcnRpY2FsID0gdGhpcy5saXN0KCkuaGVpZ2h0KCkgPiB0aGlzLmxpc3QoKS53aWR0aCgpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLnJ0bCA9IHRoaXMub3B0aW9ucygncnRsJyk7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5ydGwgPT0gbnVsbCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5ydGwgPSAoZnVuY3Rpb24oZWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICgoJycgKyBlbGVtZW50LmF0dHIoJ2RpcicpKS50b0xvd2VyQ2FzZSgpID09PSAncnRsJykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHZhciBmb3VuZCA9IGZhbHNlO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBlbGVtZW50LnBhcmVudHMoJ1tkaXJdJykuZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCgvcnRsL2kpLnRlc3QoJCh0aGlzKS5hdHRyKCdkaXInKSkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvdW5kID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZm91bmQ7XHJcbiAgICAgICAgICAgICAgICB9KHRoaXMuX2VsZW1lbnQpKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdGhpcy5sdCA9IHRoaXMudmVydGljYWwgPyAndG9wJyA6ICdsZWZ0JztcclxuXHJcbiAgICAgICAgICAgIC8vIEVuc3VyZSBiZWZvcmUgY2xvc2VzdCgpIGNhbGxcclxuICAgICAgICAgICAgdGhpcy5yZWxhdGl2ZSA9IHRoaXMubGlzdCgpLmNzcygncG9zaXRpb24nKSA9PT0gJ3JlbGF0aXZlJztcclxuXHJcbiAgICAgICAgICAgIC8vIEZvcmNlIGxpc3QgYW5kIGl0ZW1zIHJlbG9hZFxyXG4gICAgICAgICAgICB0aGlzLl9saXN0ICA9IG51bGw7XHJcbiAgICAgICAgICAgIHRoaXMuX2l0ZW1zID0gbnVsbDtcclxuXHJcbiAgICAgICAgICAgIHZhciBpdGVtID0gdGhpcy5fdGFyZ2V0ICYmIHRoaXMuaW5kZXgodGhpcy5fdGFyZ2V0KSA+PSAwID9cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fdGFyZ2V0IDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5jbG9zZXN0KCk7XHJcblxyXG4gICAgICAgICAgICAvLyBfcHJlcGFyZSgpIG5lZWRzIHRoaXMgaGVyZVxyXG4gICAgICAgICAgICB0aGlzLmNpcmN1bGFyICA9IHRoaXMub3B0aW9ucygnd3JhcCcpID09PSAnY2lyY3VsYXInO1xyXG4gICAgICAgICAgICB0aGlzLnVuZGVyZmxvdyA9IGZhbHNlO1xyXG5cclxuICAgICAgICAgICAgdmFyIHByb3BzID0geydsZWZ0JzogMCwgJ3RvcCc6IDB9O1xyXG5cclxuICAgICAgICAgICAgaWYgKGl0ZW0ubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fcHJlcGFyZShpdGVtKTtcclxuICAgICAgICAgICAgICAgIHRoaXMubGlzdCgpLmZpbmQoJ1tkYXRhLWpjYXJvdXNlbC1jbG9uZV0nKS5yZW1vdmUoKTtcclxuXHJcbiAgICAgICAgICAgICAgICAvLyBGb3JjZSBpdGVtcyByZWxvYWRcclxuICAgICAgICAgICAgICAgIHRoaXMuX2l0ZW1zID0gbnVsbDtcclxuXHJcbiAgICAgICAgICAgICAgICB0aGlzLnVuZGVyZmxvdyA9IHRoaXMuX2Z1bGx5dmlzaWJsZS5sZW5ndGggPj0gdGhpcy5pdGVtcygpLmxlbmd0aDtcclxuICAgICAgICAgICAgICAgIHRoaXMuY2lyY3VsYXIgID0gdGhpcy5jaXJjdWxhciAmJiAhdGhpcy51bmRlcmZsb3c7XHJcblxyXG4gICAgICAgICAgICAgICAgcHJvcHNbdGhpcy5sdF0gPSB0aGlzLl9wb3NpdGlvbihpdGVtKSArICdweCc7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMubW92ZShwcm9wcyk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIGxpc3Q6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5fbGlzdCA9PT0gbnVsbCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIG9wdGlvbiA9IHRoaXMub3B0aW9ucygnbGlzdCcpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fbGlzdCA9ICQuaXNGdW5jdGlvbihvcHRpb24pID8gb3B0aW9uLmNhbGwodGhpcykgOiB0aGlzLl9lbGVtZW50LmZpbmQob3B0aW9uKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX2xpc3Q7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBpdGVtczogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLl9pdGVtcyA9PT0gbnVsbCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIG9wdGlvbiA9IHRoaXMub3B0aW9ucygnaXRlbXMnKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuX2l0ZW1zID0gKCQuaXNGdW5jdGlvbihvcHRpb24pID8gb3B0aW9uLmNhbGwodGhpcykgOiB0aGlzLmxpc3QoKS5maW5kKG9wdGlvbikpLm5vdCgnW2RhdGEtamNhcm91c2VsLWNsb25lXScpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5faXRlbXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBpbmRleDogZnVuY3Rpb24oaXRlbSkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5pdGVtcygpLmluZGV4KGl0ZW0pO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgY2xvc2VzdDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHZhciBzZWxmICAgID0gdGhpcyxcclxuICAgICAgICAgICAgICAgIHBvcyAgICAgPSB0aGlzLmxpc3QoKS5wb3NpdGlvbigpW3RoaXMubHRdLFxyXG4gICAgICAgICAgICAgICAgY2xvc2VzdCA9ICQoKSwgLy8gRW5zdXJlIHdlJ3JlIHJldHVybmluZyBhIGpRdWVyeSBpbnN0YW5jZVxyXG4gICAgICAgICAgICAgICAgc3RvcCAgICA9IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgbHJiICAgICA9IHRoaXMudmVydGljYWwgPyAnYm90dG9tJyA6ICh0aGlzLnJ0bCAmJiAhdGhpcy5yZWxhdGl2ZSA/ICdsZWZ0JyA6ICdyaWdodCcpLFxyXG4gICAgICAgICAgICAgICAgd2lkdGg7XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5ydGwgJiYgdGhpcy5yZWxhdGl2ZSAmJiAhdGhpcy52ZXJ0aWNhbCkge1xyXG4gICAgICAgICAgICAgICAgcG9zICs9IHRoaXMubGlzdCgpLndpZHRoKCkgLSB0aGlzLmNsaXBwaW5nKCk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuaXRlbXMoKS5lYWNoKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgY2xvc2VzdCA9ICQodGhpcyk7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKHN0b3ApIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgdmFyIGRpbSA9IHNlbGYuZGltZW5zaW9uKGNsb3Nlc3QpO1xyXG5cclxuICAgICAgICAgICAgICAgIHBvcyArPSBkaW07XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKHBvcyA+PSAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgd2lkdGggPSBkaW0gLSB0b0Zsb2F0KGNsb3Nlc3QuY3NzKCdtYXJnaW4tJyArIGxyYikpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoKE1hdGguYWJzKHBvcykgLSBkaW0gKyAod2lkdGggLyAyKSkgPD0gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBzdG9wID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuXHJcblxyXG4gICAgICAgICAgICByZXR1cm4gY2xvc2VzdDtcclxuICAgICAgICB9LFxyXG4gICAgICAgIHRhcmdldDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl90YXJnZXQ7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBmaXJzdDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9maXJzdDtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGxhc3Q6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fbGFzdDtcclxuICAgICAgICB9LFxyXG4gICAgICAgIHZpc2libGU6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5fdmlzaWJsZTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGZ1bGx5dmlzaWJsZTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9mdWxseXZpc2libGU7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBoYXNOZXh0OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgaWYgKGZhbHNlID09PSB0aGlzLl90cmlnZ2VyKCdoYXNuZXh0JykpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB2YXIgd3JhcCA9IHRoaXMub3B0aW9ucygnd3JhcCcpLFxyXG4gICAgICAgICAgICAgICAgZW5kID0gdGhpcy5pdGVtcygpLmxlbmd0aCAtIDE7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gZW5kID49IDAgJiYgIXRoaXMudW5kZXJmbG93ICYmXHJcbiAgICAgICAgICAgICAgICAoKHdyYXAgJiYgd3JhcCAhPT0gJ2ZpcnN0JykgfHxcclxuICAgICAgICAgICAgICAgICAgICAodGhpcy5pbmRleCh0aGlzLl9sYXN0KSA8IGVuZCkgfHxcclxuICAgICAgICAgICAgICAgICAgICAodGhpcy50YWlsICYmICF0aGlzLmluVGFpbCkpID8gdHJ1ZSA6IGZhbHNlO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgaGFzUHJldjogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGlmIChmYWxzZSA9PT0gdGhpcy5fdHJpZ2dlcignaGFzcHJldicpKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdHJ1ZTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdmFyIHdyYXAgPSB0aGlzLm9wdGlvbnMoJ3dyYXAnKTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLml0ZW1zKCkubGVuZ3RoID4gMCAmJiAhdGhpcy51bmRlcmZsb3cgJiZcclxuICAgICAgICAgICAgICAgICgod3JhcCAmJiB3cmFwICE9PSAnbGFzdCcpIHx8XHJcbiAgICAgICAgICAgICAgICAgICAgKHRoaXMuaW5kZXgodGhpcy5fZmlyc3QpID4gMCkgfHxcclxuICAgICAgICAgICAgICAgICAgICAodGhpcy50YWlsICYmIHRoaXMuaW5UYWlsKSkgPyB0cnVlIDogZmFsc2U7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBjbGlwcGluZzogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9lbGVtZW50Wydpbm5lcicgKyAodGhpcy52ZXJ0aWNhbCA/ICdIZWlnaHQnIDogJ1dpZHRoJyldKCk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBkaW1lbnNpb246IGZ1bmN0aW9uKGVsZW1lbnQpIHtcclxuICAgICAgICAgICAgcmV0dXJuIGVsZW1lbnRbJ291dGVyJyArICh0aGlzLnZlcnRpY2FsID8gJ0hlaWdodCcgOiAnV2lkdGgnKV0odHJ1ZSk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBzY3JvbGw6IGZ1bmN0aW9uKHRhcmdldCwgYW5pbWF0ZSwgY2FsbGJhY2spIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuYW5pbWF0aW5nKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKGZhbHNlID09PSB0aGlzLl90cmlnZ2VyKCdzY3JvbGwnLCBudWxsLCBbdGFyZ2V0LCBhbmltYXRlXSkpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKGFuaW1hdGUpKSB7XHJcbiAgICAgICAgICAgICAgICBjYWxsYmFjayA9IGFuaW1hdGU7XHJcbiAgICAgICAgICAgICAgICBhbmltYXRlICA9IHRydWU7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHZhciBwYXJzZWQgPSAkLmpDYXJvdXNlbC5wYXJzZVRhcmdldCh0YXJnZXQpO1xyXG5cclxuICAgICAgICAgICAgaWYgKHBhcnNlZC5yZWxhdGl2ZSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGVuZCAgICA9IHRoaXMuaXRlbXMoKS5sZW5ndGggLSAxLFxyXG4gICAgICAgICAgICAgICAgICAgIHNjcm9sbCA9IE1hdGguYWJzKHBhcnNlZC50YXJnZXQpLFxyXG4gICAgICAgICAgICAgICAgICAgIHdyYXAgICA9IHRoaXMub3B0aW9ucygnd3JhcCcpLFxyXG4gICAgICAgICAgICAgICAgICAgIGN1cnJlbnQsXHJcbiAgICAgICAgICAgICAgICAgICAgZmlyc3QsXHJcbiAgICAgICAgICAgICAgICAgICAgaW5kZXgsXHJcbiAgICAgICAgICAgICAgICAgICAgc3RhcnQsXHJcbiAgICAgICAgICAgICAgICAgICAgY3VycixcclxuICAgICAgICAgICAgICAgICAgICBpc1Zpc2libGUsXHJcbiAgICAgICAgICAgICAgICAgICAgcHJvcHMsXHJcbiAgICAgICAgICAgICAgICAgICAgaTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAocGFyc2VkLnRhcmdldCA+IDApIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgbGFzdCA9IHRoaXMuaW5kZXgodGhpcy5fbGFzdCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChsYXN0ID49IGVuZCAmJiB0aGlzLnRhaWwpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCF0aGlzLmluVGFpbCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsVGFpbChhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAod3JhcCA9PT0gJ2JvdGgnIHx8IHdyYXAgPT09ICdsYXN0Jykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbCgwLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmICgkLmlzRnVuY3Rpb24oY2FsbGJhY2spKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNhbGxiYWNrLmNhbGwodGhpcywgZmFsc2UpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGN1cnJlbnQgPSB0aGlzLmluZGV4KHRoaXMuX3RhcmdldCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoKHRoaXMudW5kZXJmbG93ICYmIGN1cnJlbnQgPT09IGVuZCAmJiAod3JhcCA9PT0gJ2NpcmN1bGFyJyB8fCB3cmFwID09PSAnYm90aCcgfHwgd3JhcCA9PT0gJ2xhc3QnKSkgfHxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICghdGhpcy51bmRlcmZsb3cgJiYgbGFzdCA9PT0gZW5kICYmICh3cmFwID09PSAnYm90aCcgfHwgd3JhcCA9PT0gJ2xhc3QnKSkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3Njcm9sbCgwLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpbmRleCA9IGN1cnJlbnQgKyBzY3JvbGw7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuY2lyY3VsYXIgJiYgaW5kZXggPiBlbmQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpID0gZW5kO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZ2V0KC0xKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgd2hpbGUgKGkrKyA8IGluZGV4KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZXEoMCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlzVmlzaWJsZSA9IHRoaXMuX3Zpc2libGUuaW5kZXgoY3VycikgPj0gMDtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChpc1Zpc2libGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGN1cnIuYWZ0ZXIoY3Vyci5jbG9uZSh0cnVlKS5hdHRyKCdkYXRhLWpjYXJvdXNlbC1jbG9uZScsIHRydWUpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5saXN0KCkuYXBwZW5kKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCFpc1Zpc2libGUpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHByb3BzID0ge307XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwcm9wc1t0aGlzLmx0XSA9IHRoaXMuZGltZW5zaW9uKGN1cnIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5tb3ZlQnkocHJvcHMpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBGb3JjZSBpdGVtcyByZWxvYWRcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5faXRlbXMgPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKGN1cnIsIGFuaW1hdGUsIGNhbGxiYWNrKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKE1hdGgubWluKGluZGV4LCBlbmQpLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmluVGFpbCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9zY3JvbGwoTWF0aC5tYXgoKHRoaXMuaW5kZXgodGhpcy5fZmlyc3QpIC0gc2Nyb2xsKSArIDEsIDApLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZmlyc3QgID0gdGhpcy5pbmRleCh0aGlzLl9maXJzdCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGN1cnJlbnQgPSB0aGlzLmluZGV4KHRoaXMuX3RhcmdldCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHN0YXJ0ICA9IHRoaXMudW5kZXJmbG93ID8gY3VycmVudCA6IGZpcnN0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpbmRleCAgPSBzdGFydCAtIHNjcm9sbDtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChzdGFydCA8PSAwICYmICgodGhpcy51bmRlcmZsb3cgJiYgd3JhcCA9PT0gJ2NpcmN1bGFyJykgfHwgd3JhcCA9PT0gJ2JvdGgnIHx8IHdyYXAgPT09ICdmaXJzdCcpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9zY3JvbGwoZW5kLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5jaXJjdWxhciAmJiBpbmRleCA8IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpICAgID0gaW5kZXg7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY3VyciA9IHRoaXMuaXRlbXMoKS5nZXQoMCk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdoaWxlIChpKysgPCAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZXEoLTEpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpc1Zpc2libGUgPSB0aGlzLl92aXNpYmxlLmluZGV4KGN1cnIpID49IDA7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoaXNWaXNpYmxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJyLmFmdGVyKGN1cnIuY2xvbmUodHJ1ZSkuYXR0cignZGF0YS1qY2Fyb3VzZWwtY2xvbmUnLCB0cnVlKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMubGlzdCgpLnByZXBlbmQoY3Vycik7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBGb3JjZSBpdGVtcyByZWxvYWRcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5faXRlbXMgPSBudWxsO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmFyIGRpbSA9IHRoaXMuZGltZW5zaW9uKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJvcHMgPSB7fTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcHJvcHNbdGhpcy5sdF0gPSAtZGltO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLm1vdmVCeShwcm9wcyk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKGN1cnIsIGFuaW1hdGUsIGNhbGxiYWNrKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKE1hdGgubWF4KGluZGV4LCAwKSwgYW5pbWF0ZSwgY2FsbGJhY2spO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fc2Nyb2xsKHBhcnNlZC50YXJnZXQsIGFuaW1hdGUsIGNhbGxiYWNrKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdGhpcy5fdHJpZ2dlcignc2Nyb2xsZW5kJyk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIG1vdmVCeTogZnVuY3Rpb24ocHJvcGVydGllcywgb3B0cykge1xyXG4gICAgICAgICAgICB2YXIgcG9zaXRpb24gPSB0aGlzLmxpc3QoKS5wb3NpdGlvbigpLFxyXG4gICAgICAgICAgICAgICAgbXVsdGlwbGllciA9IDEsXHJcbiAgICAgICAgICAgICAgICBjb3JyZWN0aW9uID0gMDtcclxuXHJcbiAgICAgICAgICAgIGlmICh0aGlzLnJ0bCAmJiAhdGhpcy52ZXJ0aWNhbCkge1xyXG4gICAgICAgICAgICAgICAgbXVsdGlwbGllciA9IC0xO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLnJlbGF0aXZlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY29ycmVjdGlvbiA9IHRoaXMubGlzdCgpLndpZHRoKCkgLSB0aGlzLmNsaXBwaW5nKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmIChwcm9wZXJ0aWVzLmxlZnQpIHtcclxuICAgICAgICAgICAgICAgIHByb3BlcnRpZXMubGVmdCA9IChwb3NpdGlvbi5sZWZ0ICsgY29ycmVjdGlvbiArIHRvRmxvYXQocHJvcGVydGllcy5sZWZ0KSAqIG11bHRpcGxpZXIpICsgJ3B4JztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKHByb3BlcnRpZXMudG9wKSB7XHJcbiAgICAgICAgICAgICAgICBwcm9wZXJ0aWVzLnRvcCA9IChwb3NpdGlvbi50b3AgKyBjb3JyZWN0aW9uICsgdG9GbG9hdChwcm9wZXJ0aWVzLnRvcCkgKiBtdWx0aXBsaWVyKSArICdweCc7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLm1vdmUocHJvcGVydGllcywgb3B0cyk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBtb3ZlOiBmdW5jdGlvbihwcm9wZXJ0aWVzLCBvcHRzKSB7XHJcbiAgICAgICAgICAgIG9wdHMgPSBvcHRzIHx8IHt9O1xyXG5cclxuICAgICAgICAgICAgdmFyIG9wdGlvbiAgICAgICA9IHRoaXMub3B0aW9ucygndHJhbnNpdGlvbnMnKSxcclxuICAgICAgICAgICAgICAgIHRyYW5zaXRpb25zICA9ICEhb3B0aW9uLFxyXG4gICAgICAgICAgICAgICAgdHJhbnNmb3JtcyAgID0gISFvcHRpb24udHJhbnNmb3JtcyxcclxuICAgICAgICAgICAgICAgIHRyYW5zZm9ybXMzZCA9ICEhb3B0aW9uLnRyYW5zZm9ybXMzZCxcclxuICAgICAgICAgICAgICAgIGR1cmF0aW9uICAgICA9IG9wdHMuZHVyYXRpb24gfHwgMCxcclxuICAgICAgICAgICAgICAgIGxpc3QgICAgICAgICA9IHRoaXMubGlzdCgpO1xyXG5cclxuICAgICAgICAgICAgaWYgKCF0cmFuc2l0aW9ucyAmJiBkdXJhdGlvbiA+IDApIHtcclxuICAgICAgICAgICAgICAgIGxpc3QuYW5pbWF0ZShwcm9wZXJ0aWVzLCBvcHRzKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdmFyIGNvbXBsZXRlID0gb3B0cy5jb21wbGV0ZSB8fCAkLm5vb3AsXHJcbiAgICAgICAgICAgICAgICBjc3MgPSB7fTtcclxuXHJcbiAgICAgICAgICAgIGlmICh0cmFuc2l0aW9ucykge1xyXG4gICAgICAgICAgICAgICAgdmFyIGJhY2t1cCA9IGxpc3QuY3NzKFsndHJhbnNpdGlvbkR1cmF0aW9uJywgJ3RyYW5zaXRpb25UaW1pbmdGdW5jdGlvbicsICd0cmFuc2l0aW9uUHJvcGVydHknXSksXHJcbiAgICAgICAgICAgICAgICAgICAgb2xkQ29tcGxldGUgPSBjb21wbGV0ZTtcclxuXHJcbiAgICAgICAgICAgICAgICBjb21wbGV0ZSA9IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICQodGhpcykuY3NzKGJhY2t1cCk7XHJcbiAgICAgICAgICAgICAgICAgICAgb2xkQ29tcGxldGUuY2FsbCh0aGlzKTtcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgICAgICBjc3MgPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdHJhbnNpdGlvbkR1cmF0aW9uOiAoZHVyYXRpb24gPiAwID8gZHVyYXRpb24gLyAxMDAwIDogMCkgKyAncycsXHJcbiAgICAgICAgICAgICAgICAgICAgdHJhbnNpdGlvblRpbWluZ0Z1bmN0aW9uOiBvcHRpb24uZWFzaW5nIHx8IG9wdHMuZWFzaW5nLFxyXG4gICAgICAgICAgICAgICAgICAgIHRyYW5zaXRpb25Qcm9wZXJ0eTogZHVyYXRpb24gPiAwID8gKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAodHJhbnNmb3JtcyB8fCB0cmFuc2Zvcm1zM2QpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIFdlIGhhdmUgdG8gdXNlICdhbGwnIGJlY2F1c2UgalF1ZXJ5IGRvZXNuJ3QgcHJlZml4XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAvLyBjc3MgdmFsdWVzLCBsaWtlIHRyYW5zaXRpb24tcHJvcGVydHk6IHRyYW5zZm9ybTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAnYWxsJztcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHByb3BlcnRpZXMubGVmdCA/ICdsZWZ0JyA6ICd0b3AnO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pKCkgOiAnbm9uZScsXHJcbiAgICAgICAgICAgICAgICAgICAgdHJhbnNmb3JtOiAnbm9uZSdcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICh0cmFuc2Zvcm1zM2QpIHtcclxuICAgICAgICAgICAgICAgIGNzcy50cmFuc2Zvcm0gPSAndHJhbnNsYXRlM2QoJyArIChwcm9wZXJ0aWVzLmxlZnQgfHwgMCkgKyAnLCcgKyAocHJvcGVydGllcy50b3AgfHwgMCkgKyAnLDApJztcclxuICAgICAgICAgICAgfSBlbHNlIGlmICh0cmFuc2Zvcm1zKSB7XHJcbiAgICAgICAgICAgICAgICBjc3MudHJhbnNmb3JtID0gJ3RyYW5zbGF0ZSgnICsgKHByb3BlcnRpZXMubGVmdCB8fCAwKSArICcsJyArIChwcm9wZXJ0aWVzLnRvcCB8fCAwKSArICcpJztcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICQuZXh0ZW5kKGNzcywgcHJvcGVydGllcyk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICh0cmFuc2l0aW9ucyAmJiBkdXJhdGlvbiA+IDApIHtcclxuICAgICAgICAgICAgICAgIGxpc3Qub25lKCd0cmFuc2l0aW9uZW5kIHdlYmtpdFRyYW5zaXRpb25FbmQgb1RyYW5zaXRpb25FbmQgb3RyYW5zaXRpb25lbmQgTVNUcmFuc2l0aW9uRW5kJywgY29tcGxldGUpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBsaXN0LmNzcyhjc3MpO1xyXG5cclxuICAgICAgICAgICAgaWYgKGR1cmF0aW9uIDw9IDApIHtcclxuICAgICAgICAgICAgICAgIGxpc3QuZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICBjb21wbGV0ZS5jYWxsKHRoaXMpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIF9zY3JvbGw6IGZ1bmN0aW9uKGl0ZW0sIGFuaW1hdGUsIGNhbGxiYWNrKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmFuaW1hdGluZykge1xyXG4gICAgICAgICAgICAgICAgaWYgKCQuaXNGdW5jdGlvbihjYWxsYmFjaykpIHtcclxuICAgICAgICAgICAgICAgICAgICBjYWxsYmFjay5jYWxsKHRoaXMsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKHR5cGVvZiBpdGVtICE9PSAnb2JqZWN0Jykge1xyXG4gICAgICAgICAgICAgICAgaXRlbSA9IHRoaXMuaXRlbXMoKS5lcShpdGVtKTtcclxuICAgICAgICAgICAgfSBlbHNlIGlmICh0eXBlb2YgaXRlbS5qcXVlcnkgPT09ICd1bmRlZmluZWQnKSB7XHJcbiAgICAgICAgICAgICAgICBpdGVtID0gJChpdGVtKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKGl0ZW0ubGVuZ3RoID09PSAwKSB7XHJcbiAgICAgICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKGNhbGxiYWNrKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNhbGxiYWNrLmNhbGwodGhpcywgZmFsc2UpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB0aGlzLmluVGFpbCA9IGZhbHNlO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fcHJlcGFyZShpdGVtKTtcclxuXHJcbiAgICAgICAgICAgIHZhciBwb3MgICAgID0gdGhpcy5fcG9zaXRpb24oaXRlbSksXHJcbiAgICAgICAgICAgICAgICBjdXJyUG9zID0gdGhpcy5saXN0KCkucG9zaXRpb24oKVt0aGlzLmx0XTtcclxuXHJcbiAgICAgICAgICAgIGlmIChwb3MgPT09IGN1cnJQb3MpIHtcclxuICAgICAgICAgICAgICAgIGlmICgkLmlzRnVuY3Rpb24oY2FsbGJhY2spKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2FsbGJhY2suY2FsbCh0aGlzLCBmYWxzZSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHZhciBwcm9wZXJ0aWVzID0ge307XHJcbiAgICAgICAgICAgIHByb3BlcnRpZXNbdGhpcy5sdF0gPSBwb3MgKyAncHgnO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fYW5pbWF0ZShwcm9wZXJ0aWVzLCBhbmltYXRlLCBjYWxsYmFjayk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9zY3JvbGxUYWlsOiBmdW5jdGlvbihhbmltYXRlLCBjYWxsYmFjaykge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5hbmltYXRpbmcgfHwgIXRoaXMudGFpbCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKCQuaXNGdW5jdGlvbihjYWxsYmFjaykpIHtcclxuICAgICAgICAgICAgICAgICAgICBjYWxsYmFjay5jYWxsKHRoaXMsIGZhbHNlKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdmFyIHBvcyA9IHRoaXMubGlzdCgpLnBvc2l0aW9uKClbdGhpcy5sdF07XHJcblxyXG4gICAgICAgICAgICBpZiAodGhpcy5ydGwgJiYgdGhpcy5yZWxhdGl2ZSAmJiAhdGhpcy52ZXJ0aWNhbCkge1xyXG4gICAgICAgICAgICAgICAgcG9zICs9IHRoaXMubGlzdCgpLndpZHRoKCkgLSB0aGlzLmNsaXBwaW5nKCk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICh0aGlzLnJ0bCAmJiAhdGhpcy52ZXJ0aWNhbCkge1xyXG4gICAgICAgICAgICAgICAgcG9zICs9IHRoaXMudGFpbDtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHBvcyAtPSB0aGlzLnRhaWw7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuaW5UYWlsID0gdHJ1ZTtcclxuXHJcbiAgICAgICAgICAgIHZhciBwcm9wZXJ0aWVzID0ge307XHJcbiAgICAgICAgICAgIHByb3BlcnRpZXNbdGhpcy5sdF0gPSBwb3MgKyAncHgnO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fdXBkYXRlKHtcclxuICAgICAgICAgICAgICAgIHRhcmdldDogICAgICAgdGhpcy5fdGFyZ2V0Lm5leHQoKSxcclxuICAgICAgICAgICAgICAgIGZ1bGx5dmlzaWJsZTogdGhpcy5fZnVsbHl2aXNpYmxlLnNsaWNlKDEpLmFkZCh0aGlzLl92aXNpYmxlLmxhc3QoKSlcclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9hbmltYXRlKHByb3BlcnRpZXMsIGFuaW1hdGUsIGNhbGxiYWNrKTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2FuaW1hdGU6IGZ1bmN0aW9uKHByb3BlcnRpZXMsIGFuaW1hdGUsIGNhbGxiYWNrKSB7XHJcbiAgICAgICAgICAgIGNhbGxiYWNrID0gY2FsbGJhY2sgfHwgJC5ub29wO1xyXG5cclxuICAgICAgICAgICAgaWYgKGZhbHNlID09PSB0aGlzLl90cmlnZ2VyKCdhbmltYXRlJykpIHtcclxuICAgICAgICAgICAgICAgIGNhbGxiYWNrLmNhbGwodGhpcywgZmFsc2UpO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuYW5pbWF0aW5nID0gdHJ1ZTtcclxuXHJcbiAgICAgICAgICAgIHZhciBhbmltYXRpb24gPSB0aGlzLm9wdGlvbnMoJ2FuaW1hdGlvbicpLFxyXG4gICAgICAgICAgICAgICAgY29tcGxldGUgID0gJC5wcm94eShmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmFuaW1hdGluZyA9IGZhbHNlO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICB2YXIgYyA9IHRoaXMubGlzdCgpLmZpbmQoJ1tkYXRhLWpjYXJvdXNlbC1jbG9uZV0nKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGMubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9yZWxvYWQoKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2FuaW1hdGVlbmQnKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgY2FsbGJhY2suY2FsbCh0aGlzLCB0cnVlKTtcclxuICAgICAgICAgICAgICAgIH0sIHRoaXMpO1xyXG5cclxuICAgICAgICAgICAgdmFyIG9wdHMgPSB0eXBlb2YgYW5pbWF0aW9uID09PSAnb2JqZWN0JyA/XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICQuZXh0ZW5kKHt9LCBhbmltYXRpb24pIDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAge2R1cmF0aW9uOiBhbmltYXRpb259LFxyXG4gICAgICAgICAgICAgICAgb2xkQ29tcGxldGUgPSBvcHRzLmNvbXBsZXRlIHx8ICQubm9vcDtcclxuXHJcbiAgICAgICAgICAgIGlmIChhbmltYXRlID09PSBmYWxzZSkge1xyXG4gICAgICAgICAgICAgICAgb3B0cy5kdXJhdGlvbiA9IDA7XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAodHlwZW9mICQuZnguc3BlZWRzW29wdHMuZHVyYXRpb25dICE9PSAndW5kZWZpbmVkJykge1xyXG4gICAgICAgICAgICAgICAgb3B0cy5kdXJhdGlvbiA9ICQuZnguc3BlZWRzW29wdHMuZHVyYXRpb25dO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBvcHRzLmNvbXBsZXRlID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICBjb21wbGV0ZSgpO1xyXG4gICAgICAgICAgICAgICAgb2xkQ29tcGxldGUuY2FsbCh0aGlzKTtcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMubW92ZShwcm9wZXJ0aWVzLCBvcHRzKTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX3ByZXBhcmU6IGZ1bmN0aW9uKGl0ZW0pIHtcclxuICAgICAgICAgICAgdmFyIGluZGV4ICA9IHRoaXMuaW5kZXgoaXRlbSksXHJcbiAgICAgICAgICAgICAgICBpZHggICAgPSBpbmRleCxcclxuICAgICAgICAgICAgICAgIHdoICAgICA9IHRoaXMuZGltZW5zaW9uKGl0ZW0pLFxyXG4gICAgICAgICAgICAgICAgY2xpcCAgID0gdGhpcy5jbGlwcGluZygpLFxyXG4gICAgICAgICAgICAgICAgbHJiICAgID0gdGhpcy52ZXJ0aWNhbCA/ICdib3R0b20nIDogKHRoaXMucnRsID8gJ2xlZnQnICA6ICdyaWdodCcpLFxyXG4gICAgICAgICAgICAgICAgY2VudGVyID0gdGhpcy5vcHRpb25zKCdjZW50ZXInKSxcclxuICAgICAgICAgICAgICAgIHVwZGF0ZSA9IHtcclxuICAgICAgICAgICAgICAgICAgICB0YXJnZXQ6ICAgICAgIGl0ZW0sXHJcbiAgICAgICAgICAgICAgICAgICAgZmlyc3Q6ICAgICAgICBpdGVtLFxyXG4gICAgICAgICAgICAgICAgICAgIGxhc3Q6ICAgICAgICAgaXRlbSxcclxuICAgICAgICAgICAgICAgICAgICB2aXNpYmxlOiAgICAgIGl0ZW0sXHJcbiAgICAgICAgICAgICAgICAgICAgZnVsbHl2aXNpYmxlOiB3aCA8PSBjbGlwID8gaXRlbSA6ICQoKVxyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGN1cnIsXHJcbiAgICAgICAgICAgICAgICBpc1Zpc2libGUsXHJcbiAgICAgICAgICAgICAgICBtYXJnaW4sXHJcbiAgICAgICAgICAgICAgICBkaW07XHJcblxyXG4gICAgICAgICAgICBpZiAoY2VudGVyKSB7XHJcbiAgICAgICAgICAgICAgICB3aCAvPSAyO1xyXG4gICAgICAgICAgICAgICAgY2xpcCAvPSAyO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAod2ggPCBjbGlwKSB7XHJcbiAgICAgICAgICAgICAgICB3aGlsZSAodHJ1ZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGN1cnIgPSB0aGlzLml0ZW1zKCkuZXEoKytpZHgpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoY3Vyci5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCF0aGlzLmNpcmN1bGFyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgY3VyciA9IHRoaXMuaXRlbXMoKS5lcSgwKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChpdGVtLmdldCgwKSA9PT0gY3Vyci5nZXQoMCkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpc1Zpc2libGUgPSB0aGlzLl92aXNpYmxlLmluZGV4KGN1cnIpID49IDA7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoaXNWaXNpYmxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJyLmFmdGVyKGN1cnIuY2xvbmUodHJ1ZSkuYXR0cignZGF0YS1qY2Fyb3VzZWwtY2xvbmUnLCB0cnVlKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMubGlzdCgpLmFwcGVuZChjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICghaXNWaXNpYmxlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgcHJvcHMgPSB7fTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHByb3BzW3RoaXMubHRdID0gdGhpcy5kaW1lbnNpb24oY3Vycik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB0aGlzLm1vdmVCeShwcm9wcyk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC8vIEZvcmNlIGl0ZW1zIHJlbG9hZFxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9pdGVtcyA9IG51bGw7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBkaW0gPSB0aGlzLmRpbWVuc2lvbihjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGRpbSA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHdoICs9IGRpbTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdXBkYXRlLmxhc3QgICAgPSBjdXJyO1xyXG4gICAgICAgICAgICAgICAgICAgIHVwZGF0ZS52aXNpYmxlID0gdXBkYXRlLnZpc2libGUuYWRkKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAvLyBSZW1vdmUgcmlnaHQvYm90dG9tIG1hcmdpbiBmcm9tIHRvdGFsIHdpZHRoXHJcbiAgICAgICAgICAgICAgICAgICAgbWFyZ2luID0gdG9GbG9hdChjdXJyLmNzcygnbWFyZ2luLScgKyBscmIpKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCh3aCAtIG1hcmdpbikgPD0gY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB1cGRhdGUuZnVsbHl2aXNpYmxlID0gdXBkYXRlLmZ1bGx5dmlzaWJsZS5hZGQoY3Vycik7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAod2ggPj0gY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmICghdGhpcy5jaXJjdWxhciAmJiAhY2VudGVyICYmIHdoIDwgY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgaWR4ID0gaW5kZXg7XHJcblxyXG4gICAgICAgICAgICAgICAgd2hpbGUgKHRydWUpIHtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoLS1pZHggPCAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICAgICAgY3VyciA9IHRoaXMuaXRlbXMoKS5lcShpZHgpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoY3Vyci5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBkaW0gPSB0aGlzLmRpbWVuc2lvbihjdXJyKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGRpbSA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHdoICs9IGRpbTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdXBkYXRlLmZpcnN0ICAgPSBjdXJyO1xyXG4gICAgICAgICAgICAgICAgICAgIHVwZGF0ZS52aXNpYmxlID0gdXBkYXRlLnZpc2libGUuYWRkKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAvLyBSZW1vdmUgcmlnaHQvYm90dG9tIG1hcmdpbiBmcm9tIHRvdGFsIHdpZHRoXHJcbiAgICAgICAgICAgICAgICAgICAgbWFyZ2luID0gdG9GbG9hdChjdXJyLmNzcygnbWFyZ2luLScgKyBscmIpKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCh3aCAtIG1hcmdpbikgPD0gY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB1cGRhdGUuZnVsbHl2aXNpYmxlID0gdXBkYXRlLmZ1bGx5dmlzaWJsZS5hZGQoY3Vycik7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAod2ggPj0gY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3VwZGF0ZSh1cGRhdGUpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy50YWlsID0gMDtcclxuXHJcbiAgICAgICAgICAgIGlmICghY2VudGVyICYmXHJcbiAgICAgICAgICAgICAgICB0aGlzLm9wdGlvbnMoJ3dyYXAnKSAhPT0gJ2NpcmN1bGFyJyAmJlxyXG4gICAgICAgICAgICAgICAgdGhpcy5vcHRpb25zKCd3cmFwJykgIT09ICdjdXN0b20nICYmXHJcbiAgICAgICAgICAgICAgICB0aGlzLmluZGV4KHVwZGF0ZS5sYXN0KSA9PT0gKHRoaXMuaXRlbXMoKS5sZW5ndGggLSAxKSkge1xyXG5cclxuICAgICAgICAgICAgICAgIC8vIFJlbW92ZSByaWdodC9ib3R0b20gbWFyZ2luIGZyb20gdG90YWwgd2lkdGhcclxuICAgICAgICAgICAgICAgIHdoIC09IHRvRmxvYXQodXBkYXRlLmxhc3QuY3NzKCdtYXJnaW4tJyArIGxyYikpO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmICh3aCA+IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnRhaWwgPSB3aCAtIGNsaXA7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX3Bvc2l0aW9uOiBmdW5jdGlvbihpdGVtKSB7XHJcbiAgICAgICAgICAgIHZhciBmaXJzdCAgPSB0aGlzLl9maXJzdCxcclxuICAgICAgICAgICAgICAgIHBvcyAgICA9IGZpcnN0LnBvc2l0aW9uKClbdGhpcy5sdF0sXHJcbiAgICAgICAgICAgICAgICBjZW50ZXIgPSB0aGlzLm9wdGlvbnMoJ2NlbnRlcicpLFxyXG4gICAgICAgICAgICAgICAgY2VudGVyT2Zmc2V0ID0gY2VudGVyID8gKHRoaXMuY2xpcHBpbmcoKSAvIDIpIC0gKHRoaXMuZGltZW5zaW9uKGZpcnN0KSAvIDIpIDogMDtcclxuXHJcbiAgICAgICAgICAgIGlmICh0aGlzLnJ0bCAmJiAhdGhpcy52ZXJ0aWNhbCkge1xyXG4gICAgICAgICAgICAgICAgaWYgKHRoaXMucmVsYXRpdmUpIHtcclxuICAgICAgICAgICAgICAgICAgICBwb3MgLT0gdGhpcy5saXN0KCkud2lkdGgoKSAtIHRoaXMuZGltZW5zaW9uKGZpcnN0KTtcclxuICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcG9zIC09IHRoaXMuY2xpcHBpbmcoKSAtIHRoaXMuZGltZW5zaW9uKGZpcnN0KTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICBwb3MgKz0gY2VudGVyT2Zmc2V0O1xyXG4gICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgcG9zIC09IGNlbnRlck9mZnNldDtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKCFjZW50ZXIgJiZcclxuICAgICAgICAgICAgICAgICh0aGlzLmluZGV4KGl0ZW0pID4gdGhpcy5pbmRleChmaXJzdCkgfHwgdGhpcy5pblRhaWwpICYmXHJcbiAgICAgICAgICAgICAgICB0aGlzLnRhaWwpIHtcclxuICAgICAgICAgICAgICAgIHBvcyA9IHRoaXMucnRsICYmICF0aGlzLnZlcnRpY2FsID8gcG9zIC0gdGhpcy50YWlsIDogcG9zICsgdGhpcy50YWlsO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5pblRhaWwgPSB0cnVlO1xyXG4gICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5pblRhaWwgPSBmYWxzZTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgcmV0dXJuIC1wb3M7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfdXBkYXRlOiBmdW5jdGlvbih1cGRhdGUpIHtcclxuICAgICAgICAgICAgdmFyIHNlbGYgPSB0aGlzLFxyXG4gICAgICAgICAgICAgICAgY3VycmVudCA9IHtcclxuICAgICAgICAgICAgICAgICAgICB0YXJnZXQ6ICAgICAgIHRoaXMuX3RhcmdldCB8fCAkKCksXHJcbiAgICAgICAgICAgICAgICAgICAgZmlyc3Q6ICAgICAgICB0aGlzLl9maXJzdCB8fCAkKCksXHJcbiAgICAgICAgICAgICAgICAgICAgbGFzdDogICAgICAgICB0aGlzLl9sYXN0IHx8ICQoKSxcclxuICAgICAgICAgICAgICAgICAgICB2aXNpYmxlOiAgICAgIHRoaXMuX3Zpc2libGUgfHwgJCgpLFxyXG4gICAgICAgICAgICAgICAgICAgIGZ1bGx5dmlzaWJsZTogdGhpcy5fZnVsbHl2aXNpYmxlIHx8ICQoKVxyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGJhY2sgPSB0aGlzLmluZGV4KHVwZGF0ZS5maXJzdCB8fCBjdXJyZW50LmZpcnN0KSA8IHRoaXMuaW5kZXgoY3VycmVudC5maXJzdCksXHJcbiAgICAgICAgICAgICAgICBrZXksXHJcbiAgICAgICAgICAgICAgICBkb1VwZGF0ZSA9IGZ1bmN0aW9uKGtleSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBlbEluICA9IFtdLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBlbE91dCA9IFtdO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICB1cGRhdGVba2V5XS5lYWNoKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoY3VycmVudFtrZXldLmluZGV4KHRoaXMpIDwgMCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZWxJbi5wdXNoKHRoaXMpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGN1cnJlbnRba2V5XS5lYWNoKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAodXBkYXRlW2tleV0uaW5kZXgodGhpcykgPCAwKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlbE91dC5wdXNoKHRoaXMpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChiYWNrKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGVsSW4gPSBlbEluLnJldmVyc2UoKTtcclxuICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBlbE91dCA9IGVsT3V0LnJldmVyc2UoKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHNlbGYuX3RyaWdnZXIoa2V5ICsgJ2luJywgJChlbEluKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5fdHJpZ2dlcihrZXkgKyAnb3V0JywgJChlbE91dCkpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBzZWxmWydfJyArIGtleV0gPSB1cGRhdGVba2V5XTtcclxuICAgICAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICBmb3IgKGtleSBpbiB1cGRhdGUpIHtcclxuICAgICAgICAgICAgICAgIGRvVXBkYXRlKGtleSk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH1cclxuICAgIH0pO1xyXG59KGpRdWVyeSwgd2luZG93KSk7XHJcblxyXG4oZnVuY3Rpb24oJCkge1xyXG4gICAgJ3VzZSBzdHJpY3QnO1xyXG5cclxuICAgICQuamNhcm91c2VsLmZuLnNjcm9sbEludG9WaWV3ID0gZnVuY3Rpb24odGFyZ2V0LCBhbmltYXRlLCBjYWxsYmFjaykge1xyXG4gICAgICAgIHZhciBwYXJzZWQgPSAkLmpDYXJvdXNlbC5wYXJzZVRhcmdldCh0YXJnZXQpLFxyXG4gICAgICAgICAgICBmaXJzdCAgPSB0aGlzLmluZGV4KHRoaXMuX2Z1bGx5dmlzaWJsZS5maXJzdCgpKSxcclxuICAgICAgICAgICAgbGFzdCAgID0gdGhpcy5pbmRleCh0aGlzLl9mdWxseXZpc2libGUubGFzdCgpKSxcclxuICAgICAgICAgICAgaW5kZXg7XHJcblxyXG4gICAgICAgIGlmIChwYXJzZWQucmVsYXRpdmUpIHtcclxuICAgICAgICAgICAgaW5kZXggPSBwYXJzZWQudGFyZ2V0IDwgMCA/IE1hdGgubWF4KDAsIGZpcnN0ICsgcGFyc2VkLnRhcmdldCkgOiBsYXN0ICsgcGFyc2VkLnRhcmdldDtcclxuICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICBpbmRleCA9IHR5cGVvZiBwYXJzZWQudGFyZ2V0ICE9PSAnb2JqZWN0JyA/IHBhcnNlZC50YXJnZXQgOiB0aGlzLmluZGV4KHBhcnNlZC50YXJnZXQpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKGluZGV4IDwgZmlyc3QpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuc2Nyb2xsKGluZGV4LCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAoaW5kZXggPj0gZmlyc3QgJiYgaW5kZXggPD0gbGFzdCkge1xyXG4gICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKGNhbGxiYWNrKSkge1xyXG4gICAgICAgICAgICAgICAgY2FsbGJhY2suY2FsbCh0aGlzLCBmYWxzZSk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIGl0ZW1zID0gdGhpcy5pdGVtcygpLFxyXG4gICAgICAgICAgICBjbGlwID0gdGhpcy5jbGlwcGluZygpLFxyXG4gICAgICAgICAgICBscmIgID0gdGhpcy52ZXJ0aWNhbCA/ICdib3R0b20nIDogKHRoaXMucnRsID8gJ2xlZnQnICA6ICdyaWdodCcpLFxyXG4gICAgICAgICAgICB3aCAgID0gMCxcclxuICAgICAgICAgICAgY3VycjtcclxuXHJcbiAgICAgICAgd2hpbGUgKHRydWUpIHtcclxuICAgICAgICAgICAgY3VyciA9IGl0ZW1zLmVxKGluZGV4KTtcclxuXHJcbiAgICAgICAgICAgIGlmIChjdXJyLmxlbmd0aCA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHdoICs9IHRoaXMuZGltZW5zaW9uKGN1cnIpO1xyXG5cclxuICAgICAgICAgICAgaWYgKHdoID49IGNsaXApIHtcclxuICAgICAgICAgICAgICAgIHZhciBtYXJnaW4gPSBwYXJzZUZsb2F0KGN1cnIuY3NzKCdtYXJnaW4tJyArIGxyYikpIHx8IDA7XHJcbiAgICAgICAgICAgICAgICBpZiAoKHdoIC0gbWFyZ2luKSAhPT0gY2xpcCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGluZGV4Kys7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKGluZGV4IDw9IDApIHtcclxuICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpbmRleC0tO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcmV0dXJuIHRoaXMuc2Nyb2xsKGluZGV4LCBhbmltYXRlLCBjYWxsYmFjayk7XHJcbiAgICB9O1xyXG59KGpRdWVyeSkpO1xyXG5cclxuKGZ1bmN0aW9uKCQpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICAkLmpDYXJvdXNlbC5wbHVnaW4oJ2pjYXJvdXNlbENvbnRyb2wnLCB7XHJcbiAgICAgICAgX29wdGlvbnM6IHtcclxuICAgICAgICAgICAgdGFyZ2V0OiAnKz0xJyxcclxuICAgICAgICAgICAgZXZlbnQ6ICAnY2xpY2snLFxyXG4gICAgICAgICAgICBtZXRob2Q6ICdzY3JvbGwnXHJcbiAgICAgICAgfSxcclxuICAgICAgICBfYWN0aXZlOiBudWxsLFxyXG4gICAgICAgIF9pbml0OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5vbkRlc3Ryb3kgPSAkLnByb3h5KGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fZGVzdHJveSgpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmNyZWF0ZWVuZCcsICQucHJveHkodGhpcy5fY3JlYXRlLCB0aGlzKSk7XHJcbiAgICAgICAgICAgIH0sIHRoaXMpO1xyXG4gICAgICAgICAgICB0aGlzLm9uUmVsb2FkID0gJC5wcm94eSh0aGlzLl9yZWxvYWQsIHRoaXMpO1xyXG4gICAgICAgICAgICB0aGlzLm9uRXZlbnQgPSAkLnByb3h5KGZ1bmN0aW9uKGUpIHtcclxuICAgICAgICAgICAgICAgIGUucHJldmVudERlZmF1bHQoKTtcclxuXHJcbiAgICAgICAgICAgICAgICB2YXIgbWV0aG9kID0gdGhpcy5vcHRpb25zKCdtZXRob2QnKTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoJC5pc0Z1bmN0aW9uKG1ldGhvZCkpIHtcclxuICAgICAgICAgICAgICAgICAgICBtZXRob2QuY2FsbCh0aGlzKTtcclxuICAgICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC5qY2Fyb3VzZWwodGhpcy5vcHRpb25zKCdtZXRob2QnKSwgdGhpcy5vcHRpb25zKCd0YXJnZXQnKSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0sIHRoaXMpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2NyZWF0ZTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgLm9uZSgnamNhcm91c2VsOmRlc3Ryb3knLCB0aGlzLm9uRGVzdHJveSlcclxuICAgICAgICAgICAgICAgIC5vbignamNhcm91c2VsOnJlbG9hZGVuZCBqY2Fyb3VzZWw6c2Nyb2xsZW5kJywgdGhpcy5vblJlbG9hZCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9lbGVtZW50XHJcbiAgICAgICAgICAgICAgICAub24odGhpcy5vcHRpb25zKCdldmVudCcpICsgJy5qY2Fyb3VzZWxjb250cm9sJywgdGhpcy5vbkV2ZW50KTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3JlbG9hZCgpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX2Rlc3Ryb3k6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLl9lbGVtZW50XHJcbiAgICAgICAgICAgICAgICAub2ZmKCcuamNhcm91c2VsY29udHJvbCcsIHRoaXMub25FdmVudCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgIC5vZmYoJ2pjYXJvdXNlbDpkZXN0cm95JywgdGhpcy5vbkRlc3Ryb3kpXHJcbiAgICAgICAgICAgICAgICAub2ZmKCdqY2Fyb3VzZWw6cmVsb2FkZW5kIGpjYXJvdXNlbDpzY3JvbGxlbmQnLCB0aGlzLm9uUmVsb2FkKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9yZWxvYWQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgcGFyc2VkICAgPSAkLmpDYXJvdXNlbC5wYXJzZVRhcmdldCh0aGlzLm9wdGlvbnMoJ3RhcmdldCcpKSxcclxuICAgICAgICAgICAgICAgIGNhcm91c2VsID0gdGhpcy5jYXJvdXNlbCgpLFxyXG4gICAgICAgICAgICAgICAgYWN0aXZlO1xyXG5cclxuICAgICAgICAgICAgaWYgKHBhcnNlZC5yZWxhdGl2ZSkge1xyXG4gICAgICAgICAgICAgICAgYWN0aXZlID0gY2Fyb3VzZWxcclxuICAgICAgICAgICAgICAgICAgICAuamNhcm91c2VsKHBhcnNlZC50YXJnZXQgPiAwID8gJ2hhc05leHQnIDogJ2hhc1ByZXYnKTtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHZhciB0YXJnZXQgPSB0eXBlb2YgcGFyc2VkLnRhcmdldCAhPT0gJ29iamVjdCcgP1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNhcm91c2VsLmpjYXJvdXNlbCgnaXRlbXMnKS5lcShwYXJzZWQudGFyZ2V0KSA6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFyc2VkLnRhcmdldDtcclxuXHJcbiAgICAgICAgICAgICAgICBhY3RpdmUgPSBjYXJvdXNlbC5qY2Fyb3VzZWwoJ3RhcmdldCcpLmluZGV4KHRhcmdldCkgPj0gMDtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKHRoaXMuX2FjdGl2ZSAhPT0gYWN0aXZlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl90cmlnZ2VyKGFjdGl2ZSA/ICdhY3RpdmUnIDogJ2luYWN0aXZlJyk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9hY3RpdmUgPSBhY3RpdmU7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzO1xyXG4gICAgICAgIH1cclxuICAgIH0pO1xyXG59KGpRdWVyeSkpO1xyXG5cclxuKGZ1bmN0aW9uKCQpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICAkLmpDYXJvdXNlbC5wbHVnaW4oJ2pjYXJvdXNlbFBhZ2luYXRpb24nLCB7XHJcbiAgICAgICAgX29wdGlvbnM6IHtcclxuICAgICAgICAgICAgcGVyUGFnZTogbnVsbCxcclxuICAgICAgICAgICAgaXRlbTogZnVuY3Rpb24ocGFnZSkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuICc8YSBocmVmPVwiIycgKyBwYWdlICsgJ1wiPicgKyBwYWdlICsgJzwvYT4nO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBldmVudDogICdjbGljaycsXHJcbiAgICAgICAgICAgIG1ldGhvZDogJ3Njcm9sbCdcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9jYXJvdXNlbEl0ZW1zOiBudWxsLFxyXG4gICAgICAgIF9wYWdlczoge30sXHJcbiAgICAgICAgX2l0ZW1zOiB7fSxcclxuICAgICAgICBfY3VycmVudFBhZ2U6IG51bGwsXHJcbiAgICAgICAgX2luaXQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLm9uRGVzdHJveSA9ICQucHJveHkoZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9kZXN0cm95KCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgICAgICAub25lKCdqY2Fyb3VzZWw6Y3JlYXRlZW5kJywgJC5wcm94eSh0aGlzLl9jcmVhdGUsIHRoaXMpKTtcclxuICAgICAgICAgICAgfSwgdGhpcyk7XHJcbiAgICAgICAgICAgIHRoaXMub25SZWxvYWQgPSAkLnByb3h5KHRoaXMuX3JlbG9hZCwgdGhpcyk7XHJcbiAgICAgICAgICAgIHRoaXMub25TY3JvbGwgPSAkLnByb3h5KHRoaXMuX3VwZGF0ZSwgdGhpcyk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfY3JlYXRlOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAub25lKCdqY2Fyb3VzZWw6ZGVzdHJveScsIHRoaXMub25EZXN0cm95KVxyXG4gICAgICAgICAgICAgICAgLm9uKCdqY2Fyb3VzZWw6cmVsb2FkZW5kJywgdGhpcy5vblJlbG9hZClcclxuICAgICAgICAgICAgICAgIC5vbignamNhcm91c2VsOnNjcm9sbGVuZCcsIHRoaXMub25TY3JvbGwpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5fcmVsb2FkKCk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfZGVzdHJveTogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuX2NsZWFyKCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgIC5vZmYoJ2pjYXJvdXNlbDpkZXN0cm95JywgdGhpcy5vbkRlc3Ryb3kpXHJcbiAgICAgICAgICAgICAgICAub2ZmKCdqY2Fyb3VzZWw6cmVsb2FkZW5kJywgdGhpcy5vblJlbG9hZClcclxuICAgICAgICAgICAgICAgIC5vZmYoJ2pjYXJvdXNlbDpzY3JvbGxlbmQnLCB0aGlzLm9uU2Nyb2xsKTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX2Nhcm91c2VsSXRlbXMgPSBudWxsO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX3JlbG9hZDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHZhciBwZXJQYWdlID0gdGhpcy5vcHRpb25zKCdwZXJQYWdlJyk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl9wYWdlcyA9IHt9O1xyXG4gICAgICAgICAgICB0aGlzLl9pdGVtcyA9IHt9O1xyXG5cclxuICAgICAgICAgICAgLy8gQ2FsY3VsYXRlIHBhZ2VzXHJcbiAgICAgICAgICAgIGlmICgkLmlzRnVuY3Rpb24ocGVyUGFnZSkpIHtcclxuICAgICAgICAgICAgICAgIHBlclBhZ2UgPSBwZXJQYWdlLmNhbGwodGhpcyk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIGlmIChwZXJQYWdlID09IG51bGwpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3BhZ2VzID0gdGhpcy5fY2FsY3VsYXRlUGFnZXMoKTtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgIHZhciBwcCAgICA9IHBhcnNlSW50KHBlclBhZ2UsIDEwKSB8fCAwLFxyXG4gICAgICAgICAgICAgICAgICAgIGl0ZW1zID0gdGhpcy5fZ2V0Q2Fyb3VzZWxJdGVtcygpLFxyXG4gICAgICAgICAgICAgICAgICAgIHBhZ2UgID0gMSxcclxuICAgICAgICAgICAgICAgICAgICBpICAgICA9IDAsXHJcbiAgICAgICAgICAgICAgICAgICAgY3VycjtcclxuXHJcbiAgICAgICAgICAgICAgICB3aGlsZSAodHJ1ZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIGN1cnIgPSBpdGVtcy5lcShpKyspO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoY3Vyci5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBpZiAoIXRoaXMuX3BhZ2VzW3BhZ2VdKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuX3BhZ2VzW3BhZ2VdID0gY3VycjtcclxuICAgICAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLl9wYWdlc1twYWdlXSA9IHRoaXMuX3BhZ2VzW3BhZ2VdLmFkZChjdXJyKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChpICUgcHAgPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcGFnZSsrO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdGhpcy5fY2xlYXIoKTtcclxuXHJcbiAgICAgICAgICAgIHZhciBzZWxmICAgICA9IHRoaXMsXHJcbiAgICAgICAgICAgICAgICBjYXJvdXNlbCA9IHRoaXMuY2Fyb3VzZWwoKS5kYXRhKCdqY2Fyb3VzZWwnKSxcclxuICAgICAgICAgICAgICAgIGVsZW1lbnQgID0gdGhpcy5fZWxlbWVudCxcclxuICAgICAgICAgICAgICAgIGl0ZW0gICAgID0gdGhpcy5vcHRpb25zKCdpdGVtJyksXHJcbiAgICAgICAgICAgICAgICBudW1DYXJvdXNlbEl0ZW1zID0gdGhpcy5fZ2V0Q2Fyb3VzZWxJdGVtcygpLmxlbmd0aDtcclxuXHJcbiAgICAgICAgICAgICQuZWFjaCh0aGlzLl9wYWdlcywgZnVuY3Rpb24ocGFnZSwgY2Fyb3VzZWxJdGVtcykge1xyXG4gICAgICAgICAgICAgICAgdmFyIGN1cnJJdGVtID0gc2VsZi5faXRlbXNbcGFnZV0gPSAkKGl0ZW0uY2FsbChzZWxmLCBwYWdlLCBjYXJvdXNlbEl0ZW1zKSk7XHJcblxyXG4gICAgICAgICAgICAgICAgY3Vyckl0ZW0ub24oc2VsZi5vcHRpb25zKCdldmVudCcpICsgJy5qY2Fyb3VzZWxwYWdpbmF0aW9uJywgJC5wcm94eShmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgdGFyZ2V0ID0gY2Fyb3VzZWxJdGVtcy5lcSgwKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgLy8gSWYgY2lyY3VsYXIgd3JhcHBpbmcgZW5hYmxlZCwgZW5zdXJlIGNvcnJlY3Qgc2Nyb2xsaW5nIGRpcmVjdGlvblxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChjYXJvdXNlbC5jaXJjdWxhcikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB2YXIgY3VycmVudEluZGV4ID0gY2Fyb3VzZWwuaW5kZXgoY2Fyb3VzZWwudGFyZ2V0KCkpLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbmV3SW5kZXggICAgID0gY2Fyb3VzZWwuaW5kZXgodGFyZ2V0KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChwYXJzZUZsb2F0KHBhZ2UpID4gcGFyc2VGbG9hdChzZWxmLl9jdXJyZW50UGFnZSkpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChuZXdJbmRleCA8IGN1cnJlbnRJbmRleCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRhcmdldCA9ICcrPScgKyAobnVtQ2Fyb3VzZWxJdGVtcyAtIGN1cnJlbnRJbmRleCArIG5ld0luZGV4KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGlmIChuZXdJbmRleCA+IGN1cnJlbnRJbmRleCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRhcmdldCA9ICctPScgKyAoY3VycmVudEluZGV4ICsgKG51bUNhcm91c2VsSXRlbXMgLSBuZXdJbmRleCkpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgICAgICBjYXJvdXNlbFt0aGlzLm9wdGlvbnMoJ21ldGhvZCcpXSh0YXJnZXQpO1xyXG4gICAgICAgICAgICAgICAgfSwgc2VsZikpO1xyXG5cclxuICAgICAgICAgICAgICAgIGVsZW1lbnQuYXBwZW5kKGN1cnJJdGVtKTtcclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLl91cGRhdGUoKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF91cGRhdGU6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB2YXIgdGFyZ2V0ID0gdGhpcy5jYXJvdXNlbCgpLmpjYXJvdXNlbCgndGFyZ2V0JyksXHJcbiAgICAgICAgICAgICAgICBjdXJyZW50UGFnZTtcclxuXHJcbiAgICAgICAgICAgICQuZWFjaCh0aGlzLl9wYWdlcywgZnVuY3Rpb24ocGFnZSwgY2Fyb3VzZWxJdGVtcykge1xyXG4gICAgICAgICAgICAgICAgY2Fyb3VzZWxJdGVtcy5lYWNoKGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0YXJnZXQuaXModGhpcykpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY3VycmVudFBhZ2UgPSBwYWdlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKGN1cnJlbnRQYWdlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIGlmICh0aGlzLl9jdXJyZW50UGFnZSAhPT0gY3VycmVudFBhZ2UpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2luYWN0aXZlJywgdGhpcy5faXRlbXNbdGhpcy5fY3VycmVudFBhZ2VdKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3RyaWdnZXIoJ2FjdGl2ZScsIHRoaXMuX2l0ZW1zW2N1cnJlbnRQYWdlXSk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuX2N1cnJlbnRQYWdlID0gY3VycmVudFBhZ2U7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBpdGVtczogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9pdGVtcztcclxuICAgICAgICB9LFxyXG4gICAgICAgIHJlbG9hZENhcm91c2VsSXRlbXM6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLl9jYXJvdXNlbEl0ZW1zID0gbnVsbDtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfY2xlYXI6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICB0aGlzLl9lbGVtZW50LmVtcHR5KCk7XHJcbiAgICAgICAgICAgIHRoaXMuX2N1cnJlbnRQYWdlID0gbnVsbDtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9jYWxjdWxhdGVQYWdlczogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHZhciBjYXJvdXNlbCA9IHRoaXMuY2Fyb3VzZWwoKS5kYXRhKCdqY2Fyb3VzZWwnKSxcclxuICAgICAgICAgICAgICAgIGl0ZW1zICAgID0gdGhpcy5fZ2V0Q2Fyb3VzZWxJdGVtcygpLFxyXG4gICAgICAgICAgICAgICAgY2xpcCAgICAgPSBjYXJvdXNlbC5jbGlwcGluZygpLFxyXG4gICAgICAgICAgICAgICAgd2ggICAgICAgPSAwLFxyXG4gICAgICAgICAgICAgICAgaWR4ICAgICAgPSAwLFxyXG4gICAgICAgICAgICAgICAgcGFnZSAgICAgPSAxLFxyXG4gICAgICAgICAgICAgICAgcGFnZXMgICAgPSB7fSxcclxuICAgICAgICAgICAgICAgIGN1cnI7XHJcblxyXG4gICAgICAgICAgICB3aGlsZSAodHJ1ZSkge1xyXG4gICAgICAgICAgICAgICAgY3VyciA9IGl0ZW1zLmVxKGlkeCsrKTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoY3Vyci5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoIXBhZ2VzW3BhZ2VdKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcGFnZXNbcGFnZV0gPSBjdXJyO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICBwYWdlc1twYWdlXSA9IHBhZ2VzW3BhZ2VdLmFkZChjdXJyKTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICB3aCArPSBjYXJvdXNlbC5kaW1lbnNpb24oY3Vycik7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKHdoID49IGNsaXApIHtcclxuICAgICAgICAgICAgICAgICAgICBwYWdlKys7XHJcbiAgICAgICAgICAgICAgICAgICAgd2ggPSAwO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gcGFnZXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfZ2V0Q2Fyb3VzZWxJdGVtczogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5fY2Fyb3VzZWxJdGVtcykge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5fY2Fyb3VzZWxJdGVtcyA9IHRoaXMuY2Fyb3VzZWwoKS5qY2Fyb3VzZWwoJ2l0ZW1zJyk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9jYXJvdXNlbEl0ZW1zO1xyXG4gICAgICAgIH1cclxuICAgIH0pO1xyXG59KGpRdWVyeSkpO1xyXG5cclxuKGZ1bmN0aW9uKCQpIHtcclxuICAgICd1c2Ugc3RyaWN0JztcclxuXHJcbiAgICAkLmpDYXJvdXNlbC5wbHVnaW4oJ2pjYXJvdXNlbEF1dG9zY3JvbGwnLCB7XHJcbiAgICAgICAgX29wdGlvbnM6IHtcclxuICAgICAgICAgICAgdGFyZ2V0OiAgICAnKz0xJyxcclxuICAgICAgICAgICAgaW50ZXJ2YWw6ICAzMDAwLFxyXG4gICAgICAgICAgICBhdXRvc3RhcnQ6IHRydWVcclxuICAgICAgICB9LFxyXG4gICAgICAgIF90aW1lcjogbnVsbCxcclxuICAgICAgICBfaW5pdDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB0aGlzLm9uRGVzdHJveSA9ICQucHJveHkoZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9kZXN0cm95KCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmNhcm91c2VsKClcclxuICAgICAgICAgICAgICAgICAgICAub25lKCdqY2Fyb3VzZWw6Y3JlYXRlZW5kJywgJC5wcm94eSh0aGlzLl9jcmVhdGUsIHRoaXMpKTtcclxuICAgICAgICAgICAgfSwgdGhpcyk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLm9uQW5pbWF0ZUVuZCA9ICQucHJveHkodGhpcy5zdGFydCwgdGhpcyk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBfY3JlYXRlOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAub25lKCdqY2Fyb3VzZWw6ZGVzdHJveScsIHRoaXMub25EZXN0cm95KTtcclxuXHJcbiAgICAgICAgICAgIGlmICh0aGlzLm9wdGlvbnMoJ2F1dG9zdGFydCcpKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN0YXJ0KCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIF9kZXN0cm95OiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdGhpcy5zdG9wKCk7XHJcbiAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgLm9mZignamNhcm91c2VsOmRlc3Ryb3knLCB0aGlzLm9uRGVzdHJveSk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBzdGFydDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIHRoaXMuc3RvcCgpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpXHJcbiAgICAgICAgICAgICAgICAub25lKCdqY2Fyb3VzZWw6YW5pbWF0ZWVuZCcsIHRoaXMub25BbmltYXRlRW5kKTtcclxuXHJcbiAgICAgICAgICAgIHRoaXMuX3RpbWVyID0gc2V0VGltZW91dCgkLnByb3h5KGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5jYXJvdXNlbCgpLmpjYXJvdXNlbCgnc2Nyb2xsJywgdGhpcy5vcHRpb25zKCd0YXJnZXQnKSk7XHJcbiAgICAgICAgICAgIH0sIHRoaXMpLCB0aGlzLm9wdGlvbnMoJ2ludGVydmFsJykpO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBzdG9wOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuX3RpbWVyKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl90aW1lciA9IGNsZWFyVGltZW91dCh0aGlzLl90aW1lcik7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHRoaXMuY2Fyb3VzZWwoKVxyXG4gICAgICAgICAgICAgICAgLm9mZignamNhcm91c2VsOmFuaW1hdGVlbmQnLCB0aGlzLm9uQW5pbWF0ZUVuZCk7XHJcblxyXG4gICAgICAgICAgICByZXR1cm4gdGhpcztcclxuICAgICAgICB9XHJcbiAgICB9KTtcclxufShqUXVlcnkpKTtcclxuIiwiKGZ1bmN0aW9uICgkKVxyXG57XHJcbiAgICAkKFwiLmpjYXJvdXNlbC1lbmdpbmVcIikuZWFjaChmdW5jdGlvbiAoKVxyXG4gICAge1xyXG4gICAgICAgIHZhciBlbmdpbmUgPSAkKHRoaXMpO1xyXG4gICAgICAgIHZhciBhdXRvU3RhcnQgPSBlbmdpbmUuZGF0YShcImF1dG9zdGFydFwiKTtcclxuICAgICAgICB2YXIgaW50ZXJ2YWwgPSBlbmdpbmUuZGF0YShcImludGVydmFsXCIpIHx8IDMwMDA7XHJcbiAgICAgICAgdmFyIHRyYW5zaXRpb25zID0gZW5naW5lLmRhdGEoXCJ0cmFuc2l0aW9uc1wiKTtcclxuICAgICAgICB2YXIgZWFzaW5nID0gZW5naW5lLmRhdGEoXCJlYXNpbmdcIik7XHJcbiAgICAgICAgdmFyIHdyYXAgPSBlbmdpbmUuZGF0YShcIndyYXBcIik7XHJcbiAgICAgICAgdmFyIHZlcnRpY2FsID0gZW5naW5lLmRhdGEoXCJ2ZXJ0aWNhbFwiKTtcclxuICAgICAgICB2YXIgY2VudGVyID0gZW5naW5lLmRhdGEoXCJjZW50ZXJcIik7XHJcblxyXG4gICAgICAgIGVuZ2luZS5maW5kKFwiLmpjYXJvdXNlbFwiKS5qY2Fyb3VzZWwoe1xyXG4gICAgICAgICAgICB3cmFwOiB3cmFwLFxyXG4gICAgICAgICAgICB2ZXJ0aWNhbDogdmVydGljYWwsXHJcbiAgICAgICAgICAgIGNlbnRlcjogY2VudGVyLFxyXG4gICAgICAgICAgICB0cmFuc2l0aW9uczogdHJhbnNpdGlvbnMgPyB7XHJcbiAgICAgICAgICAgICAgICB0cmFuc2Zvcm1zOiBNb2Rlcm5penIuY3NzdHJhbnNmb3JtcyxcclxuICAgICAgICAgICAgICAgIHRyYW5zZm9ybXMzZDogTW9kZXJuaXpyLmNzc3RyYW5zZm9ybXMzZCxcclxuICAgICAgICAgICAgICAgIGVhc2luZzogZWFzaW5nXHJcbiAgICAgICAgICAgIH0gOiBmYWxzZVxyXG4gICAgICAgIH0pXHJcblxyXG4gICAgICAgIC5qY2Fyb3VzZWxBdXRvc2Nyb2xsKHtcclxuICAgICAgICAgICAgaW50ZXJ2YWw6IGludGVydmFsLFxyXG4gICAgICAgICAgICB0YXJnZXQ6IFwiKz0xXCIsXHJcbiAgICAgICAgICAgIGF1dG9zdGFydDogYXV0b1N0YXJ0XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIGVuZ2luZS5maW5kKFwiLmpjYXJvdXNlbC1wcmV2XCIpLmpjYXJvdXNlbENvbnRyb2woe1xyXG4gICAgICAgICAgICB0YXJnZXQ6IFwiLT0xXCJcclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgZW5naW5lLmZpbmQoXCIuamNhcm91c2VsLW5leHRcIikuamNhcm91c2VsQ29udHJvbCh7XHJcbiAgICAgICAgICAgIHRhcmdldDogXCIrPTFcIlxyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBlbmdpbmUuZmluZChcIi5qY2Fyb3VzZWwtcGFnaW5hdGlvblwiKS5qY2Fyb3VzZWxQYWdpbmF0aW9uKHtcclxuICAgICAgICAgICAgaXRlbTogZnVuY3Rpb24gKHBhZ2UpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBcIjxhIGhyZWY9XFxcIiNcIiArIHBhZ2UgKyBcIlxcXCI+XCIgKyBwYWdlICsgXCI8L2E+XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuICAgIH0pO1xyXG59KShqUXVlcnkpOyJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==