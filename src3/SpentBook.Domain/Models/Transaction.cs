﻿using System;

namespace SpentBook.Domain
{
    public class Transaction : IEntity
    {
        public Guid Id { get; set; }
        public Guid IdUser { get; set; }
        public Guid? IdImport { get; set; }
        public Bank Bank { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }

        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public decimal Value { get; set; }
        public decimal ValueAsPositive { get { return Math.Abs(Value); }}

        public string IdExternal { get; set; }

        public bool IsSpent()
        {
            if (Value < 0)
                return true;

            return false;
        }
    }
}
