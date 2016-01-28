using System.ComponentModel.DataAnnotations;

namespace SpentBook.Domain
{
    public enum PanelType
    {
        [Display(Name = "Tabela")]
        Table,

        [Display(Name = "Gráfico em barras")]
        ChartBar,

        [Display(Name = "Gráfico de pizza")]
        ChartDoughnut,
    }
}
