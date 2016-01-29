$(document).ready(function ($) {
    Dashboard.AddNewsPanelsIfExists();
    //Dashboard.SetDragInDrop($(document));
});

(function() {
    window.Dashboard = window.Dashboard || {};

    Dashboard = Dashboard.prototype = {
        SetDragInDrop: function (panel) {
            var panelList = Dashboard.Context;

            panel.sortable({
                // Only make the .panel-heading child elements support dragging.
                // Omit this to make then entire <li>...</li> draggable.
                handle: '.panel-heading',
                update: function () {
                    panel.find('.panel', panelList).each(function (index, elem) {
                        var $listItem = $(elem),
                            newIndex = $listItem.index();

                        // Persist the new indices.
                    });
                }
            });
        },

        AddNewsPanelsIfExists: function () {
            $.ajax({
                type: "GET",
                url: '/Panel/PanelsUpdated',
                data: { dashboardId : Dashboard.Id },
                success: function (panels) {
                    if (panels) {
                        var currentPanels = Dashboard.Panels;

                        for (var panelIndex in panels)
                        {
                            //if (currentPanels) {
                            //    for (var currentPanelIndex in currentPanels) {
                            //        if (currentPanels[currentPanelIndex].Id == panels[panelIndex].Id
                            //            && currentPanels[currentPanelIndex].LastUpdateDate < panels[panelIndex].LastUpdateDate)
                            //        {

                            //        }
                            //    }
                            //    Dashboard.SetDragInDrop($(context));
                            //}

                            var hasExists = false;

                            if (currentPanels)
                                for (var currentPanelIndex in currentPanels)
                                    if (currentPanels[currentPanelIndex].Id == panels[panelIndex].Id)
                                        hasExists = true;

                            if (!hasExists)
                                Dashboard.UpdatePanel(panels[panelIndex].Id);
                        }

                        Dashboard.Panels = panels;
                    }
                },
                error: function (error) {
                    alert(error);
                    ErrorResponse(error);
                }
            });
        },
        UpdatePanel: function (panelId) {
            $.ajax({
                type: "GET",
                url: '/Panel/Details',
                data: { dashboardId: Dashboard.Id, panelId: panelId },
                dataType: "html",
                success: function (html) {
                    var newPanel = Dashboard.Context.append(html);
                    Dashboard.SetDragInDrop(newPanel);
                },
                error: function (error) {
                    alert(error);
                    ErrorResponse(error);
                }
            });
        }
    }
}());
