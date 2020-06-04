using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using wekezapp.data.Entities.Transactions;

namespace wekezapp.data.Entities.Transactions {
    public class Loan : Transaction {
        [Column("ReceiverId")]
        public int ReceiverId { get; set; }

        [Column("DateRequested")]
        public DateTime DateRequested { get; set; }

        public bool Approved { get; set; } // a loan *can* be rejected

        public int EvaluatedBy { get; set; } // admin who approves or rejects

        public DateTime DateIssued { get; set; }

        [Column("DateDue")]
        public DateTime DateDue { get; set; }

        public float InterestRate { get; set; }

        public float AmountPayable { get; set; }

        public float AmountPaidSoFar { get; set; }

        public float LatePaymentFine { get; set; }

        public bool IsDefaulted { get; set; }

    }
}
