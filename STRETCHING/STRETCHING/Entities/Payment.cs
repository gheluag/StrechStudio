using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class Payment
    {
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public DateTime? PromisedPaymentDate { get; set; }
        public int PaymentMethodId { get; set; } = 1; // По умолчанию наличные
        public string Comment { get; set; }
    }
}
