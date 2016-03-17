using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class Panel : IEntity
    {
        private List<TransactionGroupDefinition> _groupDefinitions;

        public enum PanelDataType
        {
            OneGroup,
            TwoGroup,
            ThreeOrMoreGroup,
            NonGroup,
            NonGroupAndSortDate
        }

        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string ViewName { get; set; }
        
        public string Title { get; set; }
        public PanelType PanelType { get; set; }
        public int PanelOrder { get; set; }

        public TransactionGroupBy GroupBy { get; set; }
        public TransactionOrder GroupByOrderBy { get; set; }
        public TransactionOrderClassification GroupByOrderByClassification { get; set; }

        public TransactionGroupBy GroupBy2 { get; set; }
        public TransactionOrder GroupByOrderBy2 { get; set; }
        public TransactionOrderClassification GroupByOrderByClassification2 { get; set; }

        public TransactionGroupBy GroupBy3 { get; set; }
        public TransactionOrder GroupByOrderBy3 { get; set; }
        public TransactionOrderClassification GroupByOrderByClassification3 { get; set; }

        public TransactionOrder OrderBy { get; set; }
        public TransactionOrderClassification OrderByClassification { get; set; }

        public TransactionFilter Filter { get; set; }
        public PanelWidth PanelWidth { get; set; }

        public List<TransactionGroupDefinition> GetGroupDefinitions()
        {
            if (this._groupDefinitions == null)
            {
                this._groupDefinitions = new List<TransactionGroupDefinition>();
                var onlyOutputs = this.Filter.TransactionType == TransactionType.Output;
                var groupByName = "";
                var orderByName = "";

                if (this.GroupBy != TransactionGroupBy.None)
                {
                    var groupBy1 = new TransactionGroupDefinition()
                    {
                        GroupBy = this.GroupBy,
                        OrderBy = this.GroupByOrderBy,
                        OrderByClassification = this.GroupByOrderByClassification,
                        GroupByExpression = this.GetGroupByExpression(this.GroupBy, out groupByName),
                        GroupByName = groupByName,
                        OrderByExpression = this.GetOrderByExpression(this.GroupByOrderBy, out orderByName),
                        OrderByName = groupByName,
                    };
                    this._groupDefinitions.Add(groupBy1);
                }

                if (this.GroupBy2 != TransactionGroupBy.None)
                {
                    var groupBy2 = new TransactionGroupDefinition()
                    {
                        GroupBy = this.GroupBy2,
                        OrderBy = this.GroupByOrderBy2,
                        OrderByClassification = this.GroupByOrderByClassification2,
                        GroupByExpression = this.GetGroupByExpression(this.GroupBy2, out groupByName),
                        GroupByName = groupByName,
                        OrderByExpression = this.GetOrderByExpression(this.GroupByOrderBy2, out orderByName),
                        OrderByName = groupByName,
                    };
                    this._groupDefinitions.Add(groupBy2);
                }

                if (this.GroupBy3 != TransactionGroupBy.None)
                {
                    var groupBy3 = new TransactionGroupDefinition()
                    {
                        GroupBy = this.GroupBy3,
                        OrderBy = this.GroupByOrderBy3,
                        OrderByClassification = this.GroupByOrderByClassification3,
                        GroupByExpression = this.GetGroupByExpression(this.GroupBy3, out groupByName),
                        GroupByName = groupByName,
                        OrderByExpression = this.GetOrderByExpression(this.GroupByOrderBy3, out orderByName),
                        OrderByName = groupByName,
                    };
                    this._groupDefinitions.Add(groupBy3);
                }

            }

            return this._groupDefinitions;
        }

        public PanelDataType GetDataType()
        {
            var groupDefinition = this.GetGroupDefinitions();
            if (groupDefinition.Count > 0)
            {
                // Como só existe 1 nível, fica impossível criar categorização
                if (this.GetGroupDefinitions().Count == 1)
                    return PanelDataType.OneGroup;
                else if (groupDefinition.Count == 2)
                    return PanelDataType.TwoGroup;

                return PanelDataType.ThreeOrMoreGroup;
            }
            else
            {
                if (this.OrderBy == TransactionOrder.Date)
                    return PanelDataType.NonGroupAndSortDate;
                else
                    return PanelDataType.NonGroup;
            }
        }

        public Expression<Func<Transaction, object>> GetGroupByExpression(TransactionGroupBy group, out string groupName)
        {
            switch (group)
            {
                case TransactionGroupBy.Category:
                    groupName = "Categorias";
                    return f => f.Category;
                case TransactionGroupBy.SubCategory:
                    groupName = "Sub-categorias";
                    return f => f.Category + "/" + f.SubCategory;
                case TransactionGroupBy.DateDay:
                    groupName = "Dias";
                    return f => f.Date.ToString("yyyy/MM/dd");
                case TransactionGroupBy.DateMonth:
                    groupName = "Meses";
                    return f => f.Date.ToString("yyyy/MM");
                case TransactionGroupBy.DateYear:
                    groupName = "Anos";
                    return f => f.Date.ToString("yyyy");
                case TransactionGroupBy.Name:
                    groupName = "Nomes";
                    return f => f.Name;
            }
            groupName = "Nenhum";
            return null;
        }

        public Expression<Func<Transaction, object>> GetOrderByExpression(TransactionOrder orderBy, out string orderByName)
        {
            switch (orderBy)
            {
                case TransactionOrder.Category:
                    orderByName = "Categorias";
                    return f => f.Category;
                case TransactionOrder.SubCategory:
                    orderByName = "Sub-categorias";
                    return f => f.SubCategory;
                case TransactionOrder.Date:
                    orderByName = "Data";
                    return f => f.Date;
                case TransactionOrder.Value:
                    orderByName = "Valor";
                    return f => f.Value;
                case TransactionOrder.Name:
                    orderByName = "Nomes";
                    return f => f.Name;
            }
            orderByName = "Nenhum";
            return null;
        }
    }
}
