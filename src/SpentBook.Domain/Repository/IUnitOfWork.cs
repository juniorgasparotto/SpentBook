using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public interface IUnitOfWork
    {
        IRepository<Dashboard> Dashboards { get; }
        IRepository<Transaction> Transactions { get; }
        //IRepository<TransactionFilter> TransactionFilters { get; }
        //IRepository<Panel> Panels { get; }
    }
}
