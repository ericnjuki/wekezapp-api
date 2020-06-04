using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using wekezapp.business.Contracts;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Persistence;

namespace wekezapp.core.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerController : ControllerBase {
        private readonly WekezappContext _context;
        private readonly ILedgerService _ledgerService;

        public LedgerController(ILedgerService ledgerService, WekezappContext context) {
            _context = context;
            _ledgerService = ledgerService;
        }

        // GET: api/Ledger/requestPersonalDeposit
        [HttpPost, Route("requestPersonalDeposit")]
        public ActionResult<PersonalDeposit> RequestPersonalDeposit(PersonalDeposit transacDto) {
            try {
                _ledgerService.RequestDepositToPersonal(transacDto);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        [HttpPost, Route("confirmPersonalDeposit")]
        public ActionResult<PersonalDeposit> ConfirmPersonalDeposit(int transactionId, int confirmerId) {
            try {
                _ledgerService.ConfirmDepositToPersonal(transactionId, confirmerId);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }
    }
}
