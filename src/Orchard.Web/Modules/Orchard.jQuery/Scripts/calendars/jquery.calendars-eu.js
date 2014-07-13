/* http://keith-wood.name/calendars.html
   Basque localisation for Gregorian/Julian calendars for jQuery.
   Karrikas-ek itzulia (karrikas@karrikas.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regional['eu'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Urtarrila','Otsaila','Martxoa','Apirila','Maiatza','Ekaina',
		'Uztaila','Abuztua','Iraila','Urria','Azaroa','Abendua'],
		monthNamesShort: ['Urt','Ots','Mar','Api','Mai','Eka',
		'Uzt','Abu','Ira','Urr','Aza','Abe'],
		dayNames: ['Igandea','Astelehena','Asteartea','Asteazkena','Osteguna','Ostirala','Larunbata'],
		dayNamesShort: ['Iga','Ast','Ast','Ast','Ost','Ost','Lar'],
		dayNamesMin: ['Ig','As','As','As','Os','Os','La'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regional['eu'] =
			$.calendars.calendars.gregorian.prototype.regional['eu'];
	}
})(jQuery);
