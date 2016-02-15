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
            var transactionGroup = this.GetTransactionsGroupByPanel(dashboardId, panelId);
            var categories = new List<string>();
            var seriesDatas = new List<SeriesData>();
            var series = new List<Series>();
            var seriesDrilldown = new List<Series>();

            var dicSeries = new Dictionary<string, Series>();
            var dicSeriesData = new Dictionary<string, object[]>();

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
                foreach (var group in transactionGroup.SubGroups.Values)
                {
                    seriesDatas.Add(new SeriesData()
                    {
                        Y = (Number)group.Total,
                        Name = group.Name,
                        //Id = group.Name,
                        Drilldown = group.Name
                    });

                    if (group.SubGroups != null)
                    {
                        var seriesDatasDrill = new List<object[]>();

                        foreach (var group2 in group.SubGroups.Values)
                        {
                            seriesDatasDrill.Add(new object[] { group2.Name, group2.Total });
                        }

                        var serieDrilldown = new Series
                        {
                            //Id = group.Name,
                            Name = group.Name,
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
                foreach (var parent in transactionGroup.SubGroups.Values)
                {
                    categories.Add(parent.Name);

                    if (parent.SubGroups != null)
                    {
                        foreach (var child in parent.SubGroups.Values)
                        {
                            var serie = default(Series);
                            var serieData = default(object[]);

                            if (dicSeries.ContainsKey(child.Name))
                            {
                                serie = dicSeries[child.Name];
                                serieData = dicSeriesData[child.Name];
                            }
                            else
                            {
                                serieData = new object[transactionGroup.SubGroups.Count];
                                serie = new Series
                                {
                                    Id = child.Name,
                                    Name = child.Name,
                                    Data = new DotNet.Highcharts.Helpers.Data(serieData),
                                    //PlotOptionsBar = new PlotOptionsBar { ColorByPoint = true }
                                };

                                dicSeriesData.Add(child.Name, serieData);
                                dicSeries.Add(child.Name, serie);
                            }
                            serieData[index] = child.Total;
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

        public TransactionGroup GetTransactionsGroupByPanel(Guid dashboardId, Guid panelId)
        {
            var uow = Helper.GetUnitOfWorkByCurrentUser();
            var dashboard = uow.Dashboards.Get(f => f.Id == dashboardId).FirstOrDefault();
            var panel = dashboard.Panels.FirstOrDefault(f => f.Id == panelId);
            var query = uow.Transactions.AsQueryable();
            
            if (panel.Filter.TransactionType == TransactionType.Input)
                query = query.Where(t => t.Value > 0);

            if (panel.Filter.TransactionType == TransactionType.Output)
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

            var transactionGroup1 = new TransactionGroup();
            transactionGroup1.Name = null;
            transactionGroup1.Transactions = query.ToList();
            transactionGroup1.Total = transactionGroup1.Transactions.Sum(f => f.Value);
            transactionGroup1.Count = transactionGroup1.Transactions.Count;

            if (panel.GroupBy == TransactionGroupBy.None)
            {
                return transactionGroup1;
            }
            else 
            {   
                if (panel.GroupBy2 == TransactionGroupBy.None)
                {
                    var group1 = query.GroupBy(this.GetGroupExpression(panel.GroupBy));

                    foreach (var groupItem in group1)
                    { 
                        var transactionGroup2 = new TransactionGroup();
                        transactionGroup2.Name = groupItem.Key.ToString();
                        transactionGroup2.Transactions = groupItem.ToList();
                        transactionGroup2.Total = transactionGroup2.Transactions.Sum(f => f.Value);
                        transactionGroup2.Count = transactionGroup2.Transactions.Count;

                        transactionGroup1.SubGroups.Add(transactionGroup2.Name, transactionGroup2);
                    }
                    return transactionGroup1;
                }
                else
                {
                    var dicReturn = new Dictionary<object, Dictionary<object, List<Transaction>>>();                    
                    var group1 = query.GroupBy(this.GetGroupExpression(panel.GroupBy));

                    foreach (var groupItem in group1)
                    {
                        var dicGroup2 = new Dictionary<object, List<Transaction>>();
                        var transactionGroup2 = new TransactionGroup();
                        transactionGroup2.Name = groupItem.Key.ToString();
                        transactionGroup2.Transactions = groupItem.ToList();
                        transactionGroup2.Total = transactionGroup2.Transactions.Sum(f => f.Value);
                        transactionGroup2.Count = transactionGroup2.Transactions.Count;

                        transactionGroup1.SubGroups.Add(transactionGroup2.Name, transactionGroup2);

                        var group2 = groupItem.AsQueryable().GroupBy(this.GetGroupExpression(panel.GroupBy2));
                        foreach (var groupItem2 in group2)
                        {
                            dicGroup2.Add(groupItem2.Key, groupItem2.ToList());
                            var transactionGroup3 = new TransactionGroup();
                            transactionGroup3.ParentName = transactionGroup2.Name;
                            transactionGroup3.Name = groupItem2.Key.ToString();
                            transactionGroup3.Transactions = groupItem2.ToList();
                            transactionGroup3.Total = transactionGroup3.Transactions.Sum(f => f.Value);
                            transactionGroup3.Count = transactionGroup3.Transactions.Count;

                            transactionGroup2.SubGroups.Add(transactionGroup3.Name, transactionGroup3);
                        }
                        dicReturn.Add(groupItem.Key, dicGroup2);
                    }

                    return transactionGroup1;
                }
            }
        }

        private Expression<Func<Transaction, object>> GetGroupExpression(TransactionGroupBy group)
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

        public class TransactionGroup
        {
            public string Name { get; set; }
            public decimal Total { get; set; }
            public List<Transaction> Transactions { get; set; }
            public Dictionary<object, TransactionGroup> SubGroups { get; set; }

            public TransactionGroup()
            {
                this.Transactions = new List<Transaction>();
                this.SubGroups = new Dictionary<object, TransactionGroup>();
            }

            public int Count { get; set; }

            public string ParentName { get; set; }
        }
    }
}