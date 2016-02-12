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

(function () {

    // ****************************
    // globals
    // ****************************
    window.Dashboard = window.Dashboard || {};

    // ****************************
    // fields
    // ****************************
    var disabledWindowElement = $('<div style="width: 100%; height: 100%; position: absolute; left: 0; top: 0;" id="disabled-window"> </div>');

    // ****************************
    // private methods
    // ****************************

    var DisableWindow = function () {
        $("body").append(disabledWindowElement);
    }

    var EnabledWindow = function () {
        disabledWindowElement.remove();
    }

    var SetPanelActionsUI = function (panelHtmlObject) {
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
    };

    // ****************************
    // public methods
    // ****************************

    Dashboard = Dashboard.prototype = {
        EnableSortable: function() {
            Dashboard.Context.sortable({
                handle: '.panel-heading',
                connectWith: ".panel",
                stop: function (event, ui) {
                    DisableWindow();

                    var id = ui.item.attr("id");

                    // for final user, the position start with "1" and not "0"
                    var index = ui.item.index() + 1;

                    $.ajax({
                        type: "GET",
                        url: Dashboard.UrlChangePanelOrder,
                        data: { dashboardId: Dashboard.Id, panelId: id, newOrder: index },
                        success: function (json) {
                            EnabledWindow();
                        },
                        error: function (error) {
                            ErrorResponse(error);
                            Dashboard.Context.sortable('cancel');
                        }
                    });
                },
                update: function (event, ui) {
                    
                }
            });
        },       
        UpdateAllPanels: function () {
            $.ajax({
                type: "POST",
                url: Dashboard.UrlPanelsUpdated,
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

                        var itemTemplate = '<div class="panel panel-info {2}" id="{0}">{1}</div>';

                        for (var iPanel in panelsChangeds) {
                            var panel = panelsChangeds[iPanel];
                            var panelHtmlObjectWrapper = $(itemTemplate.format(panel.Id, "Aguarde", panel.PanelWidth));

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
                url: Dashboard.UrlPanelView,
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
                    SetPanelActionsUI(panelHtmlObjectWrapper);
                },
                error: function (error) {
                    alert("Ocorreu um erro ao tentar carregar o painel '" + panel.Id + "', tente edita-lo para resolver o problema");
                    //ErrorResponse(error);
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

function DisableWindow() {
    $("body").append('<div style="width: 100%; height: 100%; position: absolute; left: 0; top: 0;" id="disabled-window"> </div>');
}