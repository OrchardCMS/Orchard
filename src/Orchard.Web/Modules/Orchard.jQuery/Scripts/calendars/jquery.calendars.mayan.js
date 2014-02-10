/* http://keith-wood.name/calendars.html
   Mayan calendar for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Implementation of the Mayan Long Count calendar.
   See also http://en.wikipedia.org/wiki/Mayan_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function MayanCalendar(language) {
	this.local = this.regional[language || ''] || this.regional[''];
}

MayanCalendar.prototype = new $.calendars.baseCalendar;

$.extend(MayanCalendar.prototype, {
	name: 'Mayan', // The calendar name
	jdEpoch: 584282.5, // Julian date of start of Mayan epoch: 11 August 3114 BCE
	hasYearZero: true, // True if has a year zero, false if not
	minMonth: 0, // The minimum month number
	firstMonth: 0, // The first month in the year
	minDay: 0, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Mayan', // The calendar name
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
			dateFormat: 'YYYY.m.d', // See format options on BaseCalendar.formatDate
			firstDay: 0, // The first day of the week, Sun = 0, Mon = 1, ...
			isRTL: false, // True if right-to-left language, false if left-to-right
			haabMonths: ['Pop', 'Uo', 'Zip', 'Zotz', 'Tzec', 'Xul', 'Yaxkin', 'Mol', 'Chen', 'Yax',
			'Zac', 'Ceh', 'Mac', 'Kankin', 'Muan', 'Pax', 'Kayab', 'Cumku', 'Uayeb'],
			tzolkinMonths: ['Imix', 'Ik', 'Akbal', 'Kan', 'Chicchan', 'Cimi', 'Manik', 'Lamat', 'Muluc', 'Oc',
			'Chuen', 'Eb', 'Ben', 'Ix', 'Men', 'Cib', 'Caban', 'Etznab', 'Cauac', 'Ahau']
		}
	},

	/* Determine whether this date is in a leap year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	leapYear: function(year) {
		this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		return false;
	},

	/* Format the year, if not a simple sequential number.
	   @param  year  (CDate) the date to format or
	                 (number) the year to format
	   @return  (string) the formatted year
	   @throws  error if an invalid year or a different calendar used */
	formatYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		year = date.year();
		var baktun = Math.floor(year / 400);
		year = year % 400;
		year += (year < 0 ? 400 : 0);
		var katun = Math.floor(year / 20);
		return baktun + '.' + katun + '.' + (year % 20);
	},

	/* Convert from the formatted year back to a single number.
	   @param  years  (string) the year as n.n.n
	   @return  (number) the sequential year
	   @throws  error if an invalid value is supplied */
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

	/* Retrieve the number of months in a year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (number) the number of months
	   @throws  error if an invalid year or a different calendar used */
	monthsInYear: function(year) {
		this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		return 18;
	},

	/* Determine the week of the year for a date.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the week of the year
	   @throws  error if an invalid date or a different calendar used */
	weekOfYear: function(year, month, day) {
		this._validate(year, month, day, $.calendars.local.invalidDate);
		return 0;
	},

	/* Retrieve the number of days in a year.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @return  (number) the number of days
	   @throws  error if an invalid year or a different calendar used */
	daysInYear: function(year) {
		this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		return 360;
	},

	/* Retrieve the number of days in a month.
	   @param  year   (CDate) the date to examine or
	                  (number) the year of the month
	   @param  month  (number) the month
	   @return  (number) the number of days in this month
	   @throws  error if an invalid month/year or a different calendar used */
	daysInMonth: function(year, month) {
		this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
		return 20;
	},

	/* Retrieve the number of days in a week.
	   @return  (number) the number of days */
	daysInWeek: function() {
		return 5; // Just for formatting
	},

	/* Retrieve the day of the week for a date.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the day of the week: 0 to number of days - 1
	   @throws  error if an invalid date or a different calendar used */
	dayOfWeek: function(year, month, day) {
		var date = this._validate(year, month, day, $.calendars.local.invalidDate);
		return date.day();
	},

	/* Determine whether this date is a week day.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (boolean) true if a week day, false if not
	   @throws  error if an invalid date or a different calendar used */
	weekDay: function(year, month, day) {
		this._validate(year, month, day, $.calendars.local.invalidDate);
		return true;
	},

	/* Retrieve additional information about a date - Haab and Tzolkin equivalents.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (object) additional information - contents depends on calendar
	   @throws  error if an invalid date or a different calendar used */
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

	/* Retrieve Haab date from a Julian date.
	   @param  jd  (number) the Julian date
	   @return  (number[2]) corresponding Haab month and day */
	_toHaab: function(jd) {
		jd -= this.jdEpoch;
		var day = mod(jd + 8 + ((18 - 1) * 20), 365);
		return [Math.floor(day / 20) + 1, mod(day, 20)];
	},

	/* Retrieve Tzolkin date from a Julian date.
	   @param  jd  (number) the Julian date
	   @return  (number[2]) corresponding Tzolkin day and trecena */
	_toTzolkin: function(jd) {
		jd -= this.jdEpoch;
		return [amod(jd + 20, 20), amod(jd + 4, 13)];
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
		return date.day() + (date.month() * 20) + (date.year() * 360) + this.jdEpoch;
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
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