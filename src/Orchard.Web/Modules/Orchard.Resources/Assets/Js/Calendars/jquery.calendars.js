/* http://keith-wood.name/calendars.html
   Calendars for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict
	'use strict';

	function Calendars() {
		this.regionalOptions = [];
		/** Localised values.
			@memberof Calendars
			@property {string} [invalidCalendar='Calendar {0} not found']
				Error message for an unknown calendar.
			@property {string} [invalidDate='Invalid {0} date']
				Error message for an invalid date for this calendar.
			@property {string} [invalidMonth='Invalid {0} month']
				Error message for an invalid month for this calendar.
			@property {string} [invalidYear='Invalid {0} year']
				Error message for an invalid year for this calendar.
			@property {string} [differentCalendars='Cannot mix {0} and {1} dates']
				Error message for mixing different calendars. */
		this.regionalOptions[''] = {
			invalidCalendar: 'Calendar {0} not found',
			invalidDate: 'Invalid {0} date',
			invalidMonth: 'Invalid {0} month',
			invalidYear: 'Invalid {0} year',
			differentCalendars: 'Cannot mix {0} and {1} dates'
		};
		this.local = this.regionalOptions[''];
		this.calendars = {};
		this._localCals = {};
	}

	/** Create the calendars plugin.
		<p>Provides support for various world calendars in a consistent manner.</p>
		<p>Use the global instance, <code>$.calendars</code>, to access the functionality.</p>
		@class Calendars
		@example $.calendars.instance('julian').newDate(2014, 12, 25) */
	$.extend(Calendars.prototype, {

		/** Obtain a calendar implementation and localisation.
			@memberof Calendars
			@param {string} [name='gregorian'] The name of the calendar, e.g. 'gregorian', 'persian', 'islamic'.
			@param {string} [language=''] The language code to use for localisation (default is English).
			@return {Calendar} The calendar and localisation.
			@throws Error if calendar not found.
			@example $.calendars.instance()
$.calendars.instance('persian')
$.calendars.instance('hebrew', 'he') */
		instance: function(name, language) {
			name = (name || 'gregorian').toLowerCase();
			language = language || '';
			var cal = this._localCals[name + '-' + language];
			if (!cal && this.calendars[name]) {
				cal = new this.calendars[name](language);
				this._localCals[name + '-' + language] = cal;
			}
			if (!cal) {
				throw (this.local.invalidCalendar || this.regionalOptions[''].invalidCalendar).
					replace(/\{0\}/, name);
			}
			return cal;
		},

		/** Create a new date - for today if no other parameters given.
			@memberof Calendars
			@param {CDate|number} [year] The date to copy or the year for the date.
			@param {number} [month] The month for the date (if numeric <code>year</code> specified above).
			@param {number} [day] The day for the date (if numeric <code>year</code> specified above).
			@param {BaseCalendar|string} [calendar='gregorian'] The underlying calendar or the name of the calendar.
			@param {string} [language=''] The language to use for localisation (default English).
			@return {CDate} The new date.
			@throws Error if an invalid date.
			@example $.calendars.newDate()
$.calendars.newDate(otherDate)
$.calendars.newDate(2001, 1, 1)
$.calendars.newDate(1379, 10, 12, 'persian') */
		newDate: function(year, month, day, calendar, language) {
			calendar = ((typeof year !== 'undefined' && year !== null) && year.year ? year.calendar() :
				(typeof calendar === 'string' ? this.instance(calendar, language) : calendar)) || this.instance();
			return calendar.newDate(year, month, day);
		},

		/** A simple digit substitution function for localising numbers via the
			{@linkcode GregorianCalendar.regionalOptions|Calendar digits} option.
			@memberof Calendars
			@param {string[]} digits The substitute digits, for 0 through 9.
			@return {CalendarsDigits} The substitution function.
			@example digits: $.calendars.substituteDigits(['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹']) */
		substituteDigits: function(digits) {
			return function(value) {
				return (value + '').replace(/[0-9]/g, function(digit) {
					return digits[digit];
				});
			};
		},

		/** Digit substitution function for localising Chinese style numbers via the
			{@linkcode GregorianCalendar.regionalOptions|Calendar digits} option.
			@memberof Calendars
			@param {string[]} digits The substitute digits, for 0 through 9.
			@param {string[]} powers The characters denoting powers of 10, i.e. 1, 10, 100, 1000.
			@return {CalendarsDigits} The substitution function.
			@example digits: $.calendars.substituteChineseDigits(
  ['〇', '一', '二', '三', '四', '五', '六', '七', '八', '九'], ['', '十', '百', '千']) */
		substituteChineseDigits: function(digits, powers) {
			return function(value) {
				var localNumber = '';
				var power = 0;
				while (value > 0) {
					var units = value % 10;
					localNumber = (units === 0 ? '' : digits[units] + powers[power]) + localNumber;
					power++;
					value = Math.floor(value / 10);
				}
				if (localNumber.indexOf(digits[1] + powers[1]) === 0) {
					localNumber = localNumber.substr(1);
				}
				return localNumber || digits[0];
			};
		}
	});

	/** Generic date, based on a particular calendar.
		@class CDate
		@param {BaseCalendar} calendar The underlying calendar implementation.
		@param {number} year The year for this date.
		@param {number} month The month for this date.
		@param {number} day The day for this date.
		@return {CDate} The date object.
		@throws Error if an invalid date. */
	function CDate(calendar, year, month, day) {
		this._calendar = calendar;
		this._year = year;
		this._month = month;
		this._day = day;
		if (this._calendar._validateLevel === 0 &&
				!this._calendar.isValid(this._year, this._month, this._day)) {
			throw ($.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate).
				replace(/\{0\}/, this._calendar.local.name);
		}
	}

	/** Pad a numeric value with leading zeroes.
		@private
		@param {number} value The number to format.
		@param {number} length The minimum length.
		@return {string} The formatted number. */
	function pad(value, length) {
		value = '' + value;
		return '000000'.substring(0, length - value.length) + value;
	}

	$.extend(CDate.prototype, {

		/** Create a new date.
			@memberof CDate
			@param {CDate|number} [year] The date to copy or the year for the date (default to this date).
			@param {number} [month] The month for the date (if numeric <code>year</code> specified above).
			@param {number} [day] The day for the date (if numeric <code>year</code> specified above).
			@return {CDate} The new date.
			@throws Error if an invalid date.
			@example date.newDate()
date.newDate(otherDate)
date.newDate(2001, 1, 1) */
		newDate: function(year, month, day) {
			return this._calendar.newDate((typeof year === 'undefined' || year === null ? this : year), month, day);
		},

		/** Set or retrieve the year for this date.
			@memberof CDate
			@param {number} [year] The year for the date.
			@return {number|CDate} The date's year (if no parameter) or the updated date.
			@throws Error if an invalid date.
			@example date.year(2001)
var year = date.year() */
		year: function(year) {
			return (arguments.length === 0 ? this._year : this.set(year, 'y'));
		},

		/** Set or retrieve the month for this date.
			@memberof CDate
			@param {number} [month] The month for the date.
			@return {number|CDate} The date's month (if no parameter) or the updated date.
			@throws Error if an invalid date.
			@example date.month(1)
var month = date.month() */
		month: function(month) {
			return (arguments.length === 0 ? this._month : this.set(month, 'm'));
		},

		/** Set or retrieve the day for this date.
			@memberof CDate
			@param {number} [day] The day for the date.
			@return {number|CData} The date's day (if no parameter) or the updated date.
			@throws Error if an invalid date.
			@example date.day(1)
var day = date.day() */
		day: function(day) {
			return (arguments.length === 0 ? this._day : this.set(day, 'd'));
		},

		/** Set new values for this date.
			@memberof CDate
			@param {number} year The year for the date.
			@param {number} month The month for the date.
			@param {number} day The day for the date.
			@return {CDate} The updated date.
			@throws Error if an invalid date.
			@example date.date(2001, 1, 1) */
		date: function(year, month, day) {
			if (!this._calendar.isValid(year, month, day)) {
				throw ($.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate).
					replace(/\{0\}/, this._calendar.local.name);
			}
			this._year = year;
			this._month = month;
			this._day = day;
			return this;
		},

		/** Determine whether this date is in a leap year.
			@memberof CDate
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@example if (date.leapYear()) ...*/
		leapYear: function() {
			return this._calendar.leapYear(this);
		},

		/** Retrieve the epoch designator for this date, e.g. BCE or CE.
			@memberof CDate
			@return {string} The current epoch.
			@example var epoch = date.epoch() */
		epoch: function() {
			return this._calendar.epoch(this);
		},

		/** Format the year, if not a simple sequential number.
			@memberof CDate
			@return {string} The formatted year.
			@example var year = date.formatYear() */
		formatYear: function() {
			return this._calendar.formatYear(this);
		},

		/** Retrieve the month of the year for this date,
			i.e. the month's position within a numbered year.
			@memberof CDate
			@return {number} The month of the year: <code>minMonth</code> to months per year.
			@example var month = date.monthOfYear() */
		monthOfYear: function() {
			return this._calendar.monthOfYear(this);
		},

		/** Retrieve the week of the year for this date.
			@memberof CDate
			@return {number} The week of the year: 1 to weeks per year.
			@example var week = date.weekOfYear() */
		weekOfYear: function() {
			return this._calendar.weekOfYear(this);
		},

		/** Retrieve the number of days in the year for this date.
			@memberof CDate
			@return {number} The number of days in this year.
			@example var days = date.daysInYear() */
		daysInYear: function() {
			return this._calendar.daysInYear(this);
		},

		/** Retrieve the day of the year for this date.
			@memberof CDate
			@return {number} The day of the year: 1 to days per year.
			@example var doy = date.dayOfYear() */
		dayOfYear: function() {
			return this._calendar.dayOfYear(this);
		},

		/** Retrieve the number of days in the month for this date.
			@memberof CDate
			@return {number} The number of days.
			@example var days = date.daysInMonth() */
		daysInMonth: function() {
			return this._calendar.daysInMonth(this);
		},

		/** Retrieve the day of the week for this date.
			@memberof CDate
			@return {number} The day of the week: 0 to number of days - 1.
			@example var dow = date.dayOfWeek() */
		dayOfWeek: function() {
			return this._calendar.dayOfWeek(this);
		},

		/** Determine whether this date is a week day.
			@memberof CDate
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@example if (date.weekDay()) ... */
		weekDay: function() {
			return this._calendar.weekDay(this);
		},

		/** Retrieve additional information about this date.
			@memberof CDate
			@return {object} Additional information - contents depends on calendar.
			@example var info = date.extraInfo() */
		extraInfo: function() {
			return this._calendar.extraInfo(this);
		},

		/** Add period(s) to a date.
			@memberof CDate
			@param {number} offset The number of periods to adjust by.
			@param {string} period One of 'y' for years, 'm' for months, 'w' for weeks, 'd' for days.
			@return {CDate} The updated date.
			@example date.add(10, 'd') */
		add: function(offset, period) {
			return this._calendar.add(this, offset, period);
		},

		/** Set a portion of the date.
			@memberof CDate
			@param {number} value The new value for the period.
			@param {string} period One of 'y' for year, 'm' for month, 'd' for day.
			@return {CDate} The updated date.
			@throws Error if not a valid date.
			@example date.set(10, 'd') */
		set: function(value, period) {
			return this._calendar.set(this, value, period);
		},

		/** Compare this date to another date.
			@memberof CDate
			@param {CDate} date The other date.
			@return {number} -1 if this date is before the other date,
					0 if they are equal, or +1 if this date is after the other date.
			@example if (date1.compareTo(date2) < 0) ... */
		compareTo: function(date) {
			if (this._calendar.name !== date._calendar.name) {
				throw ($.calendars.local.differentCalendars || $.calendars.regionalOptions[''].differentCalendars).
					replace(/\{0\}/, this._calendar.local.name).replace(/\{1\}/, date._calendar.local.name);
			}
			var c = (this._year !== date._year ? this._year - date._year :
				this._month !== date._month ? this.monthOfYear() - date.monthOfYear() :
				this._day - date._day);
			return (c === 0 ? 0 : (c < 0 ? -1 : +1));
		},

		/** Retrieve the calendar backing this date.
			@memberof CDate
			@return {BaseCalendar} The calendar implementation.
			@example var cal = date.calendar() */
		calendar: function() {
			return this._calendar;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof CDate
			@return {number} The equivalent Julian date.
			@example var jd = date.toJD() */
		toJD: function() {
			return this._calendar.toJD(this);
		},

		/** Create a new date from a Julian date.
			@memberof CDate
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date.
			@example var date2 = date1.fromJD(jd) */
		fromJD: function(jd) {
			return this._calendar.fromJD(jd);
		},

		/** Convert this date to a standard (Gregorian) JavaScript Date.
			@memberof CDate
			@return {Date} The equivalent JavaScript date.
			@example var jsd = date.toJSDate() */
		toJSDate: function() {
			return this._calendar.toJSDate(this);
		},

		/** Create a new date from a standard (Gregorian) JavaScript Date.
			@memberof CDate
			@param {Date} jsd The JavaScript date to convert.
			@return {CDate} The equivalent date.
			@example var date2 = date1.fromJSDate(jsd) */
		fromJSDate: function(jsd) {
			return this._calendar.fromJSDate(jsd);
		},

		/** Convert to a string for display.
			@memberof CDate
			@return {string} This date as a string. */
		toString: function() {
			return (this.year() < 0 ? '-' : '') + pad(Math.abs(this.year()), 4) +
				'-' + pad(this.month(), 2) + '-' + pad(this.day(), 2);
		}
	});

	/** Basic functionality for all calendars.
		Other calendars should extend this:
		<pre>OtherCalendar.prototype = new BaseCalendar();</pre>
		@class BaseCalendar */
	function BaseCalendar() {
		this.shortYearCutoff = '+10';
	}

	$.extend(BaseCalendar.prototype, {
		_validateLevel: 0, // "Stack" to turn validation on/off

		/** Create a new date within this calendar - today if no parameters given.
			@memberof BaseCalendar
			@param {CDate|number} year The date to duplicate or the year for the date.
			@param {number} [month] The month for the date (if numeric <code>year</code> specified above).
			@param {number} [day] The day for the date (if numeric <code>year</code> specified above).
			@return {CDate} The new date.
			@throws Error if not a valid date or a different calendar is used.
			@example var date = calendar.newDate(2014, 1, 26)
var date2 = calendar.newDate(date1)
var today = calendar.newDate() */
		newDate: function(year, month, day) {
			if (typeof year === 'undefined' || year === null) {
				return this.today();
			}
			if (year.year) {
				this._validate(year, month, day,
					$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
				day = year.day();
				month = year.month();
				year = year.year();
			}
			return new CDate(this, year, month, day);
		},

		/** Create a new date for today.
			@memberof BaseCalendar
			@return {CDate} Today's date.
			@example var today = calendar.today() */
		today: function() {
			return this.fromJSDate(new Date());
		},

		/** Retrieve the epoch designator for this date.
			@memberof BaseCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {string} The current epoch.
			@throws Error if an invalid year or a different calendar is used.
			@example var epoch = calendar.epoch(date) 
var epoch = calendar.epoch(2014) */
		epoch: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			return (date.year() < 0 ? this.local.epochs[0] : this.local.epochs[1]);
		},

		/** Format the year, if not a simple sequential number
			@memberof BaseCalendar
			@param {CDate|number} year The date to format or the year to format.
			@return {string} The formatted year.
			@throws Error if an invalid year or a different calendar is used.
			@example var year = calendar.formatYear(date)
var year = calendar.formatYear(2014) */
		formatYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			return (date.year() < 0 ? '-' : '') + pad(Math.abs(date.year()), 4);
		},

		/** Retrieve the number of months in a year.
			@memberof BaseCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {number} The number of months.
			@throws Error if an invalid year or a different calendar is used.
			@example var months = calendar.monthsInYear(date)
var months = calendar.monthsInYear(2014) */
		monthsInYear: function(year) {
			this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			return 12;
		},

		/** Calculate the month's ordinal position within the year -
			for those calendars that don't start at month 1!
			@memberof BaseCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if numeric <code>year</code> specified above).
			@return {number} The ordinal position, starting from <code>minMonth</code>.
			@throws Error if an invalid year/month or a different calendar is used.
			@example var pos = calendar.monthOfYear(date)
var pos = calendar.monthOfYear(2014, 7) */
		monthOfYear: function(year, month) {
			var date = this._validate(year, month, this.minDay,
				$.calendars.local.invalidMonth || $.calendars.regionalOptions[''].invalidMonth);
			return (date.month() + this.monthsInYear(date) - this.firstMonth) %
				this.monthsInYear(date) + this.minMonth;
		},

		/** Calculate actual month from ordinal position, starting from <code>minMonth</code>.
			@memberof BaseCalendar
			@param {number} year The year to examine.
			@param {number} ord The month's ordinal position.
			@return {number} The month's number.
			@throws Error if an invalid year/month.
			@example var month = calendar.fromMonthOfYear(2014, 7) */
		fromMonthOfYear: function(year, ord) {
			var m = (ord + this.firstMonth - 2 * this.minMonth) %
				this.monthsInYear(year) + this.minMonth;
			this._validate(year, m, this.minDay,
				$.calendars.local.invalidMonth || $.calendars.regionalOptions[''].invalidMonth);
			return m;
		},

		/** Retrieve the number of days in a year.
			@memberof BaseCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {number} The number of days.
			@throws Error if an invalid year or a different calendar is used.
			@example var days = calendar.daysInYear(date)
var days = calendar.daysInYear(2014) */
		daysInYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			return (this.leapYear(date) ? 366 : 365);
		},

		/** Retrieve the day of the year for a date.
			@memberof BaseCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if numeric <code>year</code> specified above).
			@param {number} [day] The day to convert (if numeric <code>year</code> specified above).
			@return {number} The day of the year: 1 to days per year.
			@throws Error if an invalid date or a different calendar is used.
			@example var doy = calendar.dayOfYear(date)
var doy = calendar.dayOfYear(2014, 7, 1) */
		dayOfYear: function(year, month, day) {
			var date = this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
			return date.toJD() - this.newDate(date.year(),
				this.fromMonthOfYear(date.year(), this.minMonth), this.minDay).toJD() + 1;
		},

		/** Retrieve the number of days in a week.
			@memberof BaseCalendar
			@return {number} The number of days.
			@example var days = calendar.daysInWeek() */
		daysInWeek: function() {
			return 7;
		},

		/** Retrieve the day of the week for a date.
			@memberof BaseCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if numeric <code>year</code> specified above).
			@param {number} [day] The day to examine (if numeric <code>year</code> specified above).
			@return {number} The day of the week: 0 to number of days - 1.
			@throws Error if an invalid date or a different calendar is used.
			@example var dow = calendar.dayOfWeek(date)
var dow = calendar.dayOfWeek(2014, 1, 26) */
		dayOfWeek: function(year, month, day) {
			var date = this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
			return (Math.floor(this.toJD(date)) + 2) % this.daysInWeek();
		},

		/** Retrieve additional information about a date.
			@memberof BaseCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if numeric <code>year</code> specified above).
			@param {number} [day] The day to examine (if numeric <code>year</code> specified above).
			@return {object} Additional information - content depends on calendar.
			@throws Error if an invalid date or a different calendar is used.
			@example var info = calendar.extraInfo(date)
var info = calendar.extraInfo(2014, 1, 26) */
		extraInfo: function(year, month, day) {
			this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
			return {};
		},

		/** Add period(s) to a date.
			Cater for no year zero.
			@memberof BaseCalendar
			@param {CDate} date The starting date.
			@param {number} offset The number of periods to adjust by.
			@param {string} period One of 'y' for years, 'm' for months, 'w' for weeks, 'd' for days.
			@return {CDate} The updated date.
			@throws Error if a different calendar is used.
			@example calendar.add(date, 10, 'd') */
		add: function(date, offset, period) {
			this._validate(date, this.minMonth, this.minDay,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
			return this._correctAdd(date, this._add(date, offset, period), offset, period);
		},

		/** Add period(s) to a date.
			@memberof BaseCalendar
			@private
			@param {CDate} date The starting date.
			@param {number} offset The number of periods to adjust by.
			@param {string} period One of 'y' for years, 'm' for months, 'w' for weeks, 'd' for days.
			@return {number[]} The updated date as year, month, and day. */
		_add: function(date, offset, period) {
			this._validateLevel++;
			var d;
			if (period === 'd' || period === 'w') {
				var jd = date.toJD() + offset * (period === 'w' ? this.daysInWeek() : 1);
				d = date.calendar().fromJD(jd);
				this._validateLevel--;
				return [d.year(), d.month(), d.day()];
			}
			try {
				var y = date.year() + (period === 'y' ? offset : 0);
				var m = date.monthOfYear() + (period === 'm' ? offset : 0);
				d = date.day();
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
				if (period === 'y') {
					if (date.month() !== this.fromMonthOfYear(y, m)) { // Hebrew
						m = this.newDate(y, date.month(), this.minDay).monthOfYear();
					}
					m = Math.min(m, this.monthsInYear(y));
					d = Math.min(d, this.daysInMonth(y, this.fromMonthOfYear(y, m)));
				}
				else if (period === 'm') {
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

		/** Correct a candidate date after adding period(s) to a date.
			Handle no year zero if necessary.
			@memberof BaseCalendar
			@private
			@param {CDate} date The starting date.
			@param {number[]} ymd The added date.
			@param {number} offset The number of periods to adjust by.
			@param {string} period One of 'y' for years, 'm' for months, 'w' for weeks, 'd' for days.
			@return {CDate} The updated date. */
		_correctAdd: function(date, ymd, offset, period) {
			if (!this.hasYearZero && (period === 'y' || period === 'm')) {
				if (ymd[0] === 0 || // In year zero
						(date.year() > 0) !== (ymd[0] > 0)) { // Crossed year zero
					var adj = {y: [1, 1, 'y'], m: [1, this.monthsInYear(-1), 'm'],
						w: [this.daysInWeek(), this.daysInYear(-1), 'd'],
						d: [1, this.daysInYear(-1), 'd']}[period];
					var dir = (offset < 0 ? -1 : +1);
					ymd = this._add(date, offset * adj[0] + dir * adj[1], adj[2]);
				}
			}
			return date.date(ymd[0], ymd[1], ymd[2]);
		},

		/** Set a portion of the date.
			@memberof BaseCalendar
			@param {CDate} date The starting date.
			@param {number} value The new value for the period.
			@param {string} period One of 'y' for year, 'm' for month, 'd' for day.
			@return {CDate} The updated date.
			@throws Error if an invalid date or a different calendar is used.
			@example calendar.set(date, 10, 'd') */
		set: function(date, value, period) {
			this._validate(date, this.minMonth, this.minDay,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
			var y = (period === 'y' ? value : date.year());
			var m = (period === 'm' ? value : date.month());
			var d = (period === 'd' ? value : date.day());
			if (period === 'y' || period === 'm') {
				d = Math.min(d, this.daysInMonth(y, m));
			}
			return date.date(y, m, d);
		},

		/** Determine whether a date is valid for this calendar.
			@memberof BaseCalendar
			@param {number} year The year to examine.
			@param {number} month The month to examine.
			@param {number} day The day to examine.
			@return {boolean} <code>true</code> if a valid date, <code>false</code> if not.
			@example if (calendar.isValid(2014, 2, 31)) ... */
		isValid: function(year, month, day) {
			this._validateLevel++;
			var valid = (this.hasYearZero || year !== 0);
			if (valid) {
				var date = this.newDate(year, month, this.minDay);
				valid = (month >= this.minMonth && month - this.minMonth < this.monthsInYear(date)) &&
					(day >= this.minDay && day - this.minDay < this.daysInMonth(date));
			}
			this._validateLevel--;
			return valid;
		},

		/** Convert the date to a standard (Gregorian) JavaScript Date.
			@memberof BaseCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if numeric <code>year</code> specified above).
			@param {number} [day] The day to convert (if numeric <code>year</code> specified above).
			@return {Date} The equivalent JavaScript date.
			@throws Error if an invalid date or a different calendar is used.
			@example var jsd = calendar.toJSDate(date)
var jsd = calendar.toJSDate(2014, 1, 26) */
		toJSDate: function(year, month, day) {
			var date = this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
			return $.calendars.instance().fromJD(this.toJD(date)).toJSDate();
		},

		/** Convert the date from a standard (Gregorian) JavaScript Date.
			@memberof BaseCalendar
			@param {Date} jsd The JavaScript date.
			@return {CDate} The equivalent calendar date.
			@example var date = calendar.fromJSDate(jsd) */
		fromJSDate: function(jsd) {
			return this.fromJD($.calendars.instance().fromJSDate(jsd).toJD());
		},

		/** Check that a candidate date is from the same calendar and is valid.
			@memberof BaseCalendar
			@private
			@param {CDate|number} year The date to validate or the year to validate.
			@param {number} [month] The month to validate (if numeric <code>year</code> specified above).
			@param {number} [day] The day to validate (if numeric <code>year</code> specified above).
			@param {string} error Error message if invalid.
			@throws Error if an invalid date or a different calendar is used. */
		_validate: function(year, month, day, error) {
			if (year.year) {
				if (this._validateLevel === 0 && this.name !== year.calendar().name) {
					throw ($.calendars.local.differentCalendars || $.calendars.regionalOptions[''].differentCalendars).
						replace(/\{0\}/, this.local.name).replace(/\{1\}/, year.calendar().local.name);
				}
				return year;
			}
			try {
				this._validateLevel++;
				if (this._validateLevel === 1 && !this.isValid(year, month, day)) {
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

	/** Implementation of the Proleptic Gregorian Calendar.
		See <a href=":http://en.wikipedia.org/wiki/Gregorian_calendar">http://en.wikipedia.org/wiki/Gregorian_calendar</a>
		and <a href="http://en.wikipedia.org/wiki/Proleptic_Gregorian_calendar">http://en.wikipedia.org/wiki/Proleptic_Gregorian_calendar</a>.
		@class GregorianCalendar
		@augments BaseCalendar
		@param {string} [language=''] The language code (default English) for localisation. */
	function GregorianCalendar(language) {
		this.local = this.regionalOptions[language] || this.regionalOptions[''];
	}

	GregorianCalendar.prototype = new BaseCalendar();

	$.extend(GregorianCalendar.prototype, {
		/** The calendar name.
			@memberof GregorianCalendar */
		name: 'Gregorian',
		/** Julian date of start of Gregorian epoch: 1 January 0001 CE.
			@memberof GregorianCalendar */
		jdEpoch: 1721425.5,
		/** Days per month in a common year.
			@memberof GregorianCalendar */
		daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof GregorianCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof GregorianCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof GregorianCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof GregorianCalendar */
		minDay: 1,

		/** Convert a number into a localised form.
			@callback CalendarsDigits
			@param {number} value The number to convert.
			@return {string} The localised number.
			@example digits: $.calendars.substituteDigits(['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹']) */

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof GregorianCalendar
			@property {string} [name='Gregorian'] The calendar name.
			@property {string[]} [epochs=['BCE','CE']] The epoch names.
			@property {string[]} [monthNames=[...]] The long names of the months of the year.
			@property {string[]} [monthNamesShort=[...]] The short names of the months of the year.
			@property {string[]} [dayNames=[...]] The long names of the days of the week.
			@property {string[]} [dayNamesShort=[...]] The short names of the days of the week.
			@property {string[]} [dayNamesMin=[...]] The minimal names of the days of the week.
			@property {CalendarsDigits} [digits=null] Convert numbers to localised versions.
			@property {string} [dateFormat='mm/dd/yyyy'] The date format for this calendar.
					See the options on {@linkcode BaseCalendar.formatDate|formatDate} for details.
			@property {number} [firstDay=0] The number of the first day of the week, starting at 0.
			@property {boolean} [isRTL=false] <code>true</code> if this localisation reads right-to-left. */
		regionalOptions: { // Localisations
			'': {
				name: 'Gregorian',
				epochs: ['BCE', 'CE'],
				monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
				'July', 'August', 'September', 'October', 'November', 'December'],
				monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
				dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
				dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
				dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
				digits: null,
				dateFormat: 'mm/dd/yyyy',
				firstDay: 0,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof GregorianCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar is used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay,
				$.calendars.local.invalidYear || $.calendars.regionalOptions[''].invalidYear);
			year = date.year() + (date.year() < 0 ? 1 : 0); // No year zero
			return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
		},

		/** Determine the week of the year for a date - ISO 8601.
			@memberof GregorianCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if numeric <code>year</code> specified above).
			@param {number} [day] The day to examine (if numeric <code>year</code> specified above).
			@return {number} The week of the year, starting from 1.
			@throws Error if an invalid date or a different calendar is used. */
		weekOfYear: function(year, month, day) {
			// Find Thursday of this week starting on Monday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(4 - (checkDate.dayOfWeek() || 7), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a month.
			@memberof GregorianCalendar
			@param {CDate|number} year The date to examine or the year of the month.
			@param {number} [month] The month (if numeric <code>year</code> specified above).
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar is used. */
		daysInMonth: function(year, month) {
			var date = this._validate(year, month, this.minDay,
				$.calendars.local.invalidMonth || $.calendars.regionalOptions[''].invalidMonth);
			return this.daysPerMonth[date.month() - 1] +
				(date.month() === 2 && this.leapYear(date.year()) ? 1 : 0);
		},

		/** Determine whether this date is a week day.
			@memberof GregorianCalendar
			@param {CDate|number} year The date to examine or the year to examine.
			@param {number} [month] The month to examine (if numeric <code>year</code> specified above).
			@param {number} [day] The day to examine (if numeric <code>year</code> specified above).
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar is used. */
		weekDay: function(year, month, day) {
			return (this.dayOfWeek(year, month, day) || 7) < 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof GregorianCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if numeric <code>year</code> specified above).
			@param {number} [day] The day to convert (if numeric <code>year</code> specified above).
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar is used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
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

		/** Create a new date from a Julian date.
			@memberof GregorianCalendar
			@param {number} jd The Julian date to convert.
			@return {CDate} The equivalent date. */
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

		/** Convert this date to a standard (Gregorian) JavaScript Date.
			@memberof GregorianCalendar
			@param {CDate|number} year The date to convert or the year to convert.
			@param {number} [month] The month to convert (if numeric <code>year</code> specified above).
			@param {number} [day] The day to convert (if numeric <code>year</code> specified above).
			@return {Date} The equivalent JavaScript date.
			@throws Error if an invalid date or a different calendar is used. */
		toJSDate: function(year, month, day) {
			var date = this._validate(year, month, day,
				$.calendars.local.invalidDate || $.calendars.regionalOptions[''].invalidDate);
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

		/** Create a new date from a standard (Gregorian) JavaScript Date.
			@memberof GregorianCalendar
			@param {Date} jsd The JavaScript date to convert.
			@return {CDate} The equivalent date. */
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
