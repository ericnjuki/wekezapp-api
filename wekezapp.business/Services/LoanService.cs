using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Enums;
using wekezapp.data.Interfaces;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class LoanService: ILoanService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        private readonly IFlowService _flowService;
        private readonly IAtomicProcedures _atomicProcedures;

        public LoanService(WekezappContext ctx, IMapper mapper, IAtomicProcedures atomicProcedures, IFlowService flowService) {
            _ctx = ctx;
            _mapper = mapper;
            _atomicProcedures = atomicProcedures;
            _flowService = flowService;
        }

        public Loan CreateLoan(Loan loan) {
            // maker sure you have amt, receiverId and dateRequested

            //Loan newLoan = new Loan() {
            //    Amount = loan.Amount,
            //    ReceiverId = loan.ReceiverId,
            //    DateRequested = loan.DateRequested
            //};

            _ctx.Loans.Add(loan);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.LoanRequestAsRequester, loan.TransactionId);
            _flowService.AddFlowItem(NotificationType.LoanRequestAsAdmin, loan.TransactionId);
            return loan;
        }

        public Loan EvaluateLoan(Loan loanDto) {
            // make sure you have evaluatedById and Approved flag
            if (!loanDto.Approved) {
                loanDto.IsClosed = true;
                loanDto.DateClosed = DateTime.Now;
                //_ctx.Loans.Add(loanDto);
                _ctx.Entry(loanDto).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _ctx.SaveChanges();

                foreach (var item in _ctx.FlowItems.Where(i => i.NotificationType == NotificationType.LoanRequestAsAdmin || i.NotificationType == NotificationType.LoanRequestAsRequester).ToList()) {
                    if(item.TransactionId == loanDto.TransactionId) {
                        item.IsConfirmed = true;
                        _flowService.UpdateFlow(item);
                    }
                }

            } else {
                // issue the loan first bc documents are only created if money has moved around
                _atomicProcedures.ChamaToPersonal(loanDto.Amount, loanDto.ReceiverId);

                var chama = _ctx.Chamas.First();
                Document loanIssuanceDocument = new Document {
                    Transaction = loanDto,
                    TransactionId = loanDto.TransactionId,
                    DocumentType = DocumentType.LoanRemittance,
                    CreditFrom = chama.ChamaId,
                    DebitTo = loanDto.ReceiverId,
                    IsReversal = false,
                    ConfirmedBy = loanDto.EvaluatedBy,
                    Amount = loanDto.Amount,
                    TransactionDate = DateTime.Now
                };

                if (loanDto.InterestRate >= 0) loanDto.InterestRate = chama.LoanInterestRate;
                loanDto.AmountPayable = loanDto.Amount + loanDto.Amount * loanDto.InterestRate / 100;
                loanDto.DateIssued = DateTime.Now;
                loanDto.DateDue = loanDto.DateIssued.AddMonths(1); // check if set by client first

                _ctx.Entry(loanDto).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _ctx.Documents.Add(loanIssuanceDocument);
                _ctx.SaveChanges();

                _flowService.AddFlowItem(NotificationType.LoanDisbursmentAsRequester, loanDto.TransactionId);
                _flowService.AddFlowItem(NotificationType.LoanDisbursmentAsAll, loanDto.TransactionId);

            }
            return loanDto;
        }

        public Loan PayLoan(int loanId, float amountPaid) {
            var loan = _ctx.Loans.Find(loanId);
            _atomicProcedures.PersonalToChama(amountPaid, loan.ReceiverId);

            Document loanPaymentDocument = new Document {
                Transaction = loan,
                TransactionId = loan.TransactionId,
                DocumentType = DocumentType.LoanPayment,
                DebitTo = _ctx.Chamas.First().ChamaId,
                CreditFrom = loan.ReceiverId,
                IsReversal = false,
                Amount = amountPaid
            };
            loanPaymentDocument.TransactionDate = DateTime.Now;

            loan.AmountPaidSoFar += amountPaid;
            if (loan.AmountPaidSoFar >= loan.AmountPayable) {
                loan.IsClosed = true;
                loan.DateClosed = loanPaymentDocument.TransactionDate = DateTime.Now;
                // return any overpayment
                var overPayment = loan.AmountPaidSoFar - loan.AmountPayable;
                _atomicProcedures.ChamaToPersonal(overPayment, loan.ReceiverId);
            }
            _ctx.Entry(loan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _ctx.Documents.Add(loanPaymentDocument);
            _ctx.SaveChanges();
            _flowService.AddFlowItem(NotificationType.LoanRepayment, loanPaymentDocument.DocumentId);
            return loan;
        }

        public void ApplyLoanFine(int loanId) {
            //TODO: add this to the functions that are checked 'periodically', when we figure out how to do that
            var loan = _ctx.Loans.Find(loanId);
            loan.AmountPayable += loan.LatePaymentFine;

            _flowService.AddFlowItem(NotificationType.LoanFineApplication, loanId);
        }

        public void ReverseLoan(int loanId) {
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

        public IEnumerable<Loan> GetAllLoansForUser(int userId) {
            return _ctx.Loans.Where(l => l.ReceiverId == userId).ToList();
        }

    }
}
