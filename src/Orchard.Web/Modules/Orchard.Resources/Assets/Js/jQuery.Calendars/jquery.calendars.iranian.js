/* http://keith-wood.name/calendars.html
   Iranian calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) March 2025.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	/** Implementation of the Iranian or Solar Hijri calendar.
		Based on code from <a href="https://www.jqueryscript.net/demo/Persian-Jalali-Calendar-Data-Picker-Plugin-With-jQuery-kamaDatepicker/">https://www.jqueryscript.net/demo/Persian-Jalali-Calendar-Data-Picker-Plugin-With-jQuery-kamaDatepicker/</a>
		and information from <a href="https://www.time.ir/">https://www.time.ir/</a>.
		See also <a href="https://en.wikipedia.org/wiki/Iranian_calendars">https://en.wikipedia.org/wiki/Iranian_calendars</a>
		and <a href="https://en.wikipedia.org/wiki/Solar_Hijri_calendar">https://en.wikipedia.org/wiki/Solar_Hijri_calendar</a>.
		@class IranianCalendar
		@param {string} [language=''] The language code (default English) for localisation. */
	function IranianCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	IranianCalendar.prototype = new $.calendars.baseCalendar();

	$.extend(IranianCalendar.prototype, {
		/** The calendar name.
			@memberof IranianCalendar */
		name: 'Iranian',
		/** Julian date of start of Iranian epoch: 19 March 622 CE.
			@memberof IranianCalendar */
		jdEpoch: 1948320.5,
		/** Days per month in a common year.
			@memberof IranianCalendar */
		daysPerMonth: [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof IranianCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof IranianCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof IranianCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof IranianCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof IranianCalendar
			@property {string} name The calendar name.
			@property {string[]} epochs The epoch names (before/after year 0).
			@property {string[]} monthNames The long names of the months of the year.
			@property {string[]} monthNamesShort The short names of the months of the year.
			@property {string[]} dayNames The long names of the days of the week.
			@property {string[]} dayNamesShort The short names of the days of the week.
			@property {string[]} dayNamesMin The minimal names of the days of the week.
			@property {string} dateFormat The date format for this calendar.
					See the options on <a href="BaseCalendar.html#formatDate"><code>formatDate</code></a> for details.
			@property {number} firstDay The number of the first day of the week, starting at 0.
			@property {boolean} isRTL <code>true</code> if this localisation reads right-to-left. */
		regionalOptions: { // Localisations
			'': {
				name: 'Iranian',
				epochs: ['BSH', 'SH'],
				monthNames: ['Farvardin', 'Ordibehesht', 'Khordad', 'Tir', 'Mordad', 'Shahrivar',
				'Mehr', 'Aban', 'Azar', 'Dey', 'Bahman', 'Esfand'],
				monthNamesShort: ['Far', 'Ord', 'Kho', 'Tir', 'Mor', 'Sha', 'Meh', 'Aba', 'Aza', 'Dey', 'Bah', 'Esf'],
				dayNames: ['Yekshanbeh', 'Doshanbeh', 'Seshanbeh', 'Chahārshanbeh', 'Panjshanbeh', 'Jom\'eh', 'Shanbeh'],
				dayNamesShort: ['Yek', 'Do', 'Se', 'Cha', 'Panj', 'Jom', 'Sha'],
				dayNamesMin: ['Ye','Do','Se','Ch','Pa','Jo','Sh'],
				digits: null,
				dateFormat: 'yyyy/mm/dd',
				firstDay: 6,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof IranianCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return this._yearInfo(date.year()).leap === 0;
		},

		/** Determine the week of the year for a date.
			@memberof IranianCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Saturday of this week starting on Saturday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(-((checkDate.dayOfWeek() + 1) % 7), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a month.
			@memberof IranianCalendar
			@param {CDate|number} year The date to examine or the year of the month.
			@param {number} [month] The month (if only <code>year</code> specified above).
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return this.daysPerMonth[date.month() - 1] +
				(date.month() === 12 && this.leapYear(date.year()) ? 1 : 0);
		},

		/** Determine whether this date is a week day.
			@memberof IranianCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return this.dayOfWeek(year, month, day) !== 5;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof IranianCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if only <code>year</code> specified above).
			@param {number} [day] The day to convert (if only <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			var info = this._yearInfo(date.year());
			return this._g2d(info.gy, 3, info.march) + 31 * (date.month() - 1) - div(date.month(), 7) * (date.month() - 7) + date.day() - 1;
		},

		/** Create a new date from a Julian date.
			@memberof IranianCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			var day, month;
			var gy = this._d2gy(jd);
			var year = gy - 621;
			if (year <= 0) { year--; } // No year zero
			var info = this._yearInfo(year);
			var diff = jd - this._g2d(gy, 3, info.march);
			if (diff >= 0) {
				if (diff <= 185) {
					month = div(diff, 31) + 1;
					day = mod(diff, 31) + 1;
					return this.newDate(year, month, day);
				}
				diff -= 186;
			} else {
				year--;
				if (year === 0) { year--; } // No year zero
				diff += 179;
				if (info.leap === 1) {
					diff++;
				}
			}
			month = div(diff, 30) + 7;
			day = mod(diff, 30) + 1;
			return this.newDate(year, month, day);
		},

		/** Determine whether a date is valid for this calendar.
			@memberof IranianCalendar
			@param {number} year The year to examine.
			@param {number} month The month to examine.
			@param {number} day The day to examine.
			@return {boolean} <code>true</code> if a valid date, <code>false</code> if not. */
		isValid: function(year, month, day) {
			var valid = $.calendars.baseCalendar.prototype.isValid.apply(this, arguments);
			if (valid) {
				year = (typeof year.year === 'function' ? year.year() : year);
				valid = (year >= special[0] && year < special[last]);
			}
			return valid;
		},

		/** Check that a candidate date is from the same calendar and is valid.
			@memberof IranianCalendar
			@private
			@param {CDate|number} year The date to validate or the year to validate.
			@param {number} month The month to validate (if only <code>year</code> specified above).
			@param {number} day The day to validate (if only <code>year</code> specified above).
			@param {string} error Error message if invalid.
			@return {CDate} The validated date.
			@throws Error if different calendars used or invalid date. */
		_validate: function(year, month, day, error) {
			var date = $.calendars.baseCalendar.prototype._validate.apply(this, arguments);
			if (date.year() < special[0] || date.year() >= special[last]) {
				throw error.replace(/\{0\}/, this.local.name);
			}
			return date;
		},
		
		/** Convert from Julian date to Gregorian year.
			@memberof IranianCalendar
			@private
			@param {number} jd The Julian date to convert.
			@return {number} The corresponding Gregorian year. */
		_d2gy: function(jd) {
			var i = 4 * (jd + 0.5) + 139361631 + 4 * div(3 * div(4 * (jd + 0.5) + 183187720, 146097), 4) - 3908;
			var j = 5 * div(mod(i, 1461), 4) + 308;
			var gm = mod(div(j, 153), 12) + 1;
			return div(i, 1461) - 100100 + div(8 - gm, 6);
		},

		/** Convert from Gregorian details to Julian date.
			@memberof IranianCalendar
			@private
			@param {number} year The year to validate.
			@param {number} month The month to validate.
			@param {number} day The day to validate.
			@return {number} The corresponding Julian date. */
		_g2d: function(year, month, day) {
			var i = div(1461 * (year + div(month - 8, 6) + 100100), 4) +
				div(153 * mod(month + 9, 12) + 2, 5) + day - 34840408;
			return i - div(3 * div(year + 100100 + div(month - 8, 6), 100), 4) + 752 - 0.5;
		},

		/** Provide information about a given year.
			@memberof IranianCalendar
			@private
			@param {CDate|number} year The year of interest.
			@return {{gy, leap, march}} Year details. */
		_yearInfo: function(year) {
			year = year.year ? year.year() : year;
			var specDiff;
			if (year < 0) { year++; } // No year zero
			var gy = year + 621;
			var m = -14;
			var prevSpec = special[0];
			for (var i = 1; i <= last && (specDiff = special[i] - prevSpec, special[i] <= year); i++) {
				m += 8 * div(specDiff, 33) + div(mod(specDiff, 33), 4);
				prevSpec = special[i];
			}
			var offset = year - prevSpec;
			m += 8 * div(offset, 33) + div(mod(offset, 33) + 3, 4);
			if (mod(specDiff, 33) === 4 && specDiff - offset === 4) {
				m++;
			}
			var r = div(gy, 4) - div(3 * (div(gy, 100) + 1), 4) - 150;
			var march = 20 + m - r;
			if (specDiff - offset < 6) {
				offset = offset - specDiff + 33 * div(specDiff + 4, 33);
			}
			var leap = mod(mod(offset + 1, 33) - 1, 4);
			leap = (leap === -1 ? 4 : leap);
			return { gy: gy, leap: leap, march: march };
		}
	});

	function div(a, b) {
		return ~~(a / b);
	}
	function mod(a, b) {
		return a - ~~(a / b) * b;
	}
	
	var special = [-61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181,
		1210, 1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178];
	var last = special.length - 1;

	// Iranian calendar implementation
	$.calendars.calendars.iranian = IranianCalendar;

})(jQuery);