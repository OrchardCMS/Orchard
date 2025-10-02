/* http://keith-wood.name/calendars.html
   Malagasy localisation for Gregorian/Julian calendars for jQuery.
   Fran Boon (fran@aidiq.com). */
(function($) {
	'use strict';
	$.calendars.calendars.gregorian.prototype.regionalOptions.mg = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janoary','Febroary','Martsa','Aprily','Mey','Jona',
		'Jolay','Aogositra','Septambra','Oktobra','Novambra','Desembra'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mey','Jon',
		'Jol','Aog','Sep','Okt','Nov','Des'],
		dayNames: ['Alahady','Alatsinainy','Talata','Alarobia','Alakamisy','Zoma','Sabotsy'],
		dayNamesShort: ['Alah','Alat','Tal','Alar','Alak','Zom','Sab'],
		dayNamesMin: ['Ah','At','Ta','Ar','Ak','Zo','Sa'],
		digits: null,
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions.mg =
			$.calendars.calendars.gregorian.prototype.regionalOptions.mg;
	}
})(jQuery);
