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
//    public class TransactionGroup
//    {
//        public string Key { get; set; }
//        public TransactionGroupDefinition GroupByDefinition { get; set; }
//        public string Name { get; set; }
//        public double Total { get; set; }
//        public double TotalAsPositive { get; set; }
//        public List<Transaction> Transactions { get; set; }
//        public List<TransactionGroup> SubGroups { get; set; }
//        public TransactionGroup Parent { get; set; }
//        public int TransactionCount { get; set; }
//        public int Level { get; set; }

//        public TransactionGroup()
//        {
//            this.Transactions = new List<Transaction>();
//            this.SubGroups = new List<TransactionGroup>();
//        }

//        public string GetPath()
//        {
//            var paths = new List<string>();
//            this.GetPath(paths);
//            paths.Reverse();

//            if (paths.Count == 0)
//                return null;

//            return string.Join(@"/", paths.ToArray());
//        }

//        private void GetPath(List<string> paths)
//        {
//            if (this.Key != null)
//                paths.Add("{" + this.Key + "}");

//            if (this.Parent != null)
//                this.Parent.GetPath(paths);
//        }

//        public override string ToString()
//        {
//            return this.Name;
//        }
//    }
//}