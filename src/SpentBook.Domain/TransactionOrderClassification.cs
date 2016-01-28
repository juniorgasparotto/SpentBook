using System.ComponentModel.DataAnnotations;
namespace SpentBook.Domain
{
    public enum TransactionOrderClassification
    {
        [Display(Name = "Ascendente")]
        Asc,
        [Display(Name = "Descendente")]
        Desc,
    }
}
