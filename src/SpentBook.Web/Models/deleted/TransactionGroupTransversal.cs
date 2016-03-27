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
//    public class TransactionGroupTransversal
//    {
//        public string ParentPath { get; set; }
//        public string ItemGroupName { get; set; }
//        public TransactionGroupTransversalItem[] Items;

//        public class TransactionGroupTransversalItem 
//        {
//            public string ItemPath { get; set; }
//            public string Category { get; set; }
//            public double TotalItemInCategory { get; set; }

//            public override string ToString()
//            {
//                return this.ItemPath;
//            }
//        }

//        public TransactionGroupTransversal()
//        {
//        }

//        public void Add(string itemPath, string category, double total, int index)
//        {
//            this.Items[index] = new TransactionGroupTransversalItem()
//            {
//                ItemPath = itemPath,
//                Category = category,
//                TotalItemInCategory = total,
//            };
//        }

//        public override string ToString()
//        {
//            return this.ItemGroupName;
//        }

//        public int Id { get; set; }

//        public int? IdParent { get; set; }
//    }
//}