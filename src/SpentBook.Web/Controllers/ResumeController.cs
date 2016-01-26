using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpentBook.Domain;

namespace SpentBook.Web.Controllers
{
    public class ResumeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Resumo das dívidas";
            return View();
        }

        public ActionResult FilesList()
        {
            return PartialView(this.GetAllFiles().Select(f=>f.Split('\\').LastOrDefault()).ToList());
        }

        public ActionResult SpentsList()
        {
            return PartialView(this.GetSpents());
        }

        [HttpPost]
        public ActionResult MultipleUpload(IEnumerable<HttpPostedFileBase> files)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Not logged");

            var uploadPath = Server.MapPath("/Data");
            var userPath = uploadPath + "/" + userName + "/Spents";
            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);
            
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = file.FileName;
                    var fileFullName = userPath + "/" + fileName;
                    if (System.IO.File.Exists(fileFullName))
                        System.IO.File.Delete(fileFullName);

                    file.SaveAs(fileFullName);
                }
            }

            return new EmptyResult();
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
                    value = group.Sum(f=>f.ValueAsPositive),
                    label = group.Key
                };

                chartDatas.Add(chartData);
            }

            return Json(chartDatas, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartBar(string groupBy = "category")
        {
            var chartBar = new ChartBarModel();
            var dicDataSet = new Dictionary<string, ChartBarModel.DataSet>();

            chartBar.labels = new List<string>();
            chartBar.datasets = new List<ChartBarModel.DataSet>();

            var spents = this.GetSpents().OrderBy(f=>f.Date);
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
                
                foreach (var groupCategory in groupDate.OrderByDescending(f=>f.Sum(s => s.ValueAsPositive)))
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

            return Json(chartBar, JsonRequestBehavior.AllowGet);
        }

        Random randomGen = new Random();
        public string GetRandomColor()
        { 
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[randomGen.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);
            return "RGB(" + 0 + "," + randomColor.G.ToString() + "," + randomColor.B.ToString() + ")";
        }

        public string[] GetAllFiles()
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Not logged");

            var uploadPath = Server.MapPath("/Data");
            var userPath = uploadPath + "/" + userName + "/Spents";

            return Directory.GetFiles(userPath, "*.csv", SearchOption.AllDirectories);
        }

        public List<Transaction> GetSpents()
        {
            var files = GetAllFiles();
            var list = new List<Transaction>();
            if (files.Length > 0)
            {
                foreach(var file in files)
                    list.AddRange(this.GetSpents(file));
            }

            return list;
        }

        public List<Transaction> GetSpents(string fullName)
        {
            var spents = new List<Transaction>();
            using (var sr = new StreamReader(fullName))
            {
                var reader = new CsvReader(sr);
                reader.Parser.Configuration.HasHeaderRecord = false;
                reader.Parser.Configuration.IgnoreBlankLines = true;
                reader.Parser.Configuration.IgnoreHeaderWhiteSpace = true;
                reader.Parser.Configuration.Delimiter = ";";
                reader.Parser.Configuration.IgnoreReadingExceptions = true;

                spents = reader.GetRecords<Transaction>().ToList();
            }

            return spents;
        }
    }
}