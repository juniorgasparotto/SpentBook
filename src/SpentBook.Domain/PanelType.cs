using System;
using System.ComponentModel.DataAnnotations;

namespace SpentBook.Domain
{
    [Flags]
    public enum PanelType
    {
        [Display(Name = "Tabela")]
        Table = 1,

        [Display(Name = "Gráfico em barras")]
        ChartBar = 2,

        [Display(Name = "Gráfico de pizza")]
        ChartDoughnut = 4,
    }
}
