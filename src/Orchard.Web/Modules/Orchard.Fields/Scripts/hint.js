var clearHint = function (self) { if (self.val() == self.data("hint")) { self.removeClass("hinted").val("") } };
var resetHint = function (self) { setTimeout(function () { if (!self.val()) { self.addClass("hinted").val(self.data("hint")) } }, 300) };

$(document).ready(function () {
    $("label.forpicker").each(function () {
        var $this = $(this);
        var pickerInput = $("#" + $this.attr("for"));
        pickerInput.data("hint", $this.text());
        if (!pickerInput.val()) {
            pickerInput.addClass("hinted")
                .val(pickerInput.data("hint"))
                .focus(function () { clearHint($(this)); })
                .blur(function () { resetHint($(this)); });
            $this.closest("form").submit(function () { clearHint(pickerInput); pickerInput = 0; });
        }
    });
});