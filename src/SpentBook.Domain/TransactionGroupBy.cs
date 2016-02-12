using System.ComponentModel.DataAnnotations;

namespace SpentBook.Domain
{
    public enum TransactionGroupBy
    {
        [Display(Name = "Nenhum")]
        None,
        [Display(Name = "Dia")]
        DateDay,
        //DateDay,
        //DateWeek,
        [Display(Name = "Mês")]
        DateMonth,
        [Display(Name = "Ano")]
        DateYear,
        [Display(Name = "Categoria")]
        Category,
        [Display(Name = "Sub-categoria")]
        SubCategory,
        [Display(Name = "Nome da transação")]
        Name,
    }
}
