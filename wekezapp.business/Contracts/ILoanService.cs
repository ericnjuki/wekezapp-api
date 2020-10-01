using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.Entities.Transactions;

namespace wekezapp.business.Contracts {
    public interface ILoanService {
        Loan CreateLoan(Loan loan);

        Loan EvaluateLoan(Loan loanDto);

        Loan PayLoan(int loanId, float amount);

        IEnumerable<Loan> GetAllLoansForUser(int userId);

        IEnumerable<Loan> GetAllLoans();
    }
}
