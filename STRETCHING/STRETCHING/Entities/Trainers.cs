using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public List<int> SpecializationIds { get; set; } = new List<int>();
        public string SpecializationNames { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
        public string ShortName => $"{LastName} {FirstName[0]}.{MiddleName[0]}.";

        public List<string> SpecializationNamesList
        {
            get
            {
                if (string.IsNullOrEmpty(SpecializationNames))
                    return new List<string> { "Специализации не указаны" };

                return SpecializationNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
    }
}
