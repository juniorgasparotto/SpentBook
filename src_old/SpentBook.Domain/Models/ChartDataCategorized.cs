namespace SpentBook.Domain
{
    public class ChartDataCategorized
    {
        public string ParentPath { get; set; }
        public string ItemGroupName { get; set; }
        public Item[] Items;

        public class Item
        {
            public string ItemPath { get; set; }
            public string Category { get; set; }
            public decimal Total { get; set; }
            public decimal TotalPercentage { get; set; }
            public decimal Count { get; set; }
            public decimal CountPercentage { get; set; }

            public override string ToString()
            {
                return this.ItemPath;
            }

            public decimal TotalPercentageGrandParentRelation { get; set; }

            public decimal CountPercentageGrandParentRelation { get; set; }
        }

        public ChartDataCategorized()
        {
        }

        public void Add(string itemPath, string category, int index, TransactionGroup transactionGroup)
        {
            this.Items[index] = new Item()
            {
                ItemPath = itemPath,
                Category = category,
                Total = transactionGroup.Total,
                Count = transactionGroup.Count,
                TotalPercentage = transactionGroup.TotalPercentage.Count > 0 ? transactionGroup.TotalPercentage[0] : 0,
                CountPercentage = transactionGroup.CountPercentage.Count > 0 ? transactionGroup.CountPercentage[0] : 0,
                TotalPercentageGrandParentRelation = transactionGroup.TotalPercentage.Count > 1 ? transactionGroup.TotalPercentage[1] : 0,
                CountPercentageGrandParentRelation = transactionGroup.CountPercentage.Count > 1 ? transactionGroup.CountPercentage[1] : 0,
            };
        }

        public override string ToString()
        {
            return this.ItemGroupName;
        }

        public int Id { get; set; }

        public int? IdParent { get; set; }
    }
}