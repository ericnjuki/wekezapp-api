using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.Enums {
    public enum NotificationType {
        PersonalDepositAsDepositor = 1,
        PersonalDepositAsAdmin,
        PersonalWithdrawalAsWithdrawer,
        PersonalWithdrawalAsAdmin,
        DepositToChamaAsAdmin,
        DepositToChamaAsAll,
        WithdrawFromChamaAsAdmin,
        WithdrawFromChamaAsAll,
        LoanRequestAsRequester,
        LoanRequestAsAdmin,
        LoanDisbursmentAsRequester,
        LoanDisbursmentAsAll,
        LoanRepayment,
        LoanFineApplication,
        ContributionPayment,
        ContributionReminder,
        MerryGoRoundDisbursementAsReceipient,
        MerryGoRoundDisbursementAsAll,
        Payout
    }
}
