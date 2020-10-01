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
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class FlowService : IFlowService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;

        public FlowService(WekezappContext shopAssist2Context, IMapper mapper) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
        }

        public FlowItem GetFlowItem(int flowItemId) {
            return _ctx.FlowItems.Find(flowItemId);
        }

        public void AddFlowItem(NotificationType notificationType, int transactionId = -1, string body = "", string[] canBeSeenBy = null) {
            // todo: instead of instantiating each specific type of transaction for this method
            // consider the alt. table-per-hierarchy solution here:
            // https://www.learnentityframeworkcore.com/inheritance/table-per-hierarchy

            //canBeSeenBy = new string[_ctx.Users.Count()];
            //var body = string.Empty;
            var isConfirmable = false;

            var isForAll = false;
            var admins = _ctx.Users.Where(u =>
                u.Role == Role.Admin || u.Role == Role.Treasurer || u.Role == Role.Secretary).Select(u => u).ToList();
            var adminsToShowTo = new List<string>();


            switch (notificationType) {
                case NotificationType.PersonalDepositAsDepositor:
                    var personalDeposit = _ctx.PersonalDeposits.Find(transactionId);
                    canBeSeenBy = new string[] { personalDeposit.DepositorId.ToString() };
                    body = $"You deposited {personalDeposit.Amount} to your account";
                    isConfirmable = true;

                    break;

                case NotificationType.PersonalDepositAsAdmin:
                    var personalDeposit2 = _ctx.PersonalDeposits.Find(transactionId);

                    // because admin gets two notifications: one as an applying user, and one as an admin to confirm it
                    // so if there's another admin, we send *them* the confirmation notification instead of this one

                    if (IsAdmin(personalDeposit2.DepositorId)) {
                        if (admins.Count > 1) {
                            foreach (var admin in admins) {
                                if (admin.UserId != personalDeposit2.DepositorId)
                                    adminsToShowTo.Add(admin.UserId.ToString());
                            }
                            canBeSeenBy = adminsToShowTo.ToArray();
                        } else {
                            canBeSeenBy = new string[] { personalDeposit2.DepositorId.ToString() };
                        }
                    } else {
                        canBeSeenBy = admins.Select(u => u.UserId.ToString()).ToArray();
                    }


                    body = $"{_ctx.Users.Find(personalDeposit2.DepositorId).FirstName} deposited {personalDeposit2.Amount} into their account";
                    isConfirmable = true;
                    //if (_ctx.Users.Find(personalDeposit2.DepositorId).Role == Role.Admin) {
                    //    if (_ctx.Users.Count(u => u.Role == Role.Admin || u.Role == Role.Treasurer) > 1) {
                    //        canBeSeenBy = _ctx.Users
                    //            .Where(u => u.Role == Role.Admin && u.UserId != personalDeposit2.DepositorId)
                    //            .Select(u => u.UserId.ToString()).ToArray();
                    //        body =
                    //            $"{_ctx.Users.Find(personalDeposit2.DepositorId).FirstName} deposited {personalDeposit2.Amount} into their account";
                    //    } else {
                    //        canBeSeenBy = new string[] { personalDeposit2.DepositorId.ToString() };
                    //    }
                    //}
                    break;

                case NotificationType.PersonalWithdrawalAsWithdrawer:
                    var personalWithdrawal = _ctx.PersonalWithdrawals.Find(transactionId);
                    canBeSeenBy = new string[] { personalWithdrawal.WithdrawerId.ToString() };
                    body = $"You withdrew {personalWithdrawal.Amount} from your account";
                    isConfirmable = true;

                    break;

                case NotificationType.PersonalWithdrawalAsAdmin:
                    var personalWithdrawal2 = _ctx.PersonalWithdrawals.Find(transactionId);

                    if (IsAdmin(personalWithdrawal2.WithdrawerId)) {
                        if (admins.Count > 1) {
                            foreach (var admin in admins) {
                                if (admin.UserId != personalWithdrawal2.WithdrawerId)
                                    adminsToShowTo.Add(admin.UserId.ToString());
                            }
                            canBeSeenBy = adminsToShowTo.ToArray();
                        } else {
                            canBeSeenBy = new string[] { personalWithdrawal2.WithdrawerId.ToString() };
                        }
                    } else {
                        canBeSeenBy = admins.Select(u => u.UserId.ToString()).ToArray();
                    }

                    body = $"{_ctx.Users.Find(personalWithdrawal2.WithdrawerId).FirstName} requested to withdraw {personalWithdrawal2.Amount} from their account";
                    isConfirmable = true;

                    break;

                case NotificationType.DepositToChamaAsAll:
                    var chamaDepositAsAll = _ctx.ChamaDeposits.Find(transactionId);
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    isForAll = true;
                    body = $"{_ctx.Users.Find(chamaDepositAsAll.DepositorId).FirstName} deposited {chamaDepositAsAll.Amount} into the chama account";
                    break;

                case NotificationType.DepositToChamaAsAdmin:
                    var chamaDeposit = _ctx.ChamaDeposits.Find(transactionId);

                    if (IsAdmin(chamaDeposit.DepositorId)) {
                        if (admins.Count > 1) {
                            foreach (var admin in admins) {
                                if (admin.UserId != chamaDeposit.DepositorId)
                                    adminsToShowTo.Add(admin.UserId.ToString());
                            }
                            canBeSeenBy = adminsToShowTo.ToArray();
                        } else {
                            canBeSeenBy = new string[] { chamaDeposit.DepositorId.ToString() };
                        }
                    } else {
                        canBeSeenBy = admins.Select(u => u.UserId.ToString()).ToArray();
                    }

                    body = $"{_ctx.Users.Find(chamaDeposit.DepositorId).FirstName} requested to deposit {chamaDeposit.Amount} to the chama account";
                    isConfirmable = true;

                    break;

                case NotificationType.WithdrawFromChamaAsAll:
                    var chamaWithdrawalAsAll = _ctx.ChamaWithdrawals.Find(transactionId);
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    isForAll = true;
                    body = $"{_ctx.Users.Find(chamaWithdrawalAsAll.WithdrawerId).FirstName} withdrew {chamaWithdrawalAsAll.Amount} from the chama account";
                    break;

                case NotificationType.WithdrawFromChamaAsAdmin:
                    var chamaWithdrawalAsAdmin = _ctx.ChamaWithdrawals.Find(transactionId);

                    if (IsAdmin(chamaWithdrawalAsAdmin.WithdrawerId)) {
                        if (admins.Count > 1) {
                            foreach (var admin in admins) {
                                if (admin.UserId != chamaWithdrawalAsAdmin.WithdrawerId)
                                    adminsToShowTo.Add(admin.UserId.ToString());
                            }
                            canBeSeenBy = adminsToShowTo.ToArray();
                        } else {
                            canBeSeenBy = new string[] { chamaWithdrawalAsAdmin.WithdrawerId.ToString() };
                        }
                    } else {
                        canBeSeenBy = admins.Select(u => u.UserId.ToString()).ToArray();
                    }

                    body = $"{_ctx.Users.Find(chamaWithdrawalAsAdmin.WithdrawerId).FirstName} requested to withdraw {chamaWithdrawalAsAdmin.Amount} from the chama account";
                    isConfirmable = true;

                    break;

                case NotificationType.ContributionReminder:
                    var contributionBeforePayment = _ctx.Contributions.Find(transactionId);
                    canBeSeenBy = new string[] { contributionBeforePayment.ContributorId.ToString() };
                    body = $"You have Ksh. {contributionBeforePayment.Amount} chama contribution due on {contributionBeforePayment.DateDue.ToShortDateString()}";
                    isConfirmable = true;
                    break;

                case NotificationType.ContributionPayment:
                    var contributionPaymentDocument = _ctx.Documents.Find(transactionId);
                    var contributionAfterPayment = (Contribution)contributionPaymentDocument.Transaction;
                    canBeSeenBy = new string[] { contributionAfterPayment.ContributorId.ToString() };
                    body = $"You paid chama contribution of Ksh. {contributionPaymentDocument.Amount} on {contributionPaymentDocument.TransactionDate.ToShortDateString()}";
                    if (contributionAfterPayment.IsClosed)
                        body += $"\nYour contribution for {contributionAfterPayment.DateFirstRequested} is now fully settled";
                    else
                        body += $"\nYou now have Ksh. {contributionAfterPayment.Amount - contributionAfterPayment.AmountPaidSoFar} remaining for the {contributionAfterPayment.DateFirstRequested} contribution";
                    break;

                case NotificationType.MerryGoRoundDisbursementAsReceipient:
                    var mgr = _ctx.MerryGoRounds.Find(transactionId);
                    canBeSeenBy = new string[] { mgr.ReceiverId.ToString() };
                    body = $"You received regular disbursment of Ksh. {mgr.Amount} on {mgr.DateClosed.ToShortDateString()}";
                    break;

                case NotificationType.MerryGoRoundDisbursementAsAll:
                    var mgrAll = _ctx.MerryGoRounds.Find(transactionId);
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    isForAll = true;
                    body = $"{_ctx.Users.Find(mgrAll.ReceiverId).FirstName} received regular disbursment of Ksh. {mgrAll.Amount} on {mgrAll.DateClosed.ToShortDateString()}";
                    break;

                case NotificationType.LoanRequestAsRequester:
                    var loanRequestAsRequester = _ctx.Loans.Find(transactionId);
                    canBeSeenBy = new string[] { loanRequestAsRequester.ReceiverId.ToString() };
                    body = $"You requested a loan of Ksh. {loanRequestAsRequester.Amount} on {loanRequestAsRequester.DateRequested}";
                    isConfirmable = true;
                    break;

                case NotificationType.LoanRequestAsAdmin:
                    var loanRequestAsAdmin = _ctx.Loans.Find(transactionId);

                    if (IsAdmin(loanRequestAsAdmin.ReceiverId)) {
                        if (admins.Count > 1) {
                            foreach (var admin in admins) {
                                if (admin.UserId != loanRequestAsAdmin.ReceiverId)
                                    adminsToShowTo.Add(admin.UserId.ToString());
                            }
                            canBeSeenBy = adminsToShowTo.ToArray();
                        } else {
                            canBeSeenBy = new string[] { loanRequestAsAdmin.ReceiverId.ToString() };
                        }
                    } else {
                        canBeSeenBy = admins.Select(u => u.UserId.ToString()).ToArray();
                    }

                    body = $"{_ctx.Users.Find(loanRequestAsAdmin.ReceiverId).FirstName} requested loan of {loanRequestAsAdmin.Amount} from the chama account";
                    isConfirmable = true;
                    break;

                case NotificationType.LoanDisbursmentAsRequester:
                    var loanDisbursmentAsRequester = _ctx.Loans.Find(transactionId);
                    canBeSeenBy = new string[] { loanDisbursmentAsRequester.ReceiverId.ToString() };
                    body = $"Your loan of Ksh. {loanDisbursmentAsRequester.Amount} has been approved by {_ctx.Users.Find(loanDisbursmentAsRequester.EvaluatedBy).FirstName}. The amount has been deposited to your account";
                    break;

                case NotificationType.LoanDisbursmentAsAll:
                    var loanDisbursmentAsAll = _ctx.Loans.Find(transactionId);
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    body = $"{_ctx.Users.Find(loanDisbursmentAsAll.ReceiverId).FirstName}'s loan of Ksh. {loanDisbursmentAsAll.Amount} was approved on {loanDisbursmentAsAll.DateIssued}";
                    break;

                case NotificationType.LoanRepayment:
                    var loanRepayment = _ctx.Documents.Find(transactionId);
                    var loan = _ctx.Loans.Find(loanRepayment.TransactionId);

                    canBeSeenBy = new string[] { loan.ReceiverId.ToString() };
                    body = $"You made a payment of Ksh. {loanRepayment.Amount} towards your loan. ";

                    body += loan.IsClosed ? "Your loan is now fully repaid" : $"Amount payable remaining is {loan.AmountPayable - loan.AmountPaidSoFar}";
                    // since transactionId here is actually a document, we're changing it to be the actual transaction
                    transactionId = loan.TransactionId;
                    break;

                case NotificationType.LoanFineApplication:
                    var loanToBeFined = _ctx.Loans.Find(transactionId);

                    canBeSeenBy = new string[] { loanToBeFined.ReceiverId.ToString() };
                    body = $"A fine of Ksh. {loanToBeFined.LatePaymentFine} has been applied to your loan on {DateTime.Now.ToShortDateString()}. New amont payable is {loanToBeFined.AmountPayable - loanToBeFined.AmountPaidSoFar}";
                    break;

                case NotificationType.PayoutAsReceiver:
                    var payout = _ctx.PayOuts.Find(transactionId);
                    canBeSeenBy = new string[] { payout.ReceiverId.ToString() };
                    body = $"You recieved {payout.Amount} from chama account";
                    break;

                case NotificationType.PayoutAsAdmin:
                    var payoutDocument = _ctx.Documents.Find(transactionId);
                    var payoutTransac = _ctx.PayOuts.Find(payoutDocument.Transaction.TransactionId);

                    canBeSeenBy = new[] { payoutDocument.ConfirmedBy.ToString() };
                    body = $"You transferred {payoutDocument.Amount} from the chama to {_ctx.Users.Find(payoutTransac.ReceiverId).FirstName}'s account";
                    break;

                case NotificationType.Announcement:
                    if (canBeSeenBy == null) {
                        canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                        isForAll = true;
                    }
                    break;

                default:
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    isForAll = true;
                    break;
            }

            if (canBeSeenBy == null) canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();

            var flowItem = new FlowItem() {
                DateCreated = DateTime.Now,
                Body = body,
                CanBeSeenBy = canBeSeenBy,
                IsForAll = isForAll,
                TransactionId = transactionId,
                NotificationType = notificationType,
                IsConfirmable = isConfirmable
            };

            _ctx.FlowItems.Add(flowItem);
            _ctx.SaveChanges();
        }

        public IEnumerable<FlowItemDto> GetFlow(int userId) {
            var userFlow = new List<FlowItemDto>();
            foreach (var flowItem in _ctx.FlowItems) {
                if (flowItem.CanBeSeenBy.Contains(userId.ToString())) {
                    var item = new FlowItemDto() {
                        FlowItemId = flowItem.FlowItemId,
                        NotificationType = flowItem.NotificationType,
                        Body = flowItem.Body,
                        DateCreated = flowItem.DateCreated,
                        HasBeenSeenBy = flowItem.HasBeenSeenBy,
                        IsConfirmable = flowItem.IsConfirmable,
                        IsConfirmed = flowItem.IsConfirmed,
                        TransactionId = flowItem.TransactionId
                    };
                    item = AddAdditionalProperties(item);
                    userFlow.Add(item);
                }
            }
            return userFlow;
        }

        public IEnumerable<FlowItemDto> GetFlowOfType(int userId, NotificationType notificationType) {
            var userFlow = new List<FlowItemDto>();

            foreach (var flowItem in _ctx.FlowItems) {
                var item = new FlowItemDto() {
                    FlowItemId = flowItem.FlowItemId,
                    NotificationType = flowItem.NotificationType,
                    Body = flowItem.Body,
                    DateCreated = flowItem.DateCreated,
                    HasBeenSeenBy = flowItem.HasBeenSeenBy,
                    IsConfirmable = flowItem.IsConfirmable,
                    IsConfirmed = flowItem.IsConfirmed,
                    TransactionId = flowItem.TransactionId
                };

                item = AddAdditionalProperties(item);

                // determining what to return
                if (notificationType == NotificationType.All) {
                    userFlow.Add(item);
                    continue;
                }
                if (flowItem.NotificationType == notificationType) {
                    userFlow.Add(item);
                };
            }
            //foreach (var flowItem in _ctx.FlowItems.Where(f => f.NotificationType == notificationType)) {
            //    // we'll skip the canbeseenby for now, i think in this endpoint, you can request any flowitem
            //    //if (flowItem.CanBeSeenBy.Contains(userId.ToString())) {}
            //    var item = new FlowItemDto() {
            //        FlowItemId = flowItem.FlowItemId,
            //        Body = flowItem.Body,
            //        DateCreated = flowItem.DateCreated,
            //        HasBeenSeenBy = flowItem.HasBeenSeenBy,
            //        IsConfirmable = flowItem.IsConfirmable,
            //        IsConfirmed = flowItem.IsConfirmed,
            //        TransactionId = flowItem.TransactionId
            //    };
            //    userFlow.Add(item);
            //}

            return userFlow;
        }

        public void UpdateFlow(FlowItem flowItemDto) {
            flowItemDto.DateModified = DateTime.Now;
            _ctx.Entry(flowItemDto).State = EntityState.Modified;
            _ctx.SaveChanges();
        }

        private bool IsAdmin(int userId) {
            var user = _ctx.Users.Find(userId);
            if (user.Role == Role.Admin || user.Role == Role.Treasurer || user.Role == Role.Secretary) {
                return true;
            } else return false;
        }

        private FlowItemDto AddAdditionalProperties(FlowItemDto flowItem) {
            // additional properties
            if (flowItem.IsConfirmable) {
                switch (flowItem.NotificationType) {
                    case NotificationType.PersonalDepositAsAdmin:
                    case NotificationType.PersonalDepositAsDepositor:
                        var personalDeposit = _ctx.PersonalDeposits.Find(flowItem.TransactionId);
                        flowItem.Status = personalDeposit.IsClosed ? "Confirmed" : "Unconfirmed";
                        break;
                    case NotificationType.PersonalWithdrawalAsAdmin:
                    case NotificationType.PersonalWithdrawalAsWithdrawer:
                        var personalWithdrawal = _ctx.PersonalWithdrawals.Find(flowItem.TransactionId);
                        flowItem.Status = personalWithdrawal.IsClosed ? "Confirmed" : "Unconfirmed";
                        break;
                    case NotificationType.DepositToChamaAsAdmin:
                        var chamaDeposit = _ctx.ChamaDeposits.Find(flowItem.TransactionId);
                        flowItem.Status = chamaDeposit.IsClosed ? "Confirmed" : "Unconfirmed";
                        break;
                    case NotificationType.WithdrawFromChamaAsAdmin:
                        var chamaWithdrawal = _ctx.ChamaWithdrawals.Find(flowItem.TransactionId);
                        flowItem.Status = chamaWithdrawal.IsClosed ? "Confirmed" : "Unconfirmed";
                        break;
                    case NotificationType.LoanRequestAsAdmin:
                    case NotificationType.LoanRequestAsRequester:
                        var loan = _ctx.Loans.Find(flowItem.TransactionId);
                        if (loan.Approved) {
                            flowItem.Status = "Approved";
                        } else if (!loan.Approved && loan.IsClosed) {
                            flowItem.Status = "Rejected";
                        } else {
                            flowItem.Status = "Pending";
                        }
                        break;
                }
            }
            return flowItem;
        }
    }
}
