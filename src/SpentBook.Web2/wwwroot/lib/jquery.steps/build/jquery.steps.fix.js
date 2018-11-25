$(document).ready(function () {
    if (!window.jQueryStepsResize) {
        window.jQueryStepsResize = $.debounce(function () {
            $('.wizard .content').animate({
                height: $('.body.current').outerHeight()
            }, 'slow');
        }, 250);
    }

    $(window).resize(window.jQueryStepsResize);
});