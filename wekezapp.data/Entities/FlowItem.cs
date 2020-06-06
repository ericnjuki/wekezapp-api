using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace wekezapp.data.Entities {
    public class FlowItem {
        [Key]
        public int FlowItemId { get; set; }

        public string Body { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsConfirmable { get; set; }

        public bool IsConfirmed { get; set; }

        public int TransactionId { get; set; }

        public string[] CanBeSeenBy { get; set; }

        public string[] HasBeenSeenBy { get; set; }

    }
}
