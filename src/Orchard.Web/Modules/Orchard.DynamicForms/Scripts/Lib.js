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
/**
 * CLDR JavaScript Library v0.4.1
 * http://jquery.com/
 *
 * Copyright 2013 Rafael Xavier de Souza
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2015-02-25T13:51Z
 */
/*!
 * CLDR JavaScript Library v0.4.1 2015-02-25T13:51Z MIT license © Rafael Xavier
 * http://git.io/h4lmVg
 */
(function( root, factory ) {

	if ( typeof define === "function" && define.amd ) {
		// AMD.
		define( factory );
	} else if ( typeof module === "object" && typeof module.exports === "object" ) {
		// Node. CommonJS.
		module.exports = factory();
	} else {
		// Global
		root.Cldr = factory();
	}

}( this, function() {


	var arrayIsArray = Array.isArray || function( obj ) {
		return Object.prototype.toString.call( obj ) === "[object Array]";
	};




	var pathNormalize = function( path, attributes ) {
		if ( arrayIsArray( path ) ) {
			path = path.join( "/" );
		}
		if ( typeof path !== "string" ) {
			throw new Error( "invalid path \"" + path + "\"" );
		}
		// 1: Ignore leading slash `/`
		// 2: Ignore leading `cldr/`
		path = path
			.replace( /^\// , "" ) /* 1 */
			.replace( /^cldr\// , "" ); /* 2 */

		// Replace {attribute}'s
		path = path.replace( /{[a-zA-Z]+}/g, function( name ) {
			name = name.replace( /^{([^}]*)}$/, "$1" );
			return attributes[ name ];
		});

		return path.split( "/" );
	};




	var arraySome = function( array, callback ) {
		var i, length;
		if ( array.some ) {
			return array.some( callback );
		}
		for ( i = 0, length = array.length; i < length; i++ ) {
			if ( callback( array[ i ], i, array ) ) {
				return true;
			}
		}
		return false;
	};




	/**
	 * Return the maximized language id as defined in
	 * http://www.unicode.org/reports/tr35/#Likely_Subtags
	 * 1. Canonicalize.
	 * 1.1 Make sure the input locale is in canonical form: uses the right
	 * separator, and has the right casing.
	 * TODO Right casing? What df? It seems languages are lowercase, scripts are
	 * Capitalized, territory is uppercase. I am leaving this as an exercise to
	 * the user.
	 *
	 * 1.2 Replace any deprecated subtags with their canonical values using the
	 * <alias> data in supplemental metadata. Use the first value in the
	 * replacement list, if it exists. Language tag replacements may have multiple
	 * parts, such as "sh" ➞ "sr_Latn" or mo" ➞ "ro_MD". In such a case, the
	 * original script and/or region are retained if there is one. Thus
	 * "sh_Arab_AQ" ➞ "sr_Arab_AQ", not "sr_Latn_AQ".
	 * TODO What <alias> data?
	 *
	 * 1.3 If the tag is grandfathered (see <variable id="$grandfathered"
	 * type="choice"> in the supplemental data), then return it.
	 * TODO grandfathered?
	 *
	 * 1.4 Remove the script code 'Zzzz' and the region code 'ZZ' if they occur.
	 * 1.5 Get the components of the cleaned-up source tag (languages, scripts,
	 * and regions), plus any variants and extensions.
	 * 2. Lookup. Lookup each of the following in order, and stop on the first
	 * match:
	 * 2.1 languages_scripts_regions
	 * 2.2 languages_regions
	 * 2.3 languages_scripts
	 * 2.4 languages
	 * 2.5 und_scripts
	 * 3. Return
	 * 3.1 If there is no match, either return an error value, or the match for
	 * "und" (in APIs where a valid language tag is required).
	 * 3.2 Otherwise there is a match = languagem_scriptm_regionm
	 * 3.3 Let xr = xs if xs is not empty, and xm otherwise.
	 * 3.4 Return the language tag composed of languager _ scriptr _ regionr +
	 * variants + extensions.
	 *
	 * @subtags [Array] normalized language id subtags tuple (see init.js).
	 */
	var coreLikelySubtags = function( Cldr, cldr, subtags, options ) {
		var match, matchFound,
			language = subtags[ 0 ],
			script = subtags[ 1 ],
			sep = Cldr.localeSep,
			territory = subtags[ 2 ];
		options = options || {};

		// Skip if (language, script, territory) is not empty [3.3]
		if ( language !== "und" && script !== "Zzzz" && territory !== "ZZ" ) {
			return [ language, script, territory ];
		}

		// Skip if no supplemental likelySubtags data is present
		if ( typeof cldr.get( "supplemental/likelySubtags" ) === "undefined" ) {
			return;
		}

		// [2]
		matchFound = arraySome([
			[ language, script, territory ],
			[ language, territory ],
			[ language, script ],
			[ language ],
			[ "und", script ]
		], function( test ) {
			return match = !(/\b(Zzzz|ZZ)\b/).test( test.join( sep ) ) /* [1.4] */ && cldr.get( [ "supplemental/likelySubtags", test.join( sep ) ] );
		});

		// [3]
		if ( matchFound ) {
			// [3.2 .. 3.4]
			match = match.split( sep );
			return [
				language !== "und" ? language : match[ 0 ],
				script !== "Zzzz" ? script : match[ 1 ],
				territory !== "ZZ" ? territory : match[ 2 ]
			];
		} else if ( options.force ) {
			// [3.1.2]
			return cldr.get( "supplemental/likelySubtags/und" ).split( sep );
		} else {
			// [3.1.1]
			return;
		}
	};



	/**
	 * Given a locale, remove any fields that Add Likely Subtags would add.
	 * http://www.unicode.org/reports/tr35/#Likely_Subtags
	 * 1. First get max = AddLikelySubtags(inputLocale). If an error is signaled,
	 * return it.
	 * 2. Remove the variants from max.
	 * 3. Then for trial in {language, language _ region, language _ script}. If
	 * AddLikelySubtags(trial) = max, then return trial + variants.
	 * 4. If you do not get a match, return max + variants.
	 * 
	 * @maxLanguageId [Array] maxLanguageId tuple (see init.js).
	 */
	var coreRemoveLikelySubtags = function( Cldr, cldr, maxLanguageId ) {
		var match, matchFound,
			language = maxLanguageId[ 0 ],
			script = maxLanguageId[ 1 ],
			territory = maxLanguageId[ 2 ];

		// [3]
		matchFound = arraySome([
			[ [ language, "Zzzz", "ZZ" ], [ language ] ],
			[ [ language, "Zzzz", territory ], [ language, territory ] ],
			[ [ language, script, "ZZ" ], [ language, script ] ]
		], function( test ) {
			var result = coreLikelySubtags( Cldr, cldr, test[ 0 ] );
			match = test[ 1 ];
			return result && result[ 0 ] === maxLanguageId[ 0 ] &&
				result[ 1 ] === maxLanguageId[ 1 ] &&
				result[ 2 ] === maxLanguageId[ 2 ];
		});

		// [4]
		return matchFound ?  match : maxLanguageId;
	};




	/**
	 * subtags( locale )
	 *
	 * @locale [String]
	 */
	var coreSubtags = function( locale ) {
		var aux, unicodeLanguageId,
			subtags = [];

		locale = locale.replace( /_/, "-" );

		// Unicode locale extensions.
		aux = locale.split( "-u-" );
		if ( aux[ 1 ] ) {
			aux[ 1 ] = aux[ 1 ].split( "-t-" );
			locale = aux[ 0 ] + ( aux[ 1 ][ 1 ] ? "-t-" + aux[ 1 ][ 1 ] : "");
			subtags[ 4 /* unicodeLocaleExtensions */ ] = aux[ 1 ][ 0 ];
		}

		// TODO normalize transformed extensions. Currently, skipped.
		// subtags[ x ] = locale.split( "-t-" )[ 1 ];
		unicodeLanguageId = locale.split( "-t-" )[ 0 ];

		// unicode_language_id = "root"
		//   | unicode_language_subtag         
		//     (sep unicode_script_subtag)? 
		//     (sep unicode_region_subtag)?
		//     (sep unicode_variant_subtag)* ;
		//
		// Although unicode_language_subtag = alpha{2,8}, I'm using alpha{2,3}. Because, there's no language on CLDR lengthier than 3.
		aux = unicodeLanguageId.match( /^(([a-z]{2,3})(-([A-Z][a-z]{3}))?(-([A-Z]{2}|[0-9]{3}))?)(-[a-zA-Z0-9]{5,8}|[0-9][a-zA-Z0-9]{3})*$|^(root)$/ );
		if ( aux === null ) {
			return [ "und", "Zzzz", "ZZ" ];
		}
		subtags[ 0 /* language */ ] = aux[ 9 ] /* root */ || aux[ 2 ] || "und";
		subtags[ 1 /* script */ ] = aux[ 4 ] || "Zzzz";
		subtags[ 2 /* territory */ ] = aux[ 6 ] || "ZZ";
		subtags[ 3 /* variant */ ] = aux[ 7 ];

		// 0: language
		// 1: script
		// 2: territory (aka region)
		// 3: variant
		// 4: unicodeLocaleExtensions
		return subtags;
	};




	var arrayForEach = function( array, callback ) {
		var i, length;
		if ( array.forEach ) {
			return array.forEach( callback );
		}
		for ( i = 0, length = array.length; i < length; i++ ) {
			callback( array[ i ], i, array );
		}
	};




	/**
	 * bundleLookup( minLanguageId )
	 *
	 * @Cldr [Cldr class]
	 *
	 * @cldr [Cldr instance]
	 *
	 * @minLanguageId [String] requested languageId after applied remove likely subtags.
	 */
	var bundleLookup = function( Cldr, cldr, minLanguageId ) {
		var availableBundleMap = Cldr._availableBundleMap,
			availableBundleMapQueue = Cldr._availableBundleMapQueue;

		if ( availableBundleMapQueue.length ) {
			arrayForEach( availableBundleMapQueue, function( bundle ) {
				var existing, maxBundle, minBundle, subtags;
				subtags = coreSubtags( bundle );
				maxBundle = coreLikelySubtags( Cldr, cldr, subtags, { force: true } ) || subtags;
				minBundle = coreRemoveLikelySubtags( Cldr, cldr, maxBundle );
				minBundle = minBundle.join( Cldr.localeSep );
				existing = availableBundleMapQueue[ minBundle ];
				if ( existing && existing.length < bundle.length ) {
					return;
				}
				availableBundleMap[ minBundle ] = bundle;
			});
			Cldr._availableBundleMapQueue = [];
		}

		return availableBundleMap[ minLanguageId ] || null;
	};




	var objectKeys = function( object ) {
		var i,
			result = [];

		if ( Object.keys ) {
			return Object.keys( object );
		}

		for ( i in object ) {
			result.push( i );
		}

		return result;
	};




	var createError = function( code, attributes ) {
		var error, message;

		message = code + ( attributes && JSON ? ": " + JSON.stringify( attributes ) : "" );
		error = new Error( message );
		error.code = code;

		// extend( error, attributes );
		arrayForEach( objectKeys( attributes ), function( attribute ) {
			error[ attribute ] = attributes[ attribute ];
		});

		return error;
	};




	var validate = function( code, check, attributes ) {
		if ( !check ) {
			throw createError( code, attributes );
		}
	};




	var validatePresence = function( value, name ) {
		validate( "E_MISSING_PARAMETER", typeof value !== "undefined", {
			name: name
		});
	};




	var validateType = function( value, name, check, expected ) {
		validate( "E_INVALID_PAR_TYPE", check, {
			expected: expected,
			name: name,
			value: value
		});
	};




	var validateTypePath = function( value, name ) {
		validateType( value, name, typeof value === "string" || arrayIsArray( value ), "String or Array" );
	};




	/**
	 * Function inspired by jQuery Core, but reduced to our use case.
	 */
	var isPlainObject = function( obj ) {
		return obj !== null && "" + obj === "[object Object]";
	};




	var validateTypePlainObject = function( value, name ) {
		validateType( value, name, typeof value === "undefined" || isPlainObject( value ), "Plain Object" );
	};




	var validateTypeString = function( value, name ) {
		validateType( value, name, typeof value === "string", "a string" );
	};




	// @path: normalized path
	var resourceGet = function( data, path ) {
		var i,
			node = data,
			length = path.length;

		for ( i = 0; i < length - 1; i++ ) {
			node = node[ path[ i ] ];
			if ( !node ) {
				return undefined;
			}
		}
		return node[ path[ i ] ];
	};




	/**
	 * setAvailableBundles( Cldr, json )
	 *
	 * @Cldr [Cldr class]
	 *
	 * @json resolved/unresolved cldr data.
	 *
	 * Set available bundles queue based on passed json CLDR data. Considers a bundle as any String at /main/{bundle}.
	 */
	var coreSetAvailableBundles = function( Cldr, json ) {
		var bundle,
			availableBundleMapQueue = Cldr._availableBundleMapQueue,
			main = resourceGet( json, [ "main" ] );

		if ( main ) {
			for ( bundle in main ) {
				if ( main.hasOwnProperty( bundle ) && bundle !== "root" ) {
					availableBundleMapQueue.push( bundle );
				}
			}
		}
	};



	var alwaysArray = function( somethingOrArray ) {
		return arrayIsArray( somethingOrArray ) ?  somethingOrArray : [ somethingOrArray ];
	};


	var jsonMerge = (function() {

	// Returns new deeply merged JSON.
	//
	// Eg.
	// merge( { a: { b: 1, c: 2 } }, { a: { b: 3, d: 4 } } )
	// -> { a: { b: 3, c: 2, d: 4 } }
	//
	// @arguments JSON's
	// 
	var merge = function() {
		var destination = {},
			sources = [].slice.call( arguments, 0 );
		arrayForEach( sources, function( source ) {
			var prop;
			for ( prop in source ) {
				if ( prop in destination && arrayIsArray( destination[ prop ] ) ) {

					// Concat Arrays
					destination[ prop ] = destination[ prop ].concat( source[ prop ] );

				} else if ( prop in destination && typeof destination[ prop ] === "object" ) {

					// Merge Objects
					destination[ prop ] = merge( destination[ prop ], source[ prop ] );

				} else {

					// Set new values
					destination[ prop ] = source[ prop ];

				}
			}
		});
		return destination;
	};

	return merge;

}());


	/**
	 * load( Cldr, source, jsons )
	 *
	 * @Cldr [Cldr class]
	 *
	 * @source [Object]
	 *
	 * @jsons [arguments]
	 */
	var coreLoad = function( Cldr, source, jsons ) {
		var i, j, json;

		validatePresence( jsons[ 0 ], "json" );

		// Support arbitrary parameters, e.g., `Cldr.load({...}, {...})`.
		for ( i = 0; i < jsons.length; i++ ) {

			// Support array parameters, e.g., `Cldr.load([{...}, {...}])`.
			json = alwaysArray( jsons[ i ] );

			for ( j = 0; j < json.length; j++ ) {
				validateTypePlainObject( json[ j ], "json" );
				source = jsonMerge( source, json[ j ] );
				coreSetAvailableBundles( Cldr, json[ j ] );
			}
		}

		return source;
	};



	var itemGetResolved = function( Cldr, path, attributes ) {
		// Resolve path
		var normalizedPath = pathNormalize( path, attributes );

		return resourceGet( Cldr._resolved, normalizedPath );
	};




	/**
	 * new Cldr()
	 */
	var Cldr = function( locale ) {
		this.init( locale );
	};

	// Build optimization hack to avoid duplicating functions across modules.
	Cldr._alwaysArray = alwaysArray;
	Cldr._coreLoad = coreLoad;
	Cldr._createError = createError;
	Cldr._itemGetResolved = itemGetResolved;
	Cldr._jsonMerge = jsonMerge;
	Cldr._pathNormalize = pathNormalize;
	Cldr._resourceGet = resourceGet;
	Cldr._validatePresence = validatePresence;
	Cldr._validateType = validateType;
	Cldr._validateTypePath = validateTypePath;
	Cldr._validateTypePlainObject = validateTypePlainObject;

	Cldr._availableBundleMap = {};
	Cldr._availableBundleMapQueue = [];
	Cldr._resolved = {};

	// Allow user to override locale separator "-" (default) | "_". According to http://www.unicode.org/reports/tr35/#Unicode_language_identifier, both "-" and "_" are valid locale separators (eg. "en_GB", "en-GB"). According to http://unicode.org/cldr/trac/ticket/6786 its usage must be consistent throughout the data set.
	Cldr.localeSep = "-";

	/**
	 * Cldr.load( json [, json, ...] )
	 *
	 * @json [JSON] CLDR data or [Array] Array of @json's.
	 *
	 * Load resolved cldr data.
	 */
	Cldr.load = function() {
		Cldr._resolved = coreLoad( Cldr, Cldr._resolved, arguments );
	};

	/**
	 * .init() automatically run on instantiation/construction.
	 */
	Cldr.prototype.init = function( locale ) {
		var attributes, language, maxLanguageId, minLanguageId, script, subtags, territory, unicodeLocaleExtensions, variant,
			sep = Cldr.localeSep;

		validatePresence( locale, "locale" );
		validateTypeString( locale, "locale" );

		subtags = coreSubtags( locale );

		unicodeLocaleExtensions = subtags[ 4 ];
		variant = subtags[ 3 ];

		// Normalize locale code.
		// Get (or deduce) the "triple subtags": language, territory (also aliased as region), and script subtags.
		// Get the variant subtags (calendar, collation, currency, etc).
		// refs:
		// - http://www.unicode.org/reports/tr35/#Field_Definitions
		// - http://www.unicode.org/reports/tr35/#Language_and_Locale_IDs
		// - http://www.unicode.org/reports/tr35/#Unicode_locale_identifier

		// When a locale id does not specify a language, or territory (region), or script, they are obtained by Likely Subtags.
		maxLanguageId = coreLikelySubtags( Cldr, this, subtags, { force: true } ) || subtags;
		language = maxLanguageId[ 0 ];
		script = maxLanguageId[ 1 ];
		territory = maxLanguageId[ 2 ];

		minLanguageId = coreRemoveLikelySubtags( Cldr, this, maxLanguageId ).join( sep );

		// Set attributes
		this.attributes = attributes = {
			bundle: bundleLookup( Cldr, this, minLanguageId ),

			// Unicode Language Id
			minlanguageId: minLanguageId,
			maxLanguageId: maxLanguageId.join( sep ),

			// Unicode Language Id Subtabs
			language: language,
			script: script,
			territory: territory,
			region: territory, /* alias */
			variant: variant
		};

		// Unicode locale extensions.
		unicodeLocaleExtensions && ( "-" + unicodeLocaleExtensions ).replace( /-[a-z]{3,8}|(-[a-z]{2})-([a-z]{3,8})/g, function( attribute, key, type ) {

			if ( key ) {

				// Extension is in the `keyword` form.
				attributes[ "u" + key ] = type;
			} else {

				// Extension is in the `attribute` form.
				attributes[ "u" + attribute ] = true;
			}
		});

		this.locale = locale;
	};

	/**
	 * .get()
	 */
	Cldr.prototype.get = function( path ) {

		validatePresence( path, "path" );
		validateTypePath( path, "path" );

		return itemGetResolved( Cldr, path, this.attributes );
	};

	/**
	 * .main()
	 */
	Cldr.prototype.main = function( path ) {
		validatePresence( path, "path" );
		validateTypePath( path, "path" );

		validate( "E_MISSING_BUNDLE", this.attributes.bundle !== null, {
			locale: this.locale
		});

		path = alwaysArray( path );
		return this.get( [ "main/{bundle}" ].concat( path ) );
	};

	return Cldr;




}));

/**
 * CLDR JavaScript Library v0.4.1
 * http://jquery.com/
 *
 * Copyright 2013 Rafael Xavier de Souza
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2015-02-25T13:51Z
 */
/*!
 * CLDR JavaScript Library v0.4.1 2015-02-25T13:51Z MIT license © Rafael Xavier
 * http://git.io/h4lmVg
 */
(function( factory ) {

	if ( typeof define === "function" && define.amd ) {
		// AMD.
		define( [ "../cldr" ], factory );
	} else if ( typeof module === "object" && typeof module.exports === "object" ) {
		// Node. CommonJS.
		module.exports = factory( require( "cldrjs" ) );
	} else {
		// Global
		factory( Cldr );
	}

}(function( Cldr ) {

	// Build optimization hack to avoid duplicating functions across modules.
	var pathNormalize = Cldr._pathNormalize,
		validatePresence = Cldr._validatePresence,
		validateType = Cldr._validateType;

/*!
 * EventEmitter v4.2.7 - git.io/ee
 * Oliver Caldwell
 * MIT license
 * @preserve
 */

var EventEmitter;
/* jshint ignore:start */
EventEmitter = (function () {
	

	/**
	 * Class for managing events.
	 * Can be extended to provide event functionality in other classes.
	 *
	 * @class EventEmitter Manages event registering and emitting.
	 */
	function EventEmitter() {}

	// Shortcuts to improve speed and size
	var proto = EventEmitter.prototype;
	var exports = this;
	var originalGlobalValue = exports.EventEmitter;

	/**
	 * Finds the index of the listener for the event in it's storage array.
	 *
	 * @param {Function[]} listeners Array of listeners to search through.
	 * @param {Function} listener Method to look for.
	 * @return {Number} Index of the specified listener, -1 if not found
	 * @api private
	 */
	function indexOfListener(listeners, listener) {
		var i = listeners.length;
		while (i--) {
			if (listeners[i].listener === listener) {
				return i;
			}
		}

		return -1;
	}

	/**
	 * Alias a method while keeping the context correct, to allow for overwriting of target method.
	 *
	 * @param {String} name The name of the target method.
	 * @return {Function} The aliased method
	 * @api private
	 */
	function alias(name) {
		return function aliasClosure() {
			return this[name].apply(this, arguments);
		};
	}

	/**
	 * Returns the listener array for the specified event.
	 * Will initialise the event object and listener arrays if required.
	 * Will return an object if you use a regex search. The object contains keys for each matched event. So /ba[rz]/ might return an object containing bar and baz. But only if you have either defined them with defineEvent or added some listeners to them.
	 * Each property in the object response is an array of listener functions.
	 *
	 * @param {String|RegExp} evt Name of the event to return the listeners from.
	 * @return {Function[]|Object} All listener functions for the event.
	 */
	proto.getListeners = function getListeners(evt) {
		var events = this._getEvents();
		var response;
		var key;

		// Return a concatenated array of all matching events if
		// the selector is a regular expression.
		if (evt instanceof RegExp) {
			response = {};
			for (key in events) {
				if (events.hasOwnProperty(key) && evt.test(key)) {
					response[key] = events[key];
				}
			}
		}
		else {
			response = events[evt] || (events[evt] = []);
		}

		return response;
	};

	/**
	 * Takes a list of listener objects and flattens it into a list of listener functions.
	 *
	 * @param {Object[]} listeners Raw listener objects.
	 * @return {Function[]} Just the listener functions.
	 */
	proto.flattenListeners = function flattenListeners(listeners) {
		var flatListeners = [];
		var i;

		for (i = 0; i < listeners.length; i += 1) {
			flatListeners.push(listeners[i].listener);
		}

		return flatListeners;
	};

	/**
	 * Fetches the requested listeners via getListeners but will always return the results inside an object. This is mainly for internal use but others may find it useful.
	 *
	 * @param {String|RegExp} evt Name of the event to return the listeners from.
	 * @return {Object} All listener functions for an event in an object.
	 */
	proto.getListenersAsObject = function getListenersAsObject(evt) {
		var listeners = this.getListeners(evt);
		var response;

		if (listeners instanceof Array) {
			response = {};
			response[evt] = listeners;
		}

		return response || listeners;
	};

	/**
	 * Adds a listener function to the specified event.
	 * The listener will not be added if it is a duplicate.
	 * If the listener returns true then it will be removed after it is called.
	 * If you pass a regular expression as the event name then the listener will be added to all events that match it.
	 *
	 * @param {String|RegExp} evt Name of the event to attach the listener to.
	 * @param {Function} listener Method to be called when the event is emitted. If the function returns true then it will be removed after calling.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.addListener = function addListener(evt, listener) {
		var listeners = this.getListenersAsObject(evt);
		var listenerIsWrapped = typeof listener === 'object';
		var key;

		for (key in listeners) {
			if (listeners.hasOwnProperty(key) && indexOfListener(listeners[key], listener) === -1) {
				listeners[key].push(listenerIsWrapped ? listener : {
					listener: listener,
					once: false
				});
			}
		}

		return this;
	};

	/**
	 * Alias of addListener
	 */
	proto.on = alias('addListener');

	/**
	 * Semi-alias of addListener. It will add a listener that will be
	 * automatically removed after it's first execution.
	 *
	 * @param {String|RegExp} evt Name of the event to attach the listener to.
	 * @param {Function} listener Method to be called when the event is emitted. If the function returns true then it will be removed after calling.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.addOnceListener = function addOnceListener(evt, listener) {
		return this.addListener(evt, {
			listener: listener,
			once: true
		});
	};

	/**
	 * Alias of addOnceListener.
	 */
	proto.once = alias('addOnceListener');

	/**
	 * Defines an event name. This is required if you want to use a regex to add a listener to multiple events at once. If you don't do this then how do you expect it to know what event to add to? Should it just add to every possible match for a regex? No. That is scary and bad.
	 * You need to tell it what event names should be matched by a regex.
	 *
	 * @param {String} evt Name of the event to create.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.defineEvent = function defineEvent(evt) {
		this.getListeners(evt);
		return this;
	};

	/**
	 * Uses defineEvent to define multiple events.
	 *
	 * @param {String[]} evts An array of event names to define.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.defineEvents = function defineEvents(evts) {
		for (var i = 0; i < evts.length; i += 1) {
			this.defineEvent(evts[i]);
		}
		return this;
	};

	/**
	 * Removes a listener function from the specified event.
	 * When passed a regular expression as the event name, it will remove the listener from all events that match it.
	 *
	 * @param {String|RegExp} evt Name of the event to remove the listener from.
	 * @param {Function} listener Method to remove from the event.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.removeListener = function removeListener(evt, listener) {
		var listeners = this.getListenersAsObject(evt);
		var index;
		var key;

		for (key in listeners) {
			if (listeners.hasOwnProperty(key)) {
				index = indexOfListener(listeners[key], listener);

				if (index !== -1) {
					listeners[key].splice(index, 1);
				}
			}
		}

		return this;
	};

	/**
	 * Alias of removeListener
	 */
	proto.off = alias('removeListener');

	/**
	 * Adds listeners in bulk using the manipulateListeners method.
	 * If you pass an object as the second argument you can add to multiple events at once. The object should contain key value pairs of events and listeners or listener arrays. You can also pass it an event name and an array of listeners to be added.
	 * You can also pass it a regular expression to add the array of listeners to all events that match it.
	 * Yeah, this function does quite a bit. That's probably a bad thing.
	 *
	 * @param {String|Object|RegExp} evt An event name if you will pass an array of listeners next. An object if you wish to add to multiple events at once.
	 * @param {Function[]} [listeners] An optional array of listener functions to add.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.addListeners = function addListeners(evt, listeners) {
		// Pass through to manipulateListeners
		return this.manipulateListeners(false, evt, listeners);
	};

	/**
	 * Removes listeners in bulk using the manipulateListeners method.
	 * If you pass an object as the second argument you can remove from multiple events at once. The object should contain key value pairs of events and listeners or listener arrays.
	 * You can also pass it an event name and an array of listeners to be removed.
	 * You can also pass it a regular expression to remove the listeners from all events that match it.
	 *
	 * @param {String|Object|RegExp} evt An event name if you will pass an array of listeners next. An object if you wish to remove from multiple events at once.
	 * @param {Function[]} [listeners] An optional array of listener functions to remove.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.removeListeners = function removeListeners(evt, listeners) {
		// Pass through to manipulateListeners
		return this.manipulateListeners(true, evt, listeners);
	};

	/**
	 * Edits listeners in bulk. The addListeners and removeListeners methods both use this to do their job. You should really use those instead, this is a little lower level.
	 * The first argument will determine if the listeners are removed (true) or added (false).
	 * If you pass an object as the second argument you can add/remove from multiple events at once. The object should contain key value pairs of events and listeners or listener arrays.
	 * You can also pass it an event name and an array of listeners to be added/removed.
	 * You can also pass it a regular expression to manipulate the listeners of all events that match it.
	 *
	 * @param {Boolean} remove True if you want to remove listeners, false if you want to add.
	 * @param {String|Object|RegExp} evt An event name if you will pass an array of listeners next. An object if you wish to add/remove from multiple events at once.
	 * @param {Function[]} [listeners] An optional array of listener functions to add/remove.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.manipulateListeners = function manipulateListeners(remove, evt, listeners) {
		var i;
		var value;
		var single = remove ? this.removeListener : this.addListener;
		var multiple = remove ? this.removeListeners : this.addListeners;

		// If evt is an object then pass each of it's properties to this method
		if (typeof evt === 'object' && !(evt instanceof RegExp)) {
			for (i in evt) {
				if (evt.hasOwnProperty(i) && (value = evt[i])) {
					// Pass the single listener straight through to the singular method
					if (typeof value === 'function') {
						single.call(this, i, value);
					}
					else {
						// Otherwise pass back to the multiple function
						multiple.call(this, i, value);
					}
				}
			}
		}
		else {
			// So evt must be a string
			// And listeners must be an array of listeners
			// Loop over it and pass each one to the multiple method
			i = listeners.length;
			while (i--) {
				single.call(this, evt, listeners[i]);
			}
		}

		return this;
	};

	/**
	 * Removes all listeners from a specified event.
	 * If you do not specify an event then all listeners will be removed.
	 * That means every event will be emptied.
	 * You can also pass a regex to remove all events that match it.
	 *
	 * @param {String|RegExp} [evt] Optional name of the event to remove all listeners for. Will remove from every event if not passed.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.removeEvent = function removeEvent(evt) {
		var type = typeof evt;
		var events = this._getEvents();
		var key;

		// Remove different things depending on the state of evt
		if (type === 'string') {
			// Remove all listeners for the specified event
			delete events[evt];
		}
		else if (evt instanceof RegExp) {
			// Remove all events matching the regex.
			for (key in events) {
				if (events.hasOwnProperty(key) && evt.test(key)) {
					delete events[key];
				}
			}
		}
		else {
			// Remove all listeners in all events
			delete this._events;
		}

		return this;
	};

	/**
	 * Alias of removeEvent.
	 *
	 * Added to mirror the node API.
	 */
	proto.removeAllListeners = alias('removeEvent');

	/**
	 * Emits an event of your choice.
	 * When emitted, every listener attached to that event will be executed.
	 * If you pass the optional argument array then those arguments will be passed to every listener upon execution.
	 * Because it uses `apply`, your array of arguments will be passed as if you wrote them out separately.
	 * So they will not arrive within the array on the other side, they will be separate.
	 * You can also pass a regular expression to emit to all events that match it.
	 *
	 * @param {String|RegExp} evt Name of the event to emit and execute listeners for.
	 * @param {Array} [args] Optional array of arguments to be passed to each listener.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.emitEvent = function emitEvent(evt, args) {
		var listeners = this.getListenersAsObject(evt);
		var listener;
		var i;
		var key;
		var response;

		for (key in listeners) {
			if (listeners.hasOwnProperty(key)) {
				i = listeners[key].length;

				while (i--) {
					// If the listener returns true then it shall be removed from the event
					// The function is executed either with a basic call or an apply if there is an args array
					listener = listeners[key][i];

					if (listener.once === true) {
						this.removeListener(evt, listener.listener);
					}

					response = listener.listener.apply(this, args || []);

					if (response === this._getOnceReturnValue()) {
						this.removeListener(evt, listener.listener);
					}
				}
			}
		}

		return this;
	};

	/**
	 * Alias of emitEvent
	 */
	proto.trigger = alias('emitEvent');

	/**
	 * Subtly different from emitEvent in that it will pass its arguments on to the listeners, as opposed to taking a single array of arguments to pass on.
	 * As with emitEvent, you can pass a regex in place of the event name to emit to all events that match it.
	 *
	 * @param {String|RegExp} evt Name of the event to emit and execute listeners for.
	 * @param {...*} Optional additional arguments to be passed to each listener.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.emit = function emit(evt) {
		var args = Array.prototype.slice.call(arguments, 1);
		return this.emitEvent(evt, args);
	};

	/**
	 * Sets the current value to check against when executing listeners. If a
	 * listeners return value matches the one set here then it will be removed
	 * after execution. This value defaults to true.
	 *
	 * @param {*} value The new value to check for when executing listeners.
	 * @return {Object} Current instance of EventEmitter for chaining.
	 */
	proto.setOnceReturnValue = function setOnceReturnValue(value) {
		this._onceReturnValue = value;
		return this;
	};

	/**
	 * Fetches the current value to check against when executing listeners. If
	 * the listeners return value matches this one then it should be removed
	 * automatically. It will return true by default.
	 *
	 * @return {*|Boolean} The current value to check for or the default, true.
	 * @api private
	 */
	proto._getOnceReturnValue = function _getOnceReturnValue() {
		if (this.hasOwnProperty('_onceReturnValue')) {
			return this._onceReturnValue;
		}
		else {
			return true;
		}
	};

	/**
	 * Fetches the events object and creates one if required.
	 *
	 * @return {Object} The events storage object.
	 * @api private
	 */
	proto._getEvents = function _getEvents() {
		return this._events || (this._events = {});
	};

	/**
	 * Reverts the global {@link EventEmitter} to its previous value and returns a reference to this version.
	 *
	 * @return {Function} Non conflicting EventEmitter class.
	 */
	EventEmitter.noConflict = function noConflict() {
		exports.EventEmitter = originalGlobalValue;
		return EventEmitter;
	};

	return EventEmitter;
}());
/* jshint ignore:end */



	var validateTypeFunction = function( value, name ) {
		validateType( value, name, typeof value === "undefined" || typeof value === "function", "Function" );
	};




	var superGet, superInit,
		globalEe = new EventEmitter();

	function validateTypeEvent( value, name ) {
		validateType( value, name, typeof value === "string" || value instanceof RegExp, "String or RegExp" );
	}

	function validateThenCall( method, self ) {
		return function( event, listener ) {
			validatePresence( event, "event" );
			validateTypeEvent( event, "event" );

			validatePresence( listener, "listener" );
			validateTypeFunction( listener, "listener" );

			return self[ method ].apply( self, arguments );
		};
	}

	function off( self ) {
		return validateThenCall( "off", self );
	}

	function on( self ) {
		return validateThenCall( "on", self );
	}

	function once( self ) {
		return validateThenCall( "once", self );
	}

	Cldr.off = off( globalEe );
	Cldr.on = on( globalEe );
	Cldr.once = once( globalEe );

	/**
	 * Overload Cldr.prototype.init().
	 */
	superInit = Cldr.prototype.init;
	Cldr.prototype.init = function() {
		var ee;
		this.ee = ee = new EventEmitter();
		this.off = off( ee );
		this.on = on( ee );
		this.once = once( ee );
		superInit.apply( this, arguments );
	};

	/**
	 * getOverload is encapsulated, because of cldr/unresolved. If it's loaded
	 * after cldr/event (and note it overwrites .get), it can trigger this
	 * overload again.
	 */
	function getOverload() {

		/**
		 * Overload Cldr.prototype.get().
		 */
		superGet = Cldr.prototype.get;
		Cldr.prototype.get = function( path ) {
			var value = superGet.apply( this, arguments );
			path = pathNormalize( path, this.attributes ).join( "/" );
			globalEe.trigger( "get", [ path, value ] );
			this.ee.trigger( "get", [ path, value ] );
			return value;
		};
	}

	Cldr._eventInit = getOverload;
	getOverload();

	return Cldr;




}));

/**
 * CLDR JavaScript Library v0.4.1
 * http://jquery.com/
 *
 * Copyright 2013 Rafael Xavier de Souza
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2015-02-25T13:51Z
 */
/*!
 * CLDR JavaScript Library v0.4.1 2015-02-25T13:51Z MIT license © Rafael Xavier
 * http://git.io/h4lmVg
 */
(function( factory ) {

	if ( typeof define === "function" && define.amd ) {
		// AMD.
		define( [ "../cldr" ], factory );
	} else if ( typeof module === "object" && typeof module.exports === "object" ) {
		// Node. CommonJS.
		module.exports = factory( require( "cldrjs" ) );
	} else {
		// Global
		factory( Cldr );
	}

}(function( Cldr ) {

	// Build optimization hack to avoid duplicating functions across modules.
	var alwaysArray = Cldr._alwaysArray;



	var supplementalMain = function( cldr ) {

		var prepend, supplemental;
		
		prepend = function( prepend ) {
			return function( path ) {
				path = alwaysArray( path );
				return cldr.get( [ prepend ].concat( path ) );
			};
		};

		supplemental = prepend( "supplemental" );

		// Week Data
		// http://www.unicode.org/reports/tr35/tr35-dates.html#Week_Data
		supplemental.weekData = prepend( "supplemental/weekData" );

		supplemental.weekData.firstDay = function() {
			return cldr.get( "supplemental/weekData/firstDay/{territory}" ) ||
				cldr.get( "supplemental/weekData/firstDay/001" );
		};

		supplemental.weekData.minDays = function() {
			var minDays = cldr.get( "supplemental/weekData/minDays/{territory}" ) ||
				cldr.get( "supplemental/weekData/minDays/001" );
			return parseInt( minDays, 10 );
		};

		// Time Data
		// http://www.unicode.org/reports/tr35/tr35-dates.html#Time_Data
		supplemental.timeData = prepend( "supplemental/timeData" );

		supplemental.timeData.allowed = function() {
			return cldr.get( "supplemental/timeData/{territory}/_allowed" ) ||
				cldr.get( "supplemental/timeData/001/_allowed" );
		};

		supplemental.timeData.preferred = function() {
			return cldr.get( "supplemental/timeData/{territory}/_preferred" ) ||
				cldr.get( "supplemental/timeData/001/_preferred" );
		};

		return supplemental;

	};




	var initSuper = Cldr.prototype.init;

	/**
	 * .init() automatically ran on construction.
	 *
	 * Overload .init().
	 */
	Cldr.prototype.init = function() {
		initSuper.apply( this, arguments );
		this.supplemental = supplementalMain( this );
	};

	return Cldr;




}));

/**
 * Globalize v1.0.0
 *
 * http://github.com/jquery/globalize
 *
 * Copyright jQuery Foundation and other contributors
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2015-04-23T12:02Z
 */
/*!
 * Globalize v1.0.0 2015-04-23T12:02Z Released under the MIT license
 * http://git.io/TrdQbw
 */
(function( root, factory ) {

	// UMD returnExports
	if ( typeof define === "function" && define.amd ) {

		// AMD
		define([
			"cldr",
			"cldr/event"
		], factory );
	} else if ( typeof exports === "object" ) {

		// Node, CommonJS
		module.exports = factory( require( "cldrjs" ) );
	} else {

		// Global
		root.Globalize = factory( root.Cldr );
	}
}( this, function( Cldr ) {


/**
 * A toString method that outputs meaningful values for objects or arrays and
 * still performs as fast as a plain string in case variable is string, or as
 * fast as `"" + number` in case variable is a number.
 * Ref: http://jsperf.com/my-stringify
 */
var toString = function( variable ) {
	return typeof variable === "string" ? variable : ( typeof variable === "number" ? "" +
		variable : JSON.stringify( variable ) );
};




/**
 * formatMessage( message, data )
 *
 * @message [String] A message with optional {vars} to be replaced.
 *
 * @data [Array or JSON] Object with replacing-variables content.
 *
 * Return the formatted message. For example:
 *
 * - formatMessage( "{0} second", [ 1 ] ); // 1 second
 *
 * - formatMessage( "{0}/{1}", ["m", "s"] ); // m/s
 *
 * - formatMessage( "{name} <{email}>", {
 *     name: "Foo",
 *     email: "bar@baz.qux"
 *   }); // Foo <bar@baz.qux>
 */
var formatMessage = function( message, data ) {

	// Replace {attribute}'s
	message = message.replace( /{[0-9a-zA-Z-_. ]+}/g, function( name ) {
		name = name.replace( /^{([^}]*)}$/, "$1" );
		return toString( data[ name ] );
	});

	return message;
};




var objectExtend = function() {
	var destination = arguments[ 0 ],
		sources = [].slice.call( arguments, 1 );

	sources.forEach(function( source ) {
		var prop;
		for ( prop in source ) {
			destination[ prop ] = source[ prop ];
		}
	});

	return destination;
};




var createError = function( code, message, attributes ) {
	var error;

	message = code + ( message ? ": " + formatMessage( message, attributes ) : "" );
	error = new Error( message );
	error.code = code;

	objectExtend( error, attributes );

	return error;
};




var validate = function( code, message, check, attributes ) {
	if ( !check ) {
		throw createError( code, message, attributes );
	}
};




var alwaysArray = function( stringOrArray ) {
	return Array.isArray( stringOrArray ) ? stringOrArray : stringOrArray ? [ stringOrArray ] : [];
};




var validateCldr = function( path, value, options ) {
	var skipBoolean;
	options = options || {};

	skipBoolean = alwaysArray( options.skip ).some(function( pathRe ) {
		return pathRe.test( path );
	});

	validate( "E_MISSING_CLDR", "Missing required CLDR content `{path}`.", value || skipBoolean, {
		path: path
	});
};




var validateDefaultLocale = function( value ) {
	validate( "E_DEFAULT_LOCALE_NOT_DEFINED", "Default locale has not been defined.",
		value !== undefined, {} );
};




var validateParameterPresence = function( value, name ) {
	validate( "E_MISSING_PARAMETER", "Missing required parameter `{name}`.",
		value !== undefined, { name: name });
};




/**
 * range( value, name, minimum, maximum )
 *
 * @value [Number].
 *
 * @name [String] name of variable.
 *
 * @minimum [Number]. The lowest valid value, inclusive.
 *
 * @maximum [Number]. The greatest valid value, inclusive.
 */
var validateParameterRange = function( value, name, minimum, maximum ) {
	validate(
		"E_PAR_OUT_OF_RANGE",
		"Parameter `{name}` has value `{value}` out of range [{minimum}, {maximum}].",
		value === undefined || value >= minimum && value <= maximum,
		{
			maximum: maximum,
			minimum: minimum,
			name: name,
			value: value
		}
	);
};




var validateParameterType = function( value, name, check, expected ) {
	validate(
		"E_INVALID_PAR_TYPE",
		"Invalid `{name}` parameter ({value}). {expected} expected.",
		check,
		{
			expected: expected,
			name: name,
			value: value
		}
	);
};




var validateParameterTypeLocale = function( value, name ) {
	validateParameterType(
		value,
		name,
		value === undefined || typeof value === "string" || value instanceof Cldr,
		"String or Cldr instance"
	);
};




/**
 * Function inspired by jQuery Core, but reduced to our use case.
 */
var isPlainObject = function( obj ) {
	return obj !== null && "" + obj === "[object Object]";
};




var validateParameterTypePlainObject = function( value, name ) {
	validateParameterType(
		value,
		name,
		value === undefined || isPlainObject( value ),
		"Plain Object"
	);
};




var alwaysCldr = function( localeOrCldr ) {
	return localeOrCldr instanceof Cldr ? localeOrCldr : new Cldr( localeOrCldr );
};




// ref: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_Expressions?redirectlocale=en-US&redirectslug=JavaScript%2FGuide%2FRegular_Expressions
var regexpEscape = function( string ) {
	return string.replace( /([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1" );
};




var stringPad = function( str, count, right ) {
	var length;
	if ( typeof str !== "string" ) {
		str = String( str );
	}
	for ( length = str.length; length < count; length += 1 ) {
		str = ( right ? ( str + "0" ) : ( "0" + str ) );
	}
	return str;
};




function validateLikelySubtags( cldr ) {
	cldr.once( "get", validateCldr );
	cldr.get( "supplemental/likelySubtags" );
}

/**
 * [new] Globalize( locale|cldr )
 *
 * @locale [String]
 *
 * @cldr [Cldr instance]
 *
 * Create a Globalize instance.
 */
function Globalize( locale ) {
	if ( !( this instanceof Globalize ) ) {
		return new Globalize( locale );
	}

	validateParameterPresence( locale, "locale" );
	validateParameterTypeLocale( locale, "locale" );

	this.cldr = alwaysCldr( locale );

	validateLikelySubtags( this.cldr );
}

/**
 * Globalize.load( json, ... )
 *
 * @json [JSON]
 *
 * Load resolved or unresolved cldr data.
 * Somewhat equivalent to previous Globalize.addCultureInfo(...).
 */
Globalize.load = function() {
	// validations are delegated to Cldr.load().
	Cldr.load.apply( Cldr, arguments );
};

/**
 * Globalize.locale( [locale|cldr] )
 *
 * @locale [String]
 *
 * @cldr [Cldr instance]
 *
 * Set default Cldr instance if locale or cldr argument is passed.
 *
 * Return the default Cldr instance.
 */
Globalize.locale = function( locale ) {
	validateParameterTypeLocale( locale, "locale" );

	if ( arguments.length ) {
		this.cldr = alwaysCldr( locale );
		validateLikelySubtags( this.cldr );
	}
	return this.cldr;
};

/**
 * Optimization to avoid duplicating some internal functions across modules.
 */
Globalize._alwaysArray = alwaysArray;
Globalize._createError = createError;
Globalize._formatMessage = formatMessage;
Globalize._isPlainObject = isPlainObject;
Globalize._objectExtend = objectExtend;
Globalize._regexpEscape = regexpEscape;
Globalize._stringPad = stringPad;
Globalize._validate = validate;
Globalize._validateCldr = validateCldr;
Globalize._validateDefaultLocale = validateDefaultLocale;
Globalize._validateParameterPresence = validateParameterPresence;
Globalize._validateParameterRange = validateParameterRange;
Globalize._validateParameterTypePlainObject = validateParameterTypePlainObject;
Globalize._validateParameterType = validateParameterType;

return Globalize;




}));

/**
 * Globalize v1.0.0
 *
 * http://github.com/jquery/globalize
 *
 * Copyright jQuery Foundation and other contributors
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2015-04-23T12:02Z
 */
/*!
 * Globalize v1.0.0 2015-04-23T12:02Z Released under the MIT license
 * http://git.io/TrdQbw
 */
(function( root, factory ) {

	// UMD returnExports
	if ( typeof define === "function" && define.amd ) {

		// AMD
		define([
			"cldr",
			"../globalize",
			"cldr/event",
			"cldr/supplemental"
		], factory );
	} else if ( typeof exports === "object" ) {

		// Node, CommonJS
		module.exports = factory( require( "cldrjs" ), require( "globalize" ) );
	} else {

		// Global
		factory( root.Cldr, root.Globalize );
	}
}(this, function( Cldr, Globalize ) {

var createError = Globalize._createError,
	objectExtend = Globalize._objectExtend,
	regexpEscape = Globalize._regexpEscape,
	stringPad = Globalize._stringPad,
	validateCldr = Globalize._validateCldr,
	validateDefaultLocale = Globalize._validateDefaultLocale,
	validateParameterPresence = Globalize._validateParameterPresence,
	validateParameterRange = Globalize._validateParameterRange,
	validateParameterType = Globalize._validateParameterType,
	validateParameterTypePlainObject = Globalize._validateParameterTypePlainObject;


var createErrorUnsupportedFeature = function( feature ) {
	return createError( "E_UNSUPPORTED", "Unsupported {feature}.", {
		feature: feature
	});
};




var validateParameterTypeNumber = function( value, name ) {
	validateParameterType(
		value,
		name,
		value === undefined || typeof value === "number",
		"Number"
	);
};




var validateParameterTypeString = function( value, name ) {
	validateParameterType(
		value,
		name,
		value === undefined || typeof value === "string",
		"a string"
	);
};




/**
 * goupingSeparator( number, primaryGroupingSize, secondaryGroupingSize )
 *
 * @number [Number].
 *
 * @primaryGroupingSize [Number]
 *
 * @secondaryGroupingSize [Number]
 *
 * Return the formatted number with group separator.
 */
var numberFormatGroupingSeparator = function( number, primaryGroupingSize, secondaryGroupingSize ) {
	var index,
		currentGroupingSize = primaryGroupingSize,
		ret = "",
		sep = ",",
		switchToSecondary = secondaryGroupingSize ? true : false;

	number = String( number ).split( "." );
	index = number[ 0 ].length;

	while ( index > currentGroupingSize ) {
		ret = number[ 0 ].slice( index - currentGroupingSize, index ) +
			( ret.length ? sep : "" ) + ret;
		index -= currentGroupingSize;
		if ( switchToSecondary ) {
			currentGroupingSize = secondaryGroupingSize;
			switchToSecondary = false;
		}
	}

	number[ 0 ] = number[ 0 ].slice( 0, index ) + ( ret.length ? sep : "" ) + ret;
	return number.join( "." );
};




/**
 * integerFractionDigits( number, minimumIntegerDigits, minimumFractionDigits,
 * maximumFractionDigits, round, roundIncrement )
 *
 * @number [Number]
 *
 * @minimumIntegerDigits [Number]
 *
 * @minimumFractionDigits [Number]
 *
 * @maximumFractionDigits [Number]
 *
 * @round [Function]
 *
 * @roundIncrement [Function]
 *
 * Return the formatted integer and fraction digits.
 */
var numberFormatIntegerFractionDigits = function( number, minimumIntegerDigits, minimumFractionDigits, maximumFractionDigits, round,
	roundIncrement ) {

	// Fraction
	if ( maximumFractionDigits ) {

		// Rounding
		if ( roundIncrement ) {
			number = round( number, roundIncrement );

		// Maximum fraction digits
		} else {
			number = round( number, { exponent: -maximumFractionDigits } );
		}

		// Minimum fraction digits
		if ( minimumFractionDigits ) {
			number = String( number ).split( "." );
			number[ 1 ] = stringPad( number[ 1 ] || "", minimumFractionDigits, true );
			number = number.join( "." );
		}
	} else {
		number = round( number );
	}

	number = String( number );

	// Minimum integer digits
	if ( minimumIntegerDigits ) {
		number = number.split( "." );
		number[ 0 ] = stringPad( number[ 0 ], minimumIntegerDigits );
		number = number.join( "." );
	}

	return number;
};




/**
 * toPrecision( number, precision, round )
 *
 * @number (Number)
 *
 * @precision (Number) significant figures precision (not decimal precision).
 *
 * @round (Function)
 *
 * Return number.toPrecision( precision ) using the given round function.
 */
var numberToPrecision = function( number, precision, round ) {
	var roundOrder;

	// Get number at two extra significant figure precision.
	number = number.toPrecision( precision + 2 );

	// Then, round it to the required significant figure precision.
	roundOrder = Math.ceil( Math.log( Math.abs( number ) ) / Math.log( 10 ) );
	roundOrder -= precision;

	return round( number, { exponent: roundOrder } );
};




/**
 * toPrecision( number, minimumSignificantDigits, maximumSignificantDigits, round )
 *
 * @number [Number]
 *
 * @minimumSignificantDigits [Number]
 *
 * @maximumSignificantDigits [Number]
 *
 * @round [Function]
 *
 * Return the formatted significant digits number.
 */
var numberFormatSignificantDigits = function( number, minimumSignificantDigits, maximumSignificantDigits, round ) {
	var atMinimum, atMaximum;

	// Sanity check.
	if ( minimumSignificantDigits > maximumSignificantDigits ) {
		maximumSignificantDigits = minimumSignificantDigits;
	}

	atMinimum = numberToPrecision( number, minimumSignificantDigits, round );
	atMaximum = numberToPrecision( number, maximumSignificantDigits, round );

	// Use atMaximum only if it has more significant digits than atMinimum.
	number = +atMinimum === +atMaximum ? atMinimum : atMaximum;

	// Expand integer numbers, eg. 123e5 to 12300.
	number = ( +number ).toString( 10 );

	if ( (/e/).test( number ) ) {
		throw createErrorUnsupportedFeature({
			feature: "integers out of (1e21, 1e-7)"
		});
	}

	// Add trailing zeros if necessary.
	if ( minimumSignificantDigits - number.replace( /^0+|\./g, "" ).length > 0 ) {
		number = number.split( "." );
		number[ 1 ] = stringPad( number[ 1 ] || "", minimumSignificantDigits - number[ 0 ].replace( /^0+/, "" ).length, true );
		number = number.join( "." );
	}

	return number;
};




/**
 * format( number, properties )
 *
 * @number [Number].
 *
 * @properties [Object] Output of number/format-properties.
 *
 * Return the formatted number.
 * ref: http://www.unicode.org/reports/tr35/tr35-numbers.html
 */
var numberFormat = function( number, properties ) {
	var infinitySymbol, maximumFractionDigits, maximumSignificantDigits, minimumFractionDigits,
	minimumIntegerDigits, minimumSignificantDigits, nanSymbol, nuDigitsMap, padding, prefix,
	primaryGroupingSize, pattern, ret, round, roundIncrement, secondaryGroupingSize, suffix,
	symbolMap;

	padding = properties[ 1 ];
	minimumIntegerDigits = properties[ 2 ];
	minimumFractionDigits = properties[ 3 ];
	maximumFractionDigits = properties[ 4 ];
	minimumSignificantDigits = properties[ 5 ];
	maximumSignificantDigits = properties[ 6 ];
	roundIncrement = properties[ 7 ];
	primaryGroupingSize = properties[ 8 ];
	secondaryGroupingSize = properties[ 9 ];
	round = properties[ 15 ];
	infinitySymbol = properties[ 16 ];
	nanSymbol = properties[ 17 ];
	symbolMap = properties[ 18 ];
	nuDigitsMap = properties[ 19 ];

	// NaN
	if ( isNaN( number ) ) {
		return nanSymbol;
	}

	if ( number < 0 ) {
		pattern = properties[ 12 ];
		prefix = properties[ 13 ];
		suffix = properties[ 14 ];
	} else {
		pattern = properties[ 11 ];
		prefix = properties[ 0 ];
		suffix = properties[ 10 ];
	}

	// Infinity
	if ( !isFinite( number ) ) {
		return prefix + infinitySymbol + suffix;
	}

	ret = prefix;

	// Percent
	if ( pattern.indexOf( "%" ) !== -1 ) {
		number *= 100;

	// Per mille
	} else if ( pattern.indexOf( "\u2030" ) !== -1 ) {
		number *= 1000;
	}

	// Significant digit format
	if ( !isNaN( minimumSignificantDigits * maximumSignificantDigits ) ) {
		number = numberFormatSignificantDigits( number, minimumSignificantDigits,
			maximumSignificantDigits, round );

	// Integer and fractional format
	} else {
		number = numberFormatIntegerFractionDigits( number, minimumIntegerDigits,
			minimumFractionDigits, maximumFractionDigits, round, roundIncrement );
	}

	// Remove the possible number minus sign
	number = number.replace( /^-/, "" );

	// Grouping separators
	if ( primaryGroupingSize ) {
		number = numberFormatGroupingSeparator( number, primaryGroupingSize,
			secondaryGroupingSize );
	}

	ret += number;

	// Scientific notation
	// TODO implement here

	// Padding/'([^']|'')+'|''|[.,\-+E%\u2030]/g
	// TODO implement here

	ret += suffix;

	return ret.replace( /('([^']|'')+'|'')|./g, function( character, literal ) {

		// Literals
		if ( literal ) {
			literal = literal.replace( /''/, "'" );
			if ( literal.length > 2 ) {
				literal = literal.slice( 1, -1 );
			}
			return literal;
		}

		// Symbols
		character = character.replace( /[.,\-+E%\u2030]/, function( symbol ) {
			return symbolMap[ symbol ];
		});

		// Numbering system
		if ( nuDigitsMap ) {
			character = character.replace( /[0-9]/, function( digit ) {
				return nuDigitsMap[ +digit ];
			});
		}

		return character;
	});
};




/**
 * NumberingSystem( cldr )
 *
 * - http://www.unicode.org/reports/tr35/tr35-numbers.html#otherNumberingSystems
 * - http://cldr.unicode.org/index/bcp47-extension
 * - http://www.unicode.org/reports/tr35/#u_Extension
 */
var numberNumberingSystem = function( cldr ) {
	var nu = cldr.attributes[ "u-nu" ];

	if ( nu ) {
		if ( nu === "traditio" ) {
			nu = "traditional";
		}
		if ( [ "native", "traditional", "finance" ].indexOf( nu ) !== -1 ) {

			// Unicode locale extension `u-nu` is set using either (native, traditional or
			// finance). So, lookup the respective locale's numberingSystem and return it.
			return cldr.main([ "numbers/otherNumberingSystems", nu ]);
		}

		// Unicode locale extension `u-nu` is set with an explicit numberingSystem. Return it.
		return nu;
	}

	// Return the default numberingSystem.
	return cldr.main( "numbers/defaultNumberingSystem" );
};




/**
 * nuMap( cldr )
 *
 * @cldr [Cldr instance].
 *
 * Return digits map if numbering system is different than `latn`.
 */
var numberNumberingSystemDigitsMap = function( cldr ) {
	var aux,
		nu = numberNumberingSystem( cldr );

	if ( nu === "latn" ) {
		return;
	}

	aux = cldr.supplemental([ "numberingSystems", nu ]);

	if ( aux._type !== "numeric" ) {
		throw createErrorUnsupportedFeature( "`" + aux._type + "` numbering system" );
	}

	return aux._digits;
};




/**
 * EBNF representation:
 *
 * number_pattern_re =        prefix?
 *                            padding?
 *                            (integer_fraction_pattern | significant_pattern)
 *                            scientific_notation?
 *                            suffix?
 *
 * prefix =                   non_number_stuff
 *
 * padding =                  "*" regexp(.)
 *
 * integer_fraction_pattern = integer_pattern
 *                            fraction_pattern?
 *
 * integer_pattern =          regexp([#,]*[0,]*0+)
 *
 * fraction_pattern =         "." regexp(0*[0-9]*#*)
 *
 * significant_pattern =      regexp([#,]*@+#*)
 *
 * scientific_notation =      regexp(E\+?0+)
 *
 * suffix =                   non_number_stuff
 *
 * non_number_stuff =         regexp(('[^']+'|''|[^*#@0,.E])*)
 *
 *
 * Regexp groups:
 *
 *  0: number_pattern_re
 *  1: prefix
 *  2: -
 *  3: padding
 *  4: (integer_fraction_pattern | significant_pattern)
 *  5: integer_fraction_pattern
 *  6: integer_pattern
 *  7: fraction_pattern
 *  8: significant_pattern
 *  9: scientific_notation
 * 10: suffix
 * 11: -
 */
var numberPatternRe = (/^(('[^']+'|''|[^*#@0,.E])*)(\*.)?((([#,]*[0,]*0+)(\.0*[0-9]*#*)?)|([#,]*@+#*))(E\+?0+)?(('[^']+'|''|[^*#@0,.E])*)$/);




/**
 * format( number, pattern )
 *
 * @number [Number].
 *
 * @pattern [String] raw pattern for numbers.
 *
 * Return the formatted number.
 * ref: http://www.unicode.org/reports/tr35/tr35-numbers.html
 */
var numberPatternProperties = function( pattern ) {
	var aux1, aux2, fractionPattern, integerFractionOrSignificantPattern, integerPattern,
		maximumFractionDigits, maximumSignificantDigits, minimumFractionDigits,
		minimumIntegerDigits, minimumSignificantDigits, padding, prefix, primaryGroupingSize,
		roundIncrement, scientificNotation, secondaryGroupingSize, significantPattern, suffix;

	pattern = pattern.match( numberPatternRe );
	if ( !pattern ) {
		throw new Error( "Invalid pattern: " + pattern );
	}

	prefix = pattern[ 1 ];
	padding = pattern[ 3 ];
	integerFractionOrSignificantPattern = pattern[ 4 ];
	significantPattern = pattern[ 8 ];
	scientificNotation = pattern[ 9 ];
	suffix = pattern[ 10 ];

	// Significant digit format
	if ( significantPattern ) {
		significantPattern.replace( /(@+)(#*)/, function( match, minimumSignificantDigitsMatch, maximumSignificantDigitsMatch ) {
			minimumSignificantDigits = minimumSignificantDigitsMatch.length;
			maximumSignificantDigits = minimumSignificantDigits +
				maximumSignificantDigitsMatch.length;
		});

	// Integer and fractional format
	} else {
		fractionPattern = pattern[ 7 ];
		integerPattern = pattern[ 6 ];

		if ( fractionPattern ) {

			// Minimum fraction digits, and rounding.
			fractionPattern.replace( /[0-9]+/, function( match ) {
				minimumFractionDigits = match;
			});
			if ( minimumFractionDigits ) {
				roundIncrement = +( "0." + minimumFractionDigits );
				minimumFractionDigits = minimumFractionDigits.length;
			} else {
				minimumFractionDigits = 0;
			}

			// Maximum fraction digits
			// 1: ignore decimal character
			maximumFractionDigits = fractionPattern.length - 1 /* 1 */;
		}

		// Minimum integer digits
		integerPattern.replace( /0+$/, function( match ) {
			minimumIntegerDigits = match.length;
		});
	}

	// Scientific notation
	if ( scientificNotation ) {
		throw createErrorUnsupportedFeature({
			feature: "scientific notation (not implemented)"
		});
	}

	// Padding
	if ( padding ) {
		throw createErrorUnsupportedFeature({
			feature: "padding (not implemented)"
		});
	}

	// Grouping
	if ( ( aux1 = integerFractionOrSignificantPattern.lastIndexOf( "," ) ) !== -1 ) {

		// Primary grouping size is the interval between the last group separator and the end of
		// the integer (or the end of the significant pattern).
		aux2 = integerFractionOrSignificantPattern.split( "." )[ 0 ];
		primaryGroupingSize = aux2.length - aux1 - 1;

		// Secondary grouping size is the interval between the last two group separators.
		if ( ( aux2 = integerFractionOrSignificantPattern.lastIndexOf( ",", aux1 - 1 ) ) !== -1 ) {
			secondaryGroupingSize = aux1 - 1 - aux2;
		}
	}

	// Return:
	//  0: @prefix String
	//  1: @padding Array [ <character>, <count> ] TODO
	//  2: @minimumIntegerDigits non-negative integer Number value indicating the minimum integer
	//        digits to be used. Numbers will be padded with leading zeroes if necessary.
	//  3: @minimumFractionDigits and
	//  4: @maximumFractionDigits are non-negative integer Number values indicating the minimum and
	//        maximum fraction digits to be used. Numbers will be rounded or padded with trailing
	//        zeroes if necessary.
	//  5: @minimumSignificantDigits and
	//  6: @maximumSignificantDigits are positive integer Number values indicating the minimum and
	//        maximum fraction digits to be shown. Either none or both of these properties are
	//        present; if they are, they override minimum and maximum integer and fraction digits
	//        – the formatter uses however many integer and fraction digits are required to display
	//        the specified number of significant digits.
	//  7: @roundIncrement Decimal round increment or null
	//  8: @primaryGroupingSize
	//  9: @secondaryGroupingSize
	// 10: @suffix String
	return [
		prefix,
		padding,
		minimumIntegerDigits,
		minimumFractionDigits,
		maximumFractionDigits,
		minimumSignificantDigits,
		maximumSignificantDigits,
		roundIncrement,
		primaryGroupingSize,
		secondaryGroupingSize,
		suffix
	];
};




/**
 * Symbol( name, cldr )
 *
 * @name [String] Symbol name.
 *
 * @cldr [Cldr instance].
 *
 * Return the localized symbol given its name.
 */
var numberSymbol = function( name, cldr ) {
	return cldr.main([
		"numbers/symbols-numberSystem-" + numberNumberingSystem( cldr ),
		name
	]);
};




var numberSymbolName = {
	".": "decimal",
	",": "group",
	"%": "percentSign",
	"+": "plusSign",
	"-": "minusSign",
	"E": "exponential",
	"\u2030": "perMille"
};




/**
 * symbolMap( cldr )
 *
 * @cldr [Cldr instance].
 *
 * Return the (localized symbol, pattern symbol) key value pair, eg. {
 *   ".": "٫",
 *   ",": "٬",
 *   "%": "٪",
 *   ...
 * };
 */
var numberSymbolMap = function( cldr ) {
	var symbol,
		symbolMap = {};

	for ( symbol in numberSymbolName ) {
		symbolMap[ symbol ] = numberSymbol( numberSymbolName[ symbol ], cldr );
	}

	return symbolMap;
};




var numberTruncate = function( value ) {
	if ( isNaN( value ) ) {
		return NaN;
	}
	return Math[ value < 0 ? "ceil" : "floor" ]( value );
};




/**
 * round( method )
 *
 * @method [String] with either "round", "ceil", "floor", or "truncate".
 *
 * Return function( value, incrementOrExp ):
 *
 *   @value [Number] eg. 123.45.
 *
 *   @incrementOrExp [Number] optional, eg. 0.1; or
 *     [Object] Either { increment: <value> } or { exponent: <value> }
 *
 *   Return the rounded number, eg:
 *   - round( "round" )( 123.45 ): 123;
 *   - round( "ceil" )( 123.45 ): 124;
 *   - round( "floor" )( 123.45 ): 123;
 *   - round( "truncate" )( 123.45 ): 123;
 *   - round( "round" )( 123.45, 0.1 ): 123.5;
 *   - round( "round" )( 123.45, 10 ): 120;
 *
 *   Based on https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/round
 *   Ref: #376
 */
var numberRound = function( method ) {
	method = method || "round";
	method = method === "truncate" ? numberTruncate : Math[ method ];

	return function( value, incrementOrExp ) {
		var exp, increment;

		value = +value;

		// If the value is not a number, return NaN.
		if ( isNaN( value ) ) {
			return NaN;
		}

		// Exponent given.
		if ( typeof incrementOrExp === "object" && incrementOrExp.exponent ) {
			exp = +incrementOrExp.exponent;
			increment = 1;

			if ( exp === 0 ) {
				return method( value );
			}

			// If the exp is not an integer, return NaN.
			if ( !( typeof exp === "number" && exp % 1 === 0 ) ) {
				return NaN;
			}

		// Increment given.
		} else {
			increment = +incrementOrExp || 1;

			if ( increment === 1 ) {
				return method( value );
			}

			// If the increment is not a number, return NaN.
			if ( isNaN( increment ) ) {
				return NaN;
			}

			increment = increment.toExponential().split( "e" );
			exp = +increment[ 1 ];
			increment = +increment[ 0 ];
		}

		// Shift & Round
		value = value.toString().split( "e" );
		value[ 0 ] = +value[ 0 ] / increment;
		value[ 1 ] = value[ 1 ] ? ( +value[ 1 ] - exp ) : -exp;
		value = method( +(value[ 0 ] + "e" + value[ 1 ] ) );

		// Shift back
		value = value.toString().split( "e" );
		value[ 0 ] = +value[ 0 ] * increment;
		value[ 1 ] = value[ 1 ] ? ( +value[ 1 ] + exp ) : exp;
		return +( value[ 0 ] + "e" + value[ 1 ] );
	};
};




/**
 * formatProperties( pattern, cldr [, options] )
 *
 * @pattern [String] raw pattern for numbers.
 *
 * @cldr [Cldr instance].
 *
 * @options [Object]:
 * - minimumIntegerDigits [Number]
 * - minimumFractionDigits, maximumFractionDigits [Number]
 * - minimumSignificantDigits, maximumSignificantDigits [Number]
 * - round [String] "ceil", "floor", "round" (default), or "truncate".
 * - useGrouping [Boolean] default true.
 *
 * Return the processed properties that will be used in number/format.
 * ref: http://www.unicode.org/reports/tr35/tr35-numbers.html
 */
var numberFormatProperties = function( pattern, cldr, options ) {
	var negativePattern, negativePrefix, negativeProperties, negativeSuffix, positivePattern,
		properties;

	function getOptions( attribute, propertyIndex ) {
		if ( attribute in options ) {
			properties[ propertyIndex ] = options[ attribute ];
		}
	}

	options = options || {};
	pattern = pattern.split( ";" );

	positivePattern = pattern[ 0 ];

	negativePattern = pattern[ 1 ] || "-" + positivePattern;
	negativeProperties = numberPatternProperties( negativePattern );
	negativePrefix = negativeProperties[ 0 ];
	negativeSuffix = negativeProperties[ 10 ];

	properties = numberPatternProperties( positivePattern ).concat([
		positivePattern,
		negativePrefix + positivePattern + negativeSuffix,
		negativePrefix,
		negativeSuffix,
		numberRound( options.round ),
		numberSymbol( "infinity", cldr ),
		numberSymbol( "nan", cldr ),
		numberSymbolMap( cldr ),
		numberNumberingSystemDigitsMap( cldr )
	]);

	getOptions( "minimumIntegerDigits", 2 );
	getOptions( "minimumFractionDigits", 3 );
	getOptions( "maximumFractionDigits", 4 );
	getOptions( "minimumSignificantDigits", 5 );
	getOptions( "maximumSignificantDigits", 6 );

	// Grouping separators
	if ( options.useGrouping === false ) {
		properties[ 8 ] = null;
	}

	// Normalize number of digits if only one of either minimumFractionDigits or
	// maximumFractionDigits is passed in as an option
	if ( "minimumFractionDigits" in options && !( "maximumFractionDigits" in options ) ) {
		// maximumFractionDigits = Math.max( minimumFractionDigits, maximumFractionDigits );
		properties[ 4 ] = Math.max( properties[ 3 ], properties[ 4 ] );
	} else if ( !( "minimumFractionDigits" in options ) &&
			"maximumFractionDigits" in options ) {
		// minimumFractionDigits = Math.min( minimumFractionDigits, maximumFractionDigits );
		properties[ 3 ] = Math.min( properties[ 3 ], properties[ 4 ] );
	}

	// Return:
	// 0-10: see number/pattern-properties.
	// 11: @positivePattern [String] Positive pattern.
	// 12: @negativePattern [String] Negative pattern.
	// 13: @negativePrefix [String] Negative prefix.
	// 14: @negativeSuffix [String] Negative suffix.
	// 15: @round [Function] Round function.
	// 16: @infinitySymbol [String] Infinity symbol.
	// 17: @nanSymbol [String] NaN symbol.
	// 18: @symbolMap [Object] A bunch of other symbols.
	// 19: @nuDigitsMap [Array] Digits map if numbering system is different than `latn`.
	return properties;
};




/**
 * EBNF representation:
 *
 * number_pattern_re =        prefix_including_padding?
 *                            number
 *                            scientific_notation?
 *                            suffix?
 *
 * number =                   integer_including_group_separator fraction_including_decimal_separator
 *
 * integer_including_group_separator =
 *                            regexp([0-9,]*[0-9]+)
 *
 * fraction_including_decimal_separator =
 *                            regexp((\.[0-9]+)?)

 * prefix_including_padding = non_number_stuff
 *
 * scientific_notation =      regexp(E[+-]?[0-9]+)
 *
 * suffix =                   non_number_stuff
 *
 * non_number_stuff =         regexp([^0-9]*)
 *
 *
 * Regexp groups:
 *
 * 0: number_pattern_re
 * 1: prefix
 * 2: integer_including_group_separator fraction_including_decimal_separator
 * 3: integer_including_group_separator
 * 4: fraction_including_decimal_separator
 * 5: scientific_notation
 * 6: suffix
 */
var numberNumberRe = (/^([^0-9]*)(([0-9,]*[0-9]+)(\.[0-9]+)?)(E[+-]?[0-9]+)?([^0-9]*)$/);




/**
 * parse( value, properties )
 *
 * @value [String].
 *
 * @properties [Object] Parser properties is a reduced pre-processed cldr
 * data set returned by numberParserProperties().
 *
 * Return the parsed Number (including Infinity) or NaN when value is invalid.
 * ref: http://www.unicode.org/reports/tr35/tr35-numbers.html
 */
var numberParse = function( value, properties ) {
	var aux, infinitySymbol, invertedNuDigitsMap, invertedSymbolMap, localizedDigitRe,
		localizedSymbolsRe, negativePrefix, negativeSuffix, number, prefix, suffix;

	infinitySymbol = properties[ 0 ];
	invertedSymbolMap = properties[ 1 ];
	negativePrefix = properties[ 2 ];
	negativeSuffix = properties[ 3 ];
	invertedNuDigitsMap = properties[ 4 ];

	// Infinite number.
	if ( aux = value.match( infinitySymbol ) ) {

		number = Infinity;
		prefix = value.slice( 0, aux.length );
		suffix = value.slice( aux.length + 1 );

	// Finite number.
	} else {

		// TODO: Create it during setup, i.e., make it a property.
		localizedSymbolsRe = new RegExp(
			Object.keys( invertedSymbolMap ).map(function( localizedSymbol ) {
				return regexpEscape( localizedSymbol );
			}).join( "|" ),
			"g"
		);

		// Reverse localized symbols.
		value = value.replace( localizedSymbolsRe, function( localizedSymbol ) {
			return invertedSymbolMap[ localizedSymbol ];
		});

		// Reverse localized numbering system.
		if ( invertedNuDigitsMap ) {

			// TODO: Create it during setup, i.e., make it a property.
			localizedDigitRe = new RegExp(
				Object.keys( invertedNuDigitsMap ).map(function( localizedDigit ) {
					return regexpEscape( localizedDigit );
				}).join( "|" ),
				"g"
			);
			value = value.replace( localizedDigitRe, function( localizedDigit ) {
				return invertedNuDigitsMap[ localizedDigit ];
			});
		}

		// Is it a valid number?
		value = value.match( numberNumberRe );
		if ( !value ) {

			// Invalid number.
			return NaN;
		}

		prefix = value[ 1 ];
		suffix = value[ 6 ];

		// Remove grouping separators.
		number = value[ 2 ].replace( /,/g, "" );

		// Scientific notation
		if ( value[ 5 ] ) {
			number += value[ 5 ];
		}

		number = +number;

		// Is it a valid number?
		if ( isNaN( number ) ) {

			// Invalid number.
			return NaN;
		}

		// Percent
		if ( value[ 0 ].indexOf( "%" ) !== -1 ) {
			number /= 100;
			suffix = suffix.replace( "%", "" );

		// Per mille
		} else if ( value[ 0 ].indexOf( "\u2030" ) !== -1 ) {
			number /= 1000;
			suffix = suffix.replace( "\u2030", "" );
		}
	}

	// Negative number
	// "If there is an explicit negative subpattern, it serves only to specify the negative prefix
	// and suffix. If there is no explicit negative subpattern, the negative subpattern is the
	// localized minus sign prefixed to the positive subpattern" UTS#35
	if ( prefix === negativePrefix && suffix === negativeSuffix ) {
		number *= -1;
	}

	return number;
};




/**
 * symbolMap( cldr )
 *
 * @cldr [Cldr instance].
 *
 * Return the (localized symbol, pattern symbol) key value pair, eg. {
 *   "٫": ".",
 *   "٬": ",",
 *   "٪": "%",
 *   ...
 * };
 */
var numberSymbolInvertedMap = function( cldr ) {
	var symbol,
		symbolMap = {};

	for ( symbol in numberSymbolName ) {
		symbolMap[ numberSymbol( numberSymbolName[ symbol ], cldr ) ] = symbol;
	}

	return symbolMap;
};




/**
 * parseProperties( pattern, cldr )
 *
 * @pattern [String] raw pattern for numbers.
 *
 * @cldr [Cldr instance].
 *
 * Return parser properties, used to feed parser function.
 */
var numberParseProperties = function( pattern, cldr ) {
	var invertedNuDigitsMap, invertedNuDigitsMapSanityCheck, negativePattern, negativeProperties,
		nuDigitsMap = numberNumberingSystemDigitsMap( cldr );

	pattern = pattern.split( ";" );
	negativePattern = pattern[ 1 ] || "-" + pattern[ 0 ];
	negativeProperties = numberPatternProperties( negativePattern );
	if ( nuDigitsMap ) {
		invertedNuDigitsMap = nuDigitsMap.split( "" ).reduce(function( object, localizedDigit, i ) {
			object[ localizedDigit ] = String( i );
			return object;
		}, {} );
		invertedNuDigitsMapSanityCheck = "0123456789".split( "" ).reduce(function( object, digit ) {
			object[ digit ] = "invalid";
			return object;
		}, {} );
		invertedNuDigitsMap = objectExtend(
			invertedNuDigitsMapSanityCheck,
			invertedNuDigitsMap
		);
	}

	// 0: @infinitySymbol [String] Infinity symbol.
	// 1: @invertedSymbolMap [Object] Inverted symbol map augmented with sanity check.
	//    The sanity check prevents permissive parsing, i.e., it prevents symbols that doesn't
	//    belong to the localized set to pass through. This is obtained with the result of the
	//    inverted map object overloading symbol name map object (the remaining symbol name
	//    mappings will invalidate parsing, working as the sanity check).
	// 2: @negativePrefix [String] Negative prefix.
	// 3: @negativeSuffix [String] Negative suffix with percent or per mille stripped out.
	// 4: @invertedNuDigitsMap [Object] Inverted digits map if numbering system is different than
	//    `latn` augmented with sanity check (similar to invertedSymbolMap).
	return [
		numberSymbol( "infinity", cldr ),
		objectExtend( {}, numberSymbolName, numberSymbolInvertedMap( cldr ) ),
		negativeProperties[ 0 ],
		negativeProperties[ 10 ].replace( "%", "" ).replace( "\u2030", "" ),
		invertedNuDigitsMap
	];
};




/**
 * Pattern( style )
 *
 * @style [String] "decimal" (default) or "percent".
 *
 * @cldr [Cldr instance].
 */
var numberPattern = function( style, cldr ) {
	if ( style !== "decimal" && style !== "percent" ) {
		throw new Error( "Invalid style" );
	}

	return cldr.main([
		"numbers",
		style + "Formats-numberSystem-" + numberNumberingSystem( cldr ),
		"standard"
	]);
};




/**
 * .numberFormatter( [options] )
 *
 * @options [Object]:
 * - style: [String] "decimal" (default) or "percent".
 * - see also number/format options.
 *
 * Return a function that formats a number according to the given options and default/instance
 * locale.
 */
Globalize.numberFormatter =
Globalize.prototype.numberFormatter = function( options ) {
	var cldr, maximumFractionDigits, maximumSignificantDigits, minimumFractionDigits,
		minimumIntegerDigits, minimumSignificantDigits, pattern, properties;

	validateParameterTypePlainObject( options, "options" );

	options = options || {};
	cldr = this.cldr;

	validateDefaultLocale( cldr );

	cldr.on( "get", validateCldr );

	if ( options.raw ) {
		pattern = options.raw;
	} else {
		pattern = numberPattern( options.style || "decimal", cldr );
	}

	properties = numberFormatProperties( pattern, cldr, options );

	cldr.off( "get", validateCldr );

	minimumIntegerDigits = properties[ 2 ];
	minimumFractionDigits = properties[ 3 ];
	maximumFractionDigits = properties[ 4 ];

	minimumSignificantDigits = properties[ 5 ];
	maximumSignificantDigits = properties[ 6 ];

	// Validate significant digit format properties
	if ( !isNaN( minimumSignificantDigits * maximumSignificantDigits ) ) {
		validateParameterRange( minimumSignificantDigits, "minimumSignificantDigits", 1, 21 );
		validateParameterRange( maximumSignificantDigits, "maximumSignificantDigits",
			minimumSignificantDigits, 21 );

	} else if ( !isNaN( minimumSignificantDigits ) || !isNaN( maximumSignificantDigits ) ) {
		throw new Error( "Neither or both the minimum and maximum significant digits must be " +
			"present" );

	// Validate integer and fractional format
	} else {
		validateParameterRange( minimumIntegerDigits, "minimumIntegerDigits", 1, 21 );
		validateParameterRange( minimumFractionDigits, "minimumFractionDigits", 0, 20 );
		validateParameterRange( maximumFractionDigits, "maximumFractionDigits",
			minimumFractionDigits, 20 );
	}

	return function( value ) {
		validateParameterPresence( value, "value" );
		validateParameterTypeNumber( value, "value" );
		return numberFormat( value, properties );
	};
};

/**
 * .numberParser( [options] )
 *
 * @options [Object]:
 * - style: [String] "decimal" (default) or "percent".
 *
 * Return the number parser according to the default/instance locale.
 */
Globalize.numberParser =
Globalize.prototype.numberParser = function( options ) {
	var cldr, pattern, properties;

	validateParameterTypePlainObject( options, "options" );

	options = options || {};
	cldr = this.cldr;

	validateDefaultLocale( cldr );

	cldr.on( "get", validateCldr );

	if ( options.raw ) {
		pattern = options.raw;
	} else {
		pattern = numberPattern( options.style || "decimal", cldr );
	}

	properties = numberParseProperties( pattern, cldr );

	cldr.off( "get", validateCldr );

	return function( value ) {
		validateParameterPresence( value, "value" );
		validateParameterTypeString( value, "value" );
		return numberParse( value, properties );
	};
};

/**
 * .formatNumber( value [, options] )
 *
 * @value [Number] number to be formatted.
 *
 * @options [Object]: see number/format-properties.
 *
 * Format a number according to the given options and default/instance locale.
 */
Globalize.formatNumber =
Globalize.prototype.formatNumber = function( value, options ) {
	validateParameterPresence( value, "value" );
	validateParameterTypeNumber( value, "value" );

	return this.numberFormatter( options )( value );
};

/**
 * .parseNumber( value [, options] )
 *
 * @value [String]
 *
 * @options [Object]: See numberParser().
 *
 * Return the parsed Number (including Infinity) or NaN when value is invalid.
 */
Globalize.parseNumber =
Globalize.prototype.parseNumber = function( value, options ) {
	validateParameterPresence( value, "value" );
	validateParameterTypeString( value, "value" );

	return this.numberParser( options )( value );
};

/**
 * Optimization to avoid duplicating some internal functions across modules.
 */
Globalize._createErrorUnsupportedFeature = createErrorUnsupportedFeature;
Globalize._numberNumberingSystem = numberNumberingSystem;
Globalize._numberPattern = numberPattern;
Globalize._numberSymbol = numberSymbol;
Globalize._stringPad = stringPad;
Globalize._validateParameterTypeNumber = validateParameterTypeNumber;
Globalize._validateParameterTypeString = validateParameterTypeString;

return Globalize;




}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImpxdWVyeS52YWxpZGF0ZS5qcyIsImpxdWVyeS52YWxpZGF0ZS51bm9idHJ1c2l2ZS5qcyIsImpxdWVyeS52YWxpZGF0ZS51bm9idHJ1c2l2ZS5hZGRpdGlvbmFsLmpzIiwiY2xkci5qcyIsImV2ZW50LmpzIiwic3VwcGxlbWVudGFsLmpzIiwiZ2xvYmFsaXplLmpzIiwibnVtYmVyLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDN3RDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDelpBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDckJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUMzcEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDemtCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNyR0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ25XQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsImZpbGUiOiJMaWIuanMiLCJzb3VyY2VzQ29udGVudCI6WyIvKiBOVUdFVDogQkVHSU4gTElDRU5TRSBURVhUXG4gKlxuICogTWljcm9zb2Z0IGdyYW50cyB5b3UgdGhlIHJpZ2h0IHRvIHVzZSB0aGVzZSBzY3JpcHQgZmlsZXMgZm9yIHRoZSBzb2xlXG4gKiBwdXJwb3NlIG9mIGVpdGhlcjogKGkpIGludGVyYWN0aW5nIHRocm91Z2ggeW91ciBicm93c2VyIHdpdGggdGhlIE1pY3Jvc29mdFxuICogd2Vic2l0ZSBvciBvbmxpbmUgc2VydmljZSwgc3ViamVjdCB0byB0aGUgYXBwbGljYWJsZSBsaWNlbnNpbmcgb3IgdXNlXG4gKiB0ZXJtczsgb3IgKGlpKSB1c2luZyB0aGUgZmlsZXMgYXMgaW5jbHVkZWQgd2l0aCBhIE1pY3Jvc29mdCBwcm9kdWN0IHN1YmplY3RcbiAqIHRvIHRoYXQgcHJvZHVjdCdzIGxpY2Vuc2UgdGVybXMuIE1pY3Jvc29mdCByZXNlcnZlcyBhbGwgb3RoZXIgcmlnaHRzIHRvIHRoZVxuICogZmlsZXMgbm90IGV4cHJlc3NseSBncmFudGVkIGJ5IE1pY3Jvc29mdCwgd2hldGhlciBieSBpbXBsaWNhdGlvbiwgZXN0b3BwZWxcbiAqIG9yIG90aGVyd2lzZS4gSW5zb2ZhciBhcyBhIHNjcmlwdCBmaWxlIGlzIGR1YWwgbGljZW5zZWQgdW5kZXIgR1BMLFxuICogTWljcm9zb2Z0IG5laXRoZXIgdG9vayB0aGUgY29kZSB1bmRlciBHUEwgbm9yIGRpc3RyaWJ1dGVzIGl0IHRoZXJldW5kZXIgYnV0XG4gKiB1bmRlciB0aGUgdGVybXMgc2V0IG91dCBpbiB0aGlzIHBhcmFncmFwaC4gQWxsIG5vdGljZXMgYW5kIGxpY2Vuc2VzXG4gKiBiZWxvdyBhcmUgZm9yIGluZm9ybWF0aW9uYWwgcHVycG9zZXMgb25seS5cbiAqXG4gKiBOVUdFVDogRU5EIExJQ0VOU0UgVEVYVCAqL1xuLyohXG4gKiBqUXVlcnkgVmFsaWRhdGlvbiBQbHVnaW4gMS4xMS4xXG4gKlxuICogaHR0cDovL2Jhc3Npc3RhbmNlLmRlL2pxdWVyeS1wbHVnaW5zL2pxdWVyeS1wbHVnaW4tdmFsaWRhdGlvbi9cbiAqIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uXG4gKlxuICogQ29weXJpZ2h0IDIwMTMgSsO2cm4gWmFlZmZlcmVyXG4gKiBSZWxlYXNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2U6XG4gKiAgIGh0dHA6Ly93d3cub3BlbnNvdXJjZS5vcmcvbGljZW5zZXMvbWl0LWxpY2Vuc2UucGhwXG4gKi9cblxuKGZ1bmN0aW9uKCQpIHtcblxuJC5leHRlbmQoJC5mbiwge1xuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi92YWxpZGF0ZVxuXHR2YWxpZGF0ZTogZnVuY3Rpb24oIG9wdGlvbnMgKSB7XG5cblx0XHQvLyBpZiBub3RoaW5nIGlzIHNlbGVjdGVkLCByZXR1cm4gbm90aGluZzsgY2FuJ3QgY2hhaW4gYW55d2F5XG5cdFx0aWYgKCAhdGhpcy5sZW5ndGggKSB7XG5cdFx0XHRpZiAoIG9wdGlvbnMgJiYgb3B0aW9ucy5kZWJ1ZyAmJiB3aW5kb3cuY29uc29sZSApIHtcblx0XHRcdFx0Y29uc29sZS53YXJuKCBcIk5vdGhpbmcgc2VsZWN0ZWQsIGNhbid0IHZhbGlkYXRlLCByZXR1cm5pbmcgbm90aGluZy5cIiApO1xuXHRcdFx0fVxuXHRcdFx0cmV0dXJuO1xuXHRcdH1cblxuXHRcdC8vIGNoZWNrIGlmIGEgdmFsaWRhdG9yIGZvciB0aGlzIGZvcm0gd2FzIGFscmVhZHkgY3JlYXRlZFxuXHRcdHZhciB2YWxpZGF0b3IgPSAkLmRhdGEoIHRoaXNbMF0sIFwidmFsaWRhdG9yXCIgKTtcblx0XHRpZiAoIHZhbGlkYXRvciApIHtcblx0XHRcdHJldHVybiB2YWxpZGF0b3I7XG5cdFx0fVxuXG5cdFx0Ly8gQWRkIG5vdmFsaWRhdGUgdGFnIGlmIEhUTUw1LlxuXHRcdHRoaXMuYXR0ciggXCJub3ZhbGlkYXRlXCIsIFwibm92YWxpZGF0ZVwiICk7XG5cblx0XHR2YWxpZGF0b3IgPSBuZXcgJC52YWxpZGF0b3IoIG9wdGlvbnMsIHRoaXNbMF0gKTtcblx0XHQkLmRhdGEoIHRoaXNbMF0sIFwidmFsaWRhdG9yXCIsIHZhbGlkYXRvciApO1xuXG5cdFx0aWYgKCB2YWxpZGF0b3Iuc2V0dGluZ3Mub25zdWJtaXQgKSB7XG5cblx0XHRcdHRoaXMudmFsaWRhdGVEZWxlZ2F0ZSggXCI6c3VibWl0XCIsIFwiY2xpY2tcIiwgZnVuY3Rpb24oIGV2ZW50ICkge1xuXHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5ncy5zdWJtaXRIYW5kbGVyICkge1xuXHRcdFx0XHRcdHZhbGlkYXRvci5zdWJtaXRCdXR0b24gPSBldmVudC50YXJnZXQ7XG5cdFx0XHRcdH1cblx0XHRcdFx0Ly8gYWxsb3cgc3VwcHJlc3NpbmcgdmFsaWRhdGlvbiBieSBhZGRpbmcgYSBjYW5jZWwgY2xhc3MgdG8gdGhlIHN1Ym1pdCBidXR0b25cblx0XHRcdFx0aWYgKCAkKGV2ZW50LnRhcmdldCkuaGFzQ2xhc3MoXCJjYW5jZWxcIikgKSB7XG5cdFx0XHRcdFx0dmFsaWRhdG9yLmNhbmNlbFN1Ym1pdCA9IHRydWU7XG5cdFx0XHRcdH1cblxuXHRcdFx0XHQvLyBhbGxvdyBzdXBwcmVzc2luZyB2YWxpZGF0aW9uIGJ5IGFkZGluZyB0aGUgaHRtbDUgZm9ybW5vdmFsaWRhdGUgYXR0cmlidXRlIHRvIHRoZSBzdWJtaXQgYnV0dG9uXG5cdFx0XHRcdGlmICggJChldmVudC50YXJnZXQpLmF0dHIoXCJmb3Jtbm92YWxpZGF0ZVwiKSAhPT0gdW5kZWZpbmVkICkge1xuXHRcdFx0XHRcdHZhbGlkYXRvci5jYW5jZWxTdWJtaXQgPSB0cnVlO1xuXHRcdFx0XHR9XG5cdFx0XHR9KTtcblxuXHRcdFx0Ly8gdmFsaWRhdGUgdGhlIGZvcm0gb24gc3VibWl0XG5cdFx0XHR0aGlzLnN1Ym1pdCggZnVuY3Rpb24oIGV2ZW50ICkge1xuXHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5ncy5kZWJ1ZyApIHtcblx0XHRcdFx0XHQvLyBwcmV2ZW50IGZvcm0gc3VibWl0IHRvIGJlIGFibGUgdG8gc2VlIGNvbnNvbGUgb3V0cHV0XG5cdFx0XHRcdFx0ZXZlbnQucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0fVxuXHRcdFx0XHRmdW5jdGlvbiBoYW5kbGUoKSB7XG5cdFx0XHRcdFx0dmFyIGhpZGRlbjtcblx0XHRcdFx0XHRpZiAoIHZhbGlkYXRvci5zZXR0aW5ncy5zdWJtaXRIYW5kbGVyICkge1xuXHRcdFx0XHRcdFx0aWYgKCB2YWxpZGF0b3Iuc3VibWl0QnV0dG9uICkge1xuXHRcdFx0XHRcdFx0XHQvLyBpbnNlcnQgYSBoaWRkZW4gaW5wdXQgYXMgYSByZXBsYWNlbWVudCBmb3IgdGhlIG1pc3Npbmcgc3VibWl0IGJ1dHRvblxuXHRcdFx0XHRcdFx0XHRoaWRkZW4gPSAkKFwiPGlucHV0IHR5cGU9J2hpZGRlbicvPlwiKS5hdHRyKFwibmFtZVwiLCB2YWxpZGF0b3Iuc3VibWl0QnV0dG9uLm5hbWUpLnZhbCggJCh2YWxpZGF0b3Iuc3VibWl0QnV0dG9uKS52YWwoKSApLmFwcGVuZFRvKHZhbGlkYXRvci5jdXJyZW50Rm9ybSk7XG5cdFx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0XHR2YWxpZGF0b3Iuc2V0dGluZ3Muc3VibWl0SGFuZGxlci5jYWxsKCB2YWxpZGF0b3IsIHZhbGlkYXRvci5jdXJyZW50Rm9ybSwgZXZlbnQgKTtcblx0XHRcdFx0XHRcdGlmICggdmFsaWRhdG9yLnN1Ym1pdEJ1dHRvbiApIHtcblx0XHRcdFx0XHRcdFx0Ly8gYW5kIGNsZWFuIHVwIGFmdGVyd2FyZHM7IHRoYW5rcyB0byBuby1ibG9jay1zY29wZSwgaGlkZGVuIGNhbiBiZSByZWZlcmVuY2VkXG5cdFx0XHRcdFx0XHRcdGhpZGRlbi5yZW1vdmUoKTtcblx0XHRcdFx0XHRcdH1cblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0cmV0dXJuIHRydWU7XG5cdFx0XHRcdH1cblxuXHRcdFx0XHQvLyBwcmV2ZW50IHN1Ym1pdCBmb3IgaW52YWxpZCBmb3JtcyBvciBjdXN0b20gc3VibWl0IGhhbmRsZXJzXG5cdFx0XHRcdGlmICggdmFsaWRhdG9yLmNhbmNlbFN1Ym1pdCApIHtcblx0XHRcdFx0XHR2YWxpZGF0b3IuY2FuY2VsU3VibWl0ID0gZmFsc2U7XG5cdFx0XHRcdFx0cmV0dXJuIGhhbmRsZSgpO1xuXHRcdFx0XHR9XG5cdFx0XHRcdGlmICggdmFsaWRhdG9yLmZvcm0oKSApIHtcblx0XHRcdFx0XHRpZiAoIHZhbGlkYXRvci5wZW5kaW5nUmVxdWVzdCApIHtcblx0XHRcdFx0XHRcdHZhbGlkYXRvci5mb3JtU3VibWl0dGVkID0gdHJ1ZTtcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0cmV0dXJuIGhhbmRsZSgpO1xuXHRcdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRcdHZhbGlkYXRvci5mb2N1c0ludmFsaWQoKTtcblx0XHRcdFx0XHRyZXR1cm4gZmFsc2U7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXHRcdH1cblxuXHRcdHJldHVybiB2YWxpZGF0b3I7XG5cdH0sXG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL3ZhbGlkXG5cdHZhbGlkOiBmdW5jdGlvbigpIHtcblx0XHRpZiAoICQodGhpc1swXSkuaXMoXCJmb3JtXCIpKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy52YWxpZGF0ZSgpLmZvcm0oKTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0dmFyIHZhbGlkID0gdHJ1ZTtcblx0XHRcdHZhciB2YWxpZGF0b3IgPSAkKHRoaXNbMF0uZm9ybSkudmFsaWRhdGUoKTtcblx0XHRcdHRoaXMuZWFjaChmdW5jdGlvbigpIHtcblx0XHRcdFx0dmFsaWQgPSB2YWxpZCAmJiB2YWxpZGF0b3IuZWxlbWVudCh0aGlzKTtcblx0XHRcdH0pO1xuXHRcdFx0cmV0dXJuIHZhbGlkO1xuXHRcdH1cblx0fSxcblx0Ly8gYXR0cmlidXRlczogc3BhY2Ugc2VwZXJhdGVkIGxpc3Qgb2YgYXR0cmlidXRlcyB0byByZXRyaWV2ZSBhbmQgcmVtb3ZlXG5cdHJlbW92ZUF0dHJzOiBmdW5jdGlvbiggYXR0cmlidXRlcyApIHtcblx0XHR2YXIgcmVzdWx0ID0ge30sXG5cdFx0XHQkZWxlbWVudCA9IHRoaXM7XG5cdFx0JC5lYWNoKGF0dHJpYnV0ZXMuc3BsaXQoL1xccy8pLCBmdW5jdGlvbiggaW5kZXgsIHZhbHVlICkge1xuXHRcdFx0cmVzdWx0W3ZhbHVlXSA9ICRlbGVtZW50LmF0dHIodmFsdWUpO1xuXHRcdFx0JGVsZW1lbnQucmVtb3ZlQXR0cih2YWx1ZSk7XG5cdFx0fSk7XG5cdFx0cmV0dXJuIHJlc3VsdDtcblx0fSxcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vcnVsZXNcblx0cnVsZXM6IGZ1bmN0aW9uKCBjb21tYW5kLCBhcmd1bWVudCApIHtcblx0XHR2YXIgZWxlbWVudCA9IHRoaXNbMF07XG5cblx0XHRpZiAoIGNvbW1hbmQgKSB7XG5cdFx0XHR2YXIgc2V0dGluZ3MgPSAkLmRhdGEoZWxlbWVudC5mb3JtLCBcInZhbGlkYXRvclwiKS5zZXR0aW5ncztcblx0XHRcdHZhciBzdGF0aWNSdWxlcyA9IHNldHRpbmdzLnJ1bGVzO1xuXHRcdFx0dmFyIGV4aXN0aW5nUnVsZXMgPSAkLnZhbGlkYXRvci5zdGF0aWNSdWxlcyhlbGVtZW50KTtcblx0XHRcdHN3aXRjaChjb21tYW5kKSB7XG5cdFx0XHRjYXNlIFwiYWRkXCI6XG5cdFx0XHRcdCQuZXh0ZW5kKGV4aXN0aW5nUnVsZXMsICQudmFsaWRhdG9yLm5vcm1hbGl6ZVJ1bGUoYXJndW1lbnQpKTtcblx0XHRcdFx0Ly8gcmVtb3ZlIG1lc3NhZ2VzIGZyb20gcnVsZXMsIGJ1dCBhbGxvdyB0aGVtIHRvIGJlIHNldCBzZXBhcmV0ZWx5XG5cdFx0XHRcdGRlbGV0ZSBleGlzdGluZ1J1bGVzLm1lc3NhZ2VzO1xuXHRcdFx0XHRzdGF0aWNSdWxlc1tlbGVtZW50Lm5hbWVdID0gZXhpc3RpbmdSdWxlcztcblx0XHRcdFx0aWYgKCBhcmd1bWVudC5tZXNzYWdlcyApIHtcblx0XHRcdFx0XHRzZXR0aW5ncy5tZXNzYWdlc1tlbGVtZW50Lm5hbWVdID0gJC5leHRlbmQoIHNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0sIGFyZ3VtZW50Lm1lc3NhZ2VzICk7XG5cdFx0XHRcdH1cblx0XHRcdFx0YnJlYWs7XG5cdFx0XHRjYXNlIFwicmVtb3ZlXCI6XG5cdFx0XHRcdGlmICggIWFyZ3VtZW50ICkge1xuXHRcdFx0XHRcdGRlbGV0ZSBzdGF0aWNSdWxlc1tlbGVtZW50Lm5hbWVdO1xuXHRcdFx0XHRcdHJldHVybiBleGlzdGluZ1J1bGVzO1xuXHRcdFx0XHR9XG5cdFx0XHRcdHZhciBmaWx0ZXJlZCA9IHt9O1xuXHRcdFx0XHQkLmVhY2goYXJndW1lbnQuc3BsaXQoL1xccy8pLCBmdW5jdGlvbiggaW5kZXgsIG1ldGhvZCApIHtcblx0XHRcdFx0XHRmaWx0ZXJlZFttZXRob2RdID0gZXhpc3RpbmdSdWxlc1ttZXRob2RdO1xuXHRcdFx0XHRcdGRlbGV0ZSBleGlzdGluZ1J1bGVzW21ldGhvZF07XG5cdFx0XHRcdH0pO1xuXHRcdFx0XHRyZXR1cm4gZmlsdGVyZWQ7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0dmFyIGRhdGEgPSAkLnZhbGlkYXRvci5ub3JtYWxpemVSdWxlcyhcblx0XHQkLmV4dGVuZChcblx0XHRcdHt9LFxuXHRcdFx0JC52YWxpZGF0b3IuY2xhc3NSdWxlcyhlbGVtZW50KSxcblx0XHRcdCQudmFsaWRhdG9yLmF0dHJpYnV0ZVJ1bGVzKGVsZW1lbnQpLFxuXHRcdFx0JC52YWxpZGF0b3IuZGF0YVJ1bGVzKGVsZW1lbnQpLFxuXHRcdFx0JC52YWxpZGF0b3Iuc3RhdGljUnVsZXMoZWxlbWVudClcblx0XHQpLCBlbGVtZW50KTtcblxuXHRcdC8vIG1ha2Ugc3VyZSByZXF1aXJlZCBpcyBhdCBmcm9udFxuXHRcdGlmICggZGF0YS5yZXF1aXJlZCApIHtcblx0XHRcdHZhciBwYXJhbSA9IGRhdGEucmVxdWlyZWQ7XG5cdFx0XHRkZWxldGUgZGF0YS5yZXF1aXJlZDtcblx0XHRcdGRhdGEgPSAkLmV4dGVuZCh7cmVxdWlyZWQ6IHBhcmFtfSwgZGF0YSk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIGRhdGE7XG5cdH1cbn0pO1xuXG4vLyBDdXN0b20gc2VsZWN0b3JzXG4kLmV4dGVuZCgkLmV4cHJbXCI6XCJdLCB7XG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL2JsYW5rXG5cdGJsYW5rOiBmdW5jdGlvbiggYSApIHsgcmV0dXJuICEkLnRyaW0oXCJcIiArICQoYSkudmFsKCkpOyB9LFxuXHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9maWxsZWRcblx0ZmlsbGVkOiBmdW5jdGlvbiggYSApIHsgcmV0dXJuICEhJC50cmltKFwiXCIgKyAkKGEpLnZhbCgpKTsgfSxcblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vdW5jaGVja2VkXG5cdHVuY2hlY2tlZDogZnVuY3Rpb24oIGEgKSB7IHJldHVybiAhJChhKS5wcm9wKFwiY2hlY2tlZFwiKTsgfVxufSk7XG5cbi8vIGNvbnN0cnVjdG9yIGZvciB2YWxpZGF0b3JcbiQudmFsaWRhdG9yID0gZnVuY3Rpb24oIG9wdGlvbnMsIGZvcm0gKSB7XG5cdHRoaXMuc2V0dGluZ3MgPSAkLmV4dGVuZCggdHJ1ZSwge30sICQudmFsaWRhdG9yLmRlZmF1bHRzLCBvcHRpb25zICk7XG5cdHRoaXMuY3VycmVudEZvcm0gPSBmb3JtO1xuXHR0aGlzLmluaXQoKTtcbn07XG5cbiQudmFsaWRhdG9yLmZvcm1hdCA9IGZ1bmN0aW9uKCBzb3VyY2UsIHBhcmFtcyApIHtcblx0aWYgKCBhcmd1bWVudHMubGVuZ3RoID09PSAxICkge1xuXHRcdHJldHVybiBmdW5jdGlvbigpIHtcblx0XHRcdHZhciBhcmdzID0gJC5tYWtlQXJyYXkoYXJndW1lbnRzKTtcblx0XHRcdGFyZ3MudW5zaGlmdChzb3VyY2UpO1xuXHRcdFx0cmV0dXJuICQudmFsaWRhdG9yLmZvcm1hdC5hcHBseSggdGhpcywgYXJncyApO1xuXHRcdH07XG5cdH1cblx0aWYgKCBhcmd1bWVudHMubGVuZ3RoID4gMiAmJiBwYXJhbXMuY29uc3RydWN0b3IgIT09IEFycmF5ICApIHtcblx0XHRwYXJhbXMgPSAkLm1ha2VBcnJheShhcmd1bWVudHMpLnNsaWNlKDEpO1xuXHR9XG5cdGlmICggcGFyYW1zLmNvbnN0cnVjdG9yICE9PSBBcnJheSApIHtcblx0XHRwYXJhbXMgPSBbIHBhcmFtcyBdO1xuXHR9XG5cdCQuZWFjaChwYXJhbXMsIGZ1bmN0aW9uKCBpLCBuICkge1xuXHRcdHNvdXJjZSA9IHNvdXJjZS5yZXBsYWNlKCBuZXcgUmVnRXhwKFwiXFxcXHtcIiArIGkgKyBcIlxcXFx9XCIsIFwiZ1wiKSwgZnVuY3Rpb24oKSB7XG5cdFx0XHRyZXR1cm4gbjtcblx0XHR9KTtcblx0fSk7XG5cdHJldHVybiBzb3VyY2U7XG59O1xuXG4kLmV4dGVuZCgkLnZhbGlkYXRvciwge1xuXG5cdGRlZmF1bHRzOiB7XG5cdFx0bWVzc2FnZXM6IHt9LFxuXHRcdGdyb3Vwczoge30sXG5cdFx0cnVsZXM6IHt9LFxuXHRcdGVycm9yQ2xhc3M6IFwiZXJyb3JcIixcblx0XHR2YWxpZENsYXNzOiBcInZhbGlkXCIsXG5cdFx0ZXJyb3JFbGVtZW50OiBcImxhYmVsXCIsXG5cdFx0Zm9jdXNJbnZhbGlkOiB0cnVlLFxuXHRcdGVycm9yQ29udGFpbmVyOiAkKFtdKSxcblx0XHRlcnJvckxhYmVsQ29udGFpbmVyOiAkKFtdKSxcblx0XHRvbnN1Ym1pdDogdHJ1ZSxcblx0XHRpZ25vcmU6IFwiOmhpZGRlblwiLFxuXHRcdGlnbm9yZVRpdGxlOiBmYWxzZSxcblx0XHRvbmZvY3VzaW46IGZ1bmN0aW9uKCBlbGVtZW50LCBldmVudCApIHtcblx0XHRcdHRoaXMubGFzdEFjdGl2ZSA9IGVsZW1lbnQ7XG5cblx0XHRcdC8vIGhpZGUgZXJyb3IgbGFiZWwgYW5kIHJlbW92ZSBlcnJvciBjbGFzcyBvbiBmb2N1cyBpZiBlbmFibGVkXG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MuZm9jdXNDbGVhbnVwICYmICF0aGlzLmJsb2NrRm9jdXNDbGVhbnVwICkge1xuXHRcdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MudW5oaWdobGlnaHQgKSB7XG5cdFx0XHRcdFx0dGhpcy5zZXR0aW5ncy51bmhpZ2hsaWdodC5jYWxsKCB0aGlzLCBlbGVtZW50LCB0aGlzLnNldHRpbmdzLmVycm9yQ2xhc3MsIHRoaXMuc2V0dGluZ3MudmFsaWRDbGFzcyApO1xuXHRcdFx0XHR9XG5cdFx0XHRcdHRoaXMuYWRkV3JhcHBlcih0aGlzLmVycm9yc0ZvcihlbGVtZW50KSkuaGlkZSgpO1xuXHRcdFx0fVxuXHRcdH0sXG5cdFx0b25mb2N1c291dDogZnVuY3Rpb24oIGVsZW1lbnQsIGV2ZW50ICkge1xuXHRcdFx0aWYgKCAhdGhpcy5jaGVja2FibGUoZWxlbWVudCkgJiYgKGVsZW1lbnQubmFtZSBpbiB0aGlzLnN1Ym1pdHRlZCB8fCAhdGhpcy5vcHRpb25hbChlbGVtZW50KSkgKSB7XG5cdFx0XHRcdHRoaXMuZWxlbWVudChlbGVtZW50KTtcblx0XHRcdH1cblx0XHR9LFxuXHRcdG9ua2V5dXA6IGZ1bmN0aW9uKCBlbGVtZW50LCBldmVudCApIHtcblx0XHRcdGlmICggZXZlbnQud2hpY2ggPT09IDkgJiYgdGhpcy5lbGVtZW50VmFsdWUoZWxlbWVudCkgPT09IFwiXCIgKSB7XG5cdFx0XHRcdHJldHVybjtcblx0XHRcdH0gZWxzZSBpZiAoIGVsZW1lbnQubmFtZSBpbiB0aGlzLnN1Ym1pdHRlZCB8fCBlbGVtZW50ID09PSB0aGlzLmxhc3RFbGVtZW50ICkge1xuXHRcdFx0XHR0aGlzLmVsZW1lbnQoZWxlbWVudCk7XG5cdFx0XHR9XG5cdFx0fSxcblx0XHRvbmNsaWNrOiBmdW5jdGlvbiggZWxlbWVudCwgZXZlbnQgKSB7XG5cdFx0XHQvLyBjbGljayBvbiBzZWxlY3RzLCByYWRpb2J1dHRvbnMgYW5kIGNoZWNrYm94ZXNcblx0XHRcdGlmICggZWxlbWVudC5uYW1lIGluIHRoaXMuc3VibWl0dGVkICkge1xuXHRcdFx0XHR0aGlzLmVsZW1lbnQoZWxlbWVudCk7XG5cdFx0XHR9XG5cdFx0XHQvLyBvciBvcHRpb24gZWxlbWVudHMsIGNoZWNrIHBhcmVudCBzZWxlY3QgaW4gdGhhdCBjYXNlXG5cdFx0XHRlbHNlIGlmICggZWxlbWVudC5wYXJlbnROb2RlLm5hbWUgaW4gdGhpcy5zdWJtaXR0ZWQgKSB7XG5cdFx0XHRcdHRoaXMuZWxlbWVudChlbGVtZW50LnBhcmVudE5vZGUpO1xuXHRcdFx0fVxuXHRcdH0sXG5cdFx0aGlnaGxpZ2h0OiBmdW5jdGlvbiggZWxlbWVudCwgZXJyb3JDbGFzcywgdmFsaWRDbGFzcyApIHtcblx0XHRcdGlmICggZWxlbWVudC50eXBlID09PSBcInJhZGlvXCIgKSB7XG5cdFx0XHRcdHRoaXMuZmluZEJ5TmFtZShlbGVtZW50Lm5hbWUpLmFkZENsYXNzKGVycm9yQ2xhc3MpLnJlbW92ZUNsYXNzKHZhbGlkQ2xhc3MpO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0JChlbGVtZW50KS5hZGRDbGFzcyhlcnJvckNsYXNzKS5yZW1vdmVDbGFzcyh2YWxpZENsYXNzKTtcblx0XHRcdH1cblx0XHR9LFxuXHRcdHVuaGlnaGxpZ2h0OiBmdW5jdGlvbiggZWxlbWVudCwgZXJyb3JDbGFzcywgdmFsaWRDbGFzcyApIHtcblx0XHRcdGlmICggZWxlbWVudC50eXBlID09PSBcInJhZGlvXCIgKSB7XG5cdFx0XHRcdHRoaXMuZmluZEJ5TmFtZShlbGVtZW50Lm5hbWUpLnJlbW92ZUNsYXNzKGVycm9yQ2xhc3MpLmFkZENsYXNzKHZhbGlkQ2xhc3MpO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0JChlbGVtZW50KS5yZW1vdmVDbGFzcyhlcnJvckNsYXNzKS5hZGRDbGFzcyh2YWxpZENsYXNzKTtcblx0XHRcdH1cblx0XHR9XG5cdH0sXG5cblx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vVmFsaWRhdG9yL3NldERlZmF1bHRzXG5cdHNldERlZmF1bHRzOiBmdW5jdGlvbiggc2V0dGluZ3MgKSB7XG5cdFx0JC5leHRlbmQoICQudmFsaWRhdG9yLmRlZmF1bHRzLCBzZXR0aW5ncyApO1xuXHR9LFxuXG5cdG1lc3NhZ2VzOiB7XG5cdFx0cmVxdWlyZWQ6IFwiVGhpcyBmaWVsZCBpcyByZXF1aXJlZC5cIixcblx0XHRyZW1vdGU6IFwiUGxlYXNlIGZpeCB0aGlzIGZpZWxkLlwiLFxuXHRcdGVtYWlsOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIGVtYWlsIGFkZHJlc3MuXCIsXG5cdFx0dXJsOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIFVSTC5cIixcblx0XHRkYXRlOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIGRhdGUuXCIsXG5cdFx0ZGF0ZUlTTzogXCJQbGVhc2UgZW50ZXIgYSB2YWxpZCBkYXRlIChJU08pLlwiLFxuXHRcdG51bWJlcjogXCJQbGVhc2UgZW50ZXIgYSB2YWxpZCBudW1iZXIuXCIsXG5cdFx0ZGlnaXRzOiBcIlBsZWFzZSBlbnRlciBvbmx5IGRpZ2l0cy5cIixcblx0XHRjcmVkaXRjYXJkOiBcIlBsZWFzZSBlbnRlciBhIHZhbGlkIGNyZWRpdCBjYXJkIG51bWJlci5cIixcblx0XHRlcXVhbFRvOiBcIlBsZWFzZSBlbnRlciB0aGUgc2FtZSB2YWx1ZSBhZ2Fpbi5cIixcblx0XHRtYXhsZW5ndGg6ICQudmFsaWRhdG9yLmZvcm1hdChcIlBsZWFzZSBlbnRlciBubyBtb3JlIHRoYW4gezB9IGNoYXJhY3RlcnMuXCIpLFxuXHRcdG1pbmxlbmd0aDogJC52YWxpZGF0b3IuZm9ybWF0KFwiUGxlYXNlIGVudGVyIGF0IGxlYXN0IHswfSBjaGFyYWN0ZXJzLlwiKSxcblx0XHRyYW5nZWxlbmd0aDogJC52YWxpZGF0b3IuZm9ybWF0KFwiUGxlYXNlIGVudGVyIGEgdmFsdWUgYmV0d2VlbiB7MH0gYW5kIHsxfSBjaGFyYWN0ZXJzIGxvbmcuXCIpLFxuXHRcdHJhbmdlOiAkLnZhbGlkYXRvci5mb3JtYXQoXCJQbGVhc2UgZW50ZXIgYSB2YWx1ZSBiZXR3ZWVuIHswfSBhbmQgezF9LlwiKSxcblx0XHRtYXg6ICQudmFsaWRhdG9yLmZvcm1hdChcIlBsZWFzZSBlbnRlciBhIHZhbHVlIGxlc3MgdGhhbiBvciBlcXVhbCB0byB7MH0uXCIpLFxuXHRcdG1pbjogJC52YWxpZGF0b3IuZm9ybWF0KFwiUGxlYXNlIGVudGVyIGEgdmFsdWUgZ3JlYXRlciB0aGFuIG9yIGVxdWFsIHRvIHswfS5cIilcblx0fSxcblxuXHRhdXRvQ3JlYXRlUmFuZ2VzOiBmYWxzZSxcblxuXHRwcm90b3R5cGU6IHtcblxuXHRcdGluaXQ6IGZ1bmN0aW9uKCkge1xuXHRcdFx0dGhpcy5sYWJlbENvbnRhaW5lciA9ICQodGhpcy5zZXR0aW5ncy5lcnJvckxhYmVsQ29udGFpbmVyKTtcblx0XHRcdHRoaXMuZXJyb3JDb250ZXh0ID0gdGhpcy5sYWJlbENvbnRhaW5lci5sZW5ndGggJiYgdGhpcy5sYWJlbENvbnRhaW5lciB8fCAkKHRoaXMuY3VycmVudEZvcm0pO1xuXHRcdFx0dGhpcy5jb250YWluZXJzID0gJCh0aGlzLnNldHRpbmdzLmVycm9yQ29udGFpbmVyKS5hZGQoIHRoaXMuc2V0dGluZ3MuZXJyb3JMYWJlbENvbnRhaW5lciApO1xuXHRcdFx0dGhpcy5zdWJtaXR0ZWQgPSB7fTtcblx0XHRcdHRoaXMudmFsdWVDYWNoZSA9IHt9O1xuXHRcdFx0dGhpcy5wZW5kaW5nUmVxdWVzdCA9IDA7XG5cdFx0XHR0aGlzLnBlbmRpbmcgPSB7fTtcblx0XHRcdHRoaXMuaW52YWxpZCA9IHt9O1xuXHRcdFx0dGhpcy5yZXNldCgpO1xuXG5cdFx0XHR2YXIgZ3JvdXBzID0gKHRoaXMuZ3JvdXBzID0ge30pO1xuXHRcdFx0JC5lYWNoKHRoaXMuc2V0dGluZ3MuZ3JvdXBzLCBmdW5jdGlvbigga2V5LCB2YWx1ZSApIHtcblx0XHRcdFx0aWYgKCB0eXBlb2YgdmFsdWUgPT09IFwic3RyaW5nXCIgKSB7XG5cdFx0XHRcdFx0dmFsdWUgPSB2YWx1ZS5zcGxpdCgvXFxzLyk7XG5cdFx0XHRcdH1cblx0XHRcdFx0JC5lYWNoKHZhbHVlLCBmdW5jdGlvbiggaW5kZXgsIG5hbWUgKSB7XG5cdFx0XHRcdFx0Z3JvdXBzW25hbWVdID0ga2V5O1xuXHRcdFx0XHR9KTtcblx0XHRcdH0pO1xuXHRcdFx0dmFyIHJ1bGVzID0gdGhpcy5zZXR0aW5ncy5ydWxlcztcblx0XHRcdCQuZWFjaChydWxlcywgZnVuY3Rpb24oIGtleSwgdmFsdWUgKSB7XG5cdFx0XHRcdHJ1bGVzW2tleV0gPSAkLnZhbGlkYXRvci5ub3JtYWxpemVSdWxlKHZhbHVlKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRmdW5jdGlvbiBkZWxlZ2F0ZShldmVudCkge1xuXHRcdFx0XHR2YXIgdmFsaWRhdG9yID0gJC5kYXRhKHRoaXNbMF0uZm9ybSwgXCJ2YWxpZGF0b3JcIiksXG5cdFx0XHRcdFx0ZXZlbnRUeXBlID0gXCJvblwiICsgZXZlbnQudHlwZS5yZXBsYWNlKC9edmFsaWRhdGUvLCBcIlwiKTtcblx0XHRcdFx0aWYgKCB2YWxpZGF0b3Iuc2V0dGluZ3NbZXZlbnRUeXBlXSApIHtcblx0XHRcdFx0XHR2YWxpZGF0b3Iuc2V0dGluZ3NbZXZlbnRUeXBlXS5jYWxsKHZhbGlkYXRvciwgdGhpc1swXSwgZXZlbnQpO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pXG5cdFx0XHRcdC52YWxpZGF0ZURlbGVnYXRlKFwiOnRleHQsIFt0eXBlPSdwYXNzd29yZCddLCBbdHlwZT0nZmlsZSddLCBzZWxlY3QsIHRleHRhcmVhLCBcIiArXG5cdFx0XHRcdFx0XCJbdHlwZT0nbnVtYmVyJ10sIFt0eXBlPSdzZWFyY2gnXSAsW3R5cGU9J3RlbCddLCBbdHlwZT0ndXJsJ10sIFwiICtcblx0XHRcdFx0XHRcIlt0eXBlPSdlbWFpbCddLCBbdHlwZT0nZGF0ZXRpbWUnXSwgW3R5cGU9J2RhdGUnXSwgW3R5cGU9J21vbnRoJ10sIFwiICtcblx0XHRcdFx0XHRcIlt0eXBlPSd3ZWVrJ10sIFt0eXBlPSd0aW1lJ10sIFt0eXBlPSdkYXRldGltZS1sb2NhbCddLCBcIiArXG5cdFx0XHRcdFx0XCJbdHlwZT0ncmFuZ2UnXSwgW3R5cGU9J2NvbG9yJ10gXCIsXG5cdFx0XHRcdFx0XCJmb2N1c2luIGZvY3Vzb3V0IGtleXVwXCIsIGRlbGVnYXRlKVxuXHRcdFx0XHQudmFsaWRhdGVEZWxlZ2F0ZShcIlt0eXBlPSdyYWRpbyddLCBbdHlwZT0nY2hlY2tib3gnXSwgc2VsZWN0LCBvcHRpb25cIiwgXCJjbGlja1wiLCBkZWxlZ2F0ZSk7XG5cblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5pbnZhbGlkSGFuZGxlciApIHtcblx0XHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKS5iaW5kKFwiaW52YWxpZC1mb3JtLnZhbGlkYXRlXCIsIHRoaXMuc2V0dGluZ3MuaW52YWxpZEhhbmRsZXIpO1xuXHRcdFx0fVxuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3IvZm9ybVxuXHRcdGZvcm06IGZ1bmN0aW9uKCkge1xuXHRcdFx0dGhpcy5jaGVja0Zvcm0oKTtcblx0XHRcdCQuZXh0ZW5kKHRoaXMuc3VibWl0dGVkLCB0aGlzLmVycm9yTWFwKTtcblx0XHRcdHRoaXMuaW52YWxpZCA9ICQuZXh0ZW5kKHt9LCB0aGlzLmVycm9yTWFwKTtcblx0XHRcdGlmICggIXRoaXMudmFsaWQoKSApIHtcblx0XHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKS50cmlnZ2VySGFuZGxlcihcImludmFsaWQtZm9ybVwiLCBbdGhpc10pO1xuXHRcdFx0fVxuXHRcdFx0dGhpcy5zaG93RXJyb3JzKCk7XG5cdFx0XHRyZXR1cm4gdGhpcy52YWxpZCgpO1xuXHRcdH0sXG5cblx0XHRjaGVja0Zvcm06IGZ1bmN0aW9uKCkge1xuXHRcdFx0dGhpcy5wcmVwYXJlRm9ybSgpO1xuXHRcdFx0Zm9yICggdmFyIGkgPSAwLCBlbGVtZW50cyA9ICh0aGlzLmN1cnJlbnRFbGVtZW50cyA9IHRoaXMuZWxlbWVudHMoKSk7IGVsZW1lbnRzW2ldOyBpKysgKSB7XG5cdFx0XHRcdHRoaXMuY2hlY2soIGVsZW1lbnRzW2ldICk7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gdGhpcy52YWxpZCgpO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9WYWxpZGF0b3IvZWxlbWVudFxuXHRcdGVsZW1lbnQ6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0ZWxlbWVudCA9IHRoaXMudmFsaWRhdGlvblRhcmdldEZvciggdGhpcy5jbGVhbiggZWxlbWVudCApICk7XG5cdFx0XHR0aGlzLmxhc3RFbGVtZW50ID0gZWxlbWVudDtcblx0XHRcdHRoaXMucHJlcGFyZUVsZW1lbnQoIGVsZW1lbnQgKTtcblx0XHRcdHRoaXMuY3VycmVudEVsZW1lbnRzID0gJChlbGVtZW50KTtcblx0XHRcdHZhciByZXN1bHQgPSB0aGlzLmNoZWNrKCBlbGVtZW50ICkgIT09IGZhbHNlO1xuXHRcdFx0aWYgKCByZXN1bHQgKSB7XG5cdFx0XHRcdGRlbGV0ZSB0aGlzLmludmFsaWRbZWxlbWVudC5uYW1lXTtcblx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdHRoaXMuaW52YWxpZFtlbGVtZW50Lm5hbWVdID0gdHJ1ZTtcblx0XHRcdH1cblx0XHRcdGlmICggIXRoaXMubnVtYmVyT2ZJbnZhbGlkcygpICkge1xuXHRcdFx0XHQvLyBIaWRlIGVycm9yIGNvbnRhaW5lcnMgb24gbGFzdCBlcnJvclxuXHRcdFx0XHR0aGlzLnRvSGlkZSA9IHRoaXMudG9IaWRlLmFkZCggdGhpcy5jb250YWluZXJzICk7XG5cdFx0XHR9XG5cdFx0XHR0aGlzLnNob3dFcnJvcnMoKTtcblx0XHRcdHJldHVybiByZXN1bHQ7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9zaG93RXJyb3JzXG5cdFx0c2hvd0Vycm9yczogZnVuY3Rpb24oIGVycm9ycyApIHtcblx0XHRcdGlmICggZXJyb3JzICkge1xuXHRcdFx0XHQvLyBhZGQgaXRlbXMgdG8gZXJyb3IgbGlzdCBhbmQgbWFwXG5cdFx0XHRcdCQuZXh0ZW5kKCB0aGlzLmVycm9yTWFwLCBlcnJvcnMgKTtcblx0XHRcdFx0dGhpcy5lcnJvckxpc3QgPSBbXTtcblx0XHRcdFx0Zm9yICggdmFyIG5hbWUgaW4gZXJyb3JzICkge1xuXHRcdFx0XHRcdHRoaXMuZXJyb3JMaXN0LnB1c2goe1xuXHRcdFx0XHRcdFx0bWVzc2FnZTogZXJyb3JzW25hbWVdLFxuXHRcdFx0XHRcdFx0ZWxlbWVudDogdGhpcy5maW5kQnlOYW1lKG5hbWUpWzBdXG5cdFx0XHRcdFx0fSk7XG5cdFx0XHRcdH1cblx0XHRcdFx0Ly8gcmVtb3ZlIGl0ZW1zIGZyb20gc3VjY2VzcyBsaXN0XG5cdFx0XHRcdHRoaXMuc3VjY2Vzc0xpc3QgPSAkLmdyZXAoIHRoaXMuc3VjY2Vzc0xpc3QsIGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0XHRcdHJldHVybiAhKGVsZW1lbnQubmFtZSBpbiBlcnJvcnMpO1xuXHRcdFx0XHR9KTtcblx0XHRcdH1cblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5zaG93RXJyb3JzICkge1xuXHRcdFx0XHR0aGlzLnNldHRpbmdzLnNob3dFcnJvcnMuY2FsbCggdGhpcywgdGhpcy5lcnJvck1hcCwgdGhpcy5lcnJvckxpc3QgKTtcblx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdHRoaXMuZGVmYXVsdFNob3dFcnJvcnMoKTtcblx0XHRcdH1cblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vVmFsaWRhdG9yL3Jlc2V0Rm9ybVxuXHRcdHJlc2V0Rm9ybTogZnVuY3Rpb24oKSB7XG5cdFx0XHRpZiAoICQuZm4ucmVzZXRGb3JtICkge1xuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLnJlc2V0Rm9ybSgpO1xuXHRcdFx0fVxuXHRcdFx0dGhpcy5zdWJtaXR0ZWQgPSB7fTtcblx0XHRcdHRoaXMubGFzdEVsZW1lbnQgPSBudWxsO1xuXHRcdFx0dGhpcy5wcmVwYXJlRm9ybSgpO1xuXHRcdFx0dGhpcy5oaWRlRXJyb3JzKCk7XG5cdFx0XHR0aGlzLmVsZW1lbnRzKCkucmVtb3ZlQ2xhc3MoIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcyApLnJlbW92ZURhdGEoIFwicHJldmlvdXNWYWx1ZVwiICk7XG5cdFx0fSxcblxuXHRcdG51bWJlck9mSW52YWxpZHM6IGZ1bmN0aW9uKCkge1xuXHRcdFx0cmV0dXJuIHRoaXMub2JqZWN0TGVuZ3RoKHRoaXMuaW52YWxpZCk7XG5cdFx0fSxcblxuXHRcdG9iamVjdExlbmd0aDogZnVuY3Rpb24oIG9iaiApIHtcblx0XHRcdHZhciBjb3VudCA9IDA7XG5cdFx0XHRmb3IgKCB2YXIgaSBpbiBvYmogKSB7XG5cdFx0XHRcdGNvdW50Kys7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gY291bnQ7XG5cdFx0fSxcblxuXHRcdGhpZGVFcnJvcnM6IGZ1bmN0aW9uKCkge1xuXHRcdFx0dGhpcy5hZGRXcmFwcGVyKCB0aGlzLnRvSGlkZSApLmhpZGUoKTtcblx0XHR9LFxuXG5cdFx0dmFsaWQ6IGZ1bmN0aW9uKCkge1xuXHRcdFx0cmV0dXJuIHRoaXMuc2l6ZSgpID09PSAwO1xuXHRcdH0sXG5cblx0XHRzaXplOiBmdW5jdGlvbigpIHtcblx0XHRcdHJldHVybiB0aGlzLmVycm9yTGlzdC5sZW5ndGg7XG5cdFx0fSxcblxuXHRcdGZvY3VzSW52YWxpZDogZnVuY3Rpb24oKSB7XG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MuZm9jdXNJbnZhbGlkICkge1xuXHRcdFx0XHR0cnkge1xuXHRcdFx0XHRcdCQodGhpcy5maW5kTGFzdEFjdGl2ZSgpIHx8IHRoaXMuZXJyb3JMaXN0Lmxlbmd0aCAmJiB0aGlzLmVycm9yTGlzdFswXS5lbGVtZW50IHx8IFtdKVxuXHRcdFx0XHRcdC5maWx0ZXIoXCI6dmlzaWJsZVwiKVxuXHRcdFx0XHRcdC5mb2N1cygpXG5cdFx0XHRcdFx0Ly8gbWFudWFsbHkgdHJpZ2dlciBmb2N1c2luIGV2ZW50OyB3aXRob3V0IGl0LCBmb2N1c2luIGhhbmRsZXIgaXNuJ3QgY2FsbGVkLCBmaW5kTGFzdEFjdGl2ZSB3b24ndCBoYXZlIGFueXRoaW5nIHRvIGZpbmRcblx0XHRcdFx0XHQudHJpZ2dlcihcImZvY3VzaW5cIik7XG5cdFx0XHRcdH0gY2F0Y2goZSkge1xuXHRcdFx0XHRcdC8vIGlnbm9yZSBJRSB0aHJvd2luZyBlcnJvcnMgd2hlbiBmb2N1c2luZyBoaWRkZW4gZWxlbWVudHNcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdH0sXG5cblx0XHRmaW5kTGFzdEFjdGl2ZTogZnVuY3Rpb24oKSB7XG5cdFx0XHR2YXIgbGFzdEFjdGl2ZSA9IHRoaXMubGFzdEFjdGl2ZTtcblx0XHRcdHJldHVybiBsYXN0QWN0aXZlICYmICQuZ3JlcCh0aGlzLmVycm9yTGlzdCwgZnVuY3Rpb24oIG4gKSB7XG5cdFx0XHRcdHJldHVybiBuLmVsZW1lbnQubmFtZSA9PT0gbGFzdEFjdGl2ZS5uYW1lO1xuXHRcdFx0fSkubGVuZ3RoID09PSAxICYmIGxhc3RBY3RpdmU7XG5cdFx0fSxcblxuXHRcdGVsZW1lbnRzOiBmdW5jdGlvbigpIHtcblx0XHRcdHZhciB2YWxpZGF0b3IgPSB0aGlzLFxuXHRcdFx0XHRydWxlc0NhY2hlID0ge307XG5cblx0XHRcdC8vIHNlbGVjdCBhbGwgdmFsaWQgaW5wdXRzIGluc2lkZSB0aGUgZm9ybSAobm8gc3VibWl0IG9yIHJlc2V0IGJ1dHRvbnMpXG5cdFx0XHRyZXR1cm4gJCh0aGlzLmN1cnJlbnRGb3JtKVxuXHRcdFx0LmZpbmQoXCJpbnB1dCwgc2VsZWN0LCB0ZXh0YXJlYVwiKVxuXHRcdFx0Lm5vdChcIjpzdWJtaXQsIDpyZXNldCwgOmltYWdlLCBbZGlzYWJsZWRdXCIpXG5cdFx0XHQubm90KCB0aGlzLnNldHRpbmdzLmlnbm9yZSApXG5cdFx0XHQuZmlsdGVyKGZ1bmN0aW9uKCkge1xuXHRcdFx0XHRpZiAoICF0aGlzLm5hbWUgJiYgdmFsaWRhdG9yLnNldHRpbmdzLmRlYnVnICYmIHdpbmRvdy5jb25zb2xlICkge1xuXHRcdFx0XHRcdGNvbnNvbGUuZXJyb3IoIFwiJW8gaGFzIG5vIG5hbWUgYXNzaWduZWRcIiwgdGhpcyk7XG5cdFx0XHRcdH1cblxuXHRcdFx0XHQvLyBzZWxlY3Qgb25seSB0aGUgZmlyc3QgZWxlbWVudCBmb3IgZWFjaCBuYW1lLCBhbmQgb25seSB0aG9zZSB3aXRoIHJ1bGVzIHNwZWNpZmllZFxuXHRcdFx0XHRpZiAoIHRoaXMubmFtZSBpbiBydWxlc0NhY2hlIHx8ICF2YWxpZGF0b3Iub2JqZWN0TGVuZ3RoKCQodGhpcykucnVsZXMoKSkgKSB7XG5cdFx0XHRcdFx0cmV0dXJuIGZhbHNlO1xuXHRcdFx0XHR9XG5cblx0XHRcdFx0cnVsZXNDYWNoZVt0aGlzLm5hbWVdID0gdHJ1ZTtcblx0XHRcdFx0cmV0dXJuIHRydWU7XG5cdFx0XHR9KTtcblx0XHR9LFxuXG5cdFx0Y2xlYW46IGZ1bmN0aW9uKCBzZWxlY3RvciApIHtcblx0XHRcdHJldHVybiAkKHNlbGVjdG9yKVswXTtcblx0XHR9LFxuXG5cdFx0ZXJyb3JzOiBmdW5jdGlvbigpIHtcblx0XHRcdHZhciBlcnJvckNsYXNzID0gdGhpcy5zZXR0aW5ncy5lcnJvckNsYXNzLnJlcGxhY2UoXCIgXCIsIFwiLlwiKTtcblx0XHRcdHJldHVybiAkKHRoaXMuc2V0dGluZ3MuZXJyb3JFbGVtZW50ICsgXCIuXCIgKyBlcnJvckNsYXNzLCB0aGlzLmVycm9yQ29udGV4dCk7XG5cdFx0fSxcblxuXHRcdHJlc2V0OiBmdW5jdGlvbigpIHtcblx0XHRcdHRoaXMuc3VjY2Vzc0xpc3QgPSBbXTtcblx0XHRcdHRoaXMuZXJyb3JMaXN0ID0gW107XG5cdFx0XHR0aGlzLmVycm9yTWFwID0ge307XG5cdFx0XHR0aGlzLnRvU2hvdyA9ICQoW10pO1xuXHRcdFx0dGhpcy50b0hpZGUgPSAkKFtdKTtcblx0XHRcdHRoaXMuY3VycmVudEVsZW1lbnRzID0gJChbXSk7XG5cdFx0fSxcblxuXHRcdHByZXBhcmVGb3JtOiBmdW5jdGlvbigpIHtcblx0XHRcdHRoaXMucmVzZXQoKTtcblx0XHRcdHRoaXMudG9IaWRlID0gdGhpcy5lcnJvcnMoKS5hZGQoIHRoaXMuY29udGFpbmVycyApO1xuXHRcdH0sXG5cblx0XHRwcmVwYXJlRWxlbWVudDogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHR0aGlzLnJlc2V0KCk7XG5cdFx0XHR0aGlzLnRvSGlkZSA9IHRoaXMuZXJyb3JzRm9yKGVsZW1lbnQpO1xuXHRcdH0sXG5cblx0XHRlbGVtZW50VmFsdWU6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0dmFyIHR5cGUgPSAkKGVsZW1lbnQpLmF0dHIoXCJ0eXBlXCIpLFxuXHRcdFx0XHR2YWwgPSAkKGVsZW1lbnQpLnZhbCgpO1xuXG5cdFx0XHRpZiAoIHR5cGUgPT09IFwicmFkaW9cIiB8fCB0eXBlID09PSBcImNoZWNrYm94XCIgKSB7XG5cdFx0XHRcdHJldHVybiAkKFwiaW5wdXRbbmFtZT0nXCIgKyAkKGVsZW1lbnQpLmF0dHIoXCJuYW1lXCIpICsgXCInXTpjaGVja2VkXCIpLnZhbCgpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoIHR5cGVvZiB2YWwgPT09IFwic3RyaW5nXCIgKSB7XG5cdFx0XHRcdHJldHVybiB2YWwucmVwbGFjZSgvXFxyL2csIFwiXCIpO1xuXHRcdFx0fVxuXHRcdFx0cmV0dXJuIHZhbDtcblx0XHR9LFxuXG5cdFx0Y2hlY2s6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0ZWxlbWVudCA9IHRoaXMudmFsaWRhdGlvblRhcmdldEZvciggdGhpcy5jbGVhbiggZWxlbWVudCApICk7XG5cblx0XHRcdHZhciBydWxlcyA9ICQoZWxlbWVudCkucnVsZXMoKTtcblx0XHRcdHZhciBkZXBlbmRlbmN5TWlzbWF0Y2ggPSBmYWxzZTtcblx0XHRcdHZhciB2YWwgPSB0aGlzLmVsZW1lbnRWYWx1ZShlbGVtZW50KTtcblx0XHRcdHZhciByZXN1bHQ7XG5cblx0XHRcdGZvciAodmFyIG1ldGhvZCBpbiBydWxlcyApIHtcblx0XHRcdFx0dmFyIHJ1bGUgPSB7IG1ldGhvZDogbWV0aG9kLCBwYXJhbWV0ZXJzOiBydWxlc1ttZXRob2RdIH07XG5cdFx0XHRcdHRyeSB7XG5cblx0XHRcdFx0XHRyZXN1bHQgPSAkLnZhbGlkYXRvci5tZXRob2RzW21ldGhvZF0uY2FsbCggdGhpcywgdmFsLCBlbGVtZW50LCBydWxlLnBhcmFtZXRlcnMgKTtcblxuXHRcdFx0XHRcdC8vIGlmIGEgbWV0aG9kIGluZGljYXRlcyB0aGF0IHRoZSBmaWVsZCBpcyBvcHRpb25hbCBhbmQgdGhlcmVmb3JlIHZhbGlkLFxuXHRcdFx0XHRcdC8vIGRvbid0IG1hcmsgaXQgYXMgdmFsaWQgd2hlbiB0aGVyZSBhcmUgbm8gb3RoZXIgcnVsZXNcblx0XHRcdFx0XHRpZiAoIHJlc3VsdCA9PT0gXCJkZXBlbmRlbmN5LW1pc21hdGNoXCIgKSB7XG5cdFx0XHRcdFx0XHRkZXBlbmRlbmN5TWlzbWF0Y2ggPSB0cnVlO1xuXHRcdFx0XHRcdFx0Y29udGludWU7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdGRlcGVuZGVuY3lNaXNtYXRjaCA9IGZhbHNlO1xuXG5cdFx0XHRcdFx0aWYgKCByZXN1bHQgPT09IFwicGVuZGluZ1wiICkge1xuXHRcdFx0XHRcdFx0dGhpcy50b0hpZGUgPSB0aGlzLnRvSGlkZS5ub3QoIHRoaXMuZXJyb3JzRm9yKGVsZW1lbnQpICk7XG5cdFx0XHRcdFx0XHRyZXR1cm47XG5cdFx0XHRcdFx0fVxuXG5cdFx0XHRcdFx0aWYgKCAhcmVzdWx0ICkge1xuXHRcdFx0XHRcdFx0dGhpcy5mb3JtYXRBbmRBZGQoIGVsZW1lbnQsIHJ1bGUgKTtcblx0XHRcdFx0XHRcdHJldHVybiBmYWxzZTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH0gY2F0Y2goZSkge1xuXHRcdFx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5kZWJ1ZyAmJiB3aW5kb3cuY29uc29sZSApIHtcblx0XHRcdFx0XHRcdGNvbnNvbGUubG9nKCBcIkV4Y2VwdGlvbiBvY2N1cnJlZCB3aGVuIGNoZWNraW5nIGVsZW1lbnQgXCIgKyBlbGVtZW50LmlkICsgXCIsIGNoZWNrIHRoZSAnXCIgKyBydWxlLm1ldGhvZCArIFwiJyBtZXRob2QuXCIsIGUgKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0dGhyb3cgZTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdFx0aWYgKCBkZXBlbmRlbmN5TWlzbWF0Y2ggKSB7XG5cdFx0XHRcdHJldHVybjtcblx0XHRcdH1cblx0XHRcdGlmICggdGhpcy5vYmplY3RMZW5ndGgocnVsZXMpICkge1xuXHRcdFx0XHR0aGlzLnN1Y2Nlc3NMaXN0LnB1c2goZWxlbWVudCk7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gdHJ1ZTtcblx0XHR9LFxuXG5cdFx0Ly8gcmV0dXJuIHRoZSBjdXN0b20gbWVzc2FnZSBmb3IgdGhlIGdpdmVuIGVsZW1lbnQgYW5kIHZhbGlkYXRpb24gbWV0aG9kXG5cdFx0Ly8gc3BlY2lmaWVkIGluIHRoZSBlbGVtZW50J3MgSFRNTDUgZGF0YSBhdHRyaWJ1dGVcblx0XHRjdXN0b21EYXRhTWVzc2FnZTogZnVuY3Rpb24oIGVsZW1lbnQsIG1ldGhvZCApIHtcblx0XHRcdHJldHVybiAkKGVsZW1lbnQpLmRhdGEoXCJtc2ctXCIgKyBtZXRob2QudG9Mb3dlckNhc2UoKSkgfHwgKGVsZW1lbnQuYXR0cmlidXRlcyAmJiAkKGVsZW1lbnQpLmF0dHIoXCJkYXRhLW1zZy1cIiArIG1ldGhvZC50b0xvd2VyQ2FzZSgpKSk7XG5cdFx0fSxcblxuXHRcdC8vIHJldHVybiB0aGUgY3VzdG9tIG1lc3NhZ2UgZm9yIHRoZSBnaXZlbiBlbGVtZW50IG5hbWUgYW5kIHZhbGlkYXRpb24gbWV0aG9kXG5cdFx0Y3VzdG9tTWVzc2FnZTogZnVuY3Rpb24oIG5hbWUsIG1ldGhvZCApIHtcblx0XHRcdHZhciBtID0gdGhpcy5zZXR0aW5ncy5tZXNzYWdlc1tuYW1lXTtcblx0XHRcdHJldHVybiBtICYmIChtLmNvbnN0cnVjdG9yID09PSBTdHJpbmcgPyBtIDogbVttZXRob2RdKTtcblx0XHR9LFxuXG5cdFx0Ly8gcmV0dXJuIHRoZSBmaXJzdCBkZWZpbmVkIGFyZ3VtZW50LCBhbGxvd2luZyBlbXB0eSBzdHJpbmdzXG5cdFx0ZmluZERlZmluZWQ6IGZ1bmN0aW9uKCkge1xuXHRcdFx0Zm9yKHZhciBpID0gMDsgaSA8IGFyZ3VtZW50cy5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRpZiAoIGFyZ3VtZW50c1tpXSAhPT0gdW5kZWZpbmVkICkge1xuXHRcdFx0XHRcdHJldHVybiBhcmd1bWVudHNbaV07XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdHJldHVybiB1bmRlZmluZWQ7XG5cdFx0fSxcblxuXHRcdGRlZmF1bHRNZXNzYWdlOiBmdW5jdGlvbiggZWxlbWVudCwgbWV0aG9kICkge1xuXHRcdFx0cmV0dXJuIHRoaXMuZmluZERlZmluZWQoXG5cdFx0XHRcdHRoaXMuY3VzdG9tTWVzc2FnZSggZWxlbWVudC5uYW1lLCBtZXRob2QgKSxcblx0XHRcdFx0dGhpcy5jdXN0b21EYXRhTWVzc2FnZSggZWxlbWVudCwgbWV0aG9kICksXG5cdFx0XHRcdC8vIHRpdGxlIGlzIG5ldmVyIHVuZGVmaW5lZCwgc28gaGFuZGxlIGVtcHR5IHN0cmluZyBhcyB1bmRlZmluZWRcblx0XHRcdFx0IXRoaXMuc2V0dGluZ3MuaWdub3JlVGl0bGUgJiYgZWxlbWVudC50aXRsZSB8fCB1bmRlZmluZWQsXG5cdFx0XHRcdCQudmFsaWRhdG9yLm1lc3NhZ2VzW21ldGhvZF0sXG5cdFx0XHRcdFwiPHN0cm9uZz5XYXJuaW5nOiBObyBtZXNzYWdlIGRlZmluZWQgZm9yIFwiICsgZWxlbWVudC5uYW1lICsgXCI8L3N0cm9uZz5cIlxuXHRcdFx0KTtcblx0XHR9LFxuXG5cdFx0Zm9ybWF0QW5kQWRkOiBmdW5jdGlvbiggZWxlbWVudCwgcnVsZSApIHtcblx0XHRcdHZhciBtZXNzYWdlID0gdGhpcy5kZWZhdWx0TWVzc2FnZSggZWxlbWVudCwgcnVsZS5tZXRob2QgKSxcblx0XHRcdFx0dGhlcmVnZXggPSAvXFwkP1xceyhcXGQrKVxcfS9nO1xuXHRcdFx0aWYgKCB0eXBlb2YgbWVzc2FnZSA9PT0gXCJmdW5jdGlvblwiICkge1xuXHRcdFx0XHRtZXNzYWdlID0gbWVzc2FnZS5jYWxsKHRoaXMsIHJ1bGUucGFyYW1ldGVycywgZWxlbWVudCk7XG5cdFx0XHR9IGVsc2UgaWYgKHRoZXJlZ2V4LnRlc3QobWVzc2FnZSkpIHtcblx0XHRcdFx0bWVzc2FnZSA9ICQudmFsaWRhdG9yLmZvcm1hdChtZXNzYWdlLnJlcGxhY2UodGhlcmVnZXgsIFwieyQxfVwiKSwgcnVsZS5wYXJhbWV0ZXJzKTtcblx0XHRcdH1cblx0XHRcdHRoaXMuZXJyb3JMaXN0LnB1c2goe1xuXHRcdFx0XHRtZXNzYWdlOiBtZXNzYWdlLFxuXHRcdFx0XHRlbGVtZW50OiBlbGVtZW50XG5cdFx0XHR9KTtcblxuXHRcdFx0dGhpcy5lcnJvck1hcFtlbGVtZW50Lm5hbWVdID0gbWVzc2FnZTtcblx0XHRcdHRoaXMuc3VibWl0dGVkW2VsZW1lbnQubmFtZV0gPSBtZXNzYWdlO1xuXHRcdH0sXG5cblx0XHRhZGRXcmFwcGVyOiBmdW5jdGlvbiggdG9Ub2dnbGUgKSB7XG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3Mud3JhcHBlciApIHtcblx0XHRcdFx0dG9Ub2dnbGUgPSB0b1RvZ2dsZS5hZGQoIHRvVG9nZ2xlLnBhcmVudCggdGhpcy5zZXR0aW5ncy53cmFwcGVyICkgKTtcblx0XHRcdH1cblx0XHRcdHJldHVybiB0b1RvZ2dsZTtcblx0XHR9LFxuXG5cdFx0ZGVmYXVsdFNob3dFcnJvcnM6IGZ1bmN0aW9uKCkge1xuXHRcdFx0dmFyIGksIGVsZW1lbnRzO1xuXHRcdFx0Zm9yICggaSA9IDA7IHRoaXMuZXJyb3JMaXN0W2ldOyBpKysgKSB7XG5cdFx0XHRcdHZhciBlcnJvciA9IHRoaXMuZXJyb3JMaXN0W2ldO1xuXHRcdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MuaGlnaGxpZ2h0ICkge1xuXHRcdFx0XHRcdHRoaXMuc2V0dGluZ3MuaGlnaGxpZ2h0LmNhbGwoIHRoaXMsIGVycm9yLmVsZW1lbnQsIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcywgdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICk7XG5cdFx0XHRcdH1cblx0XHRcdFx0dGhpcy5zaG93TGFiZWwoIGVycm9yLmVsZW1lbnQsIGVycm9yLm1lc3NhZ2UgKTtcblx0XHRcdH1cblx0XHRcdGlmICggdGhpcy5lcnJvckxpc3QubGVuZ3RoICkge1xuXHRcdFx0XHR0aGlzLnRvU2hvdyA9IHRoaXMudG9TaG93LmFkZCggdGhpcy5jb250YWluZXJzICk7XG5cdFx0XHR9XG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3Muc3VjY2VzcyApIHtcblx0XHRcdFx0Zm9yICggaSA9IDA7IHRoaXMuc3VjY2Vzc0xpc3RbaV07IGkrKyApIHtcblx0XHRcdFx0XHR0aGlzLnNob3dMYWJlbCggdGhpcy5zdWNjZXNzTGlzdFtpXSApO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3MudW5oaWdobGlnaHQgKSB7XG5cdFx0XHRcdGZvciAoIGkgPSAwLCBlbGVtZW50cyA9IHRoaXMudmFsaWRFbGVtZW50cygpOyBlbGVtZW50c1tpXTsgaSsrICkge1xuXHRcdFx0XHRcdHRoaXMuc2V0dGluZ3MudW5oaWdobGlnaHQuY2FsbCggdGhpcywgZWxlbWVudHNbaV0sIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcywgdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdHRoaXMudG9IaWRlID0gdGhpcy50b0hpZGUubm90KCB0aGlzLnRvU2hvdyApO1xuXHRcdFx0dGhpcy5oaWRlRXJyb3JzKCk7XG5cdFx0XHR0aGlzLmFkZFdyYXBwZXIoIHRoaXMudG9TaG93ICkuc2hvdygpO1xuXHRcdH0sXG5cblx0XHR2YWxpZEVsZW1lbnRzOiBmdW5jdGlvbigpIHtcblx0XHRcdHJldHVybiB0aGlzLmN1cnJlbnRFbGVtZW50cy5ub3QodGhpcy5pbnZhbGlkRWxlbWVudHMoKSk7XG5cdFx0fSxcblxuXHRcdGludmFsaWRFbGVtZW50czogZnVuY3Rpb24oKSB7XG5cdFx0XHRyZXR1cm4gJCh0aGlzLmVycm9yTGlzdCkubWFwKGZ1bmN0aW9uKCkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5lbGVtZW50O1xuXHRcdFx0fSk7XG5cdFx0fSxcblxuXHRcdHNob3dMYWJlbDogZnVuY3Rpb24oIGVsZW1lbnQsIG1lc3NhZ2UgKSB7XG5cdFx0XHR2YXIgbGFiZWwgPSB0aGlzLmVycm9yc0ZvciggZWxlbWVudCApO1xuXHRcdFx0aWYgKCBsYWJlbC5sZW5ndGggKSB7XG5cdFx0XHRcdC8vIHJlZnJlc2ggZXJyb3Ivc3VjY2VzcyBjbGFzc1xuXHRcdFx0XHRsYWJlbC5yZW1vdmVDbGFzcyggdGhpcy5zZXR0aW5ncy52YWxpZENsYXNzICkuYWRkQ2xhc3MoIHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcyApO1xuXHRcdFx0XHQvLyByZXBsYWNlIG1lc3NhZ2Ugb24gZXhpc3RpbmcgbGFiZWxcblx0XHRcdFx0bGFiZWwuaHRtbChtZXNzYWdlKTtcblx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdC8vIGNyZWF0ZSBsYWJlbFxuXHRcdFx0XHRsYWJlbCA9ICQoXCI8XCIgKyB0aGlzLnNldHRpbmdzLmVycm9yRWxlbWVudCArIFwiPlwiKVxuXHRcdFx0XHRcdC5hdHRyKFwiZm9yXCIsIHRoaXMuaWRPck5hbWUoZWxlbWVudCkpXG5cdFx0XHRcdFx0LmFkZENsYXNzKHRoaXMuc2V0dGluZ3MuZXJyb3JDbGFzcylcblx0XHRcdFx0XHQuaHRtbChtZXNzYWdlIHx8IFwiXCIpO1xuXHRcdFx0XHRpZiAoIHRoaXMuc2V0dGluZ3Mud3JhcHBlciApIHtcblx0XHRcdFx0XHQvLyBtYWtlIHN1cmUgdGhlIGVsZW1lbnQgaXMgdmlzaWJsZSwgZXZlbiBpbiBJRVxuXHRcdFx0XHRcdC8vIGFjdHVhbGx5IHNob3dpbmcgdGhlIHdyYXBwZWQgZWxlbWVudCBpcyBoYW5kbGVkIGVsc2V3aGVyZVxuXHRcdFx0XHRcdGxhYmVsID0gbGFiZWwuaGlkZSgpLnNob3coKS53cmFwKFwiPFwiICsgdGhpcy5zZXR0aW5ncy53cmFwcGVyICsgXCIvPlwiKS5wYXJlbnQoKTtcblx0XHRcdFx0fVxuXHRcdFx0XHRpZiAoICF0aGlzLmxhYmVsQ29udGFpbmVyLmFwcGVuZChsYWJlbCkubGVuZ3RoICkge1xuXHRcdFx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5lcnJvclBsYWNlbWVudCApIHtcblx0XHRcdFx0XHRcdHRoaXMuc2V0dGluZ3MuZXJyb3JQbGFjZW1lbnQobGFiZWwsICQoZWxlbWVudCkgKTtcblx0XHRcdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRcdFx0bGFiZWwuaW5zZXJ0QWZ0ZXIoZWxlbWVudCk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0XHRpZiAoICFtZXNzYWdlICYmIHRoaXMuc2V0dGluZ3Muc3VjY2VzcyApIHtcblx0XHRcdFx0bGFiZWwudGV4dChcIlwiKTtcblx0XHRcdFx0aWYgKCB0eXBlb2YgdGhpcy5zZXR0aW5ncy5zdWNjZXNzID09PSBcInN0cmluZ1wiICkge1xuXHRcdFx0XHRcdGxhYmVsLmFkZENsYXNzKCB0aGlzLnNldHRpbmdzLnN1Y2Nlc3MgKTtcblx0XHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0XHR0aGlzLnNldHRpbmdzLnN1Y2Nlc3MoIGxhYmVsLCBlbGVtZW50ICk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdHRoaXMudG9TaG93ID0gdGhpcy50b1Nob3cuYWRkKGxhYmVsKTtcblx0XHR9LFxuXG5cdFx0ZXJyb3JzRm9yOiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHRcdHZhciBuYW1lID0gdGhpcy5pZE9yTmFtZShlbGVtZW50KTtcblx0XHRcdHJldHVybiB0aGlzLmVycm9ycygpLmZpbHRlcihmdW5jdGlvbigpIHtcblx0XHRcdFx0cmV0dXJuICQodGhpcykuYXR0cihcImZvclwiKSA9PT0gbmFtZTtcblx0XHRcdH0pO1xuXHRcdH0sXG5cblx0XHRpZE9yTmFtZTogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5ncm91cHNbZWxlbWVudC5uYW1lXSB8fCAodGhpcy5jaGVja2FibGUoZWxlbWVudCkgPyBlbGVtZW50Lm5hbWUgOiBlbGVtZW50LmlkIHx8IGVsZW1lbnQubmFtZSk7XG5cdFx0fSxcblxuXHRcdHZhbGlkYXRpb25UYXJnZXRGb3I6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0Ly8gaWYgcmFkaW8vY2hlY2tib3gsIHZhbGlkYXRlIGZpcnN0IGVsZW1lbnQgaW4gZ3JvdXAgaW5zdGVhZFxuXHRcdFx0aWYgKCB0aGlzLmNoZWNrYWJsZShlbGVtZW50KSApIHtcblx0XHRcdFx0ZWxlbWVudCA9IHRoaXMuZmluZEJ5TmFtZSggZWxlbWVudC5uYW1lICkubm90KHRoaXMuc2V0dGluZ3MuaWdub3JlKVswXTtcblx0XHRcdH1cblx0XHRcdHJldHVybiBlbGVtZW50O1xuXHRcdH0sXG5cblx0XHRjaGVja2FibGU6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuICgvcmFkaW98Y2hlY2tib3gvaSkudGVzdChlbGVtZW50LnR5cGUpO1xuXHRcdH0sXG5cblx0XHRmaW5kQnlOYW1lOiBmdW5jdGlvbiggbmFtZSApIHtcblx0XHRcdHJldHVybiAkKHRoaXMuY3VycmVudEZvcm0pLmZpbmQoXCJbbmFtZT0nXCIgKyBuYW1lICsgXCInXVwiKTtcblx0XHR9LFxuXG5cdFx0Z2V0TGVuZ3RoOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XG5cdFx0XHRzd2l0Y2goIGVsZW1lbnQubm9kZU5hbWUudG9Mb3dlckNhc2UoKSApIHtcblx0XHRcdGNhc2UgXCJzZWxlY3RcIjpcblx0XHRcdFx0cmV0dXJuICQoXCJvcHRpb246c2VsZWN0ZWRcIiwgZWxlbWVudCkubGVuZ3RoO1xuXHRcdFx0Y2FzZSBcImlucHV0XCI6XG5cdFx0XHRcdGlmICggdGhpcy5jaGVja2FibGUoIGVsZW1lbnQpICkge1xuXHRcdFx0XHRcdHJldHVybiB0aGlzLmZpbmRCeU5hbWUoZWxlbWVudC5uYW1lKS5maWx0ZXIoXCI6Y2hlY2tlZFwiKS5sZW5ndGg7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHRcdHJldHVybiB2YWx1ZS5sZW5ndGg7XG5cdFx0fSxcblxuXHRcdGRlcGVuZDogZnVuY3Rpb24oIHBhcmFtLCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuIHRoaXMuZGVwZW5kVHlwZXNbdHlwZW9mIHBhcmFtXSA/IHRoaXMuZGVwZW5kVHlwZXNbdHlwZW9mIHBhcmFtXShwYXJhbSwgZWxlbWVudCkgOiB0cnVlO1xuXHRcdH0sXG5cblx0XHRkZXBlbmRUeXBlczoge1xuXHRcdFx0XCJib29sZWFuXCI6IGZ1bmN0aW9uKCBwYXJhbSwgZWxlbWVudCApIHtcblx0XHRcdFx0cmV0dXJuIHBhcmFtO1xuXHRcdFx0fSxcblx0XHRcdFwic3RyaW5nXCI6IGZ1bmN0aW9uKCBwYXJhbSwgZWxlbWVudCApIHtcblx0XHRcdFx0cmV0dXJuICEhJChwYXJhbSwgZWxlbWVudC5mb3JtKS5sZW5ndGg7XG5cdFx0XHR9LFxuXHRcdFx0XCJmdW5jdGlvblwiOiBmdW5jdGlvbiggcGFyYW0sIGVsZW1lbnQgKSB7XG5cdFx0XHRcdHJldHVybiBwYXJhbShlbGVtZW50KTtcblx0XHRcdH1cblx0XHR9LFxuXG5cdFx0b3B0aW9uYWw6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdFx0dmFyIHZhbCA9IHRoaXMuZWxlbWVudFZhbHVlKGVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuICEkLnZhbGlkYXRvci5tZXRob2RzLnJlcXVpcmVkLmNhbGwodGhpcywgdmFsLCBlbGVtZW50KSAmJiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcblx0XHR9LFxuXG5cdFx0c3RhcnRSZXF1ZXN0OiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHRcdGlmICggIXRoaXMucGVuZGluZ1tlbGVtZW50Lm5hbWVdICkge1xuXHRcdFx0XHR0aGlzLnBlbmRpbmdSZXF1ZXN0Kys7XG5cdFx0XHRcdHRoaXMucGVuZGluZ1tlbGVtZW50Lm5hbWVdID0gdHJ1ZTtcblx0XHRcdH1cblx0XHR9LFxuXG5cdFx0c3RvcFJlcXVlc3Q6IGZ1bmN0aW9uKCBlbGVtZW50LCB2YWxpZCApIHtcblx0XHRcdHRoaXMucGVuZGluZ1JlcXVlc3QtLTtcblx0XHRcdC8vIHNvbWV0aW1lcyBzeW5jaHJvbml6YXRpb24gZmFpbHMsIG1ha2Ugc3VyZSBwZW5kaW5nUmVxdWVzdCBpcyBuZXZlciA8IDBcblx0XHRcdGlmICggdGhpcy5wZW5kaW5nUmVxdWVzdCA8IDAgKSB7XG5cdFx0XHRcdHRoaXMucGVuZGluZ1JlcXVlc3QgPSAwO1xuXHRcdFx0fVxuXHRcdFx0ZGVsZXRlIHRoaXMucGVuZGluZ1tlbGVtZW50Lm5hbWVdO1xuXHRcdFx0aWYgKCB2YWxpZCAmJiB0aGlzLnBlbmRpbmdSZXF1ZXN0ID09PSAwICYmIHRoaXMuZm9ybVN1Ym1pdHRlZCAmJiB0aGlzLmZvcm0oKSApIHtcblx0XHRcdFx0JCh0aGlzLmN1cnJlbnRGb3JtKS5zdWJtaXQoKTtcblx0XHRcdFx0dGhpcy5mb3JtU3VibWl0dGVkID0gZmFsc2U7XG5cdFx0XHR9IGVsc2UgaWYgKCF2YWxpZCAmJiB0aGlzLnBlbmRpbmdSZXF1ZXN0ID09PSAwICYmIHRoaXMuZm9ybVN1Ym1pdHRlZCkge1xuXHRcdFx0XHQkKHRoaXMuY3VycmVudEZvcm0pLnRyaWdnZXJIYW5kbGVyKFwiaW52YWxpZC1mb3JtXCIsIFt0aGlzXSk7XG5cdFx0XHRcdHRoaXMuZm9ybVN1Ym1pdHRlZCA9IGZhbHNlO1xuXHRcdFx0fVxuXHRcdH0sXG5cblx0XHRwcmV2aW91c1ZhbHVlOiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHRcdHJldHVybiAkLmRhdGEoZWxlbWVudCwgXCJwcmV2aW91c1ZhbHVlXCIpIHx8ICQuZGF0YShlbGVtZW50LCBcInByZXZpb3VzVmFsdWVcIiwge1xuXHRcdFx0XHRvbGQ6IG51bGwsXG5cdFx0XHRcdHZhbGlkOiB0cnVlLFxuXHRcdFx0XHRtZXNzYWdlOiB0aGlzLmRlZmF1bHRNZXNzYWdlKCBlbGVtZW50LCBcInJlbW90ZVwiIClcblx0XHRcdH0pO1xuXHRcdH1cblxuXHR9LFxuXG5cdGNsYXNzUnVsZVNldHRpbmdzOiB7XG5cdFx0cmVxdWlyZWQ6IHtyZXF1aXJlZDogdHJ1ZX0sXG5cdFx0ZW1haWw6IHtlbWFpbDogdHJ1ZX0sXG5cdFx0dXJsOiB7dXJsOiB0cnVlfSxcblx0XHRkYXRlOiB7ZGF0ZTogdHJ1ZX0sXG5cdFx0ZGF0ZUlTTzoge2RhdGVJU086IHRydWV9LFxuXHRcdG51bWJlcjoge251bWJlcjogdHJ1ZX0sXG5cdFx0ZGlnaXRzOiB7ZGlnaXRzOiB0cnVlfSxcblx0XHRjcmVkaXRjYXJkOiB7Y3JlZGl0Y2FyZDogdHJ1ZX1cblx0fSxcblxuXHRhZGRDbGFzc1J1bGVzOiBmdW5jdGlvbiggY2xhc3NOYW1lLCBydWxlcyApIHtcblx0XHRpZiAoIGNsYXNzTmFtZS5jb25zdHJ1Y3RvciA9PT0gU3RyaW5nICkge1xuXHRcdFx0dGhpcy5jbGFzc1J1bGVTZXR0aW5nc1tjbGFzc05hbWVdID0gcnVsZXM7XG5cdFx0fSBlbHNlIHtcblx0XHRcdCQuZXh0ZW5kKHRoaXMuY2xhc3NSdWxlU2V0dGluZ3MsIGNsYXNzTmFtZSk7XG5cdFx0fVxuXHR9LFxuXG5cdGNsYXNzUnVsZXM6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdHZhciBydWxlcyA9IHt9O1xuXHRcdHZhciBjbGFzc2VzID0gJChlbGVtZW50KS5hdHRyKFwiY2xhc3NcIik7XG5cdFx0aWYgKCBjbGFzc2VzICkge1xuXHRcdFx0JC5lYWNoKGNsYXNzZXMuc3BsaXQoXCIgXCIpLCBmdW5jdGlvbigpIHtcblx0XHRcdFx0aWYgKCB0aGlzIGluICQudmFsaWRhdG9yLmNsYXNzUnVsZVNldHRpbmdzICkge1xuXHRcdFx0XHRcdCQuZXh0ZW5kKHJ1bGVzLCAkLnZhbGlkYXRvci5jbGFzc1J1bGVTZXR0aW5nc1t0aGlzXSk7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXHRcdH1cblx0XHRyZXR1cm4gcnVsZXM7XG5cdH0sXG5cblx0YXR0cmlidXRlUnVsZXM6IGZ1bmN0aW9uKCBlbGVtZW50ICkge1xuXHRcdHZhciBydWxlcyA9IHt9O1xuXHRcdHZhciAkZWxlbWVudCA9ICQoZWxlbWVudCk7XG5cdFx0dmFyIHR5cGUgPSAkZWxlbWVudFswXS5nZXRBdHRyaWJ1dGUoXCJ0eXBlXCIpO1xuXG5cdFx0Zm9yICh2YXIgbWV0aG9kIGluICQudmFsaWRhdG9yLm1ldGhvZHMpIHtcblx0XHRcdHZhciB2YWx1ZTtcblxuXHRcdFx0Ly8gc3VwcG9ydCBmb3IgPGlucHV0IHJlcXVpcmVkPiBpbiBib3RoIGh0bWw1IGFuZCBvbGRlciBicm93c2Vyc1xuXHRcdFx0aWYgKCBtZXRob2QgPT09IFwicmVxdWlyZWRcIiApIHtcblx0XHRcdFx0dmFsdWUgPSAkZWxlbWVudC5nZXQoMCkuZ2V0QXR0cmlidXRlKG1ldGhvZCk7XG5cdFx0XHRcdC8vIFNvbWUgYnJvd3NlcnMgcmV0dXJuIGFuIGVtcHR5IHN0cmluZyBmb3IgdGhlIHJlcXVpcmVkIGF0dHJpYnV0ZVxuXHRcdFx0XHQvLyBhbmQgbm9uLUhUTUw1IGJyb3dzZXJzIG1pZ2h0IGhhdmUgcmVxdWlyZWQ9XCJcIiBtYXJrdXBcblx0XHRcdFx0aWYgKCB2YWx1ZSA9PT0gXCJcIiApIHtcblx0XHRcdFx0XHR2YWx1ZSA9IHRydWU7XG5cdFx0XHRcdH1cblx0XHRcdFx0Ly8gZm9yY2Ugbm9uLUhUTUw1IGJyb3dzZXJzIHRvIHJldHVybiBib29sXG5cdFx0XHRcdHZhbHVlID0gISF2YWx1ZTtcblx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdHZhbHVlID0gJGVsZW1lbnQuYXR0cihtZXRob2QpO1xuXHRcdFx0fVxuXG5cdFx0XHQvLyBjb252ZXJ0IHRoZSB2YWx1ZSB0byBhIG51bWJlciBmb3IgbnVtYmVyIGlucHV0cywgYW5kIGZvciB0ZXh0IGZvciBiYWNrd2FyZHMgY29tcGFiaWxpdHlcblx0XHRcdC8vIGFsbG93cyB0eXBlPVwiZGF0ZVwiIGFuZCBvdGhlcnMgdG8gYmUgY29tcGFyZWQgYXMgc3RyaW5nc1xuXHRcdFx0aWYgKCAvbWlufG1heC8udGVzdCggbWV0aG9kICkgJiYgKCB0eXBlID09PSBudWxsIHx8IC9udW1iZXJ8cmFuZ2V8dGV4dC8udGVzdCggdHlwZSApICkgKSB7XG5cdFx0XHRcdHZhbHVlID0gTnVtYmVyKHZhbHVlKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKCB2YWx1ZSApIHtcblx0XHRcdFx0cnVsZXNbbWV0aG9kXSA9IHZhbHVlO1xuXHRcdFx0fSBlbHNlIGlmICggdHlwZSA9PT0gbWV0aG9kICYmIHR5cGUgIT09ICdyYW5nZScgKSB7XG5cdFx0XHRcdC8vIGV4Y2VwdGlvbjogdGhlIGpxdWVyeSB2YWxpZGF0ZSAncmFuZ2UnIG1ldGhvZFxuXHRcdFx0XHQvLyBkb2VzIG5vdCB0ZXN0IGZvciB0aGUgaHRtbDUgJ3JhbmdlJyB0eXBlXG5cdFx0XHRcdHJ1bGVzW21ldGhvZF0gPSB0cnVlO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdC8vIG1heGxlbmd0aCBtYXkgYmUgcmV0dXJuZWQgYXMgLTEsIDIxNDc0ODM2NDcgKElFKSBhbmQgNTI0Mjg4IChzYWZhcmkpIGZvciB0ZXh0IGlucHV0c1xuXHRcdGlmICggcnVsZXMubWF4bGVuZ3RoICYmIC8tMXwyMTQ3NDgzNjQ3fDUyNDI4OC8udGVzdChydWxlcy5tYXhsZW5ndGgpICkge1xuXHRcdFx0ZGVsZXRlIHJ1bGVzLm1heGxlbmd0aDtcblx0XHR9XG5cblx0XHRyZXR1cm4gcnVsZXM7XG5cdH0sXG5cblx0ZGF0YVJ1bGVzOiBmdW5jdGlvbiggZWxlbWVudCApIHtcblx0XHR2YXIgbWV0aG9kLCB2YWx1ZSxcblx0XHRcdHJ1bGVzID0ge30sICRlbGVtZW50ID0gJChlbGVtZW50KTtcblx0XHRmb3IgKG1ldGhvZCBpbiAkLnZhbGlkYXRvci5tZXRob2RzKSB7XG5cdFx0XHR2YWx1ZSA9ICRlbGVtZW50LmRhdGEoXCJydWxlLVwiICsgbWV0aG9kLnRvTG93ZXJDYXNlKCkpO1xuXHRcdFx0aWYgKCB2YWx1ZSAhPT0gdW5kZWZpbmVkICkge1xuXHRcdFx0XHRydWxlc1ttZXRob2RdID0gdmFsdWU7XG5cdFx0XHR9XG5cdFx0fVxuXHRcdHJldHVybiBydWxlcztcblx0fSxcblxuXHRzdGF0aWNSdWxlczogZnVuY3Rpb24oIGVsZW1lbnQgKSB7XG5cdFx0dmFyIHJ1bGVzID0ge307XG5cdFx0dmFyIHZhbGlkYXRvciA9ICQuZGF0YShlbGVtZW50LmZvcm0sIFwidmFsaWRhdG9yXCIpO1xuXHRcdGlmICggdmFsaWRhdG9yLnNldHRpbmdzLnJ1bGVzICkge1xuXHRcdFx0cnVsZXMgPSAkLnZhbGlkYXRvci5ub3JtYWxpemVSdWxlKHZhbGlkYXRvci5zZXR0aW5ncy5ydWxlc1tlbGVtZW50Lm5hbWVdKSB8fCB7fTtcblx0XHR9XG5cdFx0cmV0dXJuIHJ1bGVzO1xuXHR9LFxuXG5cdG5vcm1hbGl6ZVJ1bGVzOiBmdW5jdGlvbiggcnVsZXMsIGVsZW1lbnQgKSB7XG5cdFx0Ly8gaGFuZGxlIGRlcGVuZGVuY3kgY2hlY2tcblx0XHQkLmVhY2gocnVsZXMsIGZ1bmN0aW9uKCBwcm9wLCB2YWwgKSB7XG5cdFx0XHQvLyBpZ25vcmUgcnVsZSB3aGVuIHBhcmFtIGlzIGV4cGxpY2l0bHkgZmFsc2UsIGVnLiByZXF1aXJlZDpmYWxzZVxuXHRcdFx0aWYgKCB2YWwgPT09IGZhbHNlICkge1xuXHRcdFx0XHRkZWxldGUgcnVsZXNbcHJvcF07XG5cdFx0XHRcdHJldHVybjtcblx0XHRcdH1cblx0XHRcdGlmICggdmFsLnBhcmFtIHx8IHZhbC5kZXBlbmRzICkge1xuXHRcdFx0XHR2YXIga2VlcFJ1bGUgPSB0cnVlO1xuXHRcdFx0XHRzd2l0Y2ggKHR5cGVvZiB2YWwuZGVwZW5kcykge1xuXHRcdFx0XHRjYXNlIFwic3RyaW5nXCI6XG5cdFx0XHRcdFx0a2VlcFJ1bGUgPSAhISQodmFsLmRlcGVuZHMsIGVsZW1lbnQuZm9ybSkubGVuZ3RoO1xuXHRcdFx0XHRcdGJyZWFrO1xuXHRcdFx0XHRjYXNlIFwiZnVuY3Rpb25cIjpcblx0XHRcdFx0XHRrZWVwUnVsZSA9IHZhbC5kZXBlbmRzLmNhbGwoZWxlbWVudCwgZWxlbWVudCk7XG5cdFx0XHRcdFx0YnJlYWs7XG5cdFx0XHRcdH1cblx0XHRcdFx0aWYgKCBrZWVwUnVsZSApIHtcblx0XHRcdFx0XHRydWxlc1twcm9wXSA9IHZhbC5wYXJhbSAhPT0gdW5kZWZpbmVkID8gdmFsLnBhcmFtIDogdHJ1ZTtcblx0XHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0XHRkZWxldGUgcnVsZXNbcHJvcF07XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdC8vIGV2YWx1YXRlIHBhcmFtZXRlcnNcblx0XHQkLmVhY2gocnVsZXMsIGZ1bmN0aW9uKCBydWxlLCBwYXJhbWV0ZXIgKSB7XG5cdFx0XHRydWxlc1tydWxlXSA9ICQuaXNGdW5jdGlvbihwYXJhbWV0ZXIpID8gcGFyYW1ldGVyKGVsZW1lbnQpIDogcGFyYW1ldGVyO1xuXHRcdH0pO1xuXG5cdFx0Ly8gY2xlYW4gbnVtYmVyIHBhcmFtZXRlcnNcblx0XHQkLmVhY2goWydtaW5sZW5ndGgnLCAnbWF4bGVuZ3RoJ10sIGZ1bmN0aW9uKCkge1xuXHRcdFx0aWYgKCBydWxlc1t0aGlzXSApIHtcblx0XHRcdFx0cnVsZXNbdGhpc10gPSBOdW1iZXIocnVsZXNbdGhpc10pO1xuXHRcdFx0fVxuXHRcdH0pO1xuXHRcdCQuZWFjaChbJ3JhbmdlbGVuZ3RoJywgJ3JhbmdlJ10sIGZ1bmN0aW9uKCkge1xuXHRcdFx0dmFyIHBhcnRzO1xuXHRcdFx0aWYgKCBydWxlc1t0aGlzXSApIHtcblx0XHRcdFx0aWYgKCAkLmlzQXJyYXkocnVsZXNbdGhpc10pICkge1xuXHRcdFx0XHRcdHJ1bGVzW3RoaXNdID0gW051bWJlcihydWxlc1t0aGlzXVswXSksIE51bWJlcihydWxlc1t0aGlzXVsxXSldO1xuXHRcdFx0XHR9IGVsc2UgaWYgKCB0eXBlb2YgcnVsZXNbdGhpc10gPT09IFwic3RyaW5nXCIgKSB7XG5cdFx0XHRcdFx0cGFydHMgPSBydWxlc1t0aGlzXS5zcGxpdCgvW1xccyxdKy8pO1xuXHRcdFx0XHRcdHJ1bGVzW3RoaXNdID0gW051bWJlcihwYXJ0c1swXSksIE51bWJlcihwYXJ0c1sxXSldO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHRpZiAoICQudmFsaWRhdG9yLmF1dG9DcmVhdGVSYW5nZXMgKSB7XG5cdFx0XHQvLyBhdXRvLWNyZWF0ZSByYW5nZXNcblx0XHRcdGlmICggcnVsZXMubWluICYmIHJ1bGVzLm1heCApIHtcblx0XHRcdFx0cnVsZXMucmFuZ2UgPSBbcnVsZXMubWluLCBydWxlcy5tYXhdO1xuXHRcdFx0XHRkZWxldGUgcnVsZXMubWluO1xuXHRcdFx0XHRkZWxldGUgcnVsZXMubWF4O1xuXHRcdFx0fVxuXHRcdFx0aWYgKCBydWxlcy5taW5sZW5ndGggJiYgcnVsZXMubWF4bGVuZ3RoICkge1xuXHRcdFx0XHRydWxlcy5yYW5nZWxlbmd0aCA9IFtydWxlcy5taW5sZW5ndGgsIHJ1bGVzLm1heGxlbmd0aF07XG5cdFx0XHRcdGRlbGV0ZSBydWxlcy5taW5sZW5ndGg7XG5cdFx0XHRcdGRlbGV0ZSBydWxlcy5tYXhsZW5ndGg7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHJ1bGVzO1xuXHR9LFxuXG5cdC8vIENvbnZlcnRzIGEgc2ltcGxlIHN0cmluZyB0byBhIHtzdHJpbmc6IHRydWV9IHJ1bGUsIGUuZy4sIFwicmVxdWlyZWRcIiB0byB7cmVxdWlyZWQ6dHJ1ZX1cblx0bm9ybWFsaXplUnVsZTogZnVuY3Rpb24oIGRhdGEgKSB7XG5cdFx0aWYgKCB0eXBlb2YgZGF0YSA9PT0gXCJzdHJpbmdcIiApIHtcblx0XHRcdHZhciB0cmFuc2Zvcm1lZCA9IHt9O1xuXHRcdFx0JC5lYWNoKGRhdGEuc3BsaXQoL1xccy8pLCBmdW5jdGlvbigpIHtcblx0XHRcdFx0dHJhbnNmb3JtZWRbdGhpc10gPSB0cnVlO1xuXHRcdFx0fSk7XG5cdFx0XHRkYXRhID0gdHJhbnNmb3JtZWQ7XG5cdFx0fVxuXHRcdHJldHVybiBkYXRhO1xuXHR9LFxuXG5cdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL1ZhbGlkYXRvci9hZGRNZXRob2Rcblx0YWRkTWV0aG9kOiBmdW5jdGlvbiggbmFtZSwgbWV0aG9kLCBtZXNzYWdlICkge1xuXHRcdCQudmFsaWRhdG9yLm1ldGhvZHNbbmFtZV0gPSBtZXRob2Q7XG5cdFx0JC52YWxpZGF0b3IubWVzc2FnZXNbbmFtZV0gPSBtZXNzYWdlICE9PSB1bmRlZmluZWQgPyBtZXNzYWdlIDogJC52YWxpZGF0b3IubWVzc2FnZXNbbmFtZV07XG5cdFx0aWYgKCBtZXRob2QubGVuZ3RoIDwgMyApIHtcblx0XHRcdCQudmFsaWRhdG9yLmFkZENsYXNzUnVsZXMobmFtZSwgJC52YWxpZGF0b3Iubm9ybWFsaXplUnVsZShuYW1lKSk7XG5cdFx0fVxuXHR9LFxuXG5cdG1ldGhvZHM6IHtcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvcmVxdWlyZWRcblx0XHRyZXF1aXJlZDogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50LCBwYXJhbSApIHtcblx0XHRcdC8vIGNoZWNrIGlmIGRlcGVuZGVuY3kgaXMgbWV0XG5cdFx0XHRpZiAoICF0aGlzLmRlcGVuZChwYXJhbSwgZWxlbWVudCkgKSB7XG5cdFx0XHRcdHJldHVybiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcblx0XHRcdH1cblx0XHRcdGlmICggZWxlbWVudC5ub2RlTmFtZS50b0xvd2VyQ2FzZSgpID09PSBcInNlbGVjdFwiICkge1xuXHRcdFx0XHQvLyBjb3VsZCBiZSBhbiBhcnJheSBmb3Igc2VsZWN0LW11bHRpcGxlIG9yIGEgc3RyaW5nLCBib3RoIGFyZSBmaW5lIHRoaXMgd2F5XG5cdFx0XHRcdHZhciB2YWwgPSAkKGVsZW1lbnQpLnZhbCgpO1xuXHRcdFx0XHRyZXR1cm4gdmFsICYmIHZhbC5sZW5ndGggPiAwO1xuXHRcdFx0fVxuXHRcdFx0aWYgKCB0aGlzLmNoZWNrYWJsZShlbGVtZW50KSApIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuZ2V0TGVuZ3RoKHZhbHVlLCBlbGVtZW50KSA+IDA7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gJC50cmltKHZhbHVlKS5sZW5ndGggPiAwO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2VtYWlsXG5cdFx0ZW1haWw6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcblx0XHRcdC8vIGNvbnRyaWJ1dGVkIGJ5IFNjb3R0IEdvbnphbGV6OiBodHRwOi8vcHJvamVjdHMuc2NvdHRzcGxheWdyb3VuZC5jb20vZW1haWxfYWRkcmVzc192YWxpZGF0aW9uL1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgL14oKChbYS16XXxcXGR8WyEjXFwkJSYnXFwqXFwrXFwtXFwvPVxcP1xcXl9ge1xcfH1+XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkrKFxcLihbYS16XXxcXGR8WyEjXFwkJSYnXFwqXFwrXFwtXFwvPVxcP1xcXl9ge1xcfH1+XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkrKSopfCgoXFx4MjIpKCgoKFxceDIwfFxceDA5KSooXFx4MGRcXHgwYSkpPyhcXHgyMHxcXHgwOSkrKT8oKFtcXHgwMS1cXHgwOFxceDBiXFx4MGNcXHgwZS1cXHgxZlxceDdmXXxcXHgyMXxbXFx4MjMtXFx4NWJdfFtcXHg1ZC1cXHg3ZV18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfChcXFxcKFtcXHgwMS1cXHgwOVxceDBiXFx4MGNcXHgwZC1cXHg3Zl18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKSkpKigoKFxceDIwfFxceDA5KSooXFx4MGRcXHgwYSkpPyhcXHgyMHxcXHgwOSkrKT8oXFx4MjIpKSlAKCgoW2Etel18XFxkfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkqKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkpKVxcLikrKChbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KChbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkqKFthLXpdfFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKSkpJC9pLnRlc3QodmFsdWUpO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL3VybFxuXHRcdHVybDogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xuXHRcdFx0Ly8gY29udHJpYnV0ZWQgYnkgU2NvdHQgR29uemFsZXo6IGh0dHA6Ly9wcm9qZWN0cy5zY290dHNwbGF5Z3JvdW5kLmNvbS9pcmkvXG5cdFx0XHRyZXR1cm4gdGhpcy5vcHRpb25hbChlbGVtZW50KSB8fCAvXihodHRwcz98cz9mdHApOlxcL1xcLygoKChbYS16XXxcXGR8LXxcXC58X3x+fFtcXHUwMEEwLVxcdUQ3RkZcXHVGOTAwLVxcdUZEQ0ZcXHVGREYwLVxcdUZGRUZdKXwoJVtcXGRhLWZdezJ9KXxbIVxcJCYnXFwoXFwpXFwqXFwrLDs9XXw6KSpAKT8oKChcXGR8WzEtOV1cXGR8MVxcZFxcZHwyWzAtNF1cXGR8MjVbMC01XSlcXC4oXFxkfFsxLTldXFxkfDFcXGRcXGR8MlswLTRdXFxkfDI1WzAtNV0pXFwuKFxcZHxbMS05XVxcZHwxXFxkXFxkfDJbMC00XVxcZHwyNVswLTVdKVxcLihcXGR8WzEtOV1cXGR8MVxcZFxcZHwyWzAtNF1cXGR8MjVbMC01XSkpfCgoKFthLXpdfFxcZHxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KChbYS16XXxcXGR8W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKihbYS16XXxcXGR8W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKSlcXC4pKygoW2Etel18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfCgoW2Etel18W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pKihbYS16XXxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSkpKVxcLj8pKDpcXGQqKT8pKFxcLygoKFthLXpdfFxcZHwtfFxcLnxffH58W1xcdTAwQTAtXFx1RDdGRlxcdUY5MDAtXFx1RkRDRlxcdUZERjAtXFx1RkZFRl0pfCglW1xcZGEtZl17Mn0pfFshXFwkJidcXChcXClcXCpcXCssOz1dfDp8QCkrKFxcLygoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKSopKik/KT8oXFw/KCgoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKXxbXFx1RTAwMC1cXHVGOEZGXXxcXC98XFw/KSopPygjKCgoW2Etel18XFxkfC18XFwufF98fnxbXFx1MDBBMC1cXHVEN0ZGXFx1RjkwMC1cXHVGRENGXFx1RkRGMC1cXHVGRkVGXSl8KCVbXFxkYS1mXXsyfSl8WyFcXCQmJ1xcKFxcKVxcKlxcKyw7PV18OnxAKXxcXC98XFw/KSopPyQvaS50ZXN0KHZhbHVlKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9kYXRlXG5cdFx0ZGF0ZTogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50ICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgIS9JbnZhbGlkfE5hTi8udGVzdChuZXcgRGF0ZSh2YWx1ZSkudG9TdHJpbmcoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvZGF0ZUlTT1xuXHRcdGRhdGVJU086IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IC9eXFxkezR9W1xcL1xcLV1cXGR7MSwyfVtcXC9cXC1dXFxkezEsMn0kLy50ZXN0KHZhbHVlKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9udW1iZXJcblx0XHRudW1iZXI6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IC9eLT8oPzpcXGQrfFxcZHsxLDN9KD86LFxcZHszfSkrKT8oPzpcXC5cXGQrKT8kLy50ZXN0KHZhbHVlKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9kaWdpdHNcblx0XHRkaWdpdHM6IGZ1bmN0aW9uKCB2YWx1ZSwgZWxlbWVudCApIHtcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IC9eXFxkKyQvLnRlc3QodmFsdWUpO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL2NyZWRpdGNhcmRcblx0XHQvLyBiYXNlZCBvbiBodHRwOi8vZW4ud2lraXBlZGlhLm9yZy93aWtpL0x1aG5cblx0XHRjcmVkaXRjYXJkOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQgKSB7XG5cdFx0XHRpZiAoIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgKSB7XG5cdFx0XHRcdHJldHVybiBcImRlcGVuZGVuY3ktbWlzbWF0Y2hcIjtcblx0XHRcdH1cblx0XHRcdC8vIGFjY2VwdCBvbmx5IHNwYWNlcywgZGlnaXRzIGFuZCBkYXNoZXNcblx0XHRcdGlmICggL1teMC05IFxcLV0rLy50ZXN0KHZhbHVlKSApIHtcblx0XHRcdFx0cmV0dXJuIGZhbHNlO1xuXHRcdFx0fVxuXHRcdFx0dmFyIG5DaGVjayA9IDAsXG5cdFx0XHRcdG5EaWdpdCA9IDAsXG5cdFx0XHRcdGJFdmVuID0gZmFsc2U7XG5cblx0XHRcdHZhbHVlID0gdmFsdWUucmVwbGFjZSgvXFxEL2csIFwiXCIpO1xuXG5cdFx0XHRmb3IgKHZhciBuID0gdmFsdWUubGVuZ3RoIC0gMTsgbiA+PSAwOyBuLS0pIHtcblx0XHRcdFx0dmFyIGNEaWdpdCA9IHZhbHVlLmNoYXJBdChuKTtcblx0XHRcdFx0bkRpZ2l0ID0gcGFyc2VJbnQoY0RpZ2l0LCAxMCk7XG5cdFx0XHRcdGlmICggYkV2ZW4gKSB7XG5cdFx0XHRcdFx0aWYgKCAobkRpZ2l0ICo9IDIpID4gOSApIHtcblx0XHRcdFx0XHRcdG5EaWdpdCAtPSA5O1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0fVxuXHRcdFx0XHRuQ2hlY2sgKz0gbkRpZ2l0O1xuXHRcdFx0XHRiRXZlbiA9ICFiRXZlbjtcblx0XHRcdH1cblxuXHRcdFx0cmV0dXJuIChuQ2hlY2sgJSAxMCkgPT09IDA7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvbWlubGVuZ3RoXG5cdFx0bWlubGVuZ3RoOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0dmFyIGxlbmd0aCA9ICQuaXNBcnJheSggdmFsdWUgKSA/IHZhbHVlLmxlbmd0aCA6IHRoaXMuZ2V0TGVuZ3RoKCQudHJpbSh2YWx1ZSksIGVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgbGVuZ3RoID49IHBhcmFtO1xuXHRcdH0sXG5cblx0XHQvLyBodHRwOi8vZG9jcy5qcXVlcnkuY29tL1BsdWdpbnMvVmFsaWRhdGlvbi9NZXRob2RzL21heGxlbmd0aFxuXHRcdG1heGxlbmd0aDogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50LCBwYXJhbSApIHtcblx0XHRcdHZhciBsZW5ndGggPSAkLmlzQXJyYXkoIHZhbHVlICkgPyB2YWx1ZS5sZW5ndGggOiB0aGlzLmdldExlbmd0aCgkLnRyaW0odmFsdWUpLCBlbGVtZW50KTtcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8IGxlbmd0aCA8PSBwYXJhbTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9yYW5nZWxlbmd0aFxuXHRcdHJhbmdlbGVuZ3RoOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0dmFyIGxlbmd0aCA9ICQuaXNBcnJheSggdmFsdWUgKSA/IHZhbHVlLmxlbmd0aCA6IHRoaXMuZ2V0TGVuZ3RoKCQudHJpbSh2YWx1ZSksIGVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgKCBsZW5ndGggPj0gcGFyYW1bMF0gJiYgbGVuZ3RoIDw9IHBhcmFtWzFdICk7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvbWluXG5cdFx0bWluOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgdmFsdWUgPj0gcGFyYW07XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvbWF4XG5cdFx0bWF4OiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0cmV0dXJuIHRoaXMub3B0aW9uYWwoZWxlbWVudCkgfHwgdmFsdWUgPD0gcGFyYW07XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvcmFuZ2Vcblx0XHRyYW5nZTogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50LCBwYXJhbSApIHtcblx0XHRcdHJldHVybiB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpIHx8ICggdmFsdWUgPj0gcGFyYW1bMF0gJiYgdmFsdWUgPD0gcGFyYW1bMV0gKTtcblx0XHR9LFxuXG5cdFx0Ly8gaHR0cDovL2RvY3MuanF1ZXJ5LmNvbS9QbHVnaW5zL1ZhbGlkYXRpb24vTWV0aG9kcy9lcXVhbFRvXG5cdFx0ZXF1YWxUbzogZnVuY3Rpb24oIHZhbHVlLCBlbGVtZW50LCBwYXJhbSApIHtcblx0XHRcdC8vIGJpbmQgdG8gdGhlIGJsdXIgZXZlbnQgb2YgdGhlIHRhcmdldCBpbiBvcmRlciB0byByZXZhbGlkYXRlIHdoZW5ldmVyIHRoZSB0YXJnZXQgZmllbGQgaXMgdXBkYXRlZFxuXHRcdFx0Ly8gVE9ETyBmaW5kIGEgd2F5IHRvIGJpbmQgdGhlIGV2ZW50IGp1c3Qgb25jZSwgYXZvaWRpbmcgdGhlIHVuYmluZC1yZWJpbmQgb3ZlcmhlYWRcblx0XHRcdHZhciB0YXJnZXQgPSAkKHBhcmFtKTtcblx0XHRcdGlmICggdGhpcy5zZXR0aW5ncy5vbmZvY3Vzb3V0ICkge1xuXHRcdFx0XHR0YXJnZXQudW5iaW5kKFwiLnZhbGlkYXRlLWVxdWFsVG9cIikuYmluZChcImJsdXIudmFsaWRhdGUtZXF1YWxUb1wiLCBmdW5jdGlvbigpIHtcblx0XHRcdFx0XHQkKGVsZW1lbnQpLnZhbGlkKCk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXHRcdFx0cmV0dXJuIHZhbHVlID09PSB0YXJnZXQudmFsKCk7XG5cdFx0fSxcblxuXHRcdC8vIGh0dHA6Ly9kb2NzLmpxdWVyeS5jb20vUGx1Z2lucy9WYWxpZGF0aW9uL01ldGhvZHMvcmVtb3RlXG5cdFx0cmVtb3RlOiBmdW5jdGlvbiggdmFsdWUsIGVsZW1lbnQsIHBhcmFtICkge1xuXHRcdFx0aWYgKCB0aGlzLm9wdGlvbmFsKGVsZW1lbnQpICkge1xuXHRcdFx0XHRyZXR1cm4gXCJkZXBlbmRlbmN5LW1pc21hdGNoXCI7XG5cdFx0XHR9XG5cblx0XHRcdHZhciBwcmV2aW91cyA9IHRoaXMucHJldmlvdXNWYWx1ZShlbGVtZW50KTtcblx0XHRcdGlmICghdGhpcy5zZXR0aW5ncy5tZXNzYWdlc1tlbGVtZW50Lm5hbWVdICkge1xuXHRcdFx0XHR0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0gPSB7fTtcblx0XHRcdH1cblx0XHRcdHByZXZpb3VzLm9yaWdpbmFsTWVzc2FnZSA9IHRoaXMuc2V0dGluZ3MubWVzc2FnZXNbZWxlbWVudC5uYW1lXS5yZW1vdGU7XG5cdFx0XHR0aGlzLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0ucmVtb3RlID0gcHJldmlvdXMubWVzc2FnZTtcblxuXHRcdFx0cGFyYW0gPSB0eXBlb2YgcGFyYW0gPT09IFwic3RyaW5nXCIgJiYge3VybDpwYXJhbX0gfHwgcGFyYW07XG5cblx0XHRcdGlmICggcHJldmlvdXMub2xkID09PSB2YWx1ZSApIHtcblx0XHRcdFx0cmV0dXJuIHByZXZpb3VzLnZhbGlkO1xuXHRcdFx0fVxuXG5cdFx0XHRwcmV2aW91cy5vbGQgPSB2YWx1ZTtcblx0XHRcdHZhciB2YWxpZGF0b3IgPSB0aGlzO1xuXHRcdFx0dGhpcy5zdGFydFJlcXVlc3QoZWxlbWVudCk7XG5cdFx0XHR2YXIgZGF0YSA9IHt9O1xuXHRcdFx0ZGF0YVtlbGVtZW50Lm5hbWVdID0gdmFsdWU7XG5cdFx0XHQkLmFqYXgoJC5leHRlbmQodHJ1ZSwge1xuXHRcdFx0XHR1cmw6IHBhcmFtLFxuXHRcdFx0XHRtb2RlOiBcImFib3J0XCIsXG5cdFx0XHRcdHBvcnQ6IFwidmFsaWRhdGVcIiArIGVsZW1lbnQubmFtZSxcblx0XHRcdFx0ZGF0YVR5cGU6IFwianNvblwiLFxuXHRcdFx0XHRkYXRhOiBkYXRhLFxuXHRcdFx0XHRzdWNjZXNzOiBmdW5jdGlvbiggcmVzcG9uc2UgKSB7XG5cdFx0XHRcdFx0dmFsaWRhdG9yLnNldHRpbmdzLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0ucmVtb3RlID0gcHJldmlvdXMub3JpZ2luYWxNZXNzYWdlO1xuXHRcdFx0XHRcdHZhciB2YWxpZCA9IHJlc3BvbnNlID09PSB0cnVlIHx8IHJlc3BvbnNlID09PSBcInRydWVcIjtcblx0XHRcdFx0XHRpZiAoIHZhbGlkICkge1xuXHRcdFx0XHRcdFx0dmFyIHN1Ym1pdHRlZCA9IHZhbGlkYXRvci5mb3JtU3VibWl0dGVkO1xuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLnByZXBhcmVFbGVtZW50KGVsZW1lbnQpO1xuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLmZvcm1TdWJtaXR0ZWQgPSBzdWJtaXR0ZWQ7XG5cdFx0XHRcdFx0XHR2YWxpZGF0b3Iuc3VjY2Vzc0xpc3QucHVzaChlbGVtZW50KTtcblx0XHRcdFx0XHRcdGRlbGV0ZSB2YWxpZGF0b3IuaW52YWxpZFtlbGVtZW50Lm5hbWVdO1xuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLnNob3dFcnJvcnMoKTtcblx0XHRcdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRcdFx0dmFyIGVycm9ycyA9IHt9O1xuXHRcdFx0XHRcdFx0dmFyIG1lc3NhZ2UgPSByZXNwb25zZSB8fCB2YWxpZGF0b3IuZGVmYXVsdE1lc3NhZ2UoIGVsZW1lbnQsIFwicmVtb3RlXCIgKTtcblx0XHRcdFx0XHRcdGVycm9yc1tlbGVtZW50Lm5hbWVdID0gcHJldmlvdXMubWVzc2FnZSA9ICQuaXNGdW5jdGlvbihtZXNzYWdlKSA/IG1lc3NhZ2UodmFsdWUpIDogbWVzc2FnZTtcblx0XHRcdFx0XHRcdHZhbGlkYXRvci5pbnZhbGlkW2VsZW1lbnQubmFtZV0gPSB0cnVlO1xuXHRcdFx0XHRcdFx0dmFsaWRhdG9yLnNob3dFcnJvcnMoZXJyb3JzKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0cHJldmlvdXMudmFsaWQgPSB2YWxpZDtcblx0XHRcdFx0XHR2YWxpZGF0b3Iuc3RvcFJlcXVlc3QoZWxlbWVudCwgdmFsaWQpO1xuXHRcdFx0XHR9XG5cdFx0XHR9LCBwYXJhbSkpO1xuXHRcdFx0cmV0dXJuIFwicGVuZGluZ1wiO1xuXHRcdH1cblxuXHR9XG5cbn0pO1xuXG4vLyBkZXByZWNhdGVkLCB1c2UgJC52YWxpZGF0b3IuZm9ybWF0IGluc3RlYWRcbiQuZm9ybWF0ID0gJC52YWxpZGF0b3IuZm9ybWF0O1xuXG59KGpRdWVyeSkpO1xuXG4vLyBhamF4IG1vZGU6IGFib3J0XG4vLyB1c2FnZTogJC5hamF4KHsgbW9kZTogXCJhYm9ydFwiWywgcG9ydDogXCJ1bmlxdWVwb3J0XCJdfSk7XG4vLyBpZiBtb2RlOlwiYWJvcnRcIiBpcyB1c2VkLCB0aGUgcHJldmlvdXMgcmVxdWVzdCBvbiB0aGF0IHBvcnQgKHBvcnQgY2FuIGJlIHVuZGVmaW5lZCkgaXMgYWJvcnRlZCB2aWEgWE1MSHR0cFJlcXVlc3QuYWJvcnQoKVxuKGZ1bmN0aW9uKCQpIHtcblx0dmFyIHBlbmRpbmdSZXF1ZXN0cyA9IHt9O1xuXHQvLyBVc2UgYSBwcmVmaWx0ZXIgaWYgYXZhaWxhYmxlICgxLjUrKVxuXHRpZiAoICQuYWpheFByZWZpbHRlciApIHtcblx0XHQkLmFqYXhQcmVmaWx0ZXIoZnVuY3Rpb24oIHNldHRpbmdzLCBfLCB4aHIgKSB7XG5cdFx0XHR2YXIgcG9ydCA9IHNldHRpbmdzLnBvcnQ7XG5cdFx0XHRpZiAoIHNldHRpbmdzLm1vZGUgPT09IFwiYWJvcnRcIiApIHtcblx0XHRcdFx0aWYgKCBwZW5kaW5nUmVxdWVzdHNbcG9ydF0gKSB7XG5cdFx0XHRcdFx0cGVuZGluZ1JlcXVlc3RzW3BvcnRdLmFib3J0KCk7XG5cdFx0XHRcdH1cblx0XHRcdFx0cGVuZGluZ1JlcXVlc3RzW3BvcnRdID0geGhyO1xuXHRcdFx0fVxuXHRcdH0pO1xuXHR9IGVsc2Uge1xuXHRcdC8vIFByb3h5IGFqYXhcblx0XHR2YXIgYWpheCA9ICQuYWpheDtcblx0XHQkLmFqYXggPSBmdW5jdGlvbiggc2V0dGluZ3MgKSB7XG5cdFx0XHR2YXIgbW9kZSA9ICggXCJtb2RlXCIgaW4gc2V0dGluZ3MgPyBzZXR0aW5ncyA6ICQuYWpheFNldHRpbmdzICkubW9kZSxcblx0XHRcdFx0cG9ydCA9ICggXCJwb3J0XCIgaW4gc2V0dGluZ3MgPyBzZXR0aW5ncyA6ICQuYWpheFNldHRpbmdzICkucG9ydDtcblx0XHRcdGlmICggbW9kZSA9PT0gXCJhYm9ydFwiICkge1xuXHRcdFx0XHRpZiAoIHBlbmRpbmdSZXF1ZXN0c1twb3J0XSApIHtcblx0XHRcdFx0XHRwZW5kaW5nUmVxdWVzdHNbcG9ydF0uYWJvcnQoKTtcblx0XHRcdFx0fVxuXHRcdFx0XHRwZW5kaW5nUmVxdWVzdHNbcG9ydF0gPSBhamF4LmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XG5cdFx0XHRcdHJldHVybiBwZW5kaW5nUmVxdWVzdHNbcG9ydF07XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gYWpheC5hcHBseSh0aGlzLCBhcmd1bWVudHMpO1xuXHRcdH07XG5cdH1cbn0oalF1ZXJ5KSk7XG5cbi8vIHByb3ZpZGVzIGRlbGVnYXRlKHR5cGU6IFN0cmluZywgZGVsZWdhdGU6IFNlbGVjdG9yLCBoYW5kbGVyOiBDYWxsYmFjaykgcGx1Z2luIGZvciBlYXNpZXIgZXZlbnQgZGVsZWdhdGlvblxuLy8gaGFuZGxlciBpcyBvbmx5IGNhbGxlZCB3aGVuICQoZXZlbnQudGFyZ2V0KS5pcyhkZWxlZ2F0ZSksIGluIHRoZSBzY29wZSBvZiB0aGUganF1ZXJ5LW9iamVjdCBmb3IgZXZlbnQudGFyZ2V0XG4oZnVuY3Rpb24oJCkge1xuXHQkLmV4dGVuZCgkLmZuLCB7XG5cdFx0dmFsaWRhdGVEZWxlZ2F0ZTogZnVuY3Rpb24oIGRlbGVnYXRlLCB0eXBlLCBoYW5kbGVyICkge1xuXHRcdFx0cmV0dXJuIHRoaXMuYmluZCh0eXBlLCBmdW5jdGlvbiggZXZlbnQgKSB7XG5cdFx0XHRcdHZhciB0YXJnZXQgPSAkKGV2ZW50LnRhcmdldCk7XG5cdFx0XHRcdGlmICggdGFyZ2V0LmlzKGRlbGVnYXRlKSApIHtcblx0XHRcdFx0XHRyZXR1cm4gaGFuZGxlci5hcHBseSh0YXJnZXQsIGFyZ3VtZW50cyk7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXHRcdH1cblx0fSk7XG59KGpRdWVyeSkpO1xuIiwiLyogTlVHRVQ6IEJFR0lOIExJQ0VOU0UgVEVYVFxuICpcbiAqIE1pY3Jvc29mdCBncmFudHMgeW91IHRoZSByaWdodCB0byB1c2UgdGhlc2Ugc2NyaXB0IGZpbGVzIGZvciB0aGUgc29sZVxuICogcHVycG9zZSBvZiBlaXRoZXI6IChpKSBpbnRlcmFjdGluZyB0aHJvdWdoIHlvdXIgYnJvd3NlciB3aXRoIHRoZSBNaWNyb3NvZnRcbiAqIHdlYnNpdGUgb3Igb25saW5lIHNlcnZpY2UsIHN1YmplY3QgdG8gdGhlIGFwcGxpY2FibGUgbGljZW5zaW5nIG9yIHVzZVxuICogdGVybXM7IG9yIChpaSkgdXNpbmcgdGhlIGZpbGVzIGFzIGluY2x1ZGVkIHdpdGggYSBNaWNyb3NvZnQgcHJvZHVjdCBzdWJqZWN0XG4gKiB0byB0aGF0IHByb2R1Y3QncyBsaWNlbnNlIHRlcm1zLiBNaWNyb3NvZnQgcmVzZXJ2ZXMgYWxsIG90aGVyIHJpZ2h0cyB0byB0aGVcbiAqIGZpbGVzIG5vdCBleHByZXNzbHkgZ3JhbnRlZCBieSBNaWNyb3NvZnQsIHdoZXRoZXIgYnkgaW1wbGljYXRpb24sIGVzdG9wcGVsXG4gKiBvciBvdGhlcndpc2UuIEluc29mYXIgYXMgYSBzY3JpcHQgZmlsZSBpcyBkdWFsIGxpY2Vuc2VkIHVuZGVyIEdQTCxcbiAqIE1pY3Jvc29mdCBuZWl0aGVyIHRvb2sgdGhlIGNvZGUgdW5kZXIgR1BMIG5vciBkaXN0cmlidXRlcyBpdCB0aGVyZXVuZGVyIGJ1dFxuICogdW5kZXIgdGhlIHRlcm1zIHNldCBvdXQgaW4gdGhpcyBwYXJhZ3JhcGguIEFsbCBub3RpY2VzIGFuZCBsaWNlbnNlc1xuICogYmVsb3cgYXJlIGZvciBpbmZvcm1hdGlvbmFsIHB1cnBvc2VzIG9ubHkuXG4gKlxuICogTlVHRVQ6IEVORCBMSUNFTlNFIFRFWFQgKi9cbi8qIVxuKiogVW5vYnRydXNpdmUgdmFsaWRhdGlvbiBzdXBwb3J0IGxpYnJhcnkgZm9yIGpRdWVyeSBhbmQgalF1ZXJ5IFZhbGlkYXRlXG4qKiBDb3B5cmlnaHQgKEMpIE1pY3Jvc29mdCBDb3Jwb3JhdGlvbi4gQWxsIHJpZ2h0cyByZXNlcnZlZC5cbiovXG5cbi8qanNsaW50IHdoaXRlOiB0cnVlLCBicm93c2VyOiB0cnVlLCBvbmV2YXI6IHRydWUsIHVuZGVmOiB0cnVlLCBub21lbjogdHJ1ZSwgZXFlcWVxOiB0cnVlLCBwbHVzcGx1czogdHJ1ZSwgYml0d2lzZTogdHJ1ZSwgcmVnZXhwOiB0cnVlLCBuZXdjYXA6IHRydWUsIGltbWVkOiB0cnVlLCBzdHJpY3Q6IGZhbHNlICovXG4vKmdsb2JhbCBkb2N1bWVudDogZmFsc2UsIGpRdWVyeTogZmFsc2UgKi9cblxuKGZ1bmN0aW9uICgkKSB7XG4gICAgdmFyICRqUXZhbCA9ICQudmFsaWRhdG9yLFxuICAgICAgICBhZGFwdGVycyxcbiAgICAgICAgZGF0YV92YWxpZGF0aW9uID0gXCJ1bm9idHJ1c2l2ZVZhbGlkYXRpb25cIjtcblxuICAgIGZ1bmN0aW9uIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgcnVsZU5hbWUsIHZhbHVlKSB7XG4gICAgICAgIG9wdGlvbnMucnVsZXNbcnVsZU5hbWVdID0gdmFsdWU7XG4gICAgICAgIGlmIChvcHRpb25zLm1lc3NhZ2UpIHtcbiAgICAgICAgICAgIG9wdGlvbnMubWVzc2FnZXNbcnVsZU5hbWVdID0gb3B0aW9ucy5tZXNzYWdlO1xuICAgICAgICB9XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gc3BsaXRBbmRUcmltKHZhbHVlKSB7XG4gICAgICAgIHJldHVybiB2YWx1ZS5yZXBsYWNlKC9eXFxzK3xcXHMrJC9nLCBcIlwiKS5zcGxpdCgvXFxzKixcXHMqL2cpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGVzY2FwZUF0dHJpYnV0ZVZhbHVlKHZhbHVlKSB7XG4gICAgICAgIC8vIEFzIG1lbnRpb25lZCBvbiBodHRwOi8vYXBpLmpxdWVyeS5jb20vY2F0ZWdvcnkvc2VsZWN0b3JzL1xuICAgICAgICByZXR1cm4gdmFsdWUucmVwbGFjZSgvKFshXCIjJCUmJygpKissLi86Ozw9Pj9AXFxbXFxcXFxcXV5ge3x9fl0pL2csIFwiXFxcXCQxXCIpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIGdldE1vZGVsUHJlZml4KGZpZWxkTmFtZSkge1xuICAgICAgICByZXR1cm4gZmllbGROYW1lLnN1YnN0cigwLCBmaWVsZE5hbWUubGFzdEluZGV4T2YoXCIuXCIpICsgMSk7XG4gICAgfVxuXG4gICAgZnVuY3Rpb24gYXBwZW5kTW9kZWxQcmVmaXgodmFsdWUsIHByZWZpeCkge1xuICAgICAgICBpZiAodmFsdWUuaW5kZXhPZihcIiouXCIpID09PSAwKSB7XG4gICAgICAgICAgICB2YWx1ZSA9IHZhbHVlLnJlcGxhY2UoXCIqLlwiLCBwcmVmaXgpO1xuICAgICAgICB9XG4gICAgICAgIHJldHVybiB2YWx1ZTtcbiAgICB9XG5cbiAgICBmdW5jdGlvbiBvbkVycm9yKGVycm9yLCBpbnB1dEVsZW1lbnQpIHsgIC8vICd0aGlzJyBpcyB0aGUgZm9ybSBlbGVtZW50XG4gICAgICAgIHZhciBjb250YWluZXIgPSAkKHRoaXMpLmZpbmQoXCJbZGF0YS12YWxtc2ctZm9yPSdcIiArIGVzY2FwZUF0dHJpYnV0ZVZhbHVlKGlucHV0RWxlbWVudFswXS5uYW1lKSArIFwiJ11cIiksXG4gICAgICAgICAgICByZXBsYWNlQXR0clZhbHVlID0gY29udGFpbmVyLmF0dHIoXCJkYXRhLXZhbG1zZy1yZXBsYWNlXCIpLFxuICAgICAgICAgICAgcmVwbGFjZSA9IHJlcGxhY2VBdHRyVmFsdWUgPyAkLnBhcnNlSlNPTihyZXBsYWNlQXR0clZhbHVlKSAhPT0gZmFsc2UgOiBudWxsO1xuXG4gICAgICAgIGNvbnRhaW5lci5yZW1vdmVDbGFzcyhcImZpZWxkLXZhbGlkYXRpb24tdmFsaWRcIikuYWRkQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLWVycm9yXCIpO1xuICAgICAgICBlcnJvci5kYXRhKFwidW5vYnRydXNpdmVDb250YWluZXJcIiwgY29udGFpbmVyKTtcblxuICAgICAgICBpZiAocmVwbGFjZSkge1xuICAgICAgICAgICAgY29udGFpbmVyLmVtcHR5KCk7XG4gICAgICAgICAgICBlcnJvci5yZW1vdmVDbGFzcyhcImlucHV0LXZhbGlkYXRpb24tZXJyb3JcIikuYXBwZW5kVG8oY29udGFpbmVyKTtcbiAgICAgICAgfVxuICAgICAgICBlbHNlIHtcbiAgICAgICAgICAgIGVycm9yLmhpZGUoKTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGZ1bmN0aW9uIG9uRXJyb3JzKGV2ZW50LCB2YWxpZGF0b3IpIHsgIC8vICd0aGlzJyBpcyB0aGUgZm9ybSBlbGVtZW50XG4gICAgICAgIHZhciBjb250YWluZXIgPSAkKHRoaXMpLmZpbmQoXCJbZGF0YS12YWxtc2ctc3VtbWFyeT10cnVlXVwiKSxcbiAgICAgICAgICAgIGxpc3QgPSBjb250YWluZXIuZmluZChcInVsXCIpO1xuXG4gICAgICAgIGlmIChsaXN0ICYmIGxpc3QubGVuZ3RoICYmIHZhbGlkYXRvci5lcnJvckxpc3QubGVuZ3RoKSB7XG4gICAgICAgICAgICBsaXN0LmVtcHR5KCk7XG4gICAgICAgICAgICBjb250YWluZXIuYWRkQ2xhc3MoXCJ2YWxpZGF0aW9uLXN1bW1hcnktZXJyb3JzXCIpLnJlbW92ZUNsYXNzKFwidmFsaWRhdGlvbi1zdW1tYXJ5LXZhbGlkXCIpO1xuXG4gICAgICAgICAgICAkLmVhY2godmFsaWRhdG9yLmVycm9yTGlzdCwgZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICQoXCI8bGkgLz5cIikuaHRtbCh0aGlzLm1lc3NhZ2UpLmFwcGVuZFRvKGxpc3QpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBmdW5jdGlvbiBvblN1Y2Nlc3MoZXJyb3IpIHsgIC8vICd0aGlzJyBpcyB0aGUgZm9ybSBlbGVtZW50XG4gICAgICAgIHZhciBjb250YWluZXIgPSBlcnJvci5kYXRhKFwidW5vYnRydXNpdmVDb250YWluZXJcIiksXG4gICAgICAgICAgICByZXBsYWNlQXR0clZhbHVlID0gY29udGFpbmVyLmF0dHIoXCJkYXRhLXZhbG1zZy1yZXBsYWNlXCIpLFxuICAgICAgICAgICAgcmVwbGFjZSA9IHJlcGxhY2VBdHRyVmFsdWUgPyAkLnBhcnNlSlNPTihyZXBsYWNlQXR0clZhbHVlKSA6IG51bGw7XG5cbiAgICAgICAgaWYgKGNvbnRhaW5lcikge1xuICAgICAgICAgICAgY29udGFpbmVyLmFkZENsYXNzKFwiZmllbGQtdmFsaWRhdGlvbi12YWxpZFwiKS5yZW1vdmVDbGFzcyhcImZpZWxkLXZhbGlkYXRpb24tZXJyb3JcIik7XG4gICAgICAgICAgICBlcnJvci5yZW1vdmVEYXRhKFwidW5vYnRydXNpdmVDb250YWluZXJcIik7XG5cbiAgICAgICAgICAgIGlmIChyZXBsYWNlKSB7XG4gICAgICAgICAgICAgICAgY29udGFpbmVyLmVtcHR5KCk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICBmdW5jdGlvbiBvblJlc2V0KGV2ZW50KSB7ICAvLyAndGhpcycgaXMgdGhlIGZvcm0gZWxlbWVudFxuICAgICAgICB2YXIgJGZvcm0gPSAkKHRoaXMpO1xuICAgICAgICAkZm9ybS5kYXRhKFwidmFsaWRhdG9yXCIpLnJlc2V0Rm9ybSgpO1xuICAgICAgICAkZm9ybS5maW5kKFwiLnZhbGlkYXRpb24tc3VtbWFyeS1lcnJvcnNcIilcbiAgICAgICAgICAgIC5hZGRDbGFzcyhcInZhbGlkYXRpb24tc3VtbWFyeS12YWxpZFwiKVxuICAgICAgICAgICAgLnJlbW92ZUNsYXNzKFwidmFsaWRhdGlvbi1zdW1tYXJ5LWVycm9yc1wiKTtcbiAgICAgICAgJGZvcm0uZmluZChcIi5maWVsZC12YWxpZGF0aW9uLWVycm9yXCIpXG4gICAgICAgICAgICAuYWRkQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLXZhbGlkXCIpXG4gICAgICAgICAgICAucmVtb3ZlQ2xhc3MoXCJmaWVsZC12YWxpZGF0aW9uLWVycm9yXCIpXG4gICAgICAgICAgICAucmVtb3ZlRGF0YShcInVub2J0cnVzaXZlQ29udGFpbmVyXCIpXG4gICAgICAgICAgICAuZmluZChcIj4qXCIpICAvLyBJZiB3ZSB3ZXJlIHVzaW5nIHZhbG1zZy1yZXBsYWNlLCBnZXQgdGhlIHVuZGVybHlpbmcgZXJyb3JcbiAgICAgICAgICAgICAgICAucmVtb3ZlRGF0YShcInVub2J0cnVzaXZlQ29udGFpbmVyXCIpO1xuICAgIH1cblxuICAgIGZ1bmN0aW9uIHZhbGlkYXRpb25JbmZvKGZvcm0pIHtcbiAgICAgICAgdmFyICRmb3JtID0gJChmb3JtKSxcbiAgICAgICAgICAgIHJlc3VsdCA9ICRmb3JtLmRhdGEoZGF0YV92YWxpZGF0aW9uKSxcbiAgICAgICAgICAgIG9uUmVzZXRQcm94eSA9ICQucHJveHkob25SZXNldCwgZm9ybSksXG4gICAgICAgICAgICBkZWZhdWx0T3B0aW9ucyA9ICRqUXZhbC51bm9idHJ1c2l2ZS5vcHRpb25zIHx8IHt9LFxuICAgICAgICAgICAgZXhlY0luQ29udGV4dCA9IGZ1bmN0aW9uIChuYW1lLCBhcmdzKSB7XG4gICAgICAgICAgICAgICAgdmFyIGZ1bmMgPSBkZWZhdWx0T3B0aW9uc1tuYW1lXTtcbiAgICAgICAgICAgICAgICBmdW5jICYmICQuaXNGdW5jdGlvbihmdW5jKSAmJiBmdW5jLmFwcGx5KGZvcm0sIGFyZ3MpO1xuICAgICAgICAgICAgfVxuXG4gICAgICAgIGlmICghcmVzdWx0KSB7XG4gICAgICAgICAgICByZXN1bHQgPSB7XG4gICAgICAgICAgICAgICAgb3B0aW9uczogeyAgLy8gb3B0aW9ucyBzdHJ1Y3R1cmUgcGFzc2VkIHRvIGpRdWVyeSBWYWxpZGF0ZSdzIHZhbGlkYXRlKCkgbWV0aG9kXG4gICAgICAgICAgICAgICAgICAgIGVycm9yQ2xhc3M6IGRlZmF1bHRPcHRpb25zLmVycm9yQ2xhc3MgfHwgXCJpbnB1dC12YWxpZGF0aW9uLWVycm9yXCIsXG4gICAgICAgICAgICAgICAgICAgIGVycm9yRWxlbWVudDogZGVmYXVsdE9wdGlvbnMuZXJyb3JFbGVtZW50IHx8IFwic3BhblwiLFxuICAgICAgICAgICAgICAgICAgICBlcnJvclBsYWNlbWVudDogZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgICAgICAgICAgb25FcnJvci5hcHBseShmb3JtLCBhcmd1bWVudHMpO1xuICAgICAgICAgICAgICAgICAgICAgICAgZXhlY0luQ29udGV4dChcImVycm9yUGxhY2VtZW50XCIsIGFyZ3VtZW50cyk7XG4gICAgICAgICAgICAgICAgICAgIH0sXG4gICAgICAgICAgICAgICAgICAgIGludmFsaWRIYW5kbGVyOiBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkVycm9ycy5hcHBseShmb3JtLCBhcmd1bWVudHMpO1xuICAgICAgICAgICAgICAgICAgICAgICAgZXhlY0luQ29udGV4dChcImludmFsaWRIYW5kbGVyXCIsIGFyZ3VtZW50cyk7XG4gICAgICAgICAgICAgICAgICAgIH0sXG4gICAgICAgICAgICAgICAgICAgIG1lc3NhZ2VzOiB7fSxcbiAgICAgICAgICAgICAgICAgICAgcnVsZXM6IHt9LFxuICAgICAgICAgICAgICAgICAgICBzdWNjZXNzOiBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICBvblN1Y2Nlc3MuYXBwbHkoZm9ybSwgYXJndW1lbnRzKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIGV4ZWNJbkNvbnRleHQoXCJzdWNjZXNzXCIsIGFyZ3VtZW50cyk7XG4gICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICB9LFxuICAgICAgICAgICAgICAgIGF0dGFjaFZhbGlkYXRpb246IGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgJGZvcm1cbiAgICAgICAgICAgICAgICAgICAgICAgIC5vZmYoXCJyZXNldC5cIiArIGRhdGFfdmFsaWRhdGlvbiwgb25SZXNldFByb3h5KVxuICAgICAgICAgICAgICAgICAgICAgICAgLm9uKFwicmVzZXQuXCIgKyBkYXRhX3ZhbGlkYXRpb24sIG9uUmVzZXRQcm94eSlcbiAgICAgICAgICAgICAgICAgICAgICAgIC52YWxpZGF0ZSh0aGlzLm9wdGlvbnMpO1xuICAgICAgICAgICAgICAgIH0sXG4gICAgICAgICAgICAgICAgdmFsaWRhdGU6IGZ1bmN0aW9uICgpIHsgIC8vIGEgdmFsaWRhdGlvbiBmdW5jdGlvbiB0aGF0IGlzIGNhbGxlZCBieSB1bm9idHJ1c2l2ZSBBamF4XG4gICAgICAgICAgICAgICAgICAgICRmb3JtLnZhbGlkYXRlKCk7XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiAkZm9ybS52YWxpZCgpO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH07XG4gICAgICAgICAgICAkZm9ybS5kYXRhKGRhdGFfdmFsaWRhdGlvbiwgcmVzdWx0KTtcbiAgICAgICAgfVxuXG4gICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgfVxuXG4gICAgJGpRdmFsLnVub2J0cnVzaXZlID0ge1xuICAgICAgICBhZGFwdGVyczogW10sXG5cbiAgICAgICAgcGFyc2VFbGVtZW50OiBmdW5jdGlvbiAoZWxlbWVudCwgc2tpcEF0dGFjaCkge1xuICAgICAgICAgICAgLy8vIDxzdW1tYXJ5PlxuICAgICAgICAgICAgLy8vIFBhcnNlcyBhIHNpbmdsZSBIVE1MIGVsZW1lbnQgZm9yIHVub2J0cnVzaXZlIHZhbGlkYXRpb24gYXR0cmlidXRlcy5cbiAgICAgICAgICAgIC8vLyA8L3N1bW1hcnk+XG4gICAgICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJlbGVtZW50XCIgZG9tRWxlbWVudD1cInRydWVcIj5UaGUgSFRNTCBlbGVtZW50IHRvIGJlIHBhcnNlZC48L3BhcmFtPlxuICAgICAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwic2tpcEF0dGFjaFwiIHR5cGU9XCJCb29sZWFuXCI+W09wdGlvbmFsXSB0cnVlIHRvIHNraXAgYXR0YWNoaW5nIHRoZVxuICAgICAgICAgICAgLy8vIHZhbGlkYXRpb24gdG8gdGhlIGZvcm0uIElmIHBhcnNpbmcganVzdCB0aGlzIHNpbmdsZSBlbGVtZW50LCB5b3Ugc2hvdWxkIHNwZWNpZnkgdHJ1ZS5cbiAgICAgICAgICAgIC8vLyBJZiBwYXJzaW5nIHNldmVyYWwgZWxlbWVudHMsIHlvdSBzaG91bGQgc3BlY2lmeSBmYWxzZSwgYW5kIG1hbnVhbGx5IGF0dGFjaCB0aGUgdmFsaWRhdGlvblxuICAgICAgICAgICAgLy8vIHRvIHRoZSBmb3JtIHdoZW4geW91IGFyZSBmaW5pc2hlZC4gVGhlIGRlZmF1bHQgaXMgZmFsc2UuPC9wYXJhbT5cbiAgICAgICAgICAgIHZhciAkZWxlbWVudCA9ICQoZWxlbWVudCksXG4gICAgICAgICAgICAgICAgZm9ybSA9ICRlbGVtZW50LnBhcmVudHMoXCJmb3JtXCIpWzBdLFxuICAgICAgICAgICAgICAgIHZhbEluZm8sIHJ1bGVzLCBtZXNzYWdlcztcblxuICAgICAgICAgICAgaWYgKCFmb3JtKSB7ICAvLyBDYW5ub3QgZG8gY2xpZW50LXNpZGUgdmFsaWRhdGlvbiB3aXRob3V0IGEgZm9ybVxuICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgdmFsSW5mbyA9IHZhbGlkYXRpb25JbmZvKGZvcm0pO1xuICAgICAgICAgICAgdmFsSW5mby5vcHRpb25zLnJ1bGVzW2VsZW1lbnQubmFtZV0gPSBydWxlcyA9IHt9O1xuICAgICAgICAgICAgdmFsSW5mby5vcHRpb25zLm1lc3NhZ2VzW2VsZW1lbnQubmFtZV0gPSBtZXNzYWdlcyA9IHt9O1xuXG4gICAgICAgICAgICAkLmVhY2godGhpcy5hZGFwdGVycywgZnVuY3Rpb24gKCkge1xuICAgICAgICAgICAgICAgIHZhciBwcmVmaXggPSBcImRhdGEtdmFsLVwiICsgdGhpcy5uYW1lLFxuICAgICAgICAgICAgICAgICAgICBtZXNzYWdlID0gJGVsZW1lbnQuYXR0cihwcmVmaXgpLFxuICAgICAgICAgICAgICAgICAgICBwYXJhbVZhbHVlcyA9IHt9O1xuXG4gICAgICAgICAgICAgICAgaWYgKG1lc3NhZ2UgIT09IHVuZGVmaW5lZCkgeyAgLy8gQ29tcGFyZSBhZ2FpbnN0IHVuZGVmaW5lZCwgYmVjYXVzZSBhbiBlbXB0eSBtZXNzYWdlIGlzIGxlZ2FsIChhbmQgZmFsc3kpXG4gICAgICAgICAgICAgICAgICAgIHByZWZpeCArPSBcIi1cIjtcblxuICAgICAgICAgICAgICAgICAgICAkLmVhY2godGhpcy5wYXJhbXMsIGZ1bmN0aW9uICgpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgIHBhcmFtVmFsdWVzW3RoaXNdID0gJGVsZW1lbnQuYXR0cihwcmVmaXggKyB0aGlzKTtcbiAgICAgICAgICAgICAgICAgICAgfSk7XG5cbiAgICAgICAgICAgICAgICAgICAgdGhpcy5hZGFwdCh7XG4gICAgICAgICAgICAgICAgICAgICAgICBlbGVtZW50OiBlbGVtZW50LFxuICAgICAgICAgICAgICAgICAgICAgICAgZm9ybTogZm9ybSxcbiAgICAgICAgICAgICAgICAgICAgICAgIG1lc3NhZ2U6IG1lc3NhZ2UsXG4gICAgICAgICAgICAgICAgICAgICAgICBwYXJhbXM6IHBhcmFtVmFsdWVzLFxuICAgICAgICAgICAgICAgICAgICAgICAgcnVsZXM6IHJ1bGVzLFxuICAgICAgICAgICAgICAgICAgICAgICAgbWVzc2FnZXM6IG1lc3NhZ2VzXG4gICAgICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH0pO1xuXG4gICAgICAgICAgICAkLmV4dGVuZChydWxlcywgeyBcIl9fZHVtbXlfX1wiOiB0cnVlIH0pO1xuXG4gICAgICAgICAgICBpZiAoIXNraXBBdHRhY2gpIHtcbiAgICAgICAgICAgICAgICB2YWxJbmZvLmF0dGFjaFZhbGlkYXRpb24oKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSxcblxuICAgICAgICBwYXJzZTogZnVuY3Rpb24gKHNlbGVjdG9yKSB7XG4gICAgICAgICAgICAvLy8gPHN1bW1hcnk+XG4gICAgICAgICAgICAvLy8gUGFyc2VzIGFsbCB0aGUgSFRNTCBlbGVtZW50cyBpbiB0aGUgc3BlY2lmaWVkIHNlbGVjdG9yLiBJdCBsb29rcyBmb3IgaW5wdXQgZWxlbWVudHMgZGVjb3JhdGVkXG4gICAgICAgICAgICAvLy8gd2l0aCB0aGUgW2RhdGEtdmFsPXRydWVdIGF0dHJpYnV0ZSB2YWx1ZSBhbmQgZW5hYmxlcyB2YWxpZGF0aW9uIGFjY29yZGluZyB0byB0aGUgZGF0YS12YWwtKlxuICAgICAgICAgICAgLy8vIGF0dHJpYnV0ZSB2YWx1ZXMuXG4gICAgICAgICAgICAvLy8gPC9zdW1tYXJ5PlxuICAgICAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwic2VsZWN0b3JcIiB0eXBlPVwiU3RyaW5nXCI+QW55IHZhbGlkIGpRdWVyeSBzZWxlY3Rvci48L3BhcmFtPlxuXG4gICAgICAgICAgICAvLyAkZm9ybXMgaW5jbHVkZXMgYWxsIGZvcm1zIGluIHNlbGVjdG9yJ3MgRE9NIGhpZXJhcmNoeSAocGFyZW50LCBjaGlsZHJlbiBhbmQgc2VsZikgdGhhdCBoYXZlIGF0IGxlYXN0IG9uZVxuICAgICAgICAgICAgLy8gZWxlbWVudCB3aXRoIGRhdGEtdmFsPXRydWVcbiAgICAgICAgICAgIHZhciAkc2VsZWN0b3IgPSAkKHNlbGVjdG9yKSxcbiAgICAgICAgICAgICAgICAkZm9ybXMgPSAkc2VsZWN0b3IucGFyZW50cygpXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmFkZEJhY2soKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5maWx0ZXIoXCJmb3JtXCIpXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmFkZCgkc2VsZWN0b3IuZmluZChcImZvcm1cIikpXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmhhcyhcIltkYXRhLXZhbD10cnVlXVwiKTtcblxuICAgICAgICAgICAgJHNlbGVjdG9yLmZpbmQoXCJbZGF0YS12YWw9dHJ1ZV1cIikuZWFjaChmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgJGpRdmFsLnVub2J0cnVzaXZlLnBhcnNlRWxlbWVudCh0aGlzLCB0cnVlKTtcbiAgICAgICAgICAgIH0pO1xuXG4gICAgICAgICAgICAkZm9ybXMuZWFjaChmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgdmFyIGluZm8gPSB2YWxpZGF0aW9uSW5mbyh0aGlzKTtcbiAgICAgICAgICAgICAgICBpZiAoaW5mbykge1xuICAgICAgICAgICAgICAgICAgICBpbmZvLmF0dGFjaFZhbGlkYXRpb24oKTtcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfVxuICAgIH07XG5cbiAgICBhZGFwdGVycyA9ICRqUXZhbC51bm9idHJ1c2l2ZS5hZGFwdGVycztcblxuICAgIGFkYXB0ZXJzLmFkZCA9IGZ1bmN0aW9uIChhZGFwdGVyTmFtZSwgcGFyYW1zLCBmbikge1xuICAgICAgICAvLy8gPHN1bW1hcnk+QWRkcyBhIG5ldyBhZGFwdGVyIHRvIGNvbnZlcnQgdW5vYnRydXNpdmUgSFRNTCBpbnRvIGEgalF1ZXJ5IFZhbGlkYXRlIHZhbGlkYXRpb24uPC9zdW1tYXJ5PlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJhZGFwdGVyTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgYWRhcHRlciB0byBiZSBhZGRlZC4gVGhpcyBtYXRjaGVzIHRoZSBuYW1lIHVzZWRcbiAgICAgICAgLy8vIGluIHRoZSBkYXRhLXZhbC1ubm5uIEhUTUwgYXR0cmlidXRlICh3aGVyZSBubm5uIGlzIHRoZSBhZGFwdGVyIG5hbWUpLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInBhcmFtc1wiIHR5cGU9XCJBcnJheVwiIG9wdGlvbmFsPVwidHJ1ZVwiPltPcHRpb25hbF0gQW4gYXJyYXkgb2YgcGFyYW1ldGVyIG5hbWVzIChzdHJpbmdzKSB0aGF0IHdpbGxcbiAgICAgICAgLy8vIGJlIGV4dHJhY3RlZCBmcm9tIHRoZSBkYXRhLXZhbC1ubm5uLW1tbW0gSFRNTCBhdHRyaWJ1dGVzICh3aGVyZSBubm5uIGlzIHRoZSBhZGFwdGVyIG5hbWUsIGFuZFxuICAgICAgICAvLy8gbW1tbSBpcyB0aGUgcGFyYW1ldGVyIG5hbWUpLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImZuXCIgdHlwZT1cIkZ1bmN0aW9uXCI+VGhlIGZ1bmN0aW9uIHRvIGNhbGwsIHdoaWNoIGFkYXB0cyB0aGUgdmFsdWVzIGZyb20gdGhlIEhUTUxcbiAgICAgICAgLy8vIGF0dHJpYnV0ZXMgaW50byBqUXVlcnkgVmFsaWRhdGUgcnVsZXMgYW5kL29yIG1lc3NhZ2VzLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cmV0dXJucyB0eXBlPVwialF1ZXJ5LnZhbGlkYXRvci51bm9idHJ1c2l2ZS5hZGFwdGVyc1wiIC8+XG4gICAgICAgIGlmICghZm4pIHsgIC8vIENhbGxlZCB3aXRoIG5vIHBhcmFtcywganVzdCBhIGZ1bmN0aW9uXG4gICAgICAgICAgICBmbiA9IHBhcmFtcztcbiAgICAgICAgICAgIHBhcmFtcyA9IFtdO1xuICAgICAgICB9XG4gICAgICAgIHRoaXMucHVzaCh7IG5hbWU6IGFkYXB0ZXJOYW1lLCBwYXJhbXM6IHBhcmFtcywgYWRhcHQ6IGZuIH0pO1xuICAgICAgICByZXR1cm4gdGhpcztcbiAgICB9O1xuXG4gICAgYWRhcHRlcnMuYWRkQm9vbCA9IGZ1bmN0aW9uIChhZGFwdGVyTmFtZSwgcnVsZU5hbWUpIHtcbiAgICAgICAgLy8vIDxzdW1tYXJ5PkFkZHMgYSBuZXcgYWRhcHRlciB0byBjb252ZXJ0IHVub2J0cnVzaXZlIEhUTUwgaW50byBhIGpRdWVyeSBWYWxpZGF0ZSB2YWxpZGF0aW9uLCB3aGVyZVxuICAgICAgICAvLy8gdGhlIGpRdWVyeSBWYWxpZGF0ZSB2YWxpZGF0aW9uIHJ1bGUgaGFzIG5vIHBhcmFtZXRlciB2YWx1ZXMuPC9zdW1tYXJ5PlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJhZGFwdGVyTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgYWRhcHRlciB0byBiZSBhZGRlZC4gVGhpcyBtYXRjaGVzIHRoZSBuYW1lIHVzZWRcbiAgICAgICAgLy8vIGluIHRoZSBkYXRhLXZhbC1ubm5uIEhUTUwgYXR0cmlidXRlICh3aGVyZSBubm5uIGlzIHRoZSBhZGFwdGVyIG5hbWUpLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInJ1bGVOYW1lXCIgdHlwZT1cIlN0cmluZ1wiIG9wdGlvbmFsPVwidHJ1ZVwiPltPcHRpb25hbF0gVGhlIG5hbWUgb2YgdGhlIGpRdWVyeSBWYWxpZGF0ZSBydWxlLiBJZiBub3QgcHJvdmlkZWQsIHRoZSB2YWx1ZVxuICAgICAgICAvLy8gb2YgYWRhcHRlck5hbWUgd2lsbCBiZSB1c2VkIGluc3RlYWQuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxyZXR1cm5zIHR5cGU9XCJqUXVlcnkudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzXCIgLz5cbiAgICAgICAgcmV0dXJuIHRoaXMuYWRkKGFkYXB0ZXJOYW1lLCBmdW5jdGlvbiAob3B0aW9ucykge1xuICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBydWxlTmFtZSB8fCBhZGFwdGVyTmFtZSwgdHJ1ZSk7XG4gICAgICAgIH0pO1xuICAgIH07XG5cbiAgICBhZGFwdGVycy5hZGRNaW5NYXggPSBmdW5jdGlvbiAoYWRhcHRlck5hbWUsIG1pblJ1bGVOYW1lLCBtYXhSdWxlTmFtZSwgbWluTWF4UnVsZU5hbWUsIG1pbkF0dHJpYnV0ZSwgbWF4QXR0cmlidXRlKSB7XG4gICAgICAgIC8vLyA8c3VtbWFyeT5BZGRzIGEgbmV3IGFkYXB0ZXIgdG8gY29udmVydCB1bm9idHJ1c2l2ZSBIVE1MIGludG8gYSBqUXVlcnkgVmFsaWRhdGUgdmFsaWRhdGlvbiwgd2hlcmVcbiAgICAgICAgLy8vIHRoZSBqUXVlcnkgVmFsaWRhdGUgdmFsaWRhdGlvbiBoYXMgdGhyZWUgcG90ZW50aWFsIHJ1bGVzIChvbmUgZm9yIG1pbi1vbmx5LCBvbmUgZm9yIG1heC1vbmx5LCBhbmRcbiAgICAgICAgLy8vIG9uZSBmb3IgbWluLWFuZC1tYXgpLiBUaGUgSFRNTCBwYXJhbWV0ZXJzIGFyZSBleHBlY3RlZCB0byBiZSBuYW1lZCAtbWluIGFuZCAtbWF4Ljwvc3VtbWFyeT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwiYWRhcHRlck5hbWVcIiB0eXBlPVwiU3RyaW5nXCI+VGhlIG5hbWUgb2YgdGhlIGFkYXB0ZXIgdG8gYmUgYWRkZWQuIFRoaXMgbWF0Y2hlcyB0aGUgbmFtZSB1c2VkXG4gICAgICAgIC8vLyBpbiB0aGUgZGF0YS12YWwtbm5ubiBIVE1MIGF0dHJpYnV0ZSAod2hlcmUgbm5ubiBpcyB0aGUgYWRhcHRlciBuYW1lKS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJtaW5SdWxlTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgalF1ZXJ5IFZhbGlkYXRlIHJ1bGUgdG8gYmUgdXNlZCB3aGVuIHlvdSBvbmx5XG4gICAgICAgIC8vLyBoYXZlIGEgbWluaW11bSB2YWx1ZS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJtYXhSdWxlTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgalF1ZXJ5IFZhbGlkYXRlIHJ1bGUgdG8gYmUgdXNlZCB3aGVuIHlvdSBvbmx5XG4gICAgICAgIC8vLyBoYXZlIGEgbWF4aW11bSB2YWx1ZS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJtaW5NYXhSdWxlTmFtZVwiIHR5cGU9XCJTdHJpbmdcIj5UaGUgbmFtZSBvZiB0aGUgalF1ZXJ5IFZhbGlkYXRlIHJ1bGUgdG8gYmUgdXNlZCB3aGVuIHlvdVxuICAgICAgICAvLy8gaGF2ZSBib3RoIGEgbWluaW11bSBhbmQgbWF4aW11bSB2YWx1ZS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJtaW5BdHRyaWJ1dGVcIiB0eXBlPVwiU3RyaW5nXCIgb3B0aW9uYWw9XCJ0cnVlXCI+W09wdGlvbmFsXSBUaGUgbmFtZSBvZiB0aGUgSFRNTCBhdHRyaWJ1dGUgdGhhdFxuICAgICAgICAvLy8gY29udGFpbnMgdGhlIG1pbmltdW0gdmFsdWUuIFRoZSBkZWZhdWx0IGlzIFwibWluXCIuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxwYXJhbSBuYW1lPVwibWF4QXR0cmlidXRlXCIgdHlwZT1cIlN0cmluZ1wiIG9wdGlvbmFsPVwidHJ1ZVwiPltPcHRpb25hbF0gVGhlIG5hbWUgb2YgdGhlIEhUTUwgYXR0cmlidXRlIHRoYXRcbiAgICAgICAgLy8vIGNvbnRhaW5zIHRoZSBtYXhpbXVtIHZhbHVlLiBUaGUgZGVmYXVsdCBpcyBcIm1heFwiLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cmV0dXJucyB0eXBlPVwialF1ZXJ5LnZhbGlkYXRvci51bm9idHJ1c2l2ZS5hZGFwdGVyc1wiIC8+XG4gICAgICAgIHJldHVybiB0aGlzLmFkZChhZGFwdGVyTmFtZSwgW21pbkF0dHJpYnV0ZSB8fCBcIm1pblwiLCBtYXhBdHRyaWJ1dGUgfHwgXCJtYXhcIl0sIGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gICAgICAgICAgICB2YXIgbWluID0gb3B0aW9ucy5wYXJhbXMubWluLFxuICAgICAgICAgICAgICAgIG1heCA9IG9wdGlvbnMucGFyYW1zLm1heDtcblxuICAgICAgICAgICAgaWYgKG1pbiAmJiBtYXgpIHtcbiAgICAgICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIG1pbk1heFJ1bGVOYW1lLCBbbWluLCBtYXhdKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICAgIGVsc2UgaWYgKG1pbikge1xuICAgICAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgbWluUnVsZU5hbWUsIG1pbik7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBlbHNlIGlmIChtYXgpIHtcbiAgICAgICAgICAgICAgICBzZXRWYWxpZGF0aW9uVmFsdWVzKG9wdGlvbnMsIG1heFJ1bGVOYW1lLCBtYXgpO1xuICAgICAgICAgICAgfVxuICAgICAgICB9KTtcbiAgICB9O1xuXG4gICAgYWRhcHRlcnMuYWRkU2luZ2xlVmFsID0gZnVuY3Rpb24gKGFkYXB0ZXJOYW1lLCBhdHRyaWJ1dGUsIHJ1bGVOYW1lKSB7XG4gICAgICAgIC8vLyA8c3VtbWFyeT5BZGRzIGEgbmV3IGFkYXB0ZXIgdG8gY29udmVydCB1bm9idHJ1c2l2ZSBIVE1MIGludG8gYSBqUXVlcnkgVmFsaWRhdGUgdmFsaWRhdGlvbiwgd2hlcmVcbiAgICAgICAgLy8vIHRoZSBqUXVlcnkgVmFsaWRhdGUgdmFsaWRhdGlvbiBydWxlIGhhcyBhIHNpbmdsZSB2YWx1ZS48L3N1bW1hcnk+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cImFkYXB0ZXJOYW1lXCIgdHlwZT1cIlN0cmluZ1wiPlRoZSBuYW1lIG9mIHRoZSBhZGFwdGVyIHRvIGJlIGFkZGVkLiBUaGlzIG1hdGNoZXMgdGhlIG5hbWUgdXNlZFxuICAgICAgICAvLy8gaW4gdGhlIGRhdGEtdmFsLW5ubm4gSFRNTCBhdHRyaWJ1dGUod2hlcmUgbm5ubiBpcyB0aGUgYWRhcHRlciBuYW1lKS48L3BhcmFtPlxuICAgICAgICAvLy8gPHBhcmFtIG5hbWU9XCJhdHRyaWJ1dGVcIiB0eXBlPVwiU3RyaW5nXCI+W09wdGlvbmFsXSBUaGUgbmFtZSBvZiB0aGUgSFRNTCBhdHRyaWJ1dGUgdGhhdCBjb250YWlucyB0aGUgdmFsdWUuXG4gICAgICAgIC8vLyBUaGUgZGVmYXVsdCBpcyBcInZhbFwiLjwvcGFyYW0+XG4gICAgICAgIC8vLyA8cGFyYW0gbmFtZT1cInJ1bGVOYW1lXCIgdHlwZT1cIlN0cmluZ1wiIG9wdGlvbmFsPVwidHJ1ZVwiPltPcHRpb25hbF0gVGhlIG5hbWUgb2YgdGhlIGpRdWVyeSBWYWxpZGF0ZSBydWxlLiBJZiBub3QgcHJvdmlkZWQsIHRoZSB2YWx1ZVxuICAgICAgICAvLy8gb2YgYWRhcHRlck5hbWUgd2lsbCBiZSB1c2VkIGluc3RlYWQuPC9wYXJhbT5cbiAgICAgICAgLy8vIDxyZXR1cm5zIHR5cGU9XCJqUXVlcnkudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzXCIgLz5cbiAgICAgICAgcmV0dXJuIHRoaXMuYWRkKGFkYXB0ZXJOYW1lLCBbYXR0cmlidXRlIHx8IFwidmFsXCJdLCBmdW5jdGlvbiAob3B0aW9ucykge1xuICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBydWxlTmFtZSB8fCBhZGFwdGVyTmFtZSwgb3B0aW9ucy5wYXJhbXNbYXR0cmlidXRlXSk7XG4gICAgICAgIH0pO1xuICAgIH07XG5cbiAgICAkalF2YWwuYWRkTWV0aG9kKFwiX19kdW1teV9fXCIsIGZ1bmN0aW9uICh2YWx1ZSwgZWxlbWVudCwgcGFyYW1zKSB7XG4gICAgICAgIHJldHVybiB0cnVlO1xuICAgIH0pO1xuXG4gICAgJGpRdmFsLmFkZE1ldGhvZChcInJlZ2V4XCIsIGZ1bmN0aW9uICh2YWx1ZSwgZWxlbWVudCwgcGFyYW1zKSB7XG4gICAgICAgIHZhciBtYXRjaDtcbiAgICAgICAgaWYgKHRoaXMub3B0aW9uYWwoZWxlbWVudCkpIHtcbiAgICAgICAgICAgIHJldHVybiB0cnVlO1xuICAgICAgICB9XG5cbiAgICAgICAgbWF0Y2ggPSBuZXcgUmVnRXhwKHBhcmFtcykuZXhlYyh2YWx1ZSk7XG4gICAgICAgIHJldHVybiAobWF0Y2ggJiYgKG1hdGNoLmluZGV4ID09PSAwKSAmJiAobWF0Y2hbMF0ubGVuZ3RoID09PSB2YWx1ZS5sZW5ndGgpKTtcbiAgICB9KTtcblxuICAgICRqUXZhbC5hZGRNZXRob2QoXCJub25hbHBoYW1pblwiLCBmdW5jdGlvbiAodmFsdWUsIGVsZW1lbnQsIG5vbmFscGhhbWluKSB7XG4gICAgICAgIHZhciBtYXRjaDtcbiAgICAgICAgaWYgKG5vbmFscGhhbWluKSB7XG4gICAgICAgICAgICBtYXRjaCA9IHZhbHVlLm1hdGNoKC9cXFcvZyk7XG4gICAgICAgICAgICBtYXRjaCA9IG1hdGNoICYmIG1hdGNoLmxlbmd0aCA+PSBub25hbHBoYW1pbjtcbiAgICAgICAgfVxuICAgICAgICByZXR1cm4gbWF0Y2g7XG4gICAgfSk7XG5cbiAgICBpZiAoJGpRdmFsLm1ldGhvZHMuZXh0ZW5zaW9uKSB7XG4gICAgICAgIGFkYXB0ZXJzLmFkZFNpbmdsZVZhbChcImFjY2VwdFwiLCBcIm1pbXR5cGVcIik7XG4gICAgICAgIGFkYXB0ZXJzLmFkZFNpbmdsZVZhbChcImV4dGVuc2lvblwiLCBcImV4dGVuc2lvblwiKTtcbiAgICB9IGVsc2Uge1xuICAgICAgICAvLyBmb3IgYmFja3dhcmQgY29tcGF0aWJpbGl0eSwgd2hlbiB0aGUgJ2V4dGVuc2lvbicgdmFsaWRhdGlvbiBtZXRob2QgZG9lcyBub3QgZXhpc3QsIHN1Y2ggYXMgd2l0aCB2ZXJzaW9uc1xuICAgICAgICAvLyBvZiBKUXVlcnkgVmFsaWRhdGlvbiBwbHVnaW4gcHJpb3IgdG8gMS4xMCwgd2Ugc2hvdWxkIHVzZSB0aGUgJ2FjY2VwdCcgbWV0aG9kIGZvclxuICAgICAgICAvLyB2YWxpZGF0aW5nIHRoZSBleHRlbnNpb24sIGFuZCBpZ25vcmUgbWltZS10eXBlIHZhbGlkYXRpb25zIGFzIHRoZXkgYXJlIG5vdCBzdXBwb3J0ZWQuXG4gICAgICAgIGFkYXB0ZXJzLmFkZFNpbmdsZVZhbChcImV4dGVuc2lvblwiLCBcImV4dGVuc2lvblwiLCBcImFjY2VwdFwiKTtcbiAgICB9XG5cbiAgICBhZGFwdGVycy5hZGRTaW5nbGVWYWwoXCJyZWdleFwiLCBcInBhdHRlcm5cIik7XG4gICAgYWRhcHRlcnMuYWRkQm9vbChcImNyZWRpdGNhcmRcIikuYWRkQm9vbChcImRhdGVcIikuYWRkQm9vbChcImRpZ2l0c1wiKS5hZGRCb29sKFwiZW1haWxcIikuYWRkQm9vbChcIm51bWJlclwiKS5hZGRCb29sKFwidXJsXCIpO1xuICAgIGFkYXB0ZXJzLmFkZE1pbk1heChcImxlbmd0aFwiLCBcIm1pbmxlbmd0aFwiLCBcIm1heGxlbmd0aFwiLCBcInJhbmdlbGVuZ3RoXCIpLmFkZE1pbk1heChcInJhbmdlXCIsIFwibWluXCIsIFwibWF4XCIsIFwicmFuZ2VcIik7XG4gICAgYWRhcHRlcnMuYWRkTWluTWF4KFwibWlubGVuZ3RoXCIsIFwibWlubGVuZ3RoXCIpLmFkZE1pbk1heChcIm1heGxlbmd0aFwiLCBcIm1pbmxlbmd0aFwiLCBcIm1heGxlbmd0aFwiKTtcbiAgICBhZGFwdGVycy5hZGQoXCJlcXVhbHRvXCIsIFtcIm90aGVyXCJdLCBmdW5jdGlvbiAob3B0aW9ucykge1xuICAgICAgICB2YXIgcHJlZml4ID0gZ2V0TW9kZWxQcmVmaXgob3B0aW9ucy5lbGVtZW50Lm5hbWUpLFxuICAgICAgICAgICAgb3RoZXIgPSBvcHRpb25zLnBhcmFtcy5vdGhlcixcbiAgICAgICAgICAgIGZ1bGxPdGhlck5hbWUgPSBhcHBlbmRNb2RlbFByZWZpeChvdGhlciwgcHJlZml4KSxcbiAgICAgICAgICAgIGVsZW1lbnQgPSAkKG9wdGlvbnMuZm9ybSkuZmluZChcIjppbnB1dFwiKS5maWx0ZXIoXCJbbmFtZT0nXCIgKyBlc2NhcGVBdHRyaWJ1dGVWYWx1ZShmdWxsT3RoZXJOYW1lKSArIFwiJ11cIilbMF07XG5cbiAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBcImVxdWFsVG9cIiwgZWxlbWVudCk7XG4gICAgfSk7XG4gICAgYWRhcHRlcnMuYWRkKFwicmVxdWlyZWRcIiwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgICAgICAgLy8galF1ZXJ5IFZhbGlkYXRlIGVxdWF0ZXMgXCJyZXF1aXJlZFwiIHdpdGggXCJtYW5kYXRvcnlcIiBmb3IgY2hlY2tib3ggZWxlbWVudHNcbiAgICAgICAgaWYgKG9wdGlvbnMuZWxlbWVudC50YWdOYW1lLnRvVXBwZXJDYXNlKCkgIT09IFwiSU5QVVRcIiB8fCBvcHRpb25zLmVsZW1lbnQudHlwZS50b1VwcGVyQ2FzZSgpICE9PSBcIkNIRUNLQk9YXCIpIHtcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJyZXF1aXJlZFwiLCB0cnVlKTtcbiAgICAgICAgfVxuICAgIH0pO1xuICAgIGFkYXB0ZXJzLmFkZChcInJlbW90ZVwiLCBbXCJ1cmxcIiwgXCJ0eXBlXCIsIFwiYWRkaXRpb25hbGZpZWxkc1wiXSwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgICAgICAgdmFyIHZhbHVlID0ge1xuICAgICAgICAgICAgdXJsOiBvcHRpb25zLnBhcmFtcy51cmwsXG4gICAgICAgICAgICB0eXBlOiBvcHRpb25zLnBhcmFtcy50eXBlIHx8IFwiR0VUXCIsXG4gICAgICAgICAgICBkYXRhOiB7fVxuICAgICAgICB9LFxuICAgICAgICAgICAgcHJlZml4ID0gZ2V0TW9kZWxQcmVmaXgob3B0aW9ucy5lbGVtZW50Lm5hbWUpO1xuXG4gICAgICAgICQuZWFjaChzcGxpdEFuZFRyaW0ob3B0aW9ucy5wYXJhbXMuYWRkaXRpb25hbGZpZWxkcyB8fCBvcHRpb25zLmVsZW1lbnQubmFtZSksIGZ1bmN0aW9uIChpLCBmaWVsZE5hbWUpIHtcbiAgICAgICAgICAgIHZhciBwYXJhbU5hbWUgPSBhcHBlbmRNb2RlbFByZWZpeChmaWVsZE5hbWUsIHByZWZpeCk7XG4gICAgICAgICAgICB2YWx1ZS5kYXRhW3BhcmFtTmFtZV0gPSBmdW5jdGlvbiAoKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuICQob3B0aW9ucy5mb3JtKS5maW5kKFwiOmlucHV0XCIpLmZpbHRlcihcIltuYW1lPSdcIiArIGVzY2FwZUF0dHJpYnV0ZVZhbHVlKHBhcmFtTmFtZSkgKyBcIiddXCIpLnZhbCgpO1xuICAgICAgICAgICAgfTtcbiAgICAgICAgfSk7XG5cbiAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBcInJlbW90ZVwiLCB2YWx1ZSk7XG4gICAgfSk7XG4gICAgYWRhcHRlcnMuYWRkKFwicGFzc3dvcmRcIiwgW1wibWluXCIsIFwibm9uYWxwaGFtaW5cIiwgXCJyZWdleFwiXSwgZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgICAgICAgaWYgKG9wdGlvbnMucGFyYW1zLm1pbikge1xuICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBcIm1pbmxlbmd0aFwiLCBvcHRpb25zLnBhcmFtcy5taW4pO1xuICAgICAgICB9XG4gICAgICAgIGlmIChvcHRpb25zLnBhcmFtcy5ub25hbHBoYW1pbikge1xuICAgICAgICAgICAgc2V0VmFsaWRhdGlvblZhbHVlcyhvcHRpb25zLCBcIm5vbmFscGhhbWluXCIsIG9wdGlvbnMucGFyYW1zLm5vbmFscGhhbWluKTtcbiAgICAgICAgfVxuICAgICAgICBpZiAob3B0aW9ucy5wYXJhbXMucmVnZXgpIHtcbiAgICAgICAgICAgIHNldFZhbGlkYXRpb25WYWx1ZXMob3B0aW9ucywgXCJyZWdleFwiLCBvcHRpb25zLnBhcmFtcy5yZWdleCk7XG4gICAgICAgIH1cbiAgICB9KTtcblxuICAgICQoZnVuY3Rpb24gKCkge1xuICAgICAgICAkalF2YWwudW5vYnRydXNpdmUucGFyc2UoZG9jdW1lbnQpO1xuICAgIH0pO1xufShqUXVlcnkpKTsiLCIoZnVuY3Rpb24gKCQpIHtcblxuICAgICQudmFsaWRhdG9yLmFkZE1ldGhvZChcIm9wdGlvbnJlcXVpcmVkXCIsIGZ1bmN0aW9uICh2YWx1ZSwgZWxlbWVudCwgcGFyYW0pIHtcbiAgICAgICAgdmFyIGlzVmFsaWQgPSB0cnVlO1xuXG4gICAgICAgIGlmICgkKGVsZW1lbnQpLmlzKFwiaW5wdXRcIikpIHtcbiAgICAgICAgICAgIHZhciBwYXJlbnQgPSAkKGVsZW1lbnQpLmNsb3Nlc3QoXCJvbFwiKTtcblxuICAgICAgICAgICAgaXNWYWxpZCA9IHBhcmVudC5maW5kKFwiaW5wdXQ6Y2hlY2tlZFwiKS5sZW5ndGggPiAwO1xuICAgICAgICAgICAgcGFyZW50LnRvZ2dsZUNsYXNzKFwiaW5wdXQtdmFsaWRhdGlvbi1lcnJvclwiLCAhaXNWYWxpZCk7XG4gICAgICAgIH1cbiAgICAgICAgZWxzZSBpZiAoJChlbGVtZW50KS5pcyhcInNlbGVjdFwiKSkge1xuICAgICAgICAgICAgdmFyIHYgPSAkKGVsZW1lbnQpLnZhbCgpO1xuICAgICAgICAgICAgaXNWYWxpZCA9ICEhdiAmJiB2Lmxlbmd0aCA+IDA7XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gaXNWYWxpZDtcbiAgICB9LCBcIkFuIG9wdGlvbiBpcyByZXF1aXJlZFwiKTtcblxuICAgICQudmFsaWRhdG9yLnVub2J0cnVzaXZlLmFkYXB0ZXJzLmFkZEJvb2woXCJtYW5kYXRvcnlcIiwgXCJyZXF1aXJlZFwiKTtcbiAgICAkLnZhbGlkYXRvci51bm9idHJ1c2l2ZS5hZGFwdGVycy5hZGRCb29sKFwib3B0aW9ucmVxdWlyZWRcIik7XG59KGpRdWVyeSkpOyIsIi8qKlxuICogQ0xEUiBKYXZhU2NyaXB0IExpYnJhcnkgdjAuNC4xXG4gKiBodHRwOi8vanF1ZXJ5LmNvbS9cbiAqXG4gKiBDb3B5cmlnaHQgMjAxMyBSYWZhZWwgWGF2aWVyIGRlIFNvdXphXG4gKiBSZWxlYXNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2VcbiAqIGh0dHA6Ly9qcXVlcnkub3JnL2xpY2Vuc2VcbiAqXG4gKiBEYXRlOiAyMDE1LTAyLTI1VDEzOjUxWlxuICovXG4vKiFcbiAqIENMRFIgSmF2YVNjcmlwdCBMaWJyYXJ5IHYwLjQuMSAyMDE1LTAyLTI1VDEzOjUxWiBNSVQgbGljZW5zZSDCqSBSYWZhZWwgWGF2aWVyXG4gKiBodHRwOi8vZ2l0LmlvL2g0bG1WZ1xuICovXG4oZnVuY3Rpb24oIHJvb3QsIGZhY3RvcnkgKSB7XG5cblx0aWYgKCB0eXBlb2YgZGVmaW5lID09PSBcImZ1bmN0aW9uXCIgJiYgZGVmaW5lLmFtZCApIHtcblx0XHQvLyBBTUQuXG5cdFx0ZGVmaW5lKCBmYWN0b3J5ICk7XG5cdH0gZWxzZSBpZiAoIHR5cGVvZiBtb2R1bGUgPT09IFwib2JqZWN0XCIgJiYgdHlwZW9mIG1vZHVsZS5leHBvcnRzID09PSBcIm9iamVjdFwiICkge1xuXHRcdC8vIE5vZGUuIENvbW1vbkpTLlxuXHRcdG1vZHVsZS5leHBvcnRzID0gZmFjdG9yeSgpO1xuXHR9IGVsc2Uge1xuXHRcdC8vIEdsb2JhbFxuXHRcdHJvb3QuQ2xkciA9IGZhY3RvcnkoKTtcblx0fVxuXG59KCB0aGlzLCBmdW5jdGlvbigpIHtcblxuXG5cdHZhciBhcnJheUlzQXJyYXkgPSBBcnJheS5pc0FycmF5IHx8IGZ1bmN0aW9uKCBvYmogKSB7XG5cdFx0cmV0dXJuIE9iamVjdC5wcm90b3R5cGUudG9TdHJpbmcuY2FsbCggb2JqICkgPT09IFwiW29iamVjdCBBcnJheV1cIjtcblx0fTtcblxuXG5cblxuXHR2YXIgcGF0aE5vcm1hbGl6ZSA9IGZ1bmN0aW9uKCBwYXRoLCBhdHRyaWJ1dGVzICkge1xuXHRcdGlmICggYXJyYXlJc0FycmF5KCBwYXRoICkgKSB7XG5cdFx0XHRwYXRoID0gcGF0aC5qb2luKCBcIi9cIiApO1xuXHRcdH1cblx0XHRpZiAoIHR5cGVvZiBwYXRoICE9PSBcInN0cmluZ1wiICkge1xuXHRcdFx0dGhyb3cgbmV3IEVycm9yKCBcImludmFsaWQgcGF0aCBcXFwiXCIgKyBwYXRoICsgXCJcXFwiXCIgKTtcblx0XHR9XG5cdFx0Ly8gMTogSWdub3JlIGxlYWRpbmcgc2xhc2ggYC9gXG5cdFx0Ly8gMjogSWdub3JlIGxlYWRpbmcgYGNsZHIvYFxuXHRcdHBhdGggPSBwYXRoXG5cdFx0XHQucmVwbGFjZSggL15cXC8vICwgXCJcIiApIC8qIDEgKi9cblx0XHRcdC5yZXBsYWNlKCAvXmNsZHJcXC8vICwgXCJcIiApOyAvKiAyICovXG5cblx0XHQvLyBSZXBsYWNlIHthdHRyaWJ1dGV9J3Ncblx0XHRwYXRoID0gcGF0aC5yZXBsYWNlKCAve1thLXpBLVpdK30vZywgZnVuY3Rpb24oIG5hbWUgKSB7XG5cdFx0XHRuYW1lID0gbmFtZS5yZXBsYWNlKCAvXnsoW159XSopfSQvLCBcIiQxXCIgKTtcblx0XHRcdHJldHVybiBhdHRyaWJ1dGVzWyBuYW1lIF07XG5cdFx0fSk7XG5cblx0XHRyZXR1cm4gcGF0aC5zcGxpdCggXCIvXCIgKTtcblx0fTtcblxuXG5cblxuXHR2YXIgYXJyYXlTb21lID0gZnVuY3Rpb24oIGFycmF5LCBjYWxsYmFjayApIHtcblx0XHR2YXIgaSwgbGVuZ3RoO1xuXHRcdGlmICggYXJyYXkuc29tZSApIHtcblx0XHRcdHJldHVybiBhcnJheS5zb21lKCBjYWxsYmFjayApO1xuXHRcdH1cblx0XHRmb3IgKCBpID0gMCwgbGVuZ3RoID0gYXJyYXkubGVuZ3RoOyBpIDwgbGVuZ3RoOyBpKysgKSB7XG5cdFx0XHRpZiAoIGNhbGxiYWNrKCBhcnJheVsgaSBdLCBpLCBhcnJheSApICkge1xuXHRcdFx0XHRyZXR1cm4gdHJ1ZTtcblx0XHRcdH1cblx0XHR9XG5cdFx0cmV0dXJuIGZhbHNlO1xuXHR9O1xuXG5cblxuXG5cdC8qKlxuXHQgKiBSZXR1cm4gdGhlIG1heGltaXplZCBsYW5ndWFnZSBpZCBhcyBkZWZpbmVkIGluXG5cdCAqIGh0dHA6Ly93d3cudW5pY29kZS5vcmcvcmVwb3J0cy90cjM1LyNMaWtlbHlfU3VidGFnc1xuXHQgKiAxLiBDYW5vbmljYWxpemUuXG5cdCAqIDEuMSBNYWtlIHN1cmUgdGhlIGlucHV0IGxvY2FsZSBpcyBpbiBjYW5vbmljYWwgZm9ybTogdXNlcyB0aGUgcmlnaHRcblx0ICogc2VwYXJhdG9yLCBhbmQgaGFzIHRoZSByaWdodCBjYXNpbmcuXG5cdCAqIFRPRE8gUmlnaHQgY2FzaW5nPyBXaGF0IGRmPyBJdCBzZWVtcyBsYW5ndWFnZXMgYXJlIGxvd2VyY2FzZSwgc2NyaXB0cyBhcmVcblx0ICogQ2FwaXRhbGl6ZWQsIHRlcnJpdG9yeSBpcyB1cHBlcmNhc2UuIEkgYW0gbGVhdmluZyB0aGlzIGFzIGFuIGV4ZXJjaXNlIHRvXG5cdCAqIHRoZSB1c2VyLlxuXHQgKlxuXHQgKiAxLjIgUmVwbGFjZSBhbnkgZGVwcmVjYXRlZCBzdWJ0YWdzIHdpdGggdGhlaXIgY2Fub25pY2FsIHZhbHVlcyB1c2luZyB0aGVcblx0ICogPGFsaWFzPiBkYXRhIGluIHN1cHBsZW1lbnRhbCBtZXRhZGF0YS4gVXNlIHRoZSBmaXJzdCB2YWx1ZSBpbiB0aGVcblx0ICogcmVwbGFjZW1lbnQgbGlzdCwgaWYgaXQgZXhpc3RzLiBMYW5ndWFnZSB0YWcgcmVwbGFjZW1lbnRzIG1heSBoYXZlIG11bHRpcGxlXG5cdCAqIHBhcnRzLCBzdWNoIGFzIFwic2hcIiDinp4gXCJzcl9MYXRuXCIgb3IgbW9cIiDinp4gXCJyb19NRFwiLiBJbiBzdWNoIGEgY2FzZSwgdGhlXG5cdCAqIG9yaWdpbmFsIHNjcmlwdCBhbmQvb3IgcmVnaW9uIGFyZSByZXRhaW5lZCBpZiB0aGVyZSBpcyBvbmUuIFRodXNcblx0ICogXCJzaF9BcmFiX0FRXCIg4p6eIFwic3JfQXJhYl9BUVwiLCBub3QgXCJzcl9MYXRuX0FRXCIuXG5cdCAqIFRPRE8gV2hhdCA8YWxpYXM+IGRhdGE/XG5cdCAqXG5cdCAqIDEuMyBJZiB0aGUgdGFnIGlzIGdyYW5kZmF0aGVyZWQgKHNlZSA8dmFyaWFibGUgaWQ9XCIkZ3JhbmRmYXRoZXJlZFwiXG5cdCAqIHR5cGU9XCJjaG9pY2VcIj4gaW4gdGhlIHN1cHBsZW1lbnRhbCBkYXRhKSwgdGhlbiByZXR1cm4gaXQuXG5cdCAqIFRPRE8gZ3JhbmRmYXRoZXJlZD9cblx0ICpcblx0ICogMS40IFJlbW92ZSB0aGUgc2NyaXB0IGNvZGUgJ1p6enonIGFuZCB0aGUgcmVnaW9uIGNvZGUgJ1paJyBpZiB0aGV5IG9jY3VyLlxuXHQgKiAxLjUgR2V0IHRoZSBjb21wb25lbnRzIG9mIHRoZSBjbGVhbmVkLXVwIHNvdXJjZSB0YWcgKGxhbmd1YWdlcywgc2NyaXB0cyxcblx0ICogYW5kIHJlZ2lvbnMpLCBwbHVzIGFueSB2YXJpYW50cyBhbmQgZXh0ZW5zaW9ucy5cblx0ICogMi4gTG9va3VwLiBMb29rdXAgZWFjaCBvZiB0aGUgZm9sbG93aW5nIGluIG9yZGVyLCBhbmQgc3RvcCBvbiB0aGUgZmlyc3Rcblx0ICogbWF0Y2g6XG5cdCAqIDIuMSBsYW5ndWFnZXNfc2NyaXB0c19yZWdpb25zXG5cdCAqIDIuMiBsYW5ndWFnZXNfcmVnaW9uc1xuXHQgKiAyLjMgbGFuZ3VhZ2VzX3NjcmlwdHNcblx0ICogMi40IGxhbmd1YWdlc1xuXHQgKiAyLjUgdW5kX3NjcmlwdHNcblx0ICogMy4gUmV0dXJuXG5cdCAqIDMuMSBJZiB0aGVyZSBpcyBubyBtYXRjaCwgZWl0aGVyIHJldHVybiBhbiBlcnJvciB2YWx1ZSwgb3IgdGhlIG1hdGNoIGZvclxuXHQgKiBcInVuZFwiIChpbiBBUElzIHdoZXJlIGEgdmFsaWQgbGFuZ3VhZ2UgdGFnIGlzIHJlcXVpcmVkKS5cblx0ICogMy4yIE90aGVyd2lzZSB0aGVyZSBpcyBhIG1hdGNoID0gbGFuZ3VhZ2VtX3NjcmlwdG1fcmVnaW9ubVxuXHQgKiAzLjMgTGV0IHhyID0geHMgaWYgeHMgaXMgbm90IGVtcHR5LCBhbmQgeG0gb3RoZXJ3aXNlLlxuXHQgKiAzLjQgUmV0dXJuIHRoZSBsYW5ndWFnZSB0YWcgY29tcG9zZWQgb2YgbGFuZ3VhZ2VyIF8gc2NyaXB0ciBfIHJlZ2lvbnIgK1xuXHQgKiB2YXJpYW50cyArIGV4dGVuc2lvbnMuXG5cdCAqXG5cdCAqIEBzdWJ0YWdzIFtBcnJheV0gbm9ybWFsaXplZCBsYW5ndWFnZSBpZCBzdWJ0YWdzIHR1cGxlIChzZWUgaW5pdC5qcykuXG5cdCAqL1xuXHR2YXIgY29yZUxpa2VseVN1YnRhZ3MgPSBmdW5jdGlvbiggQ2xkciwgY2xkciwgc3VidGFncywgb3B0aW9ucyApIHtcblx0XHR2YXIgbWF0Y2gsIG1hdGNoRm91bmQsXG5cdFx0XHRsYW5ndWFnZSA9IHN1YnRhZ3NbIDAgXSxcblx0XHRcdHNjcmlwdCA9IHN1YnRhZ3NbIDEgXSxcblx0XHRcdHNlcCA9IENsZHIubG9jYWxlU2VwLFxuXHRcdFx0dGVycml0b3J5ID0gc3VidGFnc1sgMiBdO1xuXHRcdG9wdGlvbnMgPSBvcHRpb25zIHx8IHt9O1xuXG5cdFx0Ly8gU2tpcCBpZiAobGFuZ3VhZ2UsIHNjcmlwdCwgdGVycml0b3J5KSBpcyBub3QgZW1wdHkgWzMuM11cblx0XHRpZiAoIGxhbmd1YWdlICE9PSBcInVuZFwiICYmIHNjcmlwdCAhPT0gXCJaenp6XCIgJiYgdGVycml0b3J5ICE9PSBcIlpaXCIgKSB7XG5cdFx0XHRyZXR1cm4gWyBsYW5ndWFnZSwgc2NyaXB0LCB0ZXJyaXRvcnkgXTtcblx0XHR9XG5cblx0XHQvLyBTa2lwIGlmIG5vIHN1cHBsZW1lbnRhbCBsaWtlbHlTdWJ0YWdzIGRhdGEgaXMgcHJlc2VudFxuXHRcdGlmICggdHlwZW9mIGNsZHIuZ2V0KCBcInN1cHBsZW1lbnRhbC9saWtlbHlTdWJ0YWdzXCIgKSA9PT0gXCJ1bmRlZmluZWRcIiApIHtcblx0XHRcdHJldHVybjtcblx0XHR9XG5cblx0XHQvLyBbMl1cblx0XHRtYXRjaEZvdW5kID0gYXJyYXlTb21lKFtcblx0XHRcdFsgbGFuZ3VhZ2UsIHNjcmlwdCwgdGVycml0b3J5IF0sXG5cdFx0XHRbIGxhbmd1YWdlLCB0ZXJyaXRvcnkgXSxcblx0XHRcdFsgbGFuZ3VhZ2UsIHNjcmlwdCBdLFxuXHRcdFx0WyBsYW5ndWFnZSBdLFxuXHRcdFx0WyBcInVuZFwiLCBzY3JpcHQgXVxuXHRcdF0sIGZ1bmN0aW9uKCB0ZXN0ICkge1xuXHRcdFx0cmV0dXJuIG1hdGNoID0gISgvXFxiKFp6enp8WlopXFxiLykudGVzdCggdGVzdC5qb2luKCBzZXAgKSApIC8qIFsxLjRdICovICYmIGNsZHIuZ2V0KCBbIFwic3VwcGxlbWVudGFsL2xpa2VseVN1YnRhZ3NcIiwgdGVzdC5qb2luKCBzZXAgKSBdICk7XG5cdFx0fSk7XG5cblx0XHQvLyBbM11cblx0XHRpZiAoIG1hdGNoRm91bmQgKSB7XG5cdFx0XHQvLyBbMy4yIC4uIDMuNF1cblx0XHRcdG1hdGNoID0gbWF0Y2guc3BsaXQoIHNlcCApO1xuXHRcdFx0cmV0dXJuIFtcblx0XHRcdFx0bGFuZ3VhZ2UgIT09IFwidW5kXCIgPyBsYW5ndWFnZSA6IG1hdGNoWyAwIF0sXG5cdFx0XHRcdHNjcmlwdCAhPT0gXCJaenp6XCIgPyBzY3JpcHQgOiBtYXRjaFsgMSBdLFxuXHRcdFx0XHR0ZXJyaXRvcnkgIT09IFwiWlpcIiA/IHRlcnJpdG9yeSA6IG1hdGNoWyAyIF1cblx0XHRcdF07XG5cdFx0fSBlbHNlIGlmICggb3B0aW9ucy5mb3JjZSApIHtcblx0XHRcdC8vIFszLjEuMl1cblx0XHRcdHJldHVybiBjbGRyLmdldCggXCJzdXBwbGVtZW50YWwvbGlrZWx5U3VidGFncy91bmRcIiApLnNwbGl0KCBzZXAgKTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0Ly8gWzMuMS4xXVxuXHRcdFx0cmV0dXJuO1xuXHRcdH1cblx0fTtcblxuXG5cblx0LyoqXG5cdCAqIEdpdmVuIGEgbG9jYWxlLCByZW1vdmUgYW55IGZpZWxkcyB0aGF0IEFkZCBMaWtlbHkgU3VidGFncyB3b3VsZCBhZGQuXG5cdCAqIGh0dHA6Ly93d3cudW5pY29kZS5vcmcvcmVwb3J0cy90cjM1LyNMaWtlbHlfU3VidGFnc1xuXHQgKiAxLiBGaXJzdCBnZXQgbWF4ID0gQWRkTGlrZWx5U3VidGFncyhpbnB1dExvY2FsZSkuIElmIGFuIGVycm9yIGlzIHNpZ25hbGVkLFxuXHQgKiByZXR1cm4gaXQuXG5cdCAqIDIuIFJlbW92ZSB0aGUgdmFyaWFudHMgZnJvbSBtYXguXG5cdCAqIDMuIFRoZW4gZm9yIHRyaWFsIGluIHtsYW5ndWFnZSwgbGFuZ3VhZ2UgXyByZWdpb24sIGxhbmd1YWdlIF8gc2NyaXB0fS4gSWZcblx0ICogQWRkTGlrZWx5U3VidGFncyh0cmlhbCkgPSBtYXgsIHRoZW4gcmV0dXJuIHRyaWFsICsgdmFyaWFudHMuXG5cdCAqIDQuIElmIHlvdSBkbyBub3QgZ2V0IGEgbWF0Y2gsIHJldHVybiBtYXggKyB2YXJpYW50cy5cblx0ICogXG5cdCAqIEBtYXhMYW5ndWFnZUlkIFtBcnJheV0gbWF4TGFuZ3VhZ2VJZCB0dXBsZSAoc2VlIGluaXQuanMpLlxuXHQgKi9cblx0dmFyIGNvcmVSZW1vdmVMaWtlbHlTdWJ0YWdzID0gZnVuY3Rpb24oIENsZHIsIGNsZHIsIG1heExhbmd1YWdlSWQgKSB7XG5cdFx0dmFyIG1hdGNoLCBtYXRjaEZvdW5kLFxuXHRcdFx0bGFuZ3VhZ2UgPSBtYXhMYW5ndWFnZUlkWyAwIF0sXG5cdFx0XHRzY3JpcHQgPSBtYXhMYW5ndWFnZUlkWyAxIF0sXG5cdFx0XHR0ZXJyaXRvcnkgPSBtYXhMYW5ndWFnZUlkWyAyIF07XG5cblx0XHQvLyBbM11cblx0XHRtYXRjaEZvdW5kID0gYXJyYXlTb21lKFtcblx0XHRcdFsgWyBsYW5ndWFnZSwgXCJaenp6XCIsIFwiWlpcIiBdLCBbIGxhbmd1YWdlIF0gXSxcblx0XHRcdFsgWyBsYW5ndWFnZSwgXCJaenp6XCIsIHRlcnJpdG9yeSBdLCBbIGxhbmd1YWdlLCB0ZXJyaXRvcnkgXSBdLFxuXHRcdFx0WyBbIGxhbmd1YWdlLCBzY3JpcHQsIFwiWlpcIiBdLCBbIGxhbmd1YWdlLCBzY3JpcHQgXSBdXG5cdFx0XSwgZnVuY3Rpb24oIHRlc3QgKSB7XG5cdFx0XHR2YXIgcmVzdWx0ID0gY29yZUxpa2VseVN1YnRhZ3MoIENsZHIsIGNsZHIsIHRlc3RbIDAgXSApO1xuXHRcdFx0bWF0Y2ggPSB0ZXN0WyAxIF07XG5cdFx0XHRyZXR1cm4gcmVzdWx0ICYmIHJlc3VsdFsgMCBdID09PSBtYXhMYW5ndWFnZUlkWyAwIF0gJiZcblx0XHRcdFx0cmVzdWx0WyAxIF0gPT09IG1heExhbmd1YWdlSWRbIDEgXSAmJlxuXHRcdFx0XHRyZXN1bHRbIDIgXSA9PT0gbWF4TGFuZ3VhZ2VJZFsgMiBdO1xuXHRcdH0pO1xuXG5cdFx0Ly8gWzRdXG5cdFx0cmV0dXJuIG1hdGNoRm91bmQgPyAgbWF0Y2ggOiBtYXhMYW5ndWFnZUlkO1xuXHR9O1xuXG5cblxuXG5cdC8qKlxuXHQgKiBzdWJ0YWdzKCBsb2NhbGUgKVxuXHQgKlxuXHQgKiBAbG9jYWxlIFtTdHJpbmddXG5cdCAqL1xuXHR2YXIgY29yZVN1YnRhZ3MgPSBmdW5jdGlvbiggbG9jYWxlICkge1xuXHRcdHZhciBhdXgsIHVuaWNvZGVMYW5ndWFnZUlkLFxuXHRcdFx0c3VidGFncyA9IFtdO1xuXG5cdFx0bG9jYWxlID0gbG9jYWxlLnJlcGxhY2UoIC9fLywgXCItXCIgKTtcblxuXHRcdC8vIFVuaWNvZGUgbG9jYWxlIGV4dGVuc2lvbnMuXG5cdFx0YXV4ID0gbG9jYWxlLnNwbGl0KCBcIi11LVwiICk7XG5cdFx0aWYgKCBhdXhbIDEgXSApIHtcblx0XHRcdGF1eFsgMSBdID0gYXV4WyAxIF0uc3BsaXQoIFwiLXQtXCIgKTtcblx0XHRcdGxvY2FsZSA9IGF1eFsgMCBdICsgKCBhdXhbIDEgXVsgMSBdID8gXCItdC1cIiArIGF1eFsgMSBdWyAxIF0gOiBcIlwiKTtcblx0XHRcdHN1YnRhZ3NbIDQgLyogdW5pY29kZUxvY2FsZUV4dGVuc2lvbnMgKi8gXSA9IGF1eFsgMSBdWyAwIF07XG5cdFx0fVxuXG5cdFx0Ly8gVE9ETyBub3JtYWxpemUgdHJhbnNmb3JtZWQgZXh0ZW5zaW9ucy4gQ3VycmVudGx5LCBza2lwcGVkLlxuXHRcdC8vIHN1YnRhZ3NbIHggXSA9IGxvY2FsZS5zcGxpdCggXCItdC1cIiApWyAxIF07XG5cdFx0dW5pY29kZUxhbmd1YWdlSWQgPSBsb2NhbGUuc3BsaXQoIFwiLXQtXCIgKVsgMCBdO1xuXG5cdFx0Ly8gdW5pY29kZV9sYW5ndWFnZV9pZCA9IFwicm9vdFwiXG5cdFx0Ly8gICB8IHVuaWNvZGVfbGFuZ3VhZ2Vfc3VidGFnICAgICAgICAgXG5cdFx0Ly8gICAgIChzZXAgdW5pY29kZV9zY3JpcHRfc3VidGFnKT8gXG5cdFx0Ly8gICAgIChzZXAgdW5pY29kZV9yZWdpb25fc3VidGFnKT9cblx0XHQvLyAgICAgKHNlcCB1bmljb2RlX3ZhcmlhbnRfc3VidGFnKSogO1xuXHRcdC8vXG5cdFx0Ly8gQWx0aG91Z2ggdW5pY29kZV9sYW5ndWFnZV9zdWJ0YWcgPSBhbHBoYXsyLDh9LCBJJ20gdXNpbmcgYWxwaGF7MiwzfS4gQmVjYXVzZSwgdGhlcmUncyBubyBsYW5ndWFnZSBvbiBDTERSIGxlbmd0aGllciB0aGFuIDMuXG5cdFx0YXV4ID0gdW5pY29kZUxhbmd1YWdlSWQubWF0Y2goIC9eKChbYS16XXsyLDN9KSgtKFtBLVpdW2Etel17M30pKT8oLShbQS1aXXsyfXxbMC05XXszfSkpPykoLVthLXpBLVowLTldezUsOH18WzAtOV1bYS16QS1aMC05XXszfSkqJHxeKHJvb3QpJC8gKTtcblx0XHRpZiAoIGF1eCA9PT0gbnVsbCApIHtcblx0XHRcdHJldHVybiBbIFwidW5kXCIsIFwiWnp6elwiLCBcIlpaXCIgXTtcblx0XHR9XG5cdFx0c3VidGFnc1sgMCAvKiBsYW5ndWFnZSAqLyBdID0gYXV4WyA5IF0gLyogcm9vdCAqLyB8fCBhdXhbIDIgXSB8fCBcInVuZFwiO1xuXHRcdHN1YnRhZ3NbIDEgLyogc2NyaXB0ICovIF0gPSBhdXhbIDQgXSB8fCBcIlp6enpcIjtcblx0XHRzdWJ0YWdzWyAyIC8qIHRlcnJpdG9yeSAqLyBdID0gYXV4WyA2IF0gfHwgXCJaWlwiO1xuXHRcdHN1YnRhZ3NbIDMgLyogdmFyaWFudCAqLyBdID0gYXV4WyA3IF07XG5cblx0XHQvLyAwOiBsYW5ndWFnZVxuXHRcdC8vIDE6IHNjcmlwdFxuXHRcdC8vIDI6IHRlcnJpdG9yeSAoYWthIHJlZ2lvbilcblx0XHQvLyAzOiB2YXJpYW50XG5cdFx0Ly8gNDogdW5pY29kZUxvY2FsZUV4dGVuc2lvbnNcblx0XHRyZXR1cm4gc3VidGFncztcblx0fTtcblxuXG5cblxuXHR2YXIgYXJyYXlGb3JFYWNoID0gZnVuY3Rpb24oIGFycmF5LCBjYWxsYmFjayApIHtcblx0XHR2YXIgaSwgbGVuZ3RoO1xuXHRcdGlmICggYXJyYXkuZm9yRWFjaCApIHtcblx0XHRcdHJldHVybiBhcnJheS5mb3JFYWNoKCBjYWxsYmFjayApO1xuXHRcdH1cblx0XHRmb3IgKCBpID0gMCwgbGVuZ3RoID0gYXJyYXkubGVuZ3RoOyBpIDwgbGVuZ3RoOyBpKysgKSB7XG5cdFx0XHRjYWxsYmFjayggYXJyYXlbIGkgXSwgaSwgYXJyYXkgKTtcblx0XHR9XG5cdH07XG5cblxuXG5cblx0LyoqXG5cdCAqIGJ1bmRsZUxvb2t1cCggbWluTGFuZ3VhZ2VJZCApXG5cdCAqXG5cdCAqIEBDbGRyIFtDbGRyIGNsYXNzXVxuXHQgKlxuXHQgKiBAY2xkciBbQ2xkciBpbnN0YW5jZV1cblx0ICpcblx0ICogQG1pbkxhbmd1YWdlSWQgW1N0cmluZ10gcmVxdWVzdGVkIGxhbmd1YWdlSWQgYWZ0ZXIgYXBwbGllZCByZW1vdmUgbGlrZWx5IHN1YnRhZ3MuXG5cdCAqL1xuXHR2YXIgYnVuZGxlTG9va3VwID0gZnVuY3Rpb24oIENsZHIsIGNsZHIsIG1pbkxhbmd1YWdlSWQgKSB7XG5cdFx0dmFyIGF2YWlsYWJsZUJ1bmRsZU1hcCA9IENsZHIuX2F2YWlsYWJsZUJ1bmRsZU1hcCxcblx0XHRcdGF2YWlsYWJsZUJ1bmRsZU1hcFF1ZXVlID0gQ2xkci5fYXZhaWxhYmxlQnVuZGxlTWFwUXVldWU7XG5cblx0XHRpZiAoIGF2YWlsYWJsZUJ1bmRsZU1hcFF1ZXVlLmxlbmd0aCApIHtcblx0XHRcdGFycmF5Rm9yRWFjaCggYXZhaWxhYmxlQnVuZGxlTWFwUXVldWUsIGZ1bmN0aW9uKCBidW5kbGUgKSB7XG5cdFx0XHRcdHZhciBleGlzdGluZywgbWF4QnVuZGxlLCBtaW5CdW5kbGUsIHN1YnRhZ3M7XG5cdFx0XHRcdHN1YnRhZ3MgPSBjb3JlU3VidGFncyggYnVuZGxlICk7XG5cdFx0XHRcdG1heEJ1bmRsZSA9IGNvcmVMaWtlbHlTdWJ0YWdzKCBDbGRyLCBjbGRyLCBzdWJ0YWdzLCB7IGZvcmNlOiB0cnVlIH0gKSB8fCBzdWJ0YWdzO1xuXHRcdFx0XHRtaW5CdW5kbGUgPSBjb3JlUmVtb3ZlTGlrZWx5U3VidGFncyggQ2xkciwgY2xkciwgbWF4QnVuZGxlICk7XG5cdFx0XHRcdG1pbkJ1bmRsZSA9IG1pbkJ1bmRsZS5qb2luKCBDbGRyLmxvY2FsZVNlcCApO1xuXHRcdFx0XHRleGlzdGluZyA9IGF2YWlsYWJsZUJ1bmRsZU1hcFF1ZXVlWyBtaW5CdW5kbGUgXTtcblx0XHRcdFx0aWYgKCBleGlzdGluZyAmJiBleGlzdGluZy5sZW5ndGggPCBidW5kbGUubGVuZ3RoICkge1xuXHRcdFx0XHRcdHJldHVybjtcblx0XHRcdFx0fVxuXHRcdFx0XHRhdmFpbGFibGVCdW5kbGVNYXBbIG1pbkJ1bmRsZSBdID0gYnVuZGxlO1xuXHRcdFx0fSk7XG5cdFx0XHRDbGRyLl9hdmFpbGFibGVCdW5kbGVNYXBRdWV1ZSA9IFtdO1xuXHRcdH1cblxuXHRcdHJldHVybiBhdmFpbGFibGVCdW5kbGVNYXBbIG1pbkxhbmd1YWdlSWQgXSB8fCBudWxsO1xuXHR9O1xuXG5cblxuXG5cdHZhciBvYmplY3RLZXlzID0gZnVuY3Rpb24oIG9iamVjdCApIHtcblx0XHR2YXIgaSxcblx0XHRcdHJlc3VsdCA9IFtdO1xuXG5cdFx0aWYgKCBPYmplY3Qua2V5cyApIHtcblx0XHRcdHJldHVybiBPYmplY3Qua2V5cyggb2JqZWN0ICk7XG5cdFx0fVxuXG5cdFx0Zm9yICggaSBpbiBvYmplY3QgKSB7XG5cdFx0XHRyZXN1bHQucHVzaCggaSApO1xuXHRcdH1cblxuXHRcdHJldHVybiByZXN1bHQ7XG5cdH07XG5cblxuXG5cblx0dmFyIGNyZWF0ZUVycm9yID0gZnVuY3Rpb24oIGNvZGUsIGF0dHJpYnV0ZXMgKSB7XG5cdFx0dmFyIGVycm9yLCBtZXNzYWdlO1xuXG5cdFx0bWVzc2FnZSA9IGNvZGUgKyAoIGF0dHJpYnV0ZXMgJiYgSlNPTiA/IFwiOiBcIiArIEpTT04uc3RyaW5naWZ5KCBhdHRyaWJ1dGVzICkgOiBcIlwiICk7XG5cdFx0ZXJyb3IgPSBuZXcgRXJyb3IoIG1lc3NhZ2UgKTtcblx0XHRlcnJvci5jb2RlID0gY29kZTtcblxuXHRcdC8vIGV4dGVuZCggZXJyb3IsIGF0dHJpYnV0ZXMgKTtcblx0XHRhcnJheUZvckVhY2goIG9iamVjdEtleXMoIGF0dHJpYnV0ZXMgKSwgZnVuY3Rpb24oIGF0dHJpYnV0ZSApIHtcblx0XHRcdGVycm9yWyBhdHRyaWJ1dGUgXSA9IGF0dHJpYnV0ZXNbIGF0dHJpYnV0ZSBdO1xuXHRcdH0pO1xuXG5cdFx0cmV0dXJuIGVycm9yO1xuXHR9O1xuXG5cblxuXG5cdHZhciB2YWxpZGF0ZSA9IGZ1bmN0aW9uKCBjb2RlLCBjaGVjaywgYXR0cmlidXRlcyApIHtcblx0XHRpZiAoICFjaGVjayApIHtcblx0XHRcdHRocm93IGNyZWF0ZUVycm9yKCBjb2RlLCBhdHRyaWJ1dGVzICk7XG5cdFx0fVxuXHR9O1xuXG5cblxuXG5cdHZhciB2YWxpZGF0ZVByZXNlbmNlID0gZnVuY3Rpb24oIHZhbHVlLCBuYW1lICkge1xuXHRcdHZhbGlkYXRlKCBcIkVfTUlTU0lOR19QQVJBTUVURVJcIiwgdHlwZW9mIHZhbHVlICE9PSBcInVuZGVmaW5lZFwiLCB7XG5cdFx0XHRuYW1lOiBuYW1lXG5cdFx0fSk7XG5cdH07XG5cblxuXG5cblx0dmFyIHZhbGlkYXRlVHlwZSA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSwgY2hlY2ssIGV4cGVjdGVkICkge1xuXHRcdHZhbGlkYXRlKCBcIkVfSU5WQUxJRF9QQVJfVFlQRVwiLCBjaGVjaywge1xuXHRcdFx0ZXhwZWN0ZWQ6IGV4cGVjdGVkLFxuXHRcdFx0bmFtZTogbmFtZSxcblx0XHRcdHZhbHVlOiB2YWx1ZVxuXHRcdH0pO1xuXHR9O1xuXG5cblxuXG5cdHZhciB2YWxpZGF0ZVR5cGVQYXRoID0gZnVuY3Rpb24oIHZhbHVlLCBuYW1lICkge1xuXHRcdHZhbGlkYXRlVHlwZSggdmFsdWUsIG5hbWUsIHR5cGVvZiB2YWx1ZSA9PT0gXCJzdHJpbmdcIiB8fCBhcnJheUlzQXJyYXkoIHZhbHVlICksIFwiU3RyaW5nIG9yIEFycmF5XCIgKTtcblx0fTtcblxuXG5cblxuXHQvKipcblx0ICogRnVuY3Rpb24gaW5zcGlyZWQgYnkgalF1ZXJ5IENvcmUsIGJ1dCByZWR1Y2VkIHRvIG91ciB1c2UgY2FzZS5cblx0ICovXG5cdHZhciBpc1BsYWluT2JqZWN0ID0gZnVuY3Rpb24oIG9iaiApIHtcblx0XHRyZXR1cm4gb2JqICE9PSBudWxsICYmIFwiXCIgKyBvYmogPT09IFwiW29iamVjdCBPYmplY3RdXCI7XG5cdH07XG5cblxuXG5cblx0dmFyIHZhbGlkYXRlVHlwZVBsYWluT2JqZWN0ID0gZnVuY3Rpb24oIHZhbHVlLCBuYW1lICkge1xuXHRcdHZhbGlkYXRlVHlwZSggdmFsdWUsIG5hbWUsIHR5cGVvZiB2YWx1ZSA9PT0gXCJ1bmRlZmluZWRcIiB8fCBpc1BsYWluT2JqZWN0KCB2YWx1ZSApLCBcIlBsYWluIE9iamVjdFwiICk7XG5cdH07XG5cblxuXG5cblx0dmFyIHZhbGlkYXRlVHlwZVN0cmluZyA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSApIHtcblx0XHR2YWxpZGF0ZVR5cGUoIHZhbHVlLCBuYW1lLCB0eXBlb2YgdmFsdWUgPT09IFwic3RyaW5nXCIsIFwiYSBzdHJpbmdcIiApO1xuXHR9O1xuXG5cblxuXG5cdC8vIEBwYXRoOiBub3JtYWxpemVkIHBhdGhcblx0dmFyIHJlc291cmNlR2V0ID0gZnVuY3Rpb24oIGRhdGEsIHBhdGggKSB7XG5cdFx0dmFyIGksXG5cdFx0XHRub2RlID0gZGF0YSxcblx0XHRcdGxlbmd0aCA9IHBhdGgubGVuZ3RoO1xuXG5cdFx0Zm9yICggaSA9IDA7IGkgPCBsZW5ndGggLSAxOyBpKysgKSB7XG5cdFx0XHRub2RlID0gbm9kZVsgcGF0aFsgaSBdIF07XG5cdFx0XHRpZiAoICFub2RlICkge1xuXHRcdFx0XHRyZXR1cm4gdW5kZWZpbmVkO1xuXHRcdFx0fVxuXHRcdH1cblx0XHRyZXR1cm4gbm9kZVsgcGF0aFsgaSBdIF07XG5cdH07XG5cblxuXG5cblx0LyoqXG5cdCAqIHNldEF2YWlsYWJsZUJ1bmRsZXMoIENsZHIsIGpzb24gKVxuXHQgKlxuXHQgKiBAQ2xkciBbQ2xkciBjbGFzc11cblx0ICpcblx0ICogQGpzb24gcmVzb2x2ZWQvdW5yZXNvbHZlZCBjbGRyIGRhdGEuXG5cdCAqXG5cdCAqIFNldCBhdmFpbGFibGUgYnVuZGxlcyBxdWV1ZSBiYXNlZCBvbiBwYXNzZWQganNvbiBDTERSIGRhdGEuIENvbnNpZGVycyBhIGJ1bmRsZSBhcyBhbnkgU3RyaW5nIGF0IC9tYWluL3tidW5kbGV9LlxuXHQgKi9cblx0dmFyIGNvcmVTZXRBdmFpbGFibGVCdW5kbGVzID0gZnVuY3Rpb24oIENsZHIsIGpzb24gKSB7XG5cdFx0dmFyIGJ1bmRsZSxcblx0XHRcdGF2YWlsYWJsZUJ1bmRsZU1hcFF1ZXVlID0gQ2xkci5fYXZhaWxhYmxlQnVuZGxlTWFwUXVldWUsXG5cdFx0XHRtYWluID0gcmVzb3VyY2VHZXQoIGpzb24sIFsgXCJtYWluXCIgXSApO1xuXG5cdFx0aWYgKCBtYWluICkge1xuXHRcdFx0Zm9yICggYnVuZGxlIGluIG1haW4gKSB7XG5cdFx0XHRcdGlmICggbWFpbi5oYXNPd25Qcm9wZXJ0eSggYnVuZGxlICkgJiYgYnVuZGxlICE9PSBcInJvb3RcIiApIHtcblx0XHRcdFx0XHRhdmFpbGFibGVCdW5kbGVNYXBRdWV1ZS5wdXNoKCBidW5kbGUgKTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdH1cblx0fTtcblxuXG5cblx0dmFyIGFsd2F5c0FycmF5ID0gZnVuY3Rpb24oIHNvbWV0aGluZ09yQXJyYXkgKSB7XG5cdFx0cmV0dXJuIGFycmF5SXNBcnJheSggc29tZXRoaW5nT3JBcnJheSApID8gIHNvbWV0aGluZ09yQXJyYXkgOiBbIHNvbWV0aGluZ09yQXJyYXkgXTtcblx0fTtcblxuXG5cdHZhciBqc29uTWVyZ2UgPSAoZnVuY3Rpb24oKSB7XG5cblx0Ly8gUmV0dXJucyBuZXcgZGVlcGx5IG1lcmdlZCBKU09OLlxuXHQvL1xuXHQvLyBFZy5cblx0Ly8gbWVyZ2UoIHsgYTogeyBiOiAxLCBjOiAyIH0gfSwgeyBhOiB7IGI6IDMsIGQ6IDQgfSB9IClcblx0Ly8gLT4geyBhOiB7IGI6IDMsIGM6IDIsIGQ6IDQgfSB9XG5cdC8vXG5cdC8vIEBhcmd1bWVudHMgSlNPTidzXG5cdC8vIFxuXHR2YXIgbWVyZ2UgPSBmdW5jdGlvbigpIHtcblx0XHR2YXIgZGVzdGluYXRpb24gPSB7fSxcblx0XHRcdHNvdXJjZXMgPSBbXS5zbGljZS5jYWxsKCBhcmd1bWVudHMsIDAgKTtcblx0XHRhcnJheUZvckVhY2goIHNvdXJjZXMsIGZ1bmN0aW9uKCBzb3VyY2UgKSB7XG5cdFx0XHR2YXIgcHJvcDtcblx0XHRcdGZvciAoIHByb3AgaW4gc291cmNlICkge1xuXHRcdFx0XHRpZiAoIHByb3AgaW4gZGVzdGluYXRpb24gJiYgYXJyYXlJc0FycmF5KCBkZXN0aW5hdGlvblsgcHJvcCBdICkgKSB7XG5cblx0XHRcdFx0XHQvLyBDb25jYXQgQXJyYXlzXG5cdFx0XHRcdFx0ZGVzdGluYXRpb25bIHByb3AgXSA9IGRlc3RpbmF0aW9uWyBwcm9wIF0uY29uY2F0KCBzb3VyY2VbIHByb3AgXSApO1xuXG5cdFx0XHRcdH0gZWxzZSBpZiAoIHByb3AgaW4gZGVzdGluYXRpb24gJiYgdHlwZW9mIGRlc3RpbmF0aW9uWyBwcm9wIF0gPT09IFwib2JqZWN0XCIgKSB7XG5cblx0XHRcdFx0XHQvLyBNZXJnZSBPYmplY3RzXG5cdFx0XHRcdFx0ZGVzdGluYXRpb25bIHByb3AgXSA9IG1lcmdlKCBkZXN0aW5hdGlvblsgcHJvcCBdLCBzb3VyY2VbIHByb3AgXSApO1xuXG5cdFx0XHRcdH0gZWxzZSB7XG5cblx0XHRcdFx0XHQvLyBTZXQgbmV3IHZhbHVlc1xuXHRcdFx0XHRcdGRlc3RpbmF0aW9uWyBwcm9wIF0gPSBzb3VyY2VbIHByb3AgXTtcblxuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fSk7XG5cdFx0cmV0dXJuIGRlc3RpbmF0aW9uO1xuXHR9O1xuXG5cdHJldHVybiBtZXJnZTtcblxufSgpKTtcblxuXG5cdC8qKlxuXHQgKiBsb2FkKCBDbGRyLCBzb3VyY2UsIGpzb25zIClcblx0ICpcblx0ICogQENsZHIgW0NsZHIgY2xhc3NdXG5cdCAqXG5cdCAqIEBzb3VyY2UgW09iamVjdF1cblx0ICpcblx0ICogQGpzb25zIFthcmd1bWVudHNdXG5cdCAqL1xuXHR2YXIgY29yZUxvYWQgPSBmdW5jdGlvbiggQ2xkciwgc291cmNlLCBqc29ucyApIHtcblx0XHR2YXIgaSwgaiwganNvbjtcblxuXHRcdHZhbGlkYXRlUHJlc2VuY2UoIGpzb25zWyAwIF0sIFwianNvblwiICk7XG5cblx0XHQvLyBTdXBwb3J0IGFyYml0cmFyeSBwYXJhbWV0ZXJzLCBlLmcuLCBgQ2xkci5sb2FkKHsuLi59LCB7Li4ufSlgLlxuXHRcdGZvciAoIGkgPSAwOyBpIDwganNvbnMubGVuZ3RoOyBpKysgKSB7XG5cblx0XHRcdC8vIFN1cHBvcnQgYXJyYXkgcGFyYW1ldGVycywgZS5nLiwgYENsZHIubG9hZChbey4uLn0sIHsuLi59XSlgLlxuXHRcdFx0anNvbiA9IGFsd2F5c0FycmF5KCBqc29uc1sgaSBdICk7XG5cblx0XHRcdGZvciAoIGogPSAwOyBqIDwganNvbi5sZW5ndGg7IGorKyApIHtcblx0XHRcdFx0dmFsaWRhdGVUeXBlUGxhaW5PYmplY3QoIGpzb25bIGogXSwgXCJqc29uXCIgKTtcblx0XHRcdFx0c291cmNlID0ganNvbk1lcmdlKCBzb3VyY2UsIGpzb25bIGogXSApO1xuXHRcdFx0XHRjb3JlU2V0QXZhaWxhYmxlQnVuZGxlcyggQ2xkciwganNvblsgaiBdICk7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHNvdXJjZTtcblx0fTtcblxuXG5cblx0dmFyIGl0ZW1HZXRSZXNvbHZlZCA9IGZ1bmN0aW9uKCBDbGRyLCBwYXRoLCBhdHRyaWJ1dGVzICkge1xuXHRcdC8vIFJlc29sdmUgcGF0aFxuXHRcdHZhciBub3JtYWxpemVkUGF0aCA9IHBhdGhOb3JtYWxpemUoIHBhdGgsIGF0dHJpYnV0ZXMgKTtcblxuXHRcdHJldHVybiByZXNvdXJjZUdldCggQ2xkci5fcmVzb2x2ZWQsIG5vcm1hbGl6ZWRQYXRoICk7XG5cdH07XG5cblxuXG5cblx0LyoqXG5cdCAqIG5ldyBDbGRyKClcblx0ICovXG5cdHZhciBDbGRyID0gZnVuY3Rpb24oIGxvY2FsZSApIHtcblx0XHR0aGlzLmluaXQoIGxvY2FsZSApO1xuXHR9O1xuXG5cdC8vIEJ1aWxkIG9wdGltaXphdGlvbiBoYWNrIHRvIGF2b2lkIGR1cGxpY2F0aW5nIGZ1bmN0aW9ucyBhY3Jvc3MgbW9kdWxlcy5cblx0Q2xkci5fYWx3YXlzQXJyYXkgPSBhbHdheXNBcnJheTtcblx0Q2xkci5fY29yZUxvYWQgPSBjb3JlTG9hZDtcblx0Q2xkci5fY3JlYXRlRXJyb3IgPSBjcmVhdGVFcnJvcjtcblx0Q2xkci5faXRlbUdldFJlc29sdmVkID0gaXRlbUdldFJlc29sdmVkO1xuXHRDbGRyLl9qc29uTWVyZ2UgPSBqc29uTWVyZ2U7XG5cdENsZHIuX3BhdGhOb3JtYWxpemUgPSBwYXRoTm9ybWFsaXplO1xuXHRDbGRyLl9yZXNvdXJjZUdldCA9IHJlc291cmNlR2V0O1xuXHRDbGRyLl92YWxpZGF0ZVByZXNlbmNlID0gdmFsaWRhdGVQcmVzZW5jZTtcblx0Q2xkci5fdmFsaWRhdGVUeXBlID0gdmFsaWRhdGVUeXBlO1xuXHRDbGRyLl92YWxpZGF0ZVR5cGVQYXRoID0gdmFsaWRhdGVUeXBlUGF0aDtcblx0Q2xkci5fdmFsaWRhdGVUeXBlUGxhaW5PYmplY3QgPSB2YWxpZGF0ZVR5cGVQbGFpbk9iamVjdDtcblxuXHRDbGRyLl9hdmFpbGFibGVCdW5kbGVNYXAgPSB7fTtcblx0Q2xkci5fYXZhaWxhYmxlQnVuZGxlTWFwUXVldWUgPSBbXTtcblx0Q2xkci5fcmVzb2x2ZWQgPSB7fTtcblxuXHQvLyBBbGxvdyB1c2VyIHRvIG92ZXJyaWRlIGxvY2FsZSBzZXBhcmF0b3IgXCItXCIgKGRlZmF1bHQpIHwgXCJfXCIuIEFjY29yZGluZyB0byBodHRwOi8vd3d3LnVuaWNvZGUub3JnL3JlcG9ydHMvdHIzNS8jVW5pY29kZV9sYW5ndWFnZV9pZGVudGlmaWVyLCBib3RoIFwiLVwiIGFuZCBcIl9cIiBhcmUgdmFsaWQgbG9jYWxlIHNlcGFyYXRvcnMgKGVnLiBcImVuX0dCXCIsIFwiZW4tR0JcIikuIEFjY29yZGluZyB0byBodHRwOi8vdW5pY29kZS5vcmcvY2xkci90cmFjL3RpY2tldC82Nzg2IGl0cyB1c2FnZSBtdXN0IGJlIGNvbnNpc3RlbnQgdGhyb3VnaG91dCB0aGUgZGF0YSBzZXQuXG5cdENsZHIubG9jYWxlU2VwID0gXCItXCI7XG5cblx0LyoqXG5cdCAqIENsZHIubG9hZCgganNvbiBbLCBqc29uLCAuLi5dIClcblx0ICpcblx0ICogQGpzb24gW0pTT05dIENMRFIgZGF0YSBvciBbQXJyYXldIEFycmF5IG9mIEBqc29uJ3MuXG5cdCAqXG5cdCAqIExvYWQgcmVzb2x2ZWQgY2xkciBkYXRhLlxuXHQgKi9cblx0Q2xkci5sb2FkID0gZnVuY3Rpb24oKSB7XG5cdFx0Q2xkci5fcmVzb2x2ZWQgPSBjb3JlTG9hZCggQ2xkciwgQ2xkci5fcmVzb2x2ZWQsIGFyZ3VtZW50cyApO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiAuaW5pdCgpIGF1dG9tYXRpY2FsbHkgcnVuIG9uIGluc3RhbnRpYXRpb24vY29uc3RydWN0aW9uLlxuXHQgKi9cblx0Q2xkci5wcm90b3R5cGUuaW5pdCA9IGZ1bmN0aW9uKCBsb2NhbGUgKSB7XG5cdFx0dmFyIGF0dHJpYnV0ZXMsIGxhbmd1YWdlLCBtYXhMYW5ndWFnZUlkLCBtaW5MYW5ndWFnZUlkLCBzY3JpcHQsIHN1YnRhZ3MsIHRlcnJpdG9yeSwgdW5pY29kZUxvY2FsZUV4dGVuc2lvbnMsIHZhcmlhbnQsXG5cdFx0XHRzZXAgPSBDbGRyLmxvY2FsZVNlcDtcblxuXHRcdHZhbGlkYXRlUHJlc2VuY2UoIGxvY2FsZSwgXCJsb2NhbGVcIiApO1xuXHRcdHZhbGlkYXRlVHlwZVN0cmluZyggbG9jYWxlLCBcImxvY2FsZVwiICk7XG5cblx0XHRzdWJ0YWdzID0gY29yZVN1YnRhZ3MoIGxvY2FsZSApO1xuXG5cdFx0dW5pY29kZUxvY2FsZUV4dGVuc2lvbnMgPSBzdWJ0YWdzWyA0IF07XG5cdFx0dmFyaWFudCA9IHN1YnRhZ3NbIDMgXTtcblxuXHRcdC8vIE5vcm1hbGl6ZSBsb2NhbGUgY29kZS5cblx0XHQvLyBHZXQgKG9yIGRlZHVjZSkgdGhlIFwidHJpcGxlIHN1YnRhZ3NcIjogbGFuZ3VhZ2UsIHRlcnJpdG9yeSAoYWxzbyBhbGlhc2VkIGFzIHJlZ2lvbiksIGFuZCBzY3JpcHQgc3VidGFncy5cblx0XHQvLyBHZXQgdGhlIHZhcmlhbnQgc3VidGFncyAoY2FsZW5kYXIsIGNvbGxhdGlvbiwgY3VycmVuY3ksIGV0YykuXG5cdFx0Ly8gcmVmczpcblx0XHQvLyAtIGh0dHA6Ly93d3cudW5pY29kZS5vcmcvcmVwb3J0cy90cjM1LyNGaWVsZF9EZWZpbml0aW9uc1xuXHRcdC8vIC0gaHR0cDovL3d3dy51bmljb2RlLm9yZy9yZXBvcnRzL3RyMzUvI0xhbmd1YWdlX2FuZF9Mb2NhbGVfSURzXG5cdFx0Ly8gLSBodHRwOi8vd3d3LnVuaWNvZGUub3JnL3JlcG9ydHMvdHIzNS8jVW5pY29kZV9sb2NhbGVfaWRlbnRpZmllclxuXG5cdFx0Ly8gV2hlbiBhIGxvY2FsZSBpZCBkb2VzIG5vdCBzcGVjaWZ5IGEgbGFuZ3VhZ2UsIG9yIHRlcnJpdG9yeSAocmVnaW9uKSwgb3Igc2NyaXB0LCB0aGV5IGFyZSBvYnRhaW5lZCBieSBMaWtlbHkgU3VidGFncy5cblx0XHRtYXhMYW5ndWFnZUlkID0gY29yZUxpa2VseVN1YnRhZ3MoIENsZHIsIHRoaXMsIHN1YnRhZ3MsIHsgZm9yY2U6IHRydWUgfSApIHx8IHN1YnRhZ3M7XG5cdFx0bGFuZ3VhZ2UgPSBtYXhMYW5ndWFnZUlkWyAwIF07XG5cdFx0c2NyaXB0ID0gbWF4TGFuZ3VhZ2VJZFsgMSBdO1xuXHRcdHRlcnJpdG9yeSA9IG1heExhbmd1YWdlSWRbIDIgXTtcblxuXHRcdG1pbkxhbmd1YWdlSWQgPSBjb3JlUmVtb3ZlTGlrZWx5U3VidGFncyggQ2xkciwgdGhpcywgbWF4TGFuZ3VhZ2VJZCApLmpvaW4oIHNlcCApO1xuXG5cdFx0Ly8gU2V0IGF0dHJpYnV0ZXNcblx0XHR0aGlzLmF0dHJpYnV0ZXMgPSBhdHRyaWJ1dGVzID0ge1xuXHRcdFx0YnVuZGxlOiBidW5kbGVMb29rdXAoIENsZHIsIHRoaXMsIG1pbkxhbmd1YWdlSWQgKSxcblxuXHRcdFx0Ly8gVW5pY29kZSBMYW5ndWFnZSBJZFxuXHRcdFx0bWlubGFuZ3VhZ2VJZDogbWluTGFuZ3VhZ2VJZCxcblx0XHRcdG1heExhbmd1YWdlSWQ6IG1heExhbmd1YWdlSWQuam9pbiggc2VwICksXG5cblx0XHRcdC8vIFVuaWNvZGUgTGFuZ3VhZ2UgSWQgU3VidGFic1xuXHRcdFx0bGFuZ3VhZ2U6IGxhbmd1YWdlLFxuXHRcdFx0c2NyaXB0OiBzY3JpcHQsXG5cdFx0XHR0ZXJyaXRvcnk6IHRlcnJpdG9yeSxcblx0XHRcdHJlZ2lvbjogdGVycml0b3J5LCAvKiBhbGlhcyAqL1xuXHRcdFx0dmFyaWFudDogdmFyaWFudFxuXHRcdH07XG5cblx0XHQvLyBVbmljb2RlIGxvY2FsZSBleHRlbnNpb25zLlxuXHRcdHVuaWNvZGVMb2NhbGVFeHRlbnNpb25zICYmICggXCItXCIgKyB1bmljb2RlTG9jYWxlRXh0ZW5zaW9ucyApLnJlcGxhY2UoIC8tW2Etel17Myw4fXwoLVthLXpdezJ9KS0oW2Etel17Myw4fSkvZywgZnVuY3Rpb24oIGF0dHJpYnV0ZSwga2V5LCB0eXBlICkge1xuXG5cdFx0XHRpZiAoIGtleSApIHtcblxuXHRcdFx0XHQvLyBFeHRlbnNpb24gaXMgaW4gdGhlIGBrZXl3b3JkYCBmb3JtLlxuXHRcdFx0XHRhdHRyaWJ1dGVzWyBcInVcIiArIGtleSBdID0gdHlwZTtcblx0XHRcdH0gZWxzZSB7XG5cblx0XHRcdFx0Ly8gRXh0ZW5zaW9uIGlzIGluIHRoZSBgYXR0cmlidXRlYCBmb3JtLlxuXHRcdFx0XHRhdHRyaWJ1dGVzWyBcInVcIiArIGF0dHJpYnV0ZSBdID0gdHJ1ZTtcblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdHRoaXMubG9jYWxlID0gbG9jYWxlO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiAuZ2V0KClcblx0ICovXG5cdENsZHIucHJvdG90eXBlLmdldCA9IGZ1bmN0aW9uKCBwYXRoICkge1xuXG5cdFx0dmFsaWRhdGVQcmVzZW5jZSggcGF0aCwgXCJwYXRoXCIgKTtcblx0XHR2YWxpZGF0ZVR5cGVQYXRoKCBwYXRoLCBcInBhdGhcIiApO1xuXG5cdFx0cmV0dXJuIGl0ZW1HZXRSZXNvbHZlZCggQ2xkciwgcGF0aCwgdGhpcy5hdHRyaWJ1dGVzICk7XG5cdH07XG5cblx0LyoqXG5cdCAqIC5tYWluKClcblx0ICovXG5cdENsZHIucHJvdG90eXBlLm1haW4gPSBmdW5jdGlvbiggcGF0aCApIHtcblx0XHR2YWxpZGF0ZVByZXNlbmNlKCBwYXRoLCBcInBhdGhcIiApO1xuXHRcdHZhbGlkYXRlVHlwZVBhdGgoIHBhdGgsIFwicGF0aFwiICk7XG5cblx0XHR2YWxpZGF0ZSggXCJFX01JU1NJTkdfQlVORExFXCIsIHRoaXMuYXR0cmlidXRlcy5idW5kbGUgIT09IG51bGwsIHtcblx0XHRcdGxvY2FsZTogdGhpcy5sb2NhbGVcblx0XHR9KTtcblxuXHRcdHBhdGggPSBhbHdheXNBcnJheSggcGF0aCApO1xuXHRcdHJldHVybiB0aGlzLmdldCggWyBcIm1haW4ve2J1bmRsZX1cIiBdLmNvbmNhdCggcGF0aCApICk7XG5cdH07XG5cblx0cmV0dXJuIENsZHI7XG5cblxuXG5cbn0pKTtcbiIsIi8qKlxuICogQ0xEUiBKYXZhU2NyaXB0IExpYnJhcnkgdjAuNC4xXG4gKiBodHRwOi8vanF1ZXJ5LmNvbS9cbiAqXG4gKiBDb3B5cmlnaHQgMjAxMyBSYWZhZWwgWGF2aWVyIGRlIFNvdXphXG4gKiBSZWxlYXNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2VcbiAqIGh0dHA6Ly9qcXVlcnkub3JnL2xpY2Vuc2VcbiAqXG4gKiBEYXRlOiAyMDE1LTAyLTI1VDEzOjUxWlxuICovXG4vKiFcbiAqIENMRFIgSmF2YVNjcmlwdCBMaWJyYXJ5IHYwLjQuMSAyMDE1LTAyLTI1VDEzOjUxWiBNSVQgbGljZW5zZSDCqSBSYWZhZWwgWGF2aWVyXG4gKiBodHRwOi8vZ2l0LmlvL2g0bG1WZ1xuICovXG4oZnVuY3Rpb24oIGZhY3RvcnkgKSB7XG5cblx0aWYgKCB0eXBlb2YgZGVmaW5lID09PSBcImZ1bmN0aW9uXCIgJiYgZGVmaW5lLmFtZCApIHtcblx0XHQvLyBBTUQuXG5cdFx0ZGVmaW5lKCBbIFwiLi4vY2xkclwiIF0sIGZhY3RvcnkgKTtcblx0fSBlbHNlIGlmICggdHlwZW9mIG1vZHVsZSA9PT0gXCJvYmplY3RcIiAmJiB0eXBlb2YgbW9kdWxlLmV4cG9ydHMgPT09IFwib2JqZWN0XCIgKSB7XG5cdFx0Ly8gTm9kZS4gQ29tbW9uSlMuXG5cdFx0bW9kdWxlLmV4cG9ydHMgPSBmYWN0b3J5KCByZXF1aXJlKCBcImNsZHJqc1wiICkgKTtcblx0fSBlbHNlIHtcblx0XHQvLyBHbG9iYWxcblx0XHRmYWN0b3J5KCBDbGRyICk7XG5cdH1cblxufShmdW5jdGlvbiggQ2xkciApIHtcblxuXHQvLyBCdWlsZCBvcHRpbWl6YXRpb24gaGFjayB0byBhdm9pZCBkdXBsaWNhdGluZyBmdW5jdGlvbnMgYWNyb3NzIG1vZHVsZXMuXG5cdHZhciBwYXRoTm9ybWFsaXplID0gQ2xkci5fcGF0aE5vcm1hbGl6ZSxcblx0XHR2YWxpZGF0ZVByZXNlbmNlID0gQ2xkci5fdmFsaWRhdGVQcmVzZW5jZSxcblx0XHR2YWxpZGF0ZVR5cGUgPSBDbGRyLl92YWxpZGF0ZVR5cGU7XG5cbi8qIVxuICogRXZlbnRFbWl0dGVyIHY0LjIuNyAtIGdpdC5pby9lZVxuICogT2xpdmVyIENhbGR3ZWxsXG4gKiBNSVQgbGljZW5zZVxuICogQHByZXNlcnZlXG4gKi9cblxudmFyIEV2ZW50RW1pdHRlcjtcbi8qIGpzaGludCBpZ25vcmU6c3RhcnQgKi9cbkV2ZW50RW1pdHRlciA9IChmdW5jdGlvbiAoKSB7XG5cdFxuXG5cdC8qKlxuXHQgKiBDbGFzcyBmb3IgbWFuYWdpbmcgZXZlbnRzLlxuXHQgKiBDYW4gYmUgZXh0ZW5kZWQgdG8gcHJvdmlkZSBldmVudCBmdW5jdGlvbmFsaXR5IGluIG90aGVyIGNsYXNzZXMuXG5cdCAqXG5cdCAqIEBjbGFzcyBFdmVudEVtaXR0ZXIgTWFuYWdlcyBldmVudCByZWdpc3RlcmluZyBhbmQgZW1pdHRpbmcuXG5cdCAqL1xuXHRmdW5jdGlvbiBFdmVudEVtaXR0ZXIoKSB7fVxuXG5cdC8vIFNob3J0Y3V0cyB0byBpbXByb3ZlIHNwZWVkIGFuZCBzaXplXG5cdHZhciBwcm90byA9IEV2ZW50RW1pdHRlci5wcm90b3R5cGU7XG5cdHZhciBleHBvcnRzID0gdGhpcztcblx0dmFyIG9yaWdpbmFsR2xvYmFsVmFsdWUgPSBleHBvcnRzLkV2ZW50RW1pdHRlcjtcblxuXHQvKipcblx0ICogRmluZHMgdGhlIGluZGV4IG9mIHRoZSBsaXN0ZW5lciBmb3IgdGhlIGV2ZW50IGluIGl0J3Mgc3RvcmFnZSBhcnJheS5cblx0ICpcblx0ICogQHBhcmFtIHtGdW5jdGlvbltdfSBsaXN0ZW5lcnMgQXJyYXkgb2YgbGlzdGVuZXJzIHRvIHNlYXJjaCB0aHJvdWdoLlxuXHQgKiBAcGFyYW0ge0Z1bmN0aW9ufSBsaXN0ZW5lciBNZXRob2QgdG8gbG9vayBmb3IuXG5cdCAqIEByZXR1cm4ge051bWJlcn0gSW5kZXggb2YgdGhlIHNwZWNpZmllZCBsaXN0ZW5lciwgLTEgaWYgbm90IGZvdW5kXG5cdCAqIEBhcGkgcHJpdmF0ZVxuXHQgKi9cblx0ZnVuY3Rpb24gaW5kZXhPZkxpc3RlbmVyKGxpc3RlbmVycywgbGlzdGVuZXIpIHtcblx0XHR2YXIgaSA9IGxpc3RlbmVycy5sZW5ndGg7XG5cdFx0d2hpbGUgKGktLSkge1xuXHRcdFx0aWYgKGxpc3RlbmVyc1tpXS5saXN0ZW5lciA9PT0gbGlzdGVuZXIpIHtcblx0XHRcdFx0cmV0dXJuIGk7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0cmV0dXJuIC0xO1xuXHR9XG5cblx0LyoqXG5cdCAqIEFsaWFzIGEgbWV0aG9kIHdoaWxlIGtlZXBpbmcgdGhlIGNvbnRleHQgY29ycmVjdCwgdG8gYWxsb3cgZm9yIG92ZXJ3cml0aW5nIG9mIHRhcmdldCBtZXRob2QuXG5cdCAqXG5cdCAqIEBwYXJhbSB7U3RyaW5nfSBuYW1lIFRoZSBuYW1lIG9mIHRoZSB0YXJnZXQgbWV0aG9kLlxuXHQgKiBAcmV0dXJuIHtGdW5jdGlvbn0gVGhlIGFsaWFzZWQgbWV0aG9kXG5cdCAqIEBhcGkgcHJpdmF0ZVxuXHQgKi9cblx0ZnVuY3Rpb24gYWxpYXMobmFtZSkge1xuXHRcdHJldHVybiBmdW5jdGlvbiBhbGlhc0Nsb3N1cmUoKSB7XG5cdFx0XHRyZXR1cm4gdGhpc1tuYW1lXS5hcHBseSh0aGlzLCBhcmd1bWVudHMpO1xuXHRcdH07XG5cdH1cblxuXHQvKipcblx0ICogUmV0dXJucyB0aGUgbGlzdGVuZXIgYXJyYXkgZm9yIHRoZSBzcGVjaWZpZWQgZXZlbnQuXG5cdCAqIFdpbGwgaW5pdGlhbGlzZSB0aGUgZXZlbnQgb2JqZWN0IGFuZCBsaXN0ZW5lciBhcnJheXMgaWYgcmVxdWlyZWQuXG5cdCAqIFdpbGwgcmV0dXJuIGFuIG9iamVjdCBpZiB5b3UgdXNlIGEgcmVnZXggc2VhcmNoLiBUaGUgb2JqZWN0IGNvbnRhaW5zIGtleXMgZm9yIGVhY2ggbWF0Y2hlZCBldmVudC4gU28gL2JhW3J6XS8gbWlnaHQgcmV0dXJuIGFuIG9iamVjdCBjb250YWluaW5nIGJhciBhbmQgYmF6LiBCdXQgb25seSBpZiB5b3UgaGF2ZSBlaXRoZXIgZGVmaW5lZCB0aGVtIHdpdGggZGVmaW5lRXZlbnQgb3IgYWRkZWQgc29tZSBsaXN0ZW5lcnMgdG8gdGhlbS5cblx0ICogRWFjaCBwcm9wZXJ0eSBpbiB0aGUgb2JqZWN0IHJlc3BvbnNlIGlzIGFuIGFycmF5IG9mIGxpc3RlbmVyIGZ1bmN0aW9ucy5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd8UmVnRXhwfSBldnQgTmFtZSBvZiB0aGUgZXZlbnQgdG8gcmV0dXJuIHRoZSBsaXN0ZW5lcnMgZnJvbS5cblx0ICogQHJldHVybiB7RnVuY3Rpb25bXXxPYmplY3R9IEFsbCBsaXN0ZW5lciBmdW5jdGlvbnMgZm9yIHRoZSBldmVudC5cblx0ICovXG5cdHByb3RvLmdldExpc3RlbmVycyA9IGZ1bmN0aW9uIGdldExpc3RlbmVycyhldnQpIHtcblx0XHR2YXIgZXZlbnRzID0gdGhpcy5fZ2V0RXZlbnRzKCk7XG5cdFx0dmFyIHJlc3BvbnNlO1xuXHRcdHZhciBrZXk7XG5cblx0XHQvLyBSZXR1cm4gYSBjb25jYXRlbmF0ZWQgYXJyYXkgb2YgYWxsIG1hdGNoaW5nIGV2ZW50cyBpZlxuXHRcdC8vIHRoZSBzZWxlY3RvciBpcyBhIHJlZ3VsYXIgZXhwcmVzc2lvbi5cblx0XHRpZiAoZXZ0IGluc3RhbmNlb2YgUmVnRXhwKSB7XG5cdFx0XHRyZXNwb25zZSA9IHt9O1xuXHRcdFx0Zm9yIChrZXkgaW4gZXZlbnRzKSB7XG5cdFx0XHRcdGlmIChldmVudHMuaGFzT3duUHJvcGVydHkoa2V5KSAmJiBldnQudGVzdChrZXkpKSB7XG5cdFx0XHRcdFx0cmVzcG9uc2Vba2V5XSA9IGV2ZW50c1trZXldO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fVxuXHRcdGVsc2Uge1xuXHRcdFx0cmVzcG9uc2UgPSBldmVudHNbZXZ0XSB8fCAoZXZlbnRzW2V2dF0gPSBbXSk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHJlc3BvbnNlO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiBUYWtlcyBhIGxpc3Qgb2YgbGlzdGVuZXIgb2JqZWN0cyBhbmQgZmxhdHRlbnMgaXQgaW50byBhIGxpc3Qgb2YgbGlzdGVuZXIgZnVuY3Rpb25zLlxuXHQgKlxuXHQgKiBAcGFyYW0ge09iamVjdFtdfSBsaXN0ZW5lcnMgUmF3IGxpc3RlbmVyIG9iamVjdHMuXG5cdCAqIEByZXR1cm4ge0Z1bmN0aW9uW119IEp1c3QgdGhlIGxpc3RlbmVyIGZ1bmN0aW9ucy5cblx0ICovXG5cdHByb3RvLmZsYXR0ZW5MaXN0ZW5lcnMgPSBmdW5jdGlvbiBmbGF0dGVuTGlzdGVuZXJzKGxpc3RlbmVycykge1xuXHRcdHZhciBmbGF0TGlzdGVuZXJzID0gW107XG5cdFx0dmFyIGk7XG5cblx0XHRmb3IgKGkgPSAwOyBpIDwgbGlzdGVuZXJzLmxlbmd0aDsgaSArPSAxKSB7XG5cdFx0XHRmbGF0TGlzdGVuZXJzLnB1c2gobGlzdGVuZXJzW2ldLmxpc3RlbmVyKTtcblx0XHR9XG5cblx0XHRyZXR1cm4gZmxhdExpc3RlbmVycztcblx0fTtcblxuXHQvKipcblx0ICogRmV0Y2hlcyB0aGUgcmVxdWVzdGVkIGxpc3RlbmVycyB2aWEgZ2V0TGlzdGVuZXJzIGJ1dCB3aWxsIGFsd2F5cyByZXR1cm4gdGhlIHJlc3VsdHMgaW5zaWRlIGFuIG9iamVjdC4gVGhpcyBpcyBtYWlubHkgZm9yIGludGVybmFsIHVzZSBidXQgb3RoZXJzIG1heSBmaW5kIGl0IHVzZWZ1bC5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd8UmVnRXhwfSBldnQgTmFtZSBvZiB0aGUgZXZlbnQgdG8gcmV0dXJuIHRoZSBsaXN0ZW5lcnMgZnJvbS5cblx0ICogQHJldHVybiB7T2JqZWN0fSBBbGwgbGlzdGVuZXIgZnVuY3Rpb25zIGZvciBhbiBldmVudCBpbiBhbiBvYmplY3QuXG5cdCAqL1xuXHRwcm90by5nZXRMaXN0ZW5lcnNBc09iamVjdCA9IGZ1bmN0aW9uIGdldExpc3RlbmVyc0FzT2JqZWN0KGV2dCkge1xuXHRcdHZhciBsaXN0ZW5lcnMgPSB0aGlzLmdldExpc3RlbmVycyhldnQpO1xuXHRcdHZhciByZXNwb25zZTtcblxuXHRcdGlmIChsaXN0ZW5lcnMgaW5zdGFuY2VvZiBBcnJheSkge1xuXHRcdFx0cmVzcG9uc2UgPSB7fTtcblx0XHRcdHJlc3BvbnNlW2V2dF0gPSBsaXN0ZW5lcnM7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHJlc3BvbnNlIHx8IGxpc3RlbmVycztcblx0fTtcblxuXHQvKipcblx0ICogQWRkcyBhIGxpc3RlbmVyIGZ1bmN0aW9uIHRvIHRoZSBzcGVjaWZpZWQgZXZlbnQuXG5cdCAqIFRoZSBsaXN0ZW5lciB3aWxsIG5vdCBiZSBhZGRlZCBpZiBpdCBpcyBhIGR1cGxpY2F0ZS5cblx0ICogSWYgdGhlIGxpc3RlbmVyIHJldHVybnMgdHJ1ZSB0aGVuIGl0IHdpbGwgYmUgcmVtb3ZlZCBhZnRlciBpdCBpcyBjYWxsZWQuXG5cdCAqIElmIHlvdSBwYXNzIGEgcmVndWxhciBleHByZXNzaW9uIGFzIHRoZSBldmVudCBuYW1lIHRoZW4gdGhlIGxpc3RlbmVyIHdpbGwgYmUgYWRkZWQgdG8gYWxsIGV2ZW50cyB0aGF0IG1hdGNoIGl0LlxuXHQgKlxuXHQgKiBAcGFyYW0ge1N0cmluZ3xSZWdFeHB9IGV2dCBOYW1lIG9mIHRoZSBldmVudCB0byBhdHRhY2ggdGhlIGxpc3RlbmVyIHRvLlxuXHQgKiBAcGFyYW0ge0Z1bmN0aW9ufSBsaXN0ZW5lciBNZXRob2QgdG8gYmUgY2FsbGVkIHdoZW4gdGhlIGV2ZW50IGlzIGVtaXR0ZWQuIElmIHRoZSBmdW5jdGlvbiByZXR1cm5zIHRydWUgdGhlbiBpdCB3aWxsIGJlIHJlbW92ZWQgYWZ0ZXIgY2FsbGluZy5cblx0ICogQHJldHVybiB7T2JqZWN0fSBDdXJyZW50IGluc3RhbmNlIG9mIEV2ZW50RW1pdHRlciBmb3IgY2hhaW5pbmcuXG5cdCAqL1xuXHRwcm90by5hZGRMaXN0ZW5lciA9IGZ1bmN0aW9uIGFkZExpc3RlbmVyKGV2dCwgbGlzdGVuZXIpIHtcblx0XHR2YXIgbGlzdGVuZXJzID0gdGhpcy5nZXRMaXN0ZW5lcnNBc09iamVjdChldnQpO1xuXHRcdHZhciBsaXN0ZW5lcklzV3JhcHBlZCA9IHR5cGVvZiBsaXN0ZW5lciA9PT0gJ29iamVjdCc7XG5cdFx0dmFyIGtleTtcblxuXHRcdGZvciAoa2V5IGluIGxpc3RlbmVycykge1xuXHRcdFx0aWYgKGxpc3RlbmVycy5oYXNPd25Qcm9wZXJ0eShrZXkpICYmIGluZGV4T2ZMaXN0ZW5lcihsaXN0ZW5lcnNba2V5XSwgbGlzdGVuZXIpID09PSAtMSkge1xuXHRcdFx0XHRsaXN0ZW5lcnNba2V5XS5wdXNoKGxpc3RlbmVySXNXcmFwcGVkID8gbGlzdGVuZXIgOiB7XG5cdFx0XHRcdFx0bGlzdGVuZXI6IGxpc3RlbmVyLFxuXHRcdFx0XHRcdG9uY2U6IGZhbHNlXG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHJldHVybiB0aGlzO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiBBbGlhcyBvZiBhZGRMaXN0ZW5lclxuXHQgKi9cblx0cHJvdG8ub24gPSBhbGlhcygnYWRkTGlzdGVuZXInKTtcblxuXHQvKipcblx0ICogU2VtaS1hbGlhcyBvZiBhZGRMaXN0ZW5lci4gSXQgd2lsbCBhZGQgYSBsaXN0ZW5lciB0aGF0IHdpbGwgYmVcblx0ICogYXV0b21hdGljYWxseSByZW1vdmVkIGFmdGVyIGl0J3MgZmlyc3QgZXhlY3V0aW9uLlxuXHQgKlxuXHQgKiBAcGFyYW0ge1N0cmluZ3xSZWdFeHB9IGV2dCBOYW1lIG9mIHRoZSBldmVudCB0byBhdHRhY2ggdGhlIGxpc3RlbmVyIHRvLlxuXHQgKiBAcGFyYW0ge0Z1bmN0aW9ufSBsaXN0ZW5lciBNZXRob2QgdG8gYmUgY2FsbGVkIHdoZW4gdGhlIGV2ZW50IGlzIGVtaXR0ZWQuIElmIHRoZSBmdW5jdGlvbiByZXR1cm5zIHRydWUgdGhlbiBpdCB3aWxsIGJlIHJlbW92ZWQgYWZ0ZXIgY2FsbGluZy5cblx0ICogQHJldHVybiB7T2JqZWN0fSBDdXJyZW50IGluc3RhbmNlIG9mIEV2ZW50RW1pdHRlciBmb3IgY2hhaW5pbmcuXG5cdCAqL1xuXHRwcm90by5hZGRPbmNlTGlzdGVuZXIgPSBmdW5jdGlvbiBhZGRPbmNlTGlzdGVuZXIoZXZ0LCBsaXN0ZW5lcikge1xuXHRcdHJldHVybiB0aGlzLmFkZExpc3RlbmVyKGV2dCwge1xuXHRcdFx0bGlzdGVuZXI6IGxpc3RlbmVyLFxuXHRcdFx0b25jZTogdHJ1ZVxuXHRcdH0pO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiBBbGlhcyBvZiBhZGRPbmNlTGlzdGVuZXIuXG5cdCAqL1xuXHRwcm90by5vbmNlID0gYWxpYXMoJ2FkZE9uY2VMaXN0ZW5lcicpO1xuXG5cdC8qKlxuXHQgKiBEZWZpbmVzIGFuIGV2ZW50IG5hbWUuIFRoaXMgaXMgcmVxdWlyZWQgaWYgeW91IHdhbnQgdG8gdXNlIGEgcmVnZXggdG8gYWRkIGEgbGlzdGVuZXIgdG8gbXVsdGlwbGUgZXZlbnRzIGF0IG9uY2UuIElmIHlvdSBkb24ndCBkbyB0aGlzIHRoZW4gaG93IGRvIHlvdSBleHBlY3QgaXQgdG8ga25vdyB3aGF0IGV2ZW50IHRvIGFkZCB0bz8gU2hvdWxkIGl0IGp1c3QgYWRkIHRvIGV2ZXJ5IHBvc3NpYmxlIG1hdGNoIGZvciBhIHJlZ2V4PyBOby4gVGhhdCBpcyBzY2FyeSBhbmQgYmFkLlxuXHQgKiBZb3UgbmVlZCB0byB0ZWxsIGl0IHdoYXQgZXZlbnQgbmFtZXMgc2hvdWxkIGJlIG1hdGNoZWQgYnkgYSByZWdleC5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd9IGV2dCBOYW1lIG9mIHRoZSBldmVudCB0byBjcmVhdGUuXG5cdCAqIEByZXR1cm4ge09iamVjdH0gQ3VycmVudCBpbnN0YW5jZSBvZiBFdmVudEVtaXR0ZXIgZm9yIGNoYWluaW5nLlxuXHQgKi9cblx0cHJvdG8uZGVmaW5lRXZlbnQgPSBmdW5jdGlvbiBkZWZpbmVFdmVudChldnQpIHtcblx0XHR0aGlzLmdldExpc3RlbmVycyhldnQpO1xuXHRcdHJldHVybiB0aGlzO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiBVc2VzIGRlZmluZUV2ZW50IHRvIGRlZmluZSBtdWx0aXBsZSBldmVudHMuXG5cdCAqXG5cdCAqIEBwYXJhbSB7U3RyaW5nW119IGV2dHMgQW4gYXJyYXkgb2YgZXZlbnQgbmFtZXMgdG8gZGVmaW5lLlxuXHQgKiBAcmV0dXJuIHtPYmplY3R9IEN1cnJlbnQgaW5zdGFuY2Ugb2YgRXZlbnRFbWl0dGVyIGZvciBjaGFpbmluZy5cblx0ICovXG5cdHByb3RvLmRlZmluZUV2ZW50cyA9IGZ1bmN0aW9uIGRlZmluZUV2ZW50cyhldnRzKSB7XG5cdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCBldnRzLmxlbmd0aDsgaSArPSAxKSB7XG5cdFx0XHR0aGlzLmRlZmluZUV2ZW50KGV2dHNbaV0pO1xuXHRcdH1cblx0XHRyZXR1cm4gdGhpcztcblx0fTtcblxuXHQvKipcblx0ICogUmVtb3ZlcyBhIGxpc3RlbmVyIGZ1bmN0aW9uIGZyb20gdGhlIHNwZWNpZmllZCBldmVudC5cblx0ICogV2hlbiBwYXNzZWQgYSByZWd1bGFyIGV4cHJlc3Npb24gYXMgdGhlIGV2ZW50IG5hbWUsIGl0IHdpbGwgcmVtb3ZlIHRoZSBsaXN0ZW5lciBmcm9tIGFsbCBldmVudHMgdGhhdCBtYXRjaCBpdC5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd8UmVnRXhwfSBldnQgTmFtZSBvZiB0aGUgZXZlbnQgdG8gcmVtb3ZlIHRoZSBsaXN0ZW5lciBmcm9tLlxuXHQgKiBAcGFyYW0ge0Z1bmN0aW9ufSBsaXN0ZW5lciBNZXRob2QgdG8gcmVtb3ZlIGZyb20gdGhlIGV2ZW50LlxuXHQgKiBAcmV0dXJuIHtPYmplY3R9IEN1cnJlbnQgaW5zdGFuY2Ugb2YgRXZlbnRFbWl0dGVyIGZvciBjaGFpbmluZy5cblx0ICovXG5cdHByb3RvLnJlbW92ZUxpc3RlbmVyID0gZnVuY3Rpb24gcmVtb3ZlTGlzdGVuZXIoZXZ0LCBsaXN0ZW5lcikge1xuXHRcdHZhciBsaXN0ZW5lcnMgPSB0aGlzLmdldExpc3RlbmVyc0FzT2JqZWN0KGV2dCk7XG5cdFx0dmFyIGluZGV4O1xuXHRcdHZhciBrZXk7XG5cblx0XHRmb3IgKGtleSBpbiBsaXN0ZW5lcnMpIHtcblx0XHRcdGlmIChsaXN0ZW5lcnMuaGFzT3duUHJvcGVydHkoa2V5KSkge1xuXHRcdFx0XHRpbmRleCA9IGluZGV4T2ZMaXN0ZW5lcihsaXN0ZW5lcnNba2V5XSwgbGlzdGVuZXIpO1xuXG5cdFx0XHRcdGlmIChpbmRleCAhPT0gLTEpIHtcblx0XHRcdFx0XHRsaXN0ZW5lcnNba2V5XS5zcGxpY2UoaW5kZXgsIDEpO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHRoaXM7XG5cdH07XG5cblx0LyoqXG5cdCAqIEFsaWFzIG9mIHJlbW92ZUxpc3RlbmVyXG5cdCAqL1xuXHRwcm90by5vZmYgPSBhbGlhcygncmVtb3ZlTGlzdGVuZXInKTtcblxuXHQvKipcblx0ICogQWRkcyBsaXN0ZW5lcnMgaW4gYnVsayB1c2luZyB0aGUgbWFuaXB1bGF0ZUxpc3RlbmVycyBtZXRob2QuXG5cdCAqIElmIHlvdSBwYXNzIGFuIG9iamVjdCBhcyB0aGUgc2Vjb25kIGFyZ3VtZW50IHlvdSBjYW4gYWRkIHRvIG11bHRpcGxlIGV2ZW50cyBhdCBvbmNlLiBUaGUgb2JqZWN0IHNob3VsZCBjb250YWluIGtleSB2YWx1ZSBwYWlycyBvZiBldmVudHMgYW5kIGxpc3RlbmVycyBvciBsaXN0ZW5lciBhcnJheXMuIFlvdSBjYW4gYWxzbyBwYXNzIGl0IGFuIGV2ZW50IG5hbWUgYW5kIGFuIGFycmF5IG9mIGxpc3RlbmVycyB0byBiZSBhZGRlZC5cblx0ICogWW91IGNhbiBhbHNvIHBhc3MgaXQgYSByZWd1bGFyIGV4cHJlc3Npb24gdG8gYWRkIHRoZSBhcnJheSBvZiBsaXN0ZW5lcnMgdG8gYWxsIGV2ZW50cyB0aGF0IG1hdGNoIGl0LlxuXHQgKiBZZWFoLCB0aGlzIGZ1bmN0aW9uIGRvZXMgcXVpdGUgYSBiaXQuIFRoYXQncyBwcm9iYWJseSBhIGJhZCB0aGluZy5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd8T2JqZWN0fFJlZ0V4cH0gZXZ0IEFuIGV2ZW50IG5hbWUgaWYgeW91IHdpbGwgcGFzcyBhbiBhcnJheSBvZiBsaXN0ZW5lcnMgbmV4dC4gQW4gb2JqZWN0IGlmIHlvdSB3aXNoIHRvIGFkZCB0byBtdWx0aXBsZSBldmVudHMgYXQgb25jZS5cblx0ICogQHBhcmFtIHtGdW5jdGlvbltdfSBbbGlzdGVuZXJzXSBBbiBvcHRpb25hbCBhcnJheSBvZiBsaXN0ZW5lciBmdW5jdGlvbnMgdG8gYWRkLlxuXHQgKiBAcmV0dXJuIHtPYmplY3R9IEN1cnJlbnQgaW5zdGFuY2Ugb2YgRXZlbnRFbWl0dGVyIGZvciBjaGFpbmluZy5cblx0ICovXG5cdHByb3RvLmFkZExpc3RlbmVycyA9IGZ1bmN0aW9uIGFkZExpc3RlbmVycyhldnQsIGxpc3RlbmVycykge1xuXHRcdC8vIFBhc3MgdGhyb3VnaCB0byBtYW5pcHVsYXRlTGlzdGVuZXJzXG5cdFx0cmV0dXJuIHRoaXMubWFuaXB1bGF0ZUxpc3RlbmVycyhmYWxzZSwgZXZ0LCBsaXN0ZW5lcnMpO1xuXHR9O1xuXG5cdC8qKlxuXHQgKiBSZW1vdmVzIGxpc3RlbmVycyBpbiBidWxrIHVzaW5nIHRoZSBtYW5pcHVsYXRlTGlzdGVuZXJzIG1ldGhvZC5cblx0ICogSWYgeW91IHBhc3MgYW4gb2JqZWN0IGFzIHRoZSBzZWNvbmQgYXJndW1lbnQgeW91IGNhbiByZW1vdmUgZnJvbSBtdWx0aXBsZSBldmVudHMgYXQgb25jZS4gVGhlIG9iamVjdCBzaG91bGQgY29udGFpbiBrZXkgdmFsdWUgcGFpcnMgb2YgZXZlbnRzIGFuZCBsaXN0ZW5lcnMgb3IgbGlzdGVuZXIgYXJyYXlzLlxuXHQgKiBZb3UgY2FuIGFsc28gcGFzcyBpdCBhbiBldmVudCBuYW1lIGFuZCBhbiBhcnJheSBvZiBsaXN0ZW5lcnMgdG8gYmUgcmVtb3ZlZC5cblx0ICogWW91IGNhbiBhbHNvIHBhc3MgaXQgYSByZWd1bGFyIGV4cHJlc3Npb24gdG8gcmVtb3ZlIHRoZSBsaXN0ZW5lcnMgZnJvbSBhbGwgZXZlbnRzIHRoYXQgbWF0Y2ggaXQuXG5cdCAqXG5cdCAqIEBwYXJhbSB7U3RyaW5nfE9iamVjdHxSZWdFeHB9IGV2dCBBbiBldmVudCBuYW1lIGlmIHlvdSB3aWxsIHBhc3MgYW4gYXJyYXkgb2YgbGlzdGVuZXJzIG5leHQuIEFuIG9iamVjdCBpZiB5b3Ugd2lzaCB0byByZW1vdmUgZnJvbSBtdWx0aXBsZSBldmVudHMgYXQgb25jZS5cblx0ICogQHBhcmFtIHtGdW5jdGlvbltdfSBbbGlzdGVuZXJzXSBBbiBvcHRpb25hbCBhcnJheSBvZiBsaXN0ZW5lciBmdW5jdGlvbnMgdG8gcmVtb3ZlLlxuXHQgKiBAcmV0dXJuIHtPYmplY3R9IEN1cnJlbnQgaW5zdGFuY2Ugb2YgRXZlbnRFbWl0dGVyIGZvciBjaGFpbmluZy5cblx0ICovXG5cdHByb3RvLnJlbW92ZUxpc3RlbmVycyA9IGZ1bmN0aW9uIHJlbW92ZUxpc3RlbmVycyhldnQsIGxpc3RlbmVycykge1xuXHRcdC8vIFBhc3MgdGhyb3VnaCB0byBtYW5pcHVsYXRlTGlzdGVuZXJzXG5cdFx0cmV0dXJuIHRoaXMubWFuaXB1bGF0ZUxpc3RlbmVycyh0cnVlLCBldnQsIGxpc3RlbmVycyk7XG5cdH07XG5cblx0LyoqXG5cdCAqIEVkaXRzIGxpc3RlbmVycyBpbiBidWxrLiBUaGUgYWRkTGlzdGVuZXJzIGFuZCByZW1vdmVMaXN0ZW5lcnMgbWV0aG9kcyBib3RoIHVzZSB0aGlzIHRvIGRvIHRoZWlyIGpvYi4gWW91IHNob3VsZCByZWFsbHkgdXNlIHRob3NlIGluc3RlYWQsIHRoaXMgaXMgYSBsaXR0bGUgbG93ZXIgbGV2ZWwuXG5cdCAqIFRoZSBmaXJzdCBhcmd1bWVudCB3aWxsIGRldGVybWluZSBpZiB0aGUgbGlzdGVuZXJzIGFyZSByZW1vdmVkICh0cnVlKSBvciBhZGRlZCAoZmFsc2UpLlxuXHQgKiBJZiB5b3UgcGFzcyBhbiBvYmplY3QgYXMgdGhlIHNlY29uZCBhcmd1bWVudCB5b3UgY2FuIGFkZC9yZW1vdmUgZnJvbSBtdWx0aXBsZSBldmVudHMgYXQgb25jZS4gVGhlIG9iamVjdCBzaG91bGQgY29udGFpbiBrZXkgdmFsdWUgcGFpcnMgb2YgZXZlbnRzIGFuZCBsaXN0ZW5lcnMgb3IgbGlzdGVuZXIgYXJyYXlzLlxuXHQgKiBZb3UgY2FuIGFsc28gcGFzcyBpdCBhbiBldmVudCBuYW1lIGFuZCBhbiBhcnJheSBvZiBsaXN0ZW5lcnMgdG8gYmUgYWRkZWQvcmVtb3ZlZC5cblx0ICogWW91IGNhbiBhbHNvIHBhc3MgaXQgYSByZWd1bGFyIGV4cHJlc3Npb24gdG8gbWFuaXB1bGF0ZSB0aGUgbGlzdGVuZXJzIG9mIGFsbCBldmVudHMgdGhhdCBtYXRjaCBpdC5cblx0ICpcblx0ICogQHBhcmFtIHtCb29sZWFufSByZW1vdmUgVHJ1ZSBpZiB5b3Ugd2FudCB0byByZW1vdmUgbGlzdGVuZXJzLCBmYWxzZSBpZiB5b3Ugd2FudCB0byBhZGQuXG5cdCAqIEBwYXJhbSB7U3RyaW5nfE9iamVjdHxSZWdFeHB9IGV2dCBBbiBldmVudCBuYW1lIGlmIHlvdSB3aWxsIHBhc3MgYW4gYXJyYXkgb2YgbGlzdGVuZXJzIG5leHQuIEFuIG9iamVjdCBpZiB5b3Ugd2lzaCB0byBhZGQvcmVtb3ZlIGZyb20gbXVsdGlwbGUgZXZlbnRzIGF0IG9uY2UuXG5cdCAqIEBwYXJhbSB7RnVuY3Rpb25bXX0gW2xpc3RlbmVyc10gQW4gb3B0aW9uYWwgYXJyYXkgb2YgbGlzdGVuZXIgZnVuY3Rpb25zIHRvIGFkZC9yZW1vdmUuXG5cdCAqIEByZXR1cm4ge09iamVjdH0gQ3VycmVudCBpbnN0YW5jZSBvZiBFdmVudEVtaXR0ZXIgZm9yIGNoYWluaW5nLlxuXHQgKi9cblx0cHJvdG8ubWFuaXB1bGF0ZUxpc3RlbmVycyA9IGZ1bmN0aW9uIG1hbmlwdWxhdGVMaXN0ZW5lcnMocmVtb3ZlLCBldnQsIGxpc3RlbmVycykge1xuXHRcdHZhciBpO1xuXHRcdHZhciB2YWx1ZTtcblx0XHR2YXIgc2luZ2xlID0gcmVtb3ZlID8gdGhpcy5yZW1vdmVMaXN0ZW5lciA6IHRoaXMuYWRkTGlzdGVuZXI7XG5cdFx0dmFyIG11bHRpcGxlID0gcmVtb3ZlID8gdGhpcy5yZW1vdmVMaXN0ZW5lcnMgOiB0aGlzLmFkZExpc3RlbmVycztcblxuXHRcdC8vIElmIGV2dCBpcyBhbiBvYmplY3QgdGhlbiBwYXNzIGVhY2ggb2YgaXQncyBwcm9wZXJ0aWVzIHRvIHRoaXMgbWV0aG9kXG5cdFx0aWYgKHR5cGVvZiBldnQgPT09ICdvYmplY3QnICYmICEoZXZ0IGluc3RhbmNlb2YgUmVnRXhwKSkge1xuXHRcdFx0Zm9yIChpIGluIGV2dCkge1xuXHRcdFx0XHRpZiAoZXZ0Lmhhc093blByb3BlcnR5KGkpICYmICh2YWx1ZSA9IGV2dFtpXSkpIHtcblx0XHRcdFx0XHQvLyBQYXNzIHRoZSBzaW5nbGUgbGlzdGVuZXIgc3RyYWlnaHQgdGhyb3VnaCB0byB0aGUgc2luZ3VsYXIgbWV0aG9kXG5cdFx0XHRcdFx0aWYgKHR5cGVvZiB2YWx1ZSA9PT0gJ2Z1bmN0aW9uJykge1xuXHRcdFx0XHRcdFx0c2luZ2xlLmNhbGwodGhpcywgaSwgdmFsdWUpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRlbHNlIHtcblx0XHRcdFx0XHRcdC8vIE90aGVyd2lzZSBwYXNzIGJhY2sgdG8gdGhlIG11bHRpcGxlIGZ1bmN0aW9uXG5cdFx0XHRcdFx0XHRtdWx0aXBsZS5jYWxsKHRoaXMsIGksIHZhbHVlKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHR9XG5cdFx0ZWxzZSB7XG5cdFx0XHQvLyBTbyBldnQgbXVzdCBiZSBhIHN0cmluZ1xuXHRcdFx0Ly8gQW5kIGxpc3RlbmVycyBtdXN0IGJlIGFuIGFycmF5IG9mIGxpc3RlbmVyc1xuXHRcdFx0Ly8gTG9vcCBvdmVyIGl0IGFuZCBwYXNzIGVhY2ggb25lIHRvIHRoZSBtdWx0aXBsZSBtZXRob2Rcblx0XHRcdGkgPSBsaXN0ZW5lcnMubGVuZ3RoO1xuXHRcdFx0d2hpbGUgKGktLSkge1xuXHRcdFx0XHRzaW5nbGUuY2FsbCh0aGlzLCBldnQsIGxpc3RlbmVyc1tpXSk7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHRoaXM7XG5cdH07XG5cblx0LyoqXG5cdCAqIFJlbW92ZXMgYWxsIGxpc3RlbmVycyBmcm9tIGEgc3BlY2lmaWVkIGV2ZW50LlxuXHQgKiBJZiB5b3UgZG8gbm90IHNwZWNpZnkgYW4gZXZlbnQgdGhlbiBhbGwgbGlzdGVuZXJzIHdpbGwgYmUgcmVtb3ZlZC5cblx0ICogVGhhdCBtZWFucyBldmVyeSBldmVudCB3aWxsIGJlIGVtcHRpZWQuXG5cdCAqIFlvdSBjYW4gYWxzbyBwYXNzIGEgcmVnZXggdG8gcmVtb3ZlIGFsbCBldmVudHMgdGhhdCBtYXRjaCBpdC5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd8UmVnRXhwfSBbZXZ0XSBPcHRpb25hbCBuYW1lIG9mIHRoZSBldmVudCB0byByZW1vdmUgYWxsIGxpc3RlbmVycyBmb3IuIFdpbGwgcmVtb3ZlIGZyb20gZXZlcnkgZXZlbnQgaWYgbm90IHBhc3NlZC5cblx0ICogQHJldHVybiB7T2JqZWN0fSBDdXJyZW50IGluc3RhbmNlIG9mIEV2ZW50RW1pdHRlciBmb3IgY2hhaW5pbmcuXG5cdCAqL1xuXHRwcm90by5yZW1vdmVFdmVudCA9IGZ1bmN0aW9uIHJlbW92ZUV2ZW50KGV2dCkge1xuXHRcdHZhciB0eXBlID0gdHlwZW9mIGV2dDtcblx0XHR2YXIgZXZlbnRzID0gdGhpcy5fZ2V0RXZlbnRzKCk7XG5cdFx0dmFyIGtleTtcblxuXHRcdC8vIFJlbW92ZSBkaWZmZXJlbnQgdGhpbmdzIGRlcGVuZGluZyBvbiB0aGUgc3RhdGUgb2YgZXZ0XG5cdFx0aWYgKHR5cGUgPT09ICdzdHJpbmcnKSB7XG5cdFx0XHQvLyBSZW1vdmUgYWxsIGxpc3RlbmVycyBmb3IgdGhlIHNwZWNpZmllZCBldmVudFxuXHRcdFx0ZGVsZXRlIGV2ZW50c1tldnRdO1xuXHRcdH1cblx0XHRlbHNlIGlmIChldnQgaW5zdGFuY2VvZiBSZWdFeHApIHtcblx0XHRcdC8vIFJlbW92ZSBhbGwgZXZlbnRzIG1hdGNoaW5nIHRoZSByZWdleC5cblx0XHRcdGZvciAoa2V5IGluIGV2ZW50cykge1xuXHRcdFx0XHRpZiAoZXZlbnRzLmhhc093blByb3BlcnR5KGtleSkgJiYgZXZ0LnRlc3Qoa2V5KSkge1xuXHRcdFx0XHRcdGRlbGV0ZSBldmVudHNba2V5XTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdH1cblx0XHRlbHNlIHtcblx0XHRcdC8vIFJlbW92ZSBhbGwgbGlzdGVuZXJzIGluIGFsbCBldmVudHNcblx0XHRcdGRlbGV0ZSB0aGlzLl9ldmVudHM7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHRoaXM7XG5cdH07XG5cblx0LyoqXG5cdCAqIEFsaWFzIG9mIHJlbW92ZUV2ZW50LlxuXHQgKlxuXHQgKiBBZGRlZCB0byBtaXJyb3IgdGhlIG5vZGUgQVBJLlxuXHQgKi9cblx0cHJvdG8ucmVtb3ZlQWxsTGlzdGVuZXJzID0gYWxpYXMoJ3JlbW92ZUV2ZW50Jyk7XG5cblx0LyoqXG5cdCAqIEVtaXRzIGFuIGV2ZW50IG9mIHlvdXIgY2hvaWNlLlxuXHQgKiBXaGVuIGVtaXR0ZWQsIGV2ZXJ5IGxpc3RlbmVyIGF0dGFjaGVkIHRvIHRoYXQgZXZlbnQgd2lsbCBiZSBleGVjdXRlZC5cblx0ICogSWYgeW91IHBhc3MgdGhlIG9wdGlvbmFsIGFyZ3VtZW50IGFycmF5IHRoZW4gdGhvc2UgYXJndW1lbnRzIHdpbGwgYmUgcGFzc2VkIHRvIGV2ZXJ5IGxpc3RlbmVyIHVwb24gZXhlY3V0aW9uLlxuXHQgKiBCZWNhdXNlIGl0IHVzZXMgYGFwcGx5YCwgeW91ciBhcnJheSBvZiBhcmd1bWVudHMgd2lsbCBiZSBwYXNzZWQgYXMgaWYgeW91IHdyb3RlIHRoZW0gb3V0IHNlcGFyYXRlbHkuXG5cdCAqIFNvIHRoZXkgd2lsbCBub3QgYXJyaXZlIHdpdGhpbiB0aGUgYXJyYXkgb24gdGhlIG90aGVyIHNpZGUsIHRoZXkgd2lsbCBiZSBzZXBhcmF0ZS5cblx0ICogWW91IGNhbiBhbHNvIHBhc3MgYSByZWd1bGFyIGV4cHJlc3Npb24gdG8gZW1pdCB0byBhbGwgZXZlbnRzIHRoYXQgbWF0Y2ggaXQuXG5cdCAqXG5cdCAqIEBwYXJhbSB7U3RyaW5nfFJlZ0V4cH0gZXZ0IE5hbWUgb2YgdGhlIGV2ZW50IHRvIGVtaXQgYW5kIGV4ZWN1dGUgbGlzdGVuZXJzIGZvci5cblx0ICogQHBhcmFtIHtBcnJheX0gW2FyZ3NdIE9wdGlvbmFsIGFycmF5IG9mIGFyZ3VtZW50cyB0byBiZSBwYXNzZWQgdG8gZWFjaCBsaXN0ZW5lci5cblx0ICogQHJldHVybiB7T2JqZWN0fSBDdXJyZW50IGluc3RhbmNlIG9mIEV2ZW50RW1pdHRlciBmb3IgY2hhaW5pbmcuXG5cdCAqL1xuXHRwcm90by5lbWl0RXZlbnQgPSBmdW5jdGlvbiBlbWl0RXZlbnQoZXZ0LCBhcmdzKSB7XG5cdFx0dmFyIGxpc3RlbmVycyA9IHRoaXMuZ2V0TGlzdGVuZXJzQXNPYmplY3QoZXZ0KTtcblx0XHR2YXIgbGlzdGVuZXI7XG5cdFx0dmFyIGk7XG5cdFx0dmFyIGtleTtcblx0XHR2YXIgcmVzcG9uc2U7XG5cblx0XHRmb3IgKGtleSBpbiBsaXN0ZW5lcnMpIHtcblx0XHRcdGlmIChsaXN0ZW5lcnMuaGFzT3duUHJvcGVydHkoa2V5KSkge1xuXHRcdFx0XHRpID0gbGlzdGVuZXJzW2tleV0ubGVuZ3RoO1xuXG5cdFx0XHRcdHdoaWxlIChpLS0pIHtcblx0XHRcdFx0XHQvLyBJZiB0aGUgbGlzdGVuZXIgcmV0dXJucyB0cnVlIHRoZW4gaXQgc2hhbGwgYmUgcmVtb3ZlZCBmcm9tIHRoZSBldmVudFxuXHRcdFx0XHRcdC8vIFRoZSBmdW5jdGlvbiBpcyBleGVjdXRlZCBlaXRoZXIgd2l0aCBhIGJhc2ljIGNhbGwgb3IgYW4gYXBwbHkgaWYgdGhlcmUgaXMgYW4gYXJncyBhcnJheVxuXHRcdFx0XHRcdGxpc3RlbmVyID0gbGlzdGVuZXJzW2tleV1baV07XG5cblx0XHRcdFx0XHRpZiAobGlzdGVuZXIub25jZSA9PT0gdHJ1ZSkge1xuXHRcdFx0XHRcdFx0dGhpcy5yZW1vdmVMaXN0ZW5lcihldnQsIGxpc3RlbmVyLmxpc3RlbmVyKTtcblx0XHRcdFx0XHR9XG5cblx0XHRcdFx0XHRyZXNwb25zZSA9IGxpc3RlbmVyLmxpc3RlbmVyLmFwcGx5KHRoaXMsIGFyZ3MgfHwgW10pO1xuXG5cdFx0XHRcdFx0aWYgKHJlc3BvbnNlID09PSB0aGlzLl9nZXRPbmNlUmV0dXJuVmFsdWUoKSkge1xuXHRcdFx0XHRcdFx0dGhpcy5yZW1vdmVMaXN0ZW5lcihldnQsIGxpc3RlbmVyLmxpc3RlbmVyKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHR9XG5cblx0XHRyZXR1cm4gdGhpcztcblx0fTtcblxuXHQvKipcblx0ICogQWxpYXMgb2YgZW1pdEV2ZW50XG5cdCAqL1xuXHRwcm90by50cmlnZ2VyID0gYWxpYXMoJ2VtaXRFdmVudCcpO1xuXG5cdC8qKlxuXHQgKiBTdWJ0bHkgZGlmZmVyZW50IGZyb20gZW1pdEV2ZW50IGluIHRoYXQgaXQgd2lsbCBwYXNzIGl0cyBhcmd1bWVudHMgb24gdG8gdGhlIGxpc3RlbmVycywgYXMgb3Bwb3NlZCB0byB0YWtpbmcgYSBzaW5nbGUgYXJyYXkgb2YgYXJndW1lbnRzIHRvIHBhc3Mgb24uXG5cdCAqIEFzIHdpdGggZW1pdEV2ZW50LCB5b3UgY2FuIHBhc3MgYSByZWdleCBpbiBwbGFjZSBvZiB0aGUgZXZlbnQgbmFtZSB0byBlbWl0IHRvIGFsbCBldmVudHMgdGhhdCBtYXRjaCBpdC5cblx0ICpcblx0ICogQHBhcmFtIHtTdHJpbmd8UmVnRXhwfSBldnQgTmFtZSBvZiB0aGUgZXZlbnQgdG8gZW1pdCBhbmQgZXhlY3V0ZSBsaXN0ZW5lcnMgZm9yLlxuXHQgKiBAcGFyYW0gey4uLip9IE9wdGlvbmFsIGFkZGl0aW9uYWwgYXJndW1lbnRzIHRvIGJlIHBhc3NlZCB0byBlYWNoIGxpc3RlbmVyLlxuXHQgKiBAcmV0dXJuIHtPYmplY3R9IEN1cnJlbnQgaW5zdGFuY2Ugb2YgRXZlbnRFbWl0dGVyIGZvciBjaGFpbmluZy5cblx0ICovXG5cdHByb3RvLmVtaXQgPSBmdW5jdGlvbiBlbWl0KGV2dCkge1xuXHRcdHZhciBhcmdzID0gQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwoYXJndW1lbnRzLCAxKTtcblx0XHRyZXR1cm4gdGhpcy5lbWl0RXZlbnQoZXZ0LCBhcmdzKTtcblx0fTtcblxuXHQvKipcblx0ICogU2V0cyB0aGUgY3VycmVudCB2YWx1ZSB0byBjaGVjayBhZ2FpbnN0IHdoZW4gZXhlY3V0aW5nIGxpc3RlbmVycy4gSWYgYVxuXHQgKiBsaXN0ZW5lcnMgcmV0dXJuIHZhbHVlIG1hdGNoZXMgdGhlIG9uZSBzZXQgaGVyZSB0aGVuIGl0IHdpbGwgYmUgcmVtb3ZlZFxuXHQgKiBhZnRlciBleGVjdXRpb24uIFRoaXMgdmFsdWUgZGVmYXVsdHMgdG8gdHJ1ZS5cblx0ICpcblx0ICogQHBhcmFtIHsqfSB2YWx1ZSBUaGUgbmV3IHZhbHVlIHRvIGNoZWNrIGZvciB3aGVuIGV4ZWN1dGluZyBsaXN0ZW5lcnMuXG5cdCAqIEByZXR1cm4ge09iamVjdH0gQ3VycmVudCBpbnN0YW5jZSBvZiBFdmVudEVtaXR0ZXIgZm9yIGNoYWluaW5nLlxuXHQgKi9cblx0cHJvdG8uc2V0T25jZVJldHVyblZhbHVlID0gZnVuY3Rpb24gc2V0T25jZVJldHVyblZhbHVlKHZhbHVlKSB7XG5cdFx0dGhpcy5fb25jZVJldHVyblZhbHVlID0gdmFsdWU7XG5cdFx0cmV0dXJuIHRoaXM7XG5cdH07XG5cblx0LyoqXG5cdCAqIEZldGNoZXMgdGhlIGN1cnJlbnQgdmFsdWUgdG8gY2hlY2sgYWdhaW5zdCB3aGVuIGV4ZWN1dGluZyBsaXN0ZW5lcnMuIElmXG5cdCAqIHRoZSBsaXN0ZW5lcnMgcmV0dXJuIHZhbHVlIG1hdGNoZXMgdGhpcyBvbmUgdGhlbiBpdCBzaG91bGQgYmUgcmVtb3ZlZFxuXHQgKiBhdXRvbWF0aWNhbGx5LiBJdCB3aWxsIHJldHVybiB0cnVlIGJ5IGRlZmF1bHQuXG5cdCAqXG5cdCAqIEByZXR1cm4geyp8Qm9vbGVhbn0gVGhlIGN1cnJlbnQgdmFsdWUgdG8gY2hlY2sgZm9yIG9yIHRoZSBkZWZhdWx0LCB0cnVlLlxuXHQgKiBAYXBpIHByaXZhdGVcblx0ICovXG5cdHByb3RvLl9nZXRPbmNlUmV0dXJuVmFsdWUgPSBmdW5jdGlvbiBfZ2V0T25jZVJldHVyblZhbHVlKCkge1xuXHRcdGlmICh0aGlzLmhhc093blByb3BlcnR5KCdfb25jZVJldHVyblZhbHVlJykpIHtcblx0XHRcdHJldHVybiB0aGlzLl9vbmNlUmV0dXJuVmFsdWU7XG5cdFx0fVxuXHRcdGVsc2Uge1xuXHRcdFx0cmV0dXJuIHRydWU7XG5cdFx0fVxuXHR9O1xuXG5cdC8qKlxuXHQgKiBGZXRjaGVzIHRoZSBldmVudHMgb2JqZWN0IGFuZCBjcmVhdGVzIG9uZSBpZiByZXF1aXJlZC5cblx0ICpcblx0ICogQHJldHVybiB7T2JqZWN0fSBUaGUgZXZlbnRzIHN0b3JhZ2Ugb2JqZWN0LlxuXHQgKiBAYXBpIHByaXZhdGVcblx0ICovXG5cdHByb3RvLl9nZXRFdmVudHMgPSBmdW5jdGlvbiBfZ2V0RXZlbnRzKCkge1xuXHRcdHJldHVybiB0aGlzLl9ldmVudHMgfHwgKHRoaXMuX2V2ZW50cyA9IHt9KTtcblx0fTtcblxuXHQvKipcblx0ICogUmV2ZXJ0cyB0aGUgZ2xvYmFsIHtAbGluayBFdmVudEVtaXR0ZXJ9IHRvIGl0cyBwcmV2aW91cyB2YWx1ZSBhbmQgcmV0dXJucyBhIHJlZmVyZW5jZSB0byB0aGlzIHZlcnNpb24uXG5cdCAqXG5cdCAqIEByZXR1cm4ge0Z1bmN0aW9ufSBOb24gY29uZmxpY3RpbmcgRXZlbnRFbWl0dGVyIGNsYXNzLlxuXHQgKi9cblx0RXZlbnRFbWl0dGVyLm5vQ29uZmxpY3QgPSBmdW5jdGlvbiBub0NvbmZsaWN0KCkge1xuXHRcdGV4cG9ydHMuRXZlbnRFbWl0dGVyID0gb3JpZ2luYWxHbG9iYWxWYWx1ZTtcblx0XHRyZXR1cm4gRXZlbnRFbWl0dGVyO1xuXHR9O1xuXG5cdHJldHVybiBFdmVudEVtaXR0ZXI7XG59KCkpO1xuLyoganNoaW50IGlnbm9yZTplbmQgKi9cblxuXG5cblx0dmFyIHZhbGlkYXRlVHlwZUZ1bmN0aW9uID0gZnVuY3Rpb24oIHZhbHVlLCBuYW1lICkge1xuXHRcdHZhbGlkYXRlVHlwZSggdmFsdWUsIG5hbWUsIHR5cGVvZiB2YWx1ZSA9PT0gXCJ1bmRlZmluZWRcIiB8fCB0eXBlb2YgdmFsdWUgPT09IFwiZnVuY3Rpb25cIiwgXCJGdW5jdGlvblwiICk7XG5cdH07XG5cblxuXG5cblx0dmFyIHN1cGVyR2V0LCBzdXBlckluaXQsXG5cdFx0Z2xvYmFsRWUgPSBuZXcgRXZlbnRFbWl0dGVyKCk7XG5cblx0ZnVuY3Rpb24gdmFsaWRhdGVUeXBlRXZlbnQoIHZhbHVlLCBuYW1lICkge1xuXHRcdHZhbGlkYXRlVHlwZSggdmFsdWUsIG5hbWUsIHR5cGVvZiB2YWx1ZSA9PT0gXCJzdHJpbmdcIiB8fCB2YWx1ZSBpbnN0YW5jZW9mIFJlZ0V4cCwgXCJTdHJpbmcgb3IgUmVnRXhwXCIgKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHZhbGlkYXRlVGhlbkNhbGwoIG1ldGhvZCwgc2VsZiApIHtcblx0XHRyZXR1cm4gZnVuY3Rpb24oIGV2ZW50LCBsaXN0ZW5lciApIHtcblx0XHRcdHZhbGlkYXRlUHJlc2VuY2UoIGV2ZW50LCBcImV2ZW50XCIgKTtcblx0XHRcdHZhbGlkYXRlVHlwZUV2ZW50KCBldmVudCwgXCJldmVudFwiICk7XG5cblx0XHRcdHZhbGlkYXRlUHJlc2VuY2UoIGxpc3RlbmVyLCBcImxpc3RlbmVyXCIgKTtcblx0XHRcdHZhbGlkYXRlVHlwZUZ1bmN0aW9uKCBsaXN0ZW5lciwgXCJsaXN0ZW5lclwiICk7XG5cblx0XHRcdHJldHVybiBzZWxmWyBtZXRob2QgXS5hcHBseSggc2VsZiwgYXJndW1lbnRzICk7XG5cdFx0fTtcblx0fVxuXG5cdGZ1bmN0aW9uIG9mZiggc2VsZiApIHtcblx0XHRyZXR1cm4gdmFsaWRhdGVUaGVuQ2FsbCggXCJvZmZcIiwgc2VsZiApO1xuXHR9XG5cblx0ZnVuY3Rpb24gb24oIHNlbGYgKSB7XG5cdFx0cmV0dXJuIHZhbGlkYXRlVGhlbkNhbGwoIFwib25cIiwgc2VsZiApO1xuXHR9XG5cblx0ZnVuY3Rpb24gb25jZSggc2VsZiApIHtcblx0XHRyZXR1cm4gdmFsaWRhdGVUaGVuQ2FsbCggXCJvbmNlXCIsIHNlbGYgKTtcblx0fVxuXG5cdENsZHIub2ZmID0gb2ZmKCBnbG9iYWxFZSApO1xuXHRDbGRyLm9uID0gb24oIGdsb2JhbEVlICk7XG5cdENsZHIub25jZSA9IG9uY2UoIGdsb2JhbEVlICk7XG5cblx0LyoqXG5cdCAqIE92ZXJsb2FkIENsZHIucHJvdG90eXBlLmluaXQoKS5cblx0ICovXG5cdHN1cGVySW5pdCA9IENsZHIucHJvdG90eXBlLmluaXQ7XG5cdENsZHIucHJvdG90eXBlLmluaXQgPSBmdW5jdGlvbigpIHtcblx0XHR2YXIgZWU7XG5cdFx0dGhpcy5lZSA9IGVlID0gbmV3IEV2ZW50RW1pdHRlcigpO1xuXHRcdHRoaXMub2ZmID0gb2ZmKCBlZSApO1xuXHRcdHRoaXMub24gPSBvbiggZWUgKTtcblx0XHR0aGlzLm9uY2UgPSBvbmNlKCBlZSApO1xuXHRcdHN1cGVySW5pdC5hcHBseSggdGhpcywgYXJndW1lbnRzICk7XG5cdH07XG5cblx0LyoqXG5cdCAqIGdldE92ZXJsb2FkIGlzIGVuY2Fwc3VsYXRlZCwgYmVjYXVzZSBvZiBjbGRyL3VucmVzb2x2ZWQuIElmIGl0J3MgbG9hZGVkXG5cdCAqIGFmdGVyIGNsZHIvZXZlbnQgKGFuZCBub3RlIGl0IG92ZXJ3cml0ZXMgLmdldCksIGl0IGNhbiB0cmlnZ2VyIHRoaXNcblx0ICogb3ZlcmxvYWQgYWdhaW4uXG5cdCAqL1xuXHRmdW5jdGlvbiBnZXRPdmVybG9hZCgpIHtcblxuXHRcdC8qKlxuXHRcdCAqIE92ZXJsb2FkIENsZHIucHJvdG90eXBlLmdldCgpLlxuXHRcdCAqL1xuXHRcdHN1cGVyR2V0ID0gQ2xkci5wcm90b3R5cGUuZ2V0O1xuXHRcdENsZHIucHJvdG90eXBlLmdldCA9IGZ1bmN0aW9uKCBwYXRoICkge1xuXHRcdFx0dmFyIHZhbHVlID0gc3VwZXJHZXQuYXBwbHkoIHRoaXMsIGFyZ3VtZW50cyApO1xuXHRcdFx0cGF0aCA9IHBhdGhOb3JtYWxpemUoIHBhdGgsIHRoaXMuYXR0cmlidXRlcyApLmpvaW4oIFwiL1wiICk7XG5cdFx0XHRnbG9iYWxFZS50cmlnZ2VyKCBcImdldFwiLCBbIHBhdGgsIHZhbHVlIF0gKTtcblx0XHRcdHRoaXMuZWUudHJpZ2dlciggXCJnZXRcIiwgWyBwYXRoLCB2YWx1ZSBdICk7XG5cdFx0XHRyZXR1cm4gdmFsdWU7XG5cdFx0fTtcblx0fVxuXG5cdENsZHIuX2V2ZW50SW5pdCA9IGdldE92ZXJsb2FkO1xuXHRnZXRPdmVybG9hZCgpO1xuXG5cdHJldHVybiBDbGRyO1xuXG5cblxuXG59KSk7XG4iLCIvKipcbiAqIENMRFIgSmF2YVNjcmlwdCBMaWJyYXJ5IHYwLjQuMVxuICogaHR0cDovL2pxdWVyeS5jb20vXG4gKlxuICogQ29weXJpZ2h0IDIwMTMgUmFmYWVsIFhhdmllciBkZSBTb3V6YVxuICogUmVsZWFzZWQgdW5kZXIgdGhlIE1JVCBsaWNlbnNlXG4gKiBodHRwOi8vanF1ZXJ5Lm9yZy9saWNlbnNlXG4gKlxuICogRGF0ZTogMjAxNS0wMi0yNVQxMzo1MVpcbiAqL1xuLyohXG4gKiBDTERSIEphdmFTY3JpcHQgTGlicmFyeSB2MC40LjEgMjAxNS0wMi0yNVQxMzo1MVogTUlUIGxpY2Vuc2UgwqkgUmFmYWVsIFhhdmllclxuICogaHR0cDovL2dpdC5pby9oNGxtVmdcbiAqL1xuKGZ1bmN0aW9uKCBmYWN0b3J5ICkge1xuXG5cdGlmICggdHlwZW9mIGRlZmluZSA9PT0gXCJmdW5jdGlvblwiICYmIGRlZmluZS5hbWQgKSB7XG5cdFx0Ly8gQU1ELlxuXHRcdGRlZmluZSggWyBcIi4uL2NsZHJcIiBdLCBmYWN0b3J5ICk7XG5cdH0gZWxzZSBpZiAoIHR5cGVvZiBtb2R1bGUgPT09IFwib2JqZWN0XCIgJiYgdHlwZW9mIG1vZHVsZS5leHBvcnRzID09PSBcIm9iamVjdFwiICkge1xuXHRcdC8vIE5vZGUuIENvbW1vbkpTLlxuXHRcdG1vZHVsZS5leHBvcnRzID0gZmFjdG9yeSggcmVxdWlyZSggXCJjbGRyanNcIiApICk7XG5cdH0gZWxzZSB7XG5cdFx0Ly8gR2xvYmFsXG5cdFx0ZmFjdG9yeSggQ2xkciApO1xuXHR9XG5cbn0oZnVuY3Rpb24oIENsZHIgKSB7XG5cblx0Ly8gQnVpbGQgb3B0aW1pemF0aW9uIGhhY2sgdG8gYXZvaWQgZHVwbGljYXRpbmcgZnVuY3Rpb25zIGFjcm9zcyBtb2R1bGVzLlxuXHR2YXIgYWx3YXlzQXJyYXkgPSBDbGRyLl9hbHdheXNBcnJheTtcblxuXG5cblx0dmFyIHN1cHBsZW1lbnRhbE1haW4gPSBmdW5jdGlvbiggY2xkciApIHtcblxuXHRcdHZhciBwcmVwZW5kLCBzdXBwbGVtZW50YWw7XG5cdFx0XG5cdFx0cHJlcGVuZCA9IGZ1bmN0aW9uKCBwcmVwZW5kICkge1xuXHRcdFx0cmV0dXJuIGZ1bmN0aW9uKCBwYXRoICkge1xuXHRcdFx0XHRwYXRoID0gYWx3YXlzQXJyYXkoIHBhdGggKTtcblx0XHRcdFx0cmV0dXJuIGNsZHIuZ2V0KCBbIHByZXBlbmQgXS5jb25jYXQoIHBhdGggKSApO1xuXHRcdFx0fTtcblx0XHR9O1xuXG5cdFx0c3VwcGxlbWVudGFsID0gcHJlcGVuZCggXCJzdXBwbGVtZW50YWxcIiApO1xuXG5cdFx0Ly8gV2VlayBEYXRhXG5cdFx0Ly8gaHR0cDovL3d3dy51bmljb2RlLm9yZy9yZXBvcnRzL3RyMzUvdHIzNS1kYXRlcy5odG1sI1dlZWtfRGF0YVxuXHRcdHN1cHBsZW1lbnRhbC53ZWVrRGF0YSA9IHByZXBlbmQoIFwic3VwcGxlbWVudGFsL3dlZWtEYXRhXCIgKTtcblxuXHRcdHN1cHBsZW1lbnRhbC53ZWVrRGF0YS5maXJzdERheSA9IGZ1bmN0aW9uKCkge1xuXHRcdFx0cmV0dXJuIGNsZHIuZ2V0KCBcInN1cHBsZW1lbnRhbC93ZWVrRGF0YS9maXJzdERheS97dGVycml0b3J5fVwiICkgfHxcblx0XHRcdFx0Y2xkci5nZXQoIFwic3VwcGxlbWVudGFsL3dlZWtEYXRhL2ZpcnN0RGF5LzAwMVwiICk7XG5cdFx0fTtcblxuXHRcdHN1cHBsZW1lbnRhbC53ZWVrRGF0YS5taW5EYXlzID0gZnVuY3Rpb24oKSB7XG5cdFx0XHR2YXIgbWluRGF5cyA9IGNsZHIuZ2V0KCBcInN1cHBsZW1lbnRhbC93ZWVrRGF0YS9taW5EYXlzL3t0ZXJyaXRvcnl9XCIgKSB8fFxuXHRcdFx0XHRjbGRyLmdldCggXCJzdXBwbGVtZW50YWwvd2Vla0RhdGEvbWluRGF5cy8wMDFcIiApO1xuXHRcdFx0cmV0dXJuIHBhcnNlSW50KCBtaW5EYXlzLCAxMCApO1xuXHRcdH07XG5cblx0XHQvLyBUaW1lIERhdGFcblx0XHQvLyBodHRwOi8vd3d3LnVuaWNvZGUub3JnL3JlcG9ydHMvdHIzNS90cjM1LWRhdGVzLmh0bWwjVGltZV9EYXRhXG5cdFx0c3VwcGxlbWVudGFsLnRpbWVEYXRhID0gcHJlcGVuZCggXCJzdXBwbGVtZW50YWwvdGltZURhdGFcIiApO1xuXG5cdFx0c3VwcGxlbWVudGFsLnRpbWVEYXRhLmFsbG93ZWQgPSBmdW5jdGlvbigpIHtcblx0XHRcdHJldHVybiBjbGRyLmdldCggXCJzdXBwbGVtZW50YWwvdGltZURhdGEve3RlcnJpdG9yeX0vX2FsbG93ZWRcIiApIHx8XG5cdFx0XHRcdGNsZHIuZ2V0KCBcInN1cHBsZW1lbnRhbC90aW1lRGF0YS8wMDEvX2FsbG93ZWRcIiApO1xuXHRcdH07XG5cblx0XHRzdXBwbGVtZW50YWwudGltZURhdGEucHJlZmVycmVkID0gZnVuY3Rpb24oKSB7XG5cdFx0XHRyZXR1cm4gY2xkci5nZXQoIFwic3VwcGxlbWVudGFsL3RpbWVEYXRhL3t0ZXJyaXRvcnl9L19wcmVmZXJyZWRcIiApIHx8XG5cdFx0XHRcdGNsZHIuZ2V0KCBcInN1cHBsZW1lbnRhbC90aW1lRGF0YS8wMDEvX3ByZWZlcnJlZFwiICk7XG5cdFx0fTtcblxuXHRcdHJldHVybiBzdXBwbGVtZW50YWw7XG5cblx0fTtcblxuXG5cblxuXHR2YXIgaW5pdFN1cGVyID0gQ2xkci5wcm90b3R5cGUuaW5pdDtcblxuXHQvKipcblx0ICogLmluaXQoKSBhdXRvbWF0aWNhbGx5IHJhbiBvbiBjb25zdHJ1Y3Rpb24uXG5cdCAqXG5cdCAqIE92ZXJsb2FkIC5pbml0KCkuXG5cdCAqL1xuXHRDbGRyLnByb3RvdHlwZS5pbml0ID0gZnVuY3Rpb24oKSB7XG5cdFx0aW5pdFN1cGVyLmFwcGx5KCB0aGlzLCBhcmd1bWVudHMgKTtcblx0XHR0aGlzLnN1cHBsZW1lbnRhbCA9IHN1cHBsZW1lbnRhbE1haW4oIHRoaXMgKTtcblx0fTtcblxuXHRyZXR1cm4gQ2xkcjtcblxuXG5cblxufSkpO1xuIiwiLyoqXG4gKiBHbG9iYWxpemUgdjEuMC4wXG4gKlxuICogaHR0cDovL2dpdGh1Yi5jb20vanF1ZXJ5L2dsb2JhbGl6ZVxuICpcbiAqIENvcHlyaWdodCBqUXVlcnkgRm91bmRhdGlvbiBhbmQgb3RoZXIgY29udHJpYnV0b3JzXG4gKiBSZWxlYXNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2VcbiAqIGh0dHA6Ly9qcXVlcnkub3JnL2xpY2Vuc2VcbiAqXG4gKiBEYXRlOiAyMDE1LTA0LTIzVDEyOjAyWlxuICovXG4vKiFcbiAqIEdsb2JhbGl6ZSB2MS4wLjAgMjAxNS0wNC0yM1QxMjowMlogUmVsZWFzZWQgdW5kZXIgdGhlIE1JVCBsaWNlbnNlXG4gKiBodHRwOi8vZ2l0LmlvL1RyZFFid1xuICovXG4oZnVuY3Rpb24oIHJvb3QsIGZhY3RvcnkgKSB7XG5cblx0Ly8gVU1EIHJldHVybkV4cG9ydHNcblx0aWYgKCB0eXBlb2YgZGVmaW5lID09PSBcImZ1bmN0aW9uXCIgJiYgZGVmaW5lLmFtZCApIHtcblxuXHRcdC8vIEFNRFxuXHRcdGRlZmluZShbXG5cdFx0XHRcImNsZHJcIixcblx0XHRcdFwiY2xkci9ldmVudFwiXG5cdFx0XSwgZmFjdG9yeSApO1xuXHR9IGVsc2UgaWYgKCB0eXBlb2YgZXhwb3J0cyA9PT0gXCJvYmplY3RcIiApIHtcblxuXHRcdC8vIE5vZGUsIENvbW1vbkpTXG5cdFx0bW9kdWxlLmV4cG9ydHMgPSBmYWN0b3J5KCByZXF1aXJlKCBcImNsZHJqc1wiICkgKTtcblx0fSBlbHNlIHtcblxuXHRcdC8vIEdsb2JhbFxuXHRcdHJvb3QuR2xvYmFsaXplID0gZmFjdG9yeSggcm9vdC5DbGRyICk7XG5cdH1cbn0oIHRoaXMsIGZ1bmN0aW9uKCBDbGRyICkge1xuXG5cbi8qKlxuICogQSB0b1N0cmluZyBtZXRob2QgdGhhdCBvdXRwdXRzIG1lYW5pbmdmdWwgdmFsdWVzIGZvciBvYmplY3RzIG9yIGFycmF5cyBhbmRcbiAqIHN0aWxsIHBlcmZvcm1zIGFzIGZhc3QgYXMgYSBwbGFpbiBzdHJpbmcgaW4gY2FzZSB2YXJpYWJsZSBpcyBzdHJpbmcsIG9yIGFzXG4gKiBmYXN0IGFzIGBcIlwiICsgbnVtYmVyYCBpbiBjYXNlIHZhcmlhYmxlIGlzIGEgbnVtYmVyLlxuICogUmVmOiBodHRwOi8vanNwZXJmLmNvbS9teS1zdHJpbmdpZnlcbiAqL1xudmFyIHRvU3RyaW5nID0gZnVuY3Rpb24oIHZhcmlhYmxlICkge1xuXHRyZXR1cm4gdHlwZW9mIHZhcmlhYmxlID09PSBcInN0cmluZ1wiID8gdmFyaWFibGUgOiAoIHR5cGVvZiB2YXJpYWJsZSA9PT0gXCJudW1iZXJcIiA/IFwiXCIgK1xuXHRcdHZhcmlhYmxlIDogSlNPTi5zdHJpbmdpZnkoIHZhcmlhYmxlICkgKTtcbn07XG5cblxuXG5cbi8qKlxuICogZm9ybWF0TWVzc2FnZSggbWVzc2FnZSwgZGF0YSApXG4gKlxuICogQG1lc3NhZ2UgW1N0cmluZ10gQSBtZXNzYWdlIHdpdGggb3B0aW9uYWwge3ZhcnN9IHRvIGJlIHJlcGxhY2VkLlxuICpcbiAqIEBkYXRhIFtBcnJheSBvciBKU09OXSBPYmplY3Qgd2l0aCByZXBsYWNpbmctdmFyaWFibGVzIGNvbnRlbnQuXG4gKlxuICogUmV0dXJuIHRoZSBmb3JtYXR0ZWQgbWVzc2FnZS4gRm9yIGV4YW1wbGU6XG4gKlxuICogLSBmb3JtYXRNZXNzYWdlKCBcInswfSBzZWNvbmRcIiwgWyAxIF0gKTsgLy8gMSBzZWNvbmRcbiAqXG4gKiAtIGZvcm1hdE1lc3NhZ2UoIFwiezB9L3sxfVwiLCBbXCJtXCIsIFwic1wiXSApOyAvLyBtL3NcbiAqXG4gKiAtIGZvcm1hdE1lc3NhZ2UoIFwie25hbWV9IDx7ZW1haWx9PlwiLCB7XG4gKiAgICAgbmFtZTogXCJGb29cIixcbiAqICAgICBlbWFpbDogXCJiYXJAYmF6LnF1eFwiXG4gKiAgIH0pOyAvLyBGb28gPGJhckBiYXoucXV4PlxuICovXG52YXIgZm9ybWF0TWVzc2FnZSA9IGZ1bmN0aW9uKCBtZXNzYWdlLCBkYXRhICkge1xuXG5cdC8vIFJlcGxhY2Uge2F0dHJpYnV0ZX0nc1xuXHRtZXNzYWdlID0gbWVzc2FnZS5yZXBsYWNlKCAve1swLTlhLXpBLVotXy4gXSt9L2csIGZ1bmN0aW9uKCBuYW1lICkge1xuXHRcdG5hbWUgPSBuYW1lLnJlcGxhY2UoIC9eeyhbXn1dKil9JC8sIFwiJDFcIiApO1xuXHRcdHJldHVybiB0b1N0cmluZyggZGF0YVsgbmFtZSBdICk7XG5cdH0pO1xuXG5cdHJldHVybiBtZXNzYWdlO1xufTtcblxuXG5cblxudmFyIG9iamVjdEV4dGVuZCA9IGZ1bmN0aW9uKCkge1xuXHR2YXIgZGVzdGluYXRpb24gPSBhcmd1bWVudHNbIDAgXSxcblx0XHRzb3VyY2VzID0gW10uc2xpY2UuY2FsbCggYXJndW1lbnRzLCAxICk7XG5cblx0c291cmNlcy5mb3JFYWNoKGZ1bmN0aW9uKCBzb3VyY2UgKSB7XG5cdFx0dmFyIHByb3A7XG5cdFx0Zm9yICggcHJvcCBpbiBzb3VyY2UgKSB7XG5cdFx0XHRkZXN0aW5hdGlvblsgcHJvcCBdID0gc291cmNlWyBwcm9wIF07XG5cdFx0fVxuXHR9KTtcblxuXHRyZXR1cm4gZGVzdGluYXRpb247XG59O1xuXG5cblxuXG52YXIgY3JlYXRlRXJyb3IgPSBmdW5jdGlvbiggY29kZSwgbWVzc2FnZSwgYXR0cmlidXRlcyApIHtcblx0dmFyIGVycm9yO1xuXG5cdG1lc3NhZ2UgPSBjb2RlICsgKCBtZXNzYWdlID8gXCI6IFwiICsgZm9ybWF0TWVzc2FnZSggbWVzc2FnZSwgYXR0cmlidXRlcyApIDogXCJcIiApO1xuXHRlcnJvciA9IG5ldyBFcnJvciggbWVzc2FnZSApO1xuXHRlcnJvci5jb2RlID0gY29kZTtcblxuXHRvYmplY3RFeHRlbmQoIGVycm9yLCBhdHRyaWJ1dGVzICk7XG5cblx0cmV0dXJuIGVycm9yO1xufTtcblxuXG5cblxudmFyIHZhbGlkYXRlID0gZnVuY3Rpb24oIGNvZGUsIG1lc3NhZ2UsIGNoZWNrLCBhdHRyaWJ1dGVzICkge1xuXHRpZiAoICFjaGVjayApIHtcblx0XHR0aHJvdyBjcmVhdGVFcnJvciggY29kZSwgbWVzc2FnZSwgYXR0cmlidXRlcyApO1xuXHR9XG59O1xuXG5cblxuXG52YXIgYWx3YXlzQXJyYXkgPSBmdW5jdGlvbiggc3RyaW5nT3JBcnJheSApIHtcblx0cmV0dXJuIEFycmF5LmlzQXJyYXkoIHN0cmluZ09yQXJyYXkgKSA/IHN0cmluZ09yQXJyYXkgOiBzdHJpbmdPckFycmF5ID8gWyBzdHJpbmdPckFycmF5IF0gOiBbXTtcbn07XG5cblxuXG5cbnZhciB2YWxpZGF0ZUNsZHIgPSBmdW5jdGlvbiggcGF0aCwgdmFsdWUsIG9wdGlvbnMgKSB7XG5cdHZhciBza2lwQm9vbGVhbjtcblx0b3B0aW9ucyA9IG9wdGlvbnMgfHwge307XG5cblx0c2tpcEJvb2xlYW4gPSBhbHdheXNBcnJheSggb3B0aW9ucy5za2lwICkuc29tZShmdW5jdGlvbiggcGF0aFJlICkge1xuXHRcdHJldHVybiBwYXRoUmUudGVzdCggcGF0aCApO1xuXHR9KTtcblxuXHR2YWxpZGF0ZSggXCJFX01JU1NJTkdfQ0xEUlwiLCBcIk1pc3NpbmcgcmVxdWlyZWQgQ0xEUiBjb250ZW50IGB7cGF0aH1gLlwiLCB2YWx1ZSB8fCBza2lwQm9vbGVhbiwge1xuXHRcdHBhdGg6IHBhdGhcblx0fSk7XG59O1xuXG5cblxuXG52YXIgdmFsaWRhdGVEZWZhdWx0TG9jYWxlID0gZnVuY3Rpb24oIHZhbHVlICkge1xuXHR2YWxpZGF0ZSggXCJFX0RFRkFVTFRfTE9DQUxFX05PVF9ERUZJTkVEXCIsIFwiRGVmYXVsdCBsb2NhbGUgaGFzIG5vdCBiZWVuIGRlZmluZWQuXCIsXG5cdFx0dmFsdWUgIT09IHVuZGVmaW5lZCwge30gKTtcbn07XG5cblxuXG5cbnZhciB2YWxpZGF0ZVBhcmFtZXRlclByZXNlbmNlID0gZnVuY3Rpb24oIHZhbHVlLCBuYW1lICkge1xuXHR2YWxpZGF0ZSggXCJFX01JU1NJTkdfUEFSQU1FVEVSXCIsIFwiTWlzc2luZyByZXF1aXJlZCBwYXJhbWV0ZXIgYHtuYW1lfWAuXCIsXG5cdFx0dmFsdWUgIT09IHVuZGVmaW5lZCwgeyBuYW1lOiBuYW1lIH0pO1xufTtcblxuXG5cblxuLyoqXG4gKiByYW5nZSggdmFsdWUsIG5hbWUsIG1pbmltdW0sIG1heGltdW0gKVxuICpcbiAqIEB2YWx1ZSBbTnVtYmVyXS5cbiAqXG4gKiBAbmFtZSBbU3RyaW5nXSBuYW1lIG9mIHZhcmlhYmxlLlxuICpcbiAqIEBtaW5pbXVtIFtOdW1iZXJdLiBUaGUgbG93ZXN0IHZhbGlkIHZhbHVlLCBpbmNsdXNpdmUuXG4gKlxuICogQG1heGltdW0gW051bWJlcl0uIFRoZSBncmVhdGVzdCB2YWxpZCB2YWx1ZSwgaW5jbHVzaXZlLlxuICovXG52YXIgdmFsaWRhdGVQYXJhbWV0ZXJSYW5nZSA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSwgbWluaW11bSwgbWF4aW11bSApIHtcblx0dmFsaWRhdGUoXG5cdFx0XCJFX1BBUl9PVVRfT0ZfUkFOR0VcIixcblx0XHRcIlBhcmFtZXRlciBge25hbWV9YCBoYXMgdmFsdWUgYHt2YWx1ZX1gIG91dCBvZiByYW5nZSBbe21pbmltdW19LCB7bWF4aW11bX1dLlwiLFxuXHRcdHZhbHVlID09PSB1bmRlZmluZWQgfHwgdmFsdWUgPj0gbWluaW11bSAmJiB2YWx1ZSA8PSBtYXhpbXVtLFxuXHRcdHtcblx0XHRcdG1heGltdW06IG1heGltdW0sXG5cdFx0XHRtaW5pbXVtOiBtaW5pbXVtLFxuXHRcdFx0bmFtZTogbmFtZSxcblx0XHRcdHZhbHVlOiB2YWx1ZVxuXHRcdH1cblx0KTtcbn07XG5cblxuXG5cbnZhciB2YWxpZGF0ZVBhcmFtZXRlclR5cGUgPSBmdW5jdGlvbiggdmFsdWUsIG5hbWUsIGNoZWNrLCBleHBlY3RlZCApIHtcblx0dmFsaWRhdGUoXG5cdFx0XCJFX0lOVkFMSURfUEFSX1RZUEVcIixcblx0XHRcIkludmFsaWQgYHtuYW1lfWAgcGFyYW1ldGVyICh7dmFsdWV9KS4ge2V4cGVjdGVkfSBleHBlY3RlZC5cIixcblx0XHRjaGVjayxcblx0XHR7XG5cdFx0XHRleHBlY3RlZDogZXhwZWN0ZWQsXG5cdFx0XHRuYW1lOiBuYW1lLFxuXHRcdFx0dmFsdWU6IHZhbHVlXG5cdFx0fVxuXHQpO1xufTtcblxuXG5cblxudmFyIHZhbGlkYXRlUGFyYW1ldGVyVHlwZUxvY2FsZSA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSApIHtcblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlKFxuXHRcdHZhbHVlLFxuXHRcdG5hbWUsXG5cdFx0dmFsdWUgPT09IHVuZGVmaW5lZCB8fCB0eXBlb2YgdmFsdWUgPT09IFwic3RyaW5nXCIgfHwgdmFsdWUgaW5zdGFuY2VvZiBDbGRyLFxuXHRcdFwiU3RyaW5nIG9yIENsZHIgaW5zdGFuY2VcIlxuXHQpO1xufTtcblxuXG5cblxuLyoqXG4gKiBGdW5jdGlvbiBpbnNwaXJlZCBieSBqUXVlcnkgQ29yZSwgYnV0IHJlZHVjZWQgdG8gb3VyIHVzZSBjYXNlLlxuICovXG52YXIgaXNQbGFpbk9iamVjdCA9IGZ1bmN0aW9uKCBvYmogKSB7XG5cdHJldHVybiBvYmogIT09IG51bGwgJiYgXCJcIiArIG9iaiA9PT0gXCJbb2JqZWN0IE9iamVjdF1cIjtcbn07XG5cblxuXG5cbnZhciB2YWxpZGF0ZVBhcmFtZXRlclR5cGVQbGFpbk9iamVjdCA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSApIHtcblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlKFxuXHRcdHZhbHVlLFxuXHRcdG5hbWUsXG5cdFx0dmFsdWUgPT09IHVuZGVmaW5lZCB8fCBpc1BsYWluT2JqZWN0KCB2YWx1ZSApLFxuXHRcdFwiUGxhaW4gT2JqZWN0XCJcblx0KTtcbn07XG5cblxuXG5cbnZhciBhbHdheXNDbGRyID0gZnVuY3Rpb24oIGxvY2FsZU9yQ2xkciApIHtcblx0cmV0dXJuIGxvY2FsZU9yQ2xkciBpbnN0YW5jZW9mIENsZHIgPyBsb2NhbGVPckNsZHIgOiBuZXcgQ2xkciggbG9jYWxlT3JDbGRyICk7XG59O1xuXG5cblxuXG4vLyByZWY6IGh0dHBzOi8vZGV2ZWxvcGVyLm1vemlsbGEub3JnL2VuLVVTL2RvY3MvV2ViL0phdmFTY3JpcHQvR3VpZGUvUmVndWxhcl9FeHByZXNzaW9ucz9yZWRpcmVjdGxvY2FsZT1lbi1VUyZyZWRpcmVjdHNsdWc9SmF2YVNjcmlwdCUyRkd1aWRlJTJGUmVndWxhcl9FeHByZXNzaW9uc1xudmFyIHJlZ2V4cEVzY2FwZSA9IGZ1bmN0aW9uKCBzdHJpbmcgKSB7XG5cdHJldHVybiBzdHJpbmcucmVwbGFjZSggLyhbLiorP149IToke30oKXxcXFtcXF1cXC9cXFxcXSkvZywgXCJcXFxcJDFcIiApO1xufTtcblxuXG5cblxudmFyIHN0cmluZ1BhZCA9IGZ1bmN0aW9uKCBzdHIsIGNvdW50LCByaWdodCApIHtcblx0dmFyIGxlbmd0aDtcblx0aWYgKCB0eXBlb2Ygc3RyICE9PSBcInN0cmluZ1wiICkge1xuXHRcdHN0ciA9IFN0cmluZyggc3RyICk7XG5cdH1cblx0Zm9yICggbGVuZ3RoID0gc3RyLmxlbmd0aDsgbGVuZ3RoIDwgY291bnQ7IGxlbmd0aCArPSAxICkge1xuXHRcdHN0ciA9ICggcmlnaHQgPyAoIHN0ciArIFwiMFwiICkgOiAoIFwiMFwiICsgc3RyICkgKTtcblx0fVxuXHRyZXR1cm4gc3RyO1xufTtcblxuXG5cblxuZnVuY3Rpb24gdmFsaWRhdGVMaWtlbHlTdWJ0YWdzKCBjbGRyICkge1xuXHRjbGRyLm9uY2UoIFwiZ2V0XCIsIHZhbGlkYXRlQ2xkciApO1xuXHRjbGRyLmdldCggXCJzdXBwbGVtZW50YWwvbGlrZWx5U3VidGFnc1wiICk7XG59XG5cbi8qKlxuICogW25ld10gR2xvYmFsaXplKCBsb2NhbGV8Y2xkciApXG4gKlxuICogQGxvY2FsZSBbU3RyaW5nXVxuICpcbiAqIEBjbGRyIFtDbGRyIGluc3RhbmNlXVxuICpcbiAqIENyZWF0ZSBhIEdsb2JhbGl6ZSBpbnN0YW5jZS5cbiAqL1xuZnVuY3Rpb24gR2xvYmFsaXplKCBsb2NhbGUgKSB7XG5cdGlmICggISggdGhpcyBpbnN0YW5jZW9mIEdsb2JhbGl6ZSApICkge1xuXHRcdHJldHVybiBuZXcgR2xvYmFsaXplKCBsb2NhbGUgKTtcblx0fVxuXG5cdHZhbGlkYXRlUGFyYW1ldGVyUHJlc2VuY2UoIGxvY2FsZSwgXCJsb2NhbGVcIiApO1xuXHR2YWxpZGF0ZVBhcmFtZXRlclR5cGVMb2NhbGUoIGxvY2FsZSwgXCJsb2NhbGVcIiApO1xuXG5cdHRoaXMuY2xkciA9IGFsd2F5c0NsZHIoIGxvY2FsZSApO1xuXG5cdHZhbGlkYXRlTGlrZWx5U3VidGFncyggdGhpcy5jbGRyICk7XG59XG5cbi8qKlxuICogR2xvYmFsaXplLmxvYWQoIGpzb24sIC4uLiApXG4gKlxuICogQGpzb24gW0pTT05dXG4gKlxuICogTG9hZCByZXNvbHZlZCBvciB1bnJlc29sdmVkIGNsZHIgZGF0YS5cbiAqIFNvbWV3aGF0IGVxdWl2YWxlbnQgdG8gcHJldmlvdXMgR2xvYmFsaXplLmFkZEN1bHR1cmVJbmZvKC4uLikuXG4gKi9cbkdsb2JhbGl6ZS5sb2FkID0gZnVuY3Rpb24oKSB7XG5cdC8vIHZhbGlkYXRpb25zIGFyZSBkZWxlZ2F0ZWQgdG8gQ2xkci5sb2FkKCkuXG5cdENsZHIubG9hZC5hcHBseSggQ2xkciwgYXJndW1lbnRzICk7XG59O1xuXG4vKipcbiAqIEdsb2JhbGl6ZS5sb2NhbGUoIFtsb2NhbGV8Y2xkcl0gKVxuICpcbiAqIEBsb2NhbGUgW1N0cmluZ11cbiAqXG4gKiBAY2xkciBbQ2xkciBpbnN0YW5jZV1cbiAqXG4gKiBTZXQgZGVmYXVsdCBDbGRyIGluc3RhbmNlIGlmIGxvY2FsZSBvciBjbGRyIGFyZ3VtZW50IGlzIHBhc3NlZC5cbiAqXG4gKiBSZXR1cm4gdGhlIGRlZmF1bHQgQ2xkciBpbnN0YW5jZS5cbiAqL1xuR2xvYmFsaXplLmxvY2FsZSA9IGZ1bmN0aW9uKCBsb2NhbGUgKSB7XG5cdHZhbGlkYXRlUGFyYW1ldGVyVHlwZUxvY2FsZSggbG9jYWxlLCBcImxvY2FsZVwiICk7XG5cblx0aWYgKCBhcmd1bWVudHMubGVuZ3RoICkge1xuXHRcdHRoaXMuY2xkciA9IGFsd2F5c0NsZHIoIGxvY2FsZSApO1xuXHRcdHZhbGlkYXRlTGlrZWx5U3VidGFncyggdGhpcy5jbGRyICk7XG5cdH1cblx0cmV0dXJuIHRoaXMuY2xkcjtcbn07XG5cbi8qKlxuICogT3B0aW1pemF0aW9uIHRvIGF2b2lkIGR1cGxpY2F0aW5nIHNvbWUgaW50ZXJuYWwgZnVuY3Rpb25zIGFjcm9zcyBtb2R1bGVzLlxuICovXG5HbG9iYWxpemUuX2Fsd2F5c0FycmF5ID0gYWx3YXlzQXJyYXk7XG5HbG9iYWxpemUuX2NyZWF0ZUVycm9yID0gY3JlYXRlRXJyb3I7XG5HbG9iYWxpemUuX2Zvcm1hdE1lc3NhZ2UgPSBmb3JtYXRNZXNzYWdlO1xuR2xvYmFsaXplLl9pc1BsYWluT2JqZWN0ID0gaXNQbGFpbk9iamVjdDtcbkdsb2JhbGl6ZS5fb2JqZWN0RXh0ZW5kID0gb2JqZWN0RXh0ZW5kO1xuR2xvYmFsaXplLl9yZWdleHBFc2NhcGUgPSByZWdleHBFc2NhcGU7XG5HbG9iYWxpemUuX3N0cmluZ1BhZCA9IHN0cmluZ1BhZDtcbkdsb2JhbGl6ZS5fdmFsaWRhdGUgPSB2YWxpZGF0ZTtcbkdsb2JhbGl6ZS5fdmFsaWRhdGVDbGRyID0gdmFsaWRhdGVDbGRyO1xuR2xvYmFsaXplLl92YWxpZGF0ZURlZmF1bHRMb2NhbGUgPSB2YWxpZGF0ZURlZmF1bHRMb2NhbGU7XG5HbG9iYWxpemUuX3ZhbGlkYXRlUGFyYW1ldGVyUHJlc2VuY2UgPSB2YWxpZGF0ZVBhcmFtZXRlclByZXNlbmNlO1xuR2xvYmFsaXplLl92YWxpZGF0ZVBhcmFtZXRlclJhbmdlID0gdmFsaWRhdGVQYXJhbWV0ZXJSYW5nZTtcbkdsb2JhbGl6ZS5fdmFsaWRhdGVQYXJhbWV0ZXJUeXBlUGxhaW5PYmplY3QgPSB2YWxpZGF0ZVBhcmFtZXRlclR5cGVQbGFpbk9iamVjdDtcbkdsb2JhbGl6ZS5fdmFsaWRhdGVQYXJhbWV0ZXJUeXBlID0gdmFsaWRhdGVQYXJhbWV0ZXJUeXBlO1xuXG5yZXR1cm4gR2xvYmFsaXplO1xuXG5cblxuXG59KSk7XG4iLCIvKipcbiAqIEdsb2JhbGl6ZSB2MS4wLjBcbiAqXG4gKiBodHRwOi8vZ2l0aHViLmNvbS9qcXVlcnkvZ2xvYmFsaXplXG4gKlxuICogQ29weXJpZ2h0IGpRdWVyeSBGb3VuZGF0aW9uIGFuZCBvdGhlciBjb250cmlidXRvcnNcbiAqIFJlbGVhc2VkIHVuZGVyIHRoZSBNSVQgbGljZW5zZVxuICogaHR0cDovL2pxdWVyeS5vcmcvbGljZW5zZVxuICpcbiAqIERhdGU6IDIwMTUtMDQtMjNUMTI6MDJaXG4gKi9cbi8qIVxuICogR2xvYmFsaXplIHYxLjAuMCAyMDE1LTA0LTIzVDEyOjAyWiBSZWxlYXNlZCB1bmRlciB0aGUgTUlUIGxpY2Vuc2VcbiAqIGh0dHA6Ly9naXQuaW8vVHJkUWJ3XG4gKi9cbihmdW5jdGlvbiggcm9vdCwgZmFjdG9yeSApIHtcblxuXHQvLyBVTUQgcmV0dXJuRXhwb3J0c1xuXHRpZiAoIHR5cGVvZiBkZWZpbmUgPT09IFwiZnVuY3Rpb25cIiAmJiBkZWZpbmUuYW1kICkge1xuXG5cdFx0Ly8gQU1EXG5cdFx0ZGVmaW5lKFtcblx0XHRcdFwiY2xkclwiLFxuXHRcdFx0XCIuLi9nbG9iYWxpemVcIixcblx0XHRcdFwiY2xkci9ldmVudFwiLFxuXHRcdFx0XCJjbGRyL3N1cHBsZW1lbnRhbFwiXG5cdFx0XSwgZmFjdG9yeSApO1xuXHR9IGVsc2UgaWYgKCB0eXBlb2YgZXhwb3J0cyA9PT0gXCJvYmplY3RcIiApIHtcblxuXHRcdC8vIE5vZGUsIENvbW1vbkpTXG5cdFx0bW9kdWxlLmV4cG9ydHMgPSBmYWN0b3J5KCByZXF1aXJlKCBcImNsZHJqc1wiICksIHJlcXVpcmUoIFwiZ2xvYmFsaXplXCIgKSApO1xuXHR9IGVsc2Uge1xuXG5cdFx0Ly8gR2xvYmFsXG5cdFx0ZmFjdG9yeSggcm9vdC5DbGRyLCByb290Lkdsb2JhbGl6ZSApO1xuXHR9XG59KHRoaXMsIGZ1bmN0aW9uKCBDbGRyLCBHbG9iYWxpemUgKSB7XG5cbnZhciBjcmVhdGVFcnJvciA9IEdsb2JhbGl6ZS5fY3JlYXRlRXJyb3IsXG5cdG9iamVjdEV4dGVuZCA9IEdsb2JhbGl6ZS5fb2JqZWN0RXh0ZW5kLFxuXHRyZWdleHBFc2NhcGUgPSBHbG9iYWxpemUuX3JlZ2V4cEVzY2FwZSxcblx0c3RyaW5nUGFkID0gR2xvYmFsaXplLl9zdHJpbmdQYWQsXG5cdHZhbGlkYXRlQ2xkciA9IEdsb2JhbGl6ZS5fdmFsaWRhdGVDbGRyLFxuXHR2YWxpZGF0ZURlZmF1bHRMb2NhbGUgPSBHbG9iYWxpemUuX3ZhbGlkYXRlRGVmYXVsdExvY2FsZSxcblx0dmFsaWRhdGVQYXJhbWV0ZXJQcmVzZW5jZSA9IEdsb2JhbGl6ZS5fdmFsaWRhdGVQYXJhbWV0ZXJQcmVzZW5jZSxcblx0dmFsaWRhdGVQYXJhbWV0ZXJSYW5nZSA9IEdsb2JhbGl6ZS5fdmFsaWRhdGVQYXJhbWV0ZXJSYW5nZSxcblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlID0gR2xvYmFsaXplLl92YWxpZGF0ZVBhcmFtZXRlclR5cGUsXG5cdHZhbGlkYXRlUGFyYW1ldGVyVHlwZVBsYWluT2JqZWN0ID0gR2xvYmFsaXplLl92YWxpZGF0ZVBhcmFtZXRlclR5cGVQbGFpbk9iamVjdDtcblxuXG52YXIgY3JlYXRlRXJyb3JVbnN1cHBvcnRlZEZlYXR1cmUgPSBmdW5jdGlvbiggZmVhdHVyZSApIHtcblx0cmV0dXJuIGNyZWF0ZUVycm9yKCBcIkVfVU5TVVBQT1JURURcIiwgXCJVbnN1cHBvcnRlZCB7ZmVhdHVyZX0uXCIsIHtcblx0XHRmZWF0dXJlOiBmZWF0dXJlXG5cdH0pO1xufTtcblxuXG5cblxudmFyIHZhbGlkYXRlUGFyYW1ldGVyVHlwZU51bWJlciA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSApIHtcblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlKFxuXHRcdHZhbHVlLFxuXHRcdG5hbWUsXG5cdFx0dmFsdWUgPT09IHVuZGVmaW5lZCB8fCB0eXBlb2YgdmFsdWUgPT09IFwibnVtYmVyXCIsXG5cdFx0XCJOdW1iZXJcIlxuXHQpO1xufTtcblxuXG5cblxudmFyIHZhbGlkYXRlUGFyYW1ldGVyVHlwZVN0cmluZyA9IGZ1bmN0aW9uKCB2YWx1ZSwgbmFtZSApIHtcblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlKFxuXHRcdHZhbHVlLFxuXHRcdG5hbWUsXG5cdFx0dmFsdWUgPT09IHVuZGVmaW5lZCB8fCB0eXBlb2YgdmFsdWUgPT09IFwic3RyaW5nXCIsXG5cdFx0XCJhIHN0cmluZ1wiXG5cdCk7XG59O1xuXG5cblxuXG4vKipcbiAqIGdvdXBpbmdTZXBhcmF0b3IoIG51bWJlciwgcHJpbWFyeUdyb3VwaW5nU2l6ZSwgc2Vjb25kYXJ5R3JvdXBpbmdTaXplIClcbiAqXG4gKiBAbnVtYmVyIFtOdW1iZXJdLlxuICpcbiAqIEBwcmltYXJ5R3JvdXBpbmdTaXplIFtOdW1iZXJdXG4gKlxuICogQHNlY29uZGFyeUdyb3VwaW5nU2l6ZSBbTnVtYmVyXVxuICpcbiAqIFJldHVybiB0aGUgZm9ybWF0dGVkIG51bWJlciB3aXRoIGdyb3VwIHNlcGFyYXRvci5cbiAqL1xudmFyIG51bWJlckZvcm1hdEdyb3VwaW5nU2VwYXJhdG9yID0gZnVuY3Rpb24oIG51bWJlciwgcHJpbWFyeUdyb3VwaW5nU2l6ZSwgc2Vjb25kYXJ5R3JvdXBpbmdTaXplICkge1xuXHR2YXIgaW5kZXgsXG5cdFx0Y3VycmVudEdyb3VwaW5nU2l6ZSA9IHByaW1hcnlHcm91cGluZ1NpemUsXG5cdFx0cmV0ID0gXCJcIixcblx0XHRzZXAgPSBcIixcIixcblx0XHRzd2l0Y2hUb1NlY29uZGFyeSA9IHNlY29uZGFyeUdyb3VwaW5nU2l6ZSA/IHRydWUgOiBmYWxzZTtcblxuXHRudW1iZXIgPSBTdHJpbmcoIG51bWJlciApLnNwbGl0KCBcIi5cIiApO1xuXHRpbmRleCA9IG51bWJlclsgMCBdLmxlbmd0aDtcblxuXHR3aGlsZSAoIGluZGV4ID4gY3VycmVudEdyb3VwaW5nU2l6ZSApIHtcblx0XHRyZXQgPSBudW1iZXJbIDAgXS5zbGljZSggaW5kZXggLSBjdXJyZW50R3JvdXBpbmdTaXplLCBpbmRleCApICtcblx0XHRcdCggcmV0Lmxlbmd0aCA/IHNlcCA6IFwiXCIgKSArIHJldDtcblx0XHRpbmRleCAtPSBjdXJyZW50R3JvdXBpbmdTaXplO1xuXHRcdGlmICggc3dpdGNoVG9TZWNvbmRhcnkgKSB7XG5cdFx0XHRjdXJyZW50R3JvdXBpbmdTaXplID0gc2Vjb25kYXJ5R3JvdXBpbmdTaXplO1xuXHRcdFx0c3dpdGNoVG9TZWNvbmRhcnkgPSBmYWxzZTtcblx0XHR9XG5cdH1cblxuXHRudW1iZXJbIDAgXSA9IG51bWJlclsgMCBdLnNsaWNlKCAwLCBpbmRleCApICsgKCByZXQubGVuZ3RoID8gc2VwIDogXCJcIiApICsgcmV0O1xuXHRyZXR1cm4gbnVtYmVyLmpvaW4oIFwiLlwiICk7XG59O1xuXG5cblxuXG4vKipcbiAqIGludGVnZXJGcmFjdGlvbkRpZ2l0cyggbnVtYmVyLCBtaW5pbXVtSW50ZWdlckRpZ2l0cywgbWluaW11bUZyYWN0aW9uRGlnaXRzLFxuICogbWF4aW11bUZyYWN0aW9uRGlnaXRzLCByb3VuZCwgcm91bmRJbmNyZW1lbnQgKVxuICpcbiAqIEBudW1iZXIgW051bWJlcl1cbiAqXG4gKiBAbWluaW11bUludGVnZXJEaWdpdHMgW051bWJlcl1cbiAqXG4gKiBAbWluaW11bUZyYWN0aW9uRGlnaXRzIFtOdW1iZXJdXG4gKlxuICogQG1heGltdW1GcmFjdGlvbkRpZ2l0cyBbTnVtYmVyXVxuICpcbiAqIEByb3VuZCBbRnVuY3Rpb25dXG4gKlxuICogQHJvdW5kSW5jcmVtZW50IFtGdW5jdGlvbl1cbiAqXG4gKiBSZXR1cm4gdGhlIGZvcm1hdHRlZCBpbnRlZ2VyIGFuZCBmcmFjdGlvbiBkaWdpdHMuXG4gKi9cbnZhciBudW1iZXJGb3JtYXRJbnRlZ2VyRnJhY3Rpb25EaWdpdHMgPSBmdW5jdGlvbiggbnVtYmVyLCBtaW5pbXVtSW50ZWdlckRpZ2l0cywgbWluaW11bUZyYWN0aW9uRGlnaXRzLCBtYXhpbXVtRnJhY3Rpb25EaWdpdHMsIHJvdW5kLFxuXHRyb3VuZEluY3JlbWVudCApIHtcblxuXHQvLyBGcmFjdGlvblxuXHRpZiAoIG1heGltdW1GcmFjdGlvbkRpZ2l0cyApIHtcblxuXHRcdC8vIFJvdW5kaW5nXG5cdFx0aWYgKCByb3VuZEluY3JlbWVudCApIHtcblx0XHRcdG51bWJlciA9IHJvdW5kKCBudW1iZXIsIHJvdW5kSW5jcmVtZW50ICk7XG5cblx0XHQvLyBNYXhpbXVtIGZyYWN0aW9uIGRpZ2l0c1xuXHRcdH0gZWxzZSB7XG5cdFx0XHRudW1iZXIgPSByb3VuZCggbnVtYmVyLCB7IGV4cG9uZW50OiAtbWF4aW11bUZyYWN0aW9uRGlnaXRzIH0gKTtcblx0XHR9XG5cblx0XHQvLyBNaW5pbXVtIGZyYWN0aW9uIGRpZ2l0c1xuXHRcdGlmICggbWluaW11bUZyYWN0aW9uRGlnaXRzICkge1xuXHRcdFx0bnVtYmVyID0gU3RyaW5nKCBudW1iZXIgKS5zcGxpdCggXCIuXCIgKTtcblx0XHRcdG51bWJlclsgMSBdID0gc3RyaW5nUGFkKCBudW1iZXJbIDEgXSB8fCBcIlwiLCBtaW5pbXVtRnJhY3Rpb25EaWdpdHMsIHRydWUgKTtcblx0XHRcdG51bWJlciA9IG51bWJlci5qb2luKCBcIi5cIiApO1xuXHRcdH1cblx0fSBlbHNlIHtcblx0XHRudW1iZXIgPSByb3VuZCggbnVtYmVyICk7XG5cdH1cblxuXHRudW1iZXIgPSBTdHJpbmcoIG51bWJlciApO1xuXG5cdC8vIE1pbmltdW0gaW50ZWdlciBkaWdpdHNcblx0aWYgKCBtaW5pbXVtSW50ZWdlckRpZ2l0cyApIHtcblx0XHRudW1iZXIgPSBudW1iZXIuc3BsaXQoIFwiLlwiICk7XG5cdFx0bnVtYmVyWyAwIF0gPSBzdHJpbmdQYWQoIG51bWJlclsgMCBdLCBtaW5pbXVtSW50ZWdlckRpZ2l0cyApO1xuXHRcdG51bWJlciA9IG51bWJlci5qb2luKCBcIi5cIiApO1xuXHR9XG5cblx0cmV0dXJuIG51bWJlcjtcbn07XG5cblxuXG5cbi8qKlxuICogdG9QcmVjaXNpb24oIG51bWJlciwgcHJlY2lzaW9uLCByb3VuZCApXG4gKlxuICogQG51bWJlciAoTnVtYmVyKVxuICpcbiAqIEBwcmVjaXNpb24gKE51bWJlcikgc2lnbmlmaWNhbnQgZmlndXJlcyBwcmVjaXNpb24gKG5vdCBkZWNpbWFsIHByZWNpc2lvbikuXG4gKlxuICogQHJvdW5kIChGdW5jdGlvbilcbiAqXG4gKiBSZXR1cm4gbnVtYmVyLnRvUHJlY2lzaW9uKCBwcmVjaXNpb24gKSB1c2luZyB0aGUgZ2l2ZW4gcm91bmQgZnVuY3Rpb24uXG4gKi9cbnZhciBudW1iZXJUb1ByZWNpc2lvbiA9IGZ1bmN0aW9uKCBudW1iZXIsIHByZWNpc2lvbiwgcm91bmQgKSB7XG5cdHZhciByb3VuZE9yZGVyO1xuXG5cdC8vIEdldCBudW1iZXIgYXQgdHdvIGV4dHJhIHNpZ25pZmljYW50IGZpZ3VyZSBwcmVjaXNpb24uXG5cdG51bWJlciA9IG51bWJlci50b1ByZWNpc2lvbiggcHJlY2lzaW9uICsgMiApO1xuXG5cdC8vIFRoZW4sIHJvdW5kIGl0IHRvIHRoZSByZXF1aXJlZCBzaWduaWZpY2FudCBmaWd1cmUgcHJlY2lzaW9uLlxuXHRyb3VuZE9yZGVyID0gTWF0aC5jZWlsKCBNYXRoLmxvZyggTWF0aC5hYnMoIG51bWJlciApICkgLyBNYXRoLmxvZyggMTAgKSApO1xuXHRyb3VuZE9yZGVyIC09IHByZWNpc2lvbjtcblxuXHRyZXR1cm4gcm91bmQoIG51bWJlciwgeyBleHBvbmVudDogcm91bmRPcmRlciB9ICk7XG59O1xuXG5cblxuXG4vKipcbiAqIHRvUHJlY2lzaW9uKCBudW1iZXIsIG1pbmltdW1TaWduaWZpY2FudERpZ2l0cywgbWF4aW11bVNpZ25pZmljYW50RGlnaXRzLCByb3VuZCApXG4gKlxuICogQG51bWJlciBbTnVtYmVyXVxuICpcbiAqIEBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMgW051bWJlcl1cbiAqXG4gKiBAbWF4aW11bVNpZ25pZmljYW50RGlnaXRzIFtOdW1iZXJdXG4gKlxuICogQHJvdW5kIFtGdW5jdGlvbl1cbiAqXG4gKiBSZXR1cm4gdGhlIGZvcm1hdHRlZCBzaWduaWZpY2FudCBkaWdpdHMgbnVtYmVyLlxuICovXG52YXIgbnVtYmVyRm9ybWF0U2lnbmlmaWNhbnREaWdpdHMgPSBmdW5jdGlvbiggbnVtYmVyLCBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMsIG1heGltdW1TaWduaWZpY2FudERpZ2l0cywgcm91bmQgKSB7XG5cdHZhciBhdE1pbmltdW0sIGF0TWF4aW11bTtcblxuXHQvLyBTYW5pdHkgY2hlY2suXG5cdGlmICggbWluaW11bVNpZ25pZmljYW50RGlnaXRzID4gbWF4aW11bVNpZ25pZmljYW50RGlnaXRzICkge1xuXHRcdG1heGltdW1TaWduaWZpY2FudERpZ2l0cyA9IG1pbmltdW1TaWduaWZpY2FudERpZ2l0cztcblx0fVxuXG5cdGF0TWluaW11bSA9IG51bWJlclRvUHJlY2lzaW9uKCBudW1iZXIsIG1pbmltdW1TaWduaWZpY2FudERpZ2l0cywgcm91bmQgKTtcblx0YXRNYXhpbXVtID0gbnVtYmVyVG9QcmVjaXNpb24oIG51bWJlciwgbWF4aW11bVNpZ25pZmljYW50RGlnaXRzLCByb3VuZCApO1xuXG5cdC8vIFVzZSBhdE1heGltdW0gb25seSBpZiBpdCBoYXMgbW9yZSBzaWduaWZpY2FudCBkaWdpdHMgdGhhbiBhdE1pbmltdW0uXG5cdG51bWJlciA9ICthdE1pbmltdW0gPT09ICthdE1heGltdW0gPyBhdE1pbmltdW0gOiBhdE1heGltdW07XG5cblx0Ly8gRXhwYW5kIGludGVnZXIgbnVtYmVycywgZWcuIDEyM2U1IHRvIDEyMzAwLlxuXHRudW1iZXIgPSAoICtudW1iZXIgKS50b1N0cmluZyggMTAgKTtcblxuXHRpZiAoICgvZS8pLnRlc3QoIG51bWJlciApICkge1xuXHRcdHRocm93IGNyZWF0ZUVycm9yVW5zdXBwb3J0ZWRGZWF0dXJlKHtcblx0XHRcdGZlYXR1cmU6IFwiaW50ZWdlcnMgb3V0IG9mICgxZTIxLCAxZS03KVwiXG5cdFx0fSk7XG5cdH1cblxuXHQvLyBBZGQgdHJhaWxpbmcgemVyb3MgaWYgbmVjZXNzYXJ5LlxuXHRpZiAoIG1pbmltdW1TaWduaWZpY2FudERpZ2l0cyAtIG51bWJlci5yZXBsYWNlKCAvXjArfFxcLi9nLCBcIlwiICkubGVuZ3RoID4gMCApIHtcblx0XHRudW1iZXIgPSBudW1iZXIuc3BsaXQoIFwiLlwiICk7XG5cdFx0bnVtYmVyWyAxIF0gPSBzdHJpbmdQYWQoIG51bWJlclsgMSBdIHx8IFwiXCIsIG1pbmltdW1TaWduaWZpY2FudERpZ2l0cyAtIG51bWJlclsgMCBdLnJlcGxhY2UoIC9eMCsvLCBcIlwiICkubGVuZ3RoLCB0cnVlICk7XG5cdFx0bnVtYmVyID0gbnVtYmVyLmpvaW4oIFwiLlwiICk7XG5cdH1cblxuXHRyZXR1cm4gbnVtYmVyO1xufTtcblxuXG5cblxuLyoqXG4gKiBmb3JtYXQoIG51bWJlciwgcHJvcGVydGllcyApXG4gKlxuICogQG51bWJlciBbTnVtYmVyXS5cbiAqXG4gKiBAcHJvcGVydGllcyBbT2JqZWN0XSBPdXRwdXQgb2YgbnVtYmVyL2Zvcm1hdC1wcm9wZXJ0aWVzLlxuICpcbiAqIFJldHVybiB0aGUgZm9ybWF0dGVkIG51bWJlci5cbiAqIHJlZjogaHR0cDovL3d3dy51bmljb2RlLm9yZy9yZXBvcnRzL3RyMzUvdHIzNS1udW1iZXJzLmh0bWxcbiAqL1xudmFyIG51bWJlckZvcm1hdCA9IGZ1bmN0aW9uKCBudW1iZXIsIHByb3BlcnRpZXMgKSB7XG5cdHZhciBpbmZpbml0eVN5bWJvbCwgbWF4aW11bUZyYWN0aW9uRGlnaXRzLCBtYXhpbXVtU2lnbmlmaWNhbnREaWdpdHMsIG1pbmltdW1GcmFjdGlvbkRpZ2l0cyxcblx0bWluaW11bUludGVnZXJEaWdpdHMsIG1pbmltdW1TaWduaWZpY2FudERpZ2l0cywgbmFuU3ltYm9sLCBudURpZ2l0c01hcCwgcGFkZGluZywgcHJlZml4LFxuXHRwcmltYXJ5R3JvdXBpbmdTaXplLCBwYXR0ZXJuLCByZXQsIHJvdW5kLCByb3VuZEluY3JlbWVudCwgc2Vjb25kYXJ5R3JvdXBpbmdTaXplLCBzdWZmaXgsXG5cdHN5bWJvbE1hcDtcblxuXHRwYWRkaW5nID0gcHJvcGVydGllc1sgMSBdO1xuXHRtaW5pbXVtSW50ZWdlckRpZ2l0cyA9IHByb3BlcnRpZXNbIDIgXTtcblx0bWluaW11bUZyYWN0aW9uRGlnaXRzID0gcHJvcGVydGllc1sgMyBdO1xuXHRtYXhpbXVtRnJhY3Rpb25EaWdpdHMgPSBwcm9wZXJ0aWVzWyA0IF07XG5cdG1pbmltdW1TaWduaWZpY2FudERpZ2l0cyA9IHByb3BlcnRpZXNbIDUgXTtcblx0bWF4aW11bVNpZ25pZmljYW50RGlnaXRzID0gcHJvcGVydGllc1sgNiBdO1xuXHRyb3VuZEluY3JlbWVudCA9IHByb3BlcnRpZXNbIDcgXTtcblx0cHJpbWFyeUdyb3VwaW5nU2l6ZSA9IHByb3BlcnRpZXNbIDggXTtcblx0c2Vjb25kYXJ5R3JvdXBpbmdTaXplID0gcHJvcGVydGllc1sgOSBdO1xuXHRyb3VuZCA9IHByb3BlcnRpZXNbIDE1IF07XG5cdGluZmluaXR5U3ltYm9sID0gcHJvcGVydGllc1sgMTYgXTtcblx0bmFuU3ltYm9sID0gcHJvcGVydGllc1sgMTcgXTtcblx0c3ltYm9sTWFwID0gcHJvcGVydGllc1sgMTggXTtcblx0bnVEaWdpdHNNYXAgPSBwcm9wZXJ0aWVzWyAxOSBdO1xuXG5cdC8vIE5hTlxuXHRpZiAoIGlzTmFOKCBudW1iZXIgKSApIHtcblx0XHRyZXR1cm4gbmFuU3ltYm9sO1xuXHR9XG5cblx0aWYgKCBudW1iZXIgPCAwICkge1xuXHRcdHBhdHRlcm4gPSBwcm9wZXJ0aWVzWyAxMiBdO1xuXHRcdHByZWZpeCA9IHByb3BlcnRpZXNbIDEzIF07XG5cdFx0c3VmZml4ID0gcHJvcGVydGllc1sgMTQgXTtcblx0fSBlbHNlIHtcblx0XHRwYXR0ZXJuID0gcHJvcGVydGllc1sgMTEgXTtcblx0XHRwcmVmaXggPSBwcm9wZXJ0aWVzWyAwIF07XG5cdFx0c3VmZml4ID0gcHJvcGVydGllc1sgMTAgXTtcblx0fVxuXG5cdC8vIEluZmluaXR5XG5cdGlmICggIWlzRmluaXRlKCBudW1iZXIgKSApIHtcblx0XHRyZXR1cm4gcHJlZml4ICsgaW5maW5pdHlTeW1ib2wgKyBzdWZmaXg7XG5cdH1cblxuXHRyZXQgPSBwcmVmaXg7XG5cblx0Ly8gUGVyY2VudFxuXHRpZiAoIHBhdHRlcm4uaW5kZXhPZiggXCIlXCIgKSAhPT0gLTEgKSB7XG5cdFx0bnVtYmVyICo9IDEwMDtcblxuXHQvLyBQZXIgbWlsbGVcblx0fSBlbHNlIGlmICggcGF0dGVybi5pbmRleE9mKCBcIlxcdTIwMzBcIiApICE9PSAtMSApIHtcblx0XHRudW1iZXIgKj0gMTAwMDtcblx0fVxuXG5cdC8vIFNpZ25pZmljYW50IGRpZ2l0IGZvcm1hdFxuXHRpZiAoICFpc05hTiggbWluaW11bVNpZ25pZmljYW50RGlnaXRzICogbWF4aW11bVNpZ25pZmljYW50RGlnaXRzICkgKSB7XG5cdFx0bnVtYmVyID0gbnVtYmVyRm9ybWF0U2lnbmlmaWNhbnREaWdpdHMoIG51bWJlciwgbWluaW11bVNpZ25pZmljYW50RGlnaXRzLFxuXHRcdFx0bWF4aW11bVNpZ25pZmljYW50RGlnaXRzLCByb3VuZCApO1xuXG5cdC8vIEludGVnZXIgYW5kIGZyYWN0aW9uYWwgZm9ybWF0XG5cdH0gZWxzZSB7XG5cdFx0bnVtYmVyID0gbnVtYmVyRm9ybWF0SW50ZWdlckZyYWN0aW9uRGlnaXRzKCBudW1iZXIsIG1pbmltdW1JbnRlZ2VyRGlnaXRzLFxuXHRcdFx0bWluaW11bUZyYWN0aW9uRGlnaXRzLCBtYXhpbXVtRnJhY3Rpb25EaWdpdHMsIHJvdW5kLCByb3VuZEluY3JlbWVudCApO1xuXHR9XG5cblx0Ly8gUmVtb3ZlIHRoZSBwb3NzaWJsZSBudW1iZXIgbWludXMgc2lnblxuXHRudW1iZXIgPSBudW1iZXIucmVwbGFjZSggL14tLywgXCJcIiApO1xuXG5cdC8vIEdyb3VwaW5nIHNlcGFyYXRvcnNcblx0aWYgKCBwcmltYXJ5R3JvdXBpbmdTaXplICkge1xuXHRcdG51bWJlciA9IG51bWJlckZvcm1hdEdyb3VwaW5nU2VwYXJhdG9yKCBudW1iZXIsIHByaW1hcnlHcm91cGluZ1NpemUsXG5cdFx0XHRzZWNvbmRhcnlHcm91cGluZ1NpemUgKTtcblx0fVxuXG5cdHJldCArPSBudW1iZXI7XG5cblx0Ly8gU2NpZW50aWZpYyBub3RhdGlvblxuXHQvLyBUT0RPIGltcGxlbWVudCBoZXJlXG5cblx0Ly8gUGFkZGluZy8nKFteJ118JycpKyd8Jyd8Wy4sXFwtK0UlXFx1MjAzMF0vZ1xuXHQvLyBUT0RPIGltcGxlbWVudCBoZXJlXG5cblx0cmV0ICs9IHN1ZmZpeDtcblxuXHRyZXR1cm4gcmV0LnJlcGxhY2UoIC8oJyhbXiddfCcnKSsnfCcnKXwuL2csIGZ1bmN0aW9uKCBjaGFyYWN0ZXIsIGxpdGVyYWwgKSB7XG5cblx0XHQvLyBMaXRlcmFsc1xuXHRcdGlmICggbGl0ZXJhbCApIHtcblx0XHRcdGxpdGVyYWwgPSBsaXRlcmFsLnJlcGxhY2UoIC8nJy8sIFwiJ1wiICk7XG5cdFx0XHRpZiAoIGxpdGVyYWwubGVuZ3RoID4gMiApIHtcblx0XHRcdFx0bGl0ZXJhbCA9IGxpdGVyYWwuc2xpY2UoIDEsIC0xICk7XG5cdFx0XHR9XG5cdFx0XHRyZXR1cm4gbGl0ZXJhbDtcblx0XHR9XG5cblx0XHQvLyBTeW1ib2xzXG5cdFx0Y2hhcmFjdGVyID0gY2hhcmFjdGVyLnJlcGxhY2UoIC9bLixcXC0rRSVcXHUyMDMwXS8sIGZ1bmN0aW9uKCBzeW1ib2wgKSB7XG5cdFx0XHRyZXR1cm4gc3ltYm9sTWFwWyBzeW1ib2wgXTtcblx0XHR9KTtcblxuXHRcdC8vIE51bWJlcmluZyBzeXN0ZW1cblx0XHRpZiAoIG51RGlnaXRzTWFwICkge1xuXHRcdFx0Y2hhcmFjdGVyID0gY2hhcmFjdGVyLnJlcGxhY2UoIC9bMC05XS8sIGZ1bmN0aW9uKCBkaWdpdCApIHtcblx0XHRcdFx0cmV0dXJuIG51RGlnaXRzTWFwWyArZGlnaXQgXTtcblx0XHRcdH0pO1xuXHRcdH1cblxuXHRcdHJldHVybiBjaGFyYWN0ZXI7XG5cdH0pO1xufTtcblxuXG5cblxuLyoqXG4gKiBOdW1iZXJpbmdTeXN0ZW0oIGNsZHIgKVxuICpcbiAqIC0gaHR0cDovL3d3dy51bmljb2RlLm9yZy9yZXBvcnRzL3RyMzUvdHIzNS1udW1iZXJzLmh0bWwjb3RoZXJOdW1iZXJpbmdTeXN0ZW1zXG4gKiAtIGh0dHA6Ly9jbGRyLnVuaWNvZGUub3JnL2luZGV4L2JjcDQ3LWV4dGVuc2lvblxuICogLSBodHRwOi8vd3d3LnVuaWNvZGUub3JnL3JlcG9ydHMvdHIzNS8jdV9FeHRlbnNpb25cbiAqL1xudmFyIG51bWJlck51bWJlcmluZ1N5c3RlbSA9IGZ1bmN0aW9uKCBjbGRyICkge1xuXHR2YXIgbnUgPSBjbGRyLmF0dHJpYnV0ZXNbIFwidS1udVwiIF07XG5cblx0aWYgKCBudSApIHtcblx0XHRpZiAoIG51ID09PSBcInRyYWRpdGlvXCIgKSB7XG5cdFx0XHRudSA9IFwidHJhZGl0aW9uYWxcIjtcblx0XHR9XG5cdFx0aWYgKCBbIFwibmF0aXZlXCIsIFwidHJhZGl0aW9uYWxcIiwgXCJmaW5hbmNlXCIgXS5pbmRleE9mKCBudSApICE9PSAtMSApIHtcblxuXHRcdFx0Ly8gVW5pY29kZSBsb2NhbGUgZXh0ZW5zaW9uIGB1LW51YCBpcyBzZXQgdXNpbmcgZWl0aGVyIChuYXRpdmUsIHRyYWRpdGlvbmFsIG9yXG5cdFx0XHQvLyBmaW5hbmNlKS4gU28sIGxvb2t1cCB0aGUgcmVzcGVjdGl2ZSBsb2NhbGUncyBudW1iZXJpbmdTeXN0ZW0gYW5kIHJldHVybiBpdC5cblx0XHRcdHJldHVybiBjbGRyLm1haW4oWyBcIm51bWJlcnMvb3RoZXJOdW1iZXJpbmdTeXN0ZW1zXCIsIG51IF0pO1xuXHRcdH1cblxuXHRcdC8vIFVuaWNvZGUgbG9jYWxlIGV4dGVuc2lvbiBgdS1udWAgaXMgc2V0IHdpdGggYW4gZXhwbGljaXQgbnVtYmVyaW5nU3lzdGVtLiBSZXR1cm4gaXQuXG5cdFx0cmV0dXJuIG51O1xuXHR9XG5cblx0Ly8gUmV0dXJuIHRoZSBkZWZhdWx0IG51bWJlcmluZ1N5c3RlbS5cblx0cmV0dXJuIGNsZHIubWFpbiggXCJudW1iZXJzL2RlZmF1bHROdW1iZXJpbmdTeXN0ZW1cIiApO1xufTtcblxuXG5cblxuLyoqXG4gKiBudU1hcCggY2xkciApXG4gKlxuICogQGNsZHIgW0NsZHIgaW5zdGFuY2VdLlxuICpcbiAqIFJldHVybiBkaWdpdHMgbWFwIGlmIG51bWJlcmluZyBzeXN0ZW0gaXMgZGlmZmVyZW50IHRoYW4gYGxhdG5gLlxuICovXG52YXIgbnVtYmVyTnVtYmVyaW5nU3lzdGVtRGlnaXRzTWFwID0gZnVuY3Rpb24oIGNsZHIgKSB7XG5cdHZhciBhdXgsXG5cdFx0bnUgPSBudW1iZXJOdW1iZXJpbmdTeXN0ZW0oIGNsZHIgKTtcblxuXHRpZiAoIG51ID09PSBcImxhdG5cIiApIHtcblx0XHRyZXR1cm47XG5cdH1cblxuXHRhdXggPSBjbGRyLnN1cHBsZW1lbnRhbChbIFwibnVtYmVyaW5nU3lzdGVtc1wiLCBudSBdKTtcblxuXHRpZiAoIGF1eC5fdHlwZSAhPT0gXCJudW1lcmljXCIgKSB7XG5cdFx0dGhyb3cgY3JlYXRlRXJyb3JVbnN1cHBvcnRlZEZlYXR1cmUoIFwiYFwiICsgYXV4Ll90eXBlICsgXCJgIG51bWJlcmluZyBzeXN0ZW1cIiApO1xuXHR9XG5cblx0cmV0dXJuIGF1eC5fZGlnaXRzO1xufTtcblxuXG5cblxuLyoqXG4gKiBFQk5GIHJlcHJlc2VudGF0aW9uOlxuICpcbiAqIG51bWJlcl9wYXR0ZXJuX3JlID0gICAgICAgIHByZWZpeD9cbiAqICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc/XG4gKiAgICAgICAgICAgICAgICAgICAgICAgICAgICAoaW50ZWdlcl9mcmFjdGlvbl9wYXR0ZXJuIHwgc2lnbmlmaWNhbnRfcGF0dGVybilcbiAqICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjaWVudGlmaWNfbm90YXRpb24/XG4gKiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzdWZmaXg/XG4gKlxuICogcHJlZml4ID0gICAgICAgICAgICAgICAgICAgbm9uX251bWJlcl9zdHVmZlxuICpcbiAqIHBhZGRpbmcgPSAgICAgICAgICAgICAgICAgIFwiKlwiIHJlZ2V4cCguKVxuICpcbiAqIGludGVnZXJfZnJhY3Rpb25fcGF0dGVybiA9IGludGVnZXJfcGF0dGVyblxuICogICAgICAgICAgICAgICAgICAgICAgICAgICAgZnJhY3Rpb25fcGF0dGVybj9cbiAqXG4gKiBpbnRlZ2VyX3BhdHRlcm4gPSAgICAgICAgICByZWdleHAoWyMsXSpbMCxdKjArKVxuICpcbiAqIGZyYWN0aW9uX3BhdHRlcm4gPSAgICAgICAgIFwiLlwiIHJlZ2V4cCgwKlswLTldKiMqKVxuICpcbiAqIHNpZ25pZmljYW50X3BhdHRlcm4gPSAgICAgIHJlZ2V4cChbIyxdKkArIyopXG4gKlxuICogc2NpZW50aWZpY19ub3RhdGlvbiA9ICAgICAgcmVnZXhwKEVcXCs/MCspXG4gKlxuICogc3VmZml4ID0gICAgICAgICAgICAgICAgICAgbm9uX251bWJlcl9zdHVmZlxuICpcbiAqIG5vbl9udW1iZXJfc3R1ZmYgPSAgICAgICAgIHJlZ2V4cCgoJ1teJ10rJ3wnJ3xbXiojQDAsLkVdKSopXG4gKlxuICpcbiAqIFJlZ2V4cCBncm91cHM6XG4gKlxuICogIDA6IG51bWJlcl9wYXR0ZXJuX3JlXG4gKiAgMTogcHJlZml4XG4gKiAgMjogLVxuICogIDM6IHBhZGRpbmdcbiAqICA0OiAoaW50ZWdlcl9mcmFjdGlvbl9wYXR0ZXJuIHwgc2lnbmlmaWNhbnRfcGF0dGVybilcbiAqICA1OiBpbnRlZ2VyX2ZyYWN0aW9uX3BhdHRlcm5cbiAqICA2OiBpbnRlZ2VyX3BhdHRlcm5cbiAqICA3OiBmcmFjdGlvbl9wYXR0ZXJuXG4gKiAgODogc2lnbmlmaWNhbnRfcGF0dGVyblxuICogIDk6IHNjaWVudGlmaWNfbm90YXRpb25cbiAqIDEwOiBzdWZmaXhcbiAqIDExOiAtXG4gKi9cbnZhciBudW1iZXJQYXR0ZXJuUmUgPSAoL14oKCdbXiddKyd8Jyd8W14qI0AwLC5FXSkqKShcXCouKT8oKChbIyxdKlswLF0qMCspKFxcLjAqWzAtOV0qIyopPyl8KFsjLF0qQCsjKikpKEVcXCs/MCspPygoJ1teJ10rJ3wnJ3xbXiojQDAsLkVdKSopJC8pO1xuXG5cblxuXG4vKipcbiAqIGZvcm1hdCggbnVtYmVyLCBwYXR0ZXJuIClcbiAqXG4gKiBAbnVtYmVyIFtOdW1iZXJdLlxuICpcbiAqIEBwYXR0ZXJuIFtTdHJpbmddIHJhdyBwYXR0ZXJuIGZvciBudW1iZXJzLlxuICpcbiAqIFJldHVybiB0aGUgZm9ybWF0dGVkIG51bWJlci5cbiAqIHJlZjogaHR0cDovL3d3dy51bmljb2RlLm9yZy9yZXBvcnRzL3RyMzUvdHIzNS1udW1iZXJzLmh0bWxcbiAqL1xudmFyIG51bWJlclBhdHRlcm5Qcm9wZXJ0aWVzID0gZnVuY3Rpb24oIHBhdHRlcm4gKSB7XG5cdHZhciBhdXgxLCBhdXgyLCBmcmFjdGlvblBhdHRlcm4sIGludGVnZXJGcmFjdGlvbk9yU2lnbmlmaWNhbnRQYXR0ZXJuLCBpbnRlZ2VyUGF0dGVybixcblx0XHRtYXhpbXVtRnJhY3Rpb25EaWdpdHMsIG1heGltdW1TaWduaWZpY2FudERpZ2l0cywgbWluaW11bUZyYWN0aW9uRGlnaXRzLFxuXHRcdG1pbmltdW1JbnRlZ2VyRGlnaXRzLCBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMsIHBhZGRpbmcsIHByZWZpeCwgcHJpbWFyeUdyb3VwaW5nU2l6ZSxcblx0XHRyb3VuZEluY3JlbWVudCwgc2NpZW50aWZpY05vdGF0aW9uLCBzZWNvbmRhcnlHcm91cGluZ1NpemUsIHNpZ25pZmljYW50UGF0dGVybiwgc3VmZml4O1xuXG5cdHBhdHRlcm4gPSBwYXR0ZXJuLm1hdGNoKCBudW1iZXJQYXR0ZXJuUmUgKTtcblx0aWYgKCAhcGF0dGVybiApIHtcblx0XHR0aHJvdyBuZXcgRXJyb3IoIFwiSW52YWxpZCBwYXR0ZXJuOiBcIiArIHBhdHRlcm4gKTtcblx0fVxuXG5cdHByZWZpeCA9IHBhdHRlcm5bIDEgXTtcblx0cGFkZGluZyA9IHBhdHRlcm5bIDMgXTtcblx0aW50ZWdlckZyYWN0aW9uT3JTaWduaWZpY2FudFBhdHRlcm4gPSBwYXR0ZXJuWyA0IF07XG5cdHNpZ25pZmljYW50UGF0dGVybiA9IHBhdHRlcm5bIDggXTtcblx0c2NpZW50aWZpY05vdGF0aW9uID0gcGF0dGVyblsgOSBdO1xuXHRzdWZmaXggPSBwYXR0ZXJuWyAxMCBdO1xuXG5cdC8vIFNpZ25pZmljYW50IGRpZ2l0IGZvcm1hdFxuXHRpZiAoIHNpZ25pZmljYW50UGF0dGVybiApIHtcblx0XHRzaWduaWZpY2FudFBhdHRlcm4ucmVwbGFjZSggLyhAKykoIyopLywgZnVuY3Rpb24oIG1hdGNoLCBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHNNYXRjaCwgbWF4aW11bVNpZ25pZmljYW50RGlnaXRzTWF0Y2ggKSB7XG5cdFx0XHRtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMgPSBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHNNYXRjaC5sZW5ndGg7XG5cdFx0XHRtYXhpbXVtU2lnbmlmaWNhbnREaWdpdHMgPSBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMgK1xuXHRcdFx0XHRtYXhpbXVtU2lnbmlmaWNhbnREaWdpdHNNYXRjaC5sZW5ndGg7XG5cdFx0fSk7XG5cblx0Ly8gSW50ZWdlciBhbmQgZnJhY3Rpb25hbCBmb3JtYXRcblx0fSBlbHNlIHtcblx0XHRmcmFjdGlvblBhdHRlcm4gPSBwYXR0ZXJuWyA3IF07XG5cdFx0aW50ZWdlclBhdHRlcm4gPSBwYXR0ZXJuWyA2IF07XG5cblx0XHRpZiAoIGZyYWN0aW9uUGF0dGVybiApIHtcblxuXHRcdFx0Ly8gTWluaW11bSBmcmFjdGlvbiBkaWdpdHMsIGFuZCByb3VuZGluZy5cblx0XHRcdGZyYWN0aW9uUGF0dGVybi5yZXBsYWNlKCAvWzAtOV0rLywgZnVuY3Rpb24oIG1hdGNoICkge1xuXHRcdFx0XHRtaW5pbXVtRnJhY3Rpb25EaWdpdHMgPSBtYXRjaDtcblx0XHRcdH0pO1xuXHRcdFx0aWYgKCBtaW5pbXVtRnJhY3Rpb25EaWdpdHMgKSB7XG5cdFx0XHRcdHJvdW5kSW5jcmVtZW50ID0gKyggXCIwLlwiICsgbWluaW11bUZyYWN0aW9uRGlnaXRzICk7XG5cdFx0XHRcdG1pbmltdW1GcmFjdGlvbkRpZ2l0cyA9IG1pbmltdW1GcmFjdGlvbkRpZ2l0cy5sZW5ndGg7XG5cdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHRtaW5pbXVtRnJhY3Rpb25EaWdpdHMgPSAwO1xuXHRcdFx0fVxuXG5cdFx0XHQvLyBNYXhpbXVtIGZyYWN0aW9uIGRpZ2l0c1xuXHRcdFx0Ly8gMTogaWdub3JlIGRlY2ltYWwgY2hhcmFjdGVyXG5cdFx0XHRtYXhpbXVtRnJhY3Rpb25EaWdpdHMgPSBmcmFjdGlvblBhdHRlcm4ubGVuZ3RoIC0gMSAvKiAxICovO1xuXHRcdH1cblxuXHRcdC8vIE1pbmltdW0gaW50ZWdlciBkaWdpdHNcblx0XHRpbnRlZ2VyUGF0dGVybi5yZXBsYWNlKCAvMCskLywgZnVuY3Rpb24oIG1hdGNoICkge1xuXHRcdFx0bWluaW11bUludGVnZXJEaWdpdHMgPSBtYXRjaC5sZW5ndGg7XG5cdFx0fSk7XG5cdH1cblxuXHQvLyBTY2llbnRpZmljIG5vdGF0aW9uXG5cdGlmICggc2NpZW50aWZpY05vdGF0aW9uICkge1xuXHRcdHRocm93IGNyZWF0ZUVycm9yVW5zdXBwb3J0ZWRGZWF0dXJlKHtcblx0XHRcdGZlYXR1cmU6IFwic2NpZW50aWZpYyBub3RhdGlvbiAobm90IGltcGxlbWVudGVkKVwiXG5cdFx0fSk7XG5cdH1cblxuXHQvLyBQYWRkaW5nXG5cdGlmICggcGFkZGluZyApIHtcblx0XHR0aHJvdyBjcmVhdGVFcnJvclVuc3VwcG9ydGVkRmVhdHVyZSh7XG5cdFx0XHRmZWF0dXJlOiBcInBhZGRpbmcgKG5vdCBpbXBsZW1lbnRlZClcIlxuXHRcdH0pO1xuXHR9XG5cblx0Ly8gR3JvdXBpbmdcblx0aWYgKCAoIGF1eDEgPSBpbnRlZ2VyRnJhY3Rpb25PclNpZ25pZmljYW50UGF0dGVybi5sYXN0SW5kZXhPZiggXCIsXCIgKSApICE9PSAtMSApIHtcblxuXHRcdC8vIFByaW1hcnkgZ3JvdXBpbmcgc2l6ZSBpcyB0aGUgaW50ZXJ2YWwgYmV0d2VlbiB0aGUgbGFzdCBncm91cCBzZXBhcmF0b3IgYW5kIHRoZSBlbmQgb2Zcblx0XHQvLyB0aGUgaW50ZWdlciAob3IgdGhlIGVuZCBvZiB0aGUgc2lnbmlmaWNhbnQgcGF0dGVybikuXG5cdFx0YXV4MiA9IGludGVnZXJGcmFjdGlvbk9yU2lnbmlmaWNhbnRQYXR0ZXJuLnNwbGl0KCBcIi5cIiApWyAwIF07XG5cdFx0cHJpbWFyeUdyb3VwaW5nU2l6ZSA9IGF1eDIubGVuZ3RoIC0gYXV4MSAtIDE7XG5cblx0XHQvLyBTZWNvbmRhcnkgZ3JvdXBpbmcgc2l6ZSBpcyB0aGUgaW50ZXJ2YWwgYmV0d2VlbiB0aGUgbGFzdCB0d28gZ3JvdXAgc2VwYXJhdG9ycy5cblx0XHRpZiAoICggYXV4MiA9IGludGVnZXJGcmFjdGlvbk9yU2lnbmlmaWNhbnRQYXR0ZXJuLmxhc3RJbmRleE9mKCBcIixcIiwgYXV4MSAtIDEgKSApICE9PSAtMSApIHtcblx0XHRcdHNlY29uZGFyeUdyb3VwaW5nU2l6ZSA9IGF1eDEgLSAxIC0gYXV4Mjtcblx0XHR9XG5cdH1cblxuXHQvLyBSZXR1cm46XG5cdC8vICAwOiBAcHJlZml4IFN0cmluZ1xuXHQvLyAgMTogQHBhZGRpbmcgQXJyYXkgWyA8Y2hhcmFjdGVyPiwgPGNvdW50PiBdIFRPRE9cblx0Ly8gIDI6IEBtaW5pbXVtSW50ZWdlckRpZ2l0cyBub24tbmVnYXRpdmUgaW50ZWdlciBOdW1iZXIgdmFsdWUgaW5kaWNhdGluZyB0aGUgbWluaW11bSBpbnRlZ2VyXG5cdC8vICAgICAgICBkaWdpdHMgdG8gYmUgdXNlZC4gTnVtYmVycyB3aWxsIGJlIHBhZGRlZCB3aXRoIGxlYWRpbmcgemVyb2VzIGlmIG5lY2Vzc2FyeS5cblx0Ly8gIDM6IEBtaW5pbXVtRnJhY3Rpb25EaWdpdHMgYW5kXG5cdC8vICA0OiBAbWF4aW11bUZyYWN0aW9uRGlnaXRzIGFyZSBub24tbmVnYXRpdmUgaW50ZWdlciBOdW1iZXIgdmFsdWVzIGluZGljYXRpbmcgdGhlIG1pbmltdW0gYW5kXG5cdC8vICAgICAgICBtYXhpbXVtIGZyYWN0aW9uIGRpZ2l0cyB0byBiZSB1c2VkLiBOdW1iZXJzIHdpbGwgYmUgcm91bmRlZCBvciBwYWRkZWQgd2l0aCB0cmFpbGluZ1xuXHQvLyAgICAgICAgemVyb2VzIGlmIG5lY2Vzc2FyeS5cblx0Ly8gIDU6IEBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMgYW5kXG5cdC8vICA2OiBAbWF4aW11bVNpZ25pZmljYW50RGlnaXRzIGFyZSBwb3NpdGl2ZSBpbnRlZ2VyIE51bWJlciB2YWx1ZXMgaW5kaWNhdGluZyB0aGUgbWluaW11bSBhbmRcblx0Ly8gICAgICAgIG1heGltdW0gZnJhY3Rpb24gZGlnaXRzIHRvIGJlIHNob3duLiBFaXRoZXIgbm9uZSBvciBib3RoIG9mIHRoZXNlIHByb3BlcnRpZXMgYXJlXG5cdC8vICAgICAgICBwcmVzZW50OyBpZiB0aGV5IGFyZSwgdGhleSBvdmVycmlkZSBtaW5pbXVtIGFuZCBtYXhpbXVtIGludGVnZXIgYW5kIGZyYWN0aW9uIGRpZ2l0c1xuXHQvLyAgICAgICAg4oCTIHRoZSBmb3JtYXR0ZXIgdXNlcyBob3dldmVyIG1hbnkgaW50ZWdlciBhbmQgZnJhY3Rpb24gZGlnaXRzIGFyZSByZXF1aXJlZCB0byBkaXNwbGF5XG5cdC8vICAgICAgICB0aGUgc3BlY2lmaWVkIG51bWJlciBvZiBzaWduaWZpY2FudCBkaWdpdHMuXG5cdC8vICA3OiBAcm91bmRJbmNyZW1lbnQgRGVjaW1hbCByb3VuZCBpbmNyZW1lbnQgb3IgbnVsbFxuXHQvLyAgODogQHByaW1hcnlHcm91cGluZ1NpemVcblx0Ly8gIDk6IEBzZWNvbmRhcnlHcm91cGluZ1NpemVcblx0Ly8gMTA6IEBzdWZmaXggU3RyaW5nXG5cdHJldHVybiBbXG5cdFx0cHJlZml4LFxuXHRcdHBhZGRpbmcsXG5cdFx0bWluaW11bUludGVnZXJEaWdpdHMsXG5cdFx0bWluaW11bUZyYWN0aW9uRGlnaXRzLFxuXHRcdG1heGltdW1GcmFjdGlvbkRpZ2l0cyxcblx0XHRtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMsXG5cdFx0bWF4aW11bVNpZ25pZmljYW50RGlnaXRzLFxuXHRcdHJvdW5kSW5jcmVtZW50LFxuXHRcdHByaW1hcnlHcm91cGluZ1NpemUsXG5cdFx0c2Vjb25kYXJ5R3JvdXBpbmdTaXplLFxuXHRcdHN1ZmZpeFxuXHRdO1xufTtcblxuXG5cblxuLyoqXG4gKiBTeW1ib2woIG5hbWUsIGNsZHIgKVxuICpcbiAqIEBuYW1lIFtTdHJpbmddIFN5bWJvbCBuYW1lLlxuICpcbiAqIEBjbGRyIFtDbGRyIGluc3RhbmNlXS5cbiAqXG4gKiBSZXR1cm4gdGhlIGxvY2FsaXplZCBzeW1ib2wgZ2l2ZW4gaXRzIG5hbWUuXG4gKi9cbnZhciBudW1iZXJTeW1ib2wgPSBmdW5jdGlvbiggbmFtZSwgY2xkciApIHtcblx0cmV0dXJuIGNsZHIubWFpbihbXG5cdFx0XCJudW1iZXJzL3N5bWJvbHMtbnVtYmVyU3lzdGVtLVwiICsgbnVtYmVyTnVtYmVyaW5nU3lzdGVtKCBjbGRyICksXG5cdFx0bmFtZVxuXHRdKTtcbn07XG5cblxuXG5cbnZhciBudW1iZXJTeW1ib2xOYW1lID0ge1xuXHRcIi5cIjogXCJkZWNpbWFsXCIsXG5cdFwiLFwiOiBcImdyb3VwXCIsXG5cdFwiJVwiOiBcInBlcmNlbnRTaWduXCIsXG5cdFwiK1wiOiBcInBsdXNTaWduXCIsXG5cdFwiLVwiOiBcIm1pbnVzU2lnblwiLFxuXHRcIkVcIjogXCJleHBvbmVudGlhbFwiLFxuXHRcIlxcdTIwMzBcIjogXCJwZXJNaWxsZVwiXG59O1xuXG5cblxuXG4vKipcbiAqIHN5bWJvbE1hcCggY2xkciApXG4gKlxuICogQGNsZHIgW0NsZHIgaW5zdGFuY2VdLlxuICpcbiAqIFJldHVybiB0aGUgKGxvY2FsaXplZCBzeW1ib2wsIHBhdHRlcm4gc3ltYm9sKSBrZXkgdmFsdWUgcGFpciwgZWcuIHtcbiAqICAgXCIuXCI6IFwi2atcIixcbiAqICAgXCIsXCI6IFwi2axcIixcbiAqICAgXCIlXCI6IFwi2apcIixcbiAqICAgLi4uXG4gKiB9O1xuICovXG52YXIgbnVtYmVyU3ltYm9sTWFwID0gZnVuY3Rpb24oIGNsZHIgKSB7XG5cdHZhciBzeW1ib2wsXG5cdFx0c3ltYm9sTWFwID0ge307XG5cblx0Zm9yICggc3ltYm9sIGluIG51bWJlclN5bWJvbE5hbWUgKSB7XG5cdFx0c3ltYm9sTWFwWyBzeW1ib2wgXSA9IG51bWJlclN5bWJvbCggbnVtYmVyU3ltYm9sTmFtZVsgc3ltYm9sIF0sIGNsZHIgKTtcblx0fVxuXG5cdHJldHVybiBzeW1ib2xNYXA7XG59O1xuXG5cblxuXG52YXIgbnVtYmVyVHJ1bmNhdGUgPSBmdW5jdGlvbiggdmFsdWUgKSB7XG5cdGlmICggaXNOYU4oIHZhbHVlICkgKSB7XG5cdFx0cmV0dXJuIE5hTjtcblx0fVxuXHRyZXR1cm4gTWF0aFsgdmFsdWUgPCAwID8gXCJjZWlsXCIgOiBcImZsb29yXCIgXSggdmFsdWUgKTtcbn07XG5cblxuXG5cbi8qKlxuICogcm91bmQoIG1ldGhvZCApXG4gKlxuICogQG1ldGhvZCBbU3RyaW5nXSB3aXRoIGVpdGhlciBcInJvdW5kXCIsIFwiY2VpbFwiLCBcImZsb29yXCIsIG9yIFwidHJ1bmNhdGVcIi5cbiAqXG4gKiBSZXR1cm4gZnVuY3Rpb24oIHZhbHVlLCBpbmNyZW1lbnRPckV4cCApOlxuICpcbiAqICAgQHZhbHVlIFtOdW1iZXJdIGVnLiAxMjMuNDUuXG4gKlxuICogICBAaW5jcmVtZW50T3JFeHAgW051bWJlcl0gb3B0aW9uYWwsIGVnLiAwLjE7IG9yXG4gKiAgICAgW09iamVjdF0gRWl0aGVyIHsgaW5jcmVtZW50OiA8dmFsdWU+IH0gb3IgeyBleHBvbmVudDogPHZhbHVlPiB9XG4gKlxuICogICBSZXR1cm4gdGhlIHJvdW5kZWQgbnVtYmVyLCBlZzpcbiAqICAgLSByb3VuZCggXCJyb3VuZFwiICkoIDEyMy40NSApOiAxMjM7XG4gKiAgIC0gcm91bmQoIFwiY2VpbFwiICkoIDEyMy40NSApOiAxMjQ7XG4gKiAgIC0gcm91bmQoIFwiZmxvb3JcIiApKCAxMjMuNDUgKTogMTIzO1xuICogICAtIHJvdW5kKCBcInRydW5jYXRlXCIgKSggMTIzLjQ1ICk6IDEyMztcbiAqICAgLSByb3VuZCggXCJyb3VuZFwiICkoIDEyMy40NSwgMC4xICk6IDEyMy41O1xuICogICAtIHJvdW5kKCBcInJvdW5kXCIgKSggMTIzLjQ1LCAxMCApOiAxMjA7XG4gKlxuICogICBCYXNlZCBvbiBodHRwczovL2RldmVsb3Blci5tb3ppbGxhLm9yZy9lbi1VUy9kb2NzL1dlYi9KYXZhU2NyaXB0L1JlZmVyZW5jZS9HbG9iYWxfT2JqZWN0cy9NYXRoL3JvdW5kXG4gKiAgIFJlZjogIzM3NlxuICovXG52YXIgbnVtYmVyUm91bmQgPSBmdW5jdGlvbiggbWV0aG9kICkge1xuXHRtZXRob2QgPSBtZXRob2QgfHwgXCJyb3VuZFwiO1xuXHRtZXRob2QgPSBtZXRob2QgPT09IFwidHJ1bmNhdGVcIiA/IG51bWJlclRydW5jYXRlIDogTWF0aFsgbWV0aG9kIF07XG5cblx0cmV0dXJuIGZ1bmN0aW9uKCB2YWx1ZSwgaW5jcmVtZW50T3JFeHAgKSB7XG5cdFx0dmFyIGV4cCwgaW5jcmVtZW50O1xuXG5cdFx0dmFsdWUgPSArdmFsdWU7XG5cblx0XHQvLyBJZiB0aGUgdmFsdWUgaXMgbm90IGEgbnVtYmVyLCByZXR1cm4gTmFOLlxuXHRcdGlmICggaXNOYU4oIHZhbHVlICkgKSB7XG5cdFx0XHRyZXR1cm4gTmFOO1xuXHRcdH1cblxuXHRcdC8vIEV4cG9uZW50IGdpdmVuLlxuXHRcdGlmICggdHlwZW9mIGluY3JlbWVudE9yRXhwID09PSBcIm9iamVjdFwiICYmIGluY3JlbWVudE9yRXhwLmV4cG9uZW50ICkge1xuXHRcdFx0ZXhwID0gK2luY3JlbWVudE9yRXhwLmV4cG9uZW50O1xuXHRcdFx0aW5jcmVtZW50ID0gMTtcblxuXHRcdFx0aWYgKCBleHAgPT09IDAgKSB7XG5cdFx0XHRcdHJldHVybiBtZXRob2QoIHZhbHVlICk7XG5cdFx0XHR9XG5cblx0XHRcdC8vIElmIHRoZSBleHAgaXMgbm90IGFuIGludGVnZXIsIHJldHVybiBOYU4uXG5cdFx0XHRpZiAoICEoIHR5cGVvZiBleHAgPT09IFwibnVtYmVyXCIgJiYgZXhwICUgMSA9PT0gMCApICkge1xuXHRcdFx0XHRyZXR1cm4gTmFOO1xuXHRcdFx0fVxuXG5cdFx0Ly8gSW5jcmVtZW50IGdpdmVuLlxuXHRcdH0gZWxzZSB7XG5cdFx0XHRpbmNyZW1lbnQgPSAraW5jcmVtZW50T3JFeHAgfHwgMTtcblxuXHRcdFx0aWYgKCBpbmNyZW1lbnQgPT09IDEgKSB7XG5cdFx0XHRcdHJldHVybiBtZXRob2QoIHZhbHVlICk7XG5cdFx0XHR9XG5cblx0XHRcdC8vIElmIHRoZSBpbmNyZW1lbnQgaXMgbm90IGEgbnVtYmVyLCByZXR1cm4gTmFOLlxuXHRcdFx0aWYgKCBpc05hTiggaW5jcmVtZW50ICkgKSB7XG5cdFx0XHRcdHJldHVybiBOYU47XG5cdFx0XHR9XG5cblx0XHRcdGluY3JlbWVudCA9IGluY3JlbWVudC50b0V4cG9uZW50aWFsKCkuc3BsaXQoIFwiZVwiICk7XG5cdFx0XHRleHAgPSAraW5jcmVtZW50WyAxIF07XG5cdFx0XHRpbmNyZW1lbnQgPSAraW5jcmVtZW50WyAwIF07XG5cdFx0fVxuXG5cdFx0Ly8gU2hpZnQgJiBSb3VuZFxuXHRcdHZhbHVlID0gdmFsdWUudG9TdHJpbmcoKS5zcGxpdCggXCJlXCIgKTtcblx0XHR2YWx1ZVsgMCBdID0gK3ZhbHVlWyAwIF0gLyBpbmNyZW1lbnQ7XG5cdFx0dmFsdWVbIDEgXSA9IHZhbHVlWyAxIF0gPyAoICt2YWx1ZVsgMSBdIC0gZXhwICkgOiAtZXhwO1xuXHRcdHZhbHVlID0gbWV0aG9kKCArKHZhbHVlWyAwIF0gKyBcImVcIiArIHZhbHVlWyAxIF0gKSApO1xuXG5cdFx0Ly8gU2hpZnQgYmFja1xuXHRcdHZhbHVlID0gdmFsdWUudG9TdHJpbmcoKS5zcGxpdCggXCJlXCIgKTtcblx0XHR2YWx1ZVsgMCBdID0gK3ZhbHVlWyAwIF0gKiBpbmNyZW1lbnQ7XG5cdFx0dmFsdWVbIDEgXSA9IHZhbHVlWyAxIF0gPyAoICt2YWx1ZVsgMSBdICsgZXhwICkgOiBleHA7XG5cdFx0cmV0dXJuICsoIHZhbHVlWyAwIF0gKyBcImVcIiArIHZhbHVlWyAxIF0gKTtcblx0fTtcbn07XG5cblxuXG5cbi8qKlxuICogZm9ybWF0UHJvcGVydGllcyggcGF0dGVybiwgY2xkciBbLCBvcHRpb25zXSApXG4gKlxuICogQHBhdHRlcm4gW1N0cmluZ10gcmF3IHBhdHRlcm4gZm9yIG51bWJlcnMuXG4gKlxuICogQGNsZHIgW0NsZHIgaW5zdGFuY2VdLlxuICpcbiAqIEBvcHRpb25zIFtPYmplY3RdOlxuICogLSBtaW5pbXVtSW50ZWdlckRpZ2l0cyBbTnVtYmVyXVxuICogLSBtaW5pbXVtRnJhY3Rpb25EaWdpdHMsIG1heGltdW1GcmFjdGlvbkRpZ2l0cyBbTnVtYmVyXVxuICogLSBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMsIG1heGltdW1TaWduaWZpY2FudERpZ2l0cyBbTnVtYmVyXVxuICogLSByb3VuZCBbU3RyaW5nXSBcImNlaWxcIiwgXCJmbG9vclwiLCBcInJvdW5kXCIgKGRlZmF1bHQpLCBvciBcInRydW5jYXRlXCIuXG4gKiAtIHVzZUdyb3VwaW5nIFtCb29sZWFuXSBkZWZhdWx0IHRydWUuXG4gKlxuICogUmV0dXJuIHRoZSBwcm9jZXNzZWQgcHJvcGVydGllcyB0aGF0IHdpbGwgYmUgdXNlZCBpbiBudW1iZXIvZm9ybWF0LlxuICogcmVmOiBodHRwOi8vd3d3LnVuaWNvZGUub3JnL3JlcG9ydHMvdHIzNS90cjM1LW51bWJlcnMuaHRtbFxuICovXG52YXIgbnVtYmVyRm9ybWF0UHJvcGVydGllcyA9IGZ1bmN0aW9uKCBwYXR0ZXJuLCBjbGRyLCBvcHRpb25zICkge1xuXHR2YXIgbmVnYXRpdmVQYXR0ZXJuLCBuZWdhdGl2ZVByZWZpeCwgbmVnYXRpdmVQcm9wZXJ0aWVzLCBuZWdhdGl2ZVN1ZmZpeCwgcG9zaXRpdmVQYXR0ZXJuLFxuXHRcdHByb3BlcnRpZXM7XG5cblx0ZnVuY3Rpb24gZ2V0T3B0aW9ucyggYXR0cmlidXRlLCBwcm9wZXJ0eUluZGV4ICkge1xuXHRcdGlmICggYXR0cmlidXRlIGluIG9wdGlvbnMgKSB7XG5cdFx0XHRwcm9wZXJ0aWVzWyBwcm9wZXJ0eUluZGV4IF0gPSBvcHRpb25zWyBhdHRyaWJ1dGUgXTtcblx0XHR9XG5cdH1cblxuXHRvcHRpb25zID0gb3B0aW9ucyB8fCB7fTtcblx0cGF0dGVybiA9IHBhdHRlcm4uc3BsaXQoIFwiO1wiICk7XG5cblx0cG9zaXRpdmVQYXR0ZXJuID0gcGF0dGVyblsgMCBdO1xuXG5cdG5lZ2F0aXZlUGF0dGVybiA9IHBhdHRlcm5bIDEgXSB8fCBcIi1cIiArIHBvc2l0aXZlUGF0dGVybjtcblx0bmVnYXRpdmVQcm9wZXJ0aWVzID0gbnVtYmVyUGF0dGVyblByb3BlcnRpZXMoIG5lZ2F0aXZlUGF0dGVybiApO1xuXHRuZWdhdGl2ZVByZWZpeCA9IG5lZ2F0aXZlUHJvcGVydGllc1sgMCBdO1xuXHRuZWdhdGl2ZVN1ZmZpeCA9IG5lZ2F0aXZlUHJvcGVydGllc1sgMTAgXTtcblxuXHRwcm9wZXJ0aWVzID0gbnVtYmVyUGF0dGVyblByb3BlcnRpZXMoIHBvc2l0aXZlUGF0dGVybiApLmNvbmNhdChbXG5cdFx0cG9zaXRpdmVQYXR0ZXJuLFxuXHRcdG5lZ2F0aXZlUHJlZml4ICsgcG9zaXRpdmVQYXR0ZXJuICsgbmVnYXRpdmVTdWZmaXgsXG5cdFx0bmVnYXRpdmVQcmVmaXgsXG5cdFx0bmVnYXRpdmVTdWZmaXgsXG5cdFx0bnVtYmVyUm91bmQoIG9wdGlvbnMucm91bmQgKSxcblx0XHRudW1iZXJTeW1ib2woIFwiaW5maW5pdHlcIiwgY2xkciApLFxuXHRcdG51bWJlclN5bWJvbCggXCJuYW5cIiwgY2xkciApLFxuXHRcdG51bWJlclN5bWJvbE1hcCggY2xkciApLFxuXHRcdG51bWJlck51bWJlcmluZ1N5c3RlbURpZ2l0c01hcCggY2xkciApXG5cdF0pO1xuXG5cdGdldE9wdGlvbnMoIFwibWluaW11bUludGVnZXJEaWdpdHNcIiwgMiApO1xuXHRnZXRPcHRpb25zKCBcIm1pbmltdW1GcmFjdGlvbkRpZ2l0c1wiLCAzICk7XG5cdGdldE9wdGlvbnMoIFwibWF4aW11bUZyYWN0aW9uRGlnaXRzXCIsIDQgKTtcblx0Z2V0T3B0aW9ucyggXCJtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHNcIiwgNSApO1xuXHRnZXRPcHRpb25zKCBcIm1heGltdW1TaWduaWZpY2FudERpZ2l0c1wiLCA2ICk7XG5cblx0Ly8gR3JvdXBpbmcgc2VwYXJhdG9yc1xuXHRpZiAoIG9wdGlvbnMudXNlR3JvdXBpbmcgPT09IGZhbHNlICkge1xuXHRcdHByb3BlcnRpZXNbIDggXSA9IG51bGw7XG5cdH1cblxuXHQvLyBOb3JtYWxpemUgbnVtYmVyIG9mIGRpZ2l0cyBpZiBvbmx5IG9uZSBvZiBlaXRoZXIgbWluaW11bUZyYWN0aW9uRGlnaXRzIG9yXG5cdC8vIG1heGltdW1GcmFjdGlvbkRpZ2l0cyBpcyBwYXNzZWQgaW4gYXMgYW4gb3B0aW9uXG5cdGlmICggXCJtaW5pbXVtRnJhY3Rpb25EaWdpdHNcIiBpbiBvcHRpb25zICYmICEoIFwibWF4aW11bUZyYWN0aW9uRGlnaXRzXCIgaW4gb3B0aW9ucyApICkge1xuXHRcdC8vIG1heGltdW1GcmFjdGlvbkRpZ2l0cyA9IE1hdGgubWF4KCBtaW5pbXVtRnJhY3Rpb25EaWdpdHMsIG1heGltdW1GcmFjdGlvbkRpZ2l0cyApO1xuXHRcdHByb3BlcnRpZXNbIDQgXSA9IE1hdGgubWF4KCBwcm9wZXJ0aWVzWyAzIF0sIHByb3BlcnRpZXNbIDQgXSApO1xuXHR9IGVsc2UgaWYgKCAhKCBcIm1pbmltdW1GcmFjdGlvbkRpZ2l0c1wiIGluIG9wdGlvbnMgKSAmJlxuXHRcdFx0XCJtYXhpbXVtRnJhY3Rpb25EaWdpdHNcIiBpbiBvcHRpb25zICkge1xuXHRcdC8vIG1pbmltdW1GcmFjdGlvbkRpZ2l0cyA9IE1hdGgubWluKCBtaW5pbXVtRnJhY3Rpb25EaWdpdHMsIG1heGltdW1GcmFjdGlvbkRpZ2l0cyApO1xuXHRcdHByb3BlcnRpZXNbIDMgXSA9IE1hdGgubWluKCBwcm9wZXJ0aWVzWyAzIF0sIHByb3BlcnRpZXNbIDQgXSApO1xuXHR9XG5cblx0Ly8gUmV0dXJuOlxuXHQvLyAwLTEwOiBzZWUgbnVtYmVyL3BhdHRlcm4tcHJvcGVydGllcy5cblx0Ly8gMTE6IEBwb3NpdGl2ZVBhdHRlcm4gW1N0cmluZ10gUG9zaXRpdmUgcGF0dGVybi5cblx0Ly8gMTI6IEBuZWdhdGl2ZVBhdHRlcm4gW1N0cmluZ10gTmVnYXRpdmUgcGF0dGVybi5cblx0Ly8gMTM6IEBuZWdhdGl2ZVByZWZpeCBbU3RyaW5nXSBOZWdhdGl2ZSBwcmVmaXguXG5cdC8vIDE0OiBAbmVnYXRpdmVTdWZmaXggW1N0cmluZ10gTmVnYXRpdmUgc3VmZml4LlxuXHQvLyAxNTogQHJvdW5kIFtGdW5jdGlvbl0gUm91bmQgZnVuY3Rpb24uXG5cdC8vIDE2OiBAaW5maW5pdHlTeW1ib2wgW1N0cmluZ10gSW5maW5pdHkgc3ltYm9sLlxuXHQvLyAxNzogQG5hblN5bWJvbCBbU3RyaW5nXSBOYU4gc3ltYm9sLlxuXHQvLyAxODogQHN5bWJvbE1hcCBbT2JqZWN0XSBBIGJ1bmNoIG9mIG90aGVyIHN5bWJvbHMuXG5cdC8vIDE5OiBAbnVEaWdpdHNNYXAgW0FycmF5XSBEaWdpdHMgbWFwIGlmIG51bWJlcmluZyBzeXN0ZW0gaXMgZGlmZmVyZW50IHRoYW4gYGxhdG5gLlxuXHRyZXR1cm4gcHJvcGVydGllcztcbn07XG5cblxuXG5cbi8qKlxuICogRUJORiByZXByZXNlbnRhdGlvbjpcbiAqXG4gKiBudW1iZXJfcGF0dGVybl9yZSA9ICAgICAgICBwcmVmaXhfaW5jbHVkaW5nX3BhZGRpbmc/XG4gKiAgICAgICAgICAgICAgICAgICAgICAgICAgICBudW1iZXJcbiAqICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNjaWVudGlmaWNfbm90YXRpb24/XG4gKiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzdWZmaXg/XG4gKlxuICogbnVtYmVyID0gICAgICAgICAgICAgICAgICAgaW50ZWdlcl9pbmNsdWRpbmdfZ3JvdXBfc2VwYXJhdG9yIGZyYWN0aW9uX2luY2x1ZGluZ19kZWNpbWFsX3NlcGFyYXRvclxuICpcbiAqIGludGVnZXJfaW5jbHVkaW5nX2dyb3VwX3NlcGFyYXRvciA9XG4gKiAgICAgICAgICAgICAgICAgICAgICAgICAgICByZWdleHAoWzAtOSxdKlswLTldKylcbiAqXG4gKiBmcmFjdGlvbl9pbmNsdWRpbmdfZGVjaW1hbF9zZXBhcmF0b3IgPVxuICogICAgICAgICAgICAgICAgICAgICAgICAgICAgcmVnZXhwKChcXC5bMC05XSspPylcblxuICogcHJlZml4X2luY2x1ZGluZ19wYWRkaW5nID0gbm9uX251bWJlcl9zdHVmZlxuICpcbiAqIHNjaWVudGlmaWNfbm90YXRpb24gPSAgICAgIHJlZ2V4cChFWystXT9bMC05XSspXG4gKlxuICogc3VmZml4ID0gICAgICAgICAgICAgICAgICAgbm9uX251bWJlcl9zdHVmZlxuICpcbiAqIG5vbl9udW1iZXJfc3R1ZmYgPSAgICAgICAgIHJlZ2V4cChbXjAtOV0qKVxuICpcbiAqXG4gKiBSZWdleHAgZ3JvdXBzOlxuICpcbiAqIDA6IG51bWJlcl9wYXR0ZXJuX3JlXG4gKiAxOiBwcmVmaXhcbiAqIDI6IGludGVnZXJfaW5jbHVkaW5nX2dyb3VwX3NlcGFyYXRvciBmcmFjdGlvbl9pbmNsdWRpbmdfZGVjaW1hbF9zZXBhcmF0b3JcbiAqIDM6IGludGVnZXJfaW5jbHVkaW5nX2dyb3VwX3NlcGFyYXRvclxuICogNDogZnJhY3Rpb25faW5jbHVkaW5nX2RlY2ltYWxfc2VwYXJhdG9yXG4gKiA1OiBzY2llbnRpZmljX25vdGF0aW9uXG4gKiA2OiBzdWZmaXhcbiAqL1xudmFyIG51bWJlck51bWJlclJlID0gKC9eKFteMC05XSopKChbMC05LF0qWzAtOV0rKShcXC5bMC05XSspPykoRVsrLV0/WzAtOV0rKT8oW14wLTldKikkLyk7XG5cblxuXG5cbi8qKlxuICogcGFyc2UoIHZhbHVlLCBwcm9wZXJ0aWVzIClcbiAqXG4gKiBAdmFsdWUgW1N0cmluZ10uXG4gKlxuICogQHByb3BlcnRpZXMgW09iamVjdF0gUGFyc2VyIHByb3BlcnRpZXMgaXMgYSByZWR1Y2VkIHByZS1wcm9jZXNzZWQgY2xkclxuICogZGF0YSBzZXQgcmV0dXJuZWQgYnkgbnVtYmVyUGFyc2VyUHJvcGVydGllcygpLlxuICpcbiAqIFJldHVybiB0aGUgcGFyc2VkIE51bWJlciAoaW5jbHVkaW5nIEluZmluaXR5KSBvciBOYU4gd2hlbiB2YWx1ZSBpcyBpbnZhbGlkLlxuICogcmVmOiBodHRwOi8vd3d3LnVuaWNvZGUub3JnL3JlcG9ydHMvdHIzNS90cjM1LW51bWJlcnMuaHRtbFxuICovXG52YXIgbnVtYmVyUGFyc2UgPSBmdW5jdGlvbiggdmFsdWUsIHByb3BlcnRpZXMgKSB7XG5cdHZhciBhdXgsIGluZmluaXR5U3ltYm9sLCBpbnZlcnRlZE51RGlnaXRzTWFwLCBpbnZlcnRlZFN5bWJvbE1hcCwgbG9jYWxpemVkRGlnaXRSZSxcblx0XHRsb2NhbGl6ZWRTeW1ib2xzUmUsIG5lZ2F0aXZlUHJlZml4LCBuZWdhdGl2ZVN1ZmZpeCwgbnVtYmVyLCBwcmVmaXgsIHN1ZmZpeDtcblxuXHRpbmZpbml0eVN5bWJvbCA9IHByb3BlcnRpZXNbIDAgXTtcblx0aW52ZXJ0ZWRTeW1ib2xNYXAgPSBwcm9wZXJ0aWVzWyAxIF07XG5cdG5lZ2F0aXZlUHJlZml4ID0gcHJvcGVydGllc1sgMiBdO1xuXHRuZWdhdGl2ZVN1ZmZpeCA9IHByb3BlcnRpZXNbIDMgXTtcblx0aW52ZXJ0ZWROdURpZ2l0c01hcCA9IHByb3BlcnRpZXNbIDQgXTtcblxuXHQvLyBJbmZpbml0ZSBudW1iZXIuXG5cdGlmICggYXV4ID0gdmFsdWUubWF0Y2goIGluZmluaXR5U3ltYm9sICkgKSB7XG5cblx0XHRudW1iZXIgPSBJbmZpbml0eTtcblx0XHRwcmVmaXggPSB2YWx1ZS5zbGljZSggMCwgYXV4Lmxlbmd0aCApO1xuXHRcdHN1ZmZpeCA9IHZhbHVlLnNsaWNlKCBhdXgubGVuZ3RoICsgMSApO1xuXG5cdC8vIEZpbml0ZSBudW1iZXIuXG5cdH0gZWxzZSB7XG5cblx0XHQvLyBUT0RPOiBDcmVhdGUgaXQgZHVyaW5nIHNldHVwLCBpLmUuLCBtYWtlIGl0IGEgcHJvcGVydHkuXG5cdFx0bG9jYWxpemVkU3ltYm9sc1JlID0gbmV3IFJlZ0V4cChcblx0XHRcdE9iamVjdC5rZXlzKCBpbnZlcnRlZFN5bWJvbE1hcCApLm1hcChmdW5jdGlvbiggbG9jYWxpemVkU3ltYm9sICkge1xuXHRcdFx0XHRyZXR1cm4gcmVnZXhwRXNjYXBlKCBsb2NhbGl6ZWRTeW1ib2wgKTtcblx0XHRcdH0pLmpvaW4oIFwifFwiICksXG5cdFx0XHRcImdcIlxuXHRcdCk7XG5cblx0XHQvLyBSZXZlcnNlIGxvY2FsaXplZCBzeW1ib2xzLlxuXHRcdHZhbHVlID0gdmFsdWUucmVwbGFjZSggbG9jYWxpemVkU3ltYm9sc1JlLCBmdW5jdGlvbiggbG9jYWxpemVkU3ltYm9sICkge1xuXHRcdFx0cmV0dXJuIGludmVydGVkU3ltYm9sTWFwWyBsb2NhbGl6ZWRTeW1ib2wgXTtcblx0XHR9KTtcblxuXHRcdC8vIFJldmVyc2UgbG9jYWxpemVkIG51bWJlcmluZyBzeXN0ZW0uXG5cdFx0aWYgKCBpbnZlcnRlZE51RGlnaXRzTWFwICkge1xuXG5cdFx0XHQvLyBUT0RPOiBDcmVhdGUgaXQgZHVyaW5nIHNldHVwLCBpLmUuLCBtYWtlIGl0IGEgcHJvcGVydHkuXG5cdFx0XHRsb2NhbGl6ZWREaWdpdFJlID0gbmV3IFJlZ0V4cChcblx0XHRcdFx0T2JqZWN0LmtleXMoIGludmVydGVkTnVEaWdpdHNNYXAgKS5tYXAoZnVuY3Rpb24oIGxvY2FsaXplZERpZ2l0ICkge1xuXHRcdFx0XHRcdHJldHVybiByZWdleHBFc2NhcGUoIGxvY2FsaXplZERpZ2l0ICk7XG5cdFx0XHRcdH0pLmpvaW4oIFwifFwiICksXG5cdFx0XHRcdFwiZ1wiXG5cdFx0XHQpO1xuXHRcdFx0dmFsdWUgPSB2YWx1ZS5yZXBsYWNlKCBsb2NhbGl6ZWREaWdpdFJlLCBmdW5jdGlvbiggbG9jYWxpemVkRGlnaXQgKSB7XG5cdFx0XHRcdHJldHVybiBpbnZlcnRlZE51RGlnaXRzTWFwWyBsb2NhbGl6ZWREaWdpdCBdO1xuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdFx0Ly8gSXMgaXQgYSB2YWxpZCBudW1iZXI/XG5cdFx0dmFsdWUgPSB2YWx1ZS5tYXRjaCggbnVtYmVyTnVtYmVyUmUgKTtcblx0XHRpZiAoICF2YWx1ZSApIHtcblxuXHRcdFx0Ly8gSW52YWxpZCBudW1iZXIuXG5cdFx0XHRyZXR1cm4gTmFOO1xuXHRcdH1cblxuXHRcdHByZWZpeCA9IHZhbHVlWyAxIF07XG5cdFx0c3VmZml4ID0gdmFsdWVbIDYgXTtcblxuXHRcdC8vIFJlbW92ZSBncm91cGluZyBzZXBhcmF0b3JzLlxuXHRcdG51bWJlciA9IHZhbHVlWyAyIF0ucmVwbGFjZSggLywvZywgXCJcIiApO1xuXG5cdFx0Ly8gU2NpZW50aWZpYyBub3RhdGlvblxuXHRcdGlmICggdmFsdWVbIDUgXSApIHtcblx0XHRcdG51bWJlciArPSB2YWx1ZVsgNSBdO1xuXHRcdH1cblxuXHRcdG51bWJlciA9ICtudW1iZXI7XG5cblx0XHQvLyBJcyBpdCBhIHZhbGlkIG51bWJlcj9cblx0XHRpZiAoIGlzTmFOKCBudW1iZXIgKSApIHtcblxuXHRcdFx0Ly8gSW52YWxpZCBudW1iZXIuXG5cdFx0XHRyZXR1cm4gTmFOO1xuXHRcdH1cblxuXHRcdC8vIFBlcmNlbnRcblx0XHRpZiAoIHZhbHVlWyAwIF0uaW5kZXhPZiggXCIlXCIgKSAhPT0gLTEgKSB7XG5cdFx0XHRudW1iZXIgLz0gMTAwO1xuXHRcdFx0c3VmZml4ID0gc3VmZml4LnJlcGxhY2UoIFwiJVwiLCBcIlwiICk7XG5cblx0XHQvLyBQZXIgbWlsbGVcblx0XHR9IGVsc2UgaWYgKCB2YWx1ZVsgMCBdLmluZGV4T2YoIFwiXFx1MjAzMFwiICkgIT09IC0xICkge1xuXHRcdFx0bnVtYmVyIC89IDEwMDA7XG5cdFx0XHRzdWZmaXggPSBzdWZmaXgucmVwbGFjZSggXCJcXHUyMDMwXCIsIFwiXCIgKTtcblx0XHR9XG5cdH1cblxuXHQvLyBOZWdhdGl2ZSBudW1iZXJcblx0Ly8gXCJJZiB0aGVyZSBpcyBhbiBleHBsaWNpdCBuZWdhdGl2ZSBzdWJwYXR0ZXJuLCBpdCBzZXJ2ZXMgb25seSB0byBzcGVjaWZ5IHRoZSBuZWdhdGl2ZSBwcmVmaXhcblx0Ly8gYW5kIHN1ZmZpeC4gSWYgdGhlcmUgaXMgbm8gZXhwbGljaXQgbmVnYXRpdmUgc3VicGF0dGVybiwgdGhlIG5lZ2F0aXZlIHN1YnBhdHRlcm4gaXMgdGhlXG5cdC8vIGxvY2FsaXplZCBtaW51cyBzaWduIHByZWZpeGVkIHRvIHRoZSBwb3NpdGl2ZSBzdWJwYXR0ZXJuXCIgVVRTIzM1XG5cdGlmICggcHJlZml4ID09PSBuZWdhdGl2ZVByZWZpeCAmJiBzdWZmaXggPT09IG5lZ2F0aXZlU3VmZml4ICkge1xuXHRcdG51bWJlciAqPSAtMTtcblx0fVxuXG5cdHJldHVybiBudW1iZXI7XG59O1xuXG5cblxuXG4vKipcbiAqIHN5bWJvbE1hcCggY2xkciApXG4gKlxuICogQGNsZHIgW0NsZHIgaW5zdGFuY2VdLlxuICpcbiAqIFJldHVybiB0aGUgKGxvY2FsaXplZCBzeW1ib2wsIHBhdHRlcm4gc3ltYm9sKSBrZXkgdmFsdWUgcGFpciwgZWcuIHtcbiAqICAgXCLZq1wiOiBcIi5cIixcbiAqICAgXCLZrFwiOiBcIixcIixcbiAqICAgXCLZqlwiOiBcIiVcIixcbiAqICAgLi4uXG4gKiB9O1xuICovXG52YXIgbnVtYmVyU3ltYm9sSW52ZXJ0ZWRNYXAgPSBmdW5jdGlvbiggY2xkciApIHtcblx0dmFyIHN5bWJvbCxcblx0XHRzeW1ib2xNYXAgPSB7fTtcblxuXHRmb3IgKCBzeW1ib2wgaW4gbnVtYmVyU3ltYm9sTmFtZSApIHtcblx0XHRzeW1ib2xNYXBbIG51bWJlclN5bWJvbCggbnVtYmVyU3ltYm9sTmFtZVsgc3ltYm9sIF0sIGNsZHIgKSBdID0gc3ltYm9sO1xuXHR9XG5cblx0cmV0dXJuIHN5bWJvbE1hcDtcbn07XG5cblxuXG5cbi8qKlxuICogcGFyc2VQcm9wZXJ0aWVzKCBwYXR0ZXJuLCBjbGRyIClcbiAqXG4gKiBAcGF0dGVybiBbU3RyaW5nXSByYXcgcGF0dGVybiBmb3IgbnVtYmVycy5cbiAqXG4gKiBAY2xkciBbQ2xkciBpbnN0YW5jZV0uXG4gKlxuICogUmV0dXJuIHBhcnNlciBwcm9wZXJ0aWVzLCB1c2VkIHRvIGZlZWQgcGFyc2VyIGZ1bmN0aW9uLlxuICovXG52YXIgbnVtYmVyUGFyc2VQcm9wZXJ0aWVzID0gZnVuY3Rpb24oIHBhdHRlcm4sIGNsZHIgKSB7XG5cdHZhciBpbnZlcnRlZE51RGlnaXRzTWFwLCBpbnZlcnRlZE51RGlnaXRzTWFwU2FuaXR5Q2hlY2ssIG5lZ2F0aXZlUGF0dGVybiwgbmVnYXRpdmVQcm9wZXJ0aWVzLFxuXHRcdG51RGlnaXRzTWFwID0gbnVtYmVyTnVtYmVyaW5nU3lzdGVtRGlnaXRzTWFwKCBjbGRyICk7XG5cblx0cGF0dGVybiA9IHBhdHRlcm4uc3BsaXQoIFwiO1wiICk7XG5cdG5lZ2F0aXZlUGF0dGVybiA9IHBhdHRlcm5bIDEgXSB8fCBcIi1cIiArIHBhdHRlcm5bIDAgXTtcblx0bmVnYXRpdmVQcm9wZXJ0aWVzID0gbnVtYmVyUGF0dGVyblByb3BlcnRpZXMoIG5lZ2F0aXZlUGF0dGVybiApO1xuXHRpZiAoIG51RGlnaXRzTWFwICkge1xuXHRcdGludmVydGVkTnVEaWdpdHNNYXAgPSBudURpZ2l0c01hcC5zcGxpdCggXCJcIiApLnJlZHVjZShmdW5jdGlvbiggb2JqZWN0LCBsb2NhbGl6ZWREaWdpdCwgaSApIHtcblx0XHRcdG9iamVjdFsgbG9jYWxpemVkRGlnaXQgXSA9IFN0cmluZyggaSApO1xuXHRcdFx0cmV0dXJuIG9iamVjdDtcblx0XHR9LCB7fSApO1xuXHRcdGludmVydGVkTnVEaWdpdHNNYXBTYW5pdHlDaGVjayA9IFwiMDEyMzQ1Njc4OVwiLnNwbGl0KCBcIlwiICkucmVkdWNlKGZ1bmN0aW9uKCBvYmplY3QsIGRpZ2l0ICkge1xuXHRcdFx0b2JqZWN0WyBkaWdpdCBdID0gXCJpbnZhbGlkXCI7XG5cdFx0XHRyZXR1cm4gb2JqZWN0O1xuXHRcdH0sIHt9ICk7XG5cdFx0aW52ZXJ0ZWROdURpZ2l0c01hcCA9IG9iamVjdEV4dGVuZChcblx0XHRcdGludmVydGVkTnVEaWdpdHNNYXBTYW5pdHlDaGVjayxcblx0XHRcdGludmVydGVkTnVEaWdpdHNNYXBcblx0XHQpO1xuXHR9XG5cblx0Ly8gMDogQGluZmluaXR5U3ltYm9sIFtTdHJpbmddIEluZmluaXR5IHN5bWJvbC5cblx0Ly8gMTogQGludmVydGVkU3ltYm9sTWFwIFtPYmplY3RdIEludmVydGVkIHN5bWJvbCBtYXAgYXVnbWVudGVkIHdpdGggc2FuaXR5IGNoZWNrLlxuXHQvLyAgICBUaGUgc2FuaXR5IGNoZWNrIHByZXZlbnRzIHBlcm1pc3NpdmUgcGFyc2luZywgaS5lLiwgaXQgcHJldmVudHMgc3ltYm9scyB0aGF0IGRvZXNuJ3Rcblx0Ly8gICAgYmVsb25nIHRvIHRoZSBsb2NhbGl6ZWQgc2V0IHRvIHBhc3MgdGhyb3VnaC4gVGhpcyBpcyBvYnRhaW5lZCB3aXRoIHRoZSByZXN1bHQgb2YgdGhlXG5cdC8vICAgIGludmVydGVkIG1hcCBvYmplY3Qgb3ZlcmxvYWRpbmcgc3ltYm9sIG5hbWUgbWFwIG9iamVjdCAodGhlIHJlbWFpbmluZyBzeW1ib2wgbmFtZVxuXHQvLyAgICBtYXBwaW5ncyB3aWxsIGludmFsaWRhdGUgcGFyc2luZywgd29ya2luZyBhcyB0aGUgc2FuaXR5IGNoZWNrKS5cblx0Ly8gMjogQG5lZ2F0aXZlUHJlZml4IFtTdHJpbmddIE5lZ2F0aXZlIHByZWZpeC5cblx0Ly8gMzogQG5lZ2F0aXZlU3VmZml4IFtTdHJpbmddIE5lZ2F0aXZlIHN1ZmZpeCB3aXRoIHBlcmNlbnQgb3IgcGVyIG1pbGxlIHN0cmlwcGVkIG91dC5cblx0Ly8gNDogQGludmVydGVkTnVEaWdpdHNNYXAgW09iamVjdF0gSW52ZXJ0ZWQgZGlnaXRzIG1hcCBpZiBudW1iZXJpbmcgc3lzdGVtIGlzIGRpZmZlcmVudCB0aGFuXG5cdC8vICAgIGBsYXRuYCBhdWdtZW50ZWQgd2l0aCBzYW5pdHkgY2hlY2sgKHNpbWlsYXIgdG8gaW52ZXJ0ZWRTeW1ib2xNYXApLlxuXHRyZXR1cm4gW1xuXHRcdG51bWJlclN5bWJvbCggXCJpbmZpbml0eVwiLCBjbGRyICksXG5cdFx0b2JqZWN0RXh0ZW5kKCB7fSwgbnVtYmVyU3ltYm9sTmFtZSwgbnVtYmVyU3ltYm9sSW52ZXJ0ZWRNYXAoIGNsZHIgKSApLFxuXHRcdG5lZ2F0aXZlUHJvcGVydGllc1sgMCBdLFxuXHRcdG5lZ2F0aXZlUHJvcGVydGllc1sgMTAgXS5yZXBsYWNlKCBcIiVcIiwgXCJcIiApLnJlcGxhY2UoIFwiXFx1MjAzMFwiLCBcIlwiICksXG5cdFx0aW52ZXJ0ZWROdURpZ2l0c01hcFxuXHRdO1xufTtcblxuXG5cblxuLyoqXG4gKiBQYXR0ZXJuKCBzdHlsZSApXG4gKlxuICogQHN0eWxlIFtTdHJpbmddIFwiZGVjaW1hbFwiIChkZWZhdWx0KSBvciBcInBlcmNlbnRcIi5cbiAqXG4gKiBAY2xkciBbQ2xkciBpbnN0YW5jZV0uXG4gKi9cbnZhciBudW1iZXJQYXR0ZXJuID0gZnVuY3Rpb24oIHN0eWxlLCBjbGRyICkge1xuXHRpZiAoIHN0eWxlICE9PSBcImRlY2ltYWxcIiAmJiBzdHlsZSAhPT0gXCJwZXJjZW50XCIgKSB7XG5cdFx0dGhyb3cgbmV3IEVycm9yKCBcIkludmFsaWQgc3R5bGVcIiApO1xuXHR9XG5cblx0cmV0dXJuIGNsZHIubWFpbihbXG5cdFx0XCJudW1iZXJzXCIsXG5cdFx0c3R5bGUgKyBcIkZvcm1hdHMtbnVtYmVyU3lzdGVtLVwiICsgbnVtYmVyTnVtYmVyaW5nU3lzdGVtKCBjbGRyICksXG5cdFx0XCJzdGFuZGFyZFwiXG5cdF0pO1xufTtcblxuXG5cblxuLyoqXG4gKiAubnVtYmVyRm9ybWF0dGVyKCBbb3B0aW9uc10gKVxuICpcbiAqIEBvcHRpb25zIFtPYmplY3RdOlxuICogLSBzdHlsZTogW1N0cmluZ10gXCJkZWNpbWFsXCIgKGRlZmF1bHQpIG9yIFwicGVyY2VudFwiLlxuICogLSBzZWUgYWxzbyBudW1iZXIvZm9ybWF0IG9wdGlvbnMuXG4gKlxuICogUmV0dXJuIGEgZnVuY3Rpb24gdGhhdCBmb3JtYXRzIGEgbnVtYmVyIGFjY29yZGluZyB0byB0aGUgZ2l2ZW4gb3B0aW9ucyBhbmQgZGVmYXVsdC9pbnN0YW5jZVxuICogbG9jYWxlLlxuICovXG5HbG9iYWxpemUubnVtYmVyRm9ybWF0dGVyID1cbkdsb2JhbGl6ZS5wcm90b3R5cGUubnVtYmVyRm9ybWF0dGVyID0gZnVuY3Rpb24oIG9wdGlvbnMgKSB7XG5cdHZhciBjbGRyLCBtYXhpbXVtRnJhY3Rpb25EaWdpdHMsIG1heGltdW1TaWduaWZpY2FudERpZ2l0cywgbWluaW11bUZyYWN0aW9uRGlnaXRzLFxuXHRcdG1pbmltdW1JbnRlZ2VyRGlnaXRzLCBtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMsIHBhdHRlcm4sIHByb3BlcnRpZXM7XG5cblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlUGxhaW5PYmplY3QoIG9wdGlvbnMsIFwib3B0aW9uc1wiICk7XG5cblx0b3B0aW9ucyA9IG9wdGlvbnMgfHwge307XG5cdGNsZHIgPSB0aGlzLmNsZHI7XG5cblx0dmFsaWRhdGVEZWZhdWx0TG9jYWxlKCBjbGRyICk7XG5cblx0Y2xkci5vbiggXCJnZXRcIiwgdmFsaWRhdGVDbGRyICk7XG5cblx0aWYgKCBvcHRpb25zLnJhdyApIHtcblx0XHRwYXR0ZXJuID0gb3B0aW9ucy5yYXc7XG5cdH0gZWxzZSB7XG5cdFx0cGF0dGVybiA9IG51bWJlclBhdHRlcm4oIG9wdGlvbnMuc3R5bGUgfHwgXCJkZWNpbWFsXCIsIGNsZHIgKTtcblx0fVxuXG5cdHByb3BlcnRpZXMgPSBudW1iZXJGb3JtYXRQcm9wZXJ0aWVzKCBwYXR0ZXJuLCBjbGRyLCBvcHRpb25zICk7XG5cblx0Y2xkci5vZmYoIFwiZ2V0XCIsIHZhbGlkYXRlQ2xkciApO1xuXG5cdG1pbmltdW1JbnRlZ2VyRGlnaXRzID0gcHJvcGVydGllc1sgMiBdO1xuXHRtaW5pbXVtRnJhY3Rpb25EaWdpdHMgPSBwcm9wZXJ0aWVzWyAzIF07XG5cdG1heGltdW1GcmFjdGlvbkRpZ2l0cyA9IHByb3BlcnRpZXNbIDQgXTtcblxuXHRtaW5pbXVtU2lnbmlmaWNhbnREaWdpdHMgPSBwcm9wZXJ0aWVzWyA1IF07XG5cdG1heGltdW1TaWduaWZpY2FudERpZ2l0cyA9IHByb3BlcnRpZXNbIDYgXTtcblxuXHQvLyBWYWxpZGF0ZSBzaWduaWZpY2FudCBkaWdpdCBmb3JtYXQgcHJvcGVydGllc1xuXHRpZiAoICFpc05hTiggbWluaW11bVNpZ25pZmljYW50RGlnaXRzICogbWF4aW11bVNpZ25pZmljYW50RGlnaXRzICkgKSB7XG5cdFx0dmFsaWRhdGVQYXJhbWV0ZXJSYW5nZSggbWluaW11bVNpZ25pZmljYW50RGlnaXRzLCBcIm1pbmltdW1TaWduaWZpY2FudERpZ2l0c1wiLCAxLCAyMSApO1xuXHRcdHZhbGlkYXRlUGFyYW1ldGVyUmFuZ2UoIG1heGltdW1TaWduaWZpY2FudERpZ2l0cywgXCJtYXhpbXVtU2lnbmlmaWNhbnREaWdpdHNcIixcblx0XHRcdG1pbmltdW1TaWduaWZpY2FudERpZ2l0cywgMjEgKTtcblxuXHR9IGVsc2UgaWYgKCAhaXNOYU4oIG1pbmltdW1TaWduaWZpY2FudERpZ2l0cyApIHx8ICFpc05hTiggbWF4aW11bVNpZ25pZmljYW50RGlnaXRzICkgKSB7XG5cdFx0dGhyb3cgbmV3IEVycm9yKCBcIk5laXRoZXIgb3IgYm90aCB0aGUgbWluaW11bSBhbmQgbWF4aW11bSBzaWduaWZpY2FudCBkaWdpdHMgbXVzdCBiZSBcIiArXG5cdFx0XHRcInByZXNlbnRcIiApO1xuXG5cdC8vIFZhbGlkYXRlIGludGVnZXIgYW5kIGZyYWN0aW9uYWwgZm9ybWF0XG5cdH0gZWxzZSB7XG5cdFx0dmFsaWRhdGVQYXJhbWV0ZXJSYW5nZSggbWluaW11bUludGVnZXJEaWdpdHMsIFwibWluaW11bUludGVnZXJEaWdpdHNcIiwgMSwgMjEgKTtcblx0XHR2YWxpZGF0ZVBhcmFtZXRlclJhbmdlKCBtaW5pbXVtRnJhY3Rpb25EaWdpdHMsIFwibWluaW11bUZyYWN0aW9uRGlnaXRzXCIsIDAsIDIwICk7XG5cdFx0dmFsaWRhdGVQYXJhbWV0ZXJSYW5nZSggbWF4aW11bUZyYWN0aW9uRGlnaXRzLCBcIm1heGltdW1GcmFjdGlvbkRpZ2l0c1wiLFxuXHRcdFx0bWluaW11bUZyYWN0aW9uRGlnaXRzLCAyMCApO1xuXHR9XG5cblx0cmV0dXJuIGZ1bmN0aW9uKCB2YWx1ZSApIHtcblx0XHR2YWxpZGF0ZVBhcmFtZXRlclByZXNlbmNlKCB2YWx1ZSwgXCJ2YWx1ZVwiICk7XG5cdFx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlTnVtYmVyKCB2YWx1ZSwgXCJ2YWx1ZVwiICk7XG5cdFx0cmV0dXJuIG51bWJlckZvcm1hdCggdmFsdWUsIHByb3BlcnRpZXMgKTtcblx0fTtcbn07XG5cbi8qKlxuICogLm51bWJlclBhcnNlciggW29wdGlvbnNdIClcbiAqXG4gKiBAb3B0aW9ucyBbT2JqZWN0XTpcbiAqIC0gc3R5bGU6IFtTdHJpbmddIFwiZGVjaW1hbFwiIChkZWZhdWx0KSBvciBcInBlcmNlbnRcIi5cbiAqXG4gKiBSZXR1cm4gdGhlIG51bWJlciBwYXJzZXIgYWNjb3JkaW5nIHRvIHRoZSBkZWZhdWx0L2luc3RhbmNlIGxvY2FsZS5cbiAqL1xuR2xvYmFsaXplLm51bWJlclBhcnNlciA9XG5HbG9iYWxpemUucHJvdG90eXBlLm51bWJlclBhcnNlciA9IGZ1bmN0aW9uKCBvcHRpb25zICkge1xuXHR2YXIgY2xkciwgcGF0dGVybiwgcHJvcGVydGllcztcblxuXHR2YWxpZGF0ZVBhcmFtZXRlclR5cGVQbGFpbk9iamVjdCggb3B0aW9ucywgXCJvcHRpb25zXCIgKTtcblxuXHRvcHRpb25zID0gb3B0aW9ucyB8fCB7fTtcblx0Y2xkciA9IHRoaXMuY2xkcjtcblxuXHR2YWxpZGF0ZURlZmF1bHRMb2NhbGUoIGNsZHIgKTtcblxuXHRjbGRyLm9uKCBcImdldFwiLCB2YWxpZGF0ZUNsZHIgKTtcblxuXHRpZiAoIG9wdGlvbnMucmF3ICkge1xuXHRcdHBhdHRlcm4gPSBvcHRpb25zLnJhdztcblx0fSBlbHNlIHtcblx0XHRwYXR0ZXJuID0gbnVtYmVyUGF0dGVybiggb3B0aW9ucy5zdHlsZSB8fCBcImRlY2ltYWxcIiwgY2xkciApO1xuXHR9XG5cblx0cHJvcGVydGllcyA9IG51bWJlclBhcnNlUHJvcGVydGllcyggcGF0dGVybiwgY2xkciApO1xuXG5cdGNsZHIub2ZmKCBcImdldFwiLCB2YWxpZGF0ZUNsZHIgKTtcblxuXHRyZXR1cm4gZnVuY3Rpb24oIHZhbHVlICkge1xuXHRcdHZhbGlkYXRlUGFyYW1ldGVyUHJlc2VuY2UoIHZhbHVlLCBcInZhbHVlXCIgKTtcblx0XHR2YWxpZGF0ZVBhcmFtZXRlclR5cGVTdHJpbmcoIHZhbHVlLCBcInZhbHVlXCIgKTtcblx0XHRyZXR1cm4gbnVtYmVyUGFyc2UoIHZhbHVlLCBwcm9wZXJ0aWVzICk7XG5cdH07XG59O1xuXG4vKipcbiAqIC5mb3JtYXROdW1iZXIoIHZhbHVlIFssIG9wdGlvbnNdIClcbiAqXG4gKiBAdmFsdWUgW051bWJlcl0gbnVtYmVyIHRvIGJlIGZvcm1hdHRlZC5cbiAqXG4gKiBAb3B0aW9ucyBbT2JqZWN0XTogc2VlIG51bWJlci9mb3JtYXQtcHJvcGVydGllcy5cbiAqXG4gKiBGb3JtYXQgYSBudW1iZXIgYWNjb3JkaW5nIHRvIHRoZSBnaXZlbiBvcHRpb25zIGFuZCBkZWZhdWx0L2luc3RhbmNlIGxvY2FsZS5cbiAqL1xuR2xvYmFsaXplLmZvcm1hdE51bWJlciA9XG5HbG9iYWxpemUucHJvdG90eXBlLmZvcm1hdE51bWJlciA9IGZ1bmN0aW9uKCB2YWx1ZSwgb3B0aW9ucyApIHtcblx0dmFsaWRhdGVQYXJhbWV0ZXJQcmVzZW5jZSggdmFsdWUsIFwidmFsdWVcIiApO1xuXHR2YWxpZGF0ZVBhcmFtZXRlclR5cGVOdW1iZXIoIHZhbHVlLCBcInZhbHVlXCIgKTtcblxuXHRyZXR1cm4gdGhpcy5udW1iZXJGb3JtYXR0ZXIoIG9wdGlvbnMgKSggdmFsdWUgKTtcbn07XG5cbi8qKlxuICogLnBhcnNlTnVtYmVyKCB2YWx1ZSBbLCBvcHRpb25zXSApXG4gKlxuICogQHZhbHVlIFtTdHJpbmddXG4gKlxuICogQG9wdGlvbnMgW09iamVjdF06IFNlZSBudW1iZXJQYXJzZXIoKS5cbiAqXG4gKiBSZXR1cm4gdGhlIHBhcnNlZCBOdW1iZXIgKGluY2x1ZGluZyBJbmZpbml0eSkgb3IgTmFOIHdoZW4gdmFsdWUgaXMgaW52YWxpZC5cbiAqL1xuR2xvYmFsaXplLnBhcnNlTnVtYmVyID1cbkdsb2JhbGl6ZS5wcm90b3R5cGUucGFyc2VOdW1iZXIgPSBmdW5jdGlvbiggdmFsdWUsIG9wdGlvbnMgKSB7XG5cdHZhbGlkYXRlUGFyYW1ldGVyUHJlc2VuY2UoIHZhbHVlLCBcInZhbHVlXCIgKTtcblx0dmFsaWRhdGVQYXJhbWV0ZXJUeXBlU3RyaW5nKCB2YWx1ZSwgXCJ2YWx1ZVwiICk7XG5cblx0cmV0dXJuIHRoaXMubnVtYmVyUGFyc2VyKCBvcHRpb25zICkoIHZhbHVlICk7XG59O1xuXG4vKipcbiAqIE9wdGltaXphdGlvbiB0byBhdm9pZCBkdXBsaWNhdGluZyBzb21lIGludGVybmFsIGZ1bmN0aW9ucyBhY3Jvc3MgbW9kdWxlcy5cbiAqL1xuR2xvYmFsaXplLl9jcmVhdGVFcnJvclVuc3VwcG9ydGVkRmVhdHVyZSA9IGNyZWF0ZUVycm9yVW5zdXBwb3J0ZWRGZWF0dXJlO1xuR2xvYmFsaXplLl9udW1iZXJOdW1iZXJpbmdTeXN0ZW0gPSBudW1iZXJOdW1iZXJpbmdTeXN0ZW07XG5HbG9iYWxpemUuX251bWJlclBhdHRlcm4gPSBudW1iZXJQYXR0ZXJuO1xuR2xvYmFsaXplLl9udW1iZXJTeW1ib2wgPSBudW1iZXJTeW1ib2w7XG5HbG9iYWxpemUuX3N0cmluZ1BhZCA9IHN0cmluZ1BhZDtcbkdsb2JhbGl6ZS5fdmFsaWRhdGVQYXJhbWV0ZXJUeXBlTnVtYmVyID0gdmFsaWRhdGVQYXJhbWV0ZXJUeXBlTnVtYmVyO1xuR2xvYmFsaXplLl92YWxpZGF0ZVBhcmFtZXRlclR5cGVTdHJpbmcgPSB2YWxpZGF0ZVBhcmFtZXRlclR5cGVTdHJpbmc7XG5cbnJldHVybiBHbG9iYWxpemU7XG5cblxuXG5cbn0pKTtcbiJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==