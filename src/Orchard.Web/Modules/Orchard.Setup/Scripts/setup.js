(function () {
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

        $(".embedded").change(function() {
            alert($(this).find(":selected").attr("selected"));
            
        });

        var handleVisibility = function() {
            if ($("#sqlserver").is(":checked") || $("#mysql").is(":checked")) {
                $("#databaseDetails").show();

                if ($("#sqlserver").is(":checked")) {
                    $("#sqlserverDetails").show();
                    $("#mysqlDetails").hide();
                } else {
                    $("#sqlserverDetails").hide();
                    $("#mysqlDetails").show();
                }
            } else {
                $("#databaseDetails").hide();
            }
        };

        

        $("input[name='DatabaseProvider']").change(handleVisibility);
        var ourClicked = $("input[name='DatabaseProvider'][checked='checked']");

        $("input[name='DatabaseProvider']").not("[checked='checked']")[0].click();
        ourClicked.click();
        
    });
})(jQuery);