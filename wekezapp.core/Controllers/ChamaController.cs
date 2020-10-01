using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class ChamaController : ControllerBase {
        private readonly IChamaService _chamaService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ChamaController(IChamaService chamaService, IUserService userService, IMapper mapper) {
            _chamaService = chamaService;
            _userService = userService;
            _mapper = mapper;
        }

        // GET: api/chama
        [HttpGet]
        public ActionResult<Chama> GetChama() {
            var chama = _chamaService.GetChama();

            if (chama == null) {
                return NotFound();
            }
            var users = _userService.GetAllUsers();
            float totalOwed = 0;
            foreach (var user in users) {
                totalOwed += user.OutstandingContributions + user.OutstandingLoans;
            }
            ChamaDto chamaDto = _mapper.Map<ChamaDto>(chama);
            chamaDto.TotalOwed = totalOwed;
            return Ok(chamaDto);
        }


        [HttpPost, Route("add")]
        public IActionResult Register(Chama chama) {
            try {
                _chamaService.AddChama(chama);
                return Ok("success");
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }


        // PUT: api/Chama/5
        [HttpPut, Route("update")]
        public IActionResult UpdateChama(Chama chamaDto) {
            try {
                return Ok(_chamaService.UpdateChama(chamaDto));
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        // PUT: api/Chama/5
        [HttpPut("{id}")]
        public IActionResult PutChama(Chama chamaDto) {
            try {
                _chamaService.UpdateChama(chamaDto);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

            return NoContent();
        }


        // DELETE: api/Chamas/5
        [HttpDelete("{id}")]
        public IActionResult DeleteChama(int id) {
            try {
                _chamaService.DeleteChama(id);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            return NoContent();
        }

        [HttpGet("isContributionsDay")]
        public ActionResult IsContributionsDay() {
            return Ok(_chamaService.IsContributionsDay());
        }
    }
}
