using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class Dashboard : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Name { get; set; }
        public List<Panel> Panels { get; set; }

        public string FriendlyUrl { get; set; }
    }
}
