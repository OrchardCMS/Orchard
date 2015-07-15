$(function () {
    if (supports_html5_storage) {
        if (sessionStorage.Swatch) {
            changeSwatch();
        }

        if (sessionStorage.FluidLayout) {
            changeFluidLayout();
        }

        if (sessionStorage.StickyFooter) {
            changeStickyFooter();
        }

        if (sessionStorage.InverseNav) {
            changeInverseNav();
        }

        if (sessionStorage.NavSearch) {
            changeNavSearch();
        }
        else {
            $('#navsearch').prop('checked', true);
        }

        if (sessionStorage.HoverMenus) {
            changeHoverMenus();
        }
    }

    $('.skin-chooser-toggle').click(function () {
        $('.skin-chooser-wrap').toggleClass('show');
    });

    $('#swatches').change(function (e) {
        if (supports_html5_storage) {
            sessionStorage.Swatch = $(this).val();

            changeSwatch();
        }
        else {
            alert('Your browser does not support HTML5 Session Storage');
        }

        location.reload();
    });

    $('#fluidlayout').change(function () {
        if (supports_html5_storage) {
            sessionStorage.FluidLayout = $(this).is(':checked');

            changeFluidLayout();
        }
        else {
            alert('Your browser does not support HTML5 Session Storage');
        }
    });

    $('#stickyfooter').change(function () {
        if (supports_html5_storage) {
            sessionStorage.StickyFooter = $(this).is(':checked');

            changeStickyFooter();
        }
        else {
            alert('Your browser does not support HTML5 Session Storage');
        }
    });

    $('#inversenav').change(function () {
        if (supports_html5_storage) {
            sessionStorage.InverseNav = $(this).is(':checked');

            changeInverseNav();
        }
        else {
            alert('Your browser does not support HTML5 Session Storage');
        }
    });

    $('#navsearch').change(function () {
        if (supports_html5_storage) {
            sessionStorage.NavSearch = $(this).is(':checked');

            changeNavSearch();
        }
        else {
            alert('Your browser does not support HTML5 Session Storage');
        }
    });

    $('#hovermenus').change(function () {
        if (supports_html5_storage) {
            sessionStorage.HoverMenus = $(this).is(':checked');

            changeHoverMenus();

            location.reload();
        }
        else {
            alert('Your browser does not support HTML5 Session Storage');
        }
    });
});

function supports_html5_storage() {
    try {
        return 'localStorage' in window && window['localStorage'] !== null;
    }
    catch (e) {
        return false;
    }
}

function changeSwatch() {
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-default.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-amelia.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-cerulean.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-cosmo.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-cyborg.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-darkly.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-flatly.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-journal.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-lumen.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-readable.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-simplex.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-slate.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-spacelab.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-superhero.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-united.min.css"]').remove();
    $('link[rel=stylesheet][href="' + window.applicationBaseUrl + '/Themes/Admin.Bootstrap/Styles/site-yeti.min.css"]').remove();

    var fileref = document.createElement("link");
    fileref.setAttribute("rel", "stylesheet");
    fileref.setAttribute("type", "text/css");
    fileref.setAttribute("href", window.applicationBaseUrl + "/Themes/Admin.Bootstrap/Styles/site-" + sessionStorage.Swatch + ".min.css");
    document.getElementsByTagName("head")[0].appendChild(fileref);

    $('#swatches').val(sessionStorage.Swatch);
}

function changeFluidLayout() {
    if (sessionStorage.FluidLayout == 'true') {
        $('html').removeClass('boxed-layout');
        $('html').addClass('fluid-layout');
        $('#fluidlayout').prop('checked', true);
    }
    else {
        $('html').removeClass('fluid-layout');
        $('html').addClass('boxed-layout');
        $('#fluidlayout').prop('checked', false);
    }
}

function changeStickyFooter() {
    if (sessionStorage.StickyFooter == 'true') {
        $('html').addClass('sticky-footer');
        $('#stickyfooter').prop('checked', true);
    }
    else {
        $('html').removeClass('sticky-footer');
        $('#stickyfooter').prop('checked', false);
    }
}

function changeInverseNav() {
    if (sessionStorage.InverseNav == 'true') {
        $('.navbar-fixed-top').removeClass('navbar-default').addClass('navbar-inverse');
        $('#inversenav').prop('checked', true);
    }
    else {
        $('.navbar-fixed-top').removeClass('navbar-inverse').addClass('navbar-default');
        $('#inversenav').prop('checked', false);
    }
}

function changeNavSearch() {
    if (sessionStorage.NavSearch == 'true') {
        $('.navbar-form').toggle(true);
        $('#navsearch').prop('checked', true);
    }
    else {
        $('.navbar-form').toggle(false);
        $('#navsearch').prop('checked', false);
    }
}

function changeHoverMenus() {
    if (sessionStorage.HoverMenus == 'true') {
        var fileref = document.createElement('script')
        fileref.setAttribute("type", "text/javascript")
        fileref.setAttribute("src", window.applicationBaseUrl + "/Themes/Admin.Bootstrap/Scripts/hover-dropdown.js")
        document.getElementsByTagName("body")[0].appendChild(fileref)

        $('#hovermenus').prop('checked', true);
    }
    else {
        $("script[src='" + window.applicationBaseUrl + "/Themes/Admin.Bootstrap/Scripts/hover-dropdown.js']").remove()

        $('#hovermenus').prop('checked', false);
    }
}