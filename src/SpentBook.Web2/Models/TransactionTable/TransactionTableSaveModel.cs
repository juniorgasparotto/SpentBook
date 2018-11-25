using System;
using System.Collections.Generic;

namespace SpentBook.Web.Models.TransactionTable
{
    public class TransactionTableSaveModel
    {
        public List<Guid> InitialIds { get; set; }
        public List<TransactionTableLineModel> Transactions { get; set; }
    }
}
