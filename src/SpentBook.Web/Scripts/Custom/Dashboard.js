$(document).ready(function ($) {
    Dashboard.UpdateAllPanels();
    Dashboard.EmptyContext.hide();
    Dashboard.EnableSortable();

    //Dashboard.IntervalRef = setInterval(
    //    function () {
    //        Dashboard.UpdateAllPanels();
    //    }
    //    , 10000
    //);

    
});

(function() {
    window.Dashboard = window.Dashboard || {};

    Dashboard = Dashboard.prototype = {
        EnableSortable: function() {
            Dashboard.Context.sortable({
                handle: '.panel-heading',
                update: function (event, ui) {
                    var id = ui.item.attr("id");

                    // for final user, the position start with "1" and not "0"
                    var index = ui.item.index() + 1;

                    $.ajax({
                        type: "GET",
                        url: '/Panel/ChangePanelOrder',
                        data: { dashboardId: Dashboard.Id, panelId: id, newOrder: index},
                        success: function (json) {

                        },
                        error: function (error) {
                            ErrorResponse(error);
                        }
                    });
                }
            });
        },
        SetPanelActionsUI: function (panelHtmlObject) {
            panelHtmlObject.find("button.btn-edit").click(function (e) {
                OpenUrl($(this).find("a").attr("href"));
            });

            panelHtmlObject.find("button.btn-delete a").click(function (e) {
                e.preventDefault();
            });

            panelHtmlObject.find("button.btn-delete").click(function (e) {
                if (confirm("Deseja realmente excluir este painel?")) {
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
                }
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
                    if (changes.Deleteds) {
                        for (var panelIndex in changes.Deleteds) {
                            var panelId = changes.Deleteds[panelIndex];
                            Dashboard.DeletePanelUI(panelId);
                        }
                    }

                    var hasPanel = changes.News.length || (Dashboard.Panels && Dashboard.Panels.length);

                    if (!hasPanel) {
                        Dashboard.ShowEmptyMessage(true);
                    }
                    else {
                        Dashboard.ShowEmptyMessage(false);

                        var panelsChangeds = Enumerable
                            .From(changes.News)
                            .Union(changes.Updateds)
                            .OrderBy(function (item) { return item.PanelOrder })
                            .ToArray();

                        var itemTemplate = '<li class="panel panel-info" id="{0}">{1}</li>';

                        for (var iPanel in panelsChangeds) {
                            var panel = panelsChangeds[iPanel];
                            var panelHtmlObjectWrapper = $(itemTemplate.format(panel.Id, "Aguarde"));

                            // remove if already exists to update
                            Dashboard.Context.find("#" + panel.Id).remove();

                            // get element before (used -2 because in interface, the start position is "1" and in DOM is "0")
                            var before = Dashboard.Context.children()[panel.PanelOrder - 2];

                            if (before)
                                $(before).after(panelHtmlObjectWrapper);
                            else
                                Dashboard.Context.prepend(panelHtmlObjectWrapper);

                            Dashboard.UpdatePanelUI(panel, panelHtmlObjectWrapper);
                        }
                    }
                },
                error: function (error) {
                    ErrorResponse(error);
                }
            });
        },
        UpdatePanelUI: function (panel, panelHtmlObjectWrapper) {
            $.ajax({
                type: "GET",
                url: '/Panel/Details',
                data: { dashboardId: Dashboard.Id, panelId: panel.Id },
                dataType: "html",
                success: function (html) {
                    if (!Dashboard.Panels)
                        Dashboard.Panels = new Array();

                    var index = Dashboard.Panels.map(function (e) { return e.Id; }).indexOf(panel.Id);

                    if (index !== -1)
                        Dashboard.Panels[index] = panel;
                    else
                        Dashboard.Panels.push(panel);

                    panelHtmlObjectWrapper.html(html);
                    Dashboard.SetPanelActionsUI(panelHtmlObjectWrapper);
                },
                error: function (error) {
                    ErrorResponse(error);
                }
            });
        },
        DeletePanelUI: function (id) {
            var index = Dashboard.Panels.map(function (e) { return e.Id; }).indexOf(id);

            if (index !== -1)
                Dashboard.Panels.splice(index, 1);

            Dashboard.Context.find("#" + id).remove();
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

String.prototype.format = function () {
    var formatted = this;
    for (var arg in arguments) {
        formatted = formatted.replace("{" + arg + "}", arguments[arg]);
    }
    return formatted;
};

function OpenUrl(url) {
    var win = window.open(url, '_self');
    win.focus();
}