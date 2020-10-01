using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Entities.Transactions;

namespace wekezapp.business.Contracts {
    public interface ILedgerService {
        Transaction GetTransactionById(int transacId);

        void RequestDepositToPersonal(PersonalDeposit transac);

        void ConfirmDepositToPersonal(int transactionId, int confirmedById);

        void RequestWithdrawalFromPersonal(PersonalWithdrawal transac);

        void ConfirmWithdrawalFromPersonal(int transactionId, int confirmedById);

        MerryGoRound CreateMgr();

        MerryGoRound DisburseMgr(int transactionId);

        void CreateContributions();

        UserDto ContributeToChama(int userId, float amount, bool startWithOld);

        Chama Payout(int userId, float amount, int confirmedBy);

        void RequestDepositToChama(ChamaDeposit transac);

        void ConfirmDepositToChama(int transactionId, int confirmedById);

        void RequestWithdrawalFromChama(ChamaWithdrawal transac);

        void ConfirmWithdrawalFromChama(int transactionId, int confirmedById);
    }
}
