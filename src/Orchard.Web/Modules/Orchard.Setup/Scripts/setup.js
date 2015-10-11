﻿(function () {
    function show() {
        window.setTimeout(function () {
            document.getElementById("throbber").style.display = "block";
        }, 2000);
    }

    if (document.addEventListener) {
        document.addEventListener("submit", show, false);
    }
    else {
        document.forms[0].attachEvent("onsubmit", show);
    }
})();

(function($) {
    $(document).ready(function() {
        $("select.recipe").change(function() { // class="recipe" on the select element 
            var description = $(this).find(":selected").data("recipe-description"); // reads the html attribute of the selected option
            $("#recipedescription").text(description); // make the contents of <div id="recipe-description"></div> be the escaped description string
        });
        $(".data").find('input[name=DatabaseProvider]:checked').click();
    });
})(jQuery);