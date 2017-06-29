using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace SpentBook.Web.Models
{
    public class PageTransactionModel
    {
        public TransactionFilterModel Filter { get; set; }
        public List<TransactionModel> Transactions { get; set; }
        public ICollection<ModelStateEntry> Errors { get; set; }
    }
}
