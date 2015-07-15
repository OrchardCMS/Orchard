
(function ($) {

    var populate = function (el, prefix) {
        var pos = 1;

        // direct children
        var children = $(el).children('li').each(function (i, child) {
            if (!prefix) prefix = '';
            child = $(child);

            // apply positions to all siblings
            child.find('.navigation-position > input').attr('value', prefix + pos);

            // recurse position for children
            child.children('ol').each(function (i, item) { populate(item, prefix + pos.toString() + '.') });

            pos++;

        });
    };

    $('.navigation-menu > ol').nestedSortable({
        disableNesting: 'no-nest',
        forcePlaceholderSize: true,
        handle: 'div',
        helper: 'clone',
        items: 'li',
        maxLevels: 6,
        opacity: 1,
        placeholder: 'navigation-placeholder',
        revert: 50,
        tabSize: 30,
        tolerance: 'pointer',
        toleranceElement: '> div',

        stop: function (event, ui) {
            // update all positions whenever a menu item was moved
            populate(this, '');
            showMessage("warning", "You need to hit \"Save All\" in order to save your changes.", "1");
            //$('#save-message').show();

            // display a message on leave if changes have been made
            window.onbeforeunload = function (e) {
                return leaveConfirmation;
            };

            // cancel leaving message on save
            $('#saveButton').click(function (e) {
                window.onbeforeunload = function () { };
            });
        }
    });

    //TODO : we need to get localized message
    //To avoid duplicate messages; name your ID with numbers
    function showMessage(type, message, id) {
        if ($("#"+id).length == 0) {
            $("#messages").append("<div id=\"" + id + "\" class=\"alert alert-" + type + " alert-dismissible\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\"><span aria-hidden=\"true\">×</span><span class=\"sr-only\">Close</span></button>" + message + "</div>");
        }
    }

})(jQuery);
