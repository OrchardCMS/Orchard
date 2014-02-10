/* http://keith-wood.name/calendars.html
   Italian localisation for calendars datepicker for jQuery.
   Written by Apaella (apaella@gmail.com). */
(function($) {
	$.calendars.picker.regional['it'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;Prec', prevStatus: 'Mese precedente',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Mostra l\'anno precedente',
		nextText: 'Succ&#x3e;', nextStatus: 'Mese successivo',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Mostra l\'anno successivo',
		currentText: 'Oggi', currentStatus: 'Mese corrente',
		todayText: 'Oggi', todayStatus: 'Mese corrente',
		clearText: 'Svuota', clearStatus: 'Annulla',
		closeText: 'Chiudi', closeStatus: 'Chiudere senza modificare',
		yearStatus: 'Seleziona un altro anno', monthStatus: 'Seleziona un altro mese',
		weekText: 'Sm', weekStatus: 'Settimana dell\'anno',
		dayStatus: '\'Seleziona\' DD, M d', defaultStatus: 'Scegliere una data',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['it']);
})(jQuery);
