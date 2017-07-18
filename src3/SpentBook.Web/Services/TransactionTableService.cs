using System;
using System.Collections.Generic;
using System.Linq;
using SpentBook.Domain;
using SpentBook.Web.Models.TransactionTable;
using System.Transactions;

namespace SpentBook.Web.Services
{
    public class TransactionTableService
    {
        private IUnitOfWork uow;

        public TransactionTableService(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        public TransactionTableModel CreateTransactionTable(List<TransactionTableLineModel> transactions)
        {
            return new TransactionTableModel
            {
                InitialIds = transactions.Where(f => f.Id != null).Select(f => f.Id),
                Transactions = transactions,
                Categories = GetCategories(),
                SubCategories = GetSubCategories(),
                Banks = GetBanks().Select(f => f.Name)
            };
        }

        public TransactionTableLineModel ConvertFromTransactionImport(TransactionImport transactionImport)
        {
            var transactionEditable = new TransactionTableLineModel
            {
                Id = null,
                IdImport = transactionImport.Id,
                IdUser = transactionImport.UserId,
                IdExternal = transactionImport.IdExternal,
                BankName = transactionImport.BankName,
                Name = transactionImport.Name,
                Date = transactionImport.Date,
                Value = transactionImport.Value,
                Category = transactionImport.Category,
                SubCategory = transactionImport.SubCategory
            };

            return transactionEditable;
        }

        public TransactionTableLineModel ConvertFromTransaction(Domain.Transaction transaction)
        {
            var transactionEditable = new TransactionTableLineModel
            {
                Id = transaction.Id,
                IdImport = transaction.IdImport,
                IdUser = transaction.IdUser,
                IdExternal = transaction.IdExternal,
                BankName = transaction.Bank.Name,
                Name = transaction.Name,
                Date = transaction.Date,
                Value = transaction.Value,
                Category = transaction.Category,
                SubCategory = transaction.SubCategory
            };

            return transactionEditable;
        }

        public void ValidateAll(List<TransactionTableLineModel> transactions, bool validateDuplicateInEditable = true, bool validadeDuplicateInDatabase = true, bool autoFillCategorySubCategory = true, bool validateBanks = true)
        {
            // clean validations
            foreach(var t in transactions)
            {
                t.Status = TransactionTableLineModel.StatusCode.None;
                t.StatusMessage?.Clear();
                t.StatusMessage = t.StatusMessage ?? new List<string>();
                t.Bank = GetBankByName(t.BankName);
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
                        var linesStr = string.Join(", ", lines.Where(f => f.item != item).Select(f => f.line.ToString()));

                        item.Status = TransactionTableLineModel.StatusCode.Error;
                        item.StatusMessage.Add($@"Você está tentando inserir esse item mais de uma vez. Os conflitos ocorreram com as linhas ""{linesStr}""");
                    }
                }
            }

            foreach (var transactionEditable in transactions)
            {
                // não valida, pois já existe erro de duplicidade dentro da propria lista
                if (transactionEditable.Status == TransactionTableLineModel.StatusCode.Error)
                    continue;

                var messages = new SortedDictionary<string, string>();

                if (validateBanks)
                {
                    if (GetBankByName(transactionEditable.BankName) == null)
                        messages.Add("bank", "O 'Banco' informado não está cadastrado");
                }

                if (string.IsNullOrWhiteSpace(transactionEditable.Name))
                    messages.Add("name", "O campo 'Nome' não pode estar vazio");
                if (string.IsNullOrWhiteSpace(transactionEditable.IdExternal))
                    messages.Add("id-external", "O campo 'Número do documento' não pode estar vazio");
                if (string.IsNullOrWhiteSpace(transactionEditable.BankName))
                    messages.Add("bank-name", "O campo 'Banco' não pode estar vazio");
                if (transactionEditable.Date == null || transactionEditable.Date == DateTime.MinValue)
                    messages.Add("date", "O campo 'Data' não pode estar vazio");
                if (transactionEditable.Value == null || transactionEditable.Value == 0)
                    messages.Add("value", "O campo 'Valor' não pode estar vazio");

                if (validadeDuplicateInDatabase)
                {
                    var exists = (from tExists in uow.Transactions.AsQueryable()
                                  where
                                      tExists.IdUser == transactionEditable.IdUser &&
                                      (transactionEditable.Bank == null ? false : tExists.Bank.Id == transactionEditable.Bank.Id) &&
                                      tExists.Name == transactionEditable.Name &&
                                      tExists.Date == transactionEditable.Date &&
                                      tExists.Value == transactionEditable.Value
                                  select tExists).FirstOrDefault();

                    if (exists != null && exists.Id != transactionEditable.Id)
                        messages.Add("duplicate", $@"Essa transação já foi inserida, veja <a href=""/Transaction/{exists.Id}"">aqui</a>");
                }

                if (messages.Count > 0)
                {
                    transactionEditable.Status = TransactionTableLineModel.StatusCode.Error;
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
                        transactionEditable.Status = TransactionTableLineModel.StatusCode.Warning;

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
                                    transactionEditable.Status = TransactionTableLineModel.StatusCode.AutomaticResolved;
                            }
                        }
                    }
                }

                transactionEditable.StatusMessage = messages.Values.ToList();
            }
        }

        public bool SaveTable(TransactionTableSaveModel table, Guid currentUserId)
        {
            // não faz o auto-fill
            ValidateAll(table.Transactions, true, true, false, true);
            var hasError = table.Transactions.Any(f => f.Status == TransactionTableLineModel.StatusCode.Error);

            if (hasError)
                return false;

            using (var transaction = new TransactionScope())
            {
                // remove as transações que existem na lista de IDs iniciais e não estão na lista enviada no model
                // isso caracteriza uma deleção de linha
                var deleteds = (
                    from initialId in table.InitialIds
                    from tInModel in table.Transactions.Where(f => f.Id == initialId).DefaultIfEmpty()
                    where tInModel == null
                    select initialId
                );

                DeleteByIds(deleteds);
                InsertOrUpdateTable(table.Transactions, currentUserId);

                uow.Save();
                transaction.Complete();

                return true;
            }
        }

        private void DeleteByIds(IEnumerable<Guid> deleteds)
        {
            foreach (var del in deleteds)
                uow.Transactions.Delete(del);
        }

        private void InsertOrUpdateTable(IEnumerable<TransactionTableLineModel> transactions, Guid currentUserId)
        {
            var banks = GetBanks();
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

                // se existir a transaction entao mantem o id, se não verifica se o model ja tem id, se tiver, mantem o do model
                // se nao tiver cria um novo.
                transaction.Id = transaction.Id != Guid.Empty ? transaction.Id : (model.Id ?? Guid.NewGuid());
                transaction.IdImport = model.IdImport;
                transaction.IdUser = model.IdUser ?? currentUserId; 
                transaction.IdExternal = transaction.IdExternal ?? model.IdExternal;
                transaction.Bank = GetBankByName(model.BankName);
                transaction.Name = model.Name;
                transaction.Date = model.Date.Value;
                transaction.Value = model.Value.Value;
                transaction.Category = model.Category;
                transaction.SubCategory = model.SubCategory;
                transaction.CreateDate = transaction.CreateDate != DateTime.MinValue ? transaction.CreateDate : DateTime.Now;
                transaction.LastUpdateDate = DateTime.Now;

                if (exists)
                    uow.Transactions.Update(transaction);
                else
                    uow.Transactions.Insert(transaction);
            }
        }

        private Bank GetBankByName(string name)
        {
            var banks = GetBanks();
            return banks.FirstOrDefault(f => f.Name.ToLower().Trim() == name?.ToLower().Trim());
        }

        private IEnumerable<string> GetCategories()
        {
            return uow.Transactions.AsQueryable().GroupBy(f => f.Category).Select(f => f.Key);
        }

        private IEnumerable<string> GetSubCategories()
        {
            return uow.Transactions.AsQueryable().GroupBy(f => f.SubCategory).Select(f => f.Key);
        }

        private IEnumerable<Bank> GetBanks()
        {
            return uow.Banks.AsQueryable();
        }
    }
}
