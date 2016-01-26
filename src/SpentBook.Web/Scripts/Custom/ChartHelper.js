window.onload = function () {
    $('#btnUpload').click(function () {
        var formData = new FormData($('#formUpload')[0]);
        $.ajax({
            url: '/Resume/MultipleUpload',  //Server script to process data
            type: 'POST',
            xhr: function () {  // Custom XMLHttpRequest
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) { // Check if upload property exists
                    myXhr.upload.addEventListener('progress', progressHandlingFunction, false); // For handling the progress of the upload
                }
                return myXhr;
            },
            //Ajax events
            beforeSend: function () {
                $("#progressBar").show();
            },
            success: updateAll,
            error: null,
            data: formData,
            cache: false,
            contentType: false,
            processData: false
        });
    });

    var updateChartDoughnut = function () {
        $("#chart-doughnut-area").removeClass("collapse");
        $.getJSON("/Resume/GetChartDoughnut", function (json) {
            if (window.myDoughnut) {
                window.myDoughnut.clear();
                window.myDoughnut.destroy();
            }

            var doughnutData = json;
            var ctx = document.getElementById("chart-doughnut-area").getContext("2d");
            window.myDoughnut = new Chart(ctx).Doughnut(doughnutData, { responsive: true, showTooltips: true });
        });
    };

    var updateChartBar = function () {
        $("#chart-bar-area").removeClass("collapse");
        $("#legend").removeClass("collapse");
        $.getJSON("/Resume/GetChartBar?groupBy=category", function (json) {
            if (window.myBar)
            {
                window.myBar.clear();
                window.myBar.destroy();
            }

            var ctx = document.getElementById("chart-bar-area").getContext("2d");
            window.myBar = new Chart(ctx).Bar(json, {
                responsive: true,
                legendTemplate: "<% for (var i=0; i<datasets.length; i++){%><span style=\"background-color:<%=datasets[i].strokeColor%>\"><%if(datasets[i].label){%><%=datasets[i].label%><%}%> = R$ <%=SumByCategory(datasets, datasets[i].label)%> </span><%}%>"
            });
            $("#legend").html("");
            $("#legend").prepend(window.myBar.generateLegend());
        });
    };

    var updateFilesList = function () {
        $.get("/Resume/FilesList", function (json) {
            var filesContainer = $("#filesContainer #content");
            filesContainer.html(json);
        });
    };

    var updateSpentList = function () {
        $.get("/Resume/SpentsList", function (json) {
            var container = $("#spentsList #content");
            container.html(json);
        });
    };

    /*
    var updateAll = function () {
        updateChartBar();
        //updateChartDoughnut();
        updateFilesList();
        updateSpentList();
    };

    updateAll();
    */
};

function SumByCategory(datasets, label) {
    var value = 0;
    for (iDataSet in datasets) {
        var dataset = datasets[iDataSet];
        //console.log(dataset)
        if (dataset.label == label) {

            for (iData in dataset.bars)
            {
                var data = dataset.bars[iData];
                value += data.value;
            }
        }
    }

    return value;
}

function progressHandlingFunction(e) {
    if (e.lengthComputable) {
        $('progress').attr({ value: e.loaded, max: e.total });
    }
}