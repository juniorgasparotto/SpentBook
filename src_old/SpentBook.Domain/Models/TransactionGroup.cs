using System.Collections.Generic;
namespace SpentBook.Domain
{
    public class TransactionGroup
    {
        public object Key { get; set; }
        public TransactionGroupDefinition GroupByDefinition { get; set; }
        public string Name { get; set; }
        public decimal Total { get; set; }        
        public int Count { get; set; }
        public List<decimal> TotalPercentage { get; set; }
        public List<decimal> CountPercentage { get; set; }
        //public double TotalAsPositive { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<TransactionGroup> SubGroups { get; set; }
        public TransactionGroup Parent { get; set; }
        
        public int Level { get; set; }

        public TransactionGroup()
        {
            this.Transactions = new List<Transaction>();
            this.SubGroups = new List<TransactionGroup>();
            this.TotalPercentage = new List<decimal>();
            this.CountPercentage = new List<decimal>();
        }

        public string GetPath()
        {
            var paths = new List<string>();
            this.GetPath(paths);
            paths.Reverse();

            if (paths.Count == 0)
                return null;

            return string.Join(@"/", paths.ToArray());
        }

        private void GetPath(List<string> paths)
        {
            if (this.Key != null)
                paths.Add("{" + this.Key + "}");

            if (this.Parent != null)
                this.Parent.GetPath(paths);
        }

        public override string ToString()
        {
            return this.Name;
        }

    }
}