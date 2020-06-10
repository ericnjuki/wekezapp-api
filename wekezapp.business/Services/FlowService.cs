using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Enums;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class FlowService : IFlowService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public FlowService(WekezappContext shopAssist2Context, IMapper mapper, IUserService userService) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
            this._userService = userService;
        }

        public FlowItem GetFlowItem(int flowItemId) {
            return _ctx.FlowItems.Find(flowItemId);
        }

        public void AddFlowItem(NotificationType notificationType, int transactionId = -1) {
            // todo: instead of instantiating each specific type of transaction for this method
            // consider the alt. table-per-hierarchy solution here:
            // https://www.learnentityframeworkcore.com/inheritance/table-per-hierarchy

            var canBeSeenBy = new string[_ctx.Users.Count()];
            var body = string.Empty;

            switch (notificationType) {
                case NotificationType.PersonalDepositAsDepositor:
                    var personalDeposit = _ctx.PersonalDeposits.Find(transactionId);
                    canBeSeenBy = new string[] { personalDeposit.DepositorId.ToString() };
                    body = $"You deposited {personalDeposit.Amount} to your account";
                    break;

                case NotificationType.PersonalDepositAsAdmin:
                    var personalDeposit2 = _ctx.PersonalDeposits.Find(transactionId);

                    // because admin gets two notifications: one as an applying user, and one as an admin to confirm it
                    // so if there's another admin, we send *them* the confirmation notification instead of this one
                    if (_userService.IsAdmin(personalDeposit2.DepositorId)) {
                        if (_ctx.Users.Count(u => u.Role == Role.Admin || u.Role == Role.Treasurer) > 1) {
                            canBeSeenBy = _ctx.Users
                                .Where(u => u.Role == Role.Admin && u.UserId != personalDeposit2.DepositorId)
                                .Select(u => u.UserId.ToString()).ToArray();
                            body =
                                $"{_ctx.Users.Find(personalDeposit2.DepositorId).FirstName} deposited {personalDeposit2.Amount} into their account";
                        } else {
                            canBeSeenBy = new string[] { personalDeposit2.DepositorId.ToString() };
                        }
                    }
                    break;

                case NotificationType.PersonalWithdrawalAsWithdrawer:
                    var personalWithdrawal = _ctx.PersonalWithdrawals.Find(transactionId);
                    canBeSeenBy = new string[] { personalWithdrawal.WithdrawerId.ToString() };
                    body = $"You withdrew {personalWithdrawal.Amount} from your account";
                    break;

                case NotificationType.PersonalWithdrawalAsAdmin:
                    var personalWithdrawal2 = _ctx.PersonalWithdrawals.Find(transactionId);
                    if (_userService.IsAdmin(personalWithdrawal2.WithdrawerId)) {
                        if (_ctx.Users.Count(u => u.Role == Role.Admin || u.Role == Role.Treasurer) > 1) {
                            canBeSeenBy = _ctx.Users
                                .Where(u => u.Role == Role.Admin && u.UserId != personalWithdrawal2.WithdrawerId)
                                .Select(u => u.UserId.ToString()).ToArray();
                            body = $"{_ctx.Users.Find(personalWithdrawal2.WithdrawerId).FirstName} requested to withdraw {personalWithdrawal2.Amount} from their account";
                        } else {
                            canBeSeenBy = new string[] { personalWithdrawal2.WithdrawerId.ToString() };
                            body = $"You requested to withdraw {personalWithdrawal2.Amount} from your account";
                        }
                    }
                    break;

                case NotificationType.DepositToChamaAsAll:
                    var chamaDepositAsAll = _ctx.ChamaDeposits.Find(transactionId);
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    body = $"{_ctx.Users.Find(chamaDepositAsAll.DepositorId).FirstName} deposited {chamaDepositAsAll.Amount} into the chama account";
                    break;

                case NotificationType.DepositToChamaAsAdmin:
                    var chamaDeposit = _ctx.ChamaDeposits.Find(transactionId);
                    if (_userService.IsAdmin(chamaDeposit.DepositorId)) {
                        if (_ctx.Users.Count(u => u.Role == Role.Admin || u.Role == Role.Treasurer) > 1) {
                            canBeSeenBy = _ctx.Users
                                .Where(u => u.Role == Role.Admin && u.UserId != chamaDeposit.DepositorId)
                                .Select(u => u.UserId.ToString()).ToArray();
                            body = $"{_ctx.Users.Find(chamaDeposit.DepositorId).FirstName} requested to deposit {chamaDeposit.Amount} to the chama account";
                        } else {
                            canBeSeenBy = new string[] { chamaDeposit.DepositorId.ToString() };
                            body = $"You requested to deposit {chamaDeposit.Amount} to the chama account";
                        }
                    }
                    break;

                case NotificationType.WithdrawFromChamaAsAll:
                    var chamaWithdrawalAsAll = _ctx.ChamaWithdrawals.Find(transactionId);
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    body = $"{_ctx.Users.Find(chamaWithdrawalAsAll.WithdrawerId).FirstName} withdrew {chamaWithdrawalAsAll.Amount} from the chama account";
                    break;

                case NotificationType.WithdrawFromChamaAsAdmin:
                    var chamaWithdrawalAsAdmin = _ctx.ChamaWithdrawals.Find(transactionId);
                    if (_userService.IsAdmin(chamaWithdrawalAsAdmin.WithdrawerId)) {
                        if (_ctx.Users.Count(u => u.Role == Role.Admin || u.Role == Role.Treasurer) > 1) {
                            canBeSeenBy = _ctx.Users
                                .Where(u => u.Role == Role.Admin && u.UserId != chamaWithdrawalAsAdmin.WithdrawerId)
                                .Select(u => u.UserId.ToString()).ToArray();
                            body = $"{_ctx.Users.Find(chamaWithdrawalAsAdmin.WithdrawerId).FirstName} requested to withdraw {chamaWithdrawalAsAdmin.Amount} from the chama account";
                        } else {
                            canBeSeenBy = new string[] { chamaWithdrawalAsAdmin.WithdrawerId.ToString() };
                            body = $"You requested to withdraw {chamaWithdrawalAsAdmin.Amount} from the chama account";
                        }
                    }
                    break;

                case NotificationType.ContributionReminder:
                    var contributionBeforePayment = _ctx.Contributions.Find(transactionId);
                    canBeSeenBy = new string[] { contributionBeforePayment.ContributorId.ToString() };
                    body = $"You have {contributionBeforePayment.Amount} chama contribution due on {contributionBeforePayment.DateDue.ToShortDateString()}";
                    break;

                case NotificationType.ContributionPayment:
                    var contributionAfterPayment = _ctx.Contributions.Find(transactionId);
                    canBeSeenBy = new string[] { contributionAfterPayment.ContributorId.ToString() };
                    body = $"You paid chama contribution on {contributionAfterPayment.DateClosed.ToShortDateString()}";
                    break;
                default:
                    canBeSeenBy = _ctx.Users.Select(u => u.UserId.ToString()).ToArray();
                    break;
            }


            var flowItem = new FlowItem() {
                DateCreated = DateTime.Now,
                Body = body,
                CanBeSeenBy = canBeSeenBy,
                TransactionId = transactionId
            };

            _ctx.FlowItems.Add(flowItem);
            _ctx.SaveChanges();
        }

        public IEnumerable<FlowItemDto> GetFlow(int userId) {
            var userFlow = new List<FlowItemDto>();
            foreach (var flowItem in _ctx.FlowItems) {
                if (flowItem.CanBeSeenBy.Contains(userId.ToString())) {
                    userFlow.Add(new FlowItemDto() {
                        FlowItemId = flowItem.FlowItemId,
                        Body = flowItem.Body,
                        DateCreated = flowItem.DateCreated,
                        HasBeenSeenBy = flowItem.HasBeenSeenBy,
                        IsConfirmable = flowItem.IsConfirmable,
                        IsConfirmed = flowItem.IsConfirmed
                    });
                }
            }
            return userFlow;
        }

        public void UpdateFlow(FlowItem flowItemDto) {
            _ctx.Entry(flowItemDto).State = EntityState.Modified;
            _ctx.SaveChanges();
        }

    }
}
