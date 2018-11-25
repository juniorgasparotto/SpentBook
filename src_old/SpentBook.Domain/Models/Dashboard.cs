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

        //public void ReorderPanels(bool addAfterIfOccurConflict = false)
        //{
        //    List<Panel> panels;

        //    // add after  : order by panelOrder and if has conflict, set the oldest first.
        //    if (addAfterIfOccurConflict)
        //        panels = this.Panels.OrderBy(one => one.PanelOrder).ThenBy(two => two.LastUpdateDate).ToList();
        //    // add before: order by panelOrder and if has conflict, set the newest first.
        //    else
        //        panels = this.Panels.OrderBy(one => one.PanelOrder).ThenByDescending(two => two.LastUpdateDate).ToList();

        //    var i = 1;
        //    foreach (var panel in panels)
        //        panel.PanelOrder = i++;
        //}

        public void ReorderPanel(Panel panelToOrder = null, int newOrder = 0)
        {
            var panels = this.Panels.OrderBy(one => one.PanelOrder).ToList();

            if (panelToOrder != null)
            {
                panels.Remove(panelToOrder);

                if (newOrder <= panels.Count)
                    panels.Insert(newOrder - 1, panelToOrder);
                else
                    panels.Add(panelToOrder);
            }

            var i = 1;
            foreach (var panel in panels)
                panel.PanelOrder = i++;
        }
    }
}
