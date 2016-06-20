using System.ComponentModel.DataAnnotations;
namespace SpentBook.Domain
{
    public enum PanelWidth
    {
        [Display(Name = "Pequeno")]
        Small,
        [Display(Name = "Médio")]
        Medium,
        [Display(Name = "Grande")]
        Large,
    }
}
