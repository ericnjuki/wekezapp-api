using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.Entities {
    public class FlowItem {
        public int ItemId { get; set; }

        public string  Body { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsConfirmable { get; set; }

        public bool IsConfirmed { get; set; }

        public int TransactionId { get; set; }

        public ICollection<int> CanBeSeenBy { get; set; }

        public ICollection<int> HasBeenSeenBy { get; set; }

    }
}
