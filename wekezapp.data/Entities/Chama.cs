using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.Entities {
    public class Chama {
        public int ChamaId { get; set; }

        public string ChamaName { get; set; }

        public virtual ICollection<ChamaUser> ChamaUsers { get; set; }
    }
}
