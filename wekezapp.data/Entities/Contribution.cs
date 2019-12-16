using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Interfaces;

namespace wekezapp.data.Entities {
    public class Contribution : Transaction {
        public int ContributerId { get; set; }

        public float Amount { get; set; }

        // navigation
        public virtual User Contributer { get; set; }
    }
}
