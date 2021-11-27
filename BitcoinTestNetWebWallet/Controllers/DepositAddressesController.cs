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
using BitcoinTestNetWebWallet.Services;
using System.Security.Claims;

namespace BitcoinTestNetWebWallet.Controllers
{
    public class DepositAddressesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBitcoinService _bitcoinService;

        public DepositAddressesController(ApplicationDbContext context, IBitcoinService bitcoinService)
        {
            _context = context;
            _bitcoinService = bitcoinService;
        }

        // GET: DepositAddresses
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get deposit amounts per address and transactions per address and put
            // it in TempData for later use by the view.
            try
            {
                TempData["DepositsByAddress"] = _bitcoinService.GetDepositsByAddress();
                TempData["TransactionsByAddress"] = _bitcoinService.GetTransactionsByAddress();
            }
            catch
            {
                return Content("Call to Bitcoin Server failed! Please report error code 2.");
            }
            // Only show deposit addresses that belong to the logged in user
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(await _context.DepositAddress.Where(u => u.UserId == userId).ToListAsync());
        }

        // GET: DepositAddresses/Create
        [Authorize]
        public IActionResult Create()
        {
            // Generate a new bitcoin address and save in TempData.
            // It will be displayed on the form and later saved to the
            // database if the user clicks create.
            try
            {
                TempData["NewAddress"] = _bitcoinService.GenerateNewAddress();
            }
            catch
            {
                return Content("Call to Bitcoin Server failed! Please report error code 3.");
            }
            return View();
        }

        // POST: DepositAddresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Comment")] DepositAddress depositAddress)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                depositAddress.UserId = userId;
                depositAddress.Address = TempData["NewAddress"].ToString();
                _context.Add(depositAddress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(depositAddress);
        }

    }
}
