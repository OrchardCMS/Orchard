/* http://keith-wood.name/calendars.html
   Latvian localisation for calendars datepicker for jQuery.
   Arturas Paleicikas <arturas.paleicikas@metasite.net>. */
(function($) {
	$.calendars.picker.regional['lv'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: 'Iepr',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Nāka', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Šodien', currentStatus: '',
		todayText: 'Šodien', todayStatus: '',
		clearText: 'Notīrīt', clearStatus: '',
		closeText: 'Aizvērt', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Nav', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['lv']);
})(jQuery);
