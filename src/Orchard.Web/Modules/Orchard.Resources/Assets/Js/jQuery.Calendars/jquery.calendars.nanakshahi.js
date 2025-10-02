/* http://keith-wood.name/calendars.html
   Nanakshahi calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) January 2016.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	/** Implementation of the Nanakshahi calendar.
		See also <a href="https://en.wikipedia.org/wiki/Nanakshahi_calendar">https://en.wikipedia.org/wiki/Nanakshahi_calendar</a>.
		@class NanakshahiCalendar
		@param {string} [language=''] The language code (default English) for localisation. */
	function NanakshahiCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	NanakshahiCalendar.prototype = new $.calendars.baseCalendar();
	
	var gregorian = $.calendars.instance('gregorian');

	$.extend(NanakshahiCalendar.prototype, {
		/** The calendar name.
			@memberof NanakshahiCalendar */
		name: 'Nanakshahi',
		/** Julian date of start of Nanakshahi epoch: 14 March 1469 CE.
			@memberof NanakshahiCalendar */
		jdEpoch: 2257673.5,
		/** Days per month in a common year.
			@memberof NanakshahiCalendar */
		daysPerMonth: [31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 30, 30],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof NanakshahiCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof NanakshahiCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof NanakshahiCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof NanakshahiCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof NanakshahiCalendar
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
				name: 'Nanakshahi',
				epochs: ['BN', 'AN'],
				monthNames: ['Chet', 'Vaisakh', 'Jeth', 'Harh', 'Sawan', 'Bhadon',
				'Assu', 'Katak', 'Maghar', 'Poh', 'Magh', 'Phagun'],
				monthNamesShort: ['Che', 'Vai', 'Jet', 'Har', 'Saw', 'Bha', 'Ass', 'Kat', 'Mgr', 'Poh', 'Mgh', 'Pha'],
				dayNames: ['Somvaar', 'Mangalvar', 'Budhvaar', 'Veervaar', 'Shukarvaar', 'Sanicharvaar', 'Etvaar'],
				dayNamesShort: ['Som', 'Mangal', 'Budh', 'Veer', 'Shukar', 'Sanichar', 'Et'],
				dayNamesMin: ['So', 'Ma', 'Bu', 'Ve', 'Sh', 'Sa', 'Et'],
				digits: null,
				dateFormat: 'dd-mm-yyyy',
				firstDay: 0,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof NanakshahiCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			return gregorian.leapYear(date.year() + (date.year() < 1 ? 1 : 0) + 1469);
		},

		/** Determine the week of the year for a date.
			@memberof NanakshahiCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Monday of this week starting on Monday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(1 - (checkDate.dayOfWeek() || 7), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a month.
			@memberof NanakshahiCalendar
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
			@memberof NanakshahiCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number}[month]  The month to examine (if only <code>year</code> specified above).
			@param {number} [day] The day to examine (if only <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return (this.dayOfWeek(year, month, day) || 7) < 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof NanakshahiCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if only <code>year</code> specified above).
			@param {number} [day] The day to convert (if only <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidMonth);
			var year = date.year();
			if (year < 0) { year++; } // No year zero
			var doy = date.day();
			for (var m = 1; m < date.month(); m++) {
				doy += this.daysPerMonth[m - 1];
			}
			return doy + gregorian.toJD(year + 1468, 3, 13);
		},

		/** Create a new date from a Julian date.
			@memberof NanakshahiCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			jd = Math.floor(jd + 0.5);
			var year = Math.floor((jd - (this.jdEpoch - 1)) / 366);
			while (jd >= this.toJD(year + 1, 1, 1)) {
				year++;
			}
			var day = jd - Math.floor(this.toJD(year, 1, 1) + 0.5) + 1;
			var month = 1;
			while (day > this.daysInMonth(year, month)) {
				day -= this.daysInMonth(year, month);
				month++;
			}
			return this.newDate(year, month, day);
		}
	});

	// Nanakshahi calendar implementation
	$.calendars.calendars.nanakshahi = NanakshahiCalendar;

})(jQuery);