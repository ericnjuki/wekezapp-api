using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Persistence;

namespace wekezapp.business.Services {
    public class UserService : IUserService {
        private readonly WekezappContext _ctx;
        private readonly IMapper _mapper;
        public UserService(WekezappContext shopAssist2Context, IMapper mapper) {
            _ctx = shopAssist2Context;
            _mapper = mapper;
        }
        public UserDto GetUserById(int userId) {
            var user = _ctx.Users.FirstOrDefault();
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public UserDto GetUserByUsername(string username) {
            var user = _ctx.Users.FirstOrDefault(x => x.UserName == username);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public IEnumerable<UserDto> GetAllUsers() {
            return _ctx.Users.ToList().Select(u => _mapper.Map<UserDto>(u));
        }

        public void AddUser(UserDto userDto) {
            var provider = new SHA1CryptoServiceProvider();

            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            // validation
            if (string.IsNullOrWhiteSpace(userDto.Password))
                throw new Exception("Password is required");

            if (string.IsNullOrWhiteSpace(userDto.Email))
                throw new Exception("Email is soo required");

            if (_ctx.Users.Any(x => x.UserName == userDto.UserName))
                throw new Exception("Username \"" + userDto.UserName + "\" is already taken");

            var user = _mapper.Map<User>(userDto);

            CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _ctx.Users.Add(user);
            _ctx.SaveChanges();
        }

        public void UpdateUser(UserDto userDto) {
            _ctx.Users.Find(userDto.UserId);
            var user = _mapper.Map<User>(userDto);

            if (!string.IsNullOrWhiteSpace(userDto.Password)) {
                CreatePasswordHash(userDto.Password, out var passwordHash, out var passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }
            _ctx.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _ctx.SaveChanges();
        }

        public UserDto Authenticate(String username, String password) {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _ctx.Users.FirstOrDefault(x => x.UserName == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful, now generate token

            return GenerateToken(_mapper.Map<UserDto>(user));
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be empty or whitespace only string.", nameof(password));

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
                    new Claim(ClaimTypes.Name, userDto.UserId.ToString())
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
    }
}
