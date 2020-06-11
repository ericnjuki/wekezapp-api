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
        private readonly IUserService _userService;

        //private readonly AtomicProcedures _atoms;
        public LedgerService(WekezappContext ctx, IMapper mapper, IFlowService flowService, IUserService userService) {
            _ctx = ctx;
            _mapper = mapper;
            _flowService = flowService;
            _userService = userService;
            //_atoms = atoms;
        }
        public void RequestDepositToPersonal(PersonalDeposit transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.PersonalDeposits.Add(transac);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.PersonalDepositAsDepositor, transac.TransactionId);
            _flowService.AddFlowItem(NotificationType.PersonalDepositAsAdmin, transac.TransactionId);

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

            _flowService.AddFlowItem(NotificationType.PersonalWithdrawalAsWithdrawer, transac.TransactionId);
            _flowService.AddFlowItem(NotificationType.PersonalWithdrawalAsAdmin, transac.TransactionId);
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

            _flowService.AddFlowItem(NotificationType.DepositToChamaAsAdmin, transac.TransactionId);
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

            _flowService.AddFlowItem(NotificationType.DepositToChamaAsAll, transac.TransactionId);
        }

        public void RequestWithdrawalFromChama(ChamaDeposit transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.ChamaDeposits.Add(transac);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.WithdrawFromChamaAsAdmin, transac.TransactionId);
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

            _flowService.AddFlowItem(NotificationType.WithdrawFromChamaAsAll, transac.TransactionId);
        }

        public void CreateContributions() {
            // todo: add to those regular functions
            // find everyone
            // add a contribution transaction for each id as the contributer
            var everyone = _ctx.Users.ToList();
            foreach (var user in everyone) {
                var cont = new Contribution() {
                    Amount = _ctx.Chamas.First().MinimumContribution,
                    ContributorId = user.UserId,
                    DateFirstRequested = DateTime.Now,
                    DateDue = _ctx.Chamas.First().Period == Period.Weekly ?
                        DateTime.Now.AddDays(7) :
                        DateTime.Now.AddMonths(1)
                };
                _ctx.Contributions.Add(cont);
                _flowService.AddFlowItem(NotificationType.ContributionReminder, cont.TransactionId);
            }

            _ctx.SaveChanges();
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

            _flowService.AddFlowItem(NotificationType.ContributionPayment, transac.TransactionId);
        }

        public void DisburseMerryGoRound(int transactionId) {
            // todo: add to those regular functions, but add it so that it is always done, unless an...
            // todo: ...admin says it to not be done, maybe they wanna skip MGR for a while
            // todo: in the regular func.s, is where we'll add flow item every week reminding admin that..
            // todo: ...an MGR will take place
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
