/* http://keith-wood.name/calendars.html
   Farsi/Iranian localisation for Iranian calendar for jQuery v2.2.0.
   From https://www.jqueryscript.net/demo/Persian-Jalali-Calendar-Data-Picker-Plugin-With-jQuery-kamaDatepicker/ March 2025. */
(function($) {
	'use strict';
	$.calendars.calendars.persian.prototype.regionalOptions.fa = {
		name: 'Iranian',
		epochs: ['BSH', 'SH'],
		monthNames: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		monthNamesShort: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		dayNames: ['یکشنبه', 'دوشنبه', 'سه شنبه', 'چهارشنبه', 'پنج شنبه', 'جمعه', 'شنبه'],
		dayNamesShort: ['يک', 'دو', 'سه', 'چهار', 'پنج', 'جمعه', 'شنبه'],
		dayNamesMin: ['ی', 'د', 'س', 'چ', 'پ', 'ج', 'ش'],
		digits: $.calendars.substituteDigits(['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹']),
		dateFormat: 'yyyy/mm/dd',
		firstDay: 6,
		isRTL: true
	};
})(jQuery);