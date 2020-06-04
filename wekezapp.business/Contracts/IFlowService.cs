using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Entities;

namespace wekezapp.business.Contracts {
    public interface IFlowService {
        void AddFlowItem(string body, ICollection<int> canBeSeenBy = null, bool isConfirmable = false, int transactionId = -1);
        //IEnumerable<FlowItem> GetFlow(int userId);
    }
}
