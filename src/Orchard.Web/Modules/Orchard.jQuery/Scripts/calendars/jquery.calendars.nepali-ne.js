/* http://keith-wood.name/calendars.html
   Nepali localisation for Nepali calendar for jQuery.
   Written by Artur Neumann (ict.projects{at}nepal.inf.org) April 2013. */
(function($) {
	$.calendars.calendars.nepali.prototype.regional['ne'] = {
		name: 'Nepali', // The calendar name
		epochs: ['BBS', 'ABS'],
		monthNames: ['बैशाख', 'जेष्ठ', 'आषाढ', 'श्रावण', 'भाद्र', 'आश्विन', 'कार्तिक', 'मंसिर', 'पौष', 'माघ', 'फाल्गुन', 'चैत्र'],
		monthNamesShort: ['बै', 'जे', 'आषा', 'श्रा', 'भा', 'आश', 'का', 'मं', 'पौ', 'मा', 'फा', 'चै'],
		dayNames: ['आइतवार', 'सोमवार', 'मगलवार', 'बुधवार', 'बिहिवार', 'शुक्रवार', 'शनिवार'],
		dayNamesShort: ['आइत', 'सोम', 'मगल', 'बुध', 'बिहि', 'शुक्र', 'शनि'],
		dayNamesMin: ['आ', 'सो', 'म', 'बु', 'बि', 'शु', 'श'],
		dateFormat: 'dd/mm/yyyy', // See format options on BaseCalendar.formatDate
		firstDay: 1, // The first day of the week, Sun = 0, Mon = 1, ...
		isRTL: false, // True if right-to-left language, false if left-to-right
		showMonthAfterYear: false // True if the year select precedes month, false for month then year
	};
})(jQuery);
