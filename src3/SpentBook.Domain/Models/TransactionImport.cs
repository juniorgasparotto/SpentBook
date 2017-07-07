using System;
using System.Collections.Generic;

namespace SpentBook.Domain
{
    public class TransactionImport
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string BankName { get; set; }
        public string FormatFile { get; set; }
    }
}
