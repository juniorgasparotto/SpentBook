using System;
using System.Collections.Generic;
using System.Linq;
using SpentBook.Domain;
using SpentBook.Web.Models;
using SpentBook.Domain.Services;
using SpentBook.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using SpentBook.Web.Helpers;
using SpentBook.Web.Services;
using Microsoft.AspNetCore.Identity;
using System.Transactions;
using SpentBook.Web.Models.TransactionTable;

namespace SpentBook.Web.Controllers
{
    [JsonOutputWhenModelInvalid]
    [JsonOutputWhenGenericException]
    public class TransactionController : Controller
    {
        private IUnitOfWork uow;
        private readonly TransactionTableService transactionTableService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly TransactionService transactionService;

        public TransactionController(IUnitOfWork uow, 
            UserManager<ApplicationUser> userManager, 
            [FromServices] TransactionTableService transactionTableService,
            [FromServices] TransactionService transactionService
        )
        {
            this.uow = uow;
            this.userManager = userManager;
            this.transactionService = transactionService;
            this.transactionTableService = transactionTableService;
        }

        [HttpPost]
        [HttpGet]
        public ActionResult Index(PageTransactionModel model)
        {
            if (model.Filter == null)
            {
                model.Filter = new TransactionFilterModel
                {
                    DateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01),
                    DateEnd = DateTime.Now.Date,
                    TransactionType = TransactionType.None,
                    OrderBy = TransactionOrder.Date, 
                    OrderByClassification = OrderClassification.Desc,
                    ValueEnd = null,
                    ValueStart = null
                };
            }

            if (!ModelState.IsValid)
                model.Errors = this.ModelState.Values.ToList();

            return View(model);
        }

        #region Multi-edition

        [HttpPost]
        public TransactionTableModel GetTable(TransactionFilterModel filter)
        {
            var transactions = (from t in Filter(filter)
                                select transactionTableService.ConvertFromTransaction(t)).ToList();

            transactionTableService.ValidateAll(transactions, false, false, false, false);
            return transactionTableService.CreateTransactionTable(transactions);
        }

        [HttpPost]
        public IActionResult SaveTable([FromBody]TransactionTableSaveModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors, (y, z) => z.Exception.Message);

                return BadRequest(errors);
            }
            
            var ok = transactionTableService.SaveTable(model, Helper.GetLoggedUserId(HttpContext, userManager));

            if (!ok)
                return new JsonResult(new { message = "Existem erros que precisam ser corrigidos.", transactions = model.Transactions });

            return new JsonResult(new { message = "OK" });
        }

        #endregion

        #region Auxs

        private IEnumerable<Domain.Transaction> Filter(TransactionFilterModel filterIn)
        {
            var filter = new TransactionFilter
            {
                DateEnd = filterIn.DateEnd,
                DateStart = filterIn.DateStart,
                OrderBy = TransactionOrder.Date, //model.Filter.OrderBy;
                OrderByClassification = OrderClassification.Desc, //model.Filter.OrderByClassification;
                ValueEnd = filterIn.ValueEnd,
                ValueStart = filterIn.ValueStart,
                TransactionType = filterIn.TransactionType,
                IdUser = Helper.GetLoggedUserId(HttpContext, userManager)
            };

            if (!string.IsNullOrWhiteSpace(filterIn.Categories))
            {
                filter.Categories = new List<string>();
                filter.Categories = filterIn.Categories.Split(',').Select(s => s.Trim()).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filterIn.SubCategories))
            {
                filter.SubCategories = new List<string>();
                filter.SubCategories = filterIn.SubCategories.Split(',').Select(s => s.Trim()).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filterIn.TransactionNames))
            {
                filter.Names = new List<string>();
                filter.Names = filterIn.TransactionNames.Split(',').Select(s => s.Trim()).ToList();
            }

            return transactionService.GetTransactionsFiltrated(filter);
        }

        #endregion


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

        //public ActionResult FilesList()
        //{
        //    var files = this.GetAllFiles() ?? new string[0];
        //    return PartialView(files.Select(f => f.Split('\\').LastOrDefault()).ToList());
        //}

        //public string[] GetAllFiles()
        //{
        //    var userName = "admin"; // User.Identity.Name;
        //    if (string.IsNullOrWhiteSpace(userName))
        //        throw new Exception("Not logged");

        //    //var uploadPath = System.Web.HttpContext.Current.Server.MapPath("/Data");
        //    var uploadPath = "/Data";
        //    var userPath = uploadPath + "/" + userName + "/Spents";
        //    if (Directory.Exists(userPath))
        //        return Directory.GetFiles(userPath, "*.csv", SearchOption.AllDirectories);

        //    return null;
        //}
    }
}