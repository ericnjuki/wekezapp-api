using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Enums;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class ChamaService : IChamaService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        private readonly IFlowService _flowService;

        //private readonly IFlowService _flowService;

        public ChamaService(WekezappContext shopAssist2Context, IMapper mapper, IFlowService flowService) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
            _flowService = flowService;
        }
        public Chama GetChama() {
            return _ctx.Chamas.FirstOrDefault();
        }

        public void AddChama(Chama chamaDto) {
            if (chamaDto == null)
                throw new ArgumentNullException(nameof(chamaDto));

            if (string.IsNullOrWhiteSpace(chamaDto.ChamaName))
                chamaDto.ChamaName = "NewChama";

            _ctx.Chamas.Add(chamaDto);
            _ctx.SaveChanges();

            var notifBody = $"{_ctx.Users.Single(u => u.Role == Role.Admin).FirstName} created chama {chamaDto.ChamaName}";
            _flowService.AddFlowItem(NotificationType.Announcement, -1, notifBody);

        }

        public Chama UpdateChama(Chama chamaDto) {
            _ctx.Entry(chamaDto).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _ctx.SaveChanges();
            return _ctx.Chamas.FirstOrDefault();
        }

        public void DeleteChama(int chamaId) {
            var chama = _ctx.Chamas.Find(chamaId);
            _ctx.Chamas.Remove(chama);
            _ctx.SaveChanges();
        }

        public bool IsContributionsDay() {
            var chama = _ctx.Chamas.First();
            return chama.NextMgrDate <= DateTime.Now ? true : false;
        }
    }
}
