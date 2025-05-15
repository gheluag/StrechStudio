using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class Clients
    {
        public int IdClient { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string HealthConditions { get; set; }

        public int RoleId { get; set; } 
        public string RoleName { get; set; }


        public string FullName => $"{LastName} {FirstName} {MiddleName}";

    }
}
