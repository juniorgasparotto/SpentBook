using System.ComponentModel.DataAnnotations;
namespace SpentBook.Domain
{
    public enum TransactionOrder
    {
        [Display(Name = "Data")]
        Date,
        [Display(Name = "Valor")]
        Value,
        [Display(Name = "Categoria")]
        Category,
        [Display(Name = "Sub-categoria")]
        SubCategory,
        [Display(Name = "Nome")]
        Name,
    }
}
