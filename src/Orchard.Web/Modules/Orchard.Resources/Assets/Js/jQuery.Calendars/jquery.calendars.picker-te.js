/* http://keith-wood.name/calendars.html
   Telugu localisation for calendars datepicker for jQuery.
   Dushyanth K. */
(function($) {
	'use strict';
    $.calendarsPicker.regionalOptions.te = {
        renderer: $.calendarsPicker.defaultRenderer,
        prevText: 'మునుపటి',
		prevStatus: 'మునుపటి నెల చూపించు',
        prevJumpText: '&lt;&lt;',
		prevJumpStatus: 'మునుపటి సంవత్సరం చూపించు',
        nextText: 'తరువాత',
		nextStatus: 'వచ్చే నెల చూపించు',
        nextJumpText: '&gt;&gt;',
		nextJumpStatus: 'వచ్చే ఏడాది చూపించు',
        currentText: 'ప్రస్తుతం',
		currentStatus: 'ప్రస్తుత నెల చూపించు',
        todayText: 'ఈరొజు',
		todayStatus: 'నేటి నెల చూపించు',
        clearText: 'తొలగించు',
		clearStatus: 'అన్ని తెధీలు తొలగించు',
        closeText: 'పుర్తైనది',
		closeStatus: 'డెట్పికర్ మూసివేయి',
        yearStatus: 'సంవత్సరం మార్చండి',
		monthStatus: 'నెల మార్చండి',
        weekText: 'వారం',
		weekStatus: 'సంవత్సరంలో వారం',
        dayStatus: 'ఎంచుకొండి DD, M d, yyyy',
		defaultStatus: 'ఒక తెధిని ఎంచుకోండి',
        isRTL: false
    };
    $.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions.te);
})(jQuery);
