using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Persistence;

namespace wekezapp.core.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private readonly WekezappContext _context;
        private readonly IUserService _userService;

        public UsersController(IUserService userService, WekezappContext context) {
            _context = context;
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers() {
            return Ok(_userService.GetAllUsers());
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUser(int id) {
            var user = _userService.GetUserById(id);

            if (user == null) {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/Users/janedoe
        [HttpGet("{username}")]
        public ActionResult<UserDto> GetUser(string username) {
            var user = _userService.GetUserByUsername(username);

            if (user == null) {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost, Route("register")]
        public IActionResult Register(UserDto user) {
            try {
                _userService.AddUser(user);
                return Ok("success");
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost, Route("login")]
        public IActionResult Authenticate([FromBody]UserDto userDto) {
            try {
                var user = _userService.Authenticate(userDto.UserName, userDto.Password);

                if (user == null) {
                    return BadRequest("Invalid Password");
                }
                return Ok(user);

            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user) {
            if (id != user.UserId) {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!UserExists(id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user) {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id) {
            var user = await _context.Users.FindAsync(id);
            if (user == null) {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id) {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
