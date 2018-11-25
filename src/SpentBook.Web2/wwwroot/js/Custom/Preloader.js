(function () {
    window.Preloader = window.Preloader || function (container, centerOrcssClass) {
        this.container = container;
        var interval = 0;

        if (centerOrcssClass === true)
            centerOrcssClass = 'preload-center';

        cssClass = centerOrcssClass ? centerOrcssClass : "";

        var progressWrapper = $('<div class="preload-wrapper {0}">'.format(cssClass) +
                                        '<div class="preload">' +
                                            '<div class="preload-bar">' +
                                            '</div>' +
                                        '</div>' +
                                  '</div>');
        var progress = progressWrapper.find('.preload');
        var bar = progressWrapper.find('.preload-bar');

        this.StartPanelPreloader = function () {
            this.container.append(progressWrapper);
            var widthProgress = progress.width();
            progressWrapper.fadeIn(200);

            var animateSlow = function () {
                bar.animate({ width: widthProgress + "px" }, 5000, function () {
                    bar.width(0);
                    animateSlow();
                });
            }

            animateSlow();
        };

        this.EndPanelPreloader = function () {
            bar.stop();
            var widthProgress = progress.width();
            var animateFast = function () {
                bar.animate({ width: widthProgress + "px" }, 400, function () {
                    progressWrapper.fadeOut(1000, function () {
                        progressWrapper.remove();
                    });
                });
            }

            animateFast();
        };
    }
})();