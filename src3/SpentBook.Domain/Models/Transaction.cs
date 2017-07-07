using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class Transaction : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BankName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }

        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public decimal Value { get; set; }
        public decimal ValueAsPositive { get { return Math.Abs(Value); }}


        public bool IsSpent()
        {
            if (Value < 0)
                return true;

            return false;
        }
    }
}
