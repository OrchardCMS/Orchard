/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for Persian calendar for jQuery v2.2.0.
   Written by Sajjad Servatjoo (sajjad.servatjoo{at}gmail.com) April 2011. */
(function($) {
	'use strict';
	/* jshint -W100 */
	$.calendars.calendars.persian.prototype.regionalOptions.fa = {
		name: 'Persian',
		epochs: ['BP', 'AP'],
		monthNames: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		monthNamesShort: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		dayNames: ['یک‌شنبه', 'د‌وشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'],
		dayNamesShort: ['یک', 'دو', 'سه', 'چهار', 'پنج', 'جمعه', 'شنبه'],
		dayNamesMin: ['ی', 'د', 'س', 'چ', 'پ', 'ج', 'ش'],
		digits: $.calendars.substituteDigits(['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹']),
		dateFormat: 'yyyy/mm/dd',
		firstDay: 6,
		isRTL: true
	};
})(jQuery);