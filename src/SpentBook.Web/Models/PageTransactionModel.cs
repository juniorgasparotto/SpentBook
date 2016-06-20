using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Web.Models
{
    public class PageTransactionModel
    {
        public TransactionFilterModel Filter { get; set; }
        public List<TransactionModel> Transactions { get; set; }
        public ICollection<System.Web.Mvc.ModelState> Errors { get; set; }
    }
}
