/* http://keith-wood.name/calendars.html
   Coptic calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) February 2010.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	/** Implementation of the Coptic calendar.
		See <a href="http://en.wikipedia.org/wiki/Coptic_calendar">http://en.wikipedia.org/wiki/Coptic_calendar</a>.
		See also Calendrical Calculations: The Millennium Edition
		(<a href="http://emr.cs.iit.edu/home/reingold/calendar-book/index.shtml">http://emr.cs.iit.edu/home/reingold/calendar-book/index.shtml</a>).
		@class CopticCalendar
		@param {string} [language=''] The language code (default English) for localisation. */
	function CopticCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	CopticCalendar.prototype = new $.calendars.baseCalendar();

	$.extend(CopticCalendar.prototype, {
		/** The calendar name.
			@memberof CopticCalendar */
		name: 'Coptic',
		/** Julian date of start of Coptic epoch: 29 August 284 CE (Gregorian).
			@memberof CopticCalendar */
		jdEpoch: 1825029.5,
		/** Days per month in a common year.
			@memberof CopticCalendar */
		daysPerMonth: [30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 5],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof CopticCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof CopticCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof CopticCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof CopticCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof CopticCalendar
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
				name: 'Coptic',
				epochs: ['BAM', 'AM'],
				monthNames: ['Thout', 'Paopi', 'Hathor', 'Koiak', 'Tobi', 'Meshir',
				'Paremhat', 'Paremoude', 'Pashons', 'Paoni', 'Epip', 'Mesori', 'Pi Kogi Enavot'],
				monthNamesShort: ['Tho', 'Pao', 'Hath', 'Koi', 'Tob', 'Mesh',
				'Pat', 'Pad', 'Pash', 'Pao', 'Epi', 'Meso', 'PiK'],
				dayNames: ['Tkyriaka', 'Pesnau', 'Pshoment', 'Peftoou', 'Ptiou', 'Psoou', 'Psabbaton'],
				dayNamesShort: ['Tky', 'Pes', 'Psh', 'Pef', 'Pti', 'Pso', 'Psa'],
				dayNamesMin: ['Tk', 'Pes', 'Psh', 'Pef', 'Pt', 'Pso', 'Psa'],
				digits: null,
				dateFormat: 'dd/mm/yyyy',
				firstDay: 0,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof CopticCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			var year = date.year() + (date.year() < 0 ? 1 : 0); // No year zero
			return year % 4 === 3 || year % 4 === -1;
		},

		/** Retrieve the number of months in a year.
			@memberof CopticCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {number} The number of months.
			@throws Error if an invalid year or a different calendar used. */
		monthsInYear: function(year) {
			this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			return 13;
		},

		/** Determine the week of the year for a date.
			@memberof CopticCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] the month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Sunday of this week starting on Sunday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(-checkDate.dayOfWeek(), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a month.
			@memberof CopticCalendar
			@param {CDate|number} year The date to examine or the year of the month.
			@param {number} [month] The month (if only <code>year</code> specified above).
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return this.daysPerMonth[date.month() - 1] +
				(date.month() === 13 && this.leapYear(date.year()) ? 1 : 0);
		},

		/** Determine whether this date is a week day.
			@memberof CopticCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} month The month to examine (if only <code>year</code> specified above).
			@param {number} day The day to examine (if only <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return (this.dayOfWeek(year, month, day) || 7) < 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof CopticCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] the month to convert (if only <code>year</code> specified above).
			@param {number} [day] The day to convert (if only <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			year = date.year();
			if (year < 0) { year++; } // No year zero
			return date.day() + (date.month() - 1) * 30 +
				(year - 1) * 365 + Math.floor(year / 4) + this.jdEpoch - 1;
		},

		/** Create a new date from a Julian date.
			@memberof CopticCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			var c = Math.floor(jd) + 0.5 - this.jdEpoch;
			var year = Math.floor((c - Math.floor((c + 366) / 1461)) / 365) + 1;
			if (year <= 0) { year--; } // No year zero
			c = Math.floor(jd) + 0.5 - this.newDate(year, 1, 1).toJD();
			var month = Math.floor(c / 30) + 1;
			var day = c - (month - 1) * 30 + 1;
			return this.newDate(year, month, day);
		}
	});

	// Coptic calendar implementation
	$.calendars.calendars.coptic = CopticCalendar;

})(jQuery);