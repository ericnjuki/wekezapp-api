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
        //private readonly IFlowService _flowService;

        public ChamaService(WekezappContext shopAssist2Context, IMapper mapper) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
        }
        public ChamaDto GetChama() {
            return _mapper.Map<ChamaDto>(_ctx.Chamas.FirstOrDefault());
        }

        public void AddChama(ChamaDto chamaDto) {
            if (chamaDto == null)
                throw new ArgumentNullException(nameof(chamaDto));

            if (string.IsNullOrWhiteSpace(chamaDto.ChamaName))
                chamaDto.ChamaName = "NewChama";

            var chama = _mapper.Map<Chama>(chamaDto);

            _ctx.Chamas.Add(chama);
            _ctx.SaveChanges();

            //var notifBody = $"{_ctx.Users.Single(u => u.Role == Role.Admin).FirstName} created chama {chama.ChamaName}";
            //_flowService.AddFlowItem(notifBody);

        }

        public ChamaDto UpdateChama(ChamaDto chamaDto) {
            var updatedChama = _mapper.Map<Chama>(chamaDto);

            _ctx.Entry(updatedChama).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _ctx.SaveChanges();
            return _mapper.Map<ChamaDto>(_ctx.Chamas.FirstOrDefault());
        }

        public void DeleteChama(int chamaId) {
            var chama = _ctx.Chamas.Find(chamaId);
            _ctx.Chamas.Remove(chama);
            _ctx.SaveChanges();
        }

    }
}
