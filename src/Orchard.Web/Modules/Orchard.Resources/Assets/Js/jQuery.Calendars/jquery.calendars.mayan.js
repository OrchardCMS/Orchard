/* http://keith-wood.name/calendars.html
   Mayan calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	/** Implementation of the Mayan Long Count calendar.
		See also <a href="http://en.wikipedia.org/wiki/Mayan_calendar">http://en.wikipedia.org/wiki/Mayan_calendar</a>.
		@class MayanCalendar
		@param {string} [language=''] The language code (default English) for localisation. */
	function MayanCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	MayanCalendar.prototype = new $.calendars.baseCalendar();

	$.extend(MayanCalendar.prototype, {
		/** The calendar name.
			@memberof MayanCalendar */
		name: 'Mayan',
		/** Julian date of start of Mayan epoch: 11 August 3114 BCE.
			@memberof MayanCalendar */
		jdEpoch: 584282.5,
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof MayanCalendar */
		hasYearZero: true,
		/** The minimum month number.
			@memberof MayanCalendar */
		minMonth: 0,
		/** The first month in the year.
			@memberof MayanCalendar */
		firstMonth: 0,
		/** The minimum day number.
			@memberof MayanCalendar */
		minDay: 0,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof MayanCalendar
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
			@property {boolean} isRTL <code>true</code> if this localisation reads right-to-left.
			@property {string[]} haabMonths The names of the Haab months.
			@property {string[]} tzolkinMonths The names of the Tzolkin months. */
		regionalOptions: { // Localisations
			'': {
				name: 'Mayan',
				epochs: ['', ''],
				monthNames: ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'10', '11', '12', '13', '14', '15', '16', '17'],
				monthNamesShort: ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'10', '11', '12', '13', '14', '15', '16', '17'],
				dayNames: ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'10', '11', '12', '13', '14', '15', '16', '17', '18', '19'],
				dayNamesShort: ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'10', '11', '12', '13', '14', '15', '16', '17', '18', '19'],
				dayNamesMin: ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'10', '11', '12', '13', '14', '15', '16', '17', '18', '19'],
				digits: null,
				dateFormat: 'YYYY.m.d',
				firstDay: 0,
				isRTL: false,
				haabMonths: ['Pop', 'Uo', 'Zip', 'Zotz', 'Tzec', 'Xul', 'Yaxkin', 'Mol', 'Chen', 'Yax',
				'Zac', 'Ceh', 'Mac', 'Kankin', 'Muan', 'Pax', 'Kayab', 'Cumku', 'Uayeb'],
				tzolkinMonths: ['Imix', 'Ik', 'Akbal', 'Kan', 'Chicchan', 'Cimi', 'Manik', 'Lamat', 'Muluc', 'Oc',
				'Chuen', 'Eb', 'Ben', 'Ix', 'Men', 'Cib', 'Caban', 'Etznab', 'Cauac', 'Ahau']
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return false;
		},

		/** Format the year, if not a simple sequential number.
			@memberof MayanCalendar
			@param {CDate|number} year The date to format or the year to format.
			@return {string} The formatted year.
			@throws Error if an invalid year or a different calendar used. */
		formatYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			year = date.year();
			var baktun = Math.floor(year / 400);
			year = year % 400;
			year += (year < 0 ? 400 : 0);
			var katun = Math.floor(year / 20);
			return baktun + '.' + katun + '.' + (year % 20);
		},

		/** Convert from the formatted year back to a single number.
			@memberof MayanCalendar
			@param {string} years The year as n.n.n.
			@return {number} The sequential year.
			@throws Error if an invalid value is supplied. */
		forYear: function(years) {
			years = years.split('.');
			if (years.length < 3) {
				throw 'Invalid Mayan year';
			}
			var year = 0;
			for (var i = 0; i < years.length; i++) {
				var y = parseInt(years[i], 10);
				if (Math.abs(y) > 19 || (i > 0 && y < 0)) {
					throw 'Invalid Mayan year';
				}
				year = year * 20 + y;
			}
			return year;
		},

		/** Retrieve the number of months in a year.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {number} The number of months.
			@throws Error if an invalid year or a different calendar used. */
		monthsInYear: function(year) {
			this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return 18;
		},

		/** Determine the week of the year for a date.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			this._validate(year, month, day, $.calendars.local.invalidDate);
			return 0;
		},

		/** Retrieve the number of days in a year.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {number} The number of days.
			@throws Error if an invalid year or a different calendar used. */
		daysInYear: function(year) {
			this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return 360;
		},

		/** Retrieve the number of days in a month.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year of the month.
			@param {number} [month] The month (if only <code>year</code> specified above).
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return 20;
		},

		/** Retrieve the number of days in a week.
			@memberof MayanCalendar
			@return {number} The number of days. */
		daysInWeek: function() {
			return 5; // Just for formatting
		},

		/** Retrieve the day of the week for a date.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The day of the week: 0 to number of days - 1.
			@throws Error if an invalid date or a different calendar used. */
		dayOfWeek: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			return date.day();
		},

		/** Determine whether this date is a week day.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			this._validate(year, month, day, $.calendars.local.invalidDate);
			return true;
		},

		/** Retrieve additional information about a date - Haab and Tzolkin equivalents.
			@memberof MayanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {object} Additional information - contents depends on calendar.
			@throws Error if an invalid date or a different calendar used. */
		extraInfo: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			var jd = date.toJD();
			var haab = this._toHaab(jd);
			var tzolkin = this._toTzolkin(jd);
			return {haabMonthName: this.local.haabMonths[haab[0] - 1],
				haabMonth: haab[0], haabDay: haab[1],
				tzolkinDayName: this.local.tzolkinMonths[tzolkin[0] - 1],
				tzolkinDay: tzolkin[0], tzolkinTrecena: tzolkin[1]};
		},

		/** Retrieve Haab date from a Julian date.
			@memberof MayanCalendar
			@private
			@param {number} jd The Julian date.
			@return {number[]} Corresponding Haab month and day. */
		_toHaab: function(jd) {
			jd -= this.jdEpoch;
			var day = mod(jd + 8 + ((18 - 1) * 20), 365);
			return [Math.floor(day / 20) + 1, mod(day, 20)];
		},

		/** Retrieve Tzolkin date from a Julian date.
			@memberof MayanCalendar
			@private
			@param {number} jd The Julian date.
			@return {number[]} Corresponding Tzolkin day and trecena. */
		_toTzolkin: function(jd) {
			jd -= this.jdEpoch;
			return [amod(jd + 20, 20), amod(jd + 4, 13)];
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof MayanCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if only <code>year</code> specified above).
			@param {number} [day] The day to convert (if only <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			return date.day() + (date.month() * 20) + (date.year() * 360) + this.jdEpoch;
		},

		/** Create a new date from a Julian date.
			@memberof MayanCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			jd = Math.floor(jd) + 0.5 - this.jdEpoch;
			var year = Math.floor(jd / 360);
			jd = jd % 360;
			jd += (jd < 0 ? 360 : 0);
			var month = Math.floor(jd / 20);
			var day = jd % 20;
			return this.newDate(year, month, day);
		}
	});

	// Modulus function which works for non-integers.
	function mod(a, b) {
		return a - (b * Math.floor(a / b));
	}

	// Modulus function which returns numerator if modulus is zero.
	function amod(a, b) {
		return mod(a - 1, b) + 1;
	}

	// Mayan calendar implementation
	$.calendars.calendars.mayan = MayanCalendar;

})(jQuery);