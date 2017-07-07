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
        public TransactionEditableGridModel GetTransactionEditableGridByImport()
        {
            var transactions = (from t in uow.TransactionsImports.AsQueryable()
                               where t.UserId == Helper.GetLoggedUserId(HttpContext, userManager)
                               select GetTransactionEditableByImport(t)).ToList();

            ValidateTransationEditableModel(transactions);
            return GetTransactionEditableGrid(transactions);
        }


        [HttpPost]
        public IActionResult Save([FromBody]TransactionEditableModel[] transactions)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors, (y, z) => z.Exception.Message);

                return BadRequest(errors);
            }

            ValidateTransationEditableModel(transactions.ToList());
            var hasError = transactions.Any(f => f.Status == TransactionEditableModel.StatusCode.Error);

            if (hasError)
                return BadRequest(new { message = "Existem erros que precisam ser corrigidos.", transactions = transactions });

            return new EmptyResult();
        }
        
        #region Auxs

        private TransactionEditableModel GetTransactionEditableByImport(TransactionImport transactionImport)
        {
            var transactionEditable = new TransactionEditableModel();
            transactionEditable.Id = null;
            transactionEditable.IdImport = transactionImport.Id;
            transactionEditable.UserId = transactionImport.UserId;
            transactionEditable.BankName = transactionImport.BankName;
            transactionEditable.Name = transactionImport.Name;
            transactionEditable.Date = transactionImport.Date;
            transactionEditable.Value = transactionImport.Value;
            transactionEditable.Category = transactionImport.Category;
            transactionEditable.SubCategory = transactionImport.SubCategory;
            
            return transactionEditable;
        }

        private void ValidateTransationEditableModel(List<TransactionEditableModel> transactions)
        {
            // clean validations
            foreach(var t in transactions)
            {
                t.Status = TransactionEditableModel.StatusCode.None;
                t.StatusMessage?.Clear();
                t.StatusMessage = t.StatusMessage ?? new List<string>();
            }

            var groupsDuplicate = from t in transactions
                                  group t by new { t.Name, t.Date, t.Value } into grp
                                  where grp.Count() > 1
                                  select grp.ToList();

            foreach (var group in groupsDuplicate)
            {
                var lines = group.Select(f => new { item = f, line = transactions.IndexOf(f) + 1});

                foreach (var item in group)
                {
                    var linesStr = string.Join(',', lines.Where(f => f.item != item).Select(f => f.line.ToString()));

                    item.Status = TransactionEditableModel.StatusCode.Error;
                    item.StatusMessage.Add($@"Você está tentando inserir esse item mais de uma vez. Os conflitos ocorreram com as linhas ""{linesStr}""");
                }
            }

            foreach (var transactionEditable in transactions)
            {
                // não valida, pois já existe erro de duplicidade dentro da propria lista
                if (transactionEditable.Status == TransactionEditableModel.StatusCode.Error)
                    continue;

                var messages = new SortedDictionary<string, string>();
                if (string.IsNullOrWhiteSpace(transactionEditable.Name))
                    messages.Add("name", "O campo 'Nome' não pode estar vazio");
                if (transactionEditable.Date == DateTime.MinValue)
                    messages.Add("date", "O campo 'Data' não pode estar vazio");
                if (transactionEditable.Value == 0)
                    messages.Add("value", "O campo 'Valor' não pode estar vazio");

                var exists = (from tExists in uow.Transactions.AsQueryable()
                              where
                                  tExists.UserId == transactionEditable.UserId &&
                                  tExists.BankName == transactionEditable.BankName &&
                                  tExists.Name == transactionEditable.Name &&
                                  tExists.Date == transactionEditable.Date &&
                                  tExists.Value == transactionEditable.Value
                              select tExists).FirstOrDefault();

                if (exists != null)
                    messages.Add("duplicate", $@"Essa transação já foi inserida, veja <a href=""/Transaction/{exists.Id}"">aqui</a>");

                if (messages.Count > 0)
                {
                    transactionEditable.Status = TransactionEditableModel.StatusCode.Error;
                }
                else
                {
                    var hasCategory = !string.IsNullOrWhiteSpace(transactionEditable.Category);
                    var hasSubCategory = !string.IsNullOrWhiteSpace(transactionEditable.SubCategory);

                    if (!hasCategory)
                        messages.Add("category", "O campo 'Categoria' esta vazio");

                    if (!hasSubCategory)
                        messages.Add("sub-category", "O campo 'Sub-Categoria' esta vazio");

                    if (messages.Count > 0)
                    {
                        transactionEditable.Status = TransactionEditableModel.StatusCode.Warning;

                        var sameName = (from tExists in uow.Transactions.AsQueryable()
                                        where
                                            tExists.Name == transactionEditable.Name
                                        select tExists).LastOrDefault();

                        if (sameName != null)
                        {
                            int automaticFill = 0;
                            if (!hasCategory && !string.IsNullOrWhiteSpace(sameName.Category))
                            {
                                automaticFill++;
                                messages.Add("auto-category", "O campo 'Categoria' foi preenchido automáticamente");
                                transactionEditable.Category = sameName.Category;

                                if (messages.ContainsKey("category"))
                                    messages.Remove("category");
                            }

                            if (!hasSubCategory && !string.IsNullOrWhiteSpace(sameName.SubCategory))
                            {
                                automaticFill++;
                                messages.Add("auto-sub-category", "O campo 'Sub-Categoria' foi preenchido automáticamente");
                                transactionEditable.SubCategory = sameName.SubCategory;

                                if (messages.ContainsKey("sub-category"))
                                    messages.Remove("sub-category");
                            }

                            if (automaticFill >= 1)
                                messages.Add("auto-details", $@"Clique <a href=""/Transaction/{sameName.Id}"">aqui</a> para ver a transação que foi referência para auto-preenchimento.");

                            // só muda para auto resolved quando os dois forem preenchidos
                            if (automaticFill == 2)
                                transactionEditable.Status = TransactionEditableModel.StatusCode.AutomaticResolved;
                        }
                    }
                }

                transactionEditable.StatusMessage = messages.Values.ToList();
            }
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

        private TransactionEditableGridModel GetTransactionEditableGrid(IEnumerable<TransactionEditableModel> transactions)
        {
            return new TransactionEditableGridModel
            {
                Transactions = transactions,
                Categories = uow.Transactions.AsQueryable().GroupBy(f => f.Category).Select(f => f.Key),
                SubCategories = uow.Transactions.AsQueryable().GroupBy(f => f.SubCategory).Select(f => f.Key)
            };
        }

        private class CSVLine
        {
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
        }

        public class TransactionEditableGridModel
        {
            public IEnumerable<TransactionEditableModel> Transactions { get; set; }
            public IEnumerable<string> Categories { get; set; }
            public IEnumerable<string> SubCategories { get; set; }
        }

        public class TransactionEditableModel
        {
            public enum StatusCode
            {
                None = 0,
                Warning = 1,
                AutomaticResolved = 2,
                Error = 3
            }

            public Guid? Id { get; set; }
            public Guid? IdImport { get; set; }
            public Guid? UserId { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public decimal Value { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string BankName { get; set; }
            public StatusCode? Status { get; set; }
            public List<string> StatusMessage { get; set; }
        }

        #endregion
    }
}
