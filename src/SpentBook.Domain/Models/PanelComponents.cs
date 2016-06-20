using System;
using System.ComponentModel.DataAnnotations;

namespace SpentBook.Domain
{
    [Flags]
    public enum PanelComponents
    {
        [Display(Name = "Tabela")]
        Table = 1,

        [Display(Name = "Gráfico")]
        Chart = 2,
    }
}
