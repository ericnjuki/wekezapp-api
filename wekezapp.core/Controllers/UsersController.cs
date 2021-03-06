using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Enums;
using wekezapp.data.Persistence;

namespace wekezapp.core.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private readonly WekezappContext _context;
        private readonly IUserService _userService;
        private readonly IFlowService _flowService;

        public UsersController(IUserService userService, IFlowService flowService, WekezappContext context) {
            _context = context;
            _userService = userService;
            _flowService = flowService;
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

        // GET: api/Users/jane@doe.com
        [HttpGet("u/{email}")]
        public ActionResult<UserDto> GetUser(string email) {
            var user = _userService.GetUserByEmail(email);

            if (user == null) {
                return NotFound();
            }

            return Ok(user);
        }


        // GET: api/Users/getFlow
        [HttpGet("getFlow")]
        public ActionResult<ICollection<FlowItem>> GetFlow(int userId) {
            try {
                return Ok(_flowService.GetFlow(userId));
            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        [HttpGet("getFlowOfType")]
        public ActionResult<ICollection<FlowItem>> GetFlowOfType(int userId, NotificationType notificationType) {
            try {
                return Ok(_flowService.GetFlowOfType(userId, notificationType));
            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // GET: api/Users/getFlowItem
        [HttpGet("getFlowItem")]
        public ActionResult<ICollection<FlowItem>> GetFlowItem(int flowItemId) {
            try {
                return Ok(_flowService.GetFlowItem(flowItemId));
            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        [HttpPost, Route("register")]
        public IActionResult Register(UserDto user) {
            try {
                _userService.AddAdmin(user);
                return Ok("success");
            } catch (InvalidDataException ex) {
                // TODO: Send an object that looks like this:
                // {statuscode, customErrorNo? (e.g. 1 means request made without password), fieldName, message}
                return BadRequest(ex);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost(), Route("addBulk")]
        public IActionResult RegisterMany(ICollection<UserDto> users, int addedBy) {
            try {
                if (users.Count == 0) {
                    return BadRequest("No members to add!");
                }

                _userService.AddUsersBulk(users, addedBy);
                return Ok();
            } catch (ArgumentNullException ex) {
                return BadRequest(ex);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost, Route("login")]
        public IActionResult Authenticate([FromBody]UserDto userDto) {
            try {
                var user = _userService.Authenticate(userDto.Email, userDto.Password);

                if (user == null) {
                    return BadRequest("Invalid Email or Password");
                }
                return Ok(user);

            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        [HttpGet, Route("sendRecoveryCode/{email}")]
        public IActionResult SendRecoveryCode(string email) {
            try {
                _userService.SendRecoveryCode(email);
                return Ok("Check email for recovery code and enter it below");

            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        [HttpGet, Route("recover/{email}/{code}")]
        public IActionResult Recover(string email, string code) {
            try {
                _userService.Recover(email, code);
                return Ok("New password sent to email");

            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        // PUT: api/Users/5
        [HttpPut]
        public IActionResult PutUser(UserDto user) {
            try {
                _userService.UpdateUser(user);
            } catch (Exception ex) {
                return NotFound(ex.Message);
            }

            return Ok("user updated successfully");
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
