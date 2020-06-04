using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.Entities.Transactions {
    public class ContributionFine : Transaction {
        public int ContributionId { get; set; }

        public float Rate { get; set; }

        public DateTime DateApplied { get; set; }
    }
}
