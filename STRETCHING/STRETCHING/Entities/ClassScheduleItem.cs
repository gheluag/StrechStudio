using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class ClassScheduleItem
    {
        public int ClassId { get; set; }
        public DateTime ClassDate { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DirectionName { get; set; }
        public string TrainerFullName { get; set; }
        public string HallName { get; set; }
        public int BookedClientsCount { get; set; }
    }
}
