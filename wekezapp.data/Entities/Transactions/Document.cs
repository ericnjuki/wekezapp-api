using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.Entities.Transactions {
    public class Document {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }

        public DocumentType DocumentType { get; set; }

        public bool IsReversal { get; set; }

        public float Amount { get; set; }

        //public DateTime RequestedDate { get; set; }

        //public int RequestedBy { get; set; }

        public int DebitTo { get; set; }

        public int CreditFrom { get; set; }

        public DateTime TransactionDate { get; set; }

        public int ConfirmedBy { get; set; }

        //public IEnumerable<int> CanBeConfirmedBy { get; set; } // reomove this, and allow just any admin to confirm

        // Nav
        public virtual Transaction Transaction { get; set; }

    }
}
