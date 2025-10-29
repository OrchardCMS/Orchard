/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for Gregorian/Julian calendars for jQuery.
   Javad Mowlanezhad -- jmowla@gmail.com */
(function($) {
	'use strict';
	/* jshint -W100 */
	$.calendars.calendars.gregorian.prototype.regionalOptions.fa = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['فروردین','اردیبهشت','خرداد','تیر','مرداد','شهریور',
		'مهر','آبان','آذر','دی','بهمن','اسفند'],
		monthNamesShort: ['1','2','3','4','5','6','7','8','9','10','11','12'],
		dayNames: ['یکشنبه','دوشنبه','سه‌شنبه','چهارشنبه','پنج‌شنبه','جمعه','شنبه'],
		dayNamesShort: ['ی','د','س','چ','پ','ج', 'ش'],
		dayNamesMin: ['ی','د','س','چ','پ','ج', 'ش'],
		digits: $.calendars.substituteDigits(['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹']),
		dateFormat: 'yyyy/mm/dd',
		firstDay: 6,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions.fa =
			$.calendars.calendars.gregorian.prototype.regionalOptions.fa;
	}
})(jQuery);
