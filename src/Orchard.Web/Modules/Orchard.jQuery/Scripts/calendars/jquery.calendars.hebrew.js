/* http://keith-wood.name/calendars.html
   Hebrew calendar for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Implementation of the Hebrew civil calendar.
   Based on code from http://www.fourmilab.ch/documents/calendar/.
   See also http://en.wikipedia.org/wiki/Hebrew_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function HebrewCalendar(language) {
	this.local = this.regional[language || ''] || this.regional[''];
}

HebrewCalendar.prototype = new $.calendars.baseCalendar;

$.extend(HebrewCalendar.prototype, {
	name: 'Hebrew', // The calendar name
	jdEpoch: 347995.5, // Julian date of start of Hebrew epoch: 7 October 3761 BCE
	daysPerMonth: [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 29], // Days per month in a common year
	hasYearZero: false, // True if has a year zero, false if not
	minMonth: 1, // The minimum month number
	firstMonth: 7, // The first month in the year
	minDay: 1, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Hebrew', // The calendar name
			epochs: ['BAM', 'AM'],
			monthNames: ['Nisan', 'Iyar', 'Sivan', 'Tammuz', 'Av', 'Elul',
			'Tishrei', 'Cheshvan', 'Kislev', 'Tevet', 'Shevat', 'Adar', 'Adar II'],
			monthNamesShort: ['Nis', 'Iya', 'Siv', 'Tam', 'Av', 'Elu', 'Tis', 'Che', 'Kis', 'Tev', 'She', 'Ada', 'Ad2'],
			dayNames: ['Yom Rishon', 'Yom Sheni', 'Yom Shlishi', 'Yom Revi\'i', 'Yom Chamishi', 'Yom Shishi', 'Yom Shabbat'],
			dayNamesShort: ['Ris', 'She', 'Shl', 'Rev', 'Cha', 'Shi', 'Sha'],
			dayNamesMin: ['Ri','She','Shl','Re','Ch','Shi','Sha'],
			dateFormat: 'dd/mm/yyyy', // See format options on BaseCalendar.formatDate
			firstDay: 0, // The first day of the week, Sun = 0, Mon = 1, ...
			isRTL: false, // True if right-to-left language, false if left-to-right
			showMonthAfterYear: false, // True if the year select precedes month, false for month then year
			yearSuffix: '' // Additional text to append to the year in the month headers
		}
	},

	/* Determine whether this date is in a leap year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	leapYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		return this._leapYear(date.year());
	},

	/* Determine whether this date is in a leap year.
	   @param  year  (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	_leapYear: function(year) {
		year = (year < 0 ? year + 1 : year);
		return mod(year * 7 + 1, 19) < 7;
	},

	/* Retrieve the number of months in a year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (number) the number of months
	   @throws  error if an invalid year or a different calendar used */
	monthsInYear: function(year) {
		this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		return this._leapYear(year.year ? year.year() : year) ? 13 : 12;
	},

	/* Determine the week of the year for a date.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the week of the year
	   @throws  error if an invalid date or a different calendar used */
	weekOfYear: function(year, month, day) {
		// Find Sunday of this week starting on Sunday
		var checkDate = this.newDate(year, month, day);
		checkDate.add(-checkDate.dayOfWeek(), 'd');
		return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
	},

	/* Retrieve the number of days in a year.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @return  (number) the number of days
	   @throws  error if an invalid year or a different calendar used */
	daysInYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		year = date.year();
		return this.toJD((year == -1 ? +1 : year + 1), 7, 1) - this.toJD(year, 7, 1);
	},

	/* Retrieve the number of days in a month.
	   @param  year   (CDate) the date to examine or
	                  (number) the year of the month
	   @param  month  (number) the month
	   @return  (number) the number of days in this month
	   @throws  error if an invalid month/year or a different calendar used */
	daysInMonth: function(year, month) {
		if (year.year) {
			month = year.month();
			year = year.year();
		}
		this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
		return (month == 12 && this.leapYear(year) ? 30 : // Adar I
				(month == 8 && mod(this.daysInYear(year), 10) == 5 ? 30 : // Cheshvan in shlemah year
				(month == 9 && mod(this.daysInYear(year), 10) == 3 ? 29 : // Kislev in chaserah year
				this.daysPerMonth[month - 1])));
	},

	/* Determine whether this date is a week day.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (boolean) true if a week day, false if not
	   @throws  error if an invalid date or a different calendar used */
	weekDay: function(year, month, day) {
		return this.dayOfWeek(year, month, day) != 6;
	},

	/* Retrieve additional information about a date - year type.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (object) additional information - contents depends on calendar
	   @throws  error if an invalid date or a different calendar used */
	extraInfo: function(year, month, day) {
		var date = this._validate(year, month, day, $.calendars.local.invalidDate);
		return {yearType: (this.leapYear(date) ? 'embolismic' : 'common') + ' ' +
			['deficient', 'regular', 'complete'][this.daysInYear(date) % 10 - 3]};
	},

	/* Retrieve the Julian date equivalent for this date,
	   i.e. days since January 1, 4713 BCE Greenwich noon.
	   @param  year   (CDate) the date to convert or
	                  (number) the year to convert
	   @param  month  (number) the month to convert
	   @param  day    (number) the day to convert
	   @return  (number) the equivalent Julian date
	   @throws  error if an invalid date or a different calendar used */
	toJD: function(year, month, day) {
		var date = this._validate(year, month, day, $.calendars.local.invalidDate);
		year = date.year();
		month = date.month();
		day = date.day();
		var adjYear = (year <= 0 ? year + 1 : year);
		var jd = this.jdEpoch + this._delay1(adjYear) +
			this._delay2(adjYear) + day + 1;
		if (month < 7) {
			for (var m = 7; m <= this.monthsInYear(year); m++) {
				jd += this.daysInMonth(year, m);
			}
			for (var m = 1; m < month; m++) {
				jd += this.daysInMonth(year, m);
			}
		}
		else {
			for (var m = 7; m < month; m++) {
				jd += this.daysInMonth(year, m);
			}
		}
		return jd;
	},

	/* Test for delay of start of new year and to avoid
	   Sunday, Wednesday, or Friday as start of the new year.
	   @param  year  (number) the year to examine
	   @return  (number) the days to offset by */
	_delay1: function(year) {
		var months = Math.floor((235 * year - 234) / 19);
		var parts = 12084 + 13753 * months;
		var day = months * 29 + Math.floor(parts / 25920);
		if (mod(3 * (day + 1), 7) < 3) {
			day++;
		}
		return day;
	},

	/* Check for delay in start of new year due to length of adjacent years.
	   @param  year  (number) the year to examine
	   @return  (number) the days to offset by */
	_delay2: function(year) {
		var last = this._delay1(year - 1);
		var present = this._delay1(year);
		var next = this._delay1(year + 1);
		return ((next - present) == 356 ? 2 : ((present - last) == 382 ? 1 : 0));
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
	fromJD: function(jd) {
		jd = Math.floor(jd) + 0.5;
		var year = Math.floor(((jd - this.jdEpoch) * 98496.0) / 35975351.0) - 1;
		while (jd >= this.toJD((year == -1 ? +1 : year + 1), 7, 1)) {
			year++;
		}
		var month = (jd < this.toJD(year, 1, 1)) ? 7 : 1;
		while (jd > this.toJD(year, month, this.daysInMonth(year, month))) {
			month++;
		}
		var day = jd - this.toJD(year, month, 1) + 1;
		return this.newDate(year, month, day);
	}
});

// Modulus function which works for non-integers.
function mod(a, b) {
	return a - (b * Math.floor(a / b));
}

// Hebrew calendar implementation
$.calendars.calendars.hebrew = HebrewCalendar;

})(jQuery);