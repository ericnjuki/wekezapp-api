using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Entities.Transactions;

namespace wekezapp.business.Contracts {
    public interface ILedgerService {
        void RequestDepositToPersonal(PersonalDeposit transac);

        void ConfirmDepositToPersonal(int transactionId, int confirmedById);
    }
}
