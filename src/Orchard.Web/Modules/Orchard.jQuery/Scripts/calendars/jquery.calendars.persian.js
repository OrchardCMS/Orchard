/* http://keith-wood.name/calendars.html
   Persian calendar for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Implementation of the Persian or Jalali calendar.
   Based on code from http://www.iranchamber.com/calendar/converter/iranian_calendar_converter.php.
   See also http://en.wikipedia.org/wiki/Iranian_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function PersianCalendar(language) {
	this.local = this.regional[language || ''] || this.regional[''];
}

PersianCalendar.prototype = new $.calendars.baseCalendar;

$.extend(PersianCalendar.prototype, {
	name: 'Persian', // The calendar name
	jdEpoch: 1948320.5, // Julian date of start of Persian epoch: 19 March 622 CE
	daysPerMonth: [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29], // Days per month in a common year
	hasYearZero: false, // True if has a year zero, false if not
	minMonth: 1, // The minimum month number
	firstMonth: 1, // The first month in the year
	minDay: 1, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Persian', // The calendar name
			epochs: ['BP', 'AP'],
			monthNames: ['Farvardin', 'Ordibehesht', 'Khordad', 'Tir', 'Mordad', 'Shahrivar',
			'Mehr', 'Aban', 'Azar', 'Day', 'Bahman', 'Esfand'],
			monthNamesShort: ['Far', 'Ord', 'Kho', 'Tir', 'Mor', 'Sha', 'Meh', 'Aba', 'Aza', 'Day', 'Bah', 'Esf'],
			dayNames: ['Yekshambe', 'Doshambe', 'Seshambe', 'Chæharshambe', 'Panjshambe', 'Jom\'e', 'Shambe'],
			dayNamesShort: ['Yek', 'Do', 'Se', 'Chæ', 'Panj', 'Jom', 'Sha'],
			dayNamesMin: ['Ye','Do','Se','Ch','Pa','Jo','Sh'],
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
		return (((((date.year() - (date.year() > 0 ? 474 : 473)) % 2820) +
			474 + 38) * 682) % 2816) < 682;
	},

	/* Determine the week of the year for a date.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the week of the year
	   @throws  error if an invalid date or a different calendar used */
	weekOfYear: function(year, month, day) {
		// Find Saturday of this week starting on Saturday
		var checkDate = this.newDate(year, month, day);
		checkDate.add(-((checkDate.dayOfWeek() + 1) % 7), 'd');
		return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
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
		var epBase = year - (year >= 0 ? 474 : 473);
		var epYear = 474 + mod(epBase, 2820);
		return day + (month <= 7 ? (month - 1) * 31 : (month - 1) * 30 + 6) +
			Math.floor((epYear * 682 - 110) / 2816) + (epYear - 1) * 365 +
			Math.floor(epBase / 2820) * 1029983 + this.jdEpoch - 1;
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
	fromJD: function(jd) {
		jd = Math.floor(jd) + 0.5;
		var depoch = jd - this.toJD(475, 1, 1);
		var cycle = Math.floor(depoch / 1029983);
		var cyear = mod(depoch, 1029983);
		var ycycle = 2820;
		if (cyear != 1029982) {
			var aux1 = Math.floor(cyear / 366);
			var aux2 = mod(cyear, 366);
			ycycle = Math.floor(((2134 * aux1) + (2816 * aux2) + 2815) / 1028522) + aux1 + 1;
		}
		var year = ycycle + (2820 * cycle) + 474;
		year = (year <= 0 ? year - 1 : year);
		var yday = jd - this.toJD(year, 1, 1) + 1;
		var month = (yday <= 186 ? Math.ceil(yday / 31) : Math.ceil((yday - 6) / 30));
		var day = jd - this.toJD(year, month, 1) + 1;
		return this.newDate(year, month, day);
	}
});

// Modulus function which works for non-integers.
function mod(a, b) {
	return a - (b * Math.floor(a / b));
}

// Persian (Jalali) calendar implementation
$.calendars.calendars.persian = PersianCalendar;
$.calendars.calendars.jalali = PersianCalendar;

})(jQuery);