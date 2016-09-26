(function (jQuery) {

    var rplus = /\+/g;

    jQuery.deserialize = function (data) {
        if (!data) return {};

        var pairs = data.split("&");
        var obj = {};

        for (var i = 0; i < pairs.length; i++) {
            var parts = pairs[i].split("=");
            var name = decodeURIComponent(parts[0].replace(rplus, "%20"));
            var value = decodeURIComponent(parts[1].replace(rplus, "%20"));
            obj[name] = value;
        }

        return obj;
    };

})(jQuery);
