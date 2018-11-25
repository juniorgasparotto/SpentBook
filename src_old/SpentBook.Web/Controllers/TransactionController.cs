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
using SpentBook.Web.Models;
using SpentBook.Domain.Services;
using SpentBook.Domain.Imports;
using SpentBook.Web.Filters;

namespace SpentBook.Web.Controllers
{
    [JsonOutputWhenModelInvalid]
    [JsonOutputWhenGenericException]
    public class TransactionController : Controller
    {
        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get)]
        public ActionResult Index(PageTransactionModel model)
        {
            ViewBag.Message = "Minhas transações";
            
            if (ModelState.IsValid)
            {
                if (model.Filter == null)
                {
                    model.Filter = new TransactionFilterModel();
                    model.Filter.DateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                    model.Filter.DateEnd = DateTime.Now.Date;
                    model.Filter.TransactionType = TransactionType.None;
                    model.Filter.OrderBy = TransactionOrder.Date; //model.Filter.OrderBy;
                    model.Filter.OrderByClassification = OrderClassification.Desc; //model.Filter.OrderByClassification;
                    model.Filter.ValueEnd = null;
                    model.Filter.ValueStart = null;
                }

                var filter = new TransactionFilter();
                filter.DateEnd = model.Filter.DateEnd;
                filter.DateStart = model.Filter.DateStart;
                filter.OrderBy = TransactionOrder.Date; //model.Filter.OrderBy;
                filter.OrderByClassification = OrderClassification.Desc; //model.Filter.OrderByClassification;
                filter.ValueEnd = model.Filter.ValueEnd;
                filter.ValueStart = model.Filter.ValueStart;
                filter.TransactionType = model.Filter.TransactionType;

                if (!string.IsNullOrWhiteSpace(model.Filter.Categories))
                {
                    filter.Categories = new List<string>();
                    filter.Categories = model.Filter.Categories.Split(',').Select(s => s.Trim()).ToList();
                }

                if (!string.IsNullOrWhiteSpace(model.Filter.SubCategories))
                {
                    filter.SubCategories = new List<string>();
                    filter.SubCategories = model.Filter.SubCategories.Split(',').Select(s => s.Trim()).ToList();
                }

                if (!string.IsNullOrWhiteSpace(model.Filter.TransactionNames))
                {
                    filter.Names = new List<string>();
                    filter.Names = model.Filter.TransactionNames.Split(',').Select(s => s.Trim()).ToList();
                }

                var transactionService = new TransactionService(new TransactionCSVUnitOfWork());
                model.Transactions = (from transaction in transactionService.GetTransactionsFiltrated(filter)
                                     select new TransactionModel
                                     {
                                         Id = transaction.Id,
                                         Name = transaction.Name,
                                         Date = transaction.Date,
                                         Value = transaction.Value,
                                         Category = transaction.Category,
                                         SubCategory = transaction.SubCategory,
                                     }).ToList();
            }
            else
            {
                model.Errors = this.ModelState.Values;
            }
            
            if (Request.IsAjaxRequest())
                return Json(model, JsonRequestBehavior.AllowGet);
            else 
                return View(model);
        }

        //[HttpGet]
        //public ActionResult Get(PageTransactionModel model)
        //{
        //    //var transactionService = new TransactionService(new TransactionCSVUnitOfWork());
        //    //var filter = new TransactionFilter();
        //    //if (model.Filter != null)
        //    //{
        //    //    filter.DateEnd = model.Filter.DateEnd;
        //    //    filter.DateStart = model.Filter.DateStart;
        //    //    filter.OrderBy = TransactionOrder.Date; //model.Filter.OrderBy;
        //    //    filter.OrderByClassification = OrderClassification.Desc; //model.Filter.OrderByClassification;
        //    //    filter.ValueEnd = model.Filter.ValueEnd;
        //    //    filter.ValueStart = model.Filter.ValueStart;
        //    //    filter.TransactionType = model.Filter.TransactionType;

        //    //    if (!string.IsNullOrWhiteSpace(model.Filter.Categories))
        //    //    {
        //    //        filter.Categories = new List<string>();
        //    //        filter.Categories = model.Filter.Categories.Split(',').Select(s => s.Trim()).ToList();
        //    //    }

        //    //    if (!string.IsNullOrWhiteSpace(model.Filter.SubCategories))
        //    //    {
        //    //        filter.SubCategories = new List<string>();
        //    //        filter.SubCategories = model.Filter.SubCategories.Split(',').Select(s => s.Trim()).ToList();
        //    //    }

        //    //    if (!string.IsNullOrWhiteSpace(model.Filter.TransactionNames))
        //    //    {
        //    //        filter.Names = new List<string>();
        //    //        filter.Names = model.Filter.TransactionNames.Split(',').Select(s => s.Trim()).ToList();
        //    //    }
        //    //}

        //    //var transactions = from transaction in transactionService.GetTransactionsFiltrated(filter)
        //    //                   select new TransactionModel {
        //    //                       Id = transaction.Id,
        //    //                       Name = transaction.Name, 
        //    //                       Date = transaction.Date,
        //    //                       Value = transaction.Value,
        //    //                       Category = transaction.Category,
        //    //                       SubCategory = transaction.SubCategory, 
        //    //                   };

        //    //return Json(transactions, JsonRequestBehavior.AllowGet);
        //}
        
        public ActionResult FilesList()
        {
            var files = this.GetAllFiles() ?? new string[0];
            return PartialView(files.Select(f => f.Split('\\').LastOrDefault()).ToList());
        }

        [HttpPost]
        public ActionResult MultipleUpload(IEnumerable<HttpPostedFileBase> files)
        {
            var transactionImport = new TransactionImportDefaultCSV();

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

                    var spents = transactionImport.GetTransactionsFromCSV(fileFullName);
                    var uow = Helper.GetUnitOfWorkByCurrentUser();

                    foreach (var spent in spents)
                        uow.Transactions.Insert(spent);
                }
            }


            return new EmptyResult();
        }

        public string[] GetAllFiles()
        {
            var userName = "admin"; // User.Identity.Name;
            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Not logged");

            var uploadPath = System.Web.HttpContext.Current.Server.MapPath("/Data");
            var userPath = uploadPath + "/" + userName + "/Spents";
            if (Directory.Exists(userPath))
                return Directory.GetFiles(userPath, "*.csv", SearchOption.AllDirectories);

            return null;
        }
    }
}