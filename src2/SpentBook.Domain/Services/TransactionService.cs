using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace SpentBook.Domain.Services
{
    public class TransactionService
    {
        private IUnitOfWork uow;
        public TransactionService(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        public List<Transaction> GetTransactionsFiltrated(TransactionFilter filter)
        {
            var query = this.uow.Transactions.AsQueryable();

            //var a = new ResumeController();
            //var transactions = a.GetSpents();
            //query = transactions.AsQueryable();

            var onlyInputs = filter.TransactionType == TransactionType.Input;
            var onlyOutputs = filter.TransactionType == TransactionType.Output;

            if (onlyInputs)
                query = query.Where(t => t.Value > 0);

            if (onlyOutputs)
                query = query.Where(t => t.Value < 0);

            if (filter.Categories != null && filter.Categories.Count > 0)
                query = query.Where(t => filter.Categories.Contains(t.Category));

            if (filter.SubCategories != null && filter.SubCategories.Count > 0)
                query = query.Where(t => filter.SubCategories.Contains(t.SubCategory));

            if (filter.Names != null && filter.Names.Count > 0)
                query = query.Where(t => filter.Names.Contains(t.Name));

            if (filter.DateStart != null)
                query = query.Where(t => t.Date >= filter.DateStart);

            if (filter.DateEnd != null)
                query = query.Where(t => t.Date <= filter.DateEnd);

            if (filter.ValueStart != null)
                query = query.Where(t => t.Value >= filter.ValueStart);

            if (filter.ValueEnd != null)
                query = query.Where(t => t.Value <= filter.ValueEnd);

            string orderByName;
            var expressionOrderBy = filter.GetOrderByExpression(filter.OrderBy, out orderByName);

            if (filter.OrderByClassification == OrderClassification.Asc)
                query = query.OrderBy(expressionOrderBy);
            else
                query = query.OrderByDescending(expressionOrderBy);

            return query.ToList();
        }

        public TransactionGroup GetTransactionGroupRoot(List<Transaction> transactions, params TransactionGroupDefinition[] groupDefinitions)
        {
            Func<List<Transaction>, TransactionGroup, int, List<TransactionGroup>> methodGroup = null;
            methodGroup = (_transactions, transactionGroupParent, groupByIndex) =>
            {
                var groupByDefinition = default(TransactionGroupDefinition);
                var transactionGroups = new List<TransactionGroup>();

                if (groupDefinitions.Length > groupByIndex)
                    groupByDefinition = groupDefinitions[groupByIndex];
                
                if (groupByDefinition != null)
                {
                    var _transactionsQueryable = _transactions.AsQueryable();

                    if (groupByDefinition.OrderByClassification != null)
                    {
                        if (groupByDefinition.OrderByClassification == OrderClassification.Asc)
                            _transactionsQueryable = _transactionsQueryable.OrderBy(groupByDefinition.OrderByExpression);
                        else
                            _transactionsQueryable = _transactionsQueryable.OrderByDescending(groupByDefinition.OrderByExpression);
                    }

                    var group = _transactionsQueryable
                        .GroupBy(groupByDefinition.GroupByExpression)
                        .ToList();

                    transactionGroupParent.GroupByDefinition = groupByDefinition;
                    
                    var nextGroupByIndex = groupByIndex + 1;
                    var transactionGroupsNews = group.Select(
                        (g, gIndex) =>
                        {
                            var list = g.ToList();
                            var name = this.GetTransactionGroupNameUsingValue(groupByDefinition.GroupBy, list);
                            var key = g.Key;
                            var transactionGroup = ConfigureTransactionGroup(new TransactionGroup(), key, name, list, null, transactionGroupParent, nextGroupByIndex);
                            transactionGroup.SubGroups = methodGroup(list, transactionGroup, nextGroupByIndex);
                            return transactionGroup;
                        }
                    );

                    if (groupByDefinition.OrderByGroupClassification != null)
                    {
                        if (groupByDefinition.OrderByGroupClassification == OrderClassification.Asc)
                            transactionGroupsNews = transactionGroupsNews.AsQueryable().OrderBy(groupByDefinition.OrderByGroupExpression);
                        else
                            transactionGroupsNews = transactionGroupsNews.AsQueryable().OrderByDescending(groupByDefinition.OrderByGroupExpression);
                    }

                    var transactionGroupsNewsList = transactionGroupsNews.ToList();
                    transactionGroups.AddRange(transactionGroupsNewsList);
                }

                return transactionGroups;
            };

            var transactionGroupRoot = new TransactionGroup();
            ConfigureTransactionGroup(transactionGroupRoot, null, "Todos", transactions, null, null, 0);
            transactionGroupRoot.SubGroups = methodGroup(transactions, transactionGroupRoot, 0);            
            return transactionGroupRoot;
        }

        public List<ChartDataCategorized> GetChartDataCategorized(TransactionGroup transactionGroupRoot, bool tryCategorize = true)
        {
            var listReturn = new List<ChartDataCategorized>();
            Action<TransactionGroup, ChartDataCategorized> methodGroup = null;
            int id = 0;
            methodGroup = (current, parentTransversal) =>
            {
                if (current.SubGroups.Count == 0)
                    return;

                //if (grandchildrenGroups != null)
                //    if (current.GroupByDefinition.OrderByClassification == TransactionOrderClassification.Asc)
                //        grandchildrenGroups.OrderBy(f => f.Key);

                //if (tryCategorize && grandchildrenGroups.Count > 0)
                // categoriza apenas o root se for solicitado
                if (tryCategorize && current.Parent == null)
                {
                    var children = current.SubGroups;
                    var grandchildrenGroups = children.SelectMany(f => f.SubGroups).GroupBy(f => f.Key).ToList();

                    foreach (var grandchildrenGroup in grandchildrenGroups)
                    {
                        var list = grandchildrenGroup.ToList();

                        var inverted = new ChartDataCategorized()
                        {
                            Id = ++id,
                            IdParent = (parentTransversal == null ? null : (int?)parentTransversal.Id),
                            ParentPath = current.GetPath(),
                            ItemGroupName = grandchildrenGroup.Key.ToString(),
                            Items = new ChartDataCategorized.Item[children.Count]
                        };

                        listReturn.Add(inverted);

                        foreach (var grandchild in list)
                        {
                            var indexOfParent = current.SubGroups.IndexOf(grandchild.Parent);
                            inverted.Add(grandchild.GetPath(), grandchild.Parent.Key.ToString(), indexOfParent, grandchild);
                            methodGroup(grandchild, inverted);
                        }
                    }
                }
                else
                {
                    var name = current.Key != null ? current.Key.ToString() : (current.GroupByDefinition != null ? current.GroupByDefinition.GroupByName : "");
                    if (current.Parent == null && name == null)
                        name = current.SubGroups.First().GroupByDefinition.GroupByName ?? "Todos";

                    var inverted = new ChartDataCategorized()
                    {
                        Id = ++id,
                        IdParent = (parentTransversal == null ? null : (int?)parentTransversal.Id),
                        ParentPath = current.GetPath(),
                        ItemGroupName = name,
                        Items = new ChartDataCategorized.Item[current.SubGroups.Count]
                    };

                    listReturn.Add(inverted);

                    var index = 0;
                    foreach (var childGroup in current.SubGroups)
                    {
                        inverted.Add(childGroup.GetPath(), childGroup.Key.ToString(), index++, childGroup);
                        methodGroup(childGroup, inverted);
                    }
                }

                //foreach(var child in children)
                //    methodGroup(child, 0);
            };

            methodGroup(transactionGroupRoot, null);

            return listReturn;
        }

        private TransactionGroup ConfigureTransactionGroup(TransactionGroup transactionGroup, object key, string name, List<Transaction> transactions, List<TransactionGroup> subGroups, TransactionGroup parent, int level)
        {
            transactionGroup.Key = key;
            //transactionGroup.GroupByDefinition = groupByDefinition;
            transactionGroup.Name = name;
            transactionGroup.Parent = parent;
            transactionGroup.Transactions = transactions;
            transactionGroup.SubGroups = subGroups;
            transactionGroup.Level = level;
            
            if (transactionGroup.Total == 0)
                transactionGroup.Total = transactionGroup.Transactions.Sum(f => f.Value);

            if (transactionGroup.Count == 0)
                transactionGroup.Count = transactionGroup.Transactions.Count;

            //transactionGroup.TotalAsPositive = Math.Abs(transactionGroup.Total);
            if (parent == null)
            {
                transactionGroup.TotalPercentage.Add(100m);
                transactionGroup.CountPercentage.Add(100m);
            }

            while (parent != null)
            {
                transactionGroup.TotalPercentage.Add((transactionGroup.Total * 100m) / parent.Total);
                transactionGroup.CountPercentage.Add((transactionGroup.Count * 100m) / parent.Count);

                parent = parent.Parent;
            }

            return transactionGroup;
        }

        private string GetTransactionGroupNameUsingValue(TransactionGroupBy? group, List<Transaction> transactions)
        {
            if (group != null)
            {
                var transaction = transactions.FirstOrDefault();
                switch (group)
                {
                    case TransactionGroupBy.Category:
                        return transaction.Category;
                    case TransactionGroupBy.SubCategory:
                        return transaction.SubCategory;
                    case TransactionGroupBy.DateDay:
                        return transaction.Date.ToString("yyyy/MM/dd");
                    case TransactionGroupBy.DateMonth:
                        return transaction.Date.ToString("yyyy/MM");
                    case TransactionGroupBy.DateYear:
                        return transaction.Date.ToString("yyyy");
                    case TransactionGroupBy.Name:
                        return transaction.Name;
                    case TransactionGroupBy.InputOutput:
                        return transaction.Value > 0 ? "Receita" : "Despesa";
                }
            }

            return null;
        }
    }
}