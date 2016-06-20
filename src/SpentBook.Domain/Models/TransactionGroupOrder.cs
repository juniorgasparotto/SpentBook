using System.ComponentModel.DataAnnotations;
namespace SpentBook.Domain
{
    public enum TransactionGroupOrder
    {
        [Display(Name = "Agrupador")]
        Agrupador,
        [Display(Name = "Total")]
        Total,
        [Display(Name = "Qtd. de transações")]
        TransactionCount,
    }
}
