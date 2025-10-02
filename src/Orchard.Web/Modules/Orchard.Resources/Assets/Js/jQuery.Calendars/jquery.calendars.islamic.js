/* http://keith-wood.name/calendars.html
   Islamic calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	/** Implementation of the Islamic or '16 civil' calendar.
		Based on code from <a href="http://www.iranchamber.com/calendar/converter/iranian_calendar_converter.php">http://www.iranchamber.com/calendar/converter/iranian_calendar_converter.php</a>.
		See also <a href="http://en.wikipedia.org/wiki/Islamic_calendar">http://en.wikipedia.org/wiki/Islamic_calendar</a>.
		@class IslamicCalendar
		@param {string} [language=''] The language code (default English) for localisation. */
	function IslamicCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	IslamicCalendar.prototype = new $.calendars.baseCalendar();

	$.extend(IslamicCalendar.prototype, {
		/** The calendar name.
			@memberof IslamicCalendar */
		name: 'Islamic',
		/** Julian date of start of Islamic epoch: 16 July 622 CE.
			@memberof IslamicCalendar */
		jdEpoch: 1948439.5,
		/** Days per month in a common year.
			@memberof IslamicCalendar */
		daysPerMonth: [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof IslamicCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof IslamicCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof IslamicCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof IslamicCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof IslamicCalendar
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
				name: 'Islamic',
				epochs: ['BH', 'AH'],
				monthNames: ['Muharram', 'Safar', 'Rabi\' al-awwal', 'Rabi\' al-thani', 'Jumada al-awwal', 'Jumada al-thani',
				'Rajab', 'Sha\'aban', 'Ramadan', 'Shawwal', 'Dhu al-Qi\'dah', 'Dhu al-Hijjah'],
				monthNamesShort: ['Muh', 'Saf', 'Rab1', 'Rab2', 'Jum1', 'Jum2', 'Raj', 'Sha\'', 'Ram', 'Shaw', 'DhuQ', 'DhuH'],
				dayNames: ['Yawm al-ahad', 'Yawm al-ithnayn', 'Yawm ath-thulaathaa\'',
				'Yawm al-arbi\'aa\'', 'Yawm al-khamīs', 'Yawm al-jum\'a', 'Yawm as-sabt'],
				dayNamesShort: ['Aha', 'Ith', 'Thu', 'Arb', 'Kha', 'Jum', 'Sab'],
				dayNamesMin: ['Ah','It','Th','Ar','Kh','Ju','Sa'],
				digits: null,
				dateFormat: 'yyyy/mm/dd',
				firstDay: 6,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof IslamicCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return (date.year() * 11 + 14) % 30 < 11;
		},

		/** Determine the week of the year for a date.
			@memberof IslamicCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Sunday of this week starting on Sunday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(-checkDate.dayOfWeek(), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a year.
			@memberof IslamicCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {number} The number of days.
			@throws Error if an invalid year or a different calendar used. */
		daysInYear: function(year) {
			return (this.leapYear(year) ? 355 : 354);
		},

		/** Retrieve the number of days in a month.
			@memberof IslamicCalendar
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
			@memberof IslamicCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] {number} The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return this.dayOfWeek(year, month, day) !== 5;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof IslamicCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if only <code>year</code> specified above).
			@param {number} [day] The day to convert (if only <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			year = date.year();
			month = date.month();
			day = date.day();
			year = (year <= 0 ? year + 1 : year);
			return day + Math.ceil(29.5 * (month - 1)) + (year - 1) * 354 +
				Math.floor((3 + (11 * year)) / 30) + this.jdEpoch - 1;
		},

		/** Create a new date from a Julian date.
			@memberof IslamicCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			jd = Math.floor(jd) + 0.5;
			var year = Math.floor((30 * (jd - this.jdEpoch) + 10646) / 10631);
			year = (year <= 0 ? year - 1 : year);
			var month = Math.min(12, Math.ceil((jd - 29 - this.toJD(year, 1, 1)) / 29.5) + 1);
			var day = jd - this.toJD(year, month, 1) + 1;
			return this.newDate(year, month, day);
		}
	});

	// Islamic (16 civil) calendar implementation
	$.calendars.calendars.islamic = IslamicCalendar;

})(jQuery);