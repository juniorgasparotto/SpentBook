using System.ComponentModel.DataAnnotations;
namespace SpentBook.Domain
{
    public enum TransactionType
    {
        [Display(Name = "Ambos")]
        None,
        [Display(Name = "Receita")]
        Input,
        [Display(Name = "Despesas")]
        Output,
    }
}
