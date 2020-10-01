using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace wekezapp.data.Entities.Transactions {
    public class MerryGoRound : Transaction
    {
        [Column("ReceiverId")]
        public int ReceiverId { get; set; }

        [Column("DateDue")]
        public DateTime DateDue { get; set; }

    }
}
