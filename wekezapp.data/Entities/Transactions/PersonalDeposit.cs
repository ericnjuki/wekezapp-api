using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace wekezapp.data.Entities.Transactions {
    public class PersonalDeposit: Transaction {
        [Required]
        [Column("DepositorId")]
        public int DepositorId { get; set; }

        [Required]
        [Column("DateRequested")]
        public DateTime DateRequested { get; set; }

    }
}
