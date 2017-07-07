using PocDatabase;
using SpentBook.Domain;
using System;
using System.Collections.Generic;

namespace SpentBook.Web
{
    internal class PocDatabaseUoW : IUnitOfWork
    {
        private readonly PocFile<Schema> pocFile;
        private readonly Dictionary<Type, IRepository<IEntity>> repositories;

        public class Schema
        {
            public List<Dashboard> Dashboards { get; set; }
            public List<Transaction> Transactions { get; set; }
            public List<TransactionImport> TransactionsImports { get; set; }
        }

        public PocDatabaseUoW()
        {
            this.pocFile = new PocFile<Schema>();
            this.repositories = new Dictionary<Type, IRepository<IEntity>>();
        }

        public IRepository<Dashboard> Dashboards
        {
            get
            {
                IRepository<Dashboard> repository;
                if (repositories.ContainsKey(typeof(Dashboard)))
                    repository = (IRepository<Dashboard>)repositories[typeof(Dashboard)];
                else
                    repository = new PocDatabaseRepository<Dashboard>(pocFile);

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
                    repository = new PocDatabaseRepository<Transaction>(pocFile);

                return repository;
            }
        }

        public IRepository<TransactionImport> TransactionsImports
        {
            get
            {
                IRepository<TransactionImport> repository;
                if (repositories.ContainsKey(typeof(TransactionImport)))
                    repository = (IRepository<TransactionImport>)repositories[typeof(TransactionImport)];
                else
                    repository = new PocDatabaseRepository<TransactionImport>(pocFile);

                return repository;
            }
        }

        public void Save()
        {
            pocFile.Save();
        }
    }
}
