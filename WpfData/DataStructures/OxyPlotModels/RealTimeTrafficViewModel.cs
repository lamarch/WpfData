using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfData.DataStructures.OxyPlotModels
{
    internal class RealTimeTrafficViewModel
    {

        public RealTimeTrafficViewModel(List<NetworkMeasure> measures)
        {
            this.Measures = measures;
        }
        public List<NetworkMeasure> Measures { get; private set; }
        public string Title { get; } = "Traffic en temps réel";
    }
}
