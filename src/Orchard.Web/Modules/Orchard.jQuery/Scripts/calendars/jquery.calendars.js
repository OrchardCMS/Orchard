/* http://keith-wood.name/calendars.html
   Calendars for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Calendars - generic date access and manipulation. */
function Calendars() {
	this.regional = [];
	this.regional[''] = {
		invalidCalendar: 'Calendar {0} not found',
		invalidDate: 'Invalid {0} date',
		invalidMonth: 'Invalid {0} month',
		invalidYear: 'Invalid {0} year',
		differentCalendars: 'Cannot mix {0} and {1} dates'
	};
	this.local = this.regional[''];
	this.calendars = {};
	this._localCals = {};
}

$.extend(Calendars.prototype, {

	/* Obtain a calendar implementation and localisation.
	   @param  name      (string) the name of the calendar,
	                     e.g. 'gregorian' (default), 'persian', 'islamic' (optional)
	   @param  language  (string) the language code to use for localisation
	                     (optional, default English = 'en')
	   @return  the calendar and localisation
	   @throws  error if calendar not found */
	instance: function(name, language) {
		name = (name || 'gregorian').toLowerCase();
		language = language || '';
		var cal = this._localCals[name + '-' + language];
		if (!cal && this.calendars[name]) {
			cal = new this.calendars[name](language);
			this._localCals[name + '-' + language] = cal;
		}
		if (!cal) {
			throw (this.local.invalidCalendar || this.regional[''].invalidCalendar).
				replace(/\{0\}/, name);
		}
		return cal;
	},

	/* Create a new date - for today if no other parameters given.
	   @param  year      (CDate) the date to copy or
	                     (number) the year for the date
	   @param  month     (number, optional) the month for the date
	   @param  day       (number, optional) the day for the date
	   @param  calendar  (*Calendar) the underlying calendar
	                     or (string) the name of the calendar (optional, default Gregorian)
	   @param  language  (string) the language to use for localisation (optional, default English)
	   @return  (CDate) the new date
	   @throws  error if an invalid date */
	newDate: function(year, month, day, calendar, language) {
		calendar = (year != null && year.year ? year.calendar() : (typeof calendar == 'string' ?
			this.instance(calendar, language) : calendar)) || this.instance();
		return calendar.newDate(year, month, day);
	}
});

/* Generic date, based on a particular calendar.
   @param  calendar  (*Calendar) the underlying calendar implementation
   @param  year      (number) the year for this date
   @param  month     (number) the month for this date
   @param  day       (number) the day for this date
   @return  (CDate) the date object
   @throws  error if an invalid date */
function CDate(calendar, year, month, day) {
	this._calendar = calendar;
	this._year = year;
	this._month = month;
	this._day = day;
	if (this._calendar._validateLevel == 0 &&
			!this._calendar.isValid(this._year, this._month, this._day)) {
		throw ($.calendars.local.invalidDate || $.calendars.regional[''].invalidDate).
			replace(/\{0\}/, this._calendar.local.name);
	}
}

/* Pad a numeric value with leading zeroes.
   @param  value   (number) the number to format
   @param  length  (number) the minimum length
   @return  (string) the formatted number */
function pad(value, length) {
	value = '' + value;
	return '000000'.substring(0, length - value.length) + value;
}

$.extend(CDate.prototype, {

	/* Create a new date.
	   @param  year   (CDate) the date to copy or
	                  (number) the year for the date (optional, default this date)
	   @param  month  (number) the month for the date (optional)
	   @param  day    (number) the day for the date (optional)
	   @return  (CDate) the new date
	   @throws  error if an invalid date */
	newDate: function(year, month, day) {
		return this._calendar.newDate((year == null ? this : year), month, day);
	},

	/* Set or retrieve the year for this date.
	   @param  year  (number) the year for the date (optional)
	   @return  (number) the date's year (if no parameter) or
	            (CDate) the updated date
	   @throws  error if an invalid date */
	year: function(year) {
		return (arguments.length == 0 ? this._year : this.set(year, 'y'));
	},

	/* Set or retrieve the month for this date.
	   @param  month  (number) the month for the date (optional)
	   @return  (number) the date's month (if no parameter) or
	            (CDate) the updated date
	   @throws  error if an invalid date */
	month: function(month) {
		return (arguments.length == 0 ? this._month : this.set(month, 'm'));
	},

	/* Set or retrieve the day for this date.
	   @param  day  (number) the day for the date (optional)
	   @return  (number) the date's day (if no parameter) or
	            (CDate) the updated date
	   @throws  error if an invalid date */
	day: function(day) {
		return (arguments.length == 0 ? this._day : this.set(day, 'd'));
	},

	/* Set new values for this date.
	   @param  year   (number) the year for the date
	   @param  month  (number) the month for the date
	   @param  day    (number) the day for the date
	   @return  (CDate) the updated date
	   @throws  error if an invalid date */
	date: function(year, month, day) {
		if (!this._calendar.isValid(year, month, day)) {
			throw ($.calendars.local.invalidDate || $.calendars.regional[''].invalidDate).
				replace(/\{0\}/, this._calendar.local.name);
		}
		this._year = year;
		this._month = month;
		this._day = day;
		return this;
	},

	/* Determine whether this date is in a leap year.
	   @return  (boolean) true if this is a leap year, false if not */
	leapYear: function() {
		return this._calendar.leapYear(this);
	},

	/* Retrieve the epoch designator for this date, e.g. BCE or CE.
	   @return  (string) the current epoch */
	epoch: function() {
		return this._calendar.epoch(this);
	},

	/* Format the year, if not a simple sequential number.
	   @return  (string) the formatted year */
	formatYear: function() {
		return this._calendar.formatYear(this);
	},

	/* Retrieve the month of the year for this date,
	   i.e. the month's position within a numbered year.
	   @return  (number) the month of the year: minMonth to months per year */
	monthOfYear: function() {
		return this._calendar.monthOfYear(this);
	},

	/* Retrieve the week of the year for this date.
	   @return  (number) the week of the year: 1 to weeks per year */
	weekOfYear: function() {
		return this._calendar.weekOfYear(this);
	},

	/* Retrieve the number of days in the year for this date.
	   @return  (number) the number of days in this year */
	daysInYear: function() {
		return this._calendar.daysInYear(this);
	},

	/* Retrieve the day of the year for this date.
	   @return  (number) the day of the year: 1 to days per year */
	dayOfYear: function() {
		return this._calendar.dayOfYear(this);
	},

	/* Retrieve the number of days in the month for this date.
	   @return  (number) the number of days */
	daysInMonth: function() {
		return this._calendar.daysInMonth(this);
	},

	/* Retrieve the day of the week for this date.
	   @return  (number) the day of the week: 0 to number of days - 1 */
	dayOfWeek: function() {
		return this._calendar.dayOfWeek(this);
	},

	/* Determine whether this date is a week day.
	   @return  (boolean) true if a week day, false if not */
	weekDay: function() {
		return this._calendar.weekDay(this);
	},

	/* Retrieve additional information about this date.
	   @return  (object) additional information - contents depends on calendar */
	extraInfo: function() {
		return this._calendar.extraInfo(this);
	},

	/* Add period(s) to a date.
	   @param  offset  (number) the number of periods to adjust by
	   @param  period  (string) one of 'y' for year, 'm' for month, 'w' for week, 'd' for day
	   @return  (CDate) the updated date */
	add: function(offset, period) {
		return this._calendar.add(this, offset, period);
	},

	/* Set a portion of the date.
	   @param  value   (number) the new value for the period
	   @param  period  (string) one of 'y' for year, 'm' for month, 'd' for day
	   @return  (CDate) the updated date
	   @throws  error if not a valid date */
	set: function(value, period) {
		return this._calendar.set(this, value, period);
	},

	/* Compare this date to another date.
	   @param  date  (CDate) the other date
	   @return  (number) -1 if this date is before the other date,
	            0 if they are equal, or +1 if this date is after the other date */
	compareTo: function(date) {
		if (this._calendar.name != date._calendar.name) {
			throw ($.calendars.local.differentCalendars || $.calendars.regional[''].differentCalendars).
				replace(/\{0\}/, this._calendar.local.name).replace(/\{1\}/, date._calendar.local.name);
		}
		var c = (this._year != date._year ? this._year - date._year :
			this._month != date._month ? this.monthOfYear() - date.monthOfYear() :
			this._day - date._day);
		return (c == 0 ? 0 : (c < 0 ? -1 : +1));
	},

	/* Retrieve the calendar backing this date.
	   @return  (*Calendar) the calendar implementation */
	calendar: function() {
		return this._calendar;
	},

	/* Retrieve the Julian date equivalent for this date,
	   i.e. days since January 1, 4713 BCE Greenwich noon.
	   @return  (number) the equivalent Julian date */
	toJD: function() {
		return this._calendar.toJD(this);
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
	fromJD: function(jd) {
		return this._calendar.fromJD(jd);
	},

	/* Convert this date to a standard (Gregorian) JavaScript Date.
	   @return  (Date) the equivalent JavaScript date */
	toJSDate: function() {
		return this._calendar.toJSDate(this);
	},

	/* Create a new date from a standard (Gregorian) JavaScript Date.
	   @param  jsd  (Date) the JavaScript date to convert
	   @return  (CDate) the equivalent date */
	fromJSDate: function(jsd) {
		return this._calendar.fromJSDate(jsd);
	},

	/* Convert to a string for display.
	   @return  (string) this date as a string */
	toString: function() {
		return (this.year() < 0 ? '-' : '') + pad(Math.abs(this.year()), 4) +
			'-' + pad(this.month(), 2) + '-' + pad(this.day(), 2);
	}
});

/* Basic functionality for all calendars.
   Other calendars should extend this:
   OtherCalendar.prototype = new BaseCalendar; */
function BaseCalendar() {
	this.shortYearCutoff = '+10';
}

$.extend(BaseCalendar.prototype, {
	_validateLevel: 0, // "Stack" to turn validation on/off

	/* Create a new date within this calendar - today if no parameters given.
	   @param  year   (CDate) the date to duplicate or
	                  (number) the year for the date
	   @param  month  (number) the month for the date
	   @param  day    (number) the day for the date
	   @return  (CDate) the new date
	   @throws  error if not a valid date or a different calendar used */
	newDate: function(year, month, day) {
		if (year == null) {
			return this.today();
		}
		if (year.year) {
			this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
			day = year.day();
			month = year.month();
			year = year.year();
		}
		return new CDate(this, year, month, day);
	},

	/* Create a new date for today.
	   @return  (CDate) today's date */
	today: function() {
		return this.fromJSDate(new Date());
	},

	/* Retrieve the epoch designator for this date.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (string) the current epoch
	   @throws  error if an invalid year or a different calendar used */
	epoch: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay,
			$.calendars.local.invalidYear || $.calendars.regional[''].invalidYear);
		return (date.year() < 0 ? this.local.epochs[0] : this.local.epochs[1]);
	},

	/* Format the year, if not a simple sequential number
	   @param  year  (CDate) the date to format or
	                 (number) the year to format
	   @return  (string) the formatted year
	   @throws  error if an invalid year or a different calendar used */
	formatYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay,
			$.calendars.local.invalidYear || $.calendars.regional[''].invalidYear);
		return (date.year() < 0 ? '-' : '') + pad(Math.abs(date.year()), 4)
	},

	/* Retrieve the number of months in a year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (number) the number of months
	   @throws  error if an invalid year or a different calendar used */
	monthsInYear: function(year) {
		this._validate(year, this.minMonth, this.minDay,
			$.calendars.local.invalidYear || $.calendars.regional[''].invalidYear);
		return 12;
	},

	/* Calculate the month's ordinal position within the year -
	   for those calendars that don't start at month 1!
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @return  (number) the ordinal position, starting from minMonth
	   @throws  error if an invalid year/month or a different calendar used */
	monthOfYear: function(year, month) {
		var date = this._validate(year, month, this.minDay,
			$.calendars.local.invalidMonth || $.calendars.regional[''].invalidMonth);
		return (date.month() + this.monthsInYear(date) - this.firstMonth) %
			this.monthsInYear(date) + this.minMonth;
	},

	/* Calculate actual month from ordinal position, starting from minMonth.
	   @param  year  (number) the year to examine
	   @param  ord   (number) the month's ordinal position
	   @return  (number) the month's number
	   @throws  error if an invalid year/month */
	fromMonthOfYear: function(year, ord) {
		var m = (ord + this.firstMonth - 2 * this.minMonth) %
			this.monthsInYear(year) + this.minMonth;
		this._validate(year, m, this.minDay,
			$.calendars.local.invalidMonth || $.calendars.regional[''].invalidMonth);
		return m;
	},

	/* Retrieve the number of days in a year.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @return  (number) the number of days
	   @throws  error if an invalid year or a different calendar used */
	daysInYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay,
			$.calendars.local.invalidYear || $.calendars.regional[''].invalidYear);
		return (this.leapYear(date) ? 366 : 365);
	},

	/* Retrieve the day of the year for a date.
	   @param  year   (CDate) the date to convert or
	                  (number) the year to convert
	   @param  month  (number) the month to convert
	   @param  day    (number) the day to convert
	   @return  (number) the day of the year
	   @throws  error if an invalid date or a different calendar used */
	dayOfYear: function(year, month, day) {
		var date = this._validate(year, month, day,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		return date.toJD() - this.newDate(date.year(),
			this.fromMonthOfYear(date.year(), this.minMonth), this.minDay).toJD() + 1;
	},

	/* Retrieve the number of days in a week.
	   @return  (number) the number of days */
	daysInWeek: function() {
		return 7;
	},

	/* Retrieve the day of the week for a date.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the day of the week: 0 to number of days - 1
	   @throws  error if an invalid date or a different calendar used */
	dayOfWeek: function(year, month, day) {
		var date = this._validate(year, month, day,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		return (Math.floor(this.toJD(date)) + 2) % this.daysInWeek();
	},

	/* Retrieve additional information about a date.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (object) additional information - contents depends on calendar
	   @throws  error if an invalid date or a different calendar used */
	extraInfo: function(year, month, day) {
		this._validate(year, month, day,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		return {};
	},

	/* Add period(s) to a date.
	   Cater for no year zero.
	   @param  date    (CDate) the starting date
	   @param  offset  (number) the number of periods to adjust by
	   @param  period  (string) one of 'y' for year, 'm' for month, 'w' for week, 'd' for day
	   @return  (CDate) the updated date
	   @throws  error if a different calendar used */
	add: function(date, offset, period) {
		this._validate(date, this.minMonth, this.minDay,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		return this._correctAdd(date, this._add(date, offset, period), offset, period);
	},

	/* Add period(s) to a date.
	   @param  date    (CDate) the starting date
	   @param  offset  (number) the number of periods to adjust by
	   @param  period  (string) one of 'y' for year, 'm' for month, 'w' for week, 'd' for day
	   @return  (CDate) the updated date */
	_add: function(date, offset, period) {
		this._validateLevel++;
		if (period == 'd' || period == 'w') {
			var jd = date.toJD() + offset * (period == 'w' ? this.daysInWeek() : 1);
			var d = date.calendar().fromJD(jd);
			this._validateLevel--;
			return [d.year(), d.month(), d.day()];
		}
		try {
			var y = date.year() + (period == 'y' ? offset : 0);
			var m = date.monthOfYear() + (period == 'm' ? offset : 0);
			var d = date.day();// + (period == 'd' ? offset : 0) +
				//(period == 'w' ? offset * this.daysInWeek() : 0);
			var resyncYearMonth = function(calendar) {
				while (m < calendar.minMonth) {
					y--;
					m += calendar.monthsInYear(y);
				}
				var yearMonths = calendar.monthsInYear(y);
				while (m > yearMonths - 1 + calendar.minMonth) {
					y++;
					m -= yearMonths;
					yearMonths = calendar.monthsInYear(y);
				}
			};
			if (period == 'y') {
				if (date.month() != this.fromMonthOfYear(y, m)) { // Hebrew
					m = this.newDate(y, date.month(), this.minDay).monthOfYear();
				}
				m = Math.min(m, this.monthsInYear(y));
				d = Math.min(d, this.daysInMonth(y, this.fromMonthOfYear(y, m)));
			}
			else if (period == 'm') {
				resyncYearMonth(this);
				d = Math.min(d, this.daysInMonth(y, this.fromMonthOfYear(y, m)));
			}
			var ymd = [y, this.fromMonthOfYear(y, m), d];
			this._validateLevel--;
			return ymd;
		}
		catch (e) {
			this._validateLevel--;
			throw e;
		}
	},

	/* Correct a candidate date after adding period(s) to a date.
	   Handle no year zero if necessary.
	   @param  date    (CDate) the starting date
	   @param  ymd     (number[3]) the added date
	   @param  offset  (number) the number of periods to adjust by
	   @param  period  (string) one of 'y' for year, 'm' for month, 'w' for week, 'd' for day
	   @return  (CDate) the updated date */
	_correctAdd: function(date, ymd, offset, period) {
		if (!this.hasYearZero && (period == 'y' || period == 'm')) {
			if (ymd[0] == 0 || // In year zero
					(date.year() > 0) != (ymd[0] > 0)) { // Crossed year zero
				var adj = {y: [1, 1, 'y'], m: [1, this.monthsInYear(-1), 'm'],
					w: [this.daysInWeek(), this.daysInYear(-1), 'd'],
					d: [1, this.daysInYear(-1), 'd']}[period];
				var dir = (offset < 0 ? -1 : +1);
				ymd = this._add(date, offset * adj[0] + dir * adj[1], adj[2]);
			}
		}
		return date.date(ymd[0], ymd[1], ymd[2]);
	},

	/* Set a portion of the date.
	   @param  date    (CDate) the starting date
	   @param  value   (number) the new value for the period
	   @param  period  (string) one of 'y' for year, 'm' for month, 'd' for day
	   @return  (CDate) the updated date
	   @throws  error if an invalid date or a different calendar used */
	set: function(date, value, period) {
		this._validate(date, this.minMonth, this.minDay,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		var y = (period == 'y' ? value : date.year());
		var m = (period == 'm' ? value : date.month());
		var d = (period == 'd' ? value : date.day());
		if (period == 'y' || period == 'm') {
			d = Math.min(d, this.daysInMonth(y, m));
		}
		return date.date(y, m, d);
	},

	/* Determine whether a date is valid for this calendar.
	   @param  year   (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (boolean) true if a valid date, false if not */
	isValid: function(year, month, day) {
		this._validateLevel++;
		var valid = (this.hasYearZero || year != 0);
		if (valid) {
			var date = this.newDate(year, month, this.minDay);
			valid = (month >= this.minMonth && month - this.minMonth < this.monthsInYear(date)) &&
				(day >= this.minDay && day - this.minDay < this.daysInMonth(date));
		}
		this._validateLevel--;
		return valid;
	},

	/* Convert the date to a standard (Gregorian) JavaScript Date.
	   @param  year   (CDate) the date to convert or
	                  (number) the year to convert
	   @param  month  (number) the month to convert
	   @param  day    (number) the day to convert
	   @return  (Date) the equivalent JavaScript date
	   @throws  error if an invalid date or a different calendar used */
	toJSDate: function(year, month, day) {
		var date = this._validate(year, month, day,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		return $.calendars.instance().fromJD(this.toJD(date)).toJSDate();
	},

	/* Convert the date from a standard (Gregorian) JavaScript Date.
	   @param  jsd  (Date) the JavaScript date
	   @return  (CDate) the equivalent DateUtils date */
	fromJSDate: function(jsd) {
		return this.fromJD($.calendars.instance().fromJSDate(jsd).toJD());
	},

	/* Check that a candidate date is from the same calendar and is valid.
	   @param  year   (CDate) the date to validate or
	                  (number) the year to validate
	   @param  month  (number) the month to validate
	   @param  day    (number) the day to validate
	   @param  error  (string) error message if invalid
	   @throws  error if different calendars used or invalid date */
	_validate: function(year, month, day, error) {
		if (year.year) {
			if (this._validateLevel == 0 && this.name != year.calendar().name) {
				throw ($.calendars.local.differentCalendars || $.calendars.regional[''].differentCalendars).
					replace(/\{0\}/, this.local.name).replace(/\{1\}/, year.calendar().local.name);
			}
			return year;
		}
		try {
			this._validateLevel++;
			if (this._validateLevel == 1 && !this.isValid(year, month, day)) {
				throw error.replace(/\{0\}/, this.local.name);
			}
			var date = this.newDate(year, month, day);
			this._validateLevel--;
			return date;
		}
		catch (e) {
			this._validateLevel--;
			throw e;
		}
	}
});

/* Implementation of the Proleptic Gregorian Calendar.
   See http://en.wikipedia.org/wiki/Gregorian_calendar
   and http://en.wikipedia.org/wiki/Proleptic_Gregorian_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function GregorianCalendar(language) {
	this.local = this.regional[language] || this.regional[''];
}

GregorianCalendar.prototype = new BaseCalendar;

$.extend(GregorianCalendar.prototype, {
	name: 'Gregorian', // The calendar name
	jdEpoch: 1721425.5, // Julian date of start of Gregorian epoch: 1 January 0001 CE
	daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31], // Days per month in a common year
	hasYearZero: false, // True if has a year zero, false if not
	minMonth: 1, // The minimum month number
	firstMonth: 1, // The first month in the year
	minDay: 1, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Gregorian', // The calendar name
			epochs: ['BCE', 'CE'],
			monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
			'July', 'August', 'September', 'October', 'November', 'December'],
			monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
			dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
			dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
			dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
			dateFormat: 'mm/dd/yyyy', // See format options on parseDate
			firstDay: 0, // The first day of the week, Sun = 0, Mon = 1, ...
			isRTL: false // True if right-to-left language, false if left-to-right
		}
	},
	
	/* Determine whether this date is in a leap year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	leapYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay,
			$.calendars.local.invalidYear || $.calendars.regional[''].invalidYear);
		var year = date.year() + (date.year() < 0 ? 1 : 0); // No year zero
		return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
	},

	/* Determine the week of the year for a date - ISO 8601.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the week of the year
	   @throws  error if an invalid date or a different calendar used */
	weekOfYear: function(year, month, day) {
		// Find Thursday of this week starting on Monday
		var checkDate = this.newDate(year, month, day);
		checkDate.add(4 - (checkDate.dayOfWeek() || 7), 'd');
		return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
	},

	/* Retrieve the number of days in a month.
	   @param  year   (CDate) the date to examine or
	                  (number) the year of the month
	   @param  month  (number) the month
	   @return  (number) the number of days in this month
	   @throws  error if an invalid month/year or a different calendar used */
	daysInMonth: function(year, month) {
		var date = this._validate(year, month, this.minDay,
			$.calendars.local.invalidMonth || $.calendars.regional[''].invalidMonth);
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
		var date = this._validate(year, month, day,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		year = date.year();
		month = date.month();
		day = date.day();
		if (year < 0) { year++; } // No year zero
		// Jean Meeus algorithm, "Astronomical Algorithms", 1991
		if (month < 3) {
			month += 12;
			year--;
		}
		var a = Math.floor(year / 100);
		var b = 2 - a + Math.floor(a / 4);
		return Math.floor(365.25 * (year + 4716)) +
			Math.floor(30.6001 * (month + 1)) + day + b - 1524.5;
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
	fromJD: function(jd) {
		// Jean Meeus algorithm, "Astronomical Algorithms", 1991
		var z = Math.floor(jd + 0.5);
		var a = Math.floor((z - 1867216.25) / 36524.25);
		a = z + 1 + a - Math.floor(a / 4);
		var b = a + 1524;
		var c = Math.floor((b - 122.1) / 365.25);
		var d = Math.floor(365.25 * c);
		var e = Math.floor((b - d) / 30.6001);
		var day = b - d - Math.floor(e * 30.6001);
		var month = e - (e > 13.5 ? 13 : 1);
		var year = c - (month > 2.5 ? 4716 : 4715);
		if (year <= 0) { year--; } // No year zero
		return this.newDate(year, month, day);
	},

	/* Convert this date to a standard (Gregorian) JavaScript Date.
	   @param  year   (CDate) the date to convert or
	                  (number) the year to convert
	   @param  month  (number) the month to convert
	   @param  day    (number) the day to convert
	   @return  (Date) the equivalent JavaScript date
	   @throws  error if an invalid date or a different calendar used */
	toJSDate: function(year, month, day) {
		var date = this._validate(year, month, day,
			$.calendars.local.invalidDate || $.calendars.regional[''].invalidDate);
		var jsd = new Date(date.year(), date.month() - 1, date.day());
		jsd.setHours(0);
		jsd.setMinutes(0);
		jsd.setSeconds(0);
		jsd.setMilliseconds(0);
		// Hours may be non-zero on daylight saving cut-over:
		// > 12 when midnight changeover, but then cannot generate
		// midnight datetime, so jump to 1AM, otherwise reset.
		jsd.setHours(jsd.getHours() > 12 ? jsd.getHours() + 2 : 0);
		return jsd;
	},

	/* Create a new date from a standard (Gregorian) JavaScript Date.
	   @param  jsd  (Date) the JavaScript date to convert
	   @return  (CDate) the equivalent date */
	fromJSDate: function(jsd) {
		return this.newDate(jsd.getFullYear(), jsd.getMonth() + 1, jsd.getDate());
	}
});

// Singleton manager
$.calendars = new Calendars();

// Date template
$.calendars.cdate = CDate;

// Base calendar template
$.calendars.baseCalendar = BaseCalendar;

// Gregorian calendar implementation
$.calendars.calendars.gregorian = GregorianCalendar;

})(jQuery);
