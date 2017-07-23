$(document).ready(wrapResize);
$(window).resize(wrapResize);

function wrapResize (){
    var nav = $('.navbar');
    var full = $('#header-full-plan');
    var wrap = $('.wrap-primary-plan');
    var wrap2 = $('.wrap-pointers');

    var sizeTop = nav.offset().top + nav.height();
    var sizeWrap = $(window).height() - sizeTop;
    wrap.css('min-height', sizeWrap + "px");
    wrap2.css('min-height', sizeWrap + "px");
}
