using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal FullPrice { get; set; }
        public int DurationMonths { get; set; }
        public int TotalClasses { get; set; }
    }
}
