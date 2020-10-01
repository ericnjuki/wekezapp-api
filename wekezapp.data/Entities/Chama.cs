using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.Entities {
    public class Chama {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChamaId { get; set; }

        [Required]
        public string ChamaName { get; set; }

        public float Balance { get; set; }

        public float LoanInterestRate { get; set; }

        public float LatePaymentFineRate { get; set; }

        public float MinimumContribution { get; set; }

        //public bool ContributionsHalted { get; set; }

        [Required]
        public Period Period { get; set; }

        public float MgrAmount { get; set; }

        public string[] MgrOrder { get; set; }

        public int NextMgrReceiverIndex { get; set; }

        public DateTime NextMgrDate { get; set; }

    }
}
