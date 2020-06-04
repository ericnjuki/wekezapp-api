using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace wekezapp.data.Entities.Transactions {
    public class Contribution : Transaction {
        public int ContributorId { get; set; }

        [Column("DateDue")]
        public DateTime DateDue { get; set; }

        [Column("DateRequested")]
        public DateTime DateFirstRequested { get; set; } // may not need  you, same as DateClosed?
    }
}
