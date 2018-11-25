using System;

namespace SpentBook.Web.Models
{
    public class CSVLineModel
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string BankName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
    }
}
