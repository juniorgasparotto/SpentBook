using System;
using System.Collections.Generic;

namespace SpentBook.Web.Models.TransactionTable
{
    public class TransactionTableModel
    {
        public IEnumerable<TransactionTableLineModel> Transactions { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<string> SubCategories { get; set; }
        public IEnumerable<Guid?> InitialIds { get; internal set; }
        public IEnumerable<string> Banks { get; internal set; }
    }
}
