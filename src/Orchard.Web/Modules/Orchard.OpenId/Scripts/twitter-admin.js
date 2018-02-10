$(document).on('click', '#colExpControl', function () {
    if ($('#colExpArea').css('display') == 'none') {
        $('#colExpArea').show(300);
        $('#colExpButton').html('-');
    } else {
        $('#colExpArea').hide(300);
        $('#colExpButton').html('+');
    }
});