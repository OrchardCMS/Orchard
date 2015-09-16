/* NUGET: BEGIN LICENSE TEXT
 *
 * Microsoft grants you the right to use these script files for the sole
 * purpose of either: (i) interacting through your browser with the Microsoft
 * website or online service, subject to the applicable licensing or use
 * terms; or (ii) using the files as included with a Microsoft product subject
 * to that product's license terms. Microsoft reserves all other rights to the
 * files not expressly granted by Microsoft, whether by implication, estoppel
 * or otherwise. Insofar as a script file is dual licensed under GPL,
 * Microsoft neither took the code under GPL nor distributes it thereunder but
 * under the terms set out in this paragraph. All notices and licenses
 * below are for informational purposes only.
 *
 * NUGET: END LICENSE TEXT */
/*!
 * jQuery Validation Plugin 1.11.1
 *
 * http://bassistance.de/jquery-plugins/jquery-plugin-validation/
 * http://docs.jquery.com/Plugins/Validation
 *
 * Copyright 2013 Jörn Zaefferer
 * Released under the MIT license:
 *   http://www.opensource.org/licenses/mit-license.php
 */

(function($) {

$.extend($.fn, {
	// http://docs.jquery.com/Plugins/Validation/validate
	validate: function( options ) {

		// if nothing is selected, return nothing; can't chain anyway
		if ( !this.length ) {
			if ( options && options.debug && window.console ) {
				console.warn( "Nothing selected, can't validate, returning nothing." );
			}
			return;
		}

		// check if a validator for this form was already created
		var validator = $.data( this[0], "validator" );
		if ( validator ) {
			return validator;
		}

		// Add novalidate tag if HTML5.
		this.attr( "novalidate", "novalidate" );

		validator = new $.validator( options, this[0] );
		$.data( this[0], "validator", validator );

		if ( validator.settings.onsubmit ) {

			this.validateDelegate( ":submit", "click", function( event ) {
				if ( validator.settings.submitHandler ) {
					validator.submitButton = event.target;
				}
				// allow suppressing validation by adding a cancel class to the submit button
				if ( $(event.target).hasClass("cancel") ) {
					validator.cancelSubmit = true;
				}

				// allow suppressing validation by adding the html5 formnovalidate attribute to the submit button
				if ( $(event.target).attr("formnovalidate") !== undefined ) {
					validator.cancelSubmit = true;
				}
			});

			// validate the form on submit
			this.submit( function( event ) {
				if ( validator.settings.debug ) {
					// prevent form submit to be able to see console output
					event.preventDefault();
				}
				function handle() {
					var hidden;
					if ( validator.settings.submitHandler ) {
						if ( validator.submitButton ) {
							// insert a hidden input as a replacement for the missing submit button
							hidden = $("<input type='hidden'/>").attr("name", validator.submitButton.name).val( $(validator.submitButton).val() ).appendTo(validator.currentForm);
						}
						validator.settings.submitHandler.call( validator, validator.currentForm, event );
						if ( validator.submitButton ) {
							// and clean up afterwards; thanks to no-block-scope, hidden can be referenced
							hidden.remove();
						}
						return false;
					}
					return true;
				}

				// prevent submit for invalid forms or custom submit handlers
				if ( validator.cancelSubmit ) {
					validator.cancelSubmit = false;
					return handle();
				}
				if ( validator.form() ) {
					if ( validator.pendingRequest ) {
						validator.formSubmitted = true;
						return false;
					}
					return handle();
				} else {
					validator.focusInvalid();
					return false;
				}
			});
		}

		return validator;
	},
	// http://docs.jquery.com/Plugins/Validation/valid
	valid: function() {
		if ( $(this[0]).is("form")) {
			return this.validate().form();
		} else {
			var valid = true;
			var validator = $(this[0].form).validate();
			this.each(function() {
				valid = valid && validator.element(this);
			});
			return valid;
		}
	},
	// attributes: space seperated list of attributes to retrieve and remove
	removeAttrs: function( attributes ) {
		var result = {},
			$element = this;
		$.each(attributes.split(/\s/), function( index, value ) {
			result[value] = $element.attr(value);
			$element.removeAttr(value);
		});
		return result;
	},
	// http://docs.jquery.com/Plugins/Validation/rules
	rules: function( command, argument ) {
		var element = this[0];

		if ( command ) {
			var settings = $.data(element.form, "validator").settings;
			var staticRules = settings.rules;
			var existingRules = $.validator.staticRules(element);
			switch(command) {
			case "add":
				$.extend(existingRules, $.validator.normalizeRule(argument));
				// remove messages from rules, but allow them to be set separetely
				delete existingRules.messages;
				staticRules[element.name] = existingRules;
				if ( argument.messages ) {
					settings.messages[element.name] = $.extend( settings.messages[element.name], argument.messages );
				}
				break;
			case "remove":
				if ( !argument ) {
					delete staticRules[element.name];
					return existingRules;
				}
				var filtered = {};
				$.each(argument.split(/\s/), function( index, method ) {
					filtered[method] = existingRules[method];
					delete existingRules[method];
				});
				return filtered;
			}
		}

		var data = $.validator.normalizeRules(
		$.extend(
			{},
			$.validator.classRules(element),
			$.validator.attributeRules(element),
			$.validator.dataRules(element),
			$.validator.staticRules(element)
		), element);

		// make sure required is at front
		if ( data.required ) {
			var param = data.required;
			delete data.required;
			data = $.extend({required: param}, data);
		}

		return data;
	}
});

// Custom selectors
$.extend($.expr[":"], {
	// http://docs.jquery.com/Plugins/Validation/blank
	blank: function( a ) { return !$.trim("" + $(a).val()); },
	// http://docs.jquery.com/Plugins/Validation/filled
	filled: function( a ) { return !!$.trim("" + $(a).val()); },
	// http://docs.jquery.com/Plugins/Validation/unchecked
	unchecked: function( a ) { return !$(a).prop("checked"); }
});

// constructor for validator
$.validator = function( options, form ) {
	this.settings = $.extend( true, {}, $.validator.defaults, options );
	this.currentForm = form;
	this.init();
};

$.validator.format = function( source, params ) {
	if ( arguments.length === 1 ) {
		return function() {
			var args = $.makeArray(arguments);
			args.unshift(source);
			return $.validator.format.apply( this, args );
		};
	}
	if ( arguments.length > 2 && params.constructor !== Array  ) {
		params = $.makeArray(arguments).slice(1);
	}
	if ( params.constructor !== Array ) {
		params = [ params ];
	}
	$.each(params, function( i, n ) {
		source = source.replace( new RegExp("\\{" + i + "\\}", "g"), function() {
			return n;
		});
	});
	return source;
};

$.extend($.validator, {

	defaults: {
		messages: {},
		groups: {},
		rules: {},
		errorClass: "error",
		validClass: "valid",
		errorElement: "label",
		focusInvalid: true,
		errorContainer: $([]),
		errorLabelContainer: $([]),
		onsubmit: true,
		ignore: ":hidden",
		ignoreTitle: false,
		onfocusin: function( element, event ) {
			this.lastActive = element;

			// hide error label and remove error class on focus if enabled
			if ( this.settings.focusCleanup && !this.blockFocusCleanup ) {
				if ( this.settings.unhighlight ) {
					this.settings.unhighlight.call( this, element, this.settings.errorClass, this.settings.validClass );
				}
				this.addWrapper(this.errorsFor(element)).hide();
			}
		},
		onfocusout: function( element, event ) {
			if ( !this.checkable(element) && (element.name in this.submitted || !this.optional(element)) ) {
				this.element(element);
			}
		},
		onkeyup: function( element, event ) {
			if ( event.which === 9 && this.elementValue(element) === "" ) {
				return;
			} else if ( element.name in this.submitted || element === this.lastElement ) {
				this.element(element);
			}
		},
		onclick: function( element, event ) {
			// click on selects, radiobuttons and checkboxes
			if ( element.name in this.submitted ) {
				this.element(element);
			}
			// or option elements, check parent select in that case
			else if ( element.parentNode.name in this.submitted ) {
				this.element(element.parentNode);
			}
		},
		highlight: function( element, errorClass, validClass ) {
			if ( element.type === "radio" ) {
				this.findByName(element.name).addClass(errorClass).removeClass(validClass);
			} else {
				$(element).addClass(errorClass).removeClass(validClass);
			}
		},
		unhighlight: function( element, errorClass, validClass ) {
			if ( element.type === "radio" ) {
				this.findByName(element.name).removeClass(errorClass).addClass(validClass);
			} else {
				$(element).removeClass(errorClass).addClass(validClass);
			}
		}
	},

	// http://docs.jquery.com/Plugins/Validation/Validator/setDefaults
	setDefaults: function( settings ) {
		$.extend( $.validator.defaults, settings );
	},

	messages: {
		required: "This field is required.",
		remote: "Please fix this field.",
		email: "Please enter a valid email address.",
		url: "Please enter a valid URL.",
		date: "Please enter a valid date.",
		dateISO: "Please enter a valid date (ISO).",
		number: "Please enter a valid number.",
		digits: "Please enter only digits.",
		creditcard: "Please enter a valid credit card number.",
		equalTo: "Please enter the same value again.",
		maxlength: $.validator.format("Please enter no more than {0} characters."),
		minlength: $.validator.format("Please enter at least {0} characters."),
		rangelength: $.validator.format("Please enter a value between {0} and {1} characters long."),
		range: $.validator.format("Please enter a value between {0} and {1}."),
		max: $.validator.format("Please enter a value less than or equal to {0}."),
		min: $.validator.format("Please enter a value greater than or equal to {0}.")
	},

	autoCreateRanges: false,

	prototype: {

		init: function() {
			this.labelContainer = $(this.settings.errorLabelContainer);
			this.errorContext = this.labelContainer.length && this.labelContainer || $(this.currentForm);
			this.containers = $(this.settings.errorContainer).add( this.settings.errorLabelContainer );
			this.submitted = {};
			this.valueCache = {};
			this.pendingRequest = 0;
			this.pending = {};
			this.invalid = {};
			this.reset();

			var groups = (this.groups = {});
			$.each(this.settings.groups, function( key, value ) {
				if ( typeof value === "string" ) {
					value = value.split(/\s/);
				}
				$.each(value, function( index, name ) {
					groups[name] = key;
				});
			});
			var rules = this.settings.rules;
			$.each(rules, function( key, value ) {
				rules[key] = $.validator.normalizeRule(value);
			});

			function delegate(event) {
				var validator = $.data(this[0].form, "validator"),
					eventType = "on" + event.type.replace(/^validate/, "");
				if ( validator.settings[eventType] ) {
					validator.settings[eventType].call(validator, this[0], event);
				}
			}
			$(this.currentForm)
				.validateDelegate(":text, [type='password'], [type='file'], select, textarea, " +
					"[type='number'], [type='search'] ,[type='tel'], [type='url'], " +
					"[type='email'], [type='datetime'], [type='date'], [type='month'], " +
					"[type='week'], [type='time'], [type='datetime-local'], " +
					"[type='range'], [type='color'] ",
					"focusin focusout keyup", delegate)
				.validateDelegate("[type='radio'], [type='checkbox'], select, option", "click", delegate);

			if ( this.settings.invalidHandler ) {
				$(this.currentForm).bind("invalid-form.validate", this.settings.invalidHandler);
			}
		},

		// http://docs.jquery.com/Plugins/Validation/Validator/form
		form: function() {
			this.checkForm();
			$.extend(this.submitted, this.errorMap);
			this.invalid = $.extend({}, this.errorMap);
			if ( !this.valid() ) {
				$(this.currentForm).triggerHandler("invalid-form", [this]);
			}
			this.showErrors();
			return this.valid();
		},

		checkForm: function() {
			this.prepareForm();
			for ( var i = 0, elements = (this.currentElements = this.elements()); elements[i]; i++ ) {
				this.check( elements[i] );
			}
			return this.valid();
		},

		// http://docs.jquery.com/Plugins/Validation/Validator/element
		element: function( element ) {
			element = this.validationTargetFor( this.clean( element ) );
			this.lastElement = element;
			this.prepareElement( element );
			this.currentElements = $(element);
			var result = this.check( element ) !== false;
			if ( result ) {
				delete this.invalid[element.name];
			} else {
				this.invalid[element.name] = true;
			}
			if ( !this.numberOfInvalids() ) {
				// Hide error containers on last error
				this.toHide = this.toHide.add( this.containers );
			}
			this.showErrors();
			return result;
		},

		// http://docs.jquery.com/Plugins/Validation/Validator/showErrors
		showErrors: function( errors ) {
			if ( errors ) {
				// add items to error list and map
				$.extend( this.errorMap, errors );
				this.errorList = [];
				for ( var name in errors ) {
					this.errorList.push({
						message: errors[name],
						element: this.findByName(name)[0]
					});
				}
				// remove items from success list
				this.successList = $.grep( this.successList, function( element ) {
					return !(element.name in errors);
				});
			}
			if ( this.settings.showErrors ) {
				this.settings.showErrors.call( this, this.errorMap, this.errorList );
			} else {
				this.defaultShowErrors();
			}
		},

		// http://docs.jquery.com/Plugins/Validation/Validator/resetForm
		resetForm: function() {
			if ( $.fn.resetForm ) {
				$(this.currentForm).resetForm();
			}
			this.submitted = {};
			this.lastElement = null;
			this.prepareForm();
			this.hideErrors();
			this.elements().removeClass( this.settings.errorClass ).removeData( "previousValue" );
		},

		numberOfInvalids: function() {
			return this.objectLength(this.invalid);
		},

		objectLength: function( obj ) {
			var count = 0;
			for ( var i in obj ) {
				count++;
			}
			return count;
		},

		hideErrors: function() {
			this.addWrapper( this.toHide ).hide();
		},

		valid: function() {
			return this.size() === 0;
		},

		size: function() {
			return this.errorList.length;
		},

		focusInvalid: function() {
			if ( this.settings.focusInvalid ) {
				try {
					$(this.findLastActive() || this.errorList.length && this.errorList[0].element || [])
					.filter(":visible")
					.focus()
					// manually trigger focusin event; without it, focusin handler isn't called, findLastActive won't have anything to find
					.trigger("focusin");
				} catch(e) {
					// ignore IE throwing errors when focusing hidden elements
				}
			}
		},

		findLastActive: function() {
			var lastActive = this.lastActive;
			return lastActive && $.grep(this.errorList, function( n ) {
				return n.element.name === lastActive.name;
			}).length === 1 && lastActive;
		},

		elements: function() {
			var validator = this,
				rulesCache = {};

			// select all valid inputs inside the form (no submit or reset buttons)
			return $(this.currentForm)
			.find("input, select, textarea")
			.not(":submit, :reset, :image, [disabled]")
			.not( this.settings.ignore )
			.filter(function() {
				if ( !this.name && validator.settings.debug && window.console ) {
					console.error( "%o has no name assigned", this);
				}

				// select only the first element for each name, and only those with rules specified
				if ( this.name in rulesCache || !validator.objectLength($(this).rules()) ) {
					return false;
				}

				rulesCache[this.name] = true;
				return true;
			});
		},

		clean: function( selector ) {
			return $(selector)[0];
		},

		errors: function() {
			var errorClass = this.settings.errorClass.replace(" ", ".");
			return $(this.settings.errorElement + "." + errorClass, this.errorContext);
		},

		reset: function() {
			this.successList = [];
			this.errorList = [];
			this.errorMap = {};
			this.toShow = $([]);
			this.toHide = $([]);
			this.currentElements = $([]);
		},

		prepareForm: function() {
			this.reset();
			this.toHide = this.errors().add( this.containers );
		},

		prepareElement: function( element ) {
			this.reset();
			this.toHide = this.errorsFor(element);
		},

		elementValue: function( element ) {
			var type = $(element).attr("type"),
				val = $(element).val();

			if ( type === "radio" || type === "checkbox" ) {
				return $("input[name='" + $(element).attr("name") + "']:checked").val();
			}

			if ( typeof val === "string" ) {
				return val.replace(/\r/g, "");
			}
			return val;
		},

		check: function( element ) {
			element = this.validationTargetFor( this.clean( element ) );

			var rules = $(element).rules();
			var dependencyMismatch = false;
			var val = this.elementValue(element);
			var result;

			for (var method in rules ) {
				var rule = { method: method, parameters: rules[method] };
				try {

					result = $.validator.methods[method].call( this, val, element, rule.parameters );

					// if a method indicates that the field is optional and therefore valid,
					// don't mark it as valid when there are no other rules
					if ( result === "dependency-mismatch" ) {
						dependencyMismatch = true;
						continue;
					}
					dependencyMismatch = false;

					if ( result === "pending" ) {
						this.toHide = this.toHide.not( this.errorsFor(element) );
						return;
					}

					if ( !result ) {
						this.formatAndAdd( element, rule );
						return false;
					}
				} catch(e) {
					if ( this.settings.debug && window.console ) {
						console.log( "Exception occurred when checking element " + element.id + ", check the '" + rule.method + "' method.", e );
					}
					throw e;
				}
			}
			if ( dependencyMismatch ) {
				return;
			}
			if ( this.objectLength(rules) ) {
				this.successList.push(element);
			}
			return true;
		},

		// return the custom message for the given element and validation method
		// specified in the element's HTML5 data attribute
		customDataMessage: function( element, method ) {
			return $(element).data("msg-" + method.toLowerCase()) || (element.attributes && $(element).attr("data-msg-" + method.toLowerCase()));
		},

		// return the custom message for the given element name and validation method
		customMessage: function( name, method ) {
			var m = this.settings.messages[name];
			return m && (m.constructor === String ? m : m[method]);
		},

		// return the first defined argument, allowing empty strings
		findDefined: function() {
			for(var i = 0; i < arguments.length; i++) {
				if ( arguments[i] !== undefined ) {
					return arguments[i];
				}
			}
			return undefined;
		},

		defaultMessage: function( element, method ) {
			return this.findDefined(
				this.customMessage( element.name, method ),
				this.customDataMessage( element, method ),
				// title is never undefined, so handle empty string as undefined
				!this.settings.ignoreTitle && element.title || undefined,
				$.validator.messages[method],
				"<strong>Warning: No message defined for " + element.name + "</strong>"
			);
		},

		formatAndAdd: function( element, rule ) {
			var message = this.defaultMessage( element, rule.method ),
				theregex = /\$?\{(\d+)\}/g;
			if ( typeof message === "function" ) {
				message = message.call(this, rule.parameters, element);
			} else if (theregex.test(message)) {
				message = $.validator.format(message.replace(theregex, "{$1}"), rule.parameters);
			}
			this.errorList.push({
				message: message,
				element: element
			});

			this.errorMap[element.name] = message;
			this.submitted[element.name] = message;
		},

		addWrapper: function( toToggle ) {
			if ( this.settings.wrapper ) {
				toToggle = toToggle.add( toToggle.parent( this.settings.wrapper ) );
			}
			return toToggle;
		},

		defaultShowErrors: function() {
			var i, elements;
			for ( i = 0; this.errorList[i]; i++ ) {
				var error = this.errorList[i];
				if ( this.settings.highlight ) {
					this.settings.highlight.call( this, error.element, this.settings.errorClass, this.settings.validClass );
				}
				this.showLabel( error.element, error.message );
			}
			if ( this.errorList.length ) {
				this.toShow = this.toShow.add( this.containers );
			}
			if ( this.settings.success ) {
				for ( i = 0; this.successList[i]; i++ ) {
					this.showLabel( this.successList[i] );
				}
			}
			if ( this.settings.unhighlight ) {
				for ( i = 0, elements = this.validElements(); elements[i]; i++ ) {
					this.settings.unhighlight.call( this, elements[i], this.settings.errorClass, this.settings.validClass );
				}
			}
			this.toHide = this.toHide.not( this.toShow );
			this.hideErrors();
			this.addWrapper( this.toShow ).show();
		},

		validElements: function() {
			return this.currentElements.not(this.invalidElements());
		},

		invalidElements: function() {
			return $(this.errorList).map(function() {
				return this.element;
			});
		},

		showLabel: function( element, message ) {
			var label = this.errorsFor( element );
			if ( label.length ) {
				// refresh error/success class
				label.removeClass( this.settings.validClass ).addClass( this.settings.errorClass );
				// replace message on existing label
				label.html(message);
			} else {
				// create label
				label = $("<" + this.settings.errorElement + ">")
					.attr("for", this.idOrName(element))
					.addClass(this.settings.errorClass)
					.html(message || "");
				if ( this.settings.wrapper ) {
					// make sure the element is visible, even in IE
					// actually showing the wrapped element is handled elsewhere
					label = label.hide().show().wrap("<" + this.settings.wrapper + "/>").parent();
				}
				if ( !this.labelContainer.append(label).length ) {
					if ( this.settings.errorPlacement ) {
						this.settings.errorPlacement(label, $(element) );
					} else {
						label.insertAfter(element);
					}
				}
			}
			if ( !message && this.settings.success ) {
				label.text("");
				if ( typeof this.settings.success === "string" ) {
					label.addClass( this.settings.success );
				} else {
					this.settings.success( label, element );
				}
			}
			this.toShow = this.toShow.add(label);
		},

		errorsFor: function( element ) {
			var name = this.idOrName(element);
			return this.errors().filter(function() {
				return $(this).attr("for") === name;
			});
		},

		idOrName: function( element ) {
			return this.groups[element.name] || (this.checkable(element) ? element.name : element.id || element.name);
		},

		validationTargetFor: function( element ) {
			// if radio/checkbox, validate first element in group instead
			if ( this.checkable(element) ) {
				element = this.findByName( element.name ).not(this.settings.ignore)[0];
			}
			return element;
		},

		checkable: function( element ) {
			return (/radio|checkbox/i).test(element.type);
		},

		findByName: function( name ) {
			return $(this.currentForm).find("[name='" + name + "']");
		},

		getLength: function( value, element ) {
			switch( element.nodeName.toLowerCase() ) {
			case "select":
				return $("option:selected", element).length;
			case "input":
				if ( this.checkable( element) ) {
					return this.findByName(element.name).filter(":checked").length;
				}
			}
			return value.length;
		},

		depend: function( param, element ) {
			return this.dependTypes[typeof param] ? this.dependTypes[typeof param](param, element) : true;
		},

		dependTypes: {
			"boolean": function( param, element ) {
				return param;
			},
			"string": function( param, element ) {
				return !!$(param, element.form).length;
			},
			"function": function( param, element ) {
				return param(element);
			}
		},

		optional: function( element ) {
			var val = this.elementValue(element);
			return !$.validator.methods.required.call(this, val, element) && "dependency-mismatch";
		},

		startRequest: function( element ) {
			if ( !this.pending[element.name] ) {
				this.pendingRequest++;
				this.pending[element.name] = true;
			}
		},

		stopRequest: function( element, valid ) {
			this.pendingRequest--;
			// sometimes synchronization fails, make sure pendingRequest is never < 0
			if ( this.pendingRequest < 0 ) {
				this.pendingRequest = 0;
			}
			delete this.pending[element.name];
			if ( valid && this.pendingRequest === 0 && this.formSubmitted && this.form() ) {
				$(this.currentForm).submit();
				this.formSubmitted = false;
			} else if (!valid && this.pendingRequest === 0 && this.formSubmitted) {
				$(this.currentForm).triggerHandler("invalid-form", [this]);
				this.formSubmitted = false;
			}
		},

		previousValue: function( element ) {
			return $.data(element, "previousValue") || $.data(element, "previousValue", {
				old: null,
				valid: true,
				message: this.defaultMessage( element, "remote" )
			});
		}

	},

	classRuleSettings: {
		required: {required: true},
		email: {email: true},
		url: {url: true},
		date: {date: true},
		dateISO: {dateISO: true},
		number: {number: true},
		digits: {digits: true},
		creditcard: {creditcard: true}
	},

	addClassRules: function( className, rules ) {
		if ( className.constructor === String ) {
			this.classRuleSettings[className] = rules;
		} else {
			$.extend(this.classRuleSettings, className);
		}
	},

	classRules: function( element ) {
		var rules = {};
		var classes = $(element).attr("class");
		if ( classes ) {
			$.each(classes.split(" "), function() {
				if ( this in $.validator.classRuleSettings ) {
					$.extend(rules, $.validator.classRuleSettings[this]);
				}
			});
		}
		return rules;
	},

	attributeRules: function( element ) {
		var rules = {};
		var $element = $(element);
		var type = $element[0].getAttribute("type");

		for (var method in $.validator.methods) {
			var value;

			// support for <input required> in both html5 and older browsers
			if ( method === "required" ) {
				value = $element.get(0).getAttribute(method);
				// Some browsers return an empty string for the required attribute
				// and non-HTML5 browsers might have required="" markup
				if ( value === "" ) {
					value = true;
				}
				// force non-HTML5 browsers to return bool
				value = !!value;
			} else {
				value = $element.attr(method);
			}

			// convert the value to a number for number inputs, and for text for backwards compability
			// allows type="date" and others to be compared as strings
			if ( /min|max/.test( method ) && ( type === null || /number|range|text/.test( type ) ) ) {
				value = Number(value);
			}

			if ( value ) {
				rules[method] = value;
			} else if ( type === method && type !== 'range' ) {
				// exception: the jquery validate 'range' method
				// does not test for the html5 'range' type
				rules[method] = true;
			}
		}

		// maxlength may be returned as -1, 2147483647 (IE) and 524288 (safari) for text inputs
		if ( rules.maxlength && /-1|2147483647|524288/.test(rules.maxlength) ) {
			delete rules.maxlength;
		}

		return rules;
	},

	dataRules: function( element ) {
		var method, value,
			rules = {}, $element = $(element);
		for (method in $.validator.methods) {
			value = $element.data("rule-" + method.toLowerCase());
			if ( value !== undefined ) {
				rules[method] = value;
			}
		}
		return rules;
	},

	staticRules: function( element ) {
		var rules = {};
		var validator = $.data(element.form, "validator");
		if ( validator.settings.rules ) {
			rules = $.validator.normalizeRule(validator.settings.rules[element.name]) || {};
		}
		return rules;
	},

	normalizeRules: function( rules, element ) {
		// handle dependency check
		$.each(rules, function( prop, val ) {
			// ignore rule when param is explicitly false, eg. required:false
			if ( val === false ) {
				delete rules[prop];
				return;
			}
			if ( val.param || val.depends ) {
				var keepRule = true;
				switch (typeof val.depends) {
				case "string":
					keepRule = !!$(val.depends, element.form).length;
					break;
				case "function":
					keepRule = val.depends.call(element, element);
					break;
				}
				if ( keepRule ) {
					rules[prop] = val.param !== undefined ? val.param : true;
				} else {
					delete rules[prop];
				}
			}
		});

		// evaluate parameters
		$.each(rules, function( rule, parameter ) {
			rules[rule] = $.isFunction(parameter) ? parameter(element) : parameter;
		});

		// clean number parameters
		$.each(['minlength', 'maxlength'], function() {
			if ( rules[this] ) {
				rules[this] = Number(rules[this]);
			}
		});
		$.each(['rangelength', 'range'], function() {
			var parts;
			if ( rules[this] ) {
				if ( $.isArray(rules[this]) ) {
					rules[this] = [Number(rules[this][0]), Number(rules[this][1])];
				} else if ( typeof rules[this] === "string" ) {
					parts = rules[this].split(/[\s,]+/);
					rules[this] = [Number(parts[0]), Number(parts[1])];
				}
			}
		});

		if ( $.validator.autoCreateRanges ) {
			// auto-create ranges
			if ( rules.min && rules.max ) {
				rules.range = [rules.min, rules.max];
				delete rules.min;
				delete rules.max;
			}
			if ( rules.minlength && rules.maxlength ) {
				rules.rangelength = [rules.minlength, rules.maxlength];
				delete rules.minlength;
				delete rules.maxlength;
			}
		}

		return rules;
	},

	// Converts a simple string to a {string: true} rule, e.g., "required" to {required:true}
	normalizeRule: function( data ) {
		if ( typeof data === "string" ) {
			var transformed = {};
			$.each(data.split(/\s/), function() {
				transformed[this] = true;
			});
			data = transformed;
		}
		return data;
	},

	// http://docs.jquery.com/Plugins/Validation/Validator/addMethod
	addMethod: function( name, method, message ) {
		$.validator.methods[name] = method;
		$.validator.messages[name] = message !== undefined ? message : $.validator.messages[name];
		if ( method.length < 3 ) {
			$.validator.addClassRules(name, $.validator.normalizeRule(name));
		}
	},

	methods: {

		// http://docs.jquery.com/Plugins/Validation/Methods/required
		required: function( value, element, param ) {
			// check if dependency is met
			if ( !this.depend(param, element) ) {
				return "dependency-mismatch";
			}
			if ( element.nodeName.toLowerCase() === "select" ) {
				// could be an array for select-multiple or a string, both are fine this way
				var val = $(element).val();
				return val && val.length > 0;
			}
			if ( this.checkable(element) ) {
				return this.getLength(value, element) > 0;
			}
			return $.trim(value).length > 0;
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/email
		email: function( value, element ) {
			// contributed by Scott Gonzalez: http://projects.scottsplayground.com/email_address_validation/
			return this.optional(element) || /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$/i.test(value);
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/url
		url: function( value, element ) {
			// contributed by Scott Gonzalez: http://projects.scottsplayground.com/iri/
			return this.optional(element) || /^(https?|s?ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i.test(value);
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/date
		date: function( value, element ) {
			return this.optional(element) || !/Invalid|NaN/.test(new Date(value).toString());
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/dateISO
		dateISO: function( value, element ) {
			return this.optional(element) || /^\d{4}[\/\-]\d{1,2}[\/\-]\d{1,2}$/.test(value);
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/number
		number: function( value, element ) {
			return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:,\d{3})+)?(?:\.\d+)?$/.test(value);
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/digits
		digits: function( value, element ) {
			return this.optional(element) || /^\d+$/.test(value);
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/creditcard
		// based on http://en.wikipedia.org/wiki/Luhn
		creditcard: function( value, element ) {
			if ( this.optional(element) ) {
				return "dependency-mismatch";
			}
			// accept only spaces, digits and dashes
			if ( /[^0-9 \-]+/.test(value) ) {
				return false;
			}
			var nCheck = 0,
				nDigit = 0,
				bEven = false;

			value = value.replace(/\D/g, "");

			for (var n = value.length - 1; n >= 0; n--) {
				var cDigit = value.charAt(n);
				nDigit = parseInt(cDigit, 10);
				if ( bEven ) {
					if ( (nDigit *= 2) > 9 ) {
						nDigit -= 9;
					}
				}
				nCheck += nDigit;
				bEven = !bEven;
			}

			return (nCheck % 10) === 0;
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/minlength
		minlength: function( value, element, param ) {
			var length = $.isArray( value ) ? value.length : this.getLength($.trim(value), element);
			return this.optional(element) || length >= param;
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/maxlength
		maxlength: function( value, element, param ) {
			var length = $.isArray( value ) ? value.length : this.getLength($.trim(value), element);
			return this.optional(element) || length <= param;
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/rangelength
		rangelength: function( value, element, param ) {
			var length = $.isArray( value ) ? value.length : this.getLength($.trim(value), element);
			return this.optional(element) || ( length >= param[0] && length <= param[1] );
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/min
		min: function( value, element, param ) {
			return this.optional(element) || value >= param;
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/max
		max: function( value, element, param ) {
			return this.optional(element) || value <= param;
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/range
		range: function( value, element, param ) {
			return this.optional(element) || ( value >= param[0] && value <= param[1] );
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/equalTo
		equalTo: function( value, element, param ) {
			// bind to the blur event of the target in order to revalidate whenever the target field is updated
			// TODO find a way to bind the event just once, avoiding the unbind-rebind overhead
			var target = $(param);
			if ( this.settings.onfocusout ) {
				target.unbind(".validate-equalTo").bind("blur.validate-equalTo", function() {
					$(element).valid();
				});
			}
			return value === target.val();
		},

		// http://docs.jquery.com/Plugins/Validation/Methods/remote
		remote: function( value, element, param ) {
			if ( this.optional(element) ) {
				return "dependency-mismatch";
			}

			var previous = this.previousValue(element);
			if (!this.settings.messages[element.name] ) {
				this.settings.messages[element.name] = {};
			}
			previous.originalMessage = this.settings.messages[element.name].remote;
			this.settings.messages[element.name].remote = previous.message;

			param = typeof param === "string" && {url:param} || param;

			if ( previous.old === value ) {
				return previous.valid;
			}

			previous.old = value;
			var validator = this;
			this.startRequest(element);
			var data = {};
			data[element.name] = value;
			$.ajax($.extend(true, {
				url: param,
				mode: "abort",
				port: "validate" + element.name,
				dataType: "json",
				data: data,
				success: function( response ) {
					validator.settings.messages[element.name].remote = previous.originalMessage;
					var valid = response === true || response === "true";
					if ( valid ) {
						var submitted = validator.formSubmitted;
						validator.prepareElement(element);
						validator.formSubmitted = submitted;
						validator.successList.push(element);
						delete validator.invalid[element.name];
						validator.showErrors();
					} else {
						var errors = {};
						var message = response || validator.defaultMessage( element, "remote" );
						errors[element.name] = previous.message = $.isFunction(message) ? message(value) : message;
						validator.invalid[element.name] = true;
						validator.showErrors(errors);
					}
					previous.valid = valid;
					validator.stopRequest(element, valid);
				}
			}, param));
			return "pending";
		}

	}

});

// deprecated, use $.validator.format instead
$.format = $.validator.format;

}(jQuery));

// ajax mode: abort
// usage: $.ajax({ mode: "abort"[, port: "uniqueport"]});
// if mode:"abort" is used, the previous request on that port (port can be undefined) is aborted via XMLHttpRequest.abort()
(function($) {
	var pendingRequests = {};
	// Use a prefilter if available (1.5+)
	if ( $.ajaxPrefilter ) {
		$.ajaxPrefilter(function( settings, _, xhr ) {
			var port = settings.port;
			if ( settings.mode === "abort" ) {
				if ( pendingRequests[port] ) {
					pendingRequests[port].abort();
				}
				pendingRequests[port] = xhr;
			}
		});
	} else {
		// Proxy ajax
		var ajax = $.ajax;
		$.ajax = function( settings ) {
			var mode = ( "mode" in settings ? settings : $.ajaxSettings ).mode,
				port = ( "port" in settings ? settings : $.ajaxSettings ).port;
			if ( mode === "abort" ) {
				if ( pendingRequests[port] ) {
					pendingRequests[port].abort();
				}
				pendingRequests[port] = ajax.apply(this, arguments);
				return pendingRequests[port];
			}
			return ajax.apply(this, arguments);
		};
	}
}(jQuery));

// provides delegate(type: String, delegate: Selector, handler: Callback) plugin for easier event delegation
// handler is only called when $(event.target).is(delegate), in the scope of the jquery-object for event.target
(function($) {
	$.extend($.fn, {
		validateDelegate: function( delegate, type, handler ) {
			return this.bind(type, function( event ) {
				var target = $(event.target);
				if ( target.is(delegate) ) {
					return handler.apply(target, arguments);
				}
			});
		}
	});
}(jQuery));

/* NUGET: BEGIN LICENSE TEXT
 *
 * Microsoft grants you the right to use these script files for the sole
 * purpose of either: (i) interacting through your browser with the Microsoft
 * website or online service, subject to the applicable licensing or use
 * terms; or (ii) using the files as included with a Microsoft product subject
 * to that product's license terms. Microsoft reserves all other rights to the
 * files not expressly granted by Microsoft, whether by implication, estoppel
 * or otherwise. Insofar as a script file is dual licensed under GPL,
 * Microsoft neither took the code under GPL nor distributes it thereunder but
 * under the terms set out in this paragraph. All notices and licenses
 * below are for informational purposes only.
 *
 * NUGET: END LICENSE TEXT */
/*!
** Unobtrusive validation support library for jQuery and jQuery Validate
** Copyright (C) Microsoft Corporation. All rights reserved.
*/

/*jslint white: true, browser: true, onevar: true, undef: true, nomen: true, eqeqeq: true, plusplus: true, bitwise: true, regexp: true, newcap: true, immed: true, strict: false */
/*global document: false, jQuery: false */

(function ($) {
    var $jQval = $.validator,
        adapters,
        data_validation = "unobtrusiveValidation";

    function setValidationValues(options, ruleName, value) {
        options.rules[ruleName] = value;
        if (options.message) {
            options.messages[ruleName] = options.message;
        }
    }

    function splitAndTrim(value) {
        return value.replace(/^\s+|\s+$/g, "").split(/\s*,\s*/g);
    }

    function escapeAttributeValue(value) {
        // As mentioned on http://api.jquery.com/category/selectors/
        return value.replace(/([!"#$%&'()*+,./:;<=>?@\[\\\]^`{|}~])/g, "\\$1");
    }

    function getModelPrefix(fieldName) {
        return fieldName.substr(0, fieldName.lastIndexOf(".") + 1);
    }

    function appendModelPrefix(value, prefix) {
        if (value.indexOf("*.") === 0) {
            value = value.replace("*.", prefix);
        }
        return value;
    }

    function onError(error, inputElement) {  // 'this' is the form element
        var container = $(this).find("[data-valmsg-for='" + escapeAttributeValue(inputElement[0].name) + "']"),
            replaceAttrValue = container.attr("data-valmsg-replace"),
            replace = replaceAttrValue ? $.parseJSON(replaceAttrValue) !== false : null;

        container.removeClass("field-validation-valid").addClass("field-validation-error");
        error.data("unobtrusiveContainer", container);

        if (replace) {
            container.empty();
            error.removeClass("input-validation-error").appendTo(container);
        }
        else {
            error.hide();
        }
    }

    function onErrors(event, validator) {  // 'this' is the form element
        var container = $(this).find("[data-valmsg-summary=true]"),
            list = container.find("ul");

        if (list && list.length && validator.errorList.length) {
            list.empty();
            container.addClass("validation-summary-errors").removeClass("validation-summary-valid");

            $.each(validator.errorList, function () {
                $("<li />").html(this.message).appendTo(list);
            });
        }
    }

    function onSuccess(error) {  // 'this' is the form element
        var container = error.data("unobtrusiveContainer"),
            replaceAttrValue = container.attr("data-valmsg-replace"),
            replace = replaceAttrValue ? $.parseJSON(replaceAttrValue) : null;

        if (container) {
            container.addClass("field-validation-valid").removeClass("field-validation-error");
            error.removeData("unobtrusiveContainer");

            if (replace) {
                container.empty();
            }
        }
    }

    function onReset(event) {  // 'this' is the form element
        var $form = $(this);
        $form.data("validator").resetForm();
        $form.find(".validation-summary-errors")
            .addClass("validation-summary-valid")
            .removeClass("validation-summary-errors");
        $form.find(".field-validation-error")
            .addClass("field-validation-valid")
            .removeClass("field-validation-error")
            .removeData("unobtrusiveContainer")
            .find(">*")  // If we were using valmsg-replace, get the underlying error
                .removeData("unobtrusiveContainer");
    }

    function validationInfo(form) {
        var $form = $(form),
            result = $form.data(data_validation),
            onResetProxy = $.proxy(onReset, form),
            defaultOptions = $jQval.unobtrusive.options || {},
            execInContext = function (name, args) {
                var func = defaultOptions[name];
                func && $.isFunction(func) && func.apply(form, args);
            }

        if (!result) {
            result = {
                options: {  // options structure passed to jQuery Validate's validate() method
                    errorClass: defaultOptions.errorClass || "input-validation-error",
                    errorElement: defaultOptions.errorElement || "span",
                    errorPlacement: function () {
                        onError.apply(form, arguments);
                        execInContext("errorPlacement", arguments);
                    },
                    invalidHandler: function () {
                        onErrors.apply(form, arguments);
                        execInContext("invalidHandler", arguments);
                    },
                    messages: {},
                    rules: {},
                    success: function () {
                        onSuccess.apply(form, arguments);
                        execInContext("success", arguments);
                    }
                },
                attachValidation: function () {
                    $form
                        .off("reset." + data_validation, onResetProxy)
                        .on("reset." + data_validation, onResetProxy)
                        .validate(this.options);
                },
                validate: function () {  // a validation function that is called by unobtrusive Ajax
                    $form.validate();
                    return $form.valid();
                }
            };
            $form.data(data_validation, result);
        }

        return result;
    }

    $jQval.unobtrusive = {
        adapters: [],

        parseElement: function (element, skipAttach) {
            /// <summary>
            /// Parses a single HTML element for unobtrusive validation attributes.
            /// </summary>
            /// <param name="element" domElement="true">The HTML element to be parsed.</param>
            /// <param name="skipAttach" type="Boolean">[Optional] true to skip attaching the
            /// validation to the form. If parsing just this single element, you should specify true.
            /// If parsing several elements, you should specify false, and manually attach the validation
            /// to the form when you are finished. The default is false.</param>
            var $element = $(element),
                form = $element.parents("form")[0],
                valInfo, rules, messages;

            if (!form) {  // Cannot do client-side validation without a form
                return;
            }

            valInfo = validationInfo(form);
            valInfo.options.rules[element.name] = rules = {};
            valInfo.options.messages[element.name] = messages = {};

            $.each(this.adapters, function () {
                var prefix = "data-val-" + this.name,
                    message = $element.attr(prefix),
                    paramValues = {};

                if (message !== undefined) {  // Compare against undefined, because an empty message is legal (and falsy)
                    prefix += "-";

                    $.each(this.params, function () {
                        paramValues[this] = $element.attr(prefix + this);
                    });

                    this.adapt({
                        element: element,
                        form: form,
                        message: message,
                        params: paramValues,
                        rules: rules,
                        messages: messages
                    });
                }
            });

            $.extend(rules, { "__dummy__": true });

            if (!skipAttach) {
                valInfo.attachValidation();
            }
        },

        parse: function (selector) {
            /// <summary>
            /// Parses all the HTML elements in the specified selector. It looks for input elements decorated
            /// with the [data-val=true] attribute value and enables validation according to the data-val-*
            /// attribute values.
            /// </summary>
            /// <param name="selector" type="String">Any valid jQuery selector.</param>

            // $forms includes all forms in selector's DOM hierarchy (parent, children and self) that have at least one
            // element with data-val=true
            var $selector = $(selector),
                $forms = $selector.parents()
                                  .addBack()
                                  .filter("form")
                                  .add($selector.find("form"))
                                  .has("[data-val=true]");

            $selector.find("[data-val=true]").each(function () {
                $jQval.unobtrusive.parseElement(this, true);
            });

            $forms.each(function () {
                var info = validationInfo(this);
                if (info) {
                    info.attachValidation();
                }
            });
        }
    };

    adapters = $jQval.unobtrusive.adapters;

    adapters.add = function (adapterName, params, fn) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute (where nnnn is the adapter name).</param>
        /// <param name="params" type="Array" optional="true">[Optional] An array of parameter names (strings) that will
        /// be extracted from the data-val-nnnn-mmmm HTML attributes (where nnnn is the adapter name, and
        /// mmmm is the parameter name).</param>
        /// <param name="fn" type="Function">The function to call, which adapts the values from the HTML
        /// attributes into jQuery Validate rules and/or messages.</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        if (!fn) {  // Called with no params, just a function
            fn = params;
            params = [];
        }
        this.push({ name: adapterName, params: params, adapt: fn });
        return this;
    };

    adapters.addBool = function (adapterName, ruleName) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation, where
        /// the jQuery Validate validation rule has no parameter values.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute (where nnnn is the adapter name).</param>
        /// <param name="ruleName" type="String" optional="true">[Optional] The name of the jQuery Validate rule. If not provided, the value
        /// of adapterName will be used instead.</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        return this.add(adapterName, function (options) {
            setValidationValues(options, ruleName || adapterName, true);
        });
    };

    adapters.addMinMax = function (adapterName, minRuleName, maxRuleName, minMaxRuleName, minAttribute, maxAttribute) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation, where
        /// the jQuery Validate validation has three potential rules (one for min-only, one for max-only, and
        /// one for min-and-max). The HTML parameters are expected to be named -min and -max.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute (where nnnn is the adapter name).</param>
        /// <param name="minRuleName" type="String">The name of the jQuery Validate rule to be used when you only
        /// have a minimum value.</param>
        /// <param name="maxRuleName" type="String">The name of the jQuery Validate rule to be used when you only
        /// have a maximum value.</param>
        /// <param name="minMaxRuleName" type="String">The name of the jQuery Validate rule to be used when you
        /// have both a minimum and maximum value.</param>
        /// <param name="minAttribute" type="String" optional="true">[Optional] The name of the HTML attribute that
        /// contains the minimum value. The default is "min".</param>
        /// <param name="maxAttribute" type="String" optional="true">[Optional] The name of the HTML attribute that
        /// contains the maximum value. The default is "max".</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        return this.add(adapterName, [minAttribute || "min", maxAttribute || "max"], function (options) {
            var min = options.params.min,
                max = options.params.max;

            if (min && max) {
                setValidationValues(options, minMaxRuleName, [min, max]);
            }
            else if (min) {
                setValidationValues(options, minRuleName, min);
            }
            else if (max) {
                setValidationValues(options, maxRuleName, max);
            }
        });
    };

    adapters.addSingleVal = function (adapterName, attribute, ruleName) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation, where
        /// the jQuery Validate validation rule has a single value.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute(where nnnn is the adapter name).</param>
        /// <param name="attribute" type="String">[Optional] The name of the HTML attribute that contains the value.
        /// The default is "val".</param>
        /// <param name="ruleName" type="String" optional="true">[Optional] The name of the jQuery Validate rule. If not provided, the value
        /// of adapterName will be used instead.</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        return this.add(adapterName, [attribute || "val"], function (options) {
            setValidationValues(options, ruleName || adapterName, options.params[attribute]);
        });
    };

    $jQval.addMethod("__dummy__", function (value, element, params) {
        return true;
    });

    $jQval.addMethod("regex", function (value, element, params) {
        var match;
        if (this.optional(element)) {
            return true;
        }

        match = new RegExp(params).exec(value);
        return (match && (match.index === 0) && (match[0].length === value.length));
    });

    $jQval.addMethod("nonalphamin", function (value, element, nonalphamin) {
        var match;
        if (nonalphamin) {
            match = value.match(/\W/g);
            match = match && match.length >= nonalphamin;
        }
        return match;
    });

    if ($jQval.methods.extension) {
        adapters.addSingleVal("accept", "mimtype");
        adapters.addSingleVal("extension", "extension");
    } else {
        // for backward compatibility, when the 'extension' validation method does not exist, such as with versions
        // of JQuery Validation plugin prior to 1.10, we should use the 'accept' method for
        // validating the extension, and ignore mime-type validations as they are not supported.
        adapters.addSingleVal("extension", "extension", "accept");
    }

    adapters.addSingleVal("regex", "pattern");
    adapters.addBool("creditcard").addBool("date").addBool("digits").addBool("email").addBool("number").addBool("url");
    adapters.addMinMax("length", "minlength", "maxlength", "rangelength").addMinMax("range", "min", "max", "range");
    adapters.addMinMax("minlength", "minlength").addMinMax("maxlength", "minlength", "maxlength");
    adapters.add("equalto", ["other"], function (options) {
        var prefix = getModelPrefix(options.element.name),
            other = options.params.other,
            fullOtherName = appendModelPrefix(other, prefix),
            element = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(fullOtherName) + "']")[0];

        setValidationValues(options, "equalTo", element);
    });
    adapters.add("required", function (options) {
        // jQuery Validate equates "required" with "mandatory" for checkbox elements
        if (options.element.tagName.toUpperCase() !== "INPUT" || options.element.type.toUpperCase() !== "CHECKBOX") {
            setValidationValues(options, "required", true);
        }
    });
    adapters.add("remote", ["url", "type", "additionalfields"], function (options) {
        var value = {
            url: options.params.url,
            type: options.params.type || "GET",
            data: {}
        },
            prefix = getModelPrefix(options.element.name);

        $.each(splitAndTrim(options.params.additionalfields || options.element.name), function (i, fieldName) {
            var paramName = appendModelPrefix(fieldName, prefix);
            value.data[paramName] = function () {
                return $(options.form).find(":input").filter("[name='" + escapeAttributeValue(paramName) + "']").val();
            };
        });

        setValidationValues(options, "remote", value);
    });
    adapters.add("password", ["min", "nonalphamin", "regex"], function (options) {
        if (options.params.min) {
            setValidationValues(options, "minlength", options.params.min);
        }
        if (options.params.nonalphamin) {
            setValidationValues(options, "nonalphamin", options.params.nonalphamin);
        }
        if (options.params.regex) {
            setValidationValues(options, "regex", options.params.regex);
        }
    });

    $(function () {
        $jQval.unobtrusive.parse(document);
    });
}(jQuery));
(function ($) {

    $.validator.addMethod("optionrequired", function (value, element, param) {
        var isValid = true;

        if ($(element).is("input")) {
            var parent = $(element).closest("ol");

            isValid = parent.find("input:checked").length > 0;
            parent.toggleClass("input-validation-error", !isValid);
        }
        else if ($(element).is("select")) {
            var v = $(element).val();
            isValid = !!v && v.length > 0;
        }

        return isValid;
    }, "An option is required");

    $.validator.unobtrusive.adapters.addBool("mandatory", "required");
    $.validator.unobtrusive.adapters.addBool("optionrequired");
}(jQuery));
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImpxdWVyeS52YWxpZGF0ZS5qcyIsImpxdWVyeS52YWxpZGF0ZS51bm9idHJ1c2l2ZS5qcyIsImpxdWVyeS52YWxpZGF0ZS51bm9idHJ1c2l2ZS5hZGRpdGlvbmFsLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDN3RDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDelpBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6IkxpYi5qcyIsInNvdXJjZXNDb250ZW50IjpbIi8qIE5VR0VUOiBCRUdJTiBMSUNFTlNFIFRFWFRcclxuICpcclxuICogTWljcm9zb2Z0IGdyYW50cyB5b3UgdGhlIHJpZ2h0IHRvIHVzZSB0aGVzZSBzY3JpcHQgZmlsZXMgZm9yIHRoZSBzb2xlXHJcbiAqIHB1cnBvc2Ugb2YgZWl0aGVyOiAoaSkgaW50ZXJhY3RpbmcgdGhyb3VnaCB5b3VyIGJyb3dzZXIgd2l0aCB0aGUgTWljcm9zb2Z0XHJcbiAqIHdlYnNpdGUgb3Igb25saW5lIHNlcnZpY2UsIHN1YmplY3QgdG8gdGhlIGFwcGxpY2FibGUgbGljZW5zaW5nIG9yIHVzZVxyXG4gKiB0ZXJtczsgb3IgKGlpKSB1c2luZyB0aGUgZmlsZXMgYXMgaW5jbHVkZWQgd2l0aCBhIE1pY3Jvc29mdCBwcm9kdWN0IHN1YmplY3RcclxuICogdG8gdGhhdCBwcm9kdWN0J3MgbGljZW5zZSB0ZXJtcy4gTWljcm9zb2Z0IHJlc2VydmVzIGFsbCBvdGhlciByaWdodHMgdG8gdGhlXHJcbiAqIGZpbGVzIG5vdCBleHByZXNzbHkgZ3JhbnRlZCBieSBNaWNyb3NvZnQsIHdoZXRoZXIgYnkgaW1wbGljYXRpb24sIGVzdG9wcGVsXHJcbiAqIG9yIG90aGVyd2lzZS4gSW5zb2ZhciBhcyBhIHNjcmlwdCBmaWxlIGlzIGR1YWwgbGljZW5zZWQgdW5kZXIgR1BMLFxyXG4gKiBNaWNyb3NvZnQgbmVpdGhlciB0b29rIHRoZSBjb2RlIHVuZGVyIEdQTCBub3IgZGlzdHJpYnV0ZXMgaXQgdGhlcmV1bmRlciBidXRcclxuICogdW5kZXIgdGhlIHRlcm1zIHNldCBvdXQgaW4gdGhpcyBwYXJhZ3JhcGguIEFsbCBub3RpY2VzIGFuZCBsaWNlbnNlc1xyXG4gKiBiZWxvdyBhcmUgZm9yIGluZm9ybWF0aW9uYWwgcHVycG9zZXMgb25seS5cclxuICpcclxuICogTlVHRVQ6IEVORCBMSUNFTlNFIFRFWFQgKi9cclxuLyohXHJcbiAqIGpRdWVyeSBWYWxpZGF0aW9uIFBsdWdpbiAxLjExLjFcclxuICpcclxuICogaHR0cDovL2Jhc3Npc3RhbmNlLmRlL2pxdWVyeS1wbHVnaW5zL2pxdWVyeS1wbHVnaW4tdmFsaWRhdGlvbi9cclxuICogaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb25cclxuICpcclxuICogQ29weXJpZ2h0IDIwMTMgSsO2cm4gWmFlZmZlcmVyXHJcbiAqIFJlbGVhc2VkIHVuZGVyIHRoZSBNSVQgbGljZW5zZTpcclxuICogICBodHRwOi8vd3d3Lm9wZW5zb3VyY2Uub3JnL2xpY2Vuc2VzL21pdC1saWNlbnNlLnBocFxyXG4gKi9cclxuXHJcbihmdW5jdGlvbigkKSB7XHJcblxyXG4kLmV4dGVuZCgkLmZuLCB7XHJcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vdmFsaWRhdGVcclxuXHR2YWxpZGF0ZTogZnVuY3Rpb24oIG9wdGlvbnMgKSB7XHJcblxyXG5cdFx0Ly8gaWYgbm90aGluZyBpcyBzZWxlY3RlZCwgcmV0dXJuIG5vdGhpbmc7IGNhbid0IGNoYWluIGFueXdheVxyXG5cdFx0aWYgKCAhdGhpcy5sZW5ndGggKSB7XHJcblx0XHRcdGlmICggb3B0aW9ucyAmJiBvcHRpb25zLmRlYnVnICYmIHdpbmRvdy5jb25zb2xlICkge1xyXG5cdFx0XHRcdGNvbnNvbGUud2FybiggXCJOb3RoaW5nIHNlbGVjdGVkLCBjYW4ndCB2YWxpZGF0ZSwgcmV0dXJuaW5nIG5vdGhpbmcuXCIgKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm47XHJcblx0XHR9XHJcblxyXG5cdFx0Ly8gY2hlY2sgaWYgYSB2YWxpZGF0b3IgZm9yIHRoaXMgZm9ybSB3YXMgYWxyZWFkeSBjcmVhdGVkXHJcblx0XHR2YXIgdmFsaWRhdG9yID0gJC5kYXRhKCB0aGlzWzBdLCBcInZhbGlkYXRvclwiICk7XHJcblx0XHRpZiAoIHZhbGlkYXRvciApIHtcclxuXHRcdFx0cmV0dXJuIHZhbGlkYXRvcjtcclxuXHRcdH1cclxuXHJcblx0XHQvLyBBZGQgbm92YWxpZGF0ZSB0YWcgaWYgSFRNTDUuXHJcblx0XHR0aGlzLmF0dHIoIFwibm92YWxpZGF0ZVwiLCBcIm5vdmFsaWRhdGVcIiApO1xyXG5cclxuXHRcdHZhbGlkYXRvciA9IG5ldyAkLnZhbGlkYXRvciggb3B0aW9ucywgdGhpc1swXSApO1xyXG5cdFx0JC5kYXRhKCB0aGlzWzBdLCBcInZhbGlkYXRvclwiLCB2YWxpZGF0b3IgKTtcclxuXHJcblx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5ncy5vbnN1Ym1pdCApIHtcclxuXHJcblx0XHRcdHRoaXMudmFsaWRhdGVEZWxlZ2F0ZSggXCI6c3VibWl0XCIsIFwiY2xpY2tcIiwgZnVuY3Rpb24oIGV2ZW50ICkge1xyXG5cdFx0XHRcdGlmICggdmFsaWRhdG9yLnNldHRpbmdzLnN1Ym1pdEhhbmRsZXIgKSB7XHJcblx0XHRcdFx0XHR2YWxpZGF0b3Iuc3VibWl0QnV0dG9uID0gZXZlbnQudGFyZ2V0O1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHQvLyBhbGxvdyBzdXBwcmVzc2luZyB2YWxpZGF0aW9uIGJ5IGFkZGluZyBhIGNhbmNlbCBjbGFzcyB0byB0aGUgc3VibWl0IGJ1dHRvblxyXG5cdFx0XHRcdGlmICggJChldmVudC50YXJnZXQpLmhhc0NsYXNzKFwiY2FuY2VsXCIpICkge1xyXG5cdFx0XHRcdFx0dmFsaWRhdG9yLmNhbmNlbFN1Ym1pdCA9IHRydWU7XHJcblx0XHRcdFx0fVxyXG5cclxuXHRcdFx0XHQvLyBhbGxvdyBzdXBwcmVzc2luZyB2YWxpZGF0aW9uIGJ5IGFkZGluZyB0aGUgaHRtbDUgZm9ybW5vdmFsaWRhdGUgYXR0cmlidXRlIHRvIHRoZSBzdWJtaXQgYnV0dG9uXHJcblx0XHRcdFx0aWYgKCAkKGV2ZW50LnRhcmdldCkuYXR0cihcImZvcm1ub3ZhbGlkYXRlXCIpICE9PSB1bmRlZmluZWQgKSB7XHJcblx0XHRcdFx0XHR2YWxpZGF0b3IuY2FuY2VsU3VibWl0ID0gdHJ1ZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cclxuXHRcdFx0Ly8gdmFsaWRhdGUgdGhlIGZvcm0gb24gc3VibWl0XHJcblx0XHRcdHRoaXMuc3VibWl0KCBmdW5jdGlvbiggZXZlbnQgKSB7XHJcblx0XHRcdFx0aWYgKCB2YWxpZGF0b3Iuc2V0dGluZ3MuZGVidWcgKSB7XHJcblx0XHRcdFx0XHQvLyBwcmV2ZW50IGZvcm0gc3VibWl0IHRvIGJlIGFibGUgdG8gc2VlIGNvbnNvbGUgb3V0cHV0XHJcblx0XHRcdFx0XHRldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRmdW5jdGlvbiBoYW5kbGUoKSB7XHJcblx0XHRcdFx0XHR2YXIgaGlkZGVuO1xyXG5cdFx0XHRcdFx0aWYgKCB2YWxpZGF0b3Iuc2V0dGluZ3Muc3VibWl0SGFuZGxlciApIHtcclxuXHRcdFx0XHRcdFx0aWYgKCB2YWxpZGF0b3Iuc3VibWl0QnV0dG9uICkge1xyXG5cdFx0XHRcdFx0XHRcdC8vIGluc2VydCBhIGhpZGRlbiBpbnB1dCBhcyBhIHJlcGxhY2VtZW50IGZvciB0aGUgbWlzc2luZyBzdWJtaXQgYnV0dG9uXHJcblx0XHRcdFx0XHRcdFx0aGlkZGVuID0gJChcIjxpbnB1dCB0eXBlPSdoaWRkZW4nLz5cIikuYXR0cihcIm5hbWVcIiwgdmFsaWRhdG9yLnN1Ym1pdEJ1dHRvbi5uYW1lKS52YWwoICQodmFsaWRhdG9yLnN1Ym1pdEJ1dHRvbikudmFsKCkgKS5hcHBlbmRUbyh2YWxpZGF0b3IuY3VycmVudEZvcm0pO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdHZhbGlkYXRvci5zZXR0aW5ncy5zdWJtaXRIYW5kbGVyLmNhbGwoIHZhbGlkYXRvciwgdmFsaWRhdG9yLmN1cnJlbnRGb3JtLCBldmVudCApO1xyXG5cdFx0XHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zdWJtaXRCdXR0b24gKSB7XHJcblx0XHRcdFx0XHRcdFx0Ly8gYW5kIGNsZWFuIHVwIGFmdGVyd2FyZHM7IHRoYW5rcyB0byBuby1ibG9jay1zY29wZSwgaGlkZGVuIGNhbiBiZSByZWZlcmVuY2VkXHJcblx0XHRcdFx0XHRcdFx0aGlkZGVuLnJlbW92ZSgpO1xyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0XHRcdH1cclxuXHJcblx0XHRcdFx0Ly8gcHJldmVudCBzdWJtaXQgZm9yIGludmFsaWQgZm9ybXMgb3IgY3VzdG9tIHN1Ym1pdCBoYW5kbGVyc1xyXG5cdFx0XHRcdGlmICggdmFsaWRhdG9yLmNhbmNlbFN1Ym1pdCApIHtcclxuXHRcdFx0XHRcdHZhbGlkYXRvci5jYW5jZWxTdWJtaXQgPSBmYWxzZTtcclxuXHRcdFx0XHRcdHJldHVybiBoYW5kbGUoKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYgKCB2YWxpZGF0b3IuZm9ybSgpICkge1xyXG5cdFx0XHRcdFx0aWYgKCB2YWxpZGF0b3IucGVuZGluZ1JlcXVlc3QgKSB7XHJcblx0XHRcdFx0XHRcdHZhbGlkYXRvci5mb3JtU3VibWl0dGVkID0gdHJ1ZTtcclxuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0cmV0dXJuIGhhbmRsZSgpO1xyXG5cdFx0XHRcdH0gZWxzZSB7XHJcblx0XHRcdFx0XHR2YWxpZGF0b3IuZm9jdXNJbnZhbGlkKCk7XHJcblx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9KTtcclxuXHRcdH1cclxuXHJcblx0XHRyZXR1cm4gdmFsaWRhdG9yO1xyXG5cdH0sXHJcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vdmFsaWRcclxuXHR2YWxpZDogZnVuY3Rpb24oKSB7XHJcblx0XHRpZiAoICQodGhpc1swXSkuaXMoXCJmb3JtXCIpKSB7XHJcblx0XHRcdHJldHVybiB0aGlzLnZhbGlkYXRlKCkuZm9ybSgpO1xyXG5cdFx0fSBlbHNlIHtcclxuXHRcdFx0dmFyIHZhbGlkID0gdHJ1ZTtcclxuXHRcdFx0dmFyIHZhbGlkYXRvciA9ICQodGhpc1swXS5mb3JtKS52YWxpZGF0ZSgpO1xyXG5cdFx0XHR0aGlzLmVhY2goZnVuY3Rpb24oKSB7XHJcblx0XHRcdFx0dmFsaWQgPSB2YWxpZCAmJiB2YWxpZGF0b3IuZWxlbWVudCh0aGlzKTtcclxuXHRcdFx0fSk7XHJcblx0XHRcdHJldHVybiB2YWxpZDtcclxuXHRcdH1cclxuXHR9LFxyXG5cdC8vIGF0dHJpYnV0ZXM6IHNwYWNlIHNlcGVyYXRlZCBsaXN0IG9mIGF0dHJpYnV0ZXMgdG8gcmV0cmlldmUgYW5kIHJlbW92ZVxyXG5cdHJlbW92ZUF0dHJzOiBmdW5jdGlvbiggYXR0cmlidXRlcyApIHtcclxuXHRcdHZhciByZXN1bHQgPSB7fSxcclxuXHRcdFx0JGVsZW1lbnQgPSB0aGlzO1xyXG5cdFx0JC5lYWNoKGF0dHJpYnV0ZXMuc3BsaXQoL1xccy8pLCBmdW5jdGlvbiggaW5kZXgsIHZhbHVlICkge1xyXG5cdFx0XHRyZXN1bHRbdmFsdWVdID0gJGVsZW1lbnQuYXR0cih2YWx1ZSk7XHJcblx0XHRcdCRlbGVtZW50LnJlbW92ZUF0dHIodmFsdWUpO1xyXG5cdFx0fSk7XHJcblx0XHRyZXR1cm4gcmVzdWx0O1xyXG5cdH0sXHJcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vcnVsZXNcclxuXHRydWxlczogZnVuY3Rpb24oIGNvbW1hbmQsIGFyZ3VtZW50ICkge1xyXG5cdFx0dmFyIGVsZW1lbnQgPSB0aGlzWzBdO1xyXG5cclxuXHRcdGlmICggY29tbWFuZCApIHtcclxuXHRcdFx0dmFyIHNldHRpbmdzID0gJC5kYXRhKGVsZW1lbnQuZm9ybSwgXCJ2YWxpZGF0b3JcIikuc2V0dGluZ3M7XHJcblx0XHRcdHZhciBzdGF0aWNSdWxlcyA9IHNldHRpbmdzLnJ1bGVzO1xyXG5cdFx0XHR2YXIgZXhpc3RpbmdSdWxlcyA9ICQudmFsaWRhdG9yLnN0YXRpY1J1bGVzKGVsZW1lbnQpO1xyXG5cdFx0XHRzd2l0Y2goY29tbWFuZCkge1xyXG5cdFx0XHRjYXNlIFwiYWRkXCI6XHJcblx0XHRcdFx0JC5leHRlbmQoZXhpc3RpbmdSdWxlcywgJC52YWxpZGF0b3Iubm9ybWFsaXplUnVsZShhcmd1bWVudCkpO1xyXG5cdFx0XHRcdC8vIHJlbW92ZSBtZXNzYWdlcyBmcm9tIHJ1bGVzLCBidXQgYWxsb3cgdGhlbSB0byBiZSBzZXQgc2VwYXJldGVseVxyXG5cdFx0XHRcdGRlbGV0ZSBleGlzdGluZ1J1bGVzLm1lc3NhZ2VzO1xyXG5cdFx0XHRcdHN0YXRpY1J1bGVzW2VsZW1lbnQubmFtZV0gPSBleGlzdGluZ1J1bGVzO1xyXG5cdFx0XHRcdGlmICggYXJndW1lbnQubWVzc2FnZXMgKSB7XHJcblx0XHRcdFx0XHRzZXR0aW5ncy5tZXNzYWdlc1tlbGVtZW50Lm5hbWVdID0gJC5leHRlbmQoIHNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0sIGFyZ3VtZW50Lm1lc3NhZ2VzICk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRjYXNlIFwicmVtb3ZlXCI6XHJcblx0XHRcdFx0aWYgKCAhYXJndW1lbnQgKSB7XHJcblx0XHRcdFx0XHRkZWxldGUgc3RhdGljUnVsZXNbZWxlbWVudC5uYW1lXTtcclxuXHRcdFx0XHRcdHJldHVybiBleGlzdGluZ1J1bGVzO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHR2YXIgZmlsdGVyZWQgPSB7fTtcclxuXHRcdFx0XHQkLmVhY2goYXJndW1lbnQuc3BsaXQoL1xccy8pLCBmdW5jdGlvbiggaW5kZXgsIG1ldGhvZCApIHtcclxuXHRcdFx0XHRcdGZpbHRlcmVkW21ldGhvZF0gPSBleGlzdGluZ1J1bGVzW21ldGhvZF07XHJcblx0XHRcdFx0XHRkZWxldGUgZXhpc3RpbmdSdWxlc1ttZXRob2RdO1xyXG5cdFx0XHRcdH0pO1xyXG5cdFx0XHRcdHJldHVybiBmaWx0ZXJlZDtcclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cclxuXHRcdHZhciBkYXRhID0gJC52YWxpZGF0b3Iubm9ybWFsaXplUnVsZXMoXHJcblx0XHQkLmV4dGVuZChcclxuXHRcdFx0e30sXHJcblx0XHRcdCQudmFsaWRhdG9yLmNsYXNzUnVsZXMoZWxlbWVudCksXHJcblx0XHRcdCQudmFsaWRhdG9yLmF0dHJpYnV0ZVJ1bGVzKGVsZW1lbnQpLFxyXG5cdFx0XHQkLnZhbGlkYXRvci5kYXRhUnVsZXMoZWxlbWVudCksXHJcblx0XHRcdCQudmFsaWRhdG9yLnN0YXRpY1J1bGVzKGVsZW1lbnQpXHJcblx0XHQpLCBlbGVtZW50KTtcclxuXHJcblx0XHQvLyBtYWtlIHN1cmUgcmVxdWlyZWQgaXMgYXQgZnJvbnRcclxuXHRcdGlmICggZGF0YS5yZXF1aXJlZCApIHtcclxuXHRcdFx0dmFyIHBhcmFtID0gZGF0YS5yZXF1aXJlZDtcclxuXHRcdFx0ZGVsZXRlIGRhdGEucmVxdWlyZWQ7XHJcblx0XHRcdGRhdGEgPSAkLmV4dGVuZCh7cmVxdWlyZWQ6IHBhcmFtfSwgZGF0YSk7XHJcblx0XHR9XHJcblxyXG5cdFx0cmV0dXJuIGRhdGE7XHJcblx0fVxyXG59KTtcclxuXHJcbi8vIEN1c3RvbSBzZWxlY3RvcnNcclxuJC5leHRlbmQoJC5leHByW1wiOlwiXSwge1xyXG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL2JsYW5rXHJcblx0Ymxhbms6IGZ1bmN0aW9uKCBhICkgeyByZXR1cm4gISQudHJpbShcIlwiICsgJChhKS52YWwoKSk7IH0sXHJcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vZmlsbGVkXHJcblx0ZmlsbGVkOiBmdW5jdGlvbiggYSApIHsgcmV0dXJuICEhJC50cmltKFwiXCIgKyAkKGEpLnZhbCgpKTsgfSxcclxuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi91bmNoZWNrZWRcclxuXHR1bmNoZWNrZWQ6IGZ1bmN0aW9uKCBhICkgeyByZXR1cm4gISQoYSkucHJvcChcImNoZWNrZWRcIik7IH1cclxufSk7XHJcblxyXG4vLyBjb25zdHJ1Y3RvciBmb3IgdmFsaWRhdG9yXHJcbiQudmFsaWRhdG9yID0gZnVuY3Rpb24oIG9wdGlvbnMsIGZvcm0gKSB7XHJcblx0dGhpcy5zZXR0aW5ncyA9ICQuZXh0ZW5kKCB0cnVlLCB7fSwgJC52YWxpZGF0b3IuZGVmYXVsdHMsIG9wdGlvbnMgKTtcclxuXHR0aGlzLmN1cnJlbnRGb3JtID0gZm9ybTtcclxuXHR0aGlzLmluaXQoKTtcclxufTtcclxuXHJcbiQudmFsaWRhdG9yLmZvcm1hdCA9IGZ1bmN0aW9uKCBzb3VyY2UsIHBhcmFtcyApIHtcclxuXHRpZiAoIGFyZ3VtZW50cy5sZW5ndGggPT09IDEgKSB7XHJcblx0XHRyZXR1cm4gZnVuY3Rpb24oKSB7XHJcblx0XHRcdHZhciBhcmdzID0gJC5tYWtlQXJyYXkoYXJndW1lbnRzKTtcclxuXHRcdFx0YXJncy51bnNoaWZ0KHNvdXJjZSk7XHJcblx0XHRcdHJldHVybiAkLnZhbGlkYXRvci5mb3JtYXQuYXBwbHkoIHRoaXMsIGFyZ3MgKTtcclxuXHRcdH07XHJcblx0fVxyXG5cdGlmICggYXJndW1lbnRzLmxlbmd0aCA+IDIgJiYgcGFyYW1zLmNvbnN0cnVjdG9yICE9PSBBcnJheSAgKSB7XHJcblx0XHRwYXJhbXMgPSAkLm1ha2VBcnJheShhcmd1bWVudHMpLnNsaWNlKDEpO1xyXG5cdH1cclxuXHRpZiAoIHBhcmFtcy5jb25zdHJ1Y3RvciAhPT0gQXJyYXkgKSB7XHJcblx0XHRwYXJhbXMgPSBbIHBhcmFtcyBdO1xyXG5cdH1cclxuXHQkLmVhY2gocGFyYW1zLCBmdW5jdGlvbiggaSwgbiApIHtcclxuXHRcdHNvdXJjZSA9IHNvdXJjZS5yZXBsYWNlKCBuZXcgUmVnRXhwKFwiXFxcXHtcIiArIGkgKyBcIlxcXFx9XCIsIFwiZ1wiKSwgZnVuY3Rpb24oKSB7XHJcblx0XHRcdHJldHVybiBuO1xyXG5cdFx0fSk7XHJcblx0fSk7XHJcblx0cmV0dXJuIHNvdXJjZTtcclxufTtcclxuXHJcbiQuZXh0ZW5kKCQudmFsaWRhdG9yLCB7XHJcblxyXG5cdGRlZmF1bHRzOiB7XHJcblx0XHRtZXNzYWdlczoge30sXHJcblx0XHRncm91cHM6IHt9LFxyXG5cdFx0cnVsZXM6IHt9LFxyXG5cdFx0ZXJyb3JDbGFzczogXCJlcnJvclwiLFxyXG5cdFx0dmFsaWRDbGFzczogXCJ2YWxpZFwiLFxyXG5cdFx0ZXJyb3JFbGVtZW50OiBcImxhYmVsXCIsXHJcblx0XHRmb2N1c0ludmFsaWQ6IHRydWUsXHJcblx0XHRlcnJvckNvbnRhaW5lcjogJChbXSksXHJcblx0XHRlcnJvckxhYmVsQ29udGFpbmVyOiAkKFtdKSxcclxuXHRcdG9uc3VibWl0OiB0cnVlLFxyXG5cdFx0aWdub3JlOiBcIjpoaWRkZW5cIixcclxuXHRcdGlnbm9yZVRpdGxlOiBmYWxzZSxcclxuXHRcdG9uZm9jdXNpbjogZnVuY3Rpb24oIGVsZW1lbnQsIGV2ZW50ICkge1xyXG5cdFx0XHR0aGlzLmxhc3RBY3RpdmUgPSBlbGVtZW50O1xyXG5cclxuXHRcdFx0Ly8gaGlkZSBlcnJvciBsYWJlbCBhbmQgcmVtb3ZlIGVycm9yIGNsYXNzIG9uIGZvY3VzIGlmIGVuYWJsZWRcclxuXHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLmZvY3VzQ2xlYW51cCAmJiAhdGhpcy5ibG9ja0ZvY3VzQ2xlYW51cCApIHtcclxuXHRcdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MudW5oaWdobGlnaHQgKSB7XHJcblx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLnVuaGlnaGxpZ2h0LmNhbGwoIHRoaXMsIGVsZW1lbnQsIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcywgdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHRoaXMuYWRkV3JhcHBlcih0aGlzLmVycm9yc0ZvcihlbGVtZW50KSkuaGlkZSgpO1xyXG5cdFx0XHR9XHJcblx0XHR9LFxyXG5cdFx0b25mb2N1c291dDogZnVuY3Rpb24oIGVsZW1lbnQsIGV2ZW50ICkge1xyXG5cdFx0XHRpZiAoICF0aGlzLmNoZWNrYWJsZShlbGVtZW50KSAmJiAoZWxlbWVudC5uYW1lIGluIHRoaXMuc3VibWl0dGVkIHx8ICF0aGlzLm9wdGlvbmFsKGVsZW1lbnQpKSApIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnQoZWxlbWVudCk7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHRvbmtleXVwOiBmdW5jdGlvbiggZWxlbWVudCwgZXZlbnQgKSB7XHJcblx0XHRcdGlmICggZXZlbnQud2hpY2ggPT09IDkgJiYgdGhpcy5lbGVtZW50VmFsdWUoZWxlbWVudCkgPT09IFwiXCIgKSB7XHJcblx0XHRcdFx0cmV0dXJuO1xyXG5cdFx0XHR9IGVsc2UgaWYgKCBlbGVtZW50Lm5hbWUgaW4gdGhpcy5zdWJtaXR0ZWQgfHwgZWxlbWVudCA9PT0gdGhpcy5sYXN0RWxlbWVudCApIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnQoZWxlbWVudCk7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHRvbmNsaWNrOiBmdW5jdGlvbiggZWxlbWVudCwgZXZlbnQgKSB7XHJcblx0XHRcdC8vIGNsaWNrIG9uIHNlbGVjdHMsIHJhZGlvYnV0dG9ucyBhbmQgY2hlY2tib3hlc1xyXG5cdFx0XHRpZiAoIGVsZW1lbnQubmFtZSBpbiB0aGlzLnN1Ym1pdHRlZCApIHtcclxuXHRcdFx0XHR0aGlzLmVsZW1lbnQoZWxlbWVudCk7XHJcblx0XHRcdH1cclxuXHRcdFx0Ly8gb3Igb3B0aW9uIGVsZW1lbnRzLCBjaGVjayBwYXJlbnQgc2VsZWN0IGluIHRoYXQgY2FzZVxyXG5cdFx0XHRlbHNlIGlmICggZWxlbWVudC5wYXJlbnROb2RlLm5hbWUgaW4gdGhpcy5zdWJtaXR0ZWQgKSB7XHJcblx0XHRcdFx0dGhpcy5lbGVtZW50KGVsZW1lbnQucGFyZW50Tm9kZSk7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHRoaWdobGlnaHQ6IGZ1bmN0aW9uKCBlbGVtZW50LCBlcnJvckNsYXNzLCB2YWxpZENsYXNzICkge1xyXG5cdFx0XHRpZiAoIGVsZW1lbnQudHlwZSA9PT0gXCJyYWRpb1wiICkge1xyXG5cdFx0XHRcdHRoaXMuZmluZEJ5TmFtZShlbGVtZW50Lm5hbWUpLmFkZENsYXNzKGVycm9yQ2xhc3MpLnJlbW92ZUNsYXNzKHZhbGlkQ2xhc3MpO1xyXG5cdFx0XHR9IGVsc2Uge1xyXG5cdFx0XHRcdCQoZWxlbWVudCkuYWRkQ2xhc3MoZXJyb3JDbGFzcykucmVtb3ZlQ2xhc3ModmFsaWRDbGFzcyk7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblx0XHR1bmhpZ2hsaWdodDogZnVuY3Rpb24oIGVsZW1lbnQsIGVycm9yQ2xhc3MsIHZhbGlkQ2xhc3MgKSB7XHJcblx0XHRcdGlmICggZWxlbWVudC50eXBlID09PSBcInJhZGlvXCIgKSB7XHJcblx0XHRcdFx0dGhpcy5maW5kQnlOYW1lKGVsZW1lbnQubmFtZSkucmVtb3ZlQ2xhc3MoZXJyb3JDbGFzcykuYWRkQ2xhc3ModmFsaWRDbGFzcyk7XHJcblx0XHRcdH0gZWxzZSB7XHJcblx0XHRcdFx0JChlbGVtZW50KS5yZW1vdmVDbGFzcyhlcnJvckNsYXNzKS5hZGRDbGFzcyh2YWxpZENsYXNzKTtcclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdH0sXHJcblxyXG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9zZXREZWZhdWx0c1xyXG5cdHNldERlZmF1bHRzOiBmdW5jdGlvbiggc2V0dGluZ3MgKSB7XHJcblx0XHQkLmV4dGVuZCggJC52YWxpZGF0b3IuZGVmYXVsdHMsIHNldHRpbmdzICk7XHJcblx0fSxcclxuXHJcblx0bWVzc2FnZXM6IHtcclxuXHRcdHJlcXVpcmVkOiBcIlRoaXMgZmllbGQgaXMgcmVxdWlyZWQuXCIsXHJcblx0XHRyZW1vdGU6IFwiUGxlYXNlIGZpeCB0aGlzIGZpZWxkLlwiLFxyXG5cdFx0ZW1haWw6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgZW1haWwgYWRkcmVzcy5cIixcclxuXHRcdHVybDogXCJQbGVhc2UgZW50ZXIgYSB2YWxpZCBVUkwuXCIsXHJcblx0XHRkYXRlOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIGRhdGUuXCIsXHJcblx0XHRkYXRlSVNPOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIGRhdGUgKElTTykuXCIsXHJcblx0XHRudW1iZXI6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgbnVtYmVyLlwiLFxyXG5cdFx0ZGlnaXRzOiBcIlBsZWFzZSBlbnRlciBvbmx5IGRpZ2l0cy5cIixcclxuXHRcdGNyZWRpdGNhcmQ6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgY3JlZGl0IGNhcmQgbnVtYmVyLlwiLFxyXG5cdFx0ZXF1YWxUbzogXCJQbGVhc2UgZW50ZXIgdGhlIHNhbWUgdmFsdWUgYWdhaW4uXCIsXHJcblx0XHRtYXhsZW5ndGg6ICQudmFsaWRhdG9yLmZvcm1hdChcIlBsZWFzZSBlbnRlciBubyBtb3JlIHRoYW4gezB9IGNoYXJhY3RlcnMuXCIpLFxyXG5cdFx0bWlubGVuZ3RoOiAkLnZhbGlkYXRvci5mb3JtYXQoXCJQbGVhc2UgZW50ZXIgYXQgbGVhc3QgezB9IGNoYXJhY3RlcnMuXCIpLFxyXG5cdFx0cmFuZ2VsZW5ndGg6ICQudmFsaWRhdG9yLmZvcm1hdChcIlBsZWFzZSBlbnRlciBhIHZhbHVlIGJldHdlZW4gezB9IGFuZCB7MX0gY2hhcmFjdGVycyBsb25nLlwiKSxcclxuXHRcdHJhbmdlOiAkLnZhbGlkYXRvci5mb3JtYXQoXCJQbGVhc2UgZW50ZXIgYSB2YWx1ZSBiZXR3ZWVuIHswfSBhbmQgezF9LlwiKSxcclxuXHRcdG1heDogJC52YWxpZGF0b3IuZm9ybWF0KFwiUGxlYXNlIGVudGVyIGEgdmFsdWUgbGVzcyB0aGFuIG9yIGVxdWFsIHRvIHswfS5cIiksXHJcblx0XHRtaW46ICQudmFsaWRhdG9yLmZvcm1hdChcIlBsZWFzZSBlbnRlciBhIHZhbHVlIGdyZWF0ZXIgdGhhbiBvciBlcXVhbCB0byB7MH0uXCIpXHJcblx0fSxcclxuXHJcblx0YXV0b0NyZWF0ZVJhbmdlczogZmFsc2UsXHJcblxyXG5cdHByb3RvdHlwZToge1xyXG5cclxuXHRcdGluaXQ6IGZ1bmN0aW9uKCkge1xyXG5cdFx0XHR0aGlzLmxhYmVsQ29udGFpbmVyID0gJCh0aGlzLnNldHRpbmdzLmVycm9yTGFiZWxDb250YWluZXIpO1xyXG5cdFx0XHR0aGlzLmVycm9yQ29udGV4dCA9IHRoaXMubGFiZWxDb250YWluZXIubGVuZ3RoICYmIHRoaXMubGFiZWxDb250YWluZXIgfHwgJCh0aGlzLmN1cnJlbnRGb3JtKTtcclxuXHRcdFx0dGhpcy5jb250YWluZXJzID0gJCh0aGlzLnNldHRpbmdzLmVycm9yQ29udGFpbmVyKS5hZGQoIHRoaXMuc2V0dGluZ3MuZXJyb3JMYWJlbENvbnRhaW5lciApO1xyXG5cdFx0XHR0aGlzLnN1Ym1pdHRlZCA9IHt9O1xyXG5cdFx0XHR0aGlzLnZhbHVlQ2FjaGUgPSB7fTtcclxuXHRcdFx0dGhpcy5wZW5kaW5nUmVxdWVzdCA9IDA7XHJcblx0XHRcdHRoaXMucGVuZGluZyA9IHt9O1xyXG5cdFx0XHR0aGlzLmludmFsaWQgPSB7fTtcclxuXHRcdFx0dGhpcy5yZXNldCgpO1xyXG5cclxuXHRcdFx0dmFyIGdyb3VwcyA9ICh0aGlzLmdyb3VwcyA9IHt9KTtcclxuXHRcdFx0JC5lYWNoKHRoaXMuc2V0dGluZ3MuZ3JvdXBzLCBmdW5jdGlvbigga2V5LCB2YWx1ZSApIHtcclxuXHRcdFx0XHRpZiAoIHR5cGVvZiB2YWx1ZSA9PT0gXCJzdHJpbmdcIiApIHtcclxuXHRcdFx0XHRcdHZhbHVlID0gdmFsdWUuc3BsaXQoL1xccy8pO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHQkLmVhY2godmFsdWUsIGZ1bmN0aW9uKCBpbmRleCwgbmFtZSApIHtcclxuXHRcdFx0XHRcdGdyb3Vwc1tuYW1lXSA9IGtleTtcclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0fSk7XHJcblx0XHRcdHZhciBydWxlcyA9IHRoaXMuc2V0dGluZ3MucnVsZXM7XHJcblx0XHRcdCQuZWFjaChydWxlcywgZnVuY3Rpb24oIGtleSwgdmFsdWUgKSB7XHJcblx0XHRcdFx0cnVsZXNba2V5XSA9ICQudmFsaWRhdG9yLm5vcm1hbGl6ZVJ1bGUodmFsdWUpO1xyXG5cdFx0XHR9KTtcclxuXHJcblx0XHRcdGZ1bmN0aW9uIGRlbGVnYXRlKGV2ZW50KSB7XHJcblx0XHRcdFx0dmFyIHZhbGlkYXRvciA9ICQuZGF0YSh0aGlzWzBdLmZvcm0sIFwidmFsaWRhdG9yXCIpLFxyXG5cdFx0XHRcdFx0ZXZlbnRUeXBlID0gXCJvblwiICsgZXZlbnQudHlwZS5yZXBsYWNlKC9edmFsaWRhdGUvLCBcIlwiKTtcclxuXHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5nc1tldmVudFR5cGVdICkge1xyXG5cdFx0XHRcdFx0dmFsaWRhdG9yLnNldHRpbmdzW2V2ZW50VHlwZV0uY2FsbCh2YWxpZGF0b3IsIHRoaXNbMF0sIGV2ZW50KTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH1cclxuXHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKVxyXG5cdFx0XHRcdC52YWxpZGF0ZURlbGVnYXRlKFwiOnRleHQsIFt0eXBlPSdwYXNzd29yZCddLCBbdHlwZT0nZmlsZSddLCBzZWxlY3QsIHRleHRhcmVhLCBcIiArXHJcblx0XHRcdFx0XHRcIlt0eXBlPSdudW1iZXInXSwgW3R5cGU9J3NlYXJjaCddICxbdHlwZT0ndGVsJ10sIFt0eXBlPSd1cmwnXSwgXCIgK1xyXG5cdFx0XHRcdFx0XCJbdHlwZT0nZW1haWwnXSwgW3R5cGU9J2RhdGV0aW1lJ10sIFt0eXBlPSdkYXRlJ10sIFt0eXBlPSdtb250aCddLCBcIiArXHJcblx0XHRcdFx0XHRcIlt0eXBlPSd3ZWVrJ10sIFt0eXBlPSd0aW1lJ10sIFt0eXBlPSdkYXRldGltZS1sb2NhbCddLCBcIiArXHJcblx0XHRcdFx0XHRcIlt0eXBlPSdyYW5nZSddLCBbdHlwZT0nY29sb3InXSBcIixcclxuXHRcdFx0XHRcdFwiZm9jdXNpbiBmb2N1c291dCBrZXl1cFwiLCBkZWxlZ2F0ZSlcclxuXHRcdFx0XHQudmFsaWRhdGVEZWxlZ2F0ZShcIlt0eXBlPSdyYWRpbyddLCBbdHlwZT0nY2hlY2tib3gnXSwgc2VsZWN0LCBvcHRpb25cIiwgXCJjbGlja1wiLCBkZWxlZ2F0ZSk7XHJcblxyXG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MuaW52YWxpZEhhbmRsZXIgKSB7XHJcblx0XHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKS5iaW5kKFwiaW52YWxpZC1mb3JtLnZhbGlkYXRlXCIsIHRoaXMuc2V0dGluZ3MuaW52YWxpZEhhbmRsZXIpO1xyXG5cdFx0XHR9XHJcblx0XHR9LFxyXG5cclxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9mb3JtXHJcblx0XHRmb3JtOiBmdW5jdGlvbigpIHtcclxuXHRcdFx0dGhpcy5jaGVja0Zvcm0oKTtcclxuXHRcdFx0JC5leHRlbmQodGhpcy5zdWJtaXR0ZWQsIHRoaXMuZXJyb3JNYXApO1xyXG5cdFx0XHR0aGlzLmludmFsaWQgPSAkLmV4dGVuZCh7fSwgdGhpcy5lcnJvck1hcCk7XHJcblx0XHRcdGlmICggIXRoaXMudmFsaWQoKSApIHtcclxuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLnRyaWdnZXJIYW5kbGVyKFwiaW52YWxpZC1mb3JtXCIsIFt0aGlzXSk7XHJcblx0XHRcdH1cclxuXHRcdFx0dGhpcy5zaG93RXJyb3JzKCk7XHJcblx0XHRcdHJldHVybiB0aGlzLnZhbGlkKCk7XHJcblx0XHR9LFxyXG5cclxuXHRcdGNoZWNrRm9ybTogZnVuY3Rpb24oKSB7XHJcblx0XHRcdHRoaXMucHJlcGFyZUZvcm0oKTtcclxuXHRcdFx0Zm9yICggdmFyIGkgPSAwLCBlbGVtZW50cyA9ICh0aGlzLmN1cnJlbnRFbGVtZW50cyA9IHRoaXMuZWxlbWVudHMoKSk7IGVsZW1lbnRzW2ldOyBpKysgKSB7XHJcblx0XHRcdFx0dGhpcy5jaGVjayggZWxlbWVudHNbaV0gKTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdGhpcy52YWxpZCgpO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3IvZWxlbWVudFxyXG5cdFx0ZWxlbWVudDogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XHJcblx0XHRcdGVsZW1lbnQgPSB0aGlzLnZhbGlkYXRpb25UYXJnZXRGb3IoIHRoaXMuY2xlYW4oIGVsZW1lbnQgKSApO1xyXG5cdFx0XHR0aGlzLmxhc3RFbGVtZW50ID0gZWxlbWVudDtcclxuXHRcdFx0dGhpcy5wcmVwYXJlRWxlbWVudCggZWxlbWVudCApO1xyXG5cdFx0XHR0aGlzLmN1cnJlbnRFbGVtZW50cyA9ICQoZWxlbWVudCk7XHJcblx0XHRcdHZhciByZXN1bHQgPSB0aGlzLmNoZWNrKCBlbGVtZW50ICkgIT09IGZhbHNlO1xyXG5cdFx0XHRpZiAoIHJlc3VsdCApIHtcclxuXHRcdFx0XHRkZWxldGUgdGhpcy5pbnZhbGlkW2VsZW1lbnQubmFtZV07XHJcblx0XHRcdH0gZWxzZSB7XHJcblx0XHRcdFx0dGhpcy5pbnZhbGlkW2VsZW1lbnQubmFtZV0gPSB0cnVlO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmICggIXRoaXMubnVtYmVyT2ZJbnZhbGlkcygpICkge1xyXG5cdFx0XHRcdC8vIEhpZGUgZXJyb3IgY29udGFpbmVycyBvbiBsYXN0IGVycm9yXHJcblx0XHRcdFx0dGhpcy50b0hpZGUgPSB0aGlzLnRvSGlkZS5hZGQoIHRoaXMuY29udGFpbmVycyApO1xyXG5cdFx0XHR9XHJcblx0XHRcdHRoaXMuc2hvd0Vycm9ycygpO1xyXG5cdFx0XHRyZXR1cm4gcmVzdWx0O1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3Ivc2hvd0Vycm9yc1xyXG5cdFx0c2hvd0Vycm9yczogZnVuY3Rpb24oIGVycm9ycyApIHtcclxuXHRcdFx0aWYgKCBlcnJvcnMgKSB7XHJcblx0XHRcdFx0Ly8gYWRkIGl0ZW1zIHRvIGVycm9yIGxpc3QgYW5kIG1hcFxyXG5cdFx0XHRcdCQuZXh0ZW5kKCB0aGlzLmVycm9yTWFwLCBlcnJvcnMgKTtcclxuXHRcdFx0XHR0aGlzLmVycm9yTGlzdCA9IFtdO1xyXG5cdFx0XHRcdGZvciAoIHZhciBuYW1lIGluIGVycm9ycyApIHtcclxuXHRcdFx0XHRcdHRoaXMuZXJyb3JMaXN0LnB1c2goe1xyXG5cdFx0XHRcdFx0XHRtZXNzYWdlOiBlcnJvcnNbbmFtZV0sXHJcblx0XHRcdFx0XHRcdGVsZW1lbnQ6IHRoaXMuZmluZEJ5TmFtZShuYW1lKVswXVxyXG5cdFx0XHRcdFx0fSk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdC8vIHJlbW92ZSBpdGVtcyBmcm9tIHN1Y2Nlc3MgbGlzdFxyXG5cdFx0XHRcdHRoaXMuc3VjY2Vzc0xpc3QgPSAkLmdyZXAoIHRoaXMuc3VjY2Vzc0xpc3QsIGZ1bmN0aW9uKCBlbGVtZW50ICkge1xyXG5cdFx0XHRcdFx0cmV0dXJuICEoZWxlbWVudC5uYW1lIGluIGVycm9ycyk7XHJcblx0XHRcdFx0fSk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLnNob3dFcnJvcnMgKSB7XHJcblx0XHRcdFx0dGhpcy5zZXR0aW5ncy5zaG93RXJyb3JzLmNhbGwoIHRoaXMsIHRoaXMuZXJyb3JNYXAsIHRoaXMuZXJyb3JMaXN0ICk7XHJcblx0XHRcdH0gZWxzZSB7XHJcblx0XHRcdFx0dGhpcy5kZWZhdWx0U2hvd0Vycm9ycygpO1xyXG5cdFx0XHR9XHJcblx0XHR9LFxyXG5cclxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9yZXNldEZvcm1cclxuXHRcdHJlc2V0Rm9ybTogZnVuY3Rpb24oKSB7XHJcblx0XHRcdGlmICggJC5mbi5yZXNldEZvcm0gKSB7XHJcblx0XHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKS5yZXNldEZvcm0oKTtcclxuXHRcdFx0fVxyXG5cdFx0XHR0aGlzLnN1Ym1pdHRlZCA9IHt9O1xyXG5cdFx0XHR0aGlzLmxhc3RFbGVtZW50ID0gbnVsbDtcclxuXHRcdFx0dGhpcy5wcmVwYXJlRm9ybSgpO1xyXG5cdFx0XHR0aGlzLmhpZGVFcnJvcnMoKTtcclxuXHRcdFx0dGhpcy5lbGVtZW50cygpLnJlbW92ZUNsYXNzKCB0aGlzLnNldHRpbmdzLmVycm9yQ2xhc3MgKS5yZW1vdmVEYXRhKCBcInByZXZpb3VzVmFsdWVcIiApO1xyXG5cdFx0fSxcclxuXHJcblx0XHRudW1iZXJPZkludmFsaWRzOiBmdW5jdGlvbigpIHtcclxuXHRcdFx0cmV0dXJuIHRoaXMub2JqZWN0TGVuZ3RoKHRoaXMuaW52YWxpZCk7XHJcblx0XHR9LFxyXG5cclxuXHRcdG9iamVjdExlbmd0aDogZnVuY3Rpb24oIG9iaiApIHtcclxuXHRcdFx0dmFyIGNvdW50ID0gMDtcclxuXHRcdFx0Zm9yICggdmFyIGkgaW4gb2JqICkge1xyXG5cdFx0XHRcdGNvdW50Kys7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIGNvdW50O1xyXG5cdFx0fSxcclxuXHJcblx0XHRoaWRlRXJyb3JzOiBmdW5jdGlvbigpIHtcclxuXHRcdFx0dGhpcy5hZGRXcmFwcGVyKCB0aGlzLnRvSGlkZSApLmhpZGUoKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0dmFsaWQ6IGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5zaXplKCkgPT09IDA7XHJcblx0XHR9LFxyXG5cclxuXHRcdHNpemU6IGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5lcnJvckxpc3QubGVuZ3RoO1xyXG5cdFx0fSxcclxuXHJcblx0XHRmb2N1c0ludmFsaWQ6IGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MuZm9jdXNJbnZhbGlkICkge1xyXG5cdFx0XHRcdHRyeSB7XHJcblx0XHRcdFx0XHQkKHRoaXMuZmluZExhc3RBY3RpdmUoKSB8fCB0aGlzLmVycm9yTGlzdC5sZW5ndGggJiYgdGhpcy5lcnJvckxpc3RbMF0uZWxlbWVudCB8fCBbXSlcclxuXHRcdFx0XHRcdC5maWx0ZXIoXCI6dmlzaWJsZVwiKVxyXG5cdFx0XHRcdFx0LmZvY3VzKClcclxuXHRcdFx0XHRcdC8vIG1hbnVhbGx5IHRyaWdnZXIgZm9jdXNpbiBldmVudDsgd2l0aG91dCBpdCwgZm9jdXNpbiBoYW5kbGVyIGlzbid0IGNhbGxlZCwgZmluZExhc3RBY3RpdmUgd29uJ3QgaGF2ZSBhbnl0aGluZyB0byBmaW5kXHJcblx0XHRcdFx0XHQudHJpZ2dlcihcImZvY3VzaW5cIik7XHJcblx0XHRcdFx0fSBjYXRjaChlKSB7XHJcblx0XHRcdFx0XHQvLyBpZ25vcmUgSUUgdGhyb3dpbmcgZXJyb3JzIHdoZW4gZm9jdXNpbmcgaGlkZGVuIGVsZW1lbnRzXHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHR9LFxyXG5cclxuXHRcdGZpbmRMYXN0QWN0aXZlOiBmdW5jdGlvbigpIHtcclxuXHRcdFx0dmFyIGxhc3RBY3RpdmUgPSB0aGlzLmxhc3RBY3RpdmU7XHJcblx0XHRcdHJldHVybiBsYXN0QWN0aXZlICYmICQuZ3JlcCh0aGlzLmVycm9yTGlzdCwgZnVuY3Rpb24oIG4gKSB7XHJcblx0XHRcdFx0cmV0dXJuIG4uZWxlbWVudC5uYW1lID09PSBsYXN0QWN0aXZlLm5hbWU7XHJcblx0XHRcdH0pLmxlbmd0aCA9PT0gMSAmJiBsYXN0QWN0aXZlO1xyXG5cdFx0fSxcclxuXHJcblx0XHRlbGVtZW50czogZnVuY3Rpb24oKSB7XHJcblx0XHRcdHZhciB2YWxpZGF0b3IgPSB0aGlzLFxyXG5cdFx0XHRcdHJ1bGVzQ2FjaGUgPSB7fTtcclxuXHJcblx0XHRcdC8vIHNlbGVjdCBhbGwgdmFsaWQgaW5wdXRzIGluc2lkZSB0aGUgZm9ybSAobm8gc3VibWl0IG9yIHJlc2V0IGJ1dHRvbnMpXHJcblx0XHRcdHJldHVybiAkKHRoaXMuY3VycmVudEZvcm0pXHJcblx0XHRcdC5maW5kKFwiaW5wdXQsIHNlbGVjdCwgdGV4dGFyZWFcIilcclxuXHRcdFx0Lm5vdChcIjpzdWJtaXQsIDpyZXNldCwgOmltYWdlLCBbZGlzYWJsZWRdXCIpXHJcblx0XHRcdC5ub3QoIHRoaXMuc2V0dGluZ3MuaWdub3JlIClcclxuXHRcdFx0LmZpbHRlcihmdW5jdGlvbigpIHtcclxuXHRcdFx0XHRpZiAoICF0aGlzLm5hbWUgJiYgdmFsaWRhdG9yLnNldHRpbmdzLmRlYnVnICYmIHdpbmRvdy5jb25zb2xlICkge1xyXG5cdFx0XHRcdFx0Y29uc29sZS5lcnJvciggXCIlbyBoYXMgbm8gbmFtZSBhc3NpZ25lZFwiLCB0aGlzKTtcclxuXHRcdFx0XHR9XHJcblxyXG5cdFx0XHRcdC8vIHNlbGVjdCBvbmx5IHRoZSBmaXJzdCBlbGVtZW50IGZvciBlYWNoIG5hbWUsIGFuZCBvbmx5IHRob3NlIHdpdGggcnVsZXMgc3BlY2lmaWVkXHJcblx0XHRcdFx0aWYgKCB0aGlzLm5hbWUgaW4gcnVsZXNDYWNoZSB8fCAhdmFsaWRhdG9yLm9iamVjdExlbmd0aCgkKHRoaXMpLnJ1bGVzKCkpICkge1xyXG5cdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdH1cclxuXHJcblx0XHRcdFx0cnVsZXNDYWNoZVt0aGlzLm5hbWVdID0gdHJ1ZTtcclxuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcclxuXHRcdFx0fSk7XHJcblx0XHR9LFxyXG5cclxuXHRcdGNsZWFuOiBmdW5jdGlvbiggc2VsZWN0b3IgKSB7XHJcblx0XHRcdHJldHVybiAkKHNlbGVjdG9yKVswXTtcclxuXHRcdH0sXHJcblxyXG5cdFx0ZXJyb3JzOiBmdW5jdGlvbigpIHtcclxuXHRcdFx0dmFyIGVycm9yQ2xhc3MgPSB0aGlzLnNldHRpbmdzLmVycm9yQ2xhc3MucmVwbGFjZShcIiBcIiwgXCIuXCIpO1xyXG5cdFx0XHRyZXR1cm4gJCh0aGlzLnNldHRpbmdzLmVycm9yRWxlbWVudCArIFwiLlwiICsgZXJyb3JDbGFzcywgdGhpcy5lcnJvckNvbnRleHQpO1xyXG5cdFx0fSxcclxuXHJcblx0XHRyZXNldDogZnVuY3Rpb24oKSB7XHJcblx0XHRcdHRoaXMuc3VjY2Vzc0xpc3QgPSBbXTtcclxuXHRcdFx0dGhpcy5lcnJvckxpc3QgPSBbXTtcclxuXHRcdFx0dGhpcy5lcnJvck1hcCA9IHt9O1xyXG5cdFx0XHR0aGlzLnRvU2hvdyA9ICQoW10pO1xyXG5cdFx0XHR0aGlzLnRvSGlkZSA9ICQoW10pO1xyXG5cdFx0XHR0aGlzLmN1cnJlbnRFbGVtZW50cyA9ICQoW10pO1xyXG5cdFx0fSxcclxuXHJcblx0XHRwcmVwYXJlRm9ybTogZnVuY3Rpb24oKSB7XHJcblx0XHRcdHRoaXMucmVzZXQoKTtcclxuXHRcdFx0dGhpcy50b0hpZGUgPSB0aGlzLmVycm9ycygpLmFkZCggdGhpcy5jb250YWluZXJzICk7XHJcblx0XHR9LFxyXG5cclxuXHRcdHByZXBhcmVFbGVtZW50OiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdFx0dGhpcy5yZXNldCgpO1xyXG5cdFx0XHR0aGlzLnRvSGlkZSA9IHRoaXMuZXJyb3JzRm9yKGVsZW1lbnQpO1xyXG5cdFx0fSxcclxuXHJcblx0XHRlbGVtZW50VmFsdWU6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xyXG5cdFx0XHR2YXIgdHlwZSA9ICQoZWxlbWVudCkuYXR0cihcInR5cGVcIiksXHJcblx0XHRcdFx0dmFsID0gJChlbGVtZW50KS52YWwoKTtcclxuXHJcblx0XHRcdGlmICggdHlwZSA9PT0gXCJyYWRpb1wiIHx8IHR5cGUgPT09IFwiY2hlY2tib3hcIiApIHtcclxuXHRcdFx0XHRyZXR1cm4gJChcImlucHV0W25hbWU9J1wiICsgJChlbGVtZW50KS5hdHRyKFwibmFtZVwiKSArIFwiJ106Y2hlY2tlZFwiKS52YWwoKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0aWYgKCB0eXBlb2YgdmFsID09PSBcInN0cmluZ1wiICkge1xyXG5cdFx0XHRcdHJldHVybiB2YWwucmVwbGFjZSgvXFxyL2csIFwiXCIpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB2YWw7XHJcblx0XHR9LFxyXG5cclxuXHRcdGNoZWNrOiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdFx0ZWxlbWVudCA9IHRoaXMudmFsaWRhdGlvblRhcmdldEZvciggdGhpcy5jbGVhbiggZWxlbWVudCApICk7XHJcblxyXG5cdFx0XHR2YXIgcnVsZXMgPSAkKGVsZW1lbnQpLnJ1bGVzKCk7XHJcblx0XHRcdHZhciBkZXBlbmRlbmN5TWlzbWF0Y2ggPSBmYWxzZTtcclxuXHRcdFx0dmFyIHZhbCA9IHRoaXMuZWxlbWVudFZhbHVlKGVsZW1lbnQpO1xyXG5cdFx0XHR2YXIgcmVzdWx0O1xyXG5cclxuXHRcdFx0Zm9yICh2YXIgbWV0aG9kIGluIHJ1bGVzICkge1xyXG5cdFx0XHRcdHZhciBydWxlID0geyBtZXRob2Q6IG1ldGhvZCwgcGFyYW1ldGVyczogcnVsZXNbbWV0aG9kXSB9O1xyXG5cdFx0XHRcdHRyeSB7XHJcblxyXG5cdFx0XHRcdFx0cmVzdWx0ID0gJC52YWxpZGF0b3IubWV0aG9kc1ttZXRob2RdLmNhbGwoIHRoaXMsIHZhbCwgZWxlbWVudCwgcnVsZS5wYXJhbWV0ZXJzICk7XHJcblxyXG5cdFx0XHRcdFx0Ly8gaWYgYSBtZXRob2QgaW5kaWNhdGVzIHRoYXQgdGhlIGZpZWxkIGlzIG9wdGlvbmFsIGFuZCB0aGVyZWZvcmUgdmFsaWQsXHJcblx0XHRcdFx0XHQvLyBkb24ndCBtYXJrIGl0IGFzIHZhbGlkIHdoZW4gdGhlcmUgYXJlIG5vIG90aGVyIHJ1bGVzXHJcblx0XHRcdFx0XHRpZiAoIHJlc3VsdCA9PT0gXCJkZXBlbmRlbmN5LW1pc21hdGNoXCIgKSB7XHJcblx0XHRcdFx0XHRcdGRlcGVuZGVuY3lNaXNtYXRjaCA9IHRydWU7XHJcblx0XHRcdFx0XHRcdGNvbnRpbnVlO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0ZGVwZW5kZW5jeU1pc21hdGNoID0gZmFsc2U7XHJcblxyXG5cdFx0XHRcdFx0aWYgKCByZXN1bHQgPT09IFwicGVuZGluZ1wiICkge1xyXG5cdFx0XHRcdFx0XHR0aGlzLnRvSGlkZSA9IHRoaXMudG9IaWRlLm5vdCggdGhpcy5lcnJvcnNGb3IoZWxlbWVudCkgKTtcclxuXHRcdFx0XHRcdFx0cmV0dXJuO1xyXG5cdFx0XHRcdFx0fVxyXG5cclxuXHRcdFx0XHRcdGlmICggIXJlc3VsdCApIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5mb3JtYXRBbmRBZGQoIGVsZW1lbnQsIHJ1bGUgKTtcclxuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdH0gY2F0Y2goZSkge1xyXG5cdFx0XHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLmRlYnVnICYmIHdpbmRvdy5jb25zb2xlICkge1xyXG5cdFx0XHRcdFx0XHRjb25zb2xlLmxvZyggXCJFeGNlcHRpb24gb2NjdXJyZWQgd2hlbiBjaGVja2luZyBlbGVtZW50IFwiICsgZWxlbWVudC5pZCArIFwiLCBjaGVjayB0aGUgJ1wiICsgcnVsZS5tZXRob2QgKyBcIicgbWV0aG9kLlwiLCBlICk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR0aHJvdyBlO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZiAoIGRlcGVuZGVuY3lNaXNtYXRjaCApIHtcclxuXHRcdFx0XHRyZXR1cm47XHJcblx0XHRcdH1cclxuXHRcdFx0aWYgKCB0aGlzLm9iamVjdExlbmd0aChydWxlcykgKSB7XHJcblx0XHRcdFx0dGhpcy5zdWNjZXNzTGlzdC5wdXNoKGVsZW1lbnQpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB0cnVlO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyByZXR1cm4gdGhlIGN1c3RvbSBtZXNzYWdlIGZvciB0aGUgZ2l2ZW4gZWxlbWVudCBhbmQgdmFsaWRhdGlvbiBtZXRob2RcclxuXHRcdC8vIHNwZWNpZmllZCBpbiB0aGUgZWxlbWVudCdzIEhUTUw1IGRhdGEgYXR0cmlidXRlXHJcblx0XHRjdXN0b21EYXRhTWVzc2FnZTogZnVuY3Rpb24oIGVsZW1lbnQsIG1ldGhvZCApIHtcclxuXHRcdFx0cmV0dXJuICQoZWxlbWVudCkuZGF0YShcIm1zZy1cIiArIG1ldGhvZC50b0xvd2VyQ2FzZSgpKSB8fCAoZWxlbWVudC5hdHRyaWJ1dGVzICYmICQoZWxlbWVudCkuYXR0cihcImRhdGEtbXNnLVwiICsgbWV0aG9kLnRvTG93ZXJDYXNlKCkpKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gcmV0dXJuIHRoZSBjdXN0b20gbWVzc2FnZSBmb3IgdGhlIGdpdmVuIGVsZW1lbnQgbmFtZSBhbmQgdmFsaWRhdGlvbiBtZXRob2RcclxuXHRcdGN1c3RvbU1lc3NhZ2U6IGZ1bmN0aW9uKCBuYW1lLCBtZXRob2QgKSB7XHJcblx0XHRcdHZhciBtID0gdGhpcy5zZXR0aW5ncy5tZXNzYWdlc1tuYW1lXTtcclxuXHRcdFx0cmV0dXJuIG0gJiYgKG0uY29uc3RydWN0b3IgPT09IFN0cmluZyA/IG0gOiBtW21ldGhvZF0pO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyByZXR1cm4gdGhlIGZpcnN0IGRlZmluZWQgYXJndW1lbnQsIGFsbG93aW5nIGVtcHR5IHN0cmluZ3NcclxuXHRcdGZpbmREZWZpbmVkOiBmdW5jdGlvbigpIHtcclxuXHRcdFx0Zm9yKHZhciBpID0gMDsgaSA8IGFyZ3VtZW50cy5sZW5ndGg7IGkrKykge1xyXG5cdFx0XHRcdGlmICggYXJndW1lbnRzW2ldICE9PSB1bmRlZmluZWQgKSB7XHJcblx0XHRcdFx0XHRyZXR1cm4gYXJndW1lbnRzW2ldO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdW5kZWZpbmVkO1xyXG5cdFx0fSxcclxuXHJcblx0XHRkZWZhdWx0TWVzc2FnZTogZnVuY3Rpb24oIGVsZW1lbnQsIG1ldGhvZCApIHtcclxuXHRcdFx0cmV0dXJuIHRoaXMuZmluZERlZmluZWQoXHJcblx0XHRcdFx0dGhpcy5jdXN0b21NZXNzYWdlKCBlbGVtZW50Lm5hbWUsIG1ldGhvZCApLFxyXG5cdFx0XHRcdHRoaXMuY3VzdG9tRGF0YU1lc3NhZ2UoIGVsZW1lbnQsIG1ldGhvZCApLFxyXG5cdFx0XHRcdC8vIHRpdGxlIGlzIG5ldmVyIHVuZGVmaW5lZCwgc28gaGFuZGxlIGVtcHR5IHN0cmluZyBhcyB1bmRlZmluZWRcclxuXHRcdFx0XHQhdGhpcy5zZXR0aW5ncy5pZ25vcmVUaXRsZSAmJiBlbGVtZW50LnRpdGxlIHx8IHVuZGVmaW5lZCxcclxuXHRcdFx0XHQkLnZhbGlkYXRvci5tZXNzYWdlc1ttZXRob2RdLFxyXG5cdFx0XHRcdFwiPHN0cm9uZz5XYXJuaW5nOiBObyBtZXNzYWdlIGRlZmluZWQgZm9yIFwiICsgZWxlbWVudC5uYW1lICsgXCI8L3N0cm9uZz5cIlxyXG5cdFx0XHQpO1xyXG5cdFx0fSxcclxuXHJcblx0XHRmb3JtYXRBbmRBZGQ6IGZ1bmN0aW9uKCBlbGVtZW50LCBydWxlICkge1xyXG5cdFx0XHR2YXIgbWVzc2FnZSA9IHRoaXMuZGVmYXVsdE1lc3NhZ2UoIGVsZW1lbnQsIHJ1bGUubWV0aG9kICksXHJcblx0XHRcdFx0dGhlcmVnZXggPSAvXFwkP1xceyhcXGQrKVxcfS9nO1xyXG5cdFx0XHRpZiAoIHR5cGVvZiBtZXNzYWdlID09PSBcImZ1bmN0aW9uXCIgKSB7XHJcblx0XHRcdFx0bWVzc2FnZSA9IG1lc3NhZ2UuY2FsbCh0aGlzLCBydWxlLnBhcmFtZXRlcnMsIGVsZW1lbnQpO1xyXG5cdFx0XHR9IGVsc2UgaWYgKHRoZXJlZ2V4LnRlc3QobWVzc2FnZSkpIHtcclxuXHRcdFx0XHRtZXNzYWdlID0gJC52YWxpZGF0b3IuZm9ybWF0KG1lc3NhZ2UucmVwbGFjZSh0aGVyZWdleCwgXCJ7JDF9XCIpLCBydWxlLnBhcmFtZXRlcnMpO1xyXG5cdFx0XHR9XHJcblx0XHRcdHRoaXMuZXJyb3JMaXN0LnB1c2goe1xyXG5cdFx0XHRcdG1lc3NhZ2U6IG1lc3NhZ2UsXHJcblx0XHRcdFx0ZWxlbWVudDogZWxlbWVudFxyXG5cdFx0XHR9KTtcclxuXHJcblx0XHRcdHRoaXMuZXJyb3JNYXBbZWxlbWVudC5uYW1lXSA9IG1lc3NhZ2U7XHJcblx0XHRcdHRoaXMuc3VibWl0dGVkW2VsZW1lbnQubmFtZV0gPSBtZXNzYWdlO1xyXG5cdFx0fSxcclxuXHJcblx0XHRhZGRXcmFwcGVyOiBmdW5jdGlvbiggdG9Ub2dnbGUgKSB7XHJcblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy53cmFwcGVyICkge1xyXG5cdFx0XHRcdHRvVG9nZ2xlID0gdG9Ub2dnbGUuYWRkKCB0b1RvZ2dsZS5wYXJlbnQoIHRoaXMuc2V0dGluZ3Mud3JhcHBlciApICk7XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIHRvVG9nZ2xlO1xyXG5cdFx0fSxcclxuXHJcblx0XHRkZWZhdWx0U2hvd0Vycm9yczogZnVuY3Rpb24oKSB7XHJcblx0XHRcdHZhciBpLCBlbGVtZW50cztcclxuXHRcdFx0Zm9yICggaSA9IDA7IHRoaXMuZXJyb3JMaXN0W2ldOyBpKysgKSB7XHJcblx0XHRcdFx0dmFyIGVycm9yID0gdGhpcy5lcnJvckxpc3RbaV07XHJcblx0XHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLmhpZ2hsaWdodCApIHtcclxuXHRcdFx0XHRcdHRoaXMuc2V0dGluZ3MuaGlnaGxpZ2h0LmNhbGwoIHRoaXMsIGVycm9yLmVsZW1lbnQsIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcywgdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHRoaXMuc2hvd0xhYmVsKCBlcnJvci5lbGVtZW50LCBlcnJvci5tZXNzYWdlICk7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYgKCB0aGlzLmVycm9yTGlzdC5sZW5ndGggKSB7XHJcblx0XHRcdFx0dGhpcy50b1Nob3cgPSB0aGlzLnRvU2hvdy5hZGQoIHRoaXMuY29udGFpbmVycyApO1xyXG5cdFx0XHR9XHJcblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5zdWNjZXNzICkge1xyXG5cdFx0XHRcdGZvciAoIGkgPSAwOyB0aGlzLnN1Y2Nlc3NMaXN0W2ldOyBpKysgKSB7XHJcblx0XHRcdFx0XHR0aGlzLnNob3dMYWJlbCggdGhpcy5zdWNjZXNzTGlzdFtpXSApO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MudW5oaWdobGlnaHQgKSB7XHJcblx0XHRcdFx0Zm9yICggaSA9IDAsIGVsZW1lbnRzID0gdGhpcy52YWxpZEVsZW1lbnRzKCk7IGVsZW1lbnRzW2ldOyBpKysgKSB7XHJcblx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLnVuaGlnaGxpZ2h0LmNhbGwoIHRoaXMsIGVsZW1lbnRzW2ldLCB0aGlzLnNldHRpbmdzLmVycm9yQ2xhc3MsIHRoaXMuc2V0dGluZ3MudmFsaWRDbGFzcyApO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0XHR0aGlzLnRvSGlkZSA9IHRoaXMudG9IaWRlLm5vdCggdGhpcy50b1Nob3cgKTtcclxuXHRcdFx0dGhpcy5oaWRlRXJyb3JzKCk7XHJcblx0XHRcdHRoaXMuYWRkV3JhcHBlciggdGhpcy50b1Nob3cgKS5zaG93KCk7XHJcblx0XHR9LFxyXG5cclxuXHRcdHZhbGlkRWxlbWVudHM6IGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5jdXJyZW50RWxlbWVudHMubm90KHRoaXMuaW52YWxpZEVsZW1lbnRzKCkpO1xyXG5cdFx0fSxcclxuXHJcblx0XHRpbnZhbGlkRWxlbWVudHM6IGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRyZXR1cm4gJCh0aGlzLmVycm9yTGlzdCkubWFwKGZ1bmN0aW9uKCkge1xyXG5cdFx0XHRcdHJldHVybiB0aGlzLmVsZW1lbnQ7XHJcblx0XHRcdH0pO1xyXG5cdFx0fSxcclxuXHJcblx0XHRzaG93TGFiZWw6IGZ1bmN0aW9uKCBlbGVtZW50LCBtZXNzYWdlICkge1xyXG5cdFx0XHR2YXIgbGFiZWwgPSB0aGlzLmVycm9yc0ZvciggZWxlbWVudCApO1xyXG5cdFx0XHRpZiAoIGxhYmVsLmxlbmd0aCApIHtcclxuXHRcdFx0XHQvLyByZWZyZXNoIGVycm9yL3N1Y2Nlc3MgY2xhc3NcclxuXHRcdFx0XHRsYWJlbC5yZW1vdmVDbGFzcyggdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICkuYWRkQ2xhc3MoIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcyApO1xyXG5cdFx0XHRcdC8vIHJlcGxhY2UgbWVzc2FnZSBvbiBleGlzdGluZyBsYWJlbFxyXG5cdFx0XHRcdGxhYmVsLmh0bWwobWVzc2FnZSk7XHJcblx0XHRcdH0gZWxzZSB7XHJcblx0XHRcdFx0Ly8gY3JlYXRlIGxhYmVsXHJcblx0XHRcdFx0bGFiZWwgPSAkKFwiPFwiICsgdGhpcy5zZXR0aW5ncy5lcnJvckVsZW1lbnQgKyBcIj5cIilcclxuXHRcdFx0XHRcdC5hdHRyKFwiZm9yXCIsIHRoaXMuaWRPck5hbWUoZWxlbWVudCkpXHJcblx0XHRcdFx0XHQuYWRkQ2xhc3ModGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzKVxyXG5cdFx0XHRcdFx0Lmh0bWwobWVzc2FnZSB8fCBcIlwiKTtcclxuXHRcdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3Mud3JhcHBlciApIHtcclxuXHRcdFx0XHRcdC8vIG1ha2Ugc3VyZSB0aGUgZWxlbWVudCBpcyB2aXNpYmxlLCBldmVuIGluIElFXHJcblx0XHRcdFx0XHQvLyBhY3R1YWxseSBzaG93aW5nIHRoZSB3cmFwcGVkIGVsZW1lbnQgaXMgaGFuZGxlZCBlbHNld2hlcmVcclxuXHRcdFx0XHRcdGxhYmVsID0gbGFiZWwuaGlkZSgpLnNob3coKS53cmFwKFwiPFwiICsgdGhpcy5zZXR0aW5ncy53cmFwcGVyICsgXCIvPlwiKS5wYXJlbnQoKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0aWYgKCAhdGhpcy5sYWJlbENvbnRhaW5lci5hcHBlbmQobGFiZWwpLmxlbmd0aCApIHtcclxuXHRcdFx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5lcnJvclBsYWNlbWVudCApIHtcclxuXHRcdFx0XHRcdFx0dGhpcy5zZXR0aW5ncy5lcnJvclBsYWNlbWVudChsYWJlbCwgJChlbGVtZW50KSApO1xyXG5cdFx0XHRcdFx0fSBlbHNlIHtcclxuXHRcdFx0XHRcdFx0bGFiZWwuaW5zZXJ0QWZ0ZXIoZWxlbWVudCk7XHJcblx0XHRcdFx0XHR9XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdGlmICggIW1lc3NhZ2UgJiYgdGhpcy5zZXR0aW5ncy5zdWNjZXNzICkge1xyXG5cdFx0XHRcdGxhYmVsLnRleHQoXCJcIik7XHJcblx0XHRcdFx0aWYgKCB0eXBlb2YgdGhpcy5zZXR0aW5ncy5zdWNjZXNzID09PSBcInN0cmluZ1wiICkge1xyXG5cdFx0XHRcdFx0bGFiZWwuYWRkQ2xhc3MoIHRoaXMuc2V0dGluZ3Muc3VjY2VzcyApO1xyXG5cdFx0XHRcdH0gZWxzZSB7XHJcblx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLnN1Y2Nlc3MoIGxhYmVsLCBlbGVtZW50ICk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdHRoaXMudG9TaG93ID0gdGhpcy50b1Nob3cuYWRkKGxhYmVsKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0ZXJyb3JzRm9yOiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdFx0dmFyIG5hbWUgPSB0aGlzLmlkT3JOYW1lKGVsZW1lbnQpO1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5lcnJvcnMoKS5maWx0ZXIoZnVuY3Rpb24oKSB7XHJcblx0XHRcdFx0cmV0dXJuICQodGhpcykuYXR0cihcImZvclwiKSA9PT0gbmFtZTtcclxuXHRcdFx0fSk7XHJcblx0XHR9LFxyXG5cclxuXHRcdGlkT3JOYW1lOiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdFx0cmV0dXJuIHRoaXMuZ3JvdXBzW2VsZW1lbnQubmFtZV0gfHwgKHRoaXMuY2hlY2thYmxlKGVsZW1lbnQpID8gZWxlbWVudC5uYW1lIDogZWxlbWVudC5pZCB8fCBlbGVtZW50Lm5hbWUpO1xyXG5cdFx0fSxcclxuXHJcblx0XHR2YWxpZGF0aW9uVGFyZ2V0Rm9yOiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdFx0Ly8gaWYgcmFkaW8vY2hlY2tib3gsIHZhbGlkYXRlIGZpcnN0IGVsZW1lbnQgaW4gZ3JvdXAgaW5zdGVhZFxyXG5cdFx0XHRpZiAoIHRoaXMuY2hlY2thYmxlKGVsZW1lbnQpICkge1xyXG5cdFx0XHRcdGVsZW1lbnQgPSB0aGlzLmZpbmRCeU5hbWUoIGVsZW1lbnQubmFtZSApLm5vdCh0aGlzLnNldHRpbmdzLmlnbm9yZSlbMF07XHJcblx0XHRcdH1cclxuXHRcdFx0cmV0dXJuIGVsZW1lbnQ7XHJcblx0XHR9LFxyXG5cclxuXHRcdGNoZWNrYWJsZTogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XHJcblx0XHRcdHJldHVybiAoL3JhZGlvfGNoZWNrYm94L2kpLnRlc3QoZWxlbWVudC50eXBlKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0ZmluZEJ5TmFtZTogZnVuY3Rpb24oIG5hbWUgKSB7XHJcblx0XHRcdHJldHVybiAkKHRoaXMuY3VycmVudEZvcm0pLmZpbmQoXCJbbmFtZT0nXCIgKyBuYW1lICsgXCInXVwiKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Z2V0TGVuZ3RoOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XHJcblx0XHRcdHN3aXRjaCggZWxlbWVudC5ub2RlTmFtZS50b0xvd2VyQ2FzZSgpICkge1xyXG5cdFx0XHRjYXNlIFwic2VsZWN0XCI6XHJcblx0XHRcdFx0cmV0dXJuICQoXCJvcHRpb246c2VsZWN0ZWRcIiwgZWxlbWVudCkubGVuZ3RoO1xyXG5cdFx0XHRjYXNlIFwiaW5wdXRcIjpcclxuXHRcdFx0XHRpZiAoIHRoaXMuY2hlY2thYmxlKCBlbGVtZW50KSApIHtcclxuXHRcdFx0XHRcdHJldHVybiB0aGlzLmZpbmRCeU5hbWUoZWxlbWVudC5uYW1lKS5maWx0ZXIoXCI6Y2hlY2tlZFwiKS5sZW5ndGg7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHRcdHJldHVybiB2YWx1ZS5sZW5ndGg7XHJcblx0XHR9LFxyXG5cclxuXHRcdGRlcGVuZDogZnVuY3Rpb24oIHBhcmFtLCBlbGVtZW50ICkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5kZXBlbmRUeXBlc1t0eXBlb2YgcGFyYW1dID8gdGhpcy5kZXBlbmRUeXBlc1t0eXBlb2YgcGFyYW1dKHBhcmFtLCBlbGVtZW50KSA6IHRydWU7XHJcblx0XHR9LFxyXG5cclxuXHRcdGRlcGVuZFR5cGVzOiB7XHJcblx0XHRcdFwiYm9vbGVhblwiOiBmdW5jdGlvbiggcGFyYW0sIGVsZW1lbnQgKSB7XHJcblx0XHRcdFx0cmV0dXJuIHBhcmFtO1xyXG5cdFx0XHR9LFxyXG5cdFx0XHRcInN0cmluZ1wiOiBmdW5jdGlvbiggcGFyYW0sIGVsZW1lbnQgKSB7XHJcblx0XHRcdFx0cmV0dXJuICEhJChwYXJhbSwgZWxlbWVudC5mb3JtKS5sZW5ndGg7XHJcblx0XHRcdH0sXHJcblx0XHRcdFwiZnVuY3Rpb25cIjogZnVuY3Rpb24oIHBhcmFtLCBlbGVtZW50ICkge1xyXG5cdFx0XHRcdHJldHVybiBwYXJhbShlbGVtZW50KTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHJcblx0XHRvcHRpb25hbDogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XHJcblx0XHRcdHZhciB2YWwgPSB0aGlzLmVsZW1lbnRWYWx1ZShlbGVtZW50KTtcclxuXHRcdFx0cmV0dXJuICEkLnZhbGlkYXRvci5tZXRob2RzLnJlcXVpcmVkLmNhbGwodGhpcywgdmFsLCBlbGVtZW50KSAmJiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcclxuXHRcdH0sXHJcblxyXG5cdFx0c3RhcnRSZXF1ZXN0OiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdFx0aWYgKCAhdGhpcy5wZW5kaW5nW2VsZW1lbnQubmFtZV0gKSB7XHJcblx0XHRcdFx0dGhpcy5wZW5kaW5nUmVxdWVzdCsrO1xyXG5cdFx0XHRcdHRoaXMucGVuZGluZ1tlbGVtZW50Lm5hbWVdID0gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0fSxcclxuXHJcblx0XHRzdG9wUmVxdWVzdDogZnVuY3Rpb24oIGVsZW1lbnQsIHZhbGlkICkge1xyXG5cdFx0XHR0aGlzLnBlbmRpbmdSZXF1ZXN0LS07XHJcblx0XHRcdC8vIHNvbWV0aW1lcyBzeW5jaHJvbml6YXRpb24gZmFpbHMsIG1ha2Ugc3VyZSBwZW5kaW5nUmVxdWVzdCBpcyBuZXZlciA8IDBcclxuXHRcdFx0aWYgKCB0aGlzLnBlbmRpbmdSZXF1ZXN0IDwgMCApIHtcclxuXHRcdFx0XHR0aGlzLnBlbmRpbmdSZXF1ZXN0ID0gMDtcclxuXHRcdFx0fVxyXG5cdFx0XHRkZWxldGUgdGhpcy5wZW5kaW5nW2VsZW1lbnQubmFtZV07XHJcblx0XHRcdGlmICggdmFsaWQgJiYgdGhpcy5wZW5kaW5nUmVxdWVzdCA9PT0gMCAmJiB0aGlzLmZvcm1TdWJtaXR0ZWQgJiYgdGhpcy5mb3JtKCkgKSB7XHJcblx0XHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKS5zdWJtaXQoKTtcclxuXHRcdFx0XHR0aGlzLmZvcm1TdWJtaXR0ZWQgPSBmYWxzZTtcclxuXHRcdFx0fSBlbHNlIGlmICghdmFsaWQgJiYgdGhpcy5wZW5kaW5nUmVxdWVzdCA9PT0gMCAmJiB0aGlzLmZvcm1TdWJtaXR0ZWQpIHtcclxuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLnRyaWdnZXJIYW5kbGVyKFwiaW52YWxpZC1mb3JtXCIsIFt0aGlzXSk7XHJcblx0XHRcdFx0dGhpcy5mb3JtU3VibWl0dGVkID0gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdH0sXHJcblxyXG5cdFx0cHJldmlvdXNWYWx1ZTogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XHJcblx0XHRcdHJldHVybiAkLmRhdGEoZWxlbWVudCwgXCJwcmV2aW91c1ZhbHVlXCIpIHx8ICQuZGF0YShlbGVtZW50LCBcInByZXZpb3VzVmFsdWVcIiwge1xyXG5cdFx0XHRcdG9sZDogbnVsbCxcclxuXHRcdFx0XHR2YWxpZDogdHJ1ZSxcclxuXHRcdFx0XHRtZXNzYWdlOiB0aGlzLmRlZmF1bHRNZXNzYWdlKCBlbGVtZW50LCBcInJlbW90ZVwiIClcclxuXHRcdFx0fSk7XHJcblx0XHR9XHJcblxyXG5cdH0sXHJcblxyXG5cdGNsYXNzUnVsZVNldHRpbmdzOiB7XHJcblx0XHRyZXF1aXJlZDoge3JlcXVpcmVkOiB0cnVlfSxcclxuXHRcdGVtYWlsOiB7ZW1haWw6IHRydWV9LFxyXG5cdFx0dXJsOiB7dXJsOiB0cnVlfSxcclxuXHRcdGRhdGU6IHtkYXRlOiB0cnVlfSxcclxuXHRcdGRhdGVJU086IHtkYXRlSVNPOiB0cnVlfSxcclxuXHRcdG51bWJlcjoge251bWJlcjogdHJ1ZX0sXHJcblx0XHRkaWdpdHM6IHtkaWdpdHM6IHRydWV9LFxyXG5cdFx0Y3JlZGl0Y2FyZDoge2NyZWRpdGNhcmQ6IHRydWV9XHJcblx0fSxcclxuXHJcblx0YWRkQ2xhc3NSdWxlczogZnVuY3Rpb24oIGNsYXNzTmFtZSwgcnVsZXMgKSB7XHJcblx0XHRpZiAoIGNsYXNzTmFtZS5jb25zdHJ1Y3RvciA9PT0gU3RyaW5nICkge1xyXG5cdFx0XHR0aGlzLmNsYXNzUnVsZVNldHRpbmdzW2NsYXNzTmFtZV0gPSBydWxlcztcclxuXHRcdH0gZWxzZSB7XHJcblx0XHRcdCQuZXh0ZW5kKHRoaXMuY2xhc3NSdWxlU2V0dGluZ3MsIGNsYXNzTmFtZSk7XHJcblx0XHR9XHJcblx0fSxcclxuXHJcblx0Y2xhc3NSdWxlczogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XHJcblx0XHR2YXIgcnVsZXMgPSB7fTtcclxuXHRcdHZhciBjbGFzc2VzID0gJChlbGVtZW50KS5hdHRyKFwiY2xhc3NcIik7XHJcblx0XHRpZiAoIGNsYXNzZXMgKSB7XHJcblx0XHRcdCQuZWFjaChjbGFzc2VzLnNwbGl0KFwiIFwiKSwgZnVuY3Rpb24oKSB7XHJcblx0XHRcdFx0aWYgKCB0aGlzIGluICQudmFsaWRhdG9yLmNsYXNzUnVsZVNldHRpbmdzICkge1xyXG5cdFx0XHRcdFx0JC5leHRlbmQocnVsZXMsICQudmFsaWRhdG9yLmNsYXNzUnVsZVNldHRpbmdzW3RoaXNdKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cdFx0fVxyXG5cdFx0cmV0dXJuIHJ1bGVzO1xyXG5cdH0sXHJcblxyXG5cdGF0dHJpYnV0ZVJ1bGVzOiBmdW5jdGlvbiggZWxlbWVudCApIHtcclxuXHRcdHZhciBydWxlcyA9IHt9O1xyXG5cdFx0dmFyICRlbGVtZW50ID0gJChlbGVtZW50KTtcclxuXHRcdHZhciB0eXBlID0gJGVsZW1lbnRbMF0uZ2V0QXR0cmlidXRlKFwidHlwZVwiKTtcclxuXHJcblx0XHRmb3IgKHZhciBtZXRob2QgaW4gJC52YWxpZGF0b3IubWV0aG9kcykge1xyXG5cdFx0XHR2YXIgdmFsdWU7XHJcblxyXG5cdFx0XHQvLyBzdXBwb3J0IGZvciA8aW5wdXQgcmVxdWlyZWQ+IGluIGJvdGggaHRtbDUgYW5kIG9sZGVyIGJyb3dzZXJzXHJcblx0XHRcdGlmICggbWV0aG9kID09PSBcInJlcXVpcmVkXCIgKSB7XHJcblx0XHRcdFx0dmFsdWUgPSAkZWxlbWVudC5nZXQoMCkuZ2V0QXR0cmlidXRlKG1ldGhvZCk7XHJcblx0XHRcdFx0Ly8gU29tZSBicm93c2VycyByZXR1cm4gYW4gZW1wdHkgc3RyaW5nIGZvciB0aGUgcmVxdWlyZWQgYXR0cmlidXRlXHJcblx0XHRcdFx0Ly8gYW5kIG5vbi1IVE1MNSBicm93c2VycyBtaWdodCBoYXZlIHJlcXVpcmVkPVwiXCIgbWFya3VwXHJcblx0XHRcdFx0aWYgKCB2YWx1ZSA9PT0gXCJcIiApIHtcclxuXHRcdFx0XHRcdHZhbHVlID0gdHJ1ZTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0Ly8gZm9yY2Ugbm9uLUhUTUw1IGJyb3dzZXJzIHRvIHJldHVybiBib29sXHJcblx0XHRcdFx0dmFsdWUgPSAhIXZhbHVlO1xyXG5cdFx0XHR9IGVsc2Uge1xyXG5cdFx0XHRcdHZhbHVlID0gJGVsZW1lbnQuYXR0cihtZXRob2QpO1xyXG5cdFx0XHR9XHJcblxyXG5cdFx0XHQvLyBjb252ZXJ0IHRoZSB2YWx1ZSB0byBhIG51bWJlciBmb3IgbnVtYmVyIGlucHV0cywgYW5kIGZvciB0ZXh0IGZvciBiYWNrd2FyZHMgY29tcGFiaWxpdHlcclxuXHRcdFx0Ly8gYWxsb3dzIHR5cGU9XCJkYXRlXCIgYW5kIG90aGVycyB0byBiZSBjb21wYXJlZCBhcyBzdHJpbmdzXHJcblx0XHRcdGlmICggL21pbnxtYXgvLnRlc3QoIG1ldGhvZCApICYmICggdHlwZSA9PT0gbnVsbCB8fCAvbnVtYmVyfHJhbmdlfHRleHQvLnRlc3QoIHR5cGUgKSApICkge1xyXG5cdFx0XHRcdHZhbHVlID0gTnVtYmVyKHZhbHVlKTtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0aWYgKCB2YWx1ZSApIHtcclxuXHRcdFx0XHRydWxlc1ttZXRob2RdID0gdmFsdWU7XHJcblx0XHRcdH0gZWxzZSBpZiAoIHR5cGUgPT09IG1ldGhvZCAmJiB0eXBlICE9PSAncmFuZ2UnICkge1xyXG5cdFx0XHRcdC8vIGV4Y2VwdGlvbjogdGhlIGpxdWVyeSB2YWxpZGF0ZSAncmFuZ2UnIG1ldGhvZFxyXG5cdFx0XHRcdC8vIGRvZXMgbm90IHRlc3QgZm9yIHRoZSBodG1sNSAncmFuZ2UnIHR5cGVcclxuXHRcdFx0XHRydWxlc1ttZXRob2RdID0gdHJ1ZTtcclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cclxuXHRcdC8vIG1heGxlbmd0aCBtYXkgYmUgcmV0dXJuZWQgYXMgLTEsIDIxNDc0ODM2NDcgKElFKSBhbmQgNTI0Mjg4IChzYWZhcmkpIGZvciB0ZXh0IGlucHV0c1xyXG5cdFx0aWYgKCBydWxlcy5tYXhsZW5ndGggJiYgLy0xfDIxNDc0ODM2NDd8NTI0Mjg4Ly50ZXN0KHJ1bGVzLm1heGxlbmd0aCkgKSB7XHJcblx0XHRcdGRlbGV0ZSBydWxlcy5tYXhsZW5ndGg7XHJcblx0XHR9XHJcblxyXG5cdFx0cmV0dXJuIHJ1bGVzO1xyXG5cdH0sXHJcblxyXG5cdGRhdGFSdWxlczogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XHJcblx0XHR2YXIgbWV0aG9kLCB2YWx1ZSxcclxuXHRcdFx0cnVsZXMgPSB7fSwgJGVsZW1lbnQgPSAkKGVsZW1lbnQpO1xyXG5cdFx0Zm9yIChtZXRob2QgaW4gJC52YWxpZGF0b3IubWV0aG9kcykge1xyXG5cdFx0XHR2YWx1ZSA9ICRlbGVtZW50LmRhdGEoXCJydWxlLVwiICsgbWV0aG9kLnRvTG93ZXJDYXNlKCkpO1xyXG5cdFx0XHRpZiAoIHZhbHVlICE9PSB1bmRlZmluZWQgKSB7XHJcblx0XHRcdFx0cnVsZXNbbWV0aG9kXSA9IHZhbHVlO1xyXG5cdFx0XHR9XHJcblx0XHR9XHJcblx0XHRyZXR1cm4gcnVsZXM7XHJcblx0fSxcclxuXHJcblx0c3RhdGljUnVsZXM6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xyXG5cdFx0dmFyIHJ1bGVzID0ge307XHJcblx0XHR2YXIgdmFsaWRhdG9yID0gJC5kYXRhKGVsZW1lbnQuZm9ybSwgXCJ2YWxpZGF0b3JcIik7XHJcblx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5ncy5ydWxlcyApIHtcclxuXHRcdFx0cnVsZXMgPSAkLnZhbGlkYXRvci5ub3JtYWxpemVSdWxlKHZhbGlkYXRvci5zZXR0aW5ncy5ydWxlc1tlbGVtZW50Lm5hbWVdKSB8fCB7fTtcclxuXHRcdH1cclxuXHRcdHJldHVybiBydWxlcztcclxuXHR9LFxyXG5cclxuXHRub3JtYWxpemVSdWxlczogZnVuY3Rpb24oIHJ1bGVzLCBlbGVtZW50ICkge1xyXG5cdFx0Ly8gaGFuZGxlIGRlcGVuZGVuY3kgY2hlY2tcclxuXHRcdCQuZWFjaChydWxlcywgZnVuY3Rpb24oIHByb3AsIHZhbCApIHtcclxuXHRcdFx0Ly8gaWdub3JlIHJ1bGUgd2hlbiBwYXJhbSBpcyBleHBsaWNpdGx5IGZhbHNlLCBlZy4gcmVxdWlyZWQ6ZmFsc2VcclxuXHRcdFx0aWYgKCB2YWwgPT09IGZhbHNlICkge1xyXG5cdFx0XHRcdGRlbGV0ZSBydWxlc1twcm9wXTtcclxuXHRcdFx0XHRyZXR1cm47XHJcblx0XHRcdH1cclxuXHRcdFx0aWYgKCB2YWwucGFyYW0gfHwgdmFsLmRlcGVuZHMgKSB7XHJcblx0XHRcdFx0dmFyIGtlZXBSdWxlID0gdHJ1ZTtcclxuXHRcdFx0XHRzd2l0Y2ggKHR5cGVvZiB2YWwuZGVwZW5kcykge1xyXG5cdFx0XHRcdGNhc2UgXCJzdHJpbmdcIjpcclxuXHRcdFx0XHRcdGtlZXBSdWxlID0gISEkKHZhbC5kZXBlbmRzLCBlbGVtZW50LmZvcm0pLmxlbmd0aDtcclxuXHRcdFx0XHRcdGJyZWFrO1xyXG5cdFx0XHRcdGNhc2UgXCJmdW5jdGlvblwiOlxyXG5cdFx0XHRcdFx0a2VlcFJ1bGUgPSB2YWwuZGVwZW5kcy5jYWxsKGVsZW1lbnQsIGVsZW1lbnQpO1xyXG5cdFx0XHRcdFx0YnJlYWs7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdGlmICgga2VlcFJ1bGUgKSB7XHJcblx0XHRcdFx0XHRydWxlc1twcm9wXSA9IHZhbC5wYXJhbSAhPT0gdW5kZWZpbmVkID8gdmFsLnBhcmFtIDogdHJ1ZTtcclxuXHRcdFx0XHR9IGVsc2Uge1xyXG5cdFx0XHRcdFx0ZGVsZXRlIHJ1bGVzW3Byb3BdO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0fVxyXG5cdFx0fSk7XHJcblxyXG5cdFx0Ly8gZXZhbHVhdGUgcGFyYW1ldGVyc1xyXG5cdFx0JC5lYWNoKHJ1bGVzLCBmdW5jdGlvbiggcnVsZSwgcGFyYW1ldGVyICkge1xyXG5cdFx0XHRydWxlc1tydWxlXSA9ICQuaXNGdW5jdGlvbihwYXJhbWV0ZXIpID8gcGFyYW1ldGVyKGVsZW1lbnQpIDogcGFyYW1ldGVyO1xyXG5cdFx0fSk7XHJcblxyXG5cdFx0Ly8gY2xlYW4gbnVtYmVyIHBhcmFtZXRlcnNcclxuXHRcdCQuZWFjaChbJ21pbmxlbmd0aCcsICdtYXhsZW5ndGgnXSwgZnVuY3Rpb24oKSB7XHJcblx0XHRcdGlmICggcnVsZXNbdGhpc10gKSB7XHJcblx0XHRcdFx0cnVsZXNbdGhpc10gPSBOdW1iZXIocnVsZXNbdGhpc10pO1xyXG5cdFx0XHR9XHJcblx0XHR9KTtcclxuXHRcdCQuZWFjaChbJ3JhbmdlbGVuZ3RoJywgJ3JhbmdlJ10sIGZ1bmN0aW9uKCkge1xyXG5cdFx0XHR2YXIgcGFydHM7XHJcblx0XHRcdGlmICggcnVsZXNbdGhpc10gKSB7XHJcblx0XHRcdFx0aWYgKCAkLmlzQXJyYXkocnVsZXNbdGhpc10pICkge1xyXG5cdFx0XHRcdFx0cnVsZXNbdGhpc10gPSBbTnVtYmVyKHJ1bGVzW3RoaXNdWzBdKSwgTnVtYmVyKHJ1bGVzW3RoaXNdWzFdKV07XHJcblx0XHRcdFx0fSBlbHNlIGlmICggdHlwZW9mIHJ1bGVzW3RoaXNdID09PSBcInN0cmluZ1wiICkge1xyXG5cdFx0XHRcdFx0cGFydHMgPSBydWxlc1t0aGlzXS5zcGxpdCgvW1xccyxdKy8pO1xyXG5cdFx0XHRcdFx0cnVsZXNbdGhpc10gPSBbTnVtYmVyKHBhcnRzWzBdKSwgTnVtYmVyKHBhcnRzWzFdKV07XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9XHJcblx0XHR9KTtcclxuXHJcblx0XHRpZiAoICQudmFsaWRhdG9yLmF1dG9DcmVhdGVSYW5nZXMgKSB7XHJcblx0XHRcdC8vIGF1dG8tY3JlYXRlIHJhbmdlc1xyXG5cdFx0XHRpZiAoIHJ1bGVzLm1pbiAmJiBydWxlcy5tYXggKSB7XHJcblx0XHRcdFx0cnVsZXMucmFuZ2UgPSBbcnVsZXMubWluLCBydWxlcy5tYXhdO1xyXG5cdFx0XHRcdGRlbGV0ZSBydWxlcy5taW47XHJcblx0XHRcdFx0ZGVsZXRlIHJ1bGVzLm1heDtcclxuXHRcdFx0fVxyXG5cdFx0XHRpZiAoIHJ1bGVzLm1pbmxlbmd0aCAmJiBydWxlcy5tYXhsZW5ndGggKSB7XHJcblx0XHRcdFx0cnVsZXMucmFuZ2VsZW5ndGggPSBbcnVsZXMubWlubGVuZ3RoLCBydWxlcy5tYXhsZW5ndGhdO1xyXG5cdFx0XHRcdGRlbGV0ZSBydWxlcy5taW5sZW5ndGg7XHJcblx0XHRcdFx0ZGVsZXRlIHJ1bGVzLm1heGxlbmd0aDtcclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cclxuXHRcdHJldHVybiBydWxlcztcclxuXHR9LFxyXG5cclxuXHQvLyBDb252ZXJ0cyBhIHNpbXBsZSBzdHJpbmcgdG8gYSB7c3RyaW5nOiB0cnVlfSBydWxlLCBlLmcuLCBcInJlcXVpcmVkXCIgdG8ge3JlcXVpcmVkOnRydWV9XHJcblx0bm9ybWFsaXplUnVsZTogZnVuY3Rpb24oIGRhdGEgKSB7XHJcblx0XHRpZiAoIHR5cGVvZiBkYXRhID09PSBcInN0cmluZ1wiICkge1xyXG5cdFx0XHR2YXIgdHJhbnNmb3JtZWQgPSB7fTtcclxuXHRcdFx0JC5lYWNoKGRhdGEuc3BsaXQoL1xccy8pLCBmdW5jdGlvbigpIHtcclxuXHRcdFx0XHR0cmFuc2Zvcm1lZFt0aGlzXSA9IHRydWU7XHJcblx0XHRcdH0pO1xyXG5cdFx0XHRkYXRhID0gdHJhbnNmb3JtZWQ7XHJcblx0XHR9XHJcblx0XHRyZXR1cm4gZGF0YTtcclxuXHR9LFxyXG5cclxuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3IvYWRkTWV0aG9kXHJcblx0YWRkTWV0aG9kOiBmdW5jdGlvbiggbmFtZSwgbWV0aG9kLCBtZXNzYWdlICkge1xyXG5cdFx0JC52YWxpZGF0b3IubWV0aG9kc1tuYW1lXSA9IG1ldGhvZDtcclxuXHRcdCQudmFsaWRhdG9yLm1lc3NhZ2VzW25hbWVdID0gbWVzc2FnZSAhPT0gdW5kZWZpbmVkID8gbWVzc2FnZSA6ICQudmFsaWRhdG9yLm1lc3NhZ2VzW25hbWVdO1xyXG5cdFx0aWYgKCBtZXRob2QubGVuZ3RoIDwgMyApIHtcclxuXHRcdFx0JC52YWxpZGF0b3IuYWRkQ2xhc3NSdWxlcyhuYW1lLCAkLnZhbGlkYXRvci5ub3JtYWxpemVSdWxlKG5hbWUpKTtcclxuXHRcdH1cclxuXHR9LFxyXG5cclxuXHRtZXRob2RzOiB7XHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yZXF1aXJlZFxyXG5cdFx0cmVxdWlyZWQ6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XHJcblx0XHRcdC8vIGNoZWNrIGlmIGRlcGVuZGVuY3kgaXMgbWV0XHJcblx0XHRcdGlmICggIXRoaXMuZGVwZW5kKHBhcmFtLCBlbGVtZW50KSApIHtcclxuXHRcdFx0XHRyZXR1cm4gXCJkZXBlbmRlbmN5LW1pc21hdGNoXCI7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYgKCBlbGVtZW50Lm5vZGVOYW1lLnRvTG93ZXJDYXNlKCkgPT09IFwic2VsZWN0XCIgKSB7XHJcblx0XHRcdFx0Ly8gY291bGQgYmUgYW4gYXJyYXkgZm9yIHNlbGVjdC1tdWx0aXBsZSBvciBhIHN0cmluZywgYm90aCBhcmUgZmluZSB0aGlzIHdheVxyXG5cdFx0XHRcdHZhciB2YWwgPSAkKGVsZW1lbnQpLnZhbCgpO1xyXG5cdFx0XHRcdHJldHVybiB2YWwgJiYgdmFsLmxlbmd0aCA+IDA7XHJcblx0XHRcdH1cclxuXHRcdFx0aWYgKCB0aGlzLmNoZWNrYWJsZShlbGVtZW50KSApIHtcclxuXHRcdFx0XHRyZXR1cm4gdGhpcy5nZXRMZW5ndGgodmFsdWUsIGVsZW1lbnQpID4gMDtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gJC50cmltKHZhbHVlKS5sZW5ndGggPiAwO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2VtYWlsXHJcblx0XHRlbWFpbDogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xyXG5cdFx0XHQvLyBjb250cmlidXRlZCBieSBTY290dCBHb256YWxlejogaHR0cDovL3Byb2plY3RzLnNjb3R0c3BsYXlncm91bmQuY29tL2VtYWlsX2FkZHJlc3NfdmFsaWRhdGlvbi9cclxuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgL14oKChbYS16XXxcXGR8WyEjXFwkJSYnXFwqXFwrXFwtXFwvPVxcP1xcXl9ge1xcfH1+XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkrKFxcLihbYS16XXxcXGR8WyEjXFwkJSYnXFwqXFwrXFwtXFwvPVxcP1xcXl9ge1xcfH1+XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkrKSopfCgoXFx4MjIpKCgoKFxceDIwfFxceDA5KSooXFx4MGRcXHgwYSkpPyhcXHgyMHxcXHgwOSkrKT8oKFtcXHgwMS1cXHgwOFxceDBiXFx4MGNcXHgwZS1cXHgxZlxceDdmXXxcXHgyMXxbXFx4MjMtXFx4NWJdfFtcXHg1ZC1cXHg3ZV18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfChcXFxcKFtcXHgwMS1cXHgwOVxceDBiXFx4MGNcXHgwZC1cXHg3Zl18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKSkpKigoKFxceDIwfFxceDA5KSooXFx4MGRcXHgwYSkpPyhcXHgyMHxcXHgwOSkrKT8oXFx4MjIpKSlAKCgoW2Etel18XFxkfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkqKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkpKVxcLikrKChbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KChbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkqKFthLXpdfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSkpJC9pLnRlc3QodmFsdWUpO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL3VybFxyXG5cdFx0dXJsOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XHJcblx0XHRcdC8vIGNvbnRyaWJ1dGVkIGJ5IFNjb3R0IEdvbnphbGV6OiBodHRwOi8vcHJvamVjdHMuc2NvdHRzcGxheWdyb3VuZC5jb20vaXJpL1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCAvXihodHRwcz98cz9mdHApOlxcL1xcLygoKChbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoJVtcXGRhLWZdezJ9KXxbIVxcJCYnXFwoXFwpXFwqXFwrLDs9XXw6KSpAKT8oKChcXGR8WzEtOV1cXGR8MVxcZFxcZHwyWzAtNF1cXGR8MjVbMC01XSlcXC4oXFxkfFsxLTldXFxkfDFcXGRcXGR8MlswLTRdXFxkfDI1WzAtNV0pXFwuKFxcZHxbMS05XVxcZHwxXFxkXFxkfDJbMC00XVxcZHwyNVswLTVdKVxcLihcXGR8WzEtOV1cXGR8MVxcZFxcZHwyWzAtNF1cXGR8MjVbMC01XSkpfCgoKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KChbYS16XXxcXGR8W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKihbYS16XXxcXGR8W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKSlcXC4pKygoW2Etel18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfCgoW2Etel18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKihbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkpKVxcLj8pKDpcXGQqKT8pKFxcLygoKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfCglW1xcZGEtZl17Mn0pfFshXFwkJidcXChcXClcXCpcXCssOz1dfDp8QCkrKFxcLygoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKSopKik/KT8oXFw/KCgoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKXxbXFx1RTAwMC1cXHVGOEZGXXxcXC98XFw/KSopPygjKCgoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKXxcXC98XFw/KSopPyQvaS50ZXN0KHZhbHVlKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9kYXRlXHJcblx0XHRkYXRlOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XHJcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8ICEvSW52YWxpZHxOYU4vLnRlc3QobmV3IERhdGUodmFsdWUpLnRvU3RyaW5nKCkpO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2RhdGVJU09cclxuXHRcdGRhdGVJU086IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcclxuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgL15cXGR7NH1bXFwvXFwtXVxcZHsxLDJ9W1xcL1xcLV1cXGR7MSwyfSQvLnRlc3QodmFsdWUpO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL251bWJlclxyXG5cdFx0bnVtYmVyOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XHJcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IC9eLT8oPzpcXGQrfFxcZHsxLDN9KD86LFxcZHszfSkrKT8oPzpcXC5cXGQrKT8kLy50ZXN0KHZhbHVlKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9kaWdpdHNcclxuXHRcdGRpZ2l0czogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCAvXlxcZCskLy50ZXN0KHZhbHVlKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9jcmVkaXRjYXJkXHJcblx0XHQvLyBiYXNlZCBvbiBodHRwOi8vZW4ud2lraXBlZGlhLm9yZy93aWtpL0x1aG5cclxuXHRcdGNyZWRpdGNhcmQ6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcclxuXHRcdFx0aWYgKCB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpICkge1xyXG5cdFx0XHRcdHJldHVybiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcclxuXHRcdFx0fVxyXG5cdFx0XHQvLyBhY2NlcHQgb25seSBzcGFjZXMsIGRpZ2l0cyBhbmQgZGFzaGVzXHJcblx0XHRcdGlmICggL1teMC05IFxcLV0rLy50ZXN0KHZhbHVlKSApIHtcclxuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XHJcblx0XHRcdH1cclxuXHRcdFx0dmFyIG5DaGVjayA9IDAsXHJcblx0XHRcdFx0bkRpZ2l0ID0gMCxcclxuXHRcdFx0XHRiRXZlbiA9IGZhbHNlO1xyXG5cclxuXHRcdFx0dmFsdWUgPSB2YWx1ZS5yZXBsYWNlKC9cXEQvZywgXCJcIik7XHJcblxyXG5cdFx0XHRmb3IgKHZhciBuID0gdmFsdWUubGVuZ3RoIC0gMTsgbiA+PSAwOyBuLS0pIHtcclxuXHRcdFx0XHR2YXIgY0RpZ2l0ID0gdmFsdWUuY2hhckF0KG4pO1xyXG5cdFx0XHRcdG5EaWdpdCA9IHBhcnNlSW50KGNEaWdpdCwgMTApO1xyXG5cdFx0XHRcdGlmICggYkV2ZW4gKSB7XHJcblx0XHRcdFx0XHRpZiAoIChuRGlnaXQgKj0gMikgPiA5ICkge1xyXG5cdFx0XHRcdFx0XHRuRGlnaXQgLT0gOTtcclxuXHRcdFx0XHRcdH1cclxuXHRcdFx0XHR9XHJcblx0XHRcdFx0bkNoZWNrICs9IG5EaWdpdDtcclxuXHRcdFx0XHRiRXZlbiA9ICFiRXZlbjtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0cmV0dXJuIChuQ2hlY2sgJSAxMCkgPT09IDA7XHJcblx0XHR9LFxyXG5cclxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvbWlubGVuZ3RoXHJcblx0XHRtaW5sZW5ndGg6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XHJcblx0XHRcdHZhciBsZW5ndGggPSAkLmlzQXJyYXkoIHZhbHVlICkgPyB2YWx1ZS5sZW5ndGggOiB0aGlzLmdldExlbmd0aCgkLnRyaW0odmFsdWUpLCBlbGVtZW50KTtcclxuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgbGVuZ3RoID49IHBhcmFtO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL21heGxlbmd0aFxyXG5cdFx0bWF4bGVuZ3RoOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xyXG5cdFx0XHR2YXIgbGVuZ3RoID0gJC5pc0FycmF5KCB2YWx1ZSApID8gdmFsdWUubGVuZ3RoIDogdGhpcy5nZXRMZW5ndGgoJC50cmltKHZhbHVlKSwgZWxlbWVudCk7XHJcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IGxlbmd0aCA8PSBwYXJhbTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yYW5nZWxlbmd0aFxyXG5cdFx0cmFuZ2VsZW5ndGg6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XHJcblx0XHRcdHZhciBsZW5ndGggPSAkLmlzQXJyYXkoIHZhbHVlICkgPyB2YWx1ZS5sZW5ndGggOiB0aGlzLmdldExlbmd0aCgkLnRyaW0odmFsdWUpLCBlbGVtZW50KTtcclxuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgKCBsZW5ndGggPj0gcGFyYW1bMF0gJiYgbGVuZ3RoIDw9IHBhcmFtWzFdICk7XHJcblx0XHR9LFxyXG5cclxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvbWluXHJcblx0XHRtaW46IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XHJcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IHZhbHVlID49IHBhcmFtO1xyXG5cdFx0fSxcclxuXHJcblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL21heFxyXG5cdFx0bWF4OiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCB2YWx1ZSA8PSBwYXJhbTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yYW5nZVxyXG5cdFx0cmFuZ2U6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XHJcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8ICggdmFsdWUgPj0gcGFyYW1bMF0gJiYgdmFsdWUgPD0gcGFyYW1bMV0gKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9lcXVhbFRvXHJcblx0XHRlcXVhbFRvOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xyXG5cdFx0XHQvLyBiaW5kIHRvIHRoZSBibHVyIGV2ZW50IG9mIHRoZSB0YXJnZXQgaW4gb3JkZXIgdG8gcmV2YWxpZGF0ZSB3aGVuZXZlciB0aGUgdGFyZ2V0IGZpZWxkIGlzIHVwZGF0ZWRcclxuXHRcdFx0Ly8gVE9ETyBmaW5kIGEgd2F5IHRvIGJpbmQgdGhlIGV2ZW50IGp1c3Qgb25jZSwgYXZvaWRpbmcgdGhlIHVuYmluZC1yZWJpbmQgb3ZlcmhlYWRcclxuXHRcdFx0dmFyIHRhcmdldCA9ICQocGFyYW0pO1xyXG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3Mub25mb2N1c291dCApIHtcclxuXHRcdFx0XHR0YXJnZXQudW5iaW5kKFwiLnZhbGlkYXRlLWVxdWFsVG9cIikuYmluZChcImJsdXIudmFsaWRhdGUtZXF1YWxUb1wiLCBmdW5jdGlvbigpIHtcclxuXHRcdFx0XHRcdCQoZWxlbWVudCkudmFsaWQoKTtcclxuXHRcdFx0XHR9KTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gdmFsdWUgPT09IHRhcmdldC52YWwoKTtcclxuXHRcdH0sXHJcblxyXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yZW1vdGVcclxuXHRcdHJlbW90ZTogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50LCBwYXJhbSApIHtcclxuXHRcdFx0aWYgKCB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpICkge1xyXG5cdFx0XHRcdHJldHVybiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0dmFyIHByZXZpb3VzID0gdGhpcy5wcmV2aW91c1ZhbHVlKGVsZW1lbnQpO1xyXG5cdFx0XHRpZiAoIXRoaXMuc2V0dGluZ3MubWVzc2FnZXNbZWxlbWVudC5uYW1lXSApIHtcclxuXHRcdFx0XHR0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0gPSB7fTtcclxuXHRcdFx0fVxyXG5cdFx0XHRwcmV2aW91cy5vcmlnaW5hbE1lc3NhZ2UgPSB0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0ucmVtb3RlO1xyXG5cdFx0XHR0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0ucmVtb3RlID0gcHJldmlvdXMubWVzc2FnZTtcclxuXHJcblx0XHRcdHBhcmFtID0gdHlwZW9mIHBhcmFtID09PSBcInN0cmluZ1wiICYmIHt1cmw6cGFyYW19IHx8IHBhcmFtO1xyXG5cclxuXHRcdFx0aWYgKCBwcmV2aW91cy5vbGQgPT09IHZhbHVlICkge1xyXG5cdFx0XHRcdHJldHVybiBwcmV2aW91cy52YWxpZDtcclxuXHRcdFx0fVxyXG5cclxuXHRcdFx0cHJldmlvdXMub2xkID0gdmFsdWU7XHJcblx0XHRcdHZhciB2YWxpZGF0b3IgPSB0aGlzO1xyXG5cdFx0XHR0aGlzLnN0YXJ0UmVxdWVzdChlbGVtZW50KTtcclxuXHRcdFx0dmFyIGRhdGEgPSB7fTtcclxuXHRcdFx0ZGF0YVtlbGVtZW50Lm5hbWVdID0gdmFsdWU7XHJcblx0XHRcdCQuYWpheCgkLmV4dGVuZCh0cnVlLCB7XHJcblx0XHRcdFx0dXJsOiBwYXJhbSxcclxuXHRcdFx0XHRtb2RlOiBcImFib3J0XCIsXHJcblx0XHRcdFx0cG9ydDogXCJ2YWxpZGF0ZVwiICsgZWxlbWVudC5uYW1lLFxyXG5cdFx0XHRcdGRhdGFUeXBlOiBcImpzb25cIixcclxuXHRcdFx0XHRkYXRhOiBkYXRhLFxyXG5cdFx0XHRcdHN1Y2Nlc3M6IGZ1bmN0aW9uKCByZXNwb25zZSApIHtcclxuXHRcdFx0XHRcdHZhbGlkYXRvci5zZXR0aW5ncy5tZXNzYWdlc1tlbGVtZW50Lm5hbWVdLnJlbW90ZSA9IHByZXZpb3VzLm9yaWdpbmFsTWVzc2FnZTtcclxuXHRcdFx0XHRcdHZhciB2YWxpZCA9IHJlc3BvbnNlID09PSB0cnVlIHx8IHJlc3BvbnNlID09PSBcInRydWVcIjtcclxuXHRcdFx0XHRcdGlmICggdmFsaWQgKSB7XHJcblx0XHRcdFx0XHRcdHZhciBzdWJtaXR0ZWQgPSB2YWxpZGF0b3IuZm9ybVN1Ym1pdHRlZDtcclxuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLnByZXBhcmVFbGVtZW50KGVsZW1lbnQpO1xyXG5cdFx0XHRcdFx0XHR2YWxpZGF0b3IuZm9ybVN1Ym1pdHRlZCA9IHN1Ym1pdHRlZDtcclxuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLnN1Y2Nlc3NMaXN0LnB1c2goZWxlbWVudCk7XHJcblx0XHRcdFx0XHRcdGRlbGV0ZSB2YWxpZGF0b3IuaW52YWxpZFtlbGVtZW50Lm5hbWVdO1xyXG5cdFx0XHRcdFx0XHR2YWxpZGF0b3Iuc2hvd0Vycm9ycygpO1xyXG5cdFx0XHRcdFx0fSBlbHNlIHtcclxuXHRcdFx0XHRcdFx0dmFyIGVycm9ycyA9IHt9O1xyXG5cdFx0XHRcdFx0XHR2YXIgbWVzc2FnZSA9IHJlc3BvbnNlIHx8IHZhbGlkYXRvci5kZWZhdWx0TWVzc2FnZSggZWxlbWVudCwgXCJyZW1vdGVcIiApO1xyXG5cdFx0XHRcdFx0XHRlcnJvcnNbZWxlbWVudC5uYW1lXSA9IHByZXZpb3VzLm1lc3NhZ2UgPSAkLmlzRnVuY3Rpb24obWVzc2FnZSkgPyBtZXNzYWdlKHZhbHVlKSA6IG1lc3NhZ2U7XHJcblx0XHRcdFx0XHRcdHZhbGlkYXRvci5pbnZhbGlkW2VsZW1lbnQubmFtZV0gPSB0cnVlO1xyXG5cdFx0XHRcdFx0XHR2YWxpZGF0b3Iuc2hvd0Vycm9ycyhlcnJvcnMpO1xyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0cHJldmlvdXMudmFsaWQgPSB2YWxpZDtcclxuXHRcdFx0XHRcdHZhbGlkYXRvci5zdG9wUmVxdWVzdChlbGVtZW50LCB2YWxpZCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHR9LCBwYXJhbSkpO1xyXG5cdFx0XHRyZXR1cm4gXCJwZW5kaW5nXCI7XHJcblx0XHR9XHJcblxyXG5cdH1cclxuXHJcbn0pO1xyXG5cclxuLy8gZGVwcmVjYXRlZCwgdXNlICQudmFsaWRhdG9yLmZvcm1hdCBpbnN0ZWFkXHJcbiQuZm9ybWF0ID0gJC52YWxpZGF0b3IuZm9ybWF0O1xyXG5cclxufShqUXVlcnkpKTtcclxuXHJcbi8vIGFqYXggbW9kZTogYWJvcnRcclxuLy8gdXNhZ2U6ICQuYWpheCh7IG1vZGU6IFwiYWJvcnRcIlssIHBvcnQ6IFwidW5pcXVlcG9ydFwiXX0pO1xyXG4vLyBpZiBtb2RlOlwiYWJvcnRcIiBpcyB1c2VkLCB0aGUgcHJldmlvdXMgcmVxdWVzdCBvbiB0aGF0IHBvcnQgKHBvcnQgY2FuIGJlIHVuZGVmaW5lZCkgaXMgYWJvcnRlZCB2aWEgWE1MSHR0cFJlcXVlc3QuYWJvcnQoKVxyXG4oZnVuY3Rpb24oJCkge1xyXG5cdHZhciBwZW5kaW5nUmVxdWVzdHMgPSB7fTtcclxuXHQvLyBVc2UgYSBwcmVmaWx0ZXIgaWYgYXZhaWxhYmxlICgxLjUrKVxyXG5cdGlmICggJC5hamF4UHJlZmlsdGVyICkge1xyXG5cdFx0JC5hamF4UHJlZmlsdGVyKGZ1bmN0aW9uKCBzZXR0aW5ncywgXywgeGhyICkge1xyXG5cdFx0XHR2YXIgcG9ydCA9IHNldHRpbmdzLnBvcnQ7XHJcblx0XHRcdGlmICggc2V0dGluZ3MubW9kZSA9PT0gXCJhYm9ydFwiICkge1xyXG5cdFx0XHRcdGlmICggcGVuZGluZ1JlcXVlc3RzW3BvcnRdICkge1xyXG5cdFx0XHRcdFx0cGVuZGluZ1JlcXVlc3RzW3BvcnRdLmFib3J0KCk7XHJcblx0XHRcdFx0fVxyXG5cdFx0XHRcdHBlbmRpbmdSZXF1ZXN0c1twb3J0XSA9IHhocjtcclxuXHRcdFx0fVxyXG5cdFx0fSk7XHJcblx0fSBlbHNlIHtcclxuXHRcdC8vIFByb3h5IGFqYXhcclxuXHRcdHZhciBhamF4ID0gJC5hamF4O1xyXG5cdFx0JC5hamF4ID0gZnVuY3Rpb24oIHNldHRpbmdzICkge1xyXG5cdFx0XHR2YXIgbW9kZSA9ICggXCJtb2RlXCIgaW4gc2V0dGluZ3MgPyBzZXR0aW5ncyA6ICQuYWpheFNldHRpbmdzICkubW9kZSxcclxuXHRcdFx0XHRwb3J0ID0gKCBcInBvcnRcIiBpbiBzZXR0aW5ncyA/IHNldHRpbmdzIDogJC5hamF4U2V0dGluZ3MgKS5wb3J0O1xyXG5cdFx0XHRpZiAoIG1vZGUgPT09IFwiYWJvcnRcIiApIHtcclxuXHRcdFx0XHRpZiAoIHBlbmRpbmdSZXF1ZXN0c1twb3J0XSApIHtcclxuXHRcdFx0XHRcdHBlbmRpbmdSZXF1ZXN0c1twb3J0XS5hYm9ydCgpO1xyXG5cdFx0XHRcdH1cclxuXHRcdFx0XHRwZW5kaW5nUmVxdWVzdHNbcG9ydF0gPSBhamF4LmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XHJcblx0XHRcdFx0cmV0dXJuIHBlbmRpbmdSZXF1ZXN0c1twb3J0XTtcclxuXHRcdFx0fVxyXG5cdFx0XHRyZXR1cm4gYWpheC5hcHBseSh0aGlzLCBhcmd1bWVudHMpO1xyXG5cdFx0fTtcclxuXHR9XHJcbn0oalF1ZXJ5KSk7XHJcblxyXG4vLyBwcm92aWRlcyBkZWxlZ2F0ZSh0eXBlOiBTdHJpbmcsIGRlbGVnYXRlOiBTZWxlY3RvciwgaGFuZGxlcjogQ2FsbGJhY2spIHBsdWdpbiBmb3IgZWFzaWVyIGV2ZW50IGRlbGVnYXRpb25cclxuLy8gaGFuZGxlciBpcyBvbmx5IGNhbGxlZCB3aGVuICQoZXZlbnQudGFyZ2V0KS5pcyhkZWxlZ2F0ZSksIGluIHRoZSBzY29wZSBvZiB0aGUganF1ZXJ5LW9iamVjdCBmb3IgZXZlbnQudGFyZ2V0XHJcbihmdW5jdGlvbigkKSB7XHJcblx0JC5leHRlbmQoJC5mbiwge1xyXG5cdFx0dmFsaWRhdGVEZWxlZ2F0ZTogZnVuY3Rpb24oIGRlbGVnYXRlLCB0eXBlLCBoYW5kbGVyICkge1xyXG5cdFx0XHRyZXR1cm4gdGhpcy5iaW5kKHR5cGUsIGZ1bmN0aW9uKCBldmVudCApIHtcclxuXHRcdFx0XHR2YXIgdGFyZ2V0ID0gJChldmVudC50YXJnZXQpO1xyXG5cdFx0XHRcdGlmICggdGFyZ2V0LmlzKGRlbGVnYXRlKSApIHtcclxuXHRcdFx0XHRcdHJldHVybiBoYW5kbGVyLmFwcGx5KHRhcmdldCwgYXJndW1lbnRzKTtcclxuXHRcdFx0XHR9XHJcblx0XHRcdH0pO1xyXG5cdFx0fVxyXG5cdH0pO1xyXG59KGpRdWVyeSkpO1xyXG4iLCIvKiBOVUdFVDogQkVHSU4gTElDRU5TRSBURVhUXHJcbiAqXHJcbiAqIE1pY3Jvc29mdCBncmFudHMgeW91IHRoZSByaWdodCB0byB1c2UgdGhlc2Ugc2NyaXB0IGZpbGVzIGZvciB0aGUgc29sZVxyXG4gKiBwdXJwb3NlIG9mIGVpdGhlcjogKGkpIGludGVyYWN0aW5nIHRocm91Z2ggeW91ciBicm93c2VyIHdpdGggdGhlIE1pY3Jvc29mdFxyXG4gKiB3ZWJzaXRlIG9yIG9ubGluZSBzZXJ2aWNlLCBzdWJqZWN0IHRvIHRoZSBhcHBsaWNhYmxlIGxpY2Vuc2luZyBvciB1c2VcclxuICogdGVybXM7IG9yIChpaSkgdXNpbmcgdGhlIGZpbGVzIGFzIGluY2x1ZGVkIHdpdGggYSBNaWNyb3NvZnQgcHJvZHVjdCBzdWJqZWN0XHJcbiAqIHRvIHRoYXQgcHJvZHVjdCdzIGxpY2Vuc2UgdGVybXMuIE1pY3Jvc29mdCByZXNlcnZlcyBhbGwgb3RoZXIgcmlnaHRzIHRvIHRoZVxyXG4gKiBmaWxlcyBub3QgZXhwcmVzc2x5IGdyYW50ZWQgYnkgTWljcm9zb2Z0LCB3aGV0aGVyIGJ5IGltcGxpY2F0aW9uLCBlc3RvcHBlbFxyXG4gKiBvciBvdGhlcndpc2UuIEluc29mYXIgYXMgYSBzY3JpcHQgZmlsZSBpcyBkdWFsIGxpY2Vuc2VkIHVuZGVyIEdQTCxcclxuICogTWljcm9zb2Z0IG5laXRoZXIgdG9vayB0aGUgY29kZSB1bmRlciBHUEwgbm9yIGRpc3RyaWJ1dGVzIGl0IHRoZXJldW5kZXIgYnV0XHJcbiAqIHVuZGVyIHRoZSB0ZXJtcyBzZXQgb3V0IGluIHRoaXMgcGFyYWdyYXBoLiBBbGwgbm90aWNlcyBhbmQgbGljZW5zZXNcclxuICogYmVsb3cgYXJlIGZvciBpbmZvcm1hdGlvbmFsIHB1cnBvc2VzIG9ubHkuXHJcbiAqXHJcbiAqIE5VR0VUOiBFTkQgTElDRU5TRSBURVhUICovXHJcbi8qIVxyXG4qKiBVbm9idHJ1c2l2ZSB2YWxpZGF0aW9uIHN1cHBvcnQgbGlicmFyeSBmb3IgalF1ZXJ5IGFuZCBqUXVlcnkgVmFsaWRhdGVcclxuKiogQ29weXJpZ2h0IChDKSBNaWNyb3NvZnQgQ29ycG9yYXRpb24uIEFsbCByaWdodHMgcmVzZXJ2ZWQuXHJcbiovXHJcblxyXG4vKmpzbGludCB3aGl0ZTogdHJ1ZSwgYnJvd3NlcjogdHJ1ZSwgb25ldmFyOiB0cnVlLCB1bmRlZjogdHJ1ZSwgbm9tZW46IHRydWUsIGVxZXFlcTogdHJ1ZSwgcGx1c3BsdXM6IHRydWUsIGJpdHdpc2U6IHRydWUsIHJlZ2V4cDogdHJ1ZSwgbmV3Y2FwOiB0cnVlLCBpbW1lZDogdHJ1ZSwgc3RyaWN0OiBmYWxzZSAqL1xyXG4vKmdsb2JhbCBkb2N1bWVudDogZmFsc2UsIGpRdWVyeTogZmFsc2UgKi9cclxuXHJcbihmdW5jdGlvbiAoJCkge1xyXG4gICAgdmFyICRqUXZhbCA9ICQudmFsaWRhdG9yLFxyXG4gICAgICAgIGFkYXB0ZXJzLFxyXG4gICAgICAgIGRhdGFfdmFsaWRhdGlvbiA9IFwidW5vYnRydXNpdmVWYWxpZGF0aW9uXCI7XHJcblxyXG4gICAgZnVuY3Rpb24gc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBydWxlTmFtZSwgdmFsdWUpIHtcclxuICAgICAgICBvcHRpb25zLnJ1bGVzW3J1bGVOYW1lXSA9IHZhbHVlO1xyXG4gICAgICAgIGlmIChvcHRpb25zLm1lc3NhZ2UpIHtcclxuICAgICAgICAgICAgb3B0aW9ucy5tZXNzYWdlc1tydWxlTmFtZV0gPSBvcHRpb25zLm1lc3NhZ2U7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIHNwbGl0QW5kVHJpbSh2YWx1ZSkge1xyXG4gICAgICAgIHJldHVybiB2YWx1ZS5yZXBsYWNlKC9eXFxzK3xcXHMrJC9nLCBcIlwiKS5zcGxpdCgvXFxzKixcXHMqL2cpO1xyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIGVzY2FwZUF0dHJpYnV0ZVZhbHVlKHZhbHVlKSB7XHJcbiAgICAgICAgLy8gQXMgbWVudGlvbmVkIG9uIGh0dHA6Ly9hcGkuanF1ZXJ5LmNvbS9jYXRlZ29yeS9zZWxlY3RvcnMvXHJcbiAgICAgICAgcmV0dXJuIHZhbHVlLnJlcGxhY2UoLyhbIVwiIyQlJicoKSorLC4vOjs8PT4/QFxcW1xcXFxcXF1eYHt8fX5dKS9nLCBcIlxcXFwkMVwiKTtcclxuICAgIH1cclxuXHJcbiAgICBmdW5jdGlvbiBnZXRNb2RlbFByZWZpeChmaWVsZE5hbWUpIHtcclxuICAgICAgICByZXR1cm4gZmllbGROYW1lLnN1YnN0cigwLCBmaWVsZE5hbWUubGFzdEluZGV4T2YoXCIuXCIpICsgMSk7XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gYXBwZW5kTW9kZWxQcmVmaXgodmFsdWUsIHByZWZpeCkge1xyXG4gICAgICAgIGlmICh2YWx1ZS5pbmRleE9mKFwiKi5cIikgPT09IDApIHtcclxuICAgICAgICAgICAgdmFsdWUgPSB2YWx1ZS5yZXBsYWNlKFwiKi5cIiwgcHJlZml4KTtcclxuICAgICAgICB9XHJcbiAgICAgICAgcmV0dXJuIHZhbHVlO1xyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIG9uRXJyb3IoZXJyb3IsIGlucHV0RWxlbWVudCkgeyAgLy8gJ3RoaXMnIGlzIHRoZSBmb3JtIGVsZW1lbnRcclxuICAgICAgICB2YXIgY29udGFpbmVyID0gJCh0aGlzKS5maW5kKFwiW2RhdGEtdmFsbXNnLWZvcj0nXCIgKyBlc2NhcGVBdHRyaWJ1dGVWYWx1ZShpbnB1dEVsZW1lbnRbMF0ubmFtZSkgKyBcIiddXCIpLFxyXG4gICAgICAgICAgICByZXBsYWNlQXR0clZhbHVlID0gY29udGFpbmVyLmF0dHIoXCJkYXRhLXZhbG1zZy1yZXBsYWNlXCIpLFxyXG4gICAgICAgICAgICByZXBsYWNlID0gcmVwbGFjZUF0dHJWYWx1ZSA/ICQucGFyc2VKU09OKHJlcGxhY2VBdHRyVmFsdWUpICE9PSBmYWxzZSA6IG51bGw7XHJcblxyXG4gICAgICAgIGNvbnRhaW5lci5yZW1vdmVDbGFzcyhcImZpZWxkLXZhbGlkYXRpb24tdmFsaWRcIikuYWRkQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLWVycm9yXCIpO1xyXG4gICAgICAgIGVycm9yLmRhdGEoXCJ1bm9idHJ1c2l2ZUNvbnRhaW5lclwiLCBjb250YWluZXIpO1xyXG5cclxuICAgICAgICBpZiAocmVwbGFjZSkge1xyXG4gICAgICAgICAgICBjb250YWluZXIuZW1wdHkoKTtcclxuICAgICAgICAgICAgZXJyb3IucmVtb3ZlQ2xhc3MoXCJpbnB1dC12YWxpZGF0aW9uLWVycm9yXCIpLmFwcGVuZFRvKGNvbnRhaW5lcik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGVsc2Uge1xyXG4gICAgICAgICAgICBlcnJvci5oaWRlKCk7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIG9uRXJyb3JzKGV2ZW50LCB2YWxpZGF0b3IpIHsgIC8vICd0aGlzJyBpcyB0aGUgZm9ybSBlbGVtZW50XHJcbiAgICAgICAgdmFyIGNvbnRhaW5lciA9ICQodGhpcykuZmluZChcIltkYXRhLXZhbG1zZy1zdW1tYXJ5PXRydWVdXCIpLFxyXG4gICAgICAgICAgICBsaXN0ID0gY29udGFpbmVyLmZpbmQoXCJ1bFwiKTtcclxuXHJcbiAgICAgICAgaWYgKGxpc3QgJiYgbGlzdC5sZW5ndGggJiYgdmFsaWRhdG9yLmVycm9yTGlzdC5sZW5ndGgpIHtcclxuICAgICAgICAgICAgbGlzdC5lbXB0eSgpO1xyXG4gICAgICAgICAgICBjb250YWluZXIuYWRkQ2xhc3MoXCJ2YWxpZGF0aW9uLXN1bW1hcnktZXJyb3JzXCIpLnJlbW92ZUNsYXNzKFwidmFsaWRhdGlvbi1zdW1tYXJ5LXZhbGlkXCIpO1xyXG5cclxuICAgICAgICAgICAgJC5lYWNoKHZhbGlkYXRvci5lcnJvckxpc3QsIGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICQoXCI8bGkgLz5cIikuaHRtbCh0aGlzLm1lc3NhZ2UpLmFwcGVuZFRvKGxpc3QpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gb25TdWNjZXNzKGVycm9yKSB7ICAvLyAndGhpcycgaXMgdGhlIGZvcm0gZWxlbWVudFxyXG4gICAgICAgIHZhciBjb250YWluZXIgPSBlcnJvci5kYXRhKFwidW5vYnRydXNpdmVDb250YWluZXJcIiksXHJcbiAgICAgICAgICAgIHJlcGxhY2VBdHRyVmFsdWUgPSBjb250YWluZXIuYXR0cihcImRhdGEtdmFsbXNnLXJlcGxhY2VcIiksXHJcbiAgICAgICAgICAgIHJlcGxhY2UgPSByZXBsYWNlQXR0clZhbHVlID8gJC5wYXJzZUpTT04ocmVwbGFjZUF0dHJWYWx1ZSkgOiBudWxsO1xyXG5cclxuICAgICAgICBpZiAoY29udGFpbmVyKSB7XHJcbiAgICAgICAgICAgIGNvbnRhaW5lci5hZGRDbGFzcyhcImZpZWxkLXZhbGlkYXRpb24tdmFsaWRcIikucmVtb3ZlQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLWVycm9yXCIpO1xyXG4gICAgICAgICAgICBlcnJvci5yZW1vdmVEYXRhKFwidW5vYnRydXNpdmVDb250YWluZXJcIik7XHJcblxyXG4gICAgICAgICAgICBpZiAocmVwbGFjZSkge1xyXG4gICAgICAgICAgICAgICAgY29udGFpbmVyLmVtcHR5KCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gb25SZXNldChldmVudCkgeyAgLy8gJ3RoaXMnIGlzIHRoZSBmb3JtIGVsZW1lbnRcclxuICAgICAgICB2YXIgJGZvcm0gPSAkKHRoaXMpO1xyXG4gICAgICAgICRmb3JtLmRhdGEoXCJ2YWxpZGF0b3JcIikucmVzZXRGb3JtKCk7XHJcbiAgICAgICAgJGZvcm0uZmluZChcIi52YWxpZGF0aW9uLXN1bW1hcnktZXJyb3JzXCIpXHJcbiAgICAgICAgICAgIC5hZGRDbGFzcyhcInZhbGlkYXRpb24tc3VtbWFyeS12YWxpZFwiKVxyXG4gICAgICAgICAgICAucmVtb3ZlQ2xhc3MoXCJ2YWxpZGF0aW9uLXN1bW1hcnktZXJyb3JzXCIpO1xyXG4gICAgICAgICRmb3JtLmZpbmQoXCIuZmllbGQtdmFsaWRhdGlvbi1lcnJvclwiKVxyXG4gICAgICAgICAgICAuYWRkQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLXZhbGlkXCIpXHJcbiAgICAgICAgICAgIC5yZW1vdmVDbGFzcyhcImZpZWxkLXZhbGlkYXRpb24tZXJyb3JcIilcclxuICAgICAgICAgICAgLnJlbW92ZURhdGEoXCJ1bm9idHJ1c2l2ZUNvbnRhaW5lclwiKVxyXG4gICAgICAgICAgICAuZmluZChcIj4qXCIpICAvLyBJZiB3ZSB3ZXJlIHVzaW5nIHZhbG1zZy1yZXBsYWNlLCBnZXQgdGhlIHVuZGVybHlpbmcgZXJyb3JcclxuICAgICAgICAgICAgICAgIC5yZW1vdmVEYXRhKFwidW5vYnRydXNpdmVDb250YWluZXJcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gdmFsaWRhdGlvbkluZm8oZm9ybSkge1xyXG4gICAgICAgIHZhciAkZm9ybSA9ICQoZm9ybSksXHJcbiAgICAgICAgICAgIHJlc3VsdCA9ICRmb3JtLmRhdGEoZGF0YV92YWxpZGF0aW9uKSxcclxuICAgICAgICAgICAgb25SZXNldFByb3h5ID0gJC5wcm94eShvblJlc2V0LCBmb3JtKSxcclxuICAgICAgICAgICAgZGVmYXVsdE9wdGlvbnMgPSAkalF2YWwudW5vYnRydXNpdmUub3B0aW9ucyB8fCB7fSxcclxuICAgICAgICAgICAgZXhlY0luQ29udGV4dCA9IGZ1bmN0aW9uIChuYW1lLCBhcmdzKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgZnVuYyA9IGRlZmF1bHRPcHRpb25zW25hbWVdO1xyXG4gICAgICAgICAgICAgICAgZnVuYyAmJiAkLmlzRnVuY3Rpb24oZnVuYykgJiYgZnVuYy5hcHBseShmb3JtLCBhcmdzKTtcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAoIXJlc3VsdCkge1xyXG4gICAgICAgICAgICByZXN1bHQgPSB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zOiB7ICAvLyBvcHRpb25zIHN0cnVjdHVyZSBwYXNzZWQgdG8galF1ZXJ5IFZhbGlkYXRlJ3MgdmFsaWRhdGUoKSBtZXRob2RcclxuICAgICAgICAgICAgICAgICAgICBlcnJvckNsYXNzOiBkZWZhdWx0T3B0aW9ucy5lcnJvckNsYXNzIHx8IFwiaW5wdXQtdmFsaWRhdGlvbi1lcnJvclwiLFxyXG4gICAgICAgICAgICAgICAgICAgIGVycm9yRWxlbWVudDogZGVmYXVsdE9wdGlvbnMuZXJyb3JFbGVtZW50IHx8IFwic3BhblwiLFxyXG4gICAgICAgICAgICAgICAgICAgIGVycm9yUGxhY2VtZW50OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG9uRXJyb3IuYXBwbHkoZm9ybSwgYXJndW1lbnRzKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZXhlY0luQ29udGV4dChcImVycm9yUGxhY2VtZW50XCIsIGFyZ3VtZW50cyk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgICAgICBpbnZhbGlkSGFuZGxlcjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBvbkVycm9ycy5hcHBseShmb3JtLCBhcmd1bWVudHMpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBleGVjSW5Db250ZXh0KFwiaW52YWxpZEhhbmRsZXJcIiwgYXJndW1lbnRzKTtcclxuICAgICAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICAgICAgICAgIG1lc3NhZ2VzOiB7fSxcclxuICAgICAgICAgICAgICAgICAgICBydWxlczoge30sXHJcbiAgICAgICAgICAgICAgICAgICAgc3VjY2VzczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBvblN1Y2Nlc3MuYXBwbHkoZm9ybSwgYXJndW1lbnRzKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZXhlY0luQ29udGV4dChcInN1Y2Nlc3NcIiwgYXJndW1lbnRzKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICAgICAgYXR0YWNoVmFsaWRhdGlvbjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICRmb3JtXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC5vZmYoXCJyZXNldC5cIiArIGRhdGFfdmFsaWRhdGlvbiwgb25SZXNldFByb3h5KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAub24oXCJyZXNldC5cIiArIGRhdGFfdmFsaWRhdGlvbiwgb25SZXNldFByb3h5KVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAudmFsaWRhdGUodGhpcy5vcHRpb25zKTtcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICB2YWxpZGF0ZTogZnVuY3Rpb24gKCkgeyAgLy8gYSB2YWxpZGF0aW9uIGZ1bmN0aW9uIHRoYXQgaXMgY2FsbGVkIGJ5IHVub2J0cnVzaXZlIEFqYXhcclxuICAgICAgICAgICAgICAgICAgICAkZm9ybS52YWxpZGF0ZSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiAkZm9ybS52YWxpZCgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICAkZm9ybS5kYXRhKGRhdGFfdmFsaWRhdGlvbiwgcmVzdWx0KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICB9XHJcblxyXG4gICAgJGpRdmFsLnVub2J0cnVzaXZlID0ge1xyXG4gICAgICAgIGFkYXB0ZXJzOiBbXSxcclxuXHJcbiAgICAgICAgcGFyc2VFbGVtZW50OiBmdW5jdGlvbiAoZWxlbWVudCwgc2tpcEF0dGFjaCkge1xyXG4gICAgICAgICAgICAvLy8gPHN1bW1hcnk+XHJcbiAgICAgICAgICAgIC8vLyBQYXJzZXMgYSBzaW5nbGUgSFRNTCBlbGVtZW50IGZvciB1bm9idHJ1c2l2ZSB2YWxpZGF0aW9uIGF0dHJpYnV0ZXMuXHJcbiAgICAgICAgICAgIC8vLyA8L3N1bW1hcnk+XHJcbiAgICAgICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImVsZW1lbnRcIiBkb21FbGVtZW50PVwidHJ1ZVwiPlRoZSBIVE1MIGVsZW1lbnQgdG8gYmUgcGFyc2VkLjwvcGFyYW0+XHJcbiAgICAgICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInNraXBBdHRhY2hcIiB0eXBlPVwiQm9vbGVhblwiPltPcHRpb25hbF0gdHJ1ZSB0byBza2lwIGF0dGFjaGluZyB0aGVcclxuICAgICAgICAgICAgLy8vIHZhbGlkYXRpb24gdG8gdGhlIGZvcm0uIElmIHBhcnNpbmcganVzdCB0aGlzIHNpbmdsZSBlbGVtZW50LCB5b3Ugc2hvdWxkIHNwZWNpZnkgdHJ1ZS5cclxuICAgICAgICAgICAgLy8vIElmIHBhcnNpbmcgc2V2ZXJhbCBlbGVtZW50cywgeW91IHNob3VsZCBzcGVjaWZ5IGZhbHNlLCBhbmQgbWFudWFsbHkgYXR0YWNoIHRoZSB2YWxpZGF0aW9uXHJcbiAgICAgICAgICAgIC8vLyB0byB0aGUgZm9ybSB3aGVuIHlvdSBhcmUgZmluaXNoZWQuIFRoZSBkZWZhdWx0IGlzIGZhbHNlLjwvcGFyYW0+XHJcbiAgICAgICAgICAgIHZhciAkZWxlbWVudCA9ICQoZWxlbWVudCksXHJcbiAgICAgICAgICAgICAgICBmb3JtID0gJGVsZW1lbnQucGFyZW50cyhcImZvcm1cIilbMF0sXHJcbiAgICAgICAgICAgICAgICB2YWxJbmZvLCBydWxlcywgbWVzc2FnZXM7XHJcblxyXG4gICAgICAgICAgICBpZiAoIWZvcm0pIHsgIC8vIENhbm5vdCBkbyBjbGllbnQtc2lkZSB2YWxpZGF0aW9uIHdpdGhvdXQgYSBmb3JtXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIHZhbEluZm8gPSB2YWxpZGF0aW9uSW5mbyhmb3JtKTtcclxuICAgICAgICAgICAgdmFsSW5mby5vcHRpb25zLnJ1bGVzW2VsZW1lbnQubmFtZV0gPSBydWxlcyA9IHt9O1xyXG4gICAgICAgICAgICB2YWxJbmZvLm9wdGlvbnMubWVzc2FnZXNbZWxlbWVudC5uYW1lXSA9IG1lc3NhZ2VzID0ge307XHJcblxyXG4gICAgICAgICAgICAkLmVhY2godGhpcy5hZGFwdGVycywgZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHByZWZpeCA9IFwiZGF0YS12YWwtXCIgKyB0aGlzLm5hbWUsXHJcbiAgICAgICAgICAgICAgICAgICAgbWVzc2FnZSA9ICRlbGVtZW50LmF0dHIocHJlZml4KSxcclxuICAgICAgICAgICAgICAgICAgICBwYXJhbVZhbHVlcyA9IHt9O1xyXG5cclxuICAgICAgICAgICAgICAgIGlmIChtZXNzYWdlICE9PSB1bmRlZmluZWQpIHsgIC8vIENvbXBhcmUgYWdhaW5zdCB1bmRlZmluZWQsIGJlY2F1c2UgYW4gZW1wdHkgbWVzc2FnZSBpcyBsZWdhbCAoYW5kIGZhbHN5KVxyXG4gICAgICAgICAgICAgICAgICAgIHByZWZpeCArPSBcIi1cIjtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgJC5lYWNoKHRoaXMucGFyYW1zLCBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHBhcmFtVmFsdWVzW3RoaXNdID0gJGVsZW1lbnQuYXR0cihwcmVmaXggKyB0aGlzKTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5hZGFwdCh7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGVsZW1lbnQ6IGVsZW1lbnQsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGZvcm06IGZvcm0sXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIG1lc3NhZ2U6IG1lc3NhZ2UsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHBhcmFtczogcGFyYW1WYWx1ZXMsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJ1bGVzOiBydWxlcyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgbWVzc2FnZXM6IG1lc3NhZ2VzXHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgJC5leHRlbmQocnVsZXMsIHsgXCJfX2R1bW15X19cIjogdHJ1ZSB9KTtcclxuXHJcbiAgICAgICAgICAgIGlmICghc2tpcEF0dGFjaCkge1xyXG4gICAgICAgICAgICAgICAgdmFsSW5mby5hdHRhY2hWYWxpZGF0aW9uKCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwYXJzZTogZnVuY3Rpb24gKHNlbGVjdG9yKSB7XHJcbiAgICAgICAgICAgIC8vLyA8c3VtbWFyeT5cclxuICAgICAgICAgICAgLy8vIFBhcnNlcyBhbGwgdGhlIEhUTUwgZWxlbWVudHMgaW4gdGhlIHNwZWNpZmllZCBzZWxlY3Rvci4gSXQgbG9va3MgZm9yIGlucHV0IGVsZW1lbnRzIGRlY29yYXRlZFxyXG4gICAgICAgICAgICAvLy8gd2l0aCB0aGUgW2RhdGEtdmFsPXRydWVdIGF0dHJpYnV0ZSB2YWx1ZSBhbmQgZW5hYmxlcyB2YWxpZGF0aW9uIGFjY29yZGluZyB0byB0aGUgZGF0YS12YWwtKlxyXG4gICAgICAgICAgICAvLy8gYXR0cmlidXRlIHZhbHVlcy5cclxuICAgICAgICAgICAgLy8vIDwvc3VtbWFyeT5cclxuICAgICAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwic2VsZWN0b3JcIiB0eXBlPVwiU3RyaW5nXCI+QW55IHZhbGlkIGpRdWVyeSBzZWxlY3Rvci48L3BhcmFtPlxyXG5cclxuICAgICAgICAgICAgLy8gJGZvcm1zIGluY2x1ZGVzIGFsbCBmb3JtcyBpbiBzZWxlY3RvcidzIERPTSBoaWVyYXJjaHkgKHBhcmVudCwgY2hpbGRyZW4gYW5kIHNlbGYpIHRoYXQgaGF2ZSBhdCBsZWFzdCBvbmVcclxuICAgICAgICAgICAgLy8gZWxlbWVudCB3aXRoIGRhdGEtdmFsPXRydWVcclxuICAgICAgICAgICAgdmFyICRzZWxlY3RvciA9ICQoc2VsZWN0b3IpLFxyXG4gICAgICAgICAgICAgICAgJGZvcm1zID0gJHNlbGVjdG9yLnBhcmVudHMoKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmFkZEJhY2soKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmZpbHRlcihcImZvcm1cIilcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hZGQoJHNlbGVjdG9yLmZpbmQoXCJmb3JtXCIpKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmhhcyhcIltkYXRhLXZhbD10cnVlXVwiKTtcclxuXHJcbiAgICAgICAgICAgICRzZWxlY3Rvci5maW5kKFwiW2RhdGEtdmFsPXRydWVdXCIpLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgJGpRdmFsLnVub2J0cnVzaXZlLnBhcnNlRWxlbWVudCh0aGlzLCB0cnVlKTtcclxuICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAkZm9ybXMuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgaW5mbyA9IHZhbGlkYXRpb25JbmZvKHRoaXMpO1xyXG4gICAgICAgICAgICAgICAgaWYgKGluZm8pIHtcclxuICAgICAgICAgICAgICAgICAgICBpbmZvLmF0dGFjaFZhbGlkYXRpb24oKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbiAgICBhZGFwdGVycyA9ICRqUXZhbC51bm9idHJ1c2l2ZS5hZGFwdGVycztcclxuXHJcbiAgICBhZGFwdGVycy5hZGQgPSBmdW5jdGlvbiAoYWRhcHRlck5hbWUsIHBhcmFtcywgZm4pIHtcclxuICAgICAgICAvLy8gPHN1bW1hcnk+QWRkcyBhIG5ldyBhZGFwdGVyIHRvIGNvbnZlcnQgdW5vYnRydXNpdmUgSFRNTCBpbnRvIGEgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24uPC9zdW1tYXJ5PlxyXG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImFkYXB0ZXJOYW1lXCIgdHlwZT1cIlN0cmluZ1wiPlRoZSBuYW1lIG9mIHRoZSBhZGFwdGVyIHRvIGJlIGFkZGVkLiBUaGlzIG1hdGNoZXMgdGhlIG5hbWUgdXNlZFxyXG4gICAgICAgIC8vLyBpbiB0aGUgZGF0YS12YWwtbm5ubiBIVE1MIGF0dHJpYnV0ZSAod2hlcmUgbm5ubiBpcyB0aGUgYWRhcHRlciBuYW1lKS48L3BhcmFtPlxyXG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInBhcmFtc1wiIHR5cGU9XCJBcnJheVwiIG9wdGlvbmFsPVwidHJ1ZVwiPltPcHRpb25hbF0gQW4gYXJyYXkgb2YgcGFyYW1ldGVyIG5hbWVzIChzdHJpbmdzKSB0aGF0IHdpbGxcclxuICAgICAgICAvLy8gYmUgZXh0cmFjdGVkIGZyb20gdGhlIGRhdGEtdmFsLW5ubm4tbW1tbSBIVE1MIGF0dHJpYnV0ZXMgKHdoZXJlIG5ubm4gaXMgdGhlIGFkYXB0ZXIgbmFtZSwgYW5kXHJcbiAgICAgICAgLy8vIG1tbW0gaXMgdGhlIHBhcmFtZXRlciBuYW1lKS48L3BhcmFtPlxyXG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImZuXCIgdHlwZT1cIkZ1bmN0aW9uXCI+VGhlIGZ1bmN0aW9uIHRvIGNhbGwsIHdoaWNoIGFkYXB0cyB0aGUgdmFsdWVzIGZyb20gdGhlIEhUTUxcclxuICAgICAgICAvLy8gYXR0cmlidXRlcyBpbnRvIGpRdWVyeSBWYWxpZGF0ZSBydWxlcyBhbmQvb3IgbWVzc2FnZXMuPC9wYXJhbT5cclxuICAgICAgICAvLy8gPHJldHVybnMgdHlwZT1cImpRdWVyeS52YWxpZGF0b3IudW5vYnRydXNpdmUuYWRhcHRlcnNcIiAvPlxyXG4gICAgICAgIGlmICghZm4pIHsgIC8vIENhbGxlZCB3aXRoIG5vIHBhcmFtcywganVzdCBhIGZ1bmN0aW9uXHJcbiAgICAgICAgICAgIGZuID0gcGFyYW1zO1xyXG4gICAgICAgICAgICBwYXJhbXMgPSBbXTtcclxuICAgICAgICB9XHJcbiAgICAgICAgdGhpcy5wdXNoKHsgbmFtZTogYWRhcHRlck5hbWUsIHBhcmFtczogcGFyYW1zLCBhZGFwdDogZm4gfSk7XHJcbiAgICAgICAgcmV0dXJuIHRoaXM7XHJcbiAgICB9O1xyXG5cclxuICAgIGFkYXB0ZXJzLmFkZEJvb2wgPSBmdW5jdGlvbiAoYWRhcHRlck5hbWUsIHJ1bGVOYW1lKSB7XHJcbiAgICAgICAgLy8vIDxzdW1tYXJ5PkFkZHMgYSBuZXcgYWRhcHRlciB0byBjb252ZXJ0IHVub2J0cnVzaXZlIEhUTUwgaW50byBhIGpRdWVyeSBWYWxpZGF0ZSB2YWxpZGF0aW9uLCB3aGVyZVxyXG4gICAgICAgIC8vLyB0aGUgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24gcnVsZSBoYXMgbm8gcGFyYW1ldGVyIHZhbHVlcy48L3N1bW1hcnk+XHJcbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiYWRhcHRlck5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGFkYXB0ZXIgdG8gYmUgYWRkZWQuIFRoaXMgbWF0Y2hlcyB0aGUgbmFtZSB1c2VkXHJcbiAgICAgICAgLy8vIGluIHRoZSBkYXRhLXZhbC1ubm5uIEhUTUwgYXR0cmlidXRlICh3aGVyZSBubm5uIGlzIHRoZSBhZGFwdGVyIG5hbWUpLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwicnVsZU5hbWVcIiB0eXBlPVwiU3RyaW5nXCIgb3B0aW9uYWw9XCJ0cnVlXCI+W09wdGlvbmFsXSBUaGUgbmFtZSBvZiB0aGUgalF1ZXJ5IFZhbGlkYXRlIHJ1bGUuIElmIG5vdCBwcm92aWRlZCwgdGhlIHZhbHVlXHJcbiAgICAgICAgLy8vIG9mIGFkYXB0ZXJOYW1lIHdpbGwgYmUgdXNlZCBpbnN0ZWFkLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxyZXR1cm5zIHR5cGU9XCJqUXVlcnkudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzXCIgLz5cclxuICAgICAgICByZXR1cm4gdGhpcy5hZGQoYWRhcHRlck5hbWUsIGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgcnVsZU5hbWUgfHwgYWRhcHRlck5hbWUsIHRydWUpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfTtcclxuXHJcbiAgICBhZGFwdGVycy5hZGRNaW5NYXggPSBmdW5jdGlvbiAoYWRhcHRlck5hbWUsIG1pblJ1bGVOYW1lLCBtYXhSdWxlTmFtZSwgbWluTWF4UnVsZU5hbWUsIG1pbkF0dHJpYnV0ZSwgbWF4QXR0cmlidXRlKSB7XHJcbiAgICAgICAgLy8vIDxzdW1tYXJ5PkFkZHMgYSBuZXcgYWRhcHRlciB0byBjb252ZXJ0IHVub2J0cnVzaXZlIEhUTUwgaW50byBhIGpRdWVyeSBWYWxpZGF0ZSB2YWxpZGF0aW9uLCB3aGVyZVxyXG4gICAgICAgIC8vLyB0aGUgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24gaGFzIHRocmVlIHBvdGVudGlhbCBydWxlcyAob25lIGZvciBtaW4tb25seSwgb25lIGZvciBtYXgtb25seSwgYW5kXHJcbiAgICAgICAgLy8vIG9uZSBmb3IgbWluLWFuZC1tYXgpLiBUaGUgSFRNTCBwYXJhbWV0ZXJzIGFyZSBleHBlY3RlZCB0byBiZSBuYW1lZCAtbWluIGFuZCAtbWF4Ljwvc3VtbWFyeT5cclxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJhZGFwdGVyTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgYWRhcHRlciB0byBiZSBhZGRlZC4gVGhpcyBtYXRjaGVzIHRoZSBuYW1lIHVzZWRcclxuICAgICAgICAvLy8gaW4gdGhlIGRhdGEtdmFsLW5ubm4gSFRNTCBhdHRyaWJ1dGUgKHdoZXJlIG5ubm4gaXMgdGhlIGFkYXB0ZXIgbmFtZSkuPC9wYXJhbT5cclxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJtaW5SdWxlTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgalF1ZXJ5IFZhbGlkYXRlIHJ1bGUgdG8gYmUgdXNlZCB3aGVuIHlvdSBvbmx5XHJcbiAgICAgICAgLy8vIGhhdmUgYSBtaW5pbXVtIHZhbHVlLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwibWF4UnVsZU5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGpRdWVyeSBWYWxpZGF0ZSBydWxlIHRvIGJlIHVzZWQgd2hlbiB5b3Ugb25seVxyXG4gICAgICAgIC8vLyBoYXZlIGEgbWF4aW11bSB2YWx1ZS48L3BhcmFtPlxyXG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cIm1pbk1heFJ1bGVOYW1lXCIgdHlwZT1cIlN0cmluZ1wiPlRoZSBuYW1lIG9mIHRoZSBqUXVlcnkgVmFsaWRhdGUgcnVsZSB0byBiZSB1c2VkIHdoZW4geW91XHJcbiAgICAgICAgLy8vIGhhdmUgYm90aCBhIG1pbmltdW0gYW5kIG1heGltdW0gdmFsdWUuPC9wYXJhbT5cclxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJtaW5BdHRyaWJ1dGVcIiB0eXBlPVwiU3RyaW5nXCIgb3B0aW9uYWw9XCJ0cnVlXCI+W09wdGlvbmFsXSBUaGUgbmFtZSBvZiB0aGUgSFRNTCBhdHRyaWJ1dGUgdGhhdFxyXG4gICAgICAgIC8vLyBjb250YWlucyB0aGUgbWluaW11bSB2YWx1ZS4gVGhlIGRlZmF1bHQgaXMgXCJtaW5cIi48L3BhcmFtPlxyXG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cIm1heEF0dHJpYnV0ZVwiIHR5cGU9XCJTdHJpbmdcIiBvcHRpb25hbD1cInRydWVcIj5bT3B0aW9uYWxdIFRoZSBuYW1lIG9mIHRoZSBIVE1MIGF0dHJpYnV0ZSB0aGF0XHJcbiAgICAgICAgLy8vIGNvbnRhaW5zIHRoZSBtYXhpbXVtIHZhbHVlLiBUaGUgZGVmYXVsdCBpcyBcIm1heFwiLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxyZXR1cm5zIHR5cGU9XCJqUXVlcnkudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzXCIgLz5cclxuICAgICAgICByZXR1cm4gdGhpcy5hZGQoYWRhcHRlck5hbWUsIFttaW5BdHRyaWJ1dGUgfHwgXCJtaW5cIiwgbWF4QXR0cmlidXRlIHx8IFwibWF4XCJdLCBmdW5jdGlvbiAob3B0aW9ucykge1xyXG4gICAgICAgICAgICB2YXIgbWluID0gb3B0aW9ucy5wYXJhbXMubWluLFxyXG4gICAgICAgICAgICAgICAgbWF4ID0gb3B0aW9ucy5wYXJhbXMubWF4O1xyXG5cclxuICAgICAgICAgICAgaWYgKG1pbiAmJiBtYXgpIHtcclxuICAgICAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgbWluTWF4UnVsZU5hbWUsIFttaW4sIG1heF0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGVsc2UgaWYgKG1pbikge1xyXG4gICAgICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBtaW5SdWxlTmFtZSwgbWluKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmIChtYXgpIHtcclxuICAgICAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgbWF4UnVsZU5hbWUsIG1heCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuICAgIH07XHJcblxyXG4gICAgYWRhcHRlcnMuYWRkU2luZ2xlVmFsID0gZnVuY3Rpb24gKGFkYXB0ZXJOYW1lLCBhdHRyaWJ1dGUsIHJ1bGVOYW1lKSB7XHJcbiAgICAgICAgLy8vIDxzdW1tYXJ5PkFkZHMgYSBuZXcgYWRhcHRlciB0byBjb252ZXJ0IHVub2J0cnVzaXZlIEhUTUwgaW50byBhIGpRdWVyeSBWYWxpZGF0ZSB2YWxpZGF0aW9uLCB3aGVyZVxyXG4gICAgICAgIC8vLyB0aGUgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24gcnVsZSBoYXMgYSBzaW5nbGUgdmFsdWUuPC9zdW1tYXJ5PlxyXG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImFkYXB0ZXJOYW1lXCIgdHlwZT1cIlN0cmluZ1wiPlRoZSBuYW1lIG9mIHRoZSBhZGFwdGVyIHRvIGJlIGFkZGVkLiBUaGlzIG1hdGNoZXMgdGhlIG5hbWUgdXNlZFxyXG4gICAgICAgIC8vLyBpbiB0aGUgZGF0YS12YWwtbm5ubiBIVE1MIGF0dHJpYnV0ZSh3aGVyZSBubm5uIGlzIHRoZSBhZGFwdGVyIG5hbWUpLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiYXR0cmlidXRlXCIgdHlwZT1cIlN0cmluZ1wiPltPcHRpb25hbF0gVGhlIG5hbWUgb2YgdGhlIEhUTUwgYXR0cmlidXRlIHRoYXQgY29udGFpbnMgdGhlIHZhbHVlLlxyXG4gICAgICAgIC8vLyBUaGUgZGVmYXVsdCBpcyBcInZhbFwiLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwicnVsZU5hbWVcIiB0eXBlPVwiU3RyaW5nXCIgb3B0aW9uYWw9XCJ0cnVlXCI+W09wdGlvbmFsXSBUaGUgbmFtZSBvZiB0aGUgalF1ZXJ5IFZhbGlkYXRlIHJ1bGUuIElmIG5vdCBwcm92aWRlZCwgdGhlIHZhbHVlXHJcbiAgICAgICAgLy8vIG9mIGFkYXB0ZXJOYW1lIHdpbGwgYmUgdXNlZCBpbnN0ZWFkLjwvcGFyYW0+XHJcbiAgICAgICAgLy8vIDxyZXR1cm5zIHR5cGU9XCJqUXVlcnkudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzXCIgLz5cclxuICAgICAgICByZXR1cm4gdGhpcy5hZGQoYWRhcHRlck5hbWUsIFthdHRyaWJ1dGUgfHwgXCJ2YWxcIl0sIGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgcnVsZU5hbWUgfHwgYWRhcHRlck5hbWUsIG9wdGlvbnMucGFyYW1zW2F0dHJpYnV0ZV0pO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfTtcclxuXHJcbiAgICAkalF2YWwuYWRkTWV0aG9kKFwiX19kdW1teV9fXCIsIGZ1bmN0aW9uICh2YWx1ZSwgZWxlbWVudCwgcGFyYW1zKSB7XHJcbiAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICB9KTtcclxuXHJcbiAgICAkalF2YWwuYWRkTWV0aG9kKFwicmVnZXhcIiwgZnVuY3Rpb24gKHZhbHVlLCBlbGVtZW50LCBwYXJhbXMpIHtcclxuICAgICAgICB2YXIgbWF0Y2g7XHJcbiAgICAgICAgaWYgKHRoaXMub3B0aW9uYWwoZWxlbWVudCkpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBtYXRjaCA9IG5ldyBSZWdFeHAocGFyYW1zKS5leGVjKHZhbHVlKTtcclxuICAgICAgICByZXR1cm4gKG1hdGNoICYmIChtYXRjaC5pbmRleCA9PT0gMCkgJiYgKG1hdGNoWzBdLmxlbmd0aCA9PT0gdmFsdWUubGVuZ3RoKSk7XHJcbiAgICB9KTtcclxuXHJcbiAgICAkalF2YWwuYWRkTWV0aG9kKFwibm9uYWxwaGFtaW5cIiwgZnVuY3Rpb24gKHZhbHVlLCBlbGVtZW50LCBub25hbHBoYW1pbikge1xyXG4gICAgICAgIHZhciBtYXRjaDtcclxuICAgICAgICBpZiAobm9uYWxwaGFtaW4pIHtcclxuICAgICAgICAgICAgbWF0Y2ggPSB2YWx1ZS5tYXRjaCgvXFxXL2cpO1xyXG4gICAgICAgICAgICBtYXRjaCA9IG1hdGNoICYmIG1hdGNoLmxlbmd0aCA+PSBub25hbHBoYW1pbjtcclxuICAgICAgICB9XHJcbiAgICAgICAgcmV0dXJuIG1hdGNoO1xyXG4gICAgfSk7XHJcblxyXG4gICAgaWYgKCRqUXZhbC5tZXRob2RzLmV4dGVuc2lvbikge1xyXG4gICAgICAgIGFkYXB0ZXJzLmFkZFNpbmdsZVZhbChcImFjY2VwdFwiLCBcIm1pbXR5cGVcIik7XHJcbiAgICAgICAgYWRhcHRlcnMuYWRkU2luZ2xlVmFsKFwiZXh0ZW5zaW9uXCIsIFwiZXh0ZW5zaW9uXCIpO1xyXG4gICAgfSBlbHNlIHtcclxuICAgICAgICAvLyBmb3IgYmFja3dhcmQgY29tcGF0aWJpbGl0eSwgd2hlbiB0aGUgJ2V4dGVuc2lvbicgdmFsaWRhdGlvbiBtZXRob2QgZG9lcyBub3QgZXhpc3QsIHN1Y2ggYXMgd2l0aCB2ZXJzaW9uc1xyXG4gICAgICAgIC8vIG9mIEpRdWVyeSBWYWxpZGF0aW9uIHBsdWdpbiBwcmlvciB0byAxLjEwLCB3ZSBzaG91bGQgdXNlIHRoZSAnYWNjZXB0JyBtZXRob2QgZm9yXHJcbiAgICAgICAgLy8gdmFsaWRhdGluZyB0aGUgZXh0ZW5zaW9uLCBhbmQgaWdub3JlIG1pbWUtdHlwZSB2YWxpZGF0aW9ucyBhcyB0aGV5IGFyZSBub3Qgc3VwcG9ydGVkLlxyXG4gICAgICAgIGFkYXB0ZXJzLmFkZFNpbmdsZVZhbChcImV4dGVuc2lvblwiLCBcImV4dGVuc2lvblwiLCBcImFjY2VwdFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBhZGFwdGVycy5hZGRTaW5nbGVWYWwoXCJyZWdleFwiLCBcInBhdHRlcm5cIik7XHJcbiAgICBhZGFwdGVycy5hZGRCb29sKFwiY3JlZGl0Y2FyZFwiKS5hZGRCb29sKFwiZGF0ZVwiKS5hZGRCb29sKFwiZGlnaXRzXCIpLmFkZEJvb2woXCJlbWFpbFwiKS5hZGRCb29sKFwibnVtYmVyXCIpLmFkZEJvb2woXCJ1cmxcIik7XHJcbiAgICBhZGFwdGVycy5hZGRNaW5NYXgoXCJsZW5ndGhcIiwgXCJtaW5sZW5ndGhcIiwgXCJtYXhsZW5ndGhcIiwgXCJyYW5nZWxlbmd0aFwiKS5hZGRNaW5NYXgoXCJyYW5nZVwiLCBcIm1pblwiLCBcIm1heFwiLCBcInJhbmdlXCIpO1xyXG4gICAgYWRhcHRlcnMuYWRkTWluTWF4KFwibWlubGVuZ3RoXCIsIFwibWlubGVuZ3RoXCIpLmFkZE1pbk1heChcIm1heGxlbmd0aFwiLCBcIm1pbmxlbmd0aFwiLCBcIm1heGxlbmd0aFwiKTtcclxuICAgIGFkYXB0ZXJzLmFkZChcImVxdWFsdG9cIiwgW1wib3RoZXJcIl0sIGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgdmFyIHByZWZpeCA9IGdldE1vZGVsUHJlZml4KG9wdGlvbnMuZWxlbWVudC5uYW1lKSxcclxuICAgICAgICAgICAgb3RoZXIgPSBvcHRpb25zLnBhcmFtcy5vdGhlcixcclxuICAgICAgICAgICAgZnVsbE90aGVyTmFtZSA9IGFwcGVuZE1vZGVsUHJlZml4KG90aGVyLCBwcmVmaXgpLFxyXG4gICAgICAgICAgICBlbGVtZW50ID0gJChvcHRpb25zLmZvcm0pLmZpbmQoXCI6aW5wdXRcIikuZmlsdGVyKFwiW25hbWU9J1wiICsgZXNjYXBlQXR0cmlidXRlVmFsdWUoZnVsbE90aGVyTmFtZSkgKyBcIiddXCIpWzBdO1xyXG5cclxuICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIFwiZXF1YWxUb1wiLCBlbGVtZW50KTtcclxuICAgIH0pO1xyXG4gICAgYWRhcHRlcnMuYWRkKFwicmVxdWlyZWRcIiwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcclxuICAgICAgICAvLyBqUXVlcnkgVmFsaWRhdGUgZXF1YXRlcyBcInJlcXVpcmVkXCIgd2l0aCBcIm1hbmRhdG9yeVwiIGZvciBjaGVja2JveCBlbGVtZW50c1xyXG4gICAgICAgIGlmIChvcHRpb25zLmVsZW1lbnQudGFnTmFtZS50b1VwcGVyQ2FzZSgpICE9PSBcIklOUFVUXCIgfHwgb3B0aW9ucy5lbGVtZW50LnR5cGUudG9VcHBlckNhc2UoKSAhPT0gXCJDSEVDS0JPWFwiKSB7XHJcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJyZXF1aXJlZFwiLCB0cnVlKTtcclxuICAgICAgICB9XHJcbiAgICB9KTtcclxuICAgIGFkYXB0ZXJzLmFkZChcInJlbW90ZVwiLCBbXCJ1cmxcIiwgXCJ0eXBlXCIsIFwiYWRkaXRpb25hbGZpZWxkc1wiXSwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcclxuICAgICAgICB2YXIgdmFsdWUgPSB7XHJcbiAgICAgICAgICAgIHVybDogb3B0aW9ucy5wYXJhbXMudXJsLFxyXG4gICAgICAgICAgICB0eXBlOiBvcHRpb25zLnBhcmFtcy50eXBlIHx8IFwiR0VUXCIsXHJcbiAgICAgICAgICAgIGRhdGE6IHt9XHJcbiAgICAgICAgfSxcclxuICAgICAgICAgICAgcHJlZml4ID0gZ2V0TW9kZWxQcmVmaXgob3B0aW9ucy5lbGVtZW50Lm5hbWUpO1xyXG5cclxuICAgICAgICAkLmVhY2goc3BsaXRBbmRUcmltKG9wdGlvbnMucGFyYW1zLmFkZGl0aW9uYWxmaWVsZHMgfHwgb3B0aW9ucy5lbGVtZW50Lm5hbWUpLCBmdW5jdGlvbiAoaSwgZmllbGROYW1lKSB7XHJcbiAgICAgICAgICAgIHZhciBwYXJhbU5hbWUgPSBhcHBlbmRNb2RlbFByZWZpeChmaWVsZE5hbWUsIHByZWZpeCk7XHJcbiAgICAgICAgICAgIHZhbHVlLmRhdGFbcGFyYW1OYW1lXSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiAkKG9wdGlvbnMuZm9ybSkuZmluZChcIjppbnB1dFwiKS5maWx0ZXIoXCJbbmFtZT0nXCIgKyBlc2NhcGVBdHRyaWJ1dGVWYWx1ZShwYXJhbU5hbWUpICsgXCInXVwiKS52YWwoKTtcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBcInJlbW90ZVwiLCB2YWx1ZSk7XHJcbiAgICB9KTtcclxuICAgIGFkYXB0ZXJzLmFkZChcInBhc3N3b3JkXCIsIFtcIm1pblwiLCBcIm5vbmFscGhhbWluXCIsIFwicmVnZXhcIl0sIGZ1bmN0aW9uIChvcHRpb25zKSB7XHJcbiAgICAgICAgaWYgKG9wdGlvbnMucGFyYW1zLm1pbikge1xyXG4gICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIFwibWlubGVuZ3RoXCIsIG9wdGlvbnMucGFyYW1zLm1pbik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGlmIChvcHRpb25zLnBhcmFtcy5ub25hbHBoYW1pbikge1xyXG4gICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIFwibm9uYWxwaGFtaW5cIiwgb3B0aW9ucy5wYXJhbXMubm9uYWxwaGFtaW4pO1xyXG4gICAgICAgIH1cclxuICAgICAgICBpZiAob3B0aW9ucy5wYXJhbXMucmVnZXgpIHtcclxuICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBcInJlZ2V4XCIsIG9wdGlvbnMucGFyYW1zLnJlZ2V4KTtcclxuICAgICAgICB9XHJcbiAgICB9KTtcclxuXHJcbiAgICAkKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAkalF2YWwudW5vYnRydXNpdmUucGFyc2UoZG9jdW1lbnQpO1xyXG4gICAgfSk7XHJcbn0oalF1ZXJ5KSk7IiwiKGZ1bmN0aW9uICgkKSB7XHJcblxyXG4gICAgJC52YWxpZGF0b3IuYWRkTWV0aG9kKFwib3B0aW9ucmVxdWlyZWRcIiwgZnVuY3Rpb24gKHZhbHVlLCBlbGVtZW50LCBwYXJhbSkge1xyXG4gICAgICAgIHZhciBpc1ZhbGlkID0gdHJ1ZTtcclxuXHJcbiAgICAgICAgaWYgKCQoZWxlbWVudCkuaXMoXCJpbnB1dFwiKSkge1xyXG4gICAgICAgICAgICB2YXIgcGFyZW50ID0gJChlbGVtZW50KS5jbG9zZXN0KFwib2xcIik7XHJcblxyXG4gICAgICAgICAgICBpc1ZhbGlkID0gcGFyZW50LmZpbmQoXCJpbnB1dDpjaGVja2VkXCIpLmxlbmd0aCA+IDA7XHJcbiAgICAgICAgICAgIHBhcmVudC50b2dnbGVDbGFzcyhcImlucHV0LXZhbGlkYXRpb24tZXJyb3JcIiwgIWlzVmFsaWQpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIGlmICgkKGVsZW1lbnQpLmlzKFwic2VsZWN0XCIpKSB7XHJcbiAgICAgICAgICAgIHZhciB2ID0gJChlbGVtZW50KS52YWwoKTtcclxuICAgICAgICAgICAgaXNWYWxpZCA9ICEhdiAmJiB2Lmxlbmd0aCA+IDA7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICByZXR1cm4gaXNWYWxpZDtcclxuICAgIH0sIFwiQW4gb3B0aW9uIGlzIHJlcXVpcmVkXCIpO1xyXG5cclxuICAgICQudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzLmFkZEJvb2woXCJtYW5kYXRvcnlcIiwgXCJyZXF1aXJlZFwiKTtcclxuICAgICQudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzLmFkZEJvb2woXCJvcHRpb25yZXF1aXJlZFwiKTtcclxufShqUXVlcnkpKTsiXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=