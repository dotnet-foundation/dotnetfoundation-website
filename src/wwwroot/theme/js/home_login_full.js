$(document).ready(wrapResize);
$(window).resize(wrapResize);

function wrapResize (){
    var back = $('.paper-back-full');

    var size = $(window).height();
    back.css('min-height', size + "px");
}
