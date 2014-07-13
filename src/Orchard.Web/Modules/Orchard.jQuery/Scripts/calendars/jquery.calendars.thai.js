/* http://keith-wood.name/calendars.html
   Thai calendar for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) February 2010.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

var gregorianCalendar = $.calendars.instance();

/* Implementation of the Thai calendar.
   See http://en.wikipedia.org/wiki/Thai_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function ThaiCalendar(language) {
	this.local = this.regional[language || ''] || this.regional[''];
}

ThaiCalendar.prototype = new $.calendars.baseCalendar;

$.extend(ThaiCalendar.prototype, {
	name: 'Thai', // The calendar name
	jdEpoch: 1523098.5, // Julian date of start of Thai epoch: 1 January 543 BCE (Gregorian)
	yearsOffset: 543, // Difference in years between Thai and Gregorian calendars
	daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31], // Days per month in a common year
	hasYearZero: false, // True if has a year zero, false if not
	minMonth: 1, // The minimum month number
	firstMonth: 1, // The first month in the year
	minDay: 1, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Thai', // The calendar name
			epochs: ['BBE', 'BE'],
			monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
			'July', 'August', 'September', 'October', 'November', 'December'],
			monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
			dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
			dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
			dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
			dateFormat: 'dd/mm/yyyy',
			firstDay: 0,
			isRTL: false
		}
	},

	/* Determine whether this date is in a leap year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	leapYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		var year = this._t2gYear(date.year());
		return gregorianCalendar.leapYear(year);
	},

	/* Determine the week of the year for a date - ISO 8601.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the week of the year
	   @throws  error if an invalid date or a different calendar used */
	weekOfYear: function(year, month, day) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		var year = this._t2gYear(date.year());
		return gregorianCalendar.weekOfYear(year, date.month(), date.day());
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
			(date.month() == 2 && this.leapYear(date.year()) ? 1 : 0);
	},

	/* Determine whether this date is a week day.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (boolean) true if a week day, false if not
	   @throws  error if an invalid date or a different calendar used */
	weekDay: function(year, month, day) {
		return (this.dayOfWeek(year, month, day) || 7) < 6;
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
		var year = this._t2gYear(date.year());
		return gregorianCalendar.toJD(year, date.month(), date.day());
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
	fromJD: function(jd) {
		var date = gregorianCalendar.fromJD(jd);
		var year = this._g2tYear(date.year());
		return this.newDate(year, date.month(), date.day());
	},

	/* Convert Thai to Gregorian year.
	   @param  year  the Thai year
	   @return  the corresponding Gregorian year */
	_t2gYear: function(year) {
		return year - this.yearsOffset - (year >= 1 && year <= this.yearsOffset ? 1 : 0);
	},

	/* Convert Gregorian to Thai year.
	   @param  year  the Gregorian year
	   @return  the corresponding Thai year */
	_g2tYear: function(year) {
		return year + this.yearsOffset + (year >= -this.yearsOffset && year <= -1 ? 1 : 0);
	}
});

// Thai calendar implementation
$.calendars.calendars.thai = ThaiCalendar;

})(jQuery);