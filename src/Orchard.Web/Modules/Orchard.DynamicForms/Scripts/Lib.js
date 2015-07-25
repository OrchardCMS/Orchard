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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImpxdWVyeS52YWxpZGF0ZS5qcyIsImpxdWVyeS52YWxpZGF0ZS51bm9idHJ1c2l2ZS5hZGRpdGlvbmFsLmpzIiwianF1ZXJ5LnZhbGlkYXRlLnVub2J0cnVzaXZlLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDN3RDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3JCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6IkxpYi5qcyIsInNvdXJjZXNDb250ZW50IjpbIi8qIE5VR0VUOiBCRUdJTiBMSUNFTlNFIFRFWFRcbiAqXG4gKiBNaWNyb3NvZnQgZ3JhbnRzIHlvdSB0aGUgcmlnaHQgdG8gdXNlIHRoZXNlIHNjcmlwdCBmaWxlcyBmb3IgdGhlIHNvbGVcbiAqIHB1cnBvc2Ugb2YgZWl0aGVyOiAoaSkgaW50ZXJhY3RpbmcgdGhyb3VnaCB5b3VyIGJyb3dzZXIgd2l0aCB0aGUgTWljcm9zb2Z0XG4gKiB3ZWJzaXRlIG9yIG9ubGluZSBzZXJ2aWNlLCBzdWJqZWN0IHRvIHRoZSBhcHBsaWNhYmxlIGxpY2Vuc2luZyBvciB1c2VcbiAqIHRlcm1zOyBvciAoaWkpIHVzaW5nIHRoZSBmaWxlcyBhcyBpbmNsdWRlZCB3aXRoIGEgTWljcm9zb2Z0IHByb2R1Y3Qgc3ViamVjdFxuICogdG8gdGhhdCBwcm9kdWN0J3MgbGljZW5zZSB0ZXJtcy4gTWljcm9zb2Z0IHJlc2VydmVzIGFsbCBvdGhlciByaWdodHMgdG8gdGhlXG4gKiBmaWxlcyBub3QgZXhwcmVzc2x5IGdyYW50ZWQgYnkgTWljcm9zb2Z0LCB3aGV0aGVyIGJ5IGltcGxpY2F0aW9uLCBlc3RvcHBlbFxuICogb3Igb3RoZXJ3aXNlLiBJbnNvZmFyIGFzIGEgc2NyaXB0IGZpbGUgaXMgZHVhbCBsaWNlbnNlZCB1bmRlciBHUEwsXG4gKiBNaWNyb3NvZnQgbmVpdGhlciB0b29rIHRoZSBjb2RlIHVuZGVyIEdQTCBub3IgZGlzdHJpYnV0ZXMgaXQgdGhlcmV1bmRlciBidXRcbiAqIHVuZGVyIHRoZSB0ZXJtcyBzZXQgb3V0IGluIHRoaXMgcGFyYWdyYXBoLiBBbGwgbm90aWNlcyBhbmQgbGljZW5zZXNcbiAqIGJlbG93IGFyZSBmb3IgaW5mb3JtYXRpb25hbCBwdXJwb3NlcyBvbmx5LlxuICpcbiAqIE5VR0VUOiBFTkQgTElDRU5TRSBURVhUICovXG4vKiFcbiAqIGpRdWVyeSBWYWxpZGF0aW9uIFBsdWdpbiAxLjExLjFcbiAqXG4gKiBodHRwOi8vYmFzc2lzdGFuY2UuZGUvanF1ZXJ5LXBsdWdpbnMvanF1ZXJ5LXBsdWdpbi12YWxpZGF0aW9uL1xuICogaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb25cbiAqXG4gKiBDb3B5cmlnaHQgMjAxMyBKw7ZybiBaYWVmZmVyZXJcbiAqIFJlbGVhc2VkIHVuZGVyIHRoZSBNSVQgbGljZW5zZTpcbiAqICAgaHR0cDovL3d3dy5vcGVuc291cmNlLm9yZy9saWNlbnNlcy9taXQtbGljZW5zZS5waHBcbiAqL1xuXG4oZnVuY3Rpb24oJCkge1xuXG4kLmV4dGVuZCgkLmZuLCB7XG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL3ZhbGlkYXRlXG5cdHZhbGlkYXRlOiBmdW5jdGlvbiggb3B0aW9ucyApIHtcblxuXHRcdC8vIGlmIG5vdGhpbmcgaXMgc2VsZWN0ZWQsIHJldHVybiBub3RoaW5nOyBjYW4ndCBjaGFpbiBhbnl3YXlcblx0XHRpZiAoICF0aGlzLmxlbmd0aCApIHtcblx0XHRcdGlmICggb3B0aW9ucyAmJiBvcHRpb25zLmRlYnVnICYmIHdpbmRvdy5jb25zb2xlICkge1xuXHRcdFx0XHRjb25zb2xlLndhcm4oIFwiTm90aGluZyBzZWxlY3RlZCwgY2FuJ3QgdmFsaWRhdGUsIHJldHVybmluZyBub3RoaW5nLlwiICk7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm47XG5cdFx0fVxuXG5cdFx0Ly8gY2hlY2sgaWYgYSB2YWxpZGF0b3IgZm9yIHRoaXMgZm9ybSB3YXMgYWxyZWFkeSBjcmVhdGVkXG5cdFx0dmFyIHZhbGlkYXRvciA9ICQuZGF0YSggdGhpc1swXSwgXCJ2YWxpZGF0b3JcIiApO1xuXHRcdGlmICggdmFsaWRhdG9yICkge1xuXHRcdFx0cmV0dXJuIHZhbGlkYXRvcjtcblx0XHR9XG5cblx0XHQvLyBBZGQgbm92YWxpZGF0ZSB0YWcgaWYgSFRNTDUuXG5cdFx0dGhpcy5hdHRyKCBcIm5vdmFsaWRhdGVcIiwgXCJub3ZhbGlkYXRlXCIgKTtcblxuXHRcdHZhbGlkYXRvciA9IG5ldyAkLnZhbGlkYXRvciggb3B0aW9ucywgdGhpc1swXSApO1xuXHRcdCQuZGF0YSggdGhpc1swXSwgXCJ2YWxpZGF0b3JcIiwgdmFsaWRhdG9yICk7XG5cblx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5ncy5vbnN1Ym1pdCApIHtcblxuXHRcdFx0dGhpcy52YWxpZGF0ZURlbGVnYXRlKCBcIjpzdWJtaXRcIiwgXCJjbGlja1wiLCBmdW5jdGlvbiggZXZlbnQgKSB7XG5cdFx0XHRcdGlmICggdmFsaWRhdG9yLnNldHRpbmdzLnN1Ym1pdEhhbmRsZXIgKSB7XG5cdFx0XHRcdFx0dmFsaWRhdG9yLnN1Ym1pdEJ1dHRvbiA9IGV2ZW50LnRhcmdldDtcblx0XHRcdFx0fVxuXHRcdFx0XHQvLyBhbGxvdyBzdXBwcmVzc2luZyB2YWxpZGF0aW9uIGJ5IGFkZGluZyBhIGNhbmNlbCBjbGFzcyB0byB0aGUgc3VibWl0IGJ1dHRvblxuXHRcdFx0XHRpZiAoICQoZXZlbnQudGFyZ2V0KS5oYXNDbGFzcyhcImNhbmNlbFwiKSApIHtcblx0XHRcdFx0XHR2YWxpZGF0b3IuY2FuY2VsU3VibWl0ID0gdHJ1ZTtcblx0XHRcdFx0fVxuXG5cdFx0XHRcdC8vIGFsbG93IHN1cHByZXNzaW5nIHZhbGlkYXRpb24gYnkgYWRkaW5nIHRoZSBodG1sNSBmb3Jtbm92YWxpZGF0ZSBhdHRyaWJ1dGUgdG8gdGhlIHN1Ym1pdCBidXR0b25cblx0XHRcdFx0aWYgKCAkKGV2ZW50LnRhcmdldCkuYXR0cihcImZvcm1ub3ZhbGlkYXRlXCIpICE9PSB1bmRlZmluZWQgKSB7XG5cdFx0XHRcdFx0dmFsaWRhdG9yLmNhbmNlbFN1Ym1pdCA9IHRydWU7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXG5cdFx0XHQvLyB2YWxpZGF0ZSB0aGUgZm9ybSBvbiBzdWJtaXRcblx0XHRcdHRoaXMuc3VibWl0KCBmdW5jdGlvbiggZXZlbnQgKSB7XG5cdFx0XHRcdGlmICggdmFsaWRhdG9yLnNldHRpbmdzLmRlYnVnICkge1xuXHRcdFx0XHRcdC8vIHByZXZlbnQgZm9ybSBzdWJtaXQgdG8gYmUgYWJsZSB0byBzZWUgY29uc29sZSBvdXRwdXRcblx0XHRcdFx0XHRldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHR9XG5cdFx0XHRcdGZ1bmN0aW9uIGhhbmRsZSgpIHtcblx0XHRcdFx0XHR2YXIgaGlkZGVuO1xuXHRcdFx0XHRcdGlmICggdmFsaWRhdG9yLnNldHRpbmdzLnN1Ym1pdEhhbmRsZXIgKSB7XG5cdFx0XHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zdWJtaXRCdXR0b24gKSB7XG5cdFx0XHRcdFx0XHRcdC8vIGluc2VydCBhIGhpZGRlbiBpbnB1dCBhcyBhIHJlcGxhY2VtZW50IGZvciB0aGUgbWlzc2luZyBzdWJtaXQgYnV0dG9uXG5cdFx0XHRcdFx0XHRcdGhpZGRlbiA9ICQoXCI8aW5wdXQgdHlwZT0naGlkZGVuJy8+XCIpLmF0dHIoXCJuYW1lXCIsIHZhbGlkYXRvci5zdWJtaXRCdXR0b24ubmFtZSkudmFsKCAkKHZhbGlkYXRvci5zdWJtaXRCdXR0b24pLnZhbCgpICkuYXBwZW5kVG8odmFsaWRhdG9yLmN1cnJlbnRGb3JtKTtcblx0XHRcdFx0XHRcdH1cblx0XHRcdFx0XHRcdHZhbGlkYXRvci5zZXR0aW5ncy5zdWJtaXRIYW5kbGVyLmNhbGwoIHZhbGlkYXRvciwgdmFsaWRhdG9yLmN1cnJlbnRGb3JtLCBldmVudCApO1xuXHRcdFx0XHRcdFx0aWYgKCB2YWxpZGF0b3Iuc3VibWl0QnV0dG9uICkge1xuXHRcdFx0XHRcdFx0XHQvLyBhbmQgY2xlYW4gdXAgYWZ0ZXJ3YXJkczsgdGhhbmtzIHRvIG5vLWJsb2NrLXNjb3BlLCBoaWRkZW4gY2FuIGJlIHJlZmVyZW5jZWRcblx0XHRcdFx0XHRcdFx0aGlkZGVuLnJlbW92ZSgpO1xuXHRcdFx0XHRcdFx0fVxuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRyZXR1cm4gdHJ1ZTtcblx0XHRcdFx0fVxuXG5cdFx0XHRcdC8vIHByZXZlbnQgc3VibWl0IGZvciBpbnZhbGlkIGZvcm1zIG9yIGN1c3RvbSBzdWJtaXQgaGFuZGxlcnNcblx0XHRcdFx0aWYgKCB2YWxpZGF0b3IuY2FuY2VsU3VibWl0ICkge1xuXHRcdFx0XHRcdHZhbGlkYXRvci5jYW5jZWxTdWJtaXQgPSBmYWxzZTtcblx0XHRcdFx0XHRyZXR1cm4gaGFuZGxlKCk7XG5cdFx0XHRcdH1cblx0XHRcdFx0aWYgKCB2YWxpZGF0b3IuZm9ybSgpICkge1xuXHRcdFx0XHRcdGlmICggdmFsaWRhdG9yLnBlbmRpbmdSZXF1ZXN0ICkge1xuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLmZvcm1TdWJtaXR0ZWQgPSB0cnVlO1xuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRyZXR1cm4gaGFuZGxlKCk7XG5cdFx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdFx0dmFsaWRhdG9yLmZvY3VzSW52YWxpZCgpO1xuXHRcdFx0XHRcdHJldHVybiBmYWxzZTtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHZhbGlkYXRvcjtcblx0fSxcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vdmFsaWRcblx0dmFsaWQ6IGZ1bmN0aW9uKCkge1xuXHRcdGlmICggJCh0aGlzWzBdKS5pcyhcImZvcm1cIikpIHtcblx0XHRcdHJldHVybiB0aGlzLnZhbGlkYXRlKCkuZm9ybSgpO1xuXHRcdH0gZWxzZSB7XG5cdFx0XHR2YXIgdmFsaWQgPSB0cnVlO1xuXHRcdFx0dmFyIHZhbGlkYXRvciA9ICQodGhpc1swXS5mb3JtKS52YWxpZGF0ZSgpO1xuXHRcdFx0dGhpcy5lYWNoKGZ1bmN0aW9uKCkge1xuXHRcdFx0XHR2YWxpZCA9IHZhbGlkICYmIHZhbGlkYXRvci5lbGVtZW50KHRoaXMpO1xuXHRcdFx0fSk7XG5cdFx0XHRyZXR1cm4gdmFsaWQ7XG5cdFx0fVxuXHR9LFxuXHQvLyBhdHRyaWJ1dGVzOiBzcGFjZSBzZXBlcmF0ZWQgbGlzdCBvZiBhdHRyaWJ1dGVzIHRvIHJldHJpZXZlIGFuZCByZW1vdmVcblx0cmVtb3ZlQXR0cnM6IGZ1bmN0aW9uKCBhdHRyaWJ1dGVzICkge1xuXHRcdHZhciByZXN1bHQgPSB7fSxcblx0XHRcdCRlbGVtZW50ID0gdGhpcztcblx0XHQkLmVhY2goYXR0cmlidXRlcy5zcGxpdCgvXFxzLyksIGZ1bmN0aW9uKCBpbmRleCwgdmFsdWUgKSB7XG5cdFx0XHRyZXN1bHRbdmFsdWVdID0gJGVsZW1lbnQuYXR0cih2YWx1ZSk7XG5cdFx0XHQkZWxlbWVudC5yZW1vdmVBdHRyKHZhbHVlKTtcblx0XHR9KTtcblx0XHRyZXR1cm4gcmVzdWx0O1xuXHR9LFxuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9ydWxlc1xuXHRydWxlczogZnVuY3Rpb24oIGNvbW1hbmQsIGFyZ3VtZW50ICkge1xuXHRcdHZhciBlbGVtZW50ID0gdGhpc1swXTtcblxuXHRcdGlmICggY29tbWFuZCApIHtcblx0XHRcdHZhciBzZXR0aW5ncyA9ICQuZGF0YShlbGVtZW50LmZvcm0sIFwidmFsaWRhdG9yXCIpLnNldHRpbmdzO1xuXHRcdFx0dmFyIHN0YXRpY1J1bGVzID0gc2V0dGluZ3MucnVsZXM7XG5cdFx0XHR2YXIgZXhpc3RpbmdSdWxlcyA9ICQudmFsaWRhdG9yLnN0YXRpY1J1bGVzKGVsZW1lbnQpO1xuXHRcdFx0c3dpdGNoKGNvbW1hbmQpIHtcblx0XHRcdGNhc2UgXCJhZGRcIjpcblx0XHRcdFx0JC5leHRlbmQoZXhpc3RpbmdSdWxlcywgJC52YWxpZGF0b3Iubm9ybWFsaXplUnVsZShhcmd1bWVudCkpO1xuXHRcdFx0XHQvLyByZW1vdmUgbWVzc2FnZXMgZnJvbSBydWxlcywgYnV0IGFsbG93IHRoZW0gdG8gYmUgc2V0IHNlcGFyZXRlbHlcblx0XHRcdFx0ZGVsZXRlIGV4aXN0aW5nUnVsZXMubWVzc2FnZXM7XG5cdFx0XHRcdHN0YXRpY1J1bGVzW2VsZW1lbnQubmFtZV0gPSBleGlzdGluZ1J1bGVzO1xuXHRcdFx0XHRpZiAoIGFyZ3VtZW50Lm1lc3NhZ2VzICkge1xuXHRcdFx0XHRcdHNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0gPSAkLmV4dGVuZCggc2V0dGluZ3MubWVzc2FnZXNbZWxlbWVudC5uYW1lXSwgYXJndW1lbnQubWVzc2FnZXMgKTtcblx0XHRcdFx0fVxuXHRcdFx0XHRicmVhaztcblx0XHRcdGNhc2UgXCJyZW1vdmVcIjpcblx0XHRcdFx0aWYgKCAhYXJndW1lbnQgKSB7XG5cdFx0XHRcdFx0ZGVsZXRlIHN0YXRpY1J1bGVzW2VsZW1lbnQubmFtZV07XG5cdFx0XHRcdFx0cmV0dXJuIGV4aXN0aW5nUnVsZXM7XG5cdFx0XHRcdH1cblx0XHRcdFx0dmFyIGZpbHRlcmVkID0ge307XG5cdFx0XHRcdCQuZWFjaChhcmd1bWVudC5zcGxpdCgvXFxzLyksIGZ1bmN0aW9uKCBpbmRleCwgbWV0aG9kICkge1xuXHRcdFx0XHRcdGZpbHRlcmVkW21ldGhvZF0gPSBleGlzdGluZ1J1bGVzW21ldGhvZF07XG5cdFx0XHRcdFx0ZGVsZXRlIGV4aXN0aW5nUnVsZXNbbWV0aG9kXTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdHJldHVybiBmaWx0ZXJlZDtcblx0XHRcdH1cblx0XHR9XG5cblx0XHR2YXIgZGF0YSA9ICQudmFsaWRhdG9yLm5vcm1hbGl6ZVJ1bGVzKFxuXHRcdCQuZXh0ZW5kKFxuXHRcdFx0e30sXG5cdFx0XHQkLnZhbGlkYXRvci5jbGFzc1J1bGVzKGVsZW1lbnQpLFxuXHRcdFx0JC52YWxpZGF0b3IuYXR0cmlidXRlUnVsZXMoZWxlbWVudCksXG5cdFx0XHQkLnZhbGlkYXRvci5kYXRhUnVsZXMoZWxlbWVudCksXG5cdFx0XHQkLnZhbGlkYXRvci5zdGF0aWNSdWxlcyhlbGVtZW50KVxuXHRcdCksIGVsZW1lbnQpO1xuXG5cdFx0Ly8gbWFrZSBzdXJlIHJlcXVpcmVkIGlzIGF0IGZyb250XG5cdFx0aWYgKCBkYXRhLnJlcXVpcmVkICkge1xuXHRcdFx0dmFyIHBhcmFtID0gZGF0YS5yZXF1aXJlZDtcblx0XHRcdGRlbGV0ZSBkYXRhLnJlcXVpcmVkO1xuXHRcdFx0ZGF0YSA9ICQuZXh0ZW5kKHtyZXF1aXJlZDogcGFyYW19LCBkYXRhKTtcblx0XHR9XG5cblx0XHRyZXR1cm4gZGF0YTtcblx0fVxufSk7XG5cbi8vIEN1c3RvbSBzZWxlY3RvcnNcbiQuZXh0ZW5kKCQuZXhwcltcIjpcIl0sIHtcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vYmxhbmtcblx0Ymxhbms6IGZ1bmN0aW9uKCBhICkgeyByZXR1cm4gISQudHJpbShcIlwiICsgJChhKS52YWwoKSk7IH0sXG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL2ZpbGxlZFxuXHRmaWxsZWQ6IGZ1bmN0aW9uKCBhICkgeyByZXR1cm4gISEkLnRyaW0oXCJcIiArICQoYSkudmFsKCkpOyB9LFxuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi91bmNoZWNrZWRcblx0dW5jaGVja2VkOiBmdW5jdGlvbiggYSApIHsgcmV0dXJuICEkKGEpLnByb3AoXCJjaGVja2VkXCIpOyB9XG59KTtcblxuLy8gY29uc3RydWN0b3IgZm9yIHZhbGlkYXRvclxuJC52YWxpZGF0b3IgPSBmdW5jdGlvbiggb3B0aW9ucywgZm9ybSApIHtcblx0dGhpcy5zZXR0aW5ncyA9ICQuZXh0ZW5kKCB0cnVlLCB7fSwgJC52YWxpZGF0b3IuZGVmYXVsdHMsIG9wdGlvbnMgKTtcblx0dGhpcy5jdXJyZW50Rm9ybSA9IGZvcm07XG5cdHRoaXMuaW5pdCgpO1xufTtcblxuJC52YWxpZGF0b3IuZm9ybWF0ID0gZnVuY3Rpb24oIHNvdXJjZSwgcGFyYW1zICkge1xuXHRpZiAoIGFyZ3VtZW50cy5sZW5ndGggPT09IDEgKSB7XG5cdFx0cmV0dXJuIGZ1bmN0aW9uKCkge1xuXHRcdFx0dmFyIGFyZ3MgPSAkLm1ha2VBcnJheShhcmd1bWVudHMpO1xuXHRcdFx0YXJncy51bnNoaWZ0KHNvdXJjZSk7XG5cdFx0XHRyZXR1cm4gJC52YWxpZGF0b3IuZm9ybWF0LmFwcGx5KCB0aGlzLCBhcmdzICk7XG5cdFx0fTtcblx0fVxuXHRpZiAoIGFyZ3VtZW50cy5sZW5ndGggPiAyICYmIHBhcmFtcy5jb25zdHJ1Y3RvciAhPT0gQXJyYXkgICkge1xuXHRcdHBhcmFtcyA9ICQubWFrZUFycmF5KGFyZ3VtZW50cykuc2xpY2UoMSk7XG5cdH1cblx0aWYgKCBwYXJhbXMuY29uc3RydWN0b3IgIT09IEFycmF5ICkge1xuXHRcdHBhcmFtcyA9IFsgcGFyYW1zIF07XG5cdH1cblx0JC5lYWNoKHBhcmFtcywgZnVuY3Rpb24oIGksIG4gKSB7XG5cdFx0c291cmNlID0gc291cmNlLnJlcGxhY2UoIG5ldyBSZWdFeHAoXCJcXFxce1wiICsgaSArIFwiXFxcXH1cIiwgXCJnXCIpLCBmdW5jdGlvbigpIHtcblx0XHRcdHJldHVybiBuO1xuXHRcdH0pO1xuXHR9KTtcblx0cmV0dXJuIHNvdXJjZTtcbn07XG5cbiQuZXh0ZW5kKCQudmFsaWRhdG9yLCB7XG5cblx0ZGVmYXVsdHM6IHtcblx0XHRtZXNzYWdlczoge30sXG5cdFx0Z3JvdXBzOiB7fSxcblx0XHRydWxlczoge30sXG5cdFx0ZXJyb3JDbGFzczogXCJlcnJvclwiLFxuXHRcdHZhbGlkQ2xhc3M6IFwidmFsaWRcIixcblx0XHRlcnJvckVsZW1lbnQ6IFwibGFiZWxcIixcblx0XHRmb2N1c0ludmFsaWQ6IHRydWUsXG5cdFx0ZXJyb3JDb250YWluZXI6ICQoW10pLFxuXHRcdGVycm9yTGFiZWxDb250YWluZXI6ICQoW10pLFxuXHRcdG9uc3VibWl0OiB0cnVlLFxuXHRcdGlnbm9yZTogXCI6aGlkZGVuXCIsXG5cdFx0aWdub3JlVGl0bGU6IGZhbHNlLFxuXHRcdG9uZm9jdXNpbjogZnVuY3Rpb24oIGVsZW1lbnQsIGV2ZW50ICkge1xuXHRcdFx0dGhpcy5sYXN0QWN0aXZlID0gZWxlbWVudDtcblxuXHRcdFx0Ly8gaGlkZSBlcnJvciBsYWJlbCBhbmQgcmVtb3ZlIGVycm9yIGNsYXNzIG9uIGZvY3VzIGlmIGVuYWJsZWRcblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5mb2N1c0NsZWFudXAgJiYgIXRoaXMuYmxvY2tGb2N1c0NsZWFudXAgKSB7XG5cdFx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy51bmhpZ2hsaWdodCApIHtcblx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLnVuaGlnaGxpZ2h0LmNhbGwoIHRoaXMsIGVsZW1lbnQsIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcywgdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICk7XG5cdFx0XHRcdH1cblx0XHRcdFx0dGhpcy5hZGRXcmFwcGVyKHRoaXMuZXJyb3JzRm9yKGVsZW1lbnQpKS5oaWRlKCk7XG5cdFx0XHR9XG5cdFx0fSxcblx0XHRvbmZvY3Vzb3V0OiBmdW5jdGlvbiggZWxlbWVudCwgZXZlbnQgKSB7XG5cdFx0XHRpZiAoICF0aGlzLmNoZWNrYWJsZShlbGVtZW50KSAmJiAoZWxlbWVudC5uYW1lIGluIHRoaXMuc3VibWl0dGVkIHx8ICF0aGlzLm9wdGlvbmFsKGVsZW1lbnQpKSApIHtcblx0XHRcdFx0dGhpcy5lbGVtZW50KGVsZW1lbnQpO1xuXHRcdFx0fVxuXHRcdH0sXG5cdFx0b25rZXl1cDogZnVuY3Rpb24oIGVsZW1lbnQsIGV2ZW50ICkge1xuXHRcdFx0aWYgKCBldmVudC53aGljaCA9PT0gOSAmJiB0aGlzLmVsZW1lbnRWYWx1ZShlbGVtZW50KSA9PT0gXCJcIiApIHtcblx0XHRcdFx0cmV0dXJuO1xuXHRcdFx0fSBlbHNlIGlmICggZWxlbWVudC5uYW1lIGluIHRoaXMuc3VibWl0dGVkIHx8IGVsZW1lbnQgPT09IHRoaXMubGFzdEVsZW1lbnQgKSB7XG5cdFx0XHRcdHRoaXMuZWxlbWVudChlbGVtZW50KTtcblx0XHRcdH1cblx0XHR9LFxuXHRcdG9uY2xpY2s6IGZ1bmN0aW9uKCBlbGVtZW50LCBldmVudCApIHtcblx0XHRcdC8vIGNsaWNrIG9uIHNlbGVjdHMsIHJhZGlvYnV0dG9ucyBhbmQgY2hlY2tib3hlc1xuXHRcdFx0aWYgKCBlbGVtZW50Lm5hbWUgaW4gdGhpcy5zdWJtaXR0ZWQgKSB7XG5cdFx0XHRcdHRoaXMuZWxlbWVudChlbGVtZW50KTtcblx0XHRcdH1cblx0XHRcdC8vIG9yIG9wdGlvbiBlbGVtZW50cywgY2hlY2sgcGFyZW50IHNlbGVjdCBpbiB0aGF0IGNhc2Vcblx0XHRcdGVsc2UgaWYgKCBlbGVtZW50LnBhcmVudE5vZGUubmFtZSBpbiB0aGlzLnN1Ym1pdHRlZCApIHtcblx0XHRcdFx0dGhpcy5lbGVtZW50KGVsZW1lbnQucGFyZW50Tm9kZSk7XG5cdFx0XHR9XG5cdFx0fSxcblx0XHRoaWdobGlnaHQ6IGZ1bmN0aW9uKCBlbGVtZW50LCBlcnJvckNsYXNzLCB2YWxpZENsYXNzICkge1xuXHRcdFx0aWYgKCBlbGVtZW50LnR5cGUgPT09IFwicmFkaW9cIiApIHtcblx0XHRcdFx0dGhpcy5maW5kQnlOYW1lKGVsZW1lbnQubmFtZSkuYWRkQ2xhc3MoZXJyb3JDbGFzcykucmVtb3ZlQ2xhc3ModmFsaWRDbGFzcyk7XG5cdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHQkKGVsZW1lbnQpLmFkZENsYXNzKGVycm9yQ2xhc3MpLnJlbW92ZUNsYXNzKHZhbGlkQ2xhc3MpO1xuXHRcdFx0fVxuXHRcdH0sXG5cdFx0dW5oaWdobGlnaHQ6IGZ1bmN0aW9uKCBlbGVtZW50LCBlcnJvckNsYXNzLCB2YWxpZENsYXNzICkge1xuXHRcdFx0aWYgKCBlbGVtZW50LnR5cGUgPT09IFwicmFkaW9cIiApIHtcblx0XHRcdFx0dGhpcy5maW5kQnlOYW1lKGVsZW1lbnQubmFtZSkucmVtb3ZlQ2xhc3MoZXJyb3JDbGFzcykuYWRkQ2xhc3ModmFsaWRDbGFzcyk7XG5cdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHQkKGVsZW1lbnQpLnJlbW92ZUNsYXNzKGVycm9yQ2xhc3MpLmFkZENsYXNzKHZhbGlkQ2xhc3MpO1xuXHRcdFx0fVxuXHRcdH1cblx0fSxcblxuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3Ivc2V0RGVmYXVsdHNcblx0c2V0RGVmYXVsdHM6IGZ1bmN0aW9uKCBzZXR0aW5ncyApIHtcblx0XHQkLmV4dGVuZCggJC52YWxpZGF0b3IuZGVmYXVsdHMsIHNldHRpbmdzICk7XG5cdH0sXG5cblx0bWVzc2FnZXM6IHtcblx0XHRyZXF1aXJlZDogXCJUaGlzIGZpZWxkIGlzIHJlcXVpcmVkLlwiLFxuXHRcdHJlbW90ZTogXCJQbGVhc2UgZml4IHRoaXMgZmllbGQuXCIsXG5cdFx0ZW1haWw6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgZW1haWwgYWRkcmVzcy5cIixcblx0XHR1cmw6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgVVJMLlwiLFxuXHRcdGRhdGU6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgZGF0ZS5cIixcblx0XHRkYXRlSVNPOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIGRhdGUgKElTTykuXCIsXG5cdFx0bnVtYmVyOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIG51bWJlci5cIixcblx0XHRkaWdpdHM6IFwiUGxlYXNlIGVudGVyIG9ubHkgZGlnaXRzLlwiLFxuXHRcdGNyZWRpdGNhcmQ6IFwiUGxlYXNlIGVudGVyIGEgdmFsaWQgY3JlZGl0IGNhcmQgbnVtYmVyLlwiLFxuXHRcdGVxdWFsVG86IFwiUGxlYXNlIGVudGVyIHRoZSBzYW1lIHZhbHVlIGFnYWluLlwiLFxuXHRcdG1heGxlbmd0aDogJC52YWxpZGF0b3IuZm9ybWF0KFwiUGxlYXNlIGVudGVyIG5vIG1vcmUgdGhhbiB7MH0gY2hhcmFjdGVycy5cIiksXG5cdFx0bWlubGVuZ3RoOiAkLnZhbGlkYXRvci5mb3JtYXQoXCJQbGVhc2UgZW50ZXIgYXQgbGVhc3QgezB9IGNoYXJhY3RlcnMuXCIpLFxuXHRcdHJhbmdlbGVuZ3RoOiAkLnZhbGlkYXRvci5mb3JtYXQoXCJQbGVhc2UgZW50ZXIgYSB2YWx1ZSBiZXR3ZWVuIHswfSBhbmQgezF9IGNoYXJhY3RlcnMgbG9uZy5cIiksXG5cdFx0cmFuZ2U6ICQudmFsaWRhdG9yLmZvcm1hdChcIlBsZWFzZSBlbnRlciBhIHZhbHVlIGJldHdlZW4gezB9IGFuZCB7MX0uXCIpLFxuXHRcdG1heDogJC52YWxpZGF0b3IuZm9ybWF0KFwiUGxlYXNlIGVudGVyIGEgdmFsdWUgbGVzcyB0aGFuIG9yIGVxdWFsIHRvIHswfS5cIiksXG5cdFx0bWluOiAkLnZhbGlkYXRvci5mb3JtYXQoXCJQbGVhc2UgZW50ZXIgYSB2YWx1ZSBncmVhdGVyIHRoYW4gb3IgZXF1YWwgdG8gezB9LlwiKVxuXHR9LFxuXG5cdGF1dG9DcmVhdGVSYW5nZXM6IGZhbHNlLFxuXG5cdHByb3RvdHlwZToge1xuXG5cdFx0aW5pdDogZnVuY3Rpb24oKSB7XG5cdFx0XHR0aGlzLmxhYmVsQ29udGFpbmVyID0gJCh0aGlzLnNldHRpbmdzLmVycm9yTGFiZWxDb250YWluZXIpO1xuXHRcdFx0dGhpcy5lcnJvckNvbnRleHQgPSB0aGlzLmxhYmVsQ29udGFpbmVyLmxlbmd0aCAmJiB0aGlzLmxhYmVsQ29udGFpbmVyIHx8ICQodGhpcy5jdXJyZW50Rm9ybSk7XG5cdFx0XHR0aGlzLmNvbnRhaW5lcnMgPSAkKHRoaXMuc2V0dGluZ3MuZXJyb3JDb250YWluZXIpLmFkZCggdGhpcy5zZXR0aW5ncy5lcnJvckxhYmVsQ29udGFpbmVyICk7XG5cdFx0XHR0aGlzLnN1Ym1pdHRlZCA9IHt9O1xuXHRcdFx0dGhpcy52YWx1ZUNhY2hlID0ge307XG5cdFx0XHR0aGlzLnBlbmRpbmdSZXF1ZXN0ID0gMDtcblx0XHRcdHRoaXMucGVuZGluZyA9IHt9O1xuXHRcdFx0dGhpcy5pbnZhbGlkID0ge307XG5cdFx0XHR0aGlzLnJlc2V0KCk7XG5cblx0XHRcdHZhciBncm91cHMgPSAodGhpcy5ncm91cHMgPSB7fSk7XG5cdFx0XHQkLmVhY2godGhpcy5zZXR0aW5ncy5ncm91cHMsIGZ1bmN0aW9uKCBrZXksIHZhbHVlICkge1xuXHRcdFx0XHRpZiAoIHR5cGVvZiB2YWx1ZSA9PT0gXCJzdHJpbmdcIiApIHtcblx0XHRcdFx0XHR2YWx1ZSA9IHZhbHVlLnNwbGl0KC9cXHMvKTtcblx0XHRcdFx0fVxuXHRcdFx0XHQkLmVhY2godmFsdWUsIGZ1bmN0aW9uKCBpbmRleCwgbmFtZSApIHtcblx0XHRcdFx0XHRncm91cHNbbmFtZV0gPSBrZXk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0fSk7XG5cdFx0XHR2YXIgcnVsZXMgPSB0aGlzLnNldHRpbmdzLnJ1bGVzO1xuXHRcdFx0JC5lYWNoKHJ1bGVzLCBmdW5jdGlvbigga2V5LCB2YWx1ZSApIHtcblx0XHRcdFx0cnVsZXNba2V5XSA9ICQudmFsaWRhdG9yLm5vcm1hbGl6ZVJ1bGUodmFsdWUpO1xuXHRcdFx0fSk7XG5cblx0XHRcdGZ1bmN0aW9uIGRlbGVnYXRlKGV2ZW50KSB7XG5cdFx0XHRcdHZhciB2YWxpZGF0b3IgPSAkLmRhdGEodGhpc1swXS5mb3JtLCBcInZhbGlkYXRvclwiKSxcblx0XHRcdFx0XHRldmVudFR5cGUgPSBcIm9uXCIgKyBldmVudC50eXBlLnJlcGxhY2UoL152YWxpZGF0ZS8sIFwiXCIpO1xuXHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5nc1tldmVudFR5cGVdICkge1xuXHRcdFx0XHRcdHZhbGlkYXRvci5zZXR0aW5nc1tldmVudFR5cGVdLmNhbGwodmFsaWRhdG9yLCB0aGlzWzBdLCBldmVudCk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdCQodGhpcy5jdXJyZW50Rm9ybSlcblx0XHRcdFx0LnZhbGlkYXRlRGVsZWdhdGUoXCI6dGV4dCwgW3R5cGU9J3Bhc3N3b3JkJ10sIFt0eXBlPSdmaWxlJ10sIHNlbGVjdCwgdGV4dGFyZWEsIFwiICtcblx0XHRcdFx0XHRcIlt0eXBlPSdudW1iZXInXSwgW3R5cGU9J3NlYXJjaCddICxbdHlwZT0ndGVsJ10sIFt0eXBlPSd1cmwnXSwgXCIgK1xuXHRcdFx0XHRcdFwiW3R5cGU9J2VtYWlsJ10sIFt0eXBlPSdkYXRldGltZSddLCBbdHlwZT0nZGF0ZSddLCBbdHlwZT0nbW9udGgnXSwgXCIgK1xuXHRcdFx0XHRcdFwiW3R5cGU9J3dlZWsnXSwgW3R5cGU9J3RpbWUnXSwgW3R5cGU9J2RhdGV0aW1lLWxvY2FsJ10sIFwiICtcblx0XHRcdFx0XHRcIlt0eXBlPSdyYW5nZSddLCBbdHlwZT0nY29sb3InXSBcIixcblx0XHRcdFx0XHRcImZvY3VzaW4gZm9jdXNvdXQga2V5dXBcIiwgZGVsZWdhdGUpXG5cdFx0XHRcdC52YWxpZGF0ZURlbGVnYXRlKFwiW3R5cGU9J3JhZGlvJ10sIFt0eXBlPSdjaGVja2JveCddLCBzZWxlY3QsIG9wdGlvblwiLCBcImNsaWNrXCIsIGRlbGVnYXRlKTtcblxuXHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLmludmFsaWRIYW5kbGVyICkge1xuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLmJpbmQoXCJpbnZhbGlkLWZvcm0udmFsaWRhdGVcIiwgdGhpcy5zZXR0aW5ncy5pbnZhbGlkSGFuZGxlcik7XG5cdFx0XHR9XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9mb3JtXG5cdFx0Zm9ybTogZnVuY3Rpb24oKSB7XG5cdFx0XHR0aGlzLmNoZWNrRm9ybSgpO1xuXHRcdFx0JC5leHRlbmQodGhpcy5zdWJtaXR0ZWQsIHRoaXMuZXJyb3JNYXApO1xuXHRcdFx0dGhpcy5pbnZhbGlkID0gJC5leHRlbmQoe30sIHRoaXMuZXJyb3JNYXApO1xuXHRcdFx0aWYgKCAhdGhpcy52YWxpZCgpICkge1xuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLnRyaWdnZXJIYW5kbGVyKFwiaW52YWxpZC1mb3JtXCIsIFt0aGlzXSk7XG5cdFx0XHR9XG5cdFx0XHR0aGlzLnNob3dFcnJvcnMoKTtcblx0XHRcdHJldHVybiB0aGlzLnZhbGlkKCk7XG5cdFx0fSxcblxuXHRcdGNoZWNrRm9ybTogZnVuY3Rpb24oKSB7XG5cdFx0XHR0aGlzLnByZXBhcmVGb3JtKCk7XG5cdFx0XHRmb3IgKCB2YXIgaSA9IDAsIGVsZW1lbnRzID0gKHRoaXMuY3VycmVudEVsZW1lbnRzID0gdGhpcy5lbGVtZW50cygpKTsgZWxlbWVudHNbaV07IGkrKyApIHtcblx0XHRcdFx0dGhpcy5jaGVjayggZWxlbWVudHNbaV0gKTtcblx0XHRcdH1cblx0XHRcdHJldHVybiB0aGlzLnZhbGlkKCk7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9lbGVtZW50XG5cdFx0ZWxlbWVudDogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHRlbGVtZW50ID0gdGhpcy52YWxpZGF0aW9uVGFyZ2V0Rm9yKCB0aGlzLmNsZWFuKCBlbGVtZW50ICkgKTtcblx0XHRcdHRoaXMubGFzdEVsZW1lbnQgPSBlbGVtZW50O1xuXHRcdFx0dGhpcy5wcmVwYXJlRWxlbWVudCggZWxlbWVudCApO1xuXHRcdFx0dGhpcy5jdXJyZW50RWxlbWVudHMgPSAkKGVsZW1lbnQpO1xuXHRcdFx0dmFyIHJlc3VsdCA9IHRoaXMuY2hlY2soIGVsZW1lbnQgKSAhPT0gZmFsc2U7XG5cdFx0XHRpZiAoIHJlc3VsdCApIHtcblx0XHRcdFx0ZGVsZXRlIHRoaXMuaW52YWxpZFtlbGVtZW50Lm5hbWVdO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0dGhpcy5pbnZhbGlkW2VsZW1lbnQubmFtZV0gPSB0cnVlO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCAhdGhpcy5udW1iZXJPZkludmFsaWRzKCkgKSB7XG5cdFx0XHRcdC8vIEhpZGUgZXJyb3IgY29udGFpbmVycyBvbiBsYXN0IGVycm9yXG5cdFx0XHRcdHRoaXMudG9IaWRlID0gdGhpcy50b0hpZGUuYWRkKCB0aGlzLmNvbnRhaW5lcnMgKTtcblx0XHRcdH1cblx0XHRcdHRoaXMuc2hvd0Vycm9ycygpO1xuXHRcdFx0cmV0dXJuIHJlc3VsdDtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vVmFsaWRhdG9yL3Nob3dFcnJvcnNcblx0XHRzaG93RXJyb3JzOiBmdW5jdGlvbiggZXJyb3JzICkge1xuXHRcdFx0aWYgKCBlcnJvcnMgKSB7XG5cdFx0XHRcdC8vIGFkZCBpdGVtcyB0byBlcnJvciBsaXN0IGFuZCBtYXBcblx0XHRcdFx0JC5leHRlbmQoIHRoaXMuZXJyb3JNYXAsIGVycm9ycyApO1xuXHRcdFx0XHR0aGlzLmVycm9yTGlzdCA9IFtdO1xuXHRcdFx0XHRmb3IgKCB2YXIgbmFtZSBpbiBlcnJvcnMgKSB7XG5cdFx0XHRcdFx0dGhpcy5lcnJvckxpc3QucHVzaCh7XG5cdFx0XHRcdFx0XHRtZXNzYWdlOiBlcnJvcnNbbmFtZV0sXG5cdFx0XHRcdFx0XHRlbGVtZW50OiB0aGlzLmZpbmRCeU5hbWUobmFtZSlbMF1cblx0XHRcdFx0XHR9KTtcblx0XHRcdFx0fVxuXHRcdFx0XHQvLyByZW1vdmUgaXRlbXMgZnJvbSBzdWNjZXNzIGxpc3Rcblx0XHRcdFx0dGhpcy5zdWNjZXNzTGlzdCA9ICQuZ3JlcCggdGhpcy5zdWNjZXNzTGlzdCwgZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHRcdFx0cmV0dXJuICEoZWxlbWVudC5uYW1lIGluIGVycm9ycyk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLnNob3dFcnJvcnMgKSB7XG5cdFx0XHRcdHRoaXMuc2V0dGluZ3Muc2hvd0Vycm9ycy5jYWxsKCB0aGlzLCB0aGlzLmVycm9yTWFwLCB0aGlzLmVycm9yTGlzdCApO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0dGhpcy5kZWZhdWx0U2hvd0Vycm9ycygpO1xuXHRcdFx0fVxuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3IvcmVzZXRGb3JtXG5cdFx0cmVzZXRGb3JtOiBmdW5jdGlvbigpIHtcblx0XHRcdGlmICggJC5mbi5yZXNldEZvcm0gKSB7XG5cdFx0XHRcdCQodGhpcy5jdXJyZW50Rm9ybSkucmVzZXRGb3JtKCk7XG5cdFx0XHR9XG5cdFx0XHR0aGlzLnN1Ym1pdHRlZCA9IHt9O1xuXHRcdFx0dGhpcy5sYXN0RWxlbWVudCA9IG51bGw7XG5cdFx0XHR0aGlzLnByZXBhcmVGb3JtKCk7XG5cdFx0XHR0aGlzLmhpZGVFcnJvcnMoKTtcblx0XHRcdHRoaXMuZWxlbWVudHMoKS5yZW1vdmVDbGFzcyggdGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzICkucmVtb3ZlRGF0YSggXCJwcmV2aW91c1ZhbHVlXCIgKTtcblx0XHR9LFxuXG5cdFx0bnVtYmVyT2ZJbnZhbGlkczogZnVuY3Rpb24oKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5vYmplY3RMZW5ndGgodGhpcy5pbnZhbGlkKTtcblx0XHR9LFxuXG5cdFx0b2JqZWN0TGVuZ3RoOiBmdW5jdGlvbiggb2JqICkge1xuXHRcdFx0dmFyIGNvdW50ID0gMDtcblx0XHRcdGZvciAoIHZhciBpIGluIG9iaiApIHtcblx0XHRcdFx0Y291bnQrKztcblx0XHRcdH1cblx0XHRcdHJldHVybiBjb3VudDtcblx0XHR9LFxuXG5cdFx0aGlkZUVycm9yczogZnVuY3Rpb24oKSB7XG5cdFx0XHR0aGlzLmFkZFdyYXBwZXIoIHRoaXMudG9IaWRlICkuaGlkZSgpO1xuXHRcdH0sXG5cblx0XHR2YWxpZDogZnVuY3Rpb24oKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5zaXplKCkgPT09IDA7XG5cdFx0fSxcblxuXHRcdHNpemU6IGZ1bmN0aW9uKCkge1xuXHRcdFx0cmV0dXJuIHRoaXMuZXJyb3JMaXN0Lmxlbmd0aDtcblx0XHR9LFxuXG5cdFx0Zm9jdXNJbnZhbGlkOiBmdW5jdGlvbigpIHtcblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5mb2N1c0ludmFsaWQgKSB7XG5cdFx0XHRcdHRyeSB7XG5cdFx0XHRcdFx0JCh0aGlzLmZpbmRMYXN0QWN0aXZlKCkgfHwgdGhpcy5lcnJvckxpc3QubGVuZ3RoICYmIHRoaXMuZXJyb3JMaXN0WzBdLmVsZW1lbnQgfHwgW10pXG5cdFx0XHRcdFx0LmZpbHRlcihcIjp2aXNpYmxlXCIpXG5cdFx0XHRcdFx0LmZvY3VzKClcblx0XHRcdFx0XHQvLyBtYW51YWxseSB0cmlnZ2VyIGZvY3VzaW4gZXZlbnQ7IHdpdGhvdXQgaXQsIGZvY3VzaW4gaGFuZGxlciBpc24ndCBjYWxsZWQsIGZpbmRMYXN0QWN0aXZlIHdvbid0IGhhdmUgYW55dGhpbmcgdG8gZmluZFxuXHRcdFx0XHRcdC50cmlnZ2VyKFwiZm9jdXNpblwiKTtcblx0XHRcdFx0fSBjYXRjaChlKSB7XG5cdFx0XHRcdFx0Ly8gaWdub3JlIElFIHRocm93aW5nIGVycm9ycyB3aGVuIGZvY3VzaW5nIGhpZGRlbiBlbGVtZW50c1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fSxcblxuXHRcdGZpbmRMYXN0QWN0aXZlOiBmdW5jdGlvbigpIHtcblx0XHRcdHZhciBsYXN0QWN0aXZlID0gdGhpcy5sYXN0QWN0aXZlO1xuXHRcdFx0cmV0dXJuIGxhc3RBY3RpdmUgJiYgJC5ncmVwKHRoaXMuZXJyb3JMaXN0LCBmdW5jdGlvbiggbiApIHtcblx0XHRcdFx0cmV0dXJuIG4uZWxlbWVudC5uYW1lID09PSBsYXN0QWN0aXZlLm5hbWU7XG5cdFx0XHR9KS5sZW5ndGggPT09IDEgJiYgbGFzdEFjdGl2ZTtcblx0XHR9LFxuXG5cdFx0ZWxlbWVudHM6IGZ1bmN0aW9uKCkge1xuXHRcdFx0dmFyIHZhbGlkYXRvciA9IHRoaXMsXG5cdFx0XHRcdHJ1bGVzQ2FjaGUgPSB7fTtcblxuXHRcdFx0Ly8gc2VsZWN0IGFsbCB2YWxpZCBpbnB1dHMgaW5zaWRlIHRoZSBmb3JtIChubyBzdWJtaXQgb3IgcmVzZXQgYnV0dG9ucylcblx0XHRcdHJldHVybiAkKHRoaXMuY3VycmVudEZvcm0pXG5cdFx0XHQuZmluZChcImlucHV0LCBzZWxlY3QsIHRleHRhcmVhXCIpXG5cdFx0XHQubm90KFwiOnN1Ym1pdCwgOnJlc2V0LCA6aW1hZ2UsIFtkaXNhYmxlZF1cIilcblx0XHRcdC5ub3QoIHRoaXMuc2V0dGluZ3MuaWdub3JlIClcblx0XHRcdC5maWx0ZXIoZnVuY3Rpb24oKSB7XG5cdFx0XHRcdGlmICggIXRoaXMubmFtZSAmJiB2YWxpZGF0b3Iuc2V0dGluZ3MuZGVidWcgJiYgd2luZG93LmNvbnNvbGUgKSB7XG5cdFx0XHRcdFx0Y29uc29sZS5lcnJvciggXCIlbyBoYXMgbm8gbmFtZSBhc3NpZ25lZFwiLCB0aGlzKTtcblx0XHRcdFx0fVxuXG5cdFx0XHRcdC8vIHNlbGVjdCBvbmx5IHRoZSBmaXJzdCBlbGVtZW50IGZvciBlYWNoIG5hbWUsIGFuZCBvbmx5IHRob3NlIHdpdGggcnVsZXMgc3BlY2lmaWVkXG5cdFx0XHRcdGlmICggdGhpcy5uYW1lIGluIHJ1bGVzQ2FjaGUgfHwgIXZhbGlkYXRvci5vYmplY3RMZW5ndGgoJCh0aGlzKS5ydWxlcygpKSApIHtcblx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XG5cdFx0XHRcdH1cblxuXHRcdFx0XHRydWxlc0NhY2hlW3RoaXMubmFtZV0gPSB0cnVlO1xuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcblx0XHRcdH0pO1xuXHRcdH0sXG5cblx0XHRjbGVhbjogZnVuY3Rpb24oIHNlbGVjdG9yICkge1xuXHRcdFx0cmV0dXJuICQoc2VsZWN0b3IpWzBdO1xuXHRcdH0sXG5cblx0XHRlcnJvcnM6IGZ1bmN0aW9uKCkge1xuXHRcdFx0dmFyIGVycm9yQ2xhc3MgPSB0aGlzLnNldHRpbmdzLmVycm9yQ2xhc3MucmVwbGFjZShcIiBcIiwgXCIuXCIpO1xuXHRcdFx0cmV0dXJuICQodGhpcy5zZXR0aW5ncy5lcnJvckVsZW1lbnQgKyBcIi5cIiArIGVycm9yQ2xhc3MsIHRoaXMuZXJyb3JDb250ZXh0KTtcblx0XHR9LFxuXG5cdFx0cmVzZXQ6IGZ1bmN0aW9uKCkge1xuXHRcdFx0dGhpcy5zdWNjZXNzTGlzdCA9IFtdO1xuXHRcdFx0dGhpcy5lcnJvckxpc3QgPSBbXTtcblx0XHRcdHRoaXMuZXJyb3JNYXAgPSB7fTtcblx0XHRcdHRoaXMudG9TaG93ID0gJChbXSk7XG5cdFx0XHR0aGlzLnRvSGlkZSA9ICQoW10pO1xuXHRcdFx0dGhpcy5jdXJyZW50RWxlbWVudHMgPSAkKFtdKTtcblx0XHR9LFxuXG5cdFx0cHJlcGFyZUZvcm06IGZ1bmN0aW9uKCkge1xuXHRcdFx0dGhpcy5yZXNldCgpO1xuXHRcdFx0dGhpcy50b0hpZGUgPSB0aGlzLmVycm9ycygpLmFkZCggdGhpcy5jb250YWluZXJzICk7XG5cdFx0fSxcblxuXHRcdHByZXBhcmVFbGVtZW50OiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHRcdHRoaXMucmVzZXQoKTtcblx0XHRcdHRoaXMudG9IaWRlID0gdGhpcy5lcnJvcnNGb3IoZWxlbWVudCk7XG5cdFx0fSxcblxuXHRcdGVsZW1lbnRWYWx1ZTogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHR2YXIgdHlwZSA9ICQoZWxlbWVudCkuYXR0cihcInR5cGVcIiksXG5cdFx0XHRcdHZhbCA9ICQoZWxlbWVudCkudmFsKCk7XG5cblx0XHRcdGlmICggdHlwZSA9PT0gXCJyYWRpb1wiIHx8IHR5cGUgPT09IFwiY2hlY2tib3hcIiApIHtcblx0XHRcdFx0cmV0dXJuICQoXCJpbnB1dFtuYW1lPSdcIiArICQoZWxlbWVudCkuYXR0cihcIm5hbWVcIikgKyBcIiddOmNoZWNrZWRcIikudmFsKCk7XG5cdFx0XHR9XG5cblx0XHRcdGlmICggdHlwZW9mIHZhbCA9PT0gXCJzdHJpbmdcIiApIHtcblx0XHRcdFx0cmV0dXJuIHZhbC5yZXBsYWNlKC9cXHIvZywgXCJcIik7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gdmFsO1xuXHRcdH0sXG5cblx0XHRjaGVjazogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHRlbGVtZW50ID0gdGhpcy52YWxpZGF0aW9uVGFyZ2V0Rm9yKCB0aGlzLmNsZWFuKCBlbGVtZW50ICkgKTtcblxuXHRcdFx0dmFyIHJ1bGVzID0gJChlbGVtZW50KS5ydWxlcygpO1xuXHRcdFx0dmFyIGRlcGVuZGVuY3lNaXNtYXRjaCA9IGZhbHNlO1xuXHRcdFx0dmFyIHZhbCA9IHRoaXMuZWxlbWVudFZhbHVlKGVsZW1lbnQpO1xuXHRcdFx0dmFyIHJlc3VsdDtcblxuXHRcdFx0Zm9yICh2YXIgbWV0aG9kIGluIHJ1bGVzICkge1xuXHRcdFx0XHR2YXIgcnVsZSA9IHsgbWV0aG9kOiBtZXRob2QsIHBhcmFtZXRlcnM6IHJ1bGVzW21ldGhvZF0gfTtcblx0XHRcdFx0dHJ5IHtcblxuXHRcdFx0XHRcdHJlc3VsdCA9ICQudmFsaWRhdG9yLm1ldGhvZHNbbWV0aG9kXS5jYWxsKCB0aGlzLCB2YWwsIGVsZW1lbnQsIHJ1bGUucGFyYW1ldGVycyApO1xuXG5cdFx0XHRcdFx0Ly8gaWYgYSBtZXRob2QgaW5kaWNhdGVzIHRoYXQgdGhlIGZpZWxkIGlzIG9wdGlvbmFsIGFuZCB0aGVyZWZvcmUgdmFsaWQsXG5cdFx0XHRcdFx0Ly8gZG9uJ3QgbWFyayBpdCBhcyB2YWxpZCB3aGVuIHRoZXJlIGFyZSBubyBvdGhlciBydWxlc1xuXHRcdFx0XHRcdGlmICggcmVzdWx0ID09PSBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIiApIHtcblx0XHRcdFx0XHRcdGRlcGVuZGVuY3lNaXNtYXRjaCA9IHRydWU7XG5cdFx0XHRcdFx0XHRjb250aW51ZTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0ZGVwZW5kZW5jeU1pc21hdGNoID0gZmFsc2U7XG5cblx0XHRcdFx0XHRpZiAoIHJlc3VsdCA9PT0gXCJwZW5kaW5nXCIgKSB7XG5cdFx0XHRcdFx0XHR0aGlzLnRvSGlkZSA9IHRoaXMudG9IaWRlLm5vdCggdGhpcy5lcnJvcnNGb3IoZWxlbWVudCkgKTtcblx0XHRcdFx0XHRcdHJldHVybjtcblx0XHRcdFx0XHR9XG5cblx0XHRcdFx0XHRpZiAoICFyZXN1bHQgKSB7XG5cdFx0XHRcdFx0XHR0aGlzLmZvcm1hdEFuZEFkZCggZWxlbWVudCwgcnVsZSApO1xuXHRcdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0fSBjYXRjaChlKSB7XG5cdFx0XHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLmRlYnVnICYmIHdpbmRvdy5jb25zb2xlICkge1xuXHRcdFx0XHRcdFx0Y29uc29sZS5sb2coIFwiRXhjZXB0aW9uIG9jY3VycmVkIHdoZW4gY2hlY2tpbmcgZWxlbWVudCBcIiArIGVsZW1lbnQuaWQgKyBcIiwgY2hlY2sgdGhlICdcIiArIHJ1bGUubWV0aG9kICsgXCInIG1ldGhvZC5cIiwgZSApO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHR0aHJvdyBlO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0XHRpZiAoIGRlcGVuZGVuY3lNaXNtYXRjaCApIHtcblx0XHRcdFx0cmV0dXJuO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCB0aGlzLm9iamVjdExlbmd0aChydWxlcykgKSB7XG5cdFx0XHRcdHRoaXMuc3VjY2Vzc0xpc3QucHVzaChlbGVtZW50KTtcblx0XHRcdH1cblx0XHRcdHJldHVybiB0cnVlO1xuXHRcdH0sXG5cblx0XHQvLyByZXR1cm4gdGhlIGN1c3RvbSBtZXNzYWdlIGZvciB0aGUgZ2l2ZW4gZWxlbWVudCBhbmQgdmFsaWRhdGlvbiBtZXRob2Rcblx0XHQvLyBzcGVjaWZpZWQgaW4gdGhlIGVsZW1lbnQncyBIVE1MNSBkYXRhIGF0dHJpYnV0ZVxuXHRcdGN1c3RvbURhdGFNZXNzYWdlOiBmdW5jdGlvbiggZWxlbWVudCwgbWV0aG9kICkge1xuXHRcdFx0cmV0dXJuICQoZWxlbWVudCkuZGF0YShcIm1zZy1cIiArIG1ldGhvZC50b0xvd2VyQ2FzZSgpKSB8fCAoZWxlbWVudC5hdHRyaWJ1dGVzICYmICQoZWxlbWVudCkuYXR0cihcImRhdGEtbXNnLVwiICsgbWV0aG9kLnRvTG93ZXJDYXNlKCkpKTtcblx0XHR9LFxuXG5cdFx0Ly8gcmV0dXJuIHRoZSBjdXN0b20gbWVzc2FnZSBmb3IgdGhlIGdpdmVuIGVsZW1lbnQgbmFtZSBhbmQgdmFsaWRhdGlvbiBtZXRob2Rcblx0XHRjdXN0b21NZXNzYWdlOiBmdW5jdGlvbiggbmFtZSwgbWV0aG9kICkge1xuXHRcdFx0dmFyIG0gPSB0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW25hbWVdO1xuXHRcdFx0cmV0dXJuIG0gJiYgKG0uY29uc3RydWN0b3IgPT09IFN0cmluZyA/IG0gOiBtW21ldGhvZF0pO1xuXHRcdH0sXG5cblx0XHQvLyByZXR1cm4gdGhlIGZpcnN0IGRlZmluZWQgYXJndW1lbnQsIGFsbG93aW5nIGVtcHR5IHN0cmluZ3Ncblx0XHRmaW5kRGVmaW5lZDogZnVuY3Rpb24oKSB7XG5cdFx0XHRmb3IodmFyIGkgPSAwOyBpIDwgYXJndW1lbnRzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRcdGlmICggYXJndW1lbnRzW2ldICE9PSB1bmRlZmluZWQgKSB7XG5cdFx0XHRcdFx0cmV0dXJuIGFyZ3VtZW50c1tpXTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdFx0cmV0dXJuIHVuZGVmaW5lZDtcblx0XHR9LFxuXG5cdFx0ZGVmYXVsdE1lc3NhZ2U6IGZ1bmN0aW9uKCBlbGVtZW50LCBtZXRob2QgKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5maW5kRGVmaW5lZChcblx0XHRcdFx0dGhpcy5jdXN0b21NZXNzYWdlKCBlbGVtZW50Lm5hbWUsIG1ldGhvZCApLFxuXHRcdFx0XHR0aGlzLmN1c3RvbURhdGFNZXNzYWdlKCBlbGVtZW50LCBtZXRob2QgKSxcblx0XHRcdFx0Ly8gdGl0bGUgaXMgbmV2ZXIgdW5kZWZpbmVkLCBzbyBoYW5kbGUgZW1wdHkgc3RyaW5nIGFzIHVuZGVmaW5lZFxuXHRcdFx0XHQhdGhpcy5zZXR0aW5ncy5pZ25vcmVUaXRsZSAmJiBlbGVtZW50LnRpdGxlIHx8IHVuZGVmaW5lZCxcblx0XHRcdFx0JC52YWxpZGF0b3IubWVzc2FnZXNbbWV0aG9kXSxcblx0XHRcdFx0XCI8c3Ryb25nPldhcm5pbmc6IE5vIG1lc3NhZ2UgZGVmaW5lZCBmb3IgXCIgKyBlbGVtZW50Lm5hbWUgKyBcIjwvc3Ryb25nPlwiXG5cdFx0XHQpO1xuXHRcdH0sXG5cblx0XHRmb3JtYXRBbmRBZGQ6IGZ1bmN0aW9uKCBlbGVtZW50LCBydWxlICkge1xuXHRcdFx0dmFyIG1lc3NhZ2UgPSB0aGlzLmRlZmF1bHRNZXNzYWdlKCBlbGVtZW50LCBydWxlLm1ldGhvZCApLFxuXHRcdFx0XHR0aGVyZWdleCA9IC9cXCQ/XFx7KFxcZCspXFx9L2c7XG5cdFx0XHRpZiAoIHR5cGVvZiBtZXNzYWdlID09PSBcImZ1bmN0aW9uXCIgKSB7XG5cdFx0XHRcdG1lc3NhZ2UgPSBtZXNzYWdlLmNhbGwodGhpcywgcnVsZS5wYXJhbWV0ZXJzLCBlbGVtZW50KTtcblx0XHRcdH0gZWxzZSBpZiAodGhlcmVnZXgudGVzdChtZXNzYWdlKSkge1xuXHRcdFx0XHRtZXNzYWdlID0gJC52YWxpZGF0b3IuZm9ybWF0KG1lc3NhZ2UucmVwbGFjZSh0aGVyZWdleCwgXCJ7JDF9XCIpLCBydWxlLnBhcmFtZXRlcnMpO1xuXHRcdFx0fVxuXHRcdFx0dGhpcy5lcnJvckxpc3QucHVzaCh7XG5cdFx0XHRcdG1lc3NhZ2U6IG1lc3NhZ2UsXG5cdFx0XHRcdGVsZW1lbnQ6IGVsZW1lbnRcblx0XHRcdH0pO1xuXG5cdFx0XHR0aGlzLmVycm9yTWFwW2VsZW1lbnQubmFtZV0gPSBtZXNzYWdlO1xuXHRcdFx0dGhpcy5zdWJtaXR0ZWRbZWxlbWVudC5uYW1lXSA9IG1lc3NhZ2U7XG5cdFx0fSxcblxuXHRcdGFkZFdyYXBwZXI6IGZ1bmN0aW9uKCB0b1RvZ2dsZSApIHtcblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy53cmFwcGVyICkge1xuXHRcdFx0XHR0b1RvZ2dsZSA9IHRvVG9nZ2xlLmFkZCggdG9Ub2dnbGUucGFyZW50KCB0aGlzLnNldHRpbmdzLndyYXBwZXIgKSApO1xuXHRcdFx0fVxuXHRcdFx0cmV0dXJuIHRvVG9nZ2xlO1xuXHRcdH0sXG5cblx0XHRkZWZhdWx0U2hvd0Vycm9yczogZnVuY3Rpb24oKSB7XG5cdFx0XHR2YXIgaSwgZWxlbWVudHM7XG5cdFx0XHRmb3IgKCBpID0gMDsgdGhpcy5lcnJvckxpc3RbaV07IGkrKyApIHtcblx0XHRcdFx0dmFyIGVycm9yID0gdGhpcy5lcnJvckxpc3RbaV07XG5cdFx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5oaWdobGlnaHQgKSB7XG5cdFx0XHRcdFx0dGhpcy5zZXR0aW5ncy5oaWdobGlnaHQuY2FsbCggdGhpcywgZXJyb3IuZWxlbWVudCwgdGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzLCB0aGlzLnNldHRpbmdzLnZhbGlkQ2xhc3MgKTtcblx0XHRcdFx0fVxuXHRcdFx0XHR0aGlzLnNob3dMYWJlbCggZXJyb3IuZWxlbWVudCwgZXJyb3IubWVzc2FnZSApO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCB0aGlzLmVycm9yTGlzdC5sZW5ndGggKSB7XG5cdFx0XHRcdHRoaXMudG9TaG93ID0gdGhpcy50b1Nob3cuYWRkKCB0aGlzLmNvbnRhaW5lcnMgKTtcblx0XHRcdH1cblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5zdWNjZXNzICkge1xuXHRcdFx0XHRmb3IgKCBpID0gMDsgdGhpcy5zdWNjZXNzTGlzdFtpXTsgaSsrICkge1xuXHRcdFx0XHRcdHRoaXMuc2hvd0xhYmVsKCB0aGlzLnN1Y2Nlc3NMaXN0W2ldICk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy51bmhpZ2hsaWdodCApIHtcblx0XHRcdFx0Zm9yICggaSA9IDAsIGVsZW1lbnRzID0gdGhpcy52YWxpZEVsZW1lbnRzKCk7IGVsZW1lbnRzW2ldOyBpKysgKSB7XG5cdFx0XHRcdFx0dGhpcy5zZXR0aW5ncy51bmhpZ2hsaWdodC5jYWxsKCB0aGlzLCBlbGVtZW50c1tpXSwgdGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzLCB0aGlzLnNldHRpbmdzLnZhbGlkQ2xhc3MgKTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdFx0dGhpcy50b0hpZGUgPSB0aGlzLnRvSGlkZS5ub3QoIHRoaXMudG9TaG93ICk7XG5cdFx0XHR0aGlzLmhpZGVFcnJvcnMoKTtcblx0XHRcdHRoaXMuYWRkV3JhcHBlciggdGhpcy50b1Nob3cgKS5zaG93KCk7XG5cdFx0fSxcblxuXHRcdHZhbGlkRWxlbWVudHM6IGZ1bmN0aW9uKCkge1xuXHRcdFx0cmV0dXJuIHRoaXMuY3VycmVudEVsZW1lbnRzLm5vdCh0aGlzLmludmFsaWRFbGVtZW50cygpKTtcblx0XHR9LFxuXG5cdFx0aW52YWxpZEVsZW1lbnRzOiBmdW5jdGlvbigpIHtcblx0XHRcdHJldHVybiAkKHRoaXMuZXJyb3JMaXN0KS5tYXAoZnVuY3Rpb24oKSB7XG5cdFx0XHRcdHJldHVybiB0aGlzLmVsZW1lbnQ7XG5cdFx0XHR9KTtcblx0XHR9LFxuXG5cdFx0c2hvd0xhYmVsOiBmdW5jdGlvbiggZWxlbWVudCwgbWVzc2FnZSApIHtcblx0XHRcdHZhciBsYWJlbCA9IHRoaXMuZXJyb3JzRm9yKCBlbGVtZW50ICk7XG5cdFx0XHRpZiAoIGxhYmVsLmxlbmd0aCApIHtcblx0XHRcdFx0Ly8gcmVmcmVzaCBlcnJvci9zdWNjZXNzIGNsYXNzXG5cdFx0XHRcdGxhYmVsLnJlbW92ZUNsYXNzKCB0aGlzLnNldHRpbmdzLnZhbGlkQ2xhc3MgKS5hZGRDbGFzcyggdGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzICk7XG5cdFx0XHRcdC8vIHJlcGxhY2UgbWVzc2FnZSBvbiBleGlzdGluZyBsYWJlbFxuXHRcdFx0XHRsYWJlbC5odG1sKG1lc3NhZ2UpO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0Ly8gY3JlYXRlIGxhYmVsXG5cdFx0XHRcdGxhYmVsID0gJChcIjxcIiArIHRoaXMuc2V0dGluZ3MuZXJyb3JFbGVtZW50ICsgXCI+XCIpXG5cdFx0XHRcdFx0LmF0dHIoXCJmb3JcIiwgdGhpcy5pZE9yTmFtZShlbGVtZW50KSlcblx0XHRcdFx0XHQuYWRkQ2xhc3ModGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzKVxuXHRcdFx0XHRcdC5odG1sKG1lc3NhZ2UgfHwgXCJcIik7XG5cdFx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy53cmFwcGVyICkge1xuXHRcdFx0XHRcdC8vIG1ha2Ugc3VyZSB0aGUgZWxlbWVudCBpcyB2aXNpYmxlLCBldmVuIGluIElFXG5cdFx0XHRcdFx0Ly8gYWN0dWFsbHkgc2hvd2luZyB0aGUgd3JhcHBlZCBlbGVtZW50IGlzIGhhbmRsZWQgZWxzZXdoZXJlXG5cdFx0XHRcdFx0bGFiZWwgPSBsYWJlbC5oaWRlKCkuc2hvdygpLndyYXAoXCI8XCIgKyB0aGlzLnNldHRpbmdzLndyYXBwZXIgKyBcIi8+XCIpLnBhcmVudCgpO1xuXHRcdFx0XHR9XG5cdFx0XHRcdGlmICggIXRoaXMubGFiZWxDb250YWluZXIuYXBwZW5kKGxhYmVsKS5sZW5ndGggKSB7XG5cdFx0XHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLmVycm9yUGxhY2VtZW50ICkge1xuXHRcdFx0XHRcdFx0dGhpcy5zZXR0aW5ncy5lcnJvclBsYWNlbWVudChsYWJlbCwgJChlbGVtZW50KSApO1xuXHRcdFx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdFx0XHRsYWJlbC5pbnNlcnRBZnRlcihlbGVtZW50KTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdGlmICggIW1lc3NhZ2UgJiYgdGhpcy5zZXR0aW5ncy5zdWNjZXNzICkge1xuXHRcdFx0XHRsYWJlbC50ZXh0KFwiXCIpO1xuXHRcdFx0XHRpZiAoIHR5cGVvZiB0aGlzLnNldHRpbmdzLnN1Y2Nlc3MgPT09IFwic3RyaW5nXCIgKSB7XG5cdFx0XHRcdFx0bGFiZWwuYWRkQ2xhc3MoIHRoaXMuc2V0dGluZ3Muc3VjY2VzcyApO1xuXHRcdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRcdHRoaXMuc2V0dGluZ3Muc3VjY2VzcyggbGFiZWwsIGVsZW1lbnQgKTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdFx0dGhpcy50b1Nob3cgPSB0aGlzLnRvU2hvdy5hZGQobGFiZWwpO1xuXHRcdH0sXG5cblx0XHRlcnJvcnNGb3I6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0dmFyIG5hbWUgPSB0aGlzLmlkT3JOYW1lKGVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuIHRoaXMuZXJyb3JzKCkuZmlsdGVyKGZ1bmN0aW9uKCkge1xuXHRcdFx0XHRyZXR1cm4gJCh0aGlzKS5hdHRyKFwiZm9yXCIpID09PSBuYW1lO1xuXHRcdFx0fSk7XG5cdFx0fSxcblxuXHRcdGlkT3JOYW1lOiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHRcdHJldHVybiB0aGlzLmdyb3Vwc1tlbGVtZW50Lm5hbWVdIHx8ICh0aGlzLmNoZWNrYWJsZShlbGVtZW50KSA/IGVsZW1lbnQubmFtZSA6IGVsZW1lbnQuaWQgfHwgZWxlbWVudC5uYW1lKTtcblx0XHR9LFxuXG5cdFx0dmFsaWRhdGlvblRhcmdldEZvcjogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHQvLyBpZiByYWRpby9jaGVja2JveCwgdmFsaWRhdGUgZmlyc3QgZWxlbWVudCBpbiBncm91cCBpbnN0ZWFkXG5cdFx0XHRpZiAoIHRoaXMuY2hlY2thYmxlKGVsZW1lbnQpICkge1xuXHRcdFx0XHRlbGVtZW50ID0gdGhpcy5maW5kQnlOYW1lKCBlbGVtZW50Lm5hbWUgKS5ub3QodGhpcy5zZXR0aW5ncy5pZ25vcmUpWzBdO1xuXHRcdFx0fVxuXHRcdFx0cmV0dXJuIGVsZW1lbnQ7XG5cdFx0fSxcblxuXHRcdGNoZWNrYWJsZTogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHRyZXR1cm4gKC9yYWRpb3xjaGVja2JveC9pKS50ZXN0KGVsZW1lbnQudHlwZSk7XG5cdFx0fSxcblxuXHRcdGZpbmRCeU5hbWU6IGZ1bmN0aW9uKCBuYW1lICkge1xuXHRcdFx0cmV0dXJuICQodGhpcy5jdXJyZW50Rm9ybSkuZmluZChcIltuYW1lPSdcIiArIG5hbWUgKyBcIiddXCIpO1xuXHRcdH0sXG5cblx0XHRnZXRMZW5ndGg6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcblx0XHRcdHN3aXRjaCggZWxlbWVudC5ub2RlTmFtZS50b0xvd2VyQ2FzZSgpICkge1xuXHRcdFx0Y2FzZSBcInNlbGVjdFwiOlxuXHRcdFx0XHRyZXR1cm4gJChcIm9wdGlvbjpzZWxlY3RlZFwiLCBlbGVtZW50KS5sZW5ndGg7XG5cdFx0XHRjYXNlIFwiaW5wdXRcIjpcblx0XHRcdFx0aWYgKCB0aGlzLmNoZWNrYWJsZSggZWxlbWVudCkgKSB7XG5cdFx0XHRcdFx0cmV0dXJuIHRoaXMuZmluZEJ5TmFtZShlbGVtZW50Lm5hbWUpLmZpbHRlcihcIjpjaGVja2VkXCIpLmxlbmd0aDtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdFx0cmV0dXJuIHZhbHVlLmxlbmd0aDtcblx0XHR9LFxuXG5cdFx0ZGVwZW5kOiBmdW5jdGlvbiggcGFyYW0sIGVsZW1lbnQgKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5kZXBlbmRUeXBlc1t0eXBlb2YgcGFyYW1dID8gdGhpcy5kZXBlbmRUeXBlc1t0eXBlb2YgcGFyYW1dKHBhcmFtLCBlbGVtZW50KSA6IHRydWU7XG5cdFx0fSxcblxuXHRcdGRlcGVuZFR5cGVzOiB7XG5cdFx0XHRcImJvb2xlYW5cIjogZnVuY3Rpb24oIHBhcmFtLCBlbGVtZW50ICkge1xuXHRcdFx0XHRyZXR1cm4gcGFyYW07XG5cdFx0XHR9LFxuXHRcdFx0XCJzdHJpbmdcIjogZnVuY3Rpb24oIHBhcmFtLCBlbGVtZW50ICkge1xuXHRcdFx0XHRyZXR1cm4gISEkKHBhcmFtLCBlbGVtZW50LmZvcm0pLmxlbmd0aDtcblx0XHRcdH0sXG5cdFx0XHRcImZ1bmN0aW9uXCI6IGZ1bmN0aW9uKCBwYXJhbSwgZWxlbWVudCApIHtcblx0XHRcdFx0cmV0dXJuIHBhcmFtKGVsZW1lbnQpO1xuXHRcdFx0fVxuXHRcdH0sXG5cblx0XHRvcHRpb25hbDogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHR2YXIgdmFsID0gdGhpcy5lbGVtZW50VmFsdWUoZWxlbWVudCk7XG5cdFx0XHRyZXR1cm4gISQudmFsaWRhdG9yLm1ldGhvZHMucmVxdWlyZWQuY2FsbCh0aGlzLCB2YWwsIGVsZW1lbnQpICYmIFwiZGVwZW5kZW5jeS1taXNtYXRjaFwiO1xuXHRcdH0sXG5cblx0XHRzdGFydFJlcXVlc3Q6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0aWYgKCAhdGhpcy5wZW5kaW5nW2VsZW1lbnQubmFtZV0gKSB7XG5cdFx0XHRcdHRoaXMucGVuZGluZ1JlcXVlc3QrKztcblx0XHRcdFx0dGhpcy5wZW5kaW5nW2VsZW1lbnQubmFtZV0gPSB0cnVlO1xuXHRcdFx0fVxuXHRcdH0sXG5cblx0XHRzdG9wUmVxdWVzdDogZnVuY3Rpb24oIGVsZW1lbnQsIHZhbGlkICkge1xuXHRcdFx0dGhpcy5wZW5kaW5nUmVxdWVzdC0tO1xuXHRcdFx0Ly8gc29tZXRpbWVzIHN5bmNocm9uaXphdGlvbiBmYWlscywgbWFrZSBzdXJlIHBlbmRpbmdSZXF1ZXN0IGlzIG5ldmVyIDwgMFxuXHRcdFx0aWYgKCB0aGlzLnBlbmRpbmdSZXF1ZXN0IDwgMCApIHtcblx0XHRcdFx0dGhpcy5wZW5kaW5nUmVxdWVzdCA9IDA7XG5cdFx0XHR9XG5cdFx0XHRkZWxldGUgdGhpcy5wZW5kaW5nW2VsZW1lbnQubmFtZV07XG5cdFx0XHRpZiAoIHZhbGlkICYmIHRoaXMucGVuZGluZ1JlcXVlc3QgPT09IDAgJiYgdGhpcy5mb3JtU3VibWl0dGVkICYmIHRoaXMuZm9ybSgpICkge1xuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLnN1Ym1pdCgpO1xuXHRcdFx0XHR0aGlzLmZvcm1TdWJtaXR0ZWQgPSBmYWxzZTtcblx0XHRcdH0gZWxzZSBpZiAoIXZhbGlkICYmIHRoaXMucGVuZGluZ1JlcXVlc3QgPT09IDAgJiYgdGhpcy5mb3JtU3VibWl0dGVkKSB7XG5cdFx0XHRcdCQodGhpcy5jdXJyZW50Rm9ybSkudHJpZ2dlckhhbmRsZXIoXCJpbnZhbGlkLWZvcm1cIiwgW3RoaXNdKTtcblx0XHRcdFx0dGhpcy5mb3JtU3VibWl0dGVkID0gZmFsc2U7XG5cdFx0XHR9XG5cdFx0fSxcblxuXHRcdHByZXZpb3VzVmFsdWU6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuICQuZGF0YShlbGVtZW50LCBcInByZXZpb3VzVmFsdWVcIikgfHwgJC5kYXRhKGVsZW1lbnQsIFwicHJldmlvdXNWYWx1ZVwiLCB7XG5cdFx0XHRcdG9sZDogbnVsbCxcblx0XHRcdFx0dmFsaWQ6IHRydWUsXG5cdFx0XHRcdG1lc3NhZ2U6IHRoaXMuZGVmYXVsdE1lc3NhZ2UoIGVsZW1lbnQsIFwicmVtb3RlXCIgKVxuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdH0sXG5cblx0Y2xhc3NSdWxlU2V0dGluZ3M6IHtcblx0XHRyZXF1aXJlZDoge3JlcXVpcmVkOiB0cnVlfSxcblx0XHRlbWFpbDoge2VtYWlsOiB0cnVlfSxcblx0XHR1cmw6IHt1cmw6IHRydWV9LFxuXHRcdGRhdGU6IHtkYXRlOiB0cnVlfSxcblx0XHRkYXRlSVNPOiB7ZGF0ZUlTTzogdHJ1ZX0sXG5cdFx0bnVtYmVyOiB7bnVtYmVyOiB0cnVlfSxcblx0XHRkaWdpdHM6IHtkaWdpdHM6IHRydWV9LFxuXHRcdGNyZWRpdGNhcmQ6IHtjcmVkaXRjYXJkOiB0cnVlfVxuXHR9LFxuXG5cdGFkZENsYXNzUnVsZXM6IGZ1bmN0aW9uKCBjbGFzc05hbWUsIHJ1bGVzICkge1xuXHRcdGlmICggY2xhc3NOYW1lLmNvbnN0cnVjdG9yID09PSBTdHJpbmcgKSB7XG5cdFx0XHR0aGlzLmNsYXNzUnVsZVNldHRpbmdzW2NsYXNzTmFtZV0gPSBydWxlcztcblx0XHR9IGVsc2Uge1xuXHRcdFx0JC5leHRlbmQodGhpcy5jbGFzc1J1bGVTZXR0aW5ncywgY2xhc3NOYW1lKTtcblx0XHR9XG5cdH0sXG5cblx0Y2xhc3NSdWxlczogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0dmFyIHJ1bGVzID0ge307XG5cdFx0dmFyIGNsYXNzZXMgPSAkKGVsZW1lbnQpLmF0dHIoXCJjbGFzc1wiKTtcblx0XHRpZiAoIGNsYXNzZXMgKSB7XG5cdFx0XHQkLmVhY2goY2xhc3Nlcy5zcGxpdChcIiBcIiksIGZ1bmN0aW9uKCkge1xuXHRcdFx0XHRpZiAoIHRoaXMgaW4gJC52YWxpZGF0b3IuY2xhc3NSdWxlU2V0dGluZ3MgKSB7XG5cdFx0XHRcdFx0JC5leHRlbmQocnVsZXMsICQudmFsaWRhdG9yLmNsYXNzUnVsZVNldHRpbmdzW3RoaXNdKTtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cdFx0fVxuXHRcdHJldHVybiBydWxlcztcblx0fSxcblxuXHRhdHRyaWJ1dGVSdWxlczogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0dmFyIHJ1bGVzID0ge307XG5cdFx0dmFyICRlbGVtZW50ID0gJChlbGVtZW50KTtcblx0XHR2YXIgdHlwZSA9ICRlbGVtZW50WzBdLmdldEF0dHJpYnV0ZShcInR5cGVcIik7XG5cblx0XHRmb3IgKHZhciBtZXRob2QgaW4gJC52YWxpZGF0b3IubWV0aG9kcykge1xuXHRcdFx0dmFyIHZhbHVlO1xuXG5cdFx0XHQvLyBzdXBwb3J0IGZvciA8aW5wdXQgcmVxdWlyZWQ+IGluIGJvdGggaHRtbDUgYW5kIG9sZGVyIGJyb3dzZXJzXG5cdFx0XHRpZiAoIG1ldGhvZCA9PT0gXCJyZXF1aXJlZFwiICkge1xuXHRcdFx0XHR2YWx1ZSA9ICRlbGVtZW50LmdldCgwKS5nZXRBdHRyaWJ1dGUobWV0aG9kKTtcblx0XHRcdFx0Ly8gU29tZSBicm93c2VycyByZXR1cm4gYW4gZW1wdHkgc3RyaW5nIGZvciB0aGUgcmVxdWlyZWQgYXR0cmlidXRlXG5cdFx0XHRcdC8vIGFuZCBub24tSFRNTDUgYnJvd3NlcnMgbWlnaHQgaGF2ZSByZXF1aXJlZD1cIlwiIG1hcmt1cFxuXHRcdFx0XHRpZiAoIHZhbHVlID09PSBcIlwiICkge1xuXHRcdFx0XHRcdHZhbHVlID0gdHJ1ZTtcblx0XHRcdFx0fVxuXHRcdFx0XHQvLyBmb3JjZSBub24tSFRNTDUgYnJvd3NlcnMgdG8gcmV0dXJuIGJvb2xcblx0XHRcdFx0dmFsdWUgPSAhIXZhbHVlO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0dmFsdWUgPSAkZWxlbWVudC5hdHRyKG1ldGhvZCk7XG5cdFx0XHR9XG5cblx0XHRcdC8vIGNvbnZlcnQgdGhlIHZhbHVlIHRvIGEgbnVtYmVyIGZvciBudW1iZXIgaW5wdXRzLCBhbmQgZm9yIHRleHQgZm9yIGJhY2t3YXJkcyBjb21wYWJpbGl0eVxuXHRcdFx0Ly8gYWxsb3dzIHR5cGU9XCJkYXRlXCIgYW5kIG90aGVycyB0byBiZSBjb21wYXJlZCBhcyBzdHJpbmdzXG5cdFx0XHRpZiAoIC9taW58bWF4Ly50ZXN0KCBtZXRob2QgKSAmJiAoIHR5cGUgPT09IG51bGwgfHwgL251bWJlcnxyYW5nZXx0ZXh0Ly50ZXN0KCB0eXBlICkgKSApIHtcblx0XHRcdFx0dmFsdWUgPSBOdW1iZXIodmFsdWUpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoIHZhbHVlICkge1xuXHRcdFx0XHRydWxlc1ttZXRob2RdID0gdmFsdWU7XG5cdFx0XHR9IGVsc2UgaWYgKCB0eXBlID09PSBtZXRob2QgJiYgdHlwZSAhPT0gJ3JhbmdlJyApIHtcblx0XHRcdFx0Ly8gZXhjZXB0aW9uOiB0aGUganF1ZXJ5IHZhbGlkYXRlICdyYW5nZScgbWV0aG9kXG5cdFx0XHRcdC8vIGRvZXMgbm90IHRlc3QgZm9yIHRoZSBodG1sNSAncmFuZ2UnIHR5cGVcblx0XHRcdFx0cnVsZXNbbWV0aG9kXSA9IHRydWU7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0Ly8gbWF4bGVuZ3RoIG1heSBiZSByZXR1cm5lZCBhcyAtMSwgMjE0NzQ4MzY0NyAoSUUpIGFuZCA1MjQyODggKHNhZmFyaSkgZm9yIHRleHQgaW5wdXRzXG5cdFx0aWYgKCBydWxlcy5tYXhsZW5ndGggJiYgLy0xfDIxNDc0ODM2NDd8NTI0Mjg4Ly50ZXN0KHJ1bGVzLm1heGxlbmd0aCkgKSB7XG5cdFx0XHRkZWxldGUgcnVsZXMubWF4bGVuZ3RoO1xuXHRcdH1cblxuXHRcdHJldHVybiBydWxlcztcblx0fSxcblxuXHRkYXRhUnVsZXM6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdHZhciBtZXRob2QsIHZhbHVlLFxuXHRcdFx0cnVsZXMgPSB7fSwgJGVsZW1lbnQgPSAkKGVsZW1lbnQpO1xuXHRcdGZvciAobWV0aG9kIGluICQudmFsaWRhdG9yLm1ldGhvZHMpIHtcblx0XHRcdHZhbHVlID0gJGVsZW1lbnQuZGF0YShcInJ1bGUtXCIgKyBtZXRob2QudG9Mb3dlckNhc2UoKSk7XG5cdFx0XHRpZiAoIHZhbHVlICE9PSB1bmRlZmluZWQgKSB7XG5cdFx0XHRcdHJ1bGVzW21ldGhvZF0gPSB2YWx1ZTtcblx0XHRcdH1cblx0XHR9XG5cdFx0cmV0dXJuIHJ1bGVzO1xuXHR9LFxuXG5cdHN0YXRpY1J1bGVzOiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHR2YXIgcnVsZXMgPSB7fTtcblx0XHR2YXIgdmFsaWRhdG9yID0gJC5kYXRhKGVsZW1lbnQuZm9ybSwgXCJ2YWxpZGF0b3JcIik7XG5cdFx0aWYgKCB2YWxpZGF0b3Iuc2V0dGluZ3MucnVsZXMgKSB7XG5cdFx0XHRydWxlcyA9ICQudmFsaWRhdG9yLm5vcm1hbGl6ZVJ1bGUodmFsaWRhdG9yLnNldHRpbmdzLnJ1bGVzW2VsZW1lbnQubmFtZV0pIHx8IHt9O1xuXHRcdH1cblx0XHRyZXR1cm4gcnVsZXM7XG5cdH0sXG5cblx0bm9ybWFsaXplUnVsZXM6IGZ1bmN0aW9uKCBydWxlcywgZWxlbWVudCApIHtcblx0XHQvLyBoYW5kbGUgZGVwZW5kZW5jeSBjaGVja1xuXHRcdCQuZWFjaChydWxlcywgZnVuY3Rpb24oIHByb3AsIHZhbCApIHtcblx0XHRcdC8vIGlnbm9yZSBydWxlIHdoZW4gcGFyYW0gaXMgZXhwbGljaXRseSBmYWxzZSwgZWcuIHJlcXVpcmVkOmZhbHNlXG5cdFx0XHRpZiAoIHZhbCA9PT0gZmFsc2UgKSB7XG5cdFx0XHRcdGRlbGV0ZSBydWxlc1twcm9wXTtcblx0XHRcdFx0cmV0dXJuO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCB2YWwucGFyYW0gfHwgdmFsLmRlcGVuZHMgKSB7XG5cdFx0XHRcdHZhciBrZWVwUnVsZSA9IHRydWU7XG5cdFx0XHRcdHN3aXRjaCAodHlwZW9mIHZhbC5kZXBlbmRzKSB7XG5cdFx0XHRcdGNhc2UgXCJzdHJpbmdcIjpcblx0XHRcdFx0XHRrZWVwUnVsZSA9ICEhJCh2YWwuZGVwZW5kcywgZWxlbWVudC5mb3JtKS5sZW5ndGg7XG5cdFx0XHRcdFx0YnJlYWs7XG5cdFx0XHRcdGNhc2UgXCJmdW5jdGlvblwiOlxuXHRcdFx0XHRcdGtlZXBSdWxlID0gdmFsLmRlcGVuZHMuY2FsbChlbGVtZW50LCBlbGVtZW50KTtcblx0XHRcdFx0XHRicmVhaztcblx0XHRcdFx0fVxuXHRcdFx0XHRpZiAoIGtlZXBSdWxlICkge1xuXHRcdFx0XHRcdHJ1bGVzW3Byb3BdID0gdmFsLnBhcmFtICE9PSB1bmRlZmluZWQgPyB2YWwucGFyYW0gOiB0cnVlO1xuXHRcdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRcdGRlbGV0ZSBydWxlc1twcm9wXTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdH0pO1xuXG5cdFx0Ly8gZXZhbHVhdGUgcGFyYW1ldGVyc1xuXHRcdCQuZWFjaChydWxlcywgZnVuY3Rpb24oIHJ1bGUsIHBhcmFtZXRlciApIHtcblx0XHRcdHJ1bGVzW3J1bGVdID0gJC5pc0Z1bmN0aW9uKHBhcmFtZXRlcikgPyBwYXJhbWV0ZXIoZWxlbWVudCkgOiBwYXJhbWV0ZXI7XG5cdFx0fSk7XG5cblx0XHQvLyBjbGVhbiBudW1iZXIgcGFyYW1ldGVyc1xuXHRcdCQuZWFjaChbJ21pbmxlbmd0aCcsICdtYXhsZW5ndGgnXSwgZnVuY3Rpb24oKSB7XG5cdFx0XHRpZiAoIHJ1bGVzW3RoaXNdICkge1xuXHRcdFx0XHRydWxlc1t0aGlzXSA9IE51bWJlcihydWxlc1t0aGlzXSk7XG5cdFx0XHR9XG5cdFx0fSk7XG5cdFx0JC5lYWNoKFsncmFuZ2VsZW5ndGgnLCAncmFuZ2UnXSwgZnVuY3Rpb24oKSB7XG5cdFx0XHR2YXIgcGFydHM7XG5cdFx0XHRpZiAoIHJ1bGVzW3RoaXNdICkge1xuXHRcdFx0XHRpZiAoICQuaXNBcnJheShydWxlc1t0aGlzXSkgKSB7XG5cdFx0XHRcdFx0cnVsZXNbdGhpc10gPSBbTnVtYmVyKHJ1bGVzW3RoaXNdWzBdKSwgTnVtYmVyKHJ1bGVzW3RoaXNdWzFdKV07XG5cdFx0XHRcdH0gZWxzZSBpZiAoIHR5cGVvZiBydWxlc1t0aGlzXSA9PT0gXCJzdHJpbmdcIiApIHtcblx0XHRcdFx0XHRwYXJ0cyA9IHJ1bGVzW3RoaXNdLnNwbGl0KC9bXFxzLF0rLyk7XG5cdFx0XHRcdFx0cnVsZXNbdGhpc10gPSBbTnVtYmVyKHBhcnRzWzBdKSwgTnVtYmVyKHBhcnRzWzFdKV07XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdGlmICggJC52YWxpZGF0b3IuYXV0b0NyZWF0ZVJhbmdlcyApIHtcblx0XHRcdC8vIGF1dG8tY3JlYXRlIHJhbmdlc1xuXHRcdFx0aWYgKCBydWxlcy5taW4gJiYgcnVsZXMubWF4ICkge1xuXHRcdFx0XHRydWxlcy5yYW5nZSA9IFtydWxlcy5taW4sIHJ1bGVzLm1heF07XG5cdFx0XHRcdGRlbGV0ZSBydWxlcy5taW47XG5cdFx0XHRcdGRlbGV0ZSBydWxlcy5tYXg7XG5cdFx0XHR9XG5cdFx0XHRpZiAoIHJ1bGVzLm1pbmxlbmd0aCAmJiBydWxlcy5tYXhsZW5ndGggKSB7XG5cdFx0XHRcdHJ1bGVzLnJhbmdlbGVuZ3RoID0gW3J1bGVzLm1pbmxlbmd0aCwgcnVsZXMubWF4bGVuZ3RoXTtcblx0XHRcdFx0ZGVsZXRlIHJ1bGVzLm1pbmxlbmd0aDtcblx0XHRcdFx0ZGVsZXRlIHJ1bGVzLm1heGxlbmd0aDtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRyZXR1cm4gcnVsZXM7XG5cdH0sXG5cblx0Ly8gQ29udmVydHMgYSBzaW1wbGUgc3RyaW5nIHRvIGEge3N0cmluZzogdHJ1ZX0gcnVsZSwgZS5nLiwgXCJyZXF1aXJlZFwiIHRvIHtyZXF1aXJlZDp0cnVlfVxuXHRub3JtYWxpemVSdWxlOiBmdW5jdGlvbiggZGF0YSApIHtcblx0XHRpZiAoIHR5cGVvZiBkYXRhID09PSBcInN0cmluZ1wiICkge1xuXHRcdFx0dmFyIHRyYW5zZm9ybWVkID0ge307XG5cdFx0XHQkLmVhY2goZGF0YS5zcGxpdCgvXFxzLyksIGZ1bmN0aW9uKCkge1xuXHRcdFx0XHR0cmFuc2Zvcm1lZFt0aGlzXSA9IHRydWU7XG5cdFx0XHR9KTtcblx0XHRcdGRhdGEgPSB0cmFuc2Zvcm1lZDtcblx0XHR9XG5cdFx0cmV0dXJuIGRhdGE7XG5cdH0sXG5cblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vVmFsaWRhdG9yL2FkZE1ldGhvZFxuXHRhZGRNZXRob2Q6IGZ1bmN0aW9uKCBuYW1lLCBtZXRob2QsIG1lc3NhZ2UgKSB7XG5cdFx0JC52YWxpZGF0b3IubWV0aG9kc1tuYW1lXSA9IG1ldGhvZDtcblx0XHQkLnZhbGlkYXRvci5tZXNzYWdlc1tuYW1lXSA9IG1lc3NhZ2UgIT09IHVuZGVmaW5lZCA/IG1lc3NhZ2UgOiAkLnZhbGlkYXRvci5tZXNzYWdlc1tuYW1lXTtcblx0XHRpZiAoIG1ldGhvZC5sZW5ndGggPCAzICkge1xuXHRcdFx0JC52YWxpZGF0b3IuYWRkQ2xhc3NSdWxlcyhuYW1lLCAkLnZhbGlkYXRvci5ub3JtYWxpemVSdWxlKG5hbWUpKTtcblx0XHR9XG5cdH0sXG5cblx0bWV0aG9kczoge1xuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yZXF1aXJlZFxuXHRcdHJlcXVpcmVkOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0Ly8gY2hlY2sgaWYgZGVwZW5kZW5jeSBpcyBtZXRcblx0XHRcdGlmICggIXRoaXMuZGVwZW5kKHBhcmFtLCBlbGVtZW50KSApIHtcblx0XHRcdFx0cmV0dXJuIFwiZGVwZW5kZW5jeS1taXNtYXRjaFwiO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCBlbGVtZW50Lm5vZGVOYW1lLnRvTG93ZXJDYXNlKCkgPT09IFwic2VsZWN0XCIgKSB7XG5cdFx0XHRcdC8vIGNvdWxkIGJlIGFuIGFycmF5IGZvciBzZWxlY3QtbXVsdGlwbGUgb3IgYSBzdHJpbmcsIGJvdGggYXJlIGZpbmUgdGhpcyB3YXlcblx0XHRcdFx0dmFyIHZhbCA9ICQoZWxlbWVudCkudmFsKCk7XG5cdFx0XHRcdHJldHVybiB2YWwgJiYgdmFsLmxlbmd0aCA+IDA7XG5cdFx0XHR9XG5cdFx0XHRpZiAoIHRoaXMuY2hlY2thYmxlKGVsZW1lbnQpICkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5nZXRMZW5ndGgodmFsdWUsIGVsZW1lbnQpID4gMDtcblx0XHRcdH1cblx0XHRcdHJldHVybiAkLnRyaW0odmFsdWUpLmxlbmd0aCA+IDA7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvZW1haWxcblx0XHRlbWFpbDogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xuXHRcdFx0Ly8gY29udHJpYnV0ZWQgYnkgU2NvdHQgR29uemFsZXo6IGh0dHA6Ly9wcm9qZWN0cy5zY290dHNwbGF5Z3JvdW5kLmNvbS9lbWFpbF9hZGRyZXNzX3ZhbGlkYXRpb24vXG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCAvXigoKFthLXpdfFxcZHxbISNcXCQlJidcXCpcXCtcXC1cXC89XFw/XFxeX2B7XFx8fX5dfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSsoXFwuKFthLXpdfFxcZHxbISNcXCQlJidcXCpcXCtcXC1cXC89XFw/XFxeX2B7XFx8fX5dfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSspKil8KChcXHgyMikoKCgoXFx4MjB8XFx4MDkpKihcXHgwZFxceDBhKSk/KFxceDIwfFxceDA5KSspPygoW1xceDAxLVxceDA4XFx4MGJcXHgwY1xceDBlLVxceDFmXFx4N2ZdfFxceDIxfFtcXHgyMy1cXHg1Yl18W1xceDVkLVxceDdlXXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KFxcXFwoW1xceDAxLVxceDA5XFx4MGJcXHgwY1xceDBkLVxceDdmXXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkpKSkqKCgoXFx4MjB8XFx4MDkpKihcXHgwZFxceDBhKSk/KFxceDIwfFxceDA5KSspPyhcXHgyMikpKUAoKChbYS16XXxcXGR8W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfCgoW2Etel18XFxkfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKShbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSooW2Etel18XFxkfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSkpXFwuKSsoKFthLXpdfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoKFthLXpdfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKShbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSooW2Etel18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKSkkL2kudGVzdCh2YWx1ZSk7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvdXJsXG5cdFx0dXJsOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XG5cdFx0XHQvLyBjb250cmlidXRlZCBieSBTY290dCBHb256YWxlejogaHR0cDovL3Byb2plY3RzLnNjb3R0c3BsYXlncm91bmQuY29tL2lyaS9cblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IC9eKGh0dHBzP3xzP2Z0cCk6XFwvXFwvKCgoKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfCglW1xcZGEtZl17Mn0pfFshXFwkJidcXChcXClcXCpcXCssOz1dfDopKkApPygoKFxcZHxbMS05XVxcZHwxXFxkXFxkfDJbMC00XVxcZHwyNVswLTVdKVxcLihcXGR8WzEtOV1cXGR8MVxcZFxcZHwyWzAtNF1cXGR8MjVbMC01XSlcXC4oXFxkfFsxLTldXFxkfDFcXGRcXGR8MlswLTRdXFxkfDI1WzAtNV0pXFwuKFxcZHxbMS05XVxcZHwxXFxkXFxkfDJbMC00XVxcZHwyNVswLTVdKSl8KCgoW2Etel18XFxkfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkqKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkpKVxcLikrKChbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KChbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkqKFthLXpdfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSkpXFwuPykoOlxcZCopPykoXFwvKCgoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKSsoXFwvKChbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoJVtcXGRhLWZdezJ9KXxbIVxcJCYnXFwoXFwpXFwqXFwrLDs9XXw6fEApKikqKT8pPyhcXD8oKChbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoJVtcXGRhLWZdezJ9KXxbIVxcJCYnXFwoXFwpXFwqXFwrLDs9XXw6fEApfFtcXHVFMDAwLVxcdUY4RkZdfFxcL3xcXD8pKik/KCMoKChbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoJVtcXGRhLWZdezJ9KXxbIVxcJCYnXFwoXFwpXFwqXFwrLDs9XXw6fEApfFxcL3xcXD8pKik/JC9pLnRlc3QodmFsdWUpO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2RhdGVcblx0XHRkYXRlOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCAhL0ludmFsaWR8TmFOLy50ZXN0KG5ldyBEYXRlKHZhbHVlKS50b1N0cmluZygpKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9kYXRlSVNPXG5cdFx0ZGF0ZUlTTzogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgL15cXGR7NH1bXFwvXFwtXVxcZHsxLDJ9W1xcL1xcLV1cXGR7MSwyfSQvLnRlc3QodmFsdWUpO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL251bWJlclxuXHRcdG51bWJlcjogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgL14tPyg/OlxcZCt8XFxkezEsM30oPzosXFxkezN9KSspPyg/OlxcLlxcZCspPyQvLnRlc3QodmFsdWUpO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2RpZ2l0c1xuXHRcdGRpZ2l0czogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgL15cXGQrJC8udGVzdCh2YWx1ZSk7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvY3JlZGl0Y2FyZFxuXHRcdC8vIGJhc2VkIG9uIGh0dHA6Ly9lbi53aWtpcGVkaWEub3JnL3dpa2kvTHVoblxuXHRcdGNyZWRpdGNhcmQ6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcblx0XHRcdGlmICggdGhpcy5vcHRpb25hbChlbGVtZW50KSApIHtcblx0XHRcdFx0cmV0dXJuIFwiZGVwZW5kZW5jeS1taXNtYXRjaFwiO1xuXHRcdFx0fVxuXHRcdFx0Ly8gYWNjZXB0IG9ubHkgc3BhY2VzLCBkaWdpdHMgYW5kIGRhc2hlc1xuXHRcdFx0aWYgKCAvW14wLTkgXFwtXSsvLnRlc3QodmFsdWUpICkge1xuXHRcdFx0XHRyZXR1cm4gZmFsc2U7XG5cdFx0XHR9XG5cdFx0XHR2YXIgbkNoZWNrID0gMCxcblx0XHRcdFx0bkRpZ2l0ID0gMCxcblx0XHRcdFx0YkV2ZW4gPSBmYWxzZTtcblxuXHRcdFx0dmFsdWUgPSB2YWx1ZS5yZXBsYWNlKC9cXEQvZywgXCJcIik7XG5cblx0XHRcdGZvciAodmFyIG4gPSB2YWx1ZS5sZW5ndGggLSAxOyBuID49IDA7IG4tLSkge1xuXHRcdFx0XHR2YXIgY0RpZ2l0ID0gdmFsdWUuY2hhckF0KG4pO1xuXHRcdFx0XHRuRGlnaXQgPSBwYXJzZUludChjRGlnaXQsIDEwKTtcblx0XHRcdFx0aWYgKCBiRXZlbiApIHtcblx0XHRcdFx0XHRpZiAoIChuRGlnaXQgKj0gMikgPiA5ICkge1xuXHRcdFx0XHRcdFx0bkRpZ2l0IC09IDk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHR9XG5cdFx0XHRcdG5DaGVjayArPSBuRGlnaXQ7XG5cdFx0XHRcdGJFdmVuID0gIWJFdmVuO1xuXHRcdFx0fVxuXG5cdFx0XHRyZXR1cm4gKG5DaGVjayAlIDEwKSA9PT0gMDtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9taW5sZW5ndGhcblx0XHRtaW5sZW5ndGg6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XG5cdFx0XHR2YXIgbGVuZ3RoID0gJC5pc0FycmF5KCB2YWx1ZSApID8gdmFsdWUubGVuZ3RoIDogdGhpcy5nZXRMZW5ndGgoJC50cmltKHZhbHVlKSwgZWxlbWVudCk7XG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCBsZW5ndGggPj0gcGFyYW07XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvbWF4bGVuZ3RoXG5cdFx0bWF4bGVuZ3RoOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0dmFyIGxlbmd0aCA9ICQuaXNBcnJheSggdmFsdWUgKSA/IHZhbHVlLmxlbmd0aCA6IHRoaXMuZ2V0TGVuZ3RoKCQudHJpbSh2YWx1ZSksIGVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgbGVuZ3RoIDw9IHBhcmFtO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL3JhbmdlbGVuZ3RoXG5cdFx0cmFuZ2VsZW5ndGg6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XG5cdFx0XHR2YXIgbGVuZ3RoID0gJC5pc0FycmF5KCB2YWx1ZSApID8gdmFsdWUubGVuZ3RoIDogdGhpcy5nZXRMZW5ndGgoJC50cmltKHZhbHVlKSwgZWxlbWVudCk7XG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCAoIGxlbmd0aCA+PSBwYXJhbVswXSAmJiBsZW5ndGggPD0gcGFyYW1bMV0gKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9taW5cblx0XHRtaW46IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCB2YWx1ZSA+PSBwYXJhbTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9tYXhcblx0XHRtYXg6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCB2YWx1ZSA8PSBwYXJhbTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yYW5nZVxuXHRcdHJhbmdlOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgKCB2YWx1ZSA+PSBwYXJhbVswXSAmJiB2YWx1ZSA8PSBwYXJhbVsxXSApO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2VxdWFsVG9cblx0XHRlcXVhbFRvOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0Ly8gYmluZCB0byB0aGUgYmx1ciBldmVudCBvZiB0aGUgdGFyZ2V0IGluIG9yZGVyIHRvIHJldmFsaWRhdGUgd2hlbmV2ZXIgdGhlIHRhcmdldCBmaWVsZCBpcyB1cGRhdGVkXG5cdFx0XHQvLyBUT0RPIGZpbmQgYSB3YXkgdG8gYmluZCB0aGUgZXZlbnQganVzdCBvbmNlLCBhdm9pZGluZyB0aGUgdW5iaW5kLXJlYmluZCBvdmVyaGVhZFxuXHRcdFx0dmFyIHRhcmdldCA9ICQocGFyYW0pO1xuXHRcdFx0aWYgKCB0aGlzLnNldHRpbmdzLm9uZm9jdXNvdXQgKSB7XG5cdFx0XHRcdHRhcmdldC51bmJpbmQoXCIudmFsaWRhdGUtZXF1YWxUb1wiKS5iaW5kKFwiYmx1ci52YWxpZGF0ZS1lcXVhbFRvXCIsIGZ1bmN0aW9uKCkge1xuXHRcdFx0XHRcdCQoZWxlbWVudCkudmFsaWQoKTtcblx0XHRcdFx0fSk7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gdmFsdWUgPT09IHRhcmdldC52YWwoKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yZW1vdGVcblx0XHRyZW1vdGU6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCwgcGFyYW0gKSB7XG5cdFx0XHRpZiAoIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgKSB7XG5cdFx0XHRcdHJldHVybiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcblx0XHRcdH1cblxuXHRcdFx0dmFyIHByZXZpb3VzID0gdGhpcy5wcmV2aW91c1ZhbHVlKGVsZW1lbnQpO1xuXHRcdFx0aWYgKCF0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0gKSB7XG5cdFx0XHRcdHRoaXMuc2V0dGluZ3MubWVzc2FnZXNbZWxlbWVudC5uYW1lXSA9IHt9O1xuXHRcdFx0fVxuXHRcdFx0cHJldmlvdXMub3JpZ2luYWxNZXNzYWdlID0gdGhpcy5zZXR0aW5ncy5tZXNzYWdlc1tlbGVtZW50Lm5hbWVdLnJlbW90ZTtcblx0XHRcdHRoaXMuc2V0dGluZ3MubWVzc2FnZXNbZWxlbWVudC5uYW1lXS5yZW1vdGUgPSBwcmV2aW91cy5tZXNzYWdlO1xuXG5cdFx0XHRwYXJhbSA9IHR5cGVvZiBwYXJhbSA9PT0gXCJzdHJpbmdcIiAmJiB7dXJsOnBhcmFtfSB8fCBwYXJhbTtcblxuXHRcdFx0aWYgKCBwcmV2aW91cy5vbGQgPT09IHZhbHVlICkge1xuXHRcdFx0XHRyZXR1cm4gcHJldmlvdXMudmFsaWQ7XG5cdFx0XHR9XG5cblx0XHRcdHByZXZpb3VzLm9sZCA9IHZhbHVlO1xuXHRcdFx0dmFyIHZhbGlkYXRvciA9IHRoaXM7XG5cdFx0XHR0aGlzLnN0YXJ0UmVxdWVzdChlbGVtZW50KTtcblx0XHRcdHZhciBkYXRhID0ge307XG5cdFx0XHRkYXRhW2VsZW1lbnQubmFtZV0gPSB2YWx1ZTtcblx0XHRcdCQuYWpheCgkLmV4dGVuZCh0cnVlLCB7XG5cdFx0XHRcdHVybDogcGFyYW0sXG5cdFx0XHRcdG1vZGU6IFwiYWJvcnRcIixcblx0XHRcdFx0cG9ydDogXCJ2YWxpZGF0ZVwiICsgZWxlbWVudC5uYW1lLFxuXHRcdFx0XHRkYXRhVHlwZTogXCJqc29uXCIsXG5cdFx0XHRcdGRhdGE6IGRhdGEsXG5cdFx0XHRcdHN1Y2Nlc3M6IGZ1bmN0aW9uKCByZXNwb25zZSApIHtcblx0XHRcdFx0XHR2YWxpZGF0b3Iuc2V0dGluZ3MubWVzc2FnZXNbZWxlbWVudC5uYW1lXS5yZW1vdGUgPSBwcmV2aW91cy5vcmlnaW5hbE1lc3NhZ2U7XG5cdFx0XHRcdFx0dmFyIHZhbGlkID0gcmVzcG9uc2UgPT09IHRydWUgfHwgcmVzcG9uc2UgPT09IFwidHJ1ZVwiO1xuXHRcdFx0XHRcdGlmICggdmFsaWQgKSB7XG5cdFx0XHRcdFx0XHR2YXIgc3VibWl0dGVkID0gdmFsaWRhdG9yLmZvcm1TdWJtaXR0ZWQ7XG5cdFx0XHRcdFx0XHR2YWxpZGF0b3IucHJlcGFyZUVsZW1lbnQoZWxlbWVudCk7XG5cdFx0XHRcdFx0XHR2YWxpZGF0b3IuZm9ybVN1Ym1pdHRlZCA9IHN1Ym1pdHRlZDtcblx0XHRcdFx0XHRcdHZhbGlkYXRvci5zdWNjZXNzTGlzdC5wdXNoKGVsZW1lbnQpO1xuXHRcdFx0XHRcdFx0ZGVsZXRlIHZhbGlkYXRvci5pbnZhbGlkW2VsZW1lbnQubmFtZV07XG5cdFx0XHRcdFx0XHR2YWxpZGF0b3Iuc2hvd0Vycm9ycygpO1xuXHRcdFx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdFx0XHR2YXIgZXJyb3JzID0ge307XG5cdFx0XHRcdFx0XHR2YXIgbWVzc2FnZSA9IHJlc3BvbnNlIHx8IHZhbGlkYXRvci5kZWZhdWx0TWVzc2FnZSggZWxlbWVudCwgXCJyZW1vdGVcIiApO1xuXHRcdFx0XHRcdFx0ZXJyb3JzW2VsZW1lbnQubmFtZV0gPSBwcmV2aW91cy5tZXNzYWdlID0gJC5pc0Z1bmN0aW9uKG1lc3NhZ2UpID8gbWVzc2FnZSh2YWx1ZSkgOiBtZXNzYWdlO1xuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLmludmFsaWRbZWxlbWVudC5uYW1lXSA9IHRydWU7XG5cdFx0XHRcdFx0XHR2YWxpZGF0b3Iuc2hvd0Vycm9ycyhlcnJvcnMpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRwcmV2aW91cy52YWxpZCA9IHZhbGlkO1xuXHRcdFx0XHRcdHZhbGlkYXRvci5zdG9wUmVxdWVzdChlbGVtZW50LCB2YWxpZCk7XG5cdFx0XHRcdH1cblx0XHRcdH0sIHBhcmFtKSk7XG5cdFx0XHRyZXR1cm4gXCJwZW5kaW5nXCI7XG5cdFx0fVxuXG5cdH1cblxufSk7XG5cbi8vIGRlcHJlY2F0ZWQsIHVzZSAkLnZhbGlkYXRvci5mb3JtYXQgaW5zdGVhZFxuJC5mb3JtYXQgPSAkLnZhbGlkYXRvci5mb3JtYXQ7XG5cbn0oalF1ZXJ5KSk7XG5cbi8vIGFqYXggbW9kZTogYWJvcnRcbi8vIHVzYWdlOiAkLmFqYXgoeyBtb2RlOiBcImFib3J0XCJbLCBwb3J0OiBcInVuaXF1ZXBvcnRcIl19KTtcbi8vIGlmIG1vZGU6XCJhYm9ydFwiIGlzIHVzZWQsIHRoZSBwcmV2aW91cyByZXF1ZXN0IG9uIHRoYXQgcG9ydCAocG9ydCBjYW4gYmUgdW5kZWZpbmVkKSBpcyBhYm9ydGVkIHZpYSBYTUxIdHRwUmVxdWVzdC5hYm9ydCgpXG4oZnVuY3Rpb24oJCkge1xuXHR2YXIgcGVuZGluZ1JlcXVlc3RzID0ge307XG5cdC8vIFVzZSBhIHByZWZpbHRlciBpZiBhdmFpbGFibGUgKDEuNSspXG5cdGlmICggJC5hamF4UHJlZmlsdGVyICkge1xuXHRcdCQuYWpheFByZWZpbHRlcihmdW5jdGlvbiggc2V0dGluZ3MsIF8sIHhociApIHtcblx0XHRcdHZhciBwb3J0ID0gc2V0dGluZ3MucG9ydDtcblx0XHRcdGlmICggc2V0dGluZ3MubW9kZSA9PT0gXCJhYm9ydFwiICkge1xuXHRcdFx0XHRpZiAoIHBlbmRpbmdSZXF1ZXN0c1twb3J0XSApIHtcblx0XHRcdFx0XHRwZW5kaW5nUmVxdWVzdHNbcG9ydF0uYWJvcnQoKTtcblx0XHRcdFx0fVxuXHRcdFx0XHRwZW5kaW5nUmVxdWVzdHNbcG9ydF0gPSB4aHI7XG5cdFx0XHR9XG5cdFx0fSk7XG5cdH0gZWxzZSB7XG5cdFx0Ly8gUHJveHkgYWpheFxuXHRcdHZhciBhamF4ID0gJC5hamF4O1xuXHRcdCQuYWpheCA9IGZ1bmN0aW9uKCBzZXR0aW5ncyApIHtcblx0XHRcdHZhciBtb2RlID0gKCBcIm1vZGVcIiBpbiBzZXR0aW5ncyA/IHNldHRpbmdzIDogJC5hamF4U2V0dGluZ3MgKS5tb2RlLFxuXHRcdFx0XHRwb3J0ID0gKCBcInBvcnRcIiBpbiBzZXR0aW5ncyA/IHNldHRpbmdzIDogJC5hamF4U2V0dGluZ3MgKS5wb3J0O1xuXHRcdFx0aWYgKCBtb2RlID09PSBcImFib3J0XCIgKSB7XG5cdFx0XHRcdGlmICggcGVuZGluZ1JlcXVlc3RzW3BvcnRdICkge1xuXHRcdFx0XHRcdHBlbmRpbmdSZXF1ZXN0c1twb3J0XS5hYm9ydCgpO1xuXHRcdFx0XHR9XG5cdFx0XHRcdHBlbmRpbmdSZXF1ZXN0c1twb3J0XSA9IGFqYXguYXBwbHkodGhpcywgYXJndW1lbnRzKTtcblx0XHRcdFx0cmV0dXJuIHBlbmRpbmdSZXF1ZXN0c1twb3J0XTtcblx0XHRcdH1cblx0XHRcdHJldHVybiBhamF4LmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XG5cdFx0fTtcblx0fVxufShqUXVlcnkpKTtcblxuLy8gcHJvdmlkZXMgZGVsZWdhdGUodHlwZTogU3RyaW5nLCBkZWxlZ2F0ZTogU2VsZWN0b3IsIGhhbmRsZXI6IENhbGxiYWNrKSBwbHVnaW4gZm9yIGVhc2llciBldmVudCBkZWxlZ2F0aW9uXG4vLyBoYW5kbGVyIGlzIG9ubHkgY2FsbGVkIHdoZW4gJChldmVudC50YXJnZXQpLmlzKGRlbGVnYXRlKSwgaW4gdGhlIHNjb3BlIG9mIHRoZSBqcXVlcnktb2JqZWN0IGZvciBldmVudC50YXJnZXRcbihmdW5jdGlvbigkKSB7XG5cdCQuZXh0ZW5kKCQuZm4sIHtcblx0XHR2YWxpZGF0ZURlbGVnYXRlOiBmdW5jdGlvbiggZGVsZWdhdGUsIHR5cGUsIGhhbmRsZXIgKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5iaW5kKHR5cGUsIGZ1bmN0aW9uKCBldmVudCApIHtcblx0XHRcdFx0dmFyIHRhcmdldCA9ICQoZXZlbnQudGFyZ2V0KTtcblx0XHRcdFx0aWYgKCB0YXJnZXQuaXMoZGVsZWdhdGUpICkge1xuXHRcdFx0XHRcdHJldHVybiBoYW5kbGVyLmFwcGx5KHRhcmdldCwgYXJndW1lbnRzKTtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cdFx0fVxuXHR9KTtcbn0oalF1ZXJ5KSk7XG4iLCIoZnVuY3Rpb24gKCQpIHtcblxuICAgICQudmFsaWRhdG9yLmFkZE1ldGhvZChcIm9wdGlvbnJlcXVpcmVkXCIsIGZ1bmN0aW9uICh2YWx1ZSwgZWxlbWVudCwgcGFyYW0pIHtcbiAgICAgICAgdmFyIGlzVmFsaWQgPSB0cnVlO1xuXG4gICAgICAgIGlmICgkKGVsZW1lbnQpLmlzKFwiaW5wdXRcIikpIHtcbiAgICAgICAgICAgIHZhciBwYXJlbnQgPSAkKGVsZW1lbnQpLmNsb3Nlc3QoXCJvbFwiKTtcblxuICAgICAgICAgICAgaXNWYWxpZCA9IHBhcmVudC5maW5kKFwiaW5wdXQ6Y2hlY2tlZFwiKS5sZW5ndGggPiAwO1xuICAgICAgICAgICAgcGFyZW50LnRvZ2dsZUNsYXNzKFwiaW5wdXQtdmFsaWRhdGlvbi1lcnJvclwiLCAhaXNWYWxpZCk7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAoJChlbGVtZW50KS5pcyhcInNlbGVjdFwiKSkge1xuICAgICAgICAgICAgdmFyIHYgPSAkKGVsZW1lbnQpLnZhbCgpO1xuICAgICAgICAgICAgaXNWYWxpZCA9ICEhdiAmJiB2Lmxlbmd0aCA+IDA7XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gaXNWYWxpZDtcbiAgICB9LCBcIkFuIG9wdGlvbiBpcyByZXF1aXJlZFwiKTtcblxuICAgICQudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzLmFkZEJvb2woXCJtYW5kYXRvcnlcIiwgXCJyZXF1aXJlZFwiKTtcbiAgICAkLnZhbGlkYXRvci51bm9idHJ1c2l2ZS5hZGFwdGVycy5hZGRCb29sKFwib3B0aW9ucmVxdWlyZWRcIik7XG59KGpRdWVyeSkpOyIsIi8qIE5VR0VUOiBCRUdJTiBMSUNFTlNFIFRFWFRcbiAqXG4gKiBNaWNyb3NvZnQgZ3JhbnRzIHlvdSB0aGUgcmlnaHQgdG8gdXNlIHRoZXNlIHNjcmlwdCBmaWxlcyBmb3IgdGhlIHNvbGVcbiAqIHB1cnBvc2Ugb2YgZWl0aGVyOiAoaSkgaW50ZXJhY3RpbmcgdGhyb3VnaCB5b3VyIGJyb3dzZXIgd2l0aCB0aGUgTWljcm9zb2Z0XG4gKiB3ZWJzaXRlIG9yIG9ubGluZSBzZXJ2aWNlLCBzdWJqZWN0IHRvIHRoZSBhcHBsaWNhYmxlIGxpY2Vuc2luZyBvciB1c2VcbiAqIHRlcm1zOyBvciAoaWkpIHVzaW5nIHRoZSBmaWxlcyBhcyBpbmNsdWRlZCB3aXRoIGEgTWljcm9zb2Z0IHByb2R1Y3Qgc3ViamVjdFxuICogdG8gdGhhdCBwcm9kdWN0J3MgbGljZW5zZSB0ZXJtcy4gTWljcm9zb2Z0IHJlc2VydmVzIGFsbCBvdGhlciByaWdodHMgdG8gdGhlXG4gKiBmaWxlcyBub3QgZXhwcmVzc2x5IGdyYW50ZWQgYnkgTWljcm9zb2Z0LCB3aGV0aGVyIGJ5IGltcGxpY2F0aW9uLCBlc3RvcHBlbFxuICogb3Igb3RoZXJ3aXNlLiBJbnNvZmFyIGFzIGEgc2NyaXB0IGZpbGUgaXMgZHVhbCBsaWNlbnNlZCB1bmRlciBHUEwsXG4gKiBNaWNyb3NvZnQgbmVpdGhlciB0b29rIHRoZSBjb2RlIHVuZGVyIEdQTCBub3IgZGlzdHJpYnV0ZXMgaXQgdGhlcmV1bmRlciBidXRcbiAqIHVuZGVyIHRoZSB0ZXJtcyBzZXQgb3V0IGluIHRoaXMgcGFyYWdyYXBoLiBBbGwgbm90aWNlcyBhbmQgbGljZW5zZXNcbiAqIGJlbG93IGFyZSBmb3IgaW5mb3JtYXRpb25hbCBwdXJwb3NlcyBvbmx5LlxuICpcbiAqIE5VR0VUOiBFTkQgTElDRU5TRSBURVhUICovXG4vKiFcbioqIFVub2J0cnVzaXZlIHZhbGlkYXRpb24gc3VwcG9ydCBsaWJyYXJ5IGZvciBqUXVlcnkgYW5kIGpRdWVyeSBWYWxpZGF0ZVxuKiogQ29weXJpZ2h0IChDKSBNaWNyb3NvZnQgQ29ycG9yYXRpb24uIEFsbCByaWdodHMgcmVzZXJ2ZWQuXG4qL1xuXG4vKmpzbGludCB3aGl0ZTogdHJ1ZSwgYnJvd3NlcjogdHJ1ZSwgb25ldmFyOiB0cnVlLCB1bmRlZjogdHJ1ZSwgbm9tZW46IHRydWUsIGVxZXFlcTogdHJ1ZSwgcGx1c3BsdXM6IHRydWUsIGJpdHdpc2U6IHRydWUsIHJlZ2V4cDogdHJ1ZSwgbmV3Y2FwOiB0cnVlLCBpbW1lZDogdHJ1ZSwgc3RyaWN0OiBmYWxzZSAqL1xuLypnbG9iYWwgZG9jdW1lbnQ6IGZhbHNlLCBqUXVlcnk6IGZhbHNlICovXG5cbihmdW5jdGlvbiAoJCkge1xuICAgIHZhciAkalF2YWwgPSAkLnZhbGlkYXRvcixcbiAgICAgICAgYWRhcHRlcnMsXG4gICAgICAgIGRhdGFfdmFsaWRhdGlvbiA9IFwidW5vYnRydXNpdmVWYWxpZGF0aW9uXCI7XG5cbiAgICBmdW5jdGlvbiBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIHJ1bGVOYW1lLCB2YWx1ZSkge1xuICAgICAgICBvcHRpb25zLnJ1bGVzW3J1bGVOYW1lXSA9IHZhbHVlO1xuICAgICAgICBpZiAob3B0aW9ucy5tZXNzYWdlKSB7XG4gICAgICAgICAgICBvcHRpb25zLm1lc3NhZ2VzW3J1bGVOYW1lXSA9IG9wdGlvbnMubWVzc2FnZTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGZ1bmN0aW9uIHNwbGl0QW5kVHJpbSh2YWx1ZSkge1xuICAgICAgICByZXR1cm4gdmFsdWUucmVwbGFjZSgvXlxccyt8XFxzKyQvZywgXCJcIikuc3BsaXQoL1xccyosXFxzKi9nKTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBlc2NhcGVBdHRyaWJ1dGVWYWx1ZSh2YWx1ZSkge1xuICAgICAgICAvLyBBcyBtZW50aW9uZWQgb24gaHR0cDovL2FwaS5qcXVlcnkuY29tL2NhdGVnb3J5L3NlbGVjdG9ycy9cbiAgICAgICAgcmV0dXJuIHZhbHVlLnJlcGxhY2UoLyhbIVwiIyQlJicoKSorLC4vOjs8PT4/QFxcW1xcXFxcXF1eYHt8fX5dKS9nLCBcIlxcXFwkMVwiKTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBnZXRNb2RlbFByZWZpeChmaWVsZE5hbWUpIHtcbiAgICAgICAgcmV0dXJuIGZpZWxkTmFtZS5zdWJzdHIoMCwgZmllbGROYW1lLmxhc3RJbmRleE9mKFwiLlwiKSArIDEpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGFwcGVuZE1vZGVsUHJlZml4KHZhbHVlLCBwcmVmaXgpIHtcbiAgICAgICAgaWYgKHZhbHVlLmluZGV4T2YoXCIqLlwiKSA9PT0gMCkge1xuICAgICAgICAgICAgdmFsdWUgPSB2YWx1ZS5yZXBsYWNlKFwiKi5cIiwgcHJlZml4KTtcbiAgICAgICAgfVxuICAgICAgICByZXR1cm4gdmFsdWU7XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gb25FcnJvcihlcnJvciwgaW5wdXRFbGVtZW50KSB7ICAvLyAndGhpcycgaXMgdGhlIGZvcm0gZWxlbWVudFxuICAgICAgICB2YXIgY29udGFpbmVyID0gJCh0aGlzKS5maW5kKFwiW2RhdGEtdmFsbXNnLWZvcj0nXCIgKyBlc2NhcGVBdHRyaWJ1dGVWYWx1ZShpbnB1dEVsZW1lbnRbMF0ubmFtZSkgKyBcIiddXCIpLFxuICAgICAgICAgICAgcmVwbGFjZUF0dHJWYWx1ZSA9IGNvbnRhaW5lci5hdHRyKFwiZGF0YS12YWxtc2ctcmVwbGFjZVwiKSxcbiAgICAgICAgICAgIHJlcGxhY2UgPSByZXBsYWNlQXR0clZhbHVlID8gJC5wYXJzZUpTT04ocmVwbGFjZUF0dHJWYWx1ZSkgIT09IGZhbHNlIDogbnVsbDtcblxuICAgICAgICBjb250YWluZXIucmVtb3ZlQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLXZhbGlkXCIpLmFkZENsYXNzKFwiZmllbGQtdmFsaWRhdGlvbi1lcnJvclwiKTtcbiAgICAgICAgZXJyb3IuZGF0YShcInVub2J0cnVzaXZlQ29udGFpbmVyXCIsIGNvbnRhaW5lcik7XG5cbiAgICAgICAgaWYgKHJlcGxhY2UpIHtcbiAgICAgICAgICAgIGNvbnRhaW5lci5lbXB0eSgpO1xuICAgICAgICAgICAgZXJyb3IucmVtb3ZlQ2xhc3MoXCJpbnB1dC12YWxpZGF0aW9uLWVycm9yXCIpLmFwcGVuZFRvKGNvbnRhaW5lcik7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSB7XG4gICAgICAgICAgICBlcnJvci5oaWRlKCk7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBmdW5jdGlvbiBvbkVycm9ycyhldmVudCwgdmFsaWRhdG9yKSB7ICAvLyAndGhpcycgaXMgdGhlIGZvcm0gZWxlbWVudFxuICAgICAgICB2YXIgY29udGFpbmVyID0gJCh0aGlzKS5maW5kKFwiW2RhdGEtdmFsbXNnLXN1bW1hcnk9dHJ1ZV1cIiksXG4gICAgICAgICAgICBsaXN0ID0gY29udGFpbmVyLmZpbmQoXCJ1bFwiKTtcblxuICAgICAgICBpZiAobGlzdCAmJiBsaXN0Lmxlbmd0aCAmJiB2YWxpZGF0b3IuZXJyb3JMaXN0Lmxlbmd0aCkge1xuICAgICAgICAgICAgbGlzdC5lbXB0eSgpO1xuICAgICAgICAgICAgY29udGFpbmVyLmFkZENsYXNzKFwidmFsaWRhdGlvbi1zdW1tYXJ5LWVycm9yc1wiKS5yZW1vdmVDbGFzcyhcInZhbGlkYXRpb24tc3VtbWFyeS12YWxpZFwiKTtcblxuICAgICAgICAgICAgJC5lYWNoKHZhbGlkYXRvci5lcnJvckxpc3QsIGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAkKFwiPGxpIC8+XCIpLmh0bWwodGhpcy5tZXNzYWdlKS5hcHBlbmRUbyhsaXN0KTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gb25TdWNjZXNzKGVycm9yKSB7ICAvLyAndGhpcycgaXMgdGhlIGZvcm0gZWxlbWVudFxuICAgICAgICB2YXIgY29udGFpbmVyID0gZXJyb3IuZGF0YShcInVub2J0cnVzaXZlQ29udGFpbmVyXCIpLFxuICAgICAgICAgICAgcmVwbGFjZUF0dHJWYWx1ZSA9IGNvbnRhaW5lci5hdHRyKFwiZGF0YS12YWxtc2ctcmVwbGFjZVwiKSxcbiAgICAgICAgICAgIHJlcGxhY2UgPSByZXBsYWNlQXR0clZhbHVlID8gJC5wYXJzZUpTT04ocmVwbGFjZUF0dHJWYWx1ZSkgOiBudWxsO1xuXG4gICAgICAgIGlmIChjb250YWluZXIpIHtcbiAgICAgICAgICAgIGNvbnRhaW5lci5hZGRDbGFzcyhcImZpZWxkLXZhbGlkYXRpb24tdmFsaWRcIikucmVtb3ZlQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLWVycm9yXCIpO1xuICAgICAgICAgICAgZXJyb3IucmVtb3ZlRGF0YShcInVub2J0cnVzaXZlQ29udGFpbmVyXCIpO1xuXG4gICAgICAgICAgICBpZiAocmVwbGFjZSkge1xuICAgICAgICAgICAgICAgIGNvbnRhaW5lci5lbXB0eSgpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gb25SZXNldChldmVudCkgeyAgLy8gJ3RoaXMnIGlzIHRoZSBmb3JtIGVsZW1lbnRcbiAgICAgICAgdmFyICRmb3JtID0gJCh0aGlzKTtcbiAgICAgICAgJGZvcm0uZGF0YShcInZhbGlkYXRvclwiKS5yZXNldEZvcm0oKTtcbiAgICAgICAgJGZvcm0uZmluZChcIi52YWxpZGF0aW9uLXN1bW1hcnktZXJyb3JzXCIpXG4gICAgICAgICAgICAuYWRkQ2xhc3MoXCJ2YWxpZGF0aW9uLXN1bW1hcnktdmFsaWRcIilcbiAgICAgICAgICAgIC5yZW1vdmVDbGFzcyhcInZhbGlkYXRpb24tc3VtbWFyeS1lcnJvcnNcIik7XG4gICAgICAgICRmb3JtLmZpbmQoXCIuZmllbGQtdmFsaWRhdGlvbi1lcnJvclwiKVxuICAgICAgICAgICAgLmFkZENsYXNzKFwiZmllbGQtdmFsaWRhdGlvbi12YWxpZFwiKVxuICAgICAgICAgICAgLnJlbW92ZUNsYXNzKFwiZmllbGQtdmFsaWRhdGlvbi1lcnJvclwiKVxuICAgICAgICAgICAgLnJlbW92ZURhdGEoXCJ1bm9idHJ1c2l2ZUNvbnRhaW5lclwiKVxuICAgICAgICAgICAgLmZpbmQoXCI+KlwiKSAgLy8gSWYgd2Ugd2VyZSB1c2luZyB2YWxtc2ctcmVwbGFjZSwgZ2V0IHRoZSB1bmRlcmx5aW5nIGVycm9yXG4gICAgICAgICAgICAgICAgLnJlbW92ZURhdGEoXCJ1bm9idHJ1c2l2ZUNvbnRhaW5lclwiKTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiB2YWxpZGF0aW9uSW5mbyhmb3JtKSB7XG4gICAgICAgIHZhciAkZm9ybSA9ICQoZm9ybSksXG4gICAgICAgICAgICByZXN1bHQgPSAkZm9ybS5kYXRhKGRhdGFfdmFsaWRhdGlvbiksXG4gICAgICAgICAgICBvblJlc2V0UHJveHkgPSAkLnByb3h5KG9uUmVzZXQsIGZvcm0pLFxuICAgICAgICAgICAgZGVmYXVsdE9wdGlvbnMgPSAkalF2YWwudW5vYnRydXNpdmUub3B0aW9ucyB8fCB7fSxcbiAgICAgICAgICAgIGV4ZWNJbkNvbnRleHQgPSBmdW5jdGlvbiAobmFtZSwgYXJncykge1xuICAgICAgICAgICAgICAgIHZhciBmdW5jID0gZGVmYXVsdE9wdGlvbnNbbmFtZV07XG4gICAgICAgICAgICAgICAgZnVuYyAmJiAkLmlzRnVuY3Rpb24oZnVuYykgJiYgZnVuYy5hcHBseShmb3JtLCBhcmdzKTtcbiAgICAgICAgICAgIH1cblxuICAgICAgICBpZiAoIXJlc3VsdCkge1xuICAgICAgICAgICAgcmVzdWx0ID0ge1xuICAgICAgICAgICAgICAgIG9wdGlvbnM6IHsgIC8vIG9wdGlvbnMgc3RydWN0dXJlIHBhc3NlZCB0byBqUXVlcnkgVmFsaWRhdGUncyB2YWxpZGF0ZSgpIG1ldGhvZFxuICAgICAgICAgICAgICAgICAgICBlcnJvckNsYXNzOiBkZWZhdWx0T3B0aW9ucy5lcnJvckNsYXNzIHx8IFwiaW5wdXQtdmFsaWRhdGlvbi1lcnJvclwiLFxuICAgICAgICAgICAgICAgICAgICBlcnJvckVsZW1lbnQ6IGRlZmF1bHRPcHRpb25zLmVycm9yRWxlbWVudCB8fCBcInNwYW5cIixcbiAgICAgICAgICAgICAgICAgICAgZXJyb3JQbGFjZW1lbnQ6IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIG9uRXJyb3IuYXBwbHkoZm9ybSwgYXJndW1lbnRzKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIGV4ZWNJbkNvbnRleHQoXCJlcnJvclBsYWNlbWVudFwiLCBhcmd1bWVudHMpO1xuICAgICAgICAgICAgICAgICAgICB9LFxuICAgICAgICAgICAgICAgICAgICBpbnZhbGlkSGFuZGxlcjogZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgb25FcnJvcnMuYXBwbHkoZm9ybSwgYXJndW1lbnRzKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIGV4ZWNJbkNvbnRleHQoXCJpbnZhbGlkSGFuZGxlclwiLCBhcmd1bWVudHMpO1xuICAgICAgICAgICAgICAgICAgICB9LFxuICAgICAgICAgICAgICAgICAgICBtZXNzYWdlczoge30sXG4gICAgICAgICAgICAgICAgICAgIHJ1bGVzOiB7fSxcbiAgICAgICAgICAgICAgICAgICAgc3VjY2VzczogZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgb25TdWNjZXNzLmFwcGx5KGZvcm0sIGFyZ3VtZW50cyk7XG4gICAgICAgICAgICAgICAgICAgICAgICBleGVjSW5Db250ZXh0KFwic3VjY2Vzc1wiLCBhcmd1bWVudHMpO1xuICAgICAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgfSxcbiAgICAgICAgICAgICAgICBhdHRhY2hWYWxpZGF0aW9uOiBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICRmb3JtXG4gICAgICAgICAgICAgICAgICAgICAgICAub2ZmKFwicmVzZXQuXCIgKyBkYXRhX3ZhbGlkYXRpb24sIG9uUmVzZXRQcm94eSlcbiAgICAgICAgICAgICAgICAgICAgICAgIC5vbihcInJlc2V0LlwiICsgZGF0YV92YWxpZGF0aW9uLCBvblJlc2V0UHJveHkpXG4gICAgICAgICAgICAgICAgICAgICAgICAudmFsaWRhdGUodGhpcy5vcHRpb25zKTtcbiAgICAgICAgICAgICAgICB9LFxuICAgICAgICAgICAgICAgIHZhbGlkYXRlOiBmdW5jdGlvbiAoKSB7ICAvLyBhIHZhbGlkYXRpb24gZnVuY3Rpb24gdGhhdCBpcyBjYWxsZWQgYnkgdW5vYnRydXNpdmUgQWpheFxuICAgICAgICAgICAgICAgICAgICAkZm9ybS52YWxpZGF0ZSgpO1xuICAgICAgICAgICAgICAgICAgICByZXR1cm4gJGZvcm0udmFsaWQoKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9O1xuICAgICAgICAgICAgJGZvcm0uZGF0YShkYXRhX3ZhbGlkYXRpb24sIHJlc3VsdCk7XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gcmVzdWx0O1xuICAgIH1cblxuICAgICRqUXZhbC51bm9idHJ1c2l2ZSA9IHtcbiAgICAgICAgYWRhcHRlcnM6IFtdLFxuXG4gICAgICAgIHBhcnNlRWxlbWVudDogZnVuY3Rpb24gKGVsZW1lbnQsIHNraXBBdHRhY2gpIHtcbiAgICAgICAgICAgIC8vLyA8c3VtbWFyeT5cbiAgICAgICAgICAgIC8vLyBQYXJzZXMgYSBzaW5nbGUgSFRNTCBlbGVtZW50IGZvciB1bm9idHJ1c2l2ZSB2YWxpZGF0aW9uIGF0dHJpYnV0ZXMuXG4gICAgICAgICAgICAvLy8gPC9zdW1tYXJ5PlxuICAgICAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiZWxlbWVudFwiIGRvbUVsZW1lbnQ9XCJ0cnVlXCI+VGhlIEhUTUwgZWxlbWVudCB0byBiZSBwYXJzZWQuPC9wYXJhbT5cbiAgICAgICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInNraXBBdHRhY2hcIiB0eXBlPVwiQm9vbGVhblwiPltPcHRpb25hbF0gdHJ1ZSB0byBza2lwIGF0dGFjaGluZyB0aGVcbiAgICAgICAgICAgIC8vLyB2YWxpZGF0aW9uIHRvIHRoZSBmb3JtLiBJZiBwYXJzaW5nIGp1c3QgdGhpcyBzaW5nbGUgZWxlbWVudCwgeW91IHNob3VsZCBzcGVjaWZ5IHRydWUuXG4gICAgICAgICAgICAvLy8gSWYgcGFyc2luZyBzZXZlcmFsIGVsZW1lbnRzLCB5b3Ugc2hvdWxkIHNwZWNpZnkgZmFsc2UsIGFuZCBtYW51YWxseSBhdHRhY2ggdGhlIHZhbGlkYXRpb25cbiAgICAgICAgICAgIC8vLyB0byB0aGUgZm9ybSB3aGVuIHlvdSBhcmUgZmluaXNoZWQuIFRoZSBkZWZhdWx0IGlzIGZhbHNlLjwvcGFyYW0+XG4gICAgICAgICAgICB2YXIgJGVsZW1lbnQgPSAkKGVsZW1lbnQpLFxuICAgICAgICAgICAgICAgIGZvcm0gPSAkZWxlbWVudC5wYXJlbnRzKFwiZm9ybVwiKVswXSxcbiAgICAgICAgICAgICAgICB2YWxJbmZvLCBydWxlcywgbWVzc2FnZXM7XG5cbiAgICAgICAgICAgIGlmICghZm9ybSkgeyAgLy8gQ2Fubm90IGRvIGNsaWVudC1zaWRlIHZhbGlkYXRpb24gd2l0aG91dCBhIGZvcm1cbiAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIHZhbEluZm8gPSB2YWxpZGF0aW9uSW5mbyhmb3JtKTtcbiAgICAgICAgICAgIHZhbEluZm8ub3B0aW9ucy5ydWxlc1tlbGVtZW50Lm5hbWVdID0gcnVsZXMgPSB7fTtcbiAgICAgICAgICAgIHZhbEluZm8ub3B0aW9ucy5tZXNzYWdlc1tlbGVtZW50Lm5hbWVdID0gbWVzc2FnZXMgPSB7fTtcblxuICAgICAgICAgICAgJC5lYWNoKHRoaXMuYWRhcHRlcnMsIGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICB2YXIgcHJlZml4ID0gXCJkYXRhLXZhbC1cIiArIHRoaXMubmFtZSxcbiAgICAgICAgICAgICAgICAgICAgbWVzc2FnZSA9ICRlbGVtZW50LmF0dHIocHJlZml4KSxcbiAgICAgICAgICAgICAgICAgICAgcGFyYW1WYWx1ZXMgPSB7fTtcblxuICAgICAgICAgICAgICAgIGlmIChtZXNzYWdlICE9PSB1bmRlZmluZWQpIHsgIC8vIENvbXBhcmUgYWdhaW5zdCB1bmRlZmluZWQsIGJlY2F1c2UgYW4gZW1wdHkgbWVzc2FnZSBpcyBsZWdhbCAoYW5kIGZhbHN5KVxuICAgICAgICAgICAgICAgICAgICBwcmVmaXggKz0gXCItXCI7XG5cbiAgICAgICAgICAgICAgICAgICAgJC5lYWNoKHRoaXMucGFyYW1zLCBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBwYXJhbVZhbHVlc1t0aGlzXSA9ICRlbGVtZW50LmF0dHIocHJlZml4ICsgdGhpcyk7XG4gICAgICAgICAgICAgICAgICAgIH0pO1xuXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYWRhcHQoe1xuICAgICAgICAgICAgICAgICAgICAgICAgZWxlbWVudDogZWxlbWVudCxcbiAgICAgICAgICAgICAgICAgICAgICAgIGZvcm06IGZvcm0sXG4gICAgICAgICAgICAgICAgICAgICAgICBtZXNzYWdlOiBtZXNzYWdlLFxuICAgICAgICAgICAgICAgICAgICAgICAgcGFyYW1zOiBwYXJhbVZhbHVlcyxcbiAgICAgICAgICAgICAgICAgICAgICAgIHJ1bGVzOiBydWxlcyxcbiAgICAgICAgICAgICAgICAgICAgICAgIG1lc3NhZ2VzOiBtZXNzYWdlc1xuICAgICAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgJC5leHRlbmQocnVsZXMsIHsgXCJfX2R1bW15X19cIjogdHJ1ZSB9KTtcblxuICAgICAgICAgICAgaWYgKCFza2lwQXR0YWNoKSB7XG4gICAgICAgICAgICAgICAgdmFsSW5mby5hdHRhY2hWYWxpZGF0aW9uKCk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH0sXG5cbiAgICAgICAgcGFyc2U6IGZ1bmN0aW9uIChzZWxlY3Rvcikge1xuICAgICAgICAgICAgLy8vIDxzdW1tYXJ5PlxuICAgICAgICAgICAgLy8vIFBhcnNlcyBhbGwgdGhlIEhUTUwgZWxlbWVudHMgaW4gdGhlIHNwZWNpZmllZCBzZWxlY3Rvci4gSXQgbG9va3MgZm9yIGlucHV0IGVsZW1lbnRzIGRlY29yYXRlZFxuICAgICAgICAgICAgLy8vIHdpdGggdGhlIFtkYXRhLXZhbD10cnVlXSBhdHRyaWJ1dGUgdmFsdWUgYW5kIGVuYWJsZXMgdmFsaWRhdGlvbiBhY2NvcmRpbmcgdG8gdGhlIGRhdGEtdmFsLSpcbiAgICAgICAgICAgIC8vLyBhdHRyaWJ1dGUgdmFsdWVzLlxuICAgICAgICAgICAgLy8vIDwvc3VtbWFyeT5cbiAgICAgICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInNlbGVjdG9yXCIgdHlwZT1cIlN0cmluZ1wiPkFueSB2YWxpZCBqUXVlcnkgc2VsZWN0b3IuPC9wYXJhbT5cblxuICAgICAgICAgICAgLy8gJGZvcm1zIGluY2x1ZGVzIGFsbCBmb3JtcyBpbiBzZWxlY3RvcidzIERPTSBoaWVyYXJjaHkgKHBhcmVudCwgY2hpbGRyZW4gYW5kIHNlbGYpIHRoYXQgaGF2ZSBhdCBsZWFzdCBvbmVcbiAgICAgICAgICAgIC8vIGVsZW1lbnQgd2l0aCBkYXRhLXZhbD10cnVlXG4gICAgICAgICAgICB2YXIgJHNlbGVjdG9yID0gJChzZWxlY3RvciksXG4gICAgICAgICAgICAgICAgJGZvcm1zID0gJHNlbGVjdG9yLnBhcmVudHMoKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hZGRCYWNrKClcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuZmlsdGVyKFwiZm9ybVwiKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hZGQoJHNlbGVjdG9yLmZpbmQoXCJmb3JtXCIpKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5oYXMoXCJbZGF0YS12YWw9dHJ1ZV1cIik7XG5cbiAgICAgICAgICAgICRzZWxlY3Rvci5maW5kKFwiW2RhdGEtdmFsPXRydWVdXCIpLmVhY2goZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICRqUXZhbC51bm9idHJ1c2l2ZS5wYXJzZUVsZW1lbnQodGhpcywgdHJ1ZSk7XG4gICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgJGZvcm1zLmVhY2goZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIHZhciBpbmZvID0gdmFsaWRhdGlvbkluZm8odGhpcyk7XG4gICAgICAgICAgICAgICAgaWYgKGluZm8pIHtcbiAgICAgICAgICAgICAgICAgICAgaW5mby5hdHRhY2hWYWxpZGF0aW9uKCk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfSk7XG4gICAgICAgIH1cbiAgICB9O1xuXG4gICAgYWRhcHRlcnMgPSAkalF2YWwudW5vYnRydXNpdmUuYWRhcHRlcnM7XG5cbiAgICBhZGFwdGVycy5hZGQgPSBmdW5jdGlvbiAoYWRhcHRlck5hbWUsIHBhcmFtcywgZm4pIHtcbiAgICAgICAgLy8vIDxzdW1tYXJ5PkFkZHMgYSBuZXcgYWRhcHRlciB0byBjb252ZXJ0IHVub2J0cnVzaXZlIEhUTUwgaW50byBhIGpRdWVyeSBWYWxpZGF0ZSB2YWxpZGF0aW9uLjwvc3VtbWFyeT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiYWRhcHRlck5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGFkYXB0ZXIgdG8gYmUgYWRkZWQuIFRoaXMgbWF0Y2hlcyB0aGUgbmFtZSB1c2VkXG4gICAgICAgIC8vLyBpbiB0aGUgZGF0YS12YWwtbm5ubiBIVE1MIGF0dHJpYnV0ZSAod2hlcmUgbm5ubiBpcyB0aGUgYWRhcHRlciBuYW1lKS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJwYXJhbXNcIiB0eXBlPVwiQXJyYXlcIiBvcHRpb25hbD1cInRydWVcIj5bT3B0aW9uYWxdIEFuIGFycmF5IG9mIHBhcmFtZXRlciBuYW1lcyAoc3RyaW5ncykgdGhhdCB3aWxsXG4gICAgICAgIC8vLyBiZSBleHRyYWN0ZWQgZnJvbSB0aGUgZGF0YS12YWwtbm5ubi1tbW1tIEhUTUwgYXR0cmlidXRlcyAod2hlcmUgbm5ubiBpcyB0aGUgYWRhcHRlciBuYW1lLCBhbmRcbiAgICAgICAgLy8vIG1tbW0gaXMgdGhlIHBhcmFtZXRlciBuYW1lKS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJmblwiIHR5cGU9XCJGdW5jdGlvblwiPlRoZSBmdW5jdGlvbiB0byBjYWxsLCB3aGljaCBhZGFwdHMgdGhlIHZhbHVlcyBmcm9tIHRoZSBIVE1MXG4gICAgICAgIC8vLyBhdHRyaWJ1dGVzIGludG8galF1ZXJ5IFZhbGlkYXRlIHJ1bGVzIGFuZC9vciBtZXNzYWdlcy48L3BhcmFtPlxuICAgICAgICAvLy8gPHJldHVybnMgdHlwZT1cImpRdWVyeS52YWxpZGF0b3IudW5vYnRydXNpdmUuYWRhcHRlcnNcIiAvPlxuICAgICAgICBpZiAoIWZuKSB7ICAvLyBDYWxsZWQgd2l0aCBubyBwYXJhbXMsIGp1c3QgYSBmdW5jdGlvblxuICAgICAgICAgICAgZm4gPSBwYXJhbXM7XG4gICAgICAgICAgICBwYXJhbXMgPSBbXTtcbiAgICAgICAgfVxuICAgICAgICB0aGlzLnB1c2goeyBuYW1lOiBhZGFwdGVyTmFtZSwgcGFyYW1zOiBwYXJhbXMsIGFkYXB0OiBmbiB9KTtcbiAgICAgICAgcmV0dXJuIHRoaXM7XG4gICAgfTtcblxuICAgIGFkYXB0ZXJzLmFkZEJvb2wgPSBmdW5jdGlvbiAoYWRhcHRlck5hbWUsIHJ1bGVOYW1lKSB7XG4gICAgICAgIC8vLyA8c3VtbWFyeT5BZGRzIGEgbmV3IGFkYXB0ZXIgdG8gY29udmVydCB1bm9idHJ1c2l2ZSBIVE1MIGludG8gYSBqUXVlcnkgVmFsaWRhdGUgdmFsaWRhdGlvbiwgd2hlcmVcbiAgICAgICAgLy8vIHRoZSBqUXVlcnkgVmFsaWRhdGUgdmFsaWRhdGlvbiBydWxlIGhhcyBubyBwYXJhbWV0ZXIgdmFsdWVzLjwvc3VtbWFyeT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiYWRhcHRlck5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGFkYXB0ZXIgdG8gYmUgYWRkZWQuIFRoaXMgbWF0Y2hlcyB0aGUgbmFtZSB1c2VkXG4gICAgICAgIC8vLyBpbiB0aGUgZGF0YS12YWwtbm5ubiBIVE1MIGF0dHJpYnV0ZSAod2hlcmUgbm5ubiBpcyB0aGUgYWRhcHRlciBuYW1lKS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJydWxlTmFtZVwiIHR5cGU9XCJTdHJpbmdcIiBvcHRpb25hbD1cInRydWVcIj5bT3B0aW9uYWxdIFRoZSBuYW1lIG9mIHRoZSBqUXVlcnkgVmFsaWRhdGUgcnVsZS4gSWYgbm90IHByb3ZpZGVkLCB0aGUgdmFsdWVcbiAgICAgICAgLy8vIG9mIGFkYXB0ZXJOYW1lIHdpbGwgYmUgdXNlZCBpbnN0ZWFkLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cmV0dXJucyB0eXBlPVwialF1ZXJ5LnZhbGlkYXRvci51bm9idHJ1c2l2ZS5hZGFwdGVyc1wiIC8+XG4gICAgICAgIHJldHVybiB0aGlzLmFkZChhZGFwdGVyTmFtZSwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgcnVsZU5hbWUgfHwgYWRhcHRlck5hbWUsIHRydWUpO1xuICAgICAgICB9KTtcbiAgICB9O1xuXG4gICAgYWRhcHRlcnMuYWRkTWluTWF4ID0gZnVuY3Rpb24gKGFkYXB0ZXJOYW1lLCBtaW5SdWxlTmFtZSwgbWF4UnVsZU5hbWUsIG1pbk1heFJ1bGVOYW1lLCBtaW5BdHRyaWJ1dGUsIG1heEF0dHJpYnV0ZSkge1xuICAgICAgICAvLy8gPHN1bW1hcnk+QWRkcyBhIG5ldyBhZGFwdGVyIHRvIGNvbnZlcnQgdW5vYnRydXNpdmUgSFRNTCBpbnRvIGEgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24sIHdoZXJlXG4gICAgICAgIC8vLyB0aGUgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24gaGFzIHRocmVlIHBvdGVudGlhbCBydWxlcyAob25lIGZvciBtaW4tb25seSwgb25lIGZvciBtYXgtb25seSwgYW5kXG4gICAgICAgIC8vLyBvbmUgZm9yIG1pbi1hbmQtbWF4KS4gVGhlIEhUTUwgcGFyYW1ldGVycyBhcmUgZXhwZWN0ZWQgdG8gYmUgbmFtZWQgLW1pbiBhbmQgLW1heC48L3N1bW1hcnk+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImFkYXB0ZXJOYW1lXCIgdHlwZT1cIlN0cmluZ1wiPlRoZSBuYW1lIG9mIHRoZSBhZGFwdGVyIHRvIGJlIGFkZGVkLiBUaGlzIG1hdGNoZXMgdGhlIG5hbWUgdXNlZFxuICAgICAgICAvLy8gaW4gdGhlIGRhdGEtdmFsLW5ubm4gSFRNTCBhdHRyaWJ1dGUgKHdoZXJlIG5ubm4gaXMgdGhlIGFkYXB0ZXIgbmFtZSkuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwibWluUnVsZU5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGpRdWVyeSBWYWxpZGF0ZSBydWxlIHRvIGJlIHVzZWQgd2hlbiB5b3Ugb25seVxuICAgICAgICAvLy8gaGF2ZSBhIG1pbmltdW0gdmFsdWUuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwibWF4UnVsZU5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGpRdWVyeSBWYWxpZGF0ZSBydWxlIHRvIGJlIHVzZWQgd2hlbiB5b3Ugb25seVxuICAgICAgICAvLy8gaGF2ZSBhIG1heGltdW0gdmFsdWUuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwibWluTWF4UnVsZU5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGpRdWVyeSBWYWxpZGF0ZSBydWxlIHRvIGJlIHVzZWQgd2hlbiB5b3VcbiAgICAgICAgLy8vIGhhdmUgYm90aCBhIG1pbmltdW0gYW5kIG1heGltdW0gdmFsdWUuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwibWluQXR0cmlidXRlXCIgdHlwZT1cIlN0cmluZ1wiIG9wdGlvbmFsPVwidHJ1ZVwiPltPcHRpb25hbF0gVGhlIG5hbWUgb2YgdGhlIEhUTUwgYXR0cmlidXRlIHRoYXRcbiAgICAgICAgLy8vIGNvbnRhaW5zIHRoZSBtaW5pbXVtIHZhbHVlLiBUaGUgZGVmYXVsdCBpcyBcIm1pblwiLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cIm1heEF0dHJpYnV0ZVwiIHR5cGU9XCJTdHJpbmdcIiBvcHRpb25hbD1cInRydWVcIj5bT3B0aW9uYWxdIFRoZSBuYW1lIG9mIHRoZSBIVE1MIGF0dHJpYnV0ZSB0aGF0XG4gICAgICAgIC8vLyBjb250YWlucyB0aGUgbWF4aW11bSB2YWx1ZS4gVGhlIGRlZmF1bHQgaXMgXCJtYXhcIi48L3BhcmFtPlxuICAgICAgICAvLy8gPHJldHVybnMgdHlwZT1cImpRdWVyeS52YWxpZGF0b3IudW5vYnRydXNpdmUuYWRhcHRlcnNcIiAvPlxuICAgICAgICByZXR1cm4gdGhpcy5hZGQoYWRhcHRlck5hbWUsIFttaW5BdHRyaWJ1dGUgfHwgXCJtaW5cIiwgbWF4QXR0cmlidXRlIHx8IFwibWF4XCJdLCBmdW5jdGlvbiAob3B0aW9ucykge1xuICAgICAgICAgICAgdmFyIG1pbiA9IG9wdGlvbnMucGFyYW1zLm1pbixcbiAgICAgICAgICAgICAgICBtYXggPSBvcHRpb25zLnBhcmFtcy5tYXg7XG5cbiAgICAgICAgICAgIGlmIChtaW4gJiYgbWF4KSB7XG4gICAgICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBtaW5NYXhSdWxlTmFtZSwgW21pbiwgbWF4XSk7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBlbHNlIGlmIChtaW4pIHtcbiAgICAgICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIG1pblJ1bGVOYW1lLCBtaW4pO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgZWxzZSBpZiAobWF4KSB7XG4gICAgICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBtYXhSdWxlTmFtZSwgbWF4KTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSk7XG4gICAgfTtcblxuICAgIGFkYXB0ZXJzLmFkZFNpbmdsZVZhbCA9IGZ1bmN0aW9uIChhZGFwdGVyTmFtZSwgYXR0cmlidXRlLCBydWxlTmFtZSkge1xuICAgICAgICAvLy8gPHN1bW1hcnk+QWRkcyBhIG5ldyBhZGFwdGVyIHRvIGNvbnZlcnQgdW5vYnRydXNpdmUgSFRNTCBpbnRvIGEgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24sIHdoZXJlXG4gICAgICAgIC8vLyB0aGUgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24gcnVsZSBoYXMgYSBzaW5nbGUgdmFsdWUuPC9zdW1tYXJ5PlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJhZGFwdGVyTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgYWRhcHRlciB0byBiZSBhZGRlZC4gVGhpcyBtYXRjaGVzIHRoZSBuYW1lIHVzZWRcbiAgICAgICAgLy8vIGluIHRoZSBkYXRhLXZhbC1ubm5uIEhUTUwgYXR0cmlidXRlKHdoZXJlIG5ubm4gaXMgdGhlIGFkYXB0ZXIgbmFtZSkuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiYXR0cmlidXRlXCIgdHlwZT1cIlN0cmluZ1wiPltPcHRpb25hbF0gVGhlIG5hbWUgb2YgdGhlIEhUTUwgYXR0cmlidXRlIHRoYXQgY29udGFpbnMgdGhlIHZhbHVlLlxuICAgICAgICAvLy8gVGhlIGRlZmF1bHQgaXMgXCJ2YWxcIi48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJydWxlTmFtZVwiIHR5cGU9XCJTdHJpbmdcIiBvcHRpb25hbD1cInRydWVcIj5bT3B0aW9uYWxdIFRoZSBuYW1lIG9mIHRoZSBqUXVlcnkgVmFsaWRhdGUgcnVsZS4gSWYgbm90IHByb3ZpZGVkLCB0aGUgdmFsdWVcbiAgICAgICAgLy8vIG9mIGFkYXB0ZXJOYW1lIHdpbGwgYmUgdXNlZCBpbnN0ZWFkLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cmV0dXJucyB0eXBlPVwialF1ZXJ5LnZhbGlkYXRvci51bm9idHJ1c2l2ZS5hZGFwdGVyc1wiIC8+XG4gICAgICAgIHJldHVybiB0aGlzLmFkZChhZGFwdGVyTmFtZSwgW2F0dHJpYnV0ZSB8fCBcInZhbFwiXSwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgcnVsZU5hbWUgfHwgYWRhcHRlck5hbWUsIG9wdGlvbnMucGFyYW1zW2F0dHJpYnV0ZV0pO1xuICAgICAgICB9KTtcbiAgICB9O1xuXG4gICAgJGpRdmFsLmFkZE1ldGhvZChcIl9fZHVtbXlfX1wiLCBmdW5jdGlvbiAodmFsdWUsIGVsZW1lbnQsIHBhcmFtcykge1xuICAgICAgICByZXR1cm4gdHJ1ZTtcbiAgICB9KTtcblxuICAgICRqUXZhbC5hZGRNZXRob2QoXCJyZWdleFwiLCBmdW5jdGlvbiAodmFsdWUsIGVsZW1lbnQsIHBhcmFtcykge1xuICAgICAgICB2YXIgbWF0Y2g7XG4gICAgICAgIGlmICh0aGlzLm9wdGlvbmFsKGVsZW1lbnQpKSB7XG4gICAgICAgICAgICByZXR1cm4gdHJ1ZTtcbiAgICAgICAgfVxuXG4gICAgICAgIG1hdGNoID0gbmV3IFJlZ0V4cChwYXJhbXMpLmV4ZWModmFsdWUpO1xuICAgICAgICByZXR1cm4gKG1hdGNoICYmIChtYXRjaC5pbmRleCA9PT0gMCkgJiYgKG1hdGNoWzBdLmxlbmd0aCA9PT0gdmFsdWUubGVuZ3RoKSk7XG4gICAgfSk7XG5cbiAgICAkalF2YWwuYWRkTWV0aG9kKFwibm9uYWxwaGFtaW5cIiwgZnVuY3Rpb24gKHZhbHVlLCBlbGVtZW50LCBub25hbHBoYW1pbikge1xuICAgICAgICB2YXIgbWF0Y2g7XG4gICAgICAgIGlmIChub25hbHBoYW1pbikge1xuICAgICAgICAgICAgbWF0Y2ggPSB2YWx1ZS5tYXRjaCgvXFxXL2cpO1xuICAgICAgICAgICAgbWF0Y2ggPSBtYXRjaCAmJiBtYXRjaC5sZW5ndGggPj0gbm9uYWxwaGFtaW47XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIG1hdGNoO1xuICAgIH0pO1xuXG4gICAgaWYgKCRqUXZhbC5tZXRob2RzLmV4dGVuc2lvbikge1xuICAgICAgICBhZGFwdGVycy5hZGRTaW5nbGVWYWwoXCJhY2NlcHRcIiwgXCJtaW10eXBlXCIpO1xuICAgICAgICBhZGFwdGVycy5hZGRTaW5nbGVWYWwoXCJleHRlbnNpb25cIiwgXCJleHRlbnNpb25cIik7XG4gICAgfSBlbHNlIHtcbiAgICAgICAgLy8gZm9yIGJhY2t3YXJkIGNvbXBhdGliaWxpdHksIHdoZW4gdGhlICdleHRlbnNpb24nIHZhbGlkYXRpb24gbWV0aG9kIGRvZXMgbm90IGV4aXN0LCBzdWNoIGFzIHdpdGggdmVyc2lvbnNcbiAgICAgICAgLy8gb2YgSlF1ZXJ5IFZhbGlkYXRpb24gcGx1Z2luIHByaW9yIHRvIDEuMTAsIHdlIHNob3VsZCB1c2UgdGhlICdhY2NlcHQnIG1ldGhvZCBmb3JcbiAgICAgICAgLy8gdmFsaWRhdGluZyB0aGUgZXh0ZW5zaW9uLCBhbmQgaWdub3JlIG1pbWUtdHlwZSB2YWxpZGF0aW9ucyBhcyB0aGV5IGFyZSBub3Qgc3VwcG9ydGVkLlxuICAgICAgICBhZGFwdGVycy5hZGRTaW5nbGVWYWwoXCJleHRlbnNpb25cIiwgXCJleHRlbnNpb25cIiwgXCJhY2NlcHRcIik7XG4gICAgfVxuXG4gICAgYWRhcHRlcnMuYWRkU2luZ2xlVmFsKFwicmVnZXhcIiwgXCJwYXR0ZXJuXCIpO1xuICAgIGFkYXB0ZXJzLmFkZEJvb2woXCJjcmVkaXRjYXJkXCIpLmFkZEJvb2woXCJkYXRlXCIpLmFkZEJvb2woXCJkaWdpdHNcIikuYWRkQm9vbChcImVtYWlsXCIpLmFkZEJvb2woXCJudW1iZXJcIikuYWRkQm9vbChcInVybFwiKTtcbiAgICBhZGFwdGVycy5hZGRNaW5NYXgoXCJsZW5ndGhcIiwgXCJtaW5sZW5ndGhcIiwgXCJtYXhsZW5ndGhcIiwgXCJyYW5nZWxlbmd0aFwiKS5hZGRNaW5NYXgoXCJyYW5nZVwiLCBcIm1pblwiLCBcIm1heFwiLCBcInJhbmdlXCIpO1xuICAgIGFkYXB0ZXJzLmFkZE1pbk1heChcIm1pbmxlbmd0aFwiLCBcIm1pbmxlbmd0aFwiKS5hZGRNaW5NYXgoXCJtYXhsZW5ndGhcIiwgXCJtaW5sZW5ndGhcIiwgXCJtYXhsZW5ndGhcIik7XG4gICAgYWRhcHRlcnMuYWRkKFwiZXF1YWx0b1wiLCBbXCJvdGhlclwiXSwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgICAgICAgdmFyIHByZWZpeCA9IGdldE1vZGVsUHJlZml4KG9wdGlvbnMuZWxlbWVudC5uYW1lKSxcbiAgICAgICAgICAgIG90aGVyID0gb3B0aW9ucy5wYXJhbXMub3RoZXIsXG4gICAgICAgICAgICBmdWxsT3RoZXJOYW1lID0gYXBwZW5kTW9kZWxQcmVmaXgob3RoZXIsIHByZWZpeCksXG4gICAgICAgICAgICBlbGVtZW50ID0gJChvcHRpb25zLmZvcm0pLmZpbmQoXCI6aW5wdXRcIikuZmlsdGVyKFwiW25hbWU9J1wiICsgZXNjYXBlQXR0cmlidXRlVmFsdWUoZnVsbE90aGVyTmFtZSkgKyBcIiddXCIpWzBdO1xuXG4gICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJlcXVhbFRvXCIsIGVsZW1lbnQpO1xuICAgIH0pO1xuICAgIGFkYXB0ZXJzLmFkZChcInJlcXVpcmVkXCIsIGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gICAgICAgIC8vIGpRdWVyeSBWYWxpZGF0ZSBlcXVhdGVzIFwicmVxdWlyZWRcIiB3aXRoIFwibWFuZGF0b3J5XCIgZm9yIGNoZWNrYm94IGVsZW1lbnRzXG4gICAgICAgIGlmIChvcHRpb25zLmVsZW1lbnQudGFnTmFtZS50b1VwcGVyQ2FzZSgpICE9PSBcIklOUFVUXCIgfHwgb3B0aW9ucy5lbGVtZW50LnR5cGUudG9VcHBlckNhc2UoKSAhPT0gXCJDSEVDS0JPWFwiKSB7XG4gICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIFwicmVxdWlyZWRcIiwgdHJ1ZSk7XG4gICAgICAgIH1cbiAgICB9KTtcbiAgICBhZGFwdGVycy5hZGQoXCJyZW1vdGVcIiwgW1widXJsXCIsIFwidHlwZVwiLCBcImFkZGl0aW9uYWxmaWVsZHNcIl0sIGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gICAgICAgIHZhciB2YWx1ZSA9IHtcbiAgICAgICAgICAgIHVybDogb3B0aW9ucy5wYXJhbXMudXJsLFxuICAgICAgICAgICAgdHlwZTogb3B0aW9ucy5wYXJhbXMudHlwZSB8fCBcIkdFVFwiLFxuICAgICAgICAgICAgZGF0YToge31cbiAgICAgICAgfSxcbiAgICAgICAgICAgIHByZWZpeCA9IGdldE1vZGVsUHJlZml4KG9wdGlvbnMuZWxlbWVudC5uYW1lKTtcblxuICAgICAgICAkLmVhY2goc3BsaXRBbmRUcmltKG9wdGlvbnMucGFyYW1zLmFkZGl0aW9uYWxmaWVsZHMgfHwgb3B0aW9ucy5lbGVtZW50Lm5hbWUpLCBmdW5jdGlvbiAoaSwgZmllbGROYW1lKSB7XG4gICAgICAgICAgICB2YXIgcGFyYW1OYW1lID0gYXBwZW5kTW9kZWxQcmVmaXgoZmllbGROYW1lLCBwcmVmaXgpO1xuICAgICAgICAgICAgdmFsdWUuZGF0YVtwYXJhbU5hbWVdID0gZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIHJldHVybiAkKG9wdGlvbnMuZm9ybSkuZmluZChcIjppbnB1dFwiKS5maWx0ZXIoXCJbbmFtZT0nXCIgKyBlc2NhcGVBdHRyaWJ1dGVWYWx1ZShwYXJhbU5hbWUpICsgXCInXVwiKS52YWwoKTtcbiAgICAgICAgICAgIH07XG4gICAgICAgIH0pO1xuXG4gICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJyZW1vdGVcIiwgdmFsdWUpO1xuICAgIH0pO1xuICAgIGFkYXB0ZXJzLmFkZChcInBhc3N3b3JkXCIsIFtcIm1pblwiLCBcIm5vbmFscGhhbWluXCIsIFwicmVnZXhcIl0sIGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gICAgICAgIGlmIChvcHRpb25zLnBhcmFtcy5taW4pIHtcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJtaW5sZW5ndGhcIiwgb3B0aW9ucy5wYXJhbXMubWluKTtcbiAgICAgICAgfVxuICAgICAgICBpZiAob3B0aW9ucy5wYXJhbXMubm9uYWxwaGFtaW4pIHtcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJub25hbHBoYW1pblwiLCBvcHRpb25zLnBhcmFtcy5ub25hbHBoYW1pbik7XG4gICAgICAgIH1cbiAgICAgICAgaWYgKG9wdGlvbnMucGFyYW1zLnJlZ2V4KSB7XG4gICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIFwicmVnZXhcIiwgb3B0aW9ucy5wYXJhbXMucmVnZXgpO1xuICAgICAgICB9XG4gICAgfSk7XG5cbiAgICAkKGZ1bmN0aW9uICgpIHtcbiAgICAgICAgJGpRdmFsLnVub2J0cnVzaXZlLnBhcnNlKGRvY3VtZW50KTtcbiAgICB9KTtcbn0oalF1ZXJ5KSk7Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9