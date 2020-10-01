using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using wekezapp.business.Contracts;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;
using wekezapp.data.Entities.Transactions;
using wekezapp.data.Persistence;

namespace wekezapp.core.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerController : ControllerBase {
        private readonly WekezappContext _context;
        private readonly ILedgerService _ledgerService;
        private readonly ILoanService _loanService;

        public LedgerController(ILedgerService ledgerService, ILoanService loanService, WekezappContext context) {
            _context = context;
            _ledgerService = ledgerService;
            _loanService = loanService;
        }

        // GET: api/Ledger/getTransactionById/5
        [HttpGet("getTransactionById/{transacId}")]
        public ActionResult<ICollection<Loan>> GetTransactionById(int transacId) {
            try {
                var transac = _ledgerService.GetTransactionById(transacId);

                return Ok(transac);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST: api/Ledger/requestPersonalDeposit
        [HttpPost, Route("requestPersonalDeposit")]
        public ActionResult<PersonalDeposit> RequestPersonalDeposit(PersonalDeposit transacDto) {
            try {
                _ledgerService.RequestDepositToPersonal(transacDto);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/confirmPersonalDeposit
        [HttpPost, Route("confirmPersonalDeposit/{transactionId}/{confirmerId}")]
        public ActionResult<PersonalDeposit> ConfirmPersonalDeposit(int transactionId, int confirmerId) {
            try {
                _ledgerService.ConfirmDepositToPersonal(transactionId, confirmerId);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/requestPersonalWithdrawal
        [HttpPost, Route("requestPersonalWithdrawal")]
        public ActionResult<PersonalWithdrawal> RequestPersonalWithdrawal(PersonalWithdrawal transacDto) {
            try {
                _ledgerService.RequestWithdrawalFromPersonal(transacDto);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/confirmPersonalWithdrawal
        [HttpPost, Route("confirmPersonalWithdrawal/{transactionId}/{confirmerId}")]
        public ActionResult<PersonalWithdrawal> ConfirmPersonalWithdrawal(int transactionId, int confirmerId) {
            try {
                _ledgerService.ConfirmWithdrawalFromPersonal(transactionId, confirmerId);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/requestChamaDeposit
        [HttpPost, Route("requestChamaDeposit")]
        public ActionResult<ChamaDeposit> RequestChamaDeposit(ChamaDeposit transacDto) {
            try {
                _ledgerService.RequestDepositToChama(transacDto);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/confirmChamaDeposit
        [HttpPost, Route("confirmChamaDeposit/{transactionId}/{confirmerId}")]
        public ActionResult<ChamaDeposit> ConfirmChamaDeposit(int transactionId, int confirmerId) {
            try {
                _ledgerService.ConfirmDepositToChama(transactionId, confirmerId);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/requestChamaWithdrawal
        [HttpPost, Route("requestChamaWithdrawal")]
        public ActionResult<ChamaWithdrawal> RequestChamaWithdrawal(ChamaWithdrawal transacDto) {
            try {
                _ledgerService.RequestWithdrawalFromChama(transacDto);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/confirmChamaWithdrawal
        [HttpPost, Route("confirmChamaWithdrawal/{transactionId}/{confirmerId}")]
        public ActionResult<ChamaWithdrawal> ConfirmChamaWithdrawal(int transactionId, int confirmerId) {
            try {
                _ledgerService.ConfirmWithdrawalFromChama(transactionId, confirmerId);
                return Ok();

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // GET: api/Ledger/getAllLoans
        [HttpGet("getAllLoans")]
        public ActionResult<ICollection<Loan>> GetAllLoans() {
            try {
                var loans = _loanService.GetAllLoans();

                return Ok(loans);
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET: api/Ledger/getLoansForUser/5
        [HttpGet("getLoansForUser/{userId}")]
        public ActionResult<ICollection<Loan>> GetLoansForUser(int userId) {
            try {
                var loans = _loanService.GetAllLoansForUser(userId);

                return Ok(loans);
            } catch(Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        // POST: api/Ledger/requestLoan
        [HttpPost, Route("requestLoan")]
        public ActionResult<Loan> RequestLoan(Loan loan) {
            try {
                return Ok(_loanService.CreateLoan(loan));

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        // POST: api/Ledger/approveLoan
        [HttpPost, Route("evaluateLoan")]
        public ActionResult<Loan> EvaluateLoan(Loan loanDto, int flowItemId) {
            try {
                var evaluatedLoan = _loanService.EvaluateLoan(loanDto);
                return Ok(evaluatedLoan);

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        // POST: api/Ledger/payLoan
        [HttpPost, Route("payLoan/{loanId}/{amount}")]
        public ActionResult<Loan> PayLoan(int loanId, float amount) {
            try {
                var paidLoan = _loanService.PayLoan(loanId, amount);
                return Ok(paidLoan);

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        // GET: api/Ledger/createAndTryDisburseMgr
        [HttpGet, Route("createMgr")]
        public ActionResult<PersonalDeposit> CreateMgr() {
            try {
                var mgr = _ledgerService.CreateMgr();
                return Ok(mgr);

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


        // POST: api/Ledger/disburseMgr
        [HttpPost, Route("disburseMgr/{transactionId}")]
        public ActionResult<PersonalDeposit> DisburseMgr(int transactionId) {
            try {
                var mgr = _ledgerService.DisburseMgr(transactionId);
                return Ok(mgr);

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet, Route("createContributions")]
        public ActionResult CreateContributions() {
            try {
                _ledgerService.CreateContributions();
                return Ok("Sussefull");

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("payContribution")]
        public ActionResult<UserDto> GetFlowOfType(int userId, float amount, bool startWithOld) {
            try {
                return Ok(_ledgerService.ContributeToChama(userId, amount, startWithOld));
            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        // POST: api/Ledger/payout/1/1000
        [HttpPost, Route("payout/{userId}/{amount}/{confirmedBy}")]
        public ActionResult<Chama> Payout(int userId, float amount, int confirmedBy) {
            try {
                var mgr = _ledgerService.Payout(userId, amount, confirmedBy);
                return Ok(mgr);

            } catch (Exception e) {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
