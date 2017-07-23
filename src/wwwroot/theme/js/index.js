$(document).ready(function(){
    $('#latest-works').bxSlider({
        hideControlOnEnd: true,
        minSlides: 2,
        maxSlides: 4,
        slideWidth: 275,
        slideMargin: 10,
        pager: false,
        nextSelector: '#bx-next4',
        prevSelector: '#bx-prev4',
        nextText: '>',
        prevText: '<',
    });

    $('#home-block').bxSlider({
        hideControlOnEnd: true,
        minSlides: 1,
        maxSlides: 1,

        pager: false,
        nextSelector: '#bx-next5',
        prevSelector: '#bx-prev5',
        nextText: '>',
        prevText: '<',
    });
});
