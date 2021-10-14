using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
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
        private readonly IAtomicProcedures _atomicProcedures;

        //private readonly AtomicProcedures _atoms;
        public LedgerService(WekezappContext ctx, IMapper mapper, IFlowService flowService, IUserService userService, IAtomicProcedures atomicProcedures) {
            _ctx = ctx;
            _mapper = mapper;
            _flowService = flowService;
            _userService = userService;
            _atomicProcedures = atomicProcedures;
            //_atoms = atoms;
        }

        public Transaction GetTransactionById(int transacId) {
            return (Transaction)_ctx.Find(typeof(Transaction), transacId);
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

            _atomicProcedures.IntoPersonal(transac.Amount, transac.DepositorId);
            Document depositToPersonalDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.PersonalAccountDeposit,
                DebitTo = transac.DepositorId,
                IsReversal = false,
                ConfirmedBy = confirmedById,
                Amount = transac.Amount
            };

            transac.IsClosed = true;
            transac.DateClosed = depositToPersonalDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(depositToPersonalDocument);
            _ctx.SaveChanges();

            
        }

        public void RequestWithdrawalFromPersonal(PersonalWithdrawal transac) {
            transac.DateRequested = DateTime.Now;
            var user = _ctx.Users.Find(transac.WithdrawerId);

            if (user.Balance < transac.Amount) {
                _flowService.AddFlowItem(NotificationType.Announcement, -1, $"You cannot withdraw {transac.Amount}, you have {user.Balance} in your account", new string[] {user.UserId.ToString()});

            } else {
                _ctx.PersonalWithdrawals.Add(transac);
                _ctx.SaveChanges();

                _flowService.AddFlowItem(NotificationType.PersonalWithdrawalAsWithdrawer, transac.TransactionId);
                _flowService.AddFlowItem(NotificationType.PersonalWithdrawalAsAdmin, transac.TransactionId);
            }
        }

        public void ConfirmWithdrawalFromPersonal(int transactionId, int confirmedById) {
            var transac = _ctx.PersonalWithdrawals.Find(transactionId);

            _atomicProcedures.OutOfPersonal(transac.Amount, transac.WithdrawerId);
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

        public void RequestDepositToChama(ChamaDeposit transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.ChamaDeposits.Add(transac);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.DepositToChamaAsAdmin, transac.TransactionId);
        }

        public void ConfirmDepositToChama(int transactionId, int confirmedById) {
            var transac = _ctx.ChamaDeposits.Find(transactionId);
            _atomicProcedures.IntoChama(transac.Amount);

            Document depositToChamaDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.DepositToChama,
                DebitTo = _ctx.Chamas.FirstOrDefault().ChamaId,
                IsReversal = false,
                ConfirmedBy = confirmedById,
                Amount = transac.Amount
            };

            transac.IsClosed = true;
            transac.DateClosed = depositToChamaDocument.TransactionDate = DateTime.Now;

            _ctx.Entry(transac).State = EntityState.Modified;
            _ctx.Documents.Add(depositToChamaDocument);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.DepositToChamaAsAll, transac.TransactionId);
        }

        public void RequestWithdrawalFromChama(ChamaWithdrawal transac) {
            transac.DateRequested = DateTime.Now;
            _ctx.ChamaWithdrawals.Add(transac);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.WithdrawFromChamaAsAdmin, transac.TransactionId);
        }

        public void ConfirmWithdrawalFromChama(int transactionId, int confirmedById) {
            var transac = _ctx.ChamaWithdrawals.Find(transactionId);
            _atomicProcedures.OutOfChama(transac.Amount);

            Document withdrawFromChamaDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.WithdrawalFromChama,
                CreditFrom = _ctx.Chamas.FirstOrDefault().ChamaId,
                IsReversal = false,
                ConfirmedBy = confirmedById,
                Amount = transac.Amount
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
                        DateTime.Now.AddMonths(1),
                    AmountPaidSoFar = 0,
                };
                _ctx.Contributions.Add(cont);
                _ctx.SaveChanges();
                _flowService.AddFlowItem(NotificationType.ContributionReminder, cont.TransactionId);
            }

            // update next due date
            var chama = _ctx.Chamas.First();
            chama.NextMgrDate = _ctx.Chamas.First().Period == Period.Weekly ?
                        chama.NextMgrDate.AddDays(7) :
                        chama.NextMgrDate.AddMonths(1);
            _ctx.Entry(chama).State = EntityState.Modified;
            _ctx.SaveChanges();
        }

        public UserDto ContributeToChama(int userId, float amount, bool startWithOld) {
            // todo: check if balance suffices? (oh, wait, that's in atomic procedures, nvm).
            var openContributions = _ctx.Contributions
                .Where(c => c.ContributorId == userId)
                .Where(c => c.IsClosed == false)
                .Select(c => c).ToList();

            if (startWithOld)
                openContributions.Sort((x, y) => DateTime.Compare(x.DateFirstRequested, y.DateFirstRequested));
            else
                openContributions.Sort((x, y) => DateTime.Compare(y.DateFirstRequested, x.DateFirstRequested));

            for (var i = 0; i < openContributions.Count; i++) {
                if (amount <= 0)
                    break;
                var amountRemaining = openContributions[i].Amount - openContributions[i].AmountPaidSoFar;

                Document contributeToChamaDocument = new Document {
                    Transaction = openContributions[i],
                    TransactionId = openContributions[i].TransactionId,
                    TransactionDate = DateTime.Now,
                    DocumentType = DocumentType.ContributionPayment,
                    CreditFrom = openContributions[i].ContributorId,
                    DebitTo = _ctx.Chamas.FirstOrDefault().ChamaId,
                    IsReversal = false,
                };

                if (amountRemaining > amount) {
                    contributeToChamaDocument.Amount = amount;
                    openContributions[i].AmountPaidSoFar += amount;

                    _atomicProcedures.PersonalToChama(amount, userId);
                } else {
                    contributeToChamaDocument.Amount = amountRemaining;
                    openContributions[i].AmountPaidSoFar += amountRemaining;
                    openContributions[i].IsClosed = true;
                    openContributions[i].DateClosed = contributeToChamaDocument.TransactionDate;

                    _atomicProcedures.PersonalToChama(amountRemaining, userId);
                }
                _ctx.Entry(openContributions[i]).State = EntityState.Modified;
                _ctx.Documents.Add(contributeToChamaDocument);
                _ctx.SaveChanges();

                _flowService.AddFlowItem(NotificationType.ContributionPayment, contributeToChamaDocument.DocumentId);

                amount -= contributeToChamaDocument.Amount;
            }
            return _userService.GetUserById(userId);
        }

        public MerryGoRound CreateMgr() {
            var chama = _ctx.Chamas.First();
            var transac = new MerryGoRound() {
                Amount = chama.MgrAmount,
                ReceiverId = Int32.Parse(chama.MgrOrder[chama.NextMgrReceiverIndex]),
                DateDue = chama.NextMgrDate,
            };
            _ctx.MerryGoRounds.Add(transac);
            _ctx.SaveChanges();

            return transac;
        }

        public MerryGoRound DisburseMgr(int transactionId) {
            // todo: add to those regular functions, but add it so that it is always done, unless an...
            // todo: ...admin says it to not be done, maybe they wanna skip MGR for a while
            // todo: in the regular func.s, is where we'll add flow item every week reminding admin that..
            // todo: ...an MGR will take place
            // todo: check if balance suffices? (oh, wait, that's in atomic procedures, nvm)

            var chama = _ctx.Chamas.First();
            var transac = _ctx.MerryGoRounds.Find(transactionId);

            try {
                _atomicProcedures.ChamaToPersonal(transac.Amount, transac.ReceiverId);

                Document merryGoRoundDocument = new Document {
                    Transaction = transac,
                    TransactionId = transac.TransactionId,
                    DocumentType = DocumentType.MerryGoRoundDisbursment,
                    CreditFrom = chama.ChamaId,
                    DebitTo = transac.ReceiverId,
                    IsReversal = false,
                };

                transac.IsClosed = true;
                transac.DateClosed = merryGoRoundDocument.TransactionDate = DateTime.Now;

                _ctx.Entry(transac).State = EntityState.Modified;
                _ctx.Documents.Add(merryGoRoundDocument);
                _ctx.SaveChanges();

                _flowService.AddFlowItem(NotificationType.MerryGoRoundDisbursementAsReceipient, transac.TransactionId);
                _flowService.AddFlowItem(NotificationType.MerryGoRoundDisbursementAsAll, transac.TransactionId);
            } catch (Exception) {
            }


            return transac;
        }

        public Chama Payout(int userId, float amount, int confirmedBy) {
            _atomicProcedures.ChamaToPersonal(amount, userId);

            var transac = new PayOut() {
                Amount = amount,
                ReceiverId = userId
            };
            Document chamaPayoutDocument = new Document {
                Transaction = transac,
                TransactionId = transac.TransactionId,
                DocumentType = DocumentType.PayOut,
                CreditFrom = _ctx.Chamas.FirstOrDefault().ChamaId,
                DebitTo = transac.ReceiverId,
                IsReversal = false,
                Amount = transac.Amount,
                ConfirmedBy = confirmedBy
            };

            transac.IsClosed = true;
            transac.DateClosed = chamaPayoutDocument.TransactionDate = DateTime.Now;

            _ctx.PayOuts.Add(transac);
            _ctx.Documents.Add(chamaPayoutDocument);
            _ctx.SaveChanges();

            _flowService.AddFlowItem(NotificationType.PayoutAsAdmin, chamaPayoutDocument.DocumentId);
            _flowService.AddFlowItem(NotificationType.PayoutAsReceiver, transac.TransactionId);

            return _ctx.Chamas.First();
        }
    }
}
