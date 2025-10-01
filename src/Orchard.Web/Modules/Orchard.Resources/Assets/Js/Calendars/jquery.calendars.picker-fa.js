/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for calendars datepicker for jQuery.
   Javad Mowlanezhad -- jmowla@gmail.com. */
(function($) {
	'use strict';
	/* jshint -W100 */
	$.calendarsPicker.regionalOptions.fa = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;قبلی',
		prevStatus: 'نمایش ماه قبل',
		prevJumpText: '&#x3c;&#x3c;',
		prevJumpStatus: '',
		nextText: 'بعدی&#x3e;',
		nextStatus: 'نمایش ماه بعد',
		nextJumpText: '&#x3e;&#x3e;',
		nextJumpStatus: '',
		currentText: 'امروز',
		currentStatus: 'نمایش ماه جاری',
		todayText: 'امروز',
		todayStatus: 'نمایش ماه جاری',
		clearText: 'حذف تاریخ',
		clearStatus: 'پاک کردن تاریخ جاری',
		closeText: 'بستن',
		closeStatus: 'بستن بدون اعمال تغییرات',
		yearStatus: 'نمایش سال متفاوت',
		monthStatus: 'نمایش ماه متفاوت',
		weekText: 'هف',
		weekStatus: 'هفته‌ی سال',
		dayStatus: 'انتخاب D, M d',
		defaultStatus: 'انتخاب تاریخ',
		isRTL: true
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions.fa);
})(jQuery);
