using CsvHelper;
using SpentBook.Data.FileSystem;
using SpentBook.Domain;
using SpentBook.Web.Filters;
using SpentBook.Web.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Helpers;
using ExpressionGraph;
using SpentBook.Domain.Services;

namespace SpentBook.Web.Controllers
{
    [JsonOutputWhenModelInvalid]
    [JsonOutputWhenGenericException]
    public class JsonDataController : Controller
    {
        TransactionService transactionService;
        TransactionService transactionServiceOnlyTransaction;
        public JsonDataController()
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            transactionService = new TransactionService(uow);

            var uow2 = new TransactionCSVUnitOfWork();
            transactionServiceOnlyTransaction = new TransactionService(uow2);
        }

        [HttpPost]
        public JsonResult Panels(Guid dashboardId, List<Panel> panelsExistsInInterface)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            var panelsExistsInDB = dashboard.Panels;

            panelsExistsInDB = panelsExistsInDB ?? new List<Panel>();
            panelsExistsInInterface = panelsExistsInInterface ?? new List<Panel>();

            // exists in DB but not exists in interface
            var news = (
                from panelDB in panelsExistsInDB
                from panelInterface in panelsExistsInInterface.Where(f => f.Id == panelDB.Id).DefaultIfEmpty()
                where panelInterface == null
                orderby panelDB.PanelOrder, panelDB.Title ascending
                select new { panelDB.Id, panelDB.Title, panelDB.LastUpdateDate, panelDB.PanelOrder, PanelWidth = panelDB.PanelWidth.ToString().ToLower() }
            ).ToList();

            // exists in both, but the update date in DB is more than interface
            var updateds = (
                from panelDB in panelsExistsInDB
                join panelInterface in panelsExistsInInterface on panelDB.Id equals panelInterface.Id
                where panelDB.LastUpdateDate > panelInterface.LastUpdateDate
                orderby panelDB.PanelOrder, panelDB.Title ascending
                select new { panelDB.Id, panelDB.LastUpdateDate, panelDB.Title, panelDB.PanelOrder, PanelWidth = panelDB.PanelWidth.ToString().ToLower() }
            ).ToList();

            // exists in interface but not exists in DB
            var deleteds = (
                from panelInterface in panelsExistsInInterface
                from panelDB in panelsExistsInDB.Where(f => f.Id == panelInterface.Id).DefaultIfEmpty()
                where panelDB == null
                orderby panelInterface.PanelOrder, panelInterface.Title ascending
                select panelInterface.Id
            ).ToList();

            var changes = new
            {
                News = news,
                Deleteds = deleteds,
                Updateds = updateds
            };

            return Json(changes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult TransactionsTable(Guid dashboardId, Guid panelId)
        {
            var htmlTable = "";
            var panel = this.GetPanel(dashboardId, panelId);
            var transactions = transactionServiceOnlyTransaction.GetTransactionsFiltrated(panel.Filter);
            var type = panel.GetDataType();
            switch (type)
            {
                case Panel.PanelDataType.NonGroup:
                case Panel.PanelDataType.NonGroupAndSortDate:
                {
                    if (type == Panel.PanelDataType.NonGroupAndSortDate)
                    {
                        var group = new TransactionGroupDefinition {
                            OrderBy = null,
                            OrderByClassification = null,
                            OrderByExpression = null,
                            OrderByName = null,
                            GroupBy = null,
                            //GroupByExpression = panel.GetGroupByExpression(panel.GroupBy, out groupByName),
                            GroupByExpression = f => f.Date.Date,                            
                            GroupByName = null,
                            OrderByGroup = null,
                            OrderByGroupClassification = null,
                            OrderByGroupExpression =null,
                            OrderByGroupName = null,
                        };

                        var transactionGroup = transactionService.GetTransactionGroupRoot(transactions, group);
                        htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsGroupDay.cshtml", transactionGroup, this);

                        //var transactionsDayGrouped = transactions.GroupBy(f => f.Date.Date).ToList();
                        //htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsGroupDay.cshtml", transactionsDayGrouped, this);
                    }
                    else
                    {
                        var group = new TransactionGroupDefinition
                        {
                            OrderBy = null,
                            OrderByClassification = null,
                            OrderByExpression = null,
                            OrderByName = null,
                            GroupBy = null,
                            //GroupByExpression = panel.GetGroupByExpression(panel.GroupBy, out groupByName),
                            GroupByExpression = f => f.GetHashCode(),
                            GroupByName = null,
                            OrderByGroup = null,
                            OrderByGroupClassification = null,
                            OrderByGroupExpression = null,
                            OrderByGroupName = null,
                        };
                        var transactionGroup = transactionService.GetTransactionGroupRoot(transactions, group);
                        htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsSimple.cshtml", transactionGroup, this);
                    }

                    break;
                }
                case SpentBook.Domain.Panel.PanelDataType.OneGroup:
                case SpentBook.Domain.Panel.PanelDataType.TwoGroup:
                case SpentBook.Domain.Panel.PanelDataType.ThreeOrMoreGroup:
                {
                    var transactionGroup = transactionService.GetTransactionGroupRoot(transactions, panel.GetGroupDefinitions().ToArray());
                    var expressions = this.GetTransactionGroupToExpression(transactionGroup);
                    htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsGroupMulti.cshtml", expressions, this);
                    break;
                }
            }

            return Json(new { Table = htmlTable }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult TransactionsHighcharts(Guid dashboardId, Guid panelId)
        {
            var series = new List<Series>();
            var seriesDrilldown = new List<Series>();

            var panel = this.GetPanel(dashboardId, panelId);
            var transactions = transactionServiceOnlyTransaction.GetTransactionsFiltrated(panel.Filter);
            var hasOnlyOutputs = true;

            var type = panel.GetDataType();
            switch (type)
            {
                case Panel.PanelDataType.NonGroup:
                case Panel.PanelDataType.NonGroupAndSortDate:
                {
                    var group = new TransactionGroupDefinition
                    {
                        OrderBy = null,
                        OrderByClassification = null,
                        OrderByExpression = null,
                        OrderByName = null,
                        GroupBy = null,
                        //GroupByExpression = panel.GetGroupByExpression(panel.GroupBy, out groupByName),
                        GroupByExpression = f => f.Date.Date,
                        GroupByName = null,
                        OrderByGroup = null,
                        OrderByGroupClassification = null,
                        OrderByGroupExpression = null,
                        OrderByGroupName = null,
                    };

                    transactions = transactions.OrderBy(f => f.Date).ToList();
                    var transactionGroup = transactionServiceOnlyTransaction.GetTransactionGroupRoot(transactions, group);

                    //var transactionsDayGrouped = transactions.OrderBy(f => f.Date).GroupBy(f=>f.Date.Date).ToList();
                    //var total = transactions.Sum(f => f.Value);
                    //var count = transactions.Count();

                    var serieData = new SeriesData[transactionGroup.SubGroups.Count];
                    var serie = new Series
                    {
                        Name = " ",
                        Data = new DotNet.Highcharts.Helpers.Data(serieData),
                    };

                    series.Add(serie);

                    var index = 0;
                    foreach (var transactionDayGroup in transactionGroup.SubGroups)
                    {
                        var y = 0d;
                        switch(panel.DisplayY)
                        {
                            case TransactionDisplayY.Value:
                                y = (double)transactionDayGroup.Total;
                                break;
                            case TransactionDisplayY.ValuePercentage:
                                y = (double)transactionDayGroup.TotalPercentage.Last();
                                break;
                            case TransactionDisplayY.Count:
                                y = transactionDayGroup.Count;
                                break;
                            case TransactionDisplayY.CountPercentage:
                                y = (double)transactionDayGroup.CountPercentage.Last();
                                break;
                        }

                        var day = (DateTime)transactionDayGroup.Key;
                        serieData[index] = new SeriesData();
                        serieData[index].X = Helper.UnixTicks(day);
                        serieData[index].Y = (Number)y;
                        serieData[index].Name = day.ToString("d");

                        if (y > 0)
                            hasOnlyOutputs = false;

                        index++;
                    }
                    break;
                }
                case SpentBook.Domain.Panel.PanelDataType.OneGroup:
                case SpentBook.Domain.Panel.PanelDataType.TwoGroup:
                case SpentBook.Domain.Panel.PanelDataType.ThreeOrMoreGroup:
                {
                    var transactionGroup = transactionService.GetTransactionGroupRoot(transactions, panel.GetGroupDefinitions().ToArray());
                    var tryCategorize = true;

                    // Como só existe 1 nível, fica impossível criar categorização
                    if (type == Panel.PanelDataType.OneGroup)
                        tryCategorize = false;

                    var chartDataCategorizeds = transactionService.GetChartDataCategorized(transactionGroup, tryCategorize);

                    foreach (var item in chartDataCategorizeds)
                    {
                        var serieData = new SeriesData[item.Items.Length];
                        var serie = new Series
                        {
                            Id = item.ParentPath,
                            Name = string.IsNullOrWhiteSpace(item.ItemGroupName) ? "Todos" : item.ItemGroupName,
                            Data = new DotNet.Highcharts.Helpers.Data(serieData),
                        };

                        if (item.ParentPath == null)
                        {
                            series.Add(serie);
                        }
                        else
                        {
                            seriesDrilldown.Add(serie);
                            serie.PlotOptionsBar = new PlotOptionsBar { ColorByPoint = false };
                        }

                        var index = 0;
                        foreach (var data in item.Items)
                        {
                            if (data == null)
                            {
                                serieData[index++] = null;
                            }
                            else
                            {
                                var y = 0d;
                                switch (panel.DisplayY)
                                {
                                    case TransactionDisplayY.Value:
                                        y = (double)data.Total;
                                        break;
                                    case TransactionDisplayY.ValuePercentage:
                                        if (tryCategorize && item.ParentPath == null)
                                            y = (double)data.TotalPercentageGrandParentRelation;
                                        else
                                            y = (double)data.TotalPercentage;
                                        break;
                                    case TransactionDisplayY.Count:
                                        y = (double)data.Count;
                                        break;
                                    case TransactionDisplayY.CountPercentage:
                                        if (tryCategorize && item.ParentPath == null)
                                            y = (double)data.CountPercentageGrandParentRelation;
                                        else
                                            y = (double)data.CountPercentage;
                                        break;
                                }

                                serieData[index++] = new SeriesData
                                {
                                    Y = (Number)y,
                                    Name = data.Category,
                                    Id = data.Category,
                                    Drilldown = data.ItemPath
                                };

                                if (data.Category == "Despesa")
                                    serieData[index - 1].Color = System.Drawing.Color.Red;

                                if (data.Total > 0)
                                    hasOnlyOutputs = false;
                            }
                        }
                    }

                    break;
                }
            }

            return Json(new { Series = series, Drilldown = seriesDrilldown, Reversed = hasOnlyOutputs, DisplatY = panel.DisplayY }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Debug(Guid dashboardId, Guid panelId, bool tryCategorize)
        {
            var panel = this.GetPanel(dashboardId, panelId);
            var groupDefinitions = panel.GetGroupDefinitions();

            if (groupDefinitions.Count > 0)
                if (groupDefinitions.Count == 1)
                    tryCategorize = false;


            var transactions = transactionService.GetTransactionsFiltrated(panel.Filter);
            var root = transactionService.GetTransactionGroupRoot(transactions, panel.GetGroupDefinitions().ToArray());
            return Json(transactionService.GetChartDataCategorized(root, tryCategorize), JsonRequestBehavior.AllowGet);
            return Json(root, JsonRequestBehavior.AllowGet);
        }

        private Panel GetPanel(Guid dashboardId, Guid panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            return dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
        }

        private List<ExpressionGraph.Expression<TransactionGroup>> GetTransactionGroupToExpression(TransactionGroup transactionGroup)
        {
            var master = new TransactionGroup { Name = "master", Key = "master" };
            master.SubGroups.Add(transactionGroup);
            return ExpressionGraph.ExpressionBuilder<TransactionGroup>.Build(master.SubGroups, f => f.SubGroups, true, false, false).ToList();
        }
    }
}