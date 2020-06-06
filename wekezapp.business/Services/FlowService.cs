using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public FlowItem GetFlowItem(int flowItemId) {
            return _ctx.FlowItems.Find(flowItemId);
        }

        public void AddFlowItem(string body, string[] canBeSeenBy = null, int transactionId = -1) {
            if (canBeSeenBy == null)
                canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();

            var flowItem = new FlowItem() {
                Body = body,
                DateCreated = DateTime.Now,
                CanBeSeenBy = canBeSeenBy,
                TransactionId = transactionId
            };

            _ctx.FlowItems.Add(flowItem);
            _ctx.SaveChanges();
        }

        public IEnumerable<FlowItem> GetFlow(int userId) {
            var userFlow = new List<FlowItem>();
            foreach (var flowItem in _ctx.FlowItems) {
                if (flowItem.CanBeSeenBy.Contains(userId.ToString())) {
                    userFlow.Add(flowItem);
                }
            }
            return userFlow;
        }

        public void UpdateFlow(FlowItem flowItemDto) {
            var flowItem = _ctx.Entry(flowItemDto).State = EntityState.Modified;
            _ctx.SaveChanges();
        }

    }
}
