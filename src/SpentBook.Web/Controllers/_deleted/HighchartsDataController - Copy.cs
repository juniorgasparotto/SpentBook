//using CsvHelper;
//using SpentBook.Data.FileSystem;
//using SpentBook.Domain;
//using SpentBook.Web.Filters;
//using SpentBook.Web.Models;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Web;
//using System.Web.Mvc;
//using System.Linq.Dynamic;
//using DotNet.Highcharts.Options;
//using DotNet.Highcharts.Helpers;
//using ExpressionGraph;

//namespace SpentBook.Web.Controllers
//{
//    [JsonOutputWhenModelInvalid]
//    [JsonOutputWhenGenericException]
//    public class HighchartsDataController : Controller
//    {
//        [HttpPost]
//        public JsonResult Panels(Guid dashboardId, List<Panel> panelsExistsInInterface)
//        {
//            var uow = Helper.GetUnitOfWorkByCurrentUser();
//            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
//            var panelsExistsInDB = dashboard.Panels;

//            panelsExistsInDB = panelsExistsInDB ?? new List<Panel>();
//            panelsExistsInInterface = panelsExistsInInterface ?? new List<Panel>();

//            // exists in DB but not exists in interface
//            var news = (
//                from panelDB in panelsExistsInDB
//                from panelInterface in panelsExistsInInterface.Where(f => f.Id == panelDB.Id).DefaultIfEmpty()
//                where panelInterface == null
//                orderby panelDB.PanelOrder, panelDB.Title ascending
//                select new { panelDB.Id, panelDB.LastUpdateDate, panelDB.PanelOrder, PanelWidth = panelDB.PanelWidth.ToString().ToLower() }
//            ).ToList();

//            // exists in both, but the update date in DB is more than interface
//            var updateds = (
//                from panelDB in panelsExistsInDB
//                join panelInterface in panelsExistsInInterface on panelDB.Id equals panelInterface.Id
//                where panelDB.LastUpdateDate > panelInterface.LastUpdateDate
//                orderby panelDB.PanelOrder, panelDB.Title ascending
//                select new { panelDB.Id, panelDB.LastUpdateDate, panelDB.PanelOrder, PanelWidth = panelDB.PanelWidth.ToString().ToLower() }
//            ).ToList();

//            // exists in interface but not exists in DB
//            var deleteds = (
//                from panelInterface in panelsExistsInInterface
//                from panelDB in panelsExistsInDB.Where(f => f.Id == panelInterface.Id).DefaultIfEmpty()
//                where panelDB == null
//                orderby panelInterface.PanelOrder, panelInterface.Title ascending
//                select panelInterface.Id
//            ).ToList();

//            var changes = new
//            {
//                News = news,
//                Deleteds = deleteds,
//                Updateds = updateds
//            };

//            return Json(changes, JsonRequestBehavior.AllowGet);
//        }

//        [HttpGet]
//        public JsonResult Transactions(Guid dashboardId, Guid panelId)
//        {
//            var transactionGroup = this.GetTransactionGroupRootByPanel(dashboardId, panelId);
//            var categories = new List<string>();
//            var seriesDatas = new List<SeriesData>();
//            var series = new List<Series>();
//            var seriesDrilldown = new List<Series>();

//            var dicSeries = new Dictionary<string, Series>();
//            var dicSeriesData = new Dictionary<string, SeriesData[]>();

//            if (transactionGroup.SubGroups == null)
//            {
//                foreach(var transaction in transactionGroup.Transactions)
//                {
//                    seriesDatas.Add(new SeriesData() {
//                        Y = (Number)transaction.Value,
//                        Name = transaction.Name
//                    });
//                }
//            }
//            else
//            {
//                foreach (var group in transactionGroup.SubGroups)
//                {
//                    seriesDatas.Add(new SeriesData()
//                    {
//                        Y = (Number)group.Total,
//                        Name = group.Name,
//                        Id = group.Key,
//                        Drilldown = group.Key
//                    });

//                    if (group.SubGroups != null)
//                    {
//                        var seriesDatasDrill = new List<object[]>();

//                        foreach (var group2 in group.SubGroups)
//                        {
//                            seriesDatasDrill.Add(new object[] { group2.Key, group2.Total });
//                        }

//                        var serieDrilldown = new Series
//                        {
//                            Name = group.Name,
//                            Id = group.Key,
//                            Data = new DotNet.Highcharts.Helpers.Data(seriesDatasDrill.ToArray()),
//                            //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = false }
//                        };
//                        seriesDrilldown.Add(serieDrilldown);
//                    }
//                }

//                // modelo 2
//                //foreach (var parent in transactionGroup.SubGroups)
//                //{
//                //    categories.Add(parent.Name);

//                //    if (parent.SubGroups != null)
//                //    {
//                //        foreach (var child in parent.SubGroups)
//                //        {
//                //            var seriesDatas2 = new SeriesData[transactionGroup.SubGroups.Count];
//                //            series.Add(new Series
//                //            {
//                //                Id = child.Name,
//                //                Name = child.Name,
//                //                Data = new DotNet.Highcharts.Helpers.Data(seriesDatas2),
//                //                PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
//                //            });

//                //            var parentIndex = transactionGroup.SubGroups.IndexOf(transactionGroup.SubGroups.FirstOrDefault(f => f.Name == child.ParentName));
//                //            seriesDatas2[parentIndex] = new SeriesData()
//                //            {
//                //                Y = (Number)child.Total,
//                //                Name = parent.Name,
//                //                //Id = group.Name,
//                //            };
//                //        }
//                //    }
//                //}
                
//                var index = 0;
//                foreach (var parent in transactionGroup.SubGroups)
//                {
//                    categories.Add(parent.Key);

//                    if (parent.SubGroups != null)
//                    {
//                        foreach (var child in parent.SubGroups)
//                        {
//                            var serie = default(Series);
//                            var serieData = default(SeriesData[]);

//                            if (dicSeries.ContainsKey(child.Key))
//                            {
//                                serie = dicSeries[child.Key];
//                                serieData = dicSeriesData[child.Key];
//                            }
//                            else
//                            {
//                                serieData = new SeriesData[transactionGroup.SubGroups.Count];
//                                serie = new Series
//                                {
//                                    Id = child.Key,
//                                    Name = child.Key,
//                                    Data = new DotNet.Highcharts.Helpers.Data(serieData),
//                                    //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
//                                };

//                                dicSeriesData.Add(child.Key, serieData);
//                                dicSeries.Add(child.Key, serie);
//                            }
//                            serieData[index] = new SeriesData {
//                                Y = (Number)child.Total,
//                                Name = child.Key,
//                                Drilldown = child.Key
//                            };
//                        }

//                        index++;
//                    }
//                }
//            }

//            //var serie1 = new Series
//            //    {
//            //        Name = "Browser brands",
//            //        Data = new DotNet.Highcharts.Helpers.Data(seriesDatas.ToArray()),
//            //        PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
//            //    };

//            //var serie2 = new Series
//            //{
//            //    Name = "Browser brands 2",
//            //    Data = new DotNet.Highcharts.Helpers.Data(seriesDatas.ToArray()),
//            //    PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
//            //};

//            //series.Add(serie1);
//            //series.Add(serie2);
            
//            var drilldown = new DotNet.Highcharts.Options.Drilldown
//            {
//                Series = seriesDrilldown.ToArray()
//            };

//            return Json(new { Categories = categories, Series = dicSeries.Values, Drilldown = drilldown }, JsonRequestBehavior.AllowGet);
//        }

//        [HttpGet]
//        public JsonResult Transactions2(Guid dashboardId, Guid panelId)
//        {
//            var transactionGroup = this.GetTransactionGroupRootByPanel(dashboardId, panelId);
//            var series = new List<Series>();
//            var seriesDrilldown = new List<Series>();

//            var dicSeries = new Dictionary<string, Series>();
//            var dicSeriesData = new Dictionary<string, SeriesData[]>();

//            // LEVEL 1
//            var index = 0;
//            foreach (var level1 in transactionGroup.SubGroups)
//            {
//                // LEVEL 1
//                if (level1.SubGroups.Count > 0)
//                {
//                    foreach (var level2 in level1.SubGroups)
//                    {
//                        var serie = default(Series);
//                        var serieData = default(SeriesData[]);

//                        if (dicSeries.ContainsKey(level2.Key))
//                        {
//                            serie = dicSeries[level2.Key];
//                            serieData = dicSeriesData[level2.Key];
//                        }
//                        else
//                        {
//                            serieData = new SeriesData[transactionGroup.SubGroups.Count];
//                            serie = new Series
//                            {
//                                Id = level2.Key,
//                                Name = level2.Name,
//                                Data = new DotNet.Highcharts.Helpers.Data(serieData),
//                                //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
//                            };

//                            dicSeriesData.Add(level2.Key, serieData);
//                            dicSeries.Add(level2.Key, serie);
//                        }
//                        serieData[index] = new SeriesData
//                        {
//                            Y = (Number)level2.Total,
//                            Name = level1.Name,
//                            Id = level1.Key,
//                            Drilldown = level2.Key
//                        };

//                        if (level2.SubGroups.Count > 0)
//                        {
//                            var seriesDatasDrill = new List<object[]>();

//                            foreach (var level3 in level2.SubGroups)
//                            {
//                                seriesDatasDrill.Add(new object[] { level3.Name, level3.Total });
//                            }

//                            var serieDrilldown = new Series
//                            {
//                                Id = level2.Key,
//                                Name = level2.Name,
//                                Data = new DotNet.Highcharts.Helpers.Data(seriesDatasDrill.ToArray()),
//                            };
//                            seriesDrilldown.Add(serieDrilldown);
//                        }
//                    }
//                    index++;
//                }
//            }

//            return Json(new { Series = dicSeries.Values, Drilldown = seriesDrilldown }, JsonRequestBehavior.AllowGet);
//        }

//        //[HttpGet]
//        //public object ConvertTransactionGroupToHighChartsData(TransactionGroup transactionGroup, List<TransactionGroup> transactionGroupParents)
//        //{
//        //    var serie = new Series();
//        //    var serieData = new SeriesData[transactionGroupParents.Count];
//        //    serie.Id = transactionGroup.Key;
//        //    serie.Name = transactionGroup.Name;
//        //    serie.Data = new DotNet.Highcharts.Helpers.Data(serieData);
//        //}

//        [HttpGet]
//        public TransactionGroup GetTransactionGroupRootByPanel(Guid dashboardId, Guid panelId)
//        {
//            var uow = Helper.GetUnitOfWorkByCurrentUser();
//            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
//            var panel = dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
//            var query = uow.Transactions.AsQueryable();

//            var a = new ResumeController();
//            var transactions = a.GetSpents();
//            query = transactions.AsQueryable();

//            var onlyInputs = panel.Filter.TransactionType == TransactionType.Input;
//            var onlyOutputs = panel.Filter.TransactionType == TransactionType.Output;

//            if (onlyInputs)
//                query = query.Where(t => t.Value > 0);

//            if (onlyOutputs)
//                query = query.Where(t => t.Value < 0);

//            if (panel.Filter.Categories != null && panel.Filter.Categories.Count > 0)
//                query = query.Where(t => panel.Filter.Categories.Contains(t.Category));

//            if (panel.Filter.SubCategories != null && panel.Filter.SubCategories.Count > 0)
//                query = query.Where(t => panel.Filter.SubCategories.Contains(t.SubCategory));

//            if (panel.Filter.Names != null && panel.Filter.Names.Count > 0)
//                query = query.Where(t => panel.Filter.Names.Contains(t.Name));

//            if (panel.Filter.DateStart != null)
//                query = query.Where(t => t.Date >= panel.Filter.DateStart);

//            if (panel.Filter.DateEnd != null)
//                query = query.Where(t => t.Date <= panel.Filter.DateEnd);

//            if (panel.Filter.ValueStart != null)
//                query = query.Where(t => t.Value >= panel.Filter.ValueStart);

//            if (panel.Filter.ValueEnd != null)
//                query = query.Where(t => t.Value <= panel.Filter.ValueEnd);

//            switch (panel.OrderBy) {
//                case TransactionOrder.Category:
//                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
//                        query = query.OrderBy(f => f.Category);
//                    else
//                        query = query.OrderByDescending(f => f.Category);
//                    break;
//                case TransactionOrder.SubCategory:
//                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
//                        query = query.OrderBy(f => f.SubCategory);
//                    else
//                        query = query.OrderByDescending(f => f.SubCategory);
//                    break;
//                case TransactionOrder.Date:
//                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
//                        query = query.OrderBy(f => f.Date);
//                    else
//                        query = query.OrderByDescending(f => f.Date);
//                    break;
//                case TransactionOrder.Name:
//                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
//                        query = query.OrderBy(f => f.Name);
//                    else
//                        query = query.OrderByDescending(f => f.Name);
//                    break;
//                case TransactionOrder.Value:
//                    if (panel.OrderByClassification == TransactionOrderClassification.Asc)
//                        query = query.OrderBy(f => f.Value);
//                    else
//                        query = query.OrderByDescending(f => f.Value);
//                    break;
//            }

//            //var groupsBy = new List<TransactionGroupBy> { panel.GroupBy, panel.GroupBy2, panel.GroupBy3 };
//            var groupsBy = new List<TransactionGroupBy> { 
//                TransactionGroupBy.DateMonth, 
//                TransactionGroupBy.DateDay, 
//                TransactionGroupBy.Category, 
//                TransactionGroupBy.SubCategory, 
//            };

//            var subGroups = this.GetTransactionGroups(query.ToList(), groupsBy, onlyOutputs);
//            var transactionsRoot = subGroups.SelectMany(f=>f.Transactions).ToList();
//            var transactionGroupRoot = CreateTransactionGroup(null, null, null, transactionsRoot, subGroups, null, 0, onlyOutputs);
            
//            foreach (var s in subGroups)
//                s.Parent = transactionGroupRoot;

//            return transactionGroupRoot;
//        }

//        private List<TransactionGroup> GetTransactionGroups(List<Transaction> transactions, List<TransactionGroupBy> groupBys, bool onlyOutputs)
//        {
//            Func<List<Transaction>, TransactionGroup, int, List<TransactionGroup>> methodGroup = null;
//            methodGroup = (_transactions, transactionGroupParent, groupByIndex) =>
//            {
//                var groupBy = TransactionGroupBy.None;
//                var transactionGroups = new List<TransactionGroup>();

//                if (groupBys.Count > groupByIndex)
//                    groupBy = groupBys[groupByIndex];
                
//                if (groupBy != TransactionGroupBy.None)
//                {
//                    var groupByName = default(string);
//                    var group = _transactions
//                        .AsQueryable()
//                        .GroupBy(this.GetGroupKeyExpression(groupBy, out groupByName))
//                        .ToList();

//                    var nextGroupByIndex = groupByIndex + 1;

//                    transactionGroups.AddRange(group.Select(
//                        (g, gIndex) =>
//                        {
//                            var list = g.ToList();
//                            var name = this.GetGroupName(groupBy, list);
//                            var key = g.Key.ToString();
//                            var transactionGroup = CreateTransactionGroup(key, name, groupByName, list, null, transactionGroupParent, nextGroupByIndex, onlyOutputs);
//                            transactionGroup.SubGroups = methodGroup(list, transactionGroup, nextGroupByIndex);
//                            return transactionGroup;
//                        }
//                    ).ToList());

//                }

//                return transactionGroups;
//            };

//            var transactionGroupsReturn = methodGroup(transactions, null, 0);
//            return transactionGroupsReturn;
//        }

//        [HttpGet]
//        public JsonResult Transactions3(Guid dashboardId, Guid panelId, bool tryCategorize)
//        {
//            var transactionGroup = this.GetTransactionGroupRootByPanel(dashboardId, panelId);
//            var inverted = GetTransactionGroupTransversal2(transactionGroup, null);
//            var series = new List<Series>();
//            var seriesDrilldown = new List<Series>();

//            foreach(var item in inverted)
//            {
//                var serieData = new SeriesData[item.Items.Length];
//                var serie = new Series
//                {
//                    Id = item.ParentPath,
//                    Name = item.ItemGroupName,
//                    Data = new DotNet.Highcharts.Helpers.Data(serieData),
//                };

//                if (item.ParentPath == null)
//                    series.Add(serie);
//                else
//                {
//                    seriesDrilldown.Add(serie);
//                    serie.PlotOptionsBar = new PlotOptionsBar { ColorByPoint = false };
//                }

//                var index = 0;
//                foreach (var data in item.Items)
//                {
//                    if (data == null)
//                    {
//                        serieData[index++] = null;
//                    }
//                    else
//                    {
//                        serieData[index++] = new SeriesData
//                        {
//                            Y = (Number)data.TotalItemInCategory,
//                            Name = data.Category,
//                            Id = data.Category,
//                            Drilldown = data.ItemPath
//                        };
//                    }
//                }
//            }

//            return Json(new { Series = series, Drilldown = seriesDrilldown }, JsonRequestBehavior.AllowGet);
//        }

//        [HttpGet]
//        public ActionResult Debug(Guid dashboardId, Guid panelId, bool tryCategorize)
//        {
//            var root = GetTransactionGroupRootByPanel(dashboardId, panelId);
//            return Json(GetTransactionGroupTransversal(root, tryCategorize), JsonRequestBehavior.AllowGet);
//        }

//        [HttpGet]
//        public ActionResult Debug2(Guid dashboardId, Guid panelId, bool tryCategorize)
//        {
//            var root = GetTransactionGroupRootByPanel(dashboardId, panelId);
//            return Json(GetTransactionGroupTransversal2(root, new int[] { 1, 2 }), JsonRequestBehavior.AllowGet);
//        }

//        [HttpGet]
//        private List<TransactionGroupTransversal> GetTransactionGroupTransversal2(TransactionGroup transactionGroupRoot, int[] categorizeLevelsWithNext)
//        {
//            var listReturn = new List<TransactionGroupTransversal>();

//            var master = new TransactionGroup { Name = "master", Key = "master" };
//            master.SubGroups.Add(transactionGroupRoot);

//            var expressions = ExpressionGraph.ExpressionBuilder<TransactionGroup>.Build(master.SubGroups, f => f.SubGroups, true, false, false);
//            var expression = expressions.FirstOrDefault();

//            var level1 = expression.Where(f=>f.Level == 2).ToEntities().ToList();
//            var level2 = expression.Where(f => f.Level == 3).ToEntities().ToList();

//            listReturn.AddRange(Get(null, level1, level2));

//            foreach (var item in level2)
//            {
//                var level3 = expression.Where(f => f.Entity == item).Descendants(1).ToEntities().ToList();
//                var level4 = expression.Where(f => f.Entity == item).Descendants(2, 2).ToEntities().ToList();
//                listReturn.AddRange(Get(item.GetPath(), level3, level4));
//            }

//            return listReturn;
            
//            //var expressionsItems = expressions.SelectMany(f => f);
            
//            //foreach (var expression in expressions)
//            //    expression.IterationAll
//            //    (
//            //        itemWhenStart =>
//            //        {
//            //            if (itemWhenStart.IsRoot())
//            //                return;

//            //            var current = itemWhenStart.Entity;
//            //            var children = itemWhenStart.Descendants(depthEnd: 1).ToList();
//            //            if (currentOpen == null)
//            //            {
//            //                currentOpen = new TransactionGroupTransversal()
//            //                {
//            //                    ParentPath = current.GetPath(),
//            //                    ItemGroupName = current.Key,
//            //                    Items = new TransactionGroupTransversal.TransactionGroupTransversalItem[children.Count]
//            //                };

//            //                listReturn.Add(currentOpen);
//            //            }
//            //            else
//            //            {
//            //                currentOpen.Add(current.GetPath(), current.Key, current.Total, 0);
//            //            }
//            //        },
//            //        itemWhenEnd =>
//            //        {
//            //            if (!itemWhenEnd.IsRoot())
//            //            {
//            //                currentOpen = null;
//            //            }
//            //        }
//            //    );

//            return listReturn;
//        }

//        private List<TransactionGroupTransversal> Get(string parentPath, List<TransactionGroup> level1, List<TransactionGroup> level2)
//        {
//            var listReturn = new List<TransactionGroupTransversal>();
//            var categories = level2.GroupBy(f => f.Key).ToList();
//            foreach (var category in categories)
//            {
//                var list = category.ToList();
//                var inverted = new TransactionGroupTransversal()
//                {
//                    ParentPath = parentPath,
//                    ItemGroupName = category.Key,
//                    Items = new TransactionGroupTransversal.TransactionGroupTransversalItem[level1.Count]
//                };

//                listReturn.Add(inverted);

//                foreach (var itemCategory in list)
//                {
//                    var indexOfParent = level1.IndexOf(itemCategory.Parent);
//                    inverted.Add(itemCategory.GetPath(), itemCategory.Parent.Key, itemCategory.Total, indexOfParent);
//                }
//            }

//            return listReturn;
//        }

//        private List<TransactionGroupTransversal> Get2(TransactionGroup current)
//        {
//            var listReturn = new List<TransactionGroupTransversal>();
//            var name = current.Key ?? current.GroupBy;
//            if (current.Parent == null && name == null)
//                name = current.SubGroups.First().GroupBy ?? "Todos";

//            var inverted = new TransactionGroupTransversal()
//            {
//                ParentPath = current.GetPath(),
//                ItemGroupName = name,
//                Items = new TransactionGroupTransversal.TransactionGroupTransversalItem[current.SubGroups.Count]
//            };

//            listReturn.Add(inverted);

//            var index = 0;
//            foreach (var childGroup in current.SubGroups)
//            {
//                inverted.Add(childGroup.GetPath(), childGroup.Key, childGroup.Total, index++);
//            }
//            return listReturn;
//        }

//        [HttpGet]
//        private List<TransactionGroupTransversal> GetTransactionGroupTransversal(TransactionGroup transactionGroupRoot, bool tryCategorize = true)
//        {
//            var listReturn = new List<TransactionGroupTransversal>();
//            Action<TransactionGroup, int> methodGroup = null;
//            methodGroup = (current, level) =>
//            {
//                if (current.SubGroups.Count == 0)
//                    return;

//                var children = current.SubGroups;
//                var grandchildrenGroups = tryCategorize ? children.SelectMany(f => f.SubGroups).GroupBy(f => f.Key).ToList() : null;
//                //if (tryCategorize && grandchildrenGroups.Count > 0)
//                // categoriza apenas o root se for solicitado
//                if (tryCategorize && current.Parent == null)
//                { 
//                    foreach (var grandchildrenGroup in grandchildrenGroups)
//                    {
//                        var list = grandchildrenGroup.ToList();
//                        var inverted = new TransactionGroupTransversal()
//                        {
//                            ParentPath = current.GetPath(),
//                            ItemGroupName = grandchildrenGroup.Key,
//                            Items = new TransactionGroupTransversal.TransactionGroupTransversalItem[children.Count]
//                        };

//                        listReturn.Add(inverted);

//                        foreach (var grandchild in list)
//                        {
//                            var indexOfParent = current.SubGroups.IndexOf(grandchild.Parent);
//                            inverted.Add(grandchild.GetPath(), grandchild.Parent.Key, grandchild.Total, indexOfParent);
//                            methodGroup(grandchild, 0);
//                        }
//                    }
//                }
//                else
//                {
//                    var name = current.Key ?? current.GroupBy;
//                    if (current.Parent == null && name == null)
//                        name = current.SubGroups.First().GroupBy ?? "Todos";

//                    var inverted = new TransactionGroupTransversal()
//                    {
//                        ParentPath = current.GetPath(),
//                        ItemGroupName = name,
//                        Items = new TransactionGroupTransversal.TransactionGroupTransversalItem[current.SubGroups.Count]
//                    };

//                    listReturn.Add(inverted);

//                    var index = 0;
//                    foreach (var childGroup in current.SubGroups)
//                    {
//                        inverted.Add(childGroup.GetPath(), childGroup.Key, childGroup.Total, index++);
//                        methodGroup(childGroup, 0);
//                    }
//                }

//                //foreach(var child in children)
//                //    methodGroup(child, 0);
//            };

//            methodGroup(transactionGroupRoot, 0);

//            return listReturn;
//        }

//        //[HttpGet]
//        //private List<TransactionGroupTransversal> GetTransactionGroupTransversal(TransactionGroup transactionGroupRoot)
//        //{
//        //    var listReturn = new List<TransactionGroupTransversal>();
//        //    Action<TransactionGroup, int> methodGroup = null;
//        //    methodGroup = (item, level) =>
//        //    {
//        //        var inverted = new TransactionGroupTransversal()
//        //        {
//        //            ParentPath = item.GetPath(),
//        //            ItemGroupName = item.Name,
//        //            Items = new TransactionGroupTransversal.TransactionGroupTransversalItem[item.SubGroups.Count]
//        //        };

//        //        listReturn.Add(inverted);

//        //        var index = 0;
//        //        foreach (var childGroup in item.SubGroups)
//        //        {
//        //            inverted.Add(childGroup.GetPath(), childGroup.Name, childGroup.Total, index++);
//        //            methodGroup(childGroup, 0);
//        //        }
//        //    };

//        //    methodGroup(transactionGroupRoot, 0);

//        //    return listReturn;
//        //}

//        private TransactionGroup CreateTransactionGroup(string key, string name, string groupBy, List<Transaction> transactions, List<TransactionGroup> subGroups, TransactionGroup parent, int level, bool onlyOutputs)
//        {
//            var transactionGroup = new TransactionGroup();
//            transactionGroup.Key = key;
//            transactionGroup.GroupBy = groupBy;
//            transactionGroup.Name = name;
//            transactionGroup.Parent = parent;
//            transactionGroup.Transactions = transactions;
//            transactionGroup.Count = transactionGroup.Transactions.Count;
//            transactionGroup.SubGroups = subGroups;
//            transactionGroup.Level = level;

//            if (onlyOutputs)
//                transactionGroup.Total = (double)transactionGroup.Transactions.Sum(f => f.ValueAsPositive);
//            else
//                transactionGroup.Total = (double)transactionGroup.Transactions.Sum(f => f.Value);

//            return transactionGroup;
//        }

//        private System.Linq.Expressions.Expression<Func<Transaction, object>> GetGroupKeyExpression(TransactionGroupBy group, out string groupName)
//        {
//            switch (group)
//            {
//                case TransactionGroupBy.Category:
//                    groupName = "Categorias";
//                    return f => f.Category;
//                case TransactionGroupBy.SubCategory:
//                    groupName = "Sub-categorias";
//                    return f => f.Category + "/" + f.SubCategory;
//                case TransactionGroupBy.DateDay:
//                    groupName = "Dias";
//                    return f => f.Date.ToString("yyyy/MM/dd");
//                case TransactionGroupBy.DateMonth:
//                    groupName = "Meses";
//                    return f => f.Date.ToString("yyyy/MM");
//                case TransactionGroupBy.DateYear:
//                    groupName = "Anos";
//                    return f => f.Date.ToString("yyyy");
//                case TransactionGroupBy.Name:
//                    groupName = "Nomes";
//                    return f => f.Name;
//            }
//            groupName = "Nenhum";
//            return null;
//        }

//        private string GetGroupName(TransactionGroupBy group, List<Transaction> transactions)
//        {
//            var transaction = transactions.FirstOrDefault();
//            switch (group)
//            {
//                case TransactionGroupBy.Category:
//                    return transaction.Category;
//                case TransactionGroupBy.SubCategory:
//                    return transaction.SubCategory;
//                case TransactionGroupBy.DateDay:
//                    return transaction.Date.ToString("yyyy/MM/dd");
//                case TransactionGroupBy.DateMonth:
//                    return transaction.Date.ToString("yyyy/MM");
//                case TransactionGroupBy.DateYear:
//                    return transaction.Date.ToString("yyyy");
//                case TransactionGroupBy.Name:
//                    return transaction.Name;
//            }
//            return null;
//        }

//        public class TransactionGroup
//        {
//            public string Key { get; set; }
//            public string GroupBy { get; set; }
//            public string Name { get; set; }
//            public double Total { get; set; }
//            public List<Transaction> Transactions { get; set; }
//            public List<TransactionGroup> SubGroups { get; set; }
//            public TransactionGroup Parent { get; set; }
//            public int Count { get; set; }

//            public TransactionGroup()
//            {
//                this.Transactions = new List<Transaction>();
//                this.SubGroups = new List<TransactionGroup>();
//            }

//            public string GetPath()
//            {
//                var paths = new List<string>();
//                this.GetPath(paths);
//                paths.Reverse();

//                if (paths.Count == 0)
//                    return null;

//                return string.Join(@"/", paths.ToArray());
//            }

//            private void GetPath(List<string> paths)
//            {
//                if (this.Key != null)
//                    paths.Add("{" + this.Key + "}");

//                if (this.Parent != null)
//                    this.Parent.GetPath(paths);
//            }

//            public override string ToString()
//            {
//                return this.Name;
//            }


//            public int Level { get; set; }
//        }

//        public class TransactionGroupTransversal
//        {
//            public string ParentPath { get; set; }
//            public string ItemGroupName { get; set; }
//            public TransactionGroupTransversalItem[] Items;

//            public class TransactionGroupTransversalItem 
//            {
//                public string ItemPath { get; set; }
//                public string Category { get; set; }
//                public double TotalItemInCategory { get; set; }

//                public override string ToString()
//                {
//                    return this.ItemPath;
//                }
//            }

//            public TransactionGroupTransversal()
//            {
//            }

//            public void Add(string itemPath, string category, double total, int index)
//            {
//                this.Items[index] = new TransactionGroupTransversalItem()
//                {
//                    ItemPath = itemPath,
//                    Category = category,
//                    TotalItemInCategory = total,
//                };
//            }

//            public override string ToString()
//            {
//                return this.ItemGroupName;
//            }
//        }
//    }
//}