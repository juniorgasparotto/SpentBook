using System;
using System.Collections.Generic;
using System.Globalization;
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
        public PanelComponents PanelComponents { get; set; }
        public int PanelOrder { get; set; }
        
        public TransactionOrder OrderBy { get; set; }
        public OrderClassification OrderByClassification { get; set; }

        public TransactionGroupBy GroupBy { get; set; }
        public TransactionOrder GroupByOrderBy { get; set; }
        public OrderClassification GroupByOrderByClassification { get; set; }
        public TransactionGroupOrder GroupByOrderByGroup { get; set; }
        public OrderClassification GroupByOrderByGroupClassification { get; set; }

        public TransactionGroupBy GroupBy2 { get; set; }
        public TransactionOrder GroupByOrderBy2 { get; set; }
        public OrderClassification GroupByOrderByClassification2 { get; set; }
        public TransactionGroupOrder GroupByOrderByGroup2 { get; set; }
        public OrderClassification GroupByOrderByGroupClassification2 { get; set; }

        public TransactionGroupBy GroupBy3 { get; set; }
        public TransactionOrder GroupByOrderBy3 { get; set; }
        public OrderClassification GroupByOrderByClassification3 { get; set; }
        public TransactionGroupOrder GroupByOrderByGroup3 { get; set; }
        public OrderClassification GroupByOrderByGroupClassification3 { get; set; }

        public TransactionFilter Filter { get; set; }
        public PanelWidth PanelWidth { get; set; }

        public List<TransactionGroupDefinition> GetGroupDefinitions()
        {
            if (this._groupDefinitions == null)
            {
                this._groupDefinitions = new List<TransactionGroupDefinition>();
                //var onlyOutputs = this.Filter.TransactionType == TransactionType.Output;
                var groupByName = "";
                var orderByName = "";

                if (this.GroupBy != TransactionGroupBy.None)
                {
                    var groupBy1 = new TransactionGroupDefinition()
                    {
                        OrderBy = this.GroupByOrderBy,
                        OrderByClassification = this.GroupByOrderByClassification,
                        OrderByExpression = this.GetOrderByExpression(this.GroupByOrderBy, out orderByName),
                        OrderByName = orderByName,
                        GroupBy = this.GroupBy,
                        GroupByExpression = this.GetGroupByExpression(this.GroupBy, out groupByName),
                        GroupByName = groupByName,
                        OrderByGroup = this.GroupByOrderByGroup,
                        OrderByGroupClassification = this.GroupByOrderByGroupClassification,
                        OrderByGroupExpression = this.GetOrderByGroupExpression(this.GroupByOrderByGroup, out orderByName),
                        OrderByGroupName = orderByName,
                    };
                    this._groupDefinitions.Add(groupBy1);
                }

                if (this.GroupBy2 != TransactionGroupBy.None)
                {
                    var groupBy2 = new TransactionGroupDefinition()
                    {
                        OrderBy = this.GroupByOrderBy2,
                        OrderByClassification = this.GroupByOrderByClassification2,
                        OrderByExpression = this.GetOrderByExpression(this.GroupByOrderBy2, out orderByName),
                        OrderByName = orderByName,
                        GroupBy = this.GroupBy2,
                        GroupByExpression = this.GetGroupByExpression(this.GroupBy2, out groupByName),
                        GroupByName = groupByName,
                        OrderByGroup = this.GroupByOrderByGroup2,
                        OrderByGroupClassification = this.GroupByOrderByGroupClassification2,
                        OrderByGroupExpression = this.GetOrderByGroupExpression(this.GroupByOrderByGroup2, out orderByName),
                        OrderByGroupName = orderByName,
                    };
                    this._groupDefinitions.Add(groupBy2);
                }

                if (this.GroupBy3 != TransactionGroupBy.None)
                {
                    var groupBy3 = new TransactionGroupDefinition()
                    {
                        OrderBy = this.GroupByOrderBy3,
                        OrderByClassification = this.GroupByOrderByClassification3,
                        OrderByExpression = this.GetOrderByExpression(this.GroupByOrderBy3, out orderByName),
                        OrderByName = orderByName,
                        GroupBy = this.GroupBy3,
                        GroupByExpression = this.GetGroupByExpression(this.GroupBy3, out groupByName),
                        GroupByName = groupByName,
                        OrderByGroup = this.GroupByOrderByGroup3,
                        OrderByGroupClassification = this.GroupByOrderByGroupClassification3,
                        OrderByGroupExpression = this.GetOrderByGroupExpression(this.GroupByOrderByGroup3, out orderByName),
                        OrderByGroupName = orderByName,
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

        public Expression<Func<TransactionGroup, object>> GetOrderByGroupExpression(TransactionGroupOrder orderBy, out string orderByName)
        {
            switch (orderBy)
            {
                case TransactionGroupOrder.Agrupador:
                    orderByName = "Agrupador";
                    break;
                case TransactionGroupOrder.Total:
                    orderByName = "Total";
                    break;
                case TransactionGroupOrder.TransactionCount:
                    orderByName = "Qtd. de transações";
                    break;
                default:
                    orderByName = "Nenhum";
                    break;
            }

            return f => GetTransactionGroupOrderByValue(f, orderBy);
        }

        public static object GetTransactionGroupOrderByValue(TransactionGroup transactionGroup, TransactionGroupOrder orderBy)
        {
            switch (orderBy)
            {
                case TransactionGroupOrder.Agrupador:
                    var groupBy = transactionGroup.Parent.GroupByDefinition.GroupBy;
                    if (groupBy == TransactionGroupBy.DateDay)
                        return DateTime.ParseExact(transactionGroup.Key, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    else if (groupBy == TransactionGroupBy.DateMonth)
                        return DateTime.ParseExact(transactionGroup.Key + "/01", "yyyy/MM/01", CultureInfo.InvariantCulture);
                    else if (groupBy == TransactionGroupBy.DateMonth)
                        return DateTime.ParseExact(transactionGroup.Key + "/01/01", "yyyy/01/01", CultureInfo.InvariantCulture);

                    return transactionGroup.Key;
                case TransactionGroupOrder.Total:
                    return transactionGroup.Total;
                case TransactionGroupOrder.TransactionCount:
                    return transactionGroup.TransactionCount;
            }

            return null;
        }
    }
}
