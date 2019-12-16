using System;
using System.Collections.Generic;
using System.Text;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;

namespace wekezapp.business.Contracts {
    public interface IUserService
    {
        UserDto GetUserById(int userId);

        UserDto GetUserByUsername(string username);

        IEnumerable<UserDto> GetAllUsers();

        void AddUser(UserDto user);

        void UpdateUser(UserDto user);

        UserDto Authenticate(string username, string password);
    }
}
