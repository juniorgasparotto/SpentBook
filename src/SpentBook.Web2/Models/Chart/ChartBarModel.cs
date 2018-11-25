using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Web
{
    public class ChartBarModel
    {
        public class DataSet
        {
            public string fillColor { get; set; }
            public string strokeColor { get; set; }
            public string highlightFill { get; set; }
            public string highlightStroke { get; set; }
            public decimal[] data { get; set; }

            public string label { get; set; }
        }

        public List<string> labels { get; set; }
        public List<DataSet> datasets { get; set; }
    }
}
