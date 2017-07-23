$(function(){
    $('#Container').mixItUp();

    // Coumnas
    $('#Cols1').click(function() {
        $('.mix').removeClass('col-md-4');
        $('.mix').removeClass('col-md-3');
        $('.mix').removeClass('col-md-6');
        $('.mix').addClass('col-md-12');
        $('#Cols1').addClass('active');
        $('#Cols2').removeClass('active');
        $('#Cols3').removeClass('active');
        $('#Cols4').removeClass('active');
    });
    $('#Cols2').click(function() {
        $('.mix').removeClass('col-md-4');
        $('.mix').removeClass('col-md-3');
        $('.mix').addClass('col-md-6');
        $('.mix').removeClass('col-md-12');
        $('#Cols1').removeClass('active');
        $('#Cols2').addClass('active');
        $('#Cols3').removeClass('active');
        $('#Cols4').removeClass('active');
    });
    $('#Cols3').click(function() {
        $('.mix').addClass('col-md-4');
        $('.mix').removeClass('col-md-3');
        $('.mix').removeClass('col-md-6');
        $('.mix').removeClass('col-md-12');
        $('#Cols1').removeClass('active');
        $('#Cols2').removeClass('active');
        $('#Cols3').addClass('active');
        $('#Cols4').removeClass('active');
    });
    $('#Cols4').click(function() {
        $('.mix').removeClass('col-md-4');
        $('.mix').addClass('col-md-3');
        $('.mix').removeClass('col-md-6');
        $('.mix').removeClass('col-md-12');
        $('#Cols1').removeClass('active');
        $('#Cols2').removeClass('active');
        $('#Cols3').removeClass('active');
        $('#Cols4').addClass('active');
    });

    $('#port-show').click(function() {
        $('.portfolio-item-caption').removeClass('hide');
        $('.portfolio-item-caption').addClass('show');
        $('#port-show').addClass('active');
        $('#port-hide').removeClass('active');
    });
    $('#port-hide').click(function() {
        $('.portfolio-item-caption').removeClass('show');
        $('.portfolio-item-caption').addClass('hide');
        $('#port-hide').addClass('active');
        $('#port-show').removeClass('active');
    });
});