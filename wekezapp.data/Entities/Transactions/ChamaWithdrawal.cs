using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace wekezapp.data.Entities.Transactions {
    public class ChamaWithdrawal : Transaction {
        [Column("WithdrawerId")]
        public int WithdrawerId { get; set; }

        [Column("DateRequested")]
        public DateTime DateRequested { get; set; }

    }
}
