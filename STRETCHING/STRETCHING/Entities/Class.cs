using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class Class
    {
        public int ClassId { get; set; }
        public int DirectionId { get; set; }
        public int TrainerId { get; set; }
        public DateTime ClassDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int HallId { get; set; }
    }
}
