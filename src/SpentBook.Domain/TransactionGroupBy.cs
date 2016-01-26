using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public enum TransactionGroupBy
    {
        Date,
        DateDay,
        DateWeek,
        DateMonth,
        DateYear,
        Category,
        SubCategory,
        Name,
    }
}
