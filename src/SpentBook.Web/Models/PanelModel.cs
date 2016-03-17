using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Web.Models
{
    public class PanelModel
    {
        public Guid Id { get; set; }
        public Dashboard Dashboard { get; set; }

        #region Panel info

        [Required]
        [StringLength(30)]
        [Display(Name = "Titulo do painel")]
        public string Title { get; set; }

        [Display(Name = "Exibir componentes")]
        [Required]
        public PanelComponents PanelComponents { get; set; }

        [Display(Name = "Tamanho do painel")]
        [Required]
        public PanelWidth PanelWidth { get; set; }

        [Display(Name = "Ordem do painel")]
        [Required]
        [Range(1, int.MaxValue)]
        public int PanelOrder { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Caminho do template")]
        public string ViewName { get; set; }

        #endregion

        #region Agrupamento

        [Display(Name = "Agrupamento principal por")]
        public TransactionGroupBy GroupBy { get; set; }
        
        [Display(Name = "Ordernar por")]
        public TransactionOrder GroupByOrderBy { get; set; }
        
        [Display(Name = "Classificar por")]
        public TransactionOrderClassification GroupByOrderByClassification { get; set; }

        [Display(Name = "Agrupamento secundário por")]
        public TransactionGroupBy GroupBy2 { get; set; }
        
        [Display(Name = "Ordernar por")]
        public TransactionOrder GroupByOrderBy2 { get; set; }
        
        [Display(Name = "Classificar por")]
        public TransactionOrderClassification GroupByOrderByClassification2 { get; set; }

        [Display(Name = "Agrupamento terciario por")]
        public TransactionGroupBy GroupBy3 { get; set; }
        
        [Display(Name = "Ordernar por")]
        public TransactionOrder GroupByOrderBy3 { get; set; }

        [Display(Name = "Classificar por")]
        public TransactionOrderClassification GroupByOrderByClassification3 { get; set; }

        #endregion

        #region Ordenação

        [Display(Name = "Order por")]
        public TransactionOrder OrderBy { get; set; }

        [Display(Name = "Classificar")]
        public TransactionOrderClassification OrderByClassification { get; set; }

        #endregion

        #region Filter

        //[Required]
        [Display(Name = "Tipo da transação")]
        public TransactionType FilterTransactionType { get; set; }

        [Display(Name = "Data de")]
        public DateTime? FilterDateStart { get; set; }

        [Display(Name = "Data até")]
        public DateTime? FilterDateEnd { get; set; }

        [Display(Name = "Valor de")]
        public decimal? FilterValueStart { get; set; }

        [Display(Name = "Valor até")]
        public decimal? FilterValueEnd { get; set; }

        [StringLength(1000)]
        [Display(Name = "Categorias")]
        public string FilterCategories { get; set; }

        [StringLength(1000)]
        [Display(Name = "Sub-Categorias")]
        public string  FilterSubCategories { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nome das transações")]
        public string FilterTransactionNames { get; set; }

        #endregion



        public Panel Panel { get; set; }
    }
}
