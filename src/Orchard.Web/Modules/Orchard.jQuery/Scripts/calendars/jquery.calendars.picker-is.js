/* http://keith-wood.name/calendars.html
   Icelandic localisation for calendars datepicker for jQuery.
   Written by Haukur H. Thorsson (haukur@eskill.is). */
(function($) {
	$.calendars.picker.regional['is'] = {
		renderer: $.calendars.picker.defaultRenderer,
		prevText: '&#x3c; Fyrri', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'N&aelig;sti &#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: '&Iacute; dag', currentStatus: '',
		todayText: '&Iacute; dag', todayStatus: '',
		clearText: 'Hreinsa', clearStatus: '',
		closeText: 'Loka', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Vika', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendars.picker.setDefaults($.calendars.picker.regional['is']);
})(jQuery);
