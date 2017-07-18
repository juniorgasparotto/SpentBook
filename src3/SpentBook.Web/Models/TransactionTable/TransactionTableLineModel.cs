using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpentBook.Web.Models.TransactionTable
{
    public class TransactionTableLineModel
    {
        public enum StatusCode
        {
            None = 0,
            Warning = 1,
            AutomaticResolved = 2,
            Error = 3
        }

        public Guid? Id { get; set; }
        public Guid? IdImport { get; set; }
        public Guid? IdUser { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Value { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string BankName { get; set; }
        public StatusCode? Status { get; set; }
        public List<string> StatusMessage { get; set; }
        public Bank Bank { get; internal set; }
        public string IdExternal { get; internal set; }
    }

}
