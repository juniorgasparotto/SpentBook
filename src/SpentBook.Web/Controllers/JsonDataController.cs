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

namespace SpentBook.Web.Controllers
{
    [JsonOutputWhenModelInvalid]
    [JsonOutputWhenGenericException]
    public class JsonDataController : Controller
    {
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
            var transactions = this.GetTransactionsFiltrated(panel);
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

                        var transactionGroup = GetTransactionGroupRoot(transactions, group);
                        htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsGroupDay.cshtml", transactionGroup, this);

                        //var transactionsDayGrouped = transactions.GroupBy(f => f.Date.Date).ToList();
                        //htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsGroupDay.cshtml", transactionsDayGrouped, this);
                    }
                    else
                    {
                        htmlTable = Helper.RenderPartialViewToString("~/Views/Templates/Tables/TransactionsSimple.cshtml", transactions, this);
                    }

                    break;
                }
                case SpentBook.Domain.Panel.PanelDataType.OneGroup:
                case SpentBook.Domain.Panel.PanelDataType.TwoGroup:
                case SpentBook.Domain.Panel.PanelDataType.ThreeOrMoreGroup:
                {
                    var transactionGroup = this.GetTransactionGroupRoot(transactions, panel.GetGroupDefinitions().ToArray());
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
            var transactions = this.GetTransactionsFiltrated(panel);
            var hasOnlyOutputs = true;

            var type = panel.GetDataType();
            switch (type)
            {
                case Panel.PanelDataType.NonGroup:
                case Panel.PanelDataType.NonGroupAndSortDate:
                {
                    var transactionsDayGrouped = transactions.OrderBy(f => f.Date).GroupBy(f=>f.Date.Date).ToList();
                    var total = transactions.Sum(f => f.Value);
                    var count = transactions.Count();

                    var serieData = new SeriesData[transactionsDayGrouped.Count];
                    var serie = new Series
                    {
                        Name = " ",
                        Data = new DotNet.Highcharts.Helpers.Data(serieData),
                    };

                    series.Add(serie);

                    var index = 0;
                    foreach (var transactionDayGroup in transactionsDayGrouped)
                    {
                        var y = 0d;
                        switch(panel.DisplayY)
                        {
                            case TransactionDisplayY.Value:
                                y = (double)transactionDayGroup.Sum(f => f.Value);
                                break;
                            case TransactionDisplayY.ValuePercentage:
                                var totalDay = transactionDayGroup.Sum(f => f.Value);
                                y = (double)((totalDay * 100m) / total);
                                break;
                            case TransactionDisplayY.Count:
                                y = transactionDayGroup.Count();
                                break;
                            case TransactionDisplayY.CountPercentage:
                                var countDay = transactionDayGroup.Sum(f => f.Value);
                                y = (double)(countDay * 100m) / count;
                                break;
                        }

                        serieData[index] = new SeriesData();
                        serieData[index].X = Helper.UnixTicks(transactionDayGroup.Key);
                        serieData[index].Y = (Number)y;
                        serieData[index].Name = transactionDayGroup.Key.ToString("d");

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
                    var transactionGroup = this.GetTransactionGroupRoot(transactions, panel.GetGroupDefinitions().ToArray());
                    var tryCategorize = true;

                    // Como só existe 1 nível, fica impossível criar categorização
                    if (type == Panel.PanelDataType.OneGroup)
                        tryCategorize = false;

                    var chartDataCategorizeds = GetChartDataCategorized(transactionGroup, tryCategorize);

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

        //[HttpGet]
        //public JsonResult Transactions(Guid dashboardId, Guid panelId, bool tryCategorize)
        //{
        //    var series = new List<Series>();
        //    var seriesDrilldown = new List<Series>();
        //    var htmlTable = default(string);
        //    var panel = this.GetPanel(dashboardId, panelId);

        //    var transactions = this.GetTransactionsFiltrated(panel);

        //    var groupDefinitions = panel.GetGroupDefinitions();
        //    if (groupDefinitions.Count > 0)
        //    {
        //        var transactionGroup = this.GetTransactionGroupRoot(panel, transactions);

        //        // Como só existe 1 nível, fica impossível criar categorização
        //        if (groupDefinitions.Count == 1)
        //            tryCategorize = false;

        //        var transactionGroupTransversal = GetTransactionGroupTransversal(transactionGroup, tryCategorize);

        //        foreach (var item in transactionGroupTransversal)
        //        {
        //            var serieData = new SeriesData[item.Items.Length];
        //            var serie = new Series
        //            {
        //                Id = item.ParentPath,
        //                Name = string.IsNullOrWhiteSpace(item.ItemGroupName) ? "Todos" : item.ItemGroupName,
        //                Data = new DotNet.Highcharts.Helpers.Data(serieData),
        //            };

        //            if (item.ParentPath == null)
        //            {
        //                series.Add(serie);
        //            }
        //            else
        //            {
        //                seriesDrilldown.Add(serie);
        //                serie.PlotOptionsBar = new PlotOptionsBar { ColorByPoint = false };
        //            }

        //            var index = 0;
        //            foreach (var data in item.Items)
        //            {
        //                if (data == null)
        //                {
        //                    serieData[index++] = null;
        //                }
        //                else
        //                {
        //                    serieData[index++] = new SeriesData
        //                    {
        //                        Y = (Number)data.TotalItemInCategory,
        //                        Name = data.Category,
        //                        Id = data.Category,
        //                        Drilldown = data.ItemPath
        //                    };
        //                }
        //            }
        //        }

        //        var expressions = this.GetTransactionGroupToExpression(transactionGroup);
        //        htmlTable = Helper.RenderPartialViewToString("~/Views/CustomPanels/TableGroupMulti.cshtml", expressions, this);
        //    }
        //    else
        //    {
        //        var transactionsDayGrouped = transactions.OrderBy(f => f.Date).GroupBy(f=>f.Date.Date).ToList();
        //        var serieData = new SeriesData[transactionsDayGrouped.Count];
        //        var serie = new Series
        //        {
        //            Name = " ",
        //            Data = new DotNet.Highcharts.Helpers.Data(serieData),
        //        };

        //        series.Add(serie);

        //        var index = 0;
        //        foreach (var transactionDayGroup in transactionsDayGrouped)
        //        {
        //            serieData[index] = new SeriesData();
        //            serieData[index].X = Helper.UnixTicks(transactionDayGroup.Key);
        //            serieData[index].Y = (Number)transactionDayGroup.Sum(f => f.Value);
        //            serieData[index].Name = transactionDayGroup.Key.ToString("d");
        //            index++;
        //        }

        //        if (panel.OrderBy == TransactionOrder.Date)
        //            htmlTable = Helper.RenderPartialViewToString("~/Views/CustomPanels/TableGroupDay.cshtml", transactionsDayGrouped, this);
        //        else
        //            htmlTable = Helper.RenderPartialViewToString("~/Views/CustomPanels/TableSimple.cshtml", transactions, this);
        //    }
            
        //    return Json(new { Table = htmlTable, Series = series, Drilldown = seriesDrilldown }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult Debug(Guid dashboardId, Guid panelId, bool tryCategorize)
        {
            var panel = this.GetPanel(dashboardId, panelId);
            var groupDefinitions = panel.GetGroupDefinitions();

            if (groupDefinitions.Count > 0)
                if (groupDefinitions.Count == 1)
                    tryCategorize = false;


            var transactions = this.GetTransactionsFiltrated(panel);
            var root = GetTransactionGroupRoot(transactions, panel.GetGroupDefinitions().ToArray());
            return Json(GetChartDataCategorized(root, tryCategorize), JsonRequestBehavior.AllowGet);
            return Json(root, JsonRequestBehavior.AllowGet);
        }

        public List<Transaction> GetTransactionsFiltrated(Panel panel)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var query = uow.Transactions.AsQueryable();

            var a = new ResumeController();
            var transactions = a.GetSpents();
            query = transactions.AsQueryable();

            var onlyInputs = panel.Filter.TransactionType == TransactionType.Input;
            var onlyOutputs = panel.Filter.TransactionType == TransactionType.Output;

            if (onlyInputs)
                query = query.Where(t => t.Value > 0);

            if (onlyOutputs)
                query = query.Where(t => t.Value < 0);

            if (panel.Filter.Categories != null && panel.Filter.Categories.Count > 0)
                query = query.Where(t => panel.Filter.Categories.Contains(t.Category));

            if (panel.Filter.SubCategories != null && panel.Filter.SubCategories.Count > 0)
                query = query.Where(t => panel.Filter.SubCategories.Contains(t.SubCategory));

            if (panel.Filter.Names != null && panel.Filter.Names.Count > 0)
                query = query.Where(t => panel.Filter.Names.Contains(t.Name));

            if (panel.Filter.DateStart != null)
                query = query.Where(t => t.Date >= panel.Filter.DateStart);

            if (panel.Filter.DateEnd != null)
                query = query.Where(t => t.Date <= panel.Filter.DateEnd);

            if (panel.Filter.ValueStart != null)
                query = query.Where(t => t.Value >= panel.Filter.ValueStart);

            if (panel.Filter.ValueEnd != null)
                query = query.Where(t => t.Value <= panel.Filter.ValueEnd);

            string orderByName;
            var expressionOrderBy = panel.GetOrderByExpression(panel.OrderBy, out orderByName);

            if (panel.OrderByClassification == OrderClassification.Asc)
                query = query.OrderBy(expressionOrderBy);
            else
                query = query.OrderByDescending(expressionOrderBy);

            return query.ToList();
        }

        public TransactionGroup GetTransactionGroupRoot(List<Transaction> transactions, params TransactionGroupDefinition[] groupDefinitions)
        {
            Func<List<Transaction>, TransactionGroup, int, List<TransactionGroup>> methodGroup = null;
            methodGroup = (_transactions, transactionGroupParent, groupByIndex) =>
            {
                var groupByDefinition = default(TransactionGroupDefinition);
                var transactionGroups = new List<TransactionGroup>();

                if (groupDefinitions.Length > groupByIndex)
                    groupByDefinition = groupDefinitions[groupByIndex];
                
                if (groupByDefinition != null)
                {
                    var _transactionsQueryable = _transactions.AsQueryable();

                    if (groupByDefinition.OrderByClassification != null)
                    {
                        if (groupByDefinition.OrderByClassification == OrderClassification.Asc)
                            _transactionsQueryable = _transactionsQueryable.OrderBy(groupByDefinition.OrderByExpression);
                        else
                            _transactionsQueryable = _transactionsQueryable.OrderByDescending(groupByDefinition.OrderByExpression);
                    }

                    var group = _transactionsQueryable
                        .GroupBy(groupByDefinition.GroupByExpression)
                        .ToList();

                    transactionGroupParent.GroupByDefinition = groupByDefinition;
                    
                    var nextGroupByIndex = groupByIndex + 1;
                    var transactionGroupsNews = group.Select(
                        (g, gIndex) =>
                        {
                            var list = g.ToList();
                            var name = this.GetGroupByNameUsingValue(groupByDefinition.GroupBy, list);
                            var key = g.Key.ToString();
                            var transactionGroup = ConfigureTransactionGroup(new TransactionGroup(), key, name, list, null, transactionGroupParent, nextGroupByIndex);
                            transactionGroup.SubGroups = methodGroup(list, transactionGroup, nextGroupByIndex);
                            return transactionGroup;
                        }
                    );

                    if (groupByDefinition.OrderByGroupClassification != null)
                    {
                        if (groupByDefinition.OrderByGroupClassification == OrderClassification.Asc)
                            transactionGroupsNews = transactionGroupsNews.AsQueryable().OrderBy(groupByDefinition.OrderByGroupExpression);
                        else
                            transactionGroupsNews = transactionGroupsNews.AsQueryable().OrderByDescending(groupByDefinition.OrderByGroupExpression);
                    }

                    var transactionGroupsNewsList = transactionGroupsNews.ToList();
                    transactionGroups.AddRange(transactionGroupsNewsList);
                }

                return transactionGroups;
            };

            var transactionGroupRoot = new TransactionGroup();
            ConfigureTransactionGroup(transactionGroupRoot, null, "Todos", transactions, null, null, 0);
            transactionGroupRoot.SubGroups = methodGroup(transactions, transactionGroupRoot, 0);

            //var transactionGroupsReturn = methodGroup(transactions, transactionGroupRoot, 0);
            //var transactionsRoot = transactionGroupsReturn.SelectMany(f => f.Transactions).ToList();
            //ConfigureTransactionGroup(transactionGroupRoot, null, "Todos", null, transactionsRoot, transactionGroupsReturn, null, 0, onlyOutputs);

            //foreach (var s in transactionGroupsReturn)
            //    s.Parent = transactionGroupRoot;

            return transactionGroupRoot;
        }

        public List<ChartDataCategorized> GetChartDataCategorized(TransactionGroup transactionGroupRoot, bool tryCategorize = true)
        {
            var listReturn = new List<ChartDataCategorized>();
            Action<TransactionGroup, ChartDataCategorized> methodGroup = null;
            int id = 0;
            methodGroup = (current, parentTransversal) =>
            {
                if (current.SubGroups.Count == 0)
                    return;

                //if (grandchildrenGroups != null)
                //    if (current.GroupByDefinition.OrderByClassification == TransactionOrderClassification.Asc)
                //        grandchildrenGroups.OrderBy(f => f.Key);

                //if (tryCategorize && grandchildrenGroups.Count > 0)
                // categoriza apenas o root se for solicitado
                if (tryCategorize && current.Parent == null)
                {
                    var children = current.SubGroups;
                    var grandchildrenGroups = children.SelectMany(f => f.SubGroups).GroupBy(f => f.Key).ToList();

                    foreach (var grandchildrenGroup in grandchildrenGroups)
                    {
                        var list = grandchildrenGroup.ToList();

                        var inverted = new ChartDataCategorized()
                        {
                            Id = ++id,
                            IdParent = (parentTransversal == null ? null : (int?)parentTransversal.Id),
                            ParentPath = current.GetPath(),
                            ItemGroupName = grandchildrenGroup.Key,
                            Items = new ChartDataCategorized.Item[children.Count]
                        };

                        listReturn.Add(inverted);

                        foreach (var grandchild in list)
                        {
                            var indexOfParent = current.SubGroups.IndexOf(grandchild.Parent);
                            inverted.Add(grandchild.GetPath(), grandchild.Parent.Key, indexOfParent, grandchild);
                            methodGroup(grandchild, inverted);
                        }
                    }
                }
                else
                {
                    var name = current.Key ?? (current.GroupByDefinition != null ? current.GroupByDefinition.GroupByName : "");
                    if (current.Parent == null && name == null)
                        name = current.SubGroups.First().GroupByDefinition.GroupByName ?? "Todos";

                    var inverted = new ChartDataCategorized()
                    {
                        Id = ++id,
                        IdParent = (parentTransversal == null ? null : (int?)parentTransversal.Id),
                        ParentPath = current.GetPath(),
                        ItemGroupName = name,
                        Items = new ChartDataCategorized.Item[current.SubGroups.Count]
                    };

                    listReturn.Add(inverted);

                    var index = 0;
                    foreach (var childGroup in current.SubGroups)
                    {
                        inverted.Add(childGroup.GetPath(), childGroup.Key, index++, childGroup);
                        methodGroup(childGroup, inverted);
                    }
                }

                //foreach(var child in children)
                //    methodGroup(child, 0);
            };

            methodGroup(transactionGroupRoot, null);

            return listReturn;
        }

        private TransactionGroup ConfigureTransactionGroup(TransactionGroup transactionGroup, string key, string name, List<Transaction> transactions, List<TransactionGroup> subGroups, TransactionGroup parent, int level)
        {
            transactionGroup.Key = key;
            //transactionGroup.GroupByDefinition = groupByDefinition;
            transactionGroup.Name = name;
            transactionGroup.Parent = parent;
            transactionGroup.Transactions = transactions;
            transactionGroup.SubGroups = subGroups;
            transactionGroup.Level = level;
            
            if (transactionGroup.Total == 0)
                transactionGroup.Total = transactionGroup.Transactions.Sum(f => f.Value);

            if (transactionGroup.Count == 0)
                transactionGroup.Count = transactionGroup.Transactions.Count;

            //transactionGroup.TotalAsPositive = Math.Abs(transactionGroup.Total);
            if (parent == null)
            {
                transactionGroup.TotalPercentage.Add(100m);
                transactionGroup.CountPercentage.Add(100m);
            }

            while (parent != null)
            {
                transactionGroup.TotalPercentage.Add((transactionGroup.Total * 100m) / parent.Total);
                transactionGroup.CountPercentage.Add((transactionGroup.Count * 100m) / parent.Count);

                parent = parent.Parent;
            }

            return transactionGroup;
        }

        private Panel GetPanel(Guid dashboardId, Guid panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            return dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
        }

        private string GetGroupByNameUsingValue(TransactionGroupBy? group, List<Transaction> transactions)
        {
            if (group != null)
            {
                var transaction = transactions.FirstOrDefault();
                switch (group)
                {
                    case TransactionGroupBy.Category:
                        return transaction.Category;
                    case TransactionGroupBy.SubCategory:
                        return transaction.SubCategory;
                    case TransactionGroupBy.DateDay:
                        return transaction.Date.ToString("yyyy/MM/dd");
                    case TransactionGroupBy.DateMonth:
                        return transaction.Date.ToString("yyyy/MM");
                    case TransactionGroupBy.DateYear:
                        return transaction.Date.ToString("yyyy");
                    case TransactionGroupBy.Name:
                        return transaction.Name;
                }
            }

            return null;
        }

        private List<ExpressionGraph.Expression<TransactionGroup>> GetTransactionGroupToExpression(TransactionGroup transactionGroup)
        {
            var master = new TransactionGroup { Name = "master", Key = "master" };
            master.SubGroups.Add(transactionGroup);
            return ExpressionGraph.ExpressionBuilder<TransactionGroup>.Build(master.SubGroups, f => f.SubGroups, true, false, false).ToList();
        }
    }
}