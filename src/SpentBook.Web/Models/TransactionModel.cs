using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Web.Models
{
    public class TransactionModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
    }
}
