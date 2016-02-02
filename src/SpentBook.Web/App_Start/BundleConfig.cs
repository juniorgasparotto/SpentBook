﻿using System.Web;
using System.Web.Optimization;

namespace SpentBook.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // scripts
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include("~/Scripts/jquery-ui-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/json-date-parse").Include("~/Scripts/json.date-extensions.js"));
            bundles.Add(new ScriptBundle("~/bundles/linq").Include("~/Scripts/linq.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-linq").Include("~/Scripts/jquery.linq.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));
            bundles.Add(new ScriptBundle("~/bundles/chart.js").Include("~/Scripts/Chart.js"));
            bundles.Add(new ScriptBundle("~/bundles/Custom/ChartHelper").Include("~/Scripts/Custom/ChartHelper.js"));
            bundles.Add(new ScriptBundle("~/bundles/Custom/Dashboards").Include("~/Scripts/Custom/Dashboards.js"));
            bundles.Add(new ScriptBundle("~/bundles/Custom/Dashboard").Include("~/Scripts/Custom/Dashboard.js"));

            //styles
            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css", "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/css/jqueryui").Include("~/Content/themes/base/*.css"));
        }
    }
}
