/* http://keith-wood.name/calendars.html
   Arabic localisation for calendars datepicker for jQuery.
   Khaled Al Horani -- خالد الحوراني -- koko.dw@gmail.com */
(function($) {
	$.calendars.picker.regional['ar'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;السابق', prevStatus: 'عرض الشهر السابق',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'التالي&#x3e;', nextStatus: 'عرض الشهر القادم',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'اليوم', currentStatus: 'عرض الشهر الحالي',
		todayText: 'اليوم', todayStatus: 'عرض الشهر الحالي',
		clearText: 'مسح', clearStatus: 'امسح التاريخ الحالي',
		closeText: 'إغلاق', closeStatus: 'إغلاق بدون حفظ',
		yearStatus: 'عرض سنة آخرى', monthStatus: 'عرض شهر آخر',
		weekText: 'أسبوع', weekStatus: 'أسبوع السنة',
		dayStatus: 'اختر D, M d', defaultStatus: 'اختر يوم',
		isRTL: true
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['ar']);
})(jQuery);
