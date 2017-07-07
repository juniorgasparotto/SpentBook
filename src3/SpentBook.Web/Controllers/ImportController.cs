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
using System.Transactions;

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
        public GetResponseModel GetByImport()
        {
            var transactions = (from t in uow.TransactionsImports.AsQueryable()
                               where t.UserId == Helper.GetLoggedUserId(HttpContext, userManager)
                               orderby t.Date ascending
                               select GetTransactionEditableByImport(t)).ToList();

            ValidateTransationsEditable(transactions);
            return GetGetResponseModel(transactions);
        }

        [HttpGet]
        public GetResponseModel Get()
        {
            var transactions = (from t in uow.Transactions.AsQueryable()
                                where t.UserId == Helper.GetLoggedUserId(HttpContext, userManager)
                                orderby t.Date ascending
                                select GetTransactionEditableByTransaction(t)).ToList();

            ValidateTransationsEditable(transactions, false, false, false);
            return GetGetResponseModel(transactions);
        }
        
        [HttpPost]
        public IActionResult Save([FromBody]SaveRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors, (y, z) => z.Exception.Message);

                return BadRequest(errors);
            }

            ValidateTransationsEditable(model.Transactions);
            var hasError = model.Transactions.Any(f => f.Status == TransactionEditableModel.StatusCode.Error);

            if (hasError)
                return new JsonResult(new { message = "Existem erros que precisam ser corrigidos.", transactions = model.Transactions });

            using (var transaction = new TransactionScope())
            {
                // remove as transações que existem na lista de IDs iniciais e não estão na lista enviada no model
                // isso caracteriza uma deleção de linha
                var deleteds = (
                    from initialId in model.InitialIds
                    from tInModel in model.Transactions.Where(f => f.Id == initialId).DefaultIfEmpty()
                    where tInModel == null
                    select initialId
                );

                DeleteTransactionsByIds(deleteds);
                InsertTransactionsByTransactionEditable(model.Transactions);
                transaction.Complete();
            }

            return new JsonResult(new { message = "OK" });
        }

        private void DeleteTransactionsByIds(IEnumerable<Guid> deleteds)
        {
            foreach (var del in deleteds)
                uow.Transactions.Delete(del);
            uow.Save();
        }

        private void InsertTransactionsByTransactionEditable(List<TransactionEditableModel> transactions)
        {
            foreach (var model in transactions)
            {
                Domain.Transaction transaction = null;
                var exists = true;

                if (model.Id != null)
                    transaction = uow.Transactions.AsQueryable().Where(f => f.Id == model.Id).FirstOrDefault();

                if (transaction == null)
                {
                    transaction = new Domain.Transaction();
                    exists = false;
                }

                transaction.Id = model.Id ?? Guid.NewGuid();
                transaction.IdImport = model.IdImport;
                transaction.UserId = model.UserId ?? Helper.GetLoggedUserId(HttpContext, userManager);
                transaction.BankName = model.BankName;
                transaction.Name = model.Name;
                transaction.Date = model.Date;
                transaction.Value = model.Value;
                transaction.Category = model.Category;
                transaction.SubCategory = model.SubCategory;
                transaction.CreateDate = transaction.CreateDate != DateTime.MinValue ? transaction.CreateDate : DateTime.Now;
                transaction.LastUpdateDate = DateTime.Now;

                if (exists)
                    uow.Transactions.Update(transaction);
                else
                    uow.Transactions.Insert(transaction);
            }

            uow.Save();
        }

        #region Auxs

        private TransactionEditableModel GetTransactionEditableByImport(TransactionImport transactionImport)
        {
            var transactionEditable = new TransactionEditableModel
            {
                Id = null,
                IdImport = transactionImport.Id,
                UserId = transactionImport.UserId,
                BankName = transactionImport.BankName,
                Name = transactionImport.Name,
                Date = transactionImport.Date,
                Value = transactionImport.Value,
                Category = transactionImport.Category,
                SubCategory = transactionImport.SubCategory
            };

            return transactionEditable;
        }

        private TransactionEditableModel GetTransactionEditableByTransaction(Domain.Transaction transaction)
        {
            var transactionEditable = new TransactionEditableModel
            {
                Id = transaction.Id,
                IdImport = transaction.IdImport,
                UserId = transaction.UserId,
                BankName = transaction.BankName,
                Name = transaction.Name,
                Date = transaction.Date,
                Value = transaction.Value,
                Category = transaction.Category,
                SubCategory = transaction.SubCategory
            };

            return transactionEditable;
        }

        private void ValidateTransationsEditable(List<TransactionEditableModel> transactions, bool validateDuplicateInEditable = true, bool validadeDuplicateInDatabase = true, bool autoFillCategorySubCategory = true)
        {
            // clean validations
            foreach(var t in transactions)
            {
                t.Status = TransactionEditableModel.StatusCode.None;
                t.StatusMessage?.Clear();
                t.StatusMessage = t.StatusMessage ?? new List<string>();
            }

            if (validateDuplicateInEditable)
            {
                var groupsDuplicate = from t in transactions
                                      group t by new { t.Name, t.Date, t.Value } into grp
                                      where grp.Count() > 1
                                      select grp.ToList();

                foreach (var group in groupsDuplicate)
                {
                    var lines = group.Select(f => new { item = f, line = transactions.IndexOf(f) + 1 });

                    foreach (var item in group)
                    {
                        var linesStr = string.Join(',', lines.Where(f => f.item != item).Select(f => f.line.ToString()));

                        item.Status = TransactionEditableModel.StatusCode.Error;
                        item.StatusMessage.Add($@"Você está tentando inserir esse item mais de uma vez. Os conflitos ocorreram com as linhas ""{linesStr}""");
                    }
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

                if (validadeDuplicateInDatabase)
                {
                    var exists = (from tExists in uow.Transactions.AsQueryable()
                                  where
                                      tExists.UserId == transactionEditable.UserId &&
                                      tExists.BankName == transactionEditable.BankName &&
                                      tExists.Name == transactionEditable.Name &&
                                      tExists.Date == transactionEditable.Date &&
                                      tExists.Value == transactionEditable.Value
                                  select tExists).FirstOrDefault();

                    if (exists != null && exists.Id != transactionEditable.Id)
                        messages.Add("duplicate", $@"Essa transação já foi inserida, veja <a href=""/Transaction/{exists.Id}"">aqui</a>");
                }

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

                        if (autoFillCategorySubCategory)
                        { 
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

        private GetResponseModel GetGetResponseModel(List<TransactionEditableModel> transactions)
        {
            return new GetResponseModel
            {
                InitialIds = transactions.Where(f => f.Id != null).Select(f => f.Id),
                Transactions = transactions,
                Categories = uow.Transactions.AsQueryable().GroupBy(f => f.Category).Select(f => f.Key).ToList(),
                SubCategories = uow.Transactions.AsQueryable().GroupBy(f => f.SubCategory).Select(f => f.Key).ToList()
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

        public class GetResponseModel
        {
            public List<TransactionEditableModel> Transactions { get; set; }
            public List<string> Categories { get; set; }
            public List<string> SubCategories { get; set; }
            public IEnumerable<Guid?> InitialIds { get; internal set; }
        }

        public class SaveRequestModel
        {
            public List<Guid> InitialIds { get; set; }
            public List<TransactionEditableModel> Transactions { get; set; }
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
