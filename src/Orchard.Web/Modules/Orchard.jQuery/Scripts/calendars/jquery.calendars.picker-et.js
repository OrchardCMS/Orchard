/* http://keith-wood.name/calendars.html
   Estonian localisation for calendars datepicker for jQuery.
   Written by Mart Sõmermaa (mrts.pydev at gmail com). */
(function($) {
	$.calendars.picker.regional['et'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: 'Eelnev', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Järgnev', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Täna', currentStatus: '',
		todayText: 'Täna', todayStatus: '',
		clearText: 'X', clearStatus: '',
		closeText: 'Sulge', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['et']);
})(jQuery);
