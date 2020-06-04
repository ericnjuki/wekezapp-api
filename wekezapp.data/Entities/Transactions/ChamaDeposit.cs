using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace wekezapp.data.Entities.Transactions {
    public class ChamaDeposit : Transaction {
        [Column("DepositorId")]
        public int DepositorId { get; set; }

        [Column("DateRequested")]
        public DateTime DateRequested { get; set; }

    }
}
