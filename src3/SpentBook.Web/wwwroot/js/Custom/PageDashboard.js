$(document).ready(function ($) {
    Dashboard.Context = $("#content-panels #sortable");
    Dashboard.EmptyContext = $("#content-panels #empty-panels");
    Dashboard.UrlChangePanelOrder = '/Panel/ChangePanelOrder';
    Dashboard.UrlPanelView = '/Panel/Details';
    Dashboard.UrlPanelsUpdated = '/JsonData/Panels';

    Dashboard.UpdateAllPanels();
    Dashboard.EmptyContext.hide();
    Dashboard.EnableSortable();

    Dashboard.IntervalRef = setInterval(
        function () {
            Dashboard.UpdateAllPanels();
        }
        , 10000
    );
});