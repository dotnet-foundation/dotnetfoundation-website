var intro = $('#splash-intro');
var content = $("#splash-content");
var intro_content = $("#splash-intro .container");

var splash_footer = $("#splash-footer");
var content_header = $("#content-header");
var navbar = $(".navbar-transparent");

$(document).ready(splashResize);
$(window).resize(splashResize);

function splashResize (){
    intro.css('min-height', $(window).height() + "px");
}
