using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;

namespace wekezapp.business.Contracts {
    public interface IUserService
    {
        UserDto GetUserById(int userId);

        UserDto GetUserByEmail(string email);

        IEnumerable<UserDto> GetAllUsers();

        void AddAdmin(UserDto user);

        void AddUsersBulk(ICollection<UserDto> users);

        void UpdateUser(UserDto user);

        bool IsAdmin(int userId);

        UserDto Authenticate(string username, string password);
    }
}
