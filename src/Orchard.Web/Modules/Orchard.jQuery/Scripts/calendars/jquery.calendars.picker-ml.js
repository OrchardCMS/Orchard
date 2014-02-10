/* http://keith-wood.name/calendars.html
   Malayalam localisation for calendars datepicker for jQuery.
   Saji Nediyanchath (saji89@gmail.com). */
(function($) {
	$.calendars.picker.regional['ml'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: 'മുന്നത്തെ', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'അടുത്തത് ', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'ഇന്ന്', currentStatus: '',
		todayText: 'ഇന്ന്', todayStatus: '',
		clearText: 'X', clearStatus: '',
		closeText: 'ശരി', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'ആ', weekStatus: '',
		dayStatus: 'DD d MM', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['ml']);
})(jQuery);
