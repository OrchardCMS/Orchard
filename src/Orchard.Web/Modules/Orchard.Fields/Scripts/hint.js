$(document).ready(function () {
    $("label.forpicker").each(function () {
        var $this = $(this);
        var pickerInput = $("#" + $this.attr("for"));
        pickerInput.data("hint", $this.text());
        if (!pickerInput.val()) {
            pickerInput.addClass("hinted")
                .val(pickerInput.data("hint"))
                .focus(function () { var $this = $(this); if ($this.val() == $this.data("hint")) { $this.removeClass("hinted").val("") } })
                .blur(function () { var $this = $(this); setTimeout(function () { if (!$this.val()) { $this.addClass("hinted").val($this.data("hint")) } }, 300) });
        }
    });
});