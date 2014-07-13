/* http://keith-wood.name/calendars.html
   Finnish localisation for calendars datepicker for jQuery.
   Written by Harri Kilpiö (harrikilpio@gmail.com). */
(function($) {
	$.calendars.picker.regional['fi'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&laquo;Edellinen', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Seuraava&raquo;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'T&auml;n&auml;&auml;n', currentStatus: '',
		todayText: 'T&auml;n&auml;&auml;n', todayStatus: '',
		clearText: 'Tyhjenn&auml;', clearStatus: '',
		closeText: 'Sulje', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Vk', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['fi']);
})(jQuery);
