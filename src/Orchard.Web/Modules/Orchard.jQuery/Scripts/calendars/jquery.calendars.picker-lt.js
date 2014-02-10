/* http://keith-wood.name/calendars.html
   Lithuanian localisation for calendars datepicker for jQuery.
   Arturas Paleicikas <arturas@avalon.lt>. */
(function($) {
	$.calendars.picker.regional['lt'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;Atgal',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Pirmyn&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Šiandien', currentStatus: '',
		todayText: 'Šiandien', todayStatus: '',
		clearText: 'Išvalyti', clearStatus: '',
		closeText: 'Uždaryti', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['lt']);
})(jQuery);
