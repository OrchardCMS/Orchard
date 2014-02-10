/* http://keith-wood.name/calendars.html
   Swiss-German localisation for calendars datepicker for jQuery.
   Written by Douglas Jose & Juerg Meier. */
(function($) {
	$.calendars.picker.regional['de-CH'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;zurück', prevStatus: 'letzten Monat zeigen',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'nächster&#x3e;', nextStatus: 'nächsten Monat zeigen',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'heute', currentStatus: '',
		todayText: 'heute', todayStatus: '',
		clearText: 'löschen', clearStatus: 'aktuelles Datum löschen',
		closeText: 'schliessen', closeStatus: 'ohne Änderungen schliessen',
		yearStatus: 'anderes Jahr anzeigen', monthStatus: 'anderen Monat anzeige',
		weekText: 'Wo', weekStatus: 'Woche des Monats',
		dayStatus: 'Wähle D, M d', defaultStatus: 'Wähle ein Datum',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['de-CH']);
})(jQuery);
