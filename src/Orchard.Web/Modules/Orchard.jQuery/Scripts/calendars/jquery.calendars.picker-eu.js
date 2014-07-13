/* http://keith-wood.name/calendars.html
   Basque localisation for calendars datepicker for jQuery.
   Karrikas-ek itzulia (karrikas@karrikas.com). */
(function($) {
	$.calendars.picker.regional['eu'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;Aur', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Hur&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Gaur', currentStatus: '',
		todayText: 'Gaur', todayStatus: '',
		clearText: 'X', clearStatus: '',
		closeText: 'Egina', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD d MM', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['eu']);
})(jQuery);
