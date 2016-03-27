using System.ComponentModel.DataAnnotations;
namespace SpentBook.Domain
{
    public enum OrderClassification
    {
        [Display(Name = "Ascendente")]
        Asc,
        [Display(Name = "Descendente")]
        Desc,
    }
}
