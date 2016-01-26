using Microsoft.AspNet.Identity.EntityFramework;
using SpentBook.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpentBook.Web.Models
{
    public class DashboardsModel
    {
        public List<Dashboard> Dashboards { get; set; }
    }

    public class DashboardModel
    {
        [Required(ErrorMessage = "O campo 'Nome' é obrigatório.")]
        public string Name { get; set; }

        public Dashboard Dashboard { get; set; }
    }
}