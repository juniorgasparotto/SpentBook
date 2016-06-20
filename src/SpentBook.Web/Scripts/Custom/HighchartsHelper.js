(function () {

    // ****************************
    // globals
    // ****************************
    window.HighchartsHelper = window.HighchartsHelper || {};

    // ****************************
    // public methods
    // ****************************

    HighchartsHelper = HighchartsHelper.prototype = {
        SetChart: function(chart, categories, series, drilldown) {
            if (categories)
                chartSet.xAxis[0].setCategories(categories);

            while (chart.series.length > 0) {
                chart.series[0].remove(true);
            }
            
            for (var index in series) {
                chart.addSeries(series[index]);

            }

            if (chart.options.drilldown) {
                chart.options.drilldown.series = drilldown;
            }
        },       
        SumByCategory: function (chart) {
            var series = chart.series,
            axis = chart.xAxis[0],
            categories = axis.categories,
            cl = categories.length,
            sl = series.length,
            i = 0, j, sum;

            for (; i < cl; i++) {

                for (j = 0, sum = 0; j < sl; j++)
                    sum += series[j].data[i].y

                categories[i] += '<br/> <b>Total:</b> (' + Highcharts.numberFormat(sum) + ')';
            }

            //chart.setSize(chart.chartWidth, chart.chartHeight);
            chart.redraw();
        },
    }
}());