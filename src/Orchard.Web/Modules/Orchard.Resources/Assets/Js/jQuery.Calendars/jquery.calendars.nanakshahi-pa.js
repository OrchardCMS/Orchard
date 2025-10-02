/* http://keith-wood.name/calendars.html
   Punjabi localisation for Nanakshahi calendar for jQuery v2.2.0.
   Written by Sarbjit Singh January 2016. */
(function($) {
	'use strict';
	$.calendars.calendars.nanakshahi.prototype.regionalOptions.pa = {
		name: 'Nanakshahi',
		epochs: ['BN', 'AN'],
		monthNames: ['ਚੇਤ', 'ਵੈਸਾਖ', 'ਜੇਠ', 'ਹਾੜ', 'ਸਾਵਣ', 'ਭਾਦੋਂ', 'ਅੱਸੂ', 'ਕੱਤਕ', 'ਮੱਘਰ', 'ਪੋਹ', 'ਮਾਘ', 'ਫੱਗਣ'],
		monthNamesShort: ['ਚੇ', 'ਵੈ', 'ਜੇ', 'ਹਾ', 'ਸਾ', 'ਭਾ', 'ਅੱ', 'ਕੱ', 'ਮੱ', 'ਪੋ', 'ਮਾ', 'ਫੱ'],
		dayNames: ['ਐਤਵਾਰ', 'ਸੋਮਵਾਰ', 'ਮੰਗਲਵਾਰ', 'ਬੁੱਧਵਾਰ', 'ਵੀਰਵਾਰ', 'ਸ਼ੁੱਕਰਵਾਰ', 'ਸ਼ਨਿੱਚਰਵਾਰ'],
		dayNamesShort: ['ਐਤ', 'ਸੋਮ', 'ਮੰਗਲ', 'ਬੁੱਧ', 'ਵੀਰ', 'ਸ਼ੁੱਕਰ', 'ਸ਼ਨਿੱਚਰ'],
		dayNamesMin: ['ਐ', 'ਸੋ', 'ਮੰ', 'ਬੁੱ', 'ਵੀ', 'ਸ਼ੁੱ', 'ਸ਼'],
		digits: $.calendars.substituteDigits(['੦', '੧', '੨', '੩', '੪', '੫', '੬', '੭', '੮', '੯']),
		dateFormat: 'dd-mm-yyyy',
		firstDay: 0,
		isRTL: false
	};
})(jQuery);
