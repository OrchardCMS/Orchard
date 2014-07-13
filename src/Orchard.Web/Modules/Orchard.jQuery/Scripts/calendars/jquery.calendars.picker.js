/* http://keith-wood.name/calendars.html
   Calendars date picker for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Calendar picker manager. */
function CalendarsPicker() {
	this._defaults = {
		calendar: $.calendars.instance(), // The calendar to use
		pickerClass: '', // CSS class to add to this instance of the datepicker
		showOnFocus: true, // True for popup on focus, false for not
		showTrigger: null, // Element to be cloned for a trigger, null for none
		showAnim: 'show', // Name of jQuery animation for popup, '' for no animation
		showOptions: {}, // Options for enhanced animations
		showSpeed: 'normal', // Duration of display/closure
		popupContainer: null, // The element to which a popup calendar is added, null for body
		alignment: 'bottom', // Alignment of popup - with nominated corner of input:
			// 'top' or 'bottom' aligns depending on language direction,
			// 'topLeft', 'topRight', 'bottomLeft', 'bottomRight'
		fixedWeeks: false, // True to always show 6 weeks, false to only show as many as are needed
		firstDay: null, // First day of the week, 0 = Sunday, 1 = Monday, ...
			// defaults to calendar local setting if null
		calculateWeek: null, // Calculate week of the year from a date, null for calendar default
		monthsToShow: 1, // How many months to show, cols or [rows, cols]
		monthsOffset: 0, // How many months to offset the primary month by
		monthsToStep: 1, // How many months to move when prev/next clicked
		monthsToJump: 12, // How many months to move when large prev/next clicked
		useMouseWheel: true, // True to use mousewheel if available, false to never use it
		changeMonth: true, // True to change month/year via drop-down, false for navigation only
		yearRange: 'c-10:c+10', // Range of years to show in drop-down: 'any' for direct text entry
			// or 'start:end', where start/end are '+-nn' for relative to today
			// or 'c+-nn' for relative to the currently selected date
			// or 'nnnn' for an absolute year
		showOtherMonths: false, // True to show dates from other months, false to not show them
		selectOtherMonths: false, // True to allow selection of dates from other months too
		defaultDate: null, // Date to show if no other selected
		selectDefaultDate: false, // True to pre-select the default date if no other is chosen
		minDate: null, // The minimum selectable date
		maxDate: null, // The maximum selectable date
		dateFormat: null, // Format for dates, defaults to calendar setting if null
		autoSize: false, // True to size the input field according to the date format
		rangeSelect: false, // Allows for selecting a date range on one date picker
		rangeSeparator: ' - ', // Text between two dates in a range
		multiSelect: 0, // Maximum number of selectable dates, zero for single select
		multiSeparator: ',', // Text between multiple dates
		onDate: null, // Callback as a date is added to the datepicker
		onShow: null, // Callback just before a datepicker is shown
		onChangeMonthYear: null, // Callback when a new month/year is selected
		onSelect: null, // Callback when a date is selected
		onClose: null, // Callback when a datepicker is closed
		altField: null, // Alternate field to update in synch with the datepicker
		altFormat: null, // Date format for alternate field, defaults to dateFormat
		constrainInput: true, // True to constrain typed input to dateFormat allowed characters
		commandsAsDateFormat: false, // True to apply formatDate to the command texts
		commands: this.commands // Command actions that may be added to a layout by name
	};
	this.regional = [];
	this.regional[''] = { // Default regional settings
		renderer: this.defaultRenderer, // The rendering templates
		prevText: '&lt;Prev', // Text for the previous month command
		prevStatus: 'Show the previous month', // Status text for the previous month command
		prevJumpText: '&lt;&lt;', // Text for the previous year command
		prevJumpStatus: 'Show the previous year', // Status text for the previous year command
		nextText: 'Next&gt;', // Text for the next month command
		nextStatus: 'Show the next month', // Status text for the next month command
		nextJumpText: '&gt;&gt;', // Text for the next year command
		nextJumpStatus: 'Show the next year', // Status text for the next year command
		currentText: 'Current', // Text for the current month command
		currentStatus: 'Show the current month', // Status text for the current month command
		todayText: 'Today', // Text for the today's month command
		todayStatus: 'Show today\'s month', // Status text for the today's month command
		clearText: 'Clear', // Text for the clear command
		clearStatus: 'Clear all the dates', // Status text for the clear command
		closeText: 'Close', // Text for the close command
		closeStatus: 'Close the datepicker', // Status text for the close command
		yearStatus: 'Change the year', // Status text for year selection
		monthStatus: 'Change the month', // Status text for month selection
		weekText: 'Wk', // Text for week of the year column header
		weekStatus: 'Week of the year', // Status text for week of the year column header
		dayStatus: 'Select DD, M d, yyyy', // Status text for selectable days
		defaultStatus: 'Select a date', // Status text shown by default
		isRTL: false // True if language is right-to-left
	};
	$.extend(this._defaults, this.regional['']);
	this._disabled = [];
}

$.extend(CalendarsPicker.prototype, {
	/* Class name added to elements to indicate already configured with calendar picker. */
	markerClassName: 'hasCalendarsPicker',
	/* Name of the data property for instance settings. */
	propertyName: 'calendarsPicker',

	_popupClass: 'calendars-popup', // Marker for popup division
	_triggerClass: 'calendars-trigger', // Marker for trigger element
	_disableClass: 'calendars-disable', // Marker for disabled element
	_monthYearClass: 'calendars-month-year', // Marker for month/year inputs
	_curMonthClass: 'calendars-month-', // Marker for current month/year
	_anyYearClass: 'calendars-any-year', // Marker for year direct input
	_curDoWClass: 'calendars-dow-', // Marker for day of week
	
	commands: { // Command actions that may be added to a layout by name
		// name: { // The command name, use '{button:name}' or '{link:name}' in layouts
		//		text: '', // The field in the regional settings for the displayed text
		//		status: '', // The field in the regional settings for the status text
		//      // The keystroke to trigger the action
		//		keystroke: {keyCode: nn, ctrlKey: boolean, altKey: boolean, shiftKey: boolean},
		//		enabled: fn, // The function that indicates the command is enabled
		//		date: fn, // The function to get the date associated with this action
		//		action: fn} // The function that implements the action
		prev: {text: 'prevText', status: 'prevStatus', // Previous month
			keystroke: {keyCode: 33}, // Page up
			enabled: function(inst) {
				var minDate = inst.curMinDate();
				return (!minDate || inst.drawDate.newDate().
					add(1 - inst.options.monthsToStep - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay).add(-1, 'd').compareTo(minDate) != -1); },
			date: function(inst) {
				return inst.drawDate.newDate().
					add(-inst.options.monthsToStep - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay); },
			action: function(inst) {
				plugin._changeMonthPlugin(this, -inst.options.monthsToStep); }
		},
		prevJump: {text: 'prevJumpText', status: 'prevJumpStatus', // Previous year
			keystroke: {keyCode: 33, ctrlKey: true}, // Ctrl + Page up
			enabled: function(inst) {
				var minDate = inst.curMinDate();
				return (!minDate || inst.drawDate.newDate().
					add(1 - inst.options.monthsToJump - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay).add(-1, 'd').compareTo(minDate) != -1); },
			date: function(inst) {
				return inst.drawDate.newDate().
					add(-inst.options.monthsToJump - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay); },
			action: function(inst) {
				plugin._changeMonthPlugin(this, -inst.options.monthsToJump); }
		},
		next: {text: 'nextText', status: 'nextStatus', // Next month
			keystroke: {keyCode: 34}, // Page down
			enabled: function(inst) {
				var maxDate = inst.get('maxDate');
				return (!maxDate || inst.drawDate.newDate().
					add(inst.options.monthsToStep - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay).compareTo(maxDate) != +1); },
			date: function(inst) {
				return inst.drawDate.newDate().
					add(inst.options.monthsToStep - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay); },
			action: function(inst) {
				plugin._changeMonthPlugin(this, inst.options.monthsToStep); }
		},
		nextJump: {text: 'nextJumpText', status: 'nextJumpStatus', // Next year
			keystroke: {keyCode: 34, ctrlKey: true}, // Ctrl + Page down
			enabled: function(inst) {
				var maxDate = inst.get('maxDate');
				return (!maxDate || inst.drawDate.newDate().
					add(inst.options.monthsToJump - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay).compareTo(maxDate) != +1);	},
			date: function(inst) {
				return inst.drawDate.newDate().
					add(inst.options.monthsToJump - inst.options.monthsOffset, 'm').
					day(inst.options.calendar.minDay); },
			action: function(inst) {
				plugin._changeMonthPlugin(this, inst.options.monthsToJump); }
		},
		current: {text: 'currentText', status: 'currentStatus', // Current month
			keystroke: {keyCode: 36, ctrlKey: true}, // Ctrl + Home
			enabled: function(inst) {
				var minDate = inst.curMinDate();
				var maxDate = inst.get('maxDate');
				var curDate = inst.selectedDates[0] || inst.options.calendar.today();
				return (!minDate || curDate.compareTo(minDate) != -1) &&
					(!maxDate || curDate.compareTo(maxDate) != +1); },
			date: function(inst) {
				return inst.selectedDates[0] || inst.options.calendar.today(); },
			action: function(inst) {
				var curDate = inst.selectedDates[0] || inst.options.calendar.today();
				plugin._showMonthPlugin(this, curDate.year(), curDate.month()); }
		},
		today: {text: 'todayText', status: 'todayStatus', // Today's month
			keystroke: {keyCode: 36, ctrlKey: true}, // Ctrl + Home
			enabled: function(inst) {
				var minDate = inst.curMinDate();
				var maxDate = inst.get('maxDate');
				return (!minDate || inst.options.calendar.today().compareTo(minDate) != -1) &&
					(!maxDate || inst.options.calendar.today().compareTo(maxDate) != +1); },
			date: function(inst) { return inst.options.calendar.today(); },
			action: function(inst) { plugin._showMonthPlugin(this); }
		},
		clear: {text: 'clearText', status: 'clearStatus', // Clear the datepicker
			keystroke: {keyCode: 35, ctrlKey: true}, // Ctrl + End
			enabled: function(inst) { return true; },
			date: function(inst) { return null; },
			action: function(inst) { plugin._clearPlugin(this); }
		},
		close: {text: 'closeText', status: 'closeStatus', // Close the datepicker
			keystroke: {keyCode: 27}, // Escape
			enabled: function(inst) { return true; },
			date: function(inst) { return null; },
			action: function(inst) { plugin._hidePlugin(this); }
		},
		prevWeek: {text: 'prevWeekText', status: 'prevWeekStatus', // Previous week
			keystroke: {keyCode: 38, ctrlKey: true}, // Ctrl + Up
			enabled: function(inst) {
				var minDate = inst.curMinDate();
				return (!minDate || inst.drawDate.newDate().
					add(-inst.options.calendar.daysInWeek(), 'd').compareTo(minDate) != -1); },
			date: function(inst) { return inst.drawDate.newDate().
				add(-inst.options.calendar.daysInWeek(), 'd'); },
			action: function(inst) { plugin._changeDayPlugin(
				this, -inst.options.calendar.daysInWeek()); }
		},
		prevDay: {text: 'prevDayText', status: 'prevDayStatus', // Previous day
			keystroke: {keyCode: 37, ctrlKey: true}, // Ctrl + Left
			enabled: function(inst) {
				var minDate = inst.curMinDate();
				return (!minDate || inst.drawDate.newDate().add(-1, 'd').
					compareTo(minDate) != -1); },
			date: function(inst) { return inst.drawDate.newDate().add(-1, 'd'); },
			action: function(inst) { plugin._changeDayPlugin(this, -1); }
		},
		nextDay: {text: 'nextDayText', status: 'nextDayStatus', // Next day
			keystroke: {keyCode: 39, ctrlKey: true}, // Ctrl + Right
			enabled: function(inst) {
				var maxDate = inst.get('maxDate');
				return (!maxDate || inst.drawDate.newDate().add(1, 'd').
					compareTo(maxDate) != +1); },
			date: function(inst) { return inst.drawDate.newDate().add(1, 'd'); },
			action: function(inst) { plugin._changeDayPlugin(this, 1); }
		},
		nextWeek: {text: 'nextWeekText', status: 'nextWeekStatus', // Next week
			keystroke: {keyCode: 40, ctrlKey: true}, // Ctrl + Down
			enabled: function(inst) {
				var maxDate = inst.get('maxDate');
				return (!maxDate || inst.drawDate.newDate().
					add(inst.options.calendar.daysInWeek(), 'd').compareTo(maxDate) != +1); },
			date: function(inst) { return inst.drawDate.newDate().
				add(inst.options.calendar.daysInWeek(), 'd'); },
			action: function(inst) { plugin._changeDayPlugin(
				this, inst.options.calendar.daysInWeek()); }
		}
	},

	/* Default template for generating a calendar picker. */
	defaultRenderer: {
		// Anywhere: '{l10n:name}' to insert localised value for name,
		// '{link:name}' to insert a link trigger for command name,
		// '{button:name}' to insert a button trigger for command name,
		// '{popup:start}...{popup:end}' to mark a section for inclusion in a popup datepicker only,
		// '{inline:start}...{inline:end}' to mark a section for inclusion in an inline datepicker only
		// Overall structure: '{months}' to insert calendar months
		picker: '<div class="calendars">' +
		'<div class="calendars-nav">{link:prev}{link:today}{link:next}</div>{months}' +
		'{popup:start}<div class="calendars-ctrl">{link:clear}{link:close}</div>{popup:end}' +
		'<div class="calendars-clear-fix"></div></div>',
		// One row of months: '{months}' to insert calendar months
		monthRow: '<div class="calendars-month-row">{months}</div>',
		// A single month: '{monthHeader:dateFormat}' to insert the month header -
		// dateFormat is optional and defaults to 'MM yyyy',
		// '{weekHeader}' to insert a week header, '{weeks}' to insert the month's weeks
		month: '<div class="calendars-month"><div class="calendars-month-header">{monthHeader}</div>' +
		'<table><thead>{weekHeader}</thead><tbody>{weeks}</tbody></table></div>',
		// A week header: '{days}' to insert individual day names
		weekHeader: '<tr>{days}</tr>',
		// Individual day header: '{day}' to insert day name
		dayHeader: '<th>{day}</th>',
		// One week of the month: '{days}' to insert the week's days, '{weekOfYear}' to insert week of year
		week: '<tr>{days}</tr>',
		// An individual day: '{day}' to insert day value
		day: '<td>{day}</td>',
		// jQuery selector, relative to picker, for a single month
		monthSelector: '.calendars-month',
		// jQuery selector, relative to picker, for individual days
		daySelector: 'td',
		// Class for right-to-left (RTL) languages
		rtlClass: 'calendars-rtl',
		// Class for multi-month datepickers
		multiClass: 'calendars-multi',
		// Class for selectable dates
		defaultClass: '',
		// Class for currently selected dates
		selectedClass: 'calendars-selected',
		// Class for highlighted dates
		highlightedClass: 'calendars-highlight',
		// Class for today
		todayClass: 'calendars-today',
		// Class for days from other months
		otherMonthClass: 'calendars-other-month',
		// Class for days on weekends
		weekendClass: 'calendars-weekend',
		// Class prefix for commands
		commandClass: 'calendars-cmd',
		// Extra class(es) for commands that are buttons
		commandButtonClass: '',
		// Extra class(es) for commands that are links
		commandLinkClass: '',
		// Class for disabled commands
		disabledClass: 'calendars-disabled'
	},

	/* Override the default settings for all calendar picker instances.
	   @param  options  (object) the new settings to use as defaults
	   @return  (CalendarPicker) this object */
	setDefaults: function(options) {
		$.extend(this._defaults, options || {});
		return this;
	},

	/* Attach the calendar picker functionality to an input field.
	   @param  target   (element) the control to affect
	   @param  options  (object) the custom options for this instance */
	_attachPlugin: function(target, options) {
		target = $(target);
		if (target.hasClass(this.markerClassName)) {
			return;
		}
		var inlineSettings = ($.fn.metadata ? target.metadata() || {} : {});
		var inst = {options: $.extend({}, this._defaults, inlineSettings, options),
			target: target, selectedDates: [], drawDate: null, pickingRange: false,
			inline: ($.inArray(target[0].nodeName.toLowerCase(), ['div', 'span']) > -1),
			get: function(name) { // Get a setting value, computing if necessary
				if ($.inArray(name, ['defaultDate', 'minDate', 'maxDate']) > -1) { // Decode date settings
					return this.options.calendar.determineDate(this.options[name], null,
						this.selectedDates[0], this.get('dateFormat'), inst.getConfig());
				}
				if (name == 'dateFormat') {
					return this.options.dateFormat || this.options.calendar.local.dateFormat;
				}
				return this.options[name];
			},
			curMinDate: function() {
				return (this.pickingRange ? this.selectedDates[0] : this.get('minDate'));
			},
			getConfig: function() {
				return {dayNamesShort: this.options.dayNamesShort, dayNames: this.options.dayNames,
					monthNamesShort: this.options.monthNamesShort, monthNames: this.options.monthNames,
					calculateWeek: this.options.calculateWeek, shortYearCutoff: this.options.shortYearCutoff};
			}
		};
		target.addClass(this.markerClassName).data(this.propertyName, inst);
		if (inst.inline) {
			this._update(target[0]);
			if ($.fn.mousewheel) {
				target.mousewheel(this._doMouseWheel);
			}
		}
		else {
			this._attachments(target, inst);
			target.bind('keydown.' + this.propertyName, this._keyDown).
				bind('keypress.' + this.propertyName, this._keyPress).
				bind('keyup.' + this.propertyName, this._keyUp);
			if (target.attr('disabled')) {
				this._disablePlugin(target[0]);
			}
		}
	},

	/* Retrieve or reconfigure the settings for a control.
	   @param  target   (element) the control to affect
	   @param  options  (object) the new options for this instance or
	                    (string) an individual property name
	   @param  value    (any) the individual property value (omit if options
	                    is an object or to retrieve the value of a setting)
	   @return  (any) if retrieving a value */
	_optionPlugin: function(target, options, value) {
		target = $(target);
		var inst = target.data(this.propertyName);
		if (!options || (typeof options == 'string' && value == null)) { // Get option
			var name = options;
			options = (inst || {}).options;
			return (options && name ? options[name] : options);
		}

		if (!target.hasClass(this.markerClassName)) {
			return;
		}
		options = options || {};
		if (typeof options == 'string') {
			var name = options;
			options = {};
			options[name] = value;
		}
		if (options.calendar && options.calendar != inst.options.calendar) {
			var discardDate = function(name) {
				return (typeof inst.options[name] == 'object' ? null : inst.options[name]);
			};
			options = $.extend({defaultDate: discardDate('defaultDate'),
				minDate: discardDate('minDate'), maxDate: discardDate('maxDate')}, options);
			inst.selectedDates = [];
			inst.drawDate = null;
		}
		var dates = inst.selectedDates;
		$.extend(inst.options, options);
		this._setDatePlugin(target[0], dates, null, false, true);
		inst.pickingRange = false;
		var calendar = inst.options.calendar;
		var defaultDate = inst.get('defaultDate');
		inst.drawDate = this._checkMinMax((defaultDate ? defaultDate : inst.drawDate) ||
			defaultDate || calendar.today(), inst).newDate();
		if (!inst.inline) {
			this._attachments(target, inst);
		}
		if (inst.inline || inst.div) {
			this._update(target[0]);
		}
	},

	/* Attach events and trigger, if necessary.
	   @param  target  (jQuery) the control to affect
	   @param  inst    (object) the current instance settings */
	_attachments: function(target, inst) {
		target.unbind('focus.' + this.propertyName);
		if (inst.options.showOnFocus) {
			target.bind('focus.' + this.propertyName, this._showPlugin);
		}
		if (inst.trigger) {
			inst.trigger.remove();
		}
		var trigger = inst.options.showTrigger;
		inst.trigger = (!trigger ? $([]) :
			$(trigger).clone().removeAttr('id').addClass(this._triggerClass)
				[inst.options.isRTL ? 'insertBefore' : 'insertAfter'](target).
				click(function() {
					if (!plugin._isDisabledPlugin(target[0])) {
						plugin[plugin.curInst == inst ? '_hidePlugin' : '_showPlugin'](target[0]);
					}
				}));
		this._autoSize(target, inst);
		var dates = this._extractDates(inst, target.val());
		if (dates) {
			this._setDatePlugin(target[0], dates, null, true);
		}
		var defaultDate = inst.get('defaultDate');
		if (inst.options.selectDefaultDate && defaultDate && inst.selectedDates.length == 0) {
			this._setDatePlugin(target[0], (defaultDate || inst.options.calendar.today()).newDate());
		}
	},

	/* Apply the maximum length for the date format.
	   @param  inst  (object) the current instance settings */
	_autoSize: function(target, inst) {
		if (inst.options.autoSize && !inst.inline) {
			var calendar = inst.options.calendar;
			var date = calendar.newDate(2009, 10, 20); // Ensure double digits
			var dateFormat = inst.get('dateFormat');
			if (dateFormat.match(/[DM]/)) {
				var findMax = function(names) {
					var max = 0;
					var maxI = 0;
					for (var i = 0; i < names.length; i++) {
						if (names[i].length > max) {
							max = names[i].length;
							maxI = i;
						}
					}
					return maxI;
				};
				date.month(findMax(calendar.local[dateFormat.match(/MM/) ? // Longest month
					'monthNames' : 'monthNamesShort']) + 1);
				date.day(findMax(calendar.local[dateFormat.match(/DD/) ? // Longest day
					'dayNames' : 'dayNamesShort']) + 20 - date.dayOfWeek());
			}
			inst.target.attr('size', date.formatDate(dateFormat).length);
		}
	},

	/* Remove the calendar picker functionality from a control.
	   @param  target  (element) the control to affect */
	_destroyPlugin: function(target) {
		target = $(target);
		if (!target.hasClass(this.markerClassName)) {
			return;
		}
		var inst = target.data(this.propertyName);
		if (inst.trigger) {
			inst.trigger.remove();
		}
		target.removeClass(this.markerClassName).removeData(this.propertyName).
			empty().unbind('.' + this.propertyName);
		if (inst.inline && $.fn.mousewheel) {
			target.unmousewheel();
		}
		if (!inst.inline && inst.options.autoSize) {
			target.removeAttr('size');
		}
	},

	/* Apply multiple event functions.
	   Usage, for example: onShow: multipleEvents(fn1, fn2, ...)
	   @param  fns  (function...) the functions to apply */
	multipleEvents: function(fns) {
		var funcs = arguments;
		return function(args) {
			for (var i = 0; i < funcs.length; i++) {
				funcs[i].apply(this, arguments);
			}
		};
	},

	/* Enable the control.
	   @param  target  (element) the control to affect */
	_enablePlugin: function(target) {
		target = $(target);
		if (!target.hasClass(this.markerClassName)) {
			return;
		}
		var inst = target.data(this.propertyName);
		if (inst.inline) {
			target.children('.' + this._disableClass).remove().end().
				find('button,select').removeAttr('disabled').end().
				find('a').attr('href', 'javascript:void(0)');
		}
		else {
			target.prop('disabled', false);
			inst.trigger.filter('button.' + this._triggerClass).
				removeAttr('disabled').end().
				filter('img.' + this._triggerClass).
				css({opacity: '1.0', cursor: ''});
		}
		this._disabled = $.map(this._disabled,
			function(value) { return (value == target[0] ? null : value); }); // Delete entry
	},

	/* Disable the control.
	   @param  target  (element) the control to affect */
	_disablePlugin: function(target) {
		target = $(target);
		if (!target.hasClass(this.markerClassName)) {
			return;
		}
		var inst = target.data(this.propertyName);
		if (inst.inline) {
			var inline = target.children(':last');
			var offset = inline.offset();
			var relOffset = {left: 0, top: 0};
			inline.parents().each(function() {
				if ($(this).css('position') == 'relative') {
					relOffset = $(this).offset();
					return false;
				}
			});
			var zIndex = target.css('zIndex');
			zIndex = (zIndex == 'auto' ? 0 : parseInt(zIndex, 10)) + 1;
			target.prepend('<div class="' + this._disableClass + '" style="' +
				'width: ' + inline.outerWidth() + 'px; height: ' + inline.outerHeight() +
				'px; left: ' + (offset.left - relOffset.left) + 'px; top: ' +
				(offset.top - relOffset.top) + 'px; z-index: ' + zIndex + '"></div>').
				find('button,select').attr('disabled', 'disabled').end().
				find('a').removeAttr('href');
		}
		else {
			target.prop('disabled', true);
			inst.trigger.filter('button.' + this._triggerClass).
				attr('disabled', 'disabled').end().
				filter('img.' + this._triggerClass).
				css({opacity: '0.5', cursor: 'default'});
		}
		this._disabled = $.map(this._disabled,
			function(value) { return (value == target[0] ? null : value); }); // Delete entry
		this._disabled.push(target[0]);
	},

	/* Is the first field in a jQuery collection disabled as a datepicker?
	   @param  target  (element) the control to examine
	   @return  (boolean) true if disabled, false if enabled */
	_isDisabledPlugin: function(target) {
		return (target && $.inArray(target, this._disabled) > -1);
	},

	/* Show a popup datepicker.
	   @param  target  (event) a focus event or
	                   (element) the control to use */
	_showPlugin: function(target) {
		target = $(target.target || target);
		var inst = target.data(plugin.propertyName);
		if (plugin.curInst == inst) {
			return;
		}
		if (plugin.curInst) {
			plugin._hidePlugin(plugin.curInst, true);
		}
		if (inst) {
			// Retrieve existing date(s)
			inst.lastVal = null;
			inst.selectedDates = plugin._extractDates(inst, target.val());
			inst.pickingRange = false;
			inst.drawDate = plugin._checkMinMax((inst.selectedDates[0] ||
				inst.get('defaultDate') || inst.options.calendar.today()).newDate(), inst);
			plugin.curInst = inst;
			// Generate content
			plugin._update(target[0], true);
			// Adjust position before showing
			var offset = plugin._checkOffset(inst);
			inst.div.css({left: offset.left, top: offset.top});
			// And display
			var showAnim = inst.options.showAnim;
			var showSpeed = inst.options.showSpeed;
			if ($.effects && $.effects[showAnim]) {
				var data = inst.div.data(); // Update old effects data
				for (var key in data) {
					if (key.match(/^ec\.storage\./)) {
						data[key] = inst._mainDiv.css(key.replace(/ec\.storage\./, ''));
					}
				}
				inst.div.data(data).show(showAnim, inst.options.showOptions, showSpeed);
			}
			else {
				inst.div[showAnim || 'show'](showAnim ? showSpeed : 0);
			}
		}
	},

	/* Extract possible dates from a string.
	   @param  inst  (object) the current instance settings
	   @param  text  (string) the text to extract from
	   @return  (CDate[]) the extracted dates */
	_extractDates: function(inst, datesText) {
		if (datesText == inst.lastVal) {
			return;
		}
		inst.lastVal = datesText;
		datesText = datesText.split(inst.options.multiSelect ? inst.options.multiSeparator :
			(inst.options.rangeSelect ? inst.options.rangeSeparator : '\x00'));
		var dates = [];
		for (var i = 0; i < datesText.length; i++) {
			try {
				var date = inst.options.calendar.parseDate(inst.get('dateFormat'), datesText[i]);
				if (date) {
					var found = false;
					for (var j = 0; j < dates.length; j++) {
						if (dates[j].compareTo(date) == 0) {
							found = true;
							break;
						}
					}
					if (!found) {
						dates.push(date);
					}
				}
			}
			catch (e) {
				// Ignore
			}
		}
		dates.splice(inst.options.multiSelect || (inst.options.rangeSelect ? 2 : 1), dates.length);
		if (inst.options.rangeSelect && dates.length == 1) {
			dates[1] = dates[0];
		}
		return dates;
	},

	/* Update the datepicker display.
	   @param  target  (event) a focus event or
	                   (element) the control to use
	   @param  hidden  (boolean) true to initially hide the datepicker */
	_update: function(target, hidden) {
		target = $(target.target || target);
		var inst = target.data(plugin.propertyName);
		if (inst) {
			if (inst.inline || plugin.curInst == inst) {
				if ($.isFunction(inst.options.onChangeMonthYear) &&
						(!inst.prevDate || inst.prevDate.year() != inst.drawDate.year() ||
						inst.prevDate.month() != inst.drawDate.month())) {
					inst.options.onChangeMonthYear.apply(target[0],
						[inst.drawDate.year(), inst.drawDate.month()]);
				}
			}
			if (inst.inline) {
				target.html(this._generateContent(target[0], inst));
			}
			else if (plugin.curInst == inst) {
				if (!inst.div) {
					inst.div = $('<div></div>').addClass(this._popupClass).
						css({display: (hidden ? 'none' : 'static'), position: 'absolute',
							left: target.offset().left,
							top: target.offset().top + target.outerHeight()}).
						appendTo($(inst.options.popupContainer || 'body'));
					if ($.fn.mousewheel) {
						inst.div.mousewheel(this._doMouseWheel);
					}
				}
				inst.div.html(this._generateContent(target[0], inst));
				target.focus();
			}
		}
	},

	/* Update the input field and any alternate field with the current dates.
	   @param  target  (element) the control to use
	   @param  keyUp   (boolean, internal) true if coming from keyUp processing */
	_updateInput: function(target, keyUp) {
		var inst = $.data(target, this.propertyName);
		if (inst) {
			var value = '';
			var altValue = '';
			var sep = (inst.options.multiSelect ? inst.options.multiSeparator :
				inst.options.rangeSeparator);
			var calendar = inst.options.calendar;
			var dateFormat = inst.get('dateFormat');
			var altFormat = inst.options.altFormat || dateFormat;
			for (var i = 0; i < inst.selectedDates.length; i++) {
				value += (keyUp ? '' : (i > 0 ? sep : '') +
					calendar.formatDate(dateFormat, inst.selectedDates[i]));
				altValue += (i > 0 ? sep : '') +
					calendar.formatDate(altFormat, inst.selectedDates[i]);
			}
			if (!inst.inline && !keyUp) {
				$(target).val(value);
			}
			$(inst.options.altField).val(altValue);
			if ($.isFunction(inst.options.onSelect) && !keyUp && !inst.inSelect) {
				inst.inSelect = true; // Prevent endless loops
				inst.options.onSelect.apply(target, [inst.selectedDates]);
				inst.inSelect = false;
			}
		}
	},

	/* Retrieve the size of left and top borders for an element.
	   @param  elem  (jQuery) the element of interest
	   @return  (number[2]) the left and top borders */
	_getBorders: function(elem) {
		var convert = function(value) {
			return {thin: 1, medium: 3, thick: 5}[value] || value;
		};
		return [parseFloat(convert(elem.css('border-left-width'))),
			parseFloat(convert(elem.css('border-top-width')))];
	},

	/* Check positioning to remain on the screen.
	   @param  inst  (object) the current instance settings
	   @return  (object) the updated offset for the datepicker */
	_checkOffset: function(inst) {
		var base = (inst.target.is(':hidden') && inst.trigger ? inst.trigger : inst.target);
		var offset = base.offset();
		var browserWidth = window.innerWidth || document.documentElement.clientWidth;
		var browserHeight = window.innerHeight || document.documentElement.clientHeight;
		if (browserWidth == 0) {
			return offset;
		}
		var isFixed = false;
		$(inst.target).parents().each(function() {
			isFixed |= $(this).css('position') == 'fixed';
			return !isFixed;
		});
		var scrollX = document.documentElement.scrollLeft || document.body.scrollLeft;
		var scrollY = document.documentElement.scrollTop || document.body.scrollTop;
		var above = offset.top - (isFixed ? scrollY : 0) - inst.div.outerHeight();
		var below = offset.top - (isFixed ? scrollY : 0) + base.outerHeight();
		var alignL = offset.left - (isFixed ? scrollX : 0);
		var alignR = offset.left - (isFixed ? scrollX : 0) + base.outerWidth() - inst.div.outerWidth();
		var tooWide = (offset.left - scrollX + inst.div.outerWidth()) > browserWidth;
		var tooHigh = (offset.top - scrollY + inst.target.outerHeight() +
			inst.div.outerHeight()) > browserHeight;
		inst.div.css('position', isFixed ? 'fixed' : 'absolute');
		var alignment = inst.options.alignment;
		if (alignment == 'topLeft') {
			offset = {left: alignL, top: above};
		}
		else if (alignment == 'topRight') {
			offset = {left: alignR, top: above};
		}
		else if (alignment == 'bottomLeft') {
			offset = {left: alignL, top: below};
		}
		else if (alignment == 'bottomRight') {
			offset = {left: alignR, top: below};
		}
		else if (alignment == 'top') {
			offset = {left: (inst.options.isRTL || tooWide ? alignR : alignL), top: above};
		}
		else { // bottom
			offset = {left: (inst.options.isRTL || tooWide ? alignR : alignL),
				top: (tooHigh ? above : below)};
		}
		offset.left = Math.max((isFixed ? 0 : scrollX), offset.left);
		offset.top = Math.max((isFixed ? 0 : scrollY), offset.top);
		return offset;
	},

	/* Close date picker if clicked elsewhere.
	   @param  event  (MouseEvent) the mouse click to check */
	_checkExternalClick: function(event) {
		if (!plugin.curInst) {
			return;
		}
		var target = $(event.target);
		if (!target.parents().andSelf().hasClass(plugin._popupClass) &&
				!target.hasClass(plugin.markerClassName) &&
				!target.parents().andSelf().hasClass(plugin._triggerClass)) {
			plugin._hidePlugin(plugin.curInst);
		}
	},

	/* Hide a popup datepicker.
	   @param  target     (element) the control to use or
	                      (object) the current instance settings
	   @param  immediate  (boolean) true to close immediately without animation */
	_hidePlugin: function(target, immediate) {
		if (!target) {
			return;
		}
		var inst = $.data(target, this.propertyName) || target;
		if (inst && inst == plugin.curInst) {
			var showAnim = (immediate ? '' : inst.options.showAnim);
			var showSpeed = inst.options.showSpeed;
			var postProcess = function() {
				if (!inst.div) {
					return;
				}
				inst.div.remove();
				inst.div = null;
				plugin.curInst = null;
				if ($.isFunction(inst.options.onClose)) {
					inst.options.onClose.apply(target, [inst.selectedDates]);
				}
			};
			inst.div.stop();
			if ($.effects && $.effects[showAnim]) {
				inst.div.hide(showAnim, inst.options.showOptions, showSpeed, postProcess);
			}
			else {
				var hideAnim = (showAnim == 'slideDown' ? 'slideUp' :
					(showAnim == 'fadeIn' ? 'fadeOut' : 'hide'));
				inst.div[hideAnim]((showAnim ? showSpeed : 0), postProcess);
			}
		}
	},

	/* Handle keystrokes in the datepicker.
	   @param  event  (KeyEvent) the keystroke
	   @return  (boolean) true if not handled, false if handled */
	_keyDown: function(event) {
		var target = event.target;
		var inst = $.data(target, plugin.propertyName);
		var handled = false;
		if (inst.div) {
			if (event.keyCode == 9) { // Tab - close
				plugin._hidePlugin(target);
			}
			else if (event.keyCode == 13) { // Enter - select
				plugin._selectDatePlugin(target,
					$('a.' + inst.options.renderer.highlightedClass, inst.div)[0]);
				handled = true;
			}
			else { // Command keystrokes
				var commands = inst.options.commands;
				for (var name in commands) {
					var command = commands[name];
					if (command.keystroke.keyCode == event.keyCode &&
							!!command.keystroke.ctrlKey == !!(event.ctrlKey || event.metaKey) &&
							!!command.keystroke.altKey == event.altKey &&
							!!command.keystroke.shiftKey == event.shiftKey) {
						plugin._performActionPlugin(target, name);
						handled = true;
						break;
					}
				}
			}
		}
		else { // Show on 'current' keystroke
			var command = inst.options.commands.current;
			if (command.keystroke.keyCode == event.keyCode &&
					!!command.keystroke.ctrlKey == !!(event.ctrlKey || event.metaKey) &&
					!!command.keystroke.altKey == event.altKey &&
					!!command.keystroke.shiftKey == event.shiftKey) {
				plugin._showPlugin(target);
				handled = true;
			}
		}
		inst.ctrlKey = ((event.keyCode < 48 && event.keyCode != 32) ||
			event.ctrlKey || event.metaKey);
		if (handled) {
			event.preventDefault();
			event.stopPropagation();
		}
		return !handled;
	},

	/* Filter keystrokes in the datepicker.
	   @param  event  (KeyEvent) the keystroke
	   @return  (boolean) true if allowed, false if not allowed */
	_keyPress: function(event) {
		var inst = $(event.target).data(plugin.propertyName);
		if (inst && inst.options.constrainInput) {
			var ch = String.fromCharCode(event.keyCode || event.charCode);
			var allowedChars = plugin._allowedChars(inst);
			return (event.metaKey || inst.ctrlKey || ch < ' ' ||
				!allowedChars || allowedChars.indexOf(ch) > -1);
		}
		return true;
	},

	/* Determine the set of characters allowed by the date format.
	   @param  inst  (object) the current instance settings
	   @return  (string) the set of allowed characters, or null if anything allowed */
	_allowedChars: function(inst) {
		var allowedChars = (inst.options.multiSelect ? inst.options.multiSeparator :
			(inst.options.rangeSelect ? inst.options.rangeSeparator : ''));
		var literal = false;
		var hasNum = false;
		var dateFormat = inst.get('dateFormat');
		for (var i = 0; i < dateFormat.length; i++) {
			var ch = dateFormat.charAt(i);
			if (literal) {
				if (ch == "'" && dateFormat.charAt(i + 1) != "'") {
					literal = false;
				}
				else {
					allowedChars += ch;
				}
			}
			else {
				switch (ch) {
					case 'd': case 'm': case 'o': case 'w':
						allowedChars += (hasNum ? '' : '0123456789'); hasNum = true; break;
					case 'y': case '@': case '!':
						allowedChars += (hasNum ? '' : '0123456789') + '-'; hasNum = true; break;
					case 'J':
						allowedChars += (hasNum ? '' : '0123456789') + '-.'; hasNum = true; break;
					case 'D': case 'M': case 'Y':
						return null; // Accept anything
					case "'":
						if (dateFormat.charAt(i + 1) == "'") {
							allowedChars += "'";
						}
						else {
							literal = true;
						}
						break;
					default:
						allowedChars += ch;
				}
			}
		}
		return allowedChars;
	},

	/* Synchronise datepicker with the field.
	   @param  event  (KeyEvent) the keystroke
	   @return  (boolean) true if allowed, false if not allowed */
	_keyUp: function(event) {
		var target = event.target;
		var inst = $.data(target, plugin.propertyName);
		if (inst && !inst.ctrlKey && inst.lastVal != inst.target.val()) {
			try {
				var dates = plugin._extractDates(inst, inst.target.val());
				if (dates.length > 0) {
					plugin._setDatePlugin(target, dates, null, true);
				}
			}
			catch (event) {
				// Ignore
			}
		}
		return true;
	},

	/* Increment/decrement month/year on mouse wheel activity.
	   @param  event  (event) the mouse wheel event
	   @param  delta  (number) the amount of change */
	_doMouseWheel: function(event, delta) {
		var target = (plugin.curInst && plugin.curInst.target[0]) ||
			$(event.target).closest('.' + plugin.markerClassName)[0];
		if (plugin._isDisabledPlugin(target)) {
			return;
		}
		var inst = $.data(target, plugin.propertyName);
		if (inst.options.useMouseWheel) {
			delta = (delta < 0 ? -1 : +1);
			plugin._changeMonthPlugin(target,
				-inst.options[event.ctrlKey ? 'monthsToJump' : 'monthsToStep'] * delta);
		}
		event.preventDefault();
	},

	/* Clear an input and close a popup datepicker.
	   @param  target  (element) the control to use */
	_clearPlugin: function(target) {
		var inst = $.data(target, this.propertyName);
		if (inst) {
			inst.selectedDates = [];
			this._hidePlugin(target);
			var defaultDate = inst.get('defaultDate');
			if (inst.options.selectDefaultDate && defaultDate) {
				this._setDatePlugin(target,
					(defaultDate || inst.options.calendar.today()).newDate());
			}
			else {
				this._updateInput(target);
			}
		}
	},

	/* Retrieve the selected date(s) for a calendar picker.
	   @param  target  (element) the control to examine
	   @return  (CDate[]) the selected date(s) */
	_getDatePlugin: function(target) {
		var inst = $.data(target, this.propertyName);
		return (inst ? inst.selectedDates : []);
	},

	/* Set the selected date(s) for a calendar picker.
	   @param  target   (element) the control to examine
	   @param  dates    (CDate or number or string or [] of these) the selected date(s)
	   @param  endDate  (CDate or number or string) the ending date for a range (optional)
	   @param  keyUp    (boolean, internal) true if coming from keyUp processing
	   @param  setOpt   (boolean, internal) true if coming from option processing */
	_setDatePlugin: function(target, dates, endDate, keyUp, setOpt) {
		var inst = $.data(target, this.propertyName);
		if (inst) {
			if (!$.isArray(dates)) {
				dates = [dates];
				if (endDate) {
					dates.push(endDate);
				}
			}
			var minDate = inst.get('minDate');
			var maxDate = inst.get('maxDate');
			var curDate = inst.selectedDates[0];
			inst.selectedDates = [];
			for (var i = 0; i < dates.length; i++) {
				var date = inst.options.calendar.determineDate(
					dates[i], null, curDate, inst.get('dateFormat'), inst.getConfig());
				if (date) {
					if ((!minDate || date.compareTo(minDate) != -1) &&
							(!maxDate || date.compareTo(maxDate) != +1)) {
						var found = false;
						for (var j = 0; j < inst.selectedDates.length; j++) {
							if (inst.selectedDates[j].compareTo(date) == 0) {
								found = true;
								break;
							}
						}
						if (!found) {
							inst.selectedDates.push(date);
						}
					}
				}
			}
			inst.selectedDates.splice(inst.options.multiSelect ||
				(inst.options.rangeSelect ? 2 : 1), inst.selectedDates.length);
			if (inst.options.rangeSelect) {
				switch (inst.selectedDates.length) {
					case 1: inst.selectedDates[1] = inst.selectedDates[0]; break;
					case 2: inst.selectedDates[1] =
						(inst.selectedDates[0].compareTo(inst.selectedDates[1]) == +1 ?
						inst.selectedDates[0] : inst.selectedDates[1]); break;
				}
				inst.pickingRange = false;
			}
			inst.prevDate = (inst.drawDate ? inst.drawDate.newDate() : null);
			inst.drawDate = this._checkMinMax((inst.selectedDates[0] ||
				inst.get('defaultDate') || inst.options.calendar.today()).newDate(), inst);
			if (!setOpt) {
				this._update(target);
				this._updateInput(target, keyUp);
			}
		}
	},

	/* Determine whether a date is selectable for this datepicker.
	   @param  target  (element) the control to check
	   @param  date    (Date or string or number) the date to check
	   @return  (boolean) true if selectable, false if not */
	_isSelectablePlugin: function(target, date) {
		var inst = $.data(target, this.propertyName);
		if (!inst) {
			return false;
		}
		date = inst.options.calendar.determineDate(date,
			inst.selectedDates[0] || inst.options.calendar.today(), null,
			inst.get('dateFormat'), inst.getConfig());
		return this._isSelectable(target, date, inst.options.onDate,
			inst.get('minDate'), inst.get('maxDate'));
	},

	/* Internally determine whether a date is selectable for this datepicker.
	   @param  target   (element) the control to check
	   @param  date     (Date) the date to check
	   @param  onDate   (function or boolean) any onDate callback or callback.selectable
	   @param  mindate  (Date) the minimum allowed date
	   @param  maxdate  (Date) the maximum allowed date
	   @return  (boolean) true if selectable, false if not */
	_isSelectable: function(target, date, onDate, minDate, maxDate) {
		var dateInfo = (typeof onDate == 'boolean' ? {selectable: onDate} :
			(!$.isFunction(onDate) ? {} : onDate.apply(target, [date, true])));
		return (dateInfo.selectable != false) &&
			(!minDate || date.toJD() >= minDate.toJD()) &&
			(!maxDate || date.toJD() <= maxDate.toJD());
	},

	/* Perform a named action for a calendar picker.
	   @param  target  (element) the control to affect
	   @param  action  (string) the name of the action */
	_performActionPlugin: function(target, action) {
		var inst = $.data(target, this.propertyName);
		if (inst && !this._isDisabledPlugin(target)) {
			var commands = inst.options.commands;
			if (commands[action] && commands[action].enabled.apply(target, [inst])) {
				commands[action].action.apply(target, [inst]);
			}
		}
	},

	/* Set the currently shown month, defaulting to today's.
	   @param  target  (element) the control to affect
	   @param  year    (number) the year to show (optional)
	   @param  month   (number) the month to show (optional)
	   @param  day     (number) the day to show (optional) */
	_showMonthPlugin: function(target, year, month, day) {
		var inst = $.data(target, this.propertyName);
		if (inst && (day != null ||
				(inst.drawDate.year() != year || inst.drawDate.month() != month))) {
			inst.prevDate = inst.drawDate.newDate();
			var calendar = inst.options.calendar;
			var show = this._checkMinMax((year != null ?
				calendar.newDate(year, month, 1) : calendar.today()), inst);
			inst.drawDate.date(show.year(), show.month(), 
				(day != null ? day : Math.min(inst.drawDate.day(),
				calendar.daysInMonth(show.year(), show.month()))));
			this._update(target);
		}
	},

	/* Adjust the currently shown month.
	   @param  target  (element) the control to affect
	   @param  offset  (number) the number of months to change by */
	_changeMonthPlugin: function(target, offset) {
		var inst = $.data(target, this.propertyName);
		if (inst) {
			var date = inst.drawDate.newDate().add(offset, 'm');
			this._showMonthPlugin(target, date.year(), date.month());
		}
	},

	/* Adjust the currently shown day.
	   @param  target  (element) the control to affect
	   @param  offset  (number) the number of days to change by */
	_changeDayPlugin: function(target, offset) {
		var inst = $.data(target, this.propertyName);
		if (inst) {
			var date = inst.drawDate.newDate().add(offset, 'd');
			this._showMonthPlugin(target, date.year(), date.month(), date.day());
		}
	},

	/* Restrict a date to the minimum/maximum specified.
	   @param  date  (CDate) the date to check
	   @param  inst  (object) the current instance settings */
	_checkMinMax: function(date, inst) {
		var minDate = inst.get('minDate');
		var maxDate = inst.get('maxDate');
		date = (minDate && date.compareTo(minDate) == -1 ? minDate.newDate() : date);
		date = (maxDate && date.compareTo(maxDate) == +1 ? maxDate.newDate() : date);
		return date;
	},

	/* Retrieve the date associated with an entry in the datepicker.
	   @param  target  (element) the control to examine
	   @param  elem    (element) the selected datepicker element
	   @return  (CDate) the corresponding date, or null */
	_retrieveDatePlugin: function(target, elem) {
		var inst = $.data(target, this.propertyName);
		return (!inst ? null : inst.options.calendar.fromJD(
			parseFloat(elem.className.replace(/^.*jd(\d+\.5).*$/, '$1'))));
	},

	/* Select a date for this datepicker.
	   @param  target  (element) the control to examine
	   @param  elem    (element) the selected datepicker element */
	_selectDatePlugin: function(target, elem) {
		var inst = $.data(target, this.propertyName);
		if (inst && !this._isDisabledPlugin(target)) {
			var date = this._retrieveDatePlugin(target, elem);
			if (inst.options.multiSelect) {
				var found = false;
				for (var i = 0; i < inst.selectedDates.length; i++) {
					if (date.compareTo(inst.selectedDates[i]) == 0) {
						inst.selectedDates.splice(i, 1);
						found = true;
						break;
					}
				}
				if (!found && inst.selectedDates.length < inst.options.multiSelect) {
					inst.selectedDates.push(date);
				}
			}
			else if (inst.options.rangeSelect) {
				if (inst.pickingRange) {
					inst.selectedDates[1] = date;
				}
				else {
					inst.selectedDates = [date, date];
				}
				inst.pickingRange = !inst.pickingRange;
			}
			else {
				inst.selectedDates = [date];
			}
			this._updateInput(target);
			if (inst.inline || inst.pickingRange || inst.selectedDates.length <
					(inst.options.multiSelect || (inst.options.rangeSelect ? 2 : 1))) {
				this._update(target);
			}
			else {
				this._hidePlugin(target);
			}
		}
	},

	/* Generate the datepicker content for this control.
	   @param  target  (element) the control to affect
	   @param  inst    (object) the current instance settings
	   @return  (jQuery) the datepicker content */
	_generateContent: function(target, inst) {
		var monthsToShow = inst.options.monthsToShow;
		monthsToShow = ($.isArray(monthsToShow) ? monthsToShow : [1, monthsToShow]);
		inst.drawDate = this._checkMinMax(
			inst.drawDate || inst.get('defaultDate') || inst.options.calendar.today(), inst);
		var drawDate = inst.drawDate.newDate().add(-inst.options.monthsOffset, 'm');
		// Generate months
		var monthRows = '';
		for (var row = 0; row < monthsToShow[0]; row++) {
			var months = '';
			for (var col = 0; col < monthsToShow[1]; col++) {
				months += this._generateMonth(target, inst, drawDate.year(),
					drawDate.month(), inst.options.calendar, inst.options.renderer, (row == 0 && col == 0));
				drawDate.add(1, 'm');
			}
			monthRows += this._prepare(inst.options.renderer.monthRow, inst).replace(/\{months\}/, months);
		}
		var picker = this._prepare(inst.options.renderer.picker, inst).replace(/\{months\}/, monthRows).
			replace(/\{weekHeader\}/g, this._generateDayHeaders(inst, inst.options.calendar, inst.options.renderer));
		// Add commands
		var addCommand = function(type, open, close, name, classes) {
			if (picker.indexOf('{' + type + ':' + name + '}') == -1) {
				return;
			}
			var command = inst.options.commands[name];
			var date = (inst.options.commandsAsDateFormat ? command.date.apply(target, [inst]) : null);
			picker = picker.replace(new RegExp('\\{' + type + ':' + name + '\\}', 'g'),
				'<' + open +
				(command.status ? ' title="' + inst.options[command.status] + '"' : '') +
				' class="' + inst.options.renderer.commandClass + ' ' +
				inst.options.renderer.commandClass + '-' + name + ' ' + classes +
				(command.enabled(inst) ? '' : ' ' + inst.options.renderer.disabledClass) + '">' +
				(date ? date.formatDate(inst.options[command.text]) : inst.options[command.text]) +
				'</' + close + '>');
		};
		for (var name in inst.options.commands) {
			addCommand('button', 'button type="button"', 'button', name,
				inst.options.renderer.commandButtonClass);
			addCommand('link', 'a href="javascript:void(0)"', 'a', name,
				inst.options.renderer.commandLinkClass);
		}
		picker = $(picker);
		if (monthsToShow[1] > 1) {
			var count = 0;
			$(inst.options.renderer.monthSelector, picker).each(function() {
				var nth = ++count % monthsToShow[1];
				$(this).addClass(nth == 1 ? 'first' : (nth == 0 ? 'last' : ''));
			});
		}
		// Add calendar behaviour
		var self = this;
		picker.find(inst.options.renderer.daySelector + ' a').hover(
				function() { $(this).addClass(inst.options.renderer.highlightedClass); },
				function() {
					(inst.inline ? $(this).parents('.' + self.markerClassName) : inst.div).
						find(inst.options.renderer.daySelector + ' a').
						removeClass(inst.options.renderer.highlightedClass);
				}).
			click(function() {
				self._selectDatePlugin(target, this);
			}).end().
			find('select.' + this._monthYearClass + ':not(.' + this._anyYearClass + ')').change(function() {
				var monthYear = $(this).val().split('/');
				self._showMonthPlugin(target, parseInt(monthYear[1], 10), parseInt(monthYear[0], 10));
			}).end().
			find('select.' + this._anyYearClass).click(function() {
				$(this).next('input').css({left: this.offsetLeft, top: this.offsetTop,
					width: this.offsetWidth, height: this.offsetHeight}).show().focus();
			}).end().
			find('input.' + self._monthYearClass).change(function() {
				try {
					var year = parseInt($(this).val(), 10);
					year = (isNaN(year) ? inst.drawDate.year() : year);
					self._showMonthPlugin(target, year, inst.drawDate.month(), inst.drawDate.day());
				}
				catch (e) {
					alert(e);
				}
			}).keydown(function(event) {
				if (event.keyCode == 27) { // Escape
					$(event.target).hide();
					inst.target.focus();
				}
			});
		// Add command behaviour
		picker.find('.' + inst.options.renderer.commandClass).click(function() {
				if (!$(this).hasClass(inst.options.renderer.disabledClass)) {
					var action = this.className.replace(
						new RegExp('^.*' + inst.options.renderer.commandClass + '-([^ ]+).*$'), '$1');
					plugin._performActionPlugin(target, action);
				}
			});
		// Add classes
		if (inst.options.isRTL) {
			picker.addClass(inst.options.renderer.rtlClass);
		}
		if (monthsToShow[0] * monthsToShow[1] > 1) {
			picker.addClass(inst.options.renderer.multiClass);
		}
		if (inst.options.pickerClass) {
			picker.addClass(inst.options.pickerClass);
		}
		// Resize
		$('body').append(picker);
		var width = 0;
		picker.find(inst.options.renderer.monthSelector).each(function() {
			width += $(this).outerWidth();
		});
		picker.width(width / monthsToShow[0]);
		// Pre-show customisation
		if ($.isFunction(inst.options.onShow)) {
			inst.options.onShow.apply(target, [picker, inst.options.calendar, inst]);
		}
		return picker;
	},

	/* Generate the content for a single month.
	   @param  target    (element) the control to affect
	   @param  inst      (object) the current instance settings
	   @param  year      (number) the year to generate
	   @param  month     (number) the month to generate
	   @param  calendar  (*Calendar) the current calendar
	   @param  renderer  (object) the rendering templates
	   @param  first     (boolean) true if first of multiple months
	   @return  (string) the month content */
	_generateMonth: function(target, inst, year, month, calendar, renderer, first) {
		var daysInMonth = calendar.daysInMonth(year, month);
		var monthsToShow = inst.options.monthsToShow;
		monthsToShow = ($.isArray(monthsToShow) ? monthsToShow : [1, monthsToShow]);
		var fixedWeeks = inst.options.fixedWeeks || (monthsToShow[0] * monthsToShow[1] > 1);
		var firstDay = inst.options.firstDay;
		firstDay = (firstDay == null ? calendar.local.firstDay : firstDay);
		var leadDays = (calendar.dayOfWeek(year, month, calendar.minDay) -
			firstDay + calendar.daysInWeek()) % calendar.daysInWeek();
		var numWeeks = (fixedWeeks ? 6 : Math.ceil((leadDays + daysInMonth) / calendar.daysInWeek()));
		var selectOtherMonths = inst.options.selectOtherMonths && inst.options.showOtherMonths;
		var minDate = (inst.pickingRange ? inst.selectedDates[0] : inst.get('minDate'));
		var maxDate = inst.get('maxDate');
		var showWeeks = renderer.week.indexOf('{weekOfYear}') > -1;
		var today = calendar.today();
		var drawDate = calendar.newDate(year, month, calendar.minDay);
		drawDate.add(-leadDays - (fixedWeeks &&
			(drawDate.dayOfWeek() == firstDay || drawDate.daysInMonth() < calendar.daysInWeek())?
			calendar.daysInWeek() : 0), 'd');
		var jd = drawDate.toJD();
		// Generate weeks
		var weeks = '';
		for (var week = 0; week < numWeeks; week++) {
			var weekOfYear = (!showWeeks ? '' : '<span class="jd' + jd + '">' +
				($.isFunction(inst.options.calculateWeek) ?
				inst.options.calculateWeek(drawDate) : drawDate.weekOfYear()) + '</span>');
			var days = '';
			for (var day = 0; day < calendar.daysInWeek(); day++) {
				var selected = false;
				if (inst.options.rangeSelect && inst.selectedDates.length > 0) {
					selected = (drawDate.compareTo(inst.selectedDates[0]) != -1 &&
						drawDate.compareTo(inst.selectedDates[1]) != +1);
				}
				else {
					for (var i = 0; i < inst.selectedDates.length; i++) {
						if (inst.selectedDates[i].compareTo(drawDate) == 0) {
							selected = true;
							break;
						}
					}
				}
				var dateInfo = (!$.isFunction(inst.options.onDate) ? {} :
					inst.options.onDate.apply(target, [drawDate, drawDate.month() == month]));
				var selectable = (selectOtherMonths || drawDate.month() == month) &&
					this._isSelectable(target, drawDate, dateInfo.selectable, minDate, maxDate);
				days += this._prepare(renderer.day, inst).replace(/\{day\}/g,
					(selectable ? '<a href="javascript:void(0)"' : '<span') +
					' class="jd' + jd + ' ' + (dateInfo.dateClass || '') +
					(selected && (selectOtherMonths || drawDate.month() == month) ?
					' ' + renderer.selectedClass : '') +
					(selectable ? ' ' + renderer.defaultClass : '') +
					(drawDate.weekDay() ? '' : ' ' + renderer.weekendClass) +
					(drawDate.month() == month ? '' : ' ' + renderer.otherMonthClass) +
					(drawDate.compareTo(today) == 0 && drawDate.month() == month ?
					' ' + renderer.todayClass : '') +
					(drawDate.compareTo(inst.drawDate) == 0 && drawDate.month() == month ?
					' ' + renderer.highlightedClass : '') + '"' +
					(dateInfo.title || (inst.options.dayStatus && selectable) ? ' title="' +
					(dateInfo.title || drawDate.formatDate(inst.options.dayStatus)) + '"' : '') + '>' +
					(inst.options.showOtherMonths || drawDate.month() == month ?
					dateInfo.content || drawDate.day() : '&nbsp;') +
					(selectable ? '</a>' : '</span>'));
				drawDate.add(1, 'd');
				jd++;
			}
			weeks += this._prepare(renderer.week, inst).replace(/\{days\}/g, days).
				replace(/\{weekOfYear\}/g, weekOfYear);
		}
		var monthHeader = this._prepare(renderer.month, inst).match(/\{monthHeader(:[^\}]+)?\}/);
		monthHeader = (monthHeader[0].length <= 13 ? 'MM yyyy' :
			monthHeader[0].substring(13, monthHeader[0].length - 1));
		monthHeader = (first ? this._generateMonthSelection(
			inst, year, month, minDate, maxDate, monthHeader, calendar, renderer) :
			calendar.formatDate(monthHeader, calendar.newDate(year, month, calendar.minDay)));
		var weekHeader = this._prepare(renderer.weekHeader, inst).
			replace(/\{days\}/g, this._generateDayHeaders(inst, calendar, renderer));
		return this._prepare(renderer.month, inst).replace(/\{monthHeader(:[^\}]+)?\}/g, monthHeader).
			replace(/\{weekHeader\}/g, weekHeader).replace(/\{weeks\}/g, weeks);
	},

	/* Generate the HTML for the day headers.
	   @param  inst      (object) the current instance settings
	   @param  calendar  (*Calendar) the current calendar
	   @param  renderer  (object) the rendering templates
	   @return  (string) a week's worth of day headers */
	_generateDayHeaders: function(inst, calendar, renderer) {
		var firstDay = inst.options.firstDay;
		firstDay = (firstDay == null ? calendar.local.firstDay : firstDay);
		var header = '';
		for (var day = 0; day < calendar.daysInWeek(); day++) {
			var dow = (day + firstDay) % calendar.daysInWeek();
			header += this._prepare(renderer.dayHeader, inst).replace(/\{day\}/g,
				'<span class="' + this._curDoWClass + dow + '" title="' +
				calendar.local.dayNames[dow] + '">' +
				calendar.local.dayNamesMin[dow] + '</span>');
		}
		return header;
	},

	/* Generate selection controls for month.
	   @param  inst         (object) the current instance settings
	   @param  year         (number) the year to generate
	   @param  month        (number) the month to generate
	   @param  minDate      (CDate) the minimum date allowed
	   @param  maxDate      (CDate) the maximum date allowed
	   @param  monthHeader  (string) the month/year format
	   @param  calendar     (*Calendar) the current calendar
	   @return  (string) the month selection content */
	_generateMonthSelection: function(inst, year, month, minDate, maxDate, monthHeader, calendar) {
		if (!inst.options.changeMonth) {
			return calendar.formatDate(monthHeader, calendar.newDate(year, month, 1));
		}
		// Months
		var monthNames = calendar.local[
			'monthNames' + (monthHeader.match(/mm/i) ? '' : 'Short')];
		var html = monthHeader.replace(/m+/i, '\\x2E').replace(/y+/i, '\\x2F');
		var selector = '<select class="' + this._monthYearClass +
			'" title="' + inst.options.monthStatus + '">';
		var maxMonth = calendar.monthsInYear(year) + calendar.minMonth;
		for (var m = calendar.minMonth; m < maxMonth; m++) {
			if ((!minDate || calendar.newDate(year, m,
					calendar.daysInMonth(year, m) - 1 + calendar.minDay).
					compareTo(minDate) != -1) &&
					(!maxDate || calendar.newDate(year, m, calendar.minDay).
					compareTo(maxDate) != +1)) {
				selector += '<option value="' + m + '/' + year + '"' +
					(month == m ? ' selected="selected"' : '') + '>' +
					monthNames[m - calendar.minMonth] + '</option>';
			}
		}
		selector += '</select>';
		html = html.replace(/\\x2E/, selector);
		// Years
		var yearRange = inst.options.yearRange;
		if (yearRange == 'any') {
			selector = '<select class="' + this._monthYearClass + ' ' + this._anyYearClass +
				'" title="' + inst.options.yearStatus + '">' +
				'<option>' + year + '</option></select>' +
				'<input class="' + this._monthYearClass + ' ' + this._curMonthClass +
				month + '" value="' + year + '">';
		}
		else {
			yearRange = yearRange.split(':');
			var todayYear = calendar.today().year();
			var start = (yearRange[0].match('c[+-].*') ? year + parseInt(yearRange[0].substring(1), 10) :
				((yearRange[0].match('[+-].*') ? todayYear : 0) + parseInt(yearRange[0], 10)));
			var end = (yearRange[1].match('c[+-].*') ? year + parseInt(yearRange[1].substring(1), 10) :
				((yearRange[1].match('[+-].*') ? todayYear : 0) + parseInt(yearRange[1], 10)));
			selector = '<select class="' + this._monthYearClass +
				'" title="' + inst.options.yearStatus + '">';
			start = calendar.newDate(start + 1, calendar.firstMonth, calendar.minDay).add(-1, 'd');
			end = calendar.newDate(end, calendar.firstMonth, calendar.minDay);
			var addYear = function(y) {
				if (y != 0 || calendar.hasYearZero) {
					selector += '<option value="' +
						Math.min(month, calendar.monthsInYear(y) - 1 + calendar.minMonth) +
						'/' + y + '"' + (year == y ? ' selected="selected"' : '') + '>' +
						y + '</option>';
				}
			};
			if (start.toJD() < end.toJD()) {
				start = (minDate && minDate.compareTo(start) == +1 ? minDate : start).year();
				end = (maxDate && maxDate.compareTo(end) == -1 ? maxDate : end).year();
				for (var y = start; y <= end; y++) {
					addYear(y);
				}
			}
			else {
				start = (maxDate && maxDate.compareTo(start) == -1 ? maxDate : start).year();
				end = (minDate && minDate.compareTo(end) == +1 ? minDate : end).year();
				for (var y = start; y >= end; y--) {
					addYear(y);
				}
			}
			selector += '</select>';
		}
		html = html.replace(/\\x2F/, selector);
		return html;
	},

	/* Prepare a render template for use.
	   Exclude popup/inline sections that are not applicable.
	   Localise text of the form: {l10n:name}.
	   @param  text  (string) the text to localise
	   @param  inst  (object) the current instance settings
	   @return  (string) the localised text */
	_prepare: function(text, inst) {
		var replaceSection = function(type, retain) {
			while (true) {
				var start = text.indexOf('{' + type + ':start}');
				if (start == -1) {
					return;
				}
				var end = text.substring(start).indexOf('{' + type + ':end}');
				if (end > -1) {
					text = text.substring(0, start) +
						(retain ? text.substr(start + type.length + 8, end - type.length - 8) : '') +
						text.substring(start + end + type.length + 6);
				}
			}
		};
		replaceSection('inline', inst.inline);
		replaceSection('popup', !inst.inline);
		var pattern = /\{l10n:([^\}]+)\}/;
		var matches = null;
		while (matches = pattern.exec(text)) {
			text = text.replace(matches[0], inst.options[matches[1]]);
		}
		return text;
	}
});

// The list of commands that return values and don't permit chaining
var getters = ['getDate', 'isDisabled', 'isSelectable', 'retrieveDate'];

/* Determine whether a command is a getter and doesn't permit chaining.
   @param  command    (string, optional) the command to run
   @param  otherArgs  ([], optional) any other arguments for the command
   @return  true if the command is a getter, false if not */
function isNotChained(command, otherArgs) {
	if (command == 'option' && (otherArgs.length == 0 ||
			(otherArgs.length == 1 && typeof otherArgs[0] == 'string'))) {
		return true;
	}
	return $.inArray(command, getters) > -1;
}

/* Attach the calendar picker functionality to a jQuery selection.
   @param  options  (object) the new settings to use for these instances (optional) or
                    (string) the command to run (optional)
   @return  (jQuery) for chaining further calls or
            (any) getter value */
$.fn.calendarsPicker = function(options) {
	var otherArgs = Array.prototype.slice.call(arguments, 1);
	if (isNotChained(options, otherArgs)) {
		return plugin['_' + options + 'Plugin'].apply(plugin, [this[0]].concat(otherArgs));
	}
	return this.each(function() {
		if (typeof options == 'string') {
			if (!plugin['_' + options + 'Plugin']) {
				throw 'Unknown command: ' + options;
			}
			plugin['_' + options + 'Plugin'].apply(plugin, [this].concat(otherArgs));
		}
		else {
			plugin._attachPlugin(this, options || {});
		}
	});
};

/* Initialise the calendar picker functionality. */
var plugin = $.calendars.picker = new CalendarsPicker(); // Singleton instance

$(function() {
	$(document).mousedown(plugin._checkExternalClick).
		resize(function() { plugin._hidePlugin(plugin.curInst); });
});

})(jQuery);
