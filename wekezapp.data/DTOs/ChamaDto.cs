using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.DTOs {
    public class ChamaDto {
        public int ChamaId { get; set; }

        public string ChamaName { get; set; }

        public float Balance { get; set; }

        public float LoanInterestRate { get; set; }

        public float LatePaymentFineRate { get; set; }

        public float MinimumContribution { get; set; }

        public Period Period { get; set; }

        public float MgrAmount { get; set; }

        public string[] MgrOrder { get; set; }

        public int NextMgrReceiverIndex { get; set; }

        public DateTime NextMgrDate { get; set; }

        public float TotalOwed { get; set; }
    }
}
