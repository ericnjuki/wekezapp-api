using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Enums;
using wekezapp.data.Interfaces;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class LoanService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        //private readonly FlowService _flowService;
        private readonly AtomicProcedures _atomicProcedures;

        public LoanService(WekezappContext ctx, IMapper mapper, AtomicProcedures atomicProcedures) {
            _ctx = ctx;
            _mapper = mapper;
            _atomicProcedures = atomicProcedures;
        }

        public Loan CreateLoan(Loan loan) {
            // maker sure you have amt, receiverId and dateRequested
            _ctx.Loans.Add(loan);
            _ctx.SaveChanges();
            return loan;
        }

        public void ApproveLoan(Loan loanDto) {
            // make sure you have evaluatedById and Approved flag
            if (!loanDto.Approved) {
                loanDto.IsClosed = true;
                loanDto.DateClosed = DateTime.Now;
                _ctx.Loans.Add(loanDto);
                _ctx.SaveChanges();
            } else {
                var chama = _ctx.Chamas.FirstOrDefault();
                if (loanDto.InterestRate >= 0) loanDto.InterestRate = chama.LoanInterestRate;
                loanDto.AmountPayable = loanDto.Amount + loanDto.Amount * loanDto.InterestRate;
                loanDto.DateIssued = DateTime.Now;
                loanDto.DateDue = loanDto.DateIssued.AddMonths(1); // check if set by client first

                // issue the loan
                _atomicProcedures.ChamaToPersonal(loanDto.Amount, loanDto.ReceiverId);

            }
        }

        public void PayLoan(int loanId, float amountPaid) {
            var loan = _ctx.Loans.Find(loanId);
            _atomicProcedures.PersonalToChama(amountPaid, loan.ReceiverId);

            loan.AmountPaidSoFar += amountPaid;
            if (loan.AmountPaidSoFar >= loan.AmountPayable) {
                loan.IsClosed = true;
                loan.DateClosed = DateTime.Now;
                // return any overpayment
                var overPayment = loan.AmountPaidSoFar - loan.AmountPayable;
                _atomicProcedures.ChamaToPersonal(overPayment, loan.ReceiverId);
            }
        }

        public void ApplyLoanFine(int loanId) {
            //TODO: add this to the functions that are checked 'periodically', when we figure out how to do that
            var loan = _ctx.Loans.Find(loanId);
            loan.AmountPayable += loan.LatePaymentFine;

        }

        public void ReverseLoan(int loanId)
        {
            var loan = _ctx.Loans.Find(loanId);
            loan.InterestRate = 0;
            loan.AmountPayable = loan.AmountPaidSoFar = loan.Amount;

            _atomicProcedures.PersonalToChama(loan.Amount, loan.ReceiverId);
            loan.IsClosed = true;
            loan.DateClosed = DateTime.Now;

        }

        public void UpdateLoan(Loan loan) {
            _ctx.Entry(loan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _ctx.SaveChanges();

        }
        public void DeleteLoanById(int loanId) {
            _ctx.Loans.Remove(_ctx.Loans.Find(loanId));
            _ctx.SaveChanges();
        }

        public IEnumerable<Loan> GetAllLoans() {
            return _ctx.Loans.ToList();
        }
    }
}
