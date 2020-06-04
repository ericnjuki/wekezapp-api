using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using wekezapp.business.Contracts;
using wekezapp.data.Entities;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class FlowService : IFlowService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        public FlowService(WekezappContext shopAssist2Context, IMapper mapper) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
        }
        public void AddFlowItem(string body, ICollection<int> canBeSeenBy = null, bool isConfirmable = false, int transactionId = -1) {
            if (canBeSeenBy == null)
                canBeSeenBy = _ctx.Users.Select(u => u.UserId).ToList();

            var chamaCreatedNotif = new FlowItem() {
                Body = body,
                DateCreated = DateTime.UtcNow,
                CanBeSeenBy = canBeSeenBy,
                HasBeenSeenBy = null,
                IsConfirmable = isConfirmable,
                TransactionId = transactionId
            };

            //_ctx.FlowItems.Add(chamaCreatedNotif);
            //_ctx.SaveChanges();
        }

        //public IEnumerable<FlowItem> GetFlow(int userId) {
        //    var userFlow = new List<FlowItem>();
        //    foreach (var flowItem in _ctx.FlowItems) {
        //        if (flowItem.CanBeSeenBy.Contains(userId)) {
        //            userFlow.Add(flowItem);
        //        }
        //    }
        //    return userFlow;
        //}

        public void UpdateFlow()
        {

        }

    }
}
