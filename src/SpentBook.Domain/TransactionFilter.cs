using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class TransactionFilter
    {       
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public decimal? ValueStart { get; set; }
        public decimal? ValueEnd { get; set; }
        public List<string> Categories { get; set; }
        public List<string> SubCategories { get; set; }
        public List<string> Names { get; set; }
        public TransactionType TransactionType { get; set; }        
    }
}
