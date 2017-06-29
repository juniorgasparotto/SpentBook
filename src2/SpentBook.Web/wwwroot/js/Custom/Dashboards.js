window.onload = function () {
    var updateDashboardBox = function () {
        $.get("/Dashboard/Dashboards", function (html) {
            var filesContainer = $("#box-container");
            filesContainer.html(html);

            filesContainer.find("button.btn-delete").click(function (e) {
                $.ajax({
                    type: "GET",
                    url: $(this).find("a").attr("href"),
                    success: function (html) {
                        updateDashboardBox();
                    },
                    error: function (error) {
                        Helper.ErrorResponse(error);
                    }
                });
            });

            filesContainer.find("#form-new-dashboard #save").click(function (e) {
                e.preventDefault();
                $.ajax({
                    type: "POST",
                    url: "/Dashboard/Create",
                    data: $("#form-new-dashboard").serialize(),
                    success: function (html) {
                        updateDashboardBox();
                    },
                    error: function (error) {
                        Helper.ErrorResponse(error);
                    }
                });
            });
        });
    };

    var updateAll = function () {
        updateDashboardBox();
    };

    updateAll();
};