using System;
using System.Collections.Generic;

namespace SpentBook.Domain
{
    public class TransactionImport
    {
        public enum StatusCode
        {
            None = 0,
            Warning = 1,
            AutomaticResolved = 2,
            Error = 3
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string BankName { get; set; }
        public string FormatFile { get; set; }
        public StatusCode Status { get; set; }
        public List<string> StatusMessage { get; set; }
    }
}
