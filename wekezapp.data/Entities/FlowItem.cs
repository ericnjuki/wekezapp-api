using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.Entities {
    public class FlowItem {
        [Key]
        public int FlowItemId { get; set; }

        public NotificationType NotificationType { get; set; }

        public int TransactionId { get; set; }

        public string Body { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public bool IsConfirmable { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsForAll { get; set; }

        public string[] CanBeSeenBy { get; set; }

        public string[] HasBeenSeenBy { get; set; }

    }
}
