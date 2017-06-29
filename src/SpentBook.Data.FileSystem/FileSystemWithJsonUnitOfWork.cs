using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Data.FileSystem
{
    public class FileSystemWithJsonUnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, IRepository<IEntity>> repositories; 
   
        public string FileDb { get; private set; }

        public IRepository<Dashboard> Dashboards
        {
            get
            {
                IRepository<Dashboard> repository;
                if (repositories.ContainsKey(typeof(Dashboard)))
                    repository = (IRepository<Dashboard>)repositories[typeof(Dashboard)];
                else
                    repository = new FileSystemWithJsonRepository<Dashboard>(this.FileDb, p => p.Dashboards);

                return repository;
            }
        }

        public IRepository<Transaction> Transactions
        {
            get
            {
                IRepository<Transaction> repository;
                if (repositories.ContainsKey(typeof(Transaction)))
                    repository = (IRepository<Transaction>)repositories[typeof(Transaction)];
                else
                    repository = new FileSystemWithJsonRepository<Transaction>(this.FileDb, p => p.Transactions);

                return repository;
            }
        }

        //public IRepository<Panel> Panels
        //{
        //    get
        //    {
        //        IRepository<Panel> repository;
        //        if (repositories.ContainsKey(typeof(Panel)))
        //            repository = (IRepository<Panel>)repositories[typeof(Panel)];
        //        else
        //            repository = new FileSystemWithJsonRepository<Panel>(this.FileDb);

        //        return repository;
        //    }
        //}

        
        //public IRepository<TransactionFilter> TransactionFilters
        //{
        //    get
        //    {
        //        IRepository<TransactionFilter> repository;
        //        if (repositories.ContainsKey(typeof(TransactionFilter)))
        //            repository = (IRepository<TransactionFilter>)repositories[typeof(TransactionFilter)];
        //        else
        //            repository = new FileSystemWithJsonRepository<TransactionFilter>(this.FileDb);

        //        return repository;
        //    }
        //}

        public FileSystemWithJsonUnitOfWork(string fileDb)
        {
            this.FileDb = fileDb;
            this.repositories = new Dictionary<Type, IRepository<IEntity>>();
        }
    }
}
