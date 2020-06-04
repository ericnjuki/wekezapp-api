using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Enums;

namespace wekezapp.data.DTOs {
    public class ChamaDto {
        public int ChamaId { get; set; }

        public string ChamaName { get; set; }

        public float Balance { get; set; }

        public float FineRate { get; set; }

        public float MinimumContribution { get; set; }

        public int Period { get; set; }

        public List<int> MgrOrder { get; set; }
    }
}
