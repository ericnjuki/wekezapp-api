using System;
using System.Collections.Generic;
using System.IO;
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
    public class ChamaController : ControllerBase {
        private readonly IChamaService _chamaService;

        public ChamaController(IChamaService chamaService) {
            _chamaService = chamaService;
        }

        // GET: api/chama
        [HttpGet]
        public ActionResult<ChamaDto> GetChama() {
            var chama = _chamaService.GetChama();

            if (chama == null) {
                return NotFound();
            }
            return Ok(chama);
        }


        [HttpPost, Route("add")]
        public IActionResult Register(ChamaDto chama) {
            try {
                _chamaService.AddChama(chama);
                return Ok("success");
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }


        // PUT: api/Chama/5
        [HttpPut, Route("update")]
        public IActionResult UpdateChama(ChamaDto chamaDto) {
            try {
                return Ok(_chamaService.UpdateChama(chamaDto));
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        // PUT: api/Chama/5
        [HttpPut("{id}")]
        public IActionResult PutChama(ChamaDto chamaDto) {
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
    }
}
