using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Enums;
using wekezapp.data.Interfaces;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class UserService : IUserService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserService(WekezappContext shopAssist2Context, IMapper mapper, IEmailService emailService) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
            _emailService = emailService;
        }
        public UserDto GetUserById(int userId) {
            var user = _ctx.Users.FirstOrDefault();
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public UserDto GetUserByEmail(string email) {
            var user = _ctx.Users.FirstOrDefault(x => x.Email == email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public IEnumerable<UserDto> GetAllUsers() {
            return _ctx.Users.ToList().Select(u => _mapper.Map<UserDto>(u));
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
        }

        public void AddUsersBulk(ICollection<UserDto> userDtos) {
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

                var emailOpts = new EmailOptions() {
                    Receipient = userDto.Email,
                    EmailType = EmailType.Welcome,
                    Message = $"Hi, You've been added as a member to the chama {_ctx.Chamas.FirstOrDefault()?.ChamaName}"
                    + "\nPlease use this email and the password below to log in"
                    + $"\nPassword: {userDto.Password}"
                    + "\nFor any information please contact your chama chairperson"
                };
                _emailService.NewEmail(emailOpts);

                CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _ctx.Users.Add(user);
            }
            _ctx.SaveChanges();

            //foreach (var user in _ctx.Users) {
            //    var newMemberNotif = $"You were added to chama {_ctx.Chamas.FirstOrDefault()?.ChamaName}";

            //    if (user.Role != Role.Member) {
            //        newMemberNotif = $"You were added as {user.Role} to chama {_ctx.Chamas.FirstOrDefault()?.ChamaName}";
            //    }
            //    _flowService.AddFlowItem(newMemberNotif, new List<int>(user.UserId));
            //}
        }

        public void UpdateUser(UserDto userDto) {
            _ctx.Users.Find(userDto.UserId);
            var user = _mapper.Map<User>(userDto);

            if (!string.IsNullOrWhiteSpace(userDto.Password)) {
                CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            // TODO: Update The Ledger with amount

            _ctx.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _ctx.SaveChanges();
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
                    new Claim(nameof(Role), userDto.Role)
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            userDto.Password = "";
            userDto.Token = tokenString;

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
