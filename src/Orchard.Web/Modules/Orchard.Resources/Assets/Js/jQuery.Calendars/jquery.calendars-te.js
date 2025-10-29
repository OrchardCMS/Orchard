/* http://keith-wood.name/calendars.html
   Telugu INDIA localisation for Gregorian/Julian calendars for jQuery.
   Written by Dushyanth Karri. */
(function($) {
	'use strict';
	$.calendars.calendars.gregorian.prototype.regionalOptions.te = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['జనవరి', 'ఫిబ్రవరి', 'మార్చ్', 'ఎప్రిల్', 'మె', 'జున్',
		'జులై', 'ఆగస్ట్', 'సెప్టెంబర్', 'అక్టొబర్', 'నవెంబర్', 'డిసెంబర్'],
		monthNamesShort: ['जन', 'फर', 'మార్చ్', 'ఎప్రిల్', 'మె', 'జున్', 'జులై', 'ఆగ్', 'సెప్', 'అక్ట్', 'నొవ్', 'డిస్'],
		dayNames: ['ఆధివారం', 'సొమవారం', 'మంగ్లవారం', 'బుధవారం', 'గురువారం', 'శుక్రవారం', 'శనివారం'],
		dayNamesShort: ['ఆధి', 'సొమ', 'మంగ్ల', 'బుధ', 'గురు', 'శుక్ర', 'శని'],
		dayNamesMin: ['ఆ','సొ','మం','బు','గు','శు','శ'],
		digits: null,
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions.te =
			$.calendars.calendars.gregorian.prototype.regionalOptions.te;
	}
})(jQuery);