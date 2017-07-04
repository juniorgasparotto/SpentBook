$(document).ready(function () {
	$(window).resize($.debounce(250, resizeJquerySteps));
});

function resizeJquerySteps() {
    $('.wizard .content').animate({
        height: $('.body.current').outerHeight()
    }, 'slow');
}