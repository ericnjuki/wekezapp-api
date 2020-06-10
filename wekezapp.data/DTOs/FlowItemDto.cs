using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.DTOs {
    public class FlowItemDto {
        public int FlowItemId { get; set; }

        public string Body { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsConfirmable { get; set; }

        public bool IsConfirmed { get; set; }

        public string[] HasBeenSeenBy { get; set; }
    }
}
