/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for Persian calendar for jQuery.
   Written by Sajjad Servatjoo (sajjad.servatjoo{at}gmail.com) April 2011. */
(function($) {
	$.calendars.calendars.persian.prototype.regional['fa'] = {
		name: 'Persian', // The calendar name
		epochs: ['BP', 'AP'],
		monthNames: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		monthNamesShort: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		dayNames: ['يک شنبه', 'دوشنبه', 'سه شنبه', 'چهار شنبه', 'پنج شنبه', 'جمعه', 'شنبه'],
		dayNamesShort: ['يک', 'دو', 'سه', 'چهار', 'پنج', 'جمعه', 'شنبه'],
		dayNamesMin: ['ي', 'د', 'س', 'چ', 'پ', 'ج', 'ش'],
		dateFormat: 'yyyy/mm/dd', // See format options on BaseCalendar.formatDate
		firstDay: 6, // The first day of the week, Sun = 0, Mon = 1, ...
		isRTL: true // True if right-to-left language, false if left-to-right
	};
})(jQuery);