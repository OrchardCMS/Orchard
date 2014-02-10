/* http://keith-wood.name/calendars.html
   Calendars extras for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

$.extend($.calendars.regional[''], {
	invalidArguments: 'Invalid arguments',
	invalidFormat: 'Cannot format a date from another calendar',
	missingNumberAt: 'Missing number at position {0}',
	unknownNameAt: 'Unknown name at position {0}',
	unexpectedLiteralAt: 'Unexpected literal at position {0}',
	unexpectedText: 'Additional text found at end'
});
$.calendars.local = $.calendars.regional[''];

$.extend($.calendars.cdate.prototype, {

	/* Format this date.
	   @param  format  (string) the date format to use (see BaseCalendar.formatDate) (optional)
	   @return  (string) the formatted date */
	formatDate: function(format) {
		return this._calendar.formatDate(format || '', this);
	}
});

$.extend($.calendars.baseCalendar.prototype, {

	UNIX_EPOCH: $.calendars.instance().newDate(1970, 1, 1).toJD(),
	SECS_PER_DAY: 24 * 60 * 60,
	TICKS_EPOCH: $.calendars.instance().jdEpoch, // 1 January 0001 CE
	TICKS_PER_DAY: 24 * 60 * 60 * 10000000,

	ATOM: 'yyyy-mm-dd', // RFC 3339/ISO 8601
	COOKIE: 'D, dd M yyyy',
	FULL: 'DD, MM d, yyyy',
	ISO_8601: 'yyyy-mm-dd',
	JULIAN: 'J',
	RFC_822: 'D, d M yy',
	RFC_850: 'DD, dd-M-yy',
	RFC_1036: 'D, d M yy',
	RFC_1123: 'D, d M yyyy',
	RFC_2822: 'D, d M yyyy',
	RSS: 'D, d M yy', // RFC 822
	TICKS: '!',
	TIMESTAMP: '@',
	W3C: 'yyyy-mm-dd', // ISO 8601

	/* Format a date object into a string value.
	   The format can be combinations of the following:
	   d  - day of month (no leading zero)
	   dd - day of month (two digit)
	   o  - day of year (no leading zeros)
	   oo - day of year (three digit)
	   D  - day name short
	   DD - day name long
	   w  - week of year (no leading zero)
	   ww - week of year (two digit)
	   m  - month of year (no leading zero)
	   mm - month of year (two digit)
	   M  - month name short
	   MM - month name long
	   yy - year (two digit)
	   yyyy - year (four digit)
	   YYYY - formatted year
	   J  - Julian date (days since January 1, 4713 BCE Greenwich noon)
	   @  - Unix timestamp (s since 01/01/1970)
	   !  - Windows ticks (100ns since 01/01/0001)
	   '...' - literal text
	   '' - single quote
	   @param  format    (string) the desired format of the date (optional, default calendar format)
	   @param  date      (CDate) the date value to format
	   @param  settings  (object) attributes include:
	                     dayNamesShort    (string[]) abbreviated names of the days from Sunday (optional)
	                     dayNames         (string[]) names of the days from Sunday (optional)
	                     monthNamesShort  (string[]) abbreviated names of the months (optional)
	                     monthNames       (string[]) names of the months (optional)
						 calculateWeek    (function) function that determines week of the year (optional)
	   @return  (string) the date in the above format
	   @throws  errors if the date is from a different calendar */
	formatDate: function(format, date, settings) {
		if (typeof format != 'string') {
			settings = date;
			date = format;
			format = '';
		}
		if (!date) {
			return '';
		}
		if (date.calendar() != this) {
			throw $.calendars.local.invalidFormat || $.calendars.regional[''].invalidFormat;
		}
		format = format || this.local.dateFormat;
		settings = settings || {};
		var dayNamesShort = settings.dayNamesShort || this.local.dayNamesShort;
		var dayNames = settings.dayNames || this.local.dayNames;
		var monthNamesShort = settings.monthNamesShort || this.local.monthNamesShort;
		var monthNames = settings.monthNames || this.local.monthNames;
		var calculateWeek = settings.calculateWeek || this.local.calculateWeek;
		// Check whether a format character is doubled
		var doubled = function(match, step) {
			var matches = 1;
			while (iFormat + matches < format.length && format.charAt(iFormat + matches) == match) {
				matches++;
			}
			iFormat += matches - 1;
			return Math.floor(matches / (step || 1)) > 1;
		};
		// Format a number, with leading zeroes if necessary
		var formatNumber = function(match, value, len, step) {
			var num = '' + value;
			if (doubled(match, step)) {
				while (num.length < len) {
					num = '0' + num;
				}
			}
			return num;
		};
		// Format a name, short or long as requested
		var formatName = function(match, value, shortNames, longNames) {
			return (doubled(match) ? longNames[value] : shortNames[value]);
		};
		var output = '';
		var literal = false;
		for (var iFormat = 0; iFormat < format.length; iFormat++) {
			if (literal) {
				if (format.charAt(iFormat) == "'" && !doubled("'")) {
					literal = false;
				}
				else {
					output += format.charAt(iFormat);
				}
			}
			else {
				switch (format.charAt(iFormat)) {
					case 'd': output += formatNumber('d', date.day(), 2); break;
					case 'D': output += formatName('D', date.dayOfWeek(),
						dayNamesShort, dayNames); break;
					case 'o': output += formatNumber('o', date.dayOfYear(), 3); break;
					case 'w': output += formatNumber('w', date.weekOfYear(), 2); break;
					case 'm': output += formatNumber('m', date.month(), 2); break;
					case 'M': output += formatName('M', date.month() - this.minMonth,
						monthNamesShort, monthNames); break;
					case 'y':
						output += (doubled('y', 2) ? date.year() :
							(date.year() % 100 < 10 ? '0' : '') + date.year() % 100);
						break;
					case 'Y':
						doubled('Y', 2);
						output += date.formatYear();
						break;
					case 'J': output += date.toJD(); break;
					case '@': output += (date.toJD() - this.UNIX_EPOCH) * this.SECS_PER_DAY; break;
					case '!': output += (date.toJD() - this.TICKS_EPOCH) * this.TICKS_PER_DAY; break;
					case "'":
						if (doubled("'")) {
							output += "'";
						}
						else {
							literal = true;
						}
						break;
					default:
						output += format.charAt(iFormat);
				}
			}
		}
		return output;
	},

	/* Parse a string value into a date object.
	   See formatDate for the possible formats, plus:
	   * - ignore rest of string
	   @param  format    (string) the expected format of the date ('' for default calendar format)
	   @param  value     (string) the date in the above format
	   @param  settings  (object) attributes include:
	                     shortYearCutoff  (number) the cutoff year for determining the century (optional)
	                     dayNamesShort    (string[]) abbreviated names of the days from Sunday (optional)
	                     dayNames         (string[]) names of the days from Sunday (optional)
	                     monthNamesShort  (string[]) abbreviated names of the months (optional)
	                     monthNames       (string[]) names of the months (optional)
	   @return  (CDate) the extracted date value or null if value is blank
	   @throws  errors if the format and/or value are missing,
	            if the value doesn't match the format,
	            or if the date is invalid */
	parseDate: function(format, value, settings) {
		if (value == null) {
			throw $.calendars.local.invalidArguments || $.calendars.regional[''].invalidArguments;
		}
		value = (typeof value == 'object' ? value.toString() : value + '');
		if (value == '') {
			return null;
		}
		format = format || this.local.dateFormat;
		settings = settings || {};
		var shortYearCutoff = settings.shortYearCutoff || this.shortYearCutoff;
		shortYearCutoff = (typeof shortYearCutoff != 'string' ? shortYearCutoff :
			this.today().year() % 100 + parseInt(shortYearCutoff, 10));
		var dayNamesShort = settings.dayNamesShort || this.local.dayNamesShort;
		var dayNames = settings.dayNames || this.local.dayNames;
		var monthNamesShort = settings.monthNamesShort || this.local.monthNamesShort;
		var monthNames = settings.monthNames || this.local.monthNames;
		var jd = -1;
		var year = -1;
		var month = -1;
		var day = -1;
		var doy = -1;
		var shortYear = false;
		var literal = false;
		// Check whether a format character is doubled
		var doubled = function(match, step) {
			var matches = 1;
			while (iFormat + matches < format.length && format.charAt(iFormat + matches) == match) {
				matches++;
			}
			iFormat += matches - 1;
			return Math.floor(matches / (step || 1)) > 1;
		};
		// Extract a number from the string value
		var getNumber = function(match, step) {
			var isDoubled = doubled(match, step);
			var size = [2, 3, isDoubled ? 4 : 2, isDoubled ? 4 : 2, 10, 11, 20]['oyYJ@!'.indexOf(match) + 1];
			var digits = new RegExp('^-?\\d{1,' + size + '}');
			var num = value.substring(iValue).match(digits);
			if (!num) {
				throw ($.calendars.local.missingNumberAt || $.calendars.regional[''].missingNumberAt).
					replace(/\{0\}/, iValue);
			}
			iValue += num[0].length;
			return parseInt(num[0], 10);
		};
		// Extract a name from the string value and convert to an index
		var calendar = this;
		var getName = function(match, shortNames, longNames, step) {
			var names = (doubled(match, step) ? longNames : shortNames);
			for (var i = 0; i < names.length; i++) {
				if (value.substr(iValue, names[i].length).toLowerCase() == names[i].toLowerCase()) {
					iValue += names[i].length;
					return i + calendar.minMonth;
				}
			}
			throw ($.calendars.local.unknownNameAt || $.calendars.regional[''].unknownNameAt).
				replace(/\{0\}/, iValue);
		};
		// Confirm that a literal character matches the string value
		var checkLiteral = function() {
			if (value.charAt(iValue) != format.charAt(iFormat)) {
				throw ($.calendars.local.unexpectedLiteralAt ||
					$.calendars.regional[''].unexpectedLiteralAt).replace(/\{0\}/, iValue);
			}
			iValue++;
		};
		var iValue = 0;
		for (var iFormat = 0; iFormat < format.length; iFormat++) {
			if (literal) {
				if (format.charAt(iFormat) == "'" && !doubled("'")) {
					literal = false;
				}
				else {
					checkLiteral();
				}
			}
			else {
				switch (format.charAt(iFormat)) {
					case 'd': day = getNumber('d'); break;
					case 'D': getName('D', dayNamesShort, dayNames); break;
					case 'o': doy = getNumber('o'); break;
					case 'w': getNumber('w'); break;
					case 'm': month = getNumber('m'); break;
					case 'M': month = getName('M', monthNamesShort, monthNames); break;
					case 'y':
						var iSave = iFormat;
						shortYear = !doubled('y', 2);
						iFormat = iSave;
						year = getNumber('y', 2);
						break;
					case 'Y': year = getNumber('Y', 2); break;
					case 'J':
						jd = getNumber('J') + 0.5;
						if (value.charAt(iValue) == '.') {
							iValue++;
							getNumber('J');
						}
						break;
					case '@': jd = getNumber('@') / this.SECS_PER_DAY + this.UNIX_EPOCH; break;
					case '!': jd = getNumber('!') / this.TICKS_PER_DAY + this.TICKS_EPOCH; break;
					case '*': iValue = value.length; break;
					case "'":
						if (doubled("'")) {
							checkLiteral();
						}
						else {
							literal = true;
						}
						break;
					default: checkLiteral();
				}
			}
		}
		if (iValue < value.length) {
			throw $.calendars.local.unexpectedText || $.calendars.regional[''].unexpectedText;
		}
		if (year == -1) {
			year = this.today().year();
		}
		else if (year < 100 && shortYear) {
			year += (shortYearCutoff == -1 ? 1900 : this.today().year() -
				this.today().year() % 100 - (year <= shortYearCutoff ? 0 : 100));
		}
		if (doy > -1) {
			month = 1;
			day = doy;
			for (var dim = this.daysInMonth(year, month); day > dim; dim = this.daysInMonth(year, month)) {
				month++;
				day -= dim;
			}
		}
		return (jd > -1 ? this.fromJD(jd) : this.newDate(year, month, day));
	},

	/* A date may be specified as an exact value or a relative one.
	   @param  dateSpec     (CDate or number or string) the date as an object or string
	                        in the given format or an offset - numeric days from today,
	                        or string amounts and periods, e.g. '+1m +2w'
	   @param  defaultDate  (CDate) the date to use if no other supplied, may be null
	   @param  currentDate  (CDate) the current date as a possible basis for relative dates,
	                        if null today is used (optional)
	   @param  dateFormat   (string) the expected date format - see formatDate above (optional)
	   @param  settings     (object) attributes include:
	                        shortYearCutoff  (number) the cutoff year for determining the century (optional)
	                        dayNamesShort    (string[7]) abbreviated names of the days from Sunday (optional)
	                        dayNames         (string[7]) names of the days from Sunday (optional)
	                        monthNamesShort  (string[12]) abbreviated names of the months (optional)
	                        monthNames       (string[12]) names of the months (optional)
	   @return  (CDate) the decoded date */
	determineDate: function(dateSpec, defaultDate, currentDate, dateFormat, settings) {
		if (currentDate && typeof currentDate != 'object') {
			settings = dateFormat;
			dateFormat = currentDate;
			currentDate = null;
		}
		if (typeof dateFormat != 'string') {
			settings = dateFormat;
			dateFormat = '';
		}
		var calendar = this;
		var offsetString = function(offset) {
			try {
				return calendar.parseDate(dateFormat, offset, settings);
			}
			catch (e) {
				// Ignore
			}
			offset = offset.toLowerCase();
			var date = (offset.match(/^c/) && currentDate ?
				currentDate.newDate() : null) || calendar.today();
			var pattern = /([+-]?[0-9]+)\s*(d|w|m|y)?/g;
			var matches = pattern.exec(offset);
			while (matches) {
				date.add(parseInt(matches[1], 10), matches[2] || 'd');
				matches = pattern.exec(offset);
			}
			return date;
		};
		defaultDate = (defaultDate ? defaultDate.newDate() : null);
		dateSpec = (dateSpec == null ? defaultDate :
			(typeof dateSpec == 'string' ? offsetString(dateSpec) : (typeof dateSpec == 'number' ?
			(isNaN(dateSpec) || dateSpec == Infinity || dateSpec == -Infinity ? defaultDate :
			calendar.today().add(dateSpec, 'd')) : calendar.newDate(dateSpec))));
		return dateSpec;
	}
});

})(jQuery);
