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
                        ErrorResponse(error);
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
                        ErrorResponse(error);
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

function ErrorResponse(error)
{
    if (error.responseJSON) {
        if (error.responseJSON.message) {
            // Type: Action Exception
            alert(error.responseJSON.message)
        }
        else if (error.responseJSON[0]) {
            // Type: Invalid Model error
            var strError = "";
            for (var iField in error.responseJSON)
                for (var iErro in error.responseJSON[iField].errors)
                    strError += "- " + error.responseJSON[iField].errors[iErro] + "\r\n";
            alert(strError);
        }
    }
    else {
        // Type: Inexpected Exception
        alert(error.statusText)
    }
}