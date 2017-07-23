$(document).ready(function() {
  var isOpen = false;

  $("#agm-configurator-btn").click(function() {
    if (isOpen) {
      $('#agm-configurator').animate({
        'left': "-300px"
      });
      isOpen = false;
    } else {
      $('#agm-configurator').animate({
        'left': "0px"
      });
      isOpen = true;
    }
  });
});


$(document).ready(function() {

    /* Difinimos variables */
    var url_base = urlofdoc ('styleswitcher.js');
    var url_css = url_base + "css/";
    var colorLink = $("link[href|= '" + url_css + "style']");
    var widthLink = $("link[href|= '" + url_css + "width']");
    var switchCheck = $('input[name="full-width-checkbox"]');

    // Comprobamos si ya hay cookies establecidas
    if($.cookie("reason-color-wp")) {
        colorLink.attr("href", url_css + $.cookie("reason-color-wp"));
    }

    if($.cookie("reason-width-wp")) {
        widthLink.attr("href", url_css + $.cookie("reason-width-wp") + ".css");
        $("#" + $.cookie("reason-width-wp") + "-radio").attr('checked', true);
    }

    if($.cookie("reason-header-wp")) {
        header_style ($.cookie("reason-header-wp"));
        $("#" + $.cookie("reason-header-wp") + "-radio").attr('checked', true);
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

    $('#container-option input').on('change', function() {
        var container_file = $('input[name="containerRadio"]:checked', '#container-option').val();
        widthLink.attr("href", url_css + container_file + ".css");
        $.cookie("reason-width-wp", container_file, {expires: 7, path: '/'});
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

function header_style (header_class) {
    if (header_class == "no-header") {
        $("#header-full-top").addClass("hidden-sm hidden-md hidden-lg");

        $("#header").removeClass('navbar-header-full');
        $("#ar-brand").removeClass("hidden-lg hidden-md hidden-sm");
        $("#bs-example-navbar-collapse-1").addClass("navbar-right");
    }

    else {
        $("#header").addClass('navbar-header-full');
        $("#ar-brand").addClass("hidden-lg hidden-md hidden-sm");
        $("#bs-example-navbar-collapse-1").removeClass("navbar-right");

        if (header_class == "header-full") {
            $("#header-full-top").removeClass("header-full-dark hidden-sm hidden-md hidden-lg");
            $("#header-full-top").addClass("header-full");
        }

        else if (header_class == "header-full-dark") {
            $("#header-full-top").removeClass("header-full hidden-sm hidden-md hidden-lg");
            $("#header-full-top").addClass("header-full-dark");
        }
    }
}

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
    var scriptElement = $("script[src*='styleswitcher.js']");

    myfile = scriptElement.attr("src");

    if(myfile.indexOf( jsfile ) >= 0) {
        myurl = myfile.substring( 0, myfile.indexOf( jsfile )-3);
    }

    return myurl;
}
