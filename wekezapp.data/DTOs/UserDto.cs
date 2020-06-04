using System;
using System.Collections.Generic;
using System.Text;

namespace wekezapp.data.DTOs {
    public class UserDto {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string Role { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public double Balance { get; set; }

    }
}
