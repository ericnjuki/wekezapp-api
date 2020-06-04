using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.Interfaces {
    public class EmailOptions {
        public string Receipient { get; set; }

        public string Message { get; set; }

        public EmailType EmailType { get; set; }
    }
}
