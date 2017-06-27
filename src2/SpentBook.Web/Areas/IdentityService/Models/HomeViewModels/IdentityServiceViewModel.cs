using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpentBook.Web.Identity.Models.HomeViewModels
{
    public class IdentityServiceViewModel
    {
        public string Issuer { get; set; }

        public IEnumerable<ClientViewModel> Clients { get; set; }
    }
}
