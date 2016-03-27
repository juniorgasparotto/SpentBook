//using CsvHelper;
//using SpentBook.Data.FileSystem;
//using SpentBook.Domain;
//using SpentBook.Web.Filters;
//using SpentBook.Web.Models;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Web;
//using System.Web.Mvc;
//using System.Linq.Dynamic;
//using DotNet.Highcharts.Options;
//using DotNet.Highcharts.Helpers;
//using ExpressionGraph;

//namespace SpentBook.Web.Models
//{
//    public class TransactionGroupDefinition
//    {
//        public TransactionGroupBy GroupBy { get; set; }
//        public TransactionOrder OrderBy { get; set; }
//        public TransactionOrderClassification OrderByClassification { get; set; }

//        public string GroupByName { get; set; }
//        public System.Linq.Expressions.Expression<Func<Transaction, object>> GroupByExpression { get; set; }

//        public string OrderByName { get; set; }
//        public System.Linq.Expressions.Expression<Func<Transaction, object>> OrderByExpression { get; set; }
//    }
//}