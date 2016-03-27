using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace SpentBook.Domain
{
    public class TransactionGroupDefinition
    {
        public TransactionGroupBy GroupBy { get; set; }
        public Expression<Func<Transaction, object>> GroupByExpression { get; set; }
        public string GroupByName { get; set; }

        public TransactionOrder OrderBy { get; set; }
        public OrderClassification OrderByClassification { get; set; }
        public Expression<Func<Transaction, object>> OrderByExpression { get; set; }
        public string OrderByName { get; set; }

        public TransactionGroupOrder OrderByGroup { get; set; }
        public OrderClassification OrderByGroupClassification { get; set; }
        public Expression<Func<TransactionGroup, object>> OrderByGroupExpression { get; set; }
        public string OrderByGroupName { get; set; }

    }
}