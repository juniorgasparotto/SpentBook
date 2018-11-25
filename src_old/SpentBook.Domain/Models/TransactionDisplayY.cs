using System.ComponentModel.DataAnnotations;

namespace SpentBook.Domain
{
    public enum TransactionDisplayY
    {
        [Display(Name = "Valor")]
        Value,
        [Display(Name = "% do valor")]
        ValuePercentage,
        [Display(Name = "Quantidade")]
        Count,
        [Display(Name = "% da quantidade")]
        CountPercentage
    }
}
