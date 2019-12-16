using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace wekezapp.data.Entities {
    public class ChamaUser {
        [Key]
        [Column(Order = 0)]
        [ForeignKey("Chama")]
        public int ChamaId { get; set; }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("User")]
        public int UserId { get; set; }


        // navigation
        public virtual Chama Chama { get; set; }

        public virtual User User { get; set; }
    }
}
