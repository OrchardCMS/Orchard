jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if (document.selection) {
                //For browsers like Internet Explorer
                this.focus();
                sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            } else if (this.selectionStart || this.selectionStart === "0") {
                //For browsers like Firefox and Webkit based
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        });
    }
});

jQuery.fn.extend({
    tokenized: function () {
        return $(this).each(function () {
            var _this = $(this);
            var textbox = _this;
            
            textbox.wrap("<div class=\"input-group\">");
            textbox.parent().append("<span class=\"input-group-btn\"><button data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\" class=\"btn btn-default\" type=\"button\"><i class=\"fa fa-leaf\" aria-hidden=\"true\"></i></button></span>");

            var btn = textbox.parent().find("button");
            var cList = $('<ul></ul>');
            var i = 0;
            cList.addClass('tokens-dropdown dropdown-menu scrollable-menu');
            $.tokens.forEach(function (item) {
                var li = document.createElement('li');
                var anchor = i == 0 ? $("<span></span>") : $("<a title=\"" + item.label + "\" href=\"" + item.value + "\"></a>");
                var row = document.createElement('div');
                var col1 = document.createElement('div');
                var col2 = document.createElement('div');
                var col3 = document.createElement('div');
                row.className = "row";
                col1.className = 'col-xs-12 col-sm-4';
                col2.className = 'col-xs-12 col-sm-4';
                col3.className = 'col-xs-12 col-sm-4';
                col1.innerHTML =  i == 0 ? "<strong>" + item.label + "</strong>" : item.label;
                col2.innerHTML = i == 0 ? "<strong>" + item.desc + "</strong>" : item.desc;
                col3.innerHTML = item.value == '' ? item.label : item.value;

                if (i == 0) {
                    col3.innerHTML = "<strong>" + col3.innerHTML + "</strong>";
                }

                row.appendChild(col1);
                row.appendChild(col2);
                row.appendChild(col3);

                anchor.append(row);
                li.appendChild(anchor.get(0));
                cList.append(li);
                i++;
            });

            cList.on("click", "a", function (e) {
                e.preventDefault();
                textbox.val(textbox.val() + $(this).attr("href"));
            });
            cList.insertAfter(btn);
        });
    },
});

$(function () {

    $.tokens = {};

    // Provide autocomplete behavior to tokenized inputs.
    // tokensUrl is initialized from the view.
    $.get(tokensUrl, function (data) {
        $.tokens = data;
        $(".tokenized").tokenized();
    });
});