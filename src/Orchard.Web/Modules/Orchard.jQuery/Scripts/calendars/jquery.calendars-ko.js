﻿/* http://keith-wood.name/calendars.html
   Korean localisation for Gregorian/Julian calendars for jQuery.
   Written by DaeKwon Kang (ncrash.dk@gmail.com), Edited by Genie. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ko'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['1월','2월','3월','4월','5월','6월',
		'7월','8월','9월','10월','11월','12월'],
		monthNamesShort: ['1월','2월','3월','4월','5월','6월',
		'7월','8월','9월','10월','11월','12월'],
		dayNames: ['일요일','월요일','화요일','수요일','목요일','금요일','토요일'],
		dayNamesShort: ['일','월','화','수','목','금','토'],
		dayNamesMin: ['일','월','화','수','목','금','토'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ko'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ko'];
	}
})(jQuery);
