using System.Web;
using System.Web.Optimization;

namespace SpentBook.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // scripts of community
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/json-date-parse").Include("~/Scripts/json.date-extensions.js"));
            bundles.Add(new ScriptBundle("~/bundles/linq").Include("~/Scripts/linq.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-linq").Include("~/Scripts/jquery.linq.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-gridster").Include("~/Scripts/jquery.gridster.js"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts")
                .Include("~/Scripts/highcharts/4.2.3/highcharts.src.js")
                .Include("~/Scripts/Highcharts/4.2.3/modules/data.src.js")
                .Include("~/Scripts/Highcharts/4.2.3/modules/exporting.src.js")
                .Include("~/Scripts/Highcharts/4.2.3/modules/drilldown.src.js")
                .Include("~/Scripts/Highcharts/4.2.3/highcharts-more.js")
                //.Include("~/Scripts/Highcharts/plugins/value-in-legend.js")
                //.Include("~/Scripts/Highcharts/4.2.3/modules/funnel.src.js")
                //.Include("~/Scripts/Highcharts/4.2.3/modules/solid-gauge.src.js")
                //.Include("~/Scripts/Highcharts/4.2.3/modules/drilldown.src.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/jquery-treegrid")
                .Include("~/Scripts/treegrid/jquery.treegrid.js")
                .Include("~/Scripts/treegrid/jquery.treegrid.bootstrap3.js")
            );

            // scripts base
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));
            
            // scripts custom
            bundles.Add(new ScriptBundle("~/bundles/chart.js").Include("~/Scripts/Chart.js"));
            bundles.Add(new ScriptBundle("~/bundles/Custom/ChartHelper").Include("~/Scripts/Custom/ChartHelper.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/custom/dashboards").Include("~/Scripts/Custom/Dashboards.js"));
            bundles.Add(new ScriptBundle("~/bundles/custom/helper").Include("~/Scripts/Custom/Helper.js"));
            bundles.Add(new ScriptBundle("~/bundles/custom/highcharts-helper").Include("~/Scripts/Custom/HighchartsHelper.js"));
            bundles.Add(new ScriptBundle("~/bundles/custom/dashboard").Include("~/Scripts/Custom/Dashboard.js"));
            bundles.Add(new ScriptBundle("~/bundles/custom/preloader").Include("~/Scripts/Custom/Preloader.js"));
            bundles.Add(new ScriptBundle("~/bundles/custom/page-dashboard").Include("~/Scripts/Custom/PageDashboard.js"));

            //styles
            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css", "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/css/jqueryui").Include("~/Content/themes/base/*.css"));
            bundles.Add(new ScriptBundle("~/Content/css/jquery-gridster").Include("~/Content/jquery.gridster.css"));
            bundles.Add(new ScriptBundle("~/Content/css/treegrid").Include("~/Content/treegrid/jquery.treegrid.css"));
        }
    }
}
