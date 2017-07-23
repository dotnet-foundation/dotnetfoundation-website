$(document).ready(function() {

    /* Difinimos variables */
    var url_base = urlofdoc ('styleswitcher_leadpage.js');
    var url_css = url_base + "css/";
    var colorLink = $("link[href|= '" + url_css + "style']");
    var widthLink = $("link[href|= '" + url_css + "width']");
    var switchCheck = $('input[name="full-width-checkbox"]');

    // Comprobamos si ya hay cookies establecidas
    if($.cookie("reason-color-wp")) {
        colorLink.attr("href", url_css + $.cookie("reason-color-wp"));
    }

    if($.cookie("reason-width-wp")) {
        widthLink.attr("href", url_css + $.cookie("reason-width-wp"));

        if ($.cookie("reason-width-wp") == "width-boxed.css" && switchCheck.bootstrapSwitch('state')) {
            switchCheck.bootstrapSwitch('state', false);
        }
    }

    if($.cookie("reason-navbar-wp")) {
        navbar_style ($.cookie("reason-navbar-wp"));
        $("#" + $.cookie("reason-navbar-wp") + "-radio").attr('checked', true);
    }


    // Comprobamos si hay cambios en los controles
    $("#color-options .color-box").click(function() {
        colorLink.attr("href", url_css + $(this).attr('rel'));
        colorLink.attr("href", url_css + $(this).attr('rel'));
        $.cookie("reason-color-wp", $(this).attr('rel'), {expires: 7, path: '/'});
        return false;
    });

    switchCheck.on('switchChange.bootstrapSwitch', function(event, state) {
        if (state) {
            widthLink.attr("href", url_css + "width-full.css");
            $.cookie("reason-width-wp", "width-full.css", {expires: 7, path: '/'}); 
        }
        else {
            widthLink.attr("href", url_css + "width-boxed.css");
            $.cookie("reason-width-wp", "width-boxed.css", {expires: 7, path: '/'});
        }
    });

    $('#header-option input').on('change', function() {
        var header_class = $('input[name="headerRadio"]:checked', '#header-option').val();
        header_style (header_class);
        $.cookie("reason-header-wp", header_class, {expires: 7, path: '/'});
    });

    $('#navbar-option input').on('change', function() {
        var navbar_class = $('input[name="navbarRadio"]:checked', '#navbar-option').val();
        navbar_style (navbar_class);
        $.cookie("reason-navbar-wp", navbar_class, {expires: 7, path: '/'});
    });

});

function navbar_style(navbar_class) {
    if(navbar_class == "navbar-light") {
         $("#header").addClass('navbar-light');
         $("#header").removeClass('navbar-dark navbar-inverse');
    }
    
    else if(navbar_class == "navbar-dark") {
         $("#header").addClass('navbar-dark');
         $("#header").removeClass('navbar-light navbar-inverse');
    }

    else if(navbar_class == "navbar-inverse") {
         $("#header").addClass('navbar-inverse');
         $("#header").removeClass('navbar-dark navbar-light');
    }
}


function urlofdoc (jsfile) {
    var i, element, myfile, myurl;
    var scriptElement = $("script[src*='styleswitcher_leadpage.js']");

    myfile = scriptElement.attr("src");

    if(myfile.indexOf( jsfile ) >= 0) {
        myurl = myfile.substring( 0, myfile.indexOf( jsfile )-3);
    }

    return myurl;
}