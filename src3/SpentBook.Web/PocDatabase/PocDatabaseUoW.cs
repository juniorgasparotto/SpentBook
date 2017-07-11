using PocDatabase;
using SpentBook.Domain;
using System;
using System.Collections.Generic;

namespace SpentBook.Web
{
    internal class PocDatabaseUoW : IUnitOfWork
    {
        public static PocFile<Schema> _staticPocFile;

        private readonly Dictionary<Type, IRepository<IEntity>> repositories;

        public class Schema
        {
            public List<Dashboard> Dashboards { get; set; }
            public List<Transaction> Transactions { get; set; }
            public List<TransactionImport> TransactionsImports { get; set; }
            public List<Bank> Banks { get; set; }
        }

        public static PocFile<Schema> PocFile
        {
            get
            {
                if (_staticPocFile == null)
                    _staticPocFile = new PocFile<Schema>();
                return _staticPocFile;
            }
            set
            {
                _staticPocFile = value;
            }
        }

        public PocDatabaseUoW()
        {
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
                    repository = new PocDatabaseRepository<Dashboard>(PocFile);

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
                    repository = new PocDatabaseRepository<Transaction>(PocFile);

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
                    repository = new PocDatabaseRepository<TransactionImport>(PocFile);

                return repository;
            }
        }

        public IRepository<Bank> Banks
        {
            get
            {
                IRepository<Bank> repository;
                if (repositories.ContainsKey(typeof(Bank)))
                    repository = (IRepository<Bank>)repositories[typeof(Bank)];
                else
                    repository = new PocDatabaseRepository<Bank>(PocFile);

                return repository;
            }
        }

        public void Save()
        {
            lock(_staticPocFile)
                PocFile.Save();
        }
    }
}
