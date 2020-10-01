using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.DTOs {
    public class LoanDto {
        public int TransactionId { get; set; }

        public float Amount { get; set; }

        public bool IsClosed { get; set; }

        public DateTime DateClosed { get; set; }

        public int ReceiverId { get; set; }

        public DateTime DateRequested { get; set; }

        public bool Approved { get; set; }

        public int EvaluatedBy { get; set; } // admin who approves or rejects

        public DateTime DateIssued { get; set; }

        public DateTime DateDue { get; set; }

        public float InterestRate { get; set; }

        public float AmountPayable { get; set; }

        public float AmountPaidSoFar { get; set; }

        public float LatePaymentFine { get; set; }

        public bool IsDefaulted { get; set; }
    }
}
