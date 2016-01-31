$(document).ready(function ($) {
    // use date parser for all JSON.parse() requests
    // make sure to call before any JSON conversions
    //    JSON.useDateParser();


    Dashboard.UpdateAllPanels();
    Dashboard.EmptyContext.hide();

    Dashboard.IntervalRef = setInterval(
        function () {
            Dashboard.UpdateAllPanels();
        }
        , 10000
    );

    Dashboard.Context.sortable({
        // Only make the .panel-heading child elements support dragging.
        // Omit this to make then entire <li>...</li> draggable.
        handle: '.panel-heading',
        update: function () {

        }
    });
});

(function() {
    window.Dashboard = window.Dashboard || {};

    Dashboard = Dashboard.prototype = {
        SetPanelActionsUI: function (panelHtmlObject) {
            //var panelList = Dashboard.Context;

            //panelList.sortable({
            //    // Only make the .panel-heading child elements support dragging.
            //    // Omit this to make then entire <li>...</li> draggable.
            //    handle: '.panel-heading',
            //    update: function () {
            //        panelList.find('.panel', panelList).each(function (index, elem) {
            //            var $listItem = $(elem)
            //            var newIndex = $listItem.index();

            //            alert(newIndex)

            //            // Persist the new indices.
            //        });
            //    }
            //});

            panelHtmlObject.find("button.btn-delete a").click(function (e) {
                e.preventDefault();
            });

            panelHtmlObject.find("button.btn-delete").click(function (e) {
                $.ajax({
                    type: "GET",
                    url: $(this).find("a").attr("href"),
                    success: function (html) {
                        Dashboard.UpdateAllPanels();
                    },
                    error: function (error) {
                        ErrorResponse(error);
                    }
                });
            });
        },
        UpdateAllPanels: function () {
            $.ajax({
                type: "POST",
                url: '/Panel/PanelsUpdated',
                data: JSON.stringify({ dashboardId: Dashboard.Id, panelsExistsInInterface: Dashboard.Panels }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (changes) {
                    if (changes.News) {
                        if (changes.News.length > 0)
                            Dashboard.ShowEmptyMessage(false);
                        else if (!Dashboard.Panels || (!Dashboard.Panels.length && !changes.News.length))
                            Dashboard.ShowEmptyMessage(true);

                        for (var panelIndex in changes.News) {
                            var panel = changes.News[panelIndex];
                            Dashboard.AddPanelUI(panel);
                        }
                    }

                    if (changes.Updateds) {
                        for (var panelIndex in changes.Updateds) {
                            var panel = changes.Updateds[panelIndex];
                            Dashboard.UpdatePanelUI(panel);
                        }
                    }

                    if (changes.Deleteds) {
                        for (var panelIndex in changes.Deleteds) {
                            var panel = changes.Deleteds[panelIndex];
                            Dashboard.DeletePanelUI(panel);
                        }
                    }

                },
                error: function (error) {
                    ErrorResponse(error);
                }
            });
        },
        AddPanelUI: function (panel) {
            $.ajax({
                type: "GET",
                url: '/Panel/Details',
                data: { dashboardId: Dashboard.Id, panelId: panel.Id },
                dataType: "html",
                success: function (html) {
                    if (!Dashboard.Panels)
                        Dashboard.Panels = new Array();
                    Dashboard.Panels.push(panel);

                    var panelHtmlObject = $(html);

                    Dashboard.Context.append(panelHtmlObject);
                    Dashboard.SetPanelActionsUI(panelHtmlObject);
                },
                error: function (error) {
                    ErrorResponse(error);
                }
            });
        },
        UpdatePanelUI: function (panel) {
            $.ajax({
                type: "GET",
                url: '/Panel/Details',
                data: { dashboardId: Dashboard.Id, panelId: panel.Id },
                dataType: "html",
                success: function (html) {
                    var index = Dashboard.Panels.map(function (e) { return e.Id; }).indexOf(panel.Id);

                    if (index !== -1)
                        Dashboard.Panels[index] = panel;

                    var panelHtmlObject = Dashboard.Context.find("." + panel.Id);
                    panelHtmlObject.replaceWith(html);
                    Dashboard.SetPanelActionsUI(panelHtmlObject);
                },
                error: function (error) {
                    ErrorResponse(error);
                }
            });
        },
        DeletePanelUI: function (panel) {
            var index = Dashboard.Panels.map(function (e) { return e.Id; }).indexOf(panel.Id);

            if (index !== -1)
                Dashboard.Panels.splice(index, 1);

            Dashboard.Context.find("." + panel.Id).remove();
            if (Dashboard.Panels.length == 0)
                Dashboard.ShowEmptyMessage(true);
        },
        ShowEmptyMessage: function (show) {
            if (show)
                Dashboard.EmptyContext.show();
            else
                Dashboard.EmptyContext.hide();
        }
    }
}());

function ErrorResponse(error) {
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