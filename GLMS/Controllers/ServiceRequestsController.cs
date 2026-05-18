using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Data;
using GLMS.Models;
using GLMS.Services;

namespace GLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(ApplicationDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var serviceRequests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
            return View(serviceRequests);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            // Get only ACTIVE contracts for service requests (Business Rule Validation)
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= DateTime.UtcNow &&
                           c.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            if (!activeContracts.Any())
            {
                TempData["Warning"] = "No active contracts available. Service requests can only be created for active contracts.";
            }

            ViewBag.Contracts = activeContracts;

            var rate = await _currencyService.GetUSDtoZARRate();
            ViewBag.ExchangeRate = rate;

            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,AmountUSD,RequiredByDate,SpecialInstructions,Priority")] ServiceRequest serviceRequest)
        {
            // BUSINESS RULE VALIDATION: Check contract is active
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Invalid contract selected.");
            }
            else if (!contract.CanCreateServiceRequest())
            {
                ModelState.AddModelError("", "❌ Service requests can ONLY be created for ACTIVE contracts that are within their valid date range.");
                ViewBag.Contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.Status == ContractStatus.Active)
                    .ToListAsync();
                ViewBag.ExchangeRate = await _currencyService.GetUSDtoZARRate();
                return View(serviceRequest);
            }

            if (ModelState.IsValid)
            {
                // Convert USD to ZAR using live exchange rate API
                serviceRequest.AmountZAR = await _currencyService.ConvertUSDtoZAR(serviceRequest.AmountUSD);
                serviceRequest.ExchangeRateUsed = await _currencyService.GetUSDtoZARRate();
                serviceRequest.Status = RequestStatus.Pending;
                serviceRequest.RequestDate = DateTime.UtcNow;
                serviceRequest.CreatedAt = DateTime.UtcNow;

                // Generate tracking number
                serviceRequest.TrackingNumber = $"SR-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Service request #{serviceRequest.TrackingNumber} created successfully! Amount in ZAR: R {serviceRequest.AmountZAR:N2}";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Contracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();
            ViewBag.ExchangeRate = await _currencyService.GetUSDtoZARRate();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequests/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatus status)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            var oldStatus = serviceRequest.Status;
            serviceRequest.Status = status;

            if (status == RequestStatus.Completed)
            {
                serviceRequest.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Service request status updated from {oldStatus} to {status}!";

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            // Only allow editing if not completed or cancelled
            if (serviceRequest.Status == RequestStatus.Completed || serviceRequest.Status == RequestStatus.Cancelled)
            {
                TempData["Error"] = "Cannot edit completed or cancelled service requests.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            ViewBag.ExchangeRate = await _currencyService.GetUSDtoZARRate();
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,AmountUSD,RequiredByDate,SpecialInstructions,Priority")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            var existingRequest = await _context.ServiceRequests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound();
            }

            // Verify contract is still active
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
            if (contract == null || !contract.CanCreateServiceRequest())
            {
                ModelState.AddModelError("", "Cannot edit - contract is no longer active.");
                ViewBag.ExchangeRate = await _currencyService.GetUSDtoZARRate();
                return View(serviceRequest);
            }

            if (ModelState.IsValid)
            {
                // Update amounts with current exchange rate
                existingRequest.Description = serviceRequest.Description;
                existingRequest.AmountUSD = serviceRequest.AmountUSD;
                existingRequest.AmountZAR = await _currencyService.ConvertUSDtoZAR(serviceRequest.AmountUSD);
                existingRequest.ExchangeRateUsed = await _currencyService.GetUSDtoZARRate();
                existingRequest.RequiredByDate = serviceRequest.RequiredByDate;
                existingRequest.SpecialInstructions = serviceRequest.SpecialInstructions;
                existingRequest.Priority = serviceRequest.Priority;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Service request updated successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            ViewBag.ExchangeRate = await _currencyService.GetUSDtoZARRate();
            return View(serviceRequest);
        }
    }
}