using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Enums;
using wekezapp.data.Interfaces;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class UserService : IUserService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IFlowService _flowService;
        private static Random random = new Random();

        public UserService(WekezappContext shopAssist2Context, IMapper mapper, IEmailService emailService, IFlowService flowService) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
            _emailService = emailService;
            _flowService = flowService;
        }
        public UserDto GetUserById(int userId) {
            var user = _ctx.Users.Find(userId);
            if (user == null) return null;
            var userDto = _mapper.Map<UserDto>(user);
            userDto = GetOutstandingLoans(GetOutstandingContributions(userDto));
            return userDto;
        }

        public UserDto GetUserByEmail(string email) {
            var user = _ctx.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return null;
            var userDto = _mapper.Map<UserDto>(user);
            userDto = GetOutstandingLoans(GetOutstandingContributions(userDto));
            return userDto;
        }

        public IEnumerable<UserDto> GetAllUsers() {
            return _ctx.Users.ToList().Select(u => GetOutstandingContributions(GetOutstandingLoans(_mapper.Map<UserDto>(u))));
        }

        public void AddAdmin(UserDto userDto) {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            // validation
            if (string.IsNullOrWhiteSpace(userDto.Password))
                throw new InvalidDataException("Password is required");

            if (string.IsNullOrWhiteSpace(userDto.Email))
                throw new InvalidDataException("Email is soo required");

            if (string.IsNullOrWhiteSpace(userDto.Role))
                userDto.Role = Role.Admin;

            if (_ctx.Users.Any(x => x.Email == userDto.Email)) {
                throw new Exception("Username \"" + userDto.Email + "\" is already taken");
            }

            var user = _mapper.Map<User>(userDto);

            CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _ctx.Users.Add(user);
            _ctx.SaveChanges();

            var bodyHtml = string.Empty;
            var bodyText = string.Empty;
            bodyHtml = bodyText = $@"Hello {user.FirstName}, thank you for creating an account with us.<br>
                    You've joined Wekezapp as an Admin.<br>
                    Follow the steps on your portal to create your chama and add new members.
                       <br><br>
                       With Wekezapp, you can:
                        <ul>
                        <li>Keep track of chama finances</li>
                        <li>Keep track of personal finances within the chama</li>
                        <li>Request and pay loans</li>
                        <li>Disburse regular payments to chama members</li>
                        <li>Receive regular contributions from chama members</li>
                        </ul>
                        <br><br>
                    <small>If you are not the admin of a chama,
                    please contact your Chairperson to add you to a chama<small>";
            //_emailService.SendGridMail(user.Email, "Welcome to Wekezapp!", bodyText, bodyHtml);
            _emailService.NewEmail(new EmailOptions{ Message = bodyText, Receipient = user.Email }, "Welcome to Wekezapp!");
        }

        public void AddUsersBulk(ICollection<UserDto> userDtos, int addedBy) {
            foreach (var userDto in userDtos) {

                if (userDto == null)
                    throw new ArgumentNullException(nameof(userDto));

                // validation
                // TODO: craft email based on this if to acknowledge user signing self up or being added by admin
                if (string.IsNullOrWhiteSpace(userDto.Password))
                    // if the user isn't signing him/herself up, generate password for him/her
                    userDto.Password = GenerateSecurePassword(5);

                if (string.IsNullOrWhiteSpace(userDto.Email))
                    throw new InvalidDataException("Email for " + userDto.FirstName + " is required");

                if (_ctx.Users.Any(u => u.Email == userDto.Email)) {
                    throw new InvalidDataException("Email already exists");
                }

                // client should specify if is any role other than member (like treasurer or secretary)
                // otherwise we'll just add them as a member
                if (string.IsNullOrWhiteSpace(userDto.Role))
                    userDto.Role = Role.Member;

                // TODO: Update The Ledger with amount

                var user = _mapper.Map<User>(userDto);

                var bodyHtml = string.Empty;
                var bodyText = string.Empty;

                bodyHtml = bodyText = $@"Hi, You've been added as a {user.Role} to the chama {_ctx.Chamas.FirstOrDefault()?.ChamaName}<br>
                    Please use this email and the password below to log in.<br><br>

                    Password: {userDto.Password}<br><br>

                    For any information please contact your chama chairperson.";
                //_emailService.SendGridMail(user.Email, "Welcome to Wekezapp!", bodyText, bodyHtml);
                _emailService.NewEmail(new EmailOptions { Message = bodyText, Receipient = user.Email }, "Welcome to Wekezapp!");


                CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _ctx.Users.Add(user);

                // notification
                var newMemberNotif = $"You were added to chama {_ctx.Chamas.FirstOrDefault()?.ChamaName}";

                if (user.Role != Role.Member) {
                    newMemberNotif = $"You were added as {user.Role} to chama {_ctx.Chamas.FirstOrDefault()?.ChamaName}";
                }
                _ctx.SaveChanges();
                _flowService.AddFlowItem(NotificationType.Announcement, -1, newMemberNotif, new string[] { $"{user.UserId}" });

                // update previous flowItems that are viewable by everyone to include this one
                var pastItems = _ctx.FlowItems.ToList();
                foreach (var pastItem in pastItems) {
                    if (pastItem.IsForAll) {
                        var newCanBeSeenBy = pastItem.CanBeSeenBy;

                        Array.Resize(ref newCanBeSeenBy, newCanBeSeenBy.Length + 1);
                        newCanBeSeenBy[newCanBeSeenBy.GetUpperBound(0)] = user.UserId.ToString();

                        pastItem.CanBeSeenBy = newCanBeSeenBy;
                        _ctx.Entry(pastItem).State = EntityState.Modified;
                    }
                }

                // update mgr order
                var chama = _ctx.Chamas.First();
                var newMgrOrder = chama.MgrOrder;

                if(newMgrOrder != null) {
                    Array.Resize(ref newMgrOrder, newMgrOrder.Length + 1);
                    newMgrOrder[newMgrOrder.GetUpperBound(0)] = user.UserId.ToString();

                    chama.MgrOrder = newMgrOrder;
                    _ctx.Entry(chama).State = EntityState.Modified;
                }

                _ctx.SaveChanges();
            }


            var newPeopleAnnouncement = $"{_ctx.Users.Find(addedBy).FirstName} added {userDtos.Count} new members to the chama";
            _flowService.AddFlowItem(NotificationType.Announcement, -1, newPeopleAnnouncement);
        }

        public void UpdateUser(UserDto userDto) {
            var user = _ctx.Users.Find(userDto.UserId);
            var updater = _ctx.Users.Find(userDto.UpdatedBy);
            var originalBalace = user.Balance;
            var balanceDifference = user.Balance - userDto.Balance;
            var now = DateTime.Now;

            if (user.Balance != userDto.Balance) {
                Document executiveEditDocument = new Document() {
                    Amount = (float)(balanceDifference < 0 ? -balanceDifference : balanceDifference),
                    DocumentType = DocumentType.ExecutiveEdit,
                    TransactionDate = now
                };

                if (balanceDifference > 0) {
                    PersonalWithdrawal personalWithdrawal = new PersonalWithdrawal() {
                        Amount = Math.Abs((float)balanceDifference),
                        WithdrawerId = user.UserId,
                        IsClosed = true
                    };
                    personalWithdrawal.DateRequested = personalWithdrawal.DateClosed = now;
                    _ctx.Add(personalWithdrawal);
                    _ctx.SaveChanges();

                    executiveEditDocument.Transaction = personalWithdrawal;
                    executiveEditDocument.CreditFrom = user.UserId;
                    executiveEditDocument.TransactionId = personalWithdrawal.TransactionId;

                    _ctx.Add(executiveEditDocument);
                } else {
                    PersonalDeposit personalDeposit = new PersonalDeposit() {
                        Amount = Math.Abs((float)balanceDifference),
                        DepositorId = user.UserId,
                        IsClosed = true
                    };
                    personalDeposit.DateClosed = personalDeposit.DateRequested = now;
                    _ctx.Add(personalDeposit);
                    _ctx.SaveChanges();

                    executiveEditDocument.Transaction = personalDeposit;
                    executiveEditDocument.DebitTo = user.UserId;
                    executiveEditDocument.TransactionId = personalDeposit.TransactionId;

                    _ctx.Add(executiveEditDocument);
                }

                var message = string.Empty;
                message = $"Your account balance was edited from {originalBalace} to {userDto.Balance} by admin. Contact admin for further details";
                if (updater != null) {
                    message = $"Your account balance was edited from {originalBalace} to {userDto.Balance} by {updater.FirstName}";
                }

                _flowService.AddFlowItem(NotificationType.Announcement, executiveEditDocument.Transaction.TransactionId, message, new string[] { user.UserId.ToString() });
            }

            user.UserId = user.UserId;
            user.PasswordHash = user.PasswordHash;
            user.PasswordSalt = user.PasswordSalt;
            user.Token = user.Token;
            user.Balance = userDto.Balance;
            user.Email = userDto.Email;
            user.FirstName = userDto.FirstName;
            user.Role = userDto.Role;
            user.SecondName = userDto.SecondName;
            user.Stake = userDto.Stake;

            if (!string.IsNullOrWhiteSpace(userDto.Password)) {
                CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                var bodyText = string.Empty;
                var bodyHtml = string.Empty;

                bodyHtml = bodyText = "Your password was changed successfully";
                //_emailService.SendGridMail(user.Email, "Password Change", bodyText, bodyHtml);
                _emailService.NewEmail(new EmailOptions { Message = bodyText, Receipient = user.Email }, "Password Change");

            }

            // TODO: Update The Ledger with amount

            _ctx.Entry(user).State = EntityState.Modified;
            _ctx.SaveChanges();
        }

        UserDto GetOutstandingContributions(UserDto userDto) {
            //var x = _ctx.Contributions.ToList();

            userDto.OutstandingContributions = _ctx.Contributions
                .Where(c => c.ContributorId == userDto.UserId)
                .Where(c => c.IsClosed == false)
                .Select(c => c).Sum(c => c.Amount - c.AmountPaidSoFar);

            return userDto;
        }

        UserDto GetOutstandingLoans(UserDto userDto) {

            userDto.OutstandingLoans = _ctx.Loans
                .Where(l => l.ReceiverId == userDto.UserId)
                .Where(l => l.Approved)
                .Where(l => l.IsClosed == false)
                .Select(l => l).Sum(l => l.AmountPayable - l.AmountPaidSoFar);

            return userDto;
        }

        public bool IsAdmin(int userId) {
            var user = _ctx.Users.Find(userId);
            if (user == null) {
                throw new ArgumentNullException($"user of userId {userId} does not exist");
            }
            if (user.Role == Role.Admin) {
                return true;
            }
            return false;
        }

        public UserDto Authenticate(String email, String password) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = _ctx.Users.FirstOrDefault(x => x.Email == email);

            // check if username exists
            if (user == null)
                throw new InvalidDataException("User doesn't exist");

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful, now generate token
            return GenerateToken(_mapper.Map<UserDto>(user));
        }

        public void SendRecoveryCode(string email) {
            if (string.IsNullOrEmpty(email))
                throw new Exception("Email does not exist");

            var user = _ctx.Users.FirstOrDefault(x => x.Email == email);

            var length = 6;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            user.Token = code;
            _ctx.Entry(user).State = EntityState.Modified;
            _ctx.SaveChanges();

            var bodyText = $@"If you initiated the recovery process
                please enter the code below to continue, otherwise, ignore it: 

                {code}";
            var bodyHtml = $@"
                <p>If you initiated the recovery process
                please enter the code below to continue, otherwise, ignore it
                </p>
                <br>
                <strong>{code}</strong>
                ";
            //this._emailService.SendGridMail(user.Email, "Password Recovery Code", bodyText, bodyHtml);
            this._emailService.NewEmail(new EmailOptions { Message = bodyText, Receipient = user.Email }, "Password Recovery Code");

        }

        public void Recover(string email, string code) {
            var newPass = "NewSecurePassword123";
            if (string.IsNullOrEmpty(email))
                throw new Exception("Email does not exist");

            var user = _ctx.Users.FirstOrDefault(x => x.Email == email);
            if (code == user.Token) {
                CreatePasswordHash(newPass, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _ctx.Entry(user).State = EntityState.Modified;
                _ctx.SaveChanges();
            } else {
                throw new Exception("Invalid Code");
            }

            var bodyText = $@"Find your new password below.
                Please change it as soon as you log in.

                {newPass}";
            var bodyHtml = $@"
                <p>Find your new password below.
                Please change it as soon as you log in.
                </p>
                <br>
                <strong>{newPass}</strong>";
            //this._emailService.SendGridMail(user.Email, "New Wekezapp Password", bodyText, bodyHtml);
            this._emailService.NewEmail(new EmailOptions { Message = bodyText, Receipient = user.Email }, "New Wekezapp Password");

        }

        public void SendEmail(User user, EmailOptions emailOptions) {
            // TODO: edit message according to emailtype and send email 
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace-only string.", nameof(password));

            using (var hmac = new HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt) {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(storedHash));
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(storedSalt));

            using (var hmac = new HMACSHA512(storedSalt)) {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++) {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        private UserDto GenerateToken(UserDto userDto) {
            if (userDto == null)
                throw new Exception("Username or password is incorrect");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("my name is eric and i was born in 1997 and this is my secret kye");
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(nameof(userDto.UserId), userDto.UserId.ToString()),
                    new Claim(nameof(userDto.Email), userDto.Email),
                    new Claim(nameof(userDto.FirstName), userDto.FirstName),
                    new Claim(nameof(Role), userDto.Role),
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            userDto.Password = "";
            userDto.Token = tokenString;
            userDto = GetOutstandingLoans(GetOutstandingContributions(userDto));

            // save token to db
            var ctxUser = _ctx.Users.FirstOrDefault(u => u.UserId == userDto.UserId);
            if (ctxUser != null) ctxUser.Token = tokenString;
            _ctx.SaveChanges();

            return userDto;
        }

        public string GenerateSecurePassword(int length) {
            var chars = "abcdefghijklmnopqrstuvwxyz@#$&ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            return result;
        }
    }
}
