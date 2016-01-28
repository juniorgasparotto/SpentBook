using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class Panel : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        
        public string Title { get; set; }
        public PanelType PanelType { get; set; }
        public int PanelOrder { get; set; }

        public TransactionGroupBy GroupBy { get; set; }
        public TransactionGroupBy GroupBy2 { get; set; }

        public TransactionOrder OrderBy { get; set; }
        public TransactionOrderClassification OrderByClassification { get; set; }

        public TransactionFilter Filter { get; set; }
    }
}
