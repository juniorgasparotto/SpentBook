using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SpentBook.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace SpentBook.Web.Controllers
{
    [Authorize]
    public class ResumeController : Controller
    {
        private IUnitOfWork uow;
        private IHostingEnvironment env;

        public ResumeController(IUnitOfWork uow, IHostingEnvironment env)
        {
            this.uow = uow;
            this.env = env;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Resumo das dívidas";
            return View();
        }

        public ActionResult FilesList()
        {
            var files = this.GetAllFiles() ?? new string[0];
            return PartialView(files.Select(f => f.Split('\\').LastOrDefault()).ToList());
        }

        public ActionResult SpentsList()
        {
            return PartialView(this.GetSpents());
        }

        [HttpPost]
        public ActionResult MultipleUpload(IEnumerable<IFormFile> files)
        {
            var userPath = GetStatementsPath();

            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = file.FileName;
                    var fileFullName = Path.Combine(userPath, fileName);
                    if (System.IO.File.Exists(fileFullName))
                        System.IO.File.Delete(fileFullName);

                    using (var fileStream = new FileStream(fileFullName, FileMode.Create))
                        file.CopyTo(fileStream);

                    var spents = this.GetSpents(fileFullName);
                    foreach (var spent in spents)
                        uow.Transactions.Insert(spent);
                    uow.Save();
                }
            }

            return new EmptyResult();
        }

        private string GetStatementsPath()
        {
            var userName = User.Identity.Name;
            var webRoot = env.WebRootPath;
            var userPath = Path.Combine(webRoot, "Data", userName, "Spents");
            return userPath;
        }

        [HttpGet]
        public JsonResult GetChartDoughnut()
        {
            var chartDatas = new List<ChartDoughnutModel>();
            var spents = this.GetSpents();
            var groups = spents.GroupBy(f => f.Category);

            foreach (var group in groups)
            {
                var chartData = new ChartDoughnutModel()
                {
                    value = group.Sum(f => f.ValueAsPositive),
                    label = group.Key
                };

                chartDatas.Add(chartData);
            }

            //return Json(chartDatas, JsonRequestBehavior.AllowGet);
            return Json(chartDatas);
        }

        [HttpGet]
        public JsonResult GetChartBar(string groupBy = "category")
        {
            var chartBar = new ChartBarModel();
            var dicDataSet = new Dictionary<string, ChartBarModel.DataSet>();

            chartBar.labels = new List<string>();
            chartBar.datasets = new List<ChartBarModel.DataSet>();

            var spents = this.GetSpents().OrderBy(f => f.Date);
            var queryNestedGroups = (from spent in spents
                                     group spent by spent.Date.ToString("MMMM-yyyy", CultureInfo.InvariantCulture) into groupDate
                                     from groupCategory in
                                         (from spent in groupDate
                                          group spent by (groupBy == "category" ? spent.Category : spent.Category + "/" + spent.SubCategory)).ToList()
                                     group groupCategory by groupDate.Key).ToList();

            var i = 0;
            foreach (var groupDate in queryNestedGroups)
            {
                chartBar.labels.Add(groupDate.Key);

                foreach (var groupCategory in groupDate.OrderByDescending(f => f.Sum(s => s.ValueAsPositive)))
                {
                    var color = "";
                    var key = groupCategory.Key;
                    ChartBarModel.DataSet dataset;
                    if (!dicDataSet.ContainsKey(key))
                    {
                        color = GetRandomColor();
                        dataset = new ChartBarModel.DataSet()
                        {
                            data = new decimal[queryNestedGroups.Count],
                            label = groupCategory.Key,
                            fillColor = color,
                            highlightFill = color,
                            highlightStroke = color,
                            strokeColor = color,
                        };

                        dicDataSet.Add(key, dataset);
                    }
                    else
                    {
                        dataset = dicDataSet[key];
                    }

                    dataset.data[i] = groupCategory.Sum(f => f.ValueAsPositive);

                    chartBar.datasets.Add(dataset);
                }

                i++;
            }

            //return Json(chartBar, JsonRequestBehavior.AllowGet);
            return Json(chartBar);
        }

        Random randomGen = new Random();
        public string GetRandomColor()
        {
            return "RGB()";
            //KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            //KnownColor randomColorName = names[randomGen.Next(names.Length)];
            //Color randomColor = Color.FromKnownColor(randomColorName);
            //return "RGB(" + 0 + "," + randomColor.G.ToString() + "," + randomColor.B.ToString() + ")";
        }

        public string[] GetAllFiles()
        {
            var userPath = GetStatementsPath();
            if (Directory.Exists(userPath))
                return Directory.GetFiles(userPath, "*.csv", SearchOption.AllDirectories);

            return null;
        }

        public List<Transaction> GetSpents()
        {
            var files = GetAllFiles();
            var list = new List<Transaction>();
            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                    list.AddRange(this.GetSpents(file));
            }

            return list;
        }

        public List<Transaction> GetSpents(string fullName)
        {
            var transactions = new List<Transaction>();
            using (var sr = new StreamReader(fullName))
            {
                var reader = new CsvReader(sr);
                reader.Parser.Configuration.ThrowOnBadData = false;
                reader.Parser.Configuration.IgnoreBlankLines = true;
                //reader.Parser.Configuration.IgnoreHeaderWhiteSpace = true;
                reader.Parser.Configuration.Delimiter = ";";
               // reader.Parser.Configuration.IgnoreReadingExceptions = true;

                var lines = reader.GetRecords<CSVLine>().ToList();
                foreach (var line in lines)
                {
                    var transaction = new Transaction()
                    {
                        Date = line.Date,
                        Category = line.Category,
                        SubCategory = line.SubCategory,
                        Name = line.Name,
                        Value = line.Value,
                    };

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        private class CSVLine
        {
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
        }
    }
}