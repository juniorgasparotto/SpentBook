using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using SpentBook.Domain;

namespace SpentBook.Domain.Imports
{
    public class TransactionImportDefaultCSV
    {
        public List<Transaction> GetTransactionsFromFolder(string path)
        {
            var files = this.GetAllFiles(path);
            var list = new List<Transaction>();
            if (files != null && files.Length > 0)
            {
                foreach(var file in files)
                    list.AddRange(this.GetTransactionsFromCSV(file));
            }

            return list;
        }

        public List<Transaction> GetTransactionsFromCSV(string fullName)
        {
            var transactions = new List<Transaction>();
            using (var sr = new StreamReader(fullName))
            {
                var reader = new CsvReader(sr);
                reader.Parser.Configuration.HasHeaderRecord = false;
                reader.Parser.Configuration.IgnoreBlankLines = true;
                reader.Parser.Configuration.IgnoreHeaderWhiteSpace = true;
                reader.Parser.Configuration.Delimiter = ";";
                reader.Parser.Configuration.IgnoreReadingExceptions = true;

                var lines = reader.GetRecords<CSVLine>().ToList();
                foreach(var line in lines)
                {
                    var transaction = new Transaction()
                    {
                        Date = line.Date,
                        Category = line.Category,
                        SubCategory = line.SubCategory,
                        Name = line.Name,
                        Value = line.Value,
                    };

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        private string[] GetAllFiles(string path)
        {
            if (Directory.Exists(path))
                return Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories);

            return null;
        }

        private class CSVLine
        {
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
        }
    }
}