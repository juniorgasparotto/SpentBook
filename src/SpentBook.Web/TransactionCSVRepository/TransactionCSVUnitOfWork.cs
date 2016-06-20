using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Web
{
    public class TransactionCSVUnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, IRepository<IEntity>> repositories = new Dictionary<Type,IRepository<IEntity>>();     
   
        public string FileDb { get; private set; }

        public IRepository<Dashboard> Dashboards
        {
            get
            {
                return null;
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
                    repository = new TransactionCSVRepository();

                return repository;
            }
        }
    }
}
