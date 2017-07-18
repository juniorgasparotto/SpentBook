using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using SpentBook.Domain;
using Microsoft.AspNetCore.Hosting;
using CsvHelper;
using SpentBook.Web.Models;
using Microsoft.AspNetCore.Identity;
using SpentBook.Web.Services;
using SpentBook.Web.Models.TransactionTable;
using SpentBook.OfxReader;
using System.Transactions;

namespace SpentBook.Web.Views.Import
{
    [Authorize]
    public class ImportController : Controller
    {
        private IUnitOfWork uow;
        private IHostingEnvironment env;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly TransactionTableService transactionTableService;

        public ImportController(IUnitOfWork uow, IHostingEnvironment env, UserManager<ApplicationUser> userManager, TransactionTableService transactionTableService)
        {
            this.uow = uow;
            this.env = env;
            this.userManager = userManager;
            this.transactionTableService = transactionTableService;
        }

        public IActionResult Index()
        {
            return View(Guid.NewGuid());
        }

        [HttpPost]
        public ActionResult Upload(string format, string bank, Guid idImport)
        {
            var userPath = GetUserImportPath(bank, format);

            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);

            var files = Request.Form.Files;

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var fileName = file.FileName;
                var fileFullName = Path.Combine(userPath, fileName);
                if (System.IO.File.Exists(fileFullName))
                    System.IO.File.Delete(fileFullName);

                using (var fileStream = new FileStream(fileFullName, FileMode.Create))
                    file.CopyTo(fileStream);

                List<TransactionImport> transactions;
                switch(bank)
                {
                    case "bradesco":
                        transactions = this.GetFromCsvOfx(bank, format, idImport, fileFullName);
                        break;
                    default:
                        transactions = this.GetFromCsvFile(bank, format, idImport, fileFullName);
                        break;
                }

                using(var scope = new TransactionScope()) {

                    foreach (var t in transactions)
                        uow.TransactionsImports.Insert(t);

                    uow.Save();
                    scope.Complete();
                }
            }

            return new EmptyResult();
        }
        
        [HttpGet]
        public TransactionTableModel GetTable(Guid idImport)
        {
            var transactions = (from t in uow.TransactionsImports.AsQueryable()
                               where t.UserId == Helper.GetLoggedUserId(HttpContext, userManager) &&
                                     t.IdImport == idImport
                                orderby t.Date ascending
                               select transactionTableService.ConvertFromTransactionImport(t)).ToList();

            transactionTableService.ValidateAll(transactions);
            return transactionTableService.CreateTransactionTable(transactions);
        }

        [HttpGet]
        public void Cancel(Guid idImport)
        {
            var transactions = (from t in uow.TransactionsImports.AsQueryable()
                                where t.UserId == Helper.GetLoggedUserId(HttpContext, userManager) &&
                                      t.IdImport == idImport
                                orderby t.Date ascending
                                select t);

            foreach (var t in transactions)
                uow.TransactionsImports.Delete(t.Id);

            uow.Save();
        }

        #region Auxs

        private List<TransactionImport> GetFromCsvFile(string bank, string format, Guid idImport, string fullName)
        {
            var transactions = new List<TransactionImport>();
            using (var sr = new StreamReader(fullName))
            {
                var reader = new CsvReader(sr);
                reader.Parser.Configuration.ThrowOnBadData = false;
                reader.Parser.Configuration.IgnoreBlankLines = true;
                reader.Parser.Configuration.Delimiter = ";";

                var lines = reader.GetRecords<CSVLineModel>().ToList();
                foreach (var line in lines)
                {
                    var transaction = new TransactionImport()
                    {
                        Id = Guid.NewGuid(),
                        IdImport = idImport,
                        UserId = Helper.GetLoggedUserId(HttpContext, userManager),
                        Date = line.Date,
                        Category = line.Category,
                        SubCategory = line.SubCategory,
                        Name = line.Name,
                        Value = line.Value,
                        BankName = !string.IsNullOrWhiteSpace(line.BankName) ? line.BankName.ToLower() : bank,
                        FormatFile = format
                    };

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        private List<TransactionImport> GetFromCsvOfx(string bank, string format, Guid idImport, string fullName)
        {
            var transactions = new List<TransactionImport>();
            using (var stream = new FileStream(fullName, FileMode.Open))
            {
                var parser = new OFXDocumentParser();
                var ofxDocument = parser.Import(stream);

                foreach(var t in ofxDocument.Transactions)
                {
                    var transaction = new TransactionImport()
                    {
                        Id = Guid.NewGuid(),
                        IdImport = idImport,
                        IdExternal = t.CheckNum,
                        UserId = Helper.GetLoggedUserId(HttpContext, userManager),
                        Date = t.Date,
                        Category = null,
                        SubCategory = null,
                        Name = t.Memo,
                        Value = t.Amount,
                        BankName = bank,
                        FormatFile = format
                    };

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        private string GetUserImportPath(string bank, string format)
        {
            var userName = Helper.GetLoggedUserName(HttpContext);
            var webRoot = env.WebRootPath;
            var userPath = Path.Combine(webRoot, "Data", userName, "Transactions", bank, format);
            return userPath;
        }

        #endregion
    }
}
