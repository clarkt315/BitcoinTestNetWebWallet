using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BitcoinTestNetWebWallet.Data;
using BitcoinTestNetWebWallet.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BitcoinTestNetWebWallet.Services;

namespace BitcoinTestNetWebWallet.Controllers
{
    public class WithdrawalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBitcoinService _bitcoinService;

        public WithdrawalsController(ApplicationDbContext context, IBitcoinService bitcoinService)
        {
            _context = context;
            _bitcoinService = bitcoinService;

        }

        // GET: Withdrawals
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Only show deposit addresses that belong to the logged in user
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(await _context.Withdrawal.Where(u => u.UserId == userId).ToListAsync());
        }

        // GET: Withdrawals/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Withdrawals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DestinationAddress,Amount,Comment")] Withdrawal withdrawal)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                // Require minimum withdrawal size to not cause problems
                decimal minWithdrawalAmount = 0.000001M;
                if (withdrawal.Amount < minWithdrawalAmount)
                {
                    return Content("You can't withdraw less than 0.000001");
                } 

                // Check that user is not trying to withdraw more than their balance.
                if (withdrawal.Amount > _bitcoinService.GetWalletBalance(userId))
                {
                    return Content("You can't withdraw more than your balance.");
                }

                // Round to 8 decimal places because that's the most bitcion can handle.
                withdrawal.Amount = decimal.Round(withdrawal.Amount, 8);

                // Check that the withdrawal address exists
                if (withdrawal.DestinationAddress == null)
                {
                    return Content("You must enter a withdrawal address.");
                }
                
                // Check that the withdrawal address is valid
                bool isValid;
                try
                {
                    isValid = _bitcoinService.ValidateBitcoinAddress(withdrawal.DestinationAddress);
                }
                catch
                {
                    return Content("Call to Bitcoin Server failed! Please report error code 4.");
                }
                if (isValid)
                {
                    string txid;
                    try
                    {
                        txid = _bitcoinService.SendTransaction(withdrawal.DestinationAddress, withdrawal.Amount);
                    }
                    catch
                    {
                        return Content("Call to Bitcoin Server failed! Please report error code 5.");
                    }
                    withdrawal.Txid = txid;    
                    withdrawal.UserId = userId;
                    _context.Add(withdrawal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return Content("Invalid withdrawal addresss!");
                }
            }
            return View(withdrawal);
        }

        private bool WithdrawalExists(int id)
        {
            return _context.Withdrawal.Any(e => e.Id == id);
        }
    }
}
