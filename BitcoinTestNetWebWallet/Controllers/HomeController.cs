using BitcoinTestNetWebWallet.Data;
using BitcoinTestNetWebWallet.Models;
using BitcoinTestNetWebWallet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinTestNetWebWallet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBitcoinService _bitcoinService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IBitcoinService bitcoinService)
        {
            _logger = logger;
            _context = context;
            _bitcoinService = bitcoinService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
