using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Web.Models
{
    public class TransactionFilterModel
    {
        [Display(Name = "Tipo da transação")]
        public TransactionType TransactionType { get; set; }

        [Display(Name = "Data de")]
        [DataType(DataType.Date)]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Data até")]
        [DataType(DataType.Date)]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Valor de")]
        public decimal? ValueStart { get; set; }

        [Display(Name = "Valor até")]
        public decimal? ValueEnd { get; set; }

        [StringLength(1000)]
        [Display(Name = "Categorias")]
        public string Categories { get; set; }

        [StringLength(1000)]
        [Display(Name = "Sub-Categorias")]
        public string  SubCategories { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nome das transações")]
        public string TransactionNames { get; set; }

        public TransactionOrder OrderBy { get; set; }
        public OrderClassification OrderByClassification { get; set; }
    }
}
