/* http://keith-wood.name/timeEntry.html
   Time entry for jQuery v1.5.2.
   Written by Keith Wood (kbwood{at}iinet.com.au) June 2007.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license.
   Please attribute the author if you use it. */

/* Turn an input field into an entry point for a time value.
   The time can be entered via directly typing the value,
   via the arrow keys, or via spinner buttons.
   It is configurable to show 12 or 24-hour time, to show or hide seconds,
   to enforce a minimum and/or maximum time, to change the spinner image,
   and to constrain the time to steps, e.g. only on the quarter hours.
   Attach it with $('input selector').timeEntry(); for default settings,
   or configure it with options like:
   $('input selector').timeEntry(
      {spinnerImage: 'spinnerSquare.png', spinnerSize: [20, 20, 0]}); */

(function($) { // Hide scope, no $ conflict

/* TimeEntry manager.
   Use the singleton instance of this class, $.timeEntry, to interact with the time entry
   functionality. Settings for (groups of) fields are maintained in an instance object,
   allowing multiple different settings on the same page. */
function TimeEntry() {
	this._disabledInputs = []; // List of time entry inputs that have been disabled
	this.regional = []; // Available regional settings, indexed by language code
	this.regional[''] = { // Default regional settings
		show24Hours: false, // True to use 24 hour time, false for 12 hour (AM/PM)
		separator: ':', // The separator between time fields
		ampmPrefix: '', // The separator before the AM/PM text
		ampmNames: ['AM', 'PM'], // Names of morning/evening markers
		spinnerTexts: ['Now', 'Previous field', 'Next field', 'Increment', 'Decrement']
		// The popup texts for the spinner image areas
	};
	this._defaults = {
		appendText: '', // Display text following the input box, e.g. showing the format
		showSeconds: false, // True to show seconds as well, false for hours/minutes only
		timeSteps: [1, 1, 1], // Steps for each of hours/minutes/seconds when incrementing/decrementing
		initialField: 0, // The field to highlight initially, 0 = hours, 1 = minutes, ...
		noSeparatorEntry: false, // True to move to next sub-field after two digits entry
		useMouseWheel: true, // True to use mouse wheel for increment/decrement if possible,
			// false to never use it
		defaultTime: null, // The time to use if none has been set, leave at null for now
		minTime: null, // The earliest selectable time, or null for no limit
		maxTime: null, // The latest selectable time, or null for no limit
		spinnerImage: 'spinnerDefault.png', // The URL of the images to use for the time spinner
			// Seven images packed horizontally for normal, each button pressed, and disabled
		spinnerSize: [20, 20, 8], // The width and height of the spinner image,
			// and size of centre button for current time
		spinnerBigImage: '', // The URL of the images to use for the expanded time spinner
			// Seven images packed horizontally for normal, each button pressed, and disabled
		spinnerBigSize: [40, 40, 16], // The width and height of the expanded spinner image,
			// and size of centre button for current time
		spinnerIncDecOnly: false, // True for increment/decrement buttons only, false for all
		spinnerRepeat: [500, 250], // Initial and subsequent waits in milliseconds
			// for repeats on the spinner buttons
		beforeShow: null, // Function that takes an input field and
			// returns a set of custom settings for the time entry
		beforeSetTime: null // Function that runs before updating the time,
			// takes the old and new times, and minimum and maximum times as parameters,
			// and returns an adjusted time if necessary
	};
	$.extend(this._defaults, this.regional['']);
}

$.extend(TimeEntry.prototype, {
	/* Class name added to elements to indicate already configured with time entry. */
	markerClassName: 'hasTimeEntry',
	/* Name of the data property for instance settings. */
	propertyName: 'timeEntry',

	/* Class name for the appended content. */
	_appendClass: 'timeEntry_append',
	/* Class name for the time entry control. */
	_controlClass: 'timeEntry_control',
	/* Class name for the expanded spinner. */
	_expandClass: 'timeEntry_expand',

	/* Override the default settings for all instances of the time entry.
	   @param  options  (object) the new settings to use as defaults (anonymous object)
	   @return  (DateEntry) this object */
	setDefaults: function(options) {
		$.extend(this._defaults, options || {});
		return this;
	},

	/* Attach the time entry handler to an input field.
	   @param  target   (element) the field to attach to
	   @param  options  (object) custom settings for this instance */
	_attachPlugin: function(target, options) {
		var input = $(target);
		if (input.hasClass(this.markerClassName)) {
			return;
		}
		var inst = {options: $.extend({}, this._defaults, options), input: input, _field: 0,
			_selectedHour: 0, _selectedMinute: 0, _selectedSecond: 0};
		input.data(this.propertyName, inst).addClass(this.markerClassName).
			bind('focus.' + this.propertyName, this._doFocus).
			bind('blur.' + this.propertyName, this._doBlur).
			bind('click.' + this.propertyName, this._doClick).
			bind('keydown.' + this.propertyName, this._doKeyDown).
			bind('keypress.' + this.propertyName, this._doKeyPress).
			bind('paste.' + this.propertyName, function(event) { // Check pastes
				setTimeout(function() { plugin._parseTime(inst); }, 1);
			});
		this._optionPlugin(target, options);
	},

	/* Retrieve or reconfigure the settings for a time entry control.
	   @param  target   (element) the control to affect
	   @param  options  (object) the new options for this instance or
	                    (string) an individual property name
	   @param  value    (any) the individual property value (omit if options
	                    is an object or to retrieve the value of a setting)
	   @return  (any) if retrieving a value  */
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
		var currentTime = this._extractTime(inst);
		$.extend(inst.options, options);
		inst._field = 0;
		if (currentTime) {
			this._setTime(inst, new Date(0, 0, 0, currentTime[0], currentTime[1], currentTime[2]));
		}
		// Remove stuff dependent on old settings
		target.next('span.' + this._appendClass).remove();
		target.parent().find('span.' + this._controlClass).remove();
		if ($.fn.mousewheel) {
			target.unmousewheel();
		}
		// And re-add if requested
		var spinner = (!inst.options.spinnerImage ? null :
			$('<span class="' + this._controlClass + '" style="display: inline-block; ' +
			'background: url(\'' + inst.options.spinnerImage + '\') 0 0 no-repeat; width: ' +
			inst.options.spinnerSize[0] + 'px; height: ' + inst.options.spinnerSize[1] + 'px;"></span>'));
		target.after(inst.options.appendText ? '<span class="' + this._appendClass + '">' +
			inst.options.appendText + '</span>' : '').after(spinner || '');
		// Allow mouse wheel usage
		if (inst.options.useMouseWheel && $.fn.mousewheel) {
			target.mousewheel(this._doMouseWheel);
		}
		if (spinner) {
			spinner.mousedown(this._handleSpinner).mouseup(this._endSpinner).
				mouseover(this._expandSpinner).mouseout(this._endSpinner).
				mousemove(this._describeSpinner);
		}
	},

	/* Enable a time entry input and any associated spinner.
	   @param  target  (element) single input field */
	_enablePlugin: function(target) {
		this._enableDisable(target, false);
	},

	/* Disable a time entry input and any associated spinner.
	   @param  target  (element) single input field */
	_disablePlugin: function(target) {
		this._enableDisable(target, true);
	},

	/* Enable or disable a time entry input and any associated spinner.
	   @param  target   (element) single input field
	   @param  disable  (boolean) true to disable, false to enable */
	_enableDisable: function(target, disable) {
		var inst = $.data(target, this.propertyName);
		if (!inst) {
			return;
		}
		target.disabled = disable;
		if (target.nextSibling && target.nextSibling.nodeName.toLowerCase() == 'span') {
			plugin._changeSpinner(inst, target.nextSibling, (disable ? 5 : -1));
		}
		plugin._disabledInputs = $.map(plugin._disabledInputs,
			function(value) { return (value == target ? null : value); }); // Delete entry
		if (disable) {
			plugin._disabledInputs.push(target);
		}
	},

	/* Check whether an input field has been disabled.
	   @param  target  (element) input field to check
	   @return  (boolean) true if this field has been disabled, false if it is enabled */
	_isDisabledPlugin: function(target) {
		return $.inArray(target, this._disabledInputs) > -1;
	},

	/* Remove the time entry functionality from an input.
	   @param  target  (element) the control to affect */
	_destroyPlugin: function(target) {
		target = $(target);
		if (!target.hasClass(this.markerClassName)) {
			return;
		}
		target.removeClass(this.markerClassName).removeData(this.propertyName).
			unbind('.' + this.propertyName);
		if ($.fn.mousewheel) {
			target.unmousewheel();
		}
		this._disabledInputs = $.map(this._disabledInputs,
			function(value) { return (value == target[0] ? null : value); }); // Delete entry
		target.siblings('.' + this._appendClass + ',.' + this._controlClass).remove();
	},

	/* Initialise the current time for a time entry input field.
	   @param  target  (element) input field to update
	   @param  time    (Date) the new time (year/month/day ignored) or null for now */
	_setTimePlugin: function(target, time) {
		var inst = $.data(target, this.propertyName);
		if (inst) {
			if (time === null || time === '') {
				inst.input.val('');
			}
			else {
				this._setTime(inst, time ? (typeof time == 'object' ?
					new Date(time.getTime()) : time) : null);
			}
		}
	},

	/* Retrieve the current time for a time entry input field.
	   @param  target  (element) input field to examine
	   @return  (Date) current time (year/month/day zero) or null if none */
	_getTimePlugin: function(target) {
		var inst = $.data(target, this.propertyName);
		var currentTime = (inst ? this._extractTime(inst) : null);
		return (!currentTime ? null :
			new Date(0, 0, 0, currentTime[0], currentTime[1], currentTime[2]));
	},

	/* Retrieve the millisecond offset for the current time.
	   @param  target  (element) input field to examine
	   @return  (number) the time as milliseconds offset or zero if none */
	_getOffsetPlugin: function(target) {
		var inst = $.data(target, this.propertyName);
		var currentTime = (inst ? this._extractTime(inst) : null);
		return (!currentTime ? 0 :
			(currentTime[0] * 3600 + currentTime[1] * 60 + currentTime[2]) * 1000);
	},

	/* Initialise time entry.
	   @param  target  (element) the input field or
	                   (event) the focus event */
	_doFocus: function(target) {
		var input = (target.nodeName && target.nodeName.toLowerCase() == 'input' ? target : this);
		if (plugin._lastInput == input || plugin._isDisabledPlugin(input)) {
			plugin._focussed = false;
			return;
		}
		var inst = $.data(input, plugin.propertyName);
		plugin._focussed = true;
		plugin._lastInput = input;
		plugin._blurredInput = null;
		$.extend(inst.options, ($.isFunction(inst.options.beforeShow) ?
			inst.options.beforeShow.apply(input, [input]) : {}));
		plugin._parseTime(inst);
		setTimeout(function() { plugin._showField(inst); }, 10);
	},

	/* Note that the field has been exited.
	   @param  event  (event) the blur event */
	_doBlur: function(event) {
		plugin._blurredInput = plugin._lastInput;
		plugin._lastInput = null;
	},

	/* Select appropriate field portion on click, if already in the field.
	   @param  event  (event) the click event */
	_doClick: function(event) {
		var input = event.target;
		var inst = $.data(input, plugin.propertyName);
		var prevField = inst._field;
		if (!plugin._focussed) {
			var fieldSize = inst.options.separator.length + 2;
			inst._field = 0;
			if (input.selectionStart != null) { // Use input select range
				for (var field = 0; field <= Math.max(1, inst._secondField, inst._ampmField); field++) {
					var end = (field != inst._ampmField ? (field * fieldSize) + 2 :
						(inst._ampmField * fieldSize) + inst.options.ampmPrefix.length +
						inst.options.ampmNames[0].length);
					inst._field = field;
					if (input.selectionStart < end) {
						break;
					}
				}
			}
			else if (input.createTextRange) { // Check against bounding boxes
				var src = $(event.srcElement);
				var range = input.createTextRange();
				var convert = function(value) {
					return {thin: 2, medium: 4, thick: 6}[value] || value;
				};
				var offsetX = event.clientX + document.documentElement.scrollLeft -
					(src.offset().left + parseInt(convert(src.css('border-left-width')), 10)) -
					range.offsetLeft; // Position - left edge - alignment
				for (var field = 0; field <= Math.max(1, inst._secondField, inst._ampmField); field++) {
					var end = (field != inst._ampmField ? (field * fieldSize) + 2 :
						(inst._ampmField * fieldSize) + inst.options.ampmPrefix.length +
						inst.options.ampmNames[0].length);
					range.collapse();
					range.moveEnd('character', end);
					inst._field = field;
					if (offsetX < range.boundingWidth) { // And compare
						break;
					}
				}
			}
		}
		if (prevField != inst._field) {
			inst._lastChr = '';
		}
		plugin._showField(inst);
		plugin._focussed = false;
	},

	/* Handle keystrokes in the field.
	   @param  event  (event) the keydown event
	   @return  (boolean) true to continue, false to stop processing */
	_doKeyDown: function(event) {
		if (event.keyCode >= 48) { // >= '0'
			return true;
		}
		var inst = $.data(event.target, plugin.propertyName);
		switch (event.keyCode) {
			case 9: return (event.shiftKey ?
						// Move to previous time field, or out if at the beginning
						plugin._changeField(inst, -1, true) :
						// Move to next time field, or out if at the end
						plugin._changeField(inst, +1, true));
			case 35: if (event.ctrlKey) { // Clear time on ctrl+end
						plugin._setValue(inst, '');
					}
					else { // Last field on end
						inst._field = Math.max(1, inst._secondField, inst._ampmField);
						plugin._adjustField(inst, 0);
					}
					break;
			case 36: if (event.ctrlKey) { // Current time on ctrl+home
						plugin._setTime(inst);
					}
					else { // First field on home
						inst._field = 0;
						plugin._adjustField(inst, 0);
					}
					break;
			case 37: plugin._changeField(inst, -1, false); break; // Previous field on left
			case 38: plugin._adjustField(inst, +1); break; // Increment time field on up
			case 39: plugin._changeField(inst, +1, false); break; // Next field on right
			case 40: plugin._adjustField(inst, -1); break; // Decrement time field on down
			case 46: plugin._setValue(inst, ''); break; // Clear time on delete
			default: return true;
		}
		return false;
	},

	/* Disallow unwanted characters.
	   @param  event  (event) the keypress event
	   @return  (boolean) true to continue, false to stop processing */
	_doKeyPress: function(event) {
		var chr = String.fromCharCode(event.charCode == undefined ? event.keyCode : event.charCode);
		if (chr < ' ') {
			return true;
		}
		var inst = $.data(event.target, plugin.propertyName);
		plugin._handleKeyPress(inst, chr);
		return false;
	},

	/* Increment/decrement on mouse wheel activity.
	   @param  event  (event) the mouse wheel event
	   @param  delta  (number) the amount of change */
	_doMouseWheel: function(event, delta) {
		if (plugin._isDisabledPlugin(event.target)) {
			return;
		}
		var inst = $.data(event.target, plugin.propertyName);
		inst.input.focus();
		if (!inst.input.val()) {
			plugin._parseTime(inst);
		}
		plugin._adjustField(inst, delta);
		event.preventDefault();
	},

	/* Expand the spinner, if possible, to make it easier to use.
	   @param  event  (event) the mouse over event */
	_expandSpinner: function(event) {
		var spinner = plugin._getSpinnerTarget(event);
		var inst = $.data(plugin._getInput(spinner), plugin.propertyName);
		if (plugin._isDisabledPlugin(inst.input[0])) {
			return;
		}
		if (inst.options.spinnerBigImage) {
			inst._expanded = true;
			var offset = $(spinner).offset();
			var relative = null;
			$(spinner).parents().each(function() {
				var parent = $(this);
				if (parent.css('position') == 'relative' ||
						parent.css('position') == 'absolute') {
					relative = parent.offset();
				}
				return !relative;
			});
			$('<div class="' + plugin._expandClass + '" style="position: absolute; left: ' +
				(offset.left - (inst.options.spinnerBigSize[0] - inst.options.spinnerSize[0]) / 2 -
				(relative ? relative.left : 0)) + 'px; top: ' +
				(offset.top - (inst.options.spinnerBigSize[1] - inst.options.spinnerSize[1]) / 2 -
				(relative ? relative.top : 0)) + 'px; width: ' +
				inst.options.spinnerBigSize[0] + 'px; height: ' +
				inst.options.spinnerBigSize[1] + 'px; background: transparent url(' +
				inst.options.spinnerBigImage + ') no-repeat 0px 0px; z-index: 10;"></div>').
				mousedown(plugin._handleSpinner).mouseup(plugin._endSpinner).
				mouseout(plugin._endExpand).mousemove(plugin._describeSpinner).
				insertAfter(spinner);
		}
	},

	/* Locate the actual input field from the spinner.
	   @param  spinner  (element) the current spinner
	   @return  (element) the corresponding input */
	_getInput: function(spinner) {
		return $(spinner).siblings('.' + plugin.markerClassName)[0];
	},

	/* Change the title based on position within the spinner.
	   @param  event  (event) the mouse move event */
	_describeSpinner: function(event) {
		var spinner = plugin._getSpinnerTarget(event);
		var inst = $.data(plugin._getInput(spinner), plugin.propertyName);
		spinner.title = inst.options.spinnerTexts[plugin._getSpinnerRegion(inst, event)];
	},

	/* Handle a click on the spinner.
	   @param  event  (event) the mouse click event */
	_handleSpinner: function(event) {
		var spinner = plugin._getSpinnerTarget(event);
		var input = plugin._getInput(spinner);
		if (plugin._isDisabledPlugin(input)) {
			return;
		}
		if (input == plugin._blurredInput) {
			plugin._lastInput = input;
			plugin._blurredInput = null;
		}
		var inst = $.data(input, plugin.propertyName);
		plugin._doFocus(input);
		var region = plugin._getSpinnerRegion(inst, event);
		plugin._changeSpinner(inst, spinner, region);
		plugin._actionSpinner(inst, region);
		plugin._timer = null;
		plugin._handlingSpinner = true;
		if (region >= 3 && inst.options.spinnerRepeat[0]) { // Repeat increment/decrement
			plugin._timer = setTimeout(
				function() { plugin._repeatSpinner(inst, region); },
				inst.options.spinnerRepeat[0]);
			$(spinner).one('mouseout', plugin._releaseSpinner).
				one('mouseup', plugin._releaseSpinner);
		}
	},

	/* Action a click on the spinner.
	   @param  inst    (object) the instance settings
	   @param  region  (number) the spinner "button" */
	_actionSpinner: function(inst, region) {
		if (!inst.input.val()) {
			plugin._parseTime(inst);
		}
		switch (region) {
			case 0: this._setTime(inst); break;
			case 1: this._changeField(inst, -1, false); break;
			case 2: this._changeField(inst, +1, false); break;
			case 3: this._adjustField(inst, +1); break;
			case 4: this._adjustField(inst, -1); break;
		}
	},

	/* Repeat a click on the spinner.
	   @param  inst    (object) the instance settings
	   @param  region  (number) the spinner "button" */
	_repeatSpinner: function(inst, region) {
		if (!plugin._timer) {
			return;
		}
		plugin._lastInput = plugin._blurredInput;
		this._actionSpinner(inst, region);
		this._timer = setTimeout(
			function() { plugin._repeatSpinner(inst, region); },
			inst.options.spinnerRepeat[1]);
	},

	/* Stop a spinner repeat.
	   @param  event  (event) the mouse event */
	_releaseSpinner: function(event) {
		clearTimeout(plugin._timer);
		plugin._timer = null;
	},

	/* Tidy up after an expanded spinner.
	   @param  event  (event) the mouse event */
	_endExpand: function(event) {
		plugin._timer = null;
		var spinner = plugin._getSpinnerTarget(event);
		var input = plugin._getInput(spinner);
		var inst = $.data(input, plugin.propertyName);
		$(spinner).remove();
		inst._expanded = false;
	},

	/* Tidy up after a spinner click.
	   @param  event  (event) the mouse event */
	_endSpinner: function(event) {
		plugin._timer = null;
		var spinner = plugin._getSpinnerTarget(event);
		var input = plugin._getInput(spinner);
		var inst = $.data(input, plugin.propertyName);
		if (!plugin._isDisabledPlugin(input)) {
			plugin._changeSpinner(inst, spinner, -1);
		}
		if (plugin._handlingSpinner) {
			plugin._lastInput = plugin._blurredInput;
		}
		if (plugin._lastInput && plugin._handlingSpinner) {
			plugin._showField(inst);
		}
		plugin._handlingSpinner = false;
	},

	/* Retrieve the spinner from the event.
	   @param  event  (event) the mouse click event
	   @return  (element) the target field */
	_getSpinnerTarget: function(event) {
		return event.target || event.srcElement;
	},

	/* Determine which "button" within the spinner was clicked.
	   @param  inst   (object) the instance settings
	   @param  event  (event) the mouse event
	   @return  (number) the spinner "button" number */
	_getSpinnerRegion: function(inst, event) {
		var spinner = this._getSpinnerTarget(event);
		var pos = $(spinner).offset();
		var scrolled = [document.documentElement.scrollLeft || document.body.scrollLeft,
			document.documentElement.scrollTop || document.body.scrollTop];
		var left = (inst.options.spinnerIncDecOnly ? 99 : event.clientX + scrolled[0] - pos.left);
		var top = event.clientY + scrolled[1] - pos.top;
		var spinnerSize = inst.options[inst._expanded ? 'spinnerBigSize' : 'spinnerSize'];
		var right = (inst.options.spinnerIncDecOnly ? 99 : spinnerSize[0] - 1 - left);
		var bottom = spinnerSize[1] - 1 - top;
		if (spinnerSize[2] > 0 && Math.abs(left - right) <= spinnerSize[2] &&
				Math.abs(top - bottom) <= spinnerSize[2]) {
			return 0; // Centre button
		}
		var min = Math.min(left, top, right, bottom);
		return (min == left ? 1 : (min == right ? 2 : (min == top ? 3 : 4))); // Nearest edge
	},

	/* Change the spinner image depending on button clicked.
	   @param  inst     (object) the instance settings
	   @param  spinner  (element) the spinner control
	   @param  region   (number) the spinner "button" */
	_changeSpinner: function(inst, spinner, region) {
		$(spinner).css('background-position', '-' + ((region + 1) *
			inst.options[inst._expanded ? 'spinnerBigSize' : 'spinnerSize'][0]) + 'px 0px');
	},

	/* Extract the time value from the input field, or default to now.
	   @param  inst  (object) the instance settings */
	_parseTime: function(inst) {
		var currentTime = this._extractTime(inst);
		if (currentTime) {
			inst._selectedHour = currentTime[0];
			inst._selectedMinute = currentTime[1];
			inst._selectedSecond = currentTime[2];
		}
		else {
			var now = this._constrainTime(inst);
			inst._selectedHour = now[0];
			inst._selectedMinute = now[1];
			inst._selectedSecond = (inst.options.showSeconds ? now[2] : 0);
		}
		inst._secondField = (inst.options.showSeconds ? 2 : -1);
		inst._ampmField = (inst.options.show24Hours ? -1 : (inst.options.showSeconds ? 3 : 2));
		inst._lastChr = '';
		inst._field = Math.max(0, Math.min(
			Math.max(1, inst._secondField, inst._ampmField), inst.options.initialField));
		if (inst.input.val() != '') {
			this._showTime(inst);
		}
	},

	/* Extract the time value from a string as an array of values, or default to null.
	   @param  inst   (object) the instance settings
	   @param  value  (string) the time value to parse
	   @return  (number[3]) the time components (hours, minutes, seconds)
	            or null if no value */
	_extractTime: function(inst, value) {
		value = value || inst.input.val();
		var currentTime = value.split(inst.options.separator);
		if (inst.options.separator == '' && value != '') {
			currentTime[0] = value.substring(0, 2);
			currentTime[1] = value.substring(2, 4);
			currentTime[2] = value.substring(4, 6);
		}
		if (currentTime.length >= 2) {
			var isAM = !inst.options.show24Hours && (value.indexOf(inst.options.ampmNames[0]) > -1);
			var isPM = !inst.options.show24Hours && (value.indexOf(inst.options.ampmNames[1]) > -1);
			var hour = parseInt(currentTime[0], 10);
			hour = (isNaN(hour) ? 0 : hour);
			hour = ((isAM || isPM) && hour == 12 ? 0 : hour) + (isPM ? 12 : 0);
			var minute = parseInt(currentTime[1], 10);
			minute = (isNaN(minute) ? 0 : minute);
			var second = (currentTime.length >= 3 ?
				parseInt(currentTime[2], 10) : 0);
			second = (isNaN(second) || !inst.options.showSeconds ? 0 : second);
			return this._constrainTime(inst, [hour, minute, second]);
		} 
		return null;
	},

	/* Constrain the given/current time to the time steps.
	   @param  inst    (object) the instance settings
	   @param  fields  (number[3]) the current time components (hours, minutes, seconds)
	   @return  (number[3]) the constrained time components (hours, minutes, seconds) */
	_constrainTime: function(inst, fields) {
		var specified = (fields != null);
		if (!specified) {
			var now = this._determineTime(inst.options.defaultTime, inst) || new Date();
			fields = [now.getHours(), now.getMinutes(), now.getSeconds()];
		}
		var reset = false;
		for (var i = 0; i < inst.options.timeSteps.length; i++) {
			if (reset) {
				fields[i] = 0;
			}
			else if (inst.options.timeSteps[i] > 1) {
				fields[i] = Math.round(fields[i] / inst.options.timeSteps[i]) *
					inst.options.timeSteps[i];
				reset = true;
			}
		}
		return fields;
	},

	/* Set the selected time into the input field.
	   @param  inst  (object) the instance settings */
	_showTime: function(inst) {
		var currentTime = (this._formatNumber(inst.options.show24Hours ? inst._selectedHour :
			((inst._selectedHour + 11) % 12) + 1) + inst.options.separator +
			this._formatNumber(inst._selectedMinute) +
			(inst.options.showSeconds ? inst.options.separator +
			this._formatNumber(inst._selectedSecond) : '') +
			(inst.options.show24Hours ?  '' : inst.options.ampmPrefix +
			inst.options.ampmNames[(inst._selectedHour < 12 ? 0 : 1)]));
		this._setValue(inst, currentTime);
		this._showField(inst);
	},

	/* Highlight the current time field.
	   @param  inst  (object) the instance settings */
	_showField: function(inst) {
		var input = inst.input[0];
		if (inst.input.is(':hidden') || plugin._lastInput != input) {
			return;
		}
		var fieldSize = inst.options.separator.length + 2;
		var start = (inst._field != inst._ampmField ? (inst._field * fieldSize) :
			(inst._ampmField * fieldSize) - inst.options.separator.length +
			inst.options.ampmPrefix.length);
		var end = start + (inst._field != inst._ampmField ? 2 : inst.options.ampmNames[0].length);
		if (input.setSelectionRange) { // Mozilla
			input.setSelectionRange(start, end);
		}
		else if (input.createTextRange) { // IE
			var range = input.createTextRange();
			range.moveStart('character', start);
			range.moveEnd('character', end - inst.input.val().length);
			range.select();
		}
		if (!input.disabled) {
			input.focus();
		}
	},

	/* Ensure displayed single number has a leading zero.
	   @param  value  (number) current value
	   @return  (string) number with at least two digits */
	_formatNumber: function(value) {
		return (value < 10 ? '0' : '') + value;
	},

	/* Update the input field and notify listeners.
	   @param  inst   (object) the instance settings
	   @param  value  (string) the new value */
	_setValue: function(inst, value) {
		if (value != inst.input.val()) {
			inst.input.val(value).trigger('change');
		}
	},

	/* Move to previous/next field, or out of field altogether if appropriate.
	   @param  inst     (object) the instance settings
	   @param  offset   (number) the direction of change (-1, +1)
	   @param  moveOut  (boolean) true if can move out of the field
	   @return  (boolean) true if exitting the field, false if not */
	_changeField: function(inst, offset, moveOut) {
		var atFirstLast = (inst.input.val() == '' || inst._field ==
			(offset == -1 ? 0 : Math.max(1, inst._secondField, inst._ampmField)));
		if (!atFirstLast) {
			inst._field += offset;
		}
		this._showField(inst);
		inst._lastChr = '';
		return (atFirstLast && moveOut);
	},

	/* Update the current field in the direction indicated.
	   @param  inst    (object) the instance settings
	   @param  offset  (number) the amount to change by */
	_adjustField: function(inst, offset) {
		if (inst.input.val() == '') {
			offset = 0;
		}
		this._setTime(inst, new Date(0, 0, 0,
			inst._selectedHour + (inst._field == 0 ? offset * inst.options.timeSteps[0] : 0) +
			(inst._field == inst._ampmField ? offset * 12 : 0),
			inst._selectedMinute + (inst._field == 1 ? offset * inst.options.timeSteps[1] : 0),
			inst._selectedSecond +
			(inst._field == inst._secondField ? offset * inst.options.timeSteps[2] : 0)));
	},

	/* Check against minimum/maximum and display time.
	   @param  inst  (object) the instance settings
	   @param  time  (Date) an actual time or
	                 (number) offset in seconds from now or
					 (string) units and periods of offsets from now */
	_setTime: function(inst, time) {
		time = this._determineTime(time, inst);
		var fields = this._constrainTime(inst, time ?
			[time.getHours(), time.getMinutes(), time.getSeconds()] : null);
		time = new Date(0, 0, 0, fields[0], fields[1], fields[2]);
		// Normalise to base date
		var time = this._normaliseTime(time);
		var minTime = this._normaliseTime(this._determineTime(inst.options.minTime, inst));
		var maxTime = this._normaliseTime(this._determineTime(inst.options.maxTime, inst));
		// Ensure it is within the bounds set
		if (minTime && maxTime && minTime > maxTime) {
			if (time < minTime && time > maxTime) {
				time = (Math.abs(time - minTime) < Math.abs(time - maxTime) ? minTime : maxTime);
			}
		}
		else {
			time = (minTime && time < minTime ? minTime :
				(maxTime && time > maxTime ? maxTime : time));
		}
		// Perform further restrictions if required
		if ($.isFunction(inst.options.beforeSetTime)) {
			time = inst.options.beforeSetTime.apply(inst.input[0],
				[this._getTimePlugin(inst.input[0]), time, minTime, maxTime]);
		}
		inst._selectedHour = time.getHours();
		inst._selectedMinute = time.getMinutes();
		inst._selectedSecond = time.getSeconds();
		this._showTime(inst);
	},

	/* A time may be specified as an exact value or a relative one.
	   @param  setting  (Date) an actual time or
	                    (number) offset in seconds from now or
	                    (string) units and periods of offsets from now
	   @param  inst     (object) the instance settings
	   @return  (Date) the calculated time */
	_determineTime: function(setting, inst) {
		var offsetNumeric = function(offset) { // E.g. +300, -2
			var time = new Date();
			time.setTime(time.getTime() + offset * 1000);
			return time;
		};
		var offsetString = function(offset) { // E.g. '+2m', '-4h', '+3h +30m' or '12:34:56PM'
			var fields = plugin._extractTime(inst, offset); // Actual time?
			var time = new Date();
			var hour = (fields ? fields[0] : time.getHours());
			var minute = (fields ? fields[1] : time.getMinutes());
			var second = (fields ? fields[2] : time.getSeconds());
			if (!fields) {
				var pattern = /([+-]?[0-9]+)\s*(s|S|m|M|h|H)?/g;
				var matches = pattern.exec(offset);
				while (matches) {
					switch (matches[2] || 's') {
						case 's' : case 'S' :
							second += parseInt(matches[1], 10); break;
						case 'm' : case 'M' :
							minute += parseInt(matches[1], 10); break;
						case 'h' : case 'H' :
							hour += parseInt(matches[1], 10); break;
					}
					matches = pattern.exec(offset);
				}
			}
			time = new Date(0, 0, 10, hour, minute, second, 0);
			if (/^!/.test(offset)) { // No wrapping
				if (time.getDate() > 10) {
					time = new Date(0, 0, 10, 23, 59, 59);
				}
				else if (time.getDate() < 10) {
					time = new Date(0, 0, 10, 0, 0, 0);
				}
			}
			return time;
		};
		return (setting ? (typeof setting == 'string' ? offsetString(setting) :
			(typeof setting == 'number' ? offsetNumeric(setting) : setting)) : null);
	},

	/* Normalise time object to a common date.
	   @param  time  (Date) the original time
	   @return  (Date) the normalised time */
	_normaliseTime: function(time) {
		if (!time) {
			return null;
		}
		time.setFullYear(1900);
		time.setMonth(0);
		time.setDate(0);
		return time;
	},

	/* Update time based on keystroke entered.
	   @param  inst  (object) the instance settings
	   @param  chr   (ch) the new character */
	_handleKeyPress: function(inst, chr) {
		if (chr == inst.options.separator) {
			this._changeField(inst, +1, false);
		}
		else if (chr >= '0' && chr <= '9') { // Allow direct entry of time
			var key = parseInt(chr, 10);
			var value = parseInt(inst._lastChr + chr, 10);
			var hour = (inst._field != 0 ? inst._selectedHour :
				(inst.options.show24Hours ? (value < 24 ? value : key) :
				(value >= 1 && value <= 12 ? value :
				(key > 0 ? key : inst._selectedHour)) % 12 +
				(inst._selectedHour >= 12 ? 12 : 0)));
			var minute = (inst._field != 1 ? inst._selectedMinute :
				(value < 60 ? value : key));
			var second = (inst._field != inst._secondField ? inst._selectedSecond :
				(value < 60 ? value : key));
			var fields = this._constrainTime(inst, [hour, minute, second]);
			this._setTime(inst, new Date(0, 0, 0, fields[0], fields[1], fields[2]));
			if (inst.options.noSeparatorEntry && inst._lastChr) {
				this._changeField(inst, +1, false);
			}
			else {
				inst._lastChr = chr;
			}
		}
		else if (!inst.options.show24Hours) { // Set am/pm based on first char of names
			chr = chr.toLowerCase();
			if ((chr == inst.options.ampmNames[0].substring(0, 1).toLowerCase() &&
					inst._selectedHour >= 12) ||
					(chr == inst.options.ampmNames[1].substring(0, 1).toLowerCase() &&
					inst._selectedHour < 12)) {
				var saveField = inst._field;
				inst._field = inst._ampmField;
				this._adjustField(inst, +1);
				inst._field = saveField;
				this._showField(inst);
			}
		}
	}
});

// The list of commands that return values and don't permit chaining
var getters = ['getOffset', 'getTime', 'isDisabled'];

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

/* Attach the time entry functionality to a jQuery selection.
   @param  options  (object) the new settings to use for these instances (optional) or
                    (string) the command to run (optional)
   @return  (jQuery) for chaining further calls or
            (any) getter value */
$.fn.timeEntry = function(options) {
	var otherArgs = Array.prototype.slice.call(arguments, 1);
	if (isNotChained(options, otherArgs)) {
		return plugin['_' + options + 'Plugin'].
			apply(plugin, [this[0]].concat(otherArgs));
	}
	return this.each(function() {
		if (typeof options == 'string') {
			if (!plugin['_' + options + 'Plugin']) {
				throw 'Unknown command: ' + options;
			}
			plugin['_' + options + 'Plugin'].
				apply(plugin, [this].concat(otherArgs));
		}
		else {
			// Check for settings on the control itself
			var inlineSettings = ($.fn.metadata ? $(this).metadata() : {});
			plugin._attachPlugin(this, $.extend({}, inlineSettings, options || {}));
		}
	});
};

/* Initialise the time entry functionality. */
var plugin = $.timeEntry = new TimeEntry(); // Singleton instance

})(jQuery);
