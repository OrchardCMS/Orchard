/* http://keith-wood.name/timeEntry.html
   Arabic initialize for the jQuery time entry extension
   Written by Mohammad Baydoun (mab@modbay.me). */
(function($) {
	$.timeEntry.regional['ar'] = {show24Hours: false, separator: ':',
		ampmPrefix: '', ampmNames: ['ص', 'م'],
        spinnerTexts: ['الأن', 'الحقل السابق', 'الحقل التالي', 'زيادة', 'تنقيص']};
	$.timeEntry.setDefaults($.timeEntry.regional['ar']);
})(jQuery);
