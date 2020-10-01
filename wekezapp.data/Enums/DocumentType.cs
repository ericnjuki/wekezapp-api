using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.Enums {
    public enum DocumentType {
        // singular actions
        PersonalAccountDeposit = 0,
        PersonalAccountWithdrawal,
        DepositToChama,
        WithdrawalFromChama,

        // double actions (involve two accounts)
        LoanRemittance, // from chama to personal
        LoanPayment, // from personal to chama
        PayOut, // from chama to personal (just a generic transaction to facilitate chama giving money to members)
        ContributionPayment, // from personal to chama
        MerryGoRoundDisbursment, // from chama to personal

        ExecutiveEdit

        // reversals
        // I wanted to add reversals here as types
        // But i decided a better model would be to use the above types to fulfill reversals
        // e.g. a DepositToChama will be reversed by creating a WithdrawalFromChama and vice versa
    }
}
