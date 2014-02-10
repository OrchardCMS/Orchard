/* http://keith-wood.name/calendars.html
   Spanish/Perú localisation for calendars datepicker for jQuery.
   Written by Fischer Tirado (fishdev@globant.com) of ASIX (http://www.asixonline.com). */
(function($) {
	$.calendars.picker.regional['es-PE'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;Ant', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Sig&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Hoy', currentStatus: '',
		todayText: 'Hoy', todayStatus: '',
		clearText: 'Limpiar', clearStatus: '',
		closeText: 'Cerrar', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Sm', weekStatus: '',
		dayStatus: 'DD d, MM yyyy', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['es-PE']);
})(jQuery);
