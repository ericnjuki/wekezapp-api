using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.Entities.Transactions {
    public class Transaction {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public float Amount { get; set; }

        public bool IsClosed { get; set; }

        public DateTime DateClosed { get; set; }

    }
}
