/* http://keith-wood.name/calendars.html
   Arabic localisation for Islamic calendar for jQuery v2.2.0.
   Written by Keith Wood (kbwood.au{at}gmail.com) August 2009.
   Updated by Fahad Alqahtani April 2016. */
(function($) {
	'use strict';
	$.calendars.calendars.islamic.prototype.regionalOptions.ar = {
		name: 'Islamic',
		epochs: ['BAM', 'AM'],
		monthNames: 'محرم_صفر_ربيع الأول_ربيع الثاني_جمادى الأول_جمادى الآخر_رجب_شعبان_رمضان_شوال_ذو القعدة_ذو الحجة'.split('_'),
		monthNamesShort: 'محرم_صفر_ربيع1_ربيع2_جمادى1_جمادى2_رجب_شعبان_رمضان_شوال_القعدة_الحجة'.split('_'),
		dayNames: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesShort: 'أحد_إثنين_ثلاثاء_أربعاء_خميس_جمعة_سبت'.split('_'),
		dayNamesMin: 'ح_ن_ث_ر_خ_ج_س'.split('_'),
		digits: $.calendars.substituteDigits(['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩']),
		dateFormat: 'yyyy/mm/dd',
		firstDay: 1,
		isRTL: true
	};
})(jQuery);
