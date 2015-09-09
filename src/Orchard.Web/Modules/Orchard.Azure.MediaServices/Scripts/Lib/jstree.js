/*globals jQuery, define, exports, require, window, document */
(function (factory) {
	"use strict";
	if (typeof define === 'function' && define.amd) {
		define(['jquery'], factory);
	}
	else if(typeof exports === 'object') {
		factory(require('jquery'));
	}
	else {
		factory(jQuery);
	}
}(function ($, undefined) {
	"use strict";
/*!
 * jsTree 3.0.0
 * http://jstree.com/
 *
 * Copyright (c) 2013 Ivan Bozhanov (http://vakata.com)
 *
 * Licensed same as jquery - under the terms of the MIT License
 *   http://www.opensource.org/licenses/mit-license.php
 */
/*!
 * if using jslint please allow for the jQuery global and use following options: 
 * jslint: browser: true, ass: true, bitwise: true, continue: true, nomen: true, plusplus: true, regexp: true, unparam: true, todo: true, white: true
 */

	// prevent another load? maybe there is a better way?
	if($.jstree) {
		return;
	}

	/**
	 * ### jsTree core functionality
	 */

	// internal variables
	var instance_counter = 0,
		ccp_node = false,
		ccp_mode = false,
		ccp_inst = false,
		themes_loaded = [],
		src = $('script:last').attr('src'),
		_d = document, _node = _d.createElement('LI'), _temp1, _temp2;

	_node.setAttribute('role', 'treeitem');
	_temp1 = _d.createElement('I');
	_temp1.className = 'jstree-icon jstree-ocl';
	_node.appendChild(_temp1);
	_temp1 = _d.createElement('A');
	_temp1.className = 'jstree-anchor';
	_temp1.setAttribute('href','#');
	_temp2 = _d.createElement('I');
	_temp2.className = 'jstree-icon jstree-themeicon';
	_temp1.appendChild(_temp2);
	_node.appendChild(_temp1);
	_temp1 = _temp2 = null;


	/**
	 * holds all jstree related functions and variables, including the actual class and methods to create, access and manipulate instances.
	 * @name $.jstree
	 */
	$.jstree = {
		/** 
		 * specifies the jstree version in use
		 * @name $.jstree.version
		 */
		version : '3.0.0-beta9',
		/**
		 * holds all the default options used when creating new instances
		 * @name $.jstree.defaults
		 */
		defaults : {
			/**
			 * configure which plugins will be active on an instance. Should be an array of strings, where each element is a plugin name. The default is `[]`
			 * @name $.jstree.defaults.plugins
			 */
			plugins : []
		},
		/**
		 * stores all loaded jstree plugins (used internally)
		 * @name $.jstree.plugins
		 */
		plugins : {},
		path : src && src.indexOf('/') !== -1 ? src.replace(/\/[^\/]+$/,'') : ''
	};
	/**
	 * creates a jstree instance
	 * @name $.jstree.create(el [, options])
	 * @param {DOMElement|jQuery|String} el the element to create the instance on, can be jQuery extended or a selector
	 * @param {Object} options options for this instance (extends `$.jstree.defaults`)
	 * @return {jsTree} the new instance
	 */
	$.jstree.create = function (el, options) {
		var tmp = new $.jstree.core(++instance_counter),
			opt = options;
		options = $.extend(true, {}, $.jstree.defaults, options);
		if(opt && opt.plugins) {
			options.plugins = opt.plugins;
		}
		$.each(options.plugins, function (i, k) {
			if(i !== 'core') {
				tmp = tmp.plugin(k, options[k]);
			}
		});
		tmp.init(el, options);
		return tmp;
	};
	/**
	 * the jstree class constructor, used only internally
	 * @private
	 * @name $.jstree.core(id)
	 * @param {Number} id this instance's index
	 */
	$.jstree.core = function (id) {
		this._id = id;
		this._cnt = 0;
		this._data = {
			core : {
				themes : {
					name : false,
					dots : false,
					icons : false
				},
				selected : [],
				last_error : {}
			}
		};
	};
	/**
	 * get a reference to an existing instance
	 *
	 * __Examples__
	 *
	 *	// provided a container with an ID of "tree", and a nested node with an ID of "branch"
	 *	// all of there will return the same instance
	 *	$.jstree.reference('tree');
	 *	$.jstree.reference('#tree');
	 *	$.jstree.reference($('#tree'));
	 *	$.jstree.reference(document.getElementByID('tree'));
	 *	$.jstree.reference('branch');
	 *	$.jstree.reference('#branch');
	 *	$.jstree.reference($('#branch'));
	 *	$.jstree.reference(document.getElementByID('branch'));
	 *
	 * @name $.jstree.reference(needle)
	 * @param {DOMElement|jQuery|String} needle
	 * @return {jsTree|null} the instance or `null` if not found
	 */
	$.jstree.reference = function (needle) {
		if(needle && !$(needle).length) {
			if(needle.id) {
				needle = needle.id;
			}
			var tmp = null;
			$('.jstree').each(function () {
				var inst = $(this).data('jstree');
				if(inst && inst._model.data[needle]) {
					tmp = inst;
					return false;
				}
			});
			return tmp;
		}
		return $(needle).closest('.jstree').data('jstree');
	};
	/**
	 * Create an instance, get an instance or invoke a command on a instance. 
	 * 
	 * If there is no instance associated with the current node a new one is created and `arg` is used to extend `$.jstree.defaults` for this new instance. There would be no return value (chaining is not broken).
	 * 
	 * If there is an existing instance and `arg` is a string the command specified by `arg` is executed on the instance, with any additional arguments passed to the function. If the function returns a value it will be returned (chaining could break depending on function).
	 * 
	 * If there is an existing instance and `arg` is not a string the instance itself is returned (similar to `$.jstree.reference`).
	 * 
	 * In any other case - nothing is returned and chaining is not broken.
	 *
	 * __Examples__
	 *
	 *	$('#tree1').jstree(); // creates an instance
	 *	$('#tree2').jstree({ plugins : [] }); // create an instance with some options
	 *	$('#tree1').jstree('open_node', '#branch_1'); // call a method on an existing instance, passing additional arguments
	 *	$('#tree2').jstree(); // get an existing instance (or create an instance)
	 *	$('#tree2').jstree(true); // get an existing instance (will not create new instance)
	 *	$('#branch_1').jstree().select_node('#branch_1'); // get an instance (using a nested element and call a method)
	 *
	 * @name $().jstree([arg])
	 * @param {String|Object} arg
	 * @return {Mixed}
	 */
	$.fn.jstree = function (arg) {
		// check for string argument
		var is_method	= (typeof arg === 'string'),
			args		= Array.prototype.slice.call(arguments, 1),
			result		= null;
		this.each(function () {
			// get the instance (if there is one) and method (if it exists)
			var instance = $.jstree.reference(this),
				method = is_method && instance ? instance[arg] : null;
			// if calling a method, and method is available - execute on the instance
			result = is_method && method ?
				method.apply(instance, args) :
				null;
			// if there is no instance and no method is being called - create one
			if(!instance && !is_method && (arg === undefined || $.isPlainObject(arg))) {
				$(this).data('jstree', new $.jstree.create(this, arg));
			}
			// if there is an instance and no method is called - return the instance
			if(instance && !is_method) {
				result = instance;
			}
			// if there was a method call which returned a result - break and return the value
			if(result !== null && result !== undefined) {
				return false;
			}
		});
		// if there was a method call with a valid return value - return that, otherwise continue the chain
		return result !== null && result !== undefined ?
			result : this;
	};
	/**
	 * used to find elements containing an instance
	 *
	 * __Examples__
	 *
	 *	$('div:jstree').each(function () {
	 *		$(this).jstree('destroy');
	 *	});
	 *
	 * @name $(':jstree')
	 * @return {jQuery}
	 */
	$.expr[':'].jstree = $.expr.createPseudo(function(search) {
		return function(a) {
			return $(a).hasClass('jstree') &&
				$(a).data('jstree') !== undefined;
		};
	});

	/**
	 * stores all defaults for the core
	 * @name $.jstree.defaults.core
	 */
	$.jstree.defaults.core = {
		/**
		 * data configuration
		 * 
		 * If left as `false` the HTML inside the jstree container element is used to populate the tree (that should be an unordered list with list items).
		 *
		 * You can also pass in a HTML string or a JSON array here.
		 * 
		 * It is possible to pass in a standard jQuery-like AJAX config and jstree will automatically determine if the response is JSON or HTML and use that to populate the tree. 
		 * In addition to the standard jQuery ajax options here you can suppy functions for `data` and `url`, the functions will be run in the current instance's scope and a param will be passed indicating which node is being loaded, the return value of those functions will be used.
		 * 
		 * The last option is to specify a function, that function will receive the node being loaded as argument and a second param which is a function which should be called with the result.
		 *
		 * __Examples__
		 *
		 *	// AJAX
		 *	$('#tree').jstree({
		 *		'core' : {
		 *			'data' : {
		 *				'url' : '/get/children/',
		 *				'data' : function (node) {
		 *					return { 'id' : node.id };
		 *				}
		 *			}
		 *		});
		 *
		 *	// direct data
		 *	$('#tree').jstree({
		 *		'core' : {
		 *			'data' : [
		 *				'Simple root node',
		 *				{
		 *					'id' : 'node_2',
		 *					'text' : 'Root node with options',
		 *					'state' : { 'opened' : true, 'selected' : true },
		 *					'children' : [ { 'text' : 'Child 1' }, 'Child 2']
		 *				}
		 *			]
		 *		});
		 *	
		 *	// function
		 *	$('#tree').jstree({
		 *		'core' : {
		 *			'data' : function (obj, callback) {
		 *				callback.call(this, ['Root 1', 'Root 2']);
		 *			}
		 *		});
		 * 
		 * @name $.jstree.defaults.core.data
		 */
		data			: false,
		/**
		 * configure the various strings used throughout the tree
		 *
		 * You can use an object where the key is the string you need to replace and the value is your replacement.
		 * Another option is to specify a function which will be called with an argument of the needed string and should return the replacement.
		 * If left as `false` no replacement is made.
		 *
		 * __Examples__
		 *
		 *	$('#tree').jstree({
		 *		'core' : {
		 *			'strings' : {
		 *				'Loading...' : 'Please wait ...'
		 *			}
		 *		}
		 *	});
		 *
		 * @name $.jstree.defaults.core.strings
		 */
		strings			: false,
		/**
		 * determines what happens when a user tries to modify the structure of the tree
		 * If left as `false` all operations like create, rename, delete, move or copy are prevented.
		 * You can set this to `true` to allow all interactions or use a function to have better control.
		 *
		 * __Examples__
		 *
		 *	$('#tree').jstree({
		 *		'core' : {
		 *			'check_callback' : function (operation, node, node_parent, node_position) {
		 *				// operation can be 'create_node', 'rename_node', 'delete_node', 'move_node' or 'copy_node'
		 *				// in case of 'rename_node' node_position is filled with the new node name
		 *				return operation === 'rename_node' ? true : false;
		 *			}
		 *		}
		 *	});
		 * 
		 * @name $.jstree.defaults.core.check_callback
		 */
		check_callback	: false,
		/**
		 * a callback called with a single object parameter in the instance's scope when something goes wrong (operation prevented, ajax failed, etc)
		 * @name $.jstree.defaults.core.error
		 */
		error			: $.noop,
		/**
		 * the open / close animation duration in milliseconds - set this to `false` to disable the animation (default is `200`)
		 * @name $.jstree.defaults.core.animation
		 */
		animation		: 200,
		/**
		 * a boolean indicating if multiple nodes can be selected
		 * @name $.jstree.defaults.core.multiple
		 */
		multiple		: true,
		/**
		 * theme configuration object
		 * @name $.jstree.defaults.core.themes
		 */
		themes			: {
			/**
			 * the name of the theme to use (if left as `false` the default theme is used)
			 * @name $.jstree.defaults.core.themes.name
			 */
			name			: false,
			/**
			 * the URL of the theme's CSS file, leave this as `false` if you have manually included the theme CSS (recommended). You can set this to `true` too which will try to autoload the theme.
			 * @name $.jstree.defaults.core.themes.url
			 */
			url				: false,
			/**
			 * the location of all jstree themes - only used if `url` is set to `true`
			 * @name $.jstree.defaults.core.themes.dir
			 */
			dir				: false,
			/**
			 * a boolean indicating if connecting dots are shown
			 * @name $.jstree.defaults.core.themes.dots
			 */
			dots			: true,
			/**
			 * a boolean indicating if node icons are shown
			 * @name $.jstree.defaults.core.themes.icons
			 */
			icons			: true,
			/**
			 * a boolean indicating if the tree background is striped
			 * @name $.jstree.defaults.core.themes.stripes
			 */
			stripes			: false,
			/**
			 * a string (or boolean `false`) specifying the theme variant to use (if the theme supports variants)
			 * @name $.jstree.defaults.core.themes.variant
			 */
			variant			: false,
			/**
			 * a boolean specifying if a reponsive version of the theme should kick in on smaller screens (if the theme supports it). Defaults to `true`.
			 * @name $.jstree.defaults.core.themes.responsive
			 */
			responsive		: true
		},
		/**
		 * if left as `true` all parents of all selected nodes will be opened once the tree loads (so that all selected nodes are visible to the user)
		 * @name $.jstree.defaults.core.expand_selected_onload
		 */
		expand_selected_onload : true
	};
	$.jstree.core.prototype = {
		/**
		 * used to decorate an instance with a plugin. Used internally.
		 * @private
		 * @name plugin(deco [, opts])
		 * @param  {String} deco the plugin to decorate with
		 * @param  {Object} opts options for the plugin
		 * @return {jsTree}
		 */
		plugin : function (deco, opts) {
			var Child = $.jstree.plugins[deco];
			if(Child) {
				this._data[deco] = {};
				Child.prototype = this;
				return new Child(opts, this);
			}
			return this;
		},
		/**
		 * used to decorate an instance with a plugin. Used internally.
		 * @private
		 * @name init(el, optons)
		 * @param {DOMElement|jQuery|String} el the element we are transforming
		 * @param {Object} options options for this instance
		 * @trigger init.jstree, loading.jstree, loaded.jstree, ready.jstree, changed.jstree
		 */
		init : function (el, options) {
			this._model = {
				data : {
					'#' : {
						id : '#',
						parent : null,
						parents : [],
						children : [],
						children_d : [],
						state : { loaded : false }
					}
				},
				changed : [],
				force_full_redraw : false,
				redraw_timeout : false,
				default_state : {
					loaded : true,
					opened : false,
					selected : false,
					disabled : false
				}
			};

			this.element = $(el).addClass('jstree jstree-' + this._id);
			this.settings = options;
			this.element.bind("destroyed", $.proxy(this.teardown, this));

			this._data.core.ready = false;
			this._data.core.loaded = false;
			this._data.core.rtl = (this.element.css("direction") === "rtl");
			this.element[this._data.core.rtl ? 'addClass' : 'removeClass']("jstree-rtl");
			this.element.attr('role','tree');

			this.bind();
			/**
			 * triggered after all events are bound
			 * @event
			 * @name init.jstree
			 */
			this.trigger("init");

			this._data.core.original_container_html = this.element.find(" > ul > li").clone(true);
			this._data.core.original_container_html
				.find("li").addBack()
				.contents().filter(function() {
					return this.nodeType === 3 && (!this.nodeValue || /^\s+$/.test(this.nodeValue));
				})
				.remove();
			this.element.html("<"+"ul class='jstree-container-ul'><"+"li class='jstree-initial-node jstree-loading jstree-leaf jstree-last'><i class='jstree-icon jstree-ocl'></i><"+"a class='jstree-anchor' href='#'><i class='jstree-icon jstree-themeicon-hidden'></i>" + this.get_string("Loading ...") + "</a></li></ul>");
			this._data.core.li_height = this.get_container_ul().children("li:eq(0)").height() || 18;
			/**
			 * triggered after the loading text is shown and before loading starts
			 * @event
			 * @name loading.jstree
			 */
			this.trigger("loading");
			this.load_node('#');
		},
		/**
		 * destroy an instance
		 * @name destroy()
		 */
		destroy : function () {
			this.element.unbind("destroyed", this.teardown);
			this.teardown();
		},
		/**
		 * part of the destroying of an instance. Used internally.
		 * @private
		 * @name teardown()
		 */
		teardown : function () {
			this.unbind();
			this.element
				.removeClass('jstree')
				.removeData('jstree')
				.find("[class^='jstree']")
					.addBack()
					.attr("class", function () { return this.className.replace(/jstree[^ ]*|$/ig,''); });
			this.element = null;
		},
		/**
		 * bind all events. Used internally.
		 * @private
		 * @name bind()
		 */
		bind : function () {
			this.element
				.on("dblclick.jstree", function () {
						if(document.selection && document.selection.empty) {
							document.selection.empty();
						}
						else {
							if(window.getSelection) {
								var sel = window.getSelection();
								try {
									sel.removeAllRanges();
									sel.collapse();
								} catch (ignore) { }
							}
						}
					})
				.on("click.jstree", ".jstree-ocl", $.proxy(function (e) {
						this.toggle_node(e.target);
					}, this))
				.on("click.jstree", ".jstree-anchor", $.proxy(function (e) {
						e.preventDefault();
						$(e.currentTarget).focus();
						this.activate_node(e.currentTarget, e);
					}, this))
				.on('keydown.jstree', '.jstree-anchor', $.proxy(function (e) {
						var o = null;
						switch(e.which) {
							case 13:
							case 32:
								e.type = "click";
								$(e.currentTarget).trigger(e);
								break;
							case 37:
								e.preventDefault();
								if(this.is_open(e.currentTarget)) {
									this.close_node(e.currentTarget);
								}
								else {
									o = this.get_prev_dom(e.currentTarget);
									if(o && o.length) { o.children('.jstree-anchor').focus(); }
								}
								break;
							case 38:
								e.preventDefault();
								o = this.get_prev_dom(e.currentTarget);
								if(o && o.length) { o.children('.jstree-anchor').focus(); }
								break;
							case 39:
								e.preventDefault();
								if(this.is_closed(e.currentTarget)) {
									this.open_node(e.currentTarget, function (o) { this.get_node(o, true).children('.jstree-anchor').focus(); });
								}
								else {
									o = this.get_next_dom(e.currentTarget);
									if(o && o.length) { o.children('.jstree-anchor').focus(); }
								}
								break;
							case 40:
								e.preventDefault();
								o = this.get_next_dom(e.currentTarget);
								if(o && o.length) { o.children('.jstree-anchor').focus(); }
								break;
							// delete
							case 46:
								e.preventDefault();
								o = this.get_node(e.currentTarget);
								if(o && o.id && o.id !== '#') {
									o = this.is_selected(o) ? this.get_selected() : o;
									// this.delete_node(o);
								}
								break;
							// f2
							case 113:
								e.preventDefault();
								o = this.get_node(e.currentTarget);
								/*!
								if(o && o.id && o.id !== '#') {
									// this.edit(o);
								}
								*/
								break;
							default:
								// console.log(e.which);
								break;
						}
					}, this))
				.on("load_node.jstree", $.proxy(function (e, data) {
						if(data.status) {
							if(data.node.id === '#' && !this._data.core.loaded) {
								this._data.core.loaded = true;
								/**
								 * triggered after the root node is loaded for the first time
								 * @event
								 * @name loaded.jstree
								 */
								this.trigger("loaded");
							}
							if(!this._data.core.ready && !this.get_container_ul().find('.jstree-loading:eq(0)').length) {
								this._data.core.ready = true;
								if(this._data.core.selected.length) {
									if(this.settings.core.expand_selected_onload) {
										var tmp = [], i, j;
										for(i = 0, j = this._data.core.selected.length; i < j; i++) {
											tmp = tmp.concat(this._model.data[this._data.core.selected[i]].parents);
										}
										tmp = $.vakata.array_unique(tmp);
										for(i = 0, j = tmp.length; i < j; i++) {
											this.open_node(tmp[i], false, 0);
										}
									}
									this.trigger('changed', { 'action' : 'ready', 'selected' : this._data.core.selected });
								}
								/**
								 * triggered after all nodes are finished loading
								 * @event
								 * @name ready.jstree
								 */
								setTimeout($.proxy(function () { this.trigger("ready"); }, this), 0);
							}
						}
					}, this))
				// THEME RELATED
				.on("init.jstree", $.proxy(function () {
						var s = this.settings.core.themes;
						this._data.core.themes.dots			= s.dots;
						this._data.core.themes.stripes		= s.stripes;
						this._data.core.themes.icons		= s.icons;
						this.set_theme(s.name || "default", s.url);
						this.set_theme_variant(s.variant);
					}, this))
				.on("loading.jstree", $.proxy(function () {
						this[ this._data.core.themes.dots ? "show_dots" : "hide_dots" ]();
						this[ this._data.core.themes.icons ? "show_icons" : "hide_icons" ]();
						this[ this._data.core.themes.stripes ? "show_stripes" : "hide_stripes" ]();
					}, this))
				.on('focus.jstree', '.jstree-anchor', $.proxy(function (e) {
						this.element.find('.jstree-hovered').not(e.currentTarget).mouseleave();
						$(e.currentTarget).mouseenter();
					}, this))
				.on('mouseenter.jstree', '.jstree-anchor', $.proxy(function (e) {
						this.hover_node(e.currentTarget);
					}, this))
				.on('mouseleave.jstree', '.jstree-anchor', $.proxy(function (e) {
						this.dehover_node(e.currentTarget);
					}, this));
		},
		/**
		 * part of the destroying of an instance. Used internally.
		 * @private
		 * @name unbind()
		 */
		unbind : function () {
			this.element.off('.jstree');
			$(document).off('.jstree-' + this._id);
		},
		/**
		 * trigger an event. Used internally.
		 * @private
		 * @name trigger(ev [, data])
		 * @param  {String} ev the name of the event to trigger
		 * @param  {Object} data additional data to pass with the event
		 */
		trigger : function (ev, data) {
			if(!data) {
				data = {};
			}
			data.instance = this;
			this.element.triggerHandler(ev.replace('.jstree','') + '.jstree', data);
		},
		/**
		 * returns the jQuery extended instance container
		 * @name get_container()
		 * @return {jQuery}
		 */
		get_container : function () {
			return this.element;
		},
		/**
		 * returns the jQuery extended main UL node inside the instance container. Used internally.
		 * @private
		 * @name get_container_ul()
		 * @return {jQuery}
		 */
		get_container_ul : function () {
			return this.element.children("ul:eq(0)");
		},
		/**
		 * gets string replacements (localization). Used internally.
		 * @private
		 * @name get_string(key)
		 * @param  {String} key
		 * @return {String}
		 */
		get_string : function (key) {
			var a = this.settings.core.strings;
			if($.isFunction(a)) { return a.call(this, key); }
			if(a && a[key]) { return a[key]; }
			return key;
		},
		/**
		 * gets the first child of a DOM node. Used internally.
		 * @private
		 * @name _firstChild(dom)
		 * @param  {DOMElement} dom
		 * @return {DOMElement}
		 */
		_firstChild : function (dom) {
			dom = dom ? dom.firstChild : null;
			while(dom !== null && dom.nodeType !== 1) {
				dom = dom.nextSibling;
			}
			return dom;
		},
		/**
		 * gets the next sibling of a DOM node. Used internally.
		 * @private
		 * @name _nextSibling(dom)
		 * @param  {DOMElement} dom
		 * @return {DOMElement}
		 */
		_nextSibling : function (dom) {
			dom = dom ? dom.nextSibling : null;
			while(dom !== null && dom.nodeType !== 1) {
				dom = dom.nextSibling;
			}
			return dom;
		},
		/**
		 * gets the previous sibling of a DOM node. Used internally.
		 * @private
		 * @name _previousSibling(dom)
		 * @param  {DOMElement} dom
		 * @return {DOMElement}
		 */
		_previousSibling : function (dom) {
			dom = dom ? dom.previousSibling : null;
			while(dom !== null && dom.nodeType !== 1) {
				dom = dom.previousSibling;
			}
			return dom;
		},
		/**
		 * get the JSON representation of a node (or the actual jQuery extended DOM node) by using any input (child DOM element, ID string, selector, etc)
		 * @name get_node(obj [, as_dom])
		 * @param  {mixed} obj
		 * @param  {Boolean} as_dom
		 * @return {Object|jQuery}
		 */
		get_node : function (obj, as_dom) {
			if(obj && obj.id) {
				obj = obj.id;
			}
			var dom;
			try {
				if(this._model.data[obj]) {
					obj = this._model.data[obj];
				}
				else if(((dom = $(obj, this.element)).length || (dom = $('#' + obj, this.element)).length) && this._model.data[dom.closest('li').attr('id')]) {
					obj = this._model.data[dom.closest('li').attr('id')];
				}
				else if((dom = $(obj, this.element)).length && dom.hasClass('jstree')) {
					obj = this._model.data['#'];
				}
				else {
					return false;
				}

				if(as_dom) {
					obj = obj.id === '#' ? this.element : $(document.getElementById(obj.id));
				}
				return obj;
			} catch (ex) { return false; }
		},
		/**
		 * get the path to a node, either consisting of node texts, or of node IDs, optionally glued together (otherwise an array)
		 * @name get_path(obj [, glue, ids])
		 * @param  {mixed} obj the node
		 * @param  {String} glue if you want the path as a string - pass the glue here (for example '/'), if a falsy value is supplied here, an array is returned
		 * @param  {Boolean} ids if set to true build the path using ID, otherwise node text is used
		 * @return {mixed}
		 */
		get_path : function (obj, glue, ids) {
			obj = obj.parents ? obj : this.get_node(obj);
			if(!obj || obj.id === '#' || !obj.parents) {
				return false;
			}
			var i, j, p = [];
			p.push(ids ? obj.id : obj.text);
			for(i = 0, j = obj.parents.length; i < j; i++) {
				p.push(ids ? obj.parents[i] : this.get_text(obj.parents[i]));
			}
			p = p.reverse().slice(1);
			return glue ? p.join(glue) : p;
		},
		/**
		 * get the next visible node that is below the `obj` node. If `strict` is set to `true` only sibling nodes are returned.
		 * @name get_next_dom(obj [, strict])
		 * @param  {mixed} obj
		 * @param  {Boolean} strict
		 * @return {jQuery}
		 */
		get_next_dom : function (obj, strict) {
			var tmp;
			obj = this.get_node(obj, true);
			if(obj[0] === this.element[0]) {
				tmp = this._firstChild(this.get_container_ul()[0]);
				return tmp ? $(tmp) : false;
			}
			if(!obj || !obj.length) {
				return false;
			}
			if(strict) {
				tmp = this._nextSibling(obj[0]);
				return tmp ? $(tmp) : false;
			}
			if(obj.hasClass("jstree-open")) {
				tmp = this._firstChild(obj.children('ul')[0]);
				return tmp ? $(tmp) : false;
			}
			if((tmp = this._nextSibling(obj[0])) !== null) {
				return $(tmp);
			}
			return obj.parentsUntil(".jstree","li").next("li").eq(0);
		},
		/**
		 * get the previous visible node that is above the `obj` node. If `strict` is set to `true` only sibling nodes are returned.
		 * @name get_prev_dom(obj [, strict])
		 * @param  {mixed} obj
		 * @param  {Boolean} strict
		 * @return {jQuery}
		 */
		get_prev_dom : function (obj, strict) {
			var tmp;
			obj = this.get_node(obj, true);
			if(obj[0] === this.element[0]) {
				tmp = this.get_container_ul()[0].lastChild;
				return tmp ? $(tmp) : false;
			}
			if(!obj || !obj.length) {
				return false;
			}
			if(strict) {
				tmp = this._previousSibling(obj[0]);
				return tmp ? $(tmp) : false;
			}
			if((tmp = this._previousSibling(obj[0])) !== null) {
				obj = $(tmp);
				while(obj.hasClass("jstree-open")) {
					obj = obj.children("ul:eq(0)").children("li:last");
				}
				return obj;
			}
			tmp = obj[0].parentNode.parentNode;
			return tmp && tmp.tagName === 'LI' ? $(tmp) : false;
		},
		/**
		 * get the parent ID of a node
		 * @name get_parent(obj)
		 * @param  {mixed} obj
		 * @return {String}
		 */
		get_parent : function (obj) {
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			return obj.parent;
		},
		/**
		 * get a jQuery collection of all the children of a node (node must be rendered)
		 * @name get_children_dom(obj)
		 * @param  {mixed} obj
		 * @return {jQuery}
		 */
		get_children_dom : function (obj) {
			obj = this.get_node(obj, true);
			if(obj[0] === this.element[0]) {
				return this.get_container_ul().children("li");
			}
			if(!obj || !obj.length) {
				return false;
			}
			return obj.children("ul").children("li");
		},
		/**
		 * checks if a node has children
		 * @name is_parent(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_parent : function (obj) {
			obj = this.get_node(obj);
			return obj && (obj.state.loaded === false || obj.children.length > 0);
		},
		/**
		 * checks if a node is loaded (its children are available)
		 * @name is_loaded(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_loaded : function (obj) {
			obj = this.get_node(obj);
			return obj && obj.state.loaded;
		},
		/**
		 * check if a node is currently loading (fetching children)
		 * @name is_loading(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_loading : function (obj) {
			obj = this.get_node(obj, true);
			return obj && obj.hasClass("jstree-loading");
		},
		/**
		 * check if a node is opened
		 * @name is_open(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_open : function (obj) {
			obj = this.get_node(obj);
			return obj && obj.state.opened;
		},
		/**
		 * check if a node is in a closed state
		 * @name is_closed(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_closed : function (obj) {
			obj = this.get_node(obj);
			return obj && this.is_parent(obj) && !obj.state.opened;
		},
		/**
		 * check if a node has no children
		 * @name is_leaf(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_leaf : function (obj) {
			return !this.is_parent(obj);
		},
		/**
		 * loads a node (fetches its children using the `core.data` setting). Multiple nodes can be passed to by using an array.
		 * @name load_node(obj [, callback])
		 * @param  {mixed} obj
		 * @param  {function} callback a function to be executed once loading is conplete, the function is executed in the instance's scope and receives two arguments - the node and a boolean status
		 * @return {Boolean}
		 * @trigger load_node.jstree
		 */
		load_node : function (obj, callback) {
			var t1, t2;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.load_node(obj[t1], callback);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj) {
				callback.call(this, obj, false);
				return false;
			}
			this.get_node(obj, true).addClass("jstree-loading");
			this._load_node(obj, $.proxy(function (status) {
				obj.state.loaded = status;
				this.get_node(obj, true).removeClass("jstree-loading");
				/**
				 * triggered after a node is loaded
				 * @event
				 * @name load_node.jstree
				 * @param {Object} node the node that was loading
				 * @param {Boolean} status was the node loaded successfully
				 */
				this.trigger('load_node', { "node" : obj, "status" : status });
				if(callback) {
					callback.call(this, obj, status);
				}
			}, this));
			return true;
		},
		/**
		 * handles the actual loading of a node. Used only internally.
		 * @private
		 * @name _load_node(obj [, callback])
		 * @param  {mixed} obj
		 * @param  {function} callback a function to be executed once loading is conplete, the function is executed in the instance's scope and receives one argument - a boolean status
		 * @return {Boolean}
		 */
		_load_node : function (obj, callback) {
			var s = this.settings.core.data, t;
			// use original HTML
			if(!s) {
				return callback.call(this, obj.id === '#' ? this._append_html_data(obj, this._data.core.original_container_html.clone(true)) : false);
			}
			if($.isFunction(s)) {
				return s.call(this, obj, $.proxy(function (d) {
					return d === false ? callback.call(this, false) : callback.call(this, this[typeof d === 'string' ? '_append_html_data' : '_append_json_data'](obj, typeof d === 'string' ? $(d) : d));
				}, this));
			}
			if(typeof s === 'object') {
				if(s.url) {
					s = $.extend(true, {}, s);
					if($.isFunction(s.url)) {
						s.url = s.url.call(this, obj);
					}
					if($.isFunction(s.data)) {
						s.data = s.data.call(this, obj);
					}
					return $.ajax(s)
						.done($.proxy(function (d,t,x) {
								var type = x.getResponseHeader('Content-Type');
								if(type.indexOf('json') !== -1) {
									return callback.call(this, this._append_json_data(obj, d));
								}
								if(type.indexOf('html') !== -1) {
									return callback.call(this, this._append_html_data(obj, $(d)));
								}
							}, this))
						.fail($.proxy(function () {
								callback.call(this, false);
								this._data.core.last_error = { 'error' : 'ajax', 'plugin' : 'core', 'id' : 'core_04', 'reason' : 'Could not load node', 'data' : JSON.stringify(s) };
								this.settings.core.error.call(this, this._data.core.last_error);
							}, this));
				}
				t = ($.isArray(s) || $.isPlainObject(s)) ? JSON.parse(JSON.stringify(s)) : s;
				return callback.call(this, this._append_json_data(obj, t));
			}
			if(typeof s === 'string') {
				return callback.call(this, this._append_html_data(obj, s));
			}
			return callback.call(this, false);
		},
		/**
		 * adds a node to the list of nodes to redraw. Used only internally.
		 * @private
		 * @name _node_changed(obj [, callback])
		 * @param  {mixed} obj
		 */
		_node_changed : function (obj) {
			obj = this.get_node(obj);
			if(obj) {
				this._model.changed.push(obj.id);
			}
		},
		/**
		 * appends HTML content to the tree. Used internally.
		 * @private
		 * @name _append_html_data(obj, data)
		 * @param  {mixed} obj the node to append to
		 * @param  {String} data the HTML string to parse and append
		 * @return {Boolean}
		 * @trigger model.jstree, changed.jstree
		 */
		_append_html_data : function (dom, data) {
			dom = this.get_node(dom);
			dom.children = [];
			dom.children_d = [];
			var dat = data.is('ul') ? data.children() : data,
				par = dom.id,
				chd = [],
				dpc = [],
				m = this._model.data,
				p = m[par],
				s = this._data.core.selected.length,
				tmp, i, j;
			dat.each($.proxy(function (i, v) {
				tmp = this._parse_model_from_html($(v), par, p.parents.concat());
				if(tmp) {
					chd.push(tmp);
					dpc.push(tmp);
					if(m[tmp].children_d.length) {
						dpc = dpc.concat(m[tmp].children_d);
					}
				}
			}, this));
			p.children = chd;
			p.children_d = dpc;
			for(i = 0, j = p.parents.length; i < j; i++) {
				m[p.parents[i]].children_d = m[p.parents[i]].children_d.concat(dpc);
			}
			/**
			 * triggered when new data is inserted to the tree model
			 * @event
			 * @name model.jstree
			 * @param {Array} nodes an array of node IDs
			 * @param {String} parent the parent ID of the nodes
			 */
			this.trigger('model', { "nodes" : dpc, 'parent' : par });
			if(par !== '#') {
				this._node_changed(par);
				this.redraw();
			}
			else {
				this.get_container_ul().children('.jstree-initial-node').remove();
				this.redraw(true);
			}
			if(this._data.core.selected.length !== s) {
				this.trigger('changed', { 'action' : 'model', 'selected' : this._data.core.selected });
			}
			return true;
		},
		/**
		 * appends JSON content to the tree. Used internally.
		 * @private
		 * @name _append_json_data(obj, data)
		 * @param  {mixed} obj the node to append to
		 * @param  {String} data the JSON object to parse and append
		 * @return {Boolean}
		 */
		_append_json_data : function (dom, data) {
			dom = this.get_node(dom);
			dom.children = [];
			dom.children_d = [];
			var dat = data,
				par = dom.id,
				chd = [],
				dpc = [],
				m = this._model.data,
				p = m[par],
				s = this._data.core.selected.length,
				tmp, i, j;
			// *%$@!!!
			if(dat.d) {
				dat = dat.d;
				if(typeof dat === "string") {
					dat = JSON.parse(dat);
				}
			}
			if(!$.isArray(dat)) { dat = [dat]; }
			if(dat.length && dat[0].id !== undefined && dat[0].parent !== undefined) {
				// Flat JSON support (for easy import from DB):
				// 1) convert to object (foreach)
				for(i = 0, j = dat.length; i < j; i++) {
					if(!dat[i].children) {
						dat[i].children = [];
					}
					m[dat[i].id] = dat[i];
				}
				// 2) populate children (foreach)
				for(i = 0, j = dat.length; i < j; i++) {
					m[dat[i].parent].children.push(dat[i].id);
					// populate parent.children_d
					p.children_d.push(dat[i].id);
				}
				// 3) normalize && populate parents and children_d with recursion
				for(i = 0, j = p.children.length; i < j; i++) {
					tmp = this._parse_model_from_flat_json(m[p.children[i]], par, p.parents.concat());
					dpc.push(tmp);
					if(m[tmp].children_d.length) {
						dpc = dpc.concat(m[tmp].children_d);
					}
				}
				// ?) three_state selection - p.state.selected && t - (if three_state foreach(dat => ch) -> foreach(parents) if(parent.selected) child.selected = true;
			}
			else {
				for(i = 0, j = dat.length; i < j; i++) {
					tmp = this._parse_model_from_json(dat[i], par, p.parents.concat());
					if(tmp) {
						chd.push(tmp);
						dpc.push(tmp);
						if(m[tmp].children_d.length) {
							dpc = dpc.concat(m[tmp].children_d);
						}
					}
				}
				p.children = chd;
				p.children_d = dpc;
				for(i = 0, j = p.parents.length; i < j; i++) {
					m[p.parents[i]].children_d = m[p.parents[i]].children_d.concat(dpc);
				}
			}
			this.trigger('model', { "nodes" : dpc, 'parent' : par });

			if(par !== '#') {
				this._node_changed(par);
				this.redraw();
			}
			else {
				// this.get_container_ul().children('.jstree-initial-node').remove();
				this.redraw(true);
			}
			if(this._data.core.selected.length !== s) {
				this.trigger('changed', { 'action' : 'model', 'selected' : this._data.core.selected });
			}
			return true;
		},
		/**
		 * parses a node from a jQuery object and appends them to the in memory tree model. Used internally.
		 * @private
		 * @name _parse_model_from_html(d [, p, ps])
		 * @param  {jQuery} d the jQuery object to parse
		 * @param  {String} p the parent ID
		 * @param  {Array} ps list of all parents
		 * @return {String} the ID of the object added to the model
		 */
		_parse_model_from_html : function (d, p, ps) {
			if(!ps) { ps = []; }
			else { ps = [].concat(ps); }
			if(p) { ps.unshift(p); }
			var c, e, m = this._model.data,
				data = {
					id			: false,
					text		: false,
					icon		: true,
					parent		: p,
					parents		: ps,
					children	: [],
					children_d	: [],
					data		: null,
					state		: { },
					li_attr		: { id : false },
					a_attr		: { href : '#' },
					original	: false
				}, i, tmp, tid;
			for(i in this._model.default_state) {
				if(this._model.default_state.hasOwnProperty(i)) {
					data.state[i] = this._model.default_state[i];
				}
			}
			tmp = $.vakata.attributes(d, true);
			$.each(tmp, function (i, v) {
				v = $.trim(v);
				if(!v.length) { return true; }
				data.li_attr[i] = v;
				if(i === 'id') {
					data.id = v;
				}
			});
			tmp = d.children('a').eq(0);
			if(tmp.length) {
				tmp = $.vakata.attributes(tmp, true);
				$.each(tmp, function (i, v) {
					v = $.trim(v);
					if(v.length) {
						data.a_attr[i] = v;
					}
				});
			}
			tmp = d.children("a:eq(0)").length ? d.children("a:eq(0)").clone() : d.clone();
			tmp.children("ins, i, ul").remove();
			tmp = tmp.html();
			tmp = $('<div />').html(tmp);
			data.text = tmp.html();
			tmp = d.data();
			data.data = tmp ? $.extend(true, {}, tmp) : null;
			data.state.opened = d.hasClass('jstree-open');
			data.state.selected = d.children('a').hasClass('jstree-clicked');
			data.state.disabled = d.children('a').hasClass('jstree-disabled');
			if(data.data && data.data.jstree) {
				for(i in data.data.jstree) {
					if(data.data.jstree.hasOwnProperty(i)) {
						data.state[i] = data.data.jstree[i];
					}
				}
			}
			tmp = d.children("a").children(".jstree-themeicon");
			if(tmp.length) {
				data.icon = tmp.hasClass('jstree-themeicon-hidden') ? false : tmp.attr('rel');
			}
			if(data.state.icon) {
				data.icon = data.state.icon;
			}
			tmp = d.children("ul").children("li");
			do {
				tid = 'j' + this._id + '_' + (++this._cnt);
			} while(m[tid]);
			data.id = data.li_attr.id || tid;
			if(tmp.length) {
				tmp.each($.proxy(function (i, v) {
					c = this._parse_model_from_html($(v), data.id, ps);
					e = this._model.data[c];
					data.children.push(c);
					if(e.children_d.length) {
						data.children_d = data.children_d.concat(e.children_d);
					}
				}, this));
				data.children_d = data.children_d.concat(data.children);
			}
			else {
				if(d.hasClass('jstree-closed')) {
					data.state.loaded = false;
				}
			}
			if(data.li_attr['class']) {
				data.li_attr['class'] = data.li_attr['class'].replace('jstree-closed','').replace('jstree-open','');
			}
			if(data.a_attr['class']) {
				data.a_attr['class'] = data.a_attr['class'].replace('jstree-clicked','').replace('jstree-disabled','');
			}
			m[data.id] = data;
			if(data.state.selected) {
				this._data.core.selected.push(data.id);
			}
			return data.id;
		},
		/**
		 * parses a node from a JSON object (used when dealing with flat data, which has no nesting of children, but has id and parent properties) and appends it to the in memory tree model. Used internally.
		 * @private
		 * @name _parse_model_from_flat_json(d [, p, ps])
		 * @param  {Object} d the JSON object to parse
		 * @param  {String} p the parent ID
		 * @param  {Array} ps list of all parents
		 * @return {String} the ID of the object added to the model
		 */
		_parse_model_from_flat_json : function (d, p, ps) {
			if(!ps) { ps = []; }
			else { ps = ps.concat(); }
			if(p) { ps.unshift(p); }
			var tid = d.id,
				m = this._model.data,
				df = this._model.default_state,
				i, j, c, e,
				tmp = {
					id			: tid,
					text		: d.text || '',
					icon		: d.icon !== undefined ? d.icon : true,
					parent		: p,
					parents		: ps,
					children	: d.children || [],
					children_d	: d.children_d || [],
					data		: d.data,
					state		: { },
					li_attr		: { id : false },
					a_attr		: { href : '#' },
					original	: false
				};
			for(i in df) {
				if(df.hasOwnProperty(i)) {
					tmp.state[i] = df[i];
				}
			}
			if(d && d.data && d.data.jstree && d.data.jstree.icon) {
				tmp.icon = d.data.jstree.icon;
			}
			if(d && d.data) {
				tmp.data = d.data;
				if(d.data.jstree) {
					for(i in d.data.jstree) {
						if(d.data.jstree.hasOwnProperty(i)) {
							tmp.state[i] = d.data.jstree[i];
						}
					}
				}
			}
			if(d && typeof d.state === 'object') {
				for (i in d.state) {
					if(d.state.hasOwnProperty(i)) {
						tmp.state[i] = d.state[i];
					}
				}
			}
			if(d && typeof d.li_attr === 'object') {
				for (i in d.li_attr) {
					if(d.li_attr.hasOwnProperty(i)) {
						tmp.li_attr[i] = d.li_attr[i];
					}
				}
			}
			if(!tmp.li_attr.id) {
				tmp.li_attr.id = tid;
			}
			if(d && typeof d.a_attr === 'object') {
				for (i in d.a_attr) {
					if(d.a_attr.hasOwnProperty(i)) {
						tmp.a_attr[i] = d.a_attr[i];
					}
				}
			}
			if(d && d.children && d.children === true) {
				tmp.state.loaded = false;
				tmp.children = [];
				tmp.children_d = [];
			}
			m[tmp.id] = tmp;
			for(i = 0, j = tmp.children.length; i < j; i++) {
				c = this._parse_model_from_flat_json(m[tmp.children[i]], tmp.id, ps);
				e = m[c];
				tmp.children_d.push(c);
				if(e.children_d.length) {
					tmp.children_d = tmp.children_d.concat(e.children_d);
				}
			}
			delete d.data;
			delete d.children;
			m[tmp.id].original = d;
			if(tmp.state.selected) {
				this._data.core.selected.push(tmp.id);
			}
			return tmp.id;
		},
		/**
		 * parses a node from a JSON object and appends it to the in memory tree model. Used internally.
		 * @private
		 * @name _parse_model_from_json(d [, p, ps])
		 * @param  {Object} d the JSON object to parse
		 * @param  {String} p the parent ID
		 * @param  {Array} ps list of all parents
		 * @return {String} the ID of the object added to the model
		 */
		_parse_model_from_json : function (d, p, ps) {
			if(!ps) { ps = []; }
			else { ps = ps.concat(); }
			if(p) { ps.unshift(p); }
			var tid = false, i, j, c, e, m = this._model.data, df = this._model.default_state, tmp;
			do {
				tid = 'j' + this._id + '_' + (++this._cnt);
			} while(m[tid]);

			tmp = {
				id			: false,
				text		: typeof d === 'string' ? d : '',
				icon		: typeof d === 'object' && d.icon !== undefined ? d.icon : true,
				parent		: p,
				parents		: ps,
				children	: [],
				children_d	: [],
				data		: null,
				state		: { },
				li_attr		: { id : false },
				a_attr		: { href : '#' },
				original	: false
			};
			for(i in df) {
				if(df.hasOwnProperty(i)) {
					tmp.state[i] = df[i];
				}
			}
			if(d && d.id) { tmp.id = d.id; }
			if(d && d.text) { tmp.text = d.text; }
			if(d && d.data && d.data.jstree && d.data.jstree.icon) {
				tmp.icon = d.data.jstree.icon;
			}
			if(d && d.data) {
				tmp.data = d.data;
				if(d.data.jstree) {
					for(i in d.data.jstree) {
						if(d.data.jstree.hasOwnProperty(i)) {
							tmp.state[i] = d.data.jstree[i];
						}
					}
				}
			}
			if(d && typeof d.state === 'object') {
				for (i in d.state) {
					if(d.state.hasOwnProperty(i)) {
						tmp.state[i] = d.state[i];
					}
				}
			}
			if(d && typeof d.li_attr === 'object') {
				for (i in d.li_attr) {
					if(d.li_attr.hasOwnProperty(i)) {
						tmp.li_attr[i] = d.li_attr[i];
					}
				}
			}
			if(tmp.li_attr.id && !tmp.id) {
				tmp.id = tmp.li_attr.id;
			}
			if(!tmp.id) {
				tmp.id = tid;
			}
			if(!tmp.li_attr.id) {
				tmp.li_attr.id = tmp.id;
			}
			if(d && typeof d.a_attr === 'object') {
				for (i in d.a_attr) {
					if(d.a_attr.hasOwnProperty(i)) {
						tmp.a_attr[i] = d.a_attr[i];
					}
				}
			}
			if(d && d.children && d.children.length) {
				for(i = 0, j = d.children.length; i < j; i++) {
					c = this._parse_model_from_json(d.children[i], tmp.id, ps);
					e = m[c];
					tmp.children.push(c);
					if(e.children_d.length) {
						tmp.children_d = tmp.children_d.concat(e.children_d);
					}
				}
				tmp.children_d = tmp.children_d.concat(tmp.children);
			}
			if(d && d.children && d.children === true) {
				tmp.state.loaded = false;
				tmp.children = [];
				tmp.children_d = [];
			}
			delete d.data;
			delete d.children;
			tmp.original = d;
			m[tmp.id] = tmp;
			if(tmp.state.selected) {
				this._data.core.selected.push(tmp.id);
			}
			return tmp.id;
		},
		/**
		 * redraws all nodes that need to be redrawn. Used internally.
		 * @private
		 * @name _redraw()
		 * @trigger redraw.jstree
		 */
		_redraw : function () {
			var nodes = this._model.force_full_redraw ? this._model.data['#'].children.concat([]) : this._model.changed.concat([]),
				f = document.createElement('UL'), tmp, i, j;
			for(i = 0, j = nodes.length; i < j; i++) {
				tmp = this.redraw_node(nodes[i], true, this._model.force_full_redraw);
				if(tmp && this._model.force_full_redraw) {
					f.appendChild(tmp);
				}
			}
			if(this._model.force_full_redraw) {
				f.className = this.get_container_ul()[0].className;
				this.element.empty().append(f);
				//this.get_container_ul()[0].appendChild(f);
			}
			this._model.force_full_redraw = false;
			this._model.changed = [];
			/**
			 * triggered after nodes are redrawn
			 * @event
			 * @name redraw.jstree
			 * @param {array} nodes the redrawn nodes
			 */
			this.trigger('redraw', { "nodes" : nodes });
		},
		/**
		 * redraws all nodes that need to be redrawn or optionally - the whole tree
		 * @name redraw([full])
		 * @param {Boolean} full if set to `true` all nodes are redrawn.
		 */
		redraw : function (full) {
			if(full) {
				this._model.force_full_redraw = true;
			}
			//if(this._model.redraw_timeout) {
			//	clearTimeout(this._model.redraw_timeout);
			//}
			//this._model.redraw_timeout = setTimeout($.proxy(this._redraw, this),0);
			this._redraw();
		},
		/**
		 * redraws a single node. Used internally.
		 * @private
		 * @name redraw_node(node, deep, is_callback)
		 * @param {mixed} node the node to redraw
		 * @param {Boolean} deep should child nodes be redrawn too
		 * @param {Boolean} is_callback is this a recursion call
		 */
		redraw_node : function (node, deep, is_callback) {
			var obj = this.get_node(node),
				par = false,
				ind = false,
				old = false,
				i = false,
				j = false,
				k = false,
				c = '',
				d = document,
				m = this._model.data,
				f = false,
				s = false;
			if(!obj) { return false; }
			if(obj.id === '#') {  return this.redraw(true); }
			deep = deep || obj.children.length === 0;
			node = d.getElementById(obj.id); //, this.element);
			if(!node) {
				deep = true;
				//node = d.createElement('LI');
				if(!is_callback) {
					par = obj.parent !== '#' ? $('#' + obj.parent, this.element)[0] : null;
					if(par !== null && (!par || !m[obj.parent].state.opened)) {
						return false;
					}
					ind = $.inArray(obj.id, par === null ? m['#'].children : m[obj.parent].children);
				}
			}
			else {
				node = $(node);
				if(!is_callback) {
					par = node.parent().parent()[0];
					if(par === this.element[0]) {
						par = null;
					}
					ind = node.index();
				}
				// m[obj.id].data = node.data(); // use only node's data, no need to touch jquery storage
				if(!deep && obj.children.length && !node.children('ul').length) {
					deep = true;
				}
				if(!deep) {
					old = node.children('UL')[0];
				}
				s = node.attr('aria-selected');
				f = node.children('.jstree-anchor')[0] === document.activeElement;
				node.remove();
				//node = d.createElement('LI');
				//node = node[0];
			}
			node = _node.cloneNode(true);
			// node is DOM, deep is boolean

			c = 'jstree-node ';
			for(i in obj.li_attr) {
				if(obj.li_attr.hasOwnProperty(i)) {
					if(i === 'id') { continue; }
					if(i !== 'class') {
						node.setAttribute(i, obj.li_attr[i]);
					}
					else {
						c += obj.li_attr[i];
					}
				}
			}
			if(s && s !== "false") {
				node.setAttribute('aria-selected', true);
			}
			if(!obj.children.length && obj.state.loaded) {
				c += ' jstree-leaf';
			}
			else {
				c += obj.state.opened ? ' jstree-open' : ' jstree-closed';
				node.setAttribute('aria-expanded', obj.state.opened);
			}
			if(obj.parent !== null && m[obj.parent].children[m[obj.parent].children.length - 1] === obj.id) {
				c += ' jstree-last';
			}
			node.id = obj.id;
			node.className = c;
			c = ( obj.state.selected ? ' jstree-clicked' : '') + ( obj.state.disabled ? ' jstree-disabled' : '');
			for(j in obj.a_attr) {
				if(obj.a_attr.hasOwnProperty(j)) {
					if(j === 'href' && obj.a_attr[j] === '#') { continue; }
					if(j !== 'class') {
						node.childNodes[1].setAttribute(j, obj.a_attr[j]);
					}
					else {
						c += ' ' + obj.a_attr[j];
					}
				}
			}
			if(c.length) {
				node.childNodes[1].className = 'jstree-anchor ' + c;
			}
			if((obj.icon && obj.icon !== true) || obj.icon === false) {
				if(obj.icon === false) {
					node.childNodes[1].childNodes[0].className += ' jstree-themeicon-hidden';
				}
				else if(obj.icon.indexOf('/') === -1 && obj.icon.indexOf('.') === -1) {
					node.childNodes[1].childNodes[0].className += ' ' + obj.icon + ' jstree-themeicon-custom';
				}
				else {
					node.childNodes[1].childNodes[0].style.backgroundImage = 'url('+obj.icon+')';
					node.childNodes[1].childNodes[0].style.backgroundPosition = 'center center';
					node.childNodes[1].childNodes[0].style.backgroundSize = 'auto';
					node.childNodes[1].childNodes[0].className += ' jstree-themeicon-custom';
				}
			}
			//node.childNodes[1].appendChild(d.createTextNode(obj.text));
			node.childNodes[1].innerHTML += obj.text;
			// if(obj.data) { $.data(node, obj.data); } // always work with node's data, no need to touch jquery store

			if(deep && obj.children.length && obj.state.opened) {
				k = d.createElement('UL');
				k.setAttribute('role', 'group');
				k.className = 'jstree-children';
				for(i = 0, j = obj.children.length; i < j; i++) {
					k.appendChild(this.redraw_node(obj.children[i], deep, true));
				}
				node.appendChild(k);
			}
			if(old) {
				node.appendChild(old);
			}
			if(!is_callback) {
				// append back using par / ind
				if(!par) {
					par = this.element[0];
				}
				if(!par.getElementsByTagName('UL').length) {
					i = d.createElement('UL');
					i.setAttribute('role', 'group');
					i.className = 'jstree-children';
					par.appendChild(i);
					par = i;
				}
				else {
					par = par.getElementsByTagName('UL')[0];
				}

				if(ind < par.childNodes.length) {
					par.insertBefore(node, par.childNodes[ind]);
				}
				else {
					par.appendChild(node);
				}
				if(f) {
					node.childNodes[1].focus();
				}
			}
			return node;
		},
		/**
		 * opens a node, revaling its children. If the node is not loaded it will be loaded and opened once ready.
		 * @name open_node(obj [, callback, animation])
		 * @param {mixed} obj the node to open
		 * @param {Function} callback a function to execute once the node is opened
		 * @param {Number} animation the animation duration in milliseconds when opening the node (overrides the `core.animation` setting). Use `false` for no animation.
		 * @trigger open_node.jstree, after_open.jstree
		 */
		open_node : function (obj, callback, animation) {
			var t1, t2, d, t;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.open_node(obj[t1], callback, animation);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			animation = animation === undefined ? this.settings.core.animation : animation;
			if(!this.is_closed(obj)) {
				if(callback) {
					callback.call(this, obj, false);
				}
				return false;
			}
			if(!this.is_loaded(obj)) {
				if(this.is_loading(obj)) {
					return setTimeout($.proxy(function () {
						this.open_node(obj, callback, animation);
					}, this), 500);
				}
				this.load_node(obj, function (o, ok) {
					return ok ? this.open_node(o, callback, animation) : (callback ? callback.call(this, o, false) : false);
				});
			}
			else {
				d = this.get_node(obj, true);
				t = this;
				if(d.length) {
					if(obj.children.length && !this._firstChild(d.children('ul')[0])) {
						obj.state.opened = true;
						this.redraw_node(obj, true);
						d = this.get_node(obj, true);
					}
					if(!animation) {
						d[0].className = d[0].className.replace('jstree-closed', 'jstree-open');
						d[0].setAttribute("aria-expanded", true);
					}
					else {
						d
							.children("ul").css("display","none").end()
							.removeClass("jstree-closed").addClass("jstree-open").attr("aria-expanded", true)
							.children("ul").stop(true, true)
								.slideDown(animation, function () {
									this.style.display = "";
									t.trigger("after_open", { "node" : obj });
								});
					}
				}
				obj.state.opened = true;
				if(callback) {
					callback.call(this, obj, true);
				}
				/**
				 * triggered when a node is opened (if there is an animation it will not be completed yet)
				 * @event
				 * @name open_node.jstree
				 * @param {Object} node the opened node
				 */
				this.trigger('open_node', { "node" : obj });
				if(!animation || !d.length) {
					/**
					 * triggered when a node is opened and the animation is complete
					 * @event
					 * @name after_open.jstree
					 * @param {Object} node the opened node
					 */
					this.trigger("after_open", { "node" : obj });
				}
			}
		},
		/**
		 * opens every parent of a node (node should be loaded)
		 * @name _open_to(obj)
		 * @param {mixed} obj the node to reveal
		 * @private
		 */
		_open_to : function (obj) {
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			var i, j, p = obj.parents;
			for(i = 0, j = p.length; i < j; i+=1) {
				if(i !== '#') {
					this.open_node(p[i], false, 0);
				}
			}
			return $(document.getElementById(obj.id));
		},
		/**
		 * closes a node, hiding its children
		 * @name close_node(obj [, animation])
		 * @param {mixed} obj the node to close
		 * @param {Number} animation the animation duration in milliseconds when closing the node (overrides the `core.animation` setting). Use `false` for no animation.
		 * @trigger close_node.jstree, after_close.jstree
		 */
		close_node : function (obj, animation) {
			var t1, t2, t, d;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.close_node(obj[t1], animation);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			animation = animation === undefined ? this.settings.core.animation : animation;
			t = this;
			d = this.get_node(obj, true);
			if(d.length) {
				if(!animation) {
					d[0].className = d[0].className.replace('jstree-open', 'jstree-closed');
					d.attr("aria-expanded", false).children('ul').remove();
				}
				else {
					d
						.children("ul").attr("style","display:block !important").end()
						.removeClass("jstree-open").addClass("jstree-closed").attr("aria-expanded", false)
						.children("ul").stop(true, true).slideUp(animation, function () {
							this.style.display = "";
							d.children('ul').remove();
							t.trigger("after_close", { "node" : obj });
						});
				}
			}
			obj.state.opened = false;
			/**
			 * triggered when a node is closed (if there is an animation it will not be complete yet)
			 * @event
			 * @name close_node.jstree
			 * @param {Object} node the closed node
			 */
			this.trigger('close_node',{ "node" : obj });
			if(!animation || !d.length) {
				/**
				 * triggered when a node is closed and the animation is complete
				 * @event
				 * @name after_close.jstree
				 * @param {Object} node the closed node
				 */
				this.trigger("after_close", { "node" : obj });
			}
		},
		/**
		 * toggles a node - closing it if it is open, opening it if it is closed
		 * @name toggle_node(obj)
		 * @param {mixed} obj the node to toggle
		 */
		toggle_node : function (obj) {
			var t1, t2;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.toggle_node(obj[t1]);
				}
				return true;
			}
			if(this.is_closed(obj)) {
				return this.open_node(obj);
			}
			if(this.is_open(obj)) {
				return this.close_node(obj);
			}
		},
		/**
		 * opens all nodes within a node (or the tree), revaling their children. If the node is not loaded it will be loaded and opened once ready.
		 * @name open_all([obj, animation, original_obj])
		 * @param {mixed} obj the node to open recursively, omit to open all nodes in the tree
		 * @param {Number} animation the animation duration in milliseconds when opening the nodes, the default is no animation
		 * @param {jQuery} reference to the node that started the process (internal use)
		 * @trigger open_all.jstree
		 */
		open_all : function (obj, animation, original_obj) {
			if(!obj) { obj = '#'; }
			obj = this.get_node(obj);
			if(!obj) { return false; }
			var dom = obj.id === '#' ? this.get_container_ul() : this.get_node(obj, true), i, j, _this;
			if(!dom.length) {
				for(i = 0, j = obj.children_d.length; i < j; i++) {
					if(this.is_closed(this._model.data[obj.children_d[i]])) {
						this._model.data[obj.children_d[i]].state.opened = true;
					}
				}
				return this.trigger('open_all', { "node" : obj });
			}
			original_obj = original_obj || dom;
			_this = this;
			dom = this.is_closed(obj) ? dom.find('li.jstree-closed').addBack() : dom.find('li.jstree-closed');
			dom.each(function () {
				_this.open_node(
					this,
					function(node, status) { if(status && this.is_parent(node)) { this.open_all(node, animation, original_obj); } },
					animation || 0
				);
			});
			if(original_obj.find('li.jstree-closed').length === 0) {
				/**
				 * triggered when an `open_all` call completes
				 * @event
				 * @name open_all.jstree
				 * @param {Object} node the opened node
				 */
				this.trigger('open_all', { "node" : this.get_node(original_obj) });
			}
		},
		/**
		 * closes all nodes within a node (or the tree), revaling their children
		 * @name close_all([obj, animation])
		 * @param {mixed} obj the node to close recursively, omit to close all nodes in the tree
		 * @param {Number} animation the animation duration in milliseconds when closing the nodes, the default is no animation
		 * @trigger close_all.jstree
		 */
		close_all : function (obj, animation) {
			if(!obj) { obj = '#'; }
			obj = this.get_node(obj);
			if(!obj) { return false; }
			var dom = obj.id === '#' ? this.get_container_ul() : this.get_node(obj, true),
				_this = this, i, j;
			if(!dom.length) {
				for(i = 0, j = obj.children_d.length; i < j; i++) {
					this._model.data[obj.children_d[i]].state.opened = false;
				}
				return this.trigger('close_all', { "node" : obj });
			}
			dom = this.is_open(obj) ? dom.find('li.jstree-open').addBack() : dom.find('li.jstree-open');
			dom.vakata_reverse().each(function () { _this.close_node(this, animation || 0); });
			/**
			 * triggered when an `close_all` call completes
			 * @event
			 * @name close_all.jstree
			 * @param {Object} node the closed node
			 */
			this.trigger('close_all', { "node" : obj });
		},
		/**
		 * checks if a node is disabled (not selectable)
		 * @name is_disabled(obj)
		 * @param  {mixed} obj
		 * @return {Boolean}
		 */
		is_disabled : function (obj) {
			obj = this.get_node(obj);
			return obj && obj.state && obj.state.disabled;
		},
		/**
		 * enables a node - so that it can be selected
		 * @name enable_node(obj)
		 * @param {mixed} obj the node to enable
		 * @trigger enable_node.jstree
		 */
		enable_node : function (obj) {
			var t1, t2;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.enable_node(obj[t1]);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			obj.state.disabled = false;
			this.get_node(obj,true).children('.jstree-anchor').removeClass('jstree-disabled');
			/**
			 * triggered when an node is enabled
			 * @event
			 * @name enable_node.jstree
			 * @param {Object} node the enabled node
			 */
			this.trigger('enable_node', { 'node' : obj });
		},
		/**
		 * disables a node - so that it can not be selected
		 * @name disable_node(obj)
		 * @param {mixed} obj the node to disable
		 * @trigger disable_node.jstree
		 */
		disable_node : function (obj) {
			var t1, t2;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.disable_node(obj[t1]);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			obj.state.disabled = true;
			this.get_node(obj,true).children('.jstree-anchor').addClass('jstree-disabled');
			/**
			 * triggered when an node is disabled
			 * @event
			 * @name disable_node.jstree
			 * @param {Object} node the disabled node
			 */
			this.trigger('disable_node', { 'node' : obj });
		},
		/**
		 * called when a node is selected by the user. Used internally.
		 * @private
		 * @name activate_node(obj, e)
		 * @param {mixed} obj the node
		 * @param {Object} e the related event
		 * @trigger activate_node.jstree
		 */
		activate_node : function (obj, e) {
			if(this.is_disabled(obj)) {
				return false;
			}
			if(!this.settings.core.multiple || (!e.metaKey && !e.ctrlKey && !e.shiftKey) || (e.shiftKey && (!this._data.core.last_clicked || !this.get_parent(obj) || this.get_parent(obj) !== this._data.core.last_clicked.parent ) )) {
				if(!this.settings.core.multiple && (e.metaKey || e.ctrlKey || e.shiftKey) && this.is_selected(obj)) {
					this.deselect_node(obj, false, false, e);
				}
				else {
					this.deselect_all(true);
					this.select_node(obj, false, false, e);
					this._data.core.last_clicked = this.get_node(obj);
				}
			}
			else {
				if(e.shiftKey) {
					var o = this.get_node(obj).id,
						l = this._data.core.last_clicked.id,
						p = this.get_node(this._data.core.last_clicked.parent).children,
						c = false,
						i, j;
					for(i = 0, j = p.length; i < j; i += 1) {
						// separate IFs work whem o and l are the same
						if(p[i] === o) {
							c = !c;
						}
						if(p[i] === l) {
							c = !c;
						}
						if(c || p[i] === o || p[i] === l) {
							this.select_node(p[i], false, false, e);
						}
						else {
							this.deselect_node(p[i], false, false, e);
						}
					}
				}
				else {
					if(!this.is_selected(obj)) {
						this.select_node(obj, false, false, e);
					}
					else {
						this.deselect_node(obj, false, false, e);
					}
				}
			}
			/**
			 * triggered when an node is clicked or intercated with by the user
			 * @event
			 * @name activate_node.jstree
			 * @param {Object} node
			 */
			this.trigger('activate_node', { 'node' : this.get_node(obj) });
		},
		/**
		 * applies the hover state on a node, called when a node is hovered by the user. Used internally.
		 * @private
		 * @name hover_node(obj)
		 * @param {mixed} obj
		 * @trigger hover_node.jstree
		 */
		hover_node : function (obj) {
			obj = this.get_node(obj, true);
			if(!obj || !obj.length || obj.children('.jstree-hovered').length) {
				return false;
			}
			var o = this.element.find('.jstree-hovered'), t = this.element;
			if(o && o.length) { this.dehover_node(o); }

			obj.children('.jstree-anchor').addClass('jstree-hovered');
			/**
			 * triggered when an node is hovered
			 * @event
			 * @name hover_node.jstree
			 * @param {Object} node
			 */
			this.trigger('hover_node', { 'node' : this.get_node(obj) });
			setTimeout(function () { t.attr('aria-activedescendant', obj[0].id); obj.attr('aria-selected', true); }, 0);
		},
		/**
		 * removes the hover state from a nodecalled when a node is no longer hovered by the user. Used internally.
		 * @private
		 * @name dehover_node(obj)
		 * @param {mixed} obj
		 * @trigger dehover_node.jstree
		 */
		dehover_node : function (obj) {
			obj = this.get_node(obj, true);
			if(!obj || !obj.length || !obj.children('.jstree-hovered').length) {
				return false;
			}
			obj.attr('aria-selected', false).children('.jstree-anchor').removeClass('jstree-hovered');
			/**
			 * triggered when an node is no longer hovered
			 * @event
			 * @name dehover_node.jstree
			 * @param {Object} node
			 */
			this.trigger('dehover_node', { 'node' : this.get_node(obj) });
		},
		/**
		 * select a node
		 * @name select_node(obj [, supress_event, prevent_open])
		 * @param {mixed} obj an array can be used to select multiple nodes
		 * @param {Boolean} supress_event if set to `true` the `changed.jstree` event won't be triggered
		 * @param {Boolean} prevent_open if set to `true` parents of the selected node won't be opened
		 * @trigger select_node.jstree, changed.jstree
		 */
		select_node : function (obj, supress_event, prevent_open, e) {
			var dom, t1, t2, th;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.select_node(obj[t1], supress_event, prevent_open, e);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			dom = this.get_node(obj, true);
			if(!obj.state.selected) {
				obj.state.selected = true;
				this._data.core.selected.push(obj.id);
				if(!prevent_open) {
					dom = this._open_to(obj);
				}
				if(dom && dom.length) {
					dom.children('.jstree-anchor').addClass('jstree-clicked');
				}
				/**
				 * triggered when an node is selected
				 * @event
				 * @name select_node.jstree
				 * @param {Object} node
				 * @param {Array} selected the current selection
				 * @param {Object} event the event (if any) that triggered this select_node
				 */
				this.trigger('select_node', { 'node' : obj, 'selected' : this._data.core.selected, 'event' : e });
				if(!supress_event) {
					/**
					 * triggered when selection changes
					 * @event
					 * @name changed.jstree
					 * @param {Object} node
					 * @param {Object} action the action that caused the selection to change
					 * @param {Array} selected the current selection
					 * @param {Object} event the event (if any) that triggered this changed event
					 */
					this.trigger('changed', { 'action' : 'select_node', 'node' : obj, 'selected' : this._data.core.selected, 'event' : e });
				}
			}
		},
		/**
		 * deselect a node
		 * @name deselect_node(obj [, supress_event])
		 * @param {mixed} obj an array can be used to deselect multiple nodes
		 * @param {Boolean} supress_event if set to `true` the `changed.jstree` event won't be triggered
		 * @trigger deselect_node.jstree, changed.jstree
		 */
		deselect_node : function (obj, supress_event, e) {
			var t1, t2, dom;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.deselect_node(obj[t1], supress_event, e);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			dom = this.get_node(obj, true);
			if(obj.state.selected) {
				obj.state.selected = false;
				this._data.core.selected = $.vakata.array_remove_item(this._data.core.selected, obj.id);
				if(dom.length) {
					dom.children('.jstree-anchor').removeClass('jstree-clicked');
				}
				/**
				 * triggered when an node is deselected
				 * @event
				 * @name deselect_node.jstree
				 * @param {Object} node
				 * @param {Array} selected the current selection
				 * @param {Object} event the event (if any) that triggered this deselect_node
				 */
				this.trigger('deselect_node', { 'node' : obj, 'selected' : this._data.core.selected, 'event' : e });
				if(!supress_event) {
					this.trigger('changed', { 'action' : 'deselect_node', 'node' : obj, 'selected' : this._data.core.selected, 'event' : e });
				}
			}
		},
		/**
		 * select all nodes in the tree
		 * @name select_all([supress_event])
		 * @param {Boolean} supress_event if set to `true` the `changed.jstree` event won't be triggered
		 * @trigger select_all.jstree, changed.jstree
		 */
		select_all : function (supress_event) {
			var tmp = this._data.core.selected.concat([]), i, j;
			this._data.core.selected = this._model.data['#'].children_d.concat();
			for(i = 0, j = this._data.core.selected.length; i < j; i++) {
				if(this._model.data[this._data.core.selected[i]]) {
					this._model.data[this._data.core.selected[i]].state.selected = true;
				}
			}
			this.redraw(true);
			/**
			 * triggered when all nodes are selected
			 * @event
			 * @name select_all.jstree
			 * @param {Array} selected the current selection
			 */
			this.trigger('select_all', { 'selected' : this._data.core.selected });
			if(!supress_event) {
				this.trigger('changed', { 'action' : 'select_all', 'selected' : this._data.core.selected, 'old_selection' : tmp });
			}
		},
		/**
		 * deselect all selected nodes
		 * @name deselect_all([supress_event])
		 * @param {Boolean} supress_event if set to `true` the `changed.jstree` event won't be triggered
		 * @trigger deselect_all.jstree, changed.jstree
		 */
		deselect_all : function (supress_event) {
			var tmp = this._data.core.selected.concat([]), i, j;
			for(i = 0, j = this._data.core.selected.length; i < j; i++) {
				if(this._model.data[this._data.core.selected[i]]) {
					this._model.data[this._data.core.selected[i]].state.selected = false;
				}
			}
			this._data.core.selected = [];
			this.element.find('.jstree-clicked').removeClass('jstree-clicked');
			/**
			 * triggered when all nodes are deselected
			 * @event
			 * @name deselect_all.jstree
			 * @param {Object} node the previous selection
			 * @param {Array} selected the current selection
			 */
			this.trigger('deselect_all', { 'selected' : this._data.core.selected, 'node' : tmp });
			if(!supress_event) {
				this.trigger('changed', { 'action' : 'deselect_all', 'selected' : this._data.core.selected, 'old_selection' : tmp });
			}
		},
		/**
		 * checks if a node is selected
		 * @name is_selected(obj)
		 * @param  {mixed}  obj
		 * @return {Boolean}
		 */
		is_selected : function (obj) {
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') {
				return false;
			}
			return obj.state.selected;
		},
		/**
		 * get an array of all selected node IDs
		 * @name get_selected([full])
		 * @param  {mixed}  full if set to `true` the returned array will consist of the full node objects, otherwise - only IDs will be returned
		 * @return {Array}
		 */
		get_selected : function (full) {
			return full ? $.map(this._data.core.selected, $.proxy(function (i) { return this.get_node(i); }, this)) : this._data.core.selected;
		},
		/**
		 * gets the current state of the tree so that it can be restored later with `set_state(state)`. Used internally.
		 * @name get_state()
		 * @private
		 * @return {Object}
		 */
		get_state : function () {
			var state	= {
				'core' : {
					'open' : [],
					'scroll' : {
						'left' : this.element.scrollLeft(),
						'top' : this.element.scrollTop()
					},
					/*!
					'themes' : {
						'name' : this.get_theme(),
						'icons' : this._data.core.themes.icons,
						'dots' : this._data.core.themes.dots
					},
					*/
					'selected' : []
				}
			}, i;
			for(i in this._model.data) {
				if(this._model.data.hasOwnProperty(i)) {
					if(i !== '#') {
						if(this._model.data[i].state.opened) {
							state.core.open.push(i);
						}
						if(this._model.data[i].state.selected) {
							state.core.selected.push(i);
						}
					}
				}
			}
			return state;
		},
		/**
		 * sets the state of the tree. Used internally.
		 * @name set_state(state [, callback])
		 * @private
		 * @param {Object} state the state to restore
		 * @param {Function} callback an optional function to execute once the state is restored.
		 * @trigger set_state.jstree
		 */
		set_state : function (state, callback) {
			if(state) {
				if(state.core) {
					var res, n, t, _this;
					if(state.core.open) {
						if(!$.isArray(state.core.open)) {
							delete state.core.open;
							this.set_state(state, callback);
							return false;
						}
						res = true;
						n = false;
						t = this;
						$.each(state.core.open.concat([]), function (i, v) {
							n = t.get_node(v);
							if(n) {
								if(t.is_loaded(v)) {
									if(t.is_closed(v)) {
										t.open_node(v, false, 0);
									}
									if(state && state.core && state.core.open) {
										$.vakata.array_remove_item(state.core.open, v);
									}
								}
								else {
									if(!t.is_loading(v)) {
										t.open_node(v, $.proxy(function () { this.set_state(state, callback); }, t), 0);
									}
									// there will be some async activity - so wait for it
									res = false;
								}
							}
						});
						if(res) {
							delete state.core.open;
							this.set_state(state, callback);
						}
						return false;
					}
					if(state.core.scroll) {
						if(state.core.scroll && state.core.scroll.left !== undefined) {
							this.element.scrollLeft(state.core.scroll.left);
						}
						if(state.core.scroll && state.core.scroll.top !== undefined) {
							this.element.scrollTop(state.core.scroll.top);
						}
						delete state.core.scroll;
						this.set_state(state, callback);
						return false;
					}
					/*!
					if(state.core.themes) {
						if(state.core.themes.name) {
							this.set_theme(state.core.themes.name);
						}
						if(typeof state.core.themes.dots !== 'undefined') {
							this[ state.core.themes.dots ? "show_dots" : "hide_dots" ]();
						}
						if(typeof state.core.themes.icons !== 'undefined') {
							this[ state.core.themes.icons ? "show_icons" : "hide_icons" ]();
						}
						delete state.core.themes;
						delete state.core.open;
						this.set_state(state, callback);
						return false;
					}
					*/
					if(state.core.selected) {
						_this = this;
						this.deselect_all();
						$.each(state.core.selected, function (i, v) {
							_this.select_node(v);
						});
						delete state.core.selected;
						this.set_state(state, callback);
						return false;
					}
					if($.isEmptyObject(state.core)) {
						delete state.core;
						this.set_state(state, callback);
						return false;
					}
				}
				if($.isEmptyObject(state)) {
					state = null;
					if(callback) { callback.call(this); }
					/**
					 * triggered when a `set_state` call completes
					 * @event
					 * @name set_state.jstree
					 */
					this.trigger('set_state');
					return false;
				}
				return true;
			}
			return false;
		},
		/**
		 * refreshes the tree - all nodes are reloaded with calls to `load_node`.
		 * @name refresh()
		 * @param {Boolean} skip_loading an option to skip showing the loading indicator
		 * @trigger refresh.jstree
		 */
		refresh : function (skip_loading) {
			this._data.core.state = this.get_state();
			this._cnt = 0;
			this._model.data = {
				'#' : {
					id : '#',
					parent : null,
					parents : [],
					children : [],
					children_d : [],
					state : { loaded : false }
				}
			};
			var c = this.get_container_ul()[0].className;
			if(!skip_loading) {
				this.element.html("<"+"ul class='jstree-container-ul'><"+"li class='jstree-initial-node jstree-loading jstree-leaf jstree-last'><i class='jstree-icon jstree-ocl'></i><"+"a class='jstree-anchor' href='#'><i class='jstree-icon jstree-themeicon-hidden'></i>" + this.get_string("Loading ...") + "</a></li></ul>");
			}
			this.load_node('#', function (o, s) {
				if(s) {
					this.get_container_ul()[0].className = c;
					this.set_state($.extend(true, {}, this._data.core.state), function () {
						/**
						 * triggered when a `refresh` call completes
						 * @event
						 * @name refresh.jstree
						 */
						this.trigger('refresh');
					});
				}
				this._data.core.state = null;
			});
		},
		/**
		 * set (change) the ID of a node
		 * @name set_id(obj, id)
		 * @param  {mixed} obj the node
		 * @param  {String} id the new ID
		 * @return {Boolean}
		 */
		set_id : function (obj, id) {
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') { return false; }
			var i, j, m = this._model.data;
			// update parents (replace current ID with new one in children and children_d)
			m[obj.parent].children[$.inArray(obj.id, m[obj.parent].children)] = id;
			for(i = 0, j = obj.parents.length; i < j; i++) {
				m[obj.parents[i]].children_d[$.inArray(obj.id, m[obj.parents[i]].children_d)] = id;
			}
			// update children (replace current ID with new one in parent and parents)
			for(i = 0, j = obj.children.length; i < j; i++) {
				m[obj.children[i]].parent = id;
			}
			for(i = 0, j = obj.children_d.length; i < j; i++) {
				m[obj.children_d[i]].parents[$.inArray(obj.id, m[obj.children_d[i]].parents)] = id;
			}
			i = $.inArray(obj.id, this._data.core.selected);
			if(i !== -1) { this._data.core.selected[i] = id; }
			// update model and obj itself (obj.id, this._model.data[KEY])
			i = this.get_node(obj.id, true);
			if(i) {
				i.attr('id', id);
			}
			delete m[obj.id];
			obj.id = id;
			m[id] = obj;
			return true;
		},
		/**
		 * get the text value of a node
		 * @name get_text(obj)
		 * @param  {mixed} obj the node
		 * @return {String}
		 */
		get_text : function (obj) {
			obj = this.get_node(obj);
			return (!obj || obj.id === '#') ? false : obj.text;
		},
		/**
		 * set the text value of a node. Used internally, please use `rename_node(obj, val)`.
		 * @private
		 * @name set_text(obj, val)
		 * @param  {mixed} obj the node, you can pass an array to set the text on multiple nodes
		 * @param  {String} val the new text value
		 * @return {Boolean}
		 * @trigger set_text.jstree
		 */
		set_text : function (obj, val) {
			var t1, t2, dom, tmp;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.set_text(obj[t1], val);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') { return false; }
			obj.text = val;
			dom = this.get_node(obj, true);
			if(dom.length) {
				dom = dom.children(".jstree-anchor:eq(0)");
				tmp = dom.children("I").clone();
				dom.html(val).prepend(tmp);
				/**
				 * triggered when a node text value is changed
				 * @event
				 * @name set_text.jstree
				 * @param {Object} obj
				 * @param {String} text the new value
				 */
				this.trigger('set_text',{ "obj" : obj, "text" : val });
			}
			return true;
		},
		/**
		 * gets a JSON representation of a node (or the whole tree)
		 * @name get_json([obj, options])
		 * @param  {mixed} obj
		 * @param  {Object} options
		 * @param  {Boolean} options.no_state do not return state information
		 * @param  {Boolean} options.no_id do not return ID
		 * @param  {Boolean} options.no_children do not include children
		 * @param  {Boolean} options.no_data do not include node data
		 * @param  {Boolean} options.flat return flat JSON instead of nested
		 * @return {Object}
		 */
		get_json : function (obj, options, flat) {
			obj = this.get_node(obj || '#');
			if(!obj) { return false; }
			if(options && options.flat && !flat) { flat = []; }
			var tmp = {
				'id' : obj.id,
				'text' : obj.text,
				'icon' : this.get_icon(obj),
				'li_attr' : obj.li_attr,
				'a_attr' : obj.a_attr,
				'state' : {},
				'data' : options && options.no_data ? false : obj.data
				//( this.get_node(obj, true).length ? this.get_node(obj, true).data() : obj.data ),
			}, i, j;
			if(options && options.flat) {
				tmp.parent = obj.parent;
			}
			else {
				tmp.children = [];
			}
			if(!options || !options.no_state) {
				for(i in obj.state) {
					if(obj.state.hasOwnProperty(i)) {
						tmp.state[i] = obj.state[i];
					}
				}
			}
			if(options && options.no_id) {
				delete tmp.id;
				if(tmp.li_attr && tmp.li_attr.id) {
					delete tmp.li_attr.id;
				}
			}
			if(options && options.flat && obj.id !== '#') {
				flat.push(tmp);
			}
			if(!options || !options.no_children) {
				for(i = 0, j = obj.children.length; i < j; i++) {
					if(options && options.flat) {
						this.get_json(obj.children[i], options, flat);
					}
					else {
						tmp.children.push(this.get_json(obj.children[i], options));
					}
				}
			}
			return options && options.flat ? flat : (obj.id === '#' ? tmp.children : tmp);
		},
		/**
		 * create a new node (do not confuse with load_node)
		 * @name create_node([obj, node, pos, callback, is_loaded])
		 * @param  {mixed}   par       the parent node
		 * @param  {mixed}   node      the data for the new node (a valid JSON object, or a simple string with the name)
		 * @param  {mixed}   pos       the index at which to insert the node, "first" and "last" are also supported, default is "last"
		 * @param  {Function} callback a function to be called once the node is created
		 * @param  {Boolean} is_loaded internal argument indicating if the parent node was succesfully loaded
		 * @return {String}            the ID of the newly create node
		 * @trigger model.jstree, create_node.jstree
		 */
		create_node : function (par, node, pos, callback, is_loaded) {
			par = this.get_node(par);
			if(!par) { return false; }
			pos = pos === undefined ? "last" : pos;
			if(!pos.toString().match(/^(before|after)$/) && !is_loaded && !this.is_loaded(par)) {
				return this.load_node(par, function () { this.create_node(par, node, pos, callback, true); });
			}
			if(!node) { node = { "text" : this.get_string('New node') }; }
			if(node.text === undefined) { node.text = this.get_string('New node'); }
			var tmp, dpc, i, j;

			if(par.id === '#') {
				if(pos === "before") { pos = "first"; }
				if(pos === "after") { pos = "last"; }
			}
			switch(pos) {
				case "before":
					tmp = this.get_node(par.parent);
					pos = $.inArray(par.id, tmp.children);
					par = tmp;
					break;
				case "after" :
					tmp = this.get_node(par.parent);
					pos = $.inArray(par.id, tmp.children) + 1;
					par = tmp;
					break;
				case "inside":
				case "first":
					pos = 0;
					break;
				case "last":
					pos = par.children.length;
					break;
				default:
					if(!pos) { pos = 0; }
					break;
			}
			if(pos > par.children.length) { pos = par.children.length; }
			if(!node.id) { node.id = true; }
			if(!this.check("create_node", node, par, pos)) {
				this.settings.core.error.call(this, this._data.core.last_error);
				return false;
			}
			if(node.id === true) { delete node.id; }
			node = this._parse_model_from_json(node, par.id, par.parents.concat());
			if(!node) { return false; }
			tmp = this.get_node(node);
			dpc = [];
			dpc.push(node);
			dpc = dpc.concat(tmp.children_d);
			this.trigger('model', { "nodes" : dpc, "parent" : par.id });

			par.children_d = par.children_d.concat(dpc);
			for(i = 0, j = par.parents.length; i < j; i++) {
				this._model.data[par.parents[i]].children_d = this._model.data[par.parents[i]].children_d.concat(dpc);
			}
			node = tmp;
			tmp = [];
			for(i = 0, j = par.children.length; i < j; i++) {
				tmp[i >= pos ? i+1 : i] = par.children[i];
			}
			tmp[pos] = node.id;
			par.children = tmp;

			this.redraw_node(par, true);
			if(callback) { callback.call(this, this.get_node(node)); }
			/**
			 * triggered when a node is created
			 * @event
			 * @name create_node.jstree
			 * @param {Object} node
			 * @param {String} parent the parent's ID
			 * @param {Number} position the position of the new node among the parent's children
			 */
			this.trigger('create_node', { "node" : this.get_node(node), "parent" : par.id, "position" : pos });
			return node.id;
		},
		/**
		 * set the text value of a node
		 * @name rename_node(obj, val)
		 * @param  {mixed} obj the node, you can pass an array to rename multiple nodes to the same name
		 * @param  {String} val the new text value
		 * @return {Boolean}
		 * @trigger rename_node.jstree
		 */
		rename_node : function (obj, val) {
			var t1, t2, old;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.rename_node(obj[t1], val);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') { return false; }
			old = obj.text;
			if(!this.check("rename_node", obj, this.get_parent(obj), val)) {
				this.settings.core.error.call(this, this._data.core.last_error);
				return false;
			}
			this.set_text(obj, val); // .apply(this, Array.prototype.slice.call(arguments))
			/**
			 * triggered when a node is renamed
			 * @event
			 * @name rename_node.jstree
			 * @param {Object} node
			 * @param {String} text the new value
			 * @param {String} old the old value
			 */
			this.trigger('rename_node', { "node" : obj, "text" : val, "old" : old });
			return true;
		},
		/**
		 * remove a node
		 * @name delete_node(obj)
		 * @param  {mixed} obj the node, you can pass an array to delete multiple nodes
		 * @return {Boolean}
		 * @trigger delete_node.jstree, changed.jstree
		 */
		delete_node : function (obj) {
			var t1, t2, par, pos, tmp, i, j, k, l, c;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.delete_node(obj[t1]);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') { return false; }
			par = this.get_node(obj.parent);
			pos = $.inArray(obj.id, par.children);
			c = false;
			if(!this.check("delete_node", obj, par, pos)) {
				this.settings.core.error.call(this, this._data.core.last_error);
				return false;
			}
			if(pos !== -1) {
				par.children = $.vakata.array_remove(par.children, pos);
			}
			tmp = obj.children_d.concat([]);
			tmp.push(obj.id);
			for(k = 0, l = tmp.length; k < l; k++) {
				for(i = 0, j = obj.parents.length; i < j; i++) {
					pos = $.inArray(tmp[k], this._model.data[obj.parents[i]].children_d);
					if(pos !== -1) {
						this._model.data[obj.parents[i]].children_d = $.vakata.array_remove(this._model.data[obj.parents[i]].children_d, pos);
					}
				}
				if(this._model.data[tmp[k]].state.selected) {
					c = true;
					pos = $.inArray(tmp[k], this._data.core.selected);
					if(pos !== -1) {
						this._data.core.selected = $.vakata.array_remove(this._data.core.selected, pos);
					}
				}
			}
			/**
			 * triggered when a node is deleted
			 * @event
			 * @name delete_node.jstree
			 * @param {Object} node
			 * @param {String} parent the parent's ID
			 */
			this.trigger('delete_node', { "node" : obj, "parent" : par.id });
			if(c) {
				this.trigger('changed', { 'action' : 'delete_node', 'node' : obj, 'selected' : this._data.core.selected, 'parent' : par.id });
			}
			for(k = 0, l = tmp.length; k < l; k++) {
				delete this._model.data[tmp[k]];
			}
			this.redraw_node(par, true);
			return true;
		},
		/**
		 * check if an operation is premitted on the tree. Used internally.
		 * @private
		 * @name check(chk, obj, par, pos)
		 * @param  {String} chk the operation to check, can be "create_node", "rename_node", "delete_node", "copy_node" or "move_node"
		 * @param  {mixed} obj the node
		 * @param  {mixed} par the parent
		 * @param  {mixed} pos the position to insert at, or if "rename_node" - the new name
		 * @return {Boolean}
		 */
		check : function (chk, obj, par, pos) {
			obj = obj && obj.id ? obj : this.get_node(obj);
			par = par && par.id ? par : this.get_node(par);
			var tmp = chk.match(/^move_node|copy_node|create_node$/i) ? par : obj,
				chc = this.settings.core.check_callback;
			if(chk === "move_node") {
				if(obj.id === par.id || $.inArray(obj.id, par.children) === pos || $.inArray(par.id, obj.children_d) !== -1) {
					this._data.core.last_error = { 'error' : 'check', 'plugin' : 'core', 'id' : 'core_01', 'reason' : 'Moving parent inside child', 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
					return false;
				}
			}
			tmp = this.get_node(tmp, true);
			if(tmp.length) { tmp = tmp.data('jstree'); }
			if(tmp && tmp.functions && (tmp.functions[chk] === false || tmp.functions[chk] === true)) {
				if(tmp.functions[chk] === false) {
					this._data.core.last_error = { 'error' : 'check', 'plugin' : 'core', 'id' : 'core_02', 'reason' : 'Node data prevents function: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
				}
				return tmp.functions[chk];
			}
			if(chc === false || ($.isFunction(chc) && chc.call(this, chk, obj, par, pos) === false) || (chc && chc[chk] === false)) {
				this._data.core.last_error = { 'error' : 'check', 'plugin' : 'core', 'id' : 'core_03', 'reason' : 'User config for core.check_callback prevents function: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
				return false;
			}
			return true;
		},
		/**
		 * get the last error
		 * @name last_error()
		 * @return {Object}
		 */
		last_error : function () {
			return this._data.core.last_error;
		},
		/**
		 * move a node to a new parent
		 * @name move_node(obj, par [, pos, callback, is_loaded])
		 * @param  {mixed} obj the node to move, pass an array to move multiple nodes
		 * @param  {mixed} par the new parent
		 * @param  {mixed} pos the position to insert at ("first" and "last" are supported, as well as "before" and "after"), defaults to `0`
		 * @param  {function} callback a function to call once the move is completed, receives 3 arguments - the node, the new parent and the position
		 * @param  {Boolean} internal parameter indicating if the parent node has been loaded
		 * @trigger move_node.jstree
		 */
		move_node : function (obj, par, pos, callback, is_loaded) {
			var t1, t2, old_par, new_par, old_ins, is_multi, dpc, tmp, i, j, k, l, p;
			if($.isArray(obj)) {
				obj = obj.reverse().slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.move_node(obj[t1], par, pos, callback, is_loaded);
				}
				return true;
			}
			obj = obj && obj.id ? obj : this.get_node(obj);
			par = this.get_node(par);
			pos = pos === undefined ? 0 : pos;

			if(!par || !obj || obj.id === '#') { return false; }
			if(!pos.toString().match(/^(before|after)$/) && !is_loaded && !this.is_loaded(par)) {
				return this.load_node(par, function () { this.move_node(obj, par, pos, callback, true); });
			}

			old_par = (obj.parent || '#').toString();
			new_par = (!pos.toString().match(/^(before|after)$/) || par.id === '#') ? par : this.get_node(par.parent);
			old_ins = this._model.data[obj.id] ? this : $.jstree.reference(obj.id);
			is_multi = !old_ins || !old_ins._id || (this._id !== old_ins._id);
			if(is_multi) {
				if(this.copy_node(obj, par, pos, callback, is_loaded)) {
					if(old_ins) { old_ins.delete_node(obj); }
					return true;
				}
				return false;
			}
			//var m = this._model.data;
			if(new_par.id === '#') {
				if(pos === "before") { pos = "first"; }
				if(pos === "after") { pos = "last"; }
			}
			switch(pos) {
				case "before":
					pos = $.inArray(par.id, new_par.children);
					break;
				case "after" :
					pos = $.inArray(par.id, new_par.children) + 1;
					break;
				case "inside":
				case "first":
					pos = 0;
					break;
				case "last":
					pos = new_par.children.length;
					break;
				default:
					if(!pos) { pos = 0; }
					break;
			}
			if(pos > new_par.children.length) { pos = new_par.children.length; }
			if(!this.check("move_node", obj, new_par, pos)) {
				this.settings.core.error.call(this, this._data.core.last_error);
				return false;
			}
			if(obj.parent === new_par.id) {
				dpc = new_par.children.concat();
				tmp = $.inArray(obj.id, dpc);
				if(tmp !== -1) {
					dpc = $.vakata.array_remove(dpc, tmp);
					if(pos > tmp) { pos--; }
				}
				tmp = [];
				for(i = 0, j = dpc.length; i < j; i++) {
					tmp[i >= pos ? i+1 : i] = dpc[i];
				}
				tmp[pos] = obj.id;
				new_par.children = tmp;
				this._node_changed(new_par.id);
				this.redraw(new_par.id === '#');
			}
			else {
				// clean old parent and up
				tmp = obj.children_d.concat();
				tmp.push(obj.id);
				for(i = 0, j = obj.parents.length; i < j; i++) {
					dpc = [];
					p = old_ins._model.data[obj.parents[i]].children_d;
					for(k = 0, l = p.length; k < l; k++) {
						if($.inArray(p[k], tmp) === -1) {
							dpc.push(p[k]);
						}
					}
					old_ins._model.data[obj.parents[i]].children_d = dpc;
				}
				old_ins._model.data[old_par].children = $.vakata.array_remove_item(old_ins._model.data[old_par].children, obj.id);

				// insert into new parent and up
				for(i = 0, j = new_par.parents.length; i < j; i++) {
					this._model.data[new_par.parents[i]].children_d = this._model.data[new_par.parents[i]].children_d.concat(tmp);
				}
				dpc = [];
				for(i = 0, j = new_par.children.length; i < j; i++) {
					dpc[i >= pos ? i+1 : i] = new_par.children[i];
				}
				dpc[pos] = obj.id;
				new_par.children = dpc;
				new_par.children_d.push(obj.id);
				new_par.children_d = new_par.children_d.concat(obj.children_d);

				// update object
				obj.parent = new_par.id;
				tmp = new_par.parents.concat();
				tmp.unshift(new_par.id);
				p = obj.parents.length;
				obj.parents = tmp;

				// update object children
				tmp = tmp.concat();
				for(i = 0, j = obj.children_d.length; i < j; i++) {
					this._model.data[obj.children_d[i]].parents = this._model.data[obj.children_d[i]].parents.slice(0,p*-1);
					Array.prototype.push.apply(this._model.data[obj.children_d[i]].parents, tmp);
				}

				this._node_changed(old_par);
				this._node_changed(new_par.id);
				this.redraw(old_par === '#' || new_par.id === '#');
			}
			if(callback) { callback.call(this, obj, new_par, pos); }
			/**
			 * triggered when a node is moved
			 * @event
			 * @name move_node.jstree
			 * @param {Object} node
			 * @param {String} parent the parent's ID
			 * @param {Number} position the position of the node among the parent's children
			 * @param {String} old_parent the old parent of the node
			 * @param {Boolean} is_multi do the node and new parent belong to different instances
			 * @param {jsTree} old_instance the instance the node came from
			 * @param {jsTree} new_instance the instance of the new parent
			 */
			this.trigger('move_node', { "node" : obj, "parent" : new_par.id, "position" : pos, "old_parent" : old_par, "is_multi" : is_multi, 'old_instance' : old_ins, 'new_instance' : this });
			return true;
		},
		/**
		 * copy a node to a new parent
		 * @name copy_node(obj, par [, pos, callback, is_loaded])
		 * @param  {mixed} obj the node to copy, pass an array to copy multiple nodes
		 * @param  {mixed} par the new parent
		 * @param  {mixed} pos the position to insert at ("first" and "last" are supported, as well as "before" and "after"), defaults to `0`
		 * @param  {function} callback a function to call once the move is completed, receives 3 arguments - the node, the new parent and the position
		 * @param  {Boolean} internal parameter indicating if the parent node has been loaded
		 * @trigger model.jstree copy_node.jstree
		 */
		copy_node : function (obj, par, pos, callback, is_loaded) {
			var t1, t2, dpc, tmp, i, j, node, old_par, new_par, old_ins, is_multi;
			if($.isArray(obj)) {
				obj = obj.reverse().slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.copy_node(obj[t1], par, pos, callback, is_loaded);
				}
				return true;
			}
			obj = obj && obj.id ? obj : this.get_node(obj);
			par = this.get_node(par);
			pos = pos === undefined ? 0 : pos;

			if(!par || !obj || obj.id === '#') { return false; }
			if(!pos.toString().match(/^(before|after)$/) && !is_loaded && !this.is_loaded(par)) {
				return this.load_node(par, function () { this.copy_node(obj, par, pos, callback, true); });
			}

			old_par = (obj.parent || '#').toString();
			new_par = (!pos.toString().match(/^(before|after)$/) || par.id === '#') ? par : this.get_node(par.parent);
			old_ins = this._model.data[obj.id] ? this : $.jstree.reference(obj.id);
			is_multi = !old_ins || !old_ins._id || (this._id !== old_ins._id);
			if(new_par.id === '#') {
				if(pos === "before") { pos = "first"; }
				if(pos === "after") { pos = "last"; }
			}
			switch(pos) {
				case "before":
					pos = $.inArray(par.id, new_par.children);
					break;
				case "after" :
					pos = $.inArray(par.id, new_par.children) + 1;
					break;
				case "inside":
				case "first":
					pos = 0;
					break;
				case "last":
					pos = new_par.children.length;
					break;
				default:
					if(!pos) { pos = 0; }
					break;
			}
			if(pos > new_par.children.length) { pos = new_par.children.length; }
			if(!this.check("copy_node", obj, new_par, pos)) {
				this.settings.core.error.call(this, this._data.core.last_error);
				return false;
			}
			node = old_ins ? old_ins.get_json(obj, { no_id : true, no_data : true, no_state : true }) : obj;
			if(!node) { return false; }
			if(node.id === true) { delete node.id; }
			node = this._parse_model_from_json(node, new_par.id, new_par.parents.concat());
			if(!node) { return false; }
			tmp = this.get_node(node);
			dpc = [];
			dpc.push(node);
			dpc = dpc.concat(tmp.children_d);
			this.trigger('model', { "nodes" : dpc, "parent" : new_par.id });

			// insert into new parent and up
			for(i = 0, j = new_par.parents.length; i < j; i++) {
				this._model.data[new_par.parents[i]].children_d = this._model.data[new_par.parents[i]].children_d.concat(dpc);
			}
			dpc = [];
			for(i = 0, j = new_par.children.length; i < j; i++) {
				dpc[i >= pos ? i+1 : i] = new_par.children[i];
			}
			dpc[pos] = tmp.id;
			new_par.children = dpc;
			new_par.children_d.push(tmp.id);
			new_par.children_d = new_par.children_d.concat(tmp.children_d);

			this._node_changed(new_par.id);
			this.redraw(new_par.id === '#');
			if(callback) { callback.call(this, tmp, new_par, pos); }
			/**
			 * triggered when a node is copied
			 * @event
			 * @name copy_node.jstree
			 * @param {Object} node the copied node
			 * @param {Object} original the original node
			 * @param {String} parent the parent's ID
			 * @param {Number} position the position of the node among the parent's children
			 * @param {String} old_parent the old parent of the node
			 * @param {Boolean} is_multi do the node and new parent belong to different instances
			 * @param {jsTree} old_instance the instance the node came from
			 * @param {jsTree} new_instance the instance of the new parent
			 */
			this.trigger('copy_node', { "node" : tmp, "original" : obj, "parent" : new_par.id, "position" : pos, "old_parent" : old_par, "is_multi" : is_multi, 'old_instance' : old_ins, 'new_instance' : this });
			return tmp.id;
		},
		/**
		 * cut a node (a later call to `paste(obj)` would move the node)
		 * @name cut(obj)
		 * @param  {mixed} obj multiple objects can be passed using an array
		 * @trigger cut.jstree
		 */
		cut : function (obj) {
			if(!obj) { obj = this._data.core.selected.concat(); }
			if(!$.isArray(obj)) { obj = [obj]; }
			if(!obj.length) { return false; }
			var tmp = [], o, t1, t2;
			for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
				o = this.get_node(obj[t1]);
				if(o && o.id && o.id !== '#') { tmp.push(o); }
			}
			if(!tmp.length) { return false; }
			ccp_node = tmp;
			ccp_inst = this;
			ccp_mode = 'move_node';
			/**
			 * triggered when nodes are added to the buffer for moving
			 * @event
			 * @name cut.jstree
			 * @param {Array} node
			 */
			this.trigger('cut', { "node" : obj });
		},
		/**
		 * copy a node (a later call to `paste(obj)` would copy the node)
		 * @name copy(obj)
		 * @param  {mixed} obj multiple objects can be passed using an array
		 * @trigger copy.jstre
		 */
		copy : function (obj) {
			if(!obj) { obj = this._data.core.selected.concat(); }
			if(!$.isArray(obj)) { obj = [obj]; }
			if(!obj.length) { return false; }
			var tmp = [], o, t1, t2;
			for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
				o = this.get_node(obj[t1]);
				if(o && o.id && o.id !== '#') { tmp.push(o); }
			}
			if(!tmp.length) { return false; }
			ccp_node = tmp;
			ccp_inst = this;
			ccp_mode = 'copy_node';
			/**
			 * triggered when nodes are added to the buffer for copying
			 * @event
			 * @name copy.jstree
			 * @param {Array} node
			 */
			this.trigger('copy', { "node" : obj });
		},
		/**
		 * get the current buffer (any nodes that are waiting for a paste operation)
		 * @name get_buffer()
		 * @return {Object} an object consisting of `mode` ("copy_node" or "move_node"), `node` (an array of objects) and `inst` (the instance)
		 */
		get_buffer : function () {
			return { 'mode' : ccp_mode, 'node' : ccp_node, 'inst' : ccp_inst };
		},
		/**
		 * check if there is something in the buffer to paste
		 * @name can_paste()
		 * @return {Boolean}
		 */
		can_paste : function () {
			return ccp_mode !== false && ccp_node !== false; // && ccp_inst._model.data[ccp_node];
		},
		/**
		 * copy or move the previously cut or copied nodes to a new parent
		 * @name paste(obj)
		 * @param  {mixed} obj the new parent
		 * @trigger paste.jstree
		 */
		paste : function (obj) {
			obj = this.get_node(obj);
			if(!obj || !ccp_mode || !ccp_mode.match(/^(copy_node|move_node)$/) || !ccp_node) { return false; }
			if(this[ccp_mode](ccp_node, obj)) {
				/**
				 * triggered when paste is invoked
				 * @event
				 * @name paste.jstree
				 * @param {String} parent the ID of the receiving node
				 * @param {Array} node the nodes in the buffer
				 * @param {String} mode the performed operation - "copy_node" or "move_node"
				 */
				this.trigger('paste', { "parent" : obj.id, "node" : ccp_node, "mode" : ccp_mode });
			}
			ccp_node = false;
			ccp_mode = false;
			ccp_inst = false;
		},
		/**
		 * put a node in edit mode (input field to rename the node)
		 * @name edit(obj [, default_text])
		 * @param  {mixed} obj
		 * @param  {String} default_text the text to populate the input with (if omitted the node text value is used)
		 */
		edit : function (obj, default_text) {
			obj = this._open_to(obj);
			if(!obj || !obj.length) { return false; }
			var rtl = this._data.core.rtl,
				w  = this.element.width(),
				a  = obj.children('.jstree-anchor'),
				s  = $('<span>'),
				/*!
				oi = obj.children("i:visible"),
				ai = a.children("i:visible"),
				w1 = oi.width() * oi.length,
				w2 = ai.width() * ai.length,
				*/
				t  = typeof default_text === 'string' ? default_text : this.get_text(obj),
				h1 = $("<"+"div />", { css : { "position" : "absolute", "top" : "-200px", "left" : (rtl ? "0px" : "-1000px"), "visibility" : "hidden" } }).appendTo("body"),
				h2 = $("<"+"input />", {
						"value" : t,
						"class" : "jstree-rename-input",
						// "size" : t.length,
						"css" : {
							"padding" : "0",
							"border" : "1px solid silver",
							"box-sizing" : "border-box",
							"display" : "inline-block",
							"height" : (this._data.core.li_height) + "px",
							"lineHeight" : (this._data.core.li_height) + "px",
							"width" : "150px" // will be set a bit further down
						},
						"blur" : $.proxy(function () {
							var i = s.children(".jstree-rename-input"),
								v = i.val();
							if(v === "") { v = t; }
							h1.remove();
							s.replaceWith(a);
							s.remove();
							this.set_text(obj, t);
							if(this.rename_node(obj, v) === false) {
								this.set_text(obj, t); // move this up? and fix #483
							}
						}, this),
						"keydown" : function (event) {
							var key = event.which;
							if(key === 27) {
								this.value = t;
							}
							if(key === 27 || key === 13 || key === 37 || key === 38 || key === 39 || key === 40 || key === 32) {
								event.stopImmediatePropagation();
							}
							if(key === 27 || key === 13) {
								event.preventDefault();
								this.blur();
							}
						},
						"click" : function (e) { e.stopImmediatePropagation(); },
						"mousedown" : function (e) { e.stopImmediatePropagation(); },
						"keyup" : function (event) {
							h2.width(Math.min(h1.text("pW" + this.value).width(),w));
						},
						"keypress" : function(event) {
							if(event.which === 13) { return false; }
						}
					}),
				fn = {
						fontFamily		: a.css('fontFamily')		|| '',
						fontSize		: a.css('fontSize')			|| '',
						fontWeight		: a.css('fontWeight')		|| '',
						fontStyle		: a.css('fontStyle')		|| '',
						fontStretch		: a.css('fontStretch')		|| '',
						fontVariant		: a.css('fontVariant')		|| '',
						letterSpacing	: a.css('letterSpacing')	|| '',
						wordSpacing		: a.css('wordSpacing')		|| ''
				};
			this.set_text(obj, "");
			s.attr('class', a.attr('class')).append(a.contents().clone()).append(h2);
			a.replaceWith(s);
			h1.css(fn);
			h2.css(fn).width(Math.min(h1.text("pW" + h2[0].value).width(),w))[0].select();
		},


		/**
		 * changes the theme
		 * @name set_theme(theme_name [, theme_url])
		 * @param {String} theme_name the name of the new theme to apply
		 * @param {mixed} theme_url  the location of the CSS file for this theme. Omit or set to `false` if you manually included the file. Set to `true` to autoload from the `core.themes.dir` directory.
		 * @trigger set_theme.jstree
		 */
		set_theme : function (theme_name, theme_url) {
			if(!theme_name) { return false; }
			if(theme_url === true) {
				var dir = this.settings.core.themes.dir;
				if(!dir) { dir = $.jstree.path + '/themes'; }
				theme_url = dir + '/' + theme_name + '/style.css';
			}
			if(theme_url && $.inArray(theme_url, themes_loaded) === -1) {
				$('head').append('<'+'link rel="stylesheet" href="' + theme_url + '" type="text/css" />');
				themes_loaded.push(theme_url);
			}
			if(this._data.core.themes.name) {
				this.element.removeClass('jstree-' + this._data.core.themes.name);
			}
			this._data.core.themes.name = theme_name;
			this.element.addClass('jstree-' + theme_name);
			this.element[this.settings.core.themes.responsive ? 'addClass' : 'removeClass' ]('jstree-' + theme_name + '-responsive');
			/**
			 * triggered when a theme is set
			 * @event
			 * @name set_theme.jstree
			 * @param {String} theme the new theme
			 */
			this.trigger('set_theme', { 'theme' : theme_name });
		},
		/**
		 * gets the name of the currently applied theme name
		 * @name get_theme()
		 * @return {String}
		 */
		get_theme : function () { return this._data.core.themes.name; },
		/**
		 * changes the theme variant (if the theme has variants)
		 * @name set_theme_variant(variant_name)
		 * @param {String|Boolean} variant_name the variant to apply (if `false` is used the current variant is removed)
		 */
		set_theme_variant : function (variant_name) {
			if(this._data.core.themes.variant) {
				this.element.removeClass('jstree-' + this._data.core.themes.name + '-' + this._data.core.themes.variant);
			}
			this._data.core.themes.variant = variant_name;
			if(variant_name) {
				this.element.addClass('jstree-' + this._data.core.themes.name + '-' + this._data.core.themes.variant);
			}
		},
		/**
		 * gets the name of the currently applied theme variant
		 * @name get_theme()
		 * @return {String}
		 */
		get_theme_variant : function () { return this._data.core.themes.variant; },
		/**
		 * shows a striped background on the container (if the theme supports it)
		 * @name show_stripes()
		 */
		show_stripes : function () { this._data.core.themes.stripes = true; this.get_container_ul().addClass("jstree-striped"); },
		/**
		 * hides the striped background on the container
		 * @name hide_stripes()
		 */
		hide_stripes : function () { this._data.core.themes.stripes = false; this.get_container_ul().removeClass("jstree-striped"); },
		/**
		 * toggles the striped background on the container
		 * @name toggle_stripes()
		 */
		toggle_stripes : function () { if(this._data.core.themes.stripes) { this.hide_stripes(); } else { this.show_stripes(); } },
		/**
		 * shows the connecting dots (if the theme supports it)
		 * @name show_dots()
		 */
		show_dots : function () { this._data.core.themes.dots = true; this.get_container_ul().removeClass("jstree-no-dots"); },
		/**
		 * hides the connecting dots
		 * @name hide_dots()
		 */
		hide_dots : function () { this._data.core.themes.dots = false; this.get_container_ul().addClass("jstree-no-dots"); },
		/**
		 * toggles the connecting dots
		 * @name toggle_dots()
		 */
		toggle_dots : function () { if(this._data.core.themes.dots) { this.hide_dots(); } else { this.show_dots(); } },
		/**
		 * show the node icons
		 * @name show_icons()
		 */
		show_icons : function () { this._data.core.themes.icons = true; this.get_container_ul().removeClass("jstree-no-icons"); },
		/**
		 * hide the node icons
		 * @name hide_icons()
		 */
		hide_icons : function () { this._data.core.themes.icons = false; this.get_container_ul().addClass("jstree-no-icons"); },
		/**
		 * toggle the node icons
		 * @name toggle_icons()
		 */
		toggle_icons : function () { if(this._data.core.themes.icons) { this.hide_icons(); } else { this.show_icons(); } },
		/**
		 * set the node icon for a node
		 * @name set_icon(obj, icon)
		 * @param {mixed} obj
		 * @param {String} icon the new icon - can be a path to an icon or a className, if using an image that is in the current directory use a `./` prefix, otherwise it will be detected as a class
		 */
		set_icon : function (obj, icon) {
			var t1, t2, dom, old;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.set_icon(obj[t1], icon);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') { return false; }
			old = obj.icon;
			obj.icon = icon;
			dom = this.get_node(obj, true).children(".jstree-anchor").children(".jstree-themeicon");
			if(icon === false) {
				this.hide_icon(obj);
			}
			else if(icon === true) {
				dom.removeClass('jstree-themeicon-custom ' + old).css("background","").removeAttr("rel");
			}
			else if(icon.indexOf("/") === -1 && icon.indexOf(".") === -1) {
				dom.removeClass(old).css("background","");
				dom.addClass(icon + ' jstree-themeicon-custom').attr("rel",icon);
			}
			else {
				dom.removeClass(old).css("background","");
				dom.addClass('jstree-themeicon-custom').css("background", "url('" + icon + "') center center no-repeat").attr("rel",icon);
			}
			return true;
		},
		/**
		 * get the node icon for a node
		 * @name get_icon(obj)
		 * @param {mixed} obj
		 * @return {String}
		 */
		get_icon : function (obj) {
			obj = this.get_node(obj);
			return (!obj || obj.id === '#') ? false : obj.icon;
		},
		/**
		 * hide the icon on an individual node
		 * @name hide_icon(obj)
		 * @param {mixed} obj
		 */
		hide_icon : function (obj) {
			var t1, t2;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.hide_icon(obj[t1]);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj === '#') { return false; }
			obj.icon = false;
			this.get_node(obj, true).children("a").children(".jstree-themeicon").addClass('jstree-themeicon-hidden');
			return true;
		},
		/**
		 * show the icon on an individual node
		 * @name show_icon(obj)
		 * @param {mixed} obj
		 */
		show_icon : function (obj) {
			var t1, t2, dom;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.show_icon(obj[t1]);
				}
				return true;
			}
			obj = this.get_node(obj);
			if(!obj || obj === '#') { return false; }
			dom = this.get_node(obj, true);
			obj.icon = dom.length ? dom.children("a").children(".jstree-themeicon").attr('rel') : true;
			if(!obj.icon) { obj.icon = true; }
			dom.children("a").children(".jstree-themeicon").removeClass('jstree-themeicon-hidden');
			return true;
		}
	};

	// helpers
	$.vakata = {};
	// reverse
	$.fn.vakata_reverse = [].reverse;
	// collect attributes
	$.vakata.attributes = function(node, with_values) {
		node = $(node)[0];
		var attr = with_values ? {} : [];
		if(node && node.attributes) {
			$.each(node.attributes, function (i, v) {
				if($.inArray(v.nodeName.toLowerCase(),['style','contenteditable','hasfocus','tabindex']) !== -1) { return; }
				if(v.nodeValue !== null && $.trim(v.nodeValue) !== '') {
					if(with_values) { attr[v.nodeName] = v.nodeValue; }
					else { attr.push(v.nodeName); }
				}
			});
		}
		return attr;
	};
	$.vakata.array_unique = function(array) {
		var a = [], i, j, l;
		for(i = 0, l = array.length; i < l; i++) {
			for(j = 0; j <= i; j++) {
				if(array[i] === array[j]) {
					break;
				}
			}
			if(j === i) { a.push(array[i]); }
		}
		return a;
	};
	// remove item from array
	$.vakata.array_remove = function(array, from, to) {
		var rest = array.slice((to || from) + 1 || array.length);
		array.length = from < 0 ? array.length + from : from;
		array.push.apply(array, rest);
		return array;
	};
	// remove item from array
	$.vakata.array_remove_item = function(array, item) {
		var tmp = $.inArray(item, array);
		return tmp !== -1 ? $.vakata.array_remove(array, tmp) : array;
	};
	// browser sniffing
	(function () {
		var browser = {},
			b_match = function(ua) {
			ua = ua.toLowerCase();

			var match =	/(chrome)[ \/]([\w.]+)/.exec( ua ) ||
						/(webkit)[ \/]([\w.]+)/.exec( ua ) ||
						/(opera)(?:.*version|)[ \/]([\w.]+)/.exec( ua ) ||
						/(msie) ([\w.]+)/.exec( ua ) ||
						(ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec( ua )) ||
						[];
				return {
					browser: match[1] || "",
					version: match[2] || "0"
				};
			},
			matched = b_match(window.navigator.userAgent);
		if(matched.browser) {
			browser[ matched.browser ] = true;
			browser.version = matched.version;
		}
		if(browser.chrome) {
			browser.webkit = true;
		}
		else if(browser.webkit) {
			browser.safari = true;
		}
		$.vakata.browser = browser;
	}());
	if($.vakata.browser.msie && $.vakata.browser.version < 8) {
		$.jstree.defaults.core.animation = 0;
	}

/**
 * ### Checkbox plugin
 *
 * This plugin renders checkbox icons in front of each node, making multiple selection much easier. 
 * It also supports tri-state behavior, meaning that if a node has a few of its children checked it will be rendered as undetermined, and state will be propagated up.
 */

	var _i = document.createElement('I');
	_i.className = 'jstree-icon jstree-checkbox';
	/**
	 * stores all defaults for the checkbox plugin
	 * @name $.jstree.defaults.checkbox
	 * @plugin checkbox
	 */
	$.jstree.defaults.checkbox = {
		/**
		 * a boolean indicating if checkboxes should be visible (can be changed at a later time using `show_checkboxes()` and `hide_checkboxes`). Defaults to `true`.
		 * @name $.jstree.defaults.checkbox.visible
		 * @plugin checkbox
		 */
		visible				: true,
		/**
		 * a boolean indicating if checkboxes should cascade down and have an undetermined state. Defaults to `true`.
		 * @name $.jstree.defaults.checkbox.three_state
		 * @plugin checkbox
		 */
		three_state			: true,
		/**
		 * a boolean indicating if clicking anywhere on the node should act as clicking on the checkbox. Defaults to `true`.
		 * @name $.jstree.defaults.checkbox.whole_node
		 * @plugin checkbox
		 */
		whole_node			: true,
		/**
		 * a boolean indicating if the selected style of a node should be kept, or removed. Defaults to `true`.
		 * @name $.jstree.defaults.checkbox.keep_selected_style
		 * @plugin checkbox
		 */
		keep_selected_style	: true
	};
	$.jstree.plugins.checkbox = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);
			this._data.checkbox.uto = false;
			this.element
				.on("init.jstree", $.proxy(function () {
						this._data.checkbox.visible = this.settings.checkbox.visible;
						if(!this.settings.checkbox.keep_selected_style) {
							this.element.addClass('jstree-checkbox-no-clicked');
						}
					}, this))
				.on("loading.jstree", $.proxy(function () {
						this[ this._data.checkbox.visible ? 'show_checkboxes' : 'hide_checkboxes' ]();
					}, this));
			if(this.settings.checkbox.three_state) {
				this.element
					.on('changed.jstree move_node.jstree copy_node.jstree redraw.jstree open_node.jstree', $.proxy(function () {
							if(this._data.checkbox.uto) { clearTimeout(this._data.checkbox.uto); }
							this._data.checkbox.uto = setTimeout($.proxy(this._undetermined, this), 50);
						}, this))
					.on('model.jstree', $.proxy(function (e, data) {
							var m = this._model.data,
								p = m[data.parent],
								dpc = data.nodes,
								chd = [],
								c, i, j, k, l, tmp;

							// apply down
							if(p.state.selected) {
								for(i = 0, j = dpc.length; i < j; i++) {
									m[dpc[i]].state.selected = true;
								}
								this._data.core.selected = this._data.core.selected.concat(dpc);
							}
							else {
								for(i = 0, j = dpc.length; i < j; i++) {
									if(m[dpc[i]].state.selected) {
										for(k = 0, l = m[dpc[i]].children_d.length; k < l; k++) {
											m[m[dpc[i]].children_d[k]].state.selected = true;
										}
										this._data.core.selected = this._data.core.selected.concat(m[dpc[i]].children_d);
									}
								}
							}

							// apply up
							for(i = 0, j = p.children_d.length; i < j; i++) {
								if(!m[p.children_d[i]].children.length) {
									chd.push(m[p.children_d[i]].parent);
								}
							}
							chd = $.vakata.array_unique(chd);
							for(k = 0, l = chd.length; k < l; k++) {
								p = m[chd[k]];
								while(p && p.id !== '#') {
									c = 0;
									for(i = 0, j = p.children.length; i < j; i++) {
										c += m[p.children[i]].state.selected;
									}
									if(c === j) {
										p.state.selected = true;
										this._data.core.selected.push(p.id);
										tmp = this.get_node(p, true);
										if(tmp && tmp.length) {
											tmp.children('.jstree-anchor').addClass('jstree-clicked');
										}
									}
									else {
										break;
									}
									p = this.get_node(p.parent);
								}
							}
							this._data.core.selected = $.vakata.array_unique(this._data.core.selected);
						}, this))
					.on('select_node.jstree', $.proxy(function (e, data) {
							var obj = data.node,
								m = this._model.data,
								par = this.get_node(obj.parent),
								dom = this.get_node(obj, true),
								i, j, c, tmp;
							this._data.core.selected = $.vakata.array_unique(this._data.core.selected.concat(obj.children_d));
							for(i = 0, j = obj.children_d.length; i < j; i++) {
								m[obj.children_d[i]].state.selected = true;
							}
							while(par && par.id !== '#') {
								c = 0;
								for(i = 0, j = par.children.length; i < j; i++) {
									c += m[par.children[i]].state.selected;
								}
								if(c === j) {
									par.state.selected = true;
									this._data.core.selected.push(par.id);
									tmp = this.get_node(par, true);
									if(tmp && tmp.length) {
										tmp.children('.jstree-anchor').addClass('jstree-clicked');
									}
								}
								else {
									break;
								}
								par = this.get_node(par.parent);
							}
							if(dom.length) {
								dom.find('.jstree-anchor').addClass('jstree-clicked');
							}
						}, this))
					.on('deselect_node.jstree', $.proxy(function (e, data) {
							var obj = data.node,
								dom = this.get_node(obj, true),
								i, j, tmp;
							for(i = 0, j = obj.children_d.length; i < j; i++) {
								this._model.data[obj.children_d[i]].state.selected = false;
							}
							for(i = 0, j = obj.parents.length; i < j; i++) {
								this._model.data[obj.parents[i]].state.selected = false;
								tmp = this.get_node(obj.parents[i], true);
								if(tmp && tmp.length) {
									tmp.children('.jstree-anchor').removeClass('jstree-clicked');
								}
							}
							tmp = [];
							for(i = 0, j = this._data.core.selected.length; i < j; i++) {
								if($.inArray(this._data.core.selected[i], obj.children_d) === -1 && $.inArray(this._data.core.selected[i], obj.parents) === -1) {
									tmp.push(this._data.core.selected[i]);
								}
							}
							this._data.core.selected = $.vakata.array_unique(tmp);
							if(dom.length) {
								dom.find('.jstree-anchor').removeClass('jstree-clicked');
							}
						}, this))
					.on('delete_node.jstree', $.proxy(function (e, data) {
							var p = this.get_node(data.parent),
								m = this._model.data,
								i, j, c, tmp;
							while(p && p.id !== '#') {
								c = 0;
								for(i = 0, j = p.children.length; i < j; i++) {
									c += m[p.children[i]].state.selected;
								}
								if(c === j) {
									p.state.selected = true;
									this._data.core.selected.push(p.id);
									tmp = this.get_node(p, true);
									if(tmp && tmp.length) {
										tmp.children('.jstree-anchor').addClass('jstree-clicked');
									}
								}
								else {
									break;
								}
								p = this.get_node(p.parent);
							}
						}, this))
					.on('move_node.jstree', $.proxy(function (e, data) {
							var is_multi = data.is_multi,
								old_par = data.old_parent,
								new_par = this.get_node(data.parent),
								m = this._model.data,
								p, c, i, j, tmp;
							if(!is_multi) {
								p = this.get_node(old_par);
								while(p && p.id !== '#') {
									c = 0;
									for(i = 0, j = p.children.length; i < j; i++) {
										c += m[p.children[i]].state.selected;
									}
									if(c === j) {
										p.state.selected = true;
										this._data.core.selected.push(p.id);
										tmp = this.get_node(p, true);
										if(tmp && tmp.length) {
											tmp.children('.jstree-anchor').addClass('jstree-clicked');
										}
									}
									else {
										break;
									}
									p = this.get_node(p.parent);
								}
							}
							p = new_par;
							while(p && p.id !== '#') {
								c = 0;
								for(i = 0, j = p.children.length; i < j; i++) {
									c += m[p.children[i]].state.selected;
								}
								if(c === j) {
									if(!p.state.selected) {
										p.state.selected = true;
										this._data.core.selected.push(p.id);
										tmp = this.get_node(p, true);
										if(tmp && tmp.length) {
											tmp.children('.jstree-anchor').addClass('jstree-clicked');
										}
									}
								}
								else {
									if(p.state.selected) {
										p.state.selected = false;
										this._data.core.selected = $.vakata.array_remove_item(this._data.core.selected, p.id);
										tmp = this.get_node(p, true);
										if(tmp && tmp.length) {
											tmp.children('.jstree-anchor').removeClass('jstree-clicked');
										}
									}
									else {
										break;
									}
								}
								p = this.get_node(p.parent);
							}
						}, this));
			}
		};
		/**
		 * set the undetermined state where and if necessary. Used internally.
		 * @private
		 * @name _undetermined()
		 * @plugin checkbox
		 */
		this._undetermined = function () {
			var i, j, m = this._model.data, s = this._data.core.selected, p = [], t = this;
			for(i = 0, j = s.length; i < j; i++) {
				if(m[s[i]] && m[s[i]].parents) {
					p = p.concat(m[s[i]].parents);
				}
			}
			// attempt for server side undetermined state
			this.element.find('.jstree-closed').not(':has(ul)')
				.each(function () {
					var tmp = t.get_node(this);
					if(!tmp.state.loaded && tmp.original && tmp.original.state && tmp.original.state.undetermined && tmp.original.state.undetermined === true) {
						p.push(tmp.id);
						p = p.concat(tmp.parents);
					}
				});
			p = $.vakata.array_unique(p);
			i = $.inArray('#', p);
			if(i !== -1) {
				p = $.vakata.array_remove(p, i);
			}

			this.element.find('.jstree-undetermined').removeClass('jstree-undetermined');
			for(i = 0, j = p.length; i < j; i++) {
				if(!m[p[i]].state.selected) {
					s = this.get_node(p[i], true);
					if(s && s.length) {
						s.children('a').children('.jstree-checkbox').addClass('jstree-undetermined');
					}
				}
			}
		};
		this.redraw_node = function(obj, deep, is_callback) {
			obj = parent.redraw_node.call(this, obj, deep, is_callback);
			if(obj) {
				var tmp = obj.getElementsByTagName('A')[0];
				tmp.insertBefore(_i.cloneNode(), tmp.childNodes[0]);
			}
			if(!is_callback && this.settings.checkbox.three_state) {
				if(this._data.checkbox.uto) { clearTimeout(this._data.checkbox.uto); }
				this._data.checkbox.uto = setTimeout($.proxy(this._undetermined, this), 50);
			}
			return obj;
		};
		this.activate_node = function (obj, e) {
			if(this.settings.checkbox.whole_node || $(e.target).hasClass('jstree-checkbox')) {
				e.ctrlKey = true;
			}
			return parent.activate_node.call(this, obj, e);
		};
		/**
		 * show the node checkbox icons
		 * @name show_checkboxes()
		 * @plugin checkbox
		 */
		this.show_checkboxes = function () { this._data.core.themes.checkboxes = true; this.element.children("ul").removeClass("jstree-no-checkboxes"); };
		/**
		 * hide the node checkbox icons
		 * @name hide_checkboxes()
		 * @plugin checkbox
		 */
		this.hide_checkboxes = function () { this._data.core.themes.checkboxes = false; this.element.children("ul").addClass("jstree-no-checkboxes"); };
		/**
		 * toggle the node icons
		 * @name toggle_checkboxes()
		 * @plugin checkbox
		 */
		this.toggle_checkboxes = function () { if(this._data.core.themes.checkboxes) { this.hide_checkboxes(); } else { this.show_checkboxes(); } };
	};

	// include the checkbox plugin by default
	// $.jstree.defaults.plugins.push("checkbox");

/**
 * ### Contextmenu plugin
 *
 * Shows a context menu when a node is right-clicked.
 */
// TODO: move logic outside of function + check multiple move

	/**
	 * stores all defaults for the contextmenu plugin
	 * @name $.jstree.defaults.contextmenu
	 * @plugin contextmenu
	 */
	$.jstree.defaults.contextmenu = {
		/**
		 * a boolean indicating if the node should be selected when the context menu is invoked on it. Defaults to `true`.
		 * @name $.jstree.defaults.contextmenu.select_node
		 * @plugin contextmenu
		 */
		select_node : true,
		/**
		 * a boolean indicating if the menu should be shown aligned with the node. Defaults to `true`, otherwise the mouse coordinates are used.
		 * @name $.jstree.defaults.contextmenu.show_at_node
		 * @plugin contextmenu
		 */
		show_at_node : true,
		/**
		 * an object of actions, or a function that accepts a node and a callback function and calls the callback function with an object of actions available for that node (you can also return the items too).
		 * 
		 * Each action consists of a key (a unique name) and a value which is an object with the following properties (only label and action are required):
		 * 
		 * * `separator_before` - a boolean indicating if there should be a separator before this item
		 * * `separator_after` - a boolean indicating if there should be a separator after this item
		 * * `_disabled` - a boolean indicating if this action should be disabled
		 * * `label` - a string - the name of the action
		 * * `action` - a function to be executed if this item is chosen
		 * * `icon` - a string, can be a path to an icon or a className, if using an image that is in the current directory use a `./` prefix, otherwise it will be detected as a class
		 * * `shortcut` - keyCode which will trigger the action if the menu is open (for example `113` for rename, which equals F2)
		 * * `shortcut_label` - shortcut label (like for example `F2` for rename)
		 * 
		 * @name $.jstree.defaults.contextmenu.items
		 * @plugin contextmenu
		 */
		items : function (o, cb) { // Could be an object directly
			return {
				"create" : {
					"separator_before"	: false,
					"separator_after"	: true,
					"_disabled"			: false, //(this.check("create_node", data.reference, {}, "last")),
					"label"				: "Create",
					"action"			: function (data) {
						var inst = $.jstree.reference(data.reference),
							obj = inst.get_node(data.reference);
						inst.create_node(obj, {}, "last", function (new_node) {
							setTimeout(function () { inst.edit(new_node); },0);
						});
					}
				},
				"rename" : {
					"separator_before"	: false,
					"separator_after"	: false,
					"_disabled"			: false, //(this.check("rename_node", data.reference, this.get_parent(data.reference), "")),
					"label"				: "Rename",
					/*
					"shortcut"			: 113,
					"shortcut_label"	: 'F2',
					"icon"				: "glyphicon glyphicon-leaf",
					*/
					"action"			: function (data) {
						var inst = $.jstree.reference(data.reference),
							obj = inst.get_node(data.reference);
						inst.edit(obj);
					}
				},
				"remove" : {
					"separator_before"	: false,
					"icon"				: false,
					"separator_after"	: false,
					"_disabled"			: false, //(this.check("delete_node", data.reference, this.get_parent(data.reference), "")),
					"label"				: "Delete",
					"action"			: function (data) {
						var inst = $.jstree.reference(data.reference),
							obj = inst.get_node(data.reference);
						if(inst.is_selected(obj)) {
							inst.delete_node(inst.get_selected());
						}
						else {
							inst.delete_node(obj);
						}
					}
				},
				"ccp" : {
					"separator_before"	: true,
					"icon"				: false,
					"separator_after"	: false,
					"label"				: "Edit",
					"action"			: false,
					"submenu" : {
						"cut" : {
							"separator_before"	: false,
							"separator_after"	: false,
							"label"				: "Cut",
							"action"			: function (data) {
								var inst = $.jstree.reference(data.reference),
									obj = inst.get_node(data.reference);
								if(inst.is_selected(obj)) {
									inst.cut(inst.get_selected());
								}
								else {
									inst.cut(obj);
								}
							}
						},
						"copy" : {
							"separator_before"	: false,
							"icon"				: false,
							"separator_after"	: false,
							"label"				: "Copy",
							"action"			: function (data) {
								var inst = $.jstree.reference(data.reference),
									obj = inst.get_node(data.reference);
								if(inst.is_selected(obj)) {
									inst.copy(inst.get_selected());
								}
								else {
									inst.copy(obj);
								}
							}
						},
						"paste" : {
							"separator_before"	: false,
							"icon"				: false,
							"_disabled"			: function (data) {
								return !$.jstree.reference(data.reference).can_paste();
							},
							"separator_after"	: false,
							"label"				: "Paste",
							"action"			: function (data) {
								var inst = $.jstree.reference(data.reference),
									obj = inst.get_node(data.reference);
								inst.paste(obj);
							}
						}
					}
				}
			};
		}
	};

	$.jstree.plugins.contextmenu = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);

			this.element
				.on("contextmenu.jstree", ".jstree-anchor", $.proxy(function (e) {
						e.preventDefault();
						if(!this.is_loading(e.currentTarget)) {
							this.show_contextmenu(e.currentTarget, e.pageX, e.pageY, e);
						}
					}, this))
				.on("click.jstree", ".jstree-anchor", $.proxy(function (e) {
						if(this._data.contextmenu.visible) {
							$.vakata.context.hide();
						}
					}, this));
			/*
			if(!('oncontextmenu' in document.body) && ('ontouchstart' in document.body)) {
				var el = null, tm = null;
				this.element
					.on("touchstart", ".jstree-anchor", function (e) {
						el = e.currentTarget;
						tm = +new Date();
						$(document).one("touchend", function (e) {
							e.target = document.elementFromPoint(e.originalEvent.targetTouches[0].pageX - window.pageXOffset, e.originalEvent.targetTouches[0].pageY - window.pageYOffset);
							e.currentTarget = e.target;
							tm = ((+(new Date())) - tm);
							if(e.target === el && tm > 600 && tm < 1000) {
								e.preventDefault();
								$(el).trigger('contextmenu', e);
							}
							el = null;
							tm = null;
						});
					});
			}
			*/
			$(document).on("context_hide.vakata", $.proxy(function () { this._data.contextmenu.visible = false; }, this));
		};
		this.teardown = function () {
			if(this._data.contextmenu.visible) {
				$.vakata.context.hide();
			}
			parent.teardown.call(this);
		};

		/**
		 * prepare and show the context menu for a node
		 * @name show_contextmenu(obj [, x, y])
		 * @param {mixed} obj the node
		 * @param {Number} x the x-coordinate relative to the document to show the menu at
		 * @param {Number} y the y-coordinate relative to the document to show the menu at
		 * @param {Object} e the event if available that triggered the contextmenu
		 * @plugin contextmenu
		 * @trigger show_contextmenu.jstree
		 */
		this.show_contextmenu = function (obj, x, y, e) {
			obj = this.get_node(obj);
			if(!obj || obj.id === '#') { return false; }
			var s = this.settings.contextmenu,
				d = this.get_node(obj, true),
				a = d.children(".jstree-anchor"),
				o = false,
				i = false;
			if(s.show_at_node || x === undefined || y === undefined) {
				o = a.offset();
				x = o.left;
				y = o.top + this._data.core.li_height;
			}
			if(this.settings.contextmenu.select_node && !this.is_selected(obj)) {
				this.deselect_all();
				this.select_node(obj, false, false, e);
			}

			i = s.items;
			if($.isFunction(i)) {
				i = i.call(this, obj, $.proxy(function (i) {
					this._show_contextmenu(obj, x, y, i);
				}, this));
			}
			if($.isPlainObject(i)) {
				this._show_contextmenu(obj, x, y, i);
			}
		};
		/**
		 * show the prepared context menu for a node
		 * @name _show_contextmenu(obj, x, y, i)
		 * @param {mixed} obj the node
		 * @param {Number} x the x-coordinate relative to the document to show the menu at
		 * @param {Number} y the y-coordinate relative to the document to show the menu at
		 * @param {Number} i the object of items to show
		 * @plugin contextmenu
		 * @trigger show_contextmenu.jstree
		 * @private
		 */
		this._show_contextmenu = function (obj, x, y, i) {
			var d = this.get_node(obj, true),
				a = d.children(".jstree-anchor");
			$(document).one("context_show.vakata", $.proxy(function (e, data) {
				var cls = 'jstree-contextmenu jstree-' + this.get_theme() + '-contextmenu';
				$(data.element).addClass(cls);
			}, this));
			this._data.contextmenu.visible = true;
			$.vakata.context.show(a, { 'x' : x, 'y' : y }, i);
			/**
			 * triggered when the contextmenu is shown for a node
			 * @event
			 * @name show_contextmenu.jstree
			 * @param {Object} node the node
			 * @param {Number} x the x-coordinate of the menu relative to the document
			 * @param {Number} y the y-coordinate of the menu relative to the document
			 * @plugin contextmenu
			 */
			this.trigger('show_contextmenu', { "node" : obj, "x" : x, "y" : y });
		};
	};

	// contextmenu helper
	(function ($) {
		var right_to_left = false,
			vakata_context = {
				element		: false,
				reference	: false,
				position_x	: 0,
				position_y	: 0,
				items		: [],
				html		: "",
				is_visible	: false
			};

		$.vakata.context = {
			settings : {
				hide_onmouseleave	: 0,
				icons				: true
			},
			_trigger : function (event_name) {
				$(document).triggerHandler("context_" + event_name + ".vakata", {
					"reference"	: vakata_context.reference,
					"element"	: vakata_context.element,
					"position"	: {
						"x" : vakata_context.position_x,
						"y" : vakata_context.position_y
					}
				});
			},
			_execute : function (i) {
				i = vakata_context.items[i];
				return i && (!i._disabled || ($.isFunction(i._disabled) && !i._disabled({ "item" : i, "reference" : vakata_context.reference, "element" : vakata_context.element }))) && i.action ? i.action.call(null, {
							"item"		: i,
							"reference"	: vakata_context.reference,
							"element"	: vakata_context.element,
							"position"	: {
								"x" : vakata_context.position_x,
								"y" : vakata_context.position_y
							}
						}) : false;
			},
			_parse : function (o, is_callback) {
				if(!o) { return false; }
				if(!is_callback) {
					vakata_context.html		= "";
					vakata_context.items	= [];
				}
				var str = "",
					sep = false,
					tmp;

				if(is_callback) { str += "<"+"ul>"; }
				$.each(o, function (i, val) {
					if(!val) { return true; }
					vakata_context.items.push(val);
					if(!sep && val.separator_before) {
						str += "<"+"li class='vakata-context-separator'><"+"a href='#' " + ($.vakata.context.settings.icons ? '' : 'style="margin-left:0px;"') + ">&#160;<"+"/a><"+"/li>";
					}
					sep = false;
					str += "<"+"li class='" + (val._class || "") + (val._disabled === true || ($.isFunction(val._disabled) && val._disabled({ "item" : val, "reference" : vakata_context.reference, "element" : vakata_context.element })) ? " vakata-contextmenu-disabled " : "") + "' "+(val.shortcut?" data-shortcut='"+val.shortcut+"' ":'')+">";
					str += "<"+"a href='#' rel='" + (vakata_context.items.length - 1) + "'>";
					if($.vakata.context.settings.icons) {
						str += "<"+"i ";
						if(val.icon) {
							if(val.icon.indexOf("/") !== -1 || val.icon.indexOf(".") !== -1) { str += " style='background:url(\"" + val.icon + "\") center center no-repeat' "; }
							else { str += " class='" + val.icon + "' "; }
						}
						str += "><"+"/i><"+"span class='vakata-contextmenu-sep'>&#160;<"+"/span>";
					}
					str += val.label + (val.shortcut?' <span class="vakata-contextmenu-shortcut vakata-contextmenu-shortcut-'+val.shortcut+'">'+ (val.shortcut_label || '') +'</span>':'') + "<"+"/a>";
					if(val.submenu) {
						tmp = $.vakata.context._parse(val.submenu, true);
						if(tmp) { str += tmp; }
					}
					str += "<"+"/li>";
					if(val.separator_after) {
						str += "<"+"li class='vakata-context-separator'><"+"a href='#' " + ($.vakata.context.settings.icons ? '' : 'style="margin-left:0px;"') + ">&#160;<"+"/a><"+"/li>";
						sep = true;
					}
				});
				str  = str.replace(/<li class\='vakata-context-separator'\><\/li\>$/,"");
				if(is_callback) { str += "</ul>"; }
				/**
				 * triggered on the document when the contextmenu is parsed (HTML is built)
				 * @event
				 * @plugin contextmenu
				 * @name context_parse.vakata
				 * @param {jQuery} reference the element that was right clicked
				 * @param {jQuery} element the DOM element of the menu itself
				 * @param {Object} position the x & y coordinates of the menu
				 */
				if(!is_callback) { vakata_context.html = str; $.vakata.context._trigger("parse"); }
				return str.length > 10 ? str : false;
			},
			_show_submenu : function (o) {
				o = $(o);
				if(!o.length || !o.children("ul").length) { return; }
				var e = o.children("ul"),
					x = o.offset().left + o.outerWidth(),
					y = o.offset().top,
					w = e.width(),
					h = e.height(),
					dw = $(window).width() + $(window).scrollLeft(),
					dh = $(window).height() + $(window).scrollTop();
				// може да се спести е една проверка - дали няма някой от класовете вече нагоре
				if(right_to_left) {
					o[x - (w + 10 + o.outerWidth()) < 0 ? "addClass" : "removeClass"]("vakata-context-left");
				}
				else {
					o[x + w + 10 > dw ? "addClass" : "removeClass"]("vakata-context-right");
				}
				if(y + h + 10 > dh) {
					e.css("bottom","-1px");
				}
				e.show();
			},
			show : function (reference, position, data) {
				var o, e, x, y, w, h, dw, dh, cond = true;
				if(vakata_context.element && vakata_context.element.length) {
					vakata_context.element.width('');
				}
				switch(cond) {
					case (!position && !reference):
						return false;
					case (!!position && !!reference):
						vakata_context.reference	= reference;
						vakata_context.position_x	= position.x;
						vakata_context.position_y	= position.y;
						break;
					case (!position && !!reference):
						vakata_context.reference	= reference;
						o = reference.offset();
						vakata_context.position_x	= o.left + reference.outerHeight();
						vakata_context.position_y	= o.top;
						break;
					case (!!position && !reference):
						vakata_context.position_x	= position.x;
						vakata_context.position_y	= position.y;
						break;
				}
				if(!!reference && !data && $(reference).data('vakata_contextmenu')) {
					data = $(reference).data('vakata_contextmenu');
				}
				if($.vakata.context._parse(data)) {
					vakata_context.element.html(vakata_context.html);
				}
				if(vakata_context.items.length) {
					e = vakata_context.element;
					x = vakata_context.position_x;
					y = vakata_context.position_y;
					w = e.width();
					h = e.height();
					dw = $(window).width() + $(window).scrollLeft();
					dh = $(window).height() + $(window).scrollTop();
					if(right_to_left) {
						x -= e.outerWidth();
						if(x < $(window).scrollLeft() + 20) {
							x = $(window).scrollLeft() + 20;
						}
					}
					if(x + w + 20 > dw) {
						x = dw - (w + 20);
					}
					if(y + h + 20 > dh) {
						y = dh - (h + 20);
					}

					vakata_context.element
						.css({ "left" : x, "top" : y })
						.show()
						.find('a:eq(0)').focus().parent().addClass("vakata-context-hover");
					vakata_context.is_visible = true;
					/**
					 * triggered on the document when the contextmenu is shown
					 * @event
					 * @plugin contextmenu
					 * @name context_show.vakata
					 * @param {jQuery} reference the element that was right clicked
					 * @param {jQuery} element the DOM element of the menu itself
					 * @param {Object} position the x & y coordinates of the menu
					 */
					$.vakata.context._trigger("show");
				}
			},
			hide : function () {
				if(vakata_context.is_visible) {
					vakata_context.element.hide().find("ul").hide().end().find(':focus').blur();
					vakata_context.is_visible = false;
					/**
					 * triggered on the document when the contextmenu is hidden
					 * @event
					 * @plugin contextmenu
					 * @name context_hide.vakata
					 * @param {jQuery} reference the element that was right clicked
					 * @param {jQuery} element the DOM element of the menu itself
					 * @param {Object} position the x & y coordinates of the menu
					 */
					$.vakata.context._trigger("hide");
				}
			}
		};
		$(function () {
			right_to_left = $("body").css("direction") === "rtl";
			var to = false;

			vakata_context.element = $("<ul class='vakata-context'></ul>");
			vakata_context.element
				.on("mouseenter", "li", function (e) {
					e.stopImmediatePropagation();

					if($.contains(this, e.relatedTarget)) {
						// премахнато заради delegate mouseleave по-долу
						// $(this).find(".vakata-context-hover").removeClass("vakata-context-hover");
						return;
					}

					if(to) { clearTimeout(to); }
					vakata_context.element.find(".vakata-context-hover").removeClass("vakata-context-hover").end();

					$(this)
						.siblings().find("ul").hide().end().end()
						.parentsUntil(".vakata-context", "li").addBack().addClass("vakata-context-hover");
					$.vakata.context._show_submenu(this);
				})
				// тестово - дали не натоварва?
				.on("mouseleave", "li", function (e) {
					if($.contains(this, e.relatedTarget)) { return; }
					$(this).find(".vakata-context-hover").addBack().removeClass("vakata-context-hover");
				})
				.on("mouseleave", function (e) {
					$(this).find(".vakata-context-hover").removeClass("vakata-context-hover");
					if($.vakata.context.settings.hide_onmouseleave) {
						to = setTimeout(
							(function (t) {
								return function () { $.vakata.context.hide(); };
							}(this)), $.vakata.context.settings.hide_onmouseleave);
					}
				})
				.on("click", "a", function (e) {
					e.preventDefault();
				})
				.on("mouseup", "a", function (e) {
					if(!$(this).blur().parent().hasClass("vakata-context-disabled") && $.vakata.context._execute($(this).attr("rel")) !== false) {
						$.vakata.context.hide();
					}
				})
				.on('keydown', 'a', function (e) {
						var o = null;
						switch(e.which) {
							case 13:
							case 32:
								e.type = "mouseup";
								e.preventDefault();
								$(e.currentTarget).trigger(e);
								break;
							case 37:
								if(vakata_context.is_visible) {
									vakata_context.element.find(".vakata-context-hover").last().parents("li:eq(0)").find("ul").hide().find(".vakata-context-hover").removeClass("vakata-context-hover").end().end().children('a').focus();
									e.stopImmediatePropagation();
									e.preventDefault();
								}
								break;
							case 38:
								if(vakata_context.is_visible) {
									o = vakata_context.element.find("ul:visible").addBack().last().children(".vakata-context-hover").removeClass("vakata-context-hover").prevAll("li:not(.vakata-context-separator)").first();
									if(!o.length) { o = vakata_context.element.find("ul:visible").addBack().last().children("li:not(.vakata-context-separator)").last(); }
									o.addClass("vakata-context-hover").children('a').focus();
									e.stopImmediatePropagation();
									e.preventDefault();
								}
								break;
							case 39:
								if(vakata_context.is_visible) {
									vakata_context.element.find(".vakata-context-hover").last().children("ul").show().children("li:not(.vakata-context-separator)").removeClass("vakata-context-hover").first().addClass("vakata-context-hover").children('a').focus();
									e.stopImmediatePropagation();
									e.preventDefault();
								}
								break;
							case 40:
								if(vakata_context.is_visible) {
									o = vakata_context.element.find("ul:visible").addBack().last().children(".vakata-context-hover").removeClass("vakata-context-hover").nextAll("li:not(.vakata-context-separator)").first();
									if(!o.length) { o = vakata_context.element.find("ul:visible").addBack().last().children("li:not(.vakata-context-separator)").first(); }
									o.addClass("vakata-context-hover").children('a').focus();
									e.stopImmediatePropagation();
									e.preventDefault();
								}
								break;
							case 27:
								$.vakata.context.hide();
								e.preventDefault();
								break;
							default:
								//console.log(e.which);
								break;
						}
					})
				.on('keydown', function (e) {
					e.preventDefault();
					var a = vakata_context.element.find('.vakata-contextmenu-shortcut-' + e.which).parent();
					if(a.parent().not('.vakata-context-disabled')) {
						a.mouseup();
					}
				})
				.appendTo("body");

			$(document)
				.on("mousedown", function (e) {
					if(vakata_context.is_visible && !$.contains(vakata_context.element[0], e.target)) { $.vakata.context.hide(); }
				})
				.on("context_show.vakata", function (e, data) {
					vakata_context.element.find("li:has(ul)").children("a").addClass("vakata-context-parent");
					if(right_to_left) {
						vakata_context.element.addClass("vakata-context-rtl").css("direction", "rtl");
					}
					// also apply a RTL class?
					vakata_context.element.find("ul").hide().end();
				});
		});
	}($));
	// $.jstree.defaults.plugins.push("contextmenu");

/**
 * ### Drag'n'drop plugin
 *
 * Enables dragging and dropping of nodes in the tree, resulting in a move or copy operations.
 */

	/**
	 * stores all defaults for the drag'n'drop plugin
	 * @name $.jstree.defaults.dnd
	 * @plugin dnd
	 */
	$.jstree.defaults.dnd = {
		/**
		 * a boolean indicating if a copy should be possible while dragging (by pressint the meta key or Ctrl). Defaults to `true`.
		 * @name $.jstree.defaults.dnd.copy
		 * @plugin dnd
		 */
		copy : true,
		/**
		 * a number indicating how long a node should remain hovered while dragging to be opened. Defaults to `500`.
		 * @name $.jstree.defaults.dnd.open_timeout
		 * @plugin dnd
		 */
		open_timeout : 500,
		/**
		 * a function invoked each time a node is about to be dragged, invoked in the tree's scope and receives the node as an argument - return `false` to prevent dragging
		 * @name $.jstree.defaults.dnd.is_draggable
		 * @plugin dnd
		 */
		is_draggable : true,
		/**
		 * a boolean indicating if checks should constantly be made while the user is dragging the node (as opposed to checking only on drop), default is `true`
		 * @name $.jstree.defaults.dnd.check_while_dragging
		 * @plugin dnd
		 */
		check_while_dragging : true
	};
	// TODO: now check works by checking for each node individually, how about max_children, unique, etc?
	// TODO: drop somewhere else - maybe demo only?
	$.jstree.plugins.dnd = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);

			this.element
				.on('mousedown touchstart', '.jstree-anchor', $.proxy(function (e) {
					var obj = this.get_node(e.target),
						mlt = this.is_selected(obj) ? this.get_selected().length : 1;
					if(obj && obj.id && obj.id !== "#" && (e.which === 1 || e.type === "touchstart") &&
						(this.settings.dnd.is_draggable === true || ($.isFunction(this.settings.dnd.is_draggable) && this.settings.dnd.is_draggable.call(this, obj)))
					) {
						this.element.trigger('mousedown.jstree');
						return $.vakata.dnd.start(e, { 'jstree' : true, 'origin' : this, 'obj' : this.get_node(obj,true), 'nodes' : mlt > 1 ? this.get_selected() : [obj.id] }, '<div id="jstree-dnd" class="jstree-' + this.get_theme() + '"><i class="jstree-icon jstree-er"></i>' + (mlt > 1 ? mlt + ' ' + this.get_string('nodes') : this.get_text(e.currentTarget, true)) + '<ins class="jstree-copy" style="display:none;">+</ins></div>');
					}
				}, this));
		};
	};

	$(function() {
		// bind only once for all instances
		var lastmv = false,
			laster = false,
			opento = false,
			marker = $('<div id="jstree-marker">&#160;</div>').hide().appendTo('body');

		$(document)
			.bind('dnd_start.vakata', function (e, data) {
				lastmv = false;
			})
			.bind('dnd_move.vakata', function (e, data) {
				if(opento) { clearTimeout(opento); }
				if(!data.data.jstree) { return; }

				// if we are hovering the marker image do nothing (can happen on "inside" drags)
				if(data.event.target.id && data.event.target.id === 'jstree-marker') {
					return;
				}

				var ins = $.jstree.reference(data.event.target),
					ref = false,
					off = false,
					rel = false,
					l, t, h, p, i, o, ok, t1, t2, op, ps, pr;
				// if we are over an instance
				if(ins && ins._data && ins._data.dnd) {
					marker.attr('class', 'jstree-' + ins.get_theme());
					data.helper
						.children().attr('class', 'jstree-' + ins.get_theme())
						.find('.jstree-copy:eq(0)')[ data.data.origin && data.data.origin.settings.dnd.copy && (data.event.metaKey || data.event.ctrlKey) ? 'show' : 'hide' ]();


					// if are hovering the container itself add a new root node
					if( (data.event.target === ins.element[0] || data.event.target === ins.get_container_ul()[0]) && ins.get_container_ul().children().length === 0) {
						ok = true;
						for(t1 = 0, t2 = data.data.nodes.length; t1 < t2; t1++) {
							ok = ok && ins.check( (data.data.origin && data.data.origin.settings.dnd.copy && (data.event.metaKey || data.event.ctrlKey) ? "copy_node" : "move_node"), (data.data.origin && data.data.origin !== ins ? data.data.origin.get_node(data.data.nodes[t1]) : data.data.nodes[t1]), '#', 'last');
							if(!ok) { break; }
						}
						if(ok) {
							lastmv = { 'ins' : ins, 'par' : '#', 'pos' : 'last' };
							marker.hide();
							data.helper.find('.jstree-icon:eq(0)').removeClass('jstree-er').addClass('jstree-ok');
							return;
						}
					}
					else {
						// if we are hovering a tree node
						ref = $(data.event.target).closest('a');
						if(ref && ref.length && ref.parent().is('.jstree-closed, .jstree-open, .jstree-leaf')) {
							off = ref.offset();
							rel = data.event.pageY - off.top;
							h = ref.height();
							if(rel < h / 3) {
								o = ['b', 'i', 'a'];
							}
							else if(rel > h - h / 3) {
								o = ['a', 'i', 'b'];
							}
							else {
								o = rel > h / 2 ? ['i', 'a', 'b'] : ['i', 'b', 'a'];
							}
							$.each(o, function (j, v) {
								switch(v) {
									case 'b':
										l = off.left - 6;
										t = off.top - 5;
										p = ins.get_parent(ref);
										i = ref.parent().index();
										break;
									case 'i':
										l = off.left - 2;
										t = off.top - 5 + h / 2 + 1;
										p = ref.parent();
										i = 0;
										break;
									case 'a':
										l = off.left - 6;
										t = off.top - 5 + h;
										p = ins.get_parent(ref);
										i = ref.parent().index() + 1;
										break;
								}
								/*!
								// TODO: moving inside, but the node is not yet loaded?
								// the check will work anyway, as when moving the node will be loaded first and checked again
								if(v === 'i' && !ins.is_loaded(p)) { }
								*/
								ok = true;
								for(t1 = 0, t2 = data.data.nodes.length; t1 < t2; t1++) {
									op = data.data.origin && data.data.origin.settings.dnd.copy && (data.event.metaKey || data.event.ctrlKey) ? "copy_node" : "move_node";
									ps = i;
									if(op === "move_node" && v === 'a' && (data.data.origin && data.data.origin === ins) && p === ins.get_parent(data.data.nodes[t1])) {
										pr = ins.get_node(p);
										if(ps > $.inArray(data.data.nodes[t1], pr.children)) {
											ps -= 1;
										}
									}
									ok = ok && ( (ins && ins.settings && ins.settings.dnd && ins.settings.dnd.check_while_dragging === false) || ins.check(op, (data.data.origin && data.data.origin !== ins ? data.data.origin.get_node(data.data.nodes[t1]) : data.data.nodes[t1]), p, ps) );
									if(!ok) {
										if(ins && ins.last_error) { laster = ins.last_error(); }
										break;
									}
								}
								if(ok) {
									if(v === 'i' && ref.parent().is('.jstree-closed') && ins.settings.dnd.open_timeout) {
										opento = setTimeout((function (x, z) { return function () { x.open_node(z); }; }(ins, ref)), ins.settings.dnd.open_timeout);
									}
									lastmv = { 'ins' : ins, 'par' : p, 'pos' : i };
									marker.css({ 'left' : l + 'px', 'top' : t + 'px' }).show();
									data.helper.find('.jstree-icon:eq(0)').removeClass('jstree-er').addClass('jstree-ok');
									laster = {};
									o = true;
									return false;
								}
							});
							if(o === true) { return; }
						}
					}
				}
				lastmv = false;
				data.helper.find('.jstree-icon').removeClass('jstree-ok').addClass('jstree-er');
				marker.hide();
			})
			.bind('dnd_scroll.vakata', function (e, data) {
				if(!data.data.jstree) { return; }
				marker.hide();
				lastmv = false;
				data.helper.find('.jstree-icon:eq(0)').removeClass('jstree-ok').addClass('jstree-er');
			})
			.bind('dnd_stop.vakata', function (e, data) {
				if(opento) { clearTimeout(opento); }
				if(!data.data.jstree) { return; }
				marker.hide();
				var i, j, nodes = [];
				if(lastmv) {
					for(i = 0, j = data.data.nodes.length; i < j; i++) {
						nodes[i] = data.data.origin ? data.data.origin.get_node(data.data.nodes[i]) : data.data.nodes[i];
					}
					lastmv.ins[ data.data.origin && data.data.origin.settings.dnd.copy && (data.event.metaKey || data.event.ctrlKey) ? 'copy_node' : 'move_node' ](nodes, lastmv.par, lastmv.pos);
				}
				else {
					i = $(data.event.target).closest('.jstree');
					if(i.length && laster && laster.error && laster.error === 'check') {
						i = i.jstree(true);
						if(i) {
							i.settings.core.error.call(this, laster);
						}
					}
				}
			})
			.bind('keyup keydown', function (e, data) {
				data = $.vakata.dnd._get();
				if(data.data && data.data.jstree) {
					data.helper.find('.jstree-copy:eq(0)')[ data.data.origin && data.data.origin.settings.dnd.copy && (e.metaKey || e.ctrlKey) ? 'show' : 'hide' ]();
				}
			});
	});

	// helpers
	(function ($) {
		$.fn.vakata_reverse = [].reverse;
		// private variable
		var vakata_dnd = {
			element	: false,
			is_down	: false,
			is_drag	: false,
			helper	: false,
			helper_w: 0,
			data	: false,
			init_x	: 0,
			init_y	: 0,
			scroll_l: 0,
			scroll_t: 0,
			scroll_e: false,
			scroll_i: false
		};
		$.vakata.dnd = {
			settings : {
				scroll_speed		: 10,
				scroll_proximity	: 20,
				helper_left			: 5,
				helper_top			: 10,
				threshold			: 5
			},
			_trigger : function (event_name, e) {
				var data = $.vakata.dnd._get();
				data.event = e;
				$(document).triggerHandler("dnd_" + event_name + ".vakata", data);
			},
			_get : function () {
				return {
					"data"		: vakata_dnd.data,
					"element"	: vakata_dnd.element,
					"helper"	: vakata_dnd.helper
				};
			},
			_clean : function () {
				if(vakata_dnd.helper) { vakata_dnd.helper.remove(); }
				if(vakata_dnd.scroll_i) { clearInterval(vakata_dnd.scroll_i); vakata_dnd.scroll_i = false; }
				vakata_dnd = {
					element	: false,
					is_down	: false,
					is_drag	: false,
					helper	: false,
					helper_w: 0,
					data	: false,
					init_x	: 0,
					init_y	: 0,
					scroll_l: 0,
					scroll_t: 0,
					scroll_e: false,
					scroll_i: false
				};
				$(document).off("mousemove touchmove", $.vakata.dnd.drag);
				$(document).off("mouseup touchend", $.vakata.dnd.stop);
			},
			_scroll : function (init_only) {
				if(!vakata_dnd.scroll_e || (!vakata_dnd.scroll_l && !vakata_dnd.scroll_t)) {
					if(vakata_dnd.scroll_i) { clearInterval(vakata_dnd.scroll_i); vakata_dnd.scroll_i = false; }
					return false;
				}
				if(!vakata_dnd.scroll_i) {
					vakata_dnd.scroll_i = setInterval($.vakata.dnd._scroll, 100);
					return false;
				}
				if(init_only === true) { return false; }

				var i = vakata_dnd.scroll_e.scrollTop(),
					j = vakata_dnd.scroll_e.scrollLeft();
				vakata_dnd.scroll_e.scrollTop(i + vakata_dnd.scroll_t * $.vakata.dnd.settings.scroll_speed);
				vakata_dnd.scroll_e.scrollLeft(j + vakata_dnd.scroll_l * $.vakata.dnd.settings.scroll_speed);
				if(i !== vakata_dnd.scroll_e.scrollTop() || j !== vakata_dnd.scroll_e.scrollLeft()) {
					/**
					 * triggered on the document when a drag causes an element to scroll
					 * @event
					 * @plugin dnd
					 * @name dnd_scroll.vakata
					 * @param {Mixed} data any data supplied with the call to $.vakata.dnd.start
					 * @param {DOM} element the DOM element being dragged
					 * @param {jQuery} helper the helper shown next to the mouse
					 * @param {jQuery} event the element that is scrolling
					 */
					$.vakata.dnd._trigger("scroll", vakata_dnd.scroll_e);
				}
			},
			start : function (e, data, html) {
				if(e.type === "touchstart" && e.originalEvent && e.originalEvent.changedTouches && e.originalEvent.changedTouches[0]) {
					e.pageX = e.originalEvent.changedTouches[0].pageX;
					e.pageY = e.originalEvent.changedTouches[0].pageY;
					e.target = document.elementFromPoint(e.originalEvent.changedTouches[0].pageX - window.pageXOffset, e.originalEvent.changedTouches[0].pageY - window.pageYOffset);
				}
				if(vakata_dnd.is_drag) { $.vakata.dnd.stop({}); }
				try {
					e.currentTarget.unselectable = "on";
					e.currentTarget.onselectstart = function() { return false; };
					if(e.currentTarget.style) { e.currentTarget.style.MozUserSelect = "none"; }
				} catch(ignore) { }
				vakata_dnd.init_x	= e.pageX;
				vakata_dnd.init_y	= e.pageY;
				vakata_dnd.data		= data;
				vakata_dnd.is_down	= true;
				vakata_dnd.element	= e.currentTarget;
				if(html !== false) {
					vakata_dnd.helper = $("<div id='vakata-dnd'></div>").html(html).css({
						"display"		: "block",
						"margin"		: "0",
						"padding"		: "0",
						"position"		: "absolute",
						"top"			: "-2000px",
						"lineHeight"	: "16px",
						"zIndex"		: "10000"
					});
				}
				$(document).bind("mousemove touchmove", $.vakata.dnd.drag);
				$(document).bind("mouseup touchend", $.vakata.dnd.stop);
				return false;
			},
			drag : function (e) {
				if(e.type === "touchmove" && e.originalEvent && e.originalEvent.changedTouches && e.originalEvent.changedTouches[0]) {
					e.pageX = e.originalEvent.changedTouches[0].pageX;
					e.pageY = e.originalEvent.changedTouches[0].pageY;
					e.target = document.elementFromPoint(e.originalEvent.changedTouches[0].pageX - window.pageXOffset, e.originalEvent.changedTouches[0].pageY - window.pageYOffset);
				}
				if(!vakata_dnd.is_down) { return; }
				if(!vakata_dnd.is_drag) {
					if(
						Math.abs(e.pageX - vakata_dnd.init_x) > $.vakata.dnd.settings.threshold ||
						Math.abs(e.pageY - vakata_dnd.init_y) > $.vakata.dnd.settings.threshold
					) {
						if(vakata_dnd.helper) {
							vakata_dnd.helper.appendTo("body");
							vakata_dnd.helper_w = vakata_dnd.helper.outerWidth();
						}
						vakata_dnd.is_drag = true;
						/**
						 * triggered on the document when a drag starts
						 * @event
						 * @plugin dnd
						 * @name dnd_start.vakata
						 * @param {Mixed} data any data supplied with the call to $.vakata.dnd.start
						 * @param {DOM} element the DOM element being dragged
						 * @param {jQuery} helper the helper shown next to the mouse
						 * @param {Object} event the event that caused the start (probably mousemove)
						 */
						$.vakata.dnd._trigger("start", e);
					}
					else { return; }
				}

				var d  = false, w  = false,
					dh = false, wh = false,
					dw = false, ww = false,
					dt = false, dl = false,
					ht = false, hl = false;

				vakata_dnd.scroll_t = 0;
				vakata_dnd.scroll_l = 0;
				vakata_dnd.scroll_e = false;
				$(e.target)
					.parentsUntil("body").addBack().vakata_reverse()
					.filter(function () {
						return	(/^auto|scroll$/).test($(this).css("overflow")) &&
								(this.scrollHeight > this.offsetHeight || this.scrollWidth > this.offsetWidth);
					})
					.each(function () {
						var t = $(this), o = t.offset();
						if(this.scrollHeight > this.offsetHeight) {
							if(o.top + t.height() - e.pageY < $.vakata.dnd.settings.scroll_proximity)	{ vakata_dnd.scroll_t = 1; }
							if(e.pageY - o.top < $.vakata.dnd.settings.scroll_proximity)				{ vakata_dnd.scroll_t = -1; }
						}
						if(this.scrollWidth > this.offsetWidth) {
							if(o.left + t.width() - e.pageX < $.vakata.dnd.settings.scroll_proximity)	{ vakata_dnd.scroll_l = 1; }
							if(e.pageX - o.left < $.vakata.dnd.settings.scroll_proximity)				{ vakata_dnd.scroll_l = -1; }
						}
						if(vakata_dnd.scroll_t || vakata_dnd.scroll_l) {
							vakata_dnd.scroll_e = $(this);
							return false;
						}
					});

				if(!vakata_dnd.scroll_e) {
					d  = $(document); w = $(window);
					dh = d.height(); wh = w.height();
					dw = d.width(); ww = w.width();
					dt = d.scrollTop(); dl = d.scrollLeft();
					if(dh > wh && e.pageY - dt < $.vakata.dnd.settings.scroll_proximity)		{ vakata_dnd.scroll_t = -1;  }
					if(dh > wh && wh - (e.pageY - dt) < $.vakata.dnd.settings.scroll_proximity)	{ vakata_dnd.scroll_t = 1; }
					if(dw > ww && e.pageX - dl < $.vakata.dnd.settings.scroll_proximity)		{ vakata_dnd.scroll_l = -1; }
					if(dw > ww && ww - (e.pageX - dl) < $.vakata.dnd.settings.scroll_proximity)	{ vakata_dnd.scroll_l = 1; }
					if(vakata_dnd.scroll_t || vakata_dnd.scroll_l) {
						vakata_dnd.scroll_e = d;
					}
				}
				if(vakata_dnd.scroll_e) { $.vakata.dnd._scroll(true); }

				if(vakata_dnd.helper) {
					ht = parseInt(e.pageY + $.vakata.dnd.settings.helper_top, 10);
					hl = parseInt(e.pageX + $.vakata.dnd.settings.helper_left, 10);
					if(dh && ht + 25 > dh) { ht = dh - 50; }
					if(dw && hl + vakata_dnd.helper_w > dw) { hl = dw - (vakata_dnd.helper_w + 2); }
					vakata_dnd.helper.css({
						left	: hl + "px",
						top		: ht + "px"
					});
				}
				/**
				 * triggered on the document when a drag is in progress
				 * @event
				 * @plugin dnd
				 * @name dnd_move.vakata
				 * @param {Mixed} data any data supplied with the call to $.vakata.dnd.start
				 * @param {DOM} element the DOM element being dragged
				 * @param {jQuery} helper the helper shown next to the mouse
				 * @param {Object} event the event that caused this to trigger (most likely mousemove)
				 */
				$.vakata.dnd._trigger("move", e);
			},
			stop : function (e) {
				if(e.type === "touchend" && e.originalEvent && e.originalEvent.changedTouches && e.originalEvent.changedTouches[0]) {
					e.pageX = e.originalEvent.changedTouches[0].pageX;
					e.pageY = e.originalEvent.changedTouches[0].pageY;
					e.target = document.elementFromPoint(e.originalEvent.changedTouches[0].pageX - window.pageXOffset, e.originalEvent.changedTouches[0].pageY - window.pageYOffset);
				}
				if(vakata_dnd.is_drag) {
					/**
					 * triggered on the document when a drag stops (the dragged element is dropped)
					 * @event
					 * @plugin dnd
					 * @name dnd_stop.vakata
					 * @param {Mixed} data any data supplied with the call to $.vakata.dnd.start
					 * @param {DOM} element the DOM element being dragged
					 * @param {jQuery} helper the helper shown next to the mouse
					 * @param {Object} event the event that caused the stop
					 */
					$.vakata.dnd._trigger("stop", e);
				}
				$.vakata.dnd._clean();
			}
		};
	}(jQuery));

	// include the dnd plugin by default
	// $.jstree.defaults.plugins.push("dnd");


/**
 * ### Search plugin
 *
 * Adds search functionality to jsTree.
 */

	/**
	 * stores all defaults for the search plugin
	 * @name $.jstree.defaults.search
	 * @plugin search
	 */
	$.jstree.defaults.search = {
		/**
		 * a jQuery-like AJAX config, which jstree uses if a server should be queried for results. 
		 * 
		 * A `str` (which is the search string) parameter will be added with the request. The expected result is a JSON array with nodes that need to be opened so that matching nodes will be revealed.
		 * Leave this setting as `false` to not query the server.
		 * @name $.jstree.defaults.search.ajax
		 * @plugin search
		 */
		ajax : false,
		/**
		 * Indicates if the search should be fuzzy or not (should `chnd3` match `child node 3`). Default is `true`.
		 * @name $.jstree.defaults.search.fuzzy
		 * @plugin search
		 */
		fuzzy : true,
		/**
		 * Indicates if the search should be case sensitive. Default is `false`.
		 * @name $.jstree.defaults.search.case_sensitive
		 * @plugin search
		 */
		case_sensitive : false,
		/**
		 * Indicates if the tree should be filtered to show only matching nodes (keep in mind this can be a heavy on large trees in old browsers). Default is `false`.
		 * @name $.jstree.defaults.search.show_only_matches
		 * @plugin search
		 */
		show_only_matches : false,
		/**
		 * Indicates if all nodes opened to reveal the search result, should be closed when the search is cleared or a new search is performed. Default is `true`.
		 * @name $.jstree.defaults.search.close_opened_onclear
		 * @plugin search
		 */
		close_opened_onclear : true
	};

	$.jstree.plugins.search = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);

			this._data.search.str = "";
			this._data.search.dom = $();
			this._data.search.res = [];
			this._data.search.opn = [];
			this._data.search.sln = null;

			if(this.settings.search.show_only_matches) {
				this.element
					.on("search.jstree", function (e, data) {
						if(data.nodes.length) {
							$(this).find("li").hide().filter('.jstree-last').filter(function() { return this.nextSibling; }).removeClass('jstree-last');
							data.nodes.parentsUntil(".jstree").addBack().show()
								.filter("ul").each(function () { $(this).children("li:visible").eq(-1).addClass("jstree-last"); });
						}
					})
					.on("clear_search.jstree", function (e, data) {
						if(data.nodes.length) {
							$(this).find("li").css("display","").filter('.jstree-last').filter(function() { return this.nextSibling; }).removeClass('jstree-last');
						}
					});
			}
		};
		/**
		 * used to search the tree nodes for a given string
		 * @name search(str [, skip_async])
		 * @param {String} str the search string
		 * @param {Boolean} skip_async if set to true server will not be queried even if configured
		 * @plugin search
		 * @trigger search.jstree
		 */
		this.search = function (str, skip_async) {
			if(str === false || $.trim(str) === "") {
				return this.clear_search();
			}
			var s = this.settings.search,
				a = s.ajax ? $.extend({}, s.ajax) : false,
				f = null,
				r = [],
				p = [], i, j;
			if(this._data.search.res.length) {
				this.clear_search();
			}
			if(!skip_async && a !== false) {
				if(!a.data) { a.data = {}; }
				a.data.str = str;
				return $.ajax(a)
					.fail($.proxy(function () {
						this._data.core.last_error = { 'error' : 'ajax', 'plugin' : 'search', 'id' : 'search_01', 'reason' : 'Could not load search parents', 'data' : JSON.stringify(a) };
						this.settings.core.error.call(this, this._data.core.last_error);
					}, this))
					.done($.proxy(function (d) {
						if(d && d.d) { d = d.d; }
						this._data.search.sln = !$.isArray(d) ? [] : d;
						this._search_load(str);
					}, this));
			}
			this._data.search.str = str;
			this._data.search.dom = $();
			this._data.search.res = [];
			this._data.search.opn = [];

			f = new $.vakata.search(str, true, { caseSensitive : s.case_sensitive, fuzzy : s.fuzzy });

			$.each(this._model.data, function (i, v) {
				if(v.text && f.search(v.text).isMatch) {
					r.push(i);
					p = p.concat(v.parents);
				}
			});
			if(r.length) {
				p = $.vakata.array_unique(p);
				this._search_open(p);
				for(i = 0, j = r.length; i < j; i++) {
					f = this.get_node(r[i], true);
					if(f) {
						this._data.search.dom = this._data.search.dom.add(f);
					}
				}
				this._data.search.res = r;
				this._data.search.dom.children(".jstree-anchor").addClass('jstree-search');
			}
			/**
			 * triggered after search is complete
			 * @event
			 * @name search.jstree
			 * @param {jQuery} nodes a jQuery collection of matching nodes
			 * @param {String} str the search string
			 * @param {Array} res a collection of objects represeing the matching nodes
			 * @plugin search
			 */
			this.trigger('search', { nodes : this._data.search.dom, str : str, res : this._data.search.res });
		};
		/**
		 * used to clear the last search (removes classes and shows all nodes if filtering is on)
		 * @name clear_search()
		 * @plugin search
		 * @trigger clear_search.jstree
		 */
		this.clear_search = function () {
			this._data.search.dom.children(".jstree-anchor").removeClass("jstree-search");
			if(this.settings.search.close_opened_onclear) {
				this.close_node(this._data.search.opn, 0);
			}
			/**
			 * triggered after search is complete
			 * @event
			 * @name clear_search.jstree
			 * @param {jQuery} nodes a jQuery collection of matching nodes (the result from the last search)
			 * @param {String} str the search string (the last search string)
			 * @param {Array} res a collection of objects represeing the matching nodes (the result from the last search)
			 * @plugin search
			 */
			this.trigger('clear_search', { 'nodes' : this._data.search.dom, str : this._data.search.str, res : this._data.search.res });
			this._data.search.str = "";
			this._data.search.res = [];
			this._data.search.opn = [];
			this._data.search.dom = $();
		};
		/**
		 * opens nodes that need to be opened to reveal the search results. Used only internally.
		 * @private
		 * @name _search_open(d)
		 * @param {Array} d an array of node IDs
		 * @plugin search
		 */
		this._search_open = function (d) {
			var t = this;
			$.each(d.concat([]), function (i, v) {
				v = document.getElementById(v);
				if(v) {
					if(t.is_closed(v)) {
						t._data.search.opn.push(v.id);
						t.open_node(v, function () { t._search_open(d); }, 0);
					}
				}
			});
		};
		/**
		 * loads nodes that need to be opened to reveal the search results. Used only internally.
		 * @private
		 * @name _search_load(d, str)
		 * @param {String} str the search string
		 * @plugin search
		 */
		this._search_load = function (str) {
			var res = true,
				t = this,
				m = t._model.data;
			if($.isArray(this._data.search.sln)) {
				if(!this._data.search.sln.length) {
					this._data.search.sln = null;
					this.search(str, true);
				}
				else {
					$.each(this._data.search.sln, function (i, v) {
						if(m[v]) {
							$.vakata.array_remove_item(t._data.search.sln, v);
							if(!m[v].state.loaded) {
								t.load_node(v, function (o, s) { if(s) { t._search_load(str); } });
								res = false;
							}
						}
					});
					if(res) {
						this._data.search.sln = [];
						this._search_load(str);
					}
				}
			}
		};
	};

	// helpers
	(function ($) {
		// from http://kiro.me/projects/fuse.html
		$.vakata.search = function(pattern, txt, options) {
			options = options || {};
			if(options.fuzzy !== false) {
				options.fuzzy = true;
			}
			pattern = options.caseSensitive ? pattern : pattern.toLowerCase();
			var MATCH_LOCATION	= options.location || 0,
				MATCH_DISTANCE	= options.distance || 100,
				MATCH_THRESHOLD	= options.threshold || 0.6,
				patternLen = pattern.length,
				matchmask, pattern_alphabet, match_bitapScore, search;
			if(patternLen > 32) {
				options.fuzzy = false;
			}
			if(options.fuzzy) {
				matchmask = 1 << (patternLen - 1);
				pattern_alphabet = (function () {
					var mask = {},
						i = 0;
					for (i = 0; i < patternLen; i++) {
						mask[pattern.charAt(i)] = 0;
					}
					for (i = 0; i < patternLen; i++) {
						mask[pattern.charAt(i)] |= 1 << (patternLen - i - 1);
					}
					return mask;
				}());
				match_bitapScore = function (e, x) {
					var accuracy = e / patternLen,
						proximity = Math.abs(MATCH_LOCATION - x);
					if(!MATCH_DISTANCE) {
						return proximity ? 1.0 : accuracy;
					}
					return accuracy + (proximity / MATCH_DISTANCE);
				};
			}
			search = function (text) {
				text = options.caseSensitive ? text : text.toLowerCase();
				if(pattern === text || text.indexOf(pattern) !== -1) {
					return {
						isMatch: true,
						score: 0
					};
				}
				if(!options.fuzzy) {
					return {
						isMatch: false,
						score: 1
					};
				}
				var i, j,
					textLen = text.length,
					scoreThreshold = MATCH_THRESHOLD,
					bestLoc = text.indexOf(pattern, MATCH_LOCATION),
					binMin, binMid,
					binMax = patternLen + textLen,
					lastRd, start, finish, rd, charMatch,
					score = 1,
					locations = [];
				if (bestLoc !== -1) {
					scoreThreshold = Math.min(match_bitapScore(0, bestLoc), scoreThreshold);
					bestLoc = text.lastIndexOf(pattern, MATCH_LOCATION + patternLen);
					if (bestLoc !== -1) {
						scoreThreshold = Math.min(match_bitapScore(0, bestLoc), scoreThreshold);
					}
				}
				bestLoc = -1;
				for (i = 0; i < patternLen; i++) {
					binMin = 0;
					binMid = binMax;
					while (binMin < binMid) {
						if (match_bitapScore(i, MATCH_LOCATION + binMid) <= scoreThreshold) {
							binMin = binMid;
						} else {
							binMax = binMid;
						}
						binMid = Math.floor((binMax - binMin) / 2 + binMin);
					}
					binMax = binMid;
					start = Math.max(1, MATCH_LOCATION - binMid + 1);
					finish = Math.min(MATCH_LOCATION + binMid, textLen) + patternLen;
					rd = new Array(finish + 2);
					rd[finish + 1] = (1 << i) - 1;
					for (j = finish; j >= start; j--) {
						charMatch = pattern_alphabet[text.charAt(j - 1)];
						if (i === 0) {
							rd[j] = ((rd[j + 1] << 1) | 1) & charMatch;
						} else {
							rd[j] = ((rd[j + 1] << 1) | 1) & charMatch | (((lastRd[j + 1] | lastRd[j]) << 1) | 1) | lastRd[j + 1];
						}
						if (rd[j] & matchmask) {
							score = match_bitapScore(i, j - 1);
							if (score <= scoreThreshold) {
								scoreThreshold = score;
								bestLoc = j - 1;
								locations.push(bestLoc);
								if (bestLoc > MATCH_LOCATION) {
									start = Math.max(1, 2 * MATCH_LOCATION - bestLoc);
								} else {
									break;
								}
							}
						}
					}
					if (match_bitapScore(i + 1, MATCH_LOCATION) > scoreThreshold) {
						break;
					}
					lastRd = rd;
				}
				return {
					isMatch: bestLoc >= 0,
					score: score
				};
			};
			return txt === true ? { 'search' : search } : search(txt);
		};
	}(jQuery));

	// include the search plugin by default
	// $.jstree.defaults.plugins.push("search");

/**
 * ### Sort plugin
 *
 * Autmatically sorts all siblings in the tree according to a sorting function.
 */

	/**
	 * the settings function used to sort the nodes.
	 * It is executed in the tree's context, accepts two nodes as arguments and should return `1` or `-1`.
	 * @name $.jstree.defaults.sort
	 * @plugin sort
	 */
	$.jstree.defaults.sort = function (a, b) {
		//return this.get_type(a) === this.get_type(b) ? (this.get_text(a) > this.get_text(b) ? 1 : -1) : this.get_type(a) >= this.get_type(b);
		return this.get_text(a) > this.get_text(b) ? 1 : -1;
	};
	$.jstree.plugins.sort = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);
			this.element
				.on("model.jstree", $.proxy(function (e, data) {
						this.sort(data.parent, true);
					}, this))
				.on("rename_node.jstree create_node.jstree", $.proxy(function (e, data) {
						this.sort(data.parent || data.node.parent, false);
						this.redraw_node(data.parent || data.node.parent, true);
					}, this))
				.on("move_node.jstree copy_node.jstree", $.proxy(function (e, data) {
						this.sort(data.parent, false);
						this.redraw_node(data.parent, true);
					}, this));
		};
		/**
		 * used to sort a node's children
		 * @private
		 * @name sort(obj [, deep])
		 * @param  {mixed} obj the node
		 * @param {Boolean} deep if set to `true` nodes are sorted recursively.
		 * @plugin sort
		 * @trigger search.jstree
		 */
		this.sort = function (obj, deep) {
			var i, j;
			obj = this.get_node(obj);
			if(obj && obj.children && obj.children.length) {
				obj.children.sort($.proxy(this.settings.sort, this));
				if(deep) {
					for(i = 0, j = obj.children_d.length; i < j; i++) {
						this.sort(obj.children_d[i], false);
					}
				}
			}
		};
	};

	// include the sort plugin by default
	// $.jstree.defaults.plugins.push("sort");

/**
 * ### State plugin
 *
 * Saves the state of the tree (selected nodes, opened nodes) on the user's computer using available options (localStorage, cookies, etc)
 */

	var to = false;
	/**
	 * stores all defaults for the state plugin
	 * @name $.jstree.defaults.state
	 * @plugin state
	 */
	$.jstree.defaults.state = {
		/**
		 * A string for the key to use when saving the current tree (change if using multiple trees in your project). Defaults to `jstree`.
		 * @name $.jstree.defaults.state.key
		 * @plugin state
		 */
		key		: 'jstree',
		/**
		 * A space separated list of events that trigger a state save. Defaults to `changed.jstree open_node.jstree close_node.jstree`.
		 * @name $.jstree.defaults.state.events
		 * @plugin state
		 */
		events	: 'changed.jstree open_node.jstree close_node.jstree',
		/**
		 * Time in milliseconds after which the state will expire. Defaults to 'false' meaning - no expire.
		 * @name $.jstree.defaults.state.ttl
		 * @plugin state
		 */
		ttl		: false,
		/**
		 * A function that will be executed prior to restoring state with one argument - the state object. Can be used to clear unwanted parts of the state.
		 * @name $.jstree.defaults.state.filter
		 * @plugin state
		 */
		filter	: false
	};
	$.jstree.plugins.state = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);
			var bind = $.proxy(function () {
				this.element.on(this.settings.state.events, $.proxy(function () {
					if(to) { clearTimeout(to); }
					to = setTimeout($.proxy(function () { this.save_state(); }, this), 100);
				}, this));
			}, this);
			this.element
				.on("ready.jstree", $.proxy(function (e, data) {
						this.element.one("restore_state.jstree", bind);
						if(!this.restore_state()) { bind(); }
					}, this));
		};
		/**
		 * save the state
		 * @name save_state()
		 * @plugin state
		 */
		this.save_state = function () {
			var st = { 'state' : this.get_state(), 'ttl' : this.settings.state.ttl, 'sec' : +(new Date()) };
			$.vakata.storage.set(this.settings.state.key, JSON.stringify(st));
		};
		/**
		 * restore the state from the user's computer
		 * @name restore_state()
		 * @plugin state
		 */
		this.restore_state = function () {
			var k = $.vakata.storage.get(this.settings.state.key);
			if(!!k) { try { k = JSON.parse(k); } catch(ex) { return false; } }
			if(!!k && k.ttl && k.sec && +(new Date()) - k.sec > k.ttl) { return false; }
			if(!!k && k.state) { k = k.state; }
			if(!!k && $.isFunction(this.settings.state.filter)) { k = this.settings.state.filter.call(this, k); }
			if(!!k) {
				this.element.one("set_state.jstree", function (e, data) { data.instance.trigger('restore_state', { 'state' : $.extend(true, {}, k) }); });
				this.set_state(k);
				return true;
			}
			return false;
		};
		/**
		 * clear the state on the user's computer
		 * @name clear_state()
		 * @plugin state
		 */
		this.clear_state = function () {
			return $.vakata.storage.del(this.settings.state.key);
		};
	};

	(function ($, undefined) {
		$.vakata.storage = {
			// simply specifying the functions in FF throws an error
			set : function (key, val) { return window.localStorage.setItem(key, val); },
			get : function (key) { return window.localStorage.getItem(key); },
			del : function (key) { return window.localStorage.removeItem(key); }
		};
	}(jQuery));

	// include the state plugin by default
	// $.jstree.defaults.plugins.push("state");

/**
 * ### Types plugin
 *
 * Makes it possible to add predefined types for groups of nodes, which make it possible to easily control nesting rules and icon for each group.
 */

	/**
	 * An object storing all types as key value pairs, where the key is the type name and the value is an object that could contain following keys (all optional).
	 * 
	 * * `max_children` the maximum number of immediate children this node type can have. Do not specify or set to `-1` for unlimited.
	 * * `max_depth` the maximum number of nesting this node type can have. A value of `1` would mean that the node can have children, but no grandchildren. Do not specify or set to `-1` for unlimited.
	 * * `valid_children` an array of node type strings, that nodes of this type can have as children. Do not specify or set to `-1` for no limits.
	 * * `icon` a string - can be a path to an icon or a className, if using an image that is in the current directory use a `./` prefix, otherwise it will be detected as a class. Omit to use the default icon from your theme.
	 *
	 * There are two predefined types:
	 * 
	 * * `#` represents the root of the tree, for example `max_children` would control the maximum number of root nodes.
	 * * `default` represents the default node - any settings here will be applied to all nodes that do not have a type specified.
	 * 
	 * @name $.jstree.defaults.types
	 * @plugin types
	 */
	$.jstree.defaults.types = {
		'#' : {},
		'default' : {}
	};

	$.jstree.plugins.types = function (options, parent) {
		this.init = function (el, options) {
			var i, j;
			if(options && options.types && options.types['default']) {
				for(i in options.types) {
					if(i !== "default" && i !== "#" && options.types.hasOwnProperty(i)) {
						for(j in options.types['default']) {
							if(options.types['default'].hasOwnProperty(j) && options.types[i][j] === undefined) {
								options.types[i][j] = options.types['default'][j];
							}
						}
					}
				}
			}
			parent.init.call(this, el, options);
			this._model.data['#'].type = '#';
		};
		this.bind = function () {
			parent.bind.call(this);
			this.element
				.on('model.jstree', $.proxy(function (e, data) {
						var m = this._model.data,
							dpc = data.nodes,
							t = this.settings.types,
							i, j, c = 'default';
						for(i = 0, j = dpc.length; i < j; i++) {
							c = 'default';
							if(m[dpc[i]].original && m[dpc[i]].original.type && t[m[dpc[i]].original.type]) {
								c = m[dpc[i]].original.type;
							}
							if(m[dpc[i]].data && m[dpc[i]].data.jstree && m[dpc[i]].data.jstree.type && t[m[dpc[i]].data.jstree.type]) {
								c = m[dpc[i]].data.jstree.type;
							}
							m[dpc[i]].type = c;
							if(m[dpc[i]].icon === true && t[c].icon !== undefined) {
								m[dpc[i]].icon = t[c].icon;
							}
						}
					}, this));
		};
		this.get_json = function (obj, options, flat) {
			var i, j,
				m = this._model.data,
				opt = options ? $.extend(true, {}, options, {no_id:false}) : {},
				tmp = parent.get_json.call(this, obj, opt, flat);
			if(tmp === false) { return false; }
			if($.isArray(tmp)) {
				for(i = 0, j = tmp.length; i < j; i++) {
					tmp[i].type = tmp[i].id && m[tmp[i].id] && m[tmp[i].id].type ? m[tmp[i].id].type : "default";
					if(options && options.no_id) {
						delete tmp[i].id;
						if(tmp[i].li_attr && tmp[i].li_attr.id) {
							delete tmp[i].li_attr.id;
						}
					}
				}
			}
			else {
				tmp.type = tmp.id && m[tmp.id] && m[tmp.id].type ? m[tmp.id].type : "default";
				if(options && options.no_id) {
					tmp = this._delete_ids(tmp);
				}
			}
			return tmp;
		};
		this._delete_ids = function (tmp) {
			if($.isArray(tmp)) {
				for(var i = 0, j = tmp.length; i < j; i++) {
					tmp[i] = this._delete_ids(tmp[i]);
				}
				return tmp;
			}
			delete tmp.id;
			if(tmp.li_attr && tmp.li_attr.id) {
				delete tmp.li_attr.id;
			}
			if(tmp.children && $.isArray(tmp.children)) {
				tmp.children = this._delete_ids(tmp.children);
			}
			return tmp;
		};
		this.check = function (chk, obj, par, pos) {
			if(parent.check.call(this, chk, obj, par, pos) === false) { return false; }
			obj = obj && obj.id ? obj : this.get_node(obj);
			par = par && par.id ? par : this.get_node(par);
			var m = obj && obj.id ? $.jstree.reference(obj.id) : null, tmp, d, i, j;
			m = m && m._model && m._model.data ? m._model.data : null;
			switch(chk) {
				case "create_node":
				case "move_node":
				case "copy_node":
					if(chk !== 'move_node' || $.inArray(obj.id, par.children) === -1) {
						tmp = this.get_rules(par);
						if(tmp.max_children !== undefined && tmp.max_children !== -1 && tmp.max_children === par.children.length) {
							this._data.core.last_error = { 'error' : 'check', 'plugin' : 'types', 'id' : 'types_01', 'reason' : 'max_children prevents function: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
							return false;
						}
						if(tmp.valid_children !== undefined && tmp.valid_children !== -1 && $.inArray(obj.type, tmp.valid_children) === -1) {
							this._data.core.last_error = { 'error' : 'check', 'plugin' : 'types', 'id' : 'types_02', 'reason' : 'valid_children prevents function: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
							return false;
						}
						if(m && obj.children_d && obj.parents) {
							d = 0;
							for(i = 0, j = obj.children_d.length; i < j; i++) {
								d = Math.max(d, m[obj.children_d[i]].parents.length);
							}
							d = d - obj.parents.length + 1;
						}
						if(d <= 0 || d === undefined) { d = 1; }
						do {
							if(tmp.max_depth !== undefined && tmp.max_depth !== -1 && tmp.max_depth < d) {
								this._data.core.last_error = { 'error' : 'check', 'plugin' : 'types', 'id' : 'types_03', 'reason' : 'max_depth prevents function: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
								return false;
							}
							par = this.get_node(par.parent);
							tmp = this.get_rules(par);
							d++;
						} while(par);
					}
					break;
			}
			return true;
		};
		/**
		 * used to retrieve the type settings object for a node
		 * @name get_rules(obj)
		 * @param {mixed} obj the node to find the rules for
		 * @return {Object}
		 * @plugin types
		 */
		this.get_rules = function (obj) {
			obj = this.get_node(obj);
			if(!obj) { return false; }
			var tmp = this.get_type(obj, true);
			if(tmp.max_depth === undefined) { tmp.max_depth = -1; }
			if(tmp.max_children === undefined) { tmp.max_children = -1; }
			if(tmp.valid_children === undefined) { tmp.valid_children = -1; }
			return tmp;
		};
		/**
		 * used to retrieve the type string or settings object for a node
		 * @name get_type(obj [, rules])
		 * @param {mixed} obj the node to find the rules for
		 * @param {Boolean} rules if set to `true` instead of a string the settings object will be returned
		 * @return {String|Object}
		 * @plugin types
		 */
		this.get_type = function (obj, rules) {
			obj = this.get_node(obj);
			return (!obj) ? false : ( rules ? $.extend({ 'type' : obj.type }, this.settings.types[obj.type]) : obj.type);
		};
		/**
		 * used to change a node's type
		 * @name set_type(obj, type)
		 * @param {mixed} obj the node to change
		 * @param {String} type the new type
		 * @plugin types
		 */
		this.set_type = function (obj, type) {
			var t, t1, t2, old_type, old_icon;
			if($.isArray(obj)) {
				obj = obj.slice();
				for(t1 = 0, t2 = obj.length; t1 < t2; t1++) {
					this.set_type(obj[t1], type);
				}
				return true;
			}
			t = this.settings.types;
			obj = this.get_node(obj);
			if(!t[type] || !obj) { return false; }
			old_type = obj.type;
			old_icon = this.get_icon(obj);
			obj.type = type;
			if(old_icon === true || (t[old_type] && t[old_type].icon && old_icon === t[old_type].icon)) {
				this.set_icon(obj, t[type].icon !== undefined ? t[type].icon : true);
			}
			return true;
		};
	};
	// include the types plugin by default
	// $.jstree.defaults.plugins.push("types");

/**
 * ### Unique plugin
 *
 * Enforces that no nodes with the same name can coexist as siblings.
 */

	$.jstree.plugins.unique = function (options, parent) {
		this.check = function (chk, obj, par, pos) {
			if(parent.check.call(this, chk, obj, par, pos) === false) { return false; }
			obj = obj && obj.id ? obj : this.get_node(obj);
			par = par && par.id ? par : this.get_node(par);
			if(!par || !par.children) { return true; }
			var n = chk === "rename_node" ? pos : obj.text,
				c = [],
				m = this._model.data, i, j;
			for(i = 0, j = par.children.length; i < j; i++) {
				c.push(m[par.children[i]].text);
			}
			switch(chk) {
				case "delete_node":
					return true;
				case "rename_node":
				case "copy_node":
					i = ($.inArray(n, c) === -1);
					if(!i) {
						this._data.core.last_error = { 'error' : 'check', 'plugin' : 'unique', 'id' : 'unique_01', 'reason' : 'Child with name ' + n + ' already exists. Preventing: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
					}
					return i;
				case "move_node":
					i = (obj.parent === par.id || $.inArray(n, c) === -1);
					if(!i) {
						this._data.core.last_error = { 'error' : 'check', 'plugin' : 'unique', 'id' : 'unique_01', 'reason' : 'Child with name ' + n + ' already exists. Preventing: ' + chk, 'data' : JSON.stringify({ 'chk' : chk, 'pos' : pos, 'obj' : obj && obj.id ? obj.id : false, 'par' : par && par.id ? par.id : false }) };
					}
					return i;
			}
			return true;
		};
	};

	// include the unique plugin by default
	// $.jstree.defaults.plugins.push("unique");


/**
 * ### Wholerow plugin
 *
 * Makes each node appear block level. Making selection easier. May cause slow down for large trees in old browsers.
 */

	var div = document.createElement('DIV');
	div.setAttribute('unselectable','on');
	div.className = 'jstree-wholerow';
	div.innerHTML = '&#160;';
	$.jstree.plugins.wholerow = function (options, parent) {
		this.bind = function () {
			parent.bind.call(this);

			this.element
				.on('loading', $.proxy(function () {
						div.style.height = this._data.core.li_height + 'px';
					}, this))
				.on('ready.jstree set_state.jstree', $.proxy(function () {
						this.hide_dots();
					}, this))
				.on("ready.jstree", $.proxy(function () {
						this.get_container_ul().addClass('jstree-wholerow-ul');
					}, this))
				.on("deselect_all.jstree", $.proxy(function (e, data) {
						this.element.find('.jstree-wholerow-clicked').removeClass('jstree-wholerow-clicked');
					}, this))
				.on("changed.jstree", $.proxy(function (e, data) {
						this.element.find('.jstree-wholerow-clicked').removeClass('jstree-wholerow-clicked');
						var tmp = false, i, j;
						for(i = 0, j = data.selected.length; i < j; i++) {
							tmp = this.get_node(data.selected[i], true);
							if(tmp && tmp.length) {
								tmp.children('.jstree-wholerow').addClass('jstree-wholerow-clicked');
							}
						}
					}, this))
				.on("open_node.jstree", $.proxy(function (e, data) {
						this.get_node(data.node, true).find('.jstree-clicked').parent().children('.jstree-wholerow').addClass('jstree-wholerow-clicked');
					}, this))
				.on("hover_node.jstree dehover_node.jstree", $.proxy(function (e, data) {
						this.get_node(data.node, true).children('.jstree-wholerow')[e.type === "hover_node"?"addClass":"removeClass"]('jstree-wholerow-hovered');
					}, this))
				.on("contextmenu.jstree", ".jstree-wholerow", $.proxy(function (e) {
						e.preventDefault();
						$(e.currentTarget).closest("li").children("a:eq(0)").trigger('contextmenu',e);
					}, this))
				.on("click.jstree", ".jstree-wholerow", function (e) {
						e.stopImmediatePropagation();
						var tmp = $.Event('click', { metaKey : e.metaKey, ctrlKey : e.ctrlKey, altKey : e.altKey, shiftKey : e.shiftKey });
						$(e.currentTarget).closest("li").children("a:eq(0)").trigger(tmp).focus();
					})
				.on("click.jstree", ".jstree-leaf > .jstree-ocl", $.proxy(function (e) {
						e.stopImmediatePropagation();
						var tmp = $.Event('click', { metaKey : e.metaKey, ctrlKey : e.ctrlKey, altKey : e.altKey, shiftKey : e.shiftKey });
						$(e.currentTarget).closest("li").children("a:eq(0)").trigger(tmp).focus();
					}, this))
				.on("mouseover.jstree", ".jstree-wholerow, .jstree-icon", $.proxy(function (e) {
						e.stopImmediatePropagation();
						this.hover_node(e.currentTarget);
						return false;
					}, this))
				.on("mouseleave.jstree", ".jstree-node", $.proxy(function (e) {
						this.dehover_node(e.currentTarget);
					}, this));
		};
		this.teardown = function () {
			if(this.settings.wholerow) {
				this.element.find(".jstree-wholerow").remove();
			}
			parent.teardown.call(this);
		};
		this.redraw_node = function(obj, deep, callback) {
			obj = parent.redraw_node.call(this, obj, deep, callback);
			if(obj) {
				var tmp = div.cloneNode(true);
				//tmp.style.height = this._data.core.li_height + 'px';
				if($.inArray(obj.id, this._data.core.selected) !== -1) { tmp.className += ' jstree-wholerow-clicked'; }
				obj.insertBefore(tmp, obj.childNodes[0]);
			}
			return obj;
		};
	};
	// include the wholerow plugin by default
	// $.jstree.defaults.plugins.push("wholerow");

}));
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJqc3RyZWUuanMiXSwic291cmNlc0NvbnRlbnQiOlsiLypnbG9iYWxzIGpRdWVyeSwgZGVmaW5lLCBleHBvcnRzLCByZXF1aXJlLCB3aW5kb3csIGRvY3VtZW50ICovXHJcbihmdW5jdGlvbiAoZmFjdG9yeSkge1xyXG5cdFwidXNlIHN0cmljdFwiO1xyXG5cdGlmICh0eXBlb2YgZGVmaW5lID09PSAnZnVuY3Rpb24nICYmIGRlZmluZS5hbWQpIHtcclxuXHRcdGRlZmluZShbJ2pxdWVyeSddLCBmYWN0b3J5KTtcclxuXHR9XHJcblx0ZWxzZSBpZih0eXBlb2YgZXhwb3J0cyA9PT0gJ29iamVjdCcpIHtcclxuXHRcdGZhY3RvcnkocmVxdWlyZSgnanF1ZXJ5JykpO1xyXG5cdH1cclxuXHRlbHNlIHtcclxuXHRcdGZhY3RvcnkoalF1ZXJ5KTtcclxuXHR9XHJcbn0oZnVuY3Rpb24gKCQsIHVuZGVmaW5lZCkge1xyXG5cdFwidXNlIHN0cmljdFwiO1xyXG4vKiFcclxuICoganNUcmVlIDMuMC4wXHJcbiAqIGh0dHA6Ly9qc3RyZWUuY29tL1xyXG4gKlxyXG4gKiBDb3B5cmlnaHQgKGMpIDIwMTMgSXZhbiBCb3poYW5vdiAoaHR0cDovL3Zha2F0YS5jb20pXHJcbiAqXHJcbiAqIExpY2Vuc2VkIHNhbWUgYXMganF1ZXJ5IC0gdW5kZXIgdGhlIHRlcm1zIG9mIHRoZSBNSVQgTGljZW5zZVxyXG4gKiAgIGh0dHA6Ly93d3cub3BlbnNvdXJjZS5vcmcvbGljZW5zZXMvbWl0LWxpY2Vuc2UucGhwXHJcbiAqL1xyXG4vKiFcclxuICogaWYgdXNpbmcganNsaW50IHBsZWFzZSBhbGxvdyBmb3IgdGhlIGpRdWVyeSBnbG9iYWwgYW5kIHVzZSBmb2xsb3dpbmcgb3B0aW9uczogXHJcbiAqIGpzbGludDogYnJvd3NlcjogdHJ1ZSwgYXNzOiB0cnVlLCBiaXR3aXNlOiB0cnVlLCBjb250aW51ZTogdHJ1ZSwgbm9tZW46IHRydWUsIHBsdXNwbHVzOiB0cnVlLCByZWdleHA6IHRydWUsIHVucGFyYW06IHRydWUsIHRvZG86IHRydWUsIHdoaXRlOiB0cnVlXHJcbiAqL1xyXG5cclxuXHQvLyBwcmV2ZW50IGFub3RoZXIgbG9hZD8gbWF5YmUgdGhlcmUgaXMgYSBiZXR0ZXIgd2F5P1xyXG5cdGlmKCQuanN0cmVlKSB7XHJcblx0XHRyZXR1cm47XHJcblx0fVxyXG5cclxuXHQvKipcclxuXHQgKiAjIyMganNUcmVlIGNvcmUgZnVuY3Rpb25hbGl0eVxyXG5cdCAqL1xyXG5cclxuXHQvLyBpbnRlcm5hbCB2YXJpYWJsZXNcclxuXHR2YXIgaW5zdGFuY2VfY291bnRlciA9IDAsXHJcblx0XHRjY3Bfbm9kZSA9IGZhbHNlLFxyXG5cdFx0Y2NwX21vZGUgPSBmYWxzZSxcclxuXHRcdGNjcF9pbnN0ID0gZmFsc2UsXHJcblx0XHR0aGVtZXNfbG9hZGVkID0gW10sXHJcblx0XHRzcmMgPSAkKCdzY3JpcHQ6bGFzdCcpLmF0dHIoJ3NyYycpLFxyXG5cdFx0X2QgPSBkb2N1bWVudCwgX25vZGUgPSBfZC5jcmVhdGVFbGVtZW50KCdMSScpLCBfdGVtcDEsIF90ZW1wMjtcclxuXHJcblx0X25vZGUuc2V0QXR0cmlidXRlKCdyb2xlJywgJ3RyZWVpdGVtJyk7XHJcblx0X3RlbXAxID0gX2QuY3JlYXRlRWxlbWVudCgnSScpO1xyXG5cdF90ZW1wMS5jbGFzc05hbWUgPSAnanN0cmVlLWljb24ganN0cmVlLW9jbCc7XHJcblx0X25vZGUuYXBwZW5kQ2hpbGQoX3RlbXAxKTtcclxuXHRfdGVtcDEgPSBfZC5jcmVhdGVFbGVtZW50KCdBJyk7XHJcblx0X3RlbXAxLmNsYXNzTmFtZSA9ICdqc3RyZWUtYW5jaG9yJztcclxuXHRfdGVtcDEuc2V0QXR0cmlidXRlKCdocmVmJywnIycpO1xyXG5cdF90ZW1wMiA9IF9kLmNyZWF0ZUVsZW1lbnQoJ0knKTtcclxuXHRfdGVtcDIuY2xhc3NOYW1lID0gJ2pzdHJlZS1pY29uIGpzdHJlZS10aGVtZWljb24nO1xyXG5cdF90ZW1wMS5hcHBlbmRDaGlsZChfdGVtcDIpO1xyXG5cdF9ub2RlLmFwcGVuZENoaWxkKF90ZW1wMSk7XHJcblx0X3RlbXAxID0gX3RlbXAyID0gbnVsbDtcclxuXHJcblxyXG5cdC8qKlxyXG5cdCAqIGhvbGRzIGFsbCBqc3RyZWUgcmVsYXRlZCBmdW5jdGlvbnMgYW5kIHZhcmlhYmxlcywgaW5jbHVkaW5nIHRoZSBhY3R1YWwgY2xhc3MgYW5kIG1ldGhvZHMgdG8gY3JlYXRlLCBhY2Nlc3MgYW5kIG1hbmlwdWxhdGUgaW5zdGFuY2VzLlxyXG5cdCAqIEBuYW1lICQuanN0cmVlXHJcblx0ICovXHJcblx0JC5qc3RyZWUgPSB7XHJcblx0XHQvKiogXHJcblx0XHQgKiBzcGVjaWZpZXMgdGhlIGpzdHJlZSB2ZXJzaW9uIGluIHVzZVxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUudmVyc2lvblxyXG5cdFx0ICovXHJcblx0XHR2ZXJzaW9uIDogJzMuMC4wLWJldGE5JyxcclxuXHRcdC8qKlxyXG5cdFx0ICogaG9sZHMgYWxsIHRoZSBkZWZhdWx0IG9wdGlvbnMgdXNlZCB3aGVuIGNyZWF0aW5nIG5ldyBpbnN0YW5jZXNcclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzXHJcblx0XHQgKi9cclxuXHRcdGRlZmF1bHRzIDoge1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogY29uZmlndXJlIHdoaWNoIHBsdWdpbnMgd2lsbCBiZSBhY3RpdmUgb24gYW4gaW5zdGFuY2UuIFNob3VsZCBiZSBhbiBhcnJheSBvZiBzdHJpbmdzLCB3aGVyZSBlYWNoIGVsZW1lbnQgaXMgYSBwbHVnaW4gbmFtZS4gVGhlIGRlZmF1bHQgaXMgYFtdYFxyXG5cdFx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5wbHVnaW5zXHJcblx0XHRcdCAqL1xyXG5cdFx0XHRwbHVnaW5zIDogW11cclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHN0b3JlcyBhbGwgbG9hZGVkIGpzdHJlZSBwbHVnaW5zICh1c2VkIGludGVybmFsbHkpXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5wbHVnaW5zXHJcblx0XHQgKi9cclxuXHRcdHBsdWdpbnMgOiB7fSxcclxuXHRcdHBhdGggOiBzcmMgJiYgc3JjLmluZGV4T2YoJy8nKSAhPT0gLTEgPyBzcmMucmVwbGFjZSgvXFwvW15cXC9dKyQvLCcnKSA6ICcnXHJcblx0fTtcclxuXHQvKipcclxuXHQgKiBjcmVhdGVzIGEganN0cmVlIGluc3RhbmNlXHJcblx0ICogQG5hbWUgJC5qc3RyZWUuY3JlYXRlKGVsIFssIG9wdGlvbnNdKVxyXG5cdCAqIEBwYXJhbSB7RE9NRWxlbWVudHxqUXVlcnl8U3RyaW5nfSBlbCB0aGUgZWxlbWVudCB0byBjcmVhdGUgdGhlIGluc3RhbmNlIG9uLCBjYW4gYmUgalF1ZXJ5IGV4dGVuZGVkIG9yIGEgc2VsZWN0b3JcclxuXHQgKiBAcGFyYW0ge09iamVjdH0gb3B0aW9ucyBvcHRpb25zIGZvciB0aGlzIGluc3RhbmNlIChleHRlbmRzIGAkLmpzdHJlZS5kZWZhdWx0c2ApXHJcblx0ICogQHJldHVybiB7anNUcmVlfSB0aGUgbmV3IGluc3RhbmNlXHJcblx0ICovXHJcblx0JC5qc3RyZWUuY3JlYXRlID0gZnVuY3Rpb24gKGVsLCBvcHRpb25zKSB7XHJcblx0XHR2YXIgdG1wID0gbmV3ICQuanN0cmVlLmNvcmUoKytpbnN0YW5jZV9jb3VudGVyKSxcclxuXHRcdFx0b3B0ID0gb3B0aW9ucztcclxuXHRcdG9wdGlvbnMgPSAkLmV4dGVuZCh0cnVlLCB7fSwgJC5qc3RyZWUuZGVmYXVsdHMsIG9wdGlvbnMpO1xyXG5cdFx0aWYob3B0ICYmIG9wdC5wbHVnaW5zKSB7XHJcblx0XHRcdG9wdGlvbnMucGx1Z2lucyA9IG9wdC5wbHVnaW5zO1xyXG5cdFx0fVxyXG5cdFx0JC5lYWNoKG9wdGlvbnMucGx1Z2lucywgZnVuY3Rpb24gKGksIGspIHtcclxuXHRcdFx0aWYoaSAhPT0gJ2NvcmUnKSB7XHJcblx0XHRcdFx0dG1wID0gdG1wLnBsdWdpbihrLCBvcHRpb25zW2tdKTtcclxuXHRcdFx0fVxyXG5cdFx0fSk7XHJcblx0XHR0bXAuaW5pdChlbCwgb3B0aW9ucyk7XHJcblx0XHRyZXR1cm4gdG1wO1xyXG5cdH07XHJcblx0LyoqXHJcblx0ICogdGhlIGpzdHJlZSBjbGFzcyBjb25zdHJ1Y3RvciwgdXNlZCBvbmx5IGludGVybmFsbHlcclxuXHQgKiBAcHJpdmF0ZVxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmNvcmUoaWQpXHJcblx0ICogQHBhcmFtIHtOdW1iZXJ9IGlkIHRoaXMgaW5zdGFuY2UncyBpbmRleFxyXG5cdCAqL1xyXG5cdCQuanN0cmVlLmNvcmUgPSBmdW5jdGlvbiAoaWQpIHtcclxuXHRcdHRoaXMuX2lkID0gaWQ7XHJcblx0XHR0aGlzLl9jbnQgPSAwO1xyXG5cdFx0dGhpcy5fZGF0YSA9IHtcclxuXHRcdFx0Y29yZSA6IHtcclxuXHRcdFx0XHR0aGVtZXMgOiB7XHJcblx0XHRcdFx0XHRuYW1lIDogZmFsc2UsXHJcblx0XHRcdFx0XHRkb3RzIDogZmFsc2UsXHJcblx0XHRcdFx0XHRpY29ucyA6IGZhbHNlXHJcblx0XHRcdFx0fSxcclxuXHRcdFx0XHRzZWxlY3RlZCA6IFtdLFxyXG5cdFx0XHRcdGxhc3RfZXJyb3IgOiB7fVxyXG5cdFx0XHR9XHJcblx0XHR9O1xyXG5cdH07XHJcblx0LyoqXHJcblx0ICogZ2V0IGEgcmVmZXJlbmNlIHRvIGFuIGV4aXN0aW5nIGluc3RhbmNlXHJcblx0ICpcclxuXHQgKiBfX0V4YW1wbGVzX19cclxuXHQgKlxyXG5cdCAqXHQvLyBwcm92aWRlZCBhIGNvbnRhaW5lciB3aXRoIGFuIElEIG9mIFwidHJlZVwiLCBhbmQgYSBuZXN0ZWQgbm9kZSB3aXRoIGFuIElEIG9mIFwiYnJhbmNoXCJcclxuXHQgKlx0Ly8gYWxsIG9mIHRoZXJlIHdpbGwgcmV0dXJuIHRoZSBzYW1lIGluc3RhbmNlXHJcblx0ICpcdCQuanN0cmVlLnJlZmVyZW5jZSgndHJlZScpO1xyXG5cdCAqXHQkLmpzdHJlZS5yZWZlcmVuY2UoJyN0cmVlJyk7XHJcblx0ICpcdCQuanN0cmVlLnJlZmVyZW5jZSgkKCcjdHJlZScpKTtcclxuXHQgKlx0JC5qc3RyZWUucmVmZXJlbmNlKGRvY3VtZW50LmdldEVsZW1lbnRCeUlEKCd0cmVlJykpO1xyXG5cdCAqXHQkLmpzdHJlZS5yZWZlcmVuY2UoJ2JyYW5jaCcpO1xyXG5cdCAqXHQkLmpzdHJlZS5yZWZlcmVuY2UoJyNicmFuY2gnKTtcclxuXHQgKlx0JC5qc3RyZWUucmVmZXJlbmNlKCQoJyNicmFuY2gnKSk7XHJcblx0ICpcdCQuanN0cmVlLnJlZmVyZW5jZShkb2N1bWVudC5nZXRFbGVtZW50QnlJRCgnYnJhbmNoJykpO1xyXG5cdCAqXHJcblx0ICogQG5hbWUgJC5qc3RyZWUucmVmZXJlbmNlKG5lZWRsZSlcclxuXHQgKiBAcGFyYW0ge0RPTUVsZW1lbnR8alF1ZXJ5fFN0cmluZ30gbmVlZGxlXHJcblx0ICogQHJldHVybiB7anNUcmVlfG51bGx9IHRoZSBpbnN0YW5jZSBvciBgbnVsbGAgaWYgbm90IGZvdW5kXHJcblx0ICovXHJcblx0JC5qc3RyZWUucmVmZXJlbmNlID0gZnVuY3Rpb24gKG5lZWRsZSkge1xyXG5cdFx0aWYobmVlZGxlICYmICEkKG5lZWRsZSkubGVuZ3RoKSB7XHJcblx0XHRcdGlmKG5lZWRsZS5pZCkge1xyXG5cdFx0XHRcdG5lZWRsZSA9IG5lZWRsZS5pZDtcclxuXHRcdFx0fVxyXG5cdFx0XHR2YXIgdG1wID0gbnVsbDtcclxuXHRcdFx0JCgnLmpzdHJlZScpLmVhY2goZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdHZhciBpbnN0ID0gJCh0aGlzKS5kYXRhKCdqc3RyZWUnKTtcclxuXHRcdFx0XHRpZihpbnN0ICYmIGluc3QuX21vZGVsLmRhdGFbbmVlZGxlXSkge1xyXG5cdFx0XHRcdFx0dG1wID0gaW5zdDtcclxuXHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cdFx0XHRyZXR1cm4gdG1wO1xyXG5cdFx0fVxyXG5cdFx0cmV0dXJuICQobmVlZGxlKS5jbG9zZXN0KCcuanN0cmVlJykuZGF0YSgnanN0cmVlJyk7XHJcblx0fTtcclxuXHQvKipcclxuXHQgKiBDcmVhdGUgYW4gaW5zdGFuY2UsIGdldCBhbiBpbnN0YW5jZSBvciBpbnZva2UgYSBjb21tYW5kIG9uIGEgaW5zdGFuY2UuIFxyXG5cdCAqIFxyXG5cdCAqIElmIHRoZXJlIGlzIG5vIGluc3RhbmNlIGFzc29jaWF0ZWQgd2l0aCB0aGUgY3VycmVudCBub2RlIGEgbmV3IG9uZSBpcyBjcmVhdGVkIGFuZCBgYXJnYCBpcyB1c2VkIHRvIGV4dGVuZCBgJC5qc3RyZWUuZGVmYXVsdHNgIGZvciB0aGlzIG5ldyBpbnN0YW5jZS4gVGhlcmUgd291bGQgYmUgbm8gcmV0dXJuIHZhbHVlIChjaGFpbmluZyBpcyBub3QgYnJva2VuKS5cclxuXHQgKiBcclxuXHQgKiBJZiB0aGVyZSBpcyBhbiBleGlzdGluZyBpbnN0YW5jZSBhbmQgYGFyZ2AgaXMgYSBzdHJpbmcgdGhlIGNvbW1hbmQgc3BlY2lmaWVkIGJ5IGBhcmdgIGlzIGV4ZWN1dGVkIG9uIHRoZSBpbnN0YW5jZSwgd2l0aCBhbnkgYWRkaXRpb25hbCBhcmd1bWVudHMgcGFzc2VkIHRvIHRoZSBmdW5jdGlvbi4gSWYgdGhlIGZ1bmN0aW9uIHJldHVybnMgYSB2YWx1ZSBpdCB3aWxsIGJlIHJldHVybmVkIChjaGFpbmluZyBjb3VsZCBicmVhayBkZXBlbmRpbmcgb24gZnVuY3Rpb24pLlxyXG5cdCAqIFxyXG5cdCAqIElmIHRoZXJlIGlzIGFuIGV4aXN0aW5nIGluc3RhbmNlIGFuZCBgYXJnYCBpcyBub3QgYSBzdHJpbmcgdGhlIGluc3RhbmNlIGl0c2VsZiBpcyByZXR1cm5lZCAoc2ltaWxhciB0byBgJC5qc3RyZWUucmVmZXJlbmNlYCkuXHJcblx0ICogXHJcblx0ICogSW4gYW55IG90aGVyIGNhc2UgLSBub3RoaW5nIGlzIHJldHVybmVkIGFuZCBjaGFpbmluZyBpcyBub3QgYnJva2VuLlxyXG5cdCAqXHJcblx0ICogX19FeGFtcGxlc19fXHJcblx0ICpcclxuXHQgKlx0JCgnI3RyZWUxJykuanN0cmVlKCk7IC8vIGNyZWF0ZXMgYW4gaW5zdGFuY2VcclxuXHQgKlx0JCgnI3RyZWUyJykuanN0cmVlKHsgcGx1Z2lucyA6IFtdIH0pOyAvLyBjcmVhdGUgYW4gaW5zdGFuY2Ugd2l0aCBzb21lIG9wdGlvbnNcclxuXHQgKlx0JCgnI3RyZWUxJykuanN0cmVlKCdvcGVuX25vZGUnLCAnI2JyYW5jaF8xJyk7IC8vIGNhbGwgYSBtZXRob2Qgb24gYW4gZXhpc3RpbmcgaW5zdGFuY2UsIHBhc3NpbmcgYWRkaXRpb25hbCBhcmd1bWVudHNcclxuXHQgKlx0JCgnI3RyZWUyJykuanN0cmVlKCk7IC8vIGdldCBhbiBleGlzdGluZyBpbnN0YW5jZSAob3IgY3JlYXRlIGFuIGluc3RhbmNlKVxyXG5cdCAqXHQkKCcjdHJlZTInKS5qc3RyZWUodHJ1ZSk7IC8vIGdldCBhbiBleGlzdGluZyBpbnN0YW5jZSAod2lsbCBub3QgY3JlYXRlIG5ldyBpbnN0YW5jZSlcclxuXHQgKlx0JCgnI2JyYW5jaF8xJykuanN0cmVlKCkuc2VsZWN0X25vZGUoJyNicmFuY2hfMScpOyAvLyBnZXQgYW4gaW5zdGFuY2UgKHVzaW5nIGEgbmVzdGVkIGVsZW1lbnQgYW5kIGNhbGwgYSBtZXRob2QpXHJcblx0ICpcclxuXHQgKiBAbmFtZSAkKCkuanN0cmVlKFthcmddKVxyXG5cdCAqIEBwYXJhbSB7U3RyaW5nfE9iamVjdH0gYXJnXHJcblx0ICogQHJldHVybiB7TWl4ZWR9XHJcblx0ICovXHJcblx0JC5mbi5qc3RyZWUgPSBmdW5jdGlvbiAoYXJnKSB7XHJcblx0XHQvLyBjaGVjayBmb3Igc3RyaW5nIGFyZ3VtZW50XHJcblx0XHR2YXIgaXNfbWV0aG9kXHQ9ICh0eXBlb2YgYXJnID09PSAnc3RyaW5nJyksXHJcblx0XHRcdGFyZ3NcdFx0PSBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChhcmd1bWVudHMsIDEpLFxyXG5cdFx0XHRyZXN1bHRcdFx0PSBudWxsO1xyXG5cdFx0dGhpcy5lYWNoKGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0Ly8gZ2V0IHRoZSBpbnN0YW5jZSAoaWYgdGhlcmUgaXMgb25lKSBhbmQgbWV0aG9kIChpZiBpdCBleGlzdHMpXHJcblx0XHRcdHZhciBpbnN0YW5jZSA9ICQuanN0cmVlLnJlZmVyZW5jZSh0aGlzKSxcclxuXHRcdFx0XHRtZXRob2QgPSBpc19tZXRob2QgJiYgaW5zdGFuY2UgPyBpbnN0YW5jZVthcmddIDogbnVsbDtcclxuXHRcdFx0Ly8gaWYgY2FsbGluZyBhIG1ldGhvZCwgYW5kIG1ldGhvZCBpcyBhdmFpbGFibGUgLSBleGVjdXRlIG9uIHRoZSBpbnN0YW5jZVxyXG5cdFx0XHRyZXN1bHQgPSBpc19tZXRob2QgJiYgbWV0aG9kID9cclxuXHRcdFx0XHRtZXRob2QuYXBwbHkoaW5zdGFuY2UsIGFyZ3MpIDpcclxuXHRcdFx0XHRudWxsO1xyXG5cdFx0XHQvLyBpZiB0aGVyZSBpcyBubyBpbnN0YW5jZSBhbmQgbm8gbWV0aG9kIGlzIGJlaW5nIGNhbGxlZCAtIGNyZWF0ZSBvbmVcclxuXHRcdFx0aWYoIWluc3RhbmNlICYmICFpc19tZXRob2QgJiYgKGFyZyA9PT0gdW5kZWZpbmVkIHx8ICQuaXNQbGFpbk9iamVjdChhcmcpKSkge1xyXG5cdFx0XHRcdCQodGhpcykuZGF0YSgnanN0cmVlJywgbmV3ICQuanN0cmVlLmNyZWF0ZSh0aGlzLCBhcmcpKTtcclxuXHRcdFx0fVxyXG5cdFx0XHQvLyBpZiB0aGVyZSBpcyBhbiBpbnN0YW5jZSBhbmQgbm8gbWV0aG9kIGlzIGNhbGxlZCAtIHJldHVybiB0aGUgaW5zdGFuY2VcclxuXHRcdFx0aWYoaW5zdGFuY2UgJiYgIWlzX21ldGhvZCkge1xyXG5cdFx0XHRcdHJlc3VsdCA9IGluc3RhbmNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdC8vIGlmIHRoZXJlIHdhcyBhIG1ldGhvZCBjYWxsIHdoaWNoIHJldHVybmVkIGEgcmVzdWx0IC0gYnJlYWsgYW5kIHJldHVybiB0aGUgdmFsdWVcclxuXHRcdFx0aWYocmVzdWx0ICE9PSBudWxsICYmIHJlc3VsdCAhPT0gdW5kZWZpbmVkKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHR9KTtcclxuXHRcdC8vIGlmIHRoZXJlIHdhcyBhIG1ldGhvZCBjYWxsIHdpdGggYSB2YWxpZCByZXR1cm4gdmFsdWUgLSByZXR1cm4gdGhhdCwgb3RoZXJ3aXNlIGNvbnRpbnVlIHRoZSBjaGFpblxyXG5cdFx0cmV0dXJuIHJlc3VsdCAhPT0gbnVsbCAmJiByZXN1bHQgIT09IHVuZGVmaW5lZCA/XHJcblx0XHRcdHJlc3VsdCA6IHRoaXM7XHJcblx0fTtcclxuXHQvKipcclxuXHQgKiB1c2VkIHRvIGZpbmQgZWxlbWVudHMgY29udGFpbmluZyBhbiBpbnN0YW5jZVxyXG5cdCAqXHJcblx0ICogX19FeGFtcGxlc19fXHJcblx0ICpcclxuXHQgKlx0JCgnZGl2OmpzdHJlZScpLmVhY2goZnVuY3Rpb24gKCkge1xyXG5cdCAqXHRcdCQodGhpcykuanN0cmVlKCdkZXN0cm95Jyk7XHJcblx0ICpcdH0pO1xyXG5cdCAqXHJcblx0ICogQG5hbWUgJCgnOmpzdHJlZScpXHJcblx0ICogQHJldHVybiB7alF1ZXJ5fVxyXG5cdCAqL1xyXG5cdCQuZXhwclsnOiddLmpzdHJlZSA9ICQuZXhwci5jcmVhdGVQc2V1ZG8oZnVuY3Rpb24oc2VhcmNoKSB7XHJcblx0XHRyZXR1cm4gZnVuY3Rpb24oYSkge1xyXG5cdFx0XHRyZXR1cm4gJChhKS5oYXNDbGFzcygnanN0cmVlJykgJiZcclxuXHRcdFx0XHQkKGEpLmRhdGEoJ2pzdHJlZScpICE9PSB1bmRlZmluZWQ7XHJcblx0XHR9O1xyXG5cdH0pO1xyXG5cclxuXHQvKipcclxuXHQgKiBzdG9yZXMgYWxsIGRlZmF1bHRzIGZvciB0aGUgY29yZVxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvcmVcclxuXHQgKi9cclxuXHQkLmpzdHJlZS5kZWZhdWx0cy5jb3JlID0ge1xyXG5cdFx0LyoqXHJcblx0XHQgKiBkYXRhIGNvbmZpZ3VyYXRpb25cclxuXHRcdCAqIFxyXG5cdFx0ICogSWYgbGVmdCBhcyBgZmFsc2VgIHRoZSBIVE1MIGluc2lkZSB0aGUganN0cmVlIGNvbnRhaW5lciBlbGVtZW50IGlzIHVzZWQgdG8gcG9wdWxhdGUgdGhlIHRyZWUgKHRoYXQgc2hvdWxkIGJlIGFuIHVub3JkZXJlZCBsaXN0IHdpdGggbGlzdCBpdGVtcykuXHJcblx0XHQgKlxyXG5cdFx0ICogWW91IGNhbiBhbHNvIHBhc3MgaW4gYSBIVE1MIHN0cmluZyBvciBhIEpTT04gYXJyYXkgaGVyZS5cclxuXHRcdCAqIFxyXG5cdFx0ICogSXQgaXMgcG9zc2libGUgdG8gcGFzcyBpbiBhIHN0YW5kYXJkIGpRdWVyeS1saWtlIEFKQVggY29uZmlnIGFuZCBqc3RyZWUgd2lsbCBhdXRvbWF0aWNhbGx5IGRldGVybWluZSBpZiB0aGUgcmVzcG9uc2UgaXMgSlNPTiBvciBIVE1MIGFuZCB1c2UgdGhhdCB0byBwb3B1bGF0ZSB0aGUgdHJlZS4gXHJcblx0XHQgKiBJbiBhZGRpdGlvbiB0byB0aGUgc3RhbmRhcmQgalF1ZXJ5IGFqYXggb3B0aW9ucyBoZXJlIHlvdSBjYW4gc3VwcHkgZnVuY3Rpb25zIGZvciBgZGF0YWAgYW5kIGB1cmxgLCB0aGUgZnVuY3Rpb25zIHdpbGwgYmUgcnVuIGluIHRoZSBjdXJyZW50IGluc3RhbmNlJ3Mgc2NvcGUgYW5kIGEgcGFyYW0gd2lsbCBiZSBwYXNzZWQgaW5kaWNhdGluZyB3aGljaCBub2RlIGlzIGJlaW5nIGxvYWRlZCwgdGhlIHJldHVybiB2YWx1ZSBvZiB0aG9zZSBmdW5jdGlvbnMgd2lsbCBiZSB1c2VkLlxyXG5cdFx0ICogXHJcblx0XHQgKiBUaGUgbGFzdCBvcHRpb24gaXMgdG8gc3BlY2lmeSBhIGZ1bmN0aW9uLCB0aGF0IGZ1bmN0aW9uIHdpbGwgcmVjZWl2ZSB0aGUgbm9kZSBiZWluZyBsb2FkZWQgYXMgYXJndW1lbnQgYW5kIGEgc2Vjb25kIHBhcmFtIHdoaWNoIGlzIGEgZnVuY3Rpb24gd2hpY2ggc2hvdWxkIGJlIGNhbGxlZCB3aXRoIHRoZSByZXN1bHQuXHJcblx0XHQgKlxyXG5cdFx0ICogX19FeGFtcGxlc19fXHJcblx0XHQgKlxyXG5cdFx0ICpcdC8vIEFKQVhcclxuXHRcdCAqXHQkKCcjdHJlZScpLmpzdHJlZSh7XHJcblx0XHQgKlx0XHQnY29yZScgOiB7XHJcblx0XHQgKlx0XHRcdCdkYXRhJyA6IHtcclxuXHRcdCAqXHRcdFx0XHQndXJsJyA6ICcvZ2V0L2NoaWxkcmVuLycsXHJcblx0XHQgKlx0XHRcdFx0J2RhdGEnIDogZnVuY3Rpb24gKG5vZGUpIHtcclxuXHRcdCAqXHRcdFx0XHRcdHJldHVybiB7ICdpZCcgOiBub2RlLmlkIH07XHJcblx0XHQgKlx0XHRcdFx0fVxyXG5cdFx0ICpcdFx0XHR9XHJcblx0XHQgKlx0XHR9KTtcclxuXHRcdCAqXHJcblx0XHQgKlx0Ly8gZGlyZWN0IGRhdGFcclxuXHRcdCAqXHQkKCcjdHJlZScpLmpzdHJlZSh7XHJcblx0XHQgKlx0XHQnY29yZScgOiB7XHJcblx0XHQgKlx0XHRcdCdkYXRhJyA6IFtcclxuXHRcdCAqXHRcdFx0XHQnU2ltcGxlIHJvb3Qgbm9kZScsXHJcblx0XHQgKlx0XHRcdFx0e1xyXG5cdFx0ICpcdFx0XHRcdFx0J2lkJyA6ICdub2RlXzInLFxyXG5cdFx0ICpcdFx0XHRcdFx0J3RleHQnIDogJ1Jvb3Qgbm9kZSB3aXRoIG9wdGlvbnMnLFxyXG5cdFx0ICpcdFx0XHRcdFx0J3N0YXRlJyA6IHsgJ29wZW5lZCcgOiB0cnVlLCAnc2VsZWN0ZWQnIDogdHJ1ZSB9LFxyXG5cdFx0ICpcdFx0XHRcdFx0J2NoaWxkcmVuJyA6IFsgeyAndGV4dCcgOiAnQ2hpbGQgMScgfSwgJ0NoaWxkIDInXVxyXG5cdFx0ICpcdFx0XHRcdH1cclxuXHRcdCAqXHRcdFx0XVxyXG5cdFx0ICpcdFx0fSk7XHJcblx0XHQgKlx0XHJcblx0XHQgKlx0Ly8gZnVuY3Rpb25cclxuXHRcdCAqXHQkKCcjdHJlZScpLmpzdHJlZSh7XHJcblx0XHQgKlx0XHQnY29yZScgOiB7XHJcblx0XHQgKlx0XHRcdCdkYXRhJyA6IGZ1bmN0aW9uIChvYmosIGNhbGxiYWNrKSB7XHJcblx0XHQgKlx0XHRcdFx0Y2FsbGJhY2suY2FsbCh0aGlzLCBbJ1Jvb3QgMScsICdSb290IDInXSk7XHJcblx0XHQgKlx0XHRcdH1cclxuXHRcdCAqXHRcdH0pO1xyXG5cdFx0ICogXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb3JlLmRhdGFcclxuXHRcdCAqL1xyXG5cdFx0ZGF0YVx0XHRcdDogZmFsc2UsXHJcblx0XHQvKipcclxuXHRcdCAqIGNvbmZpZ3VyZSB0aGUgdmFyaW91cyBzdHJpbmdzIHVzZWQgdGhyb3VnaG91dCB0aGUgdHJlZVxyXG5cdFx0ICpcclxuXHRcdCAqIFlvdSBjYW4gdXNlIGFuIG9iamVjdCB3aGVyZSB0aGUga2V5IGlzIHRoZSBzdHJpbmcgeW91IG5lZWQgdG8gcmVwbGFjZSBhbmQgdGhlIHZhbHVlIGlzIHlvdXIgcmVwbGFjZW1lbnQuXHJcblx0XHQgKiBBbm90aGVyIG9wdGlvbiBpcyB0byBzcGVjaWZ5IGEgZnVuY3Rpb24gd2hpY2ggd2lsbCBiZSBjYWxsZWQgd2l0aCBhbiBhcmd1bWVudCBvZiB0aGUgbmVlZGVkIHN0cmluZyBhbmQgc2hvdWxkIHJldHVybiB0aGUgcmVwbGFjZW1lbnQuXHJcblx0XHQgKiBJZiBsZWZ0IGFzIGBmYWxzZWAgbm8gcmVwbGFjZW1lbnQgaXMgbWFkZS5cclxuXHRcdCAqXHJcblx0XHQgKiBfX0V4YW1wbGVzX19cclxuXHRcdCAqXHJcblx0XHQgKlx0JCgnI3RyZWUnKS5qc3RyZWUoe1xyXG5cdFx0ICpcdFx0J2NvcmUnIDoge1xyXG5cdFx0ICpcdFx0XHQnc3RyaW5ncycgOiB7XHJcblx0XHQgKlx0XHRcdFx0J0xvYWRpbmcuLi4nIDogJ1BsZWFzZSB3YWl0IC4uLidcclxuXHRcdCAqXHRcdFx0fVxyXG5cdFx0ICpcdFx0fVxyXG5cdFx0ICpcdH0pO1xyXG5cdFx0ICpcclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvcmUuc3RyaW5nc1xyXG5cdFx0ICovXHJcblx0XHRzdHJpbmdzXHRcdFx0OiBmYWxzZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZGV0ZXJtaW5lcyB3aGF0IGhhcHBlbnMgd2hlbiBhIHVzZXIgdHJpZXMgdG8gbW9kaWZ5IHRoZSBzdHJ1Y3R1cmUgb2YgdGhlIHRyZWVcclxuXHRcdCAqIElmIGxlZnQgYXMgYGZhbHNlYCBhbGwgb3BlcmF0aW9ucyBsaWtlIGNyZWF0ZSwgcmVuYW1lLCBkZWxldGUsIG1vdmUgb3IgY29weSBhcmUgcHJldmVudGVkLlxyXG5cdFx0ICogWW91IGNhbiBzZXQgdGhpcyB0byBgdHJ1ZWAgdG8gYWxsb3cgYWxsIGludGVyYWN0aW9ucyBvciB1c2UgYSBmdW5jdGlvbiB0byBoYXZlIGJldHRlciBjb250cm9sLlxyXG5cdFx0ICpcclxuXHRcdCAqIF9fRXhhbXBsZXNfX1xyXG5cdFx0ICpcclxuXHRcdCAqXHQkKCcjdHJlZScpLmpzdHJlZSh7XHJcblx0XHQgKlx0XHQnY29yZScgOiB7XHJcblx0XHQgKlx0XHRcdCdjaGVja19jYWxsYmFjaycgOiBmdW5jdGlvbiAob3BlcmF0aW9uLCBub2RlLCBub2RlX3BhcmVudCwgbm9kZV9wb3NpdGlvbikge1xyXG5cdFx0ICpcdFx0XHRcdC8vIG9wZXJhdGlvbiBjYW4gYmUgJ2NyZWF0ZV9ub2RlJywgJ3JlbmFtZV9ub2RlJywgJ2RlbGV0ZV9ub2RlJywgJ21vdmVfbm9kZScgb3IgJ2NvcHlfbm9kZSdcclxuXHRcdCAqXHRcdFx0XHQvLyBpbiBjYXNlIG9mICdyZW5hbWVfbm9kZScgbm9kZV9wb3NpdGlvbiBpcyBmaWxsZWQgd2l0aCB0aGUgbmV3IG5vZGUgbmFtZVxyXG5cdFx0ICpcdFx0XHRcdHJldHVybiBvcGVyYXRpb24gPT09ICdyZW5hbWVfbm9kZScgPyB0cnVlIDogZmFsc2U7XHJcblx0XHQgKlx0XHRcdH1cclxuXHRcdCAqXHRcdH1cclxuXHRcdCAqXHR9KTtcclxuXHRcdCAqIFxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS5jaGVja19jYWxsYmFja1xyXG5cdFx0ICovXHJcblx0XHRjaGVja19jYWxsYmFja1x0OiBmYWxzZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogYSBjYWxsYmFjayBjYWxsZWQgd2l0aCBhIHNpbmdsZSBvYmplY3QgcGFyYW1ldGVyIGluIHRoZSBpbnN0YW5jZSdzIHNjb3BlIHdoZW4gc29tZXRoaW5nIGdvZXMgd3JvbmcgKG9wZXJhdGlvbiBwcmV2ZW50ZWQsIGFqYXggZmFpbGVkLCBldGMpXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb3JlLmVycm9yXHJcblx0XHQgKi9cclxuXHRcdGVycm9yXHRcdFx0OiAkLm5vb3AsXHJcblx0XHQvKipcclxuXHRcdCAqIHRoZSBvcGVuIC8gY2xvc2UgYW5pbWF0aW9uIGR1cmF0aW9uIGluIG1pbGxpc2Vjb25kcyAtIHNldCB0aGlzIHRvIGBmYWxzZWAgdG8gZGlzYWJsZSB0aGUgYW5pbWF0aW9uIChkZWZhdWx0IGlzIGAyMDBgKVxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS5hbmltYXRpb25cclxuXHRcdCAqL1xyXG5cdFx0YW5pbWF0aW9uXHRcdDogMjAwLFxyXG5cdFx0LyoqXHJcblx0XHQgKiBhIGJvb2xlYW4gaW5kaWNhdGluZyBpZiBtdWx0aXBsZSBub2RlcyBjYW4gYmUgc2VsZWN0ZWRcclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvcmUubXVsdGlwbGVcclxuXHRcdCAqL1xyXG5cdFx0bXVsdGlwbGVcdFx0OiB0cnVlLFxyXG5cdFx0LyoqXHJcblx0XHQgKiB0aGVtZSBjb25maWd1cmF0aW9uIG9iamVjdFxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS50aGVtZXNcclxuXHRcdCAqL1xyXG5cdFx0dGhlbWVzXHRcdFx0OiB7XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0aGUgbmFtZSBvZiB0aGUgdGhlbWUgdG8gdXNlIChpZiBsZWZ0IGFzIGBmYWxzZWAgdGhlIGRlZmF1bHQgdGhlbWUgaXMgdXNlZClcclxuXHRcdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS50aGVtZXMubmFtZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0bmFtZVx0XHRcdDogZmFsc2UsXHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0aGUgVVJMIG9mIHRoZSB0aGVtZSdzIENTUyBmaWxlLCBsZWF2ZSB0aGlzIGFzIGBmYWxzZWAgaWYgeW91IGhhdmUgbWFudWFsbHkgaW5jbHVkZWQgdGhlIHRoZW1lIENTUyAocmVjb21tZW5kZWQpLiBZb3UgY2FuIHNldCB0aGlzIHRvIGB0cnVlYCB0b28gd2hpY2ggd2lsbCB0cnkgdG8gYXV0b2xvYWQgdGhlIHRoZW1lLlxyXG5cdFx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb3JlLnRoZW1lcy51cmxcclxuXHRcdFx0ICovXHJcblx0XHRcdHVybFx0XHRcdFx0OiBmYWxzZSxcclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRoZSBsb2NhdGlvbiBvZiBhbGwganN0cmVlIHRoZW1lcyAtIG9ubHkgdXNlZCBpZiBgdXJsYCBpcyBzZXQgdG8gYHRydWVgXHJcblx0XHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvcmUudGhlbWVzLmRpclxyXG5cdFx0XHQgKi9cclxuXHRcdFx0ZGlyXHRcdFx0XHQ6IGZhbHNlLFxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogYSBib29sZWFuIGluZGljYXRpbmcgaWYgY29ubmVjdGluZyBkb3RzIGFyZSBzaG93blxyXG5cdFx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb3JlLnRoZW1lcy5kb3RzXHJcblx0XHRcdCAqL1xyXG5cdFx0XHRkb3RzXHRcdFx0OiB0cnVlLFxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogYSBib29sZWFuIGluZGljYXRpbmcgaWYgbm9kZSBpY29ucyBhcmUgc2hvd25cclxuXHRcdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS50aGVtZXMuaWNvbnNcclxuXHRcdFx0ICovXHJcblx0XHRcdGljb25zXHRcdFx0OiB0cnVlLFxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogYSBib29sZWFuIGluZGljYXRpbmcgaWYgdGhlIHRyZWUgYmFja2dyb3VuZCBpcyBzdHJpcGVkXHJcblx0XHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvcmUudGhlbWVzLnN0cmlwZXNcclxuXHRcdFx0ICovXHJcblx0XHRcdHN0cmlwZXNcdFx0XHQ6IGZhbHNlLFxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogYSBzdHJpbmcgKG9yIGJvb2xlYW4gYGZhbHNlYCkgc3BlY2lmeWluZyB0aGUgdGhlbWUgdmFyaWFudCB0byB1c2UgKGlmIHRoZSB0aGVtZSBzdXBwb3J0cyB2YXJpYW50cylcclxuXHRcdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS50aGVtZXMudmFyaWFudFxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dmFyaWFudFx0XHRcdDogZmFsc2UsXHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiBhIGJvb2xlYW4gc3BlY2lmeWluZyBpZiBhIHJlcG9uc2l2ZSB2ZXJzaW9uIG9mIHRoZSB0aGVtZSBzaG91bGQga2ljayBpbiBvbiBzbWFsbGVyIHNjcmVlbnMgKGlmIHRoZSB0aGVtZSBzdXBwb3J0cyBpdCkuIERlZmF1bHRzIHRvIGB0cnVlYC5cclxuXHRcdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY29yZS50aGVtZXMucmVzcG9uc2l2ZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0cmVzcG9uc2l2ZVx0XHQ6IHRydWVcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGlmIGxlZnQgYXMgYHRydWVgIGFsbCBwYXJlbnRzIG9mIGFsbCBzZWxlY3RlZCBub2RlcyB3aWxsIGJlIG9wZW5lZCBvbmNlIHRoZSB0cmVlIGxvYWRzIChzbyB0aGF0IGFsbCBzZWxlY3RlZCBub2RlcyBhcmUgdmlzaWJsZSB0byB0aGUgdXNlcilcclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvcmUuZXhwYW5kX3NlbGVjdGVkX29ubG9hZFxyXG5cdFx0ICovXHJcblx0XHRleHBhbmRfc2VsZWN0ZWRfb25sb2FkIDogdHJ1ZVxyXG5cdH07XHJcblx0JC5qc3RyZWUuY29yZS5wcm90b3R5cGUgPSB7XHJcblx0XHQvKipcclxuXHRcdCAqIHVzZWQgdG8gZGVjb3JhdGUgYW4gaW5zdGFuY2Ugd2l0aCBhIHBsdWdpbi4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIHBsdWdpbihkZWNvIFssIG9wdHNdKVxyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBkZWNvIHRoZSBwbHVnaW4gdG8gZGVjb3JhdGUgd2l0aFxyXG5cdFx0ICogQHBhcmFtICB7T2JqZWN0fSBvcHRzIG9wdGlvbnMgZm9yIHRoZSBwbHVnaW5cclxuXHRcdCAqIEByZXR1cm4ge2pzVHJlZX1cclxuXHRcdCAqL1xyXG5cdFx0cGx1Z2luIDogZnVuY3Rpb24gKGRlY28sIG9wdHMpIHtcclxuXHRcdFx0dmFyIENoaWxkID0gJC5qc3RyZWUucGx1Z2luc1tkZWNvXTtcclxuXHRcdFx0aWYoQ2hpbGQpIHtcclxuXHRcdFx0XHR0aGlzLl9kYXRhW2RlY29dID0ge307XHJcblx0XHRcdFx0Q2hpbGQucHJvdG90eXBlID0gdGhpcztcclxuXHRcdFx0XHRyZXR1cm4gbmV3IENoaWxkKG9wdHMsIHRoaXMpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB0aGlzO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogdXNlZCB0byBkZWNvcmF0ZSBhbiBpbnN0YW5jZSB3aXRoIGEgcGx1Z2luLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgaW5pdChlbCwgb3B0b25zKVxyXG5cdFx0ICogQHBhcmFtIHtET01FbGVtZW50fGpRdWVyeXxTdHJpbmd9IGVsIHRoZSBlbGVtZW50IHdlIGFyZSB0cmFuc2Zvcm1pbmdcclxuXHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zIG9wdGlvbnMgZm9yIHRoaXMgaW5zdGFuY2VcclxuXHRcdCAqIEB0cmlnZ2VyIGluaXQuanN0cmVlLCBsb2FkaW5nLmpzdHJlZSwgbG9hZGVkLmpzdHJlZSwgcmVhZHkuanN0cmVlLCBjaGFuZ2VkLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRpbml0IDogZnVuY3Rpb24gKGVsLCBvcHRpb25zKSB7XHJcblx0XHRcdHRoaXMuX21vZGVsID0ge1xyXG5cdFx0XHRcdGRhdGEgOiB7XHJcblx0XHRcdFx0XHQnIycgOiB7XHJcblx0XHRcdFx0XHRcdGlkIDogJyMnLFxyXG5cdFx0XHRcdFx0XHRwYXJlbnQgOiBudWxsLFxyXG5cdFx0XHRcdFx0XHRwYXJlbnRzIDogW10sXHJcblx0XHRcdFx0XHRcdGNoaWxkcmVuIDogW10sXHJcblx0XHRcdFx0XHRcdGNoaWxkcmVuX2QgOiBbXSxcclxuXHRcdFx0XHRcdFx0c3RhdGUgOiB7IGxvYWRlZCA6IGZhbHNlIH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9LFxyXG5cdFx0XHRcdGNoYW5nZWQgOiBbXSxcclxuXHRcdFx0XHRmb3JjZV9mdWxsX3JlZHJhdyA6IGZhbHNlLFxyXG5cdFx0XHRcdHJlZHJhd190aW1lb3V0IDogZmFsc2UsXHJcblx0XHRcdFx0ZGVmYXVsdF9zdGF0ZSA6IHtcclxuXHRcdFx0XHRcdGxvYWRlZCA6IHRydWUsXHJcblx0XHRcdFx0XHRvcGVuZWQgOiBmYWxzZSxcclxuXHRcdFx0XHRcdHNlbGVjdGVkIDogZmFsc2UsXHJcblx0XHRcdFx0XHRkaXNhYmxlZCA6IGZhbHNlXHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9O1xyXG5cclxuXHRcdFx0dGhpcy5lbGVtZW50ID0gJChlbCkuYWRkQ2xhc3MoJ2pzdHJlZSBqc3RyZWUtJyArIHRoaXMuX2lkKTtcclxuXHRcdFx0dGhpcy5zZXR0aW5ncyA9IG9wdGlvbnM7XHJcblx0XHRcdHRoaXMuZWxlbWVudC5iaW5kKFwiZGVzdHJveWVkXCIsICQucHJveHkodGhpcy50ZWFyZG93biwgdGhpcykpO1xyXG5cclxuXHRcdFx0dGhpcy5fZGF0YS5jb3JlLnJlYWR5ID0gZmFsc2U7XHJcblx0XHRcdHRoaXMuX2RhdGEuY29yZS5sb2FkZWQgPSBmYWxzZTtcclxuXHRcdFx0dGhpcy5fZGF0YS5jb3JlLnJ0bCA9ICh0aGlzLmVsZW1lbnQuY3NzKFwiZGlyZWN0aW9uXCIpID09PSBcInJ0bFwiKTtcclxuXHRcdFx0dGhpcy5lbGVtZW50W3RoaXMuX2RhdGEuY29yZS5ydGwgPyAnYWRkQ2xhc3MnIDogJ3JlbW92ZUNsYXNzJ10oXCJqc3RyZWUtcnRsXCIpO1xyXG5cdFx0XHR0aGlzLmVsZW1lbnQuYXR0cigncm9sZScsJ3RyZWUnKTtcclxuXHJcblx0XHRcdHRoaXMuYmluZCgpO1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIGFmdGVyIGFsbCBldmVudHMgYXJlIGJvdW5kXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSBpbml0LmpzdHJlZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKFwiaW5pdFwiKTtcclxuXHJcblx0XHRcdHRoaXMuX2RhdGEuY29yZS5vcmlnaW5hbF9jb250YWluZXJfaHRtbCA9IHRoaXMuZWxlbWVudC5maW5kKFwiID4gdWwgPiBsaVwiKS5jbG9uZSh0cnVlKTtcclxuXHRcdFx0dGhpcy5fZGF0YS5jb3JlLm9yaWdpbmFsX2NvbnRhaW5lcl9odG1sXHJcblx0XHRcdFx0LmZpbmQoXCJsaVwiKS5hZGRCYWNrKClcclxuXHRcdFx0XHQuY29udGVudHMoKS5maWx0ZXIoZnVuY3Rpb24oKSB7XHJcblx0XHRcdFx0XHRyZXR1cm4gdGhpcy5ub2RlVHlwZSA9PT0gMyAmJiAoIXRoaXMubm9kZVZhbHVlIHx8IC9eXFxzKyQvLnRlc3QodGhpcy5ub2RlVmFsdWUpKTtcclxuXHRcdFx0XHR9KVxyXG5cdFx0XHRcdC5yZW1vdmUoKTtcclxuXHRcdFx0dGhpcy5lbGVtZW50Lmh0bWwoXCI8XCIrXCJ1bCBjbGFzcz0nanN0cmVlLWNvbnRhaW5lci11bCc+PFwiK1wibGkgY2xhc3M9J2pzdHJlZS1pbml0aWFsLW5vZGUganN0cmVlLWxvYWRpbmcganN0cmVlLWxlYWYganN0cmVlLWxhc3QnPjxpIGNsYXNzPSdqc3RyZWUtaWNvbiBqc3RyZWUtb2NsJz48L2k+PFwiK1wiYSBjbGFzcz0nanN0cmVlLWFuY2hvcicgaHJlZj0nIyc+PGkgY2xhc3M9J2pzdHJlZS1pY29uIGpzdHJlZS10aGVtZWljb24taGlkZGVuJz48L2k+XCIgKyB0aGlzLmdldF9zdHJpbmcoXCJMb2FkaW5nIC4uLlwiKSArIFwiPC9hPjwvbGk+PC91bD5cIik7XHJcblx0XHRcdHRoaXMuX2RhdGEuY29yZS5saV9oZWlnaHQgPSB0aGlzLmdldF9jb250YWluZXJfdWwoKS5jaGlsZHJlbihcImxpOmVxKDApXCIpLmhlaWdodCgpIHx8IDE4O1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIGFmdGVyIHRoZSBsb2FkaW5nIHRleHQgaXMgc2hvd24gYW5kIGJlZm9yZSBsb2FkaW5nIHN0YXJ0c1xyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgbG9hZGluZy5qc3RyZWVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcihcImxvYWRpbmdcIik7XHJcblx0XHRcdHRoaXMubG9hZF9ub2RlKCcjJyk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBkZXN0cm95IGFuIGluc3RhbmNlXHJcblx0XHQgKiBAbmFtZSBkZXN0cm95KClcclxuXHRcdCAqL1xyXG5cdFx0ZGVzdHJveSA6IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0dGhpcy5lbGVtZW50LnVuYmluZChcImRlc3Ryb3llZFwiLCB0aGlzLnRlYXJkb3duKTtcclxuXHRcdFx0dGhpcy50ZWFyZG93bigpO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogcGFydCBvZiB0aGUgZGVzdHJveWluZyBvZiBhbiBpbnN0YW5jZS4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIHRlYXJkb3duKClcclxuXHRcdCAqL1xyXG5cdFx0dGVhcmRvd24gOiBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHRoaXMudW5iaW5kKCk7XHJcblx0XHRcdHRoaXMuZWxlbWVudFxyXG5cdFx0XHRcdC5yZW1vdmVDbGFzcygnanN0cmVlJylcclxuXHRcdFx0XHQucmVtb3ZlRGF0YSgnanN0cmVlJylcclxuXHRcdFx0XHQuZmluZChcIltjbGFzc149J2pzdHJlZSddXCIpXHJcblx0XHRcdFx0XHQuYWRkQmFjaygpXHJcblx0XHRcdFx0XHQuYXR0cihcImNsYXNzXCIsIGZ1bmN0aW9uICgpIHsgcmV0dXJuIHRoaXMuY2xhc3NOYW1lLnJlcGxhY2UoL2pzdHJlZVteIF0qfCQvaWcsJycpOyB9KTtcclxuXHRcdFx0dGhpcy5lbGVtZW50ID0gbnVsbDtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGJpbmQgYWxsIGV2ZW50cy4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIGJpbmQoKVxyXG5cdFx0ICovXHJcblx0XHRiaW5kIDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHQub24oXCJkYmxjbGljay5qc3RyZWVcIiwgZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdFx0XHRpZihkb2N1bWVudC5zZWxlY3Rpb24gJiYgZG9jdW1lbnQuc2VsZWN0aW9uLmVtcHR5KSB7XHJcblx0XHRcdFx0XHRcdFx0ZG9jdW1lbnQuc2VsZWN0aW9uLmVtcHR5KCk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdFx0aWYod2luZG93LmdldFNlbGVjdGlvbikge1xyXG5cdFx0XHRcdFx0XHRcdFx0dmFyIHNlbCA9IHdpbmRvdy5nZXRTZWxlY3Rpb24oKTtcclxuXHRcdFx0XHRcdFx0XHRcdHRyeSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHNlbC5yZW1vdmVBbGxSYW5nZXMoKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0c2VsLmNvbGxhcHNlKCk7XHJcblx0XHRcdFx0XHRcdFx0XHR9IGNhdGNoIChpZ25vcmUpIHsgfVxyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fSlcclxuXHRcdFx0XHQub24oXCJjbGljay5qc3RyZWVcIiwgXCIuanN0cmVlLW9jbFwiLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMudG9nZ2xlX25vZGUoZS50YXJnZXQpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwiY2xpY2suanN0cmVlXCIsIFwiLmpzdHJlZS1hbmNob3JcIiwgJC5wcm94eShmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XHJcblx0XHRcdFx0XHRcdCQoZS5jdXJyZW50VGFyZ2V0KS5mb2N1cygpO1xyXG5cdFx0XHRcdFx0XHR0aGlzLmFjdGl2YXRlX25vZGUoZS5jdXJyZW50VGFyZ2V0LCBlKTtcclxuXHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdC5vbigna2V5ZG93bi5qc3RyZWUnLCAnLmpzdHJlZS1hbmNob3InLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdHZhciBvID0gbnVsbDtcclxuXHRcdFx0XHRcdFx0c3dpdGNoKGUud2hpY2gpIHtcclxuXHRcdFx0XHRcdFx0XHRjYXNlIDEzOlxyXG5cdFx0XHRcdFx0XHRcdGNhc2UgMzI6XHJcblx0XHRcdFx0XHRcdFx0XHRlLnR5cGUgPSBcImNsaWNrXCI7XHJcblx0XHRcdFx0XHRcdFx0XHQkKGUuY3VycmVudFRhcmdldCkudHJpZ2dlcihlKTtcclxuXHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdGNhc2UgMzc6XHJcblx0XHRcdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XHJcblx0XHRcdFx0XHRcdFx0XHRpZih0aGlzLmlzX29wZW4oZS5jdXJyZW50VGFyZ2V0KSkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR0aGlzLmNsb3NlX25vZGUoZS5jdXJyZW50VGFyZ2V0KTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRvID0gdGhpcy5nZXRfcHJldl9kb20oZS5jdXJyZW50VGFyZ2V0KTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYobyAmJiBvLmxlbmd0aCkgeyBvLmNoaWxkcmVuKCcuanN0cmVlLWFuY2hvcicpLmZvY3VzKCk7IH1cclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdGNhc2UgMzg6XHJcblx0XHRcdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XHJcblx0XHRcdFx0XHRcdFx0XHRvID0gdGhpcy5nZXRfcHJldl9kb20oZS5jdXJyZW50VGFyZ2V0KTtcclxuXHRcdFx0XHRcdFx0XHRcdGlmKG8gJiYgby5sZW5ndGgpIHsgby5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5mb2N1cygpOyB9XHJcblx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRjYXNlIDM5OlxyXG5cdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0aWYodGhpcy5pc19jbG9zZWQoZS5jdXJyZW50VGFyZ2V0KSkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR0aGlzLm9wZW5fbm9kZShlLmN1cnJlbnRUYXJnZXQsIGZ1bmN0aW9uIChvKSB7IHRoaXMuZ2V0X25vZGUobywgdHJ1ZSkuY2hpbGRyZW4oJy5qc3RyZWUtYW5jaG9yJykuZm9jdXMoKTsgfSk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0byA9IHRoaXMuZ2V0X25leHRfZG9tKGUuY3VycmVudFRhcmdldCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGlmKG8gJiYgby5sZW5ndGgpIHsgby5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5mb2N1cygpOyB9XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRjYXNlIDQwOlxyXG5cdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0byA9IHRoaXMuZ2V0X25leHRfZG9tKGUuY3VycmVudFRhcmdldCk7XHJcblx0XHRcdFx0XHRcdFx0XHRpZihvICYmIG8ubGVuZ3RoKSB7IG8uY2hpbGRyZW4oJy5qc3RyZWUtYW5jaG9yJykuZm9jdXMoKTsgfVxyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0Ly8gZGVsZXRlXHJcblx0XHRcdFx0XHRcdFx0Y2FzZSA0NjpcclxuXHRcdFx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcclxuXHRcdFx0XHRcdFx0XHRcdG8gPSB0aGlzLmdldF9ub2RlKGUuY3VycmVudFRhcmdldCk7XHJcblx0XHRcdFx0XHRcdFx0XHRpZihvICYmIG8uaWQgJiYgby5pZCAhPT0gJyMnKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdG8gPSB0aGlzLmlzX3NlbGVjdGVkKG8pID8gdGhpcy5nZXRfc2VsZWN0ZWQoKSA6IG87XHJcblx0XHRcdFx0XHRcdFx0XHRcdC8vIHRoaXMuZGVsZXRlX25vZGUobyk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHQvLyBmMlxyXG5cdFx0XHRcdFx0XHRcdGNhc2UgMTEzOlxyXG5cdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0byA9IHRoaXMuZ2V0X25vZGUoZS5jdXJyZW50VGFyZ2V0KTtcclxuXHRcdFx0XHRcdFx0XHRcdC8qIVxyXG5cdFx0XHRcdFx0XHRcdFx0aWYobyAmJiBvLmlkICYmIG8uaWQgIT09ICcjJykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHQvLyB0aGlzLmVkaXQobyk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHQqL1xyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0ZGVmYXVsdDpcclxuXHRcdFx0XHRcdFx0XHRcdC8vIGNvbnNvbGUubG9nKGUud2hpY2gpO1xyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdC5vbihcImxvYWRfbm9kZS5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHRpZihkYXRhLnN0YXR1cykge1xyXG5cdFx0XHRcdFx0XHRcdGlmKGRhdGEubm9kZS5pZCA9PT0gJyMnICYmICF0aGlzLl9kYXRhLmNvcmUubG9hZGVkKSB7XHJcblx0XHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUubG9hZGVkID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0XHRcdC8qKlxyXG5cdFx0XHRcdFx0XHRcdFx0ICogdHJpZ2dlcmVkIGFmdGVyIHRoZSByb290IG5vZGUgaXMgbG9hZGVkIGZvciB0aGUgZmlyc3QgdGltZVxyXG5cdFx0XHRcdFx0XHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdFx0XHRcdFx0XHQgKiBAbmFtZSBsb2FkZWQuanN0cmVlXHJcblx0XHRcdFx0XHRcdFx0XHQgKi9cclxuXHRcdFx0XHRcdFx0XHRcdHRoaXMudHJpZ2dlcihcImxvYWRlZFwiKTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0aWYoIXRoaXMuX2RhdGEuY29yZS5yZWFkeSAmJiAhdGhpcy5nZXRfY29udGFpbmVyX3VsKCkuZmluZCgnLmpzdHJlZS1sb2FkaW5nOmVxKDApJykubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUucmVhZHkgPSB0cnVlO1xyXG5cdFx0XHRcdFx0XHRcdFx0aWYodGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRpZih0aGlzLnNldHRpbmdzLmNvcmUuZXhwYW5kX3NlbGVjdGVkX29ubG9hZCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHZhciB0bXAgPSBbXSwgaSwgajtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0XHR0bXAgPSB0bXAuY29uY2F0KHRoaXMuX21vZGVsLmRhdGFbdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkW2ldXS5wYXJlbnRzKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0dG1wID0gJC52YWthdGEuYXJyYXlfdW5pcXVlKHRtcCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gdG1wLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdFx0dGhpcy5vcGVuX25vZGUodG1wW2ldLCBmYWxzZSwgMCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdHRoaXMudHJpZ2dlcignY2hhbmdlZCcsIHsgJ2FjdGlvbicgOiAncmVhZHknLCAnc2VsZWN0ZWQnIDogdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkIH0pO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0LyoqXHJcblx0XHRcdFx0XHRcdFx0XHQgKiB0cmlnZ2VyZWQgYWZ0ZXIgYWxsIG5vZGVzIGFyZSBmaW5pc2hlZCBsb2FkaW5nXHJcblx0XHRcdFx0XHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHRcdFx0XHRcdCAqIEBuYW1lIHJlYWR5LmpzdHJlZVxyXG5cdFx0XHRcdFx0XHRcdFx0ICovXHJcblx0XHRcdFx0XHRcdFx0XHRzZXRUaW1lb3V0KCQucHJveHkoZnVuY3Rpb24gKCkgeyB0aGlzLnRyaWdnZXIoXCJyZWFkeVwiKTsgfSwgdGhpcyksIDApO1xyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Ly8gVEhFTUUgUkVMQVRFRFxyXG5cdFx0XHRcdC5vbihcImluaXQuanN0cmVlXCIsICQucHJveHkoZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdFx0XHR2YXIgcyA9IHRoaXMuc2V0dGluZ3MuY29yZS50aGVtZXM7XHJcblx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS50aGVtZXMuZG90c1x0XHRcdD0gcy5kb3RzO1xyXG5cdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUudGhlbWVzLnN0cmlwZXNcdFx0PSBzLnN0cmlwZXM7XHJcblx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS50aGVtZXMuaWNvbnNcdFx0PSBzLmljb25zO1xyXG5cdFx0XHRcdFx0XHR0aGlzLnNldF90aGVtZShzLm5hbWUgfHwgXCJkZWZhdWx0XCIsIHMudXJsKTtcclxuXHRcdFx0XHRcdFx0dGhpcy5zZXRfdGhlbWVfdmFyaWFudChzLnZhcmlhbnQpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwibG9hZGluZy5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHRcdHRoaXNbIHRoaXMuX2RhdGEuY29yZS50aGVtZXMuZG90cyA/IFwic2hvd19kb3RzXCIgOiBcImhpZGVfZG90c1wiIF0oKTtcclxuXHRcdFx0XHRcdFx0dGhpc1sgdGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5pY29ucyA/IFwic2hvd19pY29uc1wiIDogXCJoaWRlX2ljb25zXCIgXSgpO1xyXG5cdFx0XHRcdFx0XHR0aGlzWyB0aGlzLl9kYXRhLmNvcmUudGhlbWVzLnN0cmlwZXMgPyBcInNob3dfc3RyaXBlc1wiIDogXCJoaWRlX3N0cmlwZXNcIiBdKCk7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHQub24oJ2ZvY3VzLmpzdHJlZScsICcuanN0cmVlLWFuY2hvcicsICQucHJveHkoZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5lbGVtZW50LmZpbmQoJy5qc3RyZWUtaG92ZXJlZCcpLm5vdChlLmN1cnJlbnRUYXJnZXQpLm1vdXNlbGVhdmUoKTtcclxuXHRcdFx0XHRcdFx0JChlLmN1cnJlbnRUYXJnZXQpLm1vdXNlZW50ZXIoKTtcclxuXHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdC5vbignbW91c2VlbnRlci5qc3RyZWUnLCAnLmpzdHJlZS1hbmNob3InLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuaG92ZXJfbm9kZShlLmN1cnJlbnRUYXJnZXQpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKCdtb3VzZWxlYXZlLmpzdHJlZScsICcuanN0cmVlLWFuY2hvcicsICQucHJveHkoZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5kZWhvdmVyX25vZGUoZS5jdXJyZW50VGFyZ2V0KTtcclxuXHRcdFx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHBhcnQgb2YgdGhlIGRlc3Ryb3lpbmcgb2YgYW4gaW5zdGFuY2UuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSB1bmJpbmQoKVxyXG5cdFx0ICovXHJcblx0XHR1bmJpbmQgOiBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHRoaXMuZWxlbWVudC5vZmYoJy5qc3RyZWUnKTtcclxuXHRcdFx0JChkb2N1bWVudCkub2ZmKCcuanN0cmVlLScgKyB0aGlzLl9pZCk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiB0cmlnZ2VyIGFuIGV2ZW50LiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgdHJpZ2dlcihldiBbLCBkYXRhXSlcclxuXHRcdCAqIEBwYXJhbSAge1N0cmluZ30gZXYgdGhlIG5hbWUgb2YgdGhlIGV2ZW50IHRvIHRyaWdnZXJcclxuXHRcdCAqIEBwYXJhbSAge09iamVjdH0gZGF0YSBhZGRpdGlvbmFsIGRhdGEgdG8gcGFzcyB3aXRoIHRoZSBldmVudFxyXG5cdFx0ICovXHJcblx0XHR0cmlnZ2VyIDogZnVuY3Rpb24gKGV2LCBkYXRhKSB7XHJcblx0XHRcdGlmKCFkYXRhKSB7XHJcblx0XHRcdFx0ZGF0YSA9IHt9O1xyXG5cdFx0XHR9XHJcblx0XHRcdGRhdGEuaW5zdGFuY2UgPSB0aGlzO1xyXG5cdFx0XHR0aGlzLmVsZW1lbnQudHJpZ2dlckhhbmRsZXIoZXYucmVwbGFjZSgnLmpzdHJlZScsJycpICsgJy5qc3RyZWUnLCBkYXRhKTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHJldHVybnMgdGhlIGpRdWVyeSBleHRlbmRlZCBpbnN0YW5jZSBjb250YWluZXJcclxuXHRcdCAqIEBuYW1lIGdldF9jb250YWluZXIoKVxyXG5cdFx0ICogQHJldHVybiB7alF1ZXJ5fVxyXG5cdFx0ICovXHJcblx0XHRnZXRfY29udGFpbmVyIDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5lbGVtZW50O1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogcmV0dXJucyB0aGUgalF1ZXJ5IGV4dGVuZGVkIG1haW4gVUwgbm9kZSBpbnNpZGUgdGhlIGluc3RhbmNlIGNvbnRhaW5lci4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIGdldF9jb250YWluZXJfdWwoKVxyXG5cdFx0ICogQHJldHVybiB7alF1ZXJ5fVxyXG5cdFx0ICovXHJcblx0XHRnZXRfY29udGFpbmVyX3VsIDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5lbGVtZW50LmNoaWxkcmVuKFwidWw6ZXEoMClcIik7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBnZXRzIHN0cmluZyByZXBsYWNlbWVudHMgKGxvY2FsaXphdGlvbikuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBnZXRfc3RyaW5nKGtleSlcclxuXHRcdCAqIEBwYXJhbSAge1N0cmluZ30ga2V5XHJcblx0XHQgKiBAcmV0dXJuIHtTdHJpbmd9XHJcblx0XHQgKi9cclxuXHRcdGdldF9zdHJpbmcgOiBmdW5jdGlvbiAoa2V5KSB7XHJcblx0XHRcdHZhciBhID0gdGhpcy5zZXR0aW5ncy5jb3JlLnN0cmluZ3M7XHJcblx0XHRcdGlmKCQuaXNGdW5jdGlvbihhKSkgeyByZXR1cm4gYS5jYWxsKHRoaXMsIGtleSk7IH1cclxuXHRcdFx0aWYoYSAmJiBhW2tleV0pIHsgcmV0dXJuIGFba2V5XTsgfVxyXG5cdFx0XHRyZXR1cm4ga2V5O1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0cyB0aGUgZmlyc3QgY2hpbGQgb2YgYSBET00gbm9kZS4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIF9maXJzdENoaWxkKGRvbSlcclxuXHRcdCAqIEBwYXJhbSAge0RPTUVsZW1lbnR9IGRvbVxyXG5cdFx0ICogQHJldHVybiB7RE9NRWxlbWVudH1cclxuXHRcdCAqL1xyXG5cdFx0X2ZpcnN0Q2hpbGQgOiBmdW5jdGlvbiAoZG9tKSB7XHJcblx0XHRcdGRvbSA9IGRvbSA/IGRvbS5maXJzdENoaWxkIDogbnVsbDtcclxuXHRcdFx0d2hpbGUoZG9tICE9PSBudWxsICYmIGRvbS5ub2RlVHlwZSAhPT0gMSkge1xyXG5cdFx0XHRcdGRvbSA9IGRvbS5uZXh0U2libGluZztcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gZG9tO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0cyB0aGUgbmV4dCBzaWJsaW5nIG9mIGEgRE9NIG5vZGUuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBfbmV4dFNpYmxpbmcoZG9tKVxyXG5cdFx0ICogQHBhcmFtICB7RE9NRWxlbWVudH0gZG9tXHJcblx0XHQgKiBAcmV0dXJuIHtET01FbGVtZW50fVxyXG5cdFx0ICovXHJcblx0XHRfbmV4dFNpYmxpbmcgOiBmdW5jdGlvbiAoZG9tKSB7XHJcblx0XHRcdGRvbSA9IGRvbSA/IGRvbS5uZXh0U2libGluZyA6IG51bGw7XHJcblx0XHRcdHdoaWxlKGRvbSAhPT0gbnVsbCAmJiBkb20ubm9kZVR5cGUgIT09IDEpIHtcclxuXHRcdFx0XHRkb20gPSBkb20ubmV4dFNpYmxpbmc7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIGRvbTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGdldHMgdGhlIHByZXZpb3VzIHNpYmxpbmcgb2YgYSBET00gbm9kZS4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIF9wcmV2aW91c1NpYmxpbmcoZG9tKVxyXG5cdFx0ICogQHBhcmFtICB7RE9NRWxlbWVudH0gZG9tXHJcblx0XHQgKiBAcmV0dXJuIHtET01FbGVtZW50fVxyXG5cdFx0ICovXHJcblx0XHRfcHJldmlvdXNTaWJsaW5nIDogZnVuY3Rpb24gKGRvbSkge1xyXG5cdFx0XHRkb20gPSBkb20gPyBkb20ucHJldmlvdXNTaWJsaW5nIDogbnVsbDtcclxuXHRcdFx0d2hpbGUoZG9tICE9PSBudWxsICYmIGRvbS5ub2RlVHlwZSAhPT0gMSkge1xyXG5cdFx0XHRcdGRvbSA9IGRvbS5wcmV2aW91c1NpYmxpbmc7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIGRvbTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGdldCB0aGUgSlNPTiByZXByZXNlbnRhdGlvbiBvZiBhIG5vZGUgKG9yIHRoZSBhY3R1YWwgalF1ZXJ5IGV4dGVuZGVkIERPTSBub2RlKSBieSB1c2luZyBhbnkgaW5wdXQgKGNoaWxkIERPTSBlbGVtZW50LCBJRCBzdHJpbmcsIHNlbGVjdG9yLCBldGMpXHJcblx0XHQgKiBAbmFtZSBnZXRfbm9kZShvYmogWywgYXNfZG9tXSlcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqIEBwYXJhbSAge0Jvb2xlYW59IGFzX2RvbVxyXG5cdFx0ICogQHJldHVybiB7T2JqZWN0fGpRdWVyeX1cclxuXHRcdCAqL1xyXG5cdFx0Z2V0X25vZGUgOiBmdW5jdGlvbiAob2JqLCBhc19kb20pIHtcclxuXHRcdFx0aWYob2JqICYmIG9iai5pZCkge1xyXG5cdFx0XHRcdG9iaiA9IG9iai5pZDtcclxuXHRcdFx0fVxyXG5cdFx0XHR2YXIgZG9tO1xyXG5cdFx0XHR0cnkge1xyXG5cdFx0XHRcdGlmKHRoaXMuX21vZGVsLmRhdGFbb2JqXSkge1xyXG5cdFx0XHRcdFx0b2JqID0gdGhpcy5fbW9kZWwuZGF0YVtvYmpdO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRlbHNlIGlmKCgoZG9tID0gJChvYmosIHRoaXMuZWxlbWVudCkpLmxlbmd0aCB8fCAoZG9tID0gJCgnIycgKyBvYmosIHRoaXMuZWxlbWVudCkpLmxlbmd0aCkgJiYgdGhpcy5fbW9kZWwuZGF0YVtkb20uY2xvc2VzdCgnbGknKS5hdHRyKCdpZCcpXSkge1xyXG5cdFx0XHRcdFx0b2JqID0gdGhpcy5fbW9kZWwuZGF0YVtkb20uY2xvc2VzdCgnbGknKS5hdHRyKCdpZCcpXTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0ZWxzZSBpZigoZG9tID0gJChvYmosIHRoaXMuZWxlbWVudCkpLmxlbmd0aCAmJiBkb20uaGFzQ2xhc3MoJ2pzdHJlZScpKSB7XHJcblx0XHRcdFx0XHRvYmogPSB0aGlzLl9tb2RlbC5kYXRhWycjJ107XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdH1cclxuXHJcblx0XHRcdFx0aWYoYXNfZG9tKSB7XHJcblx0XHRcdFx0XHRvYmogPSBvYmouaWQgPT09ICcjJyA/IHRoaXMuZWxlbWVudCA6ICQoZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQob2JqLmlkKSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiBvYmo7XHJcblx0XHRcdH0gY2F0Y2ggKGV4KSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0IHRoZSBwYXRoIHRvIGEgbm9kZSwgZWl0aGVyIGNvbnNpc3Rpbmcgb2Ygbm9kZSB0ZXh0cywgb3Igb2Ygbm9kZSBJRHMsIG9wdGlvbmFsbHkgZ2x1ZWQgdG9nZXRoZXIgKG90aGVyd2lzZSBhbiBhcnJheSlcclxuXHRcdCAqIEBuYW1lIGdldF9wYXRoKG9iaiBbLCBnbHVlLCBpZHNdKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9iaiB0aGUgbm9kZVxyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBnbHVlIGlmIHlvdSB3YW50IHRoZSBwYXRoIGFzIGEgc3RyaW5nIC0gcGFzcyB0aGUgZ2x1ZSBoZXJlIChmb3IgZXhhbXBsZSAnLycpLCBpZiBhIGZhbHN5IHZhbHVlIGlzIHN1cHBsaWVkIGhlcmUsIGFuIGFycmF5IGlzIHJldHVybmVkXHJcblx0XHQgKiBAcGFyYW0gIHtCb29sZWFufSBpZHMgaWYgc2V0IHRvIHRydWUgYnVpbGQgdGhlIHBhdGggdXNpbmcgSUQsIG90aGVyd2lzZSBub2RlIHRleHQgaXMgdXNlZFxyXG5cdFx0ICogQHJldHVybiB7bWl4ZWR9XHJcblx0XHQgKi9cclxuXHRcdGdldF9wYXRoIDogZnVuY3Rpb24gKG9iaiwgZ2x1ZSwgaWRzKSB7XHJcblx0XHRcdG9iaiA9IG9iai5wYXJlbnRzID8gb2JqIDogdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnIHx8ICFvYmoucGFyZW50cykge1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHR2YXIgaSwgaiwgcCA9IFtdO1xyXG5cdFx0XHRwLnB1c2goaWRzID8gb2JqLmlkIDogb2JqLnRleHQpO1xyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSBvYmoucGFyZW50cy5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRwLnB1c2goaWRzID8gb2JqLnBhcmVudHNbaV0gOiB0aGlzLmdldF90ZXh0KG9iai5wYXJlbnRzW2ldKSk7XHJcblx0XHRcdH1cclxuXHRcdFx0cCA9IHAucmV2ZXJzZSgpLnNsaWNlKDEpO1xyXG5cdFx0XHRyZXR1cm4gZ2x1ZSA/IHAuam9pbihnbHVlKSA6IHA7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBnZXQgdGhlIG5leHQgdmlzaWJsZSBub2RlIHRoYXQgaXMgYmVsb3cgdGhlIGBvYmpgIG5vZGUuIElmIGBzdHJpY3RgIGlzIHNldCB0byBgdHJ1ZWAgb25seSBzaWJsaW5nIG5vZGVzIGFyZSByZXR1cm5lZC5cclxuXHRcdCAqIEBuYW1lIGdldF9uZXh0X2RvbShvYmogWywgc3RyaWN0XSlcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqIEBwYXJhbSAge0Jvb2xlYW59IHN0cmljdFxyXG5cdFx0ICogQHJldHVybiB7alF1ZXJ5fVxyXG5cdFx0ICovXHJcblx0XHRnZXRfbmV4dF9kb20gOiBmdW5jdGlvbiAob2JqLCBzdHJpY3QpIHtcclxuXHRcdFx0dmFyIHRtcDtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpO1xyXG5cdFx0XHRpZihvYmpbMF0gPT09IHRoaXMuZWxlbWVudFswXSkge1xyXG5cdFx0XHRcdHRtcCA9IHRoaXMuX2ZpcnN0Q2hpbGQodGhpcy5nZXRfY29udGFpbmVyX3VsKClbMF0pO1xyXG5cdFx0XHRcdHJldHVybiB0bXAgPyAkKHRtcCkgOiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZighb2JqIHx8ICFvYmoubGVuZ3RoKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHN0cmljdCkge1xyXG5cdFx0XHRcdHRtcCA9IHRoaXMuX25leHRTaWJsaW5nKG9ialswXSk7XHJcblx0XHRcdFx0cmV0dXJuIHRtcCA/ICQodG1wKSA6IGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKG9iai5oYXNDbGFzcyhcImpzdHJlZS1vcGVuXCIpKSB7XHJcblx0XHRcdFx0dG1wID0gdGhpcy5fZmlyc3RDaGlsZChvYmouY2hpbGRyZW4oJ3VsJylbMF0pO1xyXG5cdFx0XHRcdHJldHVybiB0bXAgPyAkKHRtcCkgOiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZigodG1wID0gdGhpcy5fbmV4dFNpYmxpbmcob2JqWzBdKSkgIT09IG51bGwpIHtcclxuXHRcdFx0XHRyZXR1cm4gJCh0bXApO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiBvYmoucGFyZW50c1VudGlsKFwiLmpzdHJlZVwiLFwibGlcIikubmV4dChcImxpXCIpLmVxKDApO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0IHRoZSBwcmV2aW91cyB2aXNpYmxlIG5vZGUgdGhhdCBpcyBhYm92ZSB0aGUgYG9iamAgbm9kZS4gSWYgYHN0cmljdGAgaXMgc2V0IHRvIGB0cnVlYCBvbmx5IHNpYmxpbmcgbm9kZXMgYXJlIHJldHVybmVkLlxyXG5cdFx0ICogQG5hbWUgZ2V0X3ByZXZfZG9tKG9iaiBbLCBzdHJpY3RdKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHBhcmFtICB7Qm9vbGVhbn0gc3RyaWN0XHJcblx0XHQgKiBAcmV0dXJuIHtqUXVlcnl9XHJcblx0XHQgKi9cclxuXHRcdGdldF9wcmV2X2RvbSA6IGZ1bmN0aW9uIChvYmosIHN0cmljdCkge1xyXG5cdFx0XHR2YXIgdG1wO1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaiwgdHJ1ZSk7XHJcblx0XHRcdGlmKG9ialswXSA9PT0gdGhpcy5lbGVtZW50WzBdKSB7XHJcblx0XHRcdFx0dG1wID0gdGhpcy5nZXRfY29udGFpbmVyX3VsKClbMF0ubGFzdENoaWxkO1xyXG5cdFx0XHRcdHJldHVybiB0bXAgPyAkKHRtcCkgOiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZighb2JqIHx8ICFvYmoubGVuZ3RoKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHN0cmljdCkge1xyXG5cdFx0XHRcdHRtcCA9IHRoaXMuX3ByZXZpb3VzU2libGluZyhvYmpbMF0pO1xyXG5cdFx0XHRcdHJldHVybiB0bXAgPyAkKHRtcCkgOiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZigodG1wID0gdGhpcy5fcHJldmlvdXNTaWJsaW5nKG9ialswXSkpICE9PSBudWxsKSB7XHJcblx0XHRcdFx0b2JqID0gJCh0bXApO1xyXG5cdFx0XHRcdHdoaWxlKG9iai5oYXNDbGFzcyhcImpzdHJlZS1vcGVuXCIpKSB7XHJcblx0XHRcdFx0XHRvYmogPSBvYmouY2hpbGRyZW4oXCJ1bDplcSgwKVwiKS5jaGlsZHJlbihcImxpOmxhc3RcIik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiBvYmo7XHJcblx0XHRcdH1cclxuXHRcdFx0dG1wID0gb2JqWzBdLnBhcmVudE5vZGUucGFyZW50Tm9kZTtcclxuXHRcdFx0cmV0dXJuIHRtcCAmJiB0bXAudGFnTmFtZSA9PT0gJ0xJJyA/ICQodG1wKSA6IGZhbHNlO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0IHRoZSBwYXJlbnQgSUQgb2YgYSBub2RlXHJcblx0XHQgKiBAbmFtZSBnZXRfcGFyZW50KG9iailcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqIEByZXR1cm4ge1N0cmluZ31cclxuXHRcdCAqL1xyXG5cdFx0Z2V0X3BhcmVudCA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiBvYmoucGFyZW50O1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0IGEgalF1ZXJ5IGNvbGxlY3Rpb24gb2YgYWxsIHRoZSBjaGlsZHJlbiBvZiBhIG5vZGUgKG5vZGUgbXVzdCBiZSByZW5kZXJlZClcclxuXHRcdCAqIEBuYW1lIGdldF9jaGlsZHJlbl9kb20ob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHJldHVybiB7alF1ZXJ5fVxyXG5cdFx0ICovXHJcblx0XHRnZXRfY2hpbGRyZW5fZG9tIDogZnVuY3Rpb24gKG9iaikge1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaiwgdHJ1ZSk7XHJcblx0XHRcdGlmKG9ialswXSA9PT0gdGhpcy5lbGVtZW50WzBdKSB7XHJcblx0XHRcdFx0cmV0dXJuIHRoaXMuZ2V0X2NvbnRhaW5lcl91bCgpLmNoaWxkcmVuKFwibGlcIik7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoIW9iaiB8fCAhb2JqLmxlbmd0aCkge1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gb2JqLmNoaWxkcmVuKFwidWxcIikuY2hpbGRyZW4oXCJsaVwiKTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGNoZWNrcyBpZiBhIG5vZGUgaGFzIGNoaWxkcmVuXHJcblx0XHQgKiBAbmFtZSBpc19wYXJlbnQob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHJldHVybiB7Qm9vbGVhbn1cclxuXHRcdCAqL1xyXG5cdFx0aXNfcGFyZW50IDogZnVuY3Rpb24gKG9iaikge1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdHJldHVybiBvYmogJiYgKG9iai5zdGF0ZS5sb2FkZWQgPT09IGZhbHNlIHx8IG9iai5jaGlsZHJlbi5sZW5ndGggPiAwKTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGNoZWNrcyBpZiBhIG5vZGUgaXMgbG9hZGVkIChpdHMgY2hpbGRyZW4gYXJlIGF2YWlsYWJsZSlcclxuXHRcdCAqIEBuYW1lIGlzX2xvYWRlZChvYmopXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICovXHJcblx0XHRpc19sb2FkZWQgOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0cmV0dXJuIG9iaiAmJiBvYmouc3RhdGUubG9hZGVkO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY2hlY2sgaWYgYSBub2RlIGlzIGN1cnJlbnRseSBsb2FkaW5nIChmZXRjaGluZyBjaGlsZHJlbilcclxuXHRcdCAqIEBuYW1lIGlzX2xvYWRpbmcob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHJldHVybiB7Qm9vbGVhbn1cclxuXHRcdCAqL1xyXG5cdFx0aXNfbG9hZGluZyA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpO1xyXG5cdFx0XHRyZXR1cm4gb2JqICYmIG9iai5oYXNDbGFzcyhcImpzdHJlZS1sb2FkaW5nXCIpO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY2hlY2sgaWYgYSBub2RlIGlzIG9wZW5lZFxyXG5cdFx0ICogQG5hbWUgaXNfb3BlbihvYmopXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICovXHJcblx0XHRpc19vcGVuIDogZnVuY3Rpb24gKG9iaikge1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdHJldHVybiBvYmogJiYgb2JqLnN0YXRlLm9wZW5lZDtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGNoZWNrIGlmIGEgbm9kZSBpcyBpbiBhIGNsb3NlZCBzdGF0ZVxyXG5cdFx0ICogQG5hbWUgaXNfY2xvc2VkKG9iailcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqIEByZXR1cm4ge0Jvb2xlYW59XHJcblx0XHQgKi9cclxuXHRcdGlzX2Nsb3NlZCA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRyZXR1cm4gb2JqICYmIHRoaXMuaXNfcGFyZW50KG9iaikgJiYgIW9iai5zdGF0ZS5vcGVuZWQ7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBjaGVjayBpZiBhIG5vZGUgaGFzIG5vIGNoaWxkcmVuXHJcblx0XHQgKiBAbmFtZSBpc19sZWFmKG9iailcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqIEByZXR1cm4ge0Jvb2xlYW59XHJcblx0XHQgKi9cclxuXHRcdGlzX2xlYWYgOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdHJldHVybiAhdGhpcy5pc19wYXJlbnQob2JqKTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGxvYWRzIGEgbm9kZSAoZmV0Y2hlcyBpdHMgY2hpbGRyZW4gdXNpbmcgdGhlIGBjb3JlLmRhdGFgIHNldHRpbmcpLiBNdWx0aXBsZSBub2RlcyBjYW4gYmUgcGFzc2VkIHRvIGJ5IHVzaW5nIGFuIGFycmF5LlxyXG5cdFx0ICogQG5hbWUgbG9hZF9ub2RlKG9iaiBbLCBjYWxsYmFja10pXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqXHJcblx0XHQgKiBAcGFyYW0gIHtmdW5jdGlvbn0gY2FsbGJhY2sgYSBmdW5jdGlvbiB0byBiZSBleGVjdXRlZCBvbmNlIGxvYWRpbmcgaXMgY29ucGxldGUsIHRoZSBmdW5jdGlvbiBpcyBleGVjdXRlZCBpbiB0aGUgaW5zdGFuY2UncyBzY29wZSBhbmQgcmVjZWl2ZXMgdHdvIGFyZ3VtZW50cyAtIHRoZSBub2RlIGFuZCBhIGJvb2xlYW4gc3RhdHVzXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICogQHRyaWdnZXIgbG9hZF9ub2RlLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRsb2FkX25vZGUgOiBmdW5jdGlvbiAob2JqLCBjYWxsYmFjaykge1xyXG5cdFx0XHR2YXIgdDEsIHQyO1xyXG5cdFx0XHRpZigkLmlzQXJyYXkob2JqKSkge1xyXG5cdFx0XHRcdG9iaiA9IG9iai5zbGljZSgpO1xyXG5cdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gb2JqLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0dGhpcy5sb2FkX25vZGUob2JqW3QxXSwgY2FsbGJhY2spO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmopIHtcclxuXHRcdFx0XHRjYWxsYmFjay5jYWxsKHRoaXMsIG9iaiwgZmFsc2UpO1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHR0aGlzLmdldF9ub2RlKG9iaiwgdHJ1ZSkuYWRkQ2xhc3MoXCJqc3RyZWUtbG9hZGluZ1wiKTtcclxuXHRcdFx0dGhpcy5fbG9hZF9ub2RlKG9iaiwgJC5wcm94eShmdW5jdGlvbiAoc3RhdHVzKSB7XHJcblx0XHRcdFx0b2JqLnN0YXRlLmxvYWRlZCA9IHN0YXR1cztcclxuXHRcdFx0XHR0aGlzLmdldF9ub2RlKG9iaiwgdHJ1ZSkucmVtb3ZlQ2xhc3MoXCJqc3RyZWUtbG9hZGluZ1wiKTtcclxuXHRcdFx0XHQvKipcclxuXHRcdFx0XHQgKiB0cmlnZ2VyZWQgYWZ0ZXIgYSBub2RlIGlzIGxvYWRlZFxyXG5cdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdCAqIEBuYW1lIGxvYWRfbm9kZS5qc3RyZWVcclxuXHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZSB0aGUgbm9kZSB0aGF0IHdhcyBsb2FkaW5nXHJcblx0XHRcdFx0ICogQHBhcmFtIHtCb29sZWFufSBzdGF0dXMgd2FzIHRoZSBub2RlIGxvYWRlZCBzdWNjZXNzZnVsbHlcclxuXHRcdFx0XHQgKi9cclxuXHRcdFx0XHR0aGlzLnRyaWdnZXIoJ2xvYWRfbm9kZScsIHsgXCJub2RlXCIgOiBvYmosIFwic3RhdHVzXCIgOiBzdGF0dXMgfSk7XHJcblx0XHRcdFx0aWYoY2FsbGJhY2spIHtcclxuXHRcdFx0XHRcdGNhbGxiYWNrLmNhbGwodGhpcywgb2JqLCBzdGF0dXMpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fSwgdGhpcykpO1xyXG5cdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGhhbmRsZXMgdGhlIGFjdHVhbCBsb2FkaW5nIG9mIGEgbm9kZS4gVXNlZCBvbmx5IGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgX2xvYWRfbm9kZShvYmogWywgY2FsbGJhY2tdKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHBhcmFtICB7ZnVuY3Rpb259IGNhbGxiYWNrIGEgZnVuY3Rpb24gdG8gYmUgZXhlY3V0ZWQgb25jZSBsb2FkaW5nIGlzIGNvbnBsZXRlLCB0aGUgZnVuY3Rpb24gaXMgZXhlY3V0ZWQgaW4gdGhlIGluc3RhbmNlJ3Mgc2NvcGUgYW5kIHJlY2VpdmVzIG9uZSBhcmd1bWVudCAtIGEgYm9vbGVhbiBzdGF0dXNcclxuXHRcdCAqIEByZXR1cm4ge0Jvb2xlYW59XHJcblx0XHQgKi9cclxuXHRcdF9sb2FkX25vZGUgOiBmdW5jdGlvbiAob2JqLCBjYWxsYmFjaykge1xyXG5cdFx0XHR2YXIgcyA9IHRoaXMuc2V0dGluZ3MuY29yZS5kYXRhLCB0O1xyXG5cdFx0XHQvLyB1c2Ugb3JpZ2luYWwgSFRNTFxyXG5cdFx0XHRpZighcykge1xyXG5cdFx0XHRcdHJldHVybiBjYWxsYmFjay5jYWxsKHRoaXMsIG9iai5pZCA9PT0gJyMnID8gdGhpcy5fYXBwZW5kX2h0bWxfZGF0YShvYmosIHRoaXMuX2RhdGEuY29yZS5vcmlnaW5hbF9jb250YWluZXJfaHRtbC5jbG9uZSh0cnVlKSkgOiBmYWxzZSk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoJC5pc0Z1bmN0aW9uKHMpKSB7XHJcblx0XHRcdFx0cmV0dXJuIHMuY2FsbCh0aGlzLCBvYmosICQucHJveHkoZnVuY3Rpb24gKGQpIHtcclxuXHRcdFx0XHRcdHJldHVybiBkID09PSBmYWxzZSA/IGNhbGxiYWNrLmNhbGwodGhpcywgZmFsc2UpIDogY2FsbGJhY2suY2FsbCh0aGlzLCB0aGlzW3R5cGVvZiBkID09PSAnc3RyaW5nJyA/ICdfYXBwZW5kX2h0bWxfZGF0YScgOiAnX2FwcGVuZF9qc29uX2RhdGEnXShvYmosIHR5cGVvZiBkID09PSAnc3RyaW5nJyA/ICQoZCkgOiBkKSk7XHJcblx0XHRcdFx0fSwgdGhpcykpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHR5cGVvZiBzID09PSAnb2JqZWN0Jykge1xyXG5cdFx0XHRcdGlmKHMudXJsKSB7XHJcblx0XHRcdFx0XHRzID0gJC5leHRlbmQodHJ1ZSwge30sIHMpO1xyXG5cdFx0XHRcdFx0aWYoJC5pc0Z1bmN0aW9uKHMudXJsKSkge1xyXG5cdFx0XHRcdFx0XHRzLnVybCA9IHMudXJsLmNhbGwodGhpcywgb2JqKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGlmKCQuaXNGdW5jdGlvbihzLmRhdGEpKSB7XHJcblx0XHRcdFx0XHRcdHMuZGF0YSA9IHMuZGF0YS5jYWxsKHRoaXMsIG9iaik7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRyZXR1cm4gJC5hamF4KHMpXHJcblx0XHRcdFx0XHRcdC5kb25lKCQucHJveHkoZnVuY3Rpb24gKGQsdCx4KSB7XHJcblx0XHRcdFx0XHRcdFx0XHR2YXIgdHlwZSA9IHguZ2V0UmVzcG9uc2VIZWFkZXIoJ0NvbnRlbnQtVHlwZScpO1xyXG5cdFx0XHRcdFx0XHRcdFx0aWYodHlwZS5pbmRleE9mKCdqc29uJykgIT09IC0xKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHJldHVybiBjYWxsYmFjay5jYWxsKHRoaXMsIHRoaXMuX2FwcGVuZF9qc29uX2RhdGEob2JqLCBkKSk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRpZih0eXBlLmluZGV4T2YoJ2h0bWwnKSAhPT0gLTEpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0cmV0dXJuIGNhbGxiYWNrLmNhbGwodGhpcywgdGhpcy5fYXBwZW5kX2h0bWxfZGF0YShvYmosICQoZCkpKTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHRcdFx0LmZhaWwoJC5wcm94eShmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRjYWxsYmFjay5jYWxsKHRoaXMsIGZhbHNlKTtcclxuXHRcdFx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5sYXN0X2Vycm9yID0geyAnZXJyb3InIDogJ2FqYXgnLCAncGx1Z2luJyA6ICdjb3JlJywgJ2lkJyA6ICdjb3JlXzA0JywgJ3JlYXNvbicgOiAnQ291bGQgbm90IGxvYWQgbm9kZScsICdkYXRhJyA6IEpTT04uc3RyaW5naWZ5KHMpIH07XHJcblx0XHRcdFx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLmNvcmUuZXJyb3IuY2FsbCh0aGlzLCB0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvcik7XHJcblx0XHRcdFx0XHRcdFx0fSwgdGhpcykpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHR0ID0gKCQuaXNBcnJheShzKSB8fCAkLmlzUGxhaW5PYmplY3QocykpID8gSlNPTi5wYXJzZShKU09OLnN0cmluZ2lmeShzKSkgOiBzO1xyXG5cdFx0XHRcdHJldHVybiBjYWxsYmFjay5jYWxsKHRoaXMsIHRoaXMuX2FwcGVuZF9qc29uX2RhdGEob2JqLCB0KSk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYodHlwZW9mIHMgPT09ICdzdHJpbmcnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGNhbGxiYWNrLmNhbGwodGhpcywgdGhpcy5fYXBwZW5kX2h0bWxfZGF0YShvYmosIHMpKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gY2FsbGJhY2suY2FsbCh0aGlzLCBmYWxzZSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBhZGRzIGEgbm9kZSB0byB0aGUgbGlzdCBvZiBub2RlcyB0byByZWRyYXcuIFVzZWQgb25seSBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIF9ub2RlX2NoYW5nZWQob2JqIFssIGNhbGxiYWNrXSlcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqL1xyXG5cdFx0X25vZGVfY2hhbmdlZCA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZihvYmopIHtcclxuXHRcdFx0XHR0aGlzLl9tb2RlbC5jaGFuZ2VkLnB1c2gob2JqLmlkKTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogYXBwZW5kcyBIVE1MIGNvbnRlbnQgdG8gdGhlIHRyZWUuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBfYXBwZW5kX2h0bWxfZGF0YShvYmosIGRhdGEpXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqIHRoZSBub2RlIHRvIGFwcGVuZCB0b1xyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBkYXRhIHRoZSBIVE1MIHN0cmluZyB0byBwYXJzZSBhbmQgYXBwZW5kXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICogQHRyaWdnZXIgbW9kZWwuanN0cmVlLCBjaGFuZ2VkLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRfYXBwZW5kX2h0bWxfZGF0YSA6IGZ1bmN0aW9uIChkb20sIGRhdGEpIHtcclxuXHRcdFx0ZG9tID0gdGhpcy5nZXRfbm9kZShkb20pO1xyXG5cdFx0XHRkb20uY2hpbGRyZW4gPSBbXTtcclxuXHRcdFx0ZG9tLmNoaWxkcmVuX2QgPSBbXTtcclxuXHRcdFx0dmFyIGRhdCA9IGRhdGEuaXMoJ3VsJykgPyBkYXRhLmNoaWxkcmVuKCkgOiBkYXRhLFxyXG5cdFx0XHRcdHBhciA9IGRvbS5pZCxcclxuXHRcdFx0XHRjaGQgPSBbXSxcclxuXHRcdFx0XHRkcGMgPSBbXSxcclxuXHRcdFx0XHRtID0gdGhpcy5fbW9kZWwuZGF0YSxcclxuXHRcdFx0XHRwID0gbVtwYXJdLFxyXG5cdFx0XHRcdHMgPSB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQubGVuZ3RoLFxyXG5cdFx0XHRcdHRtcCwgaSwgajtcclxuXHRcdFx0ZGF0LmVhY2goJC5wcm94eShmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdHRtcCA9IHRoaXMuX3BhcnNlX21vZGVsX2Zyb21faHRtbCgkKHYpLCBwYXIsIHAucGFyZW50cy5jb25jYXQoKSk7XHJcblx0XHRcdFx0aWYodG1wKSB7XHJcblx0XHRcdFx0XHRjaGQucHVzaCh0bXApO1xyXG5cdFx0XHRcdFx0ZHBjLnB1c2godG1wKTtcclxuXHRcdFx0XHRcdGlmKG1bdG1wXS5jaGlsZHJlbl9kLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRkcGMgPSBkcGMuY29uY2F0KG1bdG1wXS5jaGlsZHJlbl9kKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdFx0cC5jaGlsZHJlbiA9IGNoZDtcclxuXHRcdFx0cC5jaGlsZHJlbl9kID0gZHBjO1xyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSBwLnBhcmVudHMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0bVtwLnBhcmVudHNbaV1dLmNoaWxkcmVuX2QgPSBtW3AucGFyZW50c1tpXV0uY2hpbGRyZW5fZC5jb25jYXQoZHBjKTtcclxuXHRcdFx0fVxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gbmV3IGRhdGEgaXMgaW5zZXJ0ZWQgdG8gdGhlIHRyZWUgbW9kZWxcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIG1vZGVsLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge0FycmF5fSBub2RlcyBhbiBhcnJheSBvZiBub2RlIElEc1xyXG5cdFx0XHQgKiBAcGFyYW0ge1N0cmluZ30gcGFyZW50IHRoZSBwYXJlbnQgSUQgb2YgdGhlIG5vZGVzXHJcblx0XHRcdCAqL1xyXG5cdFx0XHR0aGlzLnRyaWdnZXIoJ21vZGVsJywgeyBcIm5vZGVzXCIgOiBkcGMsICdwYXJlbnQnIDogcGFyIH0pO1xyXG5cdFx0XHRpZihwYXIgIT09ICcjJykge1xyXG5cdFx0XHRcdHRoaXMuX25vZGVfY2hhbmdlZChwYXIpO1xyXG5cdFx0XHRcdHRoaXMucmVkcmF3KCk7XHJcblx0XHRcdH1cclxuXHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0dGhpcy5nZXRfY29udGFpbmVyX3VsKCkuY2hpbGRyZW4oJy5qc3RyZWUtaW5pdGlhbC1ub2RlJykucmVtb3ZlKCk7XHJcblx0XHRcdFx0dGhpcy5yZWRyYXcodHJ1ZSk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYodGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLmxlbmd0aCAhPT0gcykge1xyXG5cdFx0XHRcdHRoaXMudHJpZ2dlcignY2hhbmdlZCcsIHsgJ2FjdGlvbicgOiAnbW9kZWwnLCAnc2VsZWN0ZWQnIDogdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkIH0pO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogYXBwZW5kcyBKU09OIGNvbnRlbnQgdG8gdGhlIHRyZWUuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBfYXBwZW5kX2pzb25fZGF0YShvYmosIGRhdGEpXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqIHRoZSBub2RlIHRvIGFwcGVuZCB0b1xyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBkYXRhIHRoZSBKU09OIG9iamVjdCB0byBwYXJzZSBhbmQgYXBwZW5kXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICovXHJcblx0XHRfYXBwZW5kX2pzb25fZGF0YSA6IGZ1bmN0aW9uIChkb20sIGRhdGEpIHtcclxuXHRcdFx0ZG9tID0gdGhpcy5nZXRfbm9kZShkb20pO1xyXG5cdFx0XHRkb20uY2hpbGRyZW4gPSBbXTtcclxuXHRcdFx0ZG9tLmNoaWxkcmVuX2QgPSBbXTtcclxuXHRcdFx0dmFyIGRhdCA9IGRhdGEsXHJcblx0XHRcdFx0cGFyID0gZG9tLmlkLFxyXG5cdFx0XHRcdGNoZCA9IFtdLFxyXG5cdFx0XHRcdGRwYyA9IFtdLFxyXG5cdFx0XHRcdG0gPSB0aGlzLl9tb2RlbC5kYXRhLFxyXG5cdFx0XHRcdHAgPSBtW3Bhcl0sXHJcblx0XHRcdFx0cyA9IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZC5sZW5ndGgsXHJcblx0XHRcdFx0dG1wLCBpLCBqO1xyXG5cdFx0XHQvLyAqJSRAISEhXHJcblx0XHRcdGlmKGRhdC5kKSB7XHJcblx0XHRcdFx0ZGF0ID0gZGF0LmQ7XHJcblx0XHRcdFx0aWYodHlwZW9mIGRhdCA9PT0gXCJzdHJpbmdcIikge1xyXG5cdFx0XHRcdFx0ZGF0ID0gSlNPTi5wYXJzZShkYXQpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZighJC5pc0FycmF5KGRhdCkpIHsgZGF0ID0gW2RhdF07IH1cclxuXHRcdFx0aWYoZGF0Lmxlbmd0aCAmJiBkYXRbMF0uaWQgIT09IHVuZGVmaW5lZCAmJiBkYXRbMF0ucGFyZW50ICE9PSB1bmRlZmluZWQpIHtcclxuXHRcdFx0XHQvLyBGbGF0IEpTT04gc3VwcG9ydCAoZm9yIGVhc3kgaW1wb3J0IGZyb20gREIpOlxyXG5cdFx0XHRcdC8vIDEpIGNvbnZlcnQgdG8gb2JqZWN0IChmb3JlYWNoKVxyXG5cdFx0XHRcdGZvcihpID0gMCwgaiA9IGRhdC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdGlmKCFkYXRbaV0uY2hpbGRyZW4pIHtcclxuXHRcdFx0XHRcdFx0ZGF0W2ldLmNoaWxkcmVuID0gW107XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRtW2RhdFtpXS5pZF0gPSBkYXRbaV07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdC8vIDIpIHBvcHVsYXRlIGNoaWxkcmVuIChmb3JlYWNoKVxyXG5cdFx0XHRcdGZvcihpID0gMCwgaiA9IGRhdC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdG1bZGF0W2ldLnBhcmVudF0uY2hpbGRyZW4ucHVzaChkYXRbaV0uaWQpO1xyXG5cdFx0XHRcdFx0Ly8gcG9wdWxhdGUgcGFyZW50LmNoaWxkcmVuX2RcclxuXHRcdFx0XHRcdHAuY2hpbGRyZW5fZC5wdXNoKGRhdFtpXS5pZCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdC8vIDMpIG5vcm1hbGl6ZSAmJiBwb3B1bGF0ZSBwYXJlbnRzIGFuZCBjaGlsZHJlbl9kIHdpdGggcmVjdXJzaW9uXHJcblx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gcC5jaGlsZHJlbi5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdHRtcCA9IHRoaXMuX3BhcnNlX21vZGVsX2Zyb21fZmxhdF9qc29uKG1bcC5jaGlsZHJlbltpXV0sIHBhciwgcC5wYXJlbnRzLmNvbmNhdCgpKTtcclxuXHRcdFx0XHRcdGRwYy5wdXNoKHRtcCk7XHJcblx0XHRcdFx0XHRpZihtW3RtcF0uY2hpbGRyZW5fZC5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0ZHBjID0gZHBjLmNvbmNhdChtW3RtcF0uY2hpbGRyZW5fZCk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdC8vID8pIHRocmVlX3N0YXRlIHNlbGVjdGlvbiAtIHAuc3RhdGUuc2VsZWN0ZWQgJiYgdCAtIChpZiB0aHJlZV9zdGF0ZSBmb3JlYWNoKGRhdCA9PiBjaCkgLT4gZm9yZWFjaChwYXJlbnRzKSBpZihwYXJlbnQuc2VsZWN0ZWQpIGNoaWxkLnNlbGVjdGVkID0gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSBkYXQubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHR0bXAgPSB0aGlzLl9wYXJzZV9tb2RlbF9mcm9tX2pzb24oZGF0W2ldLCBwYXIsIHAucGFyZW50cy5jb25jYXQoKSk7XHJcblx0XHRcdFx0XHRpZih0bXApIHtcclxuXHRcdFx0XHRcdFx0Y2hkLnB1c2godG1wKTtcclxuXHRcdFx0XHRcdFx0ZHBjLnB1c2godG1wKTtcclxuXHRcdFx0XHRcdFx0aWYobVt0bXBdLmNoaWxkcmVuX2QubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0ZHBjID0gZHBjLmNvbmNhdChtW3RtcF0uY2hpbGRyZW5fZCk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cC5jaGlsZHJlbiA9IGNoZDtcclxuXHRcdFx0XHRwLmNoaWxkcmVuX2QgPSBkcGM7XHJcblx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gcC5wYXJlbnRzLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0bVtwLnBhcmVudHNbaV1dLmNoaWxkcmVuX2QgPSBtW3AucGFyZW50c1tpXV0uY2hpbGRyZW5fZC5jb25jYXQoZHBjKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdtb2RlbCcsIHsgXCJub2Rlc1wiIDogZHBjLCAncGFyZW50JyA6IHBhciB9KTtcclxuXHJcblx0XHRcdGlmKHBhciAhPT0gJyMnKSB7XHJcblx0XHRcdFx0dGhpcy5fbm9kZV9jaGFuZ2VkKHBhcik7XHJcblx0XHRcdFx0dGhpcy5yZWRyYXcoKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlIHtcclxuXHRcdFx0XHQvLyB0aGlzLmdldF9jb250YWluZXJfdWwoKS5jaGlsZHJlbignLmpzdHJlZS1pbml0aWFsLW5vZGUnKS5yZW1vdmUoKTtcclxuXHRcdFx0XHR0aGlzLnJlZHJhdyh0cnVlKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZih0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQubGVuZ3RoICE9PSBzKSB7XHJcblx0XHRcdFx0dGhpcy50cmlnZ2VyKCdjaGFuZ2VkJywgeyAnYWN0aW9uJyA6ICdtb2RlbCcsICdzZWxlY3RlZCcgOiB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQgfSk7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBwYXJzZXMgYSBub2RlIGZyb20gYSBqUXVlcnkgb2JqZWN0IGFuZCBhcHBlbmRzIHRoZW0gdG8gdGhlIGluIG1lbW9yeSB0cmVlIG1vZGVsLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgX3BhcnNlX21vZGVsX2Zyb21faHRtbChkIFssIHAsIHBzXSlcclxuXHRcdCAqIEBwYXJhbSAge2pRdWVyeX0gZCB0aGUgalF1ZXJ5IG9iamVjdCB0byBwYXJzZVxyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBwIHRoZSBwYXJlbnQgSURcclxuXHRcdCAqIEBwYXJhbSAge0FycmF5fSBwcyBsaXN0IG9mIGFsbCBwYXJlbnRzXHJcblx0XHQgKiBAcmV0dXJuIHtTdHJpbmd9IHRoZSBJRCBvZiB0aGUgb2JqZWN0IGFkZGVkIHRvIHRoZSBtb2RlbFxyXG5cdFx0ICovXHJcblx0XHRfcGFyc2VfbW9kZWxfZnJvbV9odG1sIDogZnVuY3Rpb24gKGQsIHAsIHBzKSB7XHJcblx0XHRcdGlmKCFwcykgeyBwcyA9IFtdOyB9XHJcblx0XHRcdGVsc2UgeyBwcyA9IFtdLmNvbmNhdChwcyk7IH1cclxuXHRcdFx0aWYocCkgeyBwcy51bnNoaWZ0KHApOyB9XHJcblx0XHRcdHZhciBjLCBlLCBtID0gdGhpcy5fbW9kZWwuZGF0YSxcclxuXHRcdFx0XHRkYXRhID0ge1xyXG5cdFx0XHRcdFx0aWRcdFx0XHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0dGV4dFx0XHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0aWNvblx0XHQ6IHRydWUsXHJcblx0XHRcdFx0XHRwYXJlbnRcdFx0OiBwLFxyXG5cdFx0XHRcdFx0cGFyZW50c1x0XHQ6IHBzLFxyXG5cdFx0XHRcdFx0Y2hpbGRyZW5cdDogW10sXHJcblx0XHRcdFx0XHRjaGlsZHJlbl9kXHQ6IFtdLFxyXG5cdFx0XHRcdFx0ZGF0YVx0XHQ6IG51bGwsXHJcblx0XHRcdFx0XHRzdGF0ZVx0XHQ6IHsgfSxcclxuXHRcdFx0XHRcdGxpX2F0dHJcdFx0OiB7IGlkIDogZmFsc2UgfSxcclxuXHRcdFx0XHRcdGFfYXR0clx0XHQ6IHsgaHJlZiA6ICcjJyB9LFxyXG5cdFx0XHRcdFx0b3JpZ2luYWxcdDogZmFsc2VcclxuXHRcdFx0XHR9LCBpLCB0bXAsIHRpZDtcclxuXHRcdFx0Zm9yKGkgaW4gdGhpcy5fbW9kZWwuZGVmYXVsdF9zdGF0ZSkge1xyXG5cdFx0XHRcdGlmKHRoaXMuX21vZGVsLmRlZmF1bHRfc3RhdGUuaGFzT3duUHJvcGVydHkoaSkpIHtcclxuXHRcdFx0XHRcdGRhdGEuc3RhdGVbaV0gPSB0aGlzLl9tb2RlbC5kZWZhdWx0X3N0YXRlW2ldO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHR0bXAgPSAkLnZha2F0YS5hdHRyaWJ1dGVzKGQsIHRydWUpO1xyXG5cdFx0XHQkLmVhY2godG1wLCBmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdHYgPSAkLnRyaW0odik7XHJcblx0XHRcdFx0aWYoIXYubGVuZ3RoKSB7IHJldHVybiB0cnVlOyB9XHJcblx0XHRcdFx0ZGF0YS5saV9hdHRyW2ldID0gdjtcclxuXHRcdFx0XHRpZihpID09PSAnaWQnKSB7XHJcblx0XHRcdFx0XHRkYXRhLmlkID0gdjtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cdFx0XHR0bXAgPSBkLmNoaWxkcmVuKCdhJykuZXEoMCk7XHJcblx0XHRcdGlmKHRtcC5sZW5ndGgpIHtcclxuXHRcdFx0XHR0bXAgPSAkLnZha2F0YS5hdHRyaWJ1dGVzKHRtcCwgdHJ1ZSk7XHJcblx0XHRcdFx0JC5lYWNoKHRtcCwgZnVuY3Rpb24gKGksIHYpIHtcclxuXHRcdFx0XHRcdHYgPSAkLnRyaW0odik7XHJcblx0XHRcdFx0XHRpZih2Lmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRkYXRhLmFfYXR0cltpXSA9IHY7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fSk7XHJcblx0XHRcdH1cclxuXHRcdFx0dG1wID0gZC5jaGlsZHJlbihcImE6ZXEoMClcIikubGVuZ3RoID8gZC5jaGlsZHJlbihcImE6ZXEoMClcIikuY2xvbmUoKSA6IGQuY2xvbmUoKTtcclxuXHRcdFx0dG1wLmNoaWxkcmVuKFwiaW5zLCBpLCB1bFwiKS5yZW1vdmUoKTtcclxuXHRcdFx0dG1wID0gdG1wLmh0bWwoKTtcclxuXHRcdFx0dG1wID0gJCgnPGRpdiAvPicpLmh0bWwodG1wKTtcclxuXHRcdFx0ZGF0YS50ZXh0ID0gdG1wLmh0bWwoKTtcclxuXHRcdFx0dG1wID0gZC5kYXRhKCk7XHJcblx0XHRcdGRhdGEuZGF0YSA9IHRtcCA/ICQuZXh0ZW5kKHRydWUsIHt9LCB0bXApIDogbnVsbDtcclxuXHRcdFx0ZGF0YS5zdGF0ZS5vcGVuZWQgPSBkLmhhc0NsYXNzKCdqc3RyZWUtb3BlbicpO1xyXG5cdFx0XHRkYXRhLnN0YXRlLnNlbGVjdGVkID0gZC5jaGlsZHJlbignYScpLmhhc0NsYXNzKCdqc3RyZWUtY2xpY2tlZCcpO1xyXG5cdFx0XHRkYXRhLnN0YXRlLmRpc2FibGVkID0gZC5jaGlsZHJlbignYScpLmhhc0NsYXNzKCdqc3RyZWUtZGlzYWJsZWQnKTtcclxuXHRcdFx0aWYoZGF0YS5kYXRhICYmIGRhdGEuZGF0YS5qc3RyZWUpIHtcclxuXHRcdFx0XHRmb3IoaSBpbiBkYXRhLmRhdGEuanN0cmVlKSB7XHJcblx0XHRcdFx0XHRpZihkYXRhLmRhdGEuanN0cmVlLmhhc093blByb3BlcnR5KGkpKSB7XHJcblx0XHRcdFx0XHRcdGRhdGEuc3RhdGVbaV0gPSBkYXRhLmRhdGEuanN0cmVlW2ldO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHR0bXAgPSBkLmNoaWxkcmVuKFwiYVwiKS5jaGlsZHJlbihcIi5qc3RyZWUtdGhlbWVpY29uXCIpO1xyXG5cdFx0XHRpZih0bXAubGVuZ3RoKSB7XHJcblx0XHRcdFx0ZGF0YS5pY29uID0gdG1wLmhhc0NsYXNzKCdqc3RyZWUtdGhlbWVpY29uLWhpZGRlbicpID8gZmFsc2UgOiB0bXAuYXR0cigncmVsJyk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZGF0YS5zdGF0ZS5pY29uKSB7XHJcblx0XHRcdFx0ZGF0YS5pY29uID0gZGF0YS5zdGF0ZS5pY29uO1xyXG5cdFx0XHR9XHJcblx0XHRcdHRtcCA9IGQuY2hpbGRyZW4oXCJ1bFwiKS5jaGlsZHJlbihcImxpXCIpO1xyXG5cdFx0XHRkbyB7XHJcblx0XHRcdFx0dGlkID0gJ2onICsgdGhpcy5faWQgKyAnXycgKyAoKyt0aGlzLl9jbnQpO1xyXG5cdFx0XHR9IHdoaWxlKG1bdGlkXSk7XHJcblx0XHRcdGRhdGEuaWQgPSBkYXRhLmxpX2F0dHIuaWQgfHwgdGlkO1xyXG5cdFx0XHRpZih0bXAubGVuZ3RoKSB7XHJcblx0XHRcdFx0dG1wLmVhY2goJC5wcm94eShmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdFx0YyA9IHRoaXMuX3BhcnNlX21vZGVsX2Zyb21faHRtbCgkKHYpLCBkYXRhLmlkLCBwcyk7XHJcblx0XHRcdFx0XHRlID0gdGhpcy5fbW9kZWwuZGF0YVtjXTtcclxuXHRcdFx0XHRcdGRhdGEuY2hpbGRyZW4ucHVzaChjKTtcclxuXHRcdFx0XHRcdGlmKGUuY2hpbGRyZW5fZC5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0ZGF0YS5jaGlsZHJlbl9kID0gZGF0YS5jaGlsZHJlbl9kLmNvbmNhdChlLmNoaWxkcmVuX2QpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdFx0XHRkYXRhLmNoaWxkcmVuX2QgPSBkYXRhLmNoaWxkcmVuX2QuY29uY2F0KGRhdGEuY2hpbGRyZW4pO1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdGlmKGQuaGFzQ2xhc3MoJ2pzdHJlZS1jbG9zZWQnKSkge1xyXG5cdFx0XHRcdFx0ZGF0YS5zdGF0ZS5sb2FkZWQgPSBmYWxzZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZGF0YS5saV9hdHRyWydjbGFzcyddKSB7XHJcblx0XHRcdFx0ZGF0YS5saV9hdHRyWydjbGFzcyddID0gZGF0YS5saV9hdHRyWydjbGFzcyddLnJlcGxhY2UoJ2pzdHJlZS1jbG9zZWQnLCcnKS5yZXBsYWNlKCdqc3RyZWUtb3BlbicsJycpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGRhdGEuYV9hdHRyWydjbGFzcyddKSB7XHJcblx0XHRcdFx0ZGF0YS5hX2F0dHJbJ2NsYXNzJ10gPSBkYXRhLmFfYXR0clsnY2xhc3MnXS5yZXBsYWNlKCdqc3RyZWUtY2xpY2tlZCcsJycpLnJlcGxhY2UoJ2pzdHJlZS1kaXNhYmxlZCcsJycpO1xyXG5cdFx0XHR9XHJcblx0XHRcdG1bZGF0YS5pZF0gPSBkYXRhO1xyXG5cdFx0XHRpZihkYXRhLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLnB1c2goZGF0YS5pZCk7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIGRhdGEuaWQ7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBwYXJzZXMgYSBub2RlIGZyb20gYSBKU09OIG9iamVjdCAodXNlZCB3aGVuIGRlYWxpbmcgd2l0aCBmbGF0IGRhdGEsIHdoaWNoIGhhcyBubyBuZXN0aW5nIG9mIGNoaWxkcmVuLCBidXQgaGFzIGlkIGFuZCBwYXJlbnQgcHJvcGVydGllcykgYW5kIGFwcGVuZHMgaXQgdG8gdGhlIGluIG1lbW9yeSB0cmVlIG1vZGVsLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgX3BhcnNlX21vZGVsX2Zyb21fZmxhdF9qc29uKGQgWywgcCwgcHNdKVxyXG5cdFx0ICogQHBhcmFtICB7T2JqZWN0fSBkIHRoZSBKU09OIG9iamVjdCB0byBwYXJzZVxyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBwIHRoZSBwYXJlbnQgSURcclxuXHRcdCAqIEBwYXJhbSAge0FycmF5fSBwcyBsaXN0IG9mIGFsbCBwYXJlbnRzXHJcblx0XHQgKiBAcmV0dXJuIHtTdHJpbmd9IHRoZSBJRCBvZiB0aGUgb2JqZWN0IGFkZGVkIHRvIHRoZSBtb2RlbFxyXG5cdFx0ICovXHJcblx0XHRfcGFyc2VfbW9kZWxfZnJvbV9mbGF0X2pzb24gOiBmdW5jdGlvbiAoZCwgcCwgcHMpIHtcclxuXHRcdFx0aWYoIXBzKSB7IHBzID0gW107IH1cclxuXHRcdFx0ZWxzZSB7IHBzID0gcHMuY29uY2F0KCk7IH1cclxuXHRcdFx0aWYocCkgeyBwcy51bnNoaWZ0KHApOyB9XHJcblx0XHRcdHZhciB0aWQgPSBkLmlkLFxyXG5cdFx0XHRcdG0gPSB0aGlzLl9tb2RlbC5kYXRhLFxyXG5cdFx0XHRcdGRmID0gdGhpcy5fbW9kZWwuZGVmYXVsdF9zdGF0ZSxcclxuXHRcdFx0XHRpLCBqLCBjLCBlLFxyXG5cdFx0XHRcdHRtcCA9IHtcclxuXHRcdFx0XHRcdGlkXHRcdFx0OiB0aWQsXHJcblx0XHRcdFx0XHR0ZXh0XHRcdDogZC50ZXh0IHx8ICcnLFxyXG5cdFx0XHRcdFx0aWNvblx0XHQ6IGQuaWNvbiAhPT0gdW5kZWZpbmVkID8gZC5pY29uIDogdHJ1ZSxcclxuXHRcdFx0XHRcdHBhcmVudFx0XHQ6IHAsXHJcblx0XHRcdFx0XHRwYXJlbnRzXHRcdDogcHMsXHJcblx0XHRcdFx0XHRjaGlsZHJlblx0OiBkLmNoaWxkcmVuIHx8IFtdLFxyXG5cdFx0XHRcdFx0Y2hpbGRyZW5fZFx0OiBkLmNoaWxkcmVuX2QgfHwgW10sXHJcblx0XHRcdFx0XHRkYXRhXHRcdDogZC5kYXRhLFxyXG5cdFx0XHRcdFx0c3RhdGVcdFx0OiB7IH0sXHJcblx0XHRcdFx0XHRsaV9hdHRyXHRcdDogeyBpZCA6IGZhbHNlIH0sXHJcblx0XHRcdFx0XHRhX2F0dHJcdFx0OiB7IGhyZWYgOiAnIycgfSxcclxuXHRcdFx0XHRcdG9yaWdpbmFsXHQ6IGZhbHNlXHJcblx0XHRcdFx0fTtcclxuXHRcdFx0Zm9yKGkgaW4gZGYpIHtcclxuXHRcdFx0XHRpZihkZi5oYXNPd25Qcm9wZXJ0eShpKSkge1xyXG5cdFx0XHRcdFx0dG1wLnN0YXRlW2ldID0gZGZbaV07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGQgJiYgZC5kYXRhICYmIGQuZGF0YS5qc3RyZWUgJiYgZC5kYXRhLmpzdHJlZS5pY29uKSB7XHJcblx0XHRcdFx0dG1wLmljb24gPSBkLmRhdGEuanN0cmVlLmljb247XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZCAmJiBkLmRhdGEpIHtcclxuXHRcdFx0XHR0bXAuZGF0YSA9IGQuZGF0YTtcclxuXHRcdFx0XHRpZihkLmRhdGEuanN0cmVlKSB7XHJcblx0XHRcdFx0XHRmb3IoaSBpbiBkLmRhdGEuanN0cmVlKSB7XHJcblx0XHRcdFx0XHRcdGlmKGQuZGF0YS5qc3RyZWUuaGFzT3duUHJvcGVydHkoaSkpIHtcclxuXHRcdFx0XHRcdFx0XHR0bXAuc3RhdGVbaV0gPSBkLmRhdGEuanN0cmVlW2ldO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGQgJiYgdHlwZW9mIGQuc3RhdGUgPT09ICdvYmplY3QnKSB7XHJcblx0XHRcdFx0Zm9yIChpIGluIGQuc3RhdGUpIHtcclxuXHRcdFx0XHRcdGlmKGQuc3RhdGUuaGFzT3duUHJvcGVydHkoaSkpIHtcclxuXHRcdFx0XHRcdFx0dG1wLnN0YXRlW2ldID0gZC5zdGF0ZVtpXTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZCAmJiB0eXBlb2YgZC5saV9hdHRyID09PSAnb2JqZWN0Jykge1xyXG5cdFx0XHRcdGZvciAoaSBpbiBkLmxpX2F0dHIpIHtcclxuXHRcdFx0XHRcdGlmKGQubGlfYXR0ci5oYXNPd25Qcm9wZXJ0eShpKSkge1xyXG5cdFx0XHRcdFx0XHR0bXAubGlfYXR0cltpXSA9IGQubGlfYXR0cltpXTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoIXRtcC5saV9hdHRyLmlkKSB7XHJcblx0XHRcdFx0dG1wLmxpX2F0dHIuaWQgPSB0aWQ7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZCAmJiB0eXBlb2YgZC5hX2F0dHIgPT09ICdvYmplY3QnKSB7XHJcblx0XHRcdFx0Zm9yIChpIGluIGQuYV9hdHRyKSB7XHJcblx0XHRcdFx0XHRpZihkLmFfYXR0ci5oYXNPd25Qcm9wZXJ0eShpKSkge1xyXG5cdFx0XHRcdFx0XHR0bXAuYV9hdHRyW2ldID0gZC5hX2F0dHJbaV07XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGQgJiYgZC5jaGlsZHJlbiAmJiBkLmNoaWxkcmVuID09PSB0cnVlKSB7XHJcblx0XHRcdFx0dG1wLnN0YXRlLmxvYWRlZCA9IGZhbHNlO1xyXG5cdFx0XHRcdHRtcC5jaGlsZHJlbiA9IFtdO1xyXG5cdFx0XHRcdHRtcC5jaGlsZHJlbl9kID0gW107XHJcblx0XHRcdH1cclxuXHRcdFx0bVt0bXAuaWRdID0gdG1wO1xyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSB0bXAuY2hpbGRyZW4ubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0YyA9IHRoaXMuX3BhcnNlX21vZGVsX2Zyb21fZmxhdF9qc29uKG1bdG1wLmNoaWxkcmVuW2ldXSwgdG1wLmlkLCBwcyk7XHJcblx0XHRcdFx0ZSA9IG1bY107XHJcblx0XHRcdFx0dG1wLmNoaWxkcmVuX2QucHVzaChjKTtcclxuXHRcdFx0XHRpZihlLmNoaWxkcmVuX2QubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHR0bXAuY2hpbGRyZW5fZCA9IHRtcC5jaGlsZHJlbl9kLmNvbmNhdChlLmNoaWxkcmVuX2QpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRkZWxldGUgZC5kYXRhO1xyXG5cdFx0XHRkZWxldGUgZC5jaGlsZHJlbjtcclxuXHRcdFx0bVt0bXAuaWRdLm9yaWdpbmFsID0gZDtcclxuXHRcdFx0aWYodG1wLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLnB1c2godG1wLmlkKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdG1wLmlkO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogcGFyc2VzIGEgbm9kZSBmcm9tIGEgSlNPTiBvYmplY3QgYW5kIGFwcGVuZHMgaXQgdG8gdGhlIGluIG1lbW9yeSB0cmVlIG1vZGVsLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgX3BhcnNlX21vZGVsX2Zyb21fanNvbihkIFssIHAsIHBzXSlcclxuXHRcdCAqIEBwYXJhbSAge09iamVjdH0gZCB0aGUgSlNPTiBvYmplY3QgdG8gcGFyc2VcclxuXHRcdCAqIEBwYXJhbSAge1N0cmluZ30gcCB0aGUgcGFyZW50IElEXHJcblx0XHQgKiBAcGFyYW0gIHtBcnJheX0gcHMgbGlzdCBvZiBhbGwgcGFyZW50c1xyXG5cdFx0ICogQHJldHVybiB7U3RyaW5nfSB0aGUgSUQgb2YgdGhlIG9iamVjdCBhZGRlZCB0byB0aGUgbW9kZWxcclxuXHRcdCAqL1xyXG5cdFx0X3BhcnNlX21vZGVsX2Zyb21fanNvbiA6IGZ1bmN0aW9uIChkLCBwLCBwcykge1xyXG5cdFx0XHRpZighcHMpIHsgcHMgPSBbXTsgfVxyXG5cdFx0XHRlbHNlIHsgcHMgPSBwcy5jb25jYXQoKTsgfVxyXG5cdFx0XHRpZihwKSB7IHBzLnVuc2hpZnQocCk7IH1cclxuXHRcdFx0dmFyIHRpZCA9IGZhbHNlLCBpLCBqLCBjLCBlLCBtID0gdGhpcy5fbW9kZWwuZGF0YSwgZGYgPSB0aGlzLl9tb2RlbC5kZWZhdWx0X3N0YXRlLCB0bXA7XHJcblx0XHRcdGRvIHtcclxuXHRcdFx0XHR0aWQgPSAnaicgKyB0aGlzLl9pZCArICdfJyArICgrK3RoaXMuX2NudCk7XHJcblx0XHRcdH0gd2hpbGUobVt0aWRdKTtcclxuXHJcblx0XHRcdHRtcCA9IHtcclxuXHRcdFx0XHRpZFx0XHRcdDogZmFsc2UsXHJcblx0XHRcdFx0dGV4dFx0XHQ6IHR5cGVvZiBkID09PSAnc3RyaW5nJyA/IGQgOiAnJyxcclxuXHRcdFx0XHRpY29uXHRcdDogdHlwZW9mIGQgPT09ICdvYmplY3QnICYmIGQuaWNvbiAhPT0gdW5kZWZpbmVkID8gZC5pY29uIDogdHJ1ZSxcclxuXHRcdFx0XHRwYXJlbnRcdFx0OiBwLFxyXG5cdFx0XHRcdHBhcmVudHNcdFx0OiBwcyxcclxuXHRcdFx0XHRjaGlsZHJlblx0OiBbXSxcclxuXHRcdFx0XHRjaGlsZHJlbl9kXHQ6IFtdLFxyXG5cdFx0XHRcdGRhdGFcdFx0OiBudWxsLFxyXG5cdFx0XHRcdHN0YXRlXHRcdDogeyB9LFxyXG5cdFx0XHRcdGxpX2F0dHJcdFx0OiB7IGlkIDogZmFsc2UgfSxcclxuXHRcdFx0XHRhX2F0dHJcdFx0OiB7IGhyZWYgOiAnIycgfSxcclxuXHRcdFx0XHRvcmlnaW5hbFx0OiBmYWxzZVxyXG5cdFx0XHR9O1xyXG5cdFx0XHRmb3IoaSBpbiBkZikge1xyXG5cdFx0XHRcdGlmKGRmLmhhc093blByb3BlcnR5KGkpKSB7XHJcblx0XHRcdFx0XHR0bXAuc3RhdGVbaV0gPSBkZltpXTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZCAmJiBkLmlkKSB7IHRtcC5pZCA9IGQuaWQ7IH1cclxuXHRcdFx0aWYoZCAmJiBkLnRleHQpIHsgdG1wLnRleHQgPSBkLnRleHQ7IH1cclxuXHRcdFx0aWYoZCAmJiBkLmRhdGEgJiYgZC5kYXRhLmpzdHJlZSAmJiBkLmRhdGEuanN0cmVlLmljb24pIHtcclxuXHRcdFx0XHR0bXAuaWNvbiA9IGQuZGF0YS5qc3RyZWUuaWNvbjtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZihkICYmIGQuZGF0YSkge1xyXG5cdFx0XHRcdHRtcC5kYXRhID0gZC5kYXRhO1xyXG5cdFx0XHRcdGlmKGQuZGF0YS5qc3RyZWUpIHtcclxuXHRcdFx0XHRcdGZvcihpIGluIGQuZGF0YS5qc3RyZWUpIHtcclxuXHRcdFx0XHRcdFx0aWYoZC5kYXRhLmpzdHJlZS5oYXNPd25Qcm9wZXJ0eShpKSkge1xyXG5cdFx0XHRcdFx0XHRcdHRtcC5zdGF0ZVtpXSA9IGQuZGF0YS5qc3RyZWVbaV07XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZCAmJiB0eXBlb2YgZC5zdGF0ZSA9PT0gJ29iamVjdCcpIHtcclxuXHRcdFx0XHRmb3IgKGkgaW4gZC5zdGF0ZSkge1xyXG5cdFx0XHRcdFx0aWYoZC5zdGF0ZS5oYXNPd25Qcm9wZXJ0eShpKSkge1xyXG5cdFx0XHRcdFx0XHR0bXAuc3RhdGVbaV0gPSBkLnN0YXRlW2ldO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZihkICYmIHR5cGVvZiBkLmxpX2F0dHIgPT09ICdvYmplY3QnKSB7XHJcblx0XHRcdFx0Zm9yIChpIGluIGQubGlfYXR0cikge1xyXG5cdFx0XHRcdFx0aWYoZC5saV9hdHRyLmhhc093blByb3BlcnR5KGkpKSB7XHJcblx0XHRcdFx0XHRcdHRtcC5saV9hdHRyW2ldID0gZC5saV9hdHRyW2ldO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZih0bXAubGlfYXR0ci5pZCAmJiAhdG1wLmlkKSB7XHJcblx0XHRcdFx0dG1wLmlkID0gdG1wLmxpX2F0dHIuaWQ7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoIXRtcC5pZCkge1xyXG5cdFx0XHRcdHRtcC5pZCA9IHRpZDtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZighdG1wLmxpX2F0dHIuaWQpIHtcclxuXHRcdFx0XHR0bXAubGlfYXR0ci5pZCA9IHRtcC5pZDtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZihkICYmIHR5cGVvZiBkLmFfYXR0ciA9PT0gJ29iamVjdCcpIHtcclxuXHRcdFx0XHRmb3IgKGkgaW4gZC5hX2F0dHIpIHtcclxuXHRcdFx0XHRcdGlmKGQuYV9hdHRyLmhhc093blByb3BlcnR5KGkpKSB7XHJcblx0XHRcdFx0XHRcdHRtcC5hX2F0dHJbaV0gPSBkLmFfYXR0cltpXTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoZCAmJiBkLmNoaWxkcmVuICYmIGQuY2hpbGRyZW4ubGVuZ3RoKSB7XHJcblx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gZC5jaGlsZHJlbi5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdGMgPSB0aGlzLl9wYXJzZV9tb2RlbF9mcm9tX2pzb24oZC5jaGlsZHJlbltpXSwgdG1wLmlkLCBwcyk7XHJcblx0XHRcdFx0XHRlID0gbVtjXTtcclxuXHRcdFx0XHRcdHRtcC5jaGlsZHJlbi5wdXNoKGMpO1xyXG5cdFx0XHRcdFx0aWYoZS5jaGlsZHJlbl9kLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHR0bXAuY2hpbGRyZW5fZCA9IHRtcC5jaGlsZHJlbl9kLmNvbmNhdChlLmNoaWxkcmVuX2QpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHR0bXAuY2hpbGRyZW5fZCA9IHRtcC5jaGlsZHJlbl9kLmNvbmNhdCh0bXAuY2hpbGRyZW4pO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGQgJiYgZC5jaGlsZHJlbiAmJiBkLmNoaWxkcmVuID09PSB0cnVlKSB7XHJcblx0XHRcdFx0dG1wLnN0YXRlLmxvYWRlZCA9IGZhbHNlO1xyXG5cdFx0XHRcdHRtcC5jaGlsZHJlbiA9IFtdO1xyXG5cdFx0XHRcdHRtcC5jaGlsZHJlbl9kID0gW107XHJcblx0XHRcdH1cclxuXHRcdFx0ZGVsZXRlIGQuZGF0YTtcclxuXHRcdFx0ZGVsZXRlIGQuY2hpbGRyZW47XHJcblx0XHRcdHRtcC5vcmlnaW5hbCA9IGQ7XHJcblx0XHRcdG1bdG1wLmlkXSA9IHRtcDtcclxuXHRcdFx0aWYodG1wLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLnB1c2godG1wLmlkKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdG1wLmlkO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogcmVkcmF3cyBhbGwgbm9kZXMgdGhhdCBuZWVkIHRvIGJlIHJlZHJhd24uIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBfcmVkcmF3KClcclxuXHRcdCAqIEB0cmlnZ2VyIHJlZHJhdy5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0X3JlZHJhdyA6IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0dmFyIG5vZGVzID0gdGhpcy5fbW9kZWwuZm9yY2VfZnVsbF9yZWRyYXcgPyB0aGlzLl9tb2RlbC5kYXRhWycjJ10uY2hpbGRyZW4uY29uY2F0KFtdKSA6IHRoaXMuX21vZGVsLmNoYW5nZWQuY29uY2F0KFtdKSxcclxuXHRcdFx0XHRmID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnVUwnKSwgdG1wLCBpLCBqO1xyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSBub2Rlcy5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHR0bXAgPSB0aGlzLnJlZHJhd19ub2RlKG5vZGVzW2ldLCB0cnVlLCB0aGlzLl9tb2RlbC5mb3JjZV9mdWxsX3JlZHJhdyk7XHJcblx0XHRcdFx0aWYodG1wICYmIHRoaXMuX21vZGVsLmZvcmNlX2Z1bGxfcmVkcmF3KSB7XHJcblx0XHRcdFx0XHRmLmFwcGVuZENoaWxkKHRtcCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHRoaXMuX21vZGVsLmZvcmNlX2Z1bGxfcmVkcmF3KSB7XHJcblx0XHRcdFx0Zi5jbGFzc05hbWUgPSB0aGlzLmdldF9jb250YWluZXJfdWwoKVswXS5jbGFzc05hbWU7XHJcblx0XHRcdFx0dGhpcy5lbGVtZW50LmVtcHR5KCkuYXBwZW5kKGYpO1xyXG5cdFx0XHRcdC8vdGhpcy5nZXRfY29udGFpbmVyX3VsKClbMF0uYXBwZW5kQ2hpbGQoZik7XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5fbW9kZWwuZm9yY2VfZnVsbF9yZWRyYXcgPSBmYWxzZTtcclxuXHRcdFx0dGhpcy5fbW9kZWwuY2hhbmdlZCA9IFtdO1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIGFmdGVyIG5vZGVzIGFyZSByZWRyYXduXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSByZWRyYXcuanN0cmVlXHJcblx0XHRcdCAqIEBwYXJhbSB7YXJyYXl9IG5vZGVzIHRoZSByZWRyYXduIG5vZGVzXHJcblx0XHRcdCAqL1xyXG5cdFx0XHR0aGlzLnRyaWdnZXIoJ3JlZHJhdycsIHsgXCJub2Rlc1wiIDogbm9kZXMgfSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiByZWRyYXdzIGFsbCBub2RlcyB0aGF0IG5lZWQgdG8gYmUgcmVkcmF3biBvciBvcHRpb25hbGx5IC0gdGhlIHdob2xlIHRyZWVcclxuXHRcdCAqIEBuYW1lIHJlZHJhdyhbZnVsbF0pXHJcblx0XHQgKiBAcGFyYW0ge0Jvb2xlYW59IGZ1bGwgaWYgc2V0IHRvIGB0cnVlYCBhbGwgbm9kZXMgYXJlIHJlZHJhd24uXHJcblx0XHQgKi9cclxuXHRcdHJlZHJhdyA6IGZ1bmN0aW9uIChmdWxsKSB7XHJcblx0XHRcdGlmKGZ1bGwpIHtcclxuXHRcdFx0XHR0aGlzLl9tb2RlbC5mb3JjZV9mdWxsX3JlZHJhdyA9IHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0Ly9pZih0aGlzLl9tb2RlbC5yZWRyYXdfdGltZW91dCkge1xyXG5cdFx0XHQvL1x0Y2xlYXJUaW1lb3V0KHRoaXMuX21vZGVsLnJlZHJhd190aW1lb3V0KTtcclxuXHRcdFx0Ly99XHJcblx0XHRcdC8vdGhpcy5fbW9kZWwucmVkcmF3X3RpbWVvdXQgPSBzZXRUaW1lb3V0KCQucHJveHkodGhpcy5fcmVkcmF3LCB0aGlzKSwwKTtcclxuXHRcdFx0dGhpcy5fcmVkcmF3KCk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiByZWRyYXdzIGEgc2luZ2xlIG5vZGUuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSByZWRyYXdfbm9kZShub2RlLCBkZWVwLCBpc19jYWxsYmFjaylcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG5vZGUgdGhlIG5vZGUgdG8gcmVkcmF3XHJcblx0XHQgKiBAcGFyYW0ge0Jvb2xlYW59IGRlZXAgc2hvdWxkIGNoaWxkIG5vZGVzIGJlIHJlZHJhd24gdG9vXHJcblx0XHQgKiBAcGFyYW0ge0Jvb2xlYW59IGlzX2NhbGxiYWNrIGlzIHRoaXMgYSByZWN1cnNpb24gY2FsbFxyXG5cdFx0ICovXHJcblx0XHRyZWRyYXdfbm9kZSA6IGZ1bmN0aW9uIChub2RlLCBkZWVwLCBpc19jYWxsYmFjaykge1xyXG5cdFx0XHR2YXIgb2JqID0gdGhpcy5nZXRfbm9kZShub2RlKSxcclxuXHRcdFx0XHRwYXIgPSBmYWxzZSxcclxuXHRcdFx0XHRpbmQgPSBmYWxzZSxcclxuXHRcdFx0XHRvbGQgPSBmYWxzZSxcclxuXHRcdFx0XHRpID0gZmFsc2UsXHJcblx0XHRcdFx0aiA9IGZhbHNlLFxyXG5cdFx0XHRcdGsgPSBmYWxzZSxcclxuXHRcdFx0XHRjID0gJycsXHJcblx0XHRcdFx0ZCA9IGRvY3VtZW50LFxyXG5cdFx0XHRcdG0gPSB0aGlzLl9tb2RlbC5kYXRhLFxyXG5cdFx0XHRcdGYgPSBmYWxzZSxcclxuXHRcdFx0XHRzID0gZmFsc2U7XHJcblx0XHRcdGlmKCFvYmopIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdGlmKG9iai5pZCA9PT0gJyMnKSB7ICByZXR1cm4gdGhpcy5yZWRyYXcodHJ1ZSk7IH1cclxuXHRcdFx0ZGVlcCA9IGRlZXAgfHwgb2JqLmNoaWxkcmVuLmxlbmd0aCA9PT0gMDtcclxuXHRcdFx0bm9kZSA9IGQuZ2V0RWxlbWVudEJ5SWQob2JqLmlkKTsgLy8sIHRoaXMuZWxlbWVudCk7XHJcblx0XHRcdGlmKCFub2RlKSB7XHJcblx0XHRcdFx0ZGVlcCA9IHRydWU7XHJcblx0XHRcdFx0Ly9ub2RlID0gZC5jcmVhdGVFbGVtZW50KCdMSScpO1xyXG5cdFx0XHRcdGlmKCFpc19jYWxsYmFjaykge1xyXG5cdFx0XHRcdFx0cGFyID0gb2JqLnBhcmVudCAhPT0gJyMnID8gJCgnIycgKyBvYmoucGFyZW50LCB0aGlzLmVsZW1lbnQpWzBdIDogbnVsbDtcclxuXHRcdFx0XHRcdGlmKHBhciAhPT0gbnVsbCAmJiAoIXBhciB8fCAhbVtvYmoucGFyZW50XS5zdGF0ZS5vcGVuZWQpKSB7XHJcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGluZCA9ICQuaW5BcnJheShvYmouaWQsIHBhciA9PT0gbnVsbCA/IG1bJyMnXS5jaGlsZHJlbiA6IG1bb2JqLnBhcmVudF0uY2hpbGRyZW4pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRub2RlID0gJChub2RlKTtcclxuXHRcdFx0XHRpZighaXNfY2FsbGJhY2spIHtcclxuXHRcdFx0XHRcdHBhciA9IG5vZGUucGFyZW50KCkucGFyZW50KClbMF07XHJcblx0XHRcdFx0XHRpZihwYXIgPT09IHRoaXMuZWxlbWVudFswXSkge1xyXG5cdFx0XHRcdFx0XHRwYXIgPSBudWxsO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0aW5kID0gbm9kZS5pbmRleCgpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHQvLyBtW29iai5pZF0uZGF0YSA9IG5vZGUuZGF0YSgpOyAvLyB1c2Ugb25seSBub2RlJ3MgZGF0YSwgbm8gbmVlZCB0byB0b3VjaCBqcXVlcnkgc3RvcmFnZVxyXG5cdFx0XHRcdGlmKCFkZWVwICYmIG9iai5jaGlsZHJlbi5sZW5ndGggJiYgIW5vZGUuY2hpbGRyZW4oJ3VsJykubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRkZWVwID0gdHJ1ZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYoIWRlZXApIHtcclxuXHRcdFx0XHRcdG9sZCA9IG5vZGUuY2hpbGRyZW4oJ1VMJylbMF07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHMgPSBub2RlLmF0dHIoJ2FyaWEtc2VsZWN0ZWQnKTtcclxuXHRcdFx0XHRmID0gbm9kZS5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKVswXSA9PT0gZG9jdW1lbnQuYWN0aXZlRWxlbWVudDtcclxuXHRcdFx0XHRub2RlLnJlbW92ZSgpO1xyXG5cdFx0XHRcdC8vbm9kZSA9IGQuY3JlYXRlRWxlbWVudCgnTEknKTtcclxuXHRcdFx0XHQvL25vZGUgPSBub2RlWzBdO1xyXG5cdFx0XHR9XHJcblx0XHRcdG5vZGUgPSBfbm9kZS5jbG9uZU5vZGUodHJ1ZSk7XHJcblx0XHRcdC8vIG5vZGUgaXMgRE9NLCBkZWVwIGlzIGJvb2xlYW5cclxuXHJcblx0XHRcdGMgPSAnanN0cmVlLW5vZGUgJztcclxuXHRcdFx0Zm9yKGkgaW4gb2JqLmxpX2F0dHIpIHtcclxuXHRcdFx0XHRpZihvYmoubGlfYXR0ci5oYXNPd25Qcm9wZXJ0eShpKSkge1xyXG5cdFx0XHRcdFx0aWYoaSA9PT0gJ2lkJykgeyBjb250aW51ZTsgfVxyXG5cdFx0XHRcdFx0aWYoaSAhPT0gJ2NsYXNzJykge1xyXG5cdFx0XHRcdFx0XHRub2RlLnNldEF0dHJpYnV0ZShpLCBvYmoubGlfYXR0cltpXSk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0YyArPSBvYmoubGlfYXR0cltpXTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYocyAmJiBzICE9PSBcImZhbHNlXCIpIHtcclxuXHRcdFx0XHRub2RlLnNldEF0dHJpYnV0ZSgnYXJpYS1zZWxlY3RlZCcsIHRydWUpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKCFvYmouY2hpbGRyZW4ubGVuZ3RoICYmIG9iai5zdGF0ZS5sb2FkZWQpIHtcclxuXHRcdFx0XHRjICs9ICcganN0cmVlLWxlYWYnO1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdGMgKz0gb2JqLnN0YXRlLm9wZW5lZCA/ICcganN0cmVlLW9wZW4nIDogJyBqc3RyZWUtY2xvc2VkJztcclxuXHRcdFx0XHRub2RlLnNldEF0dHJpYnV0ZSgnYXJpYS1leHBhbmRlZCcsIG9iai5zdGF0ZS5vcGVuZWQpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKG9iai5wYXJlbnQgIT09IG51bGwgJiYgbVtvYmoucGFyZW50XS5jaGlsZHJlblttW29iai5wYXJlbnRdLmNoaWxkcmVuLmxlbmd0aCAtIDFdID09PSBvYmouaWQpIHtcclxuXHRcdFx0XHRjICs9ICcganN0cmVlLWxhc3QnO1xyXG5cdFx0XHR9XHJcblx0XHRcdG5vZGUuaWQgPSBvYmouaWQ7XHJcblx0XHRcdG5vZGUuY2xhc3NOYW1lID0gYztcclxuXHRcdFx0YyA9ICggb2JqLnN0YXRlLnNlbGVjdGVkID8gJyBqc3RyZWUtY2xpY2tlZCcgOiAnJykgKyAoIG9iai5zdGF0ZS5kaXNhYmxlZCA/ICcganN0cmVlLWRpc2FibGVkJyA6ICcnKTtcclxuXHRcdFx0Zm9yKGogaW4gb2JqLmFfYXR0cikge1xyXG5cdFx0XHRcdGlmKG9iai5hX2F0dHIuaGFzT3duUHJvcGVydHkoaikpIHtcclxuXHRcdFx0XHRcdGlmKGogPT09ICdocmVmJyAmJiBvYmouYV9hdHRyW2pdID09PSAnIycpIHsgY29udGludWU7IH1cclxuXHRcdFx0XHRcdGlmKGogIT09ICdjbGFzcycpIHtcclxuXHRcdFx0XHRcdFx0bm9kZS5jaGlsZE5vZGVzWzFdLnNldEF0dHJpYnV0ZShqLCBvYmouYV9hdHRyW2pdKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRjICs9ICcgJyArIG9iai5hX2F0dHJbal07XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGMubGVuZ3RoKSB7XHJcblx0XHRcdFx0bm9kZS5jaGlsZE5vZGVzWzFdLmNsYXNzTmFtZSA9ICdqc3RyZWUtYW5jaG9yICcgKyBjO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKChvYmouaWNvbiAmJiBvYmouaWNvbiAhPT0gdHJ1ZSkgfHwgb2JqLmljb24gPT09IGZhbHNlKSB7XHJcblx0XHRcdFx0aWYob2JqLmljb24gPT09IGZhbHNlKSB7XHJcblx0XHRcdFx0XHRub2RlLmNoaWxkTm9kZXNbMV0uY2hpbGROb2Rlc1swXS5jbGFzc05hbWUgKz0gJyBqc3RyZWUtdGhlbWVpY29uLWhpZGRlbic7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGVsc2UgaWYob2JqLmljb24uaW5kZXhPZignLycpID09PSAtMSAmJiBvYmouaWNvbi5pbmRleE9mKCcuJykgPT09IC0xKSB7XHJcblx0XHRcdFx0XHRub2RlLmNoaWxkTm9kZXNbMV0uY2hpbGROb2Rlc1swXS5jbGFzc05hbWUgKz0gJyAnICsgb2JqLmljb24gKyAnIGpzdHJlZS10aGVtZWljb24tY3VzdG9tJztcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRub2RlLmNoaWxkTm9kZXNbMV0uY2hpbGROb2Rlc1swXS5zdHlsZS5iYWNrZ3JvdW5kSW1hZ2UgPSAndXJsKCcrb2JqLmljb24rJyknO1xyXG5cdFx0XHRcdFx0bm9kZS5jaGlsZE5vZGVzWzFdLmNoaWxkTm9kZXNbMF0uc3R5bGUuYmFja2dyb3VuZFBvc2l0aW9uID0gJ2NlbnRlciBjZW50ZXInO1xyXG5cdFx0XHRcdFx0bm9kZS5jaGlsZE5vZGVzWzFdLmNoaWxkTm9kZXNbMF0uc3R5bGUuYmFja2dyb3VuZFNpemUgPSAnYXV0byc7XHJcblx0XHRcdFx0XHRub2RlLmNoaWxkTm9kZXNbMV0uY2hpbGROb2Rlc1swXS5jbGFzc05hbWUgKz0gJyBqc3RyZWUtdGhlbWVpY29uLWN1c3RvbSc7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdC8vbm9kZS5jaGlsZE5vZGVzWzFdLmFwcGVuZENoaWxkKGQuY3JlYXRlVGV4dE5vZGUob2JqLnRleHQpKTtcclxuXHRcdFx0bm9kZS5jaGlsZE5vZGVzWzFdLmlubmVySFRNTCArPSBvYmoudGV4dDtcclxuXHRcdFx0Ly8gaWYob2JqLmRhdGEpIHsgJC5kYXRhKG5vZGUsIG9iai5kYXRhKTsgfSAvLyBhbHdheXMgd29yayB3aXRoIG5vZGUncyBkYXRhLCBubyBuZWVkIHRvIHRvdWNoIGpxdWVyeSBzdG9yZVxyXG5cclxuXHRcdFx0aWYoZGVlcCAmJiBvYmouY2hpbGRyZW4ubGVuZ3RoICYmIG9iai5zdGF0ZS5vcGVuZWQpIHtcclxuXHRcdFx0XHRrID0gZC5jcmVhdGVFbGVtZW50KCdVTCcpO1xyXG5cdFx0XHRcdGsuc2V0QXR0cmlidXRlKCdyb2xlJywgJ2dyb3VwJyk7XHJcblx0XHRcdFx0ay5jbGFzc05hbWUgPSAnanN0cmVlLWNoaWxkcmVuJztcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSBvYmouY2hpbGRyZW4ubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRrLmFwcGVuZENoaWxkKHRoaXMucmVkcmF3X25vZGUob2JqLmNoaWxkcmVuW2ldLCBkZWVwLCB0cnVlKSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdG5vZGUuYXBwZW5kQ2hpbGQoayk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYob2xkKSB7XHJcblx0XHRcdFx0bm9kZS5hcHBlbmRDaGlsZChvbGQpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKCFpc19jYWxsYmFjaykge1xyXG5cdFx0XHRcdC8vIGFwcGVuZCBiYWNrIHVzaW5nIHBhciAvIGluZFxyXG5cdFx0XHRcdGlmKCFwYXIpIHtcclxuXHRcdFx0XHRcdHBhciA9IHRoaXMuZWxlbWVudFswXTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYoIXBhci5nZXRFbGVtZW50c0J5VGFnTmFtZSgnVUwnKS5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdGkgPSBkLmNyZWF0ZUVsZW1lbnQoJ1VMJyk7XHJcblx0XHRcdFx0XHRpLnNldEF0dHJpYnV0ZSgncm9sZScsICdncm91cCcpO1xyXG5cdFx0XHRcdFx0aS5jbGFzc05hbWUgPSAnanN0cmVlLWNoaWxkcmVuJztcclxuXHRcdFx0XHRcdHBhci5hcHBlbmRDaGlsZChpKTtcclxuXHRcdFx0XHRcdHBhciA9IGk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0cGFyID0gcGFyLmdldEVsZW1lbnRzQnlUYWdOYW1lKCdVTCcpWzBdO1xyXG5cdFx0XHRcdH1cclxuXHJcblx0XHRcdFx0aWYoaW5kIDwgcGFyLmNoaWxkTm9kZXMubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRwYXIuaW5zZXJ0QmVmb3JlKG5vZGUsIHBhci5jaGlsZE5vZGVzW2luZF0pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdHBhci5hcHBlbmRDaGlsZChub2RlKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYoZikge1xyXG5cdFx0XHRcdFx0bm9kZS5jaGlsZE5vZGVzWzFdLmZvY3VzKCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiBub2RlO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogb3BlbnMgYSBub2RlLCByZXZhbGluZyBpdHMgY2hpbGRyZW4uIElmIHRoZSBub2RlIGlzIG5vdCBsb2FkZWQgaXQgd2lsbCBiZSBsb2FkZWQgYW5kIG9wZW5lZCBvbmNlIHJlYWR5LlxyXG5cdFx0ICogQG5hbWUgb3Blbl9ub2RlKG9iaiBbLCBjYWxsYmFjaywgYW5pbWF0aW9uXSlcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9iaiB0aGUgbm9kZSB0byBvcGVuXHJcblx0XHQgKiBAcGFyYW0ge0Z1bmN0aW9ufSBjYWxsYmFjayBhIGZ1bmN0aW9uIHRvIGV4ZWN1dGUgb25jZSB0aGUgbm9kZSBpcyBvcGVuZWRcclxuXHRcdCAqIEBwYXJhbSB7TnVtYmVyfSBhbmltYXRpb24gdGhlIGFuaW1hdGlvbiBkdXJhdGlvbiBpbiBtaWxsaXNlY29uZHMgd2hlbiBvcGVuaW5nIHRoZSBub2RlIChvdmVycmlkZXMgdGhlIGBjb3JlLmFuaW1hdGlvbmAgc2V0dGluZykuIFVzZSBgZmFsc2VgIGZvciBubyBhbmltYXRpb24uXHJcblx0XHQgKiBAdHJpZ2dlciBvcGVuX25vZGUuanN0cmVlLCBhZnRlcl9vcGVuLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRvcGVuX25vZGUgOiBmdW5jdGlvbiAob2JqLCBjYWxsYmFjaywgYW5pbWF0aW9uKSB7XHJcblx0XHRcdHZhciB0MSwgdDIsIGQsIHQ7XHJcblx0XHRcdGlmKCQuaXNBcnJheShvYmopKSB7XHJcblx0XHRcdFx0b2JqID0gb2JqLnNsaWNlKCk7XHJcblx0XHRcdFx0Zm9yKHQxID0gMCwgdDIgPSBvYmoubGVuZ3RoOyB0MSA8IHQyOyB0MSsrKSB7XHJcblx0XHRcdFx0XHR0aGlzLm9wZW5fbm9kZShvYmpbdDFdLCBjYWxsYmFjaywgYW5pbWF0aW9uKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGFuaW1hdGlvbiA9IGFuaW1hdGlvbiA9PT0gdW5kZWZpbmVkID8gdGhpcy5zZXR0aW5ncy5jb3JlLmFuaW1hdGlvbiA6IGFuaW1hdGlvbjtcclxuXHRcdFx0aWYoIXRoaXMuaXNfY2xvc2VkKG9iaikpIHtcclxuXHRcdFx0XHRpZihjYWxsYmFjaykge1xyXG5cdFx0XHRcdFx0Y2FsbGJhY2suY2FsbCh0aGlzLCBvYmosIGZhbHNlKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKCF0aGlzLmlzX2xvYWRlZChvYmopKSB7XHJcblx0XHRcdFx0aWYodGhpcy5pc19sb2FkaW5nKG9iaikpIHtcclxuXHRcdFx0XHRcdHJldHVybiBzZXRUaW1lb3V0KCQucHJveHkoZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLm9wZW5fbm9kZShvYmosIGNhbGxiYWNrLCBhbmltYXRpb24pO1xyXG5cdFx0XHRcdFx0fSwgdGhpcyksIDUwMCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHRoaXMubG9hZF9ub2RlKG9iaiwgZnVuY3Rpb24gKG8sIG9rKSB7XHJcblx0XHRcdFx0XHRyZXR1cm4gb2sgPyB0aGlzLm9wZW5fbm9kZShvLCBjYWxsYmFjaywgYW5pbWF0aW9uKSA6IChjYWxsYmFjayA/IGNhbGxiYWNrLmNhbGwodGhpcywgbywgZmFsc2UpIDogZmFsc2UpO1xyXG5cdFx0XHRcdH0pO1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdGQgPSB0aGlzLmdldF9ub2RlKG9iaiwgdHJ1ZSk7XHJcblx0XHRcdFx0dCA9IHRoaXM7XHJcblx0XHRcdFx0aWYoZC5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdGlmKG9iai5jaGlsZHJlbi5sZW5ndGggJiYgIXRoaXMuX2ZpcnN0Q2hpbGQoZC5jaGlsZHJlbigndWwnKVswXSkpIHtcclxuXHRcdFx0XHRcdFx0b2JqLnN0YXRlLm9wZW5lZCA9IHRydWU7XHJcblx0XHRcdFx0XHRcdHRoaXMucmVkcmF3X25vZGUob2JqLCB0cnVlKTtcclxuXHRcdFx0XHRcdFx0ZCA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGlmKCFhbmltYXRpb24pIHtcclxuXHRcdFx0XHRcdFx0ZFswXS5jbGFzc05hbWUgPSBkWzBdLmNsYXNzTmFtZS5yZXBsYWNlKCdqc3RyZWUtY2xvc2VkJywgJ2pzdHJlZS1vcGVuJyk7XHJcblx0XHRcdFx0XHRcdGRbMF0uc2V0QXR0cmlidXRlKFwiYXJpYS1leHBhbmRlZFwiLCB0cnVlKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRkXHJcblx0XHRcdFx0XHRcdFx0LmNoaWxkcmVuKFwidWxcIikuY3NzKFwiZGlzcGxheVwiLFwibm9uZVwiKS5lbmQoKVxyXG5cdFx0XHRcdFx0XHRcdC5yZW1vdmVDbGFzcyhcImpzdHJlZS1jbG9zZWRcIikuYWRkQ2xhc3MoXCJqc3RyZWUtb3BlblwiKS5hdHRyKFwiYXJpYS1leHBhbmRlZFwiLCB0cnVlKVxyXG5cdFx0XHRcdFx0XHRcdC5jaGlsZHJlbihcInVsXCIpLnN0b3AodHJ1ZSwgdHJ1ZSlcclxuXHRcdFx0XHRcdFx0XHRcdC5zbGlkZURvd24oYW5pbWF0aW9uLCBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHRoaXMuc3R5bGUuZGlzcGxheSA9IFwiXCI7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHQudHJpZ2dlcihcImFmdGVyX29wZW5cIiwgeyBcIm5vZGVcIiA6IG9iaiB9KTtcclxuXHRcdFx0XHRcdFx0XHRcdH0pO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRvYmouc3RhdGUub3BlbmVkID0gdHJ1ZTtcclxuXHRcdFx0XHRpZihjYWxsYmFjaykge1xyXG5cdFx0XHRcdFx0Y2FsbGJhY2suY2FsbCh0aGlzLCBvYmosIHRydWUpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHQvKipcclxuXHRcdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhIG5vZGUgaXMgb3BlbmVkIChpZiB0aGVyZSBpcyBhbiBhbmltYXRpb24gaXQgd2lsbCBub3QgYmUgY29tcGxldGVkIHlldClcclxuXHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHQgKiBAbmFtZSBvcGVuX25vZGUuanN0cmVlXHJcblx0XHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGUgdGhlIG9wZW5lZCBub2RlXHJcblx0XHRcdFx0ICovXHJcblx0XHRcdFx0dGhpcy50cmlnZ2VyKCdvcGVuX25vZGUnLCB7IFwibm9kZVwiIDogb2JqIH0pO1xyXG5cdFx0XHRcdGlmKCFhbmltYXRpb24gfHwgIWQubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHQvKipcclxuXHRcdFx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGEgbm9kZSBpcyBvcGVuZWQgYW5kIHRoZSBhbmltYXRpb24gaXMgY29tcGxldGVcclxuXHRcdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdFx0ICogQG5hbWUgYWZ0ZXJfb3Blbi5qc3RyZWVcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBub2RlIHRoZSBvcGVuZWQgbm9kZVxyXG5cdFx0XHRcdFx0ICovXHJcblx0XHRcdFx0XHR0aGlzLnRyaWdnZXIoXCJhZnRlcl9vcGVuXCIsIHsgXCJub2RlXCIgOiBvYmogfSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBvcGVucyBldmVyeSBwYXJlbnQgb2YgYSBub2RlIChub2RlIHNob3VsZCBiZSBsb2FkZWQpXHJcblx0XHQgKiBAbmFtZSBfb3Blbl90byhvYmopXHJcblx0XHQgKiBAcGFyYW0ge21peGVkfSBvYmogdGhlIG5vZGUgdG8gcmV2ZWFsXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICovXHJcblx0XHRfb3Blbl90byA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdHZhciBpLCBqLCBwID0gb2JqLnBhcmVudHM7XHJcblx0XHRcdGZvcihpID0gMCwgaiA9IHAubGVuZ3RoOyBpIDwgajsgaSs9MSkge1xyXG5cdFx0XHRcdGlmKGkgIT09ICcjJykge1xyXG5cdFx0XHRcdFx0dGhpcy5vcGVuX25vZGUocFtpXSwgZmFsc2UsIDApO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gJChkb2N1bWVudC5nZXRFbGVtZW50QnlJZChvYmouaWQpKTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGNsb3NlcyBhIG5vZGUsIGhpZGluZyBpdHMgY2hpbGRyZW5cclxuXHRcdCAqIEBuYW1lIGNsb3NlX25vZGUob2JqIFssIGFuaW1hdGlvbl0pXHJcblx0XHQgKiBAcGFyYW0ge21peGVkfSBvYmogdGhlIG5vZGUgdG8gY2xvc2VcclxuXHRcdCAqIEBwYXJhbSB7TnVtYmVyfSBhbmltYXRpb24gdGhlIGFuaW1hdGlvbiBkdXJhdGlvbiBpbiBtaWxsaXNlY29uZHMgd2hlbiBjbG9zaW5nIHRoZSBub2RlIChvdmVycmlkZXMgdGhlIGBjb3JlLmFuaW1hdGlvbmAgc2V0dGluZykuIFVzZSBgZmFsc2VgIGZvciBubyBhbmltYXRpb24uXHJcblx0XHQgKiBAdHJpZ2dlciBjbG9zZV9ub2RlLmpzdHJlZSwgYWZ0ZXJfY2xvc2UuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGNsb3NlX25vZGUgOiBmdW5jdGlvbiAob2JqLCBhbmltYXRpb24pIHtcclxuXHRcdFx0dmFyIHQxLCB0MiwgdCwgZDtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuY2xvc2Vfbm9kZShvYmpbdDFdLCBhbmltYXRpb24pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqLmlkID09PSAnIycpIHtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0YW5pbWF0aW9uID0gYW5pbWF0aW9uID09PSB1bmRlZmluZWQgPyB0aGlzLnNldHRpbmdzLmNvcmUuYW5pbWF0aW9uIDogYW5pbWF0aW9uO1xyXG5cdFx0XHR0ID0gdGhpcztcclxuXHRcdFx0ZCA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKTtcclxuXHRcdFx0aWYoZC5sZW5ndGgpIHtcclxuXHRcdFx0XHRpZighYW5pbWF0aW9uKSB7XHJcblx0XHRcdFx0XHRkWzBdLmNsYXNzTmFtZSA9IGRbMF0uY2xhc3NOYW1lLnJlcGxhY2UoJ2pzdHJlZS1vcGVuJywgJ2pzdHJlZS1jbG9zZWQnKTtcclxuXHRcdFx0XHRcdGQuYXR0cihcImFyaWEtZXhwYW5kZWRcIiwgZmFsc2UpLmNoaWxkcmVuKCd1bCcpLnJlbW92ZSgpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdGRcclxuXHRcdFx0XHRcdFx0LmNoaWxkcmVuKFwidWxcIikuYXR0cihcInN0eWxlXCIsXCJkaXNwbGF5OmJsb2NrICFpbXBvcnRhbnRcIikuZW5kKClcclxuXHRcdFx0XHRcdFx0LnJlbW92ZUNsYXNzKFwianN0cmVlLW9wZW5cIikuYWRkQ2xhc3MoXCJqc3RyZWUtY2xvc2VkXCIpLmF0dHIoXCJhcmlhLWV4cGFuZGVkXCIsIGZhbHNlKVxyXG5cdFx0XHRcdFx0XHQuY2hpbGRyZW4oXCJ1bFwiKS5zdG9wKHRydWUsIHRydWUpLnNsaWRlVXAoYW5pbWF0aW9uLCBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5zdHlsZS5kaXNwbGF5ID0gXCJcIjtcclxuXHRcdFx0XHRcdFx0XHRkLmNoaWxkcmVuKCd1bCcpLnJlbW92ZSgpO1xyXG5cdFx0XHRcdFx0XHRcdHQudHJpZ2dlcihcImFmdGVyX2Nsb3NlXCIsIHsgXCJub2RlXCIgOiBvYmogfSk7XHJcblx0XHRcdFx0XHRcdH0pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRvYmouc3RhdGUub3BlbmVkID0gZmFsc2U7XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhIG5vZGUgaXMgY2xvc2VkIChpZiB0aGVyZSBpcyBhbiBhbmltYXRpb24gaXQgd2lsbCBub3QgYmUgY29tcGxldGUgeWV0KVxyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgY2xvc2Vfbm9kZS5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGUgdGhlIGNsb3NlZCBub2RlXHJcblx0XHRcdCAqL1xyXG5cdFx0XHR0aGlzLnRyaWdnZXIoJ2Nsb3NlX25vZGUnLHsgXCJub2RlXCIgOiBvYmogfSk7XHJcblx0XHRcdGlmKCFhbmltYXRpb24gfHwgIWQubGVuZ3RoKSB7XHJcblx0XHRcdFx0LyoqXHJcblx0XHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gYSBub2RlIGlzIGNsb3NlZCBhbmQgdGhlIGFuaW1hdGlvbiBpcyBjb21wbGV0ZVxyXG5cdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdCAqIEBuYW1lIGFmdGVyX2Nsb3NlLmpzdHJlZVxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBub2RlIHRoZSBjbG9zZWQgbm9kZVxyXG5cdFx0XHRcdCAqL1xyXG5cdFx0XHRcdHRoaXMudHJpZ2dlcihcImFmdGVyX2Nsb3NlXCIsIHsgXCJub2RlXCIgOiBvYmogfSk7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHRvZ2dsZXMgYSBub2RlIC0gY2xvc2luZyBpdCBpZiBpdCBpcyBvcGVuLCBvcGVuaW5nIGl0IGlmIGl0IGlzIGNsb3NlZFxyXG5cdFx0ICogQG5hbWUgdG9nZ2xlX25vZGUob2JqKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqIHRoZSBub2RlIHRvIHRvZ2dsZVxyXG5cdFx0ICovXHJcblx0XHR0b2dnbGVfbm9kZSA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0dmFyIHQxLCB0MjtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMudG9nZ2xlX25vZGUob2JqW3QxXSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHRoaXMuaXNfY2xvc2VkKG9iaikpIHtcclxuXHRcdFx0XHRyZXR1cm4gdGhpcy5vcGVuX25vZGUob2JqKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZih0aGlzLmlzX29wZW4ob2JqKSkge1xyXG5cdFx0XHRcdHJldHVybiB0aGlzLmNsb3NlX25vZGUob2JqKTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogb3BlbnMgYWxsIG5vZGVzIHdpdGhpbiBhIG5vZGUgKG9yIHRoZSB0cmVlKSwgcmV2YWxpbmcgdGhlaXIgY2hpbGRyZW4uIElmIHRoZSBub2RlIGlzIG5vdCBsb2FkZWQgaXQgd2lsbCBiZSBsb2FkZWQgYW5kIG9wZW5lZCBvbmNlIHJlYWR5LlxyXG5cdFx0ICogQG5hbWUgb3Blbl9hbGwoW29iaiwgYW5pbWF0aW9uLCBvcmlnaW5hbF9vYmpdKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqIHRoZSBub2RlIHRvIG9wZW4gcmVjdXJzaXZlbHksIG9taXQgdG8gb3BlbiBhbGwgbm9kZXMgaW4gdGhlIHRyZWVcclxuXHRcdCAqIEBwYXJhbSB7TnVtYmVyfSBhbmltYXRpb24gdGhlIGFuaW1hdGlvbiBkdXJhdGlvbiBpbiBtaWxsaXNlY29uZHMgd2hlbiBvcGVuaW5nIHRoZSBub2RlcywgdGhlIGRlZmF1bHQgaXMgbm8gYW5pbWF0aW9uXHJcblx0XHQgKiBAcGFyYW0ge2pRdWVyeX0gcmVmZXJlbmNlIHRvIHRoZSBub2RlIHRoYXQgc3RhcnRlZCB0aGUgcHJvY2VzcyAoaW50ZXJuYWwgdXNlKVxyXG5cdFx0ICogQHRyaWdnZXIgb3Blbl9hbGwuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdG9wZW5fYWxsIDogZnVuY3Rpb24gKG9iaiwgYW5pbWF0aW9uLCBvcmlnaW5hbF9vYmopIHtcclxuXHRcdFx0aWYoIW9iaikgeyBvYmogPSAnIyc7IH1cclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHR2YXIgZG9tID0gb2JqLmlkID09PSAnIycgPyB0aGlzLmdldF9jb250YWluZXJfdWwoKSA6IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKSwgaSwgaiwgX3RoaXM7XHJcblx0XHRcdGlmKCFkb20ubGVuZ3RoKSB7XHJcblx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gb2JqLmNoaWxkcmVuX2QubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRpZih0aGlzLmlzX2Nsb3NlZCh0aGlzLl9tb2RlbC5kYXRhW29iai5jaGlsZHJlbl9kW2ldXSkpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5fbW9kZWwuZGF0YVtvYmouY2hpbGRyZW5fZFtpXV0uc3RhdGUub3BlbmVkID0gdHJ1ZTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRoaXMudHJpZ2dlcignb3Blbl9hbGwnLCB7IFwibm9kZVwiIDogb2JqIH0pO1xyXG5cdFx0XHR9XHJcblx0XHRcdG9yaWdpbmFsX29iaiA9IG9yaWdpbmFsX29iaiB8fCBkb207XHJcblx0XHRcdF90aGlzID0gdGhpcztcclxuXHRcdFx0ZG9tID0gdGhpcy5pc19jbG9zZWQob2JqKSA/IGRvbS5maW5kKCdsaS5qc3RyZWUtY2xvc2VkJykuYWRkQmFjaygpIDogZG9tLmZpbmQoJ2xpLmpzdHJlZS1jbG9zZWQnKTtcclxuXHRcdFx0ZG9tLmVhY2goZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdF90aGlzLm9wZW5fbm9kZShcclxuXHRcdFx0XHRcdHRoaXMsXHJcblx0XHRcdFx0XHRmdW5jdGlvbihub2RlLCBzdGF0dXMpIHsgaWYoc3RhdHVzICYmIHRoaXMuaXNfcGFyZW50KG5vZGUpKSB7IHRoaXMub3Blbl9hbGwobm9kZSwgYW5pbWF0aW9uLCBvcmlnaW5hbF9vYmopOyB9IH0sXHJcblx0XHRcdFx0XHRhbmltYXRpb24gfHwgMFxyXG5cdFx0XHRcdCk7XHJcblx0XHRcdH0pO1xyXG5cdFx0XHRpZihvcmlnaW5hbF9vYmouZmluZCgnbGkuanN0cmVlLWNsb3NlZCcpLmxlbmd0aCA9PT0gMCkge1xyXG5cdFx0XHRcdC8qKlxyXG5cdFx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGFuIGBvcGVuX2FsbGAgY2FsbCBjb21wbGV0ZXNcclxuXHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHQgKiBAbmFtZSBvcGVuX2FsbC5qc3RyZWVcclxuXHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZSB0aGUgb3BlbmVkIG5vZGVcclxuXHRcdFx0XHQgKi9cclxuXHRcdFx0XHR0aGlzLnRyaWdnZXIoJ29wZW5fYWxsJywgeyBcIm5vZGVcIiA6IHRoaXMuZ2V0X25vZGUob3JpZ2luYWxfb2JqKSB9KTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY2xvc2VzIGFsbCBub2RlcyB3aXRoaW4gYSBub2RlIChvciB0aGUgdHJlZSksIHJldmFsaW5nIHRoZWlyIGNoaWxkcmVuXHJcblx0XHQgKiBAbmFtZSBjbG9zZV9hbGwoW29iaiwgYW5pbWF0aW9uXSlcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9iaiB0aGUgbm9kZSB0byBjbG9zZSByZWN1cnNpdmVseSwgb21pdCB0byBjbG9zZSBhbGwgbm9kZXMgaW4gdGhlIHRyZWVcclxuXHRcdCAqIEBwYXJhbSB7TnVtYmVyfSBhbmltYXRpb24gdGhlIGFuaW1hdGlvbiBkdXJhdGlvbiBpbiBtaWxsaXNlY29uZHMgd2hlbiBjbG9zaW5nIHRoZSBub2RlcywgdGhlIGRlZmF1bHQgaXMgbm8gYW5pbWF0aW9uXHJcblx0XHQgKiBAdHJpZ2dlciBjbG9zZV9hbGwuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGNsb3NlX2FsbCA6IGZ1bmN0aW9uIChvYmosIGFuaW1hdGlvbikge1xyXG5cdFx0XHRpZighb2JqKSB7IG9iaiA9ICcjJzsgfVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmopIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdHZhciBkb20gPSBvYmouaWQgPT09ICcjJyA/IHRoaXMuZ2V0X2NvbnRhaW5lcl91bCgpIDogdGhpcy5nZXRfbm9kZShvYmosIHRydWUpLFxyXG5cdFx0XHRcdF90aGlzID0gdGhpcywgaSwgajtcclxuXHRcdFx0aWYoIWRvbS5sZW5ndGgpIHtcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSBvYmouY2hpbGRyZW5fZC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuX21vZGVsLmRhdGFbb2JqLmNoaWxkcmVuX2RbaV1dLnN0YXRlLm9wZW5lZCA9IGZhbHNlO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdGhpcy50cmlnZ2VyKCdjbG9zZV9hbGwnLCB7IFwibm9kZVwiIDogb2JqIH0pO1xyXG5cdFx0XHR9XHJcblx0XHRcdGRvbSA9IHRoaXMuaXNfb3BlbihvYmopID8gZG9tLmZpbmQoJ2xpLmpzdHJlZS1vcGVuJykuYWRkQmFjaygpIDogZG9tLmZpbmQoJ2xpLmpzdHJlZS1vcGVuJyk7XHJcblx0XHRcdGRvbS52YWthdGFfcmV2ZXJzZSgpLmVhY2goZnVuY3Rpb24gKCkgeyBfdGhpcy5jbG9zZV9ub2RlKHRoaXMsIGFuaW1hdGlvbiB8fCAwKTsgfSk7XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhbiBgY2xvc2VfYWxsYCBjYWxsIGNvbXBsZXRlc1xyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgY2xvc2VfYWxsLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZSB0aGUgY2xvc2VkIG5vZGVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignY2xvc2VfYWxsJywgeyBcIm5vZGVcIiA6IG9iaiB9KTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGNoZWNrcyBpZiBhIG5vZGUgaXMgZGlzYWJsZWQgKG5vdCBzZWxlY3RhYmxlKVxyXG5cdFx0ICogQG5hbWUgaXNfZGlzYWJsZWQob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHJldHVybiB7Qm9vbGVhbn1cclxuXHRcdCAqL1xyXG5cdFx0aXNfZGlzYWJsZWQgOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0cmV0dXJuIG9iaiAmJiBvYmouc3RhdGUgJiYgb2JqLnN0YXRlLmRpc2FibGVkO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZW5hYmxlcyBhIG5vZGUgLSBzbyB0aGF0IGl0IGNhbiBiZSBzZWxlY3RlZFxyXG5cdFx0ICogQG5hbWUgZW5hYmxlX25vZGUob2JqKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqIHRoZSBub2RlIHRvIGVuYWJsZVxyXG5cdFx0ICogQHRyaWdnZXIgZW5hYmxlX25vZGUuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGVuYWJsZV9ub2RlIDogZnVuY3Rpb24gKG9iaikge1xyXG5cdFx0XHR2YXIgdDEsIHQyO1xyXG5cdFx0XHRpZigkLmlzQXJyYXkob2JqKSkge1xyXG5cdFx0XHRcdG9iaiA9IG9iai5zbGljZSgpO1xyXG5cdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gb2JqLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0dGhpcy5lbmFibGVfbm9kZShvYmpbdDFdKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdG9iai5zdGF0ZS5kaXNhYmxlZCA9IGZhbHNlO1xyXG5cdFx0XHR0aGlzLmdldF9ub2RlKG9iaix0cnVlKS5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5yZW1vdmVDbGFzcygnanN0cmVlLWRpc2FibGVkJyk7XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhbiBub2RlIGlzIGVuYWJsZWRcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIGVuYWJsZV9ub2RlLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZSB0aGUgZW5hYmxlZCBub2RlXHJcblx0XHRcdCAqL1xyXG5cdFx0XHR0aGlzLnRyaWdnZXIoJ2VuYWJsZV9ub2RlJywgeyAnbm9kZScgOiBvYmogfSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBkaXNhYmxlcyBhIG5vZGUgLSBzbyB0aGF0IGl0IGNhbiBub3QgYmUgc2VsZWN0ZWRcclxuXHRcdCAqIEBuYW1lIGRpc2FibGVfbm9kZShvYmopXHJcblx0XHQgKiBAcGFyYW0ge21peGVkfSBvYmogdGhlIG5vZGUgdG8gZGlzYWJsZVxyXG5cdFx0ICogQHRyaWdnZXIgZGlzYWJsZV9ub2RlLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRkaXNhYmxlX25vZGUgOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdHZhciB0MSwgdDI7XHJcblx0XHRcdGlmKCQuaXNBcnJheShvYmopKSB7XHJcblx0XHRcdFx0b2JqID0gb2JqLnNsaWNlKCk7XHJcblx0XHRcdFx0Zm9yKHQxID0gMCwgdDIgPSBvYmoubGVuZ3RoOyB0MSA8IHQyOyB0MSsrKSB7XHJcblx0XHRcdFx0XHR0aGlzLmRpc2FibGVfbm9kZShvYmpbdDFdKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdG9iai5zdGF0ZS5kaXNhYmxlZCA9IHRydWU7XHJcblx0XHRcdHRoaXMuZ2V0X25vZGUob2JqLHRydWUpLmNoaWxkcmVuKCcuanN0cmVlLWFuY2hvcicpLmFkZENsYXNzKCdqc3RyZWUtZGlzYWJsZWQnKTtcclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGFuIG5vZGUgaXMgZGlzYWJsZWRcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIGRpc2FibGVfbm9kZS5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGUgdGhlIGRpc2FibGVkIG5vZGVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignZGlzYWJsZV9ub2RlJywgeyAnbm9kZScgOiBvYmogfSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBjYWxsZWQgd2hlbiBhIG5vZGUgaXMgc2VsZWN0ZWQgYnkgdGhlIHVzZXIuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBhY3RpdmF0ZV9ub2RlKG9iaiwgZSlcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9iaiB0aGUgbm9kZVxyXG5cdFx0ICogQHBhcmFtIHtPYmplY3R9IGUgdGhlIHJlbGF0ZWQgZXZlbnRcclxuXHRcdCAqIEB0cmlnZ2VyIGFjdGl2YXRlX25vZGUuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGFjdGl2YXRlX25vZGUgOiBmdW5jdGlvbiAob2JqLCBlKSB7XHJcblx0XHRcdGlmKHRoaXMuaXNfZGlzYWJsZWQob2JqKSkge1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZighdGhpcy5zZXR0aW5ncy5jb3JlLm11bHRpcGxlIHx8ICghZS5tZXRhS2V5ICYmICFlLmN0cmxLZXkgJiYgIWUuc2hpZnRLZXkpIHx8IChlLnNoaWZ0S2V5ICYmICghdGhpcy5fZGF0YS5jb3JlLmxhc3RfY2xpY2tlZCB8fCAhdGhpcy5nZXRfcGFyZW50KG9iaikgfHwgdGhpcy5nZXRfcGFyZW50KG9iaikgIT09IHRoaXMuX2RhdGEuY29yZS5sYXN0X2NsaWNrZWQucGFyZW50ICkgKSkge1xyXG5cdFx0XHRcdGlmKCF0aGlzLnNldHRpbmdzLmNvcmUubXVsdGlwbGUgJiYgKGUubWV0YUtleSB8fCBlLmN0cmxLZXkgfHwgZS5zaGlmdEtleSkgJiYgdGhpcy5pc19zZWxlY3RlZChvYmopKSB7XHJcblx0XHRcdFx0XHR0aGlzLmRlc2VsZWN0X25vZGUob2JqLCBmYWxzZSwgZmFsc2UsIGUpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdHRoaXMuZGVzZWxlY3RfYWxsKHRydWUpO1xyXG5cdFx0XHRcdFx0dGhpcy5zZWxlY3Rfbm9kZShvYmosIGZhbHNlLCBmYWxzZSwgZSk7XHJcblx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUubGFzdF9jbGlja2VkID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRpZihlLnNoaWZ0S2V5KSB7XHJcblx0XHRcdFx0XHR2YXIgbyA9IHRoaXMuZ2V0X25vZGUob2JqKS5pZCxcclxuXHRcdFx0XHRcdFx0bCA9IHRoaXMuX2RhdGEuY29yZS5sYXN0X2NsaWNrZWQuaWQsXHJcblx0XHRcdFx0XHRcdHAgPSB0aGlzLmdldF9ub2RlKHRoaXMuX2RhdGEuY29yZS5sYXN0X2NsaWNrZWQucGFyZW50KS5jaGlsZHJlbixcclxuXHRcdFx0XHRcdFx0YyA9IGZhbHNlLFxyXG5cdFx0XHRcdFx0XHRpLCBqO1xyXG5cdFx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gcC5sZW5ndGg7IGkgPCBqOyBpICs9IDEpIHtcclxuXHRcdFx0XHRcdFx0Ly8gc2VwYXJhdGUgSUZzIHdvcmsgd2hlbSBvIGFuZCBsIGFyZSB0aGUgc2FtZVxyXG5cdFx0XHRcdFx0XHRpZihwW2ldID09PSBvKSB7XHJcblx0XHRcdFx0XHRcdFx0YyA9ICFjO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdGlmKHBbaV0gPT09IGwpIHtcclxuXHRcdFx0XHRcdFx0XHRjID0gIWM7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0aWYoYyB8fCBwW2ldID09PSBvIHx8IHBbaV0gPT09IGwpIHtcclxuXHRcdFx0XHRcdFx0XHR0aGlzLnNlbGVjdF9ub2RlKHBbaV0sIGZhbHNlLCBmYWxzZSwgZSk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5kZXNlbGVjdF9ub2RlKHBbaV0sIGZhbHNlLCBmYWxzZSwgZSk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRpZighdGhpcy5pc19zZWxlY3RlZChvYmopKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuc2VsZWN0X25vZGUob2JqLCBmYWxzZSwgZmFsc2UsIGUpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuZGVzZWxlY3Rfbm9kZShvYmosIGZhbHNlLCBmYWxzZSwgZSk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhbiBub2RlIGlzIGNsaWNrZWQgb3IgaW50ZXJjYXRlZCB3aXRoIGJ5IHRoZSB1c2VyXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSBhY3RpdmF0ZV9ub2RlLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdhY3RpdmF0ZV9ub2RlJywgeyAnbm9kZScgOiB0aGlzLmdldF9ub2RlKG9iaikgfSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBhcHBsaWVzIHRoZSBob3ZlciBzdGF0ZSBvbiBhIG5vZGUsIGNhbGxlZCB3aGVuIGEgbm9kZSBpcyBob3ZlcmVkIGJ5IHRoZSB1c2VyLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgaG92ZXJfbm9kZShvYmopXHJcblx0XHQgKiBAcGFyYW0ge21peGVkfSBvYmpcclxuXHRcdCAqIEB0cmlnZ2VyIGhvdmVyX25vZGUuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGhvdmVyX25vZGUgOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKTtcclxuXHRcdFx0aWYoIW9iaiB8fCAhb2JqLmxlbmd0aCB8fCBvYmouY2hpbGRyZW4oJy5qc3RyZWUtaG92ZXJlZCcpLmxlbmd0aCkge1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHR2YXIgbyA9IHRoaXMuZWxlbWVudC5maW5kKCcuanN0cmVlLWhvdmVyZWQnKSwgdCA9IHRoaXMuZWxlbWVudDtcclxuXHRcdFx0aWYobyAmJiBvLmxlbmd0aCkgeyB0aGlzLmRlaG92ZXJfbm9kZShvKTsgfVxyXG5cclxuXHRcdFx0b2JqLmNoaWxkcmVuKCcuanN0cmVlLWFuY2hvcicpLmFkZENsYXNzKCdqc3RyZWUtaG92ZXJlZCcpO1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gYW4gbm9kZSBpcyBob3ZlcmVkXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSBob3Zlcl9ub2RlLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdob3Zlcl9ub2RlJywgeyAnbm9kZScgOiB0aGlzLmdldF9ub2RlKG9iaikgfSk7XHJcblx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24gKCkgeyB0LmF0dHIoJ2FyaWEtYWN0aXZlZGVzY2VuZGFudCcsIG9ialswXS5pZCk7IG9iai5hdHRyKCdhcmlhLXNlbGVjdGVkJywgdHJ1ZSk7IH0sIDApO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogcmVtb3ZlcyB0aGUgaG92ZXIgc3RhdGUgZnJvbSBhIG5vZGVjYWxsZWQgd2hlbiBhIG5vZGUgaXMgbm8gbG9uZ2VyIGhvdmVyZWQgYnkgdGhlIHVzZXIuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBkZWhvdmVyX25vZGUob2JqKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqXHJcblx0XHQgKiBAdHJpZ2dlciBkZWhvdmVyX25vZGUuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGRlaG92ZXJfbm9kZSA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpO1xyXG5cdFx0XHRpZighb2JqIHx8ICFvYmoubGVuZ3RoIHx8ICFvYmouY2hpbGRyZW4oJy5qc3RyZWUtaG92ZXJlZCcpLmxlbmd0aCkge1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmouYXR0cignYXJpYS1zZWxlY3RlZCcsIGZhbHNlKS5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5yZW1vdmVDbGFzcygnanN0cmVlLWhvdmVyZWQnKTtcclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGFuIG5vZGUgaXMgbm8gbG9uZ2VyIGhvdmVyZWRcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIGRlaG92ZXJfbm9kZS5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignZGVob3Zlcl9ub2RlJywgeyAnbm9kZScgOiB0aGlzLmdldF9ub2RlKG9iaikgfSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBzZWxlY3QgYSBub2RlXHJcblx0XHQgKiBAbmFtZSBzZWxlY3Rfbm9kZShvYmogWywgc3VwcmVzc19ldmVudCwgcHJldmVudF9vcGVuXSlcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9iaiBhbiBhcnJheSBjYW4gYmUgdXNlZCB0byBzZWxlY3QgbXVsdGlwbGUgbm9kZXNcclxuXHRcdCAqIEBwYXJhbSB7Qm9vbGVhbn0gc3VwcmVzc19ldmVudCBpZiBzZXQgdG8gYHRydWVgIHRoZSBgY2hhbmdlZC5qc3RyZWVgIGV2ZW50IHdvbid0IGJlIHRyaWdnZXJlZFxyXG5cdFx0ICogQHBhcmFtIHtCb29sZWFufSBwcmV2ZW50X29wZW4gaWYgc2V0IHRvIGB0cnVlYCBwYXJlbnRzIG9mIHRoZSBzZWxlY3RlZCBub2RlIHdvbid0IGJlIG9wZW5lZFxyXG5cdFx0ICogQHRyaWdnZXIgc2VsZWN0X25vZGUuanN0cmVlLCBjaGFuZ2VkLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRzZWxlY3Rfbm9kZSA6IGZ1bmN0aW9uIChvYmosIHN1cHJlc3NfZXZlbnQsIHByZXZlbnRfb3BlbiwgZSkge1xyXG5cdFx0XHR2YXIgZG9tLCB0MSwgdDIsIHRoO1xyXG5cdFx0XHRpZigkLmlzQXJyYXkob2JqKSkge1xyXG5cdFx0XHRcdG9iaiA9IG9iai5zbGljZSgpO1xyXG5cdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gb2JqLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0dGhpcy5zZWxlY3Rfbm9kZShvYmpbdDFdLCBzdXByZXNzX2V2ZW50LCBwcmV2ZW50X29wZW4sIGUpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqLmlkID09PSAnIycpIHtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0ZG9tID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpO1xyXG5cdFx0XHRpZighb2JqLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0b2JqLnN0YXRlLnNlbGVjdGVkID0gdHJ1ZTtcclxuXHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQucHVzaChvYmouaWQpO1xyXG5cdFx0XHRcdGlmKCFwcmV2ZW50X29wZW4pIHtcclxuXHRcdFx0XHRcdGRvbSA9IHRoaXMuX29wZW5fdG8ob2JqKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYoZG9tICYmIGRvbS5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdGRvbS5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5hZGRDbGFzcygnanN0cmVlLWNsaWNrZWQnKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0LyoqXHJcblx0XHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gYW4gbm9kZSBpcyBzZWxlY3RlZFxyXG5cdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdCAqIEBuYW1lIHNlbGVjdF9ub2RlLmpzdHJlZVxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBub2RlXHJcblx0XHRcdFx0ICogQHBhcmFtIHtBcnJheX0gc2VsZWN0ZWQgdGhlIGN1cnJlbnQgc2VsZWN0aW9uXHJcblx0XHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IGV2ZW50IHRoZSBldmVudCAoaWYgYW55KSB0aGF0IHRyaWdnZXJlZCB0aGlzIHNlbGVjdF9ub2RlXHJcblx0XHRcdFx0ICovXHJcblx0XHRcdFx0dGhpcy50cmlnZ2VyKCdzZWxlY3Rfbm9kZScsIHsgJ25vZGUnIDogb2JqLCAnc2VsZWN0ZWQnIDogdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLCAnZXZlbnQnIDogZSB9KTtcclxuXHRcdFx0XHRpZighc3VwcmVzc19ldmVudCkge1xyXG5cdFx0XHRcdFx0LyoqXHJcblx0XHRcdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBzZWxlY3Rpb24gY2hhbmdlc1xyXG5cdFx0XHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdFx0XHQgKiBAbmFtZSBjaGFuZ2VkLmpzdHJlZVxyXG5cdFx0XHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGVcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBhY3Rpb24gdGhlIGFjdGlvbiB0aGF0IGNhdXNlZCB0aGUgc2VsZWN0aW9uIHRvIGNoYW5nZVxyXG5cdFx0XHRcdFx0ICogQHBhcmFtIHtBcnJheX0gc2VsZWN0ZWQgdGhlIGN1cnJlbnQgc2VsZWN0aW9uXHJcblx0XHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gZXZlbnQgdGhlIGV2ZW50IChpZiBhbnkpIHRoYXQgdHJpZ2dlcmVkIHRoaXMgY2hhbmdlZCBldmVudFxyXG5cdFx0XHRcdFx0ICovXHJcblx0XHRcdFx0XHR0aGlzLnRyaWdnZXIoJ2NoYW5nZWQnLCB7ICdhY3Rpb24nIDogJ3NlbGVjdF9ub2RlJywgJ25vZGUnIDogb2JqLCAnc2VsZWN0ZWQnIDogdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLCAnZXZlbnQnIDogZSB9KTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGRlc2VsZWN0IGEgbm9kZVxyXG5cdFx0ICogQG5hbWUgZGVzZWxlY3Rfbm9kZShvYmogWywgc3VwcmVzc19ldmVudF0pXHJcblx0XHQgKiBAcGFyYW0ge21peGVkfSBvYmogYW4gYXJyYXkgY2FuIGJlIHVzZWQgdG8gZGVzZWxlY3QgbXVsdGlwbGUgbm9kZXNcclxuXHRcdCAqIEBwYXJhbSB7Qm9vbGVhbn0gc3VwcmVzc19ldmVudCBpZiBzZXQgdG8gYHRydWVgIHRoZSBgY2hhbmdlZC5qc3RyZWVgIGV2ZW50IHdvbid0IGJlIHRyaWdnZXJlZFxyXG5cdFx0ICogQHRyaWdnZXIgZGVzZWxlY3Rfbm9kZS5qc3RyZWUsIGNoYW5nZWQuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGRlc2VsZWN0X25vZGUgOiBmdW5jdGlvbiAob2JqLCBzdXByZXNzX2V2ZW50LCBlKSB7XHJcblx0XHRcdHZhciB0MSwgdDIsIGRvbTtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuZGVzZWxlY3Rfbm9kZShvYmpbdDFdLCBzdXByZXNzX2V2ZW50LCBlKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGRvbSA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKTtcclxuXHRcdFx0aWYob2JqLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0b2JqLnN0YXRlLnNlbGVjdGVkID0gZmFsc2U7XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkID0gJC52YWthdGEuYXJyYXlfcmVtb3ZlX2l0ZW0odGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLCBvYmouaWQpO1xyXG5cdFx0XHRcdGlmKGRvbS5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdGRvbS5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5yZW1vdmVDbGFzcygnanN0cmVlLWNsaWNrZWQnKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0LyoqXHJcblx0XHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gYW4gbm9kZSBpcyBkZXNlbGVjdGVkXHJcblx0XHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdFx0ICogQG5hbWUgZGVzZWxlY3Rfbm9kZS5qc3RyZWVcclxuXHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZVxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7QXJyYXl9IHNlbGVjdGVkIHRoZSBjdXJyZW50IHNlbGVjdGlvblxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBldmVudCB0aGUgZXZlbnQgKGlmIGFueSkgdGhhdCB0cmlnZ2VyZWQgdGhpcyBkZXNlbGVjdF9ub2RlXHJcblx0XHRcdFx0ICovXHJcblx0XHRcdFx0dGhpcy50cmlnZ2VyKCdkZXNlbGVjdF9ub2RlJywgeyAnbm9kZScgOiBvYmosICdzZWxlY3RlZCcgOiB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQsICdldmVudCcgOiBlIH0pO1xyXG5cdFx0XHRcdGlmKCFzdXByZXNzX2V2ZW50KSB7XHJcblx0XHRcdFx0XHR0aGlzLnRyaWdnZXIoJ2NoYW5nZWQnLCB7ICdhY3Rpb24nIDogJ2Rlc2VsZWN0X25vZGUnLCAnbm9kZScgOiBvYmosICdzZWxlY3RlZCcgOiB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQsICdldmVudCcgOiBlIH0pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogc2VsZWN0IGFsbCBub2RlcyBpbiB0aGUgdHJlZVxyXG5cdFx0ICogQG5hbWUgc2VsZWN0X2FsbChbc3VwcmVzc19ldmVudF0pXHJcblx0XHQgKiBAcGFyYW0ge0Jvb2xlYW59IHN1cHJlc3NfZXZlbnQgaWYgc2V0IHRvIGB0cnVlYCB0aGUgYGNoYW5nZWQuanN0cmVlYCBldmVudCB3b24ndCBiZSB0cmlnZ2VyZWRcclxuXHRcdCAqIEB0cmlnZ2VyIHNlbGVjdF9hbGwuanN0cmVlLCBjaGFuZ2VkLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRzZWxlY3RfYWxsIDogZnVuY3Rpb24gKHN1cHJlc3NfZXZlbnQpIHtcclxuXHRcdFx0dmFyIHRtcCA9IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZC5jb25jYXQoW10pLCBpLCBqO1xyXG5cdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQgPSB0aGlzLl9tb2RlbC5kYXRhWycjJ10uY2hpbGRyZW5fZC5jb25jYXQoKTtcclxuXHRcdFx0Zm9yKGkgPSAwLCBqID0gdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdGlmKHRoaXMuX21vZGVsLmRhdGFbdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkW2ldXSkge1xyXG5cdFx0XHRcdFx0dGhpcy5fbW9kZWwuZGF0YVt0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWRbaV1dLnN0YXRlLnNlbGVjdGVkID0gdHJ1ZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5yZWRyYXcodHJ1ZSk7XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhbGwgbm9kZXMgYXJlIHNlbGVjdGVkXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSBzZWxlY3RfYWxsLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge0FycmF5fSBzZWxlY3RlZCB0aGUgY3VycmVudCBzZWxlY3Rpb25cclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignc2VsZWN0X2FsbCcsIHsgJ3NlbGVjdGVkJyA6IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZCB9KTtcclxuXHRcdFx0aWYoIXN1cHJlc3NfZXZlbnQpIHtcclxuXHRcdFx0XHR0aGlzLnRyaWdnZXIoJ2NoYW5nZWQnLCB7ICdhY3Rpb24nIDogJ3NlbGVjdF9hbGwnLCAnc2VsZWN0ZWQnIDogdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLCAnb2xkX3NlbGVjdGlvbicgOiB0bXAgfSk7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGRlc2VsZWN0IGFsbCBzZWxlY3RlZCBub2Rlc1xyXG5cdFx0ICogQG5hbWUgZGVzZWxlY3RfYWxsKFtzdXByZXNzX2V2ZW50XSlcclxuXHRcdCAqIEBwYXJhbSB7Qm9vbGVhbn0gc3VwcmVzc19ldmVudCBpZiBzZXQgdG8gYHRydWVgIHRoZSBgY2hhbmdlZC5qc3RyZWVgIGV2ZW50IHdvbid0IGJlIHRyaWdnZXJlZFxyXG5cdFx0ICogQHRyaWdnZXIgZGVzZWxlY3RfYWxsLmpzdHJlZSwgY2hhbmdlZC5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0ZGVzZWxlY3RfYWxsIDogZnVuY3Rpb24gKHN1cHJlc3NfZXZlbnQpIHtcclxuXHRcdFx0dmFyIHRtcCA9IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZC5jb25jYXQoW10pLCBpLCBqO1xyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0aWYodGhpcy5fbW9kZWwuZGF0YVt0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWRbaV1dKSB7XHJcblx0XHRcdFx0XHR0aGlzLl9tb2RlbC5kYXRhW3RoaXMuX2RhdGEuY29yZS5zZWxlY3RlZFtpXV0uc3RhdGUuc2VsZWN0ZWQgPSBmYWxzZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkID0gW107XHJcblx0XHRcdHRoaXMuZWxlbWVudC5maW5kKCcuanN0cmVlLWNsaWNrZWQnKS5yZW1vdmVDbGFzcygnanN0cmVlLWNsaWNrZWQnKTtcclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGFsbCBub2RlcyBhcmUgZGVzZWxlY3RlZFxyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgZGVzZWxlY3RfYWxsLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZSB0aGUgcHJldmlvdXMgc2VsZWN0aW9uXHJcblx0XHRcdCAqIEBwYXJhbSB7QXJyYXl9IHNlbGVjdGVkIHRoZSBjdXJyZW50IHNlbGVjdGlvblxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdkZXNlbGVjdF9hbGwnLCB7ICdzZWxlY3RlZCcgOiB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQsICdub2RlJyA6IHRtcCB9KTtcclxuXHRcdFx0aWYoIXN1cHJlc3NfZXZlbnQpIHtcclxuXHRcdFx0XHR0aGlzLnRyaWdnZXIoJ2NoYW5nZWQnLCB7ICdhY3Rpb24nIDogJ2Rlc2VsZWN0X2FsbCcsICdzZWxlY3RlZCcgOiB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQsICdvbGRfc2VsZWN0aW9uJyA6IHRtcCB9KTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY2hlY2tzIGlmIGEgbm9kZSBpcyBzZWxlY3RlZFxyXG5cdFx0ICogQG5hbWUgaXNfc2VsZWN0ZWQob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9ICBvYmpcclxuXHRcdCAqIEByZXR1cm4ge0Jvb2xlYW59XHJcblx0XHQgKi9cclxuXHRcdGlzX3NlbGVjdGVkIDogZnVuY3Rpb24gKG9iaikge1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqLmlkID09PSAnIycpIHtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIG9iai5zdGF0ZS5zZWxlY3RlZDtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGdldCBhbiBhcnJheSBvZiBhbGwgc2VsZWN0ZWQgbm9kZSBJRHNcclxuXHRcdCAqIEBuYW1lIGdldF9zZWxlY3RlZChbZnVsbF0pXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gIGZ1bGwgaWYgc2V0IHRvIGB0cnVlYCB0aGUgcmV0dXJuZWQgYXJyYXkgd2lsbCBjb25zaXN0IG9mIHRoZSBmdWxsIG5vZGUgb2JqZWN0cywgb3RoZXJ3aXNlIC0gb25seSBJRHMgd2lsbCBiZSByZXR1cm5lZFxyXG5cdFx0ICogQHJldHVybiB7QXJyYXl9XHJcblx0XHQgKi9cclxuXHRcdGdldF9zZWxlY3RlZCA6IGZ1bmN0aW9uIChmdWxsKSB7XHJcblx0XHRcdHJldHVybiBmdWxsID8gJC5tYXAodGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLCAkLnByb3h5KGZ1bmN0aW9uIChpKSB7IHJldHVybiB0aGlzLmdldF9ub2RlKGkpOyB9LCB0aGlzKSkgOiB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQ7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBnZXRzIHRoZSBjdXJyZW50IHN0YXRlIG9mIHRoZSB0cmVlIHNvIHRoYXQgaXQgY2FuIGJlIHJlc3RvcmVkIGxhdGVyIHdpdGggYHNldF9zdGF0ZShzdGF0ZSlgLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAbmFtZSBnZXRfc3RhdGUoKVxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEByZXR1cm4ge09iamVjdH1cclxuXHRcdCAqL1xyXG5cdFx0Z2V0X3N0YXRlIDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHR2YXIgc3RhdGVcdD0ge1xyXG5cdFx0XHRcdCdjb3JlJyA6IHtcclxuXHRcdFx0XHRcdCdvcGVuJyA6IFtdLFxyXG5cdFx0XHRcdFx0J3Njcm9sbCcgOiB7XHJcblx0XHRcdFx0XHRcdCdsZWZ0JyA6IHRoaXMuZWxlbWVudC5zY3JvbGxMZWZ0KCksXHJcblx0XHRcdFx0XHRcdCd0b3AnIDogdGhpcy5lbGVtZW50LnNjcm9sbFRvcCgpXHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0LyohXHJcblx0XHRcdFx0XHQndGhlbWVzJyA6IHtcclxuXHRcdFx0XHRcdFx0J25hbWUnIDogdGhpcy5nZXRfdGhlbWUoKSxcclxuXHRcdFx0XHRcdFx0J2ljb25zJyA6IHRoaXMuX2RhdGEuY29yZS50aGVtZXMuaWNvbnMsXHJcblx0XHRcdFx0XHRcdCdkb3RzJyA6IHRoaXMuX2RhdGEuY29yZS50aGVtZXMuZG90c1xyXG5cdFx0XHRcdFx0fSxcclxuXHRcdFx0XHRcdCovXHJcblx0XHRcdFx0XHQnc2VsZWN0ZWQnIDogW11cclxuXHRcdFx0XHR9XHJcblx0XHRcdH0sIGk7XHJcblx0XHRcdGZvcihpIGluIHRoaXMuX21vZGVsLmRhdGEpIHtcclxuXHRcdFx0XHRpZih0aGlzLl9tb2RlbC5kYXRhLmhhc093blByb3BlcnR5KGkpKSB7XHJcblx0XHRcdFx0XHRpZihpICE9PSAnIycpIHtcclxuXHRcdFx0XHRcdFx0aWYodGhpcy5fbW9kZWwuZGF0YVtpXS5zdGF0ZS5vcGVuZWQpIHtcclxuXHRcdFx0XHRcdFx0XHRzdGF0ZS5jb3JlLm9wZW4ucHVzaChpKTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRpZih0aGlzLl9tb2RlbC5kYXRhW2ldLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0XHRcdFx0c3RhdGUuY29yZS5zZWxlY3RlZC5wdXNoKGkpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiBzdGF0ZTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHNldHMgdGhlIHN0YXRlIG9mIHRoZSB0cmVlLiBVc2VkIGludGVybmFsbHkuXHJcblx0XHQgKiBAbmFtZSBzZXRfc3RhdGUoc3RhdGUgWywgY2FsbGJhY2tdKVxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBzdGF0ZSB0aGUgc3RhdGUgdG8gcmVzdG9yZVxyXG5cdFx0ICogQHBhcmFtIHtGdW5jdGlvbn0gY2FsbGJhY2sgYW4gb3B0aW9uYWwgZnVuY3Rpb24gdG8gZXhlY3V0ZSBvbmNlIHRoZSBzdGF0ZSBpcyByZXN0b3JlZC5cclxuXHRcdCAqIEB0cmlnZ2VyIHNldF9zdGF0ZS5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0c2V0X3N0YXRlIDogZnVuY3Rpb24gKHN0YXRlLCBjYWxsYmFjaykge1xyXG5cdFx0XHRpZihzdGF0ZSkge1xyXG5cdFx0XHRcdGlmKHN0YXRlLmNvcmUpIHtcclxuXHRcdFx0XHRcdHZhciByZXMsIG4sIHQsIF90aGlzO1xyXG5cdFx0XHRcdFx0aWYoc3RhdGUuY29yZS5vcGVuKSB7XHJcblx0XHRcdFx0XHRcdGlmKCEkLmlzQXJyYXkoc3RhdGUuY29yZS5vcGVuKSkge1xyXG5cdFx0XHRcdFx0XHRcdGRlbGV0ZSBzdGF0ZS5jb3JlLm9wZW47XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5zZXRfc3RhdGUoc3RhdGUsIGNhbGxiYWNrKTtcclxuXHRcdFx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0cmVzID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0biA9IGZhbHNlO1xyXG5cdFx0XHRcdFx0XHR0ID0gdGhpcztcclxuXHRcdFx0XHRcdFx0JC5lYWNoKHN0YXRlLmNvcmUub3Blbi5jb25jYXQoW10pLCBmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdFx0XHRcdG4gPSB0LmdldF9ub2RlKHYpO1xyXG5cdFx0XHRcdFx0XHRcdGlmKG4pIHtcclxuXHRcdFx0XHRcdFx0XHRcdGlmKHQuaXNfbG9hZGVkKHYpKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGlmKHQuaXNfY2xvc2VkKHYpKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0dC5vcGVuX25vZGUodiwgZmFsc2UsIDApO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdGlmKHN0YXRlICYmIHN0YXRlLmNvcmUgJiYgc3RhdGUuY29yZS5vcGVuKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0JC52YWthdGEuYXJyYXlfcmVtb3ZlX2l0ZW0oc3RhdGUuY29yZS5vcGVuLCB2KTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGlmKCF0LmlzX2xvYWRpbmcodikpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0Lm9wZW5fbm9kZSh2LCAkLnByb3h5KGZ1bmN0aW9uICgpIHsgdGhpcy5zZXRfc3RhdGUoc3RhdGUsIGNhbGxiYWNrKTsgfSwgdCksIDApO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdC8vIHRoZXJlIHdpbGwgYmUgc29tZSBhc3luYyBhY3Rpdml0eSAtIHNvIHdhaXQgZm9yIGl0XHJcblx0XHRcdFx0XHRcdFx0XHRcdHJlcyA9IGZhbHNlO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0fSk7XHJcblx0XHRcdFx0XHRcdGlmKHJlcykge1xyXG5cdFx0XHRcdFx0XHRcdGRlbGV0ZSBzdGF0ZS5jb3JlLm9wZW47XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5zZXRfc3RhdGUoc3RhdGUsIGNhbGxiYWNrKTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRpZihzdGF0ZS5jb3JlLnNjcm9sbCkge1xyXG5cdFx0XHRcdFx0XHRpZihzdGF0ZS5jb3JlLnNjcm9sbCAmJiBzdGF0ZS5jb3JlLnNjcm9sbC5sZWZ0ICE9PSB1bmRlZmluZWQpIHtcclxuXHRcdFx0XHRcdFx0XHR0aGlzLmVsZW1lbnQuc2Nyb2xsTGVmdChzdGF0ZS5jb3JlLnNjcm9sbC5sZWZ0KTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRpZihzdGF0ZS5jb3JlLnNjcm9sbCAmJiBzdGF0ZS5jb3JlLnNjcm9sbC50b3AgIT09IHVuZGVmaW5lZCkge1xyXG5cdFx0XHRcdFx0XHRcdHRoaXMuZWxlbWVudC5zY3JvbGxUb3Aoc3RhdGUuY29yZS5zY3JvbGwudG9wKTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRkZWxldGUgc3RhdGUuY29yZS5zY3JvbGw7XHJcblx0XHRcdFx0XHRcdHRoaXMuc2V0X3N0YXRlKHN0YXRlLCBjYWxsYmFjayk7XHJcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdC8qIVxyXG5cdFx0XHRcdFx0aWYoc3RhdGUuY29yZS50aGVtZXMpIHtcclxuXHRcdFx0XHRcdFx0aWYoc3RhdGUuY29yZS50aGVtZXMubmFtZSkge1xyXG5cdFx0XHRcdFx0XHRcdHRoaXMuc2V0X3RoZW1lKHN0YXRlLmNvcmUudGhlbWVzLm5hbWUpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdGlmKHR5cGVvZiBzdGF0ZS5jb3JlLnRoZW1lcy5kb3RzICE9PSAndW5kZWZpbmVkJykge1xyXG5cdFx0XHRcdFx0XHRcdHRoaXNbIHN0YXRlLmNvcmUudGhlbWVzLmRvdHMgPyBcInNob3dfZG90c1wiIDogXCJoaWRlX2RvdHNcIiBdKCk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0aWYodHlwZW9mIHN0YXRlLmNvcmUudGhlbWVzLmljb25zICE9PSAndW5kZWZpbmVkJykge1xyXG5cdFx0XHRcdFx0XHRcdHRoaXNbIHN0YXRlLmNvcmUudGhlbWVzLmljb25zID8gXCJzaG93X2ljb25zXCIgOiBcImhpZGVfaWNvbnNcIiBdKCk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0ZGVsZXRlIHN0YXRlLmNvcmUudGhlbWVzO1xyXG5cdFx0XHRcdFx0XHRkZWxldGUgc3RhdGUuY29yZS5vcGVuO1xyXG5cdFx0XHRcdFx0XHR0aGlzLnNldF9zdGF0ZShzdGF0ZSwgY2FsbGJhY2spO1xyXG5cdFx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHQqL1xyXG5cdFx0XHRcdFx0aWYoc3RhdGUuY29yZS5zZWxlY3RlZCkge1xyXG5cdFx0XHRcdFx0XHRfdGhpcyA9IHRoaXM7XHJcblx0XHRcdFx0XHRcdHRoaXMuZGVzZWxlY3RfYWxsKCk7XHJcblx0XHRcdFx0XHRcdCQuZWFjaChzdGF0ZS5jb3JlLnNlbGVjdGVkLCBmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdFx0XHRcdF90aGlzLnNlbGVjdF9ub2RlKHYpO1xyXG5cdFx0XHRcdFx0XHR9KTtcclxuXHRcdFx0XHRcdFx0ZGVsZXRlIHN0YXRlLmNvcmUuc2VsZWN0ZWQ7XHJcblx0XHRcdFx0XHRcdHRoaXMuc2V0X3N0YXRlKHN0YXRlLCBjYWxsYmFjayk7XHJcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGlmKCQuaXNFbXB0eU9iamVjdChzdGF0ZS5jb3JlKSkge1xyXG5cdFx0XHRcdFx0XHRkZWxldGUgc3RhdGUuY29yZTtcclxuXHRcdFx0XHRcdFx0dGhpcy5zZXRfc3RhdGUoc3RhdGUsIGNhbGxiYWNrKTtcclxuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRpZigkLmlzRW1wdHlPYmplY3Qoc3RhdGUpKSB7XHJcblx0XHRcdFx0XHRzdGF0ZSA9IG51bGw7XHJcblx0XHRcdFx0XHRpZihjYWxsYmFjaykgeyBjYWxsYmFjay5jYWxsKHRoaXMpOyB9XHJcblx0XHRcdFx0XHQvKipcclxuXHRcdFx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGEgYHNldF9zdGF0ZWAgY2FsbCBjb21wbGV0ZXNcclxuXHRcdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdFx0ICogQG5hbWUgc2V0X3N0YXRlLmpzdHJlZVxyXG5cdFx0XHRcdFx0ICovXHJcblx0XHRcdFx0XHR0aGlzLnRyaWdnZXIoJ3NldF9zdGF0ZScpO1xyXG5cdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiByZWZyZXNoZXMgdGhlIHRyZWUgLSBhbGwgbm9kZXMgYXJlIHJlbG9hZGVkIHdpdGggY2FsbHMgdG8gYGxvYWRfbm9kZWAuXHJcblx0XHQgKiBAbmFtZSByZWZyZXNoKClcclxuXHRcdCAqIEBwYXJhbSB7Qm9vbGVhbn0gc2tpcF9sb2FkaW5nIGFuIG9wdGlvbiB0byBza2lwIHNob3dpbmcgdGhlIGxvYWRpbmcgaW5kaWNhdG9yXHJcblx0XHQgKiBAdHJpZ2dlciByZWZyZXNoLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRyZWZyZXNoIDogZnVuY3Rpb24gKHNraXBfbG9hZGluZykge1xyXG5cdFx0XHR0aGlzLl9kYXRhLmNvcmUuc3RhdGUgPSB0aGlzLmdldF9zdGF0ZSgpO1xyXG5cdFx0XHR0aGlzLl9jbnQgPSAwO1xyXG5cdFx0XHR0aGlzLl9tb2RlbC5kYXRhID0ge1xyXG5cdFx0XHRcdCcjJyA6IHtcclxuXHRcdFx0XHRcdGlkIDogJyMnLFxyXG5cdFx0XHRcdFx0cGFyZW50IDogbnVsbCxcclxuXHRcdFx0XHRcdHBhcmVudHMgOiBbXSxcclxuXHRcdFx0XHRcdGNoaWxkcmVuIDogW10sXHJcblx0XHRcdFx0XHRjaGlsZHJlbl9kIDogW10sXHJcblx0XHRcdFx0XHRzdGF0ZSA6IHsgbG9hZGVkIDogZmFsc2UgfVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fTtcclxuXHRcdFx0dmFyIGMgPSB0aGlzLmdldF9jb250YWluZXJfdWwoKVswXS5jbGFzc05hbWU7XHJcblx0XHRcdGlmKCFza2lwX2xvYWRpbmcpIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnQuaHRtbChcIjxcIitcInVsIGNsYXNzPSdqc3RyZWUtY29udGFpbmVyLXVsJz48XCIrXCJsaSBjbGFzcz0nanN0cmVlLWluaXRpYWwtbm9kZSBqc3RyZWUtbG9hZGluZyBqc3RyZWUtbGVhZiBqc3RyZWUtbGFzdCc+PGkgY2xhc3M9J2pzdHJlZS1pY29uIGpzdHJlZS1vY2wnPjwvaT48XCIrXCJhIGNsYXNzPSdqc3RyZWUtYW5jaG9yJyBocmVmPScjJz48aSBjbGFzcz0nanN0cmVlLWljb24ganN0cmVlLXRoZW1laWNvbi1oaWRkZW4nPjwvaT5cIiArIHRoaXMuZ2V0X3N0cmluZyhcIkxvYWRpbmcgLi4uXCIpICsgXCI8L2E+PC9saT48L3VsPlwiKTtcclxuXHRcdFx0fVxyXG5cdFx0XHR0aGlzLmxvYWRfbm9kZSgnIycsIGZ1bmN0aW9uIChvLCBzKSB7XHJcblx0XHRcdFx0aWYocykge1xyXG5cdFx0XHRcdFx0dGhpcy5nZXRfY29udGFpbmVyX3VsKClbMF0uY2xhc3NOYW1lID0gYztcclxuXHRcdFx0XHRcdHRoaXMuc2V0X3N0YXRlKCQuZXh0ZW5kKHRydWUsIHt9LCB0aGlzLl9kYXRhLmNvcmUuc3RhdGUpLCBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHRcdC8qKlxyXG5cdFx0XHRcdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhIGByZWZyZXNoYCBjYWxsIGNvbXBsZXRlc1xyXG5cdFx0XHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHRcdFx0ICogQG5hbWUgcmVmcmVzaC5qc3RyZWVcclxuXHRcdFx0XHRcdFx0ICovXHJcblx0XHRcdFx0XHRcdHRoaXMudHJpZ2dlcigncmVmcmVzaCcpO1xyXG5cdFx0XHRcdFx0fSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5zdGF0ZSA9IG51bGw7XHJcblx0XHRcdH0pO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogc2V0IChjaGFuZ2UpIHRoZSBJRCBvZiBhIG5vZGVcclxuXHRcdCAqIEBuYW1lIHNldF9pZChvYmosIGlkKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9iaiB0aGUgbm9kZVxyXG5cdFx0ICogQHBhcmFtICB7U3RyaW5nfSBpZCB0aGUgbmV3IElEXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICovXHJcblx0XHRzZXRfaWQgOiBmdW5jdGlvbiAob2JqLCBpZCkge1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqLmlkID09PSAnIycpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdHZhciBpLCBqLCBtID0gdGhpcy5fbW9kZWwuZGF0YTtcclxuXHRcdFx0Ly8gdXBkYXRlIHBhcmVudHMgKHJlcGxhY2UgY3VycmVudCBJRCB3aXRoIG5ldyBvbmUgaW4gY2hpbGRyZW4gYW5kIGNoaWxkcmVuX2QpXHJcblx0XHRcdG1bb2JqLnBhcmVudF0uY2hpbGRyZW5bJC5pbkFycmF5KG9iai5pZCwgbVtvYmoucGFyZW50XS5jaGlsZHJlbildID0gaWQ7XHJcblx0XHRcdGZvcihpID0gMCwgaiA9IG9iai5wYXJlbnRzLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdG1bb2JqLnBhcmVudHNbaV1dLmNoaWxkcmVuX2RbJC5pbkFycmF5KG9iai5pZCwgbVtvYmoucGFyZW50c1tpXV0uY2hpbGRyZW5fZCldID0gaWQ7XHJcblx0XHRcdH1cclxuXHRcdFx0Ly8gdXBkYXRlIGNoaWxkcmVuIChyZXBsYWNlIGN1cnJlbnQgSUQgd2l0aCBuZXcgb25lIGluIHBhcmVudCBhbmQgcGFyZW50cylcclxuXHRcdFx0Zm9yKGkgPSAwLCBqID0gb2JqLmNoaWxkcmVuLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdG1bb2JqLmNoaWxkcmVuW2ldXS5wYXJlbnQgPSBpZDtcclxuXHRcdFx0fVxyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSBvYmouY2hpbGRyZW5fZC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRtW29iai5jaGlsZHJlbl9kW2ldXS5wYXJlbnRzWyQuaW5BcnJheShvYmouaWQsIG1bb2JqLmNoaWxkcmVuX2RbaV1dLnBhcmVudHMpXSA9IGlkO1xyXG5cdFx0XHR9XHJcblx0XHRcdGkgPSAkLmluQXJyYXkob2JqLmlkLCB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQpO1xyXG5cdFx0XHRpZihpICE9PSAtMSkgeyB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWRbaV0gPSBpZDsgfVxyXG5cdFx0XHQvLyB1cGRhdGUgbW9kZWwgYW5kIG9iaiBpdHNlbGYgKG9iai5pZCwgdGhpcy5fbW9kZWwuZGF0YVtLRVldKVxyXG5cdFx0XHRpID0gdGhpcy5nZXRfbm9kZShvYmouaWQsIHRydWUpO1xyXG5cdFx0XHRpZihpKSB7XHJcblx0XHRcdFx0aS5hdHRyKCdpZCcsIGlkKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRkZWxldGUgbVtvYmouaWRdO1xyXG5cdFx0XHRvYmouaWQgPSBpZDtcclxuXHRcdFx0bVtpZF0gPSBvYmo7XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0IHRoZSB0ZXh0IHZhbHVlIG9mIGEgbm9kZVxyXG5cdFx0ICogQG5hbWUgZ2V0X3RleHQob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9iaiB0aGUgbm9kZVxyXG5cdFx0ICogQHJldHVybiB7U3RyaW5nfVxyXG5cdFx0ICovXHJcblx0XHRnZXRfdGV4dCA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRyZXR1cm4gKCFvYmogfHwgb2JqLmlkID09PSAnIycpID8gZmFsc2UgOiBvYmoudGV4dDtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHNldCB0aGUgdGV4dCB2YWx1ZSBvZiBhIG5vZGUuIFVzZWQgaW50ZXJuYWxseSwgcGxlYXNlIHVzZSBgcmVuYW1lX25vZGUob2JqLCB2YWwpYC5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBzZXRfdGV4dChvYmosIHZhbClcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmogdGhlIG5vZGUsIHlvdSBjYW4gcGFzcyBhbiBhcnJheSB0byBzZXQgdGhlIHRleHQgb24gbXVsdGlwbGUgbm9kZXNcclxuXHRcdCAqIEBwYXJhbSAge1N0cmluZ30gdmFsIHRoZSBuZXcgdGV4dCB2YWx1ZVxyXG5cdFx0ICogQHJldHVybiB7Qm9vbGVhbn1cclxuXHRcdCAqIEB0cmlnZ2VyIHNldF90ZXh0LmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRzZXRfdGV4dCA6IGZ1bmN0aW9uIChvYmosIHZhbCkge1xyXG5cdFx0XHR2YXIgdDEsIHQyLCBkb20sIHRtcDtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuc2V0X3RleHQob2JqW3QxXSwgdmFsKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRvYmoudGV4dCA9IHZhbDtcclxuXHRcdFx0ZG9tID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpO1xyXG5cdFx0XHRpZihkb20ubGVuZ3RoKSB7XHJcblx0XHRcdFx0ZG9tID0gZG9tLmNoaWxkcmVuKFwiLmpzdHJlZS1hbmNob3I6ZXEoMClcIik7XHJcblx0XHRcdFx0dG1wID0gZG9tLmNoaWxkcmVuKFwiSVwiKS5jbG9uZSgpO1xyXG5cdFx0XHRcdGRvbS5odG1sKHZhbCkucHJlcGVuZCh0bXApO1xyXG5cdFx0XHRcdC8qKlxyXG5cdFx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGEgbm9kZSB0ZXh0IHZhbHVlIGlzIGNoYW5nZWRcclxuXHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHQgKiBAbmFtZSBzZXRfdGV4dC5qc3RyZWVcclxuXHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gb2JqXHJcblx0XHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IHRleHQgdGhlIG5ldyB2YWx1ZVxyXG5cdFx0XHRcdCAqL1xyXG5cdFx0XHRcdHRoaXMudHJpZ2dlcignc2V0X3RleHQnLHsgXCJvYmpcIiA6IG9iaiwgXCJ0ZXh0XCIgOiB2YWwgfSk7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBnZXRzIGEgSlNPTiByZXByZXNlbnRhdGlvbiBvZiBhIG5vZGUgKG9yIHRoZSB3aG9sZSB0cmVlKVxyXG5cdFx0ICogQG5hbWUgZ2V0X2pzb24oW29iaiwgb3B0aW9uc10pXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqXHJcblx0XHQgKiBAcGFyYW0gIHtPYmplY3R9IG9wdGlvbnNcclxuXHRcdCAqIEBwYXJhbSAge0Jvb2xlYW59IG9wdGlvbnMubm9fc3RhdGUgZG8gbm90IHJldHVybiBzdGF0ZSBpbmZvcm1hdGlvblxyXG5cdFx0ICogQHBhcmFtICB7Qm9vbGVhbn0gb3B0aW9ucy5ub19pZCBkbyBub3QgcmV0dXJuIElEXHJcblx0XHQgKiBAcGFyYW0gIHtCb29sZWFufSBvcHRpb25zLm5vX2NoaWxkcmVuIGRvIG5vdCBpbmNsdWRlIGNoaWxkcmVuXHJcblx0XHQgKiBAcGFyYW0gIHtCb29sZWFufSBvcHRpb25zLm5vX2RhdGEgZG8gbm90IGluY2x1ZGUgbm9kZSBkYXRhXHJcblx0XHQgKiBAcGFyYW0gIHtCb29sZWFufSBvcHRpb25zLmZsYXQgcmV0dXJuIGZsYXQgSlNPTiBpbnN0ZWFkIG9mIG5lc3RlZFxyXG5cdFx0ICogQHJldHVybiB7T2JqZWN0fVxyXG5cdFx0ICovXHJcblx0XHRnZXRfanNvbiA6IGZ1bmN0aW9uIChvYmosIG9wdGlvbnMsIGZsYXQpIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmogfHwgJyMnKTtcclxuXHRcdFx0aWYoIW9iaikgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0aWYob3B0aW9ucyAmJiBvcHRpb25zLmZsYXQgJiYgIWZsYXQpIHsgZmxhdCA9IFtdOyB9XHJcblx0XHRcdHZhciB0bXAgPSB7XHJcblx0XHRcdFx0J2lkJyA6IG9iai5pZCxcclxuXHRcdFx0XHQndGV4dCcgOiBvYmoudGV4dCxcclxuXHRcdFx0XHQnaWNvbicgOiB0aGlzLmdldF9pY29uKG9iaiksXHJcblx0XHRcdFx0J2xpX2F0dHInIDogb2JqLmxpX2F0dHIsXHJcblx0XHRcdFx0J2FfYXR0cicgOiBvYmouYV9hdHRyLFxyXG5cdFx0XHRcdCdzdGF0ZScgOiB7fSxcclxuXHRcdFx0XHQnZGF0YScgOiBvcHRpb25zICYmIG9wdGlvbnMubm9fZGF0YSA/IGZhbHNlIDogb2JqLmRhdGFcclxuXHRcdFx0XHQvLyggdGhpcy5nZXRfbm9kZShvYmosIHRydWUpLmxlbmd0aCA/IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKS5kYXRhKCkgOiBvYmouZGF0YSApLFxyXG5cdFx0XHR9LCBpLCBqO1xyXG5cdFx0XHRpZihvcHRpb25zICYmIG9wdGlvbnMuZmxhdCkge1xyXG5cdFx0XHRcdHRtcC5wYXJlbnQgPSBvYmoucGFyZW50O1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdHRtcC5jaGlsZHJlbiA9IFtdO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKCFvcHRpb25zIHx8ICFvcHRpb25zLm5vX3N0YXRlKSB7XHJcblx0XHRcdFx0Zm9yKGkgaW4gb2JqLnN0YXRlKSB7XHJcblx0XHRcdFx0XHRpZihvYmouc3RhdGUuaGFzT3duUHJvcGVydHkoaSkpIHtcclxuXHRcdFx0XHRcdFx0dG1wLnN0YXRlW2ldID0gb2JqLnN0YXRlW2ldO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZihvcHRpb25zICYmIG9wdGlvbnMubm9faWQpIHtcclxuXHRcdFx0XHRkZWxldGUgdG1wLmlkO1xyXG5cdFx0XHRcdGlmKHRtcC5saV9hdHRyICYmIHRtcC5saV9hdHRyLmlkKSB7XHJcblx0XHRcdFx0XHRkZWxldGUgdG1wLmxpX2F0dHIuaWQ7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKG9wdGlvbnMgJiYgb3B0aW9ucy5mbGF0ICYmIG9iai5pZCAhPT0gJyMnKSB7XHJcblx0XHRcdFx0ZmxhdC5wdXNoKHRtcCk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoIW9wdGlvbnMgfHwgIW9wdGlvbnMubm9fY2hpbGRyZW4pIHtcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSBvYmouY2hpbGRyZW4ubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRpZihvcHRpb25zICYmIG9wdGlvbnMuZmxhdCkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmdldF9qc29uKG9iai5jaGlsZHJlbltpXSwgb3B0aW9ucywgZmxhdCk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0dG1wLmNoaWxkcmVuLnB1c2godGhpcy5nZXRfanNvbihvYmouY2hpbGRyZW5baV0sIG9wdGlvbnMpKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIG9wdGlvbnMgJiYgb3B0aW9ucy5mbGF0ID8gZmxhdCA6IChvYmouaWQgPT09ICcjJyA/IHRtcC5jaGlsZHJlbiA6IHRtcCk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBjcmVhdGUgYSBuZXcgbm9kZSAoZG8gbm90IGNvbmZ1c2Ugd2l0aCBsb2FkX25vZGUpXHJcblx0XHQgKiBAbmFtZSBjcmVhdGVfbm9kZShbb2JqLCBub2RlLCBwb3MsIGNhbGxiYWNrLCBpc19sb2FkZWRdKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9ICAgcGFyICAgICAgIHRoZSBwYXJlbnQgbm9kZVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9ICAgbm9kZSAgICAgIHRoZSBkYXRhIGZvciB0aGUgbmV3IG5vZGUgKGEgdmFsaWQgSlNPTiBvYmplY3QsIG9yIGEgc2ltcGxlIHN0cmluZyB3aXRoIHRoZSBuYW1lKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9ICAgcG9zICAgICAgIHRoZSBpbmRleCBhdCB3aGljaCB0byBpbnNlcnQgdGhlIG5vZGUsIFwiZmlyc3RcIiBhbmQgXCJsYXN0XCIgYXJlIGFsc28gc3VwcG9ydGVkLCBkZWZhdWx0IGlzIFwibGFzdFwiXHJcblx0XHQgKiBAcGFyYW0gIHtGdW5jdGlvbn0gY2FsbGJhY2sgYSBmdW5jdGlvbiB0byBiZSBjYWxsZWQgb25jZSB0aGUgbm9kZSBpcyBjcmVhdGVkXHJcblx0XHQgKiBAcGFyYW0gIHtCb29sZWFufSBpc19sb2FkZWQgaW50ZXJuYWwgYXJndW1lbnQgaW5kaWNhdGluZyBpZiB0aGUgcGFyZW50IG5vZGUgd2FzIHN1Y2Nlc2Z1bGx5IGxvYWRlZFxyXG5cdFx0ICogQHJldHVybiB7U3RyaW5nfSAgICAgICAgICAgIHRoZSBJRCBvZiB0aGUgbmV3bHkgY3JlYXRlIG5vZGVcclxuXHRcdCAqIEB0cmlnZ2VyIG1vZGVsLmpzdHJlZSwgY3JlYXRlX25vZGUuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGNyZWF0ZV9ub2RlIDogZnVuY3Rpb24gKHBhciwgbm9kZSwgcG9zLCBjYWxsYmFjaywgaXNfbG9hZGVkKSB7XHJcblx0XHRcdHBhciA9IHRoaXMuZ2V0X25vZGUocGFyKTtcclxuXHRcdFx0aWYoIXBhcikgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0cG9zID0gcG9zID09PSB1bmRlZmluZWQgPyBcImxhc3RcIiA6IHBvcztcclxuXHRcdFx0aWYoIXBvcy50b1N0cmluZygpLm1hdGNoKC9eKGJlZm9yZXxhZnRlcikkLykgJiYgIWlzX2xvYWRlZCAmJiAhdGhpcy5pc19sb2FkZWQocGFyKSkge1xyXG5cdFx0XHRcdHJldHVybiB0aGlzLmxvYWRfbm9kZShwYXIsIGZ1bmN0aW9uICgpIHsgdGhpcy5jcmVhdGVfbm9kZShwYXIsIG5vZGUsIHBvcywgY2FsbGJhY2ssIHRydWUpOyB9KTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZighbm9kZSkgeyBub2RlID0geyBcInRleHRcIiA6IHRoaXMuZ2V0X3N0cmluZygnTmV3IG5vZGUnKSB9OyB9XHJcblx0XHRcdGlmKG5vZGUudGV4dCA9PT0gdW5kZWZpbmVkKSB7IG5vZGUudGV4dCA9IHRoaXMuZ2V0X3N0cmluZygnTmV3IG5vZGUnKTsgfVxyXG5cdFx0XHR2YXIgdG1wLCBkcGMsIGksIGo7XHJcblxyXG5cdFx0XHRpZihwYXIuaWQgPT09ICcjJykge1xyXG5cdFx0XHRcdGlmKHBvcyA9PT0gXCJiZWZvcmVcIikgeyBwb3MgPSBcImZpcnN0XCI7IH1cclxuXHRcdFx0XHRpZihwb3MgPT09IFwiYWZ0ZXJcIikgeyBwb3MgPSBcImxhc3RcIjsgfVxyXG5cdFx0XHR9XHJcblx0XHRcdHN3aXRjaChwb3MpIHtcclxuXHRcdFx0XHRjYXNlIFwiYmVmb3JlXCI6XHJcblx0XHRcdFx0XHR0bXAgPSB0aGlzLmdldF9ub2RlKHBhci5wYXJlbnQpO1xyXG5cdFx0XHRcdFx0cG9zID0gJC5pbkFycmF5KHBhci5pZCwgdG1wLmNoaWxkcmVuKTtcclxuXHRcdFx0XHRcdHBhciA9IHRtcDtcclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdGNhc2UgXCJhZnRlclwiIDpcclxuXHRcdFx0XHRcdHRtcCA9IHRoaXMuZ2V0X25vZGUocGFyLnBhcmVudCk7XHJcblx0XHRcdFx0XHRwb3MgPSAkLmluQXJyYXkocGFyLmlkLCB0bXAuY2hpbGRyZW4pICsgMTtcclxuXHRcdFx0XHRcdHBhciA9IHRtcDtcclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdGNhc2UgXCJpbnNpZGVcIjpcclxuXHRcdFx0XHRjYXNlIFwiZmlyc3RcIjpcclxuXHRcdFx0XHRcdHBvcyA9IDA7XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRjYXNlIFwibGFzdFwiOlxyXG5cdFx0XHRcdFx0cG9zID0gcGFyLmNoaWxkcmVuLmxlbmd0aDtcclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdGRlZmF1bHQ6XHJcblx0XHRcdFx0XHRpZighcG9zKSB7IHBvcyA9IDA7IH1cclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHBvcyA+IHBhci5jaGlsZHJlbi5sZW5ndGgpIHsgcG9zID0gcGFyLmNoaWxkcmVuLmxlbmd0aDsgfVxyXG5cdFx0XHRpZighbm9kZS5pZCkgeyBub2RlLmlkID0gdHJ1ZTsgfVxyXG5cdFx0XHRpZighdGhpcy5jaGVjayhcImNyZWF0ZV9ub2RlXCIsIG5vZGUsIHBhciwgcG9zKSkge1xyXG5cdFx0XHRcdHRoaXMuc2V0dGluZ3MuY29yZS5lcnJvci5jYWxsKHRoaXMsIHRoaXMuX2RhdGEuY29yZS5sYXN0X2Vycm9yKTtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYobm9kZS5pZCA9PT0gdHJ1ZSkgeyBkZWxldGUgbm9kZS5pZDsgfVxyXG5cdFx0XHRub2RlID0gdGhpcy5fcGFyc2VfbW9kZWxfZnJvbV9qc29uKG5vZGUsIHBhci5pZCwgcGFyLnBhcmVudHMuY29uY2F0KCkpO1xyXG5cdFx0XHRpZighbm9kZSkgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0dG1wID0gdGhpcy5nZXRfbm9kZShub2RlKTtcclxuXHRcdFx0ZHBjID0gW107XHJcblx0XHRcdGRwYy5wdXNoKG5vZGUpO1xyXG5cdFx0XHRkcGMgPSBkcGMuY29uY2F0KHRtcC5jaGlsZHJlbl9kKTtcclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdtb2RlbCcsIHsgXCJub2Rlc1wiIDogZHBjLCBcInBhcmVudFwiIDogcGFyLmlkIH0pO1xyXG5cclxuXHRcdFx0cGFyLmNoaWxkcmVuX2QgPSBwYXIuY2hpbGRyZW5fZC5jb25jYXQoZHBjKTtcclxuXHRcdFx0Zm9yKGkgPSAwLCBqID0gcGFyLnBhcmVudHMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0dGhpcy5fbW9kZWwuZGF0YVtwYXIucGFyZW50c1tpXV0uY2hpbGRyZW5fZCA9IHRoaXMuX21vZGVsLmRhdGFbcGFyLnBhcmVudHNbaV1dLmNoaWxkcmVuX2QuY29uY2F0KGRwYyk7XHJcblx0XHRcdH1cclxuXHRcdFx0bm9kZSA9IHRtcDtcclxuXHRcdFx0dG1wID0gW107XHJcblx0XHRcdGZvcihpID0gMCwgaiA9IHBhci5jaGlsZHJlbi5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHR0bXBbaSA+PSBwb3MgPyBpKzEgOiBpXSA9IHBhci5jaGlsZHJlbltpXTtcclxuXHRcdFx0fVxyXG5cdFx0XHR0bXBbcG9zXSA9IG5vZGUuaWQ7XHJcblx0XHRcdHBhci5jaGlsZHJlbiA9IHRtcDtcclxuXHJcblx0XHRcdHRoaXMucmVkcmF3X25vZGUocGFyLCB0cnVlKTtcclxuXHRcdFx0aWYoY2FsbGJhY2spIHsgY2FsbGJhY2suY2FsbCh0aGlzLCB0aGlzLmdldF9ub2RlKG5vZGUpKTsgfVxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gYSBub2RlIGlzIGNyZWF0ZWRcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIGNyZWF0ZV9ub2RlLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gbm9kZVxyXG5cdFx0XHQgKiBAcGFyYW0ge1N0cmluZ30gcGFyZW50IHRoZSBwYXJlbnQncyBJRFxyXG5cdFx0XHQgKiBAcGFyYW0ge051bWJlcn0gcG9zaXRpb24gdGhlIHBvc2l0aW9uIG9mIHRoZSBuZXcgbm9kZSBhbW9uZyB0aGUgcGFyZW50J3MgY2hpbGRyZW5cclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignY3JlYXRlX25vZGUnLCB7IFwibm9kZVwiIDogdGhpcy5nZXRfbm9kZShub2RlKSwgXCJwYXJlbnRcIiA6IHBhci5pZCwgXCJwb3NpdGlvblwiIDogcG9zIH0pO1xyXG5cdFx0XHRyZXR1cm4gbm9kZS5pZDtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHNldCB0aGUgdGV4dCB2YWx1ZSBvZiBhIG5vZGVcclxuXHRcdCAqIEBuYW1lIHJlbmFtZV9ub2RlKG9iaiwgdmFsKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9iaiB0aGUgbm9kZSwgeW91IGNhbiBwYXNzIGFuIGFycmF5IHRvIHJlbmFtZSBtdWx0aXBsZSBub2RlcyB0byB0aGUgc2FtZSBuYW1lXHJcblx0XHQgKiBAcGFyYW0gIHtTdHJpbmd9IHZhbCB0aGUgbmV3IHRleHQgdmFsdWVcclxuXHRcdCAqIEByZXR1cm4ge0Jvb2xlYW59XHJcblx0XHQgKiBAdHJpZ2dlciByZW5hbWVfbm9kZS5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0cmVuYW1lX25vZGUgOiBmdW5jdGlvbiAob2JqLCB2YWwpIHtcclxuXHRcdFx0dmFyIHQxLCB0Miwgb2xkO1xyXG5cdFx0XHRpZigkLmlzQXJyYXkob2JqKSkge1xyXG5cdFx0XHRcdG9iaiA9IG9iai5zbGljZSgpO1xyXG5cdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gb2JqLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0dGhpcy5yZW5hbWVfbm9kZShvYmpbdDFdLCB2YWwpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqLmlkID09PSAnIycpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdG9sZCA9IG9iai50ZXh0O1xyXG5cdFx0XHRpZighdGhpcy5jaGVjayhcInJlbmFtZV9ub2RlXCIsIG9iaiwgdGhpcy5nZXRfcGFyZW50KG9iaiksIHZhbCkpIHtcclxuXHRcdFx0XHR0aGlzLnNldHRpbmdzLmNvcmUuZXJyb3IuY2FsbCh0aGlzLCB0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvcik7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdHRoaXMuc2V0X3RleHQob2JqLCB2YWwpOyAvLyAuYXBwbHkodGhpcywgQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwoYXJndW1lbnRzKSlcclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGEgbm9kZSBpcyByZW5hbWVkXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSByZW5hbWVfbm9kZS5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGVcclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IHRleHQgdGhlIG5ldyB2YWx1ZVxyXG5cdFx0XHQgKiBAcGFyYW0ge1N0cmluZ30gb2xkIHRoZSBvbGQgdmFsdWVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcigncmVuYW1lX25vZGUnLCB7IFwibm9kZVwiIDogb2JqLCBcInRleHRcIiA6IHZhbCwgXCJvbGRcIiA6IG9sZCB9KTtcclxuXHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiByZW1vdmUgYSBub2RlXHJcblx0XHQgKiBAbmFtZSBkZWxldGVfbm9kZShvYmopXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqIHRoZSBub2RlLCB5b3UgY2FuIHBhc3MgYW4gYXJyYXkgdG8gZGVsZXRlIG11bHRpcGxlIG5vZGVzXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICogQHRyaWdnZXIgZGVsZXRlX25vZGUuanN0cmVlLCBjaGFuZ2VkLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRkZWxldGVfbm9kZSA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0dmFyIHQxLCB0MiwgcGFyLCBwb3MsIHRtcCwgaSwgaiwgaywgbCwgYztcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuZGVsZXRlX25vZGUob2JqW3QxXSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHR9XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0aWYoIW9iaiB8fCBvYmouaWQgPT09ICcjJykgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0cGFyID0gdGhpcy5nZXRfbm9kZShvYmoucGFyZW50KTtcclxuXHRcdFx0cG9zID0gJC5pbkFycmF5KG9iai5pZCwgcGFyLmNoaWxkcmVuKTtcclxuXHRcdFx0YyA9IGZhbHNlO1xyXG5cdFx0XHRpZighdGhpcy5jaGVjayhcImRlbGV0ZV9ub2RlXCIsIG9iaiwgcGFyLCBwb3MpKSB7XHJcblx0XHRcdFx0dGhpcy5zZXR0aW5ncy5jb3JlLmVycm9yLmNhbGwodGhpcywgdGhpcy5fZGF0YS5jb3JlLmxhc3RfZXJyb3IpO1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZihwb3MgIT09IC0xKSB7XHJcblx0XHRcdFx0cGFyLmNoaWxkcmVuID0gJC52YWthdGEuYXJyYXlfcmVtb3ZlKHBhci5jaGlsZHJlbiwgcG9zKTtcclxuXHRcdFx0fVxyXG5cdFx0XHR0bXAgPSBvYmouY2hpbGRyZW5fZC5jb25jYXQoW10pO1xyXG5cdFx0XHR0bXAucHVzaChvYmouaWQpO1xyXG5cdFx0XHRmb3IoayA9IDAsIGwgPSB0bXAubGVuZ3RoOyBrIDwgbDsgaysrKSB7XHJcblx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gb2JqLnBhcmVudHMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRwb3MgPSAkLmluQXJyYXkodG1wW2tdLCB0aGlzLl9tb2RlbC5kYXRhW29iai5wYXJlbnRzW2ldXS5jaGlsZHJlbl9kKTtcclxuXHRcdFx0XHRcdGlmKHBvcyAhPT0gLTEpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5fbW9kZWwuZGF0YVtvYmoucGFyZW50c1tpXV0uY2hpbGRyZW5fZCA9ICQudmFrYXRhLmFycmF5X3JlbW92ZSh0aGlzLl9tb2RlbC5kYXRhW29iai5wYXJlbnRzW2ldXS5jaGlsZHJlbl9kLCBwb3MpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRpZih0aGlzLl9tb2RlbC5kYXRhW3RtcFtrXV0uc3RhdGUuc2VsZWN0ZWQpIHtcclxuXHRcdFx0XHRcdGMgPSB0cnVlO1xyXG5cdFx0XHRcdFx0cG9zID0gJC5pbkFycmF5KHRtcFtrXSwgdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkKTtcclxuXHRcdFx0XHRcdGlmKHBvcyAhPT0gLTEpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkID0gJC52YWthdGEuYXJyYXlfcmVtb3ZlKHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZCwgcG9zKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIGEgbm9kZSBpcyBkZWxldGVkXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSBkZWxldGVfbm9kZS5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGVcclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IHBhcmVudCB0aGUgcGFyZW50J3MgSURcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignZGVsZXRlX25vZGUnLCB7IFwibm9kZVwiIDogb2JqLCBcInBhcmVudFwiIDogcGFyLmlkIH0pO1xyXG5cdFx0XHRpZihjKSB7XHJcblx0XHRcdFx0dGhpcy50cmlnZ2VyKCdjaGFuZ2VkJywgeyAnYWN0aW9uJyA6ICdkZWxldGVfbm9kZScsICdub2RlJyA6IG9iaiwgJ3NlbGVjdGVkJyA6IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZCwgJ3BhcmVudCcgOiBwYXIuaWQgfSk7XHJcblx0XHRcdH1cclxuXHRcdFx0Zm9yKGsgPSAwLCBsID0gdG1wLmxlbmd0aDsgayA8IGw7IGsrKykge1xyXG5cdFx0XHRcdGRlbGV0ZSB0aGlzLl9tb2RlbC5kYXRhW3RtcFtrXV07XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5yZWRyYXdfbm9kZShwYXIsIHRydWUpO1xyXG5cdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGNoZWNrIGlmIGFuIG9wZXJhdGlvbiBpcyBwcmVtaXR0ZWQgb24gdGhlIHRyZWUuIFVzZWQgaW50ZXJuYWxseS5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBjaGVjayhjaGssIG9iaiwgcGFyLCBwb3MpXHJcblx0XHQgKiBAcGFyYW0gIHtTdHJpbmd9IGNoayB0aGUgb3BlcmF0aW9uIHRvIGNoZWNrLCBjYW4gYmUgXCJjcmVhdGVfbm9kZVwiLCBcInJlbmFtZV9ub2RlXCIsIFwiZGVsZXRlX25vZGVcIiwgXCJjb3B5X25vZGVcIiBvciBcIm1vdmVfbm9kZVwiXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqIHRoZSBub2RlXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gcGFyIHRoZSBwYXJlbnRcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBwb3MgdGhlIHBvc2l0aW9uIHRvIGluc2VydCBhdCwgb3IgaWYgXCJyZW5hbWVfbm9kZVwiIC0gdGhlIG5ldyBuYW1lXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICovXHJcblx0XHRjaGVjayA6IGZ1bmN0aW9uIChjaGssIG9iaiwgcGFyLCBwb3MpIHtcclxuXHRcdFx0b2JqID0gb2JqICYmIG9iai5pZCA/IG9iaiA6IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0cGFyID0gcGFyICYmIHBhci5pZCA/IHBhciA6IHRoaXMuZ2V0X25vZGUocGFyKTtcclxuXHRcdFx0dmFyIHRtcCA9IGNoay5tYXRjaCgvXm1vdmVfbm9kZXxjb3B5X25vZGV8Y3JlYXRlX25vZGUkL2kpID8gcGFyIDogb2JqLFxyXG5cdFx0XHRcdGNoYyA9IHRoaXMuc2V0dGluZ3MuY29yZS5jaGVja19jYWxsYmFjaztcclxuXHRcdFx0aWYoY2hrID09PSBcIm1vdmVfbm9kZVwiKSB7XHJcblx0XHRcdFx0aWYob2JqLmlkID09PSBwYXIuaWQgfHwgJC5pbkFycmF5KG9iai5pZCwgcGFyLmNoaWxkcmVuKSA9PT0gcG9zIHx8ICQuaW5BcnJheShwYXIuaWQsIG9iai5jaGlsZHJlbl9kKSAhPT0gLTEpIHtcclxuXHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5sYXN0X2Vycm9yID0geyAnZXJyb3InIDogJ2NoZWNrJywgJ3BsdWdpbicgOiAnY29yZScsICdpZCcgOiAnY29yZV8wMScsICdyZWFzb24nIDogJ01vdmluZyBwYXJlbnQgaW5zaWRlIGNoaWxkJywgJ2RhdGEnIDogSlNPTi5zdHJpbmdpZnkoeyAnY2hrJyA6IGNoaywgJ3BvcycgOiBwb3MsICdvYmonIDogb2JqICYmIG9iai5pZCA/IG9iai5pZCA6IGZhbHNlLCAncGFyJyA6IHBhciAmJiBwYXIuaWQgPyBwYXIuaWQgOiBmYWxzZSB9KSB9O1xyXG5cdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHR0bXAgPSB0aGlzLmdldF9ub2RlKHRtcCwgdHJ1ZSk7XHJcblx0XHRcdGlmKHRtcC5sZW5ndGgpIHsgdG1wID0gdG1wLmRhdGEoJ2pzdHJlZScpOyB9XHJcblx0XHRcdGlmKHRtcCAmJiB0bXAuZnVuY3Rpb25zICYmICh0bXAuZnVuY3Rpb25zW2Noa10gPT09IGZhbHNlIHx8IHRtcC5mdW5jdGlvbnNbY2hrXSA9PT0gdHJ1ZSkpIHtcclxuXHRcdFx0XHRpZih0bXAuZnVuY3Rpb25zW2Noa10gPT09IGZhbHNlKSB7XHJcblx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvciA9IHsgJ2Vycm9yJyA6ICdjaGVjaycsICdwbHVnaW4nIDogJ2NvcmUnLCAnaWQnIDogJ2NvcmVfMDInLCAncmVhc29uJyA6ICdOb2RlIGRhdGEgcHJldmVudHMgZnVuY3Rpb246ICcgKyBjaGssICdkYXRhJyA6IEpTT04uc3RyaW5naWZ5KHsgJ2NoaycgOiBjaGssICdwb3MnIDogcG9zLCAnb2JqJyA6IG9iaiAmJiBvYmouaWQgPyBvYmouaWQgOiBmYWxzZSwgJ3BhcicgOiBwYXIgJiYgcGFyLmlkID8gcGFyLmlkIDogZmFsc2UgfSkgfTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0cmV0dXJuIHRtcC5mdW5jdGlvbnNbY2hrXTtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZihjaGMgPT09IGZhbHNlIHx8ICgkLmlzRnVuY3Rpb24oY2hjKSAmJiBjaGMuY2FsbCh0aGlzLCBjaGssIG9iaiwgcGFyLCBwb3MpID09PSBmYWxzZSkgfHwgKGNoYyAmJiBjaGNbY2hrXSA9PT0gZmFsc2UpKSB7XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLmxhc3RfZXJyb3IgPSB7ICdlcnJvcicgOiAnY2hlY2snLCAncGx1Z2luJyA6ICdjb3JlJywgJ2lkJyA6ICdjb3JlXzAzJywgJ3JlYXNvbicgOiAnVXNlciBjb25maWcgZm9yIGNvcmUuY2hlY2tfY2FsbGJhY2sgcHJldmVudHMgZnVuY3Rpb246ICcgKyBjaGssICdkYXRhJyA6IEpTT04uc3RyaW5naWZ5KHsgJ2NoaycgOiBjaGssICdwb3MnIDogcG9zLCAnb2JqJyA6IG9iaiAmJiBvYmouaWQgPyBvYmouaWQgOiBmYWxzZSwgJ3BhcicgOiBwYXIgJiYgcGFyLmlkID8gcGFyLmlkIDogZmFsc2UgfSkgfTtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBnZXQgdGhlIGxhc3QgZXJyb3JcclxuXHRcdCAqIEBuYW1lIGxhc3RfZXJyb3IoKVxyXG5cdFx0ICogQHJldHVybiB7T2JqZWN0fVxyXG5cdFx0ICovXHJcblx0XHRsYXN0X2Vycm9yIDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5fZGF0YS5jb3JlLmxhc3RfZXJyb3I7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBtb3ZlIGEgbm9kZSB0byBhIG5ldyBwYXJlbnRcclxuXHRcdCAqIEBuYW1lIG1vdmVfbm9kZShvYmosIHBhciBbLCBwb3MsIGNhbGxiYWNrLCBpc19sb2FkZWRdKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9iaiB0aGUgbm9kZSB0byBtb3ZlLCBwYXNzIGFuIGFycmF5IHRvIG1vdmUgbXVsdGlwbGUgbm9kZXNcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBwYXIgdGhlIG5ldyBwYXJlbnRcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBwb3MgdGhlIHBvc2l0aW9uIHRvIGluc2VydCBhdCAoXCJmaXJzdFwiIGFuZCBcImxhc3RcIiBhcmUgc3VwcG9ydGVkLCBhcyB3ZWxsIGFzIFwiYmVmb3JlXCIgYW5kIFwiYWZ0ZXJcIiksIGRlZmF1bHRzIHRvIGAwYFxyXG5cdFx0ICogQHBhcmFtICB7ZnVuY3Rpb259IGNhbGxiYWNrIGEgZnVuY3Rpb24gdG8gY2FsbCBvbmNlIHRoZSBtb3ZlIGlzIGNvbXBsZXRlZCwgcmVjZWl2ZXMgMyBhcmd1bWVudHMgLSB0aGUgbm9kZSwgdGhlIG5ldyBwYXJlbnQgYW5kIHRoZSBwb3NpdGlvblxyXG5cdFx0ICogQHBhcmFtICB7Qm9vbGVhbn0gaW50ZXJuYWwgcGFyYW1ldGVyIGluZGljYXRpbmcgaWYgdGhlIHBhcmVudCBub2RlIGhhcyBiZWVuIGxvYWRlZFxyXG5cdFx0ICogQHRyaWdnZXIgbW92ZV9ub2RlLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRtb3ZlX25vZGUgOiBmdW5jdGlvbiAob2JqLCBwYXIsIHBvcywgY2FsbGJhY2ssIGlzX2xvYWRlZCkge1xyXG5cdFx0XHR2YXIgdDEsIHQyLCBvbGRfcGFyLCBuZXdfcGFyLCBvbGRfaW5zLCBpc19tdWx0aSwgZHBjLCB0bXAsIGksIGosIGssIGwsIHA7XHJcblx0XHRcdGlmKCQuaXNBcnJheShvYmopKSB7XHJcblx0XHRcdFx0b2JqID0gb2JqLnJldmVyc2UoKS5zbGljZSgpO1xyXG5cdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gb2JqLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0dGhpcy5tb3ZlX25vZGUob2JqW3QxXSwgcGFyLCBwb3MsIGNhbGxiYWNrLCBpc19sb2FkZWQpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSBvYmogJiYgb2JqLmlkID8gb2JqIDogdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRwYXIgPSB0aGlzLmdldF9ub2RlKHBhcik7XHJcblx0XHRcdHBvcyA9IHBvcyA9PT0gdW5kZWZpbmVkID8gMCA6IHBvcztcclxuXHJcblx0XHRcdGlmKCFwYXIgfHwgIW9iaiB8fCBvYmouaWQgPT09ICcjJykgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0aWYoIXBvcy50b1N0cmluZygpLm1hdGNoKC9eKGJlZm9yZXxhZnRlcikkLykgJiYgIWlzX2xvYWRlZCAmJiAhdGhpcy5pc19sb2FkZWQocGFyKSkge1xyXG5cdFx0XHRcdHJldHVybiB0aGlzLmxvYWRfbm9kZShwYXIsIGZ1bmN0aW9uICgpIHsgdGhpcy5tb3ZlX25vZGUob2JqLCBwYXIsIHBvcywgY2FsbGJhY2ssIHRydWUpOyB9KTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0b2xkX3BhciA9IChvYmoucGFyZW50IHx8ICcjJykudG9TdHJpbmcoKTtcclxuXHRcdFx0bmV3X3BhciA9ICghcG9zLnRvU3RyaW5nKCkubWF0Y2goL14oYmVmb3JlfGFmdGVyKSQvKSB8fCBwYXIuaWQgPT09ICcjJykgPyBwYXIgOiB0aGlzLmdldF9ub2RlKHBhci5wYXJlbnQpO1xyXG5cdFx0XHRvbGRfaW5zID0gdGhpcy5fbW9kZWwuZGF0YVtvYmouaWRdID8gdGhpcyA6ICQuanN0cmVlLnJlZmVyZW5jZShvYmouaWQpO1xyXG5cdFx0XHRpc19tdWx0aSA9ICFvbGRfaW5zIHx8ICFvbGRfaW5zLl9pZCB8fCAodGhpcy5faWQgIT09IG9sZF9pbnMuX2lkKTtcclxuXHRcdFx0aWYoaXNfbXVsdGkpIHtcclxuXHRcdFx0XHRpZih0aGlzLmNvcHlfbm9kZShvYmosIHBhciwgcG9zLCBjYWxsYmFjaywgaXNfbG9hZGVkKSkge1xyXG5cdFx0XHRcdFx0aWYob2xkX2lucykgeyBvbGRfaW5zLmRlbGV0ZV9ub2RlKG9iaik7IH1cclxuXHRcdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0Ly92YXIgbSA9IHRoaXMuX21vZGVsLmRhdGE7XHJcblx0XHRcdGlmKG5ld19wYXIuaWQgPT09ICcjJykge1xyXG5cdFx0XHRcdGlmKHBvcyA9PT0gXCJiZWZvcmVcIikgeyBwb3MgPSBcImZpcnN0XCI7IH1cclxuXHRcdFx0XHRpZihwb3MgPT09IFwiYWZ0ZXJcIikgeyBwb3MgPSBcImxhc3RcIjsgfVxyXG5cdFx0XHR9XHJcblx0XHRcdHN3aXRjaChwb3MpIHtcclxuXHRcdFx0XHRjYXNlIFwiYmVmb3JlXCI6XHJcblx0XHRcdFx0XHRwb3MgPSAkLmluQXJyYXkocGFyLmlkLCBuZXdfcGFyLmNoaWxkcmVuKTtcclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdGNhc2UgXCJhZnRlclwiIDpcclxuXHRcdFx0XHRcdHBvcyA9ICQuaW5BcnJheShwYXIuaWQsIG5ld19wYXIuY2hpbGRyZW4pICsgMTtcclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdGNhc2UgXCJpbnNpZGVcIjpcclxuXHRcdFx0XHRjYXNlIFwiZmlyc3RcIjpcclxuXHRcdFx0XHRcdHBvcyA9IDA7XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRjYXNlIFwibGFzdFwiOlxyXG5cdFx0XHRcdFx0cG9zID0gbmV3X3Bhci5jaGlsZHJlbi5sZW5ndGg7XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRkZWZhdWx0OlxyXG5cdFx0XHRcdFx0aWYoIXBvcykgeyBwb3MgPSAwOyB9XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0fVxyXG5cdFx0XHRpZihwb3MgPiBuZXdfcGFyLmNoaWxkcmVuLmxlbmd0aCkgeyBwb3MgPSBuZXdfcGFyLmNoaWxkcmVuLmxlbmd0aDsgfVxyXG5cdFx0XHRpZighdGhpcy5jaGVjayhcIm1vdmVfbm9kZVwiLCBvYmosIG5ld19wYXIsIHBvcykpIHtcclxuXHRcdFx0XHR0aGlzLnNldHRpbmdzLmNvcmUuZXJyb3IuY2FsbCh0aGlzLCB0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvcik7XHJcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKG9iai5wYXJlbnQgPT09IG5ld19wYXIuaWQpIHtcclxuXHRcdFx0XHRkcGMgPSBuZXdfcGFyLmNoaWxkcmVuLmNvbmNhdCgpO1xyXG5cdFx0XHRcdHRtcCA9ICQuaW5BcnJheShvYmouaWQsIGRwYyk7XHJcblx0XHRcdFx0aWYodG1wICE9PSAtMSkge1xyXG5cdFx0XHRcdFx0ZHBjID0gJC52YWthdGEuYXJyYXlfcmVtb3ZlKGRwYywgdG1wKTtcclxuXHRcdFx0XHRcdGlmKHBvcyA+IHRtcCkgeyBwb3MtLTsgfVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHR0bXAgPSBbXTtcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSBkcGMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHR0bXBbaSA+PSBwb3MgPyBpKzEgOiBpXSA9IGRwY1tpXTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0dG1wW3Bvc10gPSBvYmouaWQ7XHJcblx0XHRcdFx0bmV3X3Bhci5jaGlsZHJlbiA9IHRtcDtcclxuXHRcdFx0XHR0aGlzLl9ub2RlX2NoYW5nZWQobmV3X3Bhci5pZCk7XHJcblx0XHRcdFx0dGhpcy5yZWRyYXcobmV3X3Bhci5pZCA9PT0gJyMnKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRlbHNlIHtcclxuXHRcdFx0XHQvLyBjbGVhbiBvbGQgcGFyZW50IGFuZCB1cFxyXG5cdFx0XHRcdHRtcCA9IG9iai5jaGlsZHJlbl9kLmNvbmNhdCgpO1xyXG5cdFx0XHRcdHRtcC5wdXNoKG9iai5pZCk7XHJcblx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gb2JqLnBhcmVudHMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRkcGMgPSBbXTtcclxuXHRcdFx0XHRcdHAgPSBvbGRfaW5zLl9tb2RlbC5kYXRhW29iai5wYXJlbnRzW2ldXS5jaGlsZHJlbl9kO1xyXG5cdFx0XHRcdFx0Zm9yKGsgPSAwLCBsID0gcC5sZW5ndGg7IGsgPCBsOyBrKyspIHtcclxuXHRcdFx0XHRcdFx0aWYoJC5pbkFycmF5KHBba10sIHRtcCkgPT09IC0xKSB7XHJcblx0XHRcdFx0XHRcdFx0ZHBjLnB1c2gocFtrXSk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdG9sZF9pbnMuX21vZGVsLmRhdGFbb2JqLnBhcmVudHNbaV1dLmNoaWxkcmVuX2QgPSBkcGM7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdG9sZF9pbnMuX21vZGVsLmRhdGFbb2xkX3Bhcl0uY2hpbGRyZW4gPSAkLnZha2F0YS5hcnJheV9yZW1vdmVfaXRlbShvbGRfaW5zLl9tb2RlbC5kYXRhW29sZF9wYXJdLmNoaWxkcmVuLCBvYmouaWQpO1xyXG5cclxuXHRcdFx0XHQvLyBpbnNlcnQgaW50byBuZXcgcGFyZW50IGFuZCB1cFxyXG5cdFx0XHRcdGZvcihpID0gMCwgaiA9IG5ld19wYXIucGFyZW50cy5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuX21vZGVsLmRhdGFbbmV3X3Bhci5wYXJlbnRzW2ldXS5jaGlsZHJlbl9kID0gdGhpcy5fbW9kZWwuZGF0YVtuZXdfcGFyLnBhcmVudHNbaV1dLmNoaWxkcmVuX2QuY29uY2F0KHRtcCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGRwYyA9IFtdO1xyXG5cdFx0XHRcdGZvcihpID0gMCwgaiA9IG5ld19wYXIuY2hpbGRyZW4ubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRkcGNbaSA+PSBwb3MgPyBpKzEgOiBpXSA9IG5ld19wYXIuY2hpbGRyZW5baV07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGRwY1twb3NdID0gb2JqLmlkO1xyXG5cdFx0XHRcdG5ld19wYXIuY2hpbGRyZW4gPSBkcGM7XHJcblx0XHRcdFx0bmV3X3Bhci5jaGlsZHJlbl9kLnB1c2gob2JqLmlkKTtcclxuXHRcdFx0XHRuZXdfcGFyLmNoaWxkcmVuX2QgPSBuZXdfcGFyLmNoaWxkcmVuX2QuY29uY2F0KG9iai5jaGlsZHJlbl9kKTtcclxuXHJcblx0XHRcdFx0Ly8gdXBkYXRlIG9iamVjdFxyXG5cdFx0XHRcdG9iai5wYXJlbnQgPSBuZXdfcGFyLmlkO1xyXG5cdFx0XHRcdHRtcCA9IG5ld19wYXIucGFyZW50cy5jb25jYXQoKTtcclxuXHRcdFx0XHR0bXAudW5zaGlmdChuZXdfcGFyLmlkKTtcclxuXHRcdFx0XHRwID0gb2JqLnBhcmVudHMubGVuZ3RoO1xyXG5cdFx0XHRcdG9iai5wYXJlbnRzID0gdG1wO1xyXG5cclxuXHRcdFx0XHQvLyB1cGRhdGUgb2JqZWN0IGNoaWxkcmVuXHJcblx0XHRcdFx0dG1wID0gdG1wLmNvbmNhdCgpO1xyXG5cdFx0XHRcdGZvcihpID0gMCwgaiA9IG9iai5jaGlsZHJlbl9kLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0dGhpcy5fbW9kZWwuZGF0YVtvYmouY2hpbGRyZW5fZFtpXV0ucGFyZW50cyA9IHRoaXMuX21vZGVsLmRhdGFbb2JqLmNoaWxkcmVuX2RbaV1dLnBhcmVudHMuc2xpY2UoMCxwKi0xKTtcclxuXHRcdFx0XHRcdEFycmF5LnByb3RvdHlwZS5wdXNoLmFwcGx5KHRoaXMuX21vZGVsLmRhdGFbb2JqLmNoaWxkcmVuX2RbaV1dLnBhcmVudHMsIHRtcCk7XHJcblx0XHRcdFx0fVxyXG5cclxuXHRcdFx0XHR0aGlzLl9ub2RlX2NoYW5nZWQob2xkX3Bhcik7XHJcblx0XHRcdFx0dGhpcy5fbm9kZV9jaGFuZ2VkKG5ld19wYXIuaWQpO1xyXG5cdFx0XHRcdHRoaXMucmVkcmF3KG9sZF9wYXIgPT09ICcjJyB8fCBuZXdfcGFyLmlkID09PSAnIycpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKGNhbGxiYWNrKSB7IGNhbGxiYWNrLmNhbGwodGhpcywgb2JqLCBuZXdfcGFyLCBwb3MpOyB9XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhIG5vZGUgaXMgbW92ZWRcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIG1vdmVfbm9kZS5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IG5vZGVcclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IHBhcmVudCB0aGUgcGFyZW50J3MgSURcclxuXHRcdFx0ICogQHBhcmFtIHtOdW1iZXJ9IHBvc2l0aW9uIHRoZSBwb3NpdGlvbiBvZiB0aGUgbm9kZSBhbW9uZyB0aGUgcGFyZW50J3MgY2hpbGRyZW5cclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IG9sZF9wYXJlbnQgdGhlIG9sZCBwYXJlbnQgb2YgdGhlIG5vZGVcclxuXHRcdFx0ICogQHBhcmFtIHtCb29sZWFufSBpc19tdWx0aSBkbyB0aGUgbm9kZSBhbmQgbmV3IHBhcmVudCBiZWxvbmcgdG8gZGlmZmVyZW50IGluc3RhbmNlc1xyXG5cdFx0XHQgKiBAcGFyYW0ge2pzVHJlZX0gb2xkX2luc3RhbmNlIHRoZSBpbnN0YW5jZSB0aGUgbm9kZSBjYW1lIGZyb21cclxuXHRcdFx0ICogQHBhcmFtIHtqc1RyZWV9IG5ld19pbnN0YW5jZSB0aGUgaW5zdGFuY2Ugb2YgdGhlIG5ldyBwYXJlbnRcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignbW92ZV9ub2RlJywgeyBcIm5vZGVcIiA6IG9iaiwgXCJwYXJlbnRcIiA6IG5ld19wYXIuaWQsIFwicG9zaXRpb25cIiA6IHBvcywgXCJvbGRfcGFyZW50XCIgOiBvbGRfcGFyLCBcImlzX211bHRpXCIgOiBpc19tdWx0aSwgJ29sZF9pbnN0YW5jZScgOiBvbGRfaW5zLCAnbmV3X2luc3RhbmNlJyA6IHRoaXMgfSk7XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY29weSBhIG5vZGUgdG8gYSBuZXcgcGFyZW50XHJcblx0XHQgKiBAbmFtZSBjb3B5X25vZGUob2JqLCBwYXIgWywgcG9zLCBjYWxsYmFjaywgaXNfbG9hZGVkXSlcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmogdGhlIG5vZGUgdG8gY29weSwgcGFzcyBhbiBhcnJheSB0byBjb3B5IG11bHRpcGxlIG5vZGVzXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gcGFyIHRoZSBuZXcgcGFyZW50XHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gcG9zIHRoZSBwb3NpdGlvbiB0byBpbnNlcnQgYXQgKFwiZmlyc3RcIiBhbmQgXCJsYXN0XCIgYXJlIHN1cHBvcnRlZCwgYXMgd2VsbCBhcyBcImJlZm9yZVwiIGFuZCBcImFmdGVyXCIpLCBkZWZhdWx0cyB0byBgMGBcclxuXHRcdCAqIEBwYXJhbSAge2Z1bmN0aW9ufSBjYWxsYmFjayBhIGZ1bmN0aW9uIHRvIGNhbGwgb25jZSB0aGUgbW92ZSBpcyBjb21wbGV0ZWQsIHJlY2VpdmVzIDMgYXJndW1lbnRzIC0gdGhlIG5vZGUsIHRoZSBuZXcgcGFyZW50IGFuZCB0aGUgcG9zaXRpb25cclxuXHRcdCAqIEBwYXJhbSAge0Jvb2xlYW59IGludGVybmFsIHBhcmFtZXRlciBpbmRpY2F0aW5nIGlmIHRoZSBwYXJlbnQgbm9kZSBoYXMgYmVlbiBsb2FkZWRcclxuXHRcdCAqIEB0cmlnZ2VyIG1vZGVsLmpzdHJlZSBjb3B5X25vZGUuanN0cmVlXHJcblx0XHQgKi9cclxuXHRcdGNvcHlfbm9kZSA6IGZ1bmN0aW9uIChvYmosIHBhciwgcG9zLCBjYWxsYmFjaywgaXNfbG9hZGVkKSB7XHJcblx0XHRcdHZhciB0MSwgdDIsIGRwYywgdG1wLCBpLCBqLCBub2RlLCBvbGRfcGFyLCBuZXdfcGFyLCBvbGRfaW5zLCBpc19tdWx0aTtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmoucmV2ZXJzZSgpLnNsaWNlKCk7XHJcblx0XHRcdFx0Zm9yKHQxID0gMCwgdDIgPSBvYmoubGVuZ3RoOyB0MSA8IHQyOyB0MSsrKSB7XHJcblx0XHRcdFx0XHR0aGlzLmNvcHlfbm9kZShvYmpbdDFdLCBwYXIsIHBvcywgY2FsbGJhY2ssIGlzX2xvYWRlZCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHR9XHJcblx0XHRcdG9iaiA9IG9iaiAmJiBvYmouaWQgPyBvYmogOiB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdHBhciA9IHRoaXMuZ2V0X25vZGUocGFyKTtcclxuXHRcdFx0cG9zID0gcG9zID09PSB1bmRlZmluZWQgPyAwIDogcG9zO1xyXG5cclxuXHRcdFx0aWYoIXBhciB8fCAhb2JqIHx8IG9iai5pZCA9PT0gJyMnKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRpZighcG9zLnRvU3RyaW5nKCkubWF0Y2goL14oYmVmb3JlfGFmdGVyKSQvKSAmJiAhaXNfbG9hZGVkICYmICF0aGlzLmlzX2xvYWRlZChwYXIpKSB7XHJcblx0XHRcdFx0cmV0dXJuIHRoaXMubG9hZF9ub2RlKHBhciwgZnVuY3Rpb24gKCkgeyB0aGlzLmNvcHlfbm9kZShvYmosIHBhciwgcG9zLCBjYWxsYmFjaywgdHJ1ZSk7IH0pO1xyXG5cdFx0XHR9XHJcblxyXG5cdFx0XHRvbGRfcGFyID0gKG9iai5wYXJlbnQgfHwgJyMnKS50b1N0cmluZygpO1xyXG5cdFx0XHRuZXdfcGFyID0gKCFwb3MudG9TdHJpbmcoKS5tYXRjaCgvXihiZWZvcmV8YWZ0ZXIpJC8pIHx8IHBhci5pZCA9PT0gJyMnKSA/IHBhciA6IHRoaXMuZ2V0X25vZGUocGFyLnBhcmVudCk7XHJcblx0XHRcdG9sZF9pbnMgPSB0aGlzLl9tb2RlbC5kYXRhW29iai5pZF0gPyB0aGlzIDogJC5qc3RyZWUucmVmZXJlbmNlKG9iai5pZCk7XHJcblx0XHRcdGlzX211bHRpID0gIW9sZF9pbnMgfHwgIW9sZF9pbnMuX2lkIHx8ICh0aGlzLl9pZCAhPT0gb2xkX2lucy5faWQpO1xyXG5cdFx0XHRpZihuZXdfcGFyLmlkID09PSAnIycpIHtcclxuXHRcdFx0XHRpZihwb3MgPT09IFwiYmVmb3JlXCIpIHsgcG9zID0gXCJmaXJzdFwiOyB9XHJcblx0XHRcdFx0aWYocG9zID09PSBcImFmdGVyXCIpIHsgcG9zID0gXCJsYXN0XCI7IH1cclxuXHRcdFx0fVxyXG5cdFx0XHRzd2l0Y2gocG9zKSB7XHJcblx0XHRcdFx0Y2FzZSBcImJlZm9yZVwiOlxyXG5cdFx0XHRcdFx0cG9zID0gJC5pbkFycmF5KHBhci5pZCwgbmV3X3Bhci5jaGlsZHJlbik7XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRjYXNlIFwiYWZ0ZXJcIiA6XHJcblx0XHRcdFx0XHRwb3MgPSAkLmluQXJyYXkocGFyLmlkLCBuZXdfcGFyLmNoaWxkcmVuKSArIDE7XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRjYXNlIFwiaW5zaWRlXCI6XHJcblx0XHRcdFx0Y2FzZSBcImZpcnN0XCI6XHJcblx0XHRcdFx0XHRwb3MgPSAwO1xyXG5cdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0Y2FzZSBcImxhc3RcIjpcclxuXHRcdFx0XHRcdHBvcyA9IG5ld19wYXIuY2hpbGRyZW4ubGVuZ3RoO1xyXG5cdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0ZGVmYXVsdDpcclxuXHRcdFx0XHRcdGlmKCFwb3MpIHsgcG9zID0gMDsgfVxyXG5cdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYocG9zID4gbmV3X3Bhci5jaGlsZHJlbi5sZW5ndGgpIHsgcG9zID0gbmV3X3Bhci5jaGlsZHJlbi5sZW5ndGg7IH1cclxuXHRcdFx0aWYoIXRoaXMuY2hlY2soXCJjb3B5X25vZGVcIiwgb2JqLCBuZXdfcGFyLCBwb3MpKSB7XHJcblx0XHRcdFx0dGhpcy5zZXR0aW5ncy5jb3JlLmVycm9yLmNhbGwodGhpcywgdGhpcy5fZGF0YS5jb3JlLmxhc3RfZXJyb3IpO1xyXG5cdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRub2RlID0gb2xkX2lucyA/IG9sZF9pbnMuZ2V0X2pzb24ob2JqLCB7IG5vX2lkIDogdHJ1ZSwgbm9fZGF0YSA6IHRydWUsIG5vX3N0YXRlIDogdHJ1ZSB9KSA6IG9iajtcclxuXHRcdFx0aWYoIW5vZGUpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdGlmKG5vZGUuaWQgPT09IHRydWUpIHsgZGVsZXRlIG5vZGUuaWQ7IH1cclxuXHRcdFx0bm9kZSA9IHRoaXMuX3BhcnNlX21vZGVsX2Zyb21fanNvbihub2RlLCBuZXdfcGFyLmlkLCBuZXdfcGFyLnBhcmVudHMuY29uY2F0KCkpO1xyXG5cdFx0XHRpZighbm9kZSkgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0dG1wID0gdGhpcy5nZXRfbm9kZShub2RlKTtcclxuXHRcdFx0ZHBjID0gW107XHJcblx0XHRcdGRwYy5wdXNoKG5vZGUpO1xyXG5cdFx0XHRkcGMgPSBkcGMuY29uY2F0KHRtcC5jaGlsZHJlbl9kKTtcclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdtb2RlbCcsIHsgXCJub2Rlc1wiIDogZHBjLCBcInBhcmVudFwiIDogbmV3X3Bhci5pZCB9KTtcclxuXHJcblx0XHRcdC8vIGluc2VydCBpbnRvIG5ldyBwYXJlbnQgYW5kIHVwXHJcblx0XHRcdGZvcihpID0gMCwgaiA9IG5ld19wYXIucGFyZW50cy5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHR0aGlzLl9tb2RlbC5kYXRhW25ld19wYXIucGFyZW50c1tpXV0uY2hpbGRyZW5fZCA9IHRoaXMuX21vZGVsLmRhdGFbbmV3X3Bhci5wYXJlbnRzW2ldXS5jaGlsZHJlbl9kLmNvbmNhdChkcGMpO1xyXG5cdFx0XHR9XHJcblx0XHRcdGRwYyA9IFtdO1xyXG5cdFx0XHRmb3IoaSA9IDAsIGogPSBuZXdfcGFyLmNoaWxkcmVuLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdGRwY1tpID49IHBvcyA/IGkrMSA6IGldID0gbmV3X3Bhci5jaGlsZHJlbltpXTtcclxuXHRcdFx0fVxyXG5cdFx0XHRkcGNbcG9zXSA9IHRtcC5pZDtcclxuXHRcdFx0bmV3X3Bhci5jaGlsZHJlbiA9IGRwYztcclxuXHRcdFx0bmV3X3Bhci5jaGlsZHJlbl9kLnB1c2godG1wLmlkKTtcclxuXHRcdFx0bmV3X3Bhci5jaGlsZHJlbl9kID0gbmV3X3Bhci5jaGlsZHJlbl9kLmNvbmNhdCh0bXAuY2hpbGRyZW5fZCk7XHJcblxyXG5cdFx0XHR0aGlzLl9ub2RlX2NoYW5nZWQobmV3X3Bhci5pZCk7XHJcblx0XHRcdHRoaXMucmVkcmF3KG5ld19wYXIuaWQgPT09ICcjJyk7XHJcblx0XHRcdGlmKGNhbGxiYWNrKSB7IGNhbGxiYWNrLmNhbGwodGhpcywgdG1wLCBuZXdfcGFyLCBwb3MpOyB9XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhIG5vZGUgaXMgY29waWVkXHJcblx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHQgKiBAbmFtZSBjb3B5X25vZGUuanN0cmVlXHJcblx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBub2RlIHRoZSBjb3BpZWQgbm9kZVxyXG5cdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gb3JpZ2luYWwgdGhlIG9yaWdpbmFsIG5vZGVcclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IHBhcmVudCB0aGUgcGFyZW50J3MgSURcclxuXHRcdFx0ICogQHBhcmFtIHtOdW1iZXJ9IHBvc2l0aW9uIHRoZSBwb3NpdGlvbiBvZiB0aGUgbm9kZSBhbW9uZyB0aGUgcGFyZW50J3MgY2hpbGRyZW5cclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IG9sZF9wYXJlbnQgdGhlIG9sZCBwYXJlbnQgb2YgdGhlIG5vZGVcclxuXHRcdFx0ICogQHBhcmFtIHtCb29sZWFufSBpc19tdWx0aSBkbyB0aGUgbm9kZSBhbmQgbmV3IHBhcmVudCBiZWxvbmcgdG8gZGlmZmVyZW50IGluc3RhbmNlc1xyXG5cdFx0XHQgKiBAcGFyYW0ge2pzVHJlZX0gb2xkX2luc3RhbmNlIHRoZSBpbnN0YW5jZSB0aGUgbm9kZSBjYW1lIGZyb21cclxuXHRcdFx0ICogQHBhcmFtIHtqc1RyZWV9IG5ld19pbnN0YW5jZSB0aGUgaW5zdGFuY2Ugb2YgdGhlIG5ldyBwYXJlbnRcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignY29weV9ub2RlJywgeyBcIm5vZGVcIiA6IHRtcCwgXCJvcmlnaW5hbFwiIDogb2JqLCBcInBhcmVudFwiIDogbmV3X3Bhci5pZCwgXCJwb3NpdGlvblwiIDogcG9zLCBcIm9sZF9wYXJlbnRcIiA6IG9sZF9wYXIsIFwiaXNfbXVsdGlcIiA6IGlzX211bHRpLCAnb2xkX2luc3RhbmNlJyA6IG9sZF9pbnMsICduZXdfaW5zdGFuY2UnIDogdGhpcyB9KTtcclxuXHRcdFx0cmV0dXJuIHRtcC5pZDtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGN1dCBhIG5vZGUgKGEgbGF0ZXIgY2FsbCB0byBgcGFzdGUob2JqKWAgd291bGQgbW92ZSB0aGUgbm9kZSlcclxuXHRcdCAqIEBuYW1lIGN1dChvYmopXHJcblx0XHQgKiBAcGFyYW0gIHttaXhlZH0gb2JqIG11bHRpcGxlIG9iamVjdHMgY2FuIGJlIHBhc3NlZCB1c2luZyBhbiBhcnJheVxyXG5cdFx0ICogQHRyaWdnZXIgY3V0LmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRjdXQgOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdGlmKCFvYmopIHsgb2JqID0gdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLmNvbmNhdCgpOyB9XHJcblx0XHRcdGlmKCEkLmlzQXJyYXkob2JqKSkgeyBvYmogPSBbb2JqXTsgfVxyXG5cdFx0XHRpZighb2JqLmxlbmd0aCkgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0dmFyIHRtcCA9IFtdLCBvLCB0MSwgdDI7XHJcblx0XHRcdGZvcih0MSA9IDAsIHQyID0gb2JqLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdG8gPSB0aGlzLmdldF9ub2RlKG9ialt0MV0pO1xyXG5cdFx0XHRcdGlmKG8gJiYgby5pZCAmJiBvLmlkICE9PSAnIycpIHsgdG1wLnB1c2gobyk7IH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZighdG1wLmxlbmd0aCkgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0Y2NwX25vZGUgPSB0bXA7XHJcblx0XHRcdGNjcF9pbnN0ID0gdGhpcztcclxuXHRcdFx0Y2NwX21vZGUgPSAnbW92ZV9ub2RlJztcclxuXHRcdFx0LyoqXHJcblx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIG5vZGVzIGFyZSBhZGRlZCB0byB0aGUgYnVmZmVyIGZvciBtb3ZpbmdcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIGN1dC5qc3RyZWVcclxuXHRcdFx0ICogQHBhcmFtIHtBcnJheX0gbm9kZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdjdXQnLCB7IFwibm9kZVwiIDogb2JqIH0pO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY29weSBhIG5vZGUgKGEgbGF0ZXIgY2FsbCB0byBgcGFzdGUob2JqKWAgd291bGQgY29weSB0aGUgbm9kZSlcclxuXHRcdCAqIEBuYW1lIGNvcHkob2JqKVxyXG5cdFx0ICogQHBhcmFtICB7bWl4ZWR9IG9iaiBtdWx0aXBsZSBvYmplY3RzIGNhbiBiZSBwYXNzZWQgdXNpbmcgYW4gYXJyYXlcclxuXHRcdCAqIEB0cmlnZ2VyIGNvcHkuanN0cmVcclxuXHRcdCAqL1xyXG5cdFx0Y29weSA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0aWYoIW9iaikgeyBvYmogPSB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQuY29uY2F0KCk7IH1cclxuXHRcdFx0aWYoISQuaXNBcnJheShvYmopKSB7IG9iaiA9IFtvYmpdOyB9XHJcblx0XHRcdGlmKCFvYmoubGVuZ3RoKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHR2YXIgdG1wID0gW10sIG8sIHQxLCB0MjtcclxuXHRcdFx0Zm9yKHQxID0gMCwgdDIgPSBvYmoubGVuZ3RoOyB0MSA8IHQyOyB0MSsrKSB7XHJcblx0XHRcdFx0byA9IHRoaXMuZ2V0X25vZGUob2JqW3QxXSk7XHJcblx0XHRcdFx0aWYobyAmJiBvLmlkICYmIG8uaWQgIT09ICcjJykgeyB0bXAucHVzaChvKTsgfVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmKCF0bXAubGVuZ3RoKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRjY3Bfbm9kZSA9IHRtcDtcclxuXHRcdFx0Y2NwX2luc3QgPSB0aGlzO1xyXG5cdFx0XHRjY3BfbW9kZSA9ICdjb3B5X25vZGUnO1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gbm9kZXMgYXJlIGFkZGVkIHRvIHRoZSBidWZmZXIgZm9yIGNvcHlpbmdcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIGNvcHkuanN0cmVlXHJcblx0XHRcdCAqIEBwYXJhbSB7QXJyYXl9IG5vZGVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignY29weScsIHsgXCJub2RlXCIgOiBvYmogfSk7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBnZXQgdGhlIGN1cnJlbnQgYnVmZmVyIChhbnkgbm9kZXMgdGhhdCBhcmUgd2FpdGluZyBmb3IgYSBwYXN0ZSBvcGVyYXRpb24pXHJcblx0XHQgKiBAbmFtZSBnZXRfYnVmZmVyKClcclxuXHRcdCAqIEByZXR1cm4ge09iamVjdH0gYW4gb2JqZWN0IGNvbnNpc3Rpbmcgb2YgYG1vZGVgIChcImNvcHlfbm9kZVwiIG9yIFwibW92ZV9ub2RlXCIpLCBgbm9kZWAgKGFuIGFycmF5IG9mIG9iamVjdHMpIGFuZCBgaW5zdGAgKHRoZSBpbnN0YW5jZSlcclxuXHRcdCAqL1xyXG5cdFx0Z2V0X2J1ZmZlciA6IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0cmV0dXJuIHsgJ21vZGUnIDogY2NwX21vZGUsICdub2RlJyA6IGNjcF9ub2RlLCAnaW5zdCcgOiBjY3BfaW5zdCB9O1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogY2hlY2sgaWYgdGhlcmUgaXMgc29tZXRoaW5nIGluIHRoZSBidWZmZXIgdG8gcGFzdGVcclxuXHRcdCAqIEBuYW1lIGNhbl9wYXN0ZSgpXHJcblx0XHQgKiBAcmV0dXJuIHtCb29sZWFufVxyXG5cdFx0ICovXHJcblx0XHRjYW5fcGFzdGUgOiBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHJldHVybiBjY3BfbW9kZSAhPT0gZmFsc2UgJiYgY2NwX25vZGUgIT09IGZhbHNlOyAvLyAmJiBjY3BfaW5zdC5fbW9kZWwuZGF0YVtjY3Bfbm9kZV07XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBjb3B5IG9yIG1vdmUgdGhlIHByZXZpb3VzbHkgY3V0IG9yIGNvcGllZCBub2RlcyB0byBhIG5ldyBwYXJlbnRcclxuXHRcdCAqIEBuYW1lIHBhc3RlKG9iailcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmogdGhlIG5ldyBwYXJlbnRcclxuXHRcdCAqIEB0cmlnZ2VyIHBhc3RlLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRwYXN0ZSA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZighb2JqIHx8ICFjY3BfbW9kZSB8fCAhY2NwX21vZGUubWF0Y2goL14oY29weV9ub2RlfG1vdmVfbm9kZSkkLykgfHwgIWNjcF9ub2RlKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRpZih0aGlzW2NjcF9tb2RlXShjY3Bfbm9kZSwgb2JqKSkge1xyXG5cdFx0XHRcdC8qKlxyXG5cdFx0XHRcdCAqIHRyaWdnZXJlZCB3aGVuIHBhc3RlIGlzIGludm9rZWRcclxuXHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHQgKiBAbmFtZSBwYXN0ZS5qc3RyZWVcclxuXHRcdFx0XHQgKiBAcGFyYW0ge1N0cmluZ30gcGFyZW50IHRoZSBJRCBvZiB0aGUgcmVjZWl2aW5nIG5vZGVcclxuXHRcdFx0XHQgKiBAcGFyYW0ge0FycmF5fSBub2RlIHRoZSBub2RlcyBpbiB0aGUgYnVmZmVyXHJcblx0XHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IG1vZGUgdGhlIHBlcmZvcm1lZCBvcGVyYXRpb24gLSBcImNvcHlfbm9kZVwiIG9yIFwibW92ZV9ub2RlXCJcclxuXHRcdFx0XHQgKi9cclxuXHRcdFx0XHR0aGlzLnRyaWdnZXIoJ3Bhc3RlJywgeyBcInBhcmVudFwiIDogb2JqLmlkLCBcIm5vZGVcIiA6IGNjcF9ub2RlLCBcIm1vZGVcIiA6IGNjcF9tb2RlIH0pO1xyXG5cdFx0XHR9XHJcblx0XHRcdGNjcF9ub2RlID0gZmFsc2U7XHJcblx0XHRcdGNjcF9tb2RlID0gZmFsc2U7XHJcblx0XHRcdGNjcF9pbnN0ID0gZmFsc2U7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBwdXQgYSBub2RlIGluIGVkaXQgbW9kZSAoaW5wdXQgZmllbGQgdG8gcmVuYW1lIHRoZSBub2RlKVxyXG5cdFx0ICogQG5hbWUgZWRpdChvYmogWywgZGVmYXVsdF90ZXh0XSlcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmpcclxuXHRcdCAqIEBwYXJhbSAge1N0cmluZ30gZGVmYXVsdF90ZXh0IHRoZSB0ZXh0IHRvIHBvcHVsYXRlIHRoZSBpbnB1dCB3aXRoIChpZiBvbWl0dGVkIHRoZSBub2RlIHRleHQgdmFsdWUgaXMgdXNlZClcclxuXHRcdCAqL1xyXG5cdFx0ZWRpdCA6IGZ1bmN0aW9uIChvYmosIGRlZmF1bHRfdGV4dCkge1xyXG5cdFx0XHRvYmogPSB0aGlzLl9vcGVuX3RvKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgIW9iai5sZW5ndGgpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdHZhciBydGwgPSB0aGlzLl9kYXRhLmNvcmUucnRsLFxyXG5cdFx0XHRcdHcgID0gdGhpcy5lbGVtZW50LndpZHRoKCksXHJcblx0XHRcdFx0YSAgPSBvYmouY2hpbGRyZW4oJy5qc3RyZWUtYW5jaG9yJyksXHJcblx0XHRcdFx0cyAgPSAkKCc8c3Bhbj4nKSxcclxuXHRcdFx0XHQvKiFcclxuXHRcdFx0XHRvaSA9IG9iai5jaGlsZHJlbihcImk6dmlzaWJsZVwiKSxcclxuXHRcdFx0XHRhaSA9IGEuY2hpbGRyZW4oXCJpOnZpc2libGVcIiksXHJcblx0XHRcdFx0dzEgPSBvaS53aWR0aCgpICogb2kubGVuZ3RoLFxyXG5cdFx0XHRcdHcyID0gYWkud2lkdGgoKSAqIGFpLmxlbmd0aCxcclxuXHRcdFx0XHQqL1xyXG5cdFx0XHRcdHQgID0gdHlwZW9mIGRlZmF1bHRfdGV4dCA9PT0gJ3N0cmluZycgPyBkZWZhdWx0X3RleHQgOiB0aGlzLmdldF90ZXh0KG9iaiksXHJcblx0XHRcdFx0aDEgPSAkKFwiPFwiK1wiZGl2IC8+XCIsIHsgY3NzIDogeyBcInBvc2l0aW9uXCIgOiBcImFic29sdXRlXCIsIFwidG9wXCIgOiBcIi0yMDBweFwiLCBcImxlZnRcIiA6IChydGwgPyBcIjBweFwiIDogXCItMTAwMHB4XCIpLCBcInZpc2liaWxpdHlcIiA6IFwiaGlkZGVuXCIgfSB9KS5hcHBlbmRUbyhcImJvZHlcIiksXHJcblx0XHRcdFx0aDIgPSAkKFwiPFwiK1wiaW5wdXQgLz5cIiwge1xyXG5cdFx0XHRcdFx0XHRcInZhbHVlXCIgOiB0LFxyXG5cdFx0XHRcdFx0XHRcImNsYXNzXCIgOiBcImpzdHJlZS1yZW5hbWUtaW5wdXRcIixcclxuXHRcdFx0XHRcdFx0Ly8gXCJzaXplXCIgOiB0Lmxlbmd0aCxcclxuXHRcdFx0XHRcdFx0XCJjc3NcIiA6IHtcclxuXHRcdFx0XHRcdFx0XHRcInBhZGRpbmdcIiA6IFwiMFwiLFxyXG5cdFx0XHRcdFx0XHRcdFwiYm9yZGVyXCIgOiBcIjFweCBzb2xpZCBzaWx2ZXJcIixcclxuXHRcdFx0XHRcdFx0XHRcImJveC1zaXppbmdcIiA6IFwiYm9yZGVyLWJveFwiLFxyXG5cdFx0XHRcdFx0XHRcdFwiZGlzcGxheVwiIDogXCJpbmxpbmUtYmxvY2tcIixcclxuXHRcdFx0XHRcdFx0XHRcImhlaWdodFwiIDogKHRoaXMuX2RhdGEuY29yZS5saV9oZWlnaHQpICsgXCJweFwiLFxyXG5cdFx0XHRcdFx0XHRcdFwibGluZUhlaWdodFwiIDogKHRoaXMuX2RhdGEuY29yZS5saV9oZWlnaHQpICsgXCJweFwiLFxyXG5cdFx0XHRcdFx0XHRcdFwid2lkdGhcIiA6IFwiMTUwcHhcIiAvLyB3aWxsIGJlIHNldCBhIGJpdCBmdXJ0aGVyIGRvd25cclxuXHRcdFx0XHRcdFx0fSxcclxuXHRcdFx0XHRcdFx0XCJibHVyXCIgOiAkLnByb3h5KGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdFx0XHR2YXIgaSA9IHMuY2hpbGRyZW4oXCIuanN0cmVlLXJlbmFtZS1pbnB1dFwiKSxcclxuXHRcdFx0XHRcdFx0XHRcdHYgPSBpLnZhbCgpO1xyXG5cdFx0XHRcdFx0XHRcdGlmKHYgPT09IFwiXCIpIHsgdiA9IHQ7IH1cclxuXHRcdFx0XHRcdFx0XHRoMS5yZW1vdmUoKTtcclxuXHRcdFx0XHRcdFx0XHRzLnJlcGxhY2VXaXRoKGEpO1xyXG5cdFx0XHRcdFx0XHRcdHMucmVtb3ZlKCk7XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5zZXRfdGV4dChvYmosIHQpO1xyXG5cdFx0XHRcdFx0XHRcdGlmKHRoaXMucmVuYW1lX25vZGUob2JqLCB2KSA9PT0gZmFsc2UpIHtcclxuXHRcdFx0XHRcdFx0XHRcdHRoaXMuc2V0X3RleHQob2JqLCB0KTsgLy8gbW92ZSB0aGlzIHVwPyBhbmQgZml4ICM0ODNcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH0sIHRoaXMpLFxyXG5cdFx0XHRcdFx0XHRcImtleWRvd25cIiA6IGZ1bmN0aW9uIChldmVudCkge1xyXG5cdFx0XHRcdFx0XHRcdHZhciBrZXkgPSBldmVudC53aGljaDtcclxuXHRcdFx0XHRcdFx0XHRpZihrZXkgPT09IDI3KSB7XHJcblx0XHRcdFx0XHRcdFx0XHR0aGlzLnZhbHVlID0gdDtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0aWYoa2V5ID09PSAyNyB8fCBrZXkgPT09IDEzIHx8IGtleSA9PT0gMzcgfHwga2V5ID09PSAzOCB8fCBrZXkgPT09IDM5IHx8IGtleSA9PT0gNDAgfHwga2V5ID09PSAzMikge1xyXG5cdFx0XHRcdFx0XHRcdFx0ZXZlbnQuc3RvcEltbWVkaWF0ZVByb3BhZ2F0aW9uKCk7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdGlmKGtleSA9PT0gMjcgfHwga2V5ID09PSAxMykge1xyXG5cdFx0XHRcdFx0XHRcdFx0ZXZlbnQucHJldmVudERlZmF1bHQoKTtcclxuXHRcdFx0XHRcdFx0XHRcdHRoaXMuYmx1cigpO1xyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0fSxcclxuXHRcdFx0XHRcdFx0XCJjbGlja1wiIDogZnVuY3Rpb24gKGUpIHsgZS5zdG9wSW1tZWRpYXRlUHJvcGFnYXRpb24oKTsgfSxcclxuXHRcdFx0XHRcdFx0XCJtb3VzZWRvd25cIiA6IGZ1bmN0aW9uIChlKSB7IGUuc3RvcEltbWVkaWF0ZVByb3BhZ2F0aW9uKCk7IH0sXHJcblx0XHRcdFx0XHRcdFwia2V5dXBcIiA6IGZ1bmN0aW9uIChldmVudCkge1xyXG5cdFx0XHRcdFx0XHRcdGgyLndpZHRoKE1hdGgubWluKGgxLnRleHQoXCJwV1wiICsgdGhpcy52YWx1ZSkud2lkdGgoKSx3KSk7XHJcblx0XHRcdFx0XHRcdH0sXHJcblx0XHRcdFx0XHRcdFwia2V5cHJlc3NcIiA6IGZ1bmN0aW9uKGV2ZW50KSB7XHJcblx0XHRcdFx0XHRcdFx0aWYoZXZlbnQud2hpY2ggPT09IDEzKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9KSxcclxuXHRcdFx0XHRmbiA9IHtcclxuXHRcdFx0XHRcdFx0Zm9udEZhbWlseVx0XHQ6IGEuY3NzKCdmb250RmFtaWx5JylcdFx0fHwgJycsXHJcblx0XHRcdFx0XHRcdGZvbnRTaXplXHRcdDogYS5jc3MoJ2ZvbnRTaXplJylcdFx0XHR8fCAnJyxcclxuXHRcdFx0XHRcdFx0Zm9udFdlaWdodFx0XHQ6IGEuY3NzKCdmb250V2VpZ2h0JylcdFx0fHwgJycsXHJcblx0XHRcdFx0XHRcdGZvbnRTdHlsZVx0XHQ6IGEuY3NzKCdmb250U3R5bGUnKVx0XHR8fCAnJyxcclxuXHRcdFx0XHRcdFx0Zm9udFN0cmV0Y2hcdFx0OiBhLmNzcygnZm9udFN0cmV0Y2gnKVx0XHR8fCAnJyxcclxuXHRcdFx0XHRcdFx0Zm9udFZhcmlhbnRcdFx0OiBhLmNzcygnZm9udFZhcmlhbnQnKVx0XHR8fCAnJyxcclxuXHRcdFx0XHRcdFx0bGV0dGVyU3BhY2luZ1x0OiBhLmNzcygnbGV0dGVyU3BhY2luZycpXHR8fCAnJyxcclxuXHRcdFx0XHRcdFx0d29yZFNwYWNpbmdcdFx0OiBhLmNzcygnd29yZFNwYWNpbmcnKVx0XHR8fCAnJ1xyXG5cdFx0XHRcdH07XHJcblx0XHRcdHRoaXMuc2V0X3RleHQob2JqLCBcIlwiKTtcclxuXHRcdFx0cy5hdHRyKCdjbGFzcycsIGEuYXR0cignY2xhc3MnKSkuYXBwZW5kKGEuY29udGVudHMoKS5jbG9uZSgpKS5hcHBlbmQoaDIpO1xyXG5cdFx0XHRhLnJlcGxhY2VXaXRoKHMpO1xyXG5cdFx0XHRoMS5jc3MoZm4pO1xyXG5cdFx0XHRoMi5jc3MoZm4pLndpZHRoKE1hdGgubWluKGgxLnRleHQoXCJwV1wiICsgaDJbMF0udmFsdWUpLndpZHRoKCksdykpWzBdLnNlbGVjdCgpO1xyXG5cdFx0fSxcclxuXHJcblxyXG5cdFx0LyoqXHJcblx0XHQgKiBjaGFuZ2VzIHRoZSB0aGVtZVxyXG5cdFx0ICogQG5hbWUgc2V0X3RoZW1lKHRoZW1lX25hbWUgWywgdGhlbWVfdXJsXSlcclxuXHRcdCAqIEBwYXJhbSB7U3RyaW5nfSB0aGVtZV9uYW1lIHRoZSBuYW1lIG9mIHRoZSBuZXcgdGhlbWUgdG8gYXBwbHlcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IHRoZW1lX3VybCAgdGhlIGxvY2F0aW9uIG9mIHRoZSBDU1MgZmlsZSBmb3IgdGhpcyB0aGVtZS4gT21pdCBvciBzZXQgdG8gYGZhbHNlYCBpZiB5b3UgbWFudWFsbHkgaW5jbHVkZWQgdGhlIGZpbGUuIFNldCB0byBgdHJ1ZWAgdG8gYXV0b2xvYWQgZnJvbSB0aGUgYGNvcmUudGhlbWVzLmRpcmAgZGlyZWN0b3J5LlxyXG5cdFx0ICogQHRyaWdnZXIgc2V0X3RoZW1lLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHRzZXRfdGhlbWUgOiBmdW5jdGlvbiAodGhlbWVfbmFtZSwgdGhlbWVfdXJsKSB7XHJcblx0XHRcdGlmKCF0aGVtZV9uYW1lKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRpZih0aGVtZV91cmwgPT09IHRydWUpIHtcclxuXHRcdFx0XHR2YXIgZGlyID0gdGhpcy5zZXR0aW5ncy5jb3JlLnRoZW1lcy5kaXI7XHJcblx0XHRcdFx0aWYoIWRpcikgeyBkaXIgPSAkLmpzdHJlZS5wYXRoICsgJy90aGVtZXMnOyB9XHJcblx0XHRcdFx0dGhlbWVfdXJsID0gZGlyICsgJy8nICsgdGhlbWVfbmFtZSArICcvc3R5bGUuY3NzJztcclxuXHRcdFx0fVxyXG5cdFx0XHRpZih0aGVtZV91cmwgJiYgJC5pbkFycmF5KHRoZW1lX3VybCwgdGhlbWVzX2xvYWRlZCkgPT09IC0xKSB7XHJcblx0XHRcdFx0JCgnaGVhZCcpLmFwcGVuZCgnPCcrJ2xpbmsgcmVsPVwic3R5bGVzaGVldFwiIGhyZWY9XCInICsgdGhlbWVfdXJsICsgJ1wiIHR5cGU9XCJ0ZXh0L2Nzc1wiIC8+Jyk7XHJcblx0XHRcdFx0dGhlbWVzX2xvYWRlZC5wdXNoKHRoZW1lX3VybCk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYodGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5uYW1lKSB7XHJcblx0XHRcdFx0dGhpcy5lbGVtZW50LnJlbW92ZUNsYXNzKCdqc3RyZWUtJyArIHRoaXMuX2RhdGEuY29yZS50aGVtZXMubmFtZSk7XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5uYW1lID0gdGhlbWVfbmFtZTtcclxuXHRcdFx0dGhpcy5lbGVtZW50LmFkZENsYXNzKCdqc3RyZWUtJyArIHRoZW1lX25hbWUpO1xyXG5cdFx0XHR0aGlzLmVsZW1lbnRbdGhpcy5zZXR0aW5ncy5jb3JlLnRoZW1lcy5yZXNwb25zaXZlID8gJ2FkZENsYXNzJyA6ICdyZW1vdmVDbGFzcycgXSgnanN0cmVlLScgKyB0aGVtZV9uYW1lICsgJy1yZXNwb25zaXZlJyk7XHJcblx0XHRcdC8qKlxyXG5cdFx0XHQgKiB0cmlnZ2VyZWQgd2hlbiBhIHRoZW1lIGlzIHNldFxyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgc2V0X3RoZW1lLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge1N0cmluZ30gdGhlbWUgdGhlIG5ldyB0aGVtZVxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdzZXRfdGhlbWUnLCB7ICd0aGVtZScgOiB0aGVtZV9uYW1lIH0pO1xyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0cyB0aGUgbmFtZSBvZiB0aGUgY3VycmVudGx5IGFwcGxpZWQgdGhlbWUgbmFtZVxyXG5cdFx0ICogQG5hbWUgZ2V0X3RoZW1lKClcclxuXHRcdCAqIEByZXR1cm4ge1N0cmluZ31cclxuXHRcdCAqL1xyXG5cdFx0Z2V0X3RoZW1lIDogZnVuY3Rpb24gKCkgeyByZXR1cm4gdGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5uYW1lOyB9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBjaGFuZ2VzIHRoZSB0aGVtZSB2YXJpYW50IChpZiB0aGUgdGhlbWUgaGFzIHZhcmlhbnRzKVxyXG5cdFx0ICogQG5hbWUgc2V0X3RoZW1lX3ZhcmlhbnQodmFyaWFudF9uYW1lKVxyXG5cdFx0ICogQHBhcmFtIHtTdHJpbmd8Qm9vbGVhbn0gdmFyaWFudF9uYW1lIHRoZSB2YXJpYW50IHRvIGFwcGx5IChpZiBgZmFsc2VgIGlzIHVzZWQgdGhlIGN1cnJlbnQgdmFyaWFudCBpcyByZW1vdmVkKVxyXG5cdFx0ICovXHJcblx0XHRzZXRfdGhlbWVfdmFyaWFudCA6IGZ1bmN0aW9uICh2YXJpYW50X25hbWUpIHtcclxuXHRcdFx0aWYodGhpcy5fZGF0YS5jb3JlLnRoZW1lcy52YXJpYW50KSB7XHJcblx0XHRcdFx0dGhpcy5lbGVtZW50LnJlbW92ZUNsYXNzKCdqc3RyZWUtJyArIHRoaXMuX2RhdGEuY29yZS50aGVtZXMubmFtZSArICctJyArIHRoaXMuX2RhdGEuY29yZS50aGVtZXMudmFyaWFudCk7XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5fZGF0YS5jb3JlLnRoZW1lcy52YXJpYW50ID0gdmFyaWFudF9uYW1lO1xyXG5cdFx0XHRpZih2YXJpYW50X25hbWUpIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnQuYWRkQ2xhc3MoJ2pzdHJlZS0nICsgdGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5uYW1lICsgJy0nICsgdGhpcy5fZGF0YS5jb3JlLnRoZW1lcy52YXJpYW50KTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHRcdC8qKlxyXG5cdFx0ICogZ2V0cyB0aGUgbmFtZSBvZiB0aGUgY3VycmVudGx5IGFwcGxpZWQgdGhlbWUgdmFyaWFudFxyXG5cdFx0ICogQG5hbWUgZ2V0X3RoZW1lKClcclxuXHRcdCAqIEByZXR1cm4ge1N0cmluZ31cclxuXHRcdCAqL1xyXG5cdFx0Z2V0X3RoZW1lX3ZhcmlhbnQgOiBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzLl9kYXRhLmNvcmUudGhlbWVzLnZhcmlhbnQ7IH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHNob3dzIGEgc3RyaXBlZCBiYWNrZ3JvdW5kIG9uIHRoZSBjb250YWluZXIgKGlmIHRoZSB0aGVtZSBzdXBwb3J0cyBpdClcclxuXHRcdCAqIEBuYW1lIHNob3dfc3RyaXBlcygpXHJcblx0XHQgKi9cclxuXHRcdHNob3dfc3RyaXBlcyA6IGZ1bmN0aW9uICgpIHsgdGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5zdHJpcGVzID0gdHJ1ZTsgdGhpcy5nZXRfY29udGFpbmVyX3VsKCkuYWRkQ2xhc3MoXCJqc3RyZWUtc3RyaXBlZFwiKTsgfSxcclxuXHRcdC8qKlxyXG5cdFx0ICogaGlkZXMgdGhlIHN0cmlwZWQgYmFja2dyb3VuZCBvbiB0aGUgY29udGFpbmVyXHJcblx0XHQgKiBAbmFtZSBoaWRlX3N0cmlwZXMoKVxyXG5cdFx0ICovXHJcblx0XHRoaWRlX3N0cmlwZXMgOiBmdW5jdGlvbiAoKSB7IHRoaXMuX2RhdGEuY29yZS50aGVtZXMuc3RyaXBlcyA9IGZhbHNlOyB0aGlzLmdldF9jb250YWluZXJfdWwoKS5yZW1vdmVDbGFzcyhcImpzdHJlZS1zdHJpcGVkXCIpOyB9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiB0b2dnbGVzIHRoZSBzdHJpcGVkIGJhY2tncm91bmQgb24gdGhlIGNvbnRhaW5lclxyXG5cdFx0ICogQG5hbWUgdG9nZ2xlX3N0cmlwZXMoKVxyXG5cdFx0ICovXHJcblx0XHR0b2dnbGVfc3RyaXBlcyA6IGZ1bmN0aW9uICgpIHsgaWYodGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5zdHJpcGVzKSB7IHRoaXMuaGlkZV9zdHJpcGVzKCk7IH0gZWxzZSB7IHRoaXMuc2hvd19zdHJpcGVzKCk7IH0gfSxcclxuXHRcdC8qKlxyXG5cdFx0ICogc2hvd3MgdGhlIGNvbm5lY3RpbmcgZG90cyAoaWYgdGhlIHRoZW1lIHN1cHBvcnRzIGl0KVxyXG5cdFx0ICogQG5hbWUgc2hvd19kb3RzKClcclxuXHRcdCAqL1xyXG5cdFx0c2hvd19kb3RzIDogZnVuY3Rpb24gKCkgeyB0aGlzLl9kYXRhLmNvcmUudGhlbWVzLmRvdHMgPSB0cnVlOyB0aGlzLmdldF9jb250YWluZXJfdWwoKS5yZW1vdmVDbGFzcyhcImpzdHJlZS1uby1kb3RzXCIpOyB9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBoaWRlcyB0aGUgY29ubmVjdGluZyBkb3RzXHJcblx0XHQgKiBAbmFtZSBoaWRlX2RvdHMoKVxyXG5cdFx0ICovXHJcblx0XHRoaWRlX2RvdHMgOiBmdW5jdGlvbiAoKSB7IHRoaXMuX2RhdGEuY29yZS50aGVtZXMuZG90cyA9IGZhbHNlOyB0aGlzLmdldF9jb250YWluZXJfdWwoKS5hZGRDbGFzcyhcImpzdHJlZS1uby1kb3RzXCIpOyB9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiB0b2dnbGVzIHRoZSBjb25uZWN0aW5nIGRvdHNcclxuXHRcdCAqIEBuYW1lIHRvZ2dsZV9kb3RzKClcclxuXHRcdCAqL1xyXG5cdFx0dG9nZ2xlX2RvdHMgOiBmdW5jdGlvbiAoKSB7IGlmKHRoaXMuX2RhdGEuY29yZS50aGVtZXMuZG90cykgeyB0aGlzLmhpZGVfZG90cygpOyB9IGVsc2UgeyB0aGlzLnNob3dfZG90cygpOyB9IH0sXHJcblx0XHQvKipcclxuXHRcdCAqIHNob3cgdGhlIG5vZGUgaWNvbnNcclxuXHRcdCAqIEBuYW1lIHNob3dfaWNvbnMoKVxyXG5cdFx0ICovXHJcblx0XHRzaG93X2ljb25zIDogZnVuY3Rpb24gKCkgeyB0aGlzLl9kYXRhLmNvcmUudGhlbWVzLmljb25zID0gdHJ1ZTsgdGhpcy5nZXRfY29udGFpbmVyX3VsKCkucmVtb3ZlQ2xhc3MoXCJqc3RyZWUtbm8taWNvbnNcIik7IH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGhpZGUgdGhlIG5vZGUgaWNvbnNcclxuXHRcdCAqIEBuYW1lIGhpZGVfaWNvbnMoKVxyXG5cdFx0ICovXHJcblx0XHRoaWRlX2ljb25zIDogZnVuY3Rpb24gKCkgeyB0aGlzLl9kYXRhLmNvcmUudGhlbWVzLmljb25zID0gZmFsc2U7IHRoaXMuZ2V0X2NvbnRhaW5lcl91bCgpLmFkZENsYXNzKFwianN0cmVlLW5vLWljb25zXCIpOyB9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiB0b2dnbGUgdGhlIG5vZGUgaWNvbnNcclxuXHRcdCAqIEBuYW1lIHRvZ2dsZV9pY29ucygpXHJcblx0XHQgKi9cclxuXHRcdHRvZ2dsZV9pY29ucyA6IGZ1bmN0aW9uICgpIHsgaWYodGhpcy5fZGF0YS5jb3JlLnRoZW1lcy5pY29ucykgeyB0aGlzLmhpZGVfaWNvbnMoKTsgfSBlbHNlIHsgdGhpcy5zaG93X2ljb25zKCk7IH0gfSxcclxuXHRcdC8qKlxyXG5cdFx0ICogc2V0IHRoZSBub2RlIGljb24gZm9yIGEgbm9kZVxyXG5cdFx0ICogQG5hbWUgc2V0X2ljb24ob2JqLCBpY29uKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqXHJcblx0XHQgKiBAcGFyYW0ge1N0cmluZ30gaWNvbiB0aGUgbmV3IGljb24gLSBjYW4gYmUgYSBwYXRoIHRvIGFuIGljb24gb3IgYSBjbGFzc05hbWUsIGlmIHVzaW5nIGFuIGltYWdlIHRoYXQgaXMgaW4gdGhlIGN1cnJlbnQgZGlyZWN0b3J5IHVzZSBhIGAuL2AgcHJlZml4LCBvdGhlcndpc2UgaXQgd2lsbCBiZSBkZXRlY3RlZCBhcyBhIGNsYXNzXHJcblx0XHQgKi9cclxuXHRcdHNldF9pY29uIDogZnVuY3Rpb24gKG9iaiwgaWNvbikge1xyXG5cdFx0XHR2YXIgdDEsIHQyLCBkb20sIG9sZDtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuc2V0X2ljb24ob2JqW3QxXSwgaWNvbik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHR9XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0aWYoIW9iaiB8fCBvYmouaWQgPT09ICcjJykgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0b2xkID0gb2JqLmljb247XHJcblx0XHRcdG9iai5pY29uID0gaWNvbjtcclxuXHRcdFx0ZG9tID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpLmNoaWxkcmVuKFwiLmpzdHJlZS1hbmNob3JcIikuY2hpbGRyZW4oXCIuanN0cmVlLXRoZW1laWNvblwiKTtcclxuXHRcdFx0aWYoaWNvbiA9PT0gZmFsc2UpIHtcclxuXHRcdFx0XHR0aGlzLmhpZGVfaWNvbihvYmopO1xyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2UgaWYoaWNvbiA9PT0gdHJ1ZSkge1xyXG5cdFx0XHRcdGRvbS5yZW1vdmVDbGFzcygnanN0cmVlLXRoZW1laWNvbi1jdXN0b20gJyArIG9sZCkuY3NzKFwiYmFja2dyb3VuZFwiLFwiXCIpLnJlbW92ZUF0dHIoXCJyZWxcIik7XHJcblx0XHRcdH1cclxuXHRcdFx0ZWxzZSBpZihpY29uLmluZGV4T2YoXCIvXCIpID09PSAtMSAmJiBpY29uLmluZGV4T2YoXCIuXCIpID09PSAtMSkge1xyXG5cdFx0XHRcdGRvbS5yZW1vdmVDbGFzcyhvbGQpLmNzcyhcImJhY2tncm91bmRcIixcIlwiKTtcclxuXHRcdFx0XHRkb20uYWRkQ2xhc3MoaWNvbiArICcganN0cmVlLXRoZW1laWNvbi1jdXN0b20nKS5hdHRyKFwicmVsXCIsaWNvbik7XHJcblx0XHRcdH1cclxuXHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0ZG9tLnJlbW92ZUNsYXNzKG9sZCkuY3NzKFwiYmFja2dyb3VuZFwiLFwiXCIpO1xyXG5cdFx0XHRcdGRvbS5hZGRDbGFzcygnanN0cmVlLXRoZW1laWNvbi1jdXN0b20nKS5jc3MoXCJiYWNrZ3JvdW5kXCIsIFwidXJsKCdcIiArIGljb24gKyBcIicpIGNlbnRlciBjZW50ZXIgbm8tcmVwZWF0XCIpLmF0dHIoXCJyZWxcIixpY29uKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGdldCB0aGUgbm9kZSBpY29uIGZvciBhIG5vZGVcclxuXHRcdCAqIEBuYW1lIGdldF9pY29uKG9iailcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9ialxyXG5cdFx0ICogQHJldHVybiB7U3RyaW5nfVxyXG5cdFx0ICovXHJcblx0XHRnZXRfaWNvbiA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRyZXR1cm4gKCFvYmogfHwgb2JqLmlkID09PSAnIycpID8gZmFsc2UgOiBvYmouaWNvbjtcclxuXHRcdH0sXHJcblx0XHQvKipcclxuXHRcdCAqIGhpZGUgdGhlIGljb24gb24gYW4gaW5kaXZpZHVhbCBub2RlXHJcblx0XHQgKiBAbmFtZSBoaWRlX2ljb24ob2JqKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqXHJcblx0XHQgKi9cclxuXHRcdGhpZGVfaWNvbiA6IGZ1bmN0aW9uIChvYmopIHtcclxuXHRcdFx0dmFyIHQxLCB0MjtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuaGlkZV9pY29uKG9ialt0MV0pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqID09PSAnIycpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdG9iai5pY29uID0gZmFsc2U7XHJcblx0XHRcdHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKS5jaGlsZHJlbihcImFcIikuY2hpbGRyZW4oXCIuanN0cmVlLXRoZW1laWNvblwiKS5hZGRDbGFzcygnanN0cmVlLXRoZW1laWNvbi1oaWRkZW4nKTtcclxuXHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHR9LFxyXG5cdFx0LyoqXHJcblx0XHQgKiBzaG93IHRoZSBpY29uIG9uIGFuIGluZGl2aWR1YWwgbm9kZVxyXG5cdFx0ICogQG5hbWUgc2hvd19pY29uKG9iailcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9ialxyXG5cdFx0ICovXHJcblx0XHRzaG93X2ljb24gOiBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdHZhciB0MSwgdDIsIGRvbTtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuc2hvd19pY29uKG9ialt0MV0pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqID09PSAnIycpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdGRvbSA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKTtcclxuXHRcdFx0b2JqLmljb24gPSBkb20ubGVuZ3RoID8gZG9tLmNoaWxkcmVuKFwiYVwiKS5jaGlsZHJlbihcIi5qc3RyZWUtdGhlbWVpY29uXCIpLmF0dHIoJ3JlbCcpIDogdHJ1ZTtcclxuXHRcdFx0aWYoIW9iai5pY29uKSB7IG9iai5pY29uID0gdHJ1ZTsgfVxyXG5cdFx0XHRkb20uY2hpbGRyZW4oXCJhXCIpLmNoaWxkcmVuKFwiLmpzdHJlZS10aGVtZWljb25cIikucmVtb3ZlQ2xhc3MoJ2pzdHJlZS10aGVtZWljb24taGlkZGVuJyk7XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fVxyXG5cdH07XHJcblxyXG5cdC8vIGhlbHBlcnNcclxuXHQkLnZha2F0YSA9IHt9O1xyXG5cdC8vIHJldmVyc2VcclxuXHQkLmZuLnZha2F0YV9yZXZlcnNlID0gW10ucmV2ZXJzZTtcclxuXHQvLyBjb2xsZWN0IGF0dHJpYnV0ZXNcclxuXHQkLnZha2F0YS5hdHRyaWJ1dGVzID0gZnVuY3Rpb24obm9kZSwgd2l0aF92YWx1ZXMpIHtcclxuXHRcdG5vZGUgPSAkKG5vZGUpWzBdO1xyXG5cdFx0dmFyIGF0dHIgPSB3aXRoX3ZhbHVlcyA/IHt9IDogW107XHJcblx0XHRpZihub2RlICYmIG5vZGUuYXR0cmlidXRlcykge1xyXG5cdFx0XHQkLmVhY2gobm9kZS5hdHRyaWJ1dGVzLCBmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdGlmKCQuaW5BcnJheSh2Lm5vZGVOYW1lLnRvTG93ZXJDYXNlKCksWydzdHlsZScsJ2NvbnRlbnRlZGl0YWJsZScsJ2hhc2ZvY3VzJywndGFiaW5kZXgnXSkgIT09IC0xKSB7IHJldHVybjsgfVxyXG5cdFx0XHRcdGlmKHYubm9kZVZhbHVlICE9PSBudWxsICYmICQudHJpbSh2Lm5vZGVWYWx1ZSkgIT09ICcnKSB7XHJcblx0XHRcdFx0XHRpZih3aXRoX3ZhbHVlcykgeyBhdHRyW3Yubm9kZU5hbWVdID0gdi5ub2RlVmFsdWU7IH1cclxuXHRcdFx0XHRcdGVsc2UgeyBhdHRyLnB1c2godi5ub2RlTmFtZSk7IH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cdFx0fVxyXG5cdFx0cmV0dXJuIGF0dHI7XHJcblx0fTtcclxuXHQkLnZha2F0YS5hcnJheV91bmlxdWUgPSBmdW5jdGlvbihhcnJheSkge1xyXG5cdFx0dmFyIGEgPSBbXSwgaSwgaiwgbDtcclxuXHRcdGZvcihpID0gMCwgbCA9IGFycmF5Lmxlbmd0aDsgaSA8IGw7IGkrKykge1xyXG5cdFx0XHRmb3IoaiA9IDA7IGogPD0gaTsgaisrKSB7XHJcblx0XHRcdFx0aWYoYXJyYXlbaV0gPT09IGFycmF5W2pdKSB7XHJcblx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoaiA9PT0gaSkgeyBhLnB1c2goYXJyYXlbaV0pOyB9XHJcblx0XHR9XHJcblx0XHRyZXR1cm4gYTtcclxuXHR9O1xyXG5cdC8vIHJlbW92ZSBpdGVtIGZyb20gYXJyYXlcclxuXHQkLnZha2F0YS5hcnJheV9yZW1vdmUgPSBmdW5jdGlvbihhcnJheSwgZnJvbSwgdG8pIHtcclxuXHRcdHZhciByZXN0ID0gYXJyYXkuc2xpY2UoKHRvIHx8IGZyb20pICsgMSB8fCBhcnJheS5sZW5ndGgpO1xyXG5cdFx0YXJyYXkubGVuZ3RoID0gZnJvbSA8IDAgPyBhcnJheS5sZW5ndGggKyBmcm9tIDogZnJvbTtcclxuXHRcdGFycmF5LnB1c2guYXBwbHkoYXJyYXksIHJlc3QpO1xyXG5cdFx0cmV0dXJuIGFycmF5O1xyXG5cdH07XHJcblx0Ly8gcmVtb3ZlIGl0ZW0gZnJvbSBhcnJheVxyXG5cdCQudmFrYXRhLmFycmF5X3JlbW92ZV9pdGVtID0gZnVuY3Rpb24oYXJyYXksIGl0ZW0pIHtcclxuXHRcdHZhciB0bXAgPSAkLmluQXJyYXkoaXRlbSwgYXJyYXkpO1xyXG5cdFx0cmV0dXJuIHRtcCAhPT0gLTEgPyAkLnZha2F0YS5hcnJheV9yZW1vdmUoYXJyYXksIHRtcCkgOiBhcnJheTtcclxuXHR9O1xyXG5cdC8vIGJyb3dzZXIgc25pZmZpbmdcclxuXHQoZnVuY3Rpb24gKCkge1xyXG5cdFx0dmFyIGJyb3dzZXIgPSB7fSxcclxuXHRcdFx0Yl9tYXRjaCA9IGZ1bmN0aW9uKHVhKSB7XHJcblx0XHRcdHVhID0gdWEudG9Mb3dlckNhc2UoKTtcclxuXHJcblx0XHRcdHZhciBtYXRjaCA9XHQvKGNocm9tZSlbIFxcL10oW1xcdy5dKykvLmV4ZWMoIHVhICkgfHxcclxuXHRcdFx0XHRcdFx0Lyh3ZWJraXQpWyBcXC9dKFtcXHcuXSspLy5leGVjKCB1YSApIHx8XHJcblx0XHRcdFx0XHRcdC8ob3BlcmEpKD86Lip2ZXJzaW9ufClbIFxcL10oW1xcdy5dKykvLmV4ZWMoIHVhICkgfHxcclxuXHRcdFx0XHRcdFx0Lyhtc2llKSAoW1xcdy5dKykvLmV4ZWMoIHVhICkgfHxcclxuXHRcdFx0XHRcdFx0KHVhLmluZGV4T2YoXCJjb21wYXRpYmxlXCIpIDwgMCAmJiAvKG1vemlsbGEpKD86Lio/IHJ2OihbXFx3Ll0rKXwpLy5leGVjKCB1YSApKSB8fFxyXG5cdFx0XHRcdFx0XHRbXTtcclxuXHRcdFx0XHRyZXR1cm4ge1xyXG5cdFx0XHRcdFx0YnJvd3NlcjogbWF0Y2hbMV0gfHwgXCJcIixcclxuXHRcdFx0XHRcdHZlcnNpb246IG1hdGNoWzJdIHx8IFwiMFwiXHJcblx0XHRcdFx0fTtcclxuXHRcdFx0fSxcclxuXHRcdFx0bWF0Y2hlZCA9IGJfbWF0Y2god2luZG93Lm5hdmlnYXRvci51c2VyQWdlbnQpO1xyXG5cdFx0aWYobWF0Y2hlZC5icm93c2VyKSB7XHJcblx0XHRcdGJyb3dzZXJbIG1hdGNoZWQuYnJvd3NlciBdID0gdHJ1ZTtcclxuXHRcdFx0YnJvd3Nlci52ZXJzaW9uID0gbWF0Y2hlZC52ZXJzaW9uO1xyXG5cdFx0fVxyXG5cdFx0aWYoYnJvd3Nlci5jaHJvbWUpIHtcclxuXHRcdFx0YnJvd3Nlci53ZWJraXQgPSB0cnVlO1xyXG5cdFx0fVxyXG5cdFx0ZWxzZSBpZihicm93c2VyLndlYmtpdCkge1xyXG5cdFx0XHRicm93c2VyLnNhZmFyaSA9IHRydWU7XHJcblx0XHR9XHJcblx0XHQkLnZha2F0YS5icm93c2VyID0gYnJvd3NlcjtcclxuXHR9KCkpO1xyXG5cdGlmKCQudmFrYXRhLmJyb3dzZXIubXNpZSAmJiAkLnZha2F0YS5icm93c2VyLnZlcnNpb24gPCA4KSB7XHJcblx0XHQkLmpzdHJlZS5kZWZhdWx0cy5jb3JlLmFuaW1hdGlvbiA9IDA7XHJcblx0fVxyXG5cclxuLyoqXHJcbiAqICMjIyBDaGVja2JveCBwbHVnaW5cclxuICpcclxuICogVGhpcyBwbHVnaW4gcmVuZGVycyBjaGVja2JveCBpY29ucyBpbiBmcm9udCBvZiBlYWNoIG5vZGUsIG1ha2luZyBtdWx0aXBsZSBzZWxlY3Rpb24gbXVjaCBlYXNpZXIuIFxyXG4gKiBJdCBhbHNvIHN1cHBvcnRzIHRyaS1zdGF0ZSBiZWhhdmlvciwgbWVhbmluZyB0aGF0IGlmIGEgbm9kZSBoYXMgYSBmZXcgb2YgaXRzIGNoaWxkcmVuIGNoZWNrZWQgaXQgd2lsbCBiZSByZW5kZXJlZCBhcyB1bmRldGVybWluZWQsIGFuZCBzdGF0ZSB3aWxsIGJlIHByb3BhZ2F0ZWQgdXAuXHJcbiAqL1xyXG5cclxuXHR2YXIgX2kgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdJJyk7XHJcblx0X2kuY2xhc3NOYW1lID0gJ2pzdHJlZS1pY29uIGpzdHJlZS1jaGVja2JveCc7XHJcblx0LyoqXHJcblx0ICogc3RvcmVzIGFsbCBkZWZhdWx0cyBmb3IgdGhlIGNoZWNrYm94IHBsdWdpblxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNoZWNrYm94XHJcblx0ICogQHBsdWdpbiBjaGVja2JveFxyXG5cdCAqL1xyXG5cdCQuanN0cmVlLmRlZmF1bHRzLmNoZWNrYm94ID0ge1xyXG5cdFx0LyoqXHJcblx0XHQgKiBhIGJvb2xlYW4gaW5kaWNhdGluZyBpZiBjaGVja2JveGVzIHNob3VsZCBiZSB2aXNpYmxlIChjYW4gYmUgY2hhbmdlZCBhdCBhIGxhdGVyIHRpbWUgdXNpbmcgYHNob3dfY2hlY2tib3hlcygpYCBhbmQgYGhpZGVfY2hlY2tib3hlc2ApLiBEZWZhdWx0cyB0byBgdHJ1ZWAuXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jaGVja2JveC52aXNpYmxlXHJcblx0XHQgKiBAcGx1Z2luIGNoZWNrYm94XHJcblx0XHQgKi9cclxuXHRcdHZpc2libGVcdFx0XHRcdDogdHJ1ZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogYSBib29sZWFuIGluZGljYXRpbmcgaWYgY2hlY2tib3hlcyBzaG91bGQgY2FzY2FkZSBkb3duIGFuZCBoYXZlIGFuIHVuZGV0ZXJtaW5lZCBzdGF0ZS4gRGVmYXVsdHMgdG8gYHRydWVgLlxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuY2hlY2tib3gudGhyZWVfc3RhdGVcclxuXHRcdCAqIEBwbHVnaW4gY2hlY2tib3hcclxuXHRcdCAqL1xyXG5cdFx0dGhyZWVfc3RhdGVcdFx0XHQ6IHRydWUsXHJcblx0XHQvKipcclxuXHRcdCAqIGEgYm9vbGVhbiBpbmRpY2F0aW5nIGlmIGNsaWNraW5nIGFueXdoZXJlIG9uIHRoZSBub2RlIHNob3VsZCBhY3QgYXMgY2xpY2tpbmcgb24gdGhlIGNoZWNrYm94LiBEZWZhdWx0cyB0byBgdHJ1ZWAuXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jaGVja2JveC53aG9sZV9ub2RlXHJcblx0XHQgKiBAcGx1Z2luIGNoZWNrYm94XHJcblx0XHQgKi9cclxuXHRcdHdob2xlX25vZGVcdFx0XHQ6IHRydWUsXHJcblx0XHQvKipcclxuXHRcdCAqIGEgYm9vbGVhbiBpbmRpY2F0aW5nIGlmIHRoZSBzZWxlY3RlZCBzdHlsZSBvZiBhIG5vZGUgc2hvdWxkIGJlIGtlcHQsIG9yIHJlbW92ZWQuIERlZmF1bHRzIHRvIGB0cnVlYC5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNoZWNrYm94LmtlZXBfc2VsZWN0ZWRfc3R5bGVcclxuXHRcdCAqIEBwbHVnaW4gY2hlY2tib3hcclxuXHRcdCAqL1xyXG5cdFx0a2VlcF9zZWxlY3RlZF9zdHlsZVx0OiB0cnVlXHJcblx0fTtcclxuXHQkLmpzdHJlZS5wbHVnaW5zLmNoZWNrYm94ID0gZnVuY3Rpb24gKG9wdGlvbnMsIHBhcmVudCkge1xyXG5cdFx0dGhpcy5iaW5kID0gZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRwYXJlbnQuYmluZC5jYWxsKHRoaXMpO1xyXG5cdFx0XHR0aGlzLl9kYXRhLmNoZWNrYm94LnV0byA9IGZhbHNlO1xyXG5cdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHQub24oXCJpbml0LmpzdHJlZVwiLCAkLnByb3h5KGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jaGVja2JveC52aXNpYmxlID0gdGhpcy5zZXR0aW5ncy5jaGVja2JveC52aXNpYmxlO1xyXG5cdFx0XHRcdFx0XHRpZighdGhpcy5zZXR0aW5ncy5jaGVja2JveC5rZWVwX3NlbGVjdGVkX3N0eWxlKSB7XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5lbGVtZW50LmFkZENsYXNzKCdqc3RyZWUtY2hlY2tib3gtbm8tY2xpY2tlZCcpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHQub24oXCJsb2FkaW5nLmpzdHJlZVwiLCAkLnByb3h5KGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdFx0dGhpc1sgdGhpcy5fZGF0YS5jaGVja2JveC52aXNpYmxlID8gJ3Nob3dfY2hlY2tib3hlcycgOiAnaGlkZV9jaGVja2JveGVzJyBdKCk7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSk7XHJcblx0XHRcdGlmKHRoaXMuc2V0dGluZ3MuY2hlY2tib3gudGhyZWVfc3RhdGUpIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHRcdC5vbignY2hhbmdlZC5qc3RyZWUgbW92ZV9ub2RlLmpzdHJlZSBjb3B5X25vZGUuanN0cmVlIHJlZHJhdy5qc3RyZWUgb3Blbl9ub2RlLmpzdHJlZScsICQucHJveHkoZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdFx0XHRcdGlmKHRoaXMuX2RhdGEuY2hlY2tib3gudXRvKSB7IGNsZWFyVGltZW91dCh0aGlzLl9kYXRhLmNoZWNrYm94LnV0byk7IH1cclxuXHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNoZWNrYm94LnV0byA9IHNldFRpbWVvdXQoJC5wcm94eSh0aGlzLl91bmRldGVybWluZWQsIHRoaXMpLCA1MCk7XHJcblx0XHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdFx0Lm9uKCdtb2RlbC5qc3RyZWUnLCAkLnByb3h5KGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0XHRcdFx0dmFyIG0gPSB0aGlzLl9tb2RlbC5kYXRhLFxyXG5cdFx0XHRcdFx0XHRcdFx0cCA9IG1bZGF0YS5wYXJlbnRdLFxyXG5cdFx0XHRcdFx0XHRcdFx0ZHBjID0gZGF0YS5ub2RlcyxcclxuXHRcdFx0XHRcdFx0XHRcdGNoZCA9IFtdLFxyXG5cdFx0XHRcdFx0XHRcdFx0YywgaSwgaiwgaywgbCwgdG1wO1xyXG5cclxuXHRcdFx0XHRcdFx0XHQvLyBhcHBseSBkb3duXHJcblx0XHRcdFx0XHRcdFx0aWYocC5zdGF0ZS5zZWxlY3RlZCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gZHBjLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRtW2RwY1tpXV0uc3RhdGUuc2VsZWN0ZWQgPSB0cnVlO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkID0gdGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLmNvbmNhdChkcGMpO1xyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IGRwYy5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYobVtkcGNbaV1dLnN0YXRlLnNlbGVjdGVkKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0Zm9yKGsgPSAwLCBsID0gbVtkcGNbaV1dLmNoaWxkcmVuX2QubGVuZ3RoOyBrIDwgbDsgaysrKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0XHRtW21bZHBjW2ldXS5jaGlsZHJlbl9kW2tdXS5zdGF0ZS5zZWxlY3RlZCA9IHRydWU7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZCA9IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZC5jb25jYXQobVtkcGNbaV1dLmNoaWxkcmVuX2QpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cclxuXHRcdFx0XHRcdFx0XHQvLyBhcHBseSB1cFxyXG5cdFx0XHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IHAuY2hpbGRyZW5fZC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdFx0XHRcdGlmKCFtW3AuY2hpbGRyZW5fZFtpXV0uY2hpbGRyZW4ubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGNoZC5wdXNoKG1bcC5jaGlsZHJlbl9kW2ldXS5wYXJlbnQpO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRjaGQgPSAkLnZha2F0YS5hcnJheV91bmlxdWUoY2hkKTtcclxuXHRcdFx0XHRcdFx0XHRmb3IoayA9IDAsIGwgPSBjaGQubGVuZ3RoOyBrIDwgbDsgaysrKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRwID0gbVtjaGRba11dO1xyXG5cdFx0XHRcdFx0XHRcdFx0d2hpbGUocCAmJiBwLmlkICE9PSAnIycpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0YyA9IDA7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IHAuY2hpbGRyZW4ubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0YyArPSBtW3AuY2hpbGRyZW5baV1dLnN0YXRlLnNlbGVjdGVkO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdGlmKGMgPT09IGopIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRwLnN0YXRlLnNlbGVjdGVkID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQucHVzaChwLmlkKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0bXAgPSB0aGlzLmdldF9ub2RlKHAsIHRydWUpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGlmKHRtcCAmJiB0bXAubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0XHR0bXAuY2hpbGRyZW4oJy5qc3RyZWUtYW5jaG9yJykuYWRkQ2xhc3MoJ2pzdHJlZS1jbGlja2VkJyk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdHAgPSB0aGlzLmdldF9ub2RlKHAucGFyZW50KTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkID0gJC52YWthdGEuYXJyYXlfdW5pcXVlKHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZCk7XHJcblx0XHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdFx0Lm9uKCdzZWxlY3Rfbm9kZS5qc3RyZWUnLCAkLnByb3h5KGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0XHRcdFx0dmFyIG9iaiA9IGRhdGEubm9kZSxcclxuXHRcdFx0XHRcdFx0XHRcdG0gPSB0aGlzLl9tb2RlbC5kYXRhLFxyXG5cdFx0XHRcdFx0XHRcdFx0cGFyID0gdGhpcy5nZXRfbm9kZShvYmoucGFyZW50KSxcclxuXHRcdFx0XHRcdFx0XHRcdGRvbSA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKSxcclxuXHRcdFx0XHRcdFx0XHRcdGksIGosIGMsIHRtcDtcclxuXHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQgPSAkLnZha2F0YS5hcnJheV91bmlxdWUodGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLmNvbmNhdChvYmouY2hpbGRyZW5fZCkpO1xyXG5cdFx0XHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IG9iai5jaGlsZHJlbl9kLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0bVtvYmouY2hpbGRyZW5fZFtpXV0uc3RhdGUuc2VsZWN0ZWQgPSB0cnVlO1xyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR3aGlsZShwYXIgJiYgcGFyLmlkICE9PSAnIycpIHtcclxuXHRcdFx0XHRcdFx0XHRcdGMgPSAwO1xyXG5cdFx0XHRcdFx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gcGFyLmNoaWxkcmVuLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRjICs9IG1bcGFyLmNoaWxkcmVuW2ldXS5zdGF0ZS5zZWxlY3RlZDtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGlmKGMgPT09IGopIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0cGFyLnN0YXRlLnNlbGVjdGVkID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLnB1c2gocGFyLmlkKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dG1wID0gdGhpcy5nZXRfbm9kZShwYXIsIHRydWUpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRpZih0bXAgJiYgdG1wLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHRtcC5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5hZGRDbGFzcygnanN0cmVlLWNsaWNrZWQnKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0cGFyID0gdGhpcy5nZXRfbm9kZShwYXIucGFyZW50KTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0aWYoZG9tLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0ZG9tLmZpbmQoJy5qc3RyZWUtYW5jaG9yJykuYWRkQ2xhc3MoJ2pzdHJlZS1jbGlja2VkJyk7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHRcdC5vbignZGVzZWxlY3Rfbm9kZS5qc3RyZWUnLCAkLnByb3h5KGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0XHRcdFx0dmFyIG9iaiA9IGRhdGEubm9kZSxcclxuXHRcdFx0XHRcdFx0XHRcdGRvbSA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKSxcclxuXHRcdFx0XHRcdFx0XHRcdGksIGosIHRtcDtcclxuXHRcdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSBvYmouY2hpbGRyZW5fZC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdFx0XHRcdHRoaXMuX21vZGVsLmRhdGFbb2JqLmNoaWxkcmVuX2RbaV1dLnN0YXRlLnNlbGVjdGVkID0gZmFsc2U7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IG9iai5wYXJlbnRzLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0dGhpcy5fbW9kZWwuZGF0YVtvYmoucGFyZW50c1tpXV0uc3RhdGUuc2VsZWN0ZWQgPSBmYWxzZTtcclxuXHRcdFx0XHRcdFx0XHRcdHRtcCA9IHRoaXMuZ2V0X25vZGUob2JqLnBhcmVudHNbaV0sIHRydWUpO1xyXG5cdFx0XHRcdFx0XHRcdFx0aWYodG1wICYmIHRtcC5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dG1wLmNoaWxkcmVuKCcuanN0cmVlLWFuY2hvcicpLnJlbW92ZUNsYXNzKCdqc3RyZWUtY2xpY2tlZCcpO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR0bXAgPSBbXTtcclxuXHRcdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRpZigkLmluQXJyYXkodGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkW2ldLCBvYmouY2hpbGRyZW5fZCkgPT09IC0xICYmICQuaW5BcnJheSh0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWRbaV0sIG9iai5wYXJlbnRzKSA9PT0gLTEpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dG1wLnB1c2godGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkW2ldKTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkID0gJC52YWthdGEuYXJyYXlfdW5pcXVlKHRtcCk7XHJcblx0XHRcdFx0XHRcdFx0aWYoZG9tLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0ZG9tLmZpbmQoJy5qc3RyZWUtYW5jaG9yJykucmVtb3ZlQ2xhc3MoJ2pzdHJlZS1jbGlja2VkJyk7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHRcdC5vbignZGVsZXRlX25vZGUuanN0cmVlJywgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHRcdHZhciBwID0gdGhpcy5nZXRfbm9kZShkYXRhLnBhcmVudCksXHJcblx0XHRcdFx0XHRcdFx0XHRtID0gdGhpcy5fbW9kZWwuZGF0YSxcclxuXHRcdFx0XHRcdFx0XHRcdGksIGosIGMsIHRtcDtcclxuXHRcdFx0XHRcdFx0XHR3aGlsZShwICYmIHAuaWQgIT09ICcjJykge1xyXG5cdFx0XHRcdFx0XHRcdFx0YyA9IDA7XHJcblx0XHRcdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSBwLmNoaWxkcmVuLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRjICs9IG1bcC5jaGlsZHJlbltpXV0uc3RhdGUuc2VsZWN0ZWQ7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRpZihjID09PSBqKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHAuc3RhdGUuc2VsZWN0ZWQgPSB0cnVlO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQucHVzaChwLmlkKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dG1wID0gdGhpcy5nZXRfbm9kZShwLCB0cnVlKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYodG1wICYmIHRtcC5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0bXAuY2hpbGRyZW4oJy5qc3RyZWUtYW5jaG9yJykuYWRkQ2xhc3MoJ2pzdHJlZS1jbGlja2VkJyk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdHAgPSB0aGlzLmdldF9ub2RlKHAucGFyZW50KTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdFx0Lm9uKCdtb3ZlX25vZGUuanN0cmVlJywgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHRcdHZhciBpc19tdWx0aSA9IGRhdGEuaXNfbXVsdGksXHJcblx0XHRcdFx0XHRcdFx0XHRvbGRfcGFyID0gZGF0YS5vbGRfcGFyZW50LFxyXG5cdFx0XHRcdFx0XHRcdFx0bmV3X3BhciA9IHRoaXMuZ2V0X25vZGUoZGF0YS5wYXJlbnQpLFxyXG5cdFx0XHRcdFx0XHRcdFx0bSA9IHRoaXMuX21vZGVsLmRhdGEsXHJcblx0XHRcdFx0XHRcdFx0XHRwLCBjLCBpLCBqLCB0bXA7XHJcblx0XHRcdFx0XHRcdFx0aWYoIWlzX211bHRpKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRwID0gdGhpcy5nZXRfbm9kZShvbGRfcGFyKTtcclxuXHRcdFx0XHRcdFx0XHRcdHdoaWxlKHAgJiYgcC5pZCAhPT0gJyMnKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGMgPSAwO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSBwLmNoaWxkcmVuLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGMgKz0gbVtwLmNoaWxkcmVuW2ldXS5zdGF0ZS5zZWxlY3RlZDtcclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHRpZihjID09PSBqKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0cC5zdGF0ZS5zZWxlY3RlZCA9IHRydWU7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLnNlbGVjdGVkLnB1c2gocC5pZCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0dG1wID0gdGhpcy5nZXRfbm9kZShwLCB0cnVlKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRpZih0bXAgJiYgdG1wLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdFx0dG1wLmNoaWxkcmVuKCcuanN0cmVlLWFuY2hvcicpLmFkZENsYXNzKCdqc3RyZWUtY2xpY2tlZCcpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHRwID0gdGhpcy5nZXRfbm9kZShwLnBhcmVudCk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdHAgPSBuZXdfcGFyO1xyXG5cdFx0XHRcdFx0XHRcdHdoaWxlKHAgJiYgcC5pZCAhPT0gJyMnKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRjID0gMDtcclxuXHRcdFx0XHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IHAuY2hpbGRyZW4ubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGMgKz0gbVtwLmNoaWxkcmVuW2ldXS5zdGF0ZS5zZWxlY3RlZDtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGlmKGMgPT09IGopIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYoIXAuc3RhdGUuc2VsZWN0ZWQpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRwLnN0YXRlLnNlbGVjdGVkID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQucHVzaChwLmlkKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0bXAgPSB0aGlzLmdldF9ub2RlKHAsIHRydWUpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGlmKHRtcCAmJiB0bXAubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0XHR0bXAuY2hpbGRyZW4oJy5qc3RyZWUtYW5jaG9yJykuYWRkQ2xhc3MoJ2pzdHJlZS1jbGlja2VkJyk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYocC5zdGF0ZS5zZWxlY3RlZCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHAuc3RhdGUuc2VsZWN0ZWQgPSBmYWxzZTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQgPSAkLnZha2F0YS5hcnJheV9yZW1vdmVfaXRlbSh0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQsIHAuaWQpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHRtcCA9IHRoaXMuZ2V0X25vZGUocCwgdHJ1ZSk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0aWYodG1wICYmIHRtcC5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRcdHRtcC5jaGlsZHJlbignLmpzdHJlZS1hbmNob3InKS5yZW1vdmVDbGFzcygnanN0cmVlLWNsaWNrZWQnKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdHAgPSB0aGlzLmdldF9ub2RlKHAucGFyZW50KTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdFx0fVxyXG5cdFx0fTtcclxuXHRcdC8qKlxyXG5cdFx0ICogc2V0IHRoZSB1bmRldGVybWluZWQgc3RhdGUgd2hlcmUgYW5kIGlmIG5lY2Vzc2FyeS4gVXNlZCBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIF91bmRldGVybWluZWQoKVxyXG5cdFx0ICogQHBsdWdpbiBjaGVja2JveFxyXG5cdFx0ICovXHJcblx0XHR0aGlzLl91bmRldGVybWluZWQgPSBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHZhciBpLCBqLCBtID0gdGhpcy5fbW9kZWwuZGF0YSwgcyA9IHRoaXMuX2RhdGEuY29yZS5zZWxlY3RlZCwgcCA9IFtdLCB0ID0gdGhpcztcclxuXHRcdFx0Zm9yKGkgPSAwLCBqID0gcy5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRpZihtW3NbaV1dICYmIG1bc1tpXV0ucGFyZW50cykge1xyXG5cdFx0XHRcdFx0cCA9IHAuY29uY2F0KG1bc1tpXV0ucGFyZW50cyk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdC8vIGF0dGVtcHQgZm9yIHNlcnZlciBzaWRlIHVuZGV0ZXJtaW5lZCBzdGF0ZVxyXG5cdFx0XHR0aGlzLmVsZW1lbnQuZmluZCgnLmpzdHJlZS1jbG9zZWQnKS5ub3QoJzpoYXModWwpJylcclxuXHRcdFx0XHQuZWFjaChmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHR2YXIgdG1wID0gdC5nZXRfbm9kZSh0aGlzKTtcclxuXHRcdFx0XHRcdGlmKCF0bXAuc3RhdGUubG9hZGVkICYmIHRtcC5vcmlnaW5hbCAmJiB0bXAub3JpZ2luYWwuc3RhdGUgJiYgdG1wLm9yaWdpbmFsLnN0YXRlLnVuZGV0ZXJtaW5lZCAmJiB0bXAub3JpZ2luYWwuc3RhdGUudW5kZXRlcm1pbmVkID09PSB0cnVlKSB7XHJcblx0XHRcdFx0XHRcdHAucHVzaCh0bXAuaWQpO1xyXG5cdFx0XHRcdFx0XHRwID0gcC5jb25jYXQodG1wLnBhcmVudHMpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH0pO1xyXG5cdFx0XHRwID0gJC52YWthdGEuYXJyYXlfdW5pcXVlKHApO1xyXG5cdFx0XHRpID0gJC5pbkFycmF5KCcjJywgcCk7XHJcblx0XHRcdGlmKGkgIT09IC0xKSB7XHJcblx0XHRcdFx0cCA9ICQudmFrYXRhLmFycmF5X3JlbW92ZShwLCBpKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0dGhpcy5lbGVtZW50LmZpbmQoJy5qc3RyZWUtdW5kZXRlcm1pbmVkJykucmVtb3ZlQ2xhc3MoJ2pzdHJlZS11bmRldGVybWluZWQnKTtcclxuXHRcdFx0Zm9yKGkgPSAwLCBqID0gcC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRpZighbVtwW2ldXS5zdGF0ZS5zZWxlY3RlZCkge1xyXG5cdFx0XHRcdFx0cyA9IHRoaXMuZ2V0X25vZGUocFtpXSwgdHJ1ZSk7XHJcblx0XHRcdFx0XHRpZihzICYmIHMubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdHMuY2hpbGRyZW4oJ2EnKS5jaGlsZHJlbignLmpzdHJlZS1jaGVja2JveCcpLmFkZENsYXNzKCdqc3RyZWUtdW5kZXRlcm1pbmVkJyk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHR9O1xyXG5cdFx0dGhpcy5yZWRyYXdfbm9kZSA9IGZ1bmN0aW9uKG9iaiwgZGVlcCwgaXNfY2FsbGJhY2spIHtcclxuXHRcdFx0b2JqID0gcGFyZW50LnJlZHJhd19ub2RlLmNhbGwodGhpcywgb2JqLCBkZWVwLCBpc19jYWxsYmFjayk7XHJcblx0XHRcdGlmKG9iaikge1xyXG5cdFx0XHRcdHZhciB0bXAgPSBvYmouZ2V0RWxlbWVudHNCeVRhZ05hbWUoJ0EnKVswXTtcclxuXHRcdFx0XHR0bXAuaW5zZXJ0QmVmb3JlKF9pLmNsb25lTm9kZSgpLCB0bXAuY2hpbGROb2Rlc1swXSk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoIWlzX2NhbGxiYWNrICYmIHRoaXMuc2V0dGluZ3MuY2hlY2tib3gudGhyZWVfc3RhdGUpIHtcclxuXHRcdFx0XHRpZih0aGlzLl9kYXRhLmNoZWNrYm94LnV0bykgeyBjbGVhclRpbWVvdXQodGhpcy5fZGF0YS5jaGVja2JveC51dG8pOyB9XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5jaGVja2JveC51dG8gPSBzZXRUaW1lb3V0KCQucHJveHkodGhpcy5fdW5kZXRlcm1pbmVkLCB0aGlzKSwgNTApO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiBvYmo7XHJcblx0XHR9O1xyXG5cdFx0dGhpcy5hY3RpdmF0ZV9ub2RlID0gZnVuY3Rpb24gKG9iaiwgZSkge1xyXG5cdFx0XHRpZih0aGlzLnNldHRpbmdzLmNoZWNrYm94Lndob2xlX25vZGUgfHwgJChlLnRhcmdldCkuaGFzQ2xhc3MoJ2pzdHJlZS1jaGVja2JveCcpKSB7XHJcblx0XHRcdFx0ZS5jdHJsS2V5ID0gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gcGFyZW50LmFjdGl2YXRlX25vZGUuY2FsbCh0aGlzLCBvYmosIGUpO1xyXG5cdFx0fTtcclxuXHRcdC8qKlxyXG5cdFx0ICogc2hvdyB0aGUgbm9kZSBjaGVja2JveCBpY29uc1xyXG5cdFx0ICogQG5hbWUgc2hvd19jaGVja2JveGVzKClcclxuXHRcdCAqIEBwbHVnaW4gY2hlY2tib3hcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5zaG93X2NoZWNrYm94ZXMgPSBmdW5jdGlvbiAoKSB7IHRoaXMuX2RhdGEuY29yZS50aGVtZXMuY2hlY2tib3hlcyA9IHRydWU7IHRoaXMuZWxlbWVudC5jaGlsZHJlbihcInVsXCIpLnJlbW92ZUNsYXNzKFwianN0cmVlLW5vLWNoZWNrYm94ZXNcIik7IH07XHJcblx0XHQvKipcclxuXHRcdCAqIGhpZGUgdGhlIG5vZGUgY2hlY2tib3ggaWNvbnNcclxuXHRcdCAqIEBuYW1lIGhpZGVfY2hlY2tib3hlcygpXHJcblx0XHQgKiBAcGx1Z2luIGNoZWNrYm94XHJcblx0XHQgKi9cclxuXHRcdHRoaXMuaGlkZV9jaGVja2JveGVzID0gZnVuY3Rpb24gKCkgeyB0aGlzLl9kYXRhLmNvcmUudGhlbWVzLmNoZWNrYm94ZXMgPSBmYWxzZTsgdGhpcy5lbGVtZW50LmNoaWxkcmVuKFwidWxcIikuYWRkQ2xhc3MoXCJqc3RyZWUtbm8tY2hlY2tib3hlc1wiKTsgfTtcclxuXHRcdC8qKlxyXG5cdFx0ICogdG9nZ2xlIHRoZSBub2RlIGljb25zXHJcblx0XHQgKiBAbmFtZSB0b2dnbGVfY2hlY2tib3hlcygpXHJcblx0XHQgKiBAcGx1Z2luIGNoZWNrYm94XHJcblx0XHQgKi9cclxuXHRcdHRoaXMudG9nZ2xlX2NoZWNrYm94ZXMgPSBmdW5jdGlvbiAoKSB7IGlmKHRoaXMuX2RhdGEuY29yZS50aGVtZXMuY2hlY2tib3hlcykgeyB0aGlzLmhpZGVfY2hlY2tib3hlcygpOyB9IGVsc2UgeyB0aGlzLnNob3dfY2hlY2tib3hlcygpOyB9IH07XHJcblx0fTtcclxuXHJcblx0Ly8gaW5jbHVkZSB0aGUgY2hlY2tib3ggcGx1Z2luIGJ5IGRlZmF1bHRcclxuXHQvLyAkLmpzdHJlZS5kZWZhdWx0cy5wbHVnaW5zLnB1c2goXCJjaGVja2JveFwiKTtcclxuXHJcbi8qKlxyXG4gKiAjIyMgQ29udGV4dG1lbnUgcGx1Z2luXHJcbiAqXHJcbiAqIFNob3dzIGEgY29udGV4dCBtZW51IHdoZW4gYSBub2RlIGlzIHJpZ2h0LWNsaWNrZWQuXHJcbiAqL1xyXG4vLyBUT0RPOiBtb3ZlIGxvZ2ljIG91dHNpZGUgb2YgZnVuY3Rpb24gKyBjaGVjayBtdWx0aXBsZSBtb3ZlXHJcblxyXG5cdC8qKlxyXG5cdCAqIHN0b3JlcyBhbGwgZGVmYXVsdHMgZm9yIHRoZSBjb250ZXh0bWVudSBwbHVnaW5cclxuXHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb250ZXh0bWVudVxyXG5cdCAqIEBwbHVnaW4gY29udGV4dG1lbnVcclxuXHQgKi9cclxuXHQkLmpzdHJlZS5kZWZhdWx0cy5jb250ZXh0bWVudSA9IHtcclxuXHRcdC8qKlxyXG5cdFx0ICogYSBib29sZWFuIGluZGljYXRpbmcgaWYgdGhlIG5vZGUgc2hvdWxkIGJlIHNlbGVjdGVkIHdoZW4gdGhlIGNvbnRleHQgbWVudSBpcyBpbnZva2VkIG9uIGl0LiBEZWZhdWx0cyB0byBgdHJ1ZWAuXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb250ZXh0bWVudS5zZWxlY3Rfbm9kZVxyXG5cdFx0ICogQHBsdWdpbiBjb250ZXh0bWVudVxyXG5cdFx0ICovXHJcblx0XHRzZWxlY3Rfbm9kZSA6IHRydWUsXHJcblx0XHQvKipcclxuXHRcdCAqIGEgYm9vbGVhbiBpbmRpY2F0aW5nIGlmIHRoZSBtZW51IHNob3VsZCBiZSBzaG93biBhbGlnbmVkIHdpdGggdGhlIG5vZGUuIERlZmF1bHRzIHRvIGB0cnVlYCwgb3RoZXJ3aXNlIHRoZSBtb3VzZSBjb29yZGluYXRlcyBhcmUgdXNlZC5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmNvbnRleHRtZW51LnNob3dfYXRfbm9kZVxyXG5cdFx0ICogQHBsdWdpbiBjb250ZXh0bWVudVxyXG5cdFx0ICovXHJcblx0XHRzaG93X2F0X25vZGUgOiB0cnVlLFxyXG5cdFx0LyoqXHJcblx0XHQgKiBhbiBvYmplY3Qgb2YgYWN0aW9ucywgb3IgYSBmdW5jdGlvbiB0aGF0IGFjY2VwdHMgYSBub2RlIGFuZCBhIGNhbGxiYWNrIGZ1bmN0aW9uIGFuZCBjYWxscyB0aGUgY2FsbGJhY2sgZnVuY3Rpb24gd2l0aCBhbiBvYmplY3Qgb2YgYWN0aW9ucyBhdmFpbGFibGUgZm9yIHRoYXQgbm9kZSAoeW91IGNhbiBhbHNvIHJldHVybiB0aGUgaXRlbXMgdG9vKS5cclxuXHRcdCAqIFxyXG5cdFx0ICogRWFjaCBhY3Rpb24gY29uc2lzdHMgb2YgYSBrZXkgKGEgdW5pcXVlIG5hbWUpIGFuZCBhIHZhbHVlIHdoaWNoIGlzIGFuIG9iamVjdCB3aXRoIHRoZSBmb2xsb3dpbmcgcHJvcGVydGllcyAob25seSBsYWJlbCBhbmQgYWN0aW9uIGFyZSByZXF1aXJlZCk6XHJcblx0XHQgKiBcclxuXHRcdCAqICogYHNlcGFyYXRvcl9iZWZvcmVgIC0gYSBib29sZWFuIGluZGljYXRpbmcgaWYgdGhlcmUgc2hvdWxkIGJlIGEgc2VwYXJhdG9yIGJlZm9yZSB0aGlzIGl0ZW1cclxuXHRcdCAqICogYHNlcGFyYXRvcl9hZnRlcmAgLSBhIGJvb2xlYW4gaW5kaWNhdGluZyBpZiB0aGVyZSBzaG91bGQgYmUgYSBzZXBhcmF0b3IgYWZ0ZXIgdGhpcyBpdGVtXHJcblx0XHQgKiAqIGBfZGlzYWJsZWRgIC0gYSBib29sZWFuIGluZGljYXRpbmcgaWYgdGhpcyBhY3Rpb24gc2hvdWxkIGJlIGRpc2FibGVkXHJcblx0XHQgKiAqIGBsYWJlbGAgLSBhIHN0cmluZyAtIHRoZSBuYW1lIG9mIHRoZSBhY3Rpb25cclxuXHRcdCAqICogYGFjdGlvbmAgLSBhIGZ1bmN0aW9uIHRvIGJlIGV4ZWN1dGVkIGlmIHRoaXMgaXRlbSBpcyBjaG9zZW5cclxuXHRcdCAqICogYGljb25gIC0gYSBzdHJpbmcsIGNhbiBiZSBhIHBhdGggdG8gYW4gaWNvbiBvciBhIGNsYXNzTmFtZSwgaWYgdXNpbmcgYW4gaW1hZ2UgdGhhdCBpcyBpbiB0aGUgY3VycmVudCBkaXJlY3RvcnkgdXNlIGEgYC4vYCBwcmVmaXgsIG90aGVyd2lzZSBpdCB3aWxsIGJlIGRldGVjdGVkIGFzIGEgY2xhc3NcclxuXHRcdCAqICogYHNob3J0Y3V0YCAtIGtleUNvZGUgd2hpY2ggd2lsbCB0cmlnZ2VyIHRoZSBhY3Rpb24gaWYgdGhlIG1lbnUgaXMgb3BlbiAoZm9yIGV4YW1wbGUgYDExM2AgZm9yIHJlbmFtZSwgd2hpY2ggZXF1YWxzIEYyKVxyXG5cdFx0ICogKiBgc2hvcnRjdXRfbGFiZWxgIC0gc2hvcnRjdXQgbGFiZWwgKGxpa2UgZm9yIGV4YW1wbGUgYEYyYCBmb3IgcmVuYW1lKVxyXG5cdFx0ICogXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5jb250ZXh0bWVudS5pdGVtc1xyXG5cdFx0ICogQHBsdWdpbiBjb250ZXh0bWVudVxyXG5cdFx0ICovXHJcblx0XHRpdGVtcyA6IGZ1bmN0aW9uIChvLCBjYikgeyAvLyBDb3VsZCBiZSBhbiBvYmplY3QgZGlyZWN0bHlcclxuXHRcdFx0cmV0dXJuIHtcclxuXHRcdFx0XHRcImNyZWF0ZVwiIDoge1xyXG5cdFx0XHRcdFx0XCJzZXBhcmF0b3JfYmVmb3JlXCJcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRcInNlcGFyYXRvcl9hZnRlclwiXHQ6IHRydWUsXHJcblx0XHRcdFx0XHRcIl9kaXNhYmxlZFwiXHRcdFx0OiBmYWxzZSwgLy8odGhpcy5jaGVjayhcImNyZWF0ZV9ub2RlXCIsIGRhdGEucmVmZXJlbmNlLCB7fSwgXCJsYXN0XCIpKSxcclxuXHRcdFx0XHRcdFwibGFiZWxcIlx0XHRcdFx0OiBcIkNyZWF0ZVwiLFxyXG5cdFx0XHRcdFx0XCJhY3Rpb25cIlx0XHRcdDogZnVuY3Rpb24gKGRhdGEpIHtcclxuXHRcdFx0XHRcdFx0dmFyIGluc3QgPSAkLmpzdHJlZS5yZWZlcmVuY2UoZGF0YS5yZWZlcmVuY2UpLFxyXG5cdFx0XHRcdFx0XHRcdG9iaiA9IGluc3QuZ2V0X25vZGUoZGF0YS5yZWZlcmVuY2UpO1xyXG5cdFx0XHRcdFx0XHRpbnN0LmNyZWF0ZV9ub2RlKG9iaiwge30sIFwibGFzdFwiLCBmdW5jdGlvbiAobmV3X25vZGUpIHtcclxuXHRcdFx0XHRcdFx0XHRzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHsgaW5zdC5lZGl0KG5ld19ub2RlKTsgfSwwKTtcclxuXHRcdFx0XHRcdFx0fSk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fSxcclxuXHRcdFx0XHRcInJlbmFtZVwiIDoge1xyXG5cdFx0XHRcdFx0XCJzZXBhcmF0b3JfYmVmb3JlXCJcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRcInNlcGFyYXRvcl9hZnRlclwiXHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0XCJfZGlzYWJsZWRcIlx0XHRcdDogZmFsc2UsIC8vKHRoaXMuY2hlY2soXCJyZW5hbWVfbm9kZVwiLCBkYXRhLnJlZmVyZW5jZSwgdGhpcy5nZXRfcGFyZW50KGRhdGEucmVmZXJlbmNlKSwgXCJcIikpLFxyXG5cdFx0XHRcdFx0XCJsYWJlbFwiXHRcdFx0XHQ6IFwiUmVuYW1lXCIsXHJcblx0XHRcdFx0XHQvKlxyXG5cdFx0XHRcdFx0XCJzaG9ydGN1dFwiXHRcdFx0OiAxMTMsXHJcblx0XHRcdFx0XHRcInNob3J0Y3V0X2xhYmVsXCJcdDogJ0YyJyxcclxuXHRcdFx0XHRcdFwiaWNvblwiXHRcdFx0XHQ6IFwiZ2x5cGhpY29uIGdseXBoaWNvbi1sZWFmXCIsXHJcblx0XHRcdFx0XHQqL1xyXG5cdFx0XHRcdFx0XCJhY3Rpb25cIlx0XHRcdDogZnVuY3Rpb24gKGRhdGEpIHtcclxuXHRcdFx0XHRcdFx0dmFyIGluc3QgPSAkLmpzdHJlZS5yZWZlcmVuY2UoZGF0YS5yZWZlcmVuY2UpLFxyXG5cdFx0XHRcdFx0XHRcdG9iaiA9IGluc3QuZ2V0X25vZGUoZGF0YS5yZWZlcmVuY2UpO1xyXG5cdFx0XHRcdFx0XHRpbnN0LmVkaXQob2JqKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9LFxyXG5cdFx0XHRcdFwicmVtb3ZlXCIgOiB7XHJcblx0XHRcdFx0XHRcInNlcGFyYXRvcl9iZWZvcmVcIlx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFwiaWNvblwiXHRcdFx0XHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0XCJzZXBhcmF0b3JfYWZ0ZXJcIlx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFwiX2Rpc2FibGVkXCJcdFx0XHQ6IGZhbHNlLCAvLyh0aGlzLmNoZWNrKFwiZGVsZXRlX25vZGVcIiwgZGF0YS5yZWZlcmVuY2UsIHRoaXMuZ2V0X3BhcmVudChkYXRhLnJlZmVyZW5jZSksIFwiXCIpKSxcclxuXHRcdFx0XHRcdFwibGFiZWxcIlx0XHRcdFx0OiBcIkRlbGV0ZVwiLFxyXG5cdFx0XHRcdFx0XCJhY3Rpb25cIlx0XHRcdDogZnVuY3Rpb24gKGRhdGEpIHtcclxuXHRcdFx0XHRcdFx0dmFyIGluc3QgPSAkLmpzdHJlZS5yZWZlcmVuY2UoZGF0YS5yZWZlcmVuY2UpLFxyXG5cdFx0XHRcdFx0XHRcdG9iaiA9IGluc3QuZ2V0X25vZGUoZGF0YS5yZWZlcmVuY2UpO1xyXG5cdFx0XHRcdFx0XHRpZihpbnN0LmlzX3NlbGVjdGVkKG9iaikpIHtcclxuXHRcdFx0XHRcdFx0XHRpbnN0LmRlbGV0ZV9ub2RlKGluc3QuZ2V0X3NlbGVjdGVkKCkpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdGluc3QuZGVsZXRlX25vZGUob2JqKTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH0sXHJcblx0XHRcdFx0XCJjY3BcIiA6IHtcclxuXHRcdFx0XHRcdFwic2VwYXJhdG9yX2JlZm9yZVwiXHQ6IHRydWUsXHJcblx0XHRcdFx0XHRcImljb25cIlx0XHRcdFx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFwic2VwYXJhdG9yX2FmdGVyXCJcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRcImxhYmVsXCJcdFx0XHRcdDogXCJFZGl0XCIsXHJcblx0XHRcdFx0XHRcImFjdGlvblwiXHRcdFx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFwic3VibWVudVwiIDoge1xyXG5cdFx0XHRcdFx0XHRcImN1dFwiIDoge1xyXG5cdFx0XHRcdFx0XHRcdFwic2VwYXJhdG9yX2JlZm9yZVwiXHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0XHRcdFwic2VwYXJhdG9yX2FmdGVyXCJcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRcdFx0XCJsYWJlbFwiXHRcdFx0XHQ6IFwiQ3V0XCIsXHJcblx0XHRcdFx0XHRcdFx0XCJhY3Rpb25cIlx0XHRcdDogZnVuY3Rpb24gKGRhdGEpIHtcclxuXHRcdFx0XHRcdFx0XHRcdHZhciBpbnN0ID0gJC5qc3RyZWUucmVmZXJlbmNlKGRhdGEucmVmZXJlbmNlKSxcclxuXHRcdFx0XHRcdFx0XHRcdFx0b2JqID0gaW5zdC5nZXRfbm9kZShkYXRhLnJlZmVyZW5jZSk7XHJcblx0XHRcdFx0XHRcdFx0XHRpZihpbnN0LmlzX3NlbGVjdGVkKG9iaikpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aW5zdC5jdXQoaW5zdC5nZXRfc2VsZWN0ZWQoKSk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aW5zdC5jdXQob2JqKTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH0sXHJcblx0XHRcdFx0XHRcdFwiY29weVwiIDoge1xyXG5cdFx0XHRcdFx0XHRcdFwic2VwYXJhdG9yX2JlZm9yZVwiXHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0XHRcdFwiaWNvblwiXHRcdFx0XHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0XHRcdFwic2VwYXJhdG9yX2FmdGVyXCJcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRcdFx0XCJsYWJlbFwiXHRcdFx0XHQ6IFwiQ29weVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwiYWN0aW9uXCJcdFx0XHQ6IGZ1bmN0aW9uIChkYXRhKSB7XHJcblx0XHRcdFx0XHRcdFx0XHR2YXIgaW5zdCA9ICQuanN0cmVlLnJlZmVyZW5jZShkYXRhLnJlZmVyZW5jZSksXHJcblx0XHRcdFx0XHRcdFx0XHRcdG9iaiA9IGluc3QuZ2V0X25vZGUoZGF0YS5yZWZlcmVuY2UpO1xyXG5cdFx0XHRcdFx0XHRcdFx0aWYoaW5zdC5pc19zZWxlY3RlZChvYmopKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGluc3QuY29weShpbnN0LmdldF9zZWxlY3RlZCgpKTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRpbnN0LmNvcHkob2JqKTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH0sXHJcblx0XHRcdFx0XHRcdFwicGFzdGVcIiA6IHtcclxuXHRcdFx0XHRcdFx0XHRcInNlcGFyYXRvcl9iZWZvcmVcIlx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XHRcImljb25cIlx0XHRcdFx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XHRcIl9kaXNhYmxlZFwiXHRcdFx0OiBmdW5jdGlvbiAoZGF0YSkge1xyXG5cdFx0XHRcdFx0XHRcdFx0cmV0dXJuICEkLmpzdHJlZS5yZWZlcmVuY2UoZGF0YS5yZWZlcmVuY2UpLmNhbl9wYXN0ZSgpO1xyXG5cdFx0XHRcdFx0XHRcdH0sXHJcblx0XHRcdFx0XHRcdFx0XCJzZXBhcmF0b3JfYWZ0ZXJcIlx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XHRcImxhYmVsXCJcdFx0XHRcdDogXCJQYXN0ZVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwiYWN0aW9uXCJcdFx0XHQ6IGZ1bmN0aW9uIChkYXRhKSB7XHJcblx0XHRcdFx0XHRcdFx0XHR2YXIgaW5zdCA9ICQuanN0cmVlLnJlZmVyZW5jZShkYXRhLnJlZmVyZW5jZSksXHJcblx0XHRcdFx0XHRcdFx0XHRcdG9iaiA9IGluc3QuZ2V0X25vZGUoZGF0YS5yZWZlcmVuY2UpO1xyXG5cdFx0XHRcdFx0XHRcdFx0aW5zdC5wYXN0ZShvYmopO1xyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fTtcclxuXHRcdH1cclxuXHR9O1xyXG5cclxuXHQkLmpzdHJlZS5wbHVnaW5zLmNvbnRleHRtZW51ID0gZnVuY3Rpb24gKG9wdGlvbnMsIHBhcmVudCkge1xyXG5cdFx0dGhpcy5iaW5kID0gZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRwYXJlbnQuYmluZC5jYWxsKHRoaXMpO1xyXG5cclxuXHRcdFx0dGhpcy5lbGVtZW50XHJcblx0XHRcdFx0Lm9uKFwiY29udGV4dG1lbnUuanN0cmVlXCIsIFwiLmpzdHJlZS1hbmNob3JcIiwgJC5wcm94eShmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XHJcblx0XHRcdFx0XHRcdGlmKCF0aGlzLmlzX2xvYWRpbmcoZS5jdXJyZW50VGFyZ2V0KSkge1xyXG5cdFx0XHRcdFx0XHRcdHRoaXMuc2hvd19jb250ZXh0bWVudShlLmN1cnJlbnRUYXJnZXQsIGUucGFnZVgsIGUucGFnZVksIGUpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHQub24oXCJjbGljay5qc3RyZWVcIiwgXCIuanN0cmVlLWFuY2hvclwiLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdGlmKHRoaXMuX2RhdGEuY29udGV4dG1lbnUudmlzaWJsZSkge1xyXG5cdFx0XHRcdFx0XHRcdCQudmFrYXRhLmNvbnRleHQuaGlkZSgpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LCB0aGlzKSk7XHJcblx0XHRcdC8qXHJcblx0XHRcdGlmKCEoJ29uY29udGV4dG1lbnUnIGluIGRvY3VtZW50LmJvZHkpICYmICgnb250b3VjaHN0YXJ0JyBpbiBkb2N1bWVudC5ib2R5KSkge1xyXG5cdFx0XHRcdHZhciBlbCA9IG51bGwsIHRtID0gbnVsbDtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHRcdC5vbihcInRvdWNoc3RhcnRcIiwgXCIuanN0cmVlLWFuY2hvclwiLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0XHRlbCA9IGUuY3VycmVudFRhcmdldDtcclxuXHRcdFx0XHRcdFx0dG0gPSArbmV3IERhdGUoKTtcclxuXHRcdFx0XHRcdFx0JChkb2N1bWVudCkub25lKFwidG91Y2hlbmRcIiwgZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRcdFx0XHRlLnRhcmdldCA9IGRvY3VtZW50LmVsZW1lbnRGcm9tUG9pbnQoZS5vcmlnaW5hbEV2ZW50LnRhcmdldFRvdWNoZXNbMF0ucGFnZVggLSB3aW5kb3cucGFnZVhPZmZzZXQsIGUub3JpZ2luYWxFdmVudC50YXJnZXRUb3VjaGVzWzBdLnBhZ2VZIC0gd2luZG93LnBhZ2VZT2Zmc2V0KTtcclxuXHRcdFx0XHRcdFx0XHRlLmN1cnJlbnRUYXJnZXQgPSBlLnRhcmdldDtcclxuXHRcdFx0XHRcdFx0XHR0bSA9ICgoKyhuZXcgRGF0ZSgpKSkgLSB0bSk7XHJcblx0XHRcdFx0XHRcdFx0aWYoZS50YXJnZXQgPT09IGVsICYmIHRtID4gNjAwICYmIHRtIDwgMTAwMCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0JChlbCkudHJpZ2dlcignY29udGV4dG1lbnUnLCBlKTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0ZWwgPSBudWxsO1xyXG5cdFx0XHRcdFx0XHRcdHRtID0gbnVsbDtcclxuXHRcdFx0XHRcdFx0fSk7XHJcblx0XHRcdFx0XHR9KTtcclxuXHRcdFx0fVxyXG5cdFx0XHQqL1xyXG5cdFx0XHQkKGRvY3VtZW50KS5vbihcImNvbnRleHRfaGlkZS52YWthdGFcIiwgJC5wcm94eShmdW5jdGlvbiAoKSB7IHRoaXMuX2RhdGEuY29udGV4dG1lbnUudmlzaWJsZSA9IGZhbHNlOyB9LCB0aGlzKSk7XHJcblx0XHR9O1xyXG5cdFx0dGhpcy50ZWFyZG93biA9IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0aWYodGhpcy5fZGF0YS5jb250ZXh0bWVudS52aXNpYmxlKSB7XHJcblx0XHRcdFx0JC52YWthdGEuY29udGV4dC5oaWRlKCk7XHJcblx0XHRcdH1cclxuXHRcdFx0cGFyZW50LnRlYXJkb3duLmNhbGwodGhpcyk7XHJcblx0XHR9O1xyXG5cclxuXHRcdC8qKlxyXG5cdFx0ICogcHJlcGFyZSBhbmQgc2hvdyB0aGUgY29udGV4dCBtZW51IGZvciBhIG5vZGVcclxuXHRcdCAqIEBuYW1lIHNob3dfY29udGV4dG1lbnUob2JqIFssIHgsIHldKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqIHRoZSBub2RlXHJcblx0XHQgKiBAcGFyYW0ge051bWJlcn0geCB0aGUgeC1jb29yZGluYXRlIHJlbGF0aXZlIHRvIHRoZSBkb2N1bWVudCB0byBzaG93IHRoZSBtZW51IGF0XHJcblx0XHQgKiBAcGFyYW0ge051bWJlcn0geSB0aGUgeS1jb29yZGluYXRlIHJlbGF0aXZlIHRvIHRoZSBkb2N1bWVudCB0byBzaG93IHRoZSBtZW51IGF0XHJcblx0XHQgKiBAcGFyYW0ge09iamVjdH0gZSB0aGUgZXZlbnQgaWYgYXZhaWxhYmxlIHRoYXQgdHJpZ2dlcmVkIHRoZSBjb250ZXh0bWVudVxyXG5cdFx0ICogQHBsdWdpbiBjb250ZXh0bWVudVxyXG5cdFx0ICogQHRyaWdnZXIgc2hvd19jb250ZXh0bWVudS5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5zaG93X2NvbnRleHRtZW51ID0gZnVuY3Rpb24gKG9iaiwgeCwgeSwgZSkge1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCFvYmogfHwgb2JqLmlkID09PSAnIycpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdHZhciBzID0gdGhpcy5zZXR0aW5ncy5jb250ZXh0bWVudSxcclxuXHRcdFx0XHRkID0gdGhpcy5nZXRfbm9kZShvYmosIHRydWUpLFxyXG5cdFx0XHRcdGEgPSBkLmNoaWxkcmVuKFwiLmpzdHJlZS1hbmNob3JcIiksXHJcblx0XHRcdFx0byA9IGZhbHNlLFxyXG5cdFx0XHRcdGkgPSBmYWxzZTtcclxuXHRcdFx0aWYocy5zaG93X2F0X25vZGUgfHwgeCA9PT0gdW5kZWZpbmVkIHx8IHkgPT09IHVuZGVmaW5lZCkge1xyXG5cdFx0XHRcdG8gPSBhLm9mZnNldCgpO1xyXG5cdFx0XHRcdHggPSBvLmxlZnQ7XHJcblx0XHRcdFx0eSA9IG8udG9wICsgdGhpcy5fZGF0YS5jb3JlLmxpX2hlaWdodDtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZih0aGlzLnNldHRpbmdzLmNvbnRleHRtZW51LnNlbGVjdF9ub2RlICYmICF0aGlzLmlzX3NlbGVjdGVkKG9iaikpIHtcclxuXHRcdFx0XHR0aGlzLmRlc2VsZWN0X2FsbCgpO1xyXG5cdFx0XHRcdHRoaXMuc2VsZWN0X25vZGUob2JqLCBmYWxzZSwgZmFsc2UsIGUpO1xyXG5cdFx0XHR9XHJcblxyXG5cdFx0XHRpID0gcy5pdGVtcztcclxuXHRcdFx0aWYoJC5pc0Z1bmN0aW9uKGkpKSB7XHJcblx0XHRcdFx0aSA9IGkuY2FsbCh0aGlzLCBvYmosICQucHJveHkoZnVuY3Rpb24gKGkpIHtcclxuXHRcdFx0XHRcdHRoaXMuX3Nob3dfY29udGV4dG1lbnUob2JqLCB4LCB5LCBpKTtcclxuXHRcdFx0XHR9LCB0aGlzKSk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoJC5pc1BsYWluT2JqZWN0KGkpKSB7XHJcblx0XHRcdFx0dGhpcy5fc2hvd19jb250ZXh0bWVudShvYmosIHgsIHksIGkpO1xyXG5cdFx0XHR9XHJcblx0XHR9O1xyXG5cdFx0LyoqXHJcblx0XHQgKiBzaG93IHRoZSBwcmVwYXJlZCBjb250ZXh0IG1lbnUgZm9yIGEgbm9kZVxyXG5cdFx0ICogQG5hbWUgX3Nob3dfY29udGV4dG1lbnUob2JqLCB4LCB5LCBpKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqIHRoZSBub2RlXHJcblx0XHQgKiBAcGFyYW0ge051bWJlcn0geCB0aGUgeC1jb29yZGluYXRlIHJlbGF0aXZlIHRvIHRoZSBkb2N1bWVudCB0byBzaG93IHRoZSBtZW51IGF0XHJcblx0XHQgKiBAcGFyYW0ge051bWJlcn0geSB0aGUgeS1jb29yZGluYXRlIHJlbGF0aXZlIHRvIHRoZSBkb2N1bWVudCB0byBzaG93IHRoZSBtZW51IGF0XHJcblx0XHQgKiBAcGFyYW0ge051bWJlcn0gaSB0aGUgb2JqZWN0IG9mIGl0ZW1zIHRvIHNob3dcclxuXHRcdCAqIEBwbHVnaW4gY29udGV4dG1lbnVcclxuXHRcdCAqIEB0cmlnZ2VyIHNob3dfY29udGV4dG1lbnUuanN0cmVlXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICovXHJcblx0XHR0aGlzLl9zaG93X2NvbnRleHRtZW51ID0gZnVuY3Rpb24gKG9iaiwgeCwgeSwgaSkge1xyXG5cdFx0XHR2YXIgZCA9IHRoaXMuZ2V0X25vZGUob2JqLCB0cnVlKSxcclxuXHRcdFx0XHRhID0gZC5jaGlsZHJlbihcIi5qc3RyZWUtYW5jaG9yXCIpO1xyXG5cdFx0XHQkKGRvY3VtZW50KS5vbmUoXCJjb250ZXh0X3Nob3cudmFrYXRhXCIsICQucHJveHkoZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuXHRcdFx0XHR2YXIgY2xzID0gJ2pzdHJlZS1jb250ZXh0bWVudSBqc3RyZWUtJyArIHRoaXMuZ2V0X3RoZW1lKCkgKyAnLWNvbnRleHRtZW51JztcclxuXHRcdFx0XHQkKGRhdGEuZWxlbWVudCkuYWRkQ2xhc3MoY2xzKTtcclxuXHRcdFx0fSwgdGhpcykpO1xyXG5cdFx0XHR0aGlzLl9kYXRhLmNvbnRleHRtZW51LnZpc2libGUgPSB0cnVlO1xyXG5cdFx0XHQkLnZha2F0YS5jb250ZXh0LnNob3coYSwgeyAneCcgOiB4LCAneScgOiB5IH0sIGkpO1xyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIHdoZW4gdGhlIGNvbnRleHRtZW51IGlzIHNob3duIGZvciBhIG5vZGVcclxuXHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdCAqIEBuYW1lIHNob3dfY29udGV4dG1lbnUuanN0cmVlXHJcblx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBub2RlIHRoZSBub2RlXHJcblx0XHRcdCAqIEBwYXJhbSB7TnVtYmVyfSB4IHRoZSB4LWNvb3JkaW5hdGUgb2YgdGhlIG1lbnUgcmVsYXRpdmUgdG8gdGhlIGRvY3VtZW50XHJcblx0XHRcdCAqIEBwYXJhbSB7TnVtYmVyfSB5IHRoZSB5LWNvb3JkaW5hdGUgb2YgdGhlIG1lbnUgcmVsYXRpdmUgdG8gdGhlIGRvY3VtZW50XHJcblx0XHRcdCAqIEBwbHVnaW4gY29udGV4dG1lbnVcclxuXHRcdFx0ICovXHJcblx0XHRcdHRoaXMudHJpZ2dlcignc2hvd19jb250ZXh0bWVudScsIHsgXCJub2RlXCIgOiBvYmosIFwieFwiIDogeCwgXCJ5XCIgOiB5IH0pO1xyXG5cdFx0fTtcclxuXHR9O1xyXG5cclxuXHQvLyBjb250ZXh0bWVudSBoZWxwZXJcclxuXHQoZnVuY3Rpb24gKCQpIHtcclxuXHRcdHZhciByaWdodF90b19sZWZ0ID0gZmFsc2UsXHJcblx0XHRcdHZha2F0YV9jb250ZXh0ID0ge1xyXG5cdFx0XHRcdGVsZW1lbnRcdFx0OiBmYWxzZSxcclxuXHRcdFx0XHRyZWZlcmVuY2VcdDogZmFsc2UsXHJcblx0XHRcdFx0cG9zaXRpb25feFx0OiAwLFxyXG5cdFx0XHRcdHBvc2l0aW9uX3lcdDogMCxcclxuXHRcdFx0XHRpdGVtc1x0XHQ6IFtdLFxyXG5cdFx0XHRcdGh0bWxcdFx0OiBcIlwiLFxyXG5cdFx0XHRcdGlzX3Zpc2libGVcdDogZmFsc2VcclxuXHRcdFx0fTtcclxuXHJcblx0XHQkLnZha2F0YS5jb250ZXh0ID0ge1xyXG5cdFx0XHRzZXR0aW5ncyA6IHtcclxuXHRcdFx0XHRoaWRlX29ubW91c2VsZWF2ZVx0OiAwLFxyXG5cdFx0XHRcdGljb25zXHRcdFx0XHQ6IHRydWVcclxuXHRcdFx0fSxcclxuXHRcdFx0X3RyaWdnZXIgOiBmdW5jdGlvbiAoZXZlbnRfbmFtZSkge1xyXG5cdFx0XHRcdCQoZG9jdW1lbnQpLnRyaWdnZXJIYW5kbGVyKFwiY29udGV4dF9cIiArIGV2ZW50X25hbWUgKyBcIi52YWthdGFcIiwge1xyXG5cdFx0XHRcdFx0XCJyZWZlcmVuY2VcIlx0OiB2YWthdGFfY29udGV4dC5yZWZlcmVuY2UsXHJcblx0XHRcdFx0XHRcImVsZW1lbnRcIlx0OiB2YWthdGFfY29udGV4dC5lbGVtZW50LFxyXG5cdFx0XHRcdFx0XCJwb3NpdGlvblwiXHQ6IHtcclxuXHRcdFx0XHRcdFx0XCJ4XCIgOiB2YWthdGFfY29udGV4dC5wb3NpdGlvbl94LFxyXG5cdFx0XHRcdFx0XHRcInlcIiA6IHZha2F0YV9jb250ZXh0LnBvc2l0aW9uX3lcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0fSxcclxuXHRcdFx0X2V4ZWN1dGUgOiBmdW5jdGlvbiAoaSkge1xyXG5cdFx0XHRcdGkgPSB2YWthdGFfY29udGV4dC5pdGVtc1tpXTtcclxuXHRcdFx0XHRyZXR1cm4gaSAmJiAoIWkuX2Rpc2FibGVkIHx8ICgkLmlzRnVuY3Rpb24oaS5fZGlzYWJsZWQpICYmICFpLl9kaXNhYmxlZCh7IFwiaXRlbVwiIDogaSwgXCJyZWZlcmVuY2VcIiA6IHZha2F0YV9jb250ZXh0LnJlZmVyZW5jZSwgXCJlbGVtZW50XCIgOiB2YWthdGFfY29udGV4dC5lbGVtZW50IH0pKSkgJiYgaS5hY3Rpb24gPyBpLmFjdGlvbi5jYWxsKG51bGwsIHtcclxuXHRcdFx0XHRcdFx0XHRcIml0ZW1cIlx0XHQ6IGksXHJcblx0XHRcdFx0XHRcdFx0XCJyZWZlcmVuY2VcIlx0OiB2YWthdGFfY29udGV4dC5yZWZlcmVuY2UsXHJcblx0XHRcdFx0XHRcdFx0XCJlbGVtZW50XCJcdDogdmFrYXRhX2NvbnRleHQuZWxlbWVudCxcclxuXHRcdFx0XHRcdFx0XHRcInBvc2l0aW9uXCJcdDoge1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJ4XCIgOiB2YWthdGFfY29udGV4dC5wb3NpdGlvbl94LFxyXG5cdFx0XHRcdFx0XHRcdFx0XCJ5XCIgOiB2YWthdGFfY29udGV4dC5wb3NpdGlvbl95XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHR9KSA6IGZhbHNlO1xyXG5cdFx0XHR9LFxyXG5cdFx0XHRfcGFyc2UgOiBmdW5jdGlvbiAobywgaXNfY2FsbGJhY2spIHtcclxuXHRcdFx0XHRpZighbykgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0XHRpZighaXNfY2FsbGJhY2spIHtcclxuXHRcdFx0XHRcdHZha2F0YV9jb250ZXh0Lmh0bWxcdFx0PSBcIlwiO1xyXG5cdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQuaXRlbXNcdD0gW107XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHZhciBzdHIgPSBcIlwiLFxyXG5cdFx0XHRcdFx0c2VwID0gZmFsc2UsXHJcblx0XHRcdFx0XHR0bXA7XHJcblxyXG5cdFx0XHRcdGlmKGlzX2NhbGxiYWNrKSB7IHN0ciArPSBcIjxcIitcInVsPlwiOyB9XHJcblx0XHRcdFx0JC5lYWNoKG8sIGZ1bmN0aW9uIChpLCB2YWwpIHtcclxuXHRcdFx0XHRcdGlmKCF2YWwpIHsgcmV0dXJuIHRydWU7IH1cclxuXHRcdFx0XHRcdHZha2F0YV9jb250ZXh0Lml0ZW1zLnB1c2godmFsKTtcclxuXHRcdFx0XHRcdGlmKCFzZXAgJiYgdmFsLnNlcGFyYXRvcl9iZWZvcmUpIHtcclxuXHRcdFx0XHRcdFx0c3RyICs9IFwiPFwiK1wibGkgY2xhc3M9J3Zha2F0YS1jb250ZXh0LXNlcGFyYXRvcic+PFwiK1wiYSBocmVmPScjJyBcIiArICgkLnZha2F0YS5jb250ZXh0LnNldHRpbmdzLmljb25zID8gJycgOiAnc3R5bGU9XCJtYXJnaW4tbGVmdDowcHg7XCInKSArIFwiPiYjMTYwOzxcIitcIi9hPjxcIitcIi9saT5cIjtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdHNlcCA9IGZhbHNlO1xyXG5cdFx0XHRcdFx0c3RyICs9IFwiPFwiK1wibGkgY2xhc3M9J1wiICsgKHZhbC5fY2xhc3MgfHwgXCJcIikgKyAodmFsLl9kaXNhYmxlZCA9PT0gdHJ1ZSB8fCAoJC5pc0Z1bmN0aW9uKHZhbC5fZGlzYWJsZWQpICYmIHZhbC5fZGlzYWJsZWQoeyBcIml0ZW1cIiA6IHZhbCwgXCJyZWZlcmVuY2VcIiA6IHZha2F0YV9jb250ZXh0LnJlZmVyZW5jZSwgXCJlbGVtZW50XCIgOiB2YWthdGFfY29udGV4dC5lbGVtZW50IH0pKSA/IFwiIHZha2F0YS1jb250ZXh0bWVudS1kaXNhYmxlZCBcIiA6IFwiXCIpICsgXCInIFwiKyh2YWwuc2hvcnRjdXQ/XCIgZGF0YS1zaG9ydGN1dD0nXCIrdmFsLnNob3J0Y3V0K1wiJyBcIjonJykrXCI+XCI7XHJcblx0XHRcdFx0XHRzdHIgKz0gXCI8XCIrXCJhIGhyZWY9JyMnIHJlbD0nXCIgKyAodmFrYXRhX2NvbnRleHQuaXRlbXMubGVuZ3RoIC0gMSkgKyBcIic+XCI7XHJcblx0XHRcdFx0XHRpZigkLnZha2F0YS5jb250ZXh0LnNldHRpbmdzLmljb25zKSB7XHJcblx0XHRcdFx0XHRcdHN0ciArPSBcIjxcIitcImkgXCI7XHJcblx0XHRcdFx0XHRcdGlmKHZhbC5pY29uKSB7XHJcblx0XHRcdFx0XHRcdFx0aWYodmFsLmljb24uaW5kZXhPZihcIi9cIikgIT09IC0xIHx8IHZhbC5pY29uLmluZGV4T2YoXCIuXCIpICE9PSAtMSkgeyBzdHIgKz0gXCIgc3R5bGU9J2JhY2tncm91bmQ6dXJsKFxcXCJcIiArIHZhbC5pY29uICsgXCJcXFwiKSBjZW50ZXIgY2VudGVyIG5vLXJlcGVhdCcgXCI7IH1cclxuXHRcdFx0XHRcdFx0XHRlbHNlIHsgc3RyICs9IFwiIGNsYXNzPSdcIiArIHZhbC5pY29uICsgXCInIFwiOyB9XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0c3RyICs9IFwiPjxcIitcIi9pPjxcIitcInNwYW4gY2xhc3M9J3Zha2F0YS1jb250ZXh0bWVudS1zZXAnPiYjMTYwOzxcIitcIi9zcGFuPlwiO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0c3RyICs9IHZhbC5sYWJlbCArICh2YWwuc2hvcnRjdXQ/JyA8c3BhbiBjbGFzcz1cInZha2F0YS1jb250ZXh0bWVudS1zaG9ydGN1dCB2YWthdGEtY29udGV4dG1lbnUtc2hvcnRjdXQtJyt2YWwuc2hvcnRjdXQrJ1wiPicrICh2YWwuc2hvcnRjdXRfbGFiZWwgfHwgJycpICsnPC9zcGFuPic6JycpICsgXCI8XCIrXCIvYT5cIjtcclxuXHRcdFx0XHRcdGlmKHZhbC5zdWJtZW51KSB7XHJcblx0XHRcdFx0XHRcdHRtcCA9ICQudmFrYXRhLmNvbnRleHQuX3BhcnNlKHZhbC5zdWJtZW51LCB0cnVlKTtcclxuXHRcdFx0XHRcdFx0aWYodG1wKSB7IHN0ciArPSB0bXA7IH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdHN0ciArPSBcIjxcIitcIi9saT5cIjtcclxuXHRcdFx0XHRcdGlmKHZhbC5zZXBhcmF0b3JfYWZ0ZXIpIHtcclxuXHRcdFx0XHRcdFx0c3RyICs9IFwiPFwiK1wibGkgY2xhc3M9J3Zha2F0YS1jb250ZXh0LXNlcGFyYXRvcic+PFwiK1wiYSBocmVmPScjJyBcIiArICgkLnZha2F0YS5jb250ZXh0LnNldHRpbmdzLmljb25zID8gJycgOiAnc3R5bGU9XCJtYXJnaW4tbGVmdDowcHg7XCInKSArIFwiPiYjMTYwOzxcIitcIi9hPjxcIitcIi9saT5cIjtcclxuXHRcdFx0XHRcdFx0c2VwID0gdHJ1ZTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0XHRzdHIgID0gc3RyLnJlcGxhY2UoLzxsaSBjbGFzc1xcPSd2YWthdGEtY29udGV4dC1zZXBhcmF0b3InXFw+PFxcL2xpXFw+JC8sXCJcIik7XHJcblx0XHRcdFx0aWYoaXNfY2FsbGJhY2spIHsgc3RyICs9IFwiPC91bD5cIjsgfVxyXG5cdFx0XHRcdC8qKlxyXG5cdFx0XHRcdCAqIHRyaWdnZXJlZCBvbiB0aGUgZG9jdW1lbnQgd2hlbiB0aGUgY29udGV4dG1lbnUgaXMgcGFyc2VkIChIVE1MIGlzIGJ1aWx0KVxyXG5cdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdCAqIEBwbHVnaW4gY29udGV4dG1lbnVcclxuXHRcdFx0XHQgKiBAbmFtZSBjb250ZXh0X3BhcnNlLnZha2F0YVxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7alF1ZXJ5fSByZWZlcmVuY2UgdGhlIGVsZW1lbnQgdGhhdCB3YXMgcmlnaHQgY2xpY2tlZFxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7alF1ZXJ5fSBlbGVtZW50IHRoZSBET00gZWxlbWVudCBvZiB0aGUgbWVudSBpdHNlbGZcclxuXHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gcG9zaXRpb24gdGhlIHggJiB5IGNvb3JkaW5hdGVzIG9mIHRoZSBtZW51XHJcblx0XHRcdFx0ICovXHJcblx0XHRcdFx0aWYoIWlzX2NhbGxiYWNrKSB7IHZha2F0YV9jb250ZXh0Lmh0bWwgPSBzdHI7ICQudmFrYXRhLmNvbnRleHQuX3RyaWdnZXIoXCJwYXJzZVwiKTsgfVxyXG5cdFx0XHRcdHJldHVybiBzdHIubGVuZ3RoID4gMTAgPyBzdHIgOiBmYWxzZTtcclxuXHRcdFx0fSxcclxuXHRcdFx0X3Nob3dfc3VibWVudSA6IGZ1bmN0aW9uIChvKSB7XHJcblx0XHRcdFx0byA9ICQobyk7XHJcblx0XHRcdFx0aWYoIW8ubGVuZ3RoIHx8ICFvLmNoaWxkcmVuKFwidWxcIikubGVuZ3RoKSB7IHJldHVybjsgfVxyXG5cdFx0XHRcdHZhciBlID0gby5jaGlsZHJlbihcInVsXCIpLFxyXG5cdFx0XHRcdFx0eCA9IG8ub2Zmc2V0KCkubGVmdCArIG8ub3V0ZXJXaWR0aCgpLFxyXG5cdFx0XHRcdFx0eSA9IG8ub2Zmc2V0KCkudG9wLFxyXG5cdFx0XHRcdFx0dyA9IGUud2lkdGgoKSxcclxuXHRcdFx0XHRcdGggPSBlLmhlaWdodCgpLFxyXG5cdFx0XHRcdFx0ZHcgPSAkKHdpbmRvdykud2lkdGgoKSArICQod2luZG93KS5zY3JvbGxMZWZ0KCksXHJcblx0XHRcdFx0XHRkaCA9ICQod2luZG93KS5oZWlnaHQoKSArICQod2luZG93KS5zY3JvbGxUb3AoKTtcclxuXHRcdFx0XHQvLyDQvNC+0LbQtSDQtNCwINGB0LUg0YHQv9C10YHRgtC4INC1INC10LTQvdCwINC/0YDQvtCy0LXRgNC60LAgLSDQtNCw0LvQuCDQvdGP0LzQsCDQvdGP0LrQvtC5INC+0YIg0LrQu9Cw0YHQvtCy0LXRgtC1INCy0LXRh9C1INC90LDQs9C+0YDQtVxyXG5cdFx0XHRcdGlmKHJpZ2h0X3RvX2xlZnQpIHtcclxuXHRcdFx0XHRcdG9beCAtICh3ICsgMTAgKyBvLm91dGVyV2lkdGgoKSkgPCAwID8gXCJhZGRDbGFzc1wiIDogXCJyZW1vdmVDbGFzc1wiXShcInZha2F0YS1jb250ZXh0LWxlZnRcIik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdFx0b1t4ICsgdyArIDEwID4gZHcgPyBcImFkZENsYXNzXCIgOiBcInJlbW92ZUNsYXNzXCJdKFwidmFrYXRhLWNvbnRleHQtcmlnaHRcIik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGlmKHkgKyBoICsgMTAgPiBkaCkge1xyXG5cdFx0XHRcdFx0ZS5jc3MoXCJib3R0b21cIixcIi0xcHhcIik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGUuc2hvdygpO1xyXG5cdFx0XHR9LFxyXG5cdFx0XHRzaG93IDogZnVuY3Rpb24gKHJlZmVyZW5jZSwgcG9zaXRpb24sIGRhdGEpIHtcclxuXHRcdFx0XHR2YXIgbywgZSwgeCwgeSwgdywgaCwgZHcsIGRoLCBjb25kID0gdHJ1ZTtcclxuXHRcdFx0XHRpZih2YWthdGFfY29udGV4dC5lbGVtZW50ICYmIHZha2F0YV9jb250ZXh0LmVsZW1lbnQubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHR2YWthdGFfY29udGV4dC5lbGVtZW50LndpZHRoKCcnKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0c3dpdGNoKGNvbmQpIHtcclxuXHRcdFx0XHRcdGNhc2UgKCFwb3NpdGlvbiAmJiAhcmVmZXJlbmNlKTpcclxuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdFx0Y2FzZSAoISFwb3NpdGlvbiAmJiAhIXJlZmVyZW5jZSk6XHJcblx0XHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LnJlZmVyZW5jZVx0PSByZWZlcmVuY2U7XHJcblx0XHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LnBvc2l0aW9uX3hcdD0gcG9zaXRpb24ueDtcclxuXHRcdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQucG9zaXRpb25feVx0PSBwb3NpdGlvbi55O1xyXG5cdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdGNhc2UgKCFwb3NpdGlvbiAmJiAhIXJlZmVyZW5jZSk6XHJcblx0XHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LnJlZmVyZW5jZVx0PSByZWZlcmVuY2U7XHJcblx0XHRcdFx0XHRcdG8gPSByZWZlcmVuY2Uub2Zmc2V0KCk7XHJcblx0XHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LnBvc2l0aW9uX3hcdD0gby5sZWZ0ICsgcmVmZXJlbmNlLm91dGVySGVpZ2h0KCk7XHJcblx0XHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LnBvc2l0aW9uX3lcdD0gby50b3A7XHJcblx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0Y2FzZSAoISFwb3NpdGlvbiAmJiAhcmVmZXJlbmNlKTpcclxuXHRcdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQucG9zaXRpb25feFx0PSBwb3NpdGlvbi54O1xyXG5cdFx0XHRcdFx0XHR2YWthdGFfY29udGV4dC5wb3NpdGlvbl95XHQ9IHBvc2l0aW9uLnk7XHJcblx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRpZighIXJlZmVyZW5jZSAmJiAhZGF0YSAmJiAkKHJlZmVyZW5jZSkuZGF0YSgndmFrYXRhX2NvbnRleHRtZW51JykpIHtcclxuXHRcdFx0XHRcdGRhdGEgPSAkKHJlZmVyZW5jZSkuZGF0YSgndmFrYXRhX2NvbnRleHRtZW51Jyk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGlmKCQudmFrYXRhLmNvbnRleHQuX3BhcnNlKGRhdGEpKSB7XHJcblx0XHRcdFx0XHR2YWthdGFfY29udGV4dC5lbGVtZW50Lmh0bWwodmFrYXRhX2NvbnRleHQuaHRtbCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGlmKHZha2F0YV9jb250ZXh0Lml0ZW1zLmxlbmd0aCkge1xyXG5cdFx0XHRcdFx0ZSA9IHZha2F0YV9jb250ZXh0LmVsZW1lbnQ7XHJcblx0XHRcdFx0XHR4ID0gdmFrYXRhX2NvbnRleHQucG9zaXRpb25feDtcclxuXHRcdFx0XHRcdHkgPSB2YWthdGFfY29udGV4dC5wb3NpdGlvbl95O1xyXG5cdFx0XHRcdFx0dyA9IGUud2lkdGgoKTtcclxuXHRcdFx0XHRcdGggPSBlLmhlaWdodCgpO1xyXG5cdFx0XHRcdFx0ZHcgPSAkKHdpbmRvdykud2lkdGgoKSArICQod2luZG93KS5zY3JvbGxMZWZ0KCk7XHJcblx0XHRcdFx0XHRkaCA9ICQod2luZG93KS5oZWlnaHQoKSArICQod2luZG93KS5zY3JvbGxUb3AoKTtcclxuXHRcdFx0XHRcdGlmKHJpZ2h0X3RvX2xlZnQpIHtcclxuXHRcdFx0XHRcdFx0eCAtPSBlLm91dGVyV2lkdGgoKTtcclxuXHRcdFx0XHRcdFx0aWYoeCA8ICQod2luZG93KS5zY3JvbGxMZWZ0KCkgKyAyMCkge1xyXG5cdFx0XHRcdFx0XHRcdHggPSAkKHdpbmRvdykuc2Nyb2xsTGVmdCgpICsgMjA7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGlmKHggKyB3ICsgMjAgPiBkdykge1xyXG5cdFx0XHRcdFx0XHR4ID0gZHcgLSAodyArIDIwKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGlmKHkgKyBoICsgMjAgPiBkaCkge1xyXG5cdFx0XHRcdFx0XHR5ID0gZGggLSAoaCArIDIwKTtcclxuXHRcdFx0XHRcdH1cclxuXHJcblx0XHRcdFx0XHR2YWthdGFfY29udGV4dC5lbGVtZW50XHJcblx0XHRcdFx0XHRcdC5jc3MoeyBcImxlZnRcIiA6IHgsIFwidG9wXCIgOiB5IH0pXHJcblx0XHRcdFx0XHRcdC5zaG93KClcclxuXHRcdFx0XHRcdFx0LmZpbmQoJ2E6ZXEoMCknKS5mb2N1cygpLnBhcmVudCgpLmFkZENsYXNzKFwidmFrYXRhLWNvbnRleHQtaG92ZXJcIik7XHJcblx0XHRcdFx0XHR2YWthdGFfY29udGV4dC5pc192aXNpYmxlID0gdHJ1ZTtcclxuXHRcdFx0XHRcdC8qKlxyXG5cdFx0XHRcdFx0ICogdHJpZ2dlcmVkIG9uIHRoZSBkb2N1bWVudCB3aGVuIHRoZSBjb250ZXh0bWVudSBpcyBzaG93blxyXG5cdFx0XHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdFx0XHQgKiBAcGx1Z2luIGNvbnRleHRtZW51XHJcblx0XHRcdFx0XHQgKiBAbmFtZSBjb250ZXh0X3Nob3cudmFrYXRhXHJcblx0XHRcdFx0XHQgKiBAcGFyYW0ge2pRdWVyeX0gcmVmZXJlbmNlIHRoZSBlbGVtZW50IHRoYXQgd2FzIHJpZ2h0IGNsaWNrZWRcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7alF1ZXJ5fSBlbGVtZW50IHRoZSBET00gZWxlbWVudCBvZiB0aGUgbWVudSBpdHNlbGZcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBwb3NpdGlvbiB0aGUgeCAmIHkgY29vcmRpbmF0ZXMgb2YgdGhlIG1lbnVcclxuXHRcdFx0XHRcdCAqL1xyXG5cdFx0XHRcdFx0JC52YWthdGEuY29udGV4dC5fdHJpZ2dlcihcInNob3dcIik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9LFxyXG5cdFx0XHRoaWRlIDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdGlmKHZha2F0YV9jb250ZXh0LmlzX3Zpc2libGUpIHtcclxuXHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LmVsZW1lbnQuaGlkZSgpLmZpbmQoXCJ1bFwiKS5oaWRlKCkuZW5kKCkuZmluZCgnOmZvY3VzJykuYmx1cigpO1xyXG5cdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQuaXNfdmlzaWJsZSA9IGZhbHNlO1xyXG5cdFx0XHRcdFx0LyoqXHJcblx0XHRcdFx0XHQgKiB0cmlnZ2VyZWQgb24gdGhlIGRvY3VtZW50IHdoZW4gdGhlIGNvbnRleHRtZW51IGlzIGhpZGRlblxyXG5cdFx0XHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdFx0XHQgKiBAcGx1Z2luIGNvbnRleHRtZW51XHJcblx0XHRcdFx0XHQgKiBAbmFtZSBjb250ZXh0X2hpZGUudmFrYXRhXHJcblx0XHRcdFx0XHQgKiBAcGFyYW0ge2pRdWVyeX0gcmVmZXJlbmNlIHRoZSBlbGVtZW50IHRoYXQgd2FzIHJpZ2h0IGNsaWNrZWRcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7alF1ZXJ5fSBlbGVtZW50IHRoZSBET00gZWxlbWVudCBvZiB0aGUgbWVudSBpdHNlbGZcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBwb3NpdGlvbiB0aGUgeCAmIHkgY29vcmRpbmF0ZXMgb2YgdGhlIG1lbnVcclxuXHRcdFx0XHRcdCAqL1xyXG5cdFx0XHRcdFx0JC52YWthdGEuY29udGV4dC5fdHJpZ2dlcihcImhpZGVcIik7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHR9O1xyXG5cdFx0JChmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHJpZ2h0X3RvX2xlZnQgPSAkKFwiYm9keVwiKS5jc3MoXCJkaXJlY3Rpb25cIikgPT09IFwicnRsXCI7XHJcblx0XHRcdHZhciB0byA9IGZhbHNlO1xyXG5cclxuXHRcdFx0dmFrYXRhX2NvbnRleHQuZWxlbWVudCA9ICQoXCI8dWwgY2xhc3M9J3Zha2F0YS1jb250ZXh0Jz48L3VsPlwiKTtcclxuXHRcdFx0dmFrYXRhX2NvbnRleHQuZWxlbWVudFxyXG5cdFx0XHRcdC5vbihcIm1vdXNlZW50ZXJcIiwgXCJsaVwiLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0ZS5zdG9wSW1tZWRpYXRlUHJvcGFnYXRpb24oKTtcclxuXHJcblx0XHRcdFx0XHRpZigkLmNvbnRhaW5zKHRoaXMsIGUucmVsYXRlZFRhcmdldCkpIHtcclxuXHRcdFx0XHRcdFx0Ly8g0L/RgNC10LzQsNGF0L3QsNGC0L4g0LfQsNGA0LDQtNC4IGRlbGVnYXRlIG1vdXNlbGVhdmUg0L/Qvi3QtNC+0LvRg1xyXG5cdFx0XHRcdFx0XHQvLyAkKHRoaXMpLmZpbmQoXCIudmFrYXRhLWNvbnRleHQtaG92ZXJcIikucmVtb3ZlQ2xhc3MoXCJ2YWthdGEtY29udGV4dC1ob3ZlclwiKTtcclxuXHRcdFx0XHRcdFx0cmV0dXJuO1xyXG5cdFx0XHRcdFx0fVxyXG5cclxuXHRcdFx0XHRcdGlmKHRvKSB7IGNsZWFyVGltZW91dCh0byk7IH1cclxuXHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LmVsZW1lbnQuZmluZChcIi52YWthdGEtY29udGV4dC1ob3ZlclwiKS5yZW1vdmVDbGFzcyhcInZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmVuZCgpO1xyXG5cclxuXHRcdFx0XHRcdCQodGhpcylcclxuXHRcdFx0XHRcdFx0LnNpYmxpbmdzKCkuZmluZChcInVsXCIpLmhpZGUoKS5lbmQoKS5lbmQoKVxyXG5cdFx0XHRcdFx0XHQucGFyZW50c1VudGlsKFwiLnZha2F0YS1jb250ZXh0XCIsIFwibGlcIikuYWRkQmFjaygpLmFkZENsYXNzKFwidmFrYXRhLWNvbnRleHQtaG92ZXJcIik7XHJcblx0XHRcdFx0XHQkLnZha2F0YS5jb250ZXh0Ll9zaG93X3N1Ym1lbnUodGhpcyk7XHJcblx0XHRcdFx0fSlcclxuXHRcdFx0XHQvLyDRgtC10YHRgtC+0LLQviAtINC00LDQu9C4INC90LUg0L3QsNGC0L7QstCw0YDQstCwP1xyXG5cdFx0XHRcdC5vbihcIm1vdXNlbGVhdmVcIiwgXCJsaVwiLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0aWYoJC5jb250YWlucyh0aGlzLCBlLnJlbGF0ZWRUYXJnZXQpKSB7IHJldHVybjsgfVxyXG5cdFx0XHRcdFx0JCh0aGlzKS5maW5kKFwiLnZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmFkZEJhY2soKS5yZW1vdmVDbGFzcyhcInZha2F0YS1jb250ZXh0LWhvdmVyXCIpO1xyXG5cdFx0XHRcdH0pXHJcblx0XHRcdFx0Lm9uKFwibW91c2VsZWF2ZVwiLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0JCh0aGlzKS5maW5kKFwiLnZha2F0YS1jb250ZXh0LWhvdmVyXCIpLnJlbW92ZUNsYXNzKFwidmFrYXRhLWNvbnRleHQtaG92ZXJcIik7XHJcblx0XHRcdFx0XHRpZigkLnZha2F0YS5jb250ZXh0LnNldHRpbmdzLmhpZGVfb25tb3VzZWxlYXZlKSB7XHJcblx0XHRcdFx0XHRcdHRvID0gc2V0VGltZW91dChcclxuXHRcdFx0XHRcdFx0XHQoZnVuY3Rpb24gKHQpIHtcclxuXHRcdFx0XHRcdFx0XHRcdHJldHVybiBmdW5jdGlvbiAoKSB7ICQudmFrYXRhLmNvbnRleHQuaGlkZSgpOyB9O1xyXG5cdFx0XHRcdFx0XHRcdH0odGhpcykpLCAkLnZha2F0YS5jb250ZXh0LnNldHRpbmdzLmhpZGVfb25tb3VzZWxlYXZlKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9KVxyXG5cdFx0XHRcdC5vbihcImNsaWNrXCIsIFwiYVwiLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdH0pXHJcblx0XHRcdFx0Lm9uKFwibW91c2V1cFwiLCBcImFcIiwgZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRcdGlmKCEkKHRoaXMpLmJsdXIoKS5wYXJlbnQoKS5oYXNDbGFzcyhcInZha2F0YS1jb250ZXh0LWRpc2FibGVkXCIpICYmICQudmFrYXRhLmNvbnRleHQuX2V4ZWN1dGUoJCh0aGlzKS5hdHRyKFwicmVsXCIpKSAhPT0gZmFsc2UpIHtcclxuXHRcdFx0XHRcdFx0JC52YWthdGEuY29udGV4dC5oaWRlKCk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fSlcclxuXHRcdFx0XHQub24oJ2tleWRvd24nLCAnYScsIGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdHZhciBvID0gbnVsbDtcclxuXHRcdFx0XHRcdFx0c3dpdGNoKGUud2hpY2gpIHtcclxuXHRcdFx0XHRcdFx0XHRjYXNlIDEzOlxyXG5cdFx0XHRcdFx0XHRcdGNhc2UgMzI6XHJcblx0XHRcdFx0XHRcdFx0XHRlLnR5cGUgPSBcIm1vdXNldXBcIjtcclxuXHRcdFx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcclxuXHRcdFx0XHRcdFx0XHRcdCQoZS5jdXJyZW50VGFyZ2V0KS50cmlnZ2VyKGUpO1xyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0Y2FzZSAzNzpcclxuXHRcdFx0XHRcdFx0XHRcdGlmKHZha2F0YV9jb250ZXh0LmlzX3Zpc2libGUpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQuZWxlbWVudC5maW5kKFwiLnZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmxhc3QoKS5wYXJlbnRzKFwibGk6ZXEoMClcIikuZmluZChcInVsXCIpLmhpZGUoKS5maW5kKFwiLnZha2F0YS1jb250ZXh0LWhvdmVyXCIpLnJlbW92ZUNsYXNzKFwidmFrYXRhLWNvbnRleHQtaG92ZXJcIikuZW5kKCkuZW5kKCkuY2hpbGRyZW4oJ2EnKS5mb2N1cygpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRlLnN0b3BJbW1lZGlhdGVQcm9wYWdhdGlvbigpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XHJcblx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRjYXNlIDM4OlxyXG5cdFx0XHRcdFx0XHRcdFx0aWYodmFrYXRhX2NvbnRleHQuaXNfdmlzaWJsZSkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRvID0gdmFrYXRhX2NvbnRleHQuZWxlbWVudC5maW5kKFwidWw6dmlzaWJsZVwiKS5hZGRCYWNrKCkubGFzdCgpLmNoaWxkcmVuKFwiLnZha2F0YS1jb250ZXh0LWhvdmVyXCIpLnJlbW92ZUNsYXNzKFwidmFrYXRhLWNvbnRleHQtaG92ZXJcIikucHJldkFsbChcImxpOm5vdCgudmFrYXRhLWNvbnRleHQtc2VwYXJhdG9yKVwiKS5maXJzdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRpZighby5sZW5ndGgpIHsgbyA9IHZha2F0YV9jb250ZXh0LmVsZW1lbnQuZmluZChcInVsOnZpc2libGVcIikuYWRkQmFjaygpLmxhc3QoKS5jaGlsZHJlbihcImxpOm5vdCgudmFrYXRhLWNvbnRleHQtc2VwYXJhdG9yKVwiKS5sYXN0KCk7IH1cclxuXHRcdFx0XHRcdFx0XHRcdFx0by5hZGRDbGFzcyhcInZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmNoaWxkcmVuKCdhJykuZm9jdXMoKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0ZS5zdG9wSW1tZWRpYXRlUHJvcGFnYXRpb24oKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0Y2FzZSAzOTpcclxuXHRcdFx0XHRcdFx0XHRcdGlmKHZha2F0YV9jb250ZXh0LmlzX3Zpc2libGUpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQuZWxlbWVudC5maW5kKFwiLnZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmxhc3QoKS5jaGlsZHJlbihcInVsXCIpLnNob3coKS5jaGlsZHJlbihcImxpOm5vdCgudmFrYXRhLWNvbnRleHQtc2VwYXJhdG9yKVwiKS5yZW1vdmVDbGFzcyhcInZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmZpcnN0KCkuYWRkQ2xhc3MoXCJ2YWthdGEtY29udGV4dC1ob3ZlclwiKS5jaGlsZHJlbignYScpLmZvY3VzKCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGUuc3RvcEltbWVkaWF0ZVByb3BhZ2F0aW9uKCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdGNhc2UgNDA6XHJcblx0XHRcdFx0XHRcdFx0XHRpZih2YWthdGFfY29udGV4dC5pc192aXNpYmxlKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdG8gPSB2YWthdGFfY29udGV4dC5lbGVtZW50LmZpbmQoXCJ1bDp2aXNpYmxlXCIpLmFkZEJhY2soKS5sYXN0KCkuY2hpbGRyZW4oXCIudmFrYXRhLWNvbnRleHQtaG92ZXJcIikucmVtb3ZlQ2xhc3MoXCJ2YWthdGEtY29udGV4dC1ob3ZlclwiKS5uZXh0QWxsKFwibGk6bm90KC52YWthdGEtY29udGV4dC1zZXBhcmF0b3IpXCIpLmZpcnN0KCk7XHJcblx0XHRcdFx0XHRcdFx0XHRcdGlmKCFvLmxlbmd0aCkgeyBvID0gdmFrYXRhX2NvbnRleHQuZWxlbWVudC5maW5kKFwidWw6dmlzaWJsZVwiKS5hZGRCYWNrKCkubGFzdCgpLmNoaWxkcmVuKFwibGk6bm90KC52YWthdGEtY29udGV4dC1zZXBhcmF0b3IpXCIpLmZpcnN0KCk7IH1cclxuXHRcdFx0XHRcdFx0XHRcdFx0by5hZGRDbGFzcyhcInZha2F0YS1jb250ZXh0LWhvdmVyXCIpLmNoaWxkcmVuKCdhJykuZm9jdXMoKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0ZS5zdG9wSW1tZWRpYXRlUHJvcGFnYXRpb24oKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0Y2FzZSAyNzpcclxuXHRcdFx0XHRcdFx0XHRcdCQudmFrYXRhLmNvbnRleHQuaGlkZSgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0XHRcdFx0ZGVmYXVsdDpcclxuXHRcdFx0XHRcdFx0XHRcdC8vY29uc29sZS5sb2coZS53aGljaCk7XHJcblx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fSlcclxuXHRcdFx0XHQub24oJ2tleWRvd24nLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdFx0dmFyIGEgPSB2YWthdGFfY29udGV4dC5lbGVtZW50LmZpbmQoJy52YWthdGEtY29udGV4dG1lbnUtc2hvcnRjdXQtJyArIGUud2hpY2gpLnBhcmVudCgpO1xyXG5cdFx0XHRcdFx0aWYoYS5wYXJlbnQoKS5ub3QoJy52YWthdGEtY29udGV4dC1kaXNhYmxlZCcpKSB7XHJcblx0XHRcdFx0XHRcdGEubW91c2V1cCgpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH0pXHJcblx0XHRcdFx0LmFwcGVuZFRvKFwiYm9keVwiKTtcclxuXHJcblx0XHRcdCQoZG9jdW1lbnQpXHJcblx0XHRcdFx0Lm9uKFwibW91c2Vkb3duXCIsIGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRpZih2YWthdGFfY29udGV4dC5pc192aXNpYmxlICYmICEkLmNvbnRhaW5zKHZha2F0YV9jb250ZXh0LmVsZW1lbnRbMF0sIGUudGFyZ2V0KSkgeyAkLnZha2F0YS5jb250ZXh0LmhpZGUoKTsgfVxyXG5cdFx0XHRcdH0pXHJcblx0XHRcdFx0Lm9uKFwiY29udGV4dF9zaG93LnZha2F0YVwiLCBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0dmFrYXRhX2NvbnRleHQuZWxlbWVudC5maW5kKFwibGk6aGFzKHVsKVwiKS5jaGlsZHJlbihcImFcIikuYWRkQ2xhc3MoXCJ2YWthdGEtY29udGV4dC1wYXJlbnRcIik7XHJcblx0XHRcdFx0XHRpZihyaWdodF90b19sZWZ0KSB7XHJcblx0XHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LmVsZW1lbnQuYWRkQ2xhc3MoXCJ2YWthdGEtY29udGV4dC1ydGxcIikuY3NzKFwiZGlyZWN0aW9uXCIsIFwicnRsXCIpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0Ly8gYWxzbyBhcHBseSBhIFJUTCBjbGFzcz9cclxuXHRcdFx0XHRcdHZha2F0YV9jb250ZXh0LmVsZW1lbnQuZmluZChcInVsXCIpLmhpZGUoKS5lbmQoKTtcclxuXHRcdFx0XHR9KTtcclxuXHRcdH0pO1xyXG5cdH0oJCkpO1xyXG5cdC8vICQuanN0cmVlLmRlZmF1bHRzLnBsdWdpbnMucHVzaChcImNvbnRleHRtZW51XCIpO1xyXG5cclxuLyoqXHJcbiAqICMjIyBEcmFnJ24nZHJvcCBwbHVnaW5cclxuICpcclxuICogRW5hYmxlcyBkcmFnZ2luZyBhbmQgZHJvcHBpbmcgb2Ygbm9kZXMgaW4gdGhlIHRyZWUsIHJlc3VsdGluZyBpbiBhIG1vdmUgb3IgY29weSBvcGVyYXRpb25zLlxyXG4gKi9cclxuXHJcblx0LyoqXHJcblx0ICogc3RvcmVzIGFsbCBkZWZhdWx0cyBmb3IgdGhlIGRyYWcnbidkcm9wIHBsdWdpblxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmRuZFxyXG5cdCAqIEBwbHVnaW4gZG5kXHJcblx0ICovXHJcblx0JC5qc3RyZWUuZGVmYXVsdHMuZG5kID0ge1xyXG5cdFx0LyoqXHJcblx0XHQgKiBhIGJvb2xlYW4gaW5kaWNhdGluZyBpZiBhIGNvcHkgc2hvdWxkIGJlIHBvc3NpYmxlIHdoaWxlIGRyYWdnaW5nIChieSBwcmVzc2ludCB0aGUgbWV0YSBrZXkgb3IgQ3RybCkuIERlZmF1bHRzIHRvIGB0cnVlYC5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLmRuZC5jb3B5XHJcblx0XHQgKiBAcGx1Z2luIGRuZFxyXG5cdFx0ICovXHJcblx0XHRjb3B5IDogdHJ1ZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogYSBudW1iZXIgaW5kaWNhdGluZyBob3cgbG9uZyBhIG5vZGUgc2hvdWxkIHJlbWFpbiBob3ZlcmVkIHdoaWxlIGRyYWdnaW5nIHRvIGJlIG9wZW5lZC4gRGVmYXVsdHMgdG8gYDUwMGAuXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5kbmQub3Blbl90aW1lb3V0XHJcblx0XHQgKiBAcGx1Z2luIGRuZFxyXG5cdFx0ICovXHJcblx0XHRvcGVuX3RpbWVvdXQgOiA1MDAsXHJcblx0XHQvKipcclxuXHRcdCAqIGEgZnVuY3Rpb24gaW52b2tlZCBlYWNoIHRpbWUgYSBub2RlIGlzIGFib3V0IHRvIGJlIGRyYWdnZWQsIGludm9rZWQgaW4gdGhlIHRyZWUncyBzY29wZSBhbmQgcmVjZWl2ZXMgdGhlIG5vZGUgYXMgYW4gYXJndW1lbnQgLSByZXR1cm4gYGZhbHNlYCB0byBwcmV2ZW50IGRyYWdnaW5nXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5kbmQuaXNfZHJhZ2dhYmxlXHJcblx0XHQgKiBAcGx1Z2luIGRuZFxyXG5cdFx0ICovXHJcblx0XHRpc19kcmFnZ2FibGUgOiB0cnVlLFxyXG5cdFx0LyoqXHJcblx0XHQgKiBhIGJvb2xlYW4gaW5kaWNhdGluZyBpZiBjaGVja3Mgc2hvdWxkIGNvbnN0YW50bHkgYmUgbWFkZSB3aGlsZSB0aGUgdXNlciBpcyBkcmFnZ2luZyB0aGUgbm9kZSAoYXMgb3Bwb3NlZCB0byBjaGVja2luZyBvbmx5IG9uIGRyb3ApLCBkZWZhdWx0IGlzIGB0cnVlYFxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuZG5kLmNoZWNrX3doaWxlX2RyYWdnaW5nXHJcblx0XHQgKiBAcGx1Z2luIGRuZFxyXG5cdFx0ICovXHJcblx0XHRjaGVja193aGlsZV9kcmFnZ2luZyA6IHRydWVcclxuXHR9O1xyXG5cdC8vIFRPRE86IG5vdyBjaGVjayB3b3JrcyBieSBjaGVja2luZyBmb3IgZWFjaCBub2RlIGluZGl2aWR1YWxseSwgaG93IGFib3V0IG1heF9jaGlsZHJlbiwgdW5pcXVlLCBldGM/XHJcblx0Ly8gVE9ETzogZHJvcCBzb21ld2hlcmUgZWxzZSAtIG1heWJlIGRlbW8gb25seT9cclxuXHQkLmpzdHJlZS5wbHVnaW5zLmRuZCA9IGZ1bmN0aW9uIChvcHRpb25zLCBwYXJlbnQpIHtcclxuXHRcdHRoaXMuYmluZCA9IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0cGFyZW50LmJpbmQuY2FsbCh0aGlzKTtcclxuXHJcblx0XHRcdHRoaXMuZWxlbWVudFxyXG5cdFx0XHRcdC5vbignbW91c2Vkb3duIHRvdWNoc3RhcnQnLCAnLmpzdHJlZS1hbmNob3InLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHR2YXIgb2JqID0gdGhpcy5nZXRfbm9kZShlLnRhcmdldCksXHJcblx0XHRcdFx0XHRcdG1sdCA9IHRoaXMuaXNfc2VsZWN0ZWQob2JqKSA/IHRoaXMuZ2V0X3NlbGVjdGVkKCkubGVuZ3RoIDogMTtcclxuXHRcdFx0XHRcdGlmKG9iaiAmJiBvYmouaWQgJiYgb2JqLmlkICE9PSBcIiNcIiAmJiAoZS53aGljaCA9PT0gMSB8fCBlLnR5cGUgPT09IFwidG91Y2hzdGFydFwiKSAmJlxyXG5cdFx0XHRcdFx0XHQodGhpcy5zZXR0aW5ncy5kbmQuaXNfZHJhZ2dhYmxlID09PSB0cnVlIHx8ICgkLmlzRnVuY3Rpb24odGhpcy5zZXR0aW5ncy5kbmQuaXNfZHJhZ2dhYmxlKSAmJiB0aGlzLnNldHRpbmdzLmRuZC5pc19kcmFnZ2FibGUuY2FsbCh0aGlzLCBvYmopKSlcclxuXHRcdFx0XHRcdCkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmVsZW1lbnQudHJpZ2dlcignbW91c2Vkb3duLmpzdHJlZScpO1xyXG5cdFx0XHRcdFx0XHRyZXR1cm4gJC52YWthdGEuZG5kLnN0YXJ0KGUsIHsgJ2pzdHJlZScgOiB0cnVlLCAnb3JpZ2luJyA6IHRoaXMsICdvYmonIDogdGhpcy5nZXRfbm9kZShvYmosdHJ1ZSksICdub2RlcycgOiBtbHQgPiAxID8gdGhpcy5nZXRfc2VsZWN0ZWQoKSA6IFtvYmouaWRdIH0sICc8ZGl2IGlkPVwianN0cmVlLWRuZFwiIGNsYXNzPVwianN0cmVlLScgKyB0aGlzLmdldF90aGVtZSgpICsgJ1wiPjxpIGNsYXNzPVwianN0cmVlLWljb24ganN0cmVlLWVyXCI+PC9pPicgKyAobWx0ID4gMSA/IG1sdCArICcgJyArIHRoaXMuZ2V0X3N0cmluZygnbm9kZXMnKSA6IHRoaXMuZ2V0X3RleHQoZS5jdXJyZW50VGFyZ2V0LCB0cnVlKSkgKyAnPGlucyBjbGFzcz1cImpzdHJlZS1jb3B5XCIgc3R5bGU9XCJkaXNwbGF5Om5vbmU7XCI+KzwvaW5zPjwvZGl2PicpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdH07XHJcblx0fTtcclxuXHJcblx0JChmdW5jdGlvbigpIHtcclxuXHRcdC8vIGJpbmQgb25seSBvbmNlIGZvciBhbGwgaW5zdGFuY2VzXHJcblx0XHR2YXIgbGFzdG12ID0gZmFsc2UsXHJcblx0XHRcdGxhc3RlciA9IGZhbHNlLFxyXG5cdFx0XHRvcGVudG8gPSBmYWxzZSxcclxuXHRcdFx0bWFya2VyID0gJCgnPGRpdiBpZD1cImpzdHJlZS1tYXJrZXJcIj4mIzE2MDs8L2Rpdj4nKS5oaWRlKCkuYXBwZW5kVG8oJ2JvZHknKTtcclxuXHJcblx0XHQkKGRvY3VtZW50KVxyXG5cdFx0XHQuYmluZCgnZG5kX3N0YXJ0LnZha2F0YScsIGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0bGFzdG12ID0gZmFsc2U7XHJcblx0XHRcdH0pXHJcblx0XHRcdC5iaW5kKCdkbmRfbW92ZS52YWthdGEnLCBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdGlmKG9wZW50bykgeyBjbGVhclRpbWVvdXQob3BlbnRvKTsgfVxyXG5cdFx0XHRcdGlmKCFkYXRhLmRhdGEuanN0cmVlKSB7IHJldHVybjsgfVxyXG5cclxuXHRcdFx0XHQvLyBpZiB3ZSBhcmUgaG92ZXJpbmcgdGhlIG1hcmtlciBpbWFnZSBkbyBub3RoaW5nIChjYW4gaGFwcGVuIG9uIFwiaW5zaWRlXCIgZHJhZ3MpXHJcblx0XHRcdFx0aWYoZGF0YS5ldmVudC50YXJnZXQuaWQgJiYgZGF0YS5ldmVudC50YXJnZXQuaWQgPT09ICdqc3RyZWUtbWFya2VyJykge1xyXG5cdFx0XHRcdFx0cmV0dXJuO1xyXG5cdFx0XHRcdH1cclxuXHJcblx0XHRcdFx0dmFyIGlucyA9ICQuanN0cmVlLnJlZmVyZW5jZShkYXRhLmV2ZW50LnRhcmdldCksXHJcblx0XHRcdFx0XHRyZWYgPSBmYWxzZSxcclxuXHRcdFx0XHRcdG9mZiA9IGZhbHNlLFxyXG5cdFx0XHRcdFx0cmVsID0gZmFsc2UsXHJcblx0XHRcdFx0XHRsLCB0LCBoLCBwLCBpLCBvLCBvaywgdDEsIHQyLCBvcCwgcHMsIHByO1xyXG5cdFx0XHRcdC8vIGlmIHdlIGFyZSBvdmVyIGFuIGluc3RhbmNlXHJcblx0XHRcdFx0aWYoaW5zICYmIGlucy5fZGF0YSAmJiBpbnMuX2RhdGEuZG5kKSB7XHJcblx0XHRcdFx0XHRtYXJrZXIuYXR0cignY2xhc3MnLCAnanN0cmVlLScgKyBpbnMuZ2V0X3RoZW1lKCkpO1xyXG5cdFx0XHRcdFx0ZGF0YS5oZWxwZXJcclxuXHRcdFx0XHRcdFx0LmNoaWxkcmVuKCkuYXR0cignY2xhc3MnLCAnanN0cmVlLScgKyBpbnMuZ2V0X3RoZW1lKCkpXHJcblx0XHRcdFx0XHRcdC5maW5kKCcuanN0cmVlLWNvcHk6ZXEoMCknKVsgZGF0YS5kYXRhLm9yaWdpbiAmJiBkYXRhLmRhdGEub3JpZ2luLnNldHRpbmdzLmRuZC5jb3B5ICYmIChkYXRhLmV2ZW50Lm1ldGFLZXkgfHwgZGF0YS5ldmVudC5jdHJsS2V5KSA/ICdzaG93JyA6ICdoaWRlJyBdKCk7XHJcblxyXG5cclxuXHRcdFx0XHRcdC8vIGlmIGFyZSBob3ZlcmluZyB0aGUgY29udGFpbmVyIGl0c2VsZiBhZGQgYSBuZXcgcm9vdCBub2RlXHJcblx0XHRcdFx0XHRpZiggKGRhdGEuZXZlbnQudGFyZ2V0ID09PSBpbnMuZWxlbWVudFswXSB8fCBkYXRhLmV2ZW50LnRhcmdldCA9PT0gaW5zLmdldF9jb250YWluZXJfdWwoKVswXSkgJiYgaW5zLmdldF9jb250YWluZXJfdWwoKS5jaGlsZHJlbigpLmxlbmd0aCA9PT0gMCkge1xyXG5cdFx0XHRcdFx0XHRvayA9IHRydWU7XHJcblx0XHRcdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gZGF0YS5kYXRhLm5vZGVzLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0XHRcdG9rID0gb2sgJiYgaW5zLmNoZWNrKCAoZGF0YS5kYXRhLm9yaWdpbiAmJiBkYXRhLmRhdGEub3JpZ2luLnNldHRpbmdzLmRuZC5jb3B5ICYmIChkYXRhLmV2ZW50Lm1ldGFLZXkgfHwgZGF0YS5ldmVudC5jdHJsS2V5KSA/IFwiY29weV9ub2RlXCIgOiBcIm1vdmVfbm9kZVwiKSwgKGRhdGEuZGF0YS5vcmlnaW4gJiYgZGF0YS5kYXRhLm9yaWdpbiAhPT0gaW5zID8gZGF0YS5kYXRhLm9yaWdpbi5nZXRfbm9kZShkYXRhLmRhdGEubm9kZXNbdDFdKSA6IGRhdGEuZGF0YS5ub2Rlc1t0MV0pLCAnIycsICdsYXN0Jyk7XHJcblx0XHRcdFx0XHRcdFx0aWYoIW9rKSB7IGJyZWFrOyB9XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0aWYob2spIHtcclxuXHRcdFx0XHRcdFx0XHRsYXN0bXYgPSB7ICdpbnMnIDogaW5zLCAncGFyJyA6ICcjJywgJ3BvcycgOiAnbGFzdCcgfTtcclxuXHRcdFx0XHRcdFx0XHRtYXJrZXIuaGlkZSgpO1xyXG5cdFx0XHRcdFx0XHRcdGRhdGEuaGVscGVyLmZpbmQoJy5qc3RyZWUtaWNvbjplcSgwKScpLnJlbW92ZUNsYXNzKCdqc3RyZWUtZXInKS5hZGRDbGFzcygnanN0cmVlLW9rJyk7XHJcblx0XHRcdFx0XHRcdFx0cmV0dXJuO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdFx0Ly8gaWYgd2UgYXJlIGhvdmVyaW5nIGEgdHJlZSBub2RlXHJcblx0XHRcdFx0XHRcdHJlZiA9ICQoZGF0YS5ldmVudC50YXJnZXQpLmNsb3Nlc3QoJ2EnKTtcclxuXHRcdFx0XHRcdFx0aWYocmVmICYmIHJlZi5sZW5ndGggJiYgcmVmLnBhcmVudCgpLmlzKCcuanN0cmVlLWNsb3NlZCwgLmpzdHJlZS1vcGVuLCAuanN0cmVlLWxlYWYnKSkge1xyXG5cdFx0XHRcdFx0XHRcdG9mZiA9IHJlZi5vZmZzZXQoKTtcclxuXHRcdFx0XHRcdFx0XHRyZWwgPSBkYXRhLmV2ZW50LnBhZ2VZIC0gb2ZmLnRvcDtcclxuXHRcdFx0XHRcdFx0XHRoID0gcmVmLmhlaWdodCgpO1xyXG5cdFx0XHRcdFx0XHRcdGlmKHJlbCA8IGggLyAzKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRvID0gWydiJywgJ2knLCAnYSddO1xyXG5cdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRlbHNlIGlmKHJlbCA+IGggLSBoIC8gMykge1xyXG5cdFx0XHRcdFx0XHRcdFx0byA9IFsnYScsICdpJywgJ2InXTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0ZWxzZSB7XHJcblx0XHRcdFx0XHRcdFx0XHRvID0gcmVsID4gaCAvIDIgPyBbJ2knLCAnYScsICdiJ10gOiBbJ2knLCAnYicsICdhJ107XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdCQuZWFjaChvLCBmdW5jdGlvbiAoaiwgdikge1xyXG5cdFx0XHRcdFx0XHRcdFx0c3dpdGNoKHYpIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0Y2FzZSAnYic6XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0bCA9IG9mZi5sZWZ0IC0gNjtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR0ID0gb2ZmLnRvcCAtIDU7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0cCA9IGlucy5nZXRfcGFyZW50KHJlZik7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0aSA9IHJlZi5wYXJlbnQoKS5pbmRleCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRjYXNlICdpJzpcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRsID0gb2ZmLmxlZnQgLSAyO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHQgPSBvZmYudG9wIC0gNSArIGggLyAyICsgMTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRwID0gcmVmLnBhcmVudCgpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGkgPSAwO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRjYXNlICdhJzpcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRsID0gb2ZmLmxlZnQgLSA2O1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHQgPSBvZmYudG9wIC0gNSArIGg7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0cCA9IGlucy5nZXRfcGFyZW50KHJlZik7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0aSA9IHJlZi5wYXJlbnQoKS5pbmRleCgpICsgMTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdC8qIVxyXG5cdFx0XHRcdFx0XHRcdFx0Ly8gVE9ETzogbW92aW5nIGluc2lkZSwgYnV0IHRoZSBub2RlIGlzIG5vdCB5ZXQgbG9hZGVkP1xyXG5cdFx0XHRcdFx0XHRcdFx0Ly8gdGhlIGNoZWNrIHdpbGwgd29yayBhbnl3YXksIGFzIHdoZW4gbW92aW5nIHRoZSBub2RlIHdpbGwgYmUgbG9hZGVkIGZpcnN0IGFuZCBjaGVja2VkIGFnYWluXHJcblx0XHRcdFx0XHRcdFx0XHRpZih2ID09PSAnaScgJiYgIWlucy5pc19sb2FkZWQocCkpIHsgfVxyXG5cdFx0XHRcdFx0XHRcdFx0Ki9cclxuXHRcdFx0XHRcdFx0XHRcdG9rID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0XHRcdGZvcih0MSA9IDAsIHQyID0gZGF0YS5kYXRhLm5vZGVzLmxlbmd0aDsgdDEgPCB0MjsgdDErKykge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRvcCA9IGRhdGEuZGF0YS5vcmlnaW4gJiYgZGF0YS5kYXRhLm9yaWdpbi5zZXR0aW5ncy5kbmQuY29weSAmJiAoZGF0YS5ldmVudC5tZXRhS2V5IHx8IGRhdGEuZXZlbnQuY3RybEtleSkgPyBcImNvcHlfbm9kZVwiIDogXCJtb3ZlX25vZGVcIjtcclxuXHRcdFx0XHRcdFx0XHRcdFx0cHMgPSBpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRpZihvcCA9PT0gXCJtb3ZlX25vZGVcIiAmJiB2ID09PSAnYScgJiYgKGRhdGEuZGF0YS5vcmlnaW4gJiYgZGF0YS5kYXRhLm9yaWdpbiA9PT0gaW5zKSAmJiBwID09PSBpbnMuZ2V0X3BhcmVudChkYXRhLmRhdGEubm9kZXNbdDFdKSkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdHByID0gaW5zLmdldF9ub2RlKHApO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdGlmKHBzID4gJC5pbkFycmF5KGRhdGEuZGF0YS5ub2Rlc1t0MV0sIHByLmNoaWxkcmVuKSkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdFx0cHMgLT0gMTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHRcdFx0b2sgPSBvayAmJiAoIChpbnMgJiYgaW5zLnNldHRpbmdzICYmIGlucy5zZXR0aW5ncy5kbmQgJiYgaW5zLnNldHRpbmdzLmRuZC5jaGVja193aGlsZV9kcmFnZ2luZyA9PT0gZmFsc2UpIHx8IGlucy5jaGVjayhvcCwgKGRhdGEuZGF0YS5vcmlnaW4gJiYgZGF0YS5kYXRhLm9yaWdpbiAhPT0gaW5zID8gZGF0YS5kYXRhLm9yaWdpbi5nZXRfbm9kZShkYXRhLmRhdGEubm9kZXNbdDFdKSA6IGRhdGEuZGF0YS5ub2Rlc1t0MV0pLCBwLCBwcykgKTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYoIW9rKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdFx0aWYoaW5zICYmIGlucy5sYXN0X2Vycm9yKSB7IGxhc3RlciA9IGlucy5sYXN0X2Vycm9yKCk7IH1cclxuXHRcdFx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0aWYob2spIHtcclxuXHRcdFx0XHRcdFx0XHRcdFx0aWYodiA9PT0gJ2knICYmIHJlZi5wYXJlbnQoKS5pcygnLmpzdHJlZS1jbG9zZWQnKSAmJiBpbnMuc2V0dGluZ3MuZG5kLm9wZW5fdGltZW91dCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRcdG9wZW50byA9IHNldFRpbWVvdXQoKGZ1bmN0aW9uICh4LCB6KSB7IHJldHVybiBmdW5jdGlvbiAoKSB7IHgub3Blbl9ub2RlKHopOyB9OyB9KGlucywgcmVmKSksIGlucy5zZXR0aW5ncy5kbmQub3Blbl90aW1lb3V0KTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdFx0XHRsYXN0bXYgPSB7ICdpbnMnIDogaW5zLCAncGFyJyA6IHAsICdwb3MnIDogaSB9O1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRtYXJrZXIuY3NzKHsgJ2xlZnQnIDogbCArICdweCcsICd0b3AnIDogdCArICdweCcgfSkuc2hvdygpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRkYXRhLmhlbHBlci5maW5kKCcuanN0cmVlLWljb246ZXEoMCknKS5yZW1vdmVDbGFzcygnanN0cmVlLWVyJykuYWRkQ2xhc3MoJ2pzdHJlZS1vaycpO1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRsYXN0ZXIgPSB7fTtcclxuXHRcdFx0XHRcdFx0XHRcdFx0byA9IHRydWU7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9KTtcclxuXHRcdFx0XHRcdFx0XHRpZihvID09PSB0cnVlKSB7IHJldHVybjsgfVxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGxhc3RtdiA9IGZhbHNlO1xyXG5cdFx0XHRcdGRhdGEuaGVscGVyLmZpbmQoJy5qc3RyZWUtaWNvbicpLnJlbW92ZUNsYXNzKCdqc3RyZWUtb2snKS5hZGRDbGFzcygnanN0cmVlLWVyJyk7XHJcblx0XHRcdFx0bWFya2VyLmhpZGUoKTtcclxuXHRcdFx0fSlcclxuXHRcdFx0LmJpbmQoJ2RuZF9zY3JvbGwudmFrYXRhJywgZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuXHRcdFx0XHRpZighZGF0YS5kYXRhLmpzdHJlZSkgeyByZXR1cm47IH1cclxuXHRcdFx0XHRtYXJrZXIuaGlkZSgpO1xyXG5cdFx0XHRcdGxhc3RtdiA9IGZhbHNlO1xyXG5cdFx0XHRcdGRhdGEuaGVscGVyLmZpbmQoJy5qc3RyZWUtaWNvbjplcSgwKScpLnJlbW92ZUNsYXNzKCdqc3RyZWUtb2snKS5hZGRDbGFzcygnanN0cmVlLWVyJyk7XHJcblx0XHRcdH0pXHJcblx0XHRcdC5iaW5kKCdkbmRfc3RvcC52YWthdGEnLCBmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdGlmKG9wZW50bykgeyBjbGVhclRpbWVvdXQob3BlbnRvKTsgfVxyXG5cdFx0XHRcdGlmKCFkYXRhLmRhdGEuanN0cmVlKSB7IHJldHVybjsgfVxyXG5cdFx0XHRcdG1hcmtlci5oaWRlKCk7XHJcblx0XHRcdFx0dmFyIGksIGosIG5vZGVzID0gW107XHJcblx0XHRcdFx0aWYobGFzdG12KSB7XHJcblx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSBkYXRhLmRhdGEubm9kZXMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRcdG5vZGVzW2ldID0gZGF0YS5kYXRhLm9yaWdpbiA/IGRhdGEuZGF0YS5vcmlnaW4uZ2V0X25vZGUoZGF0YS5kYXRhLm5vZGVzW2ldKSA6IGRhdGEuZGF0YS5ub2Rlc1tpXTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGxhc3Rtdi5pbnNbIGRhdGEuZGF0YS5vcmlnaW4gJiYgZGF0YS5kYXRhLm9yaWdpbi5zZXR0aW5ncy5kbmQuY29weSAmJiAoZGF0YS5ldmVudC5tZXRhS2V5IHx8IGRhdGEuZXZlbnQuY3RybEtleSkgPyAnY29weV9ub2RlJyA6ICdtb3ZlX25vZGUnIF0obm9kZXMsIGxhc3Rtdi5wYXIsIGxhc3Rtdi5wb3MpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdGkgPSAkKGRhdGEuZXZlbnQudGFyZ2V0KS5jbG9zZXN0KCcuanN0cmVlJyk7XHJcblx0XHRcdFx0XHRpZihpLmxlbmd0aCAmJiBsYXN0ZXIgJiYgbGFzdGVyLmVycm9yICYmIGxhc3Rlci5lcnJvciA9PT0gJ2NoZWNrJykge1xyXG5cdFx0XHRcdFx0XHRpID0gaS5qc3RyZWUodHJ1ZSk7XHJcblx0XHRcdFx0XHRcdGlmKGkpIHtcclxuXHRcdFx0XHRcdFx0XHRpLnNldHRpbmdzLmNvcmUuZXJyb3IuY2FsbCh0aGlzLCBsYXN0ZXIpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9KVxyXG5cdFx0XHQuYmluZCgna2V5dXAga2V5ZG93bicsIGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0ZGF0YSA9ICQudmFrYXRhLmRuZC5fZ2V0KCk7XHJcblx0XHRcdFx0aWYoZGF0YS5kYXRhICYmIGRhdGEuZGF0YS5qc3RyZWUpIHtcclxuXHRcdFx0XHRcdGRhdGEuaGVscGVyLmZpbmQoJy5qc3RyZWUtY29weTplcSgwKScpWyBkYXRhLmRhdGEub3JpZ2luICYmIGRhdGEuZGF0YS5vcmlnaW4uc2V0dGluZ3MuZG5kLmNvcHkgJiYgKGUubWV0YUtleSB8fCBlLmN0cmxLZXkpID8gJ3Nob3cnIDogJ2hpZGUnIF0oKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cdH0pO1xyXG5cclxuXHQvLyBoZWxwZXJzXHJcblx0KGZ1bmN0aW9uICgkKSB7XHJcblx0XHQkLmZuLnZha2F0YV9yZXZlcnNlID0gW10ucmV2ZXJzZTtcclxuXHRcdC8vIHByaXZhdGUgdmFyaWFibGVcclxuXHRcdHZhciB2YWthdGFfZG5kID0ge1xyXG5cdFx0XHRlbGVtZW50XHQ6IGZhbHNlLFxyXG5cdFx0XHRpc19kb3duXHQ6IGZhbHNlLFxyXG5cdFx0XHRpc19kcmFnXHQ6IGZhbHNlLFxyXG5cdFx0XHRoZWxwZXJcdDogZmFsc2UsXHJcblx0XHRcdGhlbHBlcl93OiAwLFxyXG5cdFx0XHRkYXRhXHQ6IGZhbHNlLFxyXG5cdFx0XHRpbml0X3hcdDogMCxcclxuXHRcdFx0aW5pdF95XHQ6IDAsXHJcblx0XHRcdHNjcm9sbF9sOiAwLFxyXG5cdFx0XHRzY3JvbGxfdDogMCxcclxuXHRcdFx0c2Nyb2xsX2U6IGZhbHNlLFxyXG5cdFx0XHRzY3JvbGxfaTogZmFsc2VcclxuXHRcdH07XHJcblx0XHQkLnZha2F0YS5kbmQgPSB7XHJcblx0XHRcdHNldHRpbmdzIDoge1xyXG5cdFx0XHRcdHNjcm9sbF9zcGVlZFx0XHQ6IDEwLFxyXG5cdFx0XHRcdHNjcm9sbF9wcm94aW1pdHlcdDogMjAsXHJcblx0XHRcdFx0aGVscGVyX2xlZnRcdFx0XHQ6IDUsXHJcblx0XHRcdFx0aGVscGVyX3RvcFx0XHRcdDogMTAsXHJcblx0XHRcdFx0dGhyZXNob2xkXHRcdFx0OiA1XHJcblx0XHRcdH0sXHJcblx0XHRcdF90cmlnZ2VyIDogZnVuY3Rpb24gKGV2ZW50X25hbWUsIGUpIHtcclxuXHRcdFx0XHR2YXIgZGF0YSA9ICQudmFrYXRhLmRuZC5fZ2V0KCk7XHJcblx0XHRcdFx0ZGF0YS5ldmVudCA9IGU7XHJcblx0XHRcdFx0JChkb2N1bWVudCkudHJpZ2dlckhhbmRsZXIoXCJkbmRfXCIgKyBldmVudF9uYW1lICsgXCIudmFrYXRhXCIsIGRhdGEpO1xyXG5cdFx0XHR9LFxyXG5cdFx0XHRfZ2V0IDogZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdHJldHVybiB7XHJcblx0XHRcdFx0XHRcImRhdGFcIlx0XHQ6IHZha2F0YV9kbmQuZGF0YSxcclxuXHRcdFx0XHRcdFwiZWxlbWVudFwiXHQ6IHZha2F0YV9kbmQuZWxlbWVudCxcclxuXHRcdFx0XHRcdFwiaGVscGVyXCJcdDogdmFrYXRhX2RuZC5oZWxwZXJcclxuXHRcdFx0XHR9O1xyXG5cdFx0XHR9LFxyXG5cdFx0XHRfY2xlYW4gOiBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0aWYodmFrYXRhX2RuZC5oZWxwZXIpIHsgdmFrYXRhX2RuZC5oZWxwZXIucmVtb3ZlKCk7IH1cclxuXHRcdFx0XHRpZih2YWthdGFfZG5kLnNjcm9sbF9pKSB7IGNsZWFySW50ZXJ2YWwodmFrYXRhX2RuZC5zY3JvbGxfaSk7IHZha2F0YV9kbmQuc2Nyb2xsX2kgPSBmYWxzZTsgfVxyXG5cdFx0XHRcdHZha2F0YV9kbmQgPSB7XHJcblx0XHRcdFx0XHRlbGVtZW50XHQ6IGZhbHNlLFxyXG5cdFx0XHRcdFx0aXNfZG93blx0OiBmYWxzZSxcclxuXHRcdFx0XHRcdGlzX2RyYWdcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRoZWxwZXJcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRoZWxwZXJfdzogMCxcclxuXHRcdFx0XHRcdGRhdGFcdDogZmFsc2UsXHJcblx0XHRcdFx0XHRpbml0X3hcdDogMCxcclxuXHRcdFx0XHRcdGluaXRfeVx0OiAwLFxyXG5cdFx0XHRcdFx0c2Nyb2xsX2w6IDAsXHJcblx0XHRcdFx0XHRzY3JvbGxfdDogMCxcclxuXHRcdFx0XHRcdHNjcm9sbF9lOiBmYWxzZSxcclxuXHRcdFx0XHRcdHNjcm9sbF9pOiBmYWxzZVxyXG5cdFx0XHRcdH07XHJcblx0XHRcdFx0JChkb2N1bWVudCkub2ZmKFwibW91c2Vtb3ZlIHRvdWNobW92ZVwiLCAkLnZha2F0YS5kbmQuZHJhZyk7XHJcblx0XHRcdFx0JChkb2N1bWVudCkub2ZmKFwibW91c2V1cCB0b3VjaGVuZFwiLCAkLnZha2F0YS5kbmQuc3RvcCk7XHJcblx0XHRcdH0sXHJcblx0XHRcdF9zY3JvbGwgOiBmdW5jdGlvbiAoaW5pdF9vbmx5KSB7XHJcblx0XHRcdFx0aWYoIXZha2F0YV9kbmQuc2Nyb2xsX2UgfHwgKCF2YWthdGFfZG5kLnNjcm9sbF9sICYmICF2YWthdGFfZG5kLnNjcm9sbF90KSkge1xyXG5cdFx0XHRcdFx0aWYodmFrYXRhX2RuZC5zY3JvbGxfaSkgeyBjbGVhckludGVydmFsKHZha2F0YV9kbmQuc2Nyb2xsX2kpOyB2YWthdGFfZG5kLnNjcm9sbF9pID0gZmFsc2U7IH1cclxuXHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYoIXZha2F0YV9kbmQuc2Nyb2xsX2kpIHtcclxuXHRcdFx0XHRcdHZha2F0YV9kbmQuc2Nyb2xsX2kgPSBzZXRJbnRlcnZhbCgkLnZha2F0YS5kbmQuX3Njcm9sbCwgMTAwKTtcclxuXHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYoaW5pdF9vbmx5ID09PSB0cnVlKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cclxuXHRcdFx0XHR2YXIgaSA9IHZha2F0YV9kbmQuc2Nyb2xsX2Uuc2Nyb2xsVG9wKCksXHJcblx0XHRcdFx0XHRqID0gdmFrYXRhX2RuZC5zY3JvbGxfZS5zY3JvbGxMZWZ0KCk7XHJcblx0XHRcdFx0dmFrYXRhX2RuZC5zY3JvbGxfZS5zY3JvbGxUb3AoaSArIHZha2F0YV9kbmQuc2Nyb2xsX3QgKiAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3NwZWVkKTtcclxuXHRcdFx0XHR2YWthdGFfZG5kLnNjcm9sbF9lLnNjcm9sbExlZnQoaiArIHZha2F0YV9kbmQuc2Nyb2xsX2wgKiAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3NwZWVkKTtcclxuXHRcdFx0XHRpZihpICE9PSB2YWthdGFfZG5kLnNjcm9sbF9lLnNjcm9sbFRvcCgpIHx8IGogIT09IHZha2F0YV9kbmQuc2Nyb2xsX2Uuc2Nyb2xsTGVmdCgpKSB7XHJcblx0XHRcdFx0XHQvKipcclxuXHRcdFx0XHRcdCAqIHRyaWdnZXJlZCBvbiB0aGUgZG9jdW1lbnQgd2hlbiBhIGRyYWcgY2F1c2VzIGFuIGVsZW1lbnQgdG8gc2Nyb2xsXHJcblx0XHRcdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0XHRcdCAqIEBwbHVnaW4gZG5kXHJcblx0XHRcdFx0XHQgKiBAbmFtZSBkbmRfc2Nyb2xsLnZha2F0YVxyXG5cdFx0XHRcdFx0ICogQHBhcmFtIHtNaXhlZH0gZGF0YSBhbnkgZGF0YSBzdXBwbGllZCB3aXRoIHRoZSBjYWxsIHRvICQudmFrYXRhLmRuZC5zdGFydFxyXG5cdFx0XHRcdFx0ICogQHBhcmFtIHtET019IGVsZW1lbnQgdGhlIERPTSBlbGVtZW50IGJlaW5nIGRyYWdnZWRcclxuXHRcdFx0XHRcdCAqIEBwYXJhbSB7alF1ZXJ5fSBoZWxwZXIgdGhlIGhlbHBlciBzaG93biBuZXh0IHRvIHRoZSBtb3VzZVxyXG5cdFx0XHRcdFx0ICogQHBhcmFtIHtqUXVlcnl9IGV2ZW50IHRoZSBlbGVtZW50IHRoYXQgaXMgc2Nyb2xsaW5nXHJcblx0XHRcdFx0XHQgKi9cclxuXHRcdFx0XHRcdCQudmFrYXRhLmRuZC5fdHJpZ2dlcihcInNjcm9sbFwiLCB2YWthdGFfZG5kLnNjcm9sbF9lKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0sXHJcblx0XHRcdHN0YXJ0IDogZnVuY3Rpb24gKGUsIGRhdGEsIGh0bWwpIHtcclxuXHRcdFx0XHRpZihlLnR5cGUgPT09IFwidG91Y2hzdGFydFwiICYmIGUub3JpZ2luYWxFdmVudCAmJiBlLm9yaWdpbmFsRXZlbnQuY2hhbmdlZFRvdWNoZXMgJiYgZS5vcmlnaW5hbEV2ZW50LmNoYW5nZWRUb3VjaGVzWzBdKSB7XHJcblx0XHRcdFx0XHRlLnBhZ2VYID0gZS5vcmlnaW5hbEV2ZW50LmNoYW5nZWRUb3VjaGVzWzBdLnBhZ2VYO1xyXG5cdFx0XHRcdFx0ZS5wYWdlWSA9IGUub3JpZ2luYWxFdmVudC5jaGFuZ2VkVG91Y2hlc1swXS5wYWdlWTtcclxuXHRcdFx0XHRcdGUudGFyZ2V0ID0gZG9jdW1lbnQuZWxlbWVudEZyb21Qb2ludChlLm9yaWdpbmFsRXZlbnQuY2hhbmdlZFRvdWNoZXNbMF0ucGFnZVggLSB3aW5kb3cucGFnZVhPZmZzZXQsIGUub3JpZ2luYWxFdmVudC5jaGFuZ2VkVG91Y2hlc1swXS5wYWdlWSAtIHdpbmRvdy5wYWdlWU9mZnNldCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGlmKHZha2F0YV9kbmQuaXNfZHJhZykgeyAkLnZha2F0YS5kbmQuc3RvcCh7fSk7IH1cclxuXHRcdFx0XHR0cnkge1xyXG5cdFx0XHRcdFx0ZS5jdXJyZW50VGFyZ2V0LnVuc2VsZWN0YWJsZSA9IFwib25cIjtcclxuXHRcdFx0XHRcdGUuY3VycmVudFRhcmdldC5vbnNlbGVjdHN0YXJ0ID0gZnVuY3Rpb24oKSB7IHJldHVybiBmYWxzZTsgfTtcclxuXHRcdFx0XHRcdGlmKGUuY3VycmVudFRhcmdldC5zdHlsZSkgeyBlLmN1cnJlbnRUYXJnZXQuc3R5bGUuTW96VXNlclNlbGVjdCA9IFwibm9uZVwiOyB9XHJcblx0XHRcdFx0fSBjYXRjaChpZ25vcmUpIHsgfVxyXG5cdFx0XHRcdHZha2F0YV9kbmQuaW5pdF94XHQ9IGUucGFnZVg7XHJcblx0XHRcdFx0dmFrYXRhX2RuZC5pbml0X3lcdD0gZS5wYWdlWTtcclxuXHRcdFx0XHR2YWthdGFfZG5kLmRhdGFcdFx0PSBkYXRhO1xyXG5cdFx0XHRcdHZha2F0YV9kbmQuaXNfZG93blx0PSB0cnVlO1xyXG5cdFx0XHRcdHZha2F0YV9kbmQuZWxlbWVudFx0PSBlLmN1cnJlbnRUYXJnZXQ7XHJcblx0XHRcdFx0aWYoaHRtbCAhPT0gZmFsc2UpIHtcclxuXHRcdFx0XHRcdHZha2F0YV9kbmQuaGVscGVyID0gJChcIjxkaXYgaWQ9J3Zha2F0YS1kbmQnPjwvZGl2PlwiKS5odG1sKGh0bWwpLmNzcyh7XHJcblx0XHRcdFx0XHRcdFwiZGlzcGxheVwiXHRcdDogXCJibG9ja1wiLFxyXG5cdFx0XHRcdFx0XHRcIm1hcmdpblwiXHRcdDogXCIwXCIsXHJcblx0XHRcdFx0XHRcdFwicGFkZGluZ1wiXHRcdDogXCIwXCIsXHJcblx0XHRcdFx0XHRcdFwicG9zaXRpb25cIlx0XHQ6IFwiYWJzb2x1dGVcIixcclxuXHRcdFx0XHRcdFx0XCJ0b3BcIlx0XHRcdDogXCItMjAwMHB4XCIsXHJcblx0XHRcdFx0XHRcdFwibGluZUhlaWdodFwiXHQ6IFwiMTZweFwiLFxyXG5cdFx0XHRcdFx0XHRcInpJbmRleFwiXHRcdDogXCIxMDAwMFwiXHJcblx0XHRcdFx0XHR9KTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0JChkb2N1bWVudCkuYmluZChcIm1vdXNlbW92ZSB0b3VjaG1vdmVcIiwgJC52YWthdGEuZG5kLmRyYWcpO1xyXG5cdFx0XHRcdCQoZG9jdW1lbnQpLmJpbmQoXCJtb3VzZXVwIHRvdWNoZW5kXCIsICQudmFrYXRhLmRuZC5zdG9wKTtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH0sXHJcblx0XHRcdGRyYWcgOiBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdGlmKGUudHlwZSA9PT0gXCJ0b3VjaG1vdmVcIiAmJiBlLm9yaWdpbmFsRXZlbnQgJiYgZS5vcmlnaW5hbEV2ZW50LmNoYW5nZWRUb3VjaGVzICYmIGUub3JpZ2luYWxFdmVudC5jaGFuZ2VkVG91Y2hlc1swXSkge1xyXG5cdFx0XHRcdFx0ZS5wYWdlWCA9IGUub3JpZ2luYWxFdmVudC5jaGFuZ2VkVG91Y2hlc1swXS5wYWdlWDtcclxuXHRcdFx0XHRcdGUucGFnZVkgPSBlLm9yaWdpbmFsRXZlbnQuY2hhbmdlZFRvdWNoZXNbMF0ucGFnZVk7XHJcblx0XHRcdFx0XHRlLnRhcmdldCA9IGRvY3VtZW50LmVsZW1lbnRGcm9tUG9pbnQoZS5vcmlnaW5hbEV2ZW50LmNoYW5nZWRUb3VjaGVzWzBdLnBhZ2VYIC0gd2luZG93LnBhZ2VYT2Zmc2V0LCBlLm9yaWdpbmFsRXZlbnQuY2hhbmdlZFRvdWNoZXNbMF0ucGFnZVkgLSB3aW5kb3cucGFnZVlPZmZzZXQpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRpZighdmFrYXRhX2RuZC5pc19kb3duKSB7IHJldHVybjsgfVxyXG5cdFx0XHRcdGlmKCF2YWthdGFfZG5kLmlzX2RyYWcpIHtcclxuXHRcdFx0XHRcdGlmKFxyXG5cdFx0XHRcdFx0XHRNYXRoLmFicyhlLnBhZ2VYIC0gdmFrYXRhX2RuZC5pbml0X3gpID4gJC52YWthdGEuZG5kLnNldHRpbmdzLnRocmVzaG9sZCB8fFxyXG5cdFx0XHRcdFx0XHRNYXRoLmFicyhlLnBhZ2VZIC0gdmFrYXRhX2RuZC5pbml0X3kpID4gJC52YWthdGEuZG5kLnNldHRpbmdzLnRocmVzaG9sZFxyXG5cdFx0XHRcdFx0KSB7XHJcblx0XHRcdFx0XHRcdGlmKHZha2F0YV9kbmQuaGVscGVyKSB7XHJcblx0XHRcdFx0XHRcdFx0dmFrYXRhX2RuZC5oZWxwZXIuYXBwZW5kVG8oXCJib2R5XCIpO1xyXG5cdFx0XHRcdFx0XHRcdHZha2F0YV9kbmQuaGVscGVyX3cgPSB2YWthdGFfZG5kLmhlbHBlci5vdXRlcldpZHRoKCk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0dmFrYXRhX2RuZC5pc19kcmFnID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0LyoqXHJcblx0XHRcdFx0XHRcdCAqIHRyaWdnZXJlZCBvbiB0aGUgZG9jdW1lbnQgd2hlbiBhIGRyYWcgc3RhcnRzXHJcblx0XHRcdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdFx0XHQgKiBAcGx1Z2luIGRuZFxyXG5cdFx0XHRcdFx0XHQgKiBAbmFtZSBkbmRfc3RhcnQudmFrYXRhXHJcblx0XHRcdFx0XHRcdCAqIEBwYXJhbSB7TWl4ZWR9IGRhdGEgYW55IGRhdGEgc3VwcGxpZWQgd2l0aCB0aGUgY2FsbCB0byAkLnZha2F0YS5kbmQuc3RhcnRcclxuXHRcdFx0XHRcdFx0ICogQHBhcmFtIHtET019IGVsZW1lbnQgdGhlIERPTSBlbGVtZW50IGJlaW5nIGRyYWdnZWRcclxuXHRcdFx0XHRcdFx0ICogQHBhcmFtIHtqUXVlcnl9IGhlbHBlciB0aGUgaGVscGVyIHNob3duIG5leHQgdG8gdGhlIG1vdXNlXHJcblx0XHRcdFx0XHRcdCAqIEBwYXJhbSB7T2JqZWN0fSBldmVudCB0aGUgZXZlbnQgdGhhdCBjYXVzZWQgdGhlIHN0YXJ0IChwcm9iYWJseSBtb3VzZW1vdmUpXHJcblx0XHRcdFx0XHRcdCAqL1xyXG5cdFx0XHRcdFx0XHQkLnZha2F0YS5kbmQuX3RyaWdnZXIoXCJzdGFydFwiLCBlKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGVsc2UgeyByZXR1cm47IH1cclxuXHRcdFx0XHR9XHJcblxyXG5cdFx0XHRcdHZhciBkICA9IGZhbHNlLCB3ICA9IGZhbHNlLFxyXG5cdFx0XHRcdFx0ZGggPSBmYWxzZSwgd2ggPSBmYWxzZSxcclxuXHRcdFx0XHRcdGR3ID0gZmFsc2UsIHd3ID0gZmFsc2UsXHJcblx0XHRcdFx0XHRkdCA9IGZhbHNlLCBkbCA9IGZhbHNlLFxyXG5cdFx0XHRcdFx0aHQgPSBmYWxzZSwgaGwgPSBmYWxzZTtcclxuXHJcblx0XHRcdFx0dmFrYXRhX2RuZC5zY3JvbGxfdCA9IDA7XHJcblx0XHRcdFx0dmFrYXRhX2RuZC5zY3JvbGxfbCA9IDA7XHJcblx0XHRcdFx0dmFrYXRhX2RuZC5zY3JvbGxfZSA9IGZhbHNlO1xyXG5cdFx0XHRcdCQoZS50YXJnZXQpXHJcblx0XHRcdFx0XHQucGFyZW50c1VudGlsKFwiYm9keVwiKS5hZGRCYWNrKCkudmFrYXRhX3JldmVyc2UoKVxyXG5cdFx0XHRcdFx0LmZpbHRlcihmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHRcdHJldHVyblx0KC9eYXV0b3xzY3JvbGwkLykudGVzdCgkKHRoaXMpLmNzcyhcIm92ZXJmbG93XCIpKSAmJlxyXG5cdFx0XHRcdFx0XHRcdFx0KHRoaXMuc2Nyb2xsSGVpZ2h0ID4gdGhpcy5vZmZzZXRIZWlnaHQgfHwgdGhpcy5zY3JvbGxXaWR0aCA+IHRoaXMub2Zmc2V0V2lkdGgpO1xyXG5cdFx0XHRcdFx0fSlcclxuXHRcdFx0XHRcdC5lYWNoKGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdFx0dmFyIHQgPSAkKHRoaXMpLCBvID0gdC5vZmZzZXQoKTtcclxuXHRcdFx0XHRcdFx0aWYodGhpcy5zY3JvbGxIZWlnaHQgPiB0aGlzLm9mZnNldEhlaWdodCkge1xyXG5cdFx0XHRcdFx0XHRcdGlmKG8udG9wICsgdC5oZWlnaHQoKSAtIGUucGFnZVkgPCAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3Byb3hpbWl0eSlcdHsgdmFrYXRhX2RuZC5zY3JvbGxfdCA9IDE7IH1cclxuXHRcdFx0XHRcdFx0XHRpZihlLnBhZ2VZIC0gby50b3AgPCAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3Byb3hpbWl0eSlcdFx0XHRcdHsgdmFrYXRhX2RuZC5zY3JvbGxfdCA9IC0xOyB9XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0aWYodGhpcy5zY3JvbGxXaWR0aCA+IHRoaXMub2Zmc2V0V2lkdGgpIHtcclxuXHRcdFx0XHRcdFx0XHRpZihvLmxlZnQgKyB0LndpZHRoKCkgLSBlLnBhZ2VYIDwgJC52YWthdGEuZG5kLnNldHRpbmdzLnNjcm9sbF9wcm94aW1pdHkpXHR7IHZha2F0YV9kbmQuc2Nyb2xsX2wgPSAxOyB9XHJcblx0XHRcdFx0XHRcdFx0aWYoZS5wYWdlWCAtIG8ubGVmdCA8ICQudmFrYXRhLmRuZC5zZXR0aW5ncy5zY3JvbGxfcHJveGltaXR5KVx0XHRcdFx0eyB2YWthdGFfZG5kLnNjcm9sbF9sID0gLTE7IH1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRpZih2YWthdGFfZG5kLnNjcm9sbF90IHx8IHZha2F0YV9kbmQuc2Nyb2xsX2wpIHtcclxuXHRcdFx0XHRcdFx0XHR2YWthdGFfZG5kLnNjcm9sbF9lID0gJCh0aGlzKTtcclxuXHRcdFx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH0pO1xyXG5cclxuXHRcdFx0XHRpZighdmFrYXRhX2RuZC5zY3JvbGxfZSkge1xyXG5cdFx0XHRcdFx0ZCAgPSAkKGRvY3VtZW50KTsgdyA9ICQod2luZG93KTtcclxuXHRcdFx0XHRcdGRoID0gZC5oZWlnaHQoKTsgd2ggPSB3LmhlaWdodCgpO1xyXG5cdFx0XHRcdFx0ZHcgPSBkLndpZHRoKCk7IHd3ID0gdy53aWR0aCgpO1xyXG5cdFx0XHRcdFx0ZHQgPSBkLnNjcm9sbFRvcCgpOyBkbCA9IGQuc2Nyb2xsTGVmdCgpO1xyXG5cdFx0XHRcdFx0aWYoZGggPiB3aCAmJiBlLnBhZ2VZIC0gZHQgPCAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3Byb3hpbWl0eSlcdFx0eyB2YWthdGFfZG5kLnNjcm9sbF90ID0gLTE7ICB9XHJcblx0XHRcdFx0XHRpZihkaCA+IHdoICYmIHdoIC0gKGUucGFnZVkgLSBkdCkgPCAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3Byb3hpbWl0eSlcdHsgdmFrYXRhX2RuZC5zY3JvbGxfdCA9IDE7IH1cclxuXHRcdFx0XHRcdGlmKGR3ID4gd3cgJiYgZS5wYWdlWCAtIGRsIDwgJC52YWthdGEuZG5kLnNldHRpbmdzLnNjcm9sbF9wcm94aW1pdHkpXHRcdHsgdmFrYXRhX2RuZC5zY3JvbGxfbCA9IC0xOyB9XHJcblx0XHRcdFx0XHRpZihkdyA+IHd3ICYmIHd3IC0gKGUucGFnZVggLSBkbCkgPCAkLnZha2F0YS5kbmQuc2V0dGluZ3Muc2Nyb2xsX3Byb3hpbWl0eSlcdHsgdmFrYXRhX2RuZC5zY3JvbGxfbCA9IDE7IH1cclxuXHRcdFx0XHRcdGlmKHZha2F0YV9kbmQuc2Nyb2xsX3QgfHwgdmFrYXRhX2RuZC5zY3JvbGxfbCkge1xyXG5cdFx0XHRcdFx0XHR2YWthdGFfZG5kLnNjcm9sbF9lID0gZDtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYodmFrYXRhX2RuZC5zY3JvbGxfZSkgeyAkLnZha2F0YS5kbmQuX3Njcm9sbCh0cnVlKTsgfVxyXG5cclxuXHRcdFx0XHRpZih2YWthdGFfZG5kLmhlbHBlcikge1xyXG5cdFx0XHRcdFx0aHQgPSBwYXJzZUludChlLnBhZ2VZICsgJC52YWthdGEuZG5kLnNldHRpbmdzLmhlbHBlcl90b3AsIDEwKTtcclxuXHRcdFx0XHRcdGhsID0gcGFyc2VJbnQoZS5wYWdlWCArICQudmFrYXRhLmRuZC5zZXR0aW5ncy5oZWxwZXJfbGVmdCwgMTApO1xyXG5cdFx0XHRcdFx0aWYoZGggJiYgaHQgKyAyNSA+IGRoKSB7IGh0ID0gZGggLSA1MDsgfVxyXG5cdFx0XHRcdFx0aWYoZHcgJiYgaGwgKyB2YWthdGFfZG5kLmhlbHBlcl93ID4gZHcpIHsgaGwgPSBkdyAtICh2YWthdGFfZG5kLmhlbHBlcl93ICsgMik7IH1cclxuXHRcdFx0XHRcdHZha2F0YV9kbmQuaGVscGVyLmNzcyh7XHJcblx0XHRcdFx0XHRcdGxlZnRcdDogaGwgKyBcInB4XCIsXHJcblx0XHRcdFx0XHRcdHRvcFx0XHQ6IGh0ICsgXCJweFwiXHJcblx0XHRcdFx0XHR9KTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0LyoqXHJcblx0XHRcdFx0ICogdHJpZ2dlcmVkIG9uIHRoZSBkb2N1bWVudCB3aGVuIGEgZHJhZyBpcyBpbiBwcm9ncmVzc1xyXG5cdFx0XHRcdCAqIEBldmVudFxyXG5cdFx0XHRcdCAqIEBwbHVnaW4gZG5kXHJcblx0XHRcdFx0ICogQG5hbWUgZG5kX21vdmUudmFrYXRhXHJcblx0XHRcdFx0ICogQHBhcmFtIHtNaXhlZH0gZGF0YSBhbnkgZGF0YSBzdXBwbGllZCB3aXRoIHRoZSBjYWxsIHRvICQudmFrYXRhLmRuZC5zdGFydFxyXG5cdFx0XHRcdCAqIEBwYXJhbSB7RE9NfSBlbGVtZW50IHRoZSBET00gZWxlbWVudCBiZWluZyBkcmFnZ2VkXHJcblx0XHRcdFx0ICogQHBhcmFtIHtqUXVlcnl9IGhlbHBlciB0aGUgaGVscGVyIHNob3duIG5leHQgdG8gdGhlIG1vdXNlXHJcblx0XHRcdFx0ICogQHBhcmFtIHtPYmplY3R9IGV2ZW50IHRoZSBldmVudCB0aGF0IGNhdXNlZCB0aGlzIHRvIHRyaWdnZXIgKG1vc3QgbGlrZWx5IG1vdXNlbW92ZSlcclxuXHRcdFx0XHQgKi9cclxuXHRcdFx0XHQkLnZha2F0YS5kbmQuX3RyaWdnZXIoXCJtb3ZlXCIsIGUpO1xyXG5cdFx0XHR9LFxyXG5cdFx0XHRzdG9wIDogZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRpZihlLnR5cGUgPT09IFwidG91Y2hlbmRcIiAmJiBlLm9yaWdpbmFsRXZlbnQgJiYgZS5vcmlnaW5hbEV2ZW50LmNoYW5nZWRUb3VjaGVzICYmIGUub3JpZ2luYWxFdmVudC5jaGFuZ2VkVG91Y2hlc1swXSkge1xyXG5cdFx0XHRcdFx0ZS5wYWdlWCA9IGUub3JpZ2luYWxFdmVudC5jaGFuZ2VkVG91Y2hlc1swXS5wYWdlWDtcclxuXHRcdFx0XHRcdGUucGFnZVkgPSBlLm9yaWdpbmFsRXZlbnQuY2hhbmdlZFRvdWNoZXNbMF0ucGFnZVk7XHJcblx0XHRcdFx0XHRlLnRhcmdldCA9IGRvY3VtZW50LmVsZW1lbnRGcm9tUG9pbnQoZS5vcmlnaW5hbEV2ZW50LmNoYW5nZWRUb3VjaGVzWzBdLnBhZ2VYIC0gd2luZG93LnBhZ2VYT2Zmc2V0LCBlLm9yaWdpbmFsRXZlbnQuY2hhbmdlZFRvdWNoZXNbMF0ucGFnZVkgLSB3aW5kb3cucGFnZVlPZmZzZXQpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRpZih2YWthdGFfZG5kLmlzX2RyYWcpIHtcclxuXHRcdFx0XHRcdC8qKlxyXG5cdFx0XHRcdFx0ICogdHJpZ2dlcmVkIG9uIHRoZSBkb2N1bWVudCB3aGVuIGEgZHJhZyBzdG9wcyAodGhlIGRyYWdnZWQgZWxlbWVudCBpcyBkcm9wcGVkKVxyXG5cdFx0XHRcdFx0ICogQGV2ZW50XHJcblx0XHRcdFx0XHQgKiBAcGx1Z2luIGRuZFxyXG5cdFx0XHRcdFx0ICogQG5hbWUgZG5kX3N0b3AudmFrYXRhXHJcblx0XHRcdFx0XHQgKiBAcGFyYW0ge01peGVkfSBkYXRhIGFueSBkYXRhIHN1cHBsaWVkIHdpdGggdGhlIGNhbGwgdG8gJC52YWthdGEuZG5kLnN0YXJ0XHJcblx0XHRcdFx0XHQgKiBAcGFyYW0ge0RPTX0gZWxlbWVudCB0aGUgRE9NIGVsZW1lbnQgYmVpbmcgZHJhZ2dlZFxyXG5cdFx0XHRcdFx0ICogQHBhcmFtIHtqUXVlcnl9IGhlbHBlciB0aGUgaGVscGVyIHNob3duIG5leHQgdG8gdGhlIG1vdXNlXHJcblx0XHRcdFx0XHQgKiBAcGFyYW0ge09iamVjdH0gZXZlbnQgdGhlIGV2ZW50IHRoYXQgY2F1c2VkIHRoZSBzdG9wXHJcblx0XHRcdFx0XHQgKi9cclxuXHRcdFx0XHRcdCQudmFrYXRhLmRuZC5fdHJpZ2dlcihcInN0b3BcIiwgZSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdCQudmFrYXRhLmRuZC5fY2xlYW4oKTtcclxuXHRcdFx0fVxyXG5cdFx0fTtcclxuXHR9KGpRdWVyeSkpO1xyXG5cclxuXHQvLyBpbmNsdWRlIHRoZSBkbmQgcGx1Z2luIGJ5IGRlZmF1bHRcclxuXHQvLyAkLmpzdHJlZS5kZWZhdWx0cy5wbHVnaW5zLnB1c2goXCJkbmRcIik7XHJcblxyXG5cclxuLyoqXHJcbiAqICMjIyBTZWFyY2ggcGx1Z2luXHJcbiAqXHJcbiAqIEFkZHMgc2VhcmNoIGZ1bmN0aW9uYWxpdHkgdG8ganNUcmVlLlxyXG4gKi9cclxuXHJcblx0LyoqXHJcblx0ICogc3RvcmVzIGFsbCBkZWZhdWx0cyBmb3IgdGhlIHNlYXJjaCBwbHVnaW5cclxuXHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5zZWFyY2hcclxuXHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdCAqL1xyXG5cdCQuanN0cmVlLmRlZmF1bHRzLnNlYXJjaCA9IHtcclxuXHRcdC8qKlxyXG5cdFx0ICogYSBqUXVlcnktbGlrZSBBSkFYIGNvbmZpZywgd2hpY2gganN0cmVlIHVzZXMgaWYgYSBzZXJ2ZXIgc2hvdWxkIGJlIHF1ZXJpZWQgZm9yIHJlc3VsdHMuIFxyXG5cdFx0ICogXHJcblx0XHQgKiBBIGBzdHJgICh3aGljaCBpcyB0aGUgc2VhcmNoIHN0cmluZykgcGFyYW1ldGVyIHdpbGwgYmUgYWRkZWQgd2l0aCB0aGUgcmVxdWVzdC4gVGhlIGV4cGVjdGVkIHJlc3VsdCBpcyBhIEpTT04gYXJyYXkgd2l0aCBub2RlcyB0aGF0IG5lZWQgdG8gYmUgb3BlbmVkIHNvIHRoYXQgbWF0Y2hpbmcgbm9kZXMgd2lsbCBiZSByZXZlYWxlZC5cclxuXHRcdCAqIExlYXZlIHRoaXMgc2V0dGluZyBhcyBgZmFsc2VgIHRvIG5vdCBxdWVyeSB0aGUgc2VydmVyLlxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuc2VhcmNoLmFqYXhcclxuXHRcdCAqIEBwbHVnaW4gc2VhcmNoXHJcblx0XHQgKi9cclxuXHRcdGFqYXggOiBmYWxzZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogSW5kaWNhdGVzIGlmIHRoZSBzZWFyY2ggc2hvdWxkIGJlIGZ1enp5IG9yIG5vdCAoc2hvdWxkIGBjaG5kM2AgbWF0Y2ggYGNoaWxkIG5vZGUgM2ApLiBEZWZhdWx0IGlzIGB0cnVlYC5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnNlYXJjaC5mdXp6eVxyXG5cdFx0ICogQHBsdWdpbiBzZWFyY2hcclxuXHRcdCAqL1xyXG5cdFx0ZnV6enkgOiB0cnVlLFxyXG5cdFx0LyoqXHJcblx0XHQgKiBJbmRpY2F0ZXMgaWYgdGhlIHNlYXJjaCBzaG91bGQgYmUgY2FzZSBzZW5zaXRpdmUuIERlZmF1bHQgaXMgYGZhbHNlYC5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnNlYXJjaC5jYXNlX3NlbnNpdGl2ZVxyXG5cdFx0ICogQHBsdWdpbiBzZWFyY2hcclxuXHRcdCAqL1xyXG5cdFx0Y2FzZV9zZW5zaXRpdmUgOiBmYWxzZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogSW5kaWNhdGVzIGlmIHRoZSB0cmVlIHNob3VsZCBiZSBmaWx0ZXJlZCB0byBzaG93IG9ubHkgbWF0Y2hpbmcgbm9kZXMgKGtlZXAgaW4gbWluZCB0aGlzIGNhbiBiZSBhIGhlYXZ5IG9uIGxhcmdlIHRyZWVzIGluIG9sZCBicm93c2VycykuIERlZmF1bHQgaXMgYGZhbHNlYC5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnNlYXJjaC5zaG93X29ubHlfbWF0Y2hlc1xyXG5cdFx0ICogQHBsdWdpbiBzZWFyY2hcclxuXHRcdCAqL1xyXG5cdFx0c2hvd19vbmx5X21hdGNoZXMgOiBmYWxzZSxcclxuXHRcdC8qKlxyXG5cdFx0ICogSW5kaWNhdGVzIGlmIGFsbCBub2RlcyBvcGVuZWQgdG8gcmV2ZWFsIHRoZSBzZWFyY2ggcmVzdWx0LCBzaG91bGQgYmUgY2xvc2VkIHdoZW4gdGhlIHNlYXJjaCBpcyBjbGVhcmVkIG9yIGEgbmV3IHNlYXJjaCBpcyBwZXJmb3JtZWQuIERlZmF1bHQgaXMgYHRydWVgLlxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuc2VhcmNoLmNsb3NlX29wZW5lZF9vbmNsZWFyXHJcblx0XHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdFx0ICovXHJcblx0XHRjbG9zZV9vcGVuZWRfb25jbGVhciA6IHRydWVcclxuXHR9O1xyXG5cclxuXHQkLmpzdHJlZS5wbHVnaW5zLnNlYXJjaCA9IGZ1bmN0aW9uIChvcHRpb25zLCBwYXJlbnQpIHtcclxuXHRcdHRoaXMuYmluZCA9IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0cGFyZW50LmJpbmQuY2FsbCh0aGlzKTtcclxuXHJcblx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLnN0ciA9IFwiXCI7XHJcblx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLmRvbSA9ICQoKTtcclxuXHRcdFx0dGhpcy5fZGF0YS5zZWFyY2gucmVzID0gW107XHJcblx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLm9wbiA9IFtdO1xyXG5cdFx0XHR0aGlzLl9kYXRhLnNlYXJjaC5zbG4gPSBudWxsO1xyXG5cclxuXHRcdFx0aWYodGhpcy5zZXR0aW5ncy5zZWFyY2guc2hvd19vbmx5X21hdGNoZXMpIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHRcdC5vbihcInNlYXJjaC5qc3RyZWVcIiwgZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuXHRcdFx0XHRcdFx0aWYoZGF0YS5ub2Rlcy5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0XHQkKHRoaXMpLmZpbmQoXCJsaVwiKS5oaWRlKCkuZmlsdGVyKCcuanN0cmVlLWxhc3QnKS5maWx0ZXIoZnVuY3Rpb24oKSB7IHJldHVybiB0aGlzLm5leHRTaWJsaW5nOyB9KS5yZW1vdmVDbGFzcygnanN0cmVlLWxhc3QnKTtcclxuXHRcdFx0XHRcdFx0XHRkYXRhLm5vZGVzLnBhcmVudHNVbnRpbChcIi5qc3RyZWVcIikuYWRkQmFjaygpLnNob3coKVxyXG5cdFx0XHRcdFx0XHRcdFx0LmZpbHRlcihcInVsXCIpLmVhY2goZnVuY3Rpb24gKCkgeyAkKHRoaXMpLmNoaWxkcmVuKFwibGk6dmlzaWJsZVwiKS5lcSgtMSkuYWRkQ2xhc3MoXCJqc3RyZWUtbGFzdFwiKTsgfSk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH0pXHJcblx0XHRcdFx0XHQub24oXCJjbGVhcl9zZWFyY2guanN0cmVlXCIsIGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0XHRcdGlmKGRhdGEubm9kZXMubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0JCh0aGlzKS5maW5kKFwibGlcIikuY3NzKFwiZGlzcGxheVwiLFwiXCIpLmZpbHRlcignLmpzdHJlZS1sYXN0JykuZmlsdGVyKGZ1bmN0aW9uKCkgeyByZXR1cm4gdGhpcy5uZXh0U2libGluZzsgfSkucmVtb3ZlQ2xhc3MoJ2pzdHJlZS1sYXN0Jyk7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH0pO1xyXG5cdFx0XHR9XHJcblx0XHR9O1xyXG5cdFx0LyoqXHJcblx0XHQgKiB1c2VkIHRvIHNlYXJjaCB0aGUgdHJlZSBub2RlcyBmb3IgYSBnaXZlbiBzdHJpbmdcclxuXHRcdCAqIEBuYW1lIHNlYXJjaChzdHIgWywgc2tpcF9hc3luY10pXHJcblx0XHQgKiBAcGFyYW0ge1N0cmluZ30gc3RyIHRoZSBzZWFyY2ggc3RyaW5nXHJcblx0XHQgKiBAcGFyYW0ge0Jvb2xlYW59IHNraXBfYXN5bmMgaWYgc2V0IHRvIHRydWUgc2VydmVyIHdpbGwgbm90IGJlIHF1ZXJpZWQgZXZlbiBpZiBjb25maWd1cmVkXHJcblx0XHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdFx0ICogQHRyaWdnZXIgc2VhcmNoLmpzdHJlZVxyXG5cdFx0ICovXHJcblx0XHR0aGlzLnNlYXJjaCA9IGZ1bmN0aW9uIChzdHIsIHNraXBfYXN5bmMpIHtcclxuXHRcdFx0aWYoc3RyID09PSBmYWxzZSB8fCAkLnRyaW0oc3RyKSA9PT0gXCJcIikge1xyXG5cdFx0XHRcdHJldHVybiB0aGlzLmNsZWFyX3NlYXJjaCgpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHZhciBzID0gdGhpcy5zZXR0aW5ncy5zZWFyY2gsXHJcblx0XHRcdFx0YSA9IHMuYWpheCA/ICQuZXh0ZW5kKHt9LCBzLmFqYXgpIDogZmFsc2UsXHJcblx0XHRcdFx0ZiA9IG51bGwsXHJcblx0XHRcdFx0ciA9IFtdLFxyXG5cdFx0XHRcdHAgPSBbXSwgaSwgajtcclxuXHRcdFx0aWYodGhpcy5fZGF0YS5zZWFyY2gucmVzLmxlbmd0aCkge1xyXG5cdFx0XHRcdHRoaXMuY2xlYXJfc2VhcmNoKCk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYoIXNraXBfYXN5bmMgJiYgYSAhPT0gZmFsc2UpIHtcclxuXHRcdFx0XHRpZighYS5kYXRhKSB7IGEuZGF0YSA9IHt9OyB9XHJcblx0XHRcdFx0YS5kYXRhLnN0ciA9IHN0cjtcclxuXHRcdFx0XHRyZXR1cm4gJC5hamF4KGEpXHJcblx0XHRcdFx0XHQuZmFpbCgkLnByb3h5KGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5fZGF0YS5jb3JlLmxhc3RfZXJyb3IgPSB7ICdlcnJvcicgOiAnYWpheCcsICdwbHVnaW4nIDogJ3NlYXJjaCcsICdpZCcgOiAnc2VhcmNoXzAxJywgJ3JlYXNvbicgOiAnQ291bGQgbm90IGxvYWQgc2VhcmNoIHBhcmVudHMnLCAnZGF0YScgOiBKU09OLnN0cmluZ2lmeShhKSB9O1xyXG5cdFx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLmNvcmUuZXJyb3IuY2FsbCh0aGlzLCB0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvcik7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHRcdC5kb25lKCQucHJveHkoZnVuY3Rpb24gKGQpIHtcclxuXHRcdFx0XHRcdFx0aWYoZCAmJiBkLmQpIHsgZCA9IGQuZDsgfVxyXG5cdFx0XHRcdFx0XHR0aGlzLl9kYXRhLnNlYXJjaC5zbG4gPSAhJC5pc0FycmF5KGQpID8gW10gOiBkO1xyXG5cdFx0XHRcdFx0XHR0aGlzLl9zZWFyY2hfbG9hZChzdHIpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLnN0ciA9IHN0cjtcclxuXHRcdFx0dGhpcy5fZGF0YS5zZWFyY2guZG9tID0gJCgpO1xyXG5cdFx0XHR0aGlzLl9kYXRhLnNlYXJjaC5yZXMgPSBbXTtcclxuXHRcdFx0dGhpcy5fZGF0YS5zZWFyY2gub3BuID0gW107XHJcblxyXG5cdFx0XHRmID0gbmV3ICQudmFrYXRhLnNlYXJjaChzdHIsIHRydWUsIHsgY2FzZVNlbnNpdGl2ZSA6IHMuY2FzZV9zZW5zaXRpdmUsIGZ1enp5IDogcy5mdXp6eSB9KTtcclxuXHJcblx0XHRcdCQuZWFjaCh0aGlzLl9tb2RlbC5kYXRhLCBmdW5jdGlvbiAoaSwgdikge1xyXG5cdFx0XHRcdGlmKHYudGV4dCAmJiBmLnNlYXJjaCh2LnRleHQpLmlzTWF0Y2gpIHtcclxuXHRcdFx0XHRcdHIucHVzaChpKTtcclxuXHRcdFx0XHRcdHAgPSBwLmNvbmNhdCh2LnBhcmVudHMpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fSk7XHJcblx0XHRcdGlmKHIubGVuZ3RoKSB7XHJcblx0XHRcdFx0cCA9ICQudmFrYXRhLmFycmF5X3VuaXF1ZShwKTtcclxuXHRcdFx0XHR0aGlzLl9zZWFyY2hfb3BlbihwKTtcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSByLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0ZiA9IHRoaXMuZ2V0X25vZGUocltpXSwgdHJ1ZSk7XHJcblx0XHRcdFx0XHRpZihmKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLmRvbSA9IHRoaXMuX2RhdGEuc2VhcmNoLmRvbS5hZGQoZik7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLnJlcyA9IHI7XHJcblx0XHRcdFx0dGhpcy5fZGF0YS5zZWFyY2guZG9tLmNoaWxkcmVuKFwiLmpzdHJlZS1hbmNob3JcIikuYWRkQ2xhc3MoJ2pzdHJlZS1zZWFyY2gnKTtcclxuXHRcdFx0fVxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIGFmdGVyIHNlYXJjaCBpcyBjb21wbGV0ZVxyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgc2VhcmNoLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge2pRdWVyeX0gbm9kZXMgYSBqUXVlcnkgY29sbGVjdGlvbiBvZiBtYXRjaGluZyBub2Rlc1xyXG5cdFx0XHQgKiBAcGFyYW0ge1N0cmluZ30gc3RyIHRoZSBzZWFyY2ggc3RyaW5nXHJcblx0XHRcdCAqIEBwYXJhbSB7QXJyYXl9IHJlcyBhIGNvbGxlY3Rpb24gb2Ygb2JqZWN0cyByZXByZXNlaW5nIHRoZSBtYXRjaGluZyBub2Rlc1xyXG5cdFx0XHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdzZWFyY2gnLCB7IG5vZGVzIDogdGhpcy5fZGF0YS5zZWFyY2guZG9tLCBzdHIgOiBzdHIsIHJlcyA6IHRoaXMuX2RhdGEuc2VhcmNoLnJlcyB9KTtcclxuXHRcdH07XHJcblx0XHQvKipcclxuXHRcdCAqIHVzZWQgdG8gY2xlYXIgdGhlIGxhc3Qgc2VhcmNoIChyZW1vdmVzIGNsYXNzZXMgYW5kIHNob3dzIGFsbCBub2RlcyBpZiBmaWx0ZXJpbmcgaXMgb24pXHJcblx0XHQgKiBAbmFtZSBjbGVhcl9zZWFyY2goKVxyXG5cdFx0ICogQHBsdWdpbiBzZWFyY2hcclxuXHRcdCAqIEB0cmlnZ2VyIGNsZWFyX3NlYXJjaC5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5jbGVhcl9zZWFyY2ggPSBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLmRvbS5jaGlsZHJlbihcIi5qc3RyZWUtYW5jaG9yXCIpLnJlbW92ZUNsYXNzKFwianN0cmVlLXNlYXJjaFwiKTtcclxuXHRcdFx0aWYodGhpcy5zZXR0aW5ncy5zZWFyY2guY2xvc2Vfb3BlbmVkX29uY2xlYXIpIHtcclxuXHRcdFx0XHR0aGlzLmNsb3NlX25vZGUodGhpcy5fZGF0YS5zZWFyY2gub3BuLCAwKTtcclxuXHRcdFx0fVxyXG5cdFx0XHQvKipcclxuXHRcdFx0ICogdHJpZ2dlcmVkIGFmdGVyIHNlYXJjaCBpcyBjb21wbGV0ZVxyXG5cdFx0XHQgKiBAZXZlbnRcclxuXHRcdFx0ICogQG5hbWUgY2xlYXJfc2VhcmNoLmpzdHJlZVxyXG5cdFx0XHQgKiBAcGFyYW0ge2pRdWVyeX0gbm9kZXMgYSBqUXVlcnkgY29sbGVjdGlvbiBvZiBtYXRjaGluZyBub2RlcyAodGhlIHJlc3VsdCBmcm9tIHRoZSBsYXN0IHNlYXJjaClcclxuXHRcdFx0ICogQHBhcmFtIHtTdHJpbmd9IHN0ciB0aGUgc2VhcmNoIHN0cmluZyAodGhlIGxhc3Qgc2VhcmNoIHN0cmluZylcclxuXHRcdFx0ICogQHBhcmFtIHtBcnJheX0gcmVzIGEgY29sbGVjdGlvbiBvZiBvYmplY3RzIHJlcHJlc2VpbmcgdGhlIG1hdGNoaW5nIG5vZGVzICh0aGUgcmVzdWx0IGZyb20gdGhlIGxhc3Qgc2VhcmNoKVxyXG5cdFx0XHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdFx0XHQgKi9cclxuXHRcdFx0dGhpcy50cmlnZ2VyKCdjbGVhcl9zZWFyY2gnLCB7ICdub2RlcycgOiB0aGlzLl9kYXRhLnNlYXJjaC5kb20sIHN0ciA6IHRoaXMuX2RhdGEuc2VhcmNoLnN0ciwgcmVzIDogdGhpcy5fZGF0YS5zZWFyY2gucmVzIH0pO1xyXG5cdFx0XHR0aGlzLl9kYXRhLnNlYXJjaC5zdHIgPSBcIlwiO1xyXG5cdFx0XHR0aGlzLl9kYXRhLnNlYXJjaC5yZXMgPSBbXTtcclxuXHRcdFx0dGhpcy5fZGF0YS5zZWFyY2gub3BuID0gW107XHJcblx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLmRvbSA9ICQoKTtcclxuXHRcdH07XHJcblx0XHQvKipcclxuXHRcdCAqIG9wZW5zIG5vZGVzIHRoYXQgbmVlZCB0byBiZSBvcGVuZWQgdG8gcmV2ZWFsIHRoZSBzZWFyY2ggcmVzdWx0cy4gVXNlZCBvbmx5IGludGVybmFsbHkuXHJcblx0XHQgKiBAcHJpdmF0ZVxyXG5cdFx0ICogQG5hbWUgX3NlYXJjaF9vcGVuKGQpXHJcblx0XHQgKiBAcGFyYW0ge0FycmF5fSBkIGFuIGFycmF5IG9mIG5vZGUgSURzXHJcblx0XHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdFx0ICovXHJcblx0XHR0aGlzLl9zZWFyY2hfb3BlbiA9IGZ1bmN0aW9uIChkKSB7XHJcblx0XHRcdHZhciB0ID0gdGhpcztcclxuXHRcdFx0JC5lYWNoKGQuY29uY2F0KFtdKSwgZnVuY3Rpb24gKGksIHYpIHtcclxuXHRcdFx0XHR2ID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodik7XHJcblx0XHRcdFx0aWYodikge1xyXG5cdFx0XHRcdFx0aWYodC5pc19jbG9zZWQodikpIHtcclxuXHRcdFx0XHRcdFx0dC5fZGF0YS5zZWFyY2gub3BuLnB1c2godi5pZCk7XHJcblx0XHRcdFx0XHRcdHQub3Blbl9ub2RlKHYsIGZ1bmN0aW9uICgpIHsgdC5fc2VhcmNoX29wZW4oZCk7IH0sIDApO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH1cclxuXHRcdFx0fSk7XHJcblx0XHR9O1xyXG5cdFx0LyoqXHJcblx0XHQgKiBsb2FkcyBub2RlcyB0aGF0IG5lZWQgdG8gYmUgb3BlbmVkIHRvIHJldmVhbCB0aGUgc2VhcmNoIHJlc3VsdHMuIFVzZWQgb25seSBpbnRlcm5hbGx5LlxyXG5cdFx0ICogQHByaXZhdGVcclxuXHRcdCAqIEBuYW1lIF9zZWFyY2hfbG9hZChkLCBzdHIpXHJcblx0XHQgKiBAcGFyYW0ge1N0cmluZ30gc3RyIHRoZSBzZWFyY2ggc3RyaW5nXHJcblx0XHQgKiBAcGx1Z2luIHNlYXJjaFxyXG5cdFx0ICovXHJcblx0XHR0aGlzLl9zZWFyY2hfbG9hZCA9IGZ1bmN0aW9uIChzdHIpIHtcclxuXHRcdFx0dmFyIHJlcyA9IHRydWUsXHJcblx0XHRcdFx0dCA9IHRoaXMsXHJcblx0XHRcdFx0bSA9IHQuX21vZGVsLmRhdGE7XHJcblx0XHRcdGlmKCQuaXNBcnJheSh0aGlzLl9kYXRhLnNlYXJjaC5zbG4pKSB7XHJcblx0XHRcdFx0aWYoIXRoaXMuX2RhdGEuc2VhcmNoLnNsbi5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdHRoaXMuX2RhdGEuc2VhcmNoLnNsbiA9IG51bGw7XHJcblx0XHRcdFx0XHR0aGlzLnNlYXJjaChzdHIsIHRydWUpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRlbHNlIHtcclxuXHRcdFx0XHRcdCQuZWFjaCh0aGlzLl9kYXRhLnNlYXJjaC5zbG4sIGZ1bmN0aW9uIChpLCB2KSB7XHJcblx0XHRcdFx0XHRcdGlmKG1bdl0pIHtcclxuXHRcdFx0XHRcdFx0XHQkLnZha2F0YS5hcnJheV9yZW1vdmVfaXRlbSh0Ll9kYXRhLnNlYXJjaC5zbG4sIHYpO1xyXG5cdFx0XHRcdFx0XHRcdGlmKCFtW3ZdLnN0YXRlLmxvYWRlZCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0dC5sb2FkX25vZGUodiwgZnVuY3Rpb24gKG8sIHMpIHsgaWYocykgeyB0Ll9zZWFyY2hfbG9hZChzdHIpOyB9IH0pO1xyXG5cdFx0XHRcdFx0XHRcdFx0cmVzID0gZmFsc2U7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9KTtcclxuXHRcdFx0XHRcdGlmKHJlcykge1xyXG5cdFx0XHRcdFx0XHR0aGlzLl9kYXRhLnNlYXJjaC5zbG4gPSBbXTtcclxuXHRcdFx0XHRcdFx0dGhpcy5fc2VhcmNoX2xvYWQoc3RyKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdH07XHJcblx0fTtcclxuXHJcblx0Ly8gaGVscGVyc1xyXG5cdChmdW5jdGlvbiAoJCkge1xyXG5cdFx0Ly8gZnJvbSBodHRwOi8va2lyby5tZS9wcm9qZWN0cy9mdXNlLmh0bWxcclxuXHRcdCQudmFrYXRhLnNlYXJjaCA9IGZ1bmN0aW9uKHBhdHRlcm4sIHR4dCwgb3B0aW9ucykge1xyXG5cdFx0XHRvcHRpb25zID0gb3B0aW9ucyB8fCB7fTtcclxuXHRcdFx0aWYob3B0aW9ucy5mdXp6eSAhPT0gZmFsc2UpIHtcclxuXHRcdFx0XHRvcHRpb25zLmZ1enp5ID0gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0XHRwYXR0ZXJuID0gb3B0aW9ucy5jYXNlU2Vuc2l0aXZlID8gcGF0dGVybiA6IHBhdHRlcm4udG9Mb3dlckNhc2UoKTtcclxuXHRcdFx0dmFyIE1BVENIX0xPQ0FUSU9OXHQ9IG9wdGlvbnMubG9jYXRpb24gfHwgMCxcclxuXHRcdFx0XHRNQVRDSF9ESVNUQU5DRVx0PSBvcHRpb25zLmRpc3RhbmNlIHx8IDEwMCxcclxuXHRcdFx0XHRNQVRDSF9USFJFU0hPTERcdD0gb3B0aW9ucy50aHJlc2hvbGQgfHwgMC42LFxyXG5cdFx0XHRcdHBhdHRlcm5MZW4gPSBwYXR0ZXJuLmxlbmd0aCxcclxuXHRcdFx0XHRtYXRjaG1hc2ssIHBhdHRlcm5fYWxwaGFiZXQsIG1hdGNoX2JpdGFwU2NvcmUsIHNlYXJjaDtcclxuXHRcdFx0aWYocGF0dGVybkxlbiA+IDMyKSB7XHJcblx0XHRcdFx0b3B0aW9ucy5mdXp6eSA9IGZhbHNlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKG9wdGlvbnMuZnV6enkpIHtcclxuXHRcdFx0XHRtYXRjaG1hc2sgPSAxIDw8IChwYXR0ZXJuTGVuIC0gMSk7XHJcblx0XHRcdFx0cGF0dGVybl9hbHBoYWJldCA9IChmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0XHR2YXIgbWFzayA9IHt9LFxyXG5cdFx0XHRcdFx0XHRpID0gMDtcclxuXHRcdFx0XHRcdGZvciAoaSA9IDA7IGkgPCBwYXR0ZXJuTGVuOyBpKyspIHtcclxuXHRcdFx0XHRcdFx0bWFza1twYXR0ZXJuLmNoYXJBdChpKV0gPSAwO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0Zm9yIChpID0gMDsgaSA8IHBhdHRlcm5MZW47IGkrKykge1xyXG5cdFx0XHRcdFx0XHRtYXNrW3BhdHRlcm4uY2hhckF0KGkpXSB8PSAxIDw8IChwYXR0ZXJuTGVuIC0gaSAtIDEpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0cmV0dXJuIG1hc2s7XHJcblx0XHRcdFx0fSgpKTtcclxuXHRcdFx0XHRtYXRjaF9iaXRhcFNjb3JlID0gZnVuY3Rpb24gKGUsIHgpIHtcclxuXHRcdFx0XHRcdHZhciBhY2N1cmFjeSA9IGUgLyBwYXR0ZXJuTGVuLFxyXG5cdFx0XHRcdFx0XHRwcm94aW1pdHkgPSBNYXRoLmFicyhNQVRDSF9MT0NBVElPTiAtIHgpO1xyXG5cdFx0XHRcdFx0aWYoIU1BVENIX0RJU1RBTkNFKSB7XHJcblx0XHRcdFx0XHRcdHJldHVybiBwcm94aW1pdHkgPyAxLjAgOiBhY2N1cmFjeTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdHJldHVybiBhY2N1cmFjeSArIChwcm94aW1pdHkgLyBNQVRDSF9ESVNUQU5DRSk7XHJcblx0XHRcdFx0fTtcclxuXHRcdFx0fVxyXG5cdFx0XHRzZWFyY2ggPSBmdW5jdGlvbiAodGV4dCkge1xyXG5cdFx0XHRcdHRleHQgPSBvcHRpb25zLmNhc2VTZW5zaXRpdmUgPyB0ZXh0IDogdGV4dC50b0xvd2VyQ2FzZSgpO1xyXG5cdFx0XHRcdGlmKHBhdHRlcm4gPT09IHRleHQgfHwgdGV4dC5pbmRleE9mKHBhdHRlcm4pICE9PSAtMSkge1xyXG5cdFx0XHRcdFx0cmV0dXJuIHtcclxuXHRcdFx0XHRcdFx0aXNNYXRjaDogdHJ1ZSxcclxuXHRcdFx0XHRcdFx0c2NvcmU6IDBcclxuXHRcdFx0XHRcdH07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGlmKCFvcHRpb25zLmZ1enp5KSB7XHJcblx0XHRcdFx0XHRyZXR1cm4ge1xyXG5cdFx0XHRcdFx0XHRpc01hdGNoOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0c2NvcmU6IDFcclxuXHRcdFx0XHRcdH07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHZhciBpLCBqLFxyXG5cdFx0XHRcdFx0dGV4dExlbiA9IHRleHQubGVuZ3RoLFxyXG5cdFx0XHRcdFx0c2NvcmVUaHJlc2hvbGQgPSBNQVRDSF9USFJFU0hPTEQsXHJcblx0XHRcdFx0XHRiZXN0TG9jID0gdGV4dC5pbmRleE9mKHBhdHRlcm4sIE1BVENIX0xPQ0FUSU9OKSxcclxuXHRcdFx0XHRcdGJpbk1pbiwgYmluTWlkLFxyXG5cdFx0XHRcdFx0YmluTWF4ID0gcGF0dGVybkxlbiArIHRleHRMZW4sXHJcblx0XHRcdFx0XHRsYXN0UmQsIHN0YXJ0LCBmaW5pc2gsIHJkLCBjaGFyTWF0Y2gsXHJcblx0XHRcdFx0XHRzY29yZSA9IDEsXHJcblx0XHRcdFx0XHRsb2NhdGlvbnMgPSBbXTtcclxuXHRcdFx0XHRpZiAoYmVzdExvYyAhPT0gLTEpIHtcclxuXHRcdFx0XHRcdHNjb3JlVGhyZXNob2xkID0gTWF0aC5taW4obWF0Y2hfYml0YXBTY29yZSgwLCBiZXN0TG9jKSwgc2NvcmVUaHJlc2hvbGQpO1xyXG5cdFx0XHRcdFx0YmVzdExvYyA9IHRleHQubGFzdEluZGV4T2YocGF0dGVybiwgTUFUQ0hfTE9DQVRJT04gKyBwYXR0ZXJuTGVuKTtcclxuXHRcdFx0XHRcdGlmIChiZXN0TG9jICE9PSAtMSkge1xyXG5cdFx0XHRcdFx0XHRzY29yZVRocmVzaG9sZCA9IE1hdGgubWluKG1hdGNoX2JpdGFwU2NvcmUoMCwgYmVzdExvYyksIHNjb3JlVGhyZXNob2xkKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0YmVzdExvYyA9IC0xO1xyXG5cdFx0XHRcdGZvciAoaSA9IDA7IGkgPCBwYXR0ZXJuTGVuOyBpKyspIHtcclxuXHRcdFx0XHRcdGJpbk1pbiA9IDA7XHJcblx0XHRcdFx0XHRiaW5NaWQgPSBiaW5NYXg7XHJcblx0XHRcdFx0XHR3aGlsZSAoYmluTWluIDwgYmluTWlkKSB7XHJcblx0XHRcdFx0XHRcdGlmIChtYXRjaF9iaXRhcFNjb3JlKGksIE1BVENIX0xPQ0FUSU9OICsgYmluTWlkKSA8PSBzY29yZVRocmVzaG9sZCkge1xyXG5cdFx0XHRcdFx0XHRcdGJpbk1pbiA9IGJpbk1pZDtcclxuXHRcdFx0XHRcdFx0fSBlbHNlIHtcclxuXHRcdFx0XHRcdFx0XHRiaW5NYXggPSBiaW5NaWQ7XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0YmluTWlkID0gTWF0aC5mbG9vcigoYmluTWF4IC0gYmluTWluKSAvIDIgKyBiaW5NaW4pO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0YmluTWF4ID0gYmluTWlkO1xyXG5cdFx0XHRcdFx0c3RhcnQgPSBNYXRoLm1heCgxLCBNQVRDSF9MT0NBVElPTiAtIGJpbk1pZCArIDEpO1xyXG5cdFx0XHRcdFx0ZmluaXNoID0gTWF0aC5taW4oTUFUQ0hfTE9DQVRJT04gKyBiaW5NaWQsIHRleHRMZW4pICsgcGF0dGVybkxlbjtcclxuXHRcdFx0XHRcdHJkID0gbmV3IEFycmF5KGZpbmlzaCArIDIpO1xyXG5cdFx0XHRcdFx0cmRbZmluaXNoICsgMV0gPSAoMSA8PCBpKSAtIDE7XHJcblx0XHRcdFx0XHRmb3IgKGogPSBmaW5pc2g7IGogPj0gc3RhcnQ7IGotLSkge1xyXG5cdFx0XHRcdFx0XHRjaGFyTWF0Y2ggPSBwYXR0ZXJuX2FscGhhYmV0W3RleHQuY2hhckF0KGogLSAxKV07XHJcblx0XHRcdFx0XHRcdGlmIChpID09PSAwKSB7XHJcblx0XHRcdFx0XHRcdFx0cmRbal0gPSAoKHJkW2ogKyAxXSA8PCAxKSB8IDEpICYgY2hhck1hdGNoO1xyXG5cdFx0XHRcdFx0XHR9IGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdHJkW2pdID0gKChyZFtqICsgMV0gPDwgMSkgfCAxKSAmIGNoYXJNYXRjaCB8ICgoKGxhc3RSZFtqICsgMV0gfCBsYXN0UmRbal0pIDw8IDEpIHwgMSkgfCBsYXN0UmRbaiArIDFdO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdGlmIChyZFtqXSAmIG1hdGNobWFzaykge1xyXG5cdFx0XHRcdFx0XHRcdHNjb3JlID0gbWF0Y2hfYml0YXBTY29yZShpLCBqIC0gMSk7XHJcblx0XHRcdFx0XHRcdFx0aWYgKHNjb3JlIDw9IHNjb3JlVGhyZXNob2xkKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRzY29yZVRocmVzaG9sZCA9IHNjb3JlO1xyXG5cdFx0XHRcdFx0XHRcdFx0YmVzdExvYyA9IGogLSAxO1xyXG5cdFx0XHRcdFx0XHRcdFx0bG9jYXRpb25zLnB1c2goYmVzdExvYyk7XHJcblx0XHRcdFx0XHRcdFx0XHRpZiAoYmVzdExvYyA+IE1BVENIX0xPQ0FUSU9OKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRcdHN0YXJ0ID0gTWF0aC5tYXgoMSwgMiAqIE1BVENIX0xPQ0FUSU9OIC0gYmVzdExvYyk7XHJcblx0XHRcdFx0XHRcdFx0XHR9IGVsc2Uge1xyXG5cdFx0XHRcdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGlmIChtYXRjaF9iaXRhcFNjb3JlKGkgKyAxLCBNQVRDSF9MT0NBVElPTikgPiBzY29yZVRocmVzaG9sZCkge1xyXG5cdFx0XHRcdFx0XHRicmVhaztcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGxhc3RSZCA9IHJkO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4ge1xyXG5cdFx0XHRcdFx0aXNNYXRjaDogYmVzdExvYyA+PSAwLFxyXG5cdFx0XHRcdFx0c2NvcmU6IHNjb3JlXHJcblx0XHRcdFx0fTtcclxuXHRcdFx0fTtcclxuXHRcdFx0cmV0dXJuIHR4dCA9PT0gdHJ1ZSA/IHsgJ3NlYXJjaCcgOiBzZWFyY2ggfSA6IHNlYXJjaCh0eHQpO1xyXG5cdFx0fTtcclxuXHR9KGpRdWVyeSkpO1xyXG5cclxuXHQvLyBpbmNsdWRlIHRoZSBzZWFyY2ggcGx1Z2luIGJ5IGRlZmF1bHRcclxuXHQvLyAkLmpzdHJlZS5kZWZhdWx0cy5wbHVnaW5zLnB1c2goXCJzZWFyY2hcIik7XHJcblxyXG4vKipcclxuICogIyMjIFNvcnQgcGx1Z2luXHJcbiAqXHJcbiAqIEF1dG1hdGljYWxseSBzb3J0cyBhbGwgc2libGluZ3MgaW4gdGhlIHRyZWUgYWNjb3JkaW5nIHRvIGEgc29ydGluZyBmdW5jdGlvbi5cclxuICovXHJcblxyXG5cdC8qKlxyXG5cdCAqIHRoZSBzZXR0aW5ncyBmdW5jdGlvbiB1c2VkIHRvIHNvcnQgdGhlIG5vZGVzLlxyXG5cdCAqIEl0IGlzIGV4ZWN1dGVkIGluIHRoZSB0cmVlJ3MgY29udGV4dCwgYWNjZXB0cyB0d28gbm9kZXMgYXMgYXJndW1lbnRzIGFuZCBzaG91bGQgcmV0dXJuIGAxYCBvciBgLTFgLlxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnNvcnRcclxuXHQgKiBAcGx1Z2luIHNvcnRcclxuXHQgKi9cclxuXHQkLmpzdHJlZS5kZWZhdWx0cy5zb3J0ID0gZnVuY3Rpb24gKGEsIGIpIHtcclxuXHRcdC8vcmV0dXJuIHRoaXMuZ2V0X3R5cGUoYSkgPT09IHRoaXMuZ2V0X3R5cGUoYikgPyAodGhpcy5nZXRfdGV4dChhKSA+IHRoaXMuZ2V0X3RleHQoYikgPyAxIDogLTEpIDogdGhpcy5nZXRfdHlwZShhKSA+PSB0aGlzLmdldF90eXBlKGIpO1xyXG5cdFx0cmV0dXJuIHRoaXMuZ2V0X3RleHQoYSkgPiB0aGlzLmdldF90ZXh0KGIpID8gMSA6IC0xO1xyXG5cdH07XHJcblx0JC5qc3RyZWUucGx1Z2lucy5zb3J0ID0gZnVuY3Rpb24gKG9wdGlvbnMsIHBhcmVudCkge1xyXG5cdFx0dGhpcy5iaW5kID0gZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRwYXJlbnQuYmluZC5jYWxsKHRoaXMpO1xyXG5cdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHQub24oXCJtb2RlbC5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLnNvcnQoZGF0YS5wYXJlbnQsIHRydWUpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwicmVuYW1lX25vZGUuanN0cmVlIGNyZWF0ZV9ub2RlLmpzdHJlZVwiLCAkLnByb3h5KGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuc29ydChkYXRhLnBhcmVudCB8fCBkYXRhLm5vZGUucGFyZW50LCBmYWxzZSk7XHJcblx0XHRcdFx0XHRcdHRoaXMucmVkcmF3X25vZGUoZGF0YS5wYXJlbnQgfHwgZGF0YS5ub2RlLnBhcmVudCwgdHJ1ZSk7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHQub24oXCJtb3ZlX25vZGUuanN0cmVlIGNvcHlfbm9kZS5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLnNvcnQoZGF0YS5wYXJlbnQsIGZhbHNlKTtcclxuXHRcdFx0XHRcdFx0dGhpcy5yZWRyYXdfbm9kZShkYXRhLnBhcmVudCwgdHJ1ZSk7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSk7XHJcblx0XHR9O1xyXG5cdFx0LyoqXHJcblx0XHQgKiB1c2VkIHRvIHNvcnQgYSBub2RlJ3MgY2hpbGRyZW5cclxuXHRcdCAqIEBwcml2YXRlXHJcblx0XHQgKiBAbmFtZSBzb3J0KG9iaiBbLCBkZWVwXSlcclxuXHRcdCAqIEBwYXJhbSAge21peGVkfSBvYmogdGhlIG5vZGVcclxuXHRcdCAqIEBwYXJhbSB7Qm9vbGVhbn0gZGVlcCBpZiBzZXQgdG8gYHRydWVgIG5vZGVzIGFyZSBzb3J0ZWQgcmVjdXJzaXZlbHkuXHJcblx0XHQgKiBAcGx1Z2luIHNvcnRcclxuXHRcdCAqIEB0cmlnZ2VyIHNlYXJjaC5qc3RyZWVcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5zb3J0ID0gZnVuY3Rpb24gKG9iaiwgZGVlcCkge1xyXG5cdFx0XHR2YXIgaSwgajtcclxuXHRcdFx0b2JqID0gdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRpZihvYmogJiYgb2JqLmNoaWxkcmVuICYmIG9iai5jaGlsZHJlbi5sZW5ndGgpIHtcclxuXHRcdFx0XHRvYmouY2hpbGRyZW4uc29ydCgkLnByb3h5KHRoaXMuc2V0dGluZ3Muc29ydCwgdGhpcykpO1xyXG5cdFx0XHRcdGlmKGRlZXApIHtcclxuXHRcdFx0XHRcdGZvcihpID0gMCwgaiA9IG9iai5jaGlsZHJlbl9kLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0XHR0aGlzLnNvcnQob2JqLmNoaWxkcmVuX2RbaV0sIGZhbHNlKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdH07XHJcblx0fTtcclxuXHJcblx0Ly8gaW5jbHVkZSB0aGUgc29ydCBwbHVnaW4gYnkgZGVmYXVsdFxyXG5cdC8vICQuanN0cmVlLmRlZmF1bHRzLnBsdWdpbnMucHVzaChcInNvcnRcIik7XHJcblxyXG4vKipcclxuICogIyMjIFN0YXRlIHBsdWdpblxyXG4gKlxyXG4gKiBTYXZlcyB0aGUgc3RhdGUgb2YgdGhlIHRyZWUgKHNlbGVjdGVkIG5vZGVzLCBvcGVuZWQgbm9kZXMpIG9uIHRoZSB1c2VyJ3MgY29tcHV0ZXIgdXNpbmcgYXZhaWxhYmxlIG9wdGlvbnMgKGxvY2FsU3RvcmFnZSwgY29va2llcywgZXRjKVxyXG4gKi9cclxuXHJcblx0dmFyIHRvID0gZmFsc2U7XHJcblx0LyoqXHJcblx0ICogc3RvcmVzIGFsbCBkZWZhdWx0cyBmb3IgdGhlIHN0YXRlIHBsdWdpblxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnN0YXRlXHJcblx0ICogQHBsdWdpbiBzdGF0ZVxyXG5cdCAqL1xyXG5cdCQuanN0cmVlLmRlZmF1bHRzLnN0YXRlID0ge1xyXG5cdFx0LyoqXHJcblx0XHQgKiBBIHN0cmluZyBmb3IgdGhlIGtleSB0byB1c2Ugd2hlbiBzYXZpbmcgdGhlIGN1cnJlbnQgdHJlZSAoY2hhbmdlIGlmIHVzaW5nIG11bHRpcGxlIHRyZWVzIGluIHlvdXIgcHJvamVjdCkuIERlZmF1bHRzIHRvIGBqc3RyZWVgLlxyXG5cdFx0ICogQG5hbWUgJC5qc3RyZWUuZGVmYXVsdHMuc3RhdGUua2V5XHJcblx0XHQgKiBAcGx1Z2luIHN0YXRlXHJcblx0XHQgKi9cclxuXHRcdGtleVx0XHQ6ICdqc3RyZWUnLFxyXG5cdFx0LyoqXHJcblx0XHQgKiBBIHNwYWNlIHNlcGFyYXRlZCBsaXN0IG9mIGV2ZW50cyB0aGF0IHRyaWdnZXIgYSBzdGF0ZSBzYXZlLiBEZWZhdWx0cyB0byBgY2hhbmdlZC5qc3RyZWUgb3Blbl9ub2RlLmpzdHJlZSBjbG9zZV9ub2RlLmpzdHJlZWAuXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5zdGF0ZS5ldmVudHNcclxuXHRcdCAqIEBwbHVnaW4gc3RhdGVcclxuXHRcdCAqL1xyXG5cdFx0ZXZlbnRzXHQ6ICdjaGFuZ2VkLmpzdHJlZSBvcGVuX25vZGUuanN0cmVlIGNsb3NlX25vZGUuanN0cmVlJyxcclxuXHRcdC8qKlxyXG5cdFx0ICogVGltZSBpbiBtaWxsaXNlY29uZHMgYWZ0ZXIgd2hpY2ggdGhlIHN0YXRlIHdpbGwgZXhwaXJlLiBEZWZhdWx0cyB0byAnZmFsc2UnIG1lYW5pbmcgLSBubyBleHBpcmUuXHJcblx0XHQgKiBAbmFtZSAkLmpzdHJlZS5kZWZhdWx0cy5zdGF0ZS50dGxcclxuXHRcdCAqIEBwbHVnaW4gc3RhdGVcclxuXHRcdCAqL1xyXG5cdFx0dHRsXHRcdDogZmFsc2UsXHJcblx0XHQvKipcclxuXHRcdCAqIEEgZnVuY3Rpb24gdGhhdCB3aWxsIGJlIGV4ZWN1dGVkIHByaW9yIHRvIHJlc3RvcmluZyBzdGF0ZSB3aXRoIG9uZSBhcmd1bWVudCAtIHRoZSBzdGF0ZSBvYmplY3QuIENhbiBiZSB1c2VkIHRvIGNsZWFyIHVud2FudGVkIHBhcnRzIG9mIHRoZSBzdGF0ZS5cclxuXHRcdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnN0YXRlLmZpbHRlclxyXG5cdFx0ICogQHBsdWdpbiBzdGF0ZVxyXG5cdFx0ICovXHJcblx0XHRmaWx0ZXJcdDogZmFsc2VcclxuXHR9O1xyXG5cdCQuanN0cmVlLnBsdWdpbnMuc3RhdGUgPSBmdW5jdGlvbiAob3B0aW9ucywgcGFyZW50KSB7XHJcblx0XHR0aGlzLmJpbmQgPSBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHBhcmVudC5iaW5kLmNhbGwodGhpcyk7XHJcblx0XHRcdHZhciBiaW5kID0gJC5wcm94eShmdW5jdGlvbiAoKSB7XHJcblx0XHRcdFx0dGhpcy5lbGVtZW50Lm9uKHRoaXMuc2V0dGluZ3Muc3RhdGUuZXZlbnRzLCAkLnByb3h5KGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdGlmKHRvKSB7IGNsZWFyVGltZW91dCh0byk7IH1cclxuXHRcdFx0XHRcdHRvID0gc2V0VGltZW91dCgkLnByb3h5KGZ1bmN0aW9uICgpIHsgdGhpcy5zYXZlX3N0YXRlKCk7IH0sIHRoaXMpLCAxMDApO1xyXG5cdFx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdFx0fSwgdGhpcyk7XHJcblx0XHRcdHRoaXMuZWxlbWVudFxyXG5cdFx0XHRcdC5vbihcInJlYWR5LmpzdHJlZVwiLCAkLnByb3h5KGZ1bmN0aW9uIChlLCBkYXRhKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuZWxlbWVudC5vbmUoXCJyZXN0b3JlX3N0YXRlLmpzdHJlZVwiLCBiaW5kKTtcclxuXHRcdFx0XHRcdFx0aWYoIXRoaXMucmVzdG9yZV9zdGF0ZSgpKSB7IGJpbmQoKTsgfVxyXG5cdFx0XHRcdFx0fSwgdGhpcykpO1xyXG5cdFx0fTtcclxuXHRcdC8qKlxyXG5cdFx0ICogc2F2ZSB0aGUgc3RhdGVcclxuXHRcdCAqIEBuYW1lIHNhdmVfc3RhdGUoKVxyXG5cdFx0ICogQHBsdWdpbiBzdGF0ZVxyXG5cdFx0ICovXHJcblx0XHR0aGlzLnNhdmVfc3RhdGUgPSBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHZhciBzdCA9IHsgJ3N0YXRlJyA6IHRoaXMuZ2V0X3N0YXRlKCksICd0dGwnIDogdGhpcy5zZXR0aW5ncy5zdGF0ZS50dGwsICdzZWMnIDogKyhuZXcgRGF0ZSgpKSB9O1xyXG5cdFx0XHQkLnZha2F0YS5zdG9yYWdlLnNldCh0aGlzLnNldHRpbmdzLnN0YXRlLmtleSwgSlNPTi5zdHJpbmdpZnkoc3QpKTtcclxuXHRcdH07XHJcblx0XHQvKipcclxuXHRcdCAqIHJlc3RvcmUgdGhlIHN0YXRlIGZyb20gdGhlIHVzZXIncyBjb21wdXRlclxyXG5cdFx0ICogQG5hbWUgcmVzdG9yZV9zdGF0ZSgpXHJcblx0XHQgKiBAcGx1Z2luIHN0YXRlXHJcblx0XHQgKi9cclxuXHRcdHRoaXMucmVzdG9yZV9zdGF0ZSA9IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0dmFyIGsgPSAkLnZha2F0YS5zdG9yYWdlLmdldCh0aGlzLnNldHRpbmdzLnN0YXRlLmtleSk7XHJcblx0XHRcdGlmKCEhaykgeyB0cnkgeyBrID0gSlNPTi5wYXJzZShrKTsgfSBjYXRjaChleCkgeyByZXR1cm4gZmFsc2U7IH0gfVxyXG5cdFx0XHRpZighIWsgJiYgay50dGwgJiYgay5zZWMgJiYgKyhuZXcgRGF0ZSgpKSAtIGsuc2VjID4gay50dGwpIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdGlmKCEhayAmJiBrLnN0YXRlKSB7IGsgPSBrLnN0YXRlOyB9XHJcblx0XHRcdGlmKCEhayAmJiAkLmlzRnVuY3Rpb24odGhpcy5zZXR0aW5ncy5zdGF0ZS5maWx0ZXIpKSB7IGsgPSB0aGlzLnNldHRpbmdzLnN0YXRlLmZpbHRlci5jYWxsKHRoaXMsIGspOyB9XHJcblx0XHRcdGlmKCEhaykge1xyXG5cdFx0XHRcdHRoaXMuZWxlbWVudC5vbmUoXCJzZXRfc3RhdGUuanN0cmVlXCIsIGZ1bmN0aW9uIChlLCBkYXRhKSB7IGRhdGEuaW5zdGFuY2UudHJpZ2dlcigncmVzdG9yZV9zdGF0ZScsIHsgJ3N0YXRlJyA6ICQuZXh0ZW5kKHRydWUsIHt9LCBrKSB9KTsgfSk7XHJcblx0XHRcdFx0dGhpcy5zZXRfc3RhdGUoayk7XHJcblx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0fTtcclxuXHRcdC8qKlxyXG5cdFx0ICogY2xlYXIgdGhlIHN0YXRlIG9uIHRoZSB1c2VyJ3MgY29tcHV0ZXJcclxuXHRcdCAqIEBuYW1lIGNsZWFyX3N0YXRlKClcclxuXHRcdCAqIEBwbHVnaW4gc3RhdGVcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5jbGVhcl9zdGF0ZSA9IGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0cmV0dXJuICQudmFrYXRhLnN0b3JhZ2UuZGVsKHRoaXMuc2V0dGluZ3Muc3RhdGUua2V5KTtcclxuXHRcdH07XHJcblx0fTtcclxuXHJcblx0KGZ1bmN0aW9uICgkLCB1bmRlZmluZWQpIHtcclxuXHRcdCQudmFrYXRhLnN0b3JhZ2UgPSB7XHJcblx0XHRcdC8vIHNpbXBseSBzcGVjaWZ5aW5nIHRoZSBmdW5jdGlvbnMgaW4gRkYgdGhyb3dzIGFuIGVycm9yXHJcblx0XHRcdHNldCA6IGZ1bmN0aW9uIChrZXksIHZhbCkgeyByZXR1cm4gd2luZG93LmxvY2FsU3RvcmFnZS5zZXRJdGVtKGtleSwgdmFsKTsgfSxcclxuXHRcdFx0Z2V0IDogZnVuY3Rpb24gKGtleSkgeyByZXR1cm4gd2luZG93LmxvY2FsU3RvcmFnZS5nZXRJdGVtKGtleSk7IH0sXHJcblx0XHRcdGRlbCA6IGZ1bmN0aW9uIChrZXkpIHsgcmV0dXJuIHdpbmRvdy5sb2NhbFN0b3JhZ2UucmVtb3ZlSXRlbShrZXkpOyB9XHJcblx0XHR9O1xyXG5cdH0oalF1ZXJ5KSk7XHJcblxyXG5cdC8vIGluY2x1ZGUgdGhlIHN0YXRlIHBsdWdpbiBieSBkZWZhdWx0XHJcblx0Ly8gJC5qc3RyZWUuZGVmYXVsdHMucGx1Z2lucy5wdXNoKFwic3RhdGVcIik7XHJcblxyXG4vKipcclxuICogIyMjIFR5cGVzIHBsdWdpblxyXG4gKlxyXG4gKiBNYWtlcyBpdCBwb3NzaWJsZSB0byBhZGQgcHJlZGVmaW5lZCB0eXBlcyBmb3IgZ3JvdXBzIG9mIG5vZGVzLCB3aGljaCBtYWtlIGl0IHBvc3NpYmxlIHRvIGVhc2lseSBjb250cm9sIG5lc3RpbmcgcnVsZXMgYW5kIGljb24gZm9yIGVhY2ggZ3JvdXAuXHJcbiAqL1xyXG5cclxuXHQvKipcclxuXHQgKiBBbiBvYmplY3Qgc3RvcmluZyBhbGwgdHlwZXMgYXMga2V5IHZhbHVlIHBhaXJzLCB3aGVyZSB0aGUga2V5IGlzIHRoZSB0eXBlIG5hbWUgYW5kIHRoZSB2YWx1ZSBpcyBhbiBvYmplY3QgdGhhdCBjb3VsZCBjb250YWluIGZvbGxvd2luZyBrZXlzIChhbGwgb3B0aW9uYWwpLlxyXG5cdCAqIFxyXG5cdCAqICogYG1heF9jaGlsZHJlbmAgdGhlIG1heGltdW0gbnVtYmVyIG9mIGltbWVkaWF0ZSBjaGlsZHJlbiB0aGlzIG5vZGUgdHlwZSBjYW4gaGF2ZS4gRG8gbm90IHNwZWNpZnkgb3Igc2V0IHRvIGAtMWAgZm9yIHVubGltaXRlZC5cclxuXHQgKiAqIGBtYXhfZGVwdGhgIHRoZSBtYXhpbXVtIG51bWJlciBvZiBuZXN0aW5nIHRoaXMgbm9kZSB0eXBlIGNhbiBoYXZlLiBBIHZhbHVlIG9mIGAxYCB3b3VsZCBtZWFuIHRoYXQgdGhlIG5vZGUgY2FuIGhhdmUgY2hpbGRyZW4sIGJ1dCBubyBncmFuZGNoaWxkcmVuLiBEbyBub3Qgc3BlY2lmeSBvciBzZXQgdG8gYC0xYCBmb3IgdW5saW1pdGVkLlxyXG5cdCAqICogYHZhbGlkX2NoaWxkcmVuYCBhbiBhcnJheSBvZiBub2RlIHR5cGUgc3RyaW5ncywgdGhhdCBub2RlcyBvZiB0aGlzIHR5cGUgY2FuIGhhdmUgYXMgY2hpbGRyZW4uIERvIG5vdCBzcGVjaWZ5IG9yIHNldCB0byBgLTFgIGZvciBubyBsaW1pdHMuXHJcblx0ICogKiBgaWNvbmAgYSBzdHJpbmcgLSBjYW4gYmUgYSBwYXRoIHRvIGFuIGljb24gb3IgYSBjbGFzc05hbWUsIGlmIHVzaW5nIGFuIGltYWdlIHRoYXQgaXMgaW4gdGhlIGN1cnJlbnQgZGlyZWN0b3J5IHVzZSBhIGAuL2AgcHJlZml4LCBvdGhlcndpc2UgaXQgd2lsbCBiZSBkZXRlY3RlZCBhcyBhIGNsYXNzLiBPbWl0IHRvIHVzZSB0aGUgZGVmYXVsdCBpY29uIGZyb20geW91ciB0aGVtZS5cclxuXHQgKlxyXG5cdCAqIFRoZXJlIGFyZSB0d28gcHJlZGVmaW5lZCB0eXBlczpcclxuXHQgKiBcclxuXHQgKiAqIGAjYCByZXByZXNlbnRzIHRoZSByb290IG9mIHRoZSB0cmVlLCBmb3IgZXhhbXBsZSBgbWF4X2NoaWxkcmVuYCB3b3VsZCBjb250cm9sIHRoZSBtYXhpbXVtIG51bWJlciBvZiByb290IG5vZGVzLlxyXG5cdCAqICogYGRlZmF1bHRgIHJlcHJlc2VudHMgdGhlIGRlZmF1bHQgbm9kZSAtIGFueSBzZXR0aW5ncyBoZXJlIHdpbGwgYmUgYXBwbGllZCB0byBhbGwgbm9kZXMgdGhhdCBkbyBub3QgaGF2ZSBhIHR5cGUgc3BlY2lmaWVkLlxyXG5cdCAqIFxyXG5cdCAqIEBuYW1lICQuanN0cmVlLmRlZmF1bHRzLnR5cGVzXHJcblx0ICogQHBsdWdpbiB0eXBlc1xyXG5cdCAqL1xyXG5cdCQuanN0cmVlLmRlZmF1bHRzLnR5cGVzID0ge1xyXG5cdFx0JyMnIDoge30sXHJcblx0XHQnZGVmYXVsdCcgOiB7fVxyXG5cdH07XHJcblxyXG5cdCQuanN0cmVlLnBsdWdpbnMudHlwZXMgPSBmdW5jdGlvbiAob3B0aW9ucywgcGFyZW50KSB7XHJcblx0XHR0aGlzLmluaXQgPSBmdW5jdGlvbiAoZWwsIG9wdGlvbnMpIHtcclxuXHRcdFx0dmFyIGksIGo7XHJcblx0XHRcdGlmKG9wdGlvbnMgJiYgb3B0aW9ucy50eXBlcyAmJiBvcHRpb25zLnR5cGVzWydkZWZhdWx0J10pIHtcclxuXHRcdFx0XHRmb3IoaSBpbiBvcHRpb25zLnR5cGVzKSB7XHJcblx0XHRcdFx0XHRpZihpICE9PSBcImRlZmF1bHRcIiAmJiBpICE9PSBcIiNcIiAmJiBvcHRpb25zLnR5cGVzLmhhc093blByb3BlcnR5KGkpKSB7XHJcblx0XHRcdFx0XHRcdGZvcihqIGluIG9wdGlvbnMudHlwZXNbJ2RlZmF1bHQnXSkge1xyXG5cdFx0XHRcdFx0XHRcdGlmKG9wdGlvbnMudHlwZXNbJ2RlZmF1bHQnXS5oYXNPd25Qcm9wZXJ0eShqKSAmJiBvcHRpb25zLnR5cGVzW2ldW2pdID09PSB1bmRlZmluZWQpIHtcclxuXHRcdFx0XHRcdFx0XHRcdG9wdGlvbnMudHlwZXNbaV1bal0gPSBvcHRpb25zLnR5cGVzWydkZWZhdWx0J11bal07XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdHBhcmVudC5pbml0LmNhbGwodGhpcywgZWwsIG9wdGlvbnMpO1xyXG5cdFx0XHR0aGlzLl9tb2RlbC5kYXRhWycjJ10udHlwZSA9ICcjJztcclxuXHRcdH07XHJcblx0XHR0aGlzLmJpbmQgPSBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHBhcmVudC5iaW5kLmNhbGwodGhpcyk7XHJcblx0XHRcdHRoaXMuZWxlbWVudFxyXG5cdFx0XHRcdC5vbignbW9kZWwuanN0cmVlJywgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHR2YXIgbSA9IHRoaXMuX21vZGVsLmRhdGEsXHJcblx0XHRcdFx0XHRcdFx0ZHBjID0gZGF0YS5ub2RlcyxcclxuXHRcdFx0XHRcdFx0XHR0ID0gdGhpcy5zZXR0aW5ncy50eXBlcyxcclxuXHRcdFx0XHRcdFx0XHRpLCBqLCBjID0gJ2RlZmF1bHQnO1xyXG5cdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSBkcGMubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHRcdFx0YyA9ICdkZWZhdWx0JztcclxuXHRcdFx0XHRcdFx0XHRpZihtW2RwY1tpXV0ub3JpZ2luYWwgJiYgbVtkcGNbaV1dLm9yaWdpbmFsLnR5cGUgJiYgdFttW2RwY1tpXV0ub3JpZ2luYWwudHlwZV0pIHtcclxuXHRcdFx0XHRcdFx0XHRcdGMgPSBtW2RwY1tpXV0ub3JpZ2luYWwudHlwZTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdFx0aWYobVtkcGNbaV1dLmRhdGEgJiYgbVtkcGNbaV1dLmRhdGEuanN0cmVlICYmIG1bZHBjW2ldXS5kYXRhLmpzdHJlZS50eXBlICYmIHRbbVtkcGNbaV1dLmRhdGEuanN0cmVlLnR5cGVdKSB7XHJcblx0XHRcdFx0XHRcdFx0XHRjID0gbVtkcGNbaV1dLmRhdGEuanN0cmVlLnR5cGU7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdG1bZHBjW2ldXS50eXBlID0gYztcclxuXHRcdFx0XHRcdFx0XHRpZihtW2RwY1tpXV0uaWNvbiA9PT0gdHJ1ZSAmJiB0W2NdLmljb24gIT09IHVuZGVmaW5lZCkge1xyXG5cdFx0XHRcdFx0XHRcdFx0bVtkcGNbaV1dLmljb24gPSB0W2NdLmljb247XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LCB0aGlzKSk7XHJcblx0XHR9O1xyXG5cdFx0dGhpcy5nZXRfanNvbiA9IGZ1bmN0aW9uIChvYmosIG9wdGlvbnMsIGZsYXQpIHtcclxuXHRcdFx0dmFyIGksIGosXHJcblx0XHRcdFx0bSA9IHRoaXMuX21vZGVsLmRhdGEsXHJcblx0XHRcdFx0b3B0ID0gb3B0aW9ucyA/ICQuZXh0ZW5kKHRydWUsIHt9LCBvcHRpb25zLCB7bm9faWQ6ZmFsc2V9KSA6IHt9LFxyXG5cdFx0XHRcdHRtcCA9IHBhcmVudC5nZXRfanNvbi5jYWxsKHRoaXMsIG9iaiwgb3B0LCBmbGF0KTtcclxuXHRcdFx0aWYodG1wID09PSBmYWxzZSkgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0aWYoJC5pc0FycmF5KHRtcCkpIHtcclxuXHRcdFx0XHRmb3IoaSA9IDAsIGogPSB0bXAubGVuZ3RoOyBpIDwgajsgaSsrKSB7XHJcblx0XHRcdFx0XHR0bXBbaV0udHlwZSA9IHRtcFtpXS5pZCAmJiBtW3RtcFtpXS5pZF0gJiYgbVt0bXBbaV0uaWRdLnR5cGUgPyBtW3RtcFtpXS5pZF0udHlwZSA6IFwiZGVmYXVsdFwiO1xyXG5cdFx0XHRcdFx0aWYob3B0aW9ucyAmJiBvcHRpb25zLm5vX2lkKSB7XHJcblx0XHRcdFx0XHRcdGRlbGV0ZSB0bXBbaV0uaWQ7XHJcblx0XHRcdFx0XHRcdGlmKHRtcFtpXS5saV9hdHRyICYmIHRtcFtpXS5saV9hdHRyLmlkKSB7XHJcblx0XHRcdFx0XHRcdFx0ZGVsZXRlIHRtcFtpXS5saV9hdHRyLmlkO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGVsc2Uge1xyXG5cdFx0XHRcdHRtcC50eXBlID0gdG1wLmlkICYmIG1bdG1wLmlkXSAmJiBtW3RtcC5pZF0udHlwZSA/IG1bdG1wLmlkXS50eXBlIDogXCJkZWZhdWx0XCI7XHJcblx0XHRcdFx0aWYob3B0aW9ucyAmJiBvcHRpb25zLm5vX2lkKSB7XHJcblx0XHRcdFx0XHR0bXAgPSB0aGlzLl9kZWxldGVfaWRzKHRtcCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB0bXA7XHJcblx0XHR9O1xyXG5cdFx0dGhpcy5fZGVsZXRlX2lkcyA9IGZ1bmN0aW9uICh0bXApIHtcclxuXHRcdFx0aWYoJC5pc0FycmF5KHRtcCkpIHtcclxuXHRcdFx0XHRmb3IodmFyIGkgPSAwLCBqID0gdG1wLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdFx0dG1wW2ldID0gdGhpcy5fZGVsZXRlX2lkcyh0bXBbaV0pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRyZXR1cm4gdG1wO1xyXG5cdFx0XHR9XHJcblx0XHRcdGRlbGV0ZSB0bXAuaWQ7XHJcblx0XHRcdGlmKHRtcC5saV9hdHRyICYmIHRtcC5saV9hdHRyLmlkKSB7XHJcblx0XHRcdFx0ZGVsZXRlIHRtcC5saV9hdHRyLmlkO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmKHRtcC5jaGlsZHJlbiAmJiAkLmlzQXJyYXkodG1wLmNoaWxkcmVuKSkge1xyXG5cdFx0XHRcdHRtcC5jaGlsZHJlbiA9IHRoaXMuX2RlbGV0ZV9pZHModG1wLmNoaWxkcmVuKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdG1wO1xyXG5cdFx0fTtcclxuXHRcdHRoaXMuY2hlY2sgPSBmdW5jdGlvbiAoY2hrLCBvYmosIHBhciwgcG9zKSB7XHJcblx0XHRcdGlmKHBhcmVudC5jaGVjay5jYWxsKHRoaXMsIGNoaywgb2JqLCBwYXIsIHBvcykgPT09IGZhbHNlKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRvYmogPSBvYmogJiYgb2JqLmlkID8gb2JqIDogdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRwYXIgPSBwYXIgJiYgcGFyLmlkID8gcGFyIDogdGhpcy5nZXRfbm9kZShwYXIpO1xyXG5cdFx0XHR2YXIgbSA9IG9iaiAmJiBvYmouaWQgPyAkLmpzdHJlZS5yZWZlcmVuY2Uob2JqLmlkKSA6IG51bGwsIHRtcCwgZCwgaSwgajtcclxuXHRcdFx0bSA9IG0gJiYgbS5fbW9kZWwgJiYgbS5fbW9kZWwuZGF0YSA/IG0uX21vZGVsLmRhdGEgOiBudWxsO1xyXG5cdFx0XHRzd2l0Y2goY2hrKSB7XHJcblx0XHRcdFx0Y2FzZSBcImNyZWF0ZV9ub2RlXCI6XHJcblx0XHRcdFx0Y2FzZSBcIm1vdmVfbm9kZVwiOlxyXG5cdFx0XHRcdGNhc2UgXCJjb3B5X25vZGVcIjpcclxuXHRcdFx0XHRcdGlmKGNoayAhPT0gJ21vdmVfbm9kZScgfHwgJC5pbkFycmF5KG9iai5pZCwgcGFyLmNoaWxkcmVuKSA9PT0gLTEpIHtcclxuXHRcdFx0XHRcdFx0dG1wID0gdGhpcy5nZXRfcnVsZXMocGFyKTtcclxuXHRcdFx0XHRcdFx0aWYodG1wLm1heF9jaGlsZHJlbiAhPT0gdW5kZWZpbmVkICYmIHRtcC5tYXhfY2hpbGRyZW4gIT09IC0xICYmIHRtcC5tYXhfY2hpbGRyZW4gPT09IHBhci5jaGlsZHJlbi5sZW5ndGgpIHtcclxuXHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvciA9IHsgJ2Vycm9yJyA6ICdjaGVjaycsICdwbHVnaW4nIDogJ3R5cGVzJywgJ2lkJyA6ICd0eXBlc18wMScsICdyZWFzb24nIDogJ21heF9jaGlsZHJlbiBwcmV2ZW50cyBmdW5jdGlvbjogJyArIGNoaywgJ2RhdGEnIDogSlNPTi5zdHJpbmdpZnkoeyAnY2hrJyA6IGNoaywgJ3BvcycgOiBwb3MsICdvYmonIDogb2JqICYmIG9iai5pZCA/IG9iai5pZCA6IGZhbHNlLCAncGFyJyA6IHBhciAmJiBwYXIuaWQgPyBwYXIuaWQgOiBmYWxzZSB9KSB9O1xyXG5cdFx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRpZih0bXAudmFsaWRfY2hpbGRyZW4gIT09IHVuZGVmaW5lZCAmJiB0bXAudmFsaWRfY2hpbGRyZW4gIT09IC0xICYmICQuaW5BcnJheShvYmoudHlwZSwgdG1wLnZhbGlkX2NoaWxkcmVuKSA9PT0gLTEpIHtcclxuXHRcdFx0XHRcdFx0XHR0aGlzLl9kYXRhLmNvcmUubGFzdF9lcnJvciA9IHsgJ2Vycm9yJyA6ICdjaGVjaycsICdwbHVnaW4nIDogJ3R5cGVzJywgJ2lkJyA6ICd0eXBlc18wMicsICdyZWFzb24nIDogJ3ZhbGlkX2NoaWxkcmVuIHByZXZlbnRzIGZ1bmN0aW9uOiAnICsgY2hrLCAnZGF0YScgOiBKU09OLnN0cmluZ2lmeSh7ICdjaGsnIDogY2hrLCAncG9zJyA6IHBvcywgJ29iaicgOiBvYmogJiYgb2JqLmlkID8gb2JqLmlkIDogZmFsc2UsICdwYXInIDogcGFyICYmIHBhci5pZCA/IHBhci5pZCA6IGZhbHNlIH0pIH07XHJcblx0XHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdGlmKG0gJiYgb2JqLmNoaWxkcmVuX2QgJiYgb2JqLnBhcmVudHMpIHtcclxuXHRcdFx0XHRcdFx0XHRkID0gMDtcclxuXHRcdFx0XHRcdFx0XHRmb3IoaSA9IDAsIGogPSBvYmouY2hpbGRyZW5fZC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdFx0XHRcdGQgPSBNYXRoLm1heChkLCBtW29iai5jaGlsZHJlbl9kW2ldXS5wYXJlbnRzLmxlbmd0aCk7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdGQgPSBkIC0gb2JqLnBhcmVudHMubGVuZ3RoICsgMTtcclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRpZihkIDw9IDAgfHwgZCA9PT0gdW5kZWZpbmVkKSB7IGQgPSAxOyB9XHJcblx0XHRcdFx0XHRcdGRvIHtcclxuXHRcdFx0XHRcdFx0XHRpZih0bXAubWF4X2RlcHRoICE9PSB1bmRlZmluZWQgJiYgdG1wLm1heF9kZXB0aCAhPT0gLTEgJiYgdG1wLm1heF9kZXB0aCA8IGQpIHtcclxuXHRcdFx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5sYXN0X2Vycm9yID0geyAnZXJyb3InIDogJ2NoZWNrJywgJ3BsdWdpbicgOiAndHlwZXMnLCAnaWQnIDogJ3R5cGVzXzAzJywgJ3JlYXNvbicgOiAnbWF4X2RlcHRoIHByZXZlbnRzIGZ1bmN0aW9uOiAnICsgY2hrLCAnZGF0YScgOiBKU09OLnN0cmluZ2lmeSh7ICdjaGsnIDogY2hrLCAncG9zJyA6IHBvcywgJ29iaicgOiBvYmogJiYgb2JqLmlkID8gb2JqLmlkIDogZmFsc2UsICdwYXInIDogcGFyICYmIHBhci5pZCA/IHBhci5pZCA6IGZhbHNlIH0pIH07XHJcblx0XHRcdFx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0XHRcdHBhciA9IHRoaXMuZ2V0X25vZGUocGFyLnBhcmVudCk7XHJcblx0XHRcdFx0XHRcdFx0dG1wID0gdGhpcy5nZXRfcnVsZXMocGFyKTtcclxuXHRcdFx0XHRcdFx0XHRkKys7XHJcblx0XHRcdFx0XHRcdH0gd2hpbGUocGFyKTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fTtcclxuXHRcdC8qKlxyXG5cdFx0ICogdXNlZCB0byByZXRyaWV2ZSB0aGUgdHlwZSBzZXR0aW5ncyBvYmplY3QgZm9yIGEgbm9kZVxyXG5cdFx0ICogQG5hbWUgZ2V0X3J1bGVzKG9iailcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9iaiB0aGUgbm9kZSB0byBmaW5kIHRoZSBydWxlcyBmb3JcclxuXHRcdCAqIEByZXR1cm4ge09iamVjdH1cclxuXHRcdCAqIEBwbHVnaW4gdHlwZXNcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5nZXRfcnVsZXMgPSBmdW5jdGlvbiAob2JqKSB7XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0aWYoIW9iaikgeyByZXR1cm4gZmFsc2U7IH1cclxuXHRcdFx0dmFyIHRtcCA9IHRoaXMuZ2V0X3R5cGUob2JqLCB0cnVlKTtcclxuXHRcdFx0aWYodG1wLm1heF9kZXB0aCA9PT0gdW5kZWZpbmVkKSB7IHRtcC5tYXhfZGVwdGggPSAtMTsgfVxyXG5cdFx0XHRpZih0bXAubWF4X2NoaWxkcmVuID09PSB1bmRlZmluZWQpIHsgdG1wLm1heF9jaGlsZHJlbiA9IC0xOyB9XHJcblx0XHRcdGlmKHRtcC52YWxpZF9jaGlsZHJlbiA9PT0gdW5kZWZpbmVkKSB7IHRtcC52YWxpZF9jaGlsZHJlbiA9IC0xOyB9XHJcblx0XHRcdHJldHVybiB0bXA7XHJcblx0XHR9O1xyXG5cdFx0LyoqXHJcblx0XHQgKiB1c2VkIHRvIHJldHJpZXZlIHRoZSB0eXBlIHN0cmluZyBvciBzZXR0aW5ncyBvYmplY3QgZm9yIGEgbm9kZVxyXG5cdFx0ICogQG5hbWUgZ2V0X3R5cGUob2JqIFssIHJ1bGVzXSlcclxuXHRcdCAqIEBwYXJhbSB7bWl4ZWR9IG9iaiB0aGUgbm9kZSB0byBmaW5kIHRoZSBydWxlcyBmb3JcclxuXHRcdCAqIEBwYXJhbSB7Qm9vbGVhbn0gcnVsZXMgaWYgc2V0IHRvIGB0cnVlYCBpbnN0ZWFkIG9mIGEgc3RyaW5nIHRoZSBzZXR0aW5ncyBvYmplY3Qgd2lsbCBiZSByZXR1cm5lZFxyXG5cdFx0ICogQHJldHVybiB7U3RyaW5nfE9iamVjdH1cclxuXHRcdCAqIEBwbHVnaW4gdHlwZXNcclxuXHRcdCAqL1xyXG5cdFx0dGhpcy5nZXRfdHlwZSA9IGZ1bmN0aW9uIChvYmosIHJ1bGVzKSB7XHJcblx0XHRcdG9iaiA9IHRoaXMuZ2V0X25vZGUob2JqKTtcclxuXHRcdFx0cmV0dXJuICghb2JqKSA/IGZhbHNlIDogKCBydWxlcyA/ICQuZXh0ZW5kKHsgJ3R5cGUnIDogb2JqLnR5cGUgfSwgdGhpcy5zZXR0aW5ncy50eXBlc1tvYmoudHlwZV0pIDogb2JqLnR5cGUpO1xyXG5cdFx0fTtcclxuXHRcdC8qKlxyXG5cdFx0ICogdXNlZCB0byBjaGFuZ2UgYSBub2RlJ3MgdHlwZVxyXG5cdFx0ICogQG5hbWUgc2V0X3R5cGUob2JqLCB0eXBlKVxyXG5cdFx0ICogQHBhcmFtIHttaXhlZH0gb2JqIHRoZSBub2RlIHRvIGNoYW5nZVxyXG5cdFx0ICogQHBhcmFtIHtTdHJpbmd9IHR5cGUgdGhlIG5ldyB0eXBlXHJcblx0XHQgKiBAcGx1Z2luIHR5cGVzXHJcblx0XHQgKi9cclxuXHRcdHRoaXMuc2V0X3R5cGUgPSBmdW5jdGlvbiAob2JqLCB0eXBlKSB7XHJcblx0XHRcdHZhciB0LCB0MSwgdDIsIG9sZF90eXBlLCBvbGRfaWNvbjtcclxuXHRcdFx0aWYoJC5pc0FycmF5KG9iaikpIHtcclxuXHRcdFx0XHRvYmogPSBvYmouc2xpY2UoKTtcclxuXHRcdFx0XHRmb3IodDEgPSAwLCB0MiA9IG9iai5sZW5ndGg7IHQxIDwgdDI7IHQxKyspIHtcclxuXHRcdFx0XHRcdHRoaXMuc2V0X3R5cGUob2JqW3QxXSwgdHlwZSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHR9XHJcblx0XHRcdHQgPSB0aGlzLnNldHRpbmdzLnR5cGVzO1xyXG5cdFx0XHRvYmogPSB0aGlzLmdldF9ub2RlKG9iaik7XHJcblx0XHRcdGlmKCF0W3R5cGVdIHx8ICFvYmopIHsgcmV0dXJuIGZhbHNlOyB9XHJcblx0XHRcdG9sZF90eXBlID0gb2JqLnR5cGU7XHJcblx0XHRcdG9sZF9pY29uID0gdGhpcy5nZXRfaWNvbihvYmopO1xyXG5cdFx0XHRvYmoudHlwZSA9IHR5cGU7XHJcblx0XHRcdGlmKG9sZF9pY29uID09PSB0cnVlIHx8ICh0W29sZF90eXBlXSAmJiB0W29sZF90eXBlXS5pY29uICYmIG9sZF9pY29uID09PSB0W29sZF90eXBlXS5pY29uKSkge1xyXG5cdFx0XHRcdHRoaXMuc2V0X2ljb24ob2JqLCB0W3R5cGVdLmljb24gIT09IHVuZGVmaW5lZCA/IHRbdHlwZV0uaWNvbiA6IHRydWUpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fTtcclxuXHR9O1xyXG5cdC8vIGluY2x1ZGUgdGhlIHR5cGVzIHBsdWdpbiBieSBkZWZhdWx0XHJcblx0Ly8gJC5qc3RyZWUuZGVmYXVsdHMucGx1Z2lucy5wdXNoKFwidHlwZXNcIik7XHJcblxyXG4vKipcclxuICogIyMjIFVuaXF1ZSBwbHVnaW5cclxuICpcclxuICogRW5mb3JjZXMgdGhhdCBubyBub2RlcyB3aXRoIHRoZSBzYW1lIG5hbWUgY2FuIGNvZXhpc3QgYXMgc2libGluZ3MuXHJcbiAqL1xyXG5cclxuXHQkLmpzdHJlZS5wbHVnaW5zLnVuaXF1ZSA9IGZ1bmN0aW9uIChvcHRpb25zLCBwYXJlbnQpIHtcclxuXHRcdHRoaXMuY2hlY2sgPSBmdW5jdGlvbiAoY2hrLCBvYmosIHBhciwgcG9zKSB7XHJcblx0XHRcdGlmKHBhcmVudC5jaGVjay5jYWxsKHRoaXMsIGNoaywgb2JqLCBwYXIsIHBvcykgPT09IGZhbHNlKSB7IHJldHVybiBmYWxzZTsgfVxyXG5cdFx0XHRvYmogPSBvYmogJiYgb2JqLmlkID8gb2JqIDogdGhpcy5nZXRfbm9kZShvYmopO1xyXG5cdFx0XHRwYXIgPSBwYXIgJiYgcGFyLmlkID8gcGFyIDogdGhpcy5nZXRfbm9kZShwYXIpO1xyXG5cdFx0XHRpZighcGFyIHx8ICFwYXIuY2hpbGRyZW4pIHsgcmV0dXJuIHRydWU7IH1cclxuXHRcdFx0dmFyIG4gPSBjaGsgPT09IFwicmVuYW1lX25vZGVcIiA/IHBvcyA6IG9iai50ZXh0LFxyXG5cdFx0XHRcdGMgPSBbXSxcclxuXHRcdFx0XHRtID0gdGhpcy5fbW9kZWwuZGF0YSwgaSwgajtcclxuXHRcdFx0Zm9yKGkgPSAwLCBqID0gcGFyLmNoaWxkcmVuLmxlbmd0aDsgaSA8IGo7IGkrKykge1xyXG5cdFx0XHRcdGMucHVzaChtW3Bhci5jaGlsZHJlbltpXV0udGV4dCk7XHJcblx0XHRcdH1cclxuXHRcdFx0c3dpdGNoKGNoaykge1xyXG5cdFx0XHRcdGNhc2UgXCJkZWxldGVfbm9kZVwiOlxyXG5cdFx0XHRcdFx0cmV0dXJuIHRydWU7XHJcblx0XHRcdFx0Y2FzZSBcInJlbmFtZV9ub2RlXCI6XHJcblx0XHRcdFx0Y2FzZSBcImNvcHlfbm9kZVwiOlxyXG5cdFx0XHRcdFx0aSA9ICgkLmluQXJyYXkobiwgYykgPT09IC0xKTtcclxuXHRcdFx0XHRcdGlmKCFpKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5sYXN0X2Vycm9yID0geyAnZXJyb3InIDogJ2NoZWNrJywgJ3BsdWdpbicgOiAndW5pcXVlJywgJ2lkJyA6ICd1bmlxdWVfMDEnLCAncmVhc29uJyA6ICdDaGlsZCB3aXRoIG5hbWUgJyArIG4gKyAnIGFscmVhZHkgZXhpc3RzLiBQcmV2ZW50aW5nOiAnICsgY2hrLCAnZGF0YScgOiBKU09OLnN0cmluZ2lmeSh7ICdjaGsnIDogY2hrLCAncG9zJyA6IHBvcywgJ29iaicgOiBvYmogJiYgb2JqLmlkID8gb2JqLmlkIDogZmFsc2UsICdwYXInIDogcGFyICYmIHBhci5pZCA/IHBhci5pZCA6IGZhbHNlIH0pIH07XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRyZXR1cm4gaTtcclxuXHRcdFx0XHRjYXNlIFwibW92ZV9ub2RlXCI6XHJcblx0XHRcdFx0XHRpID0gKG9iai5wYXJlbnQgPT09IHBhci5pZCB8fCAkLmluQXJyYXkobiwgYykgPT09IC0xKTtcclxuXHRcdFx0XHRcdGlmKCFpKSB7XHJcblx0XHRcdFx0XHRcdHRoaXMuX2RhdGEuY29yZS5sYXN0X2Vycm9yID0geyAnZXJyb3InIDogJ2NoZWNrJywgJ3BsdWdpbicgOiAndW5pcXVlJywgJ2lkJyA6ICd1bmlxdWVfMDEnLCAncmVhc29uJyA6ICdDaGlsZCB3aXRoIG5hbWUgJyArIG4gKyAnIGFscmVhZHkgZXhpc3RzLiBQcmV2ZW50aW5nOiAnICsgY2hrLCAnZGF0YScgOiBKU09OLnN0cmluZ2lmeSh7ICdjaGsnIDogY2hrLCAncG9zJyA6IHBvcywgJ29iaicgOiBvYmogJiYgb2JqLmlkID8gb2JqLmlkIDogZmFsc2UsICdwYXInIDogcGFyICYmIHBhci5pZCA/IHBhci5pZCA6IGZhbHNlIH0pIH07XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRyZXR1cm4gaTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdH07XHJcblx0fTtcclxuXHJcblx0Ly8gaW5jbHVkZSB0aGUgdW5pcXVlIHBsdWdpbiBieSBkZWZhdWx0XHJcblx0Ly8gJC5qc3RyZWUuZGVmYXVsdHMucGx1Z2lucy5wdXNoKFwidW5pcXVlXCIpO1xyXG5cclxuXHJcbi8qKlxyXG4gKiAjIyMgV2hvbGVyb3cgcGx1Z2luXHJcbiAqXHJcbiAqIE1ha2VzIGVhY2ggbm9kZSBhcHBlYXIgYmxvY2sgbGV2ZWwuIE1ha2luZyBzZWxlY3Rpb24gZWFzaWVyLiBNYXkgY2F1c2Ugc2xvdyBkb3duIGZvciBsYXJnZSB0cmVlcyBpbiBvbGQgYnJvd3NlcnMuXHJcbiAqL1xyXG5cclxuXHR2YXIgZGl2ID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnRElWJyk7XHJcblx0ZGl2LnNldEF0dHJpYnV0ZSgndW5zZWxlY3RhYmxlJywnb24nKTtcclxuXHRkaXYuY2xhc3NOYW1lID0gJ2pzdHJlZS13aG9sZXJvdyc7XHJcblx0ZGl2LmlubmVySFRNTCA9ICcmIzE2MDsnO1xyXG5cdCQuanN0cmVlLnBsdWdpbnMud2hvbGVyb3cgPSBmdW5jdGlvbiAob3B0aW9ucywgcGFyZW50KSB7XHJcblx0XHR0aGlzLmJpbmQgPSBmdW5jdGlvbiAoKSB7XHJcblx0XHRcdHBhcmVudC5iaW5kLmNhbGwodGhpcyk7XHJcblxyXG5cdFx0XHR0aGlzLmVsZW1lbnRcclxuXHRcdFx0XHQub24oJ2xvYWRpbmcnLCAkLnByb3h5KGZ1bmN0aW9uICgpIHtcclxuXHRcdFx0XHRcdFx0ZGl2LnN0eWxlLmhlaWdodCA9IHRoaXMuX2RhdGEuY29yZS5saV9oZWlnaHQgKyAncHgnO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKCdyZWFkeS5qc3RyZWUgc2V0X3N0YXRlLmpzdHJlZScsICQucHJveHkoZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmhpZGVfZG90cygpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwicmVhZHkuanN0cmVlXCIsICQucHJveHkoZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmdldF9jb250YWluZXJfdWwoKS5hZGRDbGFzcygnanN0cmVlLXdob2xlcm93LXVsJyk7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHQub24oXCJkZXNlbGVjdF9hbGwuanN0cmVlXCIsICQucHJveHkoZnVuY3Rpb24gKGUsIGRhdGEpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5lbGVtZW50LmZpbmQoJy5qc3RyZWUtd2hvbGVyb3ctY2xpY2tlZCcpLnJlbW92ZUNsYXNzKCdqc3RyZWUtd2hvbGVyb3ctY2xpY2tlZCcpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwiY2hhbmdlZC5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmVsZW1lbnQuZmluZCgnLmpzdHJlZS13aG9sZXJvdy1jbGlja2VkJykucmVtb3ZlQ2xhc3MoJ2pzdHJlZS13aG9sZXJvdy1jbGlja2VkJyk7XHJcblx0XHRcdFx0XHRcdHZhciB0bXAgPSBmYWxzZSwgaSwgajtcclxuXHRcdFx0XHRcdFx0Zm9yKGkgPSAwLCBqID0gZGF0YS5zZWxlY3RlZC5sZW5ndGg7IGkgPCBqOyBpKyspIHtcclxuXHRcdFx0XHRcdFx0XHR0bXAgPSB0aGlzLmdldF9ub2RlKGRhdGEuc2VsZWN0ZWRbaV0sIHRydWUpO1xyXG5cdFx0XHRcdFx0XHRcdGlmKHRtcCAmJiB0bXAubGVuZ3RoKSB7XHJcblx0XHRcdFx0XHRcdFx0XHR0bXAuY2hpbGRyZW4oJy5qc3RyZWUtd2hvbGVyb3cnKS5hZGRDbGFzcygnanN0cmVlLXdob2xlcm93LWNsaWNrZWQnKTtcclxuXHRcdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdC5vbihcIm9wZW5fbm9kZS5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmdldF9ub2RlKGRhdGEubm9kZSwgdHJ1ZSkuZmluZCgnLmpzdHJlZS1jbGlja2VkJykucGFyZW50KCkuY2hpbGRyZW4oJy5qc3RyZWUtd2hvbGVyb3cnKS5hZGRDbGFzcygnanN0cmVlLXdob2xlcm93LWNsaWNrZWQnKTtcclxuXHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdC5vbihcImhvdmVyX25vZGUuanN0cmVlIGRlaG92ZXJfbm9kZS5qc3RyZWVcIiwgJC5wcm94eShmdW5jdGlvbiAoZSwgZGF0YSkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLmdldF9ub2RlKGRhdGEubm9kZSwgdHJ1ZSkuY2hpbGRyZW4oJy5qc3RyZWUtd2hvbGVyb3cnKVtlLnR5cGUgPT09IFwiaG92ZXJfbm9kZVwiP1wiYWRkQ2xhc3NcIjpcInJlbW92ZUNsYXNzXCJdKCdqc3RyZWUtd2hvbGVyb3ctaG92ZXJlZCcpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwiY29udGV4dG1lbnUuanN0cmVlXCIsIFwiLmpzdHJlZS13aG9sZXJvd1wiLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcclxuXHRcdFx0XHRcdFx0JChlLmN1cnJlbnRUYXJnZXQpLmNsb3Nlc3QoXCJsaVwiKS5jaGlsZHJlbihcImE6ZXEoMClcIikudHJpZ2dlcignY29udGV4dG1lbnUnLGUpO1xyXG5cdFx0XHRcdFx0fSwgdGhpcykpXHJcblx0XHRcdFx0Lm9uKFwiY2xpY2suanN0cmVlXCIsIFwiLmpzdHJlZS13aG9sZXJvd1wiLCBmdW5jdGlvbiAoZSkge1xyXG5cdFx0XHRcdFx0XHRlLnN0b3BJbW1lZGlhdGVQcm9wYWdhdGlvbigpO1xyXG5cdFx0XHRcdFx0XHR2YXIgdG1wID0gJC5FdmVudCgnY2xpY2snLCB7IG1ldGFLZXkgOiBlLm1ldGFLZXksIGN0cmxLZXkgOiBlLmN0cmxLZXksIGFsdEtleSA6IGUuYWx0S2V5LCBzaGlmdEtleSA6IGUuc2hpZnRLZXkgfSk7XHJcblx0XHRcdFx0XHRcdCQoZS5jdXJyZW50VGFyZ2V0KS5jbG9zZXN0KFwibGlcIikuY2hpbGRyZW4oXCJhOmVxKDApXCIpLnRyaWdnZXIodG1wKS5mb2N1cygpO1xyXG5cdFx0XHRcdFx0fSlcclxuXHRcdFx0XHQub24oXCJjbGljay5qc3RyZWVcIiwgXCIuanN0cmVlLWxlYWYgPiAuanN0cmVlLW9jbFwiLCAkLnByb3h5KGZ1bmN0aW9uIChlKSB7XHJcblx0XHRcdFx0XHRcdGUuc3RvcEltbWVkaWF0ZVByb3BhZ2F0aW9uKCk7XHJcblx0XHRcdFx0XHRcdHZhciB0bXAgPSAkLkV2ZW50KCdjbGljaycsIHsgbWV0YUtleSA6IGUubWV0YUtleSwgY3RybEtleSA6IGUuY3RybEtleSwgYWx0S2V5IDogZS5hbHRLZXksIHNoaWZ0S2V5IDogZS5zaGlmdEtleSB9KTtcclxuXHRcdFx0XHRcdFx0JChlLmN1cnJlbnRUYXJnZXQpLmNsb3Nlc3QoXCJsaVwiKS5jaGlsZHJlbihcImE6ZXEoMClcIikudHJpZ2dlcih0bXApLmZvY3VzKCk7XHJcblx0XHRcdFx0XHR9LCB0aGlzKSlcclxuXHRcdFx0XHQub24oXCJtb3VzZW92ZXIuanN0cmVlXCIsIFwiLmpzdHJlZS13aG9sZXJvdywgLmpzdHJlZS1pY29uXCIsICQucHJveHkoZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRcdFx0ZS5zdG9wSW1tZWRpYXRlUHJvcGFnYXRpb24oKTtcclxuXHRcdFx0XHRcdFx0dGhpcy5ob3Zlcl9ub2RlKGUuY3VycmVudFRhcmdldCk7XHJcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdH0sIHRoaXMpKVxyXG5cdFx0XHRcdC5vbihcIm1vdXNlbGVhdmUuanN0cmVlXCIsIFwiLmpzdHJlZS1ub2RlXCIsICQucHJveHkoZnVuY3Rpb24gKGUpIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5kZWhvdmVyX25vZGUoZS5jdXJyZW50VGFyZ2V0KTtcclxuXHRcdFx0XHRcdH0sIHRoaXMpKTtcclxuXHRcdH07XHJcblx0XHR0aGlzLnRlYXJkb3duID0gZnVuY3Rpb24gKCkge1xyXG5cdFx0XHRpZih0aGlzLnNldHRpbmdzLndob2xlcm93KSB7XHJcblx0XHRcdFx0dGhpcy5lbGVtZW50LmZpbmQoXCIuanN0cmVlLXdob2xlcm93XCIpLnJlbW92ZSgpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHBhcmVudC50ZWFyZG93bi5jYWxsKHRoaXMpO1xyXG5cdFx0fTtcclxuXHRcdHRoaXMucmVkcmF3X25vZGUgPSBmdW5jdGlvbihvYmosIGRlZXAsIGNhbGxiYWNrKSB7XHJcblx0XHRcdG9iaiA9IHBhcmVudC5yZWRyYXdfbm9kZS5jYWxsKHRoaXMsIG9iaiwgZGVlcCwgY2FsbGJhY2spO1xyXG5cdFx0XHRpZihvYmopIHtcclxuXHRcdFx0XHR2YXIgdG1wID0gZGl2LmNsb25lTm9kZSh0cnVlKTtcclxuXHRcdFx0XHQvL3RtcC5zdHlsZS5oZWlnaHQgPSB0aGlzLl9kYXRhLmNvcmUubGlfaGVpZ2h0ICsgJ3B4JztcclxuXHRcdFx0XHRpZigkLmluQXJyYXkob2JqLmlkLCB0aGlzLl9kYXRhLmNvcmUuc2VsZWN0ZWQpICE9PSAtMSkgeyB0bXAuY2xhc3NOYW1lICs9ICcganN0cmVlLXdob2xlcm93LWNsaWNrZWQnOyB9XHJcblx0XHRcdFx0b2JqLmluc2VydEJlZm9yZSh0bXAsIG9iai5jaGlsZE5vZGVzWzBdKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gb2JqO1xyXG5cdFx0fTtcclxuXHR9O1xyXG5cdC8vIGluY2x1ZGUgdGhlIHdob2xlcm93IHBsdWdpbiBieSBkZWZhdWx0XHJcblx0Ly8gJC5qc3RyZWUuZGVmYXVsdHMucGx1Z2lucy5wdXNoKFwid2hvbGVyb3dcIik7XHJcblxyXG59KSk7Il0sImZpbGUiOiJqc3RyZWUuanMiLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==