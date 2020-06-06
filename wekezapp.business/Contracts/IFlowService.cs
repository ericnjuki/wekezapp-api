using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Entities;

namespace wekezapp.business.Contracts {
    public interface IFlowService
    {
        FlowItem GetFlowItem(int flowItemId);

        void AddFlowItem(string body, string[] canBeSeenBy = null, int transactionId = -1);

        IEnumerable<FlowItem> GetFlow(int userId);
    }
}
