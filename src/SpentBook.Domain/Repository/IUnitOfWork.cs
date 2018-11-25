namespace SpentBook.Domain
{
    public interface IUnitOfWork
    {
        IRepository<Dashboard> Dashboards { get; }
        IRepository<Transaction> Transactions { get; }
        IRepository<TransactionImport> TransactionsImports { get; }
        IRepository<Bank> Banks { get; }
        void Save();
    }
}
