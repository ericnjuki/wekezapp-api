using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using wekezapp.business.Contracts;
using wekezapp.data.Entities;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Enums;
using wekezapp.data.Interfaces;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class LedgerService : ILedgerService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        private readonly IFlowService _flowService;
        //private readonly AtomicProcedures _atoms;
        public LedgerService(WekezappContext ctx, IMapper mapper, IFlowService flowService) {
            _ctx = ctx;
            _mapper = mapper;
            _flowService = flowService;
            //_atoms = atoms;
        }
        public void RequestDepositToPersonal(PersonalDeposit transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.PersonalDeposits.Add(transac);
            _ctx.SaveChanges();

            _flowService
                .AddFlowItem("PersonalDepositAsDepositor",
                    new string[] { transac.DepositorId.ToString() },
                    transac.TransactionId);
            _flowService
                .AddFlowItem("PersonalDepositAsAdmin",
                    _ctx.Users.Where(u => u.Role == Role.Admin).Select(u => u.UserId.ToString()).ToArray(),
                    transac.TransactionId);
        }

        public void ConfirmDepositToPersonal(int transactionId, int confirmedById) {
            var transac = _ctx.PersonalDeposits.Find(transactionId);

            Document depositToPersonalDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.PersonalAccountDeposit,
                DebitTo = transac.DepositorId,
                IsReversal = false,
                ConfirmedBy = confirmedById
            };

            transac.IsClosed = true;
            transac.DateClosed = depositToPersonalDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(depositToPersonalDocument);
            _ctx.SaveChanges();
        }

        public void RequestWithdrawFromPersonal(PersonalWithdrawal transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.PersonalWithdrawals.Add(transac);
            _ctx.SaveChanges();
        }

        public void ConfirmWithdrawFromPersonal(int transactionId, int confirmedById) {
            var transac = _ctx.PersonalWithdrawals.Find(transactionId);
            Document withdrawFromPersonalDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.PersonalAccountWithdrawal,
                CreditFrom = transac.WithdrawerId,
                IsReversal = false,
                ConfirmedBy = confirmedById
            };

            transac.IsClosed = true;
            transac.DateClosed = withdrawFromPersonalDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(withdrawFromPersonalDocument);
            _ctx.SaveChanges();
        }

        public void RequestDepositToChama(ChamaWithdrawal transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.ChamaWithdrawals.Add(transac);
            _ctx.SaveChanges();
        }

        public void DepositToChama(int transactionId, int confirmedById) {
            var transac = _ctx.ChamaDeposits.Find(transactionId);
            Document depositToChamaDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.DepositToChama,
                DebitTo = _ctx.Chamas.FirstOrDefault().ChamaId,
                IsReversal = false,
                ConfirmedBy = confirmedById
            };

            transac.IsClosed = true;
            transac.DateClosed = depositToChamaDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(depositToChamaDocument);
            _ctx.SaveChanges();
        }

        public void RequestWithdrawalFromChama(ChamaDeposit transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.ChamaDeposits.Add(transac);
            _ctx.SaveChanges();
        }

        public void WithdrawFromChama(int transactionId, int confirmedById) {
            var transac = _ctx.ChamaWithdrawals.Find(transactionId);
            Document withdrawFromChamaDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.WithdrawalFromChama,
                CreditFrom = _ctx.Chamas.FirstOrDefault().ChamaId,
                IsReversal = false,
                ConfirmedBy = confirmedById
            };

            transac.IsClosed = true;
            transac.DateClosed = withdrawFromChamaDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(withdrawFromChamaDocument);
            _ctx.SaveChanges();
        }

        public void CreateContributions() {
            // todo: add to those regular functions
            // find everyone
            // add a contribution transaction for each id as the contributer
        }

        public void ContributeToChama(int transactionId) {
            // todo: check if balance suffices? (oh, wait, that's in atomic procedures, nvm)
            var transac = _ctx.Contributions.Find(transactionId);
            Document contributeToChamaDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.ContributionPayment,
                CreditFrom = transac.ContributorId,
                DebitTo = _ctx.Chamas.FirstOrDefault().ChamaId,
                IsReversal = false,
                // ConfirmedBy = confirmedById // not a confirmable action
            };

            transac.IsClosed = true;
            transac.DateClosed = contributeToChamaDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(contributeToChamaDocument);
            _ctx.SaveChanges();
        }

        public void DisburseMerryGoRound(int transactionId) {
            // todo: add to those regular functions, but add it so that it is always done, unless an
            // admin says it to not be done, maybe they wanna skip MGR for a while
            // todo: check if balance suffices? (oh, wait, that's in atomic procedures, nvm)
            var transac = _ctx.Contributions.Find(transactionId);
            Document merryGoRoundDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.MerryGoRoundDisbursment,
                CreditFrom = _ctx.Chamas.FirstOrDefault().ChamaId,
                DebitTo = transac.ContributorId,
                IsReversal = false,
                // ConfirmedBy = confirmedById
            };

            transac.IsClosed = true;
            transac.DateClosed = merryGoRoundDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(merryGoRoundDocument);
            _ctx.SaveChanges();
        }

        public void ChamaToPersonal(int transactionId) {
            // todo: check if balance suffices? (oh, wait, that's in atomic procedures, nvm)
            var transac = _ctx.PayOuts.Find(transactionId);
            Document chamaPayoutDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.PayOut,
                CreditFrom = _ctx.Chamas.FirstOrDefault().ChamaId,
                DebitTo = transac.ReceiverId,
                IsReversal = false,
                // ConfirmedBy = confirmedById
            };

            transac.IsClosed = true;
            transac.DateClosed = chamaPayoutDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(chamaPayoutDocument);
            _ctx.SaveChanges();
        }
    }
}
