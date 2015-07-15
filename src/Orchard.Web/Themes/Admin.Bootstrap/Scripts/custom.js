$(document).ready(function () {
    // NAVIGATION
    // change submenu arrow
    $('.dropdown-menu i').removeClass('fa-angle-down').addClass('fa-angle-right').addClass('pull-right');

    // make sure submenu item links work
    $('.dropdown-menu > .dropdown > a').click(function () {
        window.location.href = $(this).attr('href');
    });

    // add top margin to first container below menu when using fixed navbar
    $('.navbar-wrapper .navbar-fixed-top').parent('div').next('div').css('margin-top', ($('.navbar-fixed-top').height() + 20) + 'px');

    // add bottom margin to body equal to the footer height when using sticky footer
    if ($('html').hasClass('sticky-footer')) {
        var footerHeight = $('#layout-footer').height();

        $('body').css('margin-bottom', footerHeight + 'px');
    }

    // ALERTS
    $('.alert').alert();

    // PAGINATION
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

    //MENU TOGGLE
    $("#menu-toggle").click(function (e) {
        e.preventDefault();
        $("#wrapper").toggleClass("toggled");
    });

    //PREVENT TRANSITIONS ON PAGE LOAD
    window.addEventListener('load', function load() {
        window.removeEventListener('load', load, false);
        document.body.classList.remove('load');
    }, false);
});

//Verify if object exist
jQuery.fn.exists = function () { return this.length > 0; }

//TODO add confirm dialog box
function submitForm(form, action) {
    $('input#publishActions').val(action);
    if ($('button[name="submit.BulkEdit"]').exists()) {
        var button = $('button[name="submit.BulkEdit"]').click();
    }
    else if ($('button[name="submit.BulkExecute"]').exists()) {
        var button = $('button[name="submit.BulkExecute"]').click();
    }
    else {
        form.submit();
    }
}

//add custom tooltip to buttons
$('.btn').tooltip();