/* http://keith-wood.name/calendars.html
   Iniciacion en galego para a extensión 'UI date picker' para jQuery.
   Traducido por Manuel (McNuel@gmx.net). */
(function($) {
	$.calendars.picker.regional['gl'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c;Ant', prevStatus: 'Amosar mes anterior',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Amosar ano anterior',
		nextText: 'Seg&#x3e;', nextStatus: 'Amosar mes seguinte',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Amosar ano seguinte',
		currentText: 'Hoxe', currentStatus: 'Amosar mes actual',
		todayText: 'Hoxe', todayStatus: 'Amosar mes actual',
		clearText: 'Limpar', clearStatus: 'Borrar data actual',
		closeText: 'Pechar', closeStatus: 'Pechar sen gardar',
		yearStatus: 'Amosar outro ano', monthStatus: 'Amosar outro mes',
		weekHeader: 'Sm', weekStatus: 'Semana do ano',
		dayStatus: 'D, M d', defaultStatus: 'Selecciona Data',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['gl']);
})(jQuery);
