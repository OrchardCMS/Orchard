/* http://keith-wood.name/calendars.html
   Islamic calendar for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Implementation of the Islamic or '16 civil' calendar.
   Based on code from http://www.iranchamber.com/calendar/converter/iranian_calendar_converter.php.
   See also http://en.wikipedia.org/wiki/Islamic_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function IslamicCalendar(language) {
	this.local = this.regional[language || ''] || this.regional[''];
}

IslamicCalendar.prototype = new $.calendars.baseCalendar;

$.extend(IslamicCalendar.prototype, {
	name: 'Islamic', // The calendar name
	jdEpoch: 1948439.5, // Julian date of start of Islamic epoch: 16 July 622 CE
	daysPerMonth: [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29], // Days per month in a common year
	hasYearZero: false, // True if has a year zero, false if not
	minMonth: 1, // The minimum month number
	firstMonth: 1, // The first month in the year
	minDay: 1, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Islamic', // The calendar name
			epochs: ['BH', 'AH'],
			monthNames: ['Muharram', 'Safar', 'Rabi\' al-awwal', 'Rabi\' al-thani', 'Jumada al-awwal', 'Jumada al-thani',
			'Rajab', 'Sha\'aban', 'Ramadan', 'Shawwal', 'Dhu al-Qi\'dah', 'Dhu al-Hijjah'],
			monthNamesShort: ['Muh', 'Saf', 'Rab1', 'Rab2', 'Jum1', 'Jum2', 'Raj', 'Sha\'', 'Ram', 'Shaw', 'DhuQ', 'DhuH'],
			dayNames: ['Yawm al-ahad', 'Yawm al-ithnayn', 'Yawm ath-thulaathaa\'',
			'Yawm al-arbi\'aa\'', 'Yawm al-khamīs', 'Yawm al-jum\'a', 'Yawm as-sabt'],
			dayNamesShort: ['Aha', 'Ith', 'Thu', 'Arb', 'Kha', 'Jum', 'Sab'],
			dayNamesMin: ['Ah','It','Th','Ar','Kh','Ju','Sa'],
			dateFormat: 'yyyy/mm/dd', // See format options on BaseCalendar.formatDate
			firstDay: 6, // The first day of the week, Sun = 0, Mon = 1, ...
			isRTL: false // True if right-to-left language, false if left-to-right
		}
	},

	/* Determine whether this date is in a leap year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	leapYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		return (date.year() * 11 + 14) % 30 < 11;
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
		return (this.leapYear(year) ? 355 : 354);
	},

	/* Retrieve the number of days in a month.
	   @param  year   (CDate) the date to examine or
	                  (number) the year of the month
	   @param  month  (number) the month
	   @return  (number) the number of days in this month
	   @throws  error if an invalid month/year or a different calendar used */
	daysInMonth: function(year, month) {
		var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
		return this.daysPerMonth[date.month() - 1] +
			(date.month() == 12 && this.leapYear(date.year()) ? 1 : 0);
	},

	/* Determine whether this date is a week day.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (boolean) true if a week day, false if not
	   @throws  error if an invalid date or a different calendar used */
	weekDay: function(year, month, day) {
		return this.dayOfWeek(year, month, day) != 5;
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
		year = (year <= 0 ? year + 1 : year);
		return day + Math.ceil(29.5 * (month - 1)) + (year - 1) * 354 +
			Math.floor((3 + (11 * year)) / 30) + this.jdEpoch - 1;
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
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