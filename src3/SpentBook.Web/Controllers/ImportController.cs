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
using System.Text;

namespace SpentBook.Web.Views.Import
{
    [Authorize]
    public class ImportController : Controller
    {
        private IUnitOfWork uow;
        private IHostingEnvironment env;
        private readonly UserManager<ApplicationUser> userManager;

        public ImportController(IUnitOfWork uow, IHostingEnvironment env, UserManager<ApplicationUser> userManager)
        {
            this.uow = uow;
            this.env = env;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(string format, string bank)
        {
            var userPath = GetTransactionPath(bank, format);

            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);
            var files = Request.Form.Files;

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

                    switch(format)
                    {
                        case "bradesco":
                            break;
                        default:
                            var transactions = this.GetTransactionsImportsCsvFromFile(bank, format, fileFullName);
                            foreach (var t in transactions)
                                uow.TransactionsImports.Insert(t);
                            uow.Save();
                            break;
                    }
                }
            }

            return new EmptyResult();
        }
        
        [HttpGet]
        public TransactionEditableGrid GetTransactionsEditable()
        {
            var transactions = from t in uow.TransactionsImports.AsQueryable()
                               where t.UserId == Helper.GetLoggedUserId(HttpContext, userManager)
                               select GetTransactionImportWithErrorIfExists(t);

            return new TransactionEditableGrid
            {
                Transactions = transactions,
                Categories = uow.Transactions.AsQueryable().GroupBy(f => f.Category).Select(f => f.Key),
                SubCategories = uow.Transactions.AsQueryable().GroupBy(f => f.SubCategory).Select(f => f.Key)
            };
        }
        
        private TransactionImport GetTransactionImportWithErrorIfExists(TransactionImport transactionImport)
        {
            var messages = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(transactionImport.Name))
                messages.Add("name", "O campo 'Nome' não pode estar vazio");
            if (transactionImport.Date == DateTime.MinValue)
                messages.Add("date", "O campo 'Data' não pode estar vazio");
            if (transactionImport.Value == 0)
                messages.Add("value", "O campo 'Valor' não pode estar vazio");
            
            var exists = (from tExists in uow.Transactions.AsQueryable()
                          where
                              tExists.UserId == transactionImport.UserId &&
                              tExists.BankName == transactionImport.BankName &&
                              tExists.Name == transactionImport.Name &&
                              tExists.Date == transactionImport.Date &&
                              tExists.Value == transactionImport.Value
                          select tExists).FirstOrDefault();

            if (exists != null)
                messages.Add("duplicate", $@"Essa transação já foi inserida, veja <a href=""/Transaction/{exists.Id}"">aqui</a>");

            if (messages.Count > 0)
            {
                transactionImport.Status = TransactionImport.StatusCode.Error;
            }
            else
            {
                var hasCategory = !string.IsNullOrWhiteSpace(transactionImport.Category);
                var hasSubCategory = !string.IsNullOrWhiteSpace(transactionImport.SubCategory);

                if (!hasCategory)
                    messages.Add("category", "O campo 'Categoria' esta vazio");

                if (!hasSubCategory)
                    messages.Add("sub-category", "O campo 'Sub-Categoria' esta vazio");

                if (messages.Count > 0)
                {
                    transactionImport.Status = TransactionImport.StatusCode.Warning;

                    var sameName = (from tExists in uow.Transactions.AsQueryable()
                                    where
                                        tExists.Name == transactionImport.Name
                                    select tExists).LastOrDefault();

                    if (sameName != null)
                    {
                        int automaticFill = 0;
                        if (!hasCategory && !string.IsNullOrWhiteSpace(sameName.Category))
                        {
                            automaticFill++;
                            messages.Add("auto-category", "O campo 'Categoria' foi preenchido automáticamente");
                            transactionImport.Category = sameName.Category;

                            if (messages.ContainsKey("category"))
                                messages.Remove("category");
                        }

                        if (!hasSubCategory && !string.IsNullOrWhiteSpace(sameName.SubCategory))
                        {
                            automaticFill++;
                            messages.Add("auto-sub-category", "O campo 'Sub-Categoria' foi preenchido automáticamente");
                            transactionImport.SubCategory = sameName.SubCategory;

                            if (messages.ContainsKey("sub-category"))
                                messages.Remove("sub-category");
                        }

                        if (automaticFill >= 1)
                            messages.Add("auto-details", $@"Clique <a href=""/Transaction/{sameName.Id}"">aqui</a> para ver a transação que foi referência para auto-preenchimento.");

                        // só muda para auto resolved quando os dois forem preenchidos
                        if (automaticFill == 2)
                            transactionImport.Status = TransactionImport.StatusCode.AutomaticResolved;
                    }
                }
            }

            transactionImport.StatusMessage = messages.Values.ToList();
            return transactionImport;
        }

        [HttpPost]
        public void Save(IEnumerable<TransactionImport> transactions)
        {

        }

        private string GetTransactionPath(string bank, string format)
        {
            var userName = Helper.GetLoggedUserName(HttpContext);
            var webRoot = env.WebRootPath;
            var userPath = Path.Combine(webRoot, "Data", userName, "Transactions", bank, format);
            return userPath;
        }

        private List<TransactionImport> GetTransactionsImportsCsvFromFile(string bank, string format, string fullName)
        {
            var transactions = new List<TransactionImport>();
            using (var sr = new StreamReader(fullName))
            {
                var reader = new CsvReader(sr);
                reader.Parser.Configuration.ThrowOnBadData = false;
                reader.Parser.Configuration.IgnoreBlankLines = true;
                reader.Parser.Configuration.Delimiter = ";";

                var lines = reader.GetRecords<CSVLine>().ToList();
                foreach (var line in lines)
                {
                    var transaction = new TransactionImport()
                    {
                        Id = Guid.NewGuid(),
                        UserId = Helper.GetLoggedUserId(HttpContext, userManager),
                        Date = line.Date,
                        Category = line.Category,
                        SubCategory = line.SubCategory,
                        Name = line.Name,
                        Value = line.Value,
                        BankName = bank,
                        FormatFile = format
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

        public class TransactionEditableGrid
        {
            public IEnumerable<TransactionImport> Transactions { get; set; }
            public IEnumerable<string> Categories { get; set; }
            public IEnumerable<string> SubCategories { get; set; }
        }
    }
}
