using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.Interfaces {
    public class Transaction {
        DateTime TransactionDate { get; set; }

        bool IsApproved { get; set; }

        IEnumerable<int> Approvers { get; set; }
    }
}
