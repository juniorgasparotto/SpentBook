(function () {

    // ****************************
    // globals
    // ****************************
    window.Dashboard = window.Dashboard || {};

    // ****************************
    // fields
    // ****************************
    // var ...

    // ****************************
    // private methods
    // ****************************

    var SetPanelActionsUI = function (panelHtmlObject) {
        // success state
        panelHtmlObject.find("button.btn-edit").click(function (e) {
            Helper.OpenUrl($(this).find("a").attr("href"));
        });

        panelHtmlObject.find("button.btn-delete a").click(function (e) {
            e.preventDefault();
        });

        panelHtmlObject.find("button.btn-delete").click(function (e) {
            Dashboard.DeletePanelServer($(this).find("a").attr("href"));
        });

        // Error state
        panelHtmlObject.find(".error a.btn-edit").click(function (e) {
            Helper.OpenUrl($(this).attr("href"));
        });

        panelHtmlObject.find(".error a.btn-delete").click(function (e) {
            Dashboard.DeletePanelServer($(this).attr("href"));
            e.preventDefault();
        });

        
    };

    // ****************************
    // public methods
    // ****************************

    Dashboard = Dashboard.prototype = {
        PanelDataUpdateTimer: 0,
        EnableSortable: function () {
            Dashboard.Context.sortable({
                handle: '.panel-heading',
                connectWith: ".panel",
                stop: function (event, ui) {
                    Helper.DisableWindow();

                    var id = ui.item.attr("id");

                    // for final user, the position start with "1" and not "0"
                    var index = ui.item.index() + 1;

                    var preloader = new Preloader($('body'), 'preload-body');

                    $.ajax({
                        type: "GET",
                        url: Dashboard.UrlChangePanelOrder,
                        data: { dashboardId: Dashboard.Id, panelId: id, newOrder: index },
                        beforeSend: function () {
                            preloader.StartPanelPreloader();
                        },
                        complete: function () {
                            preloader.EndPanelPreloader();
                        },
                        success: function (json) {
                            Helper.EnabledWindow();
                        },
                        error: function (error) {
                            Helper.ErrorResponse(error);
                            Dashboard.Context.sortable('cancel');
                        }
                    });
                },
                update: function (event, ui) {

                }
            });
        },
        UpdateAllPanels: function () {
            var preloader = new Preloader($('body'), 'preload-body');
            
            $.ajax({
                type: "POST",
                url: Dashboard.UrlPanelsUpdated,
                data: JSON.stringify({ dashboardId: Dashboard.Id, panelsExistsInInterface: Dashboard.Panels }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                beforeSend: function () {
                    preloader.StartPanelPreloader();
                },
                complete: function () {
                    preloader.EndPanelPreloader();
                },
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

                        var itemTemplate = '<div class="panel panel-info {0}" id="{1}"></div>';

                        for (var iPanel in panelsChangeds) {
                            var panel = panelsChangeds[iPanel];
                            var panelHtmlObjectWrapper = $(itemTemplate.format(panel.PanelWidth, panel.Id));

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
                    Helper.ErrorResponse(error);
                }
            });
        },
        UpdatePanelUI: function (panel, panelHtmlObjectWrapper) {
            var content = panelHtmlObjectWrapper.find('.content-panel');
            var preloader = new Preloader(panelHtmlObjectWrapper, 'preload-panel');

            if (!Dashboard.Panels)
                Dashboard.Panels = new Array();

            var index = Dashboard.Panels.map(function (e) { return e.Id; }).indexOf(panel.Id);

            if (index !== -1)
                Dashboard.Panels[index] = panel;
            else
                Dashboard.Panels.push(panel);

            $.ajax({
                type: "GET",
                url: Dashboard.UrlPanelView,
                data: { dashboardId: Dashboard.Id, panelId: panel.Id },
                dataType: "html",
                beforeSend: function () {
                    preloader.StartPanelPreloader();
                },
                complete: function () {
                    preloader.EndPanelPreloader();
                },
                success: function (html) {
                    panelHtmlObjectWrapper.append(html);
                    SetPanelActionsUI(panelHtmlObjectWrapper);
                },
                error: function (error) {
                    var editUrl = '/Panel/Edit?dashboardId={0}&panelId={1}'.format(Dashboard.Id, panel.Id);
                    var removeUrl = '/Panel/Delete?dashboardId={0}&panelId={1}'.format(Dashboard.Id, panel.Id);
                    var msgError = "<p class='error'>Ocorreu um erro ao tentar carregar o painel '" + panel.Title + "', tente <a href='{0}' class='btn-edit'>edita-lo</a> para resolver o problema ou até mesmo <a href='{1}' class='btn-delete'>remove-lo</a></p>".format(editUrl, removeUrl);
                    panelHtmlObjectWrapper.append(msgError);
                    SetPanelActionsUI(panelHtmlObjectWrapper);
                }
            });
        },
        DeletePanelUI: function (id) {
            var index = Dashboard.Panels.map(function (e) { return e.Id; }).indexOf(id);

            if (index !== -1)
                Dashboard.Panels.splice(index, 1);

            Dashboard.Context.find("#" + id).remove();
            if (!Dashboard.Panels.length)
                Dashboard.ShowEmptyMessage(true);
        },
        DeletePanelServer: function (removeUrl) {
            if (confirm("Deseja realmente excluir este painel?")) {
                var preloader = new Preloader($('body'), 'preload-body');

                $.ajax({
                    type: "GET",
                    url: removeUrl,
                    beforeSend: function () {
                        preloader.StartPanelPreloader();
                    },
                    complete: function () {
                        preloader.EndPanelPreloader();
                    },
                    success: function (html) {
                        Dashboard.UpdateAllPanels();
                    },
                    error: function (error) {
                        Helper.ErrorResponse(error);
                    }
                });
            }
        },
        ShowEmptyMessage: function (show) {
            if (show)
                Dashboard.EmptyContext.show();
            else
                Dashboard.EmptyContext.hide();
        },
    };
}());