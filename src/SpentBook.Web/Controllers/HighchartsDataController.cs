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

namespace SpentBook.Web.Controllers
{
    [JsonOutputWhenModelInvalid]
    [JsonOutputWhenGenericException]
    public class HighchartsDataController : Controller
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
                select new { panelDB.Id, panelDB.LastUpdateDate, panelDB.PanelOrder, PanelWidth = panelDB.PanelWidth.ToString().ToLower() }
            ).ToList();

            // exists in both, but the update date in DB is more than interface
            var updateds = (
                from panelDB in panelsExistsInDB
                join panelInterface in panelsExistsInInterface on panelDB.Id equals panelInterface.Id
                where panelDB.LastUpdateDate > panelInterface.LastUpdateDate
                orderby panelDB.PanelOrder, panelDB.Title ascending
                select new { panelDB.Id, panelDB.LastUpdateDate, panelDB.PanelOrder, PanelWidth = panelDB.PanelWidth.ToString().ToLower() }
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
        public JsonResult Transactions(Guid dashboardId, Guid panelId)
        {
            var transactionGroup = this.GetTransactionGroupRootByPanel(dashboardId, panelId);
            var categories = new List<string>();
            var seriesDatas = new List<SeriesData>();
            var series = new List<Series>();
            var seriesDrilldown = new List<Series>();

            var dicSeries = new Dictionary<string, Series>();
            var dicSeriesData = new Dictionary<string, SeriesData[]>();

            if (transactionGroup.SubGroups == null)
            {
                foreach(var transaction in transactionGroup.Transactions)
                {
                    seriesDatas.Add(new SeriesData() {
                        Y = (Number)transaction.Value,
                        Name = transaction.Name
                    });
                }
            }
            else
            {
                foreach (var group in transactionGroup.SubGroups)
                {
                    seriesDatas.Add(new SeriesData()
                    {
                        Y = (Number)group.Total,
                        Name = group.Name,
                        Id = group.Key,
                        Drilldown = group.Key
                    });

                    if (group.SubGroups != null)
                    {
                        var seriesDatasDrill = new List<object[]>();

                        foreach (var group2 in group.SubGroups)
                        {
                            seriesDatasDrill.Add(new object[] { group2.Key, group2.Total });
                        }

                        var serieDrilldown = new Series
                        {
                            Name = group.Name,
                            Id = group.Key,
                            Data = new DotNet.Highcharts.Helpers.Data(seriesDatasDrill.ToArray()),
                            //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = false }
                        };
                        seriesDrilldown.Add(serieDrilldown);
                    }
                }

                // modelo 2
                //foreach (var parent in transactionGroup.SubGroups)
                //{
                //    categories.Add(parent.Name);

                //    if (parent.SubGroups != null)
                //    {
                //        foreach (var child in parent.SubGroups)
                //        {
                //            var seriesDatas2 = new SeriesData[transactionGroup.SubGroups.Count];
                //            series.Add(new Series
                //            {
                //                Id = child.Name,
                //                Name = child.Name,
                //                Data = new DotNet.Highcharts.Helpers.Data(seriesDatas2),
                //                PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
                //            });

                //            var parentIndex = transactionGroup.SubGroups.IndexOf(transactionGroup.SubGroups.FirstOrDefault(f => f.Name == child.ParentName));
                //            seriesDatas2[parentIndex] = new SeriesData()
                //            {
                //                Y = (Number)child.Total,
                //                Name = parent.Name,
                //                //Id = group.Name,
                //            };
                //        }
                //    }
                //}
                
                var index = 0;
                foreach (var parent in transactionGroup.SubGroups)
                {
                    categories.Add(parent.Key);

                    if (parent.SubGroups != null)
                    {
                        foreach (var child in parent.SubGroups)
                        {
                            var serie = default(Series);
                            var serieData = default(SeriesData[]);

                            if (dicSeries.ContainsKey(child.Key))
                            {
                                serie = dicSeries[child.Key];
                                serieData = dicSeriesData[child.Key];
                            }
                            else
                            {
                                serieData = new SeriesData[transactionGroup.SubGroups.Count];
                                serie = new Series
                                {
                                    Id = child.Key,
                                    Name = child.Key,
                                    Data = new DotNet.Highcharts.Helpers.Data(serieData),
                                    //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
                                };

                                dicSeriesData.Add(child.Key, serieData);
                                dicSeries.Add(child.Key, serie);
                            }
                            serieData[index] = new SeriesData {
                                Y = (Number)child.Total,
                                Name = child.Key,
                                Drilldown = child.Key
                            };
                        }

                        index++;
                    }
                }
            }

            //var serie1 = new Series
            //    {
            //        Name = "Browser brands",
            //        Data = new DotNet.Highcharts.Helpers.Data(seriesDatas.ToArray()),
            //        PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
            //    };

            //var serie2 = new Series
            //{
            //    Name = "Browser brands 2",
            //    Data = new DotNet.Highcharts.Helpers.Data(seriesDatas.ToArray()),
            //    PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
            //};

            //series.Add(serie1);
            //series.Add(serie2);
            
            var drilldown = new DotNet.Highcharts.Options.Drilldown
            {
                Series = seriesDrilldown.ToArray()
            };

            return Json(new { Categories = categories, Series = dicSeries.Values, Drilldown = drilldown }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Transactions2(Guid dashboardId, Guid panelId)
        {
            var transactionGroup = this.GetTransactionGroupRootByPanel(dashboardId, panelId);
            var series = new List<Series>();
            var seriesDrilldown = new List<Series>();

            var dicSeries = new Dictionary<string, Series>();
            var dicSeriesData = new Dictionary<string, SeriesData[]>();

            // LEVEL 1
            var index = 0;
            foreach (var level1 in transactionGroup.SubGroups)
            {
                // LEVEL 1
                if (level1.SubGroups.Count > 0)
                {
                    foreach (var level2 in level1.SubGroups)
                    {
                        var serie = default(Series);
                        var serieData = default(SeriesData[]);

                        if (dicSeries.ContainsKey(level2.Key))
                        {
                            serie = dicSeries[level2.Key];
                            serieData = dicSeriesData[level2.Key];
                        }
                        else
                        {
                            serieData = new SeriesData[transactionGroup.SubGroups.Count];
                            serie = new Series
                            {
                                Id = level2.Key,
                                Name = level2.Name,
                                Data = new DotNet.Highcharts.Helpers.Data(serieData),
                                //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
                            };

                            dicSeriesData.Add(level2.Key, serieData);
                            dicSeries.Add(level2.Key, serie);
                        }
                        serieData[index] = new SeriesData
                        {
                            Y = (Number)level2.Total,
                            Name = level1.Name,
                            Id = level1.Key,
                            Drilldown = level2.Key
                        };

                        if (level2.SubGroups.Count > 0)
                        {
                            var seriesDatasDrill = new List<object[]>();

                            foreach (var level3 in level2.SubGroups)
                            {
                                seriesDatasDrill.Add(new object[] { level3.Name, level3.Total });
                            }

                            var serieDrilldown = new Series
                            {
                                Id = level2.Key,
                                Name = level2.Name,
                                Data = new DotNet.Highcharts.Helpers.Data(seriesDatasDrill.ToArray()),
                            };
                            seriesDrilldown.Add(serieDrilldown);
                        }
                    }
                    index++;
                }
            }

            return Json(new { Series = dicSeries.Values, Drilldown = seriesDrilldown }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public object ConvertTransactionGroupToHighChartsData(TransactionGroup transactionGroup, List<TransactionGroup> transactionGroupParents)
        //{
        //    var serie = new Series();
        //    var serieData = new SeriesData[transactionGroupParents.Count];
        //    serie.Id = transactionGroup.Key;
        //    serie.Name = transactionGroup.Name;
        //    serie.Data = new DotNet.Highcharts.Helpers.Data(serieData);
        //}

        [HttpGet]
        public TransactionGroup GetTransactionGroupRootByPanel(Guid dashboardId, Guid panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            var panel = dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
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

            switch (panel.OrderBy) {
                case TransactionOrder.Category:
                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
                        query = query.OrderBy(f => f.Category);
                    else
                        query = query.OrderByDescending(f => f.Category);
                    break;
                case TransactionOrder.SubCategory:
                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
                        query = query.OrderBy(f => f.SubCategory);
                    else
                        query = query.OrderByDescending(f => f.SubCategory);
                    break;
                case TransactionOrder.Date:
                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
                        query = query.OrderBy(f => f.Date);
                    else
                        query = query.OrderByDescending(f => f.Date);
                    break;
                case TransactionOrder.Name:
                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
                        query = query.OrderBy(f => f.Name);
                    else
                        query = query.OrderByDescending(f => f.Name);
                    break;
                case TransactionOrder.Value:
                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
                        query = query.OrderBy(f => f.Value);
                    else
                        query = query.OrderByDescending(f => f.Value);
                    break;
            }

            var groupsBy = new List<TransactionGroupBy> { panel.GroupBy, panel.GroupBy2, panel.GroupBy3 };
            var subGroups = this.GetTransactionGroups(query.ToList(), groupsBy, onlyOutputs);
            var transactionsRoot = subGroups.SelectMany(f=>f.Transactions).ToList();
            var transactionGroupRoot = CreateTransactionGroup(null, null, transactionsRoot, subGroups, null, onlyOutputs);
            
            foreach (var s in subGroups)
                s.Parent = transactionGroupRoot;

            return transactionGroupRoot;
        }

        private List<TransactionGroup> GetTransactionGroups(List<Transaction> transactions, List<TransactionGroupBy> groupBys, bool onlyOutputs)
        {
            Func<List<Transaction>, TransactionGroup, int, List<TransactionGroup>> methodGroup = null;
            methodGroup = (_transactions, transactionGroupParent, groupByIndex) =>
            {
                var groupBy = TransactionGroupBy.None;
                var transactionGroups = new List<TransactionGroup>();

                if (groupBys.Count > groupByIndex)
                    groupBy = groupBys[groupByIndex];

                if (groupBy != TransactionGroupBy.None)
                {
                    var group = _transactions
                        .AsQueryable()
                        .GroupBy(this.GetGroupKeyExpression(groupBy))
                        .ToList();

                    var nextGroupByIndex = groupByIndex + 1;

                    transactionGroups.AddRange(group.Select(
                        (g, gIndex) =>
                        {
                            var list = g.ToList();
                            var name = this.GetGroupName(groupBy, list);
                            var key = g.Key.ToString();
                            var transactionGroup = CreateTransactionGroup(key, name, list, null, transactionGroupParent, onlyOutputs);
                            transactionGroup.SubGroups = methodGroup(list, transactionGroup, nextGroupByIndex);
                            return transactionGroup;
                        }
                    ).ToList());

                }

                return transactionGroups;
            };

            var transactionGroupsReturn = methodGroup(transactions, null, 0);
            return transactionGroupsReturn;
        }

        [HttpGet]
        public JsonResult Transactions3(Guid dashboardId, Guid panelId)
        {
            var transactionGroup = this.GetTransactionGroupRootByPanel(dashboardId, panelId);
            var inverted = GetTransactionGroupParents(transactionGroup);
            var series = new List<Series>();
            var seriesDrilldown = new List<Series>();

            foreach(var item in inverted[0])
            {
                var serieData = new SeriesData[item.Items.Length];
                var serie = new Series
                {
                    Id = item.Parent,
                    Name = item.ItemGroupName,
                    Data = new DotNet.Highcharts.Helpers.Data(serieData),
                };

                if (item.Parent == null)
                    series.Add(serie);
                else
                    seriesDrilldown.Add(serie);

                var index = 0;
                foreach (var data in item.Items)
                {
                    if (data == null)
                    {
                        serieData[index++] = null;
                    }
                    else
                    {
                        serieData[index++] = new SeriesData
                        {
                            Y = (Number)data.TotalItemInCategory,
                            Name = data.Category,
                            Id = data.Category,
                            Drilldown = data.ItemPath
                        };
                    }
                }
            }

            return Json(new { Series = series, Drilldown = seriesDrilldown }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Debug(Guid dashboardId, Guid panelId)
        {
            var root = GetTransactionGroupRootByPanel(dashboardId, panelId);
            return Json(GetTransactionGroupParents(root), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        private Dictionary<int, List<TransactionGroupInverted>> GetTransactionGroupParents(TransactionGroup transactionGroupRoot)
        {
            var dicStores = new Dictionary<int, List<TransactionGroupInverted>>();
            Action<TransactionGroup, int> methodGroup = null;
            methodGroup = (parent, level) =>
            {
                var children = parent.SubGroups;
                var grandchildrenGroups = children.SelectMany(f => f.SubGroups).GroupBy(f => f.Key).ToList();

                if (!dicStores.ContainsKey(level) && grandchildrenGroups.Count > 0)
                    dicStores.Add(level, new List<TransactionGroupInverted>());

                foreach (var grandchildrenGroup in grandchildrenGroups)
                {
                    var inverted = new TransactionGroupInverted()
                    {
                        Parent = parent.Key,
                        ItemGroupName = grandchildrenGroup.Key,
                        Items = new TransactionGroupInverted.TransactionGroupInvertedItem[children.Count]
                    };

                    dicStores[level].Add(inverted);

                    var list = grandchildrenGroup.ToList();

                    foreach (var grandchild in list)
                    {
                        var indexOfParent = parent.SubGroups.IndexOf(grandchild.Parent);
                        inverted.AddChildToParent(grandchild, indexOfParent);
                    }
                }

                foreach(var child in children)
                    methodGroup(child, 0);
            };

            methodGroup(transactionGroupRoot, 0);

            return dicStores;
        }

        private TransactionGroup CreateTransactionGroup(string key, string name, List<Transaction> transactions, List<TransactionGroup> subGroups, TransactionGroup parent, bool onlyOutputs)
        {
            var transactionGroup = new TransactionGroup();
            transactionGroup.Key = key;
            transactionGroup.Name = name;
            transactionGroup.Parent = parent;
            transactionGroup.Transactions = transactions;
            transactionGroup.Count = transactionGroup.Transactions.Count;
            transactionGroup.SubGroups = subGroups;

            if (onlyOutputs)
                transactionGroup.Total = (double)transactionGroup.Transactions.Sum(f => f.ValueAsPositive);
            else
                transactionGroup.Total = (double)transactionGroup.Transactions.Sum(f => f.Value);

            return transactionGroup;
        }

        private Expression<Func<Transaction, object>> GetGroupKeyExpression(TransactionGroupBy group)
        {
            switch (group)
            {
                case TransactionGroupBy.Category:
                    return f => f.Category;
                case TransactionGroupBy.SubCategory:
                    return f => f.Category + "/" + f.SubCategory;
                case TransactionGroupBy.DateDay:
                    return f => f.Date.ToString("yyyy/MM/dd");
                case TransactionGroupBy.DateMonth:
                    return f => f.Date.ToString("yyyy/MM");
                case TransactionGroupBy.DateYear:
                    return f => f.Date.ToString("yyyy");
                case TransactionGroupBy.Name:
                    return f => f.Name;
            }
            return null;
        }

        private string GetGroupName(TransactionGroupBy group, List<Transaction> transactions)
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
            return null;
        }

        public class TransactionGroup
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public double Total { get; set; }
            public List<Transaction> Transactions { get; set; }
            public List<TransactionGroup> SubGroups { get; set; }
            public TransactionGroup Parent { get; set; }
            public int Count { get; set; }

            public TransactionGroup()
            {
                this.Transactions = new List<Transaction>();
                this.SubGroups = new List<TransactionGroup>();
            }

            public string GetPath(List<string> paths = null)
            {
                if (paths == null)
                    paths = new List<string>();
                if (this.Name != null)
                    paths.Add("{" + this.Name + "}");
                if (this.Parent != null)
                    this.Parent.GetPath(paths);
                
                paths.Reverse();
                return string.Join(@"/", paths.ToArray());
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class TransactionGroupInverted
        {
            //public string ChildGroupKey { get; set; }
            public string Parent { get; set; }
            public string ItemGroupName { get; set; }
            public TransactionGroupInvertedItem[] Items;

            public class TransactionGroupInvertedItem 
            {
                public string ItemPath { get; set; }
                //public string ItemName { get; set; }
                public string Category { get; set; }
                public double TotalItemInCategory { get; set; }

                public override string ToString()
                {
                    return this.ItemPath;
                }
            }

            public TransactionGroupInverted()
            {
            }

            public void AddChildToParent(TransactionGroup transactionGroup, int index)
            {
                this.Items[index] = new TransactionGroupInvertedItem()
                {
                    ItemPath = transactionGroup.GetPath(),
                    //ItemName = transactionGroup.Name,
                    Category = transactionGroup.Parent.Key,
                    TotalItemInCategory = transactionGroup.Total,
                };
            }

            public override string ToString()
            {
                return this.ItemGroupName;
            }
        }
    }
}