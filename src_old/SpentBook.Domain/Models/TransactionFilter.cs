using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class TransactionFilter
    {       
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public decimal? ValueStart { get; set; }
        public decimal? ValueEnd { get; set; }
        public List<string> Categories { get; set; }
        public List<string> SubCategories { get; set; }
        public List<string> Names { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionOrder OrderBy { get; set; }
        public OrderClassification OrderByClassification { get; set; }

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
                case TransactionGroupBy.InputOutput:
                    groupName = "Receitas e despesas";
                    return f => f.Value > 0 ? "Receita" : "Despesa";
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
                        return DateTime.ParseExact(transactionGroup.Key.ToString(), "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    else if (groupBy == TransactionGroupBy.DateMonth)
                        return DateTime.ParseExact(transactionGroup.Key.ToString() + "/01", "yyyy/MM/01", CultureInfo.InvariantCulture);
                    else if (groupBy == TransactionGroupBy.DateMonth)
                        return DateTime.ParseExact(transactionGroup.Key.ToString() + "/01/01", "yyyy/01/01", CultureInfo.InvariantCulture);

                    return transactionGroup.Key;
                case TransactionGroupOrder.Total:
                    return transactionGroup.Total;
                case TransactionGroupOrder.TransactionCount:
                    return transactionGroup.Count;
            }

            return null;
        }

    }
}
