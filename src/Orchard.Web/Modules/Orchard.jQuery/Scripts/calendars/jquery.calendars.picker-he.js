/* http://keith-wood.name/calendars.html
   Hebrew localisation for calendars datepicker for jQuery.
   Written by Amir Hardon (ahardon at gmail dot com). */
(function($) {
	$.calendars.picker.regional['he'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;הקודם', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'הבא&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'היום', currentStatus: '',
		todayText: 'היום', todayStatus: '',
		clearText: 'נקה', clearStatus: '',
		closeText: 'סגור', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: true
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['he']);
})(jQuery);
