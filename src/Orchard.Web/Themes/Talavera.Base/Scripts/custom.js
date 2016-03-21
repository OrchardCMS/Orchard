$(document).ready(function () {
    // NAVIGATION
    // change submenu arrow
    $('.dropdown-menu i').removeClass('fa-angle-down').addClass('fa-angle-right').addClass('pull-right');

    // make sure submenu item links work
    $('.dropdown-menu > .dropdown > a').click(function () {
        window.location.href = $(this).attr('href');
    });    

    // ALERTS
    $('.alert').alert();

    // FORMS
    $('.input-validation-error').addClass('form-control');
    $('.input-validation-error').prev('label').addClass('control-label');
    $('.input-validation-error').closest('.form-group').addClass('has-error');

    // PAGINATION
    $('#pagination ul').removeClass('pager').addClass('pagination');
    $('#pager-current').parent('li').addClass('active');

    // TO TOP
    $(window).scroll(function () {
        if ($(this).scrollTop() !== 0) {
            $("#toTop").fadeIn();
        } else {
            $("#toTop").fadeOut();
        }
    });

    $("#toTop").click(function () {
        $("body,html").animate({ scrollTop: 0 }, 2e3);
    });

    // PASSWORD CAPS LOCK ON ALERT
    $('input[type=password]').keypress(function (e) {
        kc = e.keyCode ? e.keyCode : e.which;
        sk = e.shiftKey ? e.shiftKey : ((kc == 16) ? true : false);
        if (((kc >= 65 && kc <= 90) && !sk) || ((kc >= 97 && kc <= 122) && sk)) {
            if (!$(this).next('.capsAlert').length) {
                $(this).after('<span class="capsAlert text-warning"><i class="fa fa-warning"></i>&nbsp;Caps Lock is on</span>');
            }
        }
        else {
            $(this).next('.capsAlert').remove();
        }
    });
});