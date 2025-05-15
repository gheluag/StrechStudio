using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Org.BouncyCastle.Asn1.X509;

namespace STRETCHING.Entities
{
    public class Administrators
    {
        public int Id { get; set; }
        public string Login {  get; set; }
        public string Password { get; set; }
        public string Last_name { get; set; }
        public string First_name { get; set; }
        public string Middle_name { get; set; }
        public int Role { get; set; }


    }
}
