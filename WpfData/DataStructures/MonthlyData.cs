using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfData
{
    public class MonthlyData
    {
        public List<MonthlyDataMeasure> Measures = new List<MonthlyDataMeasure>();
    }

    public class MonthlyDataMeasure
    {
        DateTime dateTime;
        double use;
        double max;

        public MonthlyDataMeasure (DateTime dateTime, double use, double max)
        {
            this.dateTime = dateTime;
            this.use = use;
            this.max = max;
        }
    }
}
