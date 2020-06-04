using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.DTOs;

namespace wekezapp.business.Contracts {
    public interface IChamaService {
        ChamaDto GetChama();

        void AddChama(ChamaDto chamaDto);

        ChamaDto UpdateChama(ChamaDto chamaDto);

        void DeleteChama(int chamaId);

    }
}
