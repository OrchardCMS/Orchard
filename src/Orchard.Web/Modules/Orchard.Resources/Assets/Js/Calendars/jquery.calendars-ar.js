/* http://keith-wood.name/calendars.html
   Arabic localisation for Gregorian/Julian calendars for jQuery.
   Khaled Al Horani -- خالد الحوراني -- koko.dw@gmail.com.
   Updated by Fahad Alqahtani April 2016. */
/* NOTE: monthNames are the original months names and they are the Arabic names,
   not the new months name فبراير - يناير and there isn't any Arabic roots for these months */
(function($) {
	'use strict';
	$.calendars.calendars.gregorian.prototype.regionalOptions.ar = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['كانون الثاني', 'شباط', 'آذار', 'نيسان', 'أيار', 'حزيران',
		'تموز', 'آب', 'أيلول', 'تشرين الأول', 'تشرين الثاني', 'كانون الأول'],
		monthNamesShort: 'كانون2_شباط_آذار_نيسان_آذار_حزيران_تموز_آب_أيلول_تشرين1_تشرين2_كانون1'.split('_'),
		dayNames: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesShort: 'أحد_إثنين_ثلاثاء_أربعاء_خميس_جمعة_سبت'.split('_'),
		dayNamesMin: 'ح_ن_ث_ر_خ_ج_س'.split('_'),
		digits: $.calendars.substituteDigits(['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩']),
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions.ar =
			$.calendars.calendars.gregorian.prototype.regionalOptions.ar;
	}
})(jQuery);
