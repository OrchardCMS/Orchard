
(function ($) {


    var updateMenuItems = function (e) {
        debugger;
        var id = e.target.value;
        var origParentMenuId = $("#originalParentMenuItem").val();

        var adminIndex = location.href.toLowerCase().indexOf("/admin/");
        if (adminIndex === -1) return;
        var url = location.href.substr(0, adminIndex) + "/Admin/Navigation/GetMenuItemsForDropdown/"+id;
        $.ajax({
            url: url,
            dataType: 'json',
            cache: false,
            success: function (items) {
                var menuItemDd = $("#ParentMenuItem");
                menuItemDd.append($("<option/>", {
                    value: -1,
                    text: 'Default'
                }));

                $.each(items, function (index, value) {
                    menuItemDd.append($("<option/>", {
                        value: value.MenuItemId,
                        text: addPadding(value.Text, value.Position),
                        selected: value.MenuItemId == origParentMenuId
                    }));
                });
            }
        });
        e.preventDefault();
    }

    var addPadding = function (menuText, position) {
        var ss = position.split('.');
        var str = "&nbsp;&nbsp;&nbsp;";
        for (var i = 1; i < ss.length; i++) {
            menuText = htmlDecode(str + menuText);
        }
        return menuText;
    }
    var htmlDecode = function (value) {
        return $('<div/>').html(value).text();
    }

    var selectedMenu = $("#currentMenuId");
    selectedMenu.change(updateMenuItems);
    selectedMenu.trigger("change");

})(jQuery);
