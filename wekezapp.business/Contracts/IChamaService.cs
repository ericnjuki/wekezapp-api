using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;

namespace wekezapp.business.Contracts {
    public interface IChamaService {
        Chama GetChama();

        void AddChama(Chama chamaDto);

        Chama UpdateChama(Chama chamaDto);

        void DeleteChama(int chamaId);

        bool IsContributionsDay();

    }
}
