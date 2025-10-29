/* http://keith-wood.name/calendars.html
   Hebrew localisation for Hebrew calendar for jQuery v2.2.0.
   Amir Hardon (ahardon at gmail dot com). */
(function($) {
	'use strict';
	$.calendars.calendars.hebrew.prototype.regionalOptions.he = {
		name: 'הלוח העברי',
		epochs: ['BAM', 'AM'],
		monthNames: ['ינואר','פברואר','מרץ','אפריל','מאי','יוני',
		'יולי','אוגוסט','ספטמבר','אוקטובר','נובמבר','דצמבר'],
		monthNamesShort: ['1','2','3','4','5','6',
		'7','8','9','10','11','12'],
		dayNames: ['ראשון','שני','שלישי','רביעי','חמישי','שישי','שבת'],
		dayNamesShort: ['א\'','ב\'','ג\'','ד\'','ה\'','ו\'','שבת'],
		dayNamesMin: ['א\'','ב\'','ג\'','ד\'','ה\'','ו\'','שבת'],
		digits: null,
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: true
	};
})(jQuery);
