$("#getting-started")
    .countdown("2017/04/7", function(event) {
        $(this).html(event.strftime('<ul class="coming-date coming-date-black"><li>%D <span>Days</span></li><li class="colon">:</li><li>%H <span>Hours</span></li><li class="colon">:</li><li>%M <span>Minutes</span></li><li class="colon">:</li><li>%S <span>Seconds</span></li></ul>'));
});
