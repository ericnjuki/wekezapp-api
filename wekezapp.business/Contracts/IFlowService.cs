using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Enums;

namespace wekezapp.business.Contracts {
    public interface IFlowService
    {
        FlowItem GetFlowItem(int flowItemId);

        void AddFlowItem(NotificationType notificationType, int transactionId = -1, string body = "", string[] canBeSeenBy = null);

        IEnumerable<FlowItemDto> GetFlow(int userId);

        IEnumerable<FlowItemDto> GetFlowOfType(int userId, NotificationType notificationType);

        void UpdateFlow(FlowItem flowItemDto);
    }
}
