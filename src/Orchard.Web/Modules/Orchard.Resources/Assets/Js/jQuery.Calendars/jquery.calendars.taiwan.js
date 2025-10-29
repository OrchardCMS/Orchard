/* http://keith-wood.name/calendars.html
   Taiwanese (Minguo) calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) February 2010.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	var gregorianCalendar = $.calendars.instance();

	/** Implementation of the Taiwanese calendar.
		See http://en.wikipedia.org/wiki/Minguo_calendar.
		@class TaiwanCalendar
		@param [{string} language=''] The language code (default English) for localisation. */
	function TaiwanCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	TaiwanCalendar.prototype = new $.calendars.baseCalendar();

	$.extend(TaiwanCalendar.prototype, {
		/** The calendar name.
			@memberof TaiwanCalendar */
		name: 'Taiwan',
		/** Julian date of start of Taiwan epoch: 1 January 1912 CE (Gregorian).
			@memberof TaiwanCalendar */
		jdEpoch: 2419402.5,
		/** Difference in years between Taiwan and Gregorian calendars.
			@memberof TaiwanCalendar */
		yearsOffset: 1911,
		/** Days per month in a common year.
			@memberof TaiwanCalendar */
		daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof TaiwanCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof TaiwanCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof TaiwanCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof TaiwanCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof TaiwanCalendar
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
				name: 'Taiwan',
				epochs: ['BROC', 'ROC'],
				monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
				'July', 'August', 'September', 'October', 'November', 'December'],
				monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
				dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
				dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
				dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
				digits: null,
				dateFormat: 'yyyy/mm/dd',
				firstDay: 1,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof TaiwanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			var year = this._t2gYear(date.year());
			return gregorianCalendar.leapYear(year);
		},

		/** Determine the week of the year for a date - ISO 8601.
			@memberof TaiwanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidYear);
			var year = this._t2gYear(date.year());
			return gregorianCalendar.weekOfYear(year, date.month(), date.day());
		},

		/** Retrieve the number of days in a month.
			@memberof TaiwanCalendar
			@param {CDate|number} year The date to examine or the year of the month.
			@param {number} [month] The month (if only <code>year</code> specified above).
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return this.daysPerMonth[date.month() - 1] +
				(date.month() === 2 && this.leapYear(date.year()) ? 1 : 0);
		},

		/** Determine whether this date is a week day.
			@memberof TaiwanCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return (this.dayOfWeek(year, month, day) || 7) < 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof TaiwanCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if only <code>year</code> specified above).
			@param {number} [day] The day to convert (if only <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			var year = this._t2gYear(date.year());
			return gregorianCalendar.toJD(year, date.month(), date.day());
		},

		/** Create a new date from a Julian date.
			@memberof TaiwanCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			var date = gregorianCalendar.fromJD(jd);
			var year = this._g2tYear(date.year());
			return this.newDate(year, date.month(), date.day());
		},

		/** Convert Taiwanese to Gregorian year.
			@memberof TaiwanCalendar
			@private
			@param {number} year The Taiwanese year.
			@return {number} The corresponding Gregorian year. */
		_t2gYear: function(year) {
			return year + this.yearsOffset + (year >= -this.yearsOffset && year <= -1 ? 1 : 0);
		},

		/** Convert Gregorian to Taiwanese year.
			@memberof TaiwanCalendar
			@private
			@param {number} year The Gregorian year.
			@return {number} The corresponding Taiwanese year. */
		_g2tYear: function(year) {
			return year - this.yearsOffset - (year >= 1 && year <= this.yearsOffset ? 1 : 0);
		}
	});

	// Taiwan calendar implementation
	$.calendars.calendars.taiwan = TaiwanCalendar;

})(jQuery);