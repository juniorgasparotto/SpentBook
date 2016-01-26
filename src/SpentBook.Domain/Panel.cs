using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public class Panel : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Title { get; set; }
        public int PanelOrder { get; set; }
        public TransactionFilter Filter { get; set; }
        public List<PanelBlock> Blocks { get; set; }

        public void Save()
        {
            if (this.Id == null)
                this.Id = Guid.NewGuid();
        }
    }
}
