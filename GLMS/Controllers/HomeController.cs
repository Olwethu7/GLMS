using GLMS.Data;
using GLMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Dashboard statistics
            var totalClients = await _context.Clients.CountAsync();
            var totalContracts = await _context.Contracts.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            var totalServiceRequests = await _context.ServiceRequests.CountAsync();
            var pendingRequests = await _context.ServiceRequests.CountAsync(sr => sr.Status == RequestStatus.Pending);

            // Recent contracts
            var recentContracts = await _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Recent service requests
            var recentRequests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(sr => sr.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalClients = totalClients;
            ViewBag.TotalContracts = totalContracts;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.TotalServiceRequests = totalServiceRequests;
            ViewBag.PendingRequests = pendingRequests;

            ViewBag.RecentContracts = recentContracts;
            ViewBag.RecentRequests = recentRequests;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}